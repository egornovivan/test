using System.Collections.Generic;
using Pathea.Maths;
using PETools;
using UnityEngine;

namespace RedGrass;

public class RGScene : MonoBehaviour
{
	private INTVECTOR2 mCenter = new INTVECTOR2(-10000, -10000);

	private INTVECTOR2 mSafeCenter = new INTVECTOR2(-10000, -10000);

	public EvniAsset evniAsset;

	public Transform tracedObj;

	public ChunkDataIO dataIO;

	public bool refreshNow;

	public bool cullObjs = true;

	private INTVECTOR2 _curCenter;

	public RGCreator meshCreator;

	private INTVECTOR2 mRenderCenter;

	private RGLODQuadTree mTree;

	private Dictionary<int, RGLODQuadTreeNode> mRootNodes;

	private Dictionary<int, RGLODQuadTreeNode> mTargetNodes;

	private Dictionary<int, Dictionary<int, RGLODQuadTreeNode>> mTailNodes;

	private int[] mXLodIndex;

	private int[] mZLodIndex;

	private bool mUpdateMesh;

	private bool mInitIndex;

	private RGLODQuadTree.NodeDel _delTrue = delegate(RGLODQuadTreeNode item0)
	{
		if (item0.gameObject != null && !item0.gameObject.activeSelf)
		{
			item0.gameObject.SetActive(value: true);
		}
	};

	private RGLODQuadTree.NodeDel _delFalse = delegate(RGLODQuadTreeNode item0)
	{
		if (item0.gameObject != null && item0.gameObject.activeSelf)
		{
			item0.gameObject.SetActive(value: false);
		}
	};

	public RGDataSource data { get; private set; }

	public void Init()
	{
		RedGrassInstance.Init();
		data = new RGDataSource();
		data.Init(evniAsset, dataIO);
		RGPoolSig.Init();
		mTree = new RGLODQuadTree(evniAsset);
	}

	public void Free()
	{
		if (data != null)
		{
			data.Free();
			data = null;
		}
		RGPoolSig.Destroy();
	}

	private bool DefaultRefreshDataDetect()
	{
		bool flag = false;
		Transform transform = ((!(tracedObj == null)) ? tracedObj : PEUtil.MainCamTransform);
		Vector3 position = transform.position;
		int num = Mathf.FloorToInt(position.x) >> evniAsset.SHIFT;
		int num2 = Mathf.FloorToInt(position.z) >> evniAsset.SHIFT;
		if (mCenter.x != num || mCenter.y != num2)
		{
			flag = true;
			_curCenter.x = num;
			_curCenter.y = num2;
		}
		if (flag && dataIO.isActive())
		{
			dataIO.StopProcess();
		}
		return flag;
	}

	private void UpdateDataSource()
	{
		if (!(dataIO is IRefreshDataDetect refreshDataDetect))
		{
			if (DefaultRefreshDataDetect())
			{
				data.ReqsUpdate(_curCenter.x, _curCenter.y);
				mCenter.x = _curCenter.x;
				mCenter.y = _curCenter.y;
			}
		}
		else if (refreshDataDetect.CanRefresh(this))
		{
			Transform transform = ((!(tracedObj == null)) ? tracedObj : PEUtil.MainCamTransform);
			Vector3 position = transform.position;
			int num = Mathf.FloorToInt(position.x) >> evniAsset.SHIFT;
			int num2 = Mathf.FloorToInt(position.z) >> evniAsset.SHIFT;
			data.ReqsUpdate(num, num2);
			mCenter.x = num;
			mCenter.y = num2;
		}
	}

	private INTBOUNDS3 GetSafeBound(int center_cx, int center_cz)
	{
		int a = evniAsset.XStart >> evniAsset.SHIFT;
		int a2 = evniAsset.ZStart >> evniAsset.SHIFT;
		int a3 = evniAsset.XEnd >> evniAsset.SHIFT;
		int a4 = evniAsset.ZEnd >> evniAsset.SHIFT;
		int dataExpandNum = evniAsset.DataExpandNum;
		a = Mathf.Max(a, center_cx - dataExpandNum);
		a2 = Mathf.Max(a2, center_cz - dataExpandNum);
		a3 = Mathf.Min(a3, center_cx + dataExpandNum);
		a4 = Mathf.Min(a4, center_cz + dataExpandNum);
		return new INTBOUNDS3(new INTVECTOR3(a, 0, a2), new INTVECTOR3(a3, 0, a4));
	}

	public void RefreshImmediately()
	{
		Transform transform = ((!(tracedObj == null)) ? tracedObj : PEUtil.MainCamTransform);
		Vector3 position = transform.position;
		int num = Mathf.FloorToInt(position.x) >> evniAsset.SHIFT;
		int num2 = Mathf.FloorToInt(position.z) >> evniAsset.SHIFT;
		data.RefreshImmediately(num, num2);
		mCenter = new INTVECTOR2(num, num2);
		UpdateMeshImmediately();
	}

	private void Awake()
	{
		Init();
		Init_Render();
	}

	private void OnDestroy()
	{
		Free();
	}

	private void Start()
	{
		UpdateDataSource();
	}

	private void Update()
	{
		RefreshDirtyReqs();
		UpdateDataSource();
		if (!data.reqsOutput.IsEmpty() && !data.IsProcessReqs)
		{
			data.SumbmitReqs(mCenter);
			mUpdateMesh = true;
		}
		UpdateMesh();
		if (cullObjs)
		{
			CullObject();
		}
		if (refreshNow)
		{
			RefreshImmediately();
			refreshNow = false;
		}
	}

	private void Init_Render()
	{
		mRenderCenter = new INTVECTOR2(-10000, -10000);
		mTree = new RGLODQuadTree(evniAsset);
		mRootNodes = new Dictionary<int, RGLODQuadTreeNode>();
		mTargetNodes = new Dictionary<int, RGLODQuadTreeNode>();
		mTailNodes = new Dictionary<int, Dictionary<int, RGLODQuadTreeNode>>();
		int maxLOD = evniAsset.MaxLOD;
		mXLodIndex = new int[maxLOD + 1];
		mZLodIndex = new int[maxLOD + 1];
		mInitIndex = true;
		if (meshCreator != null)
		{
			meshCreator.Init(evniAsset, data);
		}
	}

	private void UpdateLodIndex()
	{
		int maxLOD = evniAsset.MaxLOD;
		if (mXLodIndex == null || mXLodIndex.Length != maxLOD + 1)
		{
			mXLodIndex = new int[maxLOD + 1];
			mZLodIndex = new int[maxLOD + 1];
			mInitIndex = true;
		}
	}

	public void UpdateMeshImmediately()
	{
		lock (meshCreator.procLockObj)
		{
			meshCreator.StopProcess();
			mRenderCenter = mCenter;
			List<GameObject> gos = new List<GameObject>();
			Dictionary<int, RGLODQuadTreeNode> rootNodes = mTree.GetRootNodes();
			foreach (RGLODQuadTreeNode value in rootNodes.Values)
			{
				if (value == null)
				{
					continue;
				}
				mTree.TraversalNode(value, delegate(RGLODQuadTreeNode item0)
				{
					item0.UpdateTimeStamp();
					if (item0.gameObject != null)
					{
						gos.Add(item0.gameObject);
						item0.gameObject = null;
					}
				});
			}
			meshCreator.ClearMeshes(gos);
			int maxLOD = evniAsset.MaxLOD;
			UpdateLodIndex();
			for (int num = maxLOD; num >= 0; num--)
			{
				int num2 = 0;
				int num3 = 0;
				num2 = ((mRenderCenter.x < 0) ? (mRenderCenter.x / (1 << num) * (1 << num) - (1 << num)) : (mRenderCenter.x / (1 << num) * (1 << num)));
				num3 = ((mRenderCenter.y < 0) ? (mRenderCenter.y / (1 << num) * (1 << num) - (1 << num)) : (mRenderCenter.y / (1 << num) * (1 << num)));
				mXLodIndex[num] = num2;
				mZLodIndex[num] = num3;
				mTree.UpdateTree(num, mXLodIndex[num], mZLodIndex[num]);
			}
			mTargetNodes = mTree.GetRootNodes();
			mRootNodes = mTargetNodes;
			foreach (KeyValuePair<int, RGLODQuadTreeNode> mTargetNode in mTargetNodes)
			{
				List<RGLODQuadTreeNode> list = new List<RGLODQuadTreeNode>();
				List<GameObject> list2 = new List<GameObject>();
				TraverseNode(mTargetNode.Value, list2, list);
				RGRegion rGRegion = new RGRegion();
				rGRegion.nodes = list;
				rGRegion.oldGos = list2;
				meshCreator.AddReqs(rGRegion);
			}
			meshCreator.ProcessReqsImmediatly();
		}
	}

	public void UpdateMesh()
	{
		if (meshCreator.isActive() || !mUpdateMesh)
		{
			return;
		}
		mUpdateMesh = false;
		mRenderCenter = mCenter;
		int maxLOD = evniAsset.MaxLOD;
		if (!mInitIndex)
		{
			for (int num = maxLOD; num >= 0; num--)
			{
				int num2 = 0;
				int num3 = 0;
				num2 = ((mRenderCenter.x < 0) ? (mRenderCenter.x / (1 << num) * (1 << num) - (1 << num)) : (mRenderCenter.x / (1 << num) * (1 << num)));
				num3 = ((mRenderCenter.y < 0) ? (mRenderCenter.y / (1 << num) * (1 << num) - (1 << num)) : (mRenderCenter.y / (1 << num) * (1 << num)));
				if (num2 != mXLodIndex[num] || num3 != mZLodIndex[num])
				{
					mXLodIndex[num] = num2;
					mZLodIndex[num] = num3;
					mTree.UpdateTree(num, mXLodIndex[num], mZLodIndex[num]);
				}
			}
		}
		else
		{
			for (int num4 = maxLOD; num4 >= 0; num4--)
			{
				int num5 = 0;
				int num6 = 0;
				num5 = ((mRenderCenter.x < 0) ? (mRenderCenter.x / (1 << num4) * (1 << num4) - (1 << num4)) : (mRenderCenter.x / (1 << num4) * (1 << num4)));
				num6 = ((mRenderCenter.y < 0) ? (mRenderCenter.y / (1 << num4) * (1 << num4) - (1 << num4)) : (mRenderCenter.y / (1 << num4) * (1 << num4)));
				mXLodIndex[num4] = num5;
				mZLodIndex[num4] = num6;
				mTree.UpdateTree(num4, mXLodIndex[num4], mZLodIndex[num4]);
			}
			mInitIndex = false;
		}
		mTargetNodes = mTree.GetRootNodes();
		DestroyExtraGo();
		mRootNodes = mTargetNodes;
		int num7 = 0;
		foreach (KeyValuePair<int, RGLODQuadTreeNode> mTargetNode in mTargetNodes)
		{
			List<RGLODQuadTreeNode> list = new List<RGLODQuadTreeNode>();
			List<GameObject> list2 = new List<GameObject>();
			TraverseNode(mTargetNode.Value, list2, list);
			if (list.Count != 0)
			{
				num7++;
			}
			RGRegion rGRegion = new RGRegion();
			rGRegion.nodes = list;
			rGRegion.oldGos = list2;
			meshCreator.AddReqs(rGRegion);
		}
		meshCreator.Active(null);
	}

	private void CullObject()
	{
		Plane[] planes = GeometryUtility.CalculateFrustumPlanes(Camera.main);
		foreach (KeyValuePair<int, RGLODQuadTreeNode> mRootNode in mRootNodes)
		{
			INTVECTOR3 iNTVECTOR = Utils.IndexToPos(mRootNode.Key);
			iNTVECTOR.x <<= evniAsset.SHIFT;
			iNTVECTOR.z <<= evniAsset.SHIFT;
			int num = evniAsset.CHUNKSIZE * (1 << mRootNode.Value.LOD);
			int num2 = num / 2;
			Bounds bounds = new Bounds(new Vector3(iNTVECTOR.x + num2, 500f, iNTVECTOR.z + num2), new Vector3(num, 1100f, num));
			if (GeometryUtility.TestPlanesAABB(planes, bounds))
			{
				mRootNode.Value.visible = true;
				mTree.TraversalNode(mRootNode.Value, _delTrue);
			}
			else
			{
				mRootNode.Value.visible = false;
				mTree.TraversalNode(mRootNode.Value, _delFalse);
			}
		}
	}

	private void DrawAreaInEditor()
	{
		if (!Application.isEditor)
		{
			return;
		}
		mTailNodes = mTree.GetLODTailNode();
		for (int i = 0; i <= evniAsset.MaxLOD; i++)
		{
			if (!mTailNodes.ContainsKey(i))
			{
				continue;
			}
			foreach (RGLODQuadTreeNode value in mTailNodes[i].Values)
			{
				Vector3 vector = new Vector3(value.xIndex * evniAsset.CHUNKSIZE, 0f, evniAsset.CHUNKSIZE * value.zIndex);
				Vector3 vector2 = vector + (Vector3.right + Vector3.forward) * 4f;
				Vector3 vector3 = vector + Vector3.right * ((float)(evniAsset.CHUNKSIZE * (1 << value.LOD)) - 4f) + Vector3.forward * 4f;
				Vector3 vector4 = vector + (Vector3.right + Vector3.forward) * ((float)(evniAsset.CHUNKSIZE * (1 << value.LOD)) - 4f);
				Vector3 vector5 = vector + Vector3.forward * ((float)(evniAsset.CHUNKSIZE * (1 << value.LOD)) - 4f) + Vector3.right * 4f;
				Color color = new Color(1f, (float)value.LOD * 0.2f, (float)value.LOD * 0.4f, 1f);
				Debug.DrawLine(vector2, vector3, color);
				Debug.DrawLine(vector3, vector4, color);
				Debug.DrawLine(vector4, vector5, color);
				Debug.DrawLine(vector5, vector2, color);
			}
		}
	}

	private void DestroyExtraGo()
	{
		List<int> list = new List<int>();
		foreach (KeyValuePair<int, RGLODQuadTreeNode> mRootNode in mRootNodes)
		{
			if (!mTargetNodes.ContainsKey(mRootNode.Key))
			{
				List<GameObject> list2 = new List<GameObject>();
				TraverseNode(mRootNode.Value, list2);
				meshCreator.ClearMeshes(list2);
				list.Add(mRootNode.Key);
			}
		}
		foreach (int item in list)
		{
			mRootNodes.Remove(item);
		}
	}

	private void TraverseNode(RGLODQuadTreeNode node, List<GameObject> go_list)
	{
		if (node == null)
		{
			return;
		}
		mTree.TraversalNode(node, delegate(RGLODQuadTreeNode item0)
		{
			item0.UpdateTimeStamp();
			if (item0.gameObject != null)
			{
				go_list.Add(item0.gameObject);
			}
		});
	}

	private void TraverseNode(RGLODQuadTreeNode node, List<GameObject> old_list, List<RGLODQuadTreeNode> new_list)
	{
		if (node == null)
		{
			return;
		}
		mTree.TraversalNode(node, delegate(RGLODQuadTreeNode item0)
		{
			if (item0.isTail)
			{
				if (item0.gameObject == null)
				{
					new_list.Add(item0);
				}
			}
			else
			{
				item0.UpdateTimeStamp();
				if (item0.gameObject != null)
				{
					old_list.Add(item0.gameObject);
				}
			}
		});
	}

	private void RefreshDirtyReqs()
	{
		if (meshCreator.isActive())
		{
			return;
		}
		lock (meshCreator.procLockObj)
		{
			if (!RGChunk.AnyDirty)
			{
				return;
			}
			Dictionary<int, RGLODQuadTreeNode>.ValueCollection values = mRootNodes.Values;
			foreach (RGLODQuadTreeNode item in values)
			{
				if (item != null)
				{
					mTree.TraversalNode4Req(item, this);
				}
			}
			RGChunk.AnyDirty = false;
		}
	}

	public void OnTraversalNode(RGLODQuadTreeNode node)
	{
		if (node.isTail && node.IsDirty(data))
		{
			meshCreator.ClearMesh(node.gameObject);
			meshCreator.ComputeNow(node, data, evniAsset.LODDensities[node.LOD] * evniAsset.Density);
			node.Dirty(data, dirty: false);
		}
	}
}

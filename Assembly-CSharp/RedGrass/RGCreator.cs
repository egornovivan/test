using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;

namespace RedGrass;

public class RGCreator : DataIO<RGRegion>
{
	public class OutputStruct
	{
		public List<Vector3[]> mVerts;

		public List<Vector3[]> mNorms;

		public List<Vector2[]> mUVs;

		public List<Vector2[]> mUV2s;

		public List<Color32[]> mColors32;

		public List<int[]> mIndices;

		public OutputStruct()
		{
			mVerts = new List<Vector3[]>();
			mNorms = new List<Vector3[]>();
			mUVs = new List<Vector2[]>();
			mUV2s = new List<Vector2[]>();
			mColors32 = new List<Color32[]>();
			mIndices = new List<int[]>();
		}

		public void Clear()
		{
			mVerts.Clear();
			mNorms.Clear();
			mUVs.Clear();
			mUV2s.Clear();
			mColors32.Clear();
			mIndices.Clear();
		}

		public bool Empity()
		{
			return mVerts.Count == 0;
		}

		public int GetCount()
		{
			return mVerts.Count;
		}
	}

	public class OutputGroup
	{
		public OutputStruct billboard;

		public OutputStruct tri;

		public RGLODQuadTreeNode node;

		public OutputGroup()
		{
			billboard = new OutputStruct();
			tri = new OutputStruct();
		}
	}

	public class OutpuRegion
	{
		public List<OutputGroup> groups;

		public List<GameObject> oldGos;

		public OutpuRegion()
		{
			groups = new List<OutputGroup>();
		}
	}

	public const int MaxCreateCountPerFrame = 1;

	private const int c_Interval = 8;

	public Material grassMat;

	public Material triMat;

	private Queue<RGRegion> mReqs;

	private RGDataSource mData;

	private OnReqsFinished onReqsFinished;

	private List<RedGrassInstance> _grasses = new List<RedGrassInstance>();

	private List<RedGrassInstance> _triGrasses = new List<RedGrassInstance>();

	private Thread mThread;

	private bool mRunning = true;

	private bool mActive;

	private bool mStart;

	private List<OutpuRegion> mOutputs = new List<OutpuRegion>();

	public object procLockObj = new object();

	private bool mIsGenerating;

	public void Init(EvniAsset evni, RGDataSource data)
	{
		base.Init(evni);
		mData = data;
	}

	public override bool AddReqs(RGRegion region)
	{
		if (mActive)
		{
			Debug.LogError("The Creator is already acitve");
			return false;
		}
		lock (mReqs)
		{
			mReqs.Enqueue(region);
			foreach (RGLODQuadTreeNode node in region.nodes)
			{
				node.SyncTimeStamp();
			}
		}
		return true;
	}

	public override bool AddReqs(List<RGRegion> nodes)
	{
		return false;
	}

	public override void ClearReqs()
	{
	}

	public override void StopProcess()
	{
		mActive = false;
		lock (mReqs)
		{
			mReqs.Clear();
		}
		lock (mOutputs)
		{
			mOutputs.Clear();
		}
		StopAllCoroutines();
	}

	public override void ProcessReqsImmediatly()
	{
		try
		{
			mActive = false;
			ProcessReqs(mReqs);
			foreach (OutpuRegion mOutput in mOutputs)
			{
				List<OutputGroup> groups = mOutput.groups;
				for (int i = 0; i < groups.Count; i++)
				{
					OutputGroup outputGroup = groups[i];
					if (!outputGroup.node.IsOutOfDate())
					{
						RGLODQuadTreeNode node = outputGroup.node;
						string desc = "Grass Batch [" + node.xIndex.ToString() + ", " + node.zIndex.ToString() + "] _ LOD [" + node.LOD + "]";
						GameObject gameObject = (node.gameObject = CreateBatchGos(desc, outputGroup.billboard, outputGroup.tri));
						if (gameObject != null)
						{
							gameObject.transform.parent = base.transform;
							gameObject.transform.localPosition = Vector3.zero;
						}
					}
				}
				ClearMeshes(mOutput.oldGos);
			}
			mOutputs.Clear();
			mIsGenerating = false;
			mActive = false;
		}
		catch
		{
		}
	}

	public override bool Active(OnReqsFinished call_back)
	{
		if (IsEmpty())
		{
			mActive = false;
			return false;
		}
		mActive = true;
		onReqsFinished = call_back;
		mStart = true;
		return true;
	}

	public override void Deactive()
	{
		mActive = false;
	}

	public override bool isActive()
	{
		return mActive;
	}

	public void ClearMeshes(List<GameObject> gos)
	{
		if (gos == null)
		{
			return;
		}
		foreach (GameObject go in gos)
		{
			MeshFilter[] componentsInChildren = go.GetComponentsInChildren<MeshFilter>(includeInactive: true);
			MeshFilter[] array = componentsInChildren;
			foreach (MeshFilter meshFilter in array)
			{
				UnityEngine.Object.Destroy(meshFilter.mesh);
				meshFilter.mesh = null;
			}
			UnityEngine.Object.Destroy(go);
		}
		gos.Clear();
	}

	public void ClearMesh(GameObject go)
	{
		if (!(go == null))
		{
			MeshFilter[] componentsInChildren = go.GetComponentsInChildren<MeshFilter>(includeInactive: true);
			MeshFilter[] array = componentsInChildren;
			foreach (MeshFilter meshFilter in array)
			{
				UnityEngine.Object.Destroy(meshFilter.mesh);
				meshFilter.mesh = null;
			}
			UnityEngine.Object.Destroy(go);
		}
	}

	private bool IsEmpty()
	{
		bool flag = false;
		lock (mReqs)
		{
			return mReqs.Count == 0;
		}
	}

	public void SetClip(int min_y, int max_y)
	{
		if (grassMat.HasProperty("_Clip"))
		{
			grassMat.SetVector("_Clip", new Vector4(min_y, max_y));
		}
		triMat.SetVector("_Clip", new Vector4(min_y, max_y));
	}

	public void ProcessReqs(Queue<RGRegion> reqs)
	{
		while (true)
		{
			RGRegion rGRegion = null;
			lock (reqs)
			{
				if (mReqs.Count == 0)
				{
					break;
				}
				rGRegion = reqs.Dequeue();
			}
			OutpuRegion outpuRegion = new OutpuRegion();
			foreach (RGLODQuadTreeNode node in rGRegion.nodes)
			{
				List<RGChunk> chunks = node.GetChunks(mData);
				if (chunks == null)
				{
					continue;
				}
				foreach (RGChunk item in chunks)
				{
					foreach (KeyValuePair<int, RedGrassInstance> grass in item.grasses)
					{
						if (grass.Value.Layer == 0)
						{
							_grasses.Add(grass.Value);
						}
						else
						{
							_triGrasses.Add(grass.Value);
						}
						if (!item.HGrass.ContainsKey(grass.Key))
						{
							continue;
						}
						foreach (RedGrassInstance item2 in item.HGrass[grass.Key])
						{
							if (item2.Layer == 0)
							{
								_grasses.Add(item2);
							}
							else
							{
								_triGrasses.Add(item2);
							}
						}
					}
				}
				if (node.LOD >= mEvni.LODDensities.Length)
				{
					node.LOD = mEvni.LODDensities.Length - 1;
				}
				if (node.LOD < 0)
				{
					node.LOD = 0;
				}
				RedGrassMeshComputer.ComputeMesh(_grasses, _triGrasses, mEvni.Density * mEvni.LODDensities[node.LOD]);
				_grasses.Clear();
				_triGrasses.Clear();
				OutputGroup outputGroup = new OutputGroup();
				outputGroup.node = node;
				int num = RedGrassMeshComputer.s_Output.BillboardCount;
				int num2 = 0;
				while (num > 0)
				{
					int num3 = Mathf.Min(num, mEvni.MeshQuadMaxCount);
					Vector3[] array = new Vector3[num3 * 4];
					Vector3[] array2 = new Vector3[num3 * 4];
					Vector2[] array3 = new Vector2[num3 * 4];
					Vector2[] array4 = new Vector2[num3 * 4];
					Color32[] array5 = new Color32[num3 * 4];
					int[] array6 = new int[num3 * 6];
					Array.Copy(RedGrassMeshComputer.s_Output.Verts, num2 * 4, array, 0, num3 * 4);
					Array.Copy(RedGrassMeshComputer.s_Output.Norms, num2 * 4, array2, 0, num3 * 4);
					Array.Copy(RedGrassMeshComputer.s_Output.UVs, num2 * 4, array3, 0, num3 * 4);
					Array.Copy(RedGrassMeshComputer.s_Output.UV2s, num2 * 4, array4, 0, num3 * 4);
					Array.Copy(RedGrassMeshComputer.s_Output.Color32s, num2 * 4, array5, 0, num3 * 4);
					Array.Copy(RedGrassMeshComputer.s_Output.Indices, array6, num3 * 6);
					outputGroup.billboard.mVerts.Add(array);
					outputGroup.billboard.mNorms.Add(array2);
					outputGroup.billboard.mUVs.Add(array3);
					outputGroup.billboard.mUV2s.Add(array4);
					outputGroup.billboard.mColors32.Add(array5);
					outputGroup.billboard.mIndices.Add(array6);
					num -= mEvni.MeshQuadMaxCount;
					num2 += mEvni.MeshQuadMaxCount;
				}
				int num4 = RedGrassMeshComputer.s_Output.TriquadCount;
				num2 = RedGrassMeshComputer.s_Output.BillboardCount;
				while (num4 > 0)
				{
					int num5 = Mathf.Min(num4, mEvni.MeshQuadMaxCount);
					Vector3[] array7 = new Vector3[num5 * 4];
					Vector3[] array8 = new Vector3[num5 * 4];
					Vector2[] array9 = new Vector2[num5 * 4];
					Vector2[] array10 = new Vector2[num5 * 4];
					Color32[] array11 = new Color32[num5 * 4];
					int[] array12 = new int[num5 * 6];
					Array.Copy(RedGrassMeshComputer.s_Output.Verts, num2 * 4, array7, 0, num5 * 4);
					Array.Copy(RedGrassMeshComputer.s_Output.Norms, num2 * 4, array8, 0, num5 * 4);
					Array.Copy(RedGrassMeshComputer.s_Output.UVs, num2 * 4, array9, 0, num5 * 4);
					Array.Copy(RedGrassMeshComputer.s_Output.UV2s, num2 * 4, array10, 0, num5 * 4);
					Array.Copy(RedGrassMeshComputer.s_Output.Color32s, num2 * 4, array11, 0, num5 * 4);
					Array.Copy(RedGrassMeshComputer.s_Output.Indices, array12, num5 * 6);
					outputGroup.tri.mVerts.Add(array7);
					outputGroup.tri.mNorms.Add(array8);
					outputGroup.tri.mUVs.Add(array9);
					outputGroup.tri.mUV2s.Add(array10);
					outputGroup.tri.mColors32.Add(array11);
					outputGroup.tri.mIndices.Add(array12);
					num4 -= mEvni.MeshQuadMaxCount;
					num2 += mEvni.MeshQuadMaxCount;
				}
				outpuRegion.groups.Add(outputGroup);
				outpuRegion.oldGos = rGRegion.oldGos;
			}
			lock (mOutputs)
			{
				mOutputs.Add(outpuRegion);
			}
		}
	}

	public GameObject ComputeNow(RGLODQuadTreeNode node, RGDataSource data, float density)
	{
		try
		{
			List<RGChunk> chunks = node.GetChunks(data);
			if (chunks == null)
			{
				return null;
			}
			foreach (RGChunk item in chunks)
			{
				foreach (KeyValuePair<int, RedGrassInstance> grass in item.grasses)
				{
					if (grass.Value.Layer == 0)
					{
						_grasses.Add(grass.Value);
					}
					else
					{
						_triGrasses.Add(grass.Value);
					}
					if (!item.HGrass.ContainsKey(grass.Key))
					{
						continue;
					}
					foreach (RedGrassInstance item2 in item.HGrass[grass.Key])
					{
						if (item2.Layer == 0)
						{
							_grasses.Add(item2);
						}
						else
						{
							_triGrasses.Add(item2);
						}
					}
				}
			}
			RedGrassMeshComputer.ComputeMesh(_grasses, _triGrasses, density);
			_grasses.Clear();
			_triGrasses.Clear();
			OutputGroup outputGroup = new OutputGroup();
			int num = RedGrassMeshComputer.s_Output.BillboardCount;
			int num2 = 0;
			while (num > 0)
			{
				int num3 = Mathf.Min(num, mEvni.MeshQuadMaxCount);
				Vector3[] array = new Vector3[num3 * 4];
				Vector3[] array2 = new Vector3[num3 * 4];
				Vector2[] array3 = new Vector2[num3 * 4];
				Vector2[] array4 = new Vector2[num3 * 4];
				Color32[] array5 = new Color32[num3 * 4];
				int[] array6 = new int[num3 * 6];
				Array.Copy(RedGrassMeshComputer.s_Output.Verts, num2 * 4, array, 0, num3 * 4);
				Array.Copy(RedGrassMeshComputer.s_Output.Norms, num2 * 4, array2, 0, num3 * 4);
				Array.Copy(RedGrassMeshComputer.s_Output.UVs, num2 * 4, array3, 0, num3 * 4);
				Array.Copy(RedGrassMeshComputer.s_Output.UV2s, num2 * 4, array4, 0, num3 * 4);
				Array.Copy(RedGrassMeshComputer.s_Output.Color32s, num2 * 4, array5, 0, num3 * 4);
				Array.Copy(RedGrassMeshComputer.s_Output.Indices, array6, num3 * 6);
				outputGroup.billboard.mVerts.Add(array);
				outputGroup.billboard.mNorms.Add(array2);
				outputGroup.billboard.mUVs.Add(array3);
				outputGroup.billboard.mUV2s.Add(array4);
				outputGroup.billboard.mColors32.Add(array5);
				outputGroup.billboard.mIndices.Add(array6);
				num -= mEvni.MeshQuadMaxCount;
				num2 += mEvni.MeshQuadMaxCount;
			}
			int num4 = RedGrassMeshComputer.s_Output.TriquadCount;
			num2 = RedGrassMeshComputer.s_Output.BillboardCount;
			while (num4 > 0)
			{
				int num5 = Mathf.Min(num4, mEvni.MeshQuadMaxCount);
				Vector3[] array7 = new Vector3[num5 * 4];
				Vector3[] array8 = new Vector3[num5 * 4];
				Vector2[] array9 = new Vector2[num5 * 4];
				Vector2[] array10 = new Vector2[num5 * 4];
				Color32[] array11 = new Color32[num5 * 4];
				int[] array12 = new int[num5 * 6];
				Array.Copy(RedGrassMeshComputer.s_Output.Verts, num2 * 4, array7, 0, num5 * 4);
				Array.Copy(RedGrassMeshComputer.s_Output.Norms, num2 * 4, array8, 0, num5 * 4);
				Array.Copy(RedGrassMeshComputer.s_Output.UVs, num2 * 4, array9, 0, num5 * 4);
				Array.Copy(RedGrassMeshComputer.s_Output.UV2s, num2 * 4, array10, 0, num5 * 4);
				Array.Copy(RedGrassMeshComputer.s_Output.Color32s, num2 * 4, array11, 0, num5 * 4);
				Array.Copy(RedGrassMeshComputer.s_Output.Indices, array12, num5 * 6);
				outputGroup.tri.mVerts.Add(array7);
				outputGroup.tri.mNorms.Add(array8);
				outputGroup.tri.mUVs.Add(array9);
				outputGroup.tri.mUV2s.Add(array10);
				outputGroup.tri.mColors32.Add(array11);
				outputGroup.tri.mIndices.Add(array12);
				num4 -= mEvni.MeshQuadMaxCount;
				num2 += mEvni.MeshQuadMaxCount;
			}
			string desc = "Grass Batch [" + node.xIndex.ToString() + ", " + node.zIndex.ToString() + "] _ LOD [" + node.LOD + "]";
			GameObject gameObject = (node.gameObject = CreateBatchGos(desc, outputGroup.billboard, outputGroup.tri));
			if (gameObject != null)
			{
				gameObject.transform.parent = base.transform;
				gameObject.transform.localPosition = Vector3.zero;
				if (gameObject.activeSelf != node.visible)
				{
					gameObject.SetActive(node.visible);
				}
			}
			return gameObject;
		}
		catch
		{
			return null;
		}
	}

	private void Process()
	{
		try
		{
			while (mRunning)
			{
				if (!mActive)
				{
					Thread.Sleep(5);
					continue;
				}
				lock (procLockObj)
				{
					ProcessReqs(mReqs);
				}
				mStart = false;
				Thread.Sleep(5);
			}
		}
		catch
		{
			StopProcess();
			mThread = new Thread(Process);
			mThread.Start();
		}
	}

	private void Awake()
	{
		mReqs = new Queue<RGRegion>();
		mOutputs = new List<OutpuRegion>();
		RedGrassMeshComputer.Init();
	}

	private void Start()
	{
		mThread = new Thread(Process);
		mThread.Start();
	}

	private void OnDestroy()
	{
		mRunning = false;
	}

	private void Update()
	{
		if (mActive && !mStart && !mIsGenerating)
		{
			StartCoroutine("Generate");
		}
	}

	private IEnumerator Generate()
	{
		mIsGenerating = true;
		int cnt = 0;
		foreach (OutpuRegion opr in mOutputs)
		{
			List<OutputGroup> group = opr.groups;
			for (int j = 0; j < group.Count; j++)
			{
				OutputGroup opg = group[j];
				if (opg.node.IsOutOfDate())
				{
					continue;
				}
				opg.node.Dirty(mData, dirty: false);
				RGLODQuadTreeNode node = opg.node;
				string desc = "Grass Batch [" + node.xIndex.ToString() + ", " + node.zIndex.ToString() + "] _ LOD [" + node.LOD + "]";
				GameObject go = (node.gameObject = CreateBatchGos(desc, opg.billboard, opg.tri));
				if (go != null)
				{
					go.transform.parent = base.transform;
					go.transform.localPosition = Vector3.zero;
					if (go.activeSelf != node.visible)
					{
						go.SetActive(node.visible);
					}
					if (cnt % 8 == 0)
					{
						yield return 0;
					}
					cnt++;
				}
			}
			ClearMeshes(opr.oldGos);
		}
		if (onReqsFinished != null)
		{
			onReqsFinished();
		}
		mOutputs.Clear();
		mIsGenerating = false;
		mActive = false;
	}

	private GameObject CreateBatchGos(string desc, OutputStruct billboard, OutputStruct tri)
	{
		GameObject gameObject = null;
		for (int i = 0; i < billboard.GetCount(); i++)
		{
			GameObject gameObject2 = new GameObject();
			if (gameObject != null)
			{
				gameObject2.transform.parent = gameObject.transform;
				gameObject2.name = "Billboard";
			}
			else
			{
				gameObject = gameObject2;
				gameObject2.name = desc + " Billboard";
			}
			MeshFilter meshFilter = gameObject2.AddComponent<MeshFilter>();
			gameObject2.AddComponent<MeshRenderer>().material = grassMat;
			meshFilter.mesh.vertices = billboard.mVerts[i];
			meshFilter.mesh.triangles = billboard.mIndices[i];
			meshFilter.mesh.normals = billboard.mNorms[i];
			meshFilter.mesh.uv = billboard.mUVs[i];
			meshFilter.mesh.uv2 = billboard.mUV2s[i];
			meshFilter.mesh.colors32 = billboard.mColors32[i];
		}
		for (int j = 0; j < tri.GetCount(); j++)
		{
			GameObject gameObject3 = new GameObject();
			if (gameObject != null)
			{
				gameObject3.transform.parent = gameObject.transform;
				gameObject3.name = "Tri-angle";
			}
			else
			{
				gameObject = gameObject3;
				gameObject3.name = desc + " Tri-angle";
			}
			MeshFilter meshFilter2 = gameObject3.AddComponent<MeshFilter>();
			gameObject3.AddComponent<MeshRenderer>().material = triMat;
			meshFilter2.mesh.vertices = tri.mVerts[j];
			meshFilter2.mesh.triangles = tri.mIndices[j];
			meshFilter2.mesh.normals = tri.mNorms[j];
			meshFilter2.mesh.uv = tri.mUVs[j];
			meshFilter2.mesh.uv2 = tri.mUV2s[j];
			meshFilter2.mesh.colors32 = tri.mColors32[j];
		}
		return gameObject;
	}
}

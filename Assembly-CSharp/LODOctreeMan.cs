using System;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;

public class LODOctreeMan
{
	public const int Lod0NodeShift = 5;

	public const int Lod0NodeSize = 32;

	public const int MaxLod = 4;

	private const int __defxLodRootChunkCount = 8;

	private const int __defyLodRootChunkCount = 4;

	private const int __defzLodRootChunkCount = 8;

	public List<LODOctreeNode> nodeListVisible = new List<LODOctreeNode>();

	public List<LODOctreeNode> nodeListInvisible = new List<LODOctreeNode>();

	public List<LODOctreeNode> nodeListMeshDestroy = new List<LODOctreeNode>();

	public List<IntVector4> nodePosListCollider = new List<IntVector4>();

	public List<IntVector4> nodePosListVisible = new List<IntVector4>();

	public List<IntVector4> nodePosListInvisible = new List<IntVector4>();

	public List<IntVector4> nodePosListMeshDestroy = new List<IntVector4>();

	public List<IntVector4> posListMeshDestroy = new List<IntVector4>();

	public List<IntVector4> posListRootPrePos = new List<IntVector4>();

	public List<IntVector4> posListRootNewPos = new List<IntVector4>();

	private static int __xLodRootChunkCount = 8;

	private static int __yLodRootChunkCount = 4;

	private static int __zLodRootChunkCount = 8;

	public static object obj4LockLod = new object();

	public static readonly Vector3 InitPos = new Vector3(0f, -999999f, 0f);

	private Thread _threadLodRefresh;

	private Vector3 _curCamPos;

	private Vector3 _lastCamPos;

	private bool _bThreadOn = true;

	private int _sqrRefreshThreshold;

	private int _treeSideLen;

	private Bounds[] _viewBoundsLod;

	internal IntVector3 _viewBoundsSize;

	private LODOctree[] _lodTrees;

	private LODOctreeNode[][,,] _lodTreeNodes;

	public bool HandlerExistMeshCreated => this.handlerNodeMeshCreated != null;

	public bool HandlerExistMeshDestroy => this.handlerNodeMeshDestroy != null;

	public bool HandlerExistMeshPhyxReady => this.handlerNodePhyxReady != null;

	public bool HandlerExistRootPosChange => this.handlerRootPosChange != null;

	public static int _maxLod { get; set; }

	public static int _xLodRootChunkCount => __xLodRootChunkCount;

	public static int _yLodRootChunkCount => __yLodRootChunkCount;

	public static int _zLodRootChunkCount => __zLodRootChunkCount;

	public static int _xChunkCount => __xLodRootChunkCount << _maxLod;

	public static int _yChunkCount => __yLodRootChunkCount << _maxLod;

	public static int _zChunkCount => __zLodRootChunkCount << _maxLod;

	public static int _xLod0VoxelCount => __xLodRootChunkCount << 5;

	public static int _yLod0VoxelCount => __yLodRootChunkCount << 5;

	public static int _zLod0VoxelCount => __zLodRootChunkCount << 5;

	private static int _xVoxelCount => __xLodRootChunkCount << _maxLod + 5;

	private static int _yVoxelCount => __yLodRootChunkCount << _maxLod + 5;

	private static int _zVoxelCount => __zLodRootChunkCount << _maxLod + 5;

	public static LODOctreeMan self { get; private set; }

	public LODOctreeNode[][,,] LodTreeNodes => _lodTreeNodes;

	public Bounds _viewBounds => _viewBoundsLod[_maxLod];

	public Bounds _Lod0ViewBounds => _viewBoundsLod[0];

	public bool IsFirstRefreshed
	{
		get
		{
			float y = _lastCamPos.y;
			Vector3 initPos = InitPos;
			return y > initPos.y + 1f;
		}
	}

	public Vector3 LastRefreshPos => _lastCamPos;

	public Transform Observer { get; set; }

	public event DelegateNodeMeshCreated handlerNodeMeshCreated;

	public event DelegateNodeMeshDestroy handlerNodeMeshDestroy;

	public event DelegateNodePhyxReady handlerNodePhyxReady;

	public event DelegateRootPosChange handlerRootPosChange;

	private event Action _procPostInit;

	private event Action _procPostUpdate;

	private event Action _procPostRefresh;

	public LODOctreeMan(ILODNodeDataMan[] lodNodeDataMans, int maxlod, int refreshThreshold = 1, Transform observer = null)
	{
		self = this;
		_maxLod = maxlod;
		Observer = observer;
		int xChunkCount = _xChunkCount;
		int yChunkCount = _yChunkCount;
		int zChunkCount = _zChunkCount;
		int xVoxelCount = _xVoxelCount;
		int yVoxelCount = _yVoxelCount;
		int zVoxelCount = _zVoxelCount;
		_curCamPos = InitPos;
		_lastCamPos = InitPos;
		_sqrRefreshThreshold = refreshThreshold * refreshThreshold;
		_treeSideLen = 32 << _maxLod;
		_viewBoundsSize = new IntVector3(xVoxelCount, yVoxelCount, zVoxelCount);
		_viewBoundsLod = new Bounds[_maxLod + 1];
		int num = 0;
		for (num = _maxLod; num >= 0; num--)
		{
			int num2 = _maxLod - num;
			ref Bounds reference = ref _viewBoundsLod[num];
			reference = new Bounds(InitPos, new Vector3(xVoxelCount >> num2, yVoxelCount >> num2, zVoxelCount >> num2));
		}
		LODOctreeNode.ClearNodeDataCreation();
		foreach (ILODNodeDataMan iLODNodeDataMan in lodNodeDataMans)
		{
			LODOctreeNode.AddNodeDataCreation(iLODNodeDataMan);
			iLODNodeDataMan.LodMan = this;
			this._procPostInit = (Action)Delegate.Combine(this._procPostInit, new Action(iLODNodeDataMan.ProcPostLodInit));
			this._procPostUpdate = (Action)Delegate.Combine(this._procPostUpdate, new Action(iLODNodeDataMan.ProcPostLodUpdate));
			this._procPostRefresh = (Action)Delegate.Combine(this._procPostRefresh, new Action(iLODNodeDataMan.ProcPostLodRefresh));
		}
		LODOctreeNode.InitHalfLen();
		_lodTreeNodes = new LODOctreeNode[_maxLod + 1][,,];
		for (num = 0; num <= _maxLod; num++)
		{
			_lodTreeNodes[num] = new LODOctreeNode[xChunkCount >> num, yChunkCount >> num, zChunkCount >> num];
		}
		_lodTrees = new LODOctree[_xLodRootChunkCount * _yLodRootChunkCount * _zLodRootChunkCount];
		int num3 = 0;
		for (int j = 0; j < _xLodRootChunkCount; j++)
		{
			for (int k = 0; k < _yLodRootChunkCount; k++)
			{
				for (int l = 0; l < _zLodRootChunkCount; l++)
				{
					_lodTrees[num3] = new LODOctree(this, _maxLod, new IntVector3(j, k, l), new IntVector3(_treeSideLen, _treeSideLen, _treeSideLen));
					_lodTrees[num3].FillTreeNodeArray(ref _lodTreeNodes);
					num3++;
				}
			}
		}
		if (this._procPostInit != null)
		{
			this._procPostInit();
		}
		StartLodThread();
	}

	public bool Contains(IntVector4 node)
	{
		return nodePosListCollider.Contains(node);
	}

	public bool Contains(IntVector2 node)
	{
		return nodePosListCollider.Find((IntVector4 ret) => ret.x == node.x && ret.z == node.y) != null;
	}

	public IntVector4[] GetNodes(IntVector2 node)
	{
		return nodePosListCollider.FindAll((IntVector4 ret) => ret.x == node.x && ret.z == node.y).ToArray();
	}

	public void DispatchNodeEventMeshCreated(IntVector4 nodePosLod)
	{
		try
		{
			this.handlerNodeMeshCreated(nodePosLod);
		}
		catch (Exception ex)
		{
			Debug.LogError(string.Concat("[LODEVENT]:Error in handlerNodeMeshCreated", nodePosLod, ex));
		}
	}

	public void DispatchNodeEventPhyxReady(IntVector4 nodePosLod)
	{
		if (!nodePosListCollider.Contains(nodePosLod))
		{
			nodePosListCollider.Add(nodePosLod);
		}
		try
		{
			this.handlerNodePhyxReady(nodePosLod);
		}
		catch (Exception ex)
		{
			Debug.LogError(string.Concat("[LODEVENT]:Error in handlerNodePhyxReady", nodePosLod, ex));
		}
	}

	private void DispatchNodeEvents()
	{
		int count = posListMeshDestroy.Count;
		if (count > 0)
		{
			lock (posListMeshDestroy)
			{
				count = posListMeshDestroy.Count;
				for (int i = 0; i < count; i++)
				{
					if (nodePosListCollider.Contains(posListMeshDestroy[i]))
					{
						nodePosListCollider.Remove(posListMeshDestroy[i]);
					}
					try
					{
						this.handlerNodeMeshDestroy(posListMeshDestroy[i]);
					}
					catch (Exception ex)
					{
						Debug.LogError(string.Concat("[LODEVENT]:Error in handlerNodeMeshDestroy", posListMeshDestroy[i], ex));
					}
				}
				posListMeshDestroy.Clear();
			}
		}
		count = posListRootPrePos.Count;
		if (count > 0)
		{
			lock (posListRootPrePos)
			{
				count = posListRootPrePos.Count;
				for (int j = 0; j < count; j++)
				{
					try
					{
						this.handlerRootPosChange(posListRootPrePos[j], posListRootNewPos[j]);
					}
					catch (Exception ex2)
					{
						Debug.LogError(string.Concat("[LODEVENT]:Error in handlerRootPosChange", posListRootPrePos[j], posListRootNewPos[j], ex2));
					}
				}
				posListRootPrePos.Clear();
				posListRootNewPos.Clear();
			}
		}
		count = nodeListInvisible.Count;
		if (count > 0)
		{
			lock (nodeListInvisible)
			{
				count = nodeListInvisible.Count;
				for (int k = 0; k < count; k++)
				{
					nodeListInvisible[k].HandleInvisible(nodePosListInvisible[k]);
				}
				nodeListInvisible.Clear();
				nodePosListInvisible.Clear();
			}
		}
		count = nodeListVisible.Count;
		if (count > 0)
		{
			lock (nodeListVisible)
			{
				count = nodeListVisible.Count;
				for (int l = 0; l < count; l++)
				{
					nodeListVisible[l].HandleVisible(nodePosListVisible[l]);
				}
				nodeListVisible.Clear();
				nodePosListVisible.Clear();
			}
		}
		count = nodeListMeshDestroy.Count;
		if (count <= 0)
		{
			return;
		}
		lock (nodeListMeshDestroy)
		{
			count = nodeListMeshDestroy.Count;
			for (int m = 0; m < count; m++)
			{
				nodeListMeshDestroy[m].HandleMeshDestroy(nodePosListMeshDestroy[m]);
			}
			nodeListMeshDestroy.Clear();
			nodePosListMeshDestroy.Clear();
		}
	}

	public void AttachEvents(DelegateNodeMeshCreated meshCreatedHandler, DelegateNodeMeshDestroy meshDestroyHandler, DelegateNodePhyxReady phyxReadyHandler, DelegateRootPosChange posChangeHandler)
	{
		if (meshCreatedHandler != null)
		{
			this.handlerNodeMeshCreated = (DelegateNodeMeshCreated)Delegate.Combine(this.handlerNodeMeshCreated, meshCreatedHandler);
		}
		if (meshDestroyHandler != null)
		{
			this.handlerNodeMeshDestroy = (DelegateNodeMeshDestroy)Delegate.Combine(this.handlerNodeMeshDestroy, meshDestroyHandler);
		}
		if (phyxReadyHandler != null)
		{
			this.handlerNodePhyxReady = (DelegateNodePhyxReady)Delegate.Combine(this.handlerNodePhyxReady, phyxReadyHandler);
		}
		if (posChangeHandler != null)
		{
			this.handlerRootPosChange = (DelegateRootPosChange)Delegate.Combine(this.handlerRootPosChange, posChangeHandler);
		}
	}

	public void DetachEvents(DelegateNodeMeshCreated meshCreatedHandler, DelegateNodeMeshDestroy meshDestroyHandler, DelegateNodePhyxReady phyxReadyHandler, DelegateRootPosChange posChangeHandler)
	{
		if (meshCreatedHandler != null)
		{
			this.handlerNodeMeshCreated = (DelegateNodeMeshCreated)Delegate.Remove(this.handlerNodeMeshCreated, meshCreatedHandler);
		}
		if (meshDestroyHandler != null)
		{
			this.handlerNodeMeshDestroy = (DelegateNodeMeshDestroy)Delegate.Remove(this.handlerNodeMeshDestroy, meshDestroyHandler);
		}
		if (phyxReadyHandler != null)
		{
			this.handlerNodePhyxReady = (DelegateNodePhyxReady)Delegate.Remove(this.handlerNodePhyxReady, phyxReadyHandler);
		}
		if (posChangeHandler != null)
		{
			this.handlerRootPosChange = (DelegateRootPosChange)Delegate.Remove(this.handlerRootPosChange, posChangeHandler);
		}
	}

	public void AttachNodeEvents(DelegateNodeVisible visibleHandler, DelegateNodeInvisible invisibleHandler, DelegateNodeMeshCreated meshCreatedHandler, DelegateNodeMeshDestroy meshDestroyHandler, DelegateNodePhyxReady phyxReadyHandler, Vector3 pos, int nodeLod = 0)
	{
		LODOctreeNode parentNodeWithPos = GetParentNodeWithPos(pos, nodeLod);
		if (visibleHandler != null)
		{
			parentNodeWithPos.handlerVisible += visibleHandler;
		}
		if (invisibleHandler != null)
		{
			parentNodeWithPos.handlerInvisible += invisibleHandler;
		}
		if (meshCreatedHandler != null)
		{
			parentNodeWithPos.handlerMeshCreated += meshCreatedHandler;
		}
		if (meshDestroyHandler != null)
		{
			parentNodeWithPos.handlerMeshDestroy += meshDestroyHandler;
		}
		if (phyxReadyHandler != null)
		{
			parentNodeWithPos.handlerPhyxReady += phyxReadyHandler;
		}
	}

	public void DetachNodeEvents(DelegateNodeVisible visibleHandler, DelegateNodeInvisible invisibleHandler, DelegateNodeMeshCreated meshCreatedHandler, DelegateNodeMeshDestroy meshDestroyHandler, DelegateNodePhyxReady phyxReadyHandler, Vector3 pos, int nodeLod = 0)
	{
		LODOctreeNode parentNodeWithPos = GetParentNodeWithPos(pos, nodeLod);
		if (visibleHandler != null)
		{
			parentNodeWithPos.handlerVisible -= visibleHandler;
		}
		if (invisibleHandler != null)
		{
			parentNodeWithPos.handlerInvisible -= invisibleHandler;
		}
		if (meshCreatedHandler != null)
		{
			parentNodeWithPos.handlerMeshCreated -= meshCreatedHandler;
		}
		if (meshDestroyHandler != null)
		{
			parentNodeWithPos.handlerMeshDestroy -= meshDestroyHandler;
		}
		if (phyxReadyHandler != null)
		{
			parentNodeWithPos.handlerPhyxReady -= phyxReadyHandler;
		}
	}

	public bool IsPosVisible(IntVector4 pos)
	{
		return _viewBoundsLod[pos.w].Contains(pos.ToVector3());
	}

	public bool IsPosValid(IntVector4 pos)
	{
		if (pos.w != 0)
		{
			return false;
		}
		LODOctreeNode nodeWithCPos = GetNodeWithCPos(pos.x >> 5, pos.y >> 5, pos.z >> 5, pos.w);
		if (nodeWithCPos == null)
		{
			return false;
		}
		int num = nodeWithCPos._data.Length;
		for (int i = 0; i < num; i++)
		{
		}
		return true;
	}

	public LODOctreeNode GetNodeWithCPos(int cx, int cy, int cz, int lod)
	{
		int num = _xChunkCount >> lod;
		int num2 = _yChunkCount >> lod;
		int num3 = _zChunkCount >> lod;
		int num4 = (cx >> lod) % num;
		int num5 = (cy >> lod) % num2;
		int num6 = (cz >> lod) % num3;
		if (num4 < 0)
		{
			num4 += num;
		}
		if (num5 < 0)
		{
			num5 += num2;
		}
		if (num6 < 0)
		{
			num6 += num3;
		}
		LODOctreeNode lODOctreeNode = _lodTreeNodes[lod][num4, num5, num6];
		if (lODOctreeNode.CX == cx && lODOctreeNode.CY == cy && lODOctreeNode.CZ == cz)
		{
			return lODOctreeNode;
		}
		return null;
	}

	public LODOctreeNode GetParentNodeWithPos(Vector3 pos, int nodeLod = 0)
	{
		int num = Mathf.FloorToInt(pos.x / (float)_treeSideLen);
		int num2 = Mathf.FloorToInt(pos.y / (float)_treeSideLen);
		int num3 = Mathf.FloorToInt(pos.z / (float)_treeSideLen);
		int num4 = num % _xLodRootChunkCount;
		int num5 = num2 % _yLodRootChunkCount;
		int num6 = num3 % _zLodRootChunkCount;
		if (num4 < 0)
		{
			num4 += _xLodRootChunkCount;
		}
		if (num5 < 0)
		{
			num5 += _yLodRootChunkCount;
		}
		if (num6 < 0)
		{
			num6 += _zLodRootChunkCount;
		}
		int num7 = (num4 * _yLodRootChunkCount + num5) * _zLodRootChunkCount + num6;
		LODOctreeNode lODOctreeNode = _lodTrees[num7]._root;
		if (nodeLod == _maxLod)
		{
			return lODOctreeNode;
		}
		IntVector3 intVector = new IntVector3(pos.x - (float)(num * _treeSideLen), pos.y - (float)(num2 * _treeSideLen), pos.z - (float)(num3 * _treeSideLen));
		for (int num8 = _maxLod; num8 > nodeLod; num8--)
		{
			int num9 = LODOctreeNode._halfLens[num8];
			int num10 = 0;
			if (intVector.x >= num9)
			{
				num10++;
				intVector.x -= num9;
			}
			if (intVector.y >= num9)
			{
				num10 += 2;
				intVector.y -= num9;
			}
			if (intVector.z >= num9)
			{
				num10 += 4;
				intVector.z -= num9;
			}
			lODOctreeNode = lODOctreeNode._child[num10];
		}
		return lODOctreeNode;
	}

	public static void ResetRootChunkCount(int xCnt = 8, int yCnt = 4, int zCnt = 8)
	{
		__xLodRootChunkCount = xCnt;
		__yLodRootChunkCount = yCnt;
		__zLodRootChunkCount = zCnt;
	}

	private void ReqRefresh(Vector3 camPos)
	{
		lock (obj4LockLod)
		{
			_curCamPos = camPos;
		}
		if (this._procPostUpdate != null)
		{
			this._procPostUpdate();
		}
		DispatchNodeEvents();
	}

	public void Reset()
	{
		ReqRefresh(InitPos);
	}

	public bool ReqRefresh()
	{
		if (Observer == null)
		{
			return false;
		}
		ReqRefresh(Observer.position);
		return true;
	}

	public void ReqDestroy()
	{
		if (self != null)
		{
			_bThreadOn = false;
			try
			{
				_threadLodRefresh.Join();
				MonoBehaviour.print("[LOD]Thread stopped");
			}
			catch
			{
				MonoBehaviour.print("[LOD]Thread stopped with exception");
			}
			self = null;
		}
	}

	private void StartLodThread()
	{
		_threadLodRefresh = new Thread(threadLodRefreshExec);
		_threadLodRefresh.Start();
	}

	private void threadLodRefreshExec()
	{
		_bThreadOn = true;
		while (_bThreadOn)
		{
			try
			{
				ExecRefresh();
			}
			catch (Exception message)
			{
				Debug.LogWarning(message);
			}
			Thread.Sleep(8);
		}
		ExecDestroy();
	}

	private void ExecDestroy()
	{
		int num = _lodTrees.Length;
		for (int i = 0; i < num; i++)
		{
			_lodTrees[i].OnDestroy();
		}
		_lodTrees = null;
	}

	private void ExecRefresh()
	{
		Vector3 curCamPos;
		lock (obj4LockLod)
		{
			curCamPos = _curCamPos;
		}
		ExecRefresh(curCamPos);
		if (this._procPostRefresh != null)
		{
			this._procPostRefresh();
		}
	}

	private bool ExecRefresh(Vector3 camPos)
	{
		if (camPos.y <= -99999f)
		{
			_lastCamPos = camPos;
			return false;
		}
		if (new Vector3(camPos.x - _lastCamPos.x, camPos.y - _lastCamPos.y, camPos.z - _lastCamPos.z).sqrMagnitude > (float)_sqrRefreshThreshold)
		{
			for (int i = 0; i <= _maxLod; i++)
			{
				_viewBoundsLod[i].center = camPos;
			}
			for (int j = 0; j < _lodTrees.Length; j++)
			{
				_lodTrees[j].RefreshPos(camPos);
			}
			if (_maxLod > 0)
			{
				for (int k = 0; k < _lodTrees.Length; k++)
				{
					_lodTrees[k]._root.RefreshNodeLod(ref _viewBoundsLod, _maxLod - 1);
				}
			}
			else
			{
				for (int l = 0; l < _lodTrees.Length; l++)
				{
					_lodTrees[l]._root.SetReqLod(0);
				}
			}
			_lastCamPos = camPos;
			return true;
		}
		return false;
	}

	public void ClearNodes()
	{
		for (int i = 0; i < _lodTrees.Length; i++)
		{
			ClearNode(_lodTrees[i]._root);
		}
	}

	private void ClearNode(LODOctreeNode node)
	{
		if (node._data[0].IsEmpty)
		{
			node._data = null;
		}
		if (node._child != null)
		{
			for (int i = 0; i < 8; i++)
			{
				ClearNode(node._child[i]);
			}
		}
	}
}

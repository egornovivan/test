using System;
using System.Collections.Generic;
using UnityEngine;

public class LODOctreeNode
{
	private static int idxDataMax = 0;

	private static List<DelegateToCreateLODNodeData> handlerArrayCreateLODNodeData = new List<DelegateToCreateLODNodeData>();

	public static int[] _halfLens;

	private int _lod;

	private int _reqLod;

	private int _posx;

	private int _posy;

	private int _posz;

	public LODOctreeNode _parent;

	public LODOctreeNode[] _child;

	public ILODNodeData[] _data;

	public int CX => _posx >> 5;

	public int CY => _posy >> 5;

	public int CZ => _posz >> 5;

	public int Lod => _lod;

	public IntVector4 PosLod => new IntVector4(_posx, _posy, _posz, _lod);

	public IntVector4 ChunkPosLod => new IntVector4(_posx >> 5, _posy >> 5, _posz >> 5, _lod);

	public Vector3 Center
	{
		get
		{
			int num = _halfLens[_lod];
			return new Vector3(_posx + num, _posy + num, _posz + num);
		}
	}

	public bool IsInReq => _reqLod == _lod;

	public event DelegateNodeVisible handlerVisible;

	public event DelegateNodeInvisible handlerInvisible;

	public event DelegateNodeMeshCreated handlerMeshCreated;

	public event DelegateNodeMeshDestroy handlerMeshDestroy;

	public event DelegateNodePhyxReady handlerPhyxReady;

	public LODOctreeNode(LODOctreeNode parent, int lod, int posx, int posy, int posz)
	{
		_lod = lod;
		_reqLod = 255;
		_posx = posx;
		_posy = posy;
		_posz = posz;
		_parent = parent;
		_child = null;
		initChildren();
		createNodeData();
	}

	public void OnVisible()
	{
		if (this.handlerVisible != null)
		{
			lock (LODOctreeMan.self.nodeListVisible)
			{
				LODOctreeMan.self.nodeListVisible.Add(this);
				LODOctreeMan.self.nodePosListVisible.Add(PosLod);
			}
		}
	}

	public void OnInvisible()
	{
		if (this.handlerInvisible != null)
		{
			lock (LODOctreeMan.self.nodeListInvisible)
			{
				LODOctreeMan.self.nodeListInvisible.Add(this);
				LODOctreeMan.self.nodePosListInvisible.Add(PosLod);
			}
		}
		if (_data[VFVoxelTerrain.self.IdxInLODNodeData].IsEmpty)
		{
			return;
		}
		if (this.handlerMeshDestroy != null)
		{
			lock (LODOctreeMan.self.nodeListMeshDestroy)
			{
				LODOctreeMan.self.nodeListMeshDestroy.Add(this);
				LODOctreeMan.self.nodePosListMeshDestroy.Add(PosLod);
			}
		}
		if (LODOctreeMan.self.HandlerExistMeshDestroy)
		{
			lock (LODOctreeMan.self.posListMeshDestroy)
			{
				LODOctreeMan.self.posListMeshDestroy.Add(PosLod);
			}
		}
	}

	public void HandleVisible(IntVector4 nodePosLod)
	{
		if (this.handlerVisible != null)
		{
			try
			{
				this.handlerVisible(nodePosLod);
			}
			catch (Exception ex)
			{
				Debug.LogError(string.Concat("[LODEVENT]Error in HandleVisible", nodePosLod, ex));
			}
		}
	}

	public void HandleInvisible(IntVector4 nodePosLod)
	{
		if (this.handlerInvisible != null)
		{
			try
			{
				this.handlerInvisible(nodePosLod);
			}
			catch (Exception ex)
			{
				Debug.LogError(string.Concat("[LODEVENT]Error in HandleInvisible", nodePosLod, ex));
			}
		}
	}

	public void HandleMeshDestroy(IntVector4 nodePosLod)
	{
		if (this.handlerMeshDestroy != null)
		{
			try
			{
				this.handlerMeshDestroy(nodePosLod);
			}
			catch (Exception ex)
			{
				Debug.LogError(string.Concat("[LODEVENT]Error in HandleMeshDestroy", nodePosLod, ex));
			}
		}
	}

	public void OnMeshCreated()
	{
		if (this.handlerMeshCreated != null)
		{
			try
			{
				this.handlerMeshCreated(PosLod);
			}
			catch (Exception ex)
			{
				Debug.LogError(string.Concat("[LODEVENT]Error in handlerMeshCreated", PosLod, ex));
			}
		}
		if (LODOctreeMan.self.HandlerExistMeshCreated)
		{
			LODOctreeMan.self.DispatchNodeEventMeshCreated(PosLod);
		}
	}

	public void OnPhyxReady()
	{
		if (this.handlerPhyxReady != null)
		{
			try
			{
				this.handlerPhyxReady(PosLod);
			}
			catch (Exception ex)
			{
				Debug.LogError(string.Concat("[LODEVENT]Error in handlerPhyxReady", PosLod, ex));
			}
		}
		if (LODOctreeMan.self.HandlerExistMeshPhyxReady)
		{
			LODOctreeMan.self.DispatchNodeEventPhyxReady(PosLod);
		}
	}

	public void OnRootPreMove(int newPosx, int newPosy, int newPosz)
	{
		if (LODOctreeMan.self.HandlerExistRootPosChange)
		{
			lock (LODOctreeMan.self.posListRootPrePos)
			{
				LODOctreeMan.self.posListRootPrePos.Add(PosLod);
				LODOctreeMan.self.posListRootNewPos.Add(new IntVector4(newPosx, newPosy, newPosz, _lod));
			}
		}
	}

	public static void ClearNodeDataCreation()
	{
		handlerArrayCreateLODNodeData.Clear();
		idxDataMax = 0;
	}

	public static void AddNodeDataCreation(ILODNodeDataMan nodeDataMan)
	{
		handlerArrayCreateLODNodeData.Add(nodeDataMan.CreateLODNodeData);
		idxDataMax = handlerArrayCreateLODNodeData.Count;
		nodeDataMan.IdxInLODNodeData = idxDataMax - 1;
	}

	public static void CreateNodeData(LODOctreeNode node)
	{
		for (int i = 0; i < idxDataMax; i++)
		{
			node._data[i].OnDestroyNodeData();
		}
		node._data = null;
		if (node._child != null)
		{
			for (int j = 0; j < 8; j++)
			{
				DestroyNodeData(node._child[j]);
			}
		}
	}

	public static void DestroyNodeData(LODOctreeNode node)
	{
		for (int i = 0; i < idxDataMax; i++)
		{
			node._data[i].OnDestroyNodeData();
		}
		node._data = null;
		if (node._child != null)
		{
			for (int j = 0; j < 8; j++)
			{
				DestroyNodeData(node._child[j]);
			}
		}
	}

	public static void InitHalfLen()
	{
		_halfLens = new int[5];
		for (int i = 0; i <= 4; i++)
		{
			_halfLens[i] = 1 << 4 + i;
		}
	}

	public void Reposition(int newPosx, int newPosy, int newPosz)
	{
		if (_posx == newPosx && _posy == newPosy && _posz == newPosz)
		{
			return;
		}
		if (_parent == null)
		{
			OnRootPreMove(newPosx, newPosy, newPosz);
		}
		if (_reqLod == _lod)
		{
			OnInvisible();
		}
		_posx = newPosx;
		_posy = newPosy;
		_posz = newPosz;
		_reqLod = 255;
		if (_child != null)
		{
			int num = _halfLens[_lod];
			for (int i = 0; i < 8; i++)
			{
				_child[i].Reposition(_posx + num * (i & 1), _posy + num * ((i >> 1) & 1), _posz + num * ((i >> 2) & 1));
			}
		}
	}

	public void SetReqLod(int reqLod)
	{
		if (_reqLod == reqLod)
		{
			return;
		}
		for (int i = 0; i < idxDataMax; i++)
		{
			_data[i].UpdateTimeStamp();
		}
		int reqLod2 = _reqLod;
		_reqLod = reqLod;
		if (_reqLod == _lod)
		{
			if (reqLod2 != _lod)
			{
				OnVisible();
			}
			for (int j = 0; j < idxDataMax; j++)
			{
				_data[j].BegUpdateNodeData();
			}
		}
		else if (reqLod2 == _lod)
		{
			OnInvisible();
		}
	}

	public void RefreshNodeLod(ref Bounds[] viewBounds, int boundsLod)
	{
		int num = _halfLens[_lod] << 1;
		Vector3 vector = new Vector3((float)_posx + 0.1f, (float)_posy + 0.1f, (float)_posz + 0.1f);
		Vector3 vector2 = new Vector3((float)(_posx + num) - 0.1f, (float)(_posy + num) - 0.1f, (float)(_posz + num) - 0.1f);
		Vector3 min = viewBounds[boundsLod].min;
		Vector3 max = viewBounds[boundsLod].max;
		if (min.x >= vector2.x || min.y >= vector2.y || min.z >= vector2.z || max.x <= vector.x || max.y <= vector.y || max.z <= vector.z)
		{
			int familyReqLod = ((boundsLod + 1 <= _lod) ? (boundsLod + 1) : _lod);
			SetFamilyReqLod(familyReqLod);
		}
		else if (min.x <= vector.x && min.y <= vector.y && min.z <= vector.z && max.x >= vector2.x && max.y >= vector2.y && max.z >= vector2.z)
		{
			if (boundsLod > 0)
			{
				RefreshNodeLod(ref viewBounds, boundsLod - 1);
				return;
			}
			int familyReqLod2 = 0;
			SetFamilyReqLod(familyReqLod2);
		}
		else if (_child != null)
		{
			int num2 = ((boundsLod >= _child[0]._lod) ? (_child[0]._lod - 1) : boundsLod);
			if (num2 < 0)
			{
				int reqLod = 0;
				for (int i = 0; i < 8; i++)
				{
					_child[i].SetReqLod(reqLod);
				}
				SetReqLod(reqLod);
				return;
			}
			int num3 = boundsLod + 1;
			for (int j = 0; j < 8; j++)
			{
				_child[j].RefreshNodeLod(ref viewBounds, num2);
				num3 = ((num3 >= _child[j]._reqLod) ? _child[j]._reqLod : num3);
			}
			SetReqLod(num3);
		}
		else
		{
			Debug.LogError("[RefreshNodeLod]Never run here");
			SetReqLod(boundsLod);
		}
	}

	public void OnEndUpdateNodeData(ILODNodeData cdata)
	{
		for (int i = 0; i < idxDataMax; i++)
		{
			if (cdata == _data[i])
			{
				DestroyInactiveNodeData(i);
				break;
			}
		}
	}

	private void createNodeData()
	{
		_data = new ILODNodeData[idxDataMax];
		for (int i = 0; i < idxDataMax; i++)
		{
			_data[i] = handlerArrayCreateLODNodeData[i](this);
		}
	}

	private void initChildren()
	{
		if (_lod != 0)
		{
			_child = new LODOctreeNode[8];
			int num = _halfLens[_lod];
			for (int i = 0; i < 8; i++)
			{
				_child[i] = new LODOctreeNode(this, _lod - 1, _posx + num * (i & 1), _posy + num * ((i >> 1) & 1), _posz + num * ((i >> 2) & 1));
			}
		}
	}

	private void SetFamilyReqLod(int reqLod)
	{
		if (_child != null)
		{
			for (int i = 0; i < 8; i++)
			{
				_child[i].SetFamilyReqLod(reqLod);
			}
		}
		SetReqLod(reqLod);
	}

	private void DestroyChildNodeData(int idxData, int reqLod)
	{
		for (int i = 0; i < 8; i++)
		{
			if (reqLod != _child[i]._lod)
			{
				_child[i]._data[idxData].OnDestroyNodeData();
			}
			if (_child[i]._child != null)
			{
				_child[i].DestroyChildNodeData(idxData, reqLod);
			}
		}
	}

	private void DestroyParentNodeData(int idxData, int reqLod)
	{
		if (_parent._child[0]._data[idxData].IsIdle && _parent._child[1]._data[idxData].IsIdle && _parent._child[2]._data[idxData].IsIdle && _parent._child[3]._data[idxData].IsIdle && _parent._child[4]._data[idxData].IsIdle && _parent._child[5]._data[idxData].IsIdle && _parent._child[6]._data[idxData].IsIdle && _parent._child[7]._data[idxData].IsIdle)
		{
			if (reqLod != _parent._lod)
			{
				_parent._data[idxData].OnDestroyNodeData();
			}
			if (_parent._parent != null)
			{
				_parent.DestroyParentNodeData(idxData, reqLod);
			}
		}
	}

	private void DestroyInactiveNodeData(int idxData)
	{
		if (_reqLod != _lod)
		{
			_data[idxData].OnDestroyNodeData();
		}
		if (_child != null)
		{
			DestroyChildNodeData(idxData, _reqLod);
		}
		if (_parent != null && _parent._child[0]._reqLod <= _reqLod && _parent._child[1]._reqLod <= _reqLod && _parent._child[2]._reqLod <= _reqLod && _parent._child[3]._reqLod <= _reqLod && _parent._child[4]._reqLod <= _reqLod && _parent._child[5]._reqLod <= _reqLod && _parent._child[6]._reqLod <= _reqLod && _parent._child[7]._reqLod <= _reqLod)
		{
			DestroyParentNodeData(idxData, _reqLod);
		}
	}
}

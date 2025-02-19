using System;
using System.Collections.Generic;
using UnityEngine;

public class Block45LODNodeData : ILODNodeData
{
	public delegate void ProcOnWriteVoxel(LODOctreeNode node, byte oldVol, byte newVol, int idxVol);

	public const int BuildStep_NotInBuild = 0;

	public const int BuildStep_StartDataLoading = 1;

	public const int BuildStep_FinDataLoading = 2;

	public const int BuildStep_StartGoCreating = 3;

	public const int BuildStepMask = 15;

	public const int StampMask = -16;

	private const int Block45OctNodeStatus_Idle = 0;

	private const int Block45OctNodeStatus_InBuild = 1;

	private IVxChunkHelperProc _helperProc;

	private LODOctreeNode _node;

	private IntVector4 _chunkPosLod = new IntVector4(0, 0, 0, -1);

	private int _stampOfUpdating;

	private List<Block45OctNode> _lstBlock45Datas;

	private List<int> _lstBlock45DatasStatus;

	private Vector3 Pos => new Vector3(_chunkPosLod.x << 5, _chunkPosLod.y << 5, _chunkPosLod.z << 5);

	public int LOD => _chunkPosLod.w;

	public IntVector4 ChunkPosLod => _chunkPosLod;

	public int StampOfChnkUpdating => _stampOfUpdating;

	public bool IsEmpty => IsAllOctNodeEmpty();

	public bool IsIdle => BuildStep == 0;

	public bool IsReady => BuildStep == 0;

	public bool IsInReq => _node.IsInReq;

	public int BuildStep
	{
		get
		{
			return _stampOfUpdating & 0xF;
		}
		set
		{
			_stampOfUpdating = (_stampOfUpdating & -16) | value;
		}
	}

	public IVxChunkHelperProc HelperProc
	{
		set
		{
			_helperProc = value;
		}
	}

	public IntVector4 ChunkPosLod_w
	{
		set
		{
			_chunkPosLod = value;
		}
	}

	public Block45LODNodeData(LODOctreeNode node)
	{
		_node = node;
	}

	public bool IsNodePosChange()
	{
		return _node.CX != _chunkPosLod.x || _node.CY != _chunkPosLod.y || _node.CZ != _chunkPosLod.z || _node.Lod != _chunkPosLod.w;
	}

	private bool IsAllOctNodeEmpty()
	{
		if (_lstBlock45Datas == null)
		{
			return true;
		}
		int count = _lstBlock45Datas.Count;
		for (int i = 0; i < count; i++)
		{
			if ((object)_lstBlock45Datas[i].ChunkGo != null)
			{
				return false;
			}
		}
		return true;
	}

	public bool IsAllOctNodeReady()
	{
		if (!IsIdle)
		{
			return false;
		}
		if (_lstBlock45Datas == null)
		{
			return true;
		}
		int count = _lstBlock45Datas.Count;
		for (int i = 0; i < count; i++)
		{
			Block45ChunkGo chunkGo = _lstBlock45Datas[i].ChunkGo;
			if ((object)chunkGo != null && (object)chunkGo._mc.sharedMesh == null)
			{
				return false;
			}
		}
		return true;
	}

	public Block45OctNode PickNodeToSetCol()
	{
		if (!IsIdle)
		{
			return null;
		}
		if (_lstBlock45Datas == null)
		{
			return null;
		}
		int count = _lstBlock45Datas.Count;
		for (int i = 0; i < count; i++)
		{
			Block45ChunkGo chunkGo = _lstBlock45Datas[i].ChunkGo;
			if ((object)chunkGo != null && (object)chunkGo._mc.sharedMesh == null)
			{
				return _lstBlock45Datas[i];
			}
		}
		return null;
	}

	public void AddOctNode(Block45OctNode octNode)
	{
		if (_lstBlock45Datas == null)
		{
			_lstBlock45Datas = new List<Block45OctNode>();
			_lstBlock45DatasStatus = new List<int>();
		}
		_lstBlock45Datas.Add(octNode);
		_lstBlock45DatasStatus.Add(0);
		if (IsInReq && octNode.VecData != null)
		{
			BuildStep = 3;
			_lstBlock45DatasStatus[_lstBlock45DatasStatus.Count - 1] = 1;
			_helperProc.SurfExtractor.AddSurfExtractReq(SurfExtractReqB45.Get(octNode.GetStamp(), octNode, _helperProc.ChunkProcPostGenMesh));
		}
	}

	public void SetBlock45Datas(List<Block45OctNode> lstDatas)
	{
		if (_lstBlock45Datas != null)
		{
			int count = _lstBlock45Datas.Count;
			for (int i = 0; i < count; i++)
			{
				_lstBlock45Datas[i].DetachLODNode();
			}
		}
		_lstBlock45Datas = null;
		_lstBlock45DatasStatus = null;
		if (lstDatas != null && lstDatas.Count > 0)
		{
			int count2 = lstDatas.Count;
			for (int j = 0; j < count2; j++)
			{
				lstDatas[j].AttachLODNode(this);
			}
		}
		if (BuildStep != 3)
		{
			BuildStep = 0;
			EndUpdateNodeData();
		}
	}

	private void AddToBuildList()
	{
		if (_lstBlock45Datas != null && _lstBlock45Datas.Count > 0)
		{
			int count = _lstBlock45Datas.Count;
			for (int i = 0; i < count; i++)
			{
				if (_lstBlock45Datas[i].VecData != null)
				{
					BuildStep = 3;
					_lstBlock45DatasStatus[i] = 1;
					_helperProc.SurfExtractor.AddSurfExtractReq(SurfExtractReqB45.Get(_lstBlock45Datas[i].GetStamp(), _lstBlock45Datas[i], _helperProc.ChunkProcPostGenMesh));
				}
			}
		}
		if (BuildStep != 3)
		{
			BuildStep = 0;
			EndUpdateNodeData();
		}
	}

	public void AddToBuildList(Block45OctNode octNode)
	{
		if (_lstBlock45Datas != null)
		{
			int count = _lstBlock45Datas.Count;
			for (int i = 0; i < count; i++)
			{
				if (_lstBlock45Datas[i] == octNode)
				{
					_lstBlock45DatasStatus[i] = 1;
					BuildStep = 3;
					break;
				}
			}
		}
		_helperProc.SurfExtractor.AddSurfExtractReq(SurfExtractReqB45.Get(octNode.GetStamp(), octNode, _helperProc.ChunkProcPostGenMesh));
	}

	public void EndUpdateOctNode(Block45OctNode octNode)
	{
		if (_lstBlock45Datas != null)
		{
			int count = _lstBlock45Datas.Count;
			for (int i = 0; i < count; i++)
			{
				if (_lstBlock45Datas[i] == octNode)
				{
					_lstBlock45DatasStatus[i] = 0;
					break;
				}
			}
			for (int j = 0; j < count; j++)
			{
				if (_lstBlock45DatasStatus[j] != 0)
				{
					return;
				}
			}
		}
		BuildStep = 0;
		EndUpdateNodeData();
	}

	public void UpdateTimeStamp()
	{
		_stampOfUpdating += 16;
	}

	public void BegUpdateNodeData()
	{
		_helperProc.OnBegUpdateNodeData(this);
		int cX = _node.CX;
		int cY = _node.CY;
		int cZ = _node.CZ;
		int lod = _node.Lod;
		if (_chunkPosLod.x != cX || _chunkPosLod.y != cY || _chunkPosLod.z != cZ || _chunkPosLod.w != lod)
		{
			_chunkPosLod.x = cX;
			_chunkPosLod.y = cY;
			_chunkPosLod.z = cZ;
			_chunkPosLod.w = lod;
			BuildStep = 1;
		}
		if (BuildStep == 0)
		{
			if (_lstBlock45Datas == null || _lstBlock45Datas.Count == 0)
			{
				EndUpdateNodeData();
				return;
			}
			BuildStep = 2;
		}
		if (BuildStep == 1)
		{
			_helperProc.ChunkProcPreLoadData(this);
			BuildStep = 2;
		}
		if (BuildStep >= 2)
		{
			AddToBuildList();
		}
	}

	public void EndUpdateNodeData()
	{
		_helperProc.OnEndUpdateNodeData(this);
		_node.OnEndUpdateNodeData(this);
	}

	public void OnDestroyNodeData()
	{
		try
		{
			_helperProc.OnDestroyNodeData(this);
			if (_lstBlock45Datas != null)
			{
				int count = _lstBlock45Datas.Count;
				for (int i = 0; i < count; i++)
				{
					_lstBlock45Datas[i].DestroyChunkGo();
					_lstBlock45DatasStatus[i] = 0;
				}
			}
			BuildStep = 0;
		}
		catch (Exception ex)
		{
			Debug.Log("Block45LODNodeData.OnDestroyNodeData:" + ex.Message);
		}
	}
}

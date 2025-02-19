using System.Collections.Generic;
using UnityEngine;

public class SceneChunkDependence : ISceneObjActivationDependence
{
	private static SceneChunkDependence _inst;

	private IntVector4 _tmpPos = new IntVector4();

	private Dictionary<IntVector4, EDependChunkType> _lstValidChunks = new Dictionary<IntVector4, EDependChunkType>();

	public static SceneChunkDependence Instance
	{
		get
		{
			if (_inst == null)
			{
				_inst = new SceneChunkDependence();
			}
			return _inst;
		}
	}

	private void SetDependChunkType(IntVector4 cposlod, EDependChunkType type, bool bAdd)
	{
		lock (_lstValidChunks)
		{
			EDependChunkType value = EDependChunkType.ChunkNotAvailable;
			_lstValidChunks.TryGetValue(cposlod, out value);
			value = ((!bAdd) ? (value & ~type) : (value | type));
			if (value == EDependChunkType.ChunkNotAvailable)
			{
				_lstValidChunks.Remove(cposlod);
			}
			else
			{
				_lstValidChunks[new IntVector4(cposlod)] = value;
			}
		}
	}

	private void GetDependChunkType(IntVector4 cposlod, out EDependChunkType val)
	{
		lock (_lstValidChunks)
		{
			_lstValidChunks.TryGetValue(cposlod, out val);
		}
	}

	private void ClearAllDependChunks()
	{
		lock (_lstValidChunks)
		{
			_lstValidChunks.Clear();
		}
	}

	public void Reset()
	{
		ClearAllDependChunks();
	}

	public void ValidListAdd(IntVector4 cposlod, EDependChunkType type)
	{
		SetDependChunkType(cposlod, type, bAdd: true);
		SceneMan.OnActivationDependenceDirty(bAdd: true);
	}

	public void ValidListRemove(IntVector4 cposlod, EDependChunkType type)
	{
		SetDependChunkType(cposlod, type, bAdd: false);
		SceneMan.OnActivationDependenceDirty(bAdd: false);
	}

	public bool IsDependableForAgent(ISceneObjAgent agent, ref EDependChunkType type)
	{
		lock (_lstValidChunks)
		{
			Vector3 pos = agent.Pos;
			int x = (int)pos.x >> 5;
			int z = (int)pos.z >> 5;
			EDependChunkType val = EDependChunkType.ChunkNotAvailable;
			_tmpPos.x = x;
			_tmpPos.z = z;
			_tmpPos.w = 0;
			if (agent.TstYOnActivate)
			{
				int y = (int)pos.y >> 5;
				_tmpPos.y = y;
				GetDependChunkType(_tmpPos, out val);
				if ((val & EDependChunkType.ChunkBlkMask) != 0 && (val & EDependChunkType.ChunkTerMask) != 0)
				{
					type |= val;
					return true;
				}
				return false;
			}
			int num = (int)SceneMan.LastRefreshPos.y >> 5;
			_tmpPos.y = num;
			GetDependChunkType(_tmpPos, out val);
			if ((val & EDependChunkType.ChunkBlkMask) != 0 && (val & EDependChunkType.ChunkTerCol) != 0)
			{
				type |= val;
				return true;
			}
			for (int i = 1; i <= 8; i++)
			{
				_tmpPos.y = num - i;
				GetDependChunkType(_tmpPos, out val);
				if ((val & EDependChunkType.ChunkBlkMask) != 0 && (val & EDependChunkType.ChunkTerCol) != 0)
				{
					type |= val;
					return true;
				}
				_tmpPos.y = num + i;
				GetDependChunkType(_tmpPos, out val);
				if ((val & EDependChunkType.ChunkBlkMask) != 0 && (val & EDependChunkType.ChunkTerCol) != 0)
				{
					type |= val;
					return true;
				}
			}
			return false;
		}
	}
}

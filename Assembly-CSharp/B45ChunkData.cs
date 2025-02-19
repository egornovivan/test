using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class B45ChunkData
{
	public enum EBuildStep
	{
		EBuildStep_NotStart,
		EBuildStep_StartDataLoading,
		EBuildStep_FinDataLoading,
		EBuildStep_StartGoCreating,
		EBuildStep_FinGoCreating
	}

	public static uint DS_IN_QUEUE = 1u;

	public static uint DS_BEING_DESTROYED = 2u;

	private uint d_state;

	public static GenericArrayPool<byte> s_ChunkDataPool = new GenericArrayPool<byte>(2000);

	public static List<B45ChunkGo> s_ChunkGosToDestroy = new List<B45ChunkGo>();

	public static byte[] s_ChunkDataAir = new byte[2];

	public static byte[] S_ChunkDataNull = new byte[0];

	private BiLookup<int, B45ChunkData> _buildPosDatList;

	private LODOctreeNode _node;

	public BlockVectorNode _bvNode;

	private IntVector4 _chunkPosLod = new IntVector4(0, 0, 0, -1);

	private IntVector4 OldChunkPos;

	private B45ChunkGo _goChunk;

	public int _maskTrans;

	public int svn_key;

	public int svn_key_ba;

	public List<UpdateVector> updateVectors;

	public List<UVKeyCount> uvVersionKeys;

	private int _buildStep;

	private bool _bNoVerts;

	private bool _bFromPool;

	private byte[] _chunkData;

	private ChunkColliderMan colliderMan;

	public int[] UpdateDatas => updateVectors.Select((UpdateVector iter) => iter.Data).ToArray();

	private Vector3 Pos => new Vector3(_chunkPosLod.x << 3, _chunkPosLod.y << 3, _chunkPosLod.z << 3);

	public int LOD => _chunkPosLod.w;

	public IntVector4 ChunkPos => _chunkPosLod;

	public IntVector4 ChunkPosLod => _chunkPosLod;

	public int BuildStep => _buildStep;

	public byte[] DataVT => _chunkData;

	public bool IsHollow => _chunkData.Length == 2;

	public B45ChunkGo ChunkGo => _goChunk;

	public bool IsChunkInReq => _node == null || _node.IsInReq;

	public bool IsGoCreating => _buildStep >= 3;

	public bool IsDataLoading => _buildStep == 1;

	public int StampOfChnkUpdating => VFVoxelChunkData.GenStampOfUpdating(_chunkPosLod);

	public BiLookup<int, B45ChunkData> BuildList
	{
		set
		{
			_buildPosDatList = value;
		}
	}

	public IntVector4 ChunkPosLod_w
	{
		set
		{
			_chunkPosLod = value;
		}
	}

	public byte[] DataVT_w
	{
		set
		{
			if (_bFromPool)
			{
				s_ChunkDataPool.Free(_chunkData);
			}
			_chunkData = value;
		}
	}

	public bool isInQueue => (d_state & DS_IN_QUEUE) != 0;

	public bool isBeingDestroyed => (d_state & DS_BEING_DESTROYED) != 0;

	public B45Block this[IntVector3 idx] => ReadVoxelAtIdx(idx.x, idx.y, idx.z);

	public B45ChunkData(byte[] dataArray)
	{
		_chunkData = dataArray;
		_buildStep = 2;
	}

	public B45ChunkData(ChunkColliderMan _colliderMan)
	{
		_chunkData = s_ChunkDataPool.Get();
		_bFromPool = true;
		Array.Clear(_chunkData, 0, 2000);
		_buildStep = 2;
		d_state = 0u;
		colliderMan = _colliderMan;
	}

	public bool IsStampIdentical(int stamp)
	{
		return stamp == VFVoxelChunkData.GenStampOfUpdating(_chunkPosLod);
	}

	public void AddToBuildList()
	{
		if (IsHollow)
		{
			FinHollowUpdate();
			return;
		}
		_buildStep = 3;
		_buildPosDatList.Add(VFVoxelChunkData.GenStampOfUpdating(_chunkPosLod), this);
		if (colliderMan != null)
		{
			colliderMan.addRebuildChunk(ChunkPos.XYZ);
		}
		setInQueue();
	}

	public void safeToRemoveCollider()
	{
		colliderMan.colliderBuilt(ChunkPos.XYZ);
	}

	public void bp()
	{
	}

	public void setInQueue()
	{
		d_state |= DS_IN_QUEUE;
	}

	public void setNotInQueue()
	{
		d_state &= ~DS_IN_QUEUE;
	}

	public void setBeingDestroyed()
	{
		d_state |= DS_BEING_DESTROYED;
	}

	public void InitUpdateVectors()
	{
		updateVectors = new List<UpdateVector>();
		uvVersionKeys = new List<UVKeyCount>();
		uvVersionKeys.Add(new UVKeyCount());
		svn_key = 1;
		svn_key_ba = 1;
	}

	public void ClearMem()
	{
		if (isInQueue)
		{
		}
		d_state = 0u;
		DataVT_w = S_ChunkDataNull;
		_bFromPool = false;
		_buildStep = 0;
	}

	public void ApplyUpdateVectors()
	{
		if (updateVectors != null)
		{
			B45Block b45Block = default(B45Block);
			for (int i = 0; i < updateVectors.Count; i++)
			{
				b45Block.blockType = updateVectors[i].voxelData0;
				b45Block.materialType = updateVectors[i].voxelData1;
			}
		}
	}

	public string GenChunkIdentifier()
	{
		return string.Empty + ChunkPos.x + "_" + ChunkPos.y + "_" + ChunkPos.z;
	}

	public void CreateGameObject()
	{
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
			_buildStep = 1;
			DataVT_w = S_ChunkDataNull;
			_bFromPool = false;
			_bNoVerts = false;
		}
		if (_buildStep >= 4)
		{
			if ((object)_goChunk != null || _bNoVerts)
			{
				_node.OnEndUpdateNodeData(null);
				return;
			}
			_buildStep = ((_chunkData.Length <= 0) ? 1 : 2);
		}
		if (_buildStep >= 2 && _buildStep < 4 && !_buildPosDatList.Contains(VFVoxelChunkData.GenStampOfUpdating(_chunkPosLod), this))
		{
			AddToBuildList();
		}
	}

	public void OnDataLoaded(byte[] data, bool bFromPool = false)
	{
		DataVT_w = data;
		_bFromPool = bFromPool;
		_buildStep = 2;
		AddToBuildList();
	}

	public void DestroyChunkGO()
	{
		if ((object)_goChunk != null)
		{
			if (_node == null)
			{
				_goChunk.Destroy();
			}
			else
			{
				s_ChunkGosToDestroy.Add(_goChunk);
			}
			_goChunk = null;
		}
	}

	public void DestroyGameObject()
	{
		DestroyChunkGO();
		ClearMem();
	}

	private bool FinHollowUpdate()
	{
		DestroyChunkGO();
		if (_node != null)
		{
			_node.OnEndUpdateNodeData(null);
		}
		_bNoVerts = true;
		_buildStep = 4;
		return true;
	}

	public static string OccupiedVecsStr(byte[] byteData)
	{
		string text = string.Empty;
		B45Block b45Block = default(B45Block);
		for (int i = 0; i < 10; i++)
		{
			for (int j = 0; j < 10; j++)
			{
				for (int k = 0; k < 10; k++)
				{
					b45Block.blockType = byteData[OneIndexNoPrefix(k, j, i) * 2];
					if (b45Block.blockType != 0)
					{
						string text2 = text;
						text = text2 + "(" + (k - 1) + "," + (j - 1) + "," + (i - 1) + "); ";
					}
				}
			}
		}
		return text;
	}

	public string OccupiedVecsStr()
	{
		string text = string.Empty;
		B45Block b45Block = default(B45Block);
		for (int i = 0; i < 10; i++)
		{
			for (int j = 0; j < 10; j++)
			{
				for (int k = 0; k < 10; k++)
				{
					b45Block.blockType = _chunkData[OneIndexNoPrefix(k, j, i) * 2];
					if (b45Block.blockType != 0)
					{
						string text2 = text;
						text = text2 + "(" + (k - 1) + "," + (j - 1) + "," + (i - 1) + "); ";
					}
				}
			}
		}
		return text;
	}

	public List<BuildingEditOps.OpInfo> OccupiedVecs()
	{
		return new List<BuildingEditOps.OpInfo>();
	}

	public void getCheckSum(out long volSum, out long typeSum)
	{
		long num = 0L;
		long num2 = 0L;
		for (int i = 0; i < _chunkData.Length; i += 2)
		{
			num += (int)_chunkData[i];
			num2 += (int)_chunkData[i + 1];
		}
		volSum = num;
		typeSum = num2;
	}

	public int getFillRate()
	{
		int num = 0;
		for (int i = 0; i < _chunkData.Length; i += 2)
		{
			if (_chunkData[i] != 0)
			{
				num++;
			}
		}
		return num;
	}

	public static int numDots_d(byte[] d_array)
	{
		int num = 0;
		for (int i = 0; i < d_array.Length; i += 2)
		{
			if (d_array[i] != 0)
			{
				num++;
			}
		}
		return num;
	}

	public void FillHollowPblc()
	{
		FillHollow();
	}

	private void FillHollow()
	{
		byte b = _chunkData[0];
		byte b2 = _chunkData[1];
		_chunkData = s_ChunkDataPool.Get();
		_bFromPool = true;
		if (b != 0)
		{
			int num = 0;
			while (num < 2000)
			{
				_chunkData[num++] = b;
				_chunkData[num++] = b2;
			}
		}
		else
		{
			Array.Clear(_chunkData, 0, 2000);
		}
	}

	public void AttachChunkGo(B45ChunkGo vfGo, int mat_idx = 0)
	{
		if (null != _goChunk)
		{
			UnityEngine.Object.Destroy(_goChunk.gameObject);
			_goChunk = null;
		}
		if (vfGo != null)
		{
			float num = (float)(1 << _chunkPosLod.w) * 0.5f;
			_goChunk = vfGo;
			_goChunk.name = "B45Chnk_m" + mat_idx + "_" + _chunkPosLod.x + "_" + _chunkPosLod.y + "_" + _chunkPosLod.z + "_" + _chunkPosLod.w;
			_goChunk.transform.localScale = new Vector3(num, num, num);
			_goChunk.transform.localPosition = Pos * num;
			_goChunk._data = this;
		}
		_bNoVerts = vfGo == null;
		_buildStep = 4;
		if (_node != null)
		{
			if (_bNoVerts && _chunkPosLod.w > 0)
			{
				DataVT_w = S_ChunkDataNull;
				_bFromPool = false;
			}
			_node.OnEndUpdateNodeData(null);
		}
	}

	public B45Block ReadVoxelAtPos(int x, int y, int z)
	{
		return ReadVoxelAtIdx((x >> LOD) & 0x1F, (y >> LOD) & 0x1F, (z >> LOD) & 0x1F);
	}

	public B45Block ReadVoxelAtIdx(int x, int y, int z)
	{
		if (_chunkData == S_ChunkDataNull)
		{
			return new B45Block(0);
		}
		try
		{
			int num = ((!IsHollow) ? OneIndex(x, y, z) : 0);
			int num2 = num * 2;
			return new B45Block(_chunkData[num2], _chunkData[num2 + 1]);
		}
		catch
		{
			Debug.LogWarning("[VFTERRAIN]:Failed to read voxel[" + x + "," + y + "," + z + "] of ChunkData" + ChunkPosLod);
			return new B45Block(0);
		}
	}

	public bool WriteVoxelAtIdx(int x, int y, int z, B45Block voxel, bool bLodUpdate = false)
	{
		if (_chunkData == S_ChunkDataNull)
		{
			return false;
		}
		try
		{
			if (IsHollow)
			{
				FillHollow();
			}
			int num = OneIndex(x, y, z);
			int num2 = num * 2;
			_chunkData[num2] = voxel.blockType;
			_chunkData[num2 + 1] = voxel.materialType;
			if (_node == null)
			{
				AddToBuildList();
			}
			else if (_node.IsInReq)
			{
				AddToBuildList();
			}
		}
		catch (Exception ex)
		{
			Debug.LogWarning(string.Concat("[VFTERRAIN]:Failed to write voxel[", x, ",", y, ",", z, "] of ChunkData", ChunkPosLod, " : ", ex.ToString()));
		}
		if (bLodUpdate && VFVoxelTerrain.self.LodDataUpdate != null)
		{
			VFVoxelTerrain.self.LodDataUpdate.InsertUpdateCoord(x, y, z, _chunkPosLod);
		}
		return true;
	}

	public bool WriteVoxelAtIdx4LodUpdate(int x, int y, int z, B45Block voxel)
	{
		if (_chunkData == null)
		{
			return false;
		}
		try
		{
			if (IsHollow)
			{
				FillHollow();
			}
			int num = OneIndexNoPrefix(x, y, z);
			int num2 = num * 2;
			_chunkData[num2] = voxel.blockType;
			_chunkData[num2 + 1] = voxel.materialType;
			if (_node == null)
			{
				AddToBuildList();
			}
			else if (_node.IsInReq)
			{
				AddToBuildList();
			}
		}
		catch
		{
			Debug.LogWarning("[VFTERRAIN]:Failed to write lod voxel[" + x + "," + y + "," + z + "] of ChunkData" + ChunkPosLod);
		}
		return true;
	}

	public static int OneIndexNoPrefix(int x, int y, int z)
	{
		return z * 100 + y * 10 + x;
	}

	public static int OneIndex(int x, int y, int z)
	{
		return (z + 1) * 100 + (y + 1) * 10 + (x + 1);
	}
}

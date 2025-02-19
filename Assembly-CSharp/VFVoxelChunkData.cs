using System;
using System.Collections.Generic;
using UnityEngine;

public class VFVoxelChunkData : ILODNodeData
{
	public struct EndUpdateReq
	{
		public VFVoxelChunkData _chunk;

		public int _sig;

		public int _stamp;

		public EndUpdateReq(SurfExtractReqMC req)
		{
			_chunk = req._chunk;
			_sig = req._chunkSig;
			_stamp = req._chunkStamp;
		}

		public bool IsValid()
		{
			return _chunk != null && _chunk.IsMatch(_sig, _stamp);
		}
	}

	public delegate void ProcOnWriteVoxel(LODOctreeNode node, byte oldVol, byte newVol, int idxVol);

	public const int BuildStep_NotInBuild = 0;

	public const int BuildStep_StartDataLoading = 1;

	public const int BuildStep_FinDataLoading = 2;

	public const int BuildStep_StartGoCreating = 3;

	public const int BuildStepMask = 15;

	public const int StampMask = -16;

	private IVxChunkHelperProc _helperProc;

	private LODOctreeNode _node;

	private IntVector4 _chunkPosLod = new IntVector4(0, 0, 0, -1);

	private VFVoxelChunkGo _goChunk;

	private int _stampOfUpdating;

	private bool _bNoVerts;

	private bool _bFromPool;

	private byte[] _chunkData;

	private static List<IntVector3> multiDirtyChunkPosList;

	public static GenericArrayPool<byte> s_ChunkDataPool;

	private static List<EndUpdateReq> s_lstReqsToEndUpdateNodeData;

	public static readonly byte[][] S_ChunkDataSolid;

	public static readonly byte[] S_ChunkDataNull;

	public static readonly byte[] S_ChunkDataAir;

	public static readonly byte[] S_ChunkDataWaterSolid;

	public static readonly byte[] S_ChunkDataWaterPlane;

	public static int[][] s_DirtyChunkIndexes;

	public static int[] s_OfsVTInNeibourChunks;

	public static readonly int[,] S_NearChunkOfs;

	public static readonly int S_MinNoDirtyIdx;

	public static readonly int S_MaxNoDirtyIdx;

	private Vector3 Pos => new Vector3(_chunkPosLod.x << 5, _chunkPosLod.y << 5, _chunkPosLod.z << 5);

	public int LOD => _chunkPosLod.w;

	public IntVector4 ChunkPosLod => _chunkPosLod;

	public byte[] DataVT => _chunkData;

	public bool IsHollow => _chunkData.Length == 2;

	public bool IsEmpty => (object)_goChunk == null;

	public VFVoxelChunkGo ChunkGo => _goChunk;

	public bool IsNoVerts => _bNoVerts;

	public bool IsIdle => BuildStep == 0;

	public int StampOfUpdating => _stampOfUpdating;

	public int SigOfChnk => _chunkPosLod.GetHashCode() + (5 * _helperProc.ChunkSig << 28);

	public int SigOfType => _helperProc.ChunkSig;

	public int VertsThreshold => (_node == null) ? 64998 : 3999;

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
		get
		{
			return _helperProc;
		}
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

	public VFVoxel this[IntVector3 idx] => ReadVoxelAtIdx(idx.x, idx.y, idx.z);

	public VFVoxelChunkData(LODOctreeNode node)
	{
		_node = node;
		_chunkData = S_ChunkDataNull;
	}

	public VFVoxelChunkData(LODOctreeNode node, byte[] dataArray, bool bFromPool = false)
	{
		_node = node;
		_chunkData = dataArray;
		_bFromPool = bFromPool;
		BuildStep = 2;
	}

	static VFVoxelChunkData()
	{
		multiDirtyChunkPosList = new List<IntVector3>();
		s_ChunkDataPool = new GenericArrayPool<byte>(85750, 768);
		s_lstReqsToEndUpdateNodeData = new List<EndUpdateReq>();
		S_ChunkDataSolid = new byte[256][];
		S_ChunkDataNull = new byte[0];
		S_ChunkDataAir = new byte[2];
		S_ChunkDataWaterSolid = new byte[2] { 255, 128 };
		S_ChunkDataWaterPlane = new byte[2] { 128, 128 };
		s_DirtyChunkIndexes = new int[42875][];
		s_OfsVTInNeibourChunks = new int[27];
		S_NearChunkOfs = new int[8, 3]
		{
			{ 0, 0, 0 },
			{ 1, 0, 0 },
			{ 0, 1, 0 },
			{ 1, 1, 0 },
			{ 0, 0, 1 },
			{ 1, 0, 1 },
			{ 0, 1, 1 },
			{ 1, 1, 1 }
		};
		S_MinNoDirtyIdx = 2;
		S_MaxNoDirtyIdx = 31;
		for (int i = 0; i < 256; i++)
		{
			S_ChunkDataSolid[i] = new byte[2]
			{
				255,
				(byte)i
			};
		}
		int num = 0;
		for (int j = -1; j <= 1; j++)
		{
			for (int k = -1; k <= 1; k++)
			{
				for (int l = -1; l <= 1; l++)
				{
					s_OfsVTInNeibourChunks[num++] = OneIndexNoPrefix(-l << 5, -k << 5, -j << 5) * 2;
				}
			}
		}
		int num2 = -1;
		int num3 = 35 + num2;
		int num4 = 0;
		int num5 = 0;
		int num6 = 0;
		int num7 = 0;
		int num8 = 0;
		int num9 = 0;
		int num10 = 0;
		List<int> list = new List<int>(8);
		for (int m = num2; m < num3; m++)
		{
			if (m < S_MinNoDirtyIdx)
			{
				num6 = -1;
				num9 = 68;
			}
			else if (m >= S_MaxNoDirtyIdx)
			{
				num6 = 1;
				num9 = 4;
			}
			else
			{
				num6 = 0;
				num9 = 0;
			}
			for (int n = num2; n < num3; n++)
			{
				if (n < S_MinNoDirtyIdx)
				{
					num5 = -1;
					num8 = 34;
				}
				else if (n >= S_MaxNoDirtyIdx)
				{
					num5 = 1;
					num8 = 2;
				}
				else
				{
					num5 = 0;
					num8 = 0;
				}
				for (int num11 = num2; num11 < num3; num11++)
				{
					if (num11 < S_MinNoDirtyIdx)
					{
						num4 = -1;
						num7 = 17;
					}
					else if (num11 >= S_MaxNoDirtyIdx)
					{
						num4 = 1;
						num7 = 1;
					}
					else
					{
						num4 = 0;
						num7 = 0;
					}
					int num12 = num9 | num8 | num7;
					if (num12 != 0)
					{
						list.Clear();
						for (int num13 = 1; num13 < 8; num13++)
						{
							if ((num12 & num13) == num13)
							{
								int num14 = num4 * S_NearChunkOfs[num13, 0];
								int num15 = num5 * S_NearChunkOfs[num13, 1];
								int num16 = num6 * S_NearChunkOfs[num13, 2];
								list.Add((num16 + 1) * 9 + (num15 + 1) * 3 + (num14 + 1));
							}
						}
						s_DirtyChunkIndexes[num10] = list.ToArray();
					}
					num10++;
				}
			}
		}
	}

	public void ClearMem()
	{
		SetDataVT(S_ChunkDataNull);
		BuildStep = 0;
	}

	public void SetDataVT(byte[] data, bool bFromPool = false)
	{
		if (_helperProc != null)
		{
			_helperProc.ChunkProcPreSetDataVT(this, data, bFromPool);
		}
		if (_bFromPool)
		{
			s_ChunkDataPool.Free(_chunkData);
		}
		_chunkData = data;
		_bFromPool = bFromPool;
	}

	public VFVoxelChunkData GetNodeData(int idx)
	{
		return (_node != null) ? ((VFVoxelChunkData)_node._data[idx]) : null;
	}

	public bool IsStampIdentical(int stamp)
	{
		return stamp == _stampOfUpdating;
	}

	public bool IsMatch(int sig, int stamp)
	{
		return stamp == _stampOfUpdating && sig == SigOfChnk;
	}

	public bool IsNodePosChange()
	{
		return _node.CX != _chunkPosLod.x || _node.CY != _chunkPosLod.y || _node.CZ != _chunkPosLod.z || _node.Lod != _chunkPosLod.w;
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
			SetDataVT(S_ChunkDataNull);
			_chunkPosLod.x = cX;
			_chunkPosLod.y = cY;
			_chunkPosLod.z = cZ;
			_chunkPosLod.w = lod;
			BuildStep = 1;
			_bNoVerts = false;
		}
		if (BuildStep == 0)
		{
			if (_chunkData.Length == 0 && _chunkPosLod.w == 0)
			{
				BuildStep = 1;
			}
			else
			{
				if ((object)_goChunk != null || _bNoVerts)
				{
					EndUpdateNodeData();
					return;
				}
				BuildStep = ((_chunkData.Length == 0) ? 1 : 2);
			}
		}
		if (BuildStep == 1)
		{
			_helperProc.ChunkProcPreLoadData(this);
		}
		else if (BuildStep >= 2)
		{
			AddToBuildList();
		}
	}

	public void EndUpdateNodeData()
	{
		BuildStep = 0;
		if (!IsHollow && _bNoVerts && _chunkPosLod.w > 0)
		{
			SetDataVT(S_ChunkDataNull);
		}
		_helperProc.OnEndUpdateNodeData(this);
		_node.OnEndUpdateNodeData(this);
	}

	public void OnDestroyNodeData()
	{
		_helperProc.OnDestroyNodeData(this);
		DestroyChunkGO();
		ClearMem();
	}

	public void DestroyChunkGO()
	{
		if ((object)_goChunk != null)
		{
			if (_node == null)
			{
				VFGoPool<VFVoxelChunkGo>.FreeGo(_goChunk);
			}
			else
			{
				VFGoPool<VFVoxelChunkGo>.ReqFreeGo(_goChunk);
			}
			_goChunk = null;
		}
	}

	public void OnDataLoaded(byte[] data, bool bFromPool = false)
	{
		SetDataVT(data, bFromPool);
		BuildStep = 2;
		AddToBuildList();
		if (GameConfig.IsMultiMode && SigOfType == VFVoxelTerrain.self.ChunkSig)
		{
			ChunkManager.Instance.AddCacheReq(_chunkPosLod.ToVector3());
		}
	}

	private bool FinHollowUpdate()
	{
		DestroyChunkGO();
		_bNoVerts = true;
		BuildStep = 0;
		if (_node != null)
		{
			EndUpdateNodeData();
		}
		return true;
	}

	public void AddToBuildList()
	{
		if (IsHollow)
		{
			FinHollowUpdate();
			return;
		}
		BuildStep = 3;
		_helperProc.SurfExtractor.AddSurfExtractReq(SurfExtractReqMC.Get(this, VertsThreshold));
	}

	public void AttachChunkGo(VFVoxelChunkGo vfGo, SurfExtractReqMC req = null)
	{
		if (vfGo != null)
		{
			float num = 1 << _chunkPosLod.w;
			vfGo.transform.localScale = new Vector3(num, num, num);
			vfGo.transform.position = Pos;
			vfGo.Data = this;
			vfGo.OriginalChunkGo = null;
			if (null != _goChunk && _goChunk.name.Equals(vfGo.name))
			{
				if (_goChunk.OriginalChunkGo != null)
				{
					vfGo.OriginalChunkGo = _goChunk.OriginalChunkGo;
					_goChunk.OriginalChunkGo = null;
				}
				else if (_goChunk.Mc.sharedMesh != null)
				{
					vfGo.OriginalChunkGo = _goChunk;
					_goChunk.Mr.enabled = false;
					_goChunk = null;
				}
			}
			else if (_node != null && SigOfType == VFVoxelTerrain.self.ChunkSig)
			{
				_node.OnMeshCreated();
			}
		}
		if (null != _goChunk)
		{
			VFGoPool<VFVoxelChunkGo>.FreeGo(_goChunk);
		}
		_goChunk = vfGo;
		_bNoVerts = vfGo == null;
		if (_node != null && req != null)
		{
			if (!req.IsInvalid)
			{
				lock (s_lstReqsToEndUpdateNodeData)
				{
					s_lstReqsToEndUpdateNodeData.Add(new EndUpdateReq(req));
				}
			}
		}
		else
		{
			BuildStep = 0;
		}
	}

	public void OnGoColliderReady()
	{
		if (_node != null && SigOfType == VFVoxelTerrain.self.ChunkSig)
		{
			_node.OnPhyxReady();
		}
	}

	public VFVoxel ReadVoxelAtPos(int x, int y, int z)
	{
		return ReadVoxelAtIdx((x >> LOD) & 0x1F, (y >> LOD) & 0x1F, (z >> LOD) & 0x1F);
	}

	public VFVoxel ReadVoxelAtIdx(int x, int y, int z)
	{
		if (_chunkData == S_ChunkDataNull)
		{
			return new VFVoxel(0);
		}
		try
		{
			if (IsHollow)
			{
				return _helperProc.ChunkProcExtractData(this, x, y, z);
			}
			int num = OneIndex(x, y, z) * 2;
			return new VFVoxel(_chunkData[num], _chunkData[num + 1]);
		}
		catch
		{
			Debug.LogWarning("[VFTERRAIN]:Failed to read voxel[" + x + "," + y + "," + z + "] of ChunkData" + ChunkPosLod);
			return new VFVoxel(0);
		}
	}

	public bool WriteVoxelAtIdx(int x, int y, int z, VFVoxel voxel, bool bLodUpdate = false, ProcOnWriteVoxel OnWrite = null)
	{
		if (_chunkData == S_ChunkDataNull)
		{
			return false;
		}
		try
		{
			if (IsHollow && !_helperProc.ChunkProcExtractData(this))
			{
				return false;
			}
			int num = OneIndex(x, y, z);
			int num2 = num * 2;
			OnWrite?.Invoke(_node, _chunkData[num2], voxel.Volume, num2);
			_chunkData[num2] = voxel.Volume;
			_chunkData[num2 + 1] = voxel.Type;
			UpdateTimeStamp();
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
			Debug.LogWarning("[VFTERRAIN]:Failed to write voxel[" + x + "," + y + "," + z + "] of ChunkData" + ChunkPosLod);
		}
		if (bLodUpdate && VFVoxelTerrain.self.LodDataUpdate != null)
		{
			VFVoxelTerrain.self.LodDataUpdate.InsertUpdateCoord(x, y, z, _chunkPosLod);
		}
		return true;
	}

	public bool WriteVoxelAtIdxNoReq(int x, int y, int z, VFVoxel voxel, bool bLodUpdate = false)
	{
		if (_chunkData == S_ChunkDataNull)
		{
			return false;
		}
		try
		{
			if (IsHollow && !_helperProc.ChunkProcExtractData(this))
			{
				return false;
			}
			int num = OneIndex(x, y, z);
			int num2 = num * 2;
			_chunkData[num2] = voxel.Volume;
			_chunkData[num2 + 1] = voxel.Type;
			UpdateTimeStamp();
		}
		catch
		{
			Debug.LogWarning("[VFTERRAIN]:Failed to write voxel[" + x + "," + y + "," + z + "] of ChunkData" + ChunkPosLod);
		}
		if (bLodUpdate && VFVoxelTerrain.self.LodDataUpdate != null)
		{
			VFVoxelTerrain.self.LodDataUpdate.InsertUpdateCoord(x, y, z, _chunkPosLod);
		}
		return true;
	}

	public bool WriteVoxelAtIdx4LodUpdate(int x, int y, int z, VFVoxel voxel)
	{
		if (_chunkData == null || _chunkData == S_ChunkDataNull)
		{
			return false;
		}
		try
		{
			if (IsHollow && !_helperProc.ChunkProcExtractData(this))
			{
				return false;
			}
			int num = OneIndexNoPrefix(x, y, z);
			int num2 = num * 2;
			_chunkData[num2] = voxel.Volume;
			_chunkData[num2 + 1] = voxel.Type;
			UpdateTimeStamp();
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

	public bool BeginBatchWriteVoxels()
	{
		if (_chunkData == S_ChunkDataNull)
		{
			return false;
		}
		if (IsHollow && !_helperProc.ChunkProcExtractData(this))
		{
			return false;
		}
		return true;
	}

	public void EndBatchWriteVoxels(bool bLodUpdate = false)
	{
		UpdateTimeStamp();
		if (_node == null)
		{
			AddToBuildList();
		}
		else if (_node.IsInReq)
		{
			AddToBuildList();
		}
		if (bLodUpdate && VFVoxelTerrain.self.LodDataUpdate != null)
		{
			VFVoxelTerrain.self.LodDataUpdate.InsertUpdateCoord(0, 0, 0, _chunkPosLod);
		}
	}

	public static void EndAllReqs()
	{
		lock (s_lstReqsToEndUpdateNodeData)
		{
			int count = s_lstReqsToEndUpdateNodeData.Count;
			for (int i = 0; i < count; i++)
			{
				if (s_lstReqsToEndUpdateNodeData[i].IsValid())
				{
					s_lstReqsToEndUpdateNodeData[i]._chunk.EndUpdateNodeData();
				}
			}
			s_lstReqsToEndUpdateNodeData.Clear();
		}
	}

	public static int OneIndexNoPrefix(int x, int y, int z)
	{
		return z * 1225 + y * 35 + x;
	}

	public static int OneIndex(int x, int y, int z)
	{
		return (z + 1) * 1225 + (y + 1) * 35 + (x + 1);
	}

	public static List<IntVector3> GetDirtyChunkPosListMulti(int x, int y, int z)
	{
		int num = x >> 5;
		int num2 = y >> 5;
		int num3 = z >> 5;
		int num4 = x & 0x1F;
		int num5 = y & 0x1F;
		int num6 = z & 0x1F;
		multiDirtyChunkPosList.Clear();
		multiDirtyChunkPosList.Add(new IntVector3(num, num2, num3));
		int num7 = 0;
		int num8 = 0;
		int num9 = 0;
		int num10 = 128;
		if (num4 < S_MinNoDirtyIdx)
		{
			num7 = -1;
			num10 |= 0x11;
		}
		else if (num4 >= S_MaxNoDirtyIdx)
		{
			num7 = 1;
			num10 |= 1;
		}
		if (num5 < S_MinNoDirtyIdx)
		{
			num8 = -1;
			num10 |= 0x22;
		}
		else if (num5 >= S_MaxNoDirtyIdx)
		{
			num8 = 1;
			num10 |= 2;
		}
		if (num6 < S_MinNoDirtyIdx)
		{
			num9 = -1;
			num10 |= 0x44;
		}
		else if (num6 >= S_MaxNoDirtyIdx)
		{
			num9 = 1;
			num10 |= 4;
		}
		if (num10 != 128)
		{
			for (int i = 1; i < 8; i++)
			{
				if ((num10 & i) == i)
				{
					int num11 = num7 * S_NearChunkOfs[i, 0];
					int num12 = num8 * S_NearChunkOfs[i, 1];
					int num13 = num9 * S_NearChunkOfs[i, 2];
					multiDirtyChunkPosList.Add(new IntVector3(num + num11, num2 + num12, num3 + num13));
				}
			}
		}
		return multiDirtyChunkPosList;
	}

	public static List<IntVector3> GetDirtyChunkPosList(int x, int y, int z)
	{
		int num = x >> 5;
		int num2 = y >> 5;
		int num3 = z >> 5;
		int num4 = x & 0x1F;
		int num5 = y & 0x1F;
		int num6 = z & 0x1F;
		List<IntVector3> list = new List<IntVector3>();
		list.Add(new IntVector3(num, num2, num3));
		int num7 = 0;
		int num8 = 0;
		int num9 = 0;
		int num10 = 128;
		if (num4 < S_MinNoDirtyIdx)
		{
			num7 = -1;
			num10 |= 0x11;
		}
		else if (num4 >= S_MaxNoDirtyIdx)
		{
			num7 = 1;
			num10 |= 1;
		}
		if (num5 < S_MinNoDirtyIdx)
		{
			num8 = -1;
			num10 |= 0x22;
		}
		else if (num5 >= S_MaxNoDirtyIdx)
		{
			num8 = 1;
			num10 |= 2;
		}
		if (num6 < S_MinNoDirtyIdx)
		{
			num9 = -1;
			num10 |= 0x44;
		}
		else if (num6 >= S_MaxNoDirtyIdx)
		{
			num9 = 1;
			num10 |= 4;
		}
		if (num10 != 128)
		{
			for (int i = 1; i < 8; i++)
			{
				if ((num10 & i) == i)
				{
					int num11 = num7 * S_NearChunkOfs[i, 0];
					int num12 = num8 * S_NearChunkOfs[i, 1];
					int num13 = num9 * S_NearChunkOfs[i, 2];
					list.Add(new IntVector3(num + num11, num2 + num12, num3 + num13));
				}
			}
		}
		return list;
	}

	public static void ExpandHollowChunkData(VFVoxelChunkData chunk)
	{
		byte b = chunk._chunkData[0];
		byte b2 = chunk._chunkData[1];
		byte[] array = s_ChunkDataPool.Get();
		if (b != 0)
		{
			int num = 0;
			while (num < 85750)
			{
				array[num++] = b;
				array[num++] = b2;
			}
		}
		else
		{
			Array.Clear(array, 0, 85750);
		}
		chunk._chunkData = array;
		chunk._bFromPool = true;
	}

	public static void ModVolume(VFVoxelChunkData curChunk, int idxVT, byte volume, VFVoxelChunkData[] neibourChunks, VFVoxelChunkData[] dirtyNeibourChunks)
	{
		int[] array = s_DirtyChunkIndexes[idxVT >> 1];
		if (array != null)
		{
			int num = array.Length;
			for (int i = 0; i < num; i++)
			{
				int num2 = array[i];
				VFVoxelChunkData vFVoxelChunkData = dirtyNeibourChunks[num2];
				if (vFVoxelChunkData == null)
				{
					vFVoxelChunkData = neibourChunks[num2];
					if (vFVoxelChunkData == null || !vFVoxelChunkData.BeginBatchWriteVoxels())
					{
						return;
					}
					dirtyNeibourChunks[num2] = vFVoxelChunkData;
				}
			}
			for (int j = 0; j < num; j++)
			{
				int num3 = array[j];
				int num4 = idxVT + s_OfsVTInNeibourChunks[num3];
				VFVoxelChunkData vFVoxelChunkData2 = dirtyNeibourChunks[num3];
				if (num4 < vFVoxelChunkData2.DataVT.Length)
				{
					vFVoxelChunkData2.DataVT[num4] = volume;
				}
			}
		}
		if (idxVT < curChunk.DataVT.Length)
		{
			curChunk.DataVT[idxVT] = volume;
		}
		dirtyNeibourChunks[13] = curChunk;
	}

	public static void ModVolumeType(VFVoxelChunkData curChunk, int idxVT, byte volume, byte type, VFVoxelChunkData[] neibourChunks, VFVoxelChunkData[] dirtyNeibourChunks)
	{
		int[] array = s_DirtyChunkIndexes[idxVT >> 1];
		if (array != null)
		{
			int num = array.Length;
			for (int i = 0; i < num; i++)
			{
				int num2 = array[i];
				VFVoxelChunkData vFVoxelChunkData = dirtyNeibourChunks[num2];
				if (vFVoxelChunkData == null)
				{
					vFVoxelChunkData = neibourChunks[num2];
					if (vFVoxelChunkData == null || !vFVoxelChunkData.BeginBatchWriteVoxels())
					{
						return;
					}
					dirtyNeibourChunks[num2] = vFVoxelChunkData;
				}
			}
			for (int j = 0; j < num; j++)
			{
				int num3 = array[j];
				int num4 = idxVT + s_OfsVTInNeibourChunks[num3];
				VFVoxelChunkData vFVoxelChunkData2 = dirtyNeibourChunks[num3];
				if (num4 < vFVoxelChunkData2.DataVT.Length)
				{
					vFVoxelChunkData2.DataVT[num4] = volume;
					vFVoxelChunkData2.DataVT[num4 + 1] = type;
				}
			}
		}
		if (idxVT < curChunk.DataVT.Length)
		{
			curChunk.DataVT[idxVT] = volume;
			curChunk.DataVT[idxVT + 1] = type;
		}
		dirtyNeibourChunks[13] = curChunk;
	}

	public static int GenStampOfUpdating(IntVector4 cposlod)
	{
		return cposlod.GetHashCode();
	}
}

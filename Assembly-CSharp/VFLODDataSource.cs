using System.Collections.Generic;
using UnityEngine;

public class VFLODDataSource : IVxDataSource
{
	private int _idxData;

	private LODOctreeNode[][,,] _lodNodes;

	private List<VFVoxelChunkData> _dirtyChunkList = new List<VFVoxelChunkData>();

	public List<VFVoxelChunkData> DirtyChunkList => _dirtyChunkList;

	public byte this[IntVector3 idx, IntVector4 cposlod]
	{
		get
		{
			int w = cposlod.w;
			int num = LODOctreeMan._xChunkCount >> w;
			int num2 = LODOctreeMan._yChunkCount >> w;
			int num3 = LODOctreeMan._zChunkCount >> w;
			int num4 = (cposlod.x >> w) % num;
			int num5 = (cposlod.y >> w) % num2;
			int num6 = (cposlod.z >> w) % num3;
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
			VFVoxelChunkData vFVoxelChunkData = (VFVoxelChunkData)_lodNodes[w][num4, num5, num6]._data[_idxData];
			if (!cposlod.Equals(vFVoxelChunkData.ChunkPosLod))
			{
				return 0;
			}
			if (idx.x < -1 || idx.y < -1 || idx.z < -1)
			{
				int x = ((idx.x >= -1) ? idx.x : (-1));
				int y = ((idx.y >= -1) ? idx.y : (-1));
				int z = ((idx.z >= -1) ? idx.z : (-1));
				return vFVoxelChunkData.ReadVoxelAtIdx(x, y, z).Volume;
			}
			return vFVoxelChunkData.ReadVoxelAtIdx(idx.x, idx.y, idx.z).Volume;
		}
	}

	public VFLODDataSource(LODOctreeNode[][,,] lodNodes, int idxData = 0)
	{
		_lodNodes = lodNodes;
		_idxData = idxData;
	}

	public VFVoxelChunkData readChunk(int cx, int cy, int cz, int lod = 0)
	{
		int num = LODOctreeMan._xChunkCount >> lod;
		int num2 = LODOctreeMan._yChunkCount >> lod;
		int num3 = LODOctreeMan._zChunkCount >> lod;
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
		VFVoxelChunkData vFVoxelChunkData = (VFVoxelChunkData)_lodNodes[lod][num4, num5, num6]._data[_idxData];
		IntVector4 chunkPosLod = vFVoxelChunkData.ChunkPosLod;
		if (chunkPosLod.x != cx || chunkPosLod.y != cy || chunkPosLod.z != cz || chunkPosLod.w != lod)
		{
			return null;
		}
		return vFVoxelChunkData;
	}

	public void writeChunk(int cposx, int cposy, int cposz, VFVoxelChunkData data, int lod = 0)
	{
		Debug.LogError("Unavailable interface: writeChunk");
	}

	public VFVoxel Read(int x, int y, int z, int lod = 0)
	{
		VFVoxelChunkData vFVoxelChunkData = readChunk(x >> 5, y >> 5, z >> 5, lod);
		if (vFVoxelChunkData == null)
		{
			return new VFVoxel(0);
		}
		int x2 = (x >> lod) & 0x1F;
		int y2 = (y >> lod) & 0x1F;
		int z2 = (z >> lod) & 0x1F;
		return vFVoxelChunkData.ReadVoxelAtIdx(x2, y2, z2);
	}

	public int Write(int x, int y, int z, VFVoxel voxel, int lod = 0, bool bUpdateLod = true)
	{
		_dirtyChunkList.Clear();
		int num = LODOctreeMan._xChunkCount >> lod;
		int num2 = LODOctreeMan._yChunkCount >> lod;
		int num3 = LODOctreeMan._zChunkCount >> lod;
		int num4 = x >> 5;
		int num5 = y >> 5;
		int num6 = z >> 5;
		int num7 = (num4 >> lod) % num;
		int num8 = (num5 >> lod) % num2;
		int num9 = (num6 >> lod) % num3;
		if (num7 < 0)
		{
			num7 += num;
		}
		if (num8 < 0)
		{
			num8 += num2;
		}
		if (num9 < 0)
		{
			num9 += num3;
		}
		int num10 = (x >> lod) & 0x1F;
		int num11 = (y >> lod) & 0x1F;
		int num12 = (z >> lod) & 0x1F;
		LODOctreeNode lODOctreeNode = _lodNodes[lod][num7, num8, num9];
		VFVoxelChunkData vFVoxelChunkData = (VFVoxelChunkData)lODOctreeNode._data[_idxData];
		IntVector4 chunkPosLod = vFVoxelChunkData.ChunkPosLod;
		if (chunkPosLod == null || chunkPosLod.x != num4 || chunkPosLod.y != num5 || chunkPosLod.z != num6)
		{
			return 0;
		}
		VFVoxelChunkData.ProcOnWriteVoxel onWrite = null;
		if (_idxData == 0 && VFVoxelWater.self != null)
		{
			onWrite = VFVoxelWater.self.OnWriteVoxelOfTerrain;
		}
		if (!vFVoxelChunkData.WriteVoxelAtIdx(num10, num11, num12, voxel, bUpdateLod, onWrite))
		{
			return 256;
		}
		_dirtyChunkList.Add(vFVoxelChunkData);
		int num13 = 0;
		int num14 = 0;
		int num15 = 0;
		int num16 = 128;
		if (num10 < VFVoxelChunkData.S_MinNoDirtyIdx)
		{
			num13 = -1;
			num16 |= 0x11;
		}
		else if (num10 >= VFVoxelChunkData.S_MaxNoDirtyIdx)
		{
			num13 = 1;
			num16 |= 1;
		}
		if (num11 < VFVoxelChunkData.S_MinNoDirtyIdx)
		{
			num14 = -1;
			num16 |= 0x22;
		}
		else if (num11 >= VFVoxelChunkData.S_MaxNoDirtyIdx)
		{
			num14 = 1;
			num16 |= 2;
		}
		if (num12 < VFVoxelChunkData.S_MinNoDirtyIdx)
		{
			num15 = -1;
			num16 |= 0x44;
		}
		else if (num12 >= VFVoxelChunkData.S_MaxNoDirtyIdx)
		{
			num15 = 1;
			num16 |= 4;
		}
		if (num16 != 128)
		{
			int num17 = num4 >> lod;
			int num18 = num5 >> lod;
			int num19 = num6 >> lod;
			for (int i = 1; i < 8; i++)
			{
				if ((num16 & i) == i)
				{
					int num20 = num13 * VFVoxelChunkData.S_NearChunkOfs[i, 0];
					int num21 = num14 * VFVoxelChunkData.S_NearChunkOfs[i, 1];
					int num22 = num15 * VFVoxelChunkData.S_NearChunkOfs[i, 2];
					num7 = (num17 + num20) % num;
					num8 = (num18 + num21) % num2;
					num9 = (num19 + num22) % num3;
					if (num7 < 0)
					{
						num7 += num;
					}
					if (num8 < 0)
					{
						num8 += num2;
					}
					if (num9 < 0)
					{
						num9 += num3;
					}
					vFVoxelChunkData = (VFVoxelChunkData)_lodNodes[lod][num7, num8, num9]._data[_idxData];
					if (!vFVoxelChunkData.WriteVoxelAtIdx(num10 - num20 * 32, num11 - num21 * 32, num12 - num22 * 32, voxel, bUpdateLod))
					{
						num16 |= 1 << ((i + 8) & 0x1F);
					}
					else
					{
						_dirtyChunkList.Add(vFVoxelChunkData);
					}
				}
			}
		}
		return num16;
	}

	public VFVoxel SafeRead(int x, int y, int z, int lod = 0)
	{
		return Read(x, y, z, lod);
	}

	public bool SafeWrite(int x, int y, int z, VFVoxel voxel, int lod = 0)
	{
		if (y < 1)
		{
			return false;
		}
		Write(x, y, z, voxel, lod);
		return true;
	}
}

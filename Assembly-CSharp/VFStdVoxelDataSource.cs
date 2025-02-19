using System;
using System.Collections.Generic;
using UnityEngine;

public class VFStdVoxelDataSource : IVxDataSource
{
	private List<VFVoxelChunkData> _dirtyChunkList = new List<VFVoxelChunkData>();

	private int _chunkNumX;

	private int _chunkNumY;

	private int _chunkNumZ;

	private VFVoxelChunkData[,,] _chunks;

	public List<VFVoxelChunkData> DirtyChunkList => _dirtyChunkList;

	public byte this[IntVector3 idx, IntVector4 cposlod]
	{
		get
		{
			int num = cposlod.x % _chunkNumX;
			int num2 = cposlod.y % _chunkNumY;
			int num3 = cposlod.z % _chunkNumZ;
			return _chunks[num, num2, num3].ReadVoxelAtIdx(idx.x, idx.y, idx.z).Volume;
		}
	}

	public VFStdVoxelDataSource(int width, int height, int depth)
	{
		_chunkNumX = width;
		_chunkNumY = height;
		_chunkNumZ = depth;
		_chunks = new VFVoxelChunkData[_chunkNumX, _chunkNumY, _chunkNumZ];
	}

	public VFVoxelChunkData readChunk(int x, int y, int z, int lod = 0)
	{
		return _chunks[x % _chunkNumX, y % _chunkNumY, z % _chunkNumZ];
	}

	public void writeChunk(int x, int y, int z, VFVoxelChunkData vc, int lod = 0)
	{
		try
		{
			_chunks[x % _chunkNumX, y % _chunkNumY, z % _chunkNumZ] = vc;
			vc.ChunkPosLod_w = new IntVector4(x, y, z, lod);
		}
		catch
		{
			Debug.LogError(string.Format("writeChunk Exception. Max Length:({0}, {1}, {2}}), CurPos({3}, {4}, {5})"));
		}
	}

	public VFVoxel Read(int x, int y, int z, int lod = 0)
	{
		int num = 5;
		int num2 = (x >> num) % _chunkNumX;
		int num3 = (y >> num) % _chunkNumY;
		int num4 = (z >> num) % _chunkNumZ;
		int x2 = x & 0x1F;
		int y2 = y & 0x1F;
		int z2 = z & 0x1F;
		return _chunks[num2, num3, num4].ReadVoxelAtIdx(x2, y2, z2);
	}

	public int Write(int x, int y, int z, VFVoxel voxel, int lod = 0, bool bUpdateLod = true)
	{
		_dirtyChunkList.Clear();
		int num = 5;
		int num2 = x >> num;
		int num3 = y >> num;
		int num4 = z >> num;
		int num5 = num2 % _chunkNumX;
		int num6 = num3 % _chunkNumY;
		int num7 = num4 % _chunkNumZ;
		int num8 = x & 0x1F;
		int num9 = y & 0x1F;
		int num10 = z & 0x1F;
		VFVoxelChunkData vFVoxelChunkData = _chunks[num5, num6, num7];
		if (!vFVoxelChunkData.WriteVoxelAtIdx(num8, num9, num10, voxel))
		{
			return 0;
		}
		_dirtyChunkList.Add(vFVoxelChunkData);
		int num11 = 0;
		int num12 = 0;
		int num13 = 0;
		int num14 = 128;
		if (num8 < VFVoxelChunkData.S_MinNoDirtyIdx)
		{
			num11 = -1;
			num14 |= 0x11;
		}
		else if (num8 >= VFVoxelChunkData.S_MaxNoDirtyIdx)
		{
			num11 = 1;
			num14 |= 1;
		}
		if (num9 < VFVoxelChunkData.S_MinNoDirtyIdx)
		{
			num12 = -1;
			num14 |= 0x22;
		}
		else if (num9 >= VFVoxelChunkData.S_MaxNoDirtyIdx)
		{
			num12 = 1;
			num14 |= 2;
		}
		if (num10 < VFVoxelChunkData.S_MinNoDirtyIdx)
		{
			num13 = -1;
			num14 |= 0x44;
		}
		else if (num10 >= VFVoxelChunkData.S_MaxNoDirtyIdx)
		{
			num13 = 1;
			num14 |= 4;
		}
		if (num14 != 128)
		{
			for (int i = 1; i < 8; i++)
			{
				if ((num14 & i) != i)
				{
					continue;
				}
				int num15 = num11 * VFVoxelChunkData.S_NearChunkOfs[i, 0];
				int num16 = num12 * VFVoxelChunkData.S_NearChunkOfs[i, 1];
				int num17 = num13 * VFVoxelChunkData.S_NearChunkOfs[i, 2];
				num5 = (num2 + num15) % _chunkNumX;
				num6 = (num3 + num16) % _chunkNumY;
				num7 = (num4 + num17) % _chunkNumZ;
				try
				{
					vFVoxelChunkData = _chunks[num5, num6, num7];
					if (!vFVoxelChunkData.WriteVoxelAtIdx(num8 - num15 * 32, num9 - num16 * 32, num10 - num17 * 32, voxel))
					{
						num14 |= i << 8;
					}
					else
					{
						_dirtyChunkList.Add(vFVoxelChunkData);
					}
				}
				catch (Exception ex)
				{
					Debug.LogError("Unexpected voxel write at(" + num5 + "," + num6 + "," + num7 + ")" + ex.ToString());
				}
			}
		}
		return num14;
	}

	public VFVoxel SafeRead(int x, int y, int z, int lod = 0)
	{
		if (x < 0 || x >= 18432 || y < 0 || y >= 2944 || z < 0 || z >= 18432)
		{
			return new VFVoxel(0);
		}
		return Read(x, y, z, lod);
	}

	public bool SafeWrite(int x, int y, int z, VFVoxel voxel, int lod = 0)
	{
		if (x < 0 || x >= _chunkNumX << 5 || y < 1 || y >= _chunkNumY << 5 || z < 0 || z >= _chunkNumZ << 5)
		{
			return false;
		}
		Write(x, y, z, voxel, lod);
		return true;
	}
}

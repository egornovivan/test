using System;
using System.Collections.Generic;
using UnityEngine;

public class VFCreationDataSource : IVxDataSource
{
	private const int SHIFT = 5;

	private const int MASK = 31;

	private const int VOXELCNT = 32;

	private int m_ChunkNumX;

	private int m_ChunkNumY;

	private int m_ChunkNumZ;

	private VFVoxelChunkData[,,] m_Chunks;

	private int[,,] m_ChunkDirtyFlags;

	public IntVector3 ChunkNum => new IntVector3(m_ChunkNumX, m_ChunkNumY, m_ChunkNumZ);

	public List<VFVoxelChunkData> DirtyChunkList => null;

	public byte this[IntVector3 idx, IntVector4 cposlod] => m_Chunks[cposlod.x, cposlod.y, cposlod.z].ReadVoxelAtIdx(idx.x, idx.y, idx.z).Volume;

	public VFCreationDataSource(int x_chunk_num, int y_chunk_num, int z_chunk_num)
	{
		m_ChunkNumX = x_chunk_num;
		m_ChunkNumY = y_chunk_num;
		m_ChunkNumZ = z_chunk_num;
		m_Chunks = new VFVoxelChunkData[m_ChunkNumX, m_ChunkNumY, m_ChunkNumZ];
		m_ChunkDirtyFlags = new int[m_ChunkNumX, m_ChunkNumY, m_ChunkNumZ];
	}

	public VFVoxelChunkData readChunk(int x, int y, int z, int lod = 0)
	{
		return m_Chunks[x & 0x1F, y & 0x1F, z & 0x1F];
	}

	public void writeChunk(int x, int y, int z, VFVoxelChunkData data, int lod = 0)
	{
		try
		{
			m_Chunks[x & 0x1F, y & 0x1F, z & 0x1F] = data;
			data.ChunkPosLod_w = new IntVector4(x, y, z, 0);
		}
		catch (Exception ex)
		{
			Debug.LogError("[VC] Write Chunk (" + x + "," + y + "," + z + ") Error: " + ex.ToString());
		}
	}

	public VFVoxel Read(int x, int y, int z, int lod = 0)
	{
		return m_Chunks[x >> 5, y >> 5, z >> 5].ReadVoxelAtIdx(x & 0x1F, y & 0x1F, z & 0x1F);
	}

	public int Write(int x, int y, int z, VFVoxel voxel, int lod = 0, bool bUpdateLod = false)
	{
		int num = x >> 5;
		int num2 = y >> 5;
		int num3 = z >> 5;
		int num4 = x & 0x1F;
		int num5 = y & 0x1F;
		int num6 = z & 0x1F;
		VFVoxelChunkData vFVoxelChunkData = m_Chunks[num, num2, num3];
		if (!vFVoxelChunkData.WriteVoxelAtIdxNoReq(num4, num5, num6, voxel))
		{
			return 0;
		}
		m_ChunkDirtyFlags[num, num2, num3]++;
		int num7 = 2;
		int num8 = 31;
		int num9 = 0;
		int num10 = 0;
		int num11 = 0;
		int num12 = 128;
		if (num4 < num7 && num > 0)
		{
			num9 = -1;
			num12 |= 0x11;
		}
		else if (num4 >= num8 && num < m_ChunkNumX - 1)
		{
			num9 = 1;
			num12 |= 1;
		}
		if (num5 < num7 && num2 > 0)
		{
			num10 = -1;
			num12 |= 0x22;
		}
		else if (num5 >= num8 && num2 < m_ChunkNumY - 1)
		{
			num10 = 1;
			num12 |= 2;
		}
		if (num6 < num7 && num3 > 0)
		{
			num11 = -1;
			num12 |= 0x44;
		}
		else if (num6 >= num8 && num3 < m_ChunkNumZ - 1)
		{
			num11 = 1;
			num12 |= 4;
		}
		if (num12 != 128)
		{
			for (int i = 1; i < 8; i++)
			{
				if ((num12 & i) != i)
				{
					continue;
				}
				int num13 = num9 * VFVoxelChunkData.S_NearChunkOfs[i, 0];
				int num14 = num10 * VFVoxelChunkData.S_NearChunkOfs[i, 1];
				int num15 = num11 * VFVoxelChunkData.S_NearChunkOfs[i, 2];
				try
				{
					vFVoxelChunkData = m_Chunks[num + num13, num2 + num14, num3 + num15];
					if (vFVoxelChunkData.WriteVoxelAtIdxNoReq(num4 - num13 * 32, num5 - num14 * 32, num6 - num15 * 32, voxel))
					{
						m_ChunkDirtyFlags[num + num13, num2 + num14, num3 + num15]++;
					}
					else
					{
						num12 |= i << 8;
					}
				}
				catch (Exception ex)
				{
					Debug.LogError("Unexpected voxel write at (" + (num + num13) + "," + (num2 + num14) + "," + (num3 + num15) + ") Error :" + ex.ToString());
				}
			}
		}
		return num12;
	}

	public VFVoxel SafeRead(int x, int y, int z, int lod = 0)
	{
		if (x < 0 || x >= m_ChunkNumX << 5 || y < 0 || y >= m_ChunkNumY << 5 || z < 0 || z >= m_ChunkNumZ << 5)
		{
			return new VFVoxel(0);
		}
		return Read(x, y, z);
	}

	public bool SafeWrite(int x, int y, int z, VFVoxel voxel, int lod = 0)
	{
		if (x < 0 || x >= m_ChunkNumX << 5 || y < 0 || y >= m_ChunkNumY << 5 || z < 0 || z >= m_ChunkNumZ << 5)
		{
			return false;
		}
		Write(x, y, z, voxel);
		return true;
	}

	public void ClearDirtyFlags()
	{
		for (int i = 0; i < m_ChunkNumX; i++)
		{
			for (int j = 0; j < m_ChunkNumY; j++)
			{
				for (int k = 0; k < m_ChunkNumZ; k++)
				{
					m_ChunkDirtyFlags[i, j, k] = 0;
				}
			}
		}
	}

	public void SubmitReq()
	{
		for (int i = 0; i < m_ChunkNumX; i++)
		{
			for (int j = 0; j < m_ChunkNumY; j++)
			{
				for (int k = 0; k < m_ChunkNumZ; k++)
				{
					if (m_ChunkDirtyFlags[i, j, k] > 0)
					{
						m_Chunks[i, j, k].AddToBuildList();
					}
				}
			}
		}
		ClearDirtyFlags();
	}
}

using System;
using System.Collections.Generic;
using UnityEngine;

public class BSBlock45Data : IBSDataSource
{
	public static Action<int[]> voxelWrite;

	public static int s_ScaleInverted => 2;

	public static float s_Scale => 0.5f;

	public int ScaleInverted => s_ScaleInverted;

	public float Scale => s_Scale;

	public Vector3 Offset => Vector3.zero;

	public float DiagonalOffset => 0.25f;

	public Bounds Lod0Bound => Block45Man.self.LodMan._Lod0ViewBounds;

	public int DataType => 1;

	public BSVoxel Read(int x, int y, int z, int lod = 0)
	{
		B45Block block = Block45Man.self.DataSource.Read(x, y, z, lod);
		return new BSVoxel(block);
	}

	public int Write(BSVoxel voxel, int x, int y, int z, int lod = 0)
	{
		if (voxelWrite != null)
		{
			voxelWrite(new int[3] { x, y, z });
		}
		return Block45Man.self.DataSource.Write(voxel.ToBlock(), x, y, z, lod);
	}

	public BSVoxel SafeRead(int x, int y, int z, int lod = 0)
	{
		B45Block block = Block45Man.self.DataSource.SafeRead(x, y, z, lod);
		return new BSVoxel(block);
	}

	public bool SafeWrite(BSVoxel voxel, int x, int y, int z, int lod = 0)
	{
		if (voxelWrite != null)
		{
			voxelWrite(new int[3] { x, y, z });
		}
		return Block45Man.self.DataSource.SafeWrite(voxel.ToBlock(), x, y, z, lod);
	}

	public bool VoxelIsZero(BSVoxel voxel, float volume)
	{
		return voxel.value0 >> 2 == 0;
	}

	public BSVoxel Add(BSVoxel voxel, int x, int y, int z, int lod = 0)
	{
		if (voxel.value0 >> 2 == 0)
		{
			return new BSVoxel(Block45Man.self.DataSource.Read(x, y, z));
		}
		Block45Man.self.DataSource.Write(voxel.ToBlock(), x, y, z, lod);
		return voxel;
	}

	public BSVoxel Subtract(BSVoxel voxel, int x, int y, int z, int lod = 0)
	{
		B45Block b45Block = default(B45Block);
		Block45Man.self.DataSource.Write(b45Block, x, y, z, lod);
		return new BSVoxel(b45Block);
	}

	public bool ReadExtendableBlock(IntVector4 pos, out List<IntVector4> posList, out List<BSVoxel> voxels)
	{
		List<B45Block> blocks = null;
		if (Block45Man.self.DataSource.ReadExtendableBlock(pos, out posList, out blocks))
		{
			voxels = new List<BSVoxel>();
			foreach (B45Block item in blocks)
			{
				voxels.Add(new BSVoxel(item));
			}
			return true;
		}
		posList = null;
		voxels = null;
		return false;
	}
}

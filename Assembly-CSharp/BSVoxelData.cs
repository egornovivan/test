using System.Collections.Generic;
using UnityEngine;

public class BSVoxelData : IBSDataSource
{
	public static int s_ScaleInverted => 1;

	public static float s_Scale => 1f;

	public int ScaleInverted => s_ScaleInverted;

	public float Scale => s_Scale;

	public Vector3 Offset => new Vector3(-0.5f, -0.5f, -0.5f);

	public float DiagonalOffset => 0.15f;

	public int DataType => 0;

	public Bounds Lod0Bound => VFVoxelTerrain.self.LodMan._Lod0ViewBounds;

	public BSVoxel Read(int x, int y, int z, int lod = 0)
	{
		VFVoxel voxel = VFVoxelTerrain.self.Voxels.Read(x, y, z, lod);
		return new BSVoxel(voxel);
	}

	public int Write(BSVoxel voxel, int x, int y, int z, int lod = 0)
	{
		VFVoxelTerrain.self.AlterVoxelInBuild(x, y, z, voxel.ToVoxel());
		return 0;
	}

	public BSVoxel SafeRead(int x, int y, int z, int lod = 0)
	{
		VFVoxel voxel = VFVoxelTerrain.self.Voxels.SafeRead(x, y, z, lod);
		return new BSVoxel(voxel);
	}

	public bool SafeWrite(BSVoxel voxel, int x, int y, int z, int lod = 0)
	{
		VFVoxelTerrain.self.AlterVoxelInBuild(x, y, z, voxel.ToVoxel());
		return true;
	}

	public bool VoxelIsZero(BSVoxel voxel, float volume)
	{
		return (float)(int)voxel.value0 < volume;
	}

	public BSVoxel Add(BSVoxel voxel, int x, int y, int z, int lod = 0)
	{
		voxel.volmue = (byte)Mathf.Clamp(VFVoxelTerrain.self.Voxels.Read(x, y, z, lod).Volume + voxel.volmue, 0, 255);
		VFVoxelTerrain.self.Voxels.Write(x, y, z, voxel.ToVoxel(), lod);
		return voxel;
	}

	public BSVoxel Subtract(BSVoxel voxel, int x, int y, int z, int lod = 0)
	{
		VFVoxel vFVoxel = VFVoxelTerrain.self.Voxels.Read(x, y, z, lod);
		VFVoxel voxel2 = new VFVoxel(vFVoxel.Volume, vFVoxel.Type);
		voxel2.Volume = (byte)Mathf.Clamp(vFVoxel.Volume - voxel.volmue, 0, 255);
		VFVoxelTerrain.self.AlterVoxelInBuild(x, y, z, voxel2);
		return voxel;
	}

	public bool ReadExtendableBlock(IntVector4 pos, out List<IntVector4> posList, out List<BSVoxel> voxels)
	{
		posList = new List<IntVector4>();
		voxels = new List<BSVoxel>();
		return false;
	}
}

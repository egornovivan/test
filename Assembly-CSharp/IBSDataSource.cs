using System.Collections.Generic;
using UnityEngine;

public interface IBSDataSource
{
	int ScaleInverted { get; }

	float Scale { get; }

	Vector3 Offset { get; }

	float DiagonalOffset { get; }

	Bounds Lod0Bound { get; }

	int DataType { get; }

	BSVoxel Read(int x, int y, int z, int lod = 0);

	int Write(BSVoxel voxel, int x, int y, int z, int lod = 0);

	BSVoxel SafeRead(int x, int y, int z, int lod = 0);

	bool SafeWrite(BSVoxel voxel, int x, int y, int z, int lod = 0);

	BSVoxel Add(BSVoxel voxel, int x, int y, int z, int lod = 0);

	BSVoxel Subtract(BSVoxel voxel, int x, int y, int z, int lod = 0);

	bool VoxelIsZero(BSVoxel voxel, float vomlue);

	bool ReadExtendableBlock(IntVector4 pos, out List<IntVector4> posList, out List<BSVoxel> voxels);
}

using System.Collections.Generic;

public interface IVxDataSource
{
	List<VFVoxelChunkData> DirtyChunkList { get; }

	byte this[IntVector3 idx, IntVector4 cposlod] { get; }

	VFVoxelChunkData readChunk(int cx, int cy, int cz, int lod = 0);

	void writeChunk(int cx, int cy, int cz, VFVoxelChunkData data, int lod = 0);

	VFVoxel Read(int x, int y, int z, int lod = 0);

	int Write(int x, int y, int z, VFVoxel voxel, int lod = 0, bool bUpdateLod = true);

	VFVoxel SafeRead(int x, int y, int z, int lod = 0);

	bool SafeWrite(int x, int y, int z, VFVoxel voxel, int lod = 0);
}

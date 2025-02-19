using System;

[Flags]
public enum EDependChunkType
{
	ChunkNotAvailable = 0,
	ChunkTerEmp = 1,
	ChunkTerCol = 2,
	ChunkBlkEmp = 4,
	ChunkBlkCol = 8,
	ChunkWatEmp = 0x10,
	ChunkWatCol = 0x20,
	ChunkTerMask = 3,
	ChunkBlkMask = 0xC,
	ChunkWatMask = 0x30,
	ChunkEmpMask = 0x15,
	ChunkColMask = 0x2A
}

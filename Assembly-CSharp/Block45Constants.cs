public class Block45Constants
{
	public const int MAX_LOD = 4;

	public const int _shift = 3;

	public const int _mask = 7;

	public const int _numVoxelsPerAxis = 8;

	public const int _numVoxelsPrefix = 1;

	public const int _numVoxelsPostfix = 1;

	public const int _shiftToScale = -1;

	public const int _scaledShift = 2;

	public const int _scaledSize = 4;

	public const int ChunkPhysicalSize = 4;

	public const float _scale = 0.5f;

	public const int _scaleInverted = 2;

	public const int VOXEL_ARRAY_AXIS_SIZE = 10;

	public const int VOXEL_ARRAY_AXIS_SQUARED = 100;

	public const int VOXEL_ARRAY_LENGTH = 1000;

	public const int VOXEL_ARRAY_LENGTH_VT = 2000;

	public const int _worldSideLenX = 20100;

	public const int _worldSideLenY = 2048;

	public const int _worldSideLenZ = 20100;

	public const int _worldMaxCX = 2512;

	public const int _worldMaxCY = 256;

	public const int _worldMaxCZ = 2512;

	public const int _worldSideLenXInVoxels = 512;

	public const int _worldSideLenYInVoxels = 128;

	public const int _worldSideLenZInVoxels = 512;

	public const int _MeshDistanceX = 256;

	public const int _MeshDistanceY = 256;

	public const int _MeshDistanceZ = 256;

	public const int _LodMaxX = 256;

	public const int _LodMaxY = 256;

	public const int _LodMaxZ = 256;

	public const int MaxMaterialCount = 256;

	public const int NumChunksPerFrame = 30;

	public static int Size(int lod)
	{
		return 1 << 2 + lod;
	}

	public static int CenterOfs(int lod)
	{
		return 1 << 2 + lod - 1;
	}

	public static IntVector4 ToWorldUnitPos(int x, int y, int z, int lod)
	{
		IntVector4 zero = IntVector4.Zero;
		return new IntVector4(x >> 1, y >> 1, z >> 1, lod);
	}

	public static IntVector4 ToBlockUnitPos(int x, int y, int z, int lod)
	{
		return new IntVector4(x << 1, y << 1, z << 1, lod);
	}
}

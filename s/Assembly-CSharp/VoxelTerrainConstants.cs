public class VoxelTerrainConstants
{
	public const int MAX_LOD = 4;

	public const int _shift = 5;

	public const int _mask = 31;

	public const int _numVoxelsPerAxis = 32;

	public const int _numVoxelsPrefix = 1;

	public const int _numVoxelsPostfix = 2;

	public const float _scale = 1f;

	public const float _isolevel = 0.5f;

	public const byte _isolevelInByte = 127;

	public const int _mapFileWidth = 6144;

	public const int _mapFileHeight = 2944;

	public const int _mapPieceSize = 128;

	public const int _mapPieceCountXZ = 48;

	public const int _mapPieceCountY = 23;

	public const int _mapChunkCountXZ = 192;

	public const int _mapChunkCountY = 92;

	public const int _ChunksPerPiecePerAxis = 4;

	public const int _xChunkPerPiece = 4;

	public const int _yChunkPerPiece = 4;

	public const int _zChunkPerPiece = 4;

	public const int _ChunksPerPiece = 64;

	public const int TRANSVOXEL_EDGE_SIZE = 65;

	public const int VOXEL_ARRAY_AXIS_SIZE = 35;

	public const int VOXEL_ARRAY_AXIS_SIZE_VT = 70;

	public const int VOXEL_ARRAY_AXIS_SQUARED = 1225;

	public const int VOXEL_ARRAY_AXIS_SQUARED_VT = 2450;

	public const int VOXEL_ARRAY_LENGTH = 42875;

	public const int VOXEL_ARRAY_LENGTH_VT = 85750;

	public const int VOXEL_ARRAY_LENGTH_OTHER = 42875;

	public const int VOXEL_NUM_PER_PIECE = 2744000;

	public const int _worldSideLenX = 18432;

	public const int _worldSideLenY = 2944;

	public const int _worldSideLenZ = 18432;

	public const int _worldMaxCX = 576;

	public const int _worldMaxCY = 92;

	public const int _worldMaxCZ = 576;

	public static int _maxLod = 3;

	private static int __xLodRootChunkCount = 8;

	private static int __yLodRootChunkCount = 4;

	private static int __zLodRootChunkCount = 8;

	public static int _xLodRootChunkCount => __xLodRootChunkCount;

	public static int _yLodRootChunkCount => __yLodRootChunkCount;

	public static int _zLodRootChunkCount => __zLodRootChunkCount;

	public static int _xChunkCount => __xLodRootChunkCount << _maxLod;

	public static int _yChunkCount => __yLodRootChunkCount << _maxLod;

	public static int _zChunkCount => __zLodRootChunkCount << _maxLod;

	public static int _xVoxelCount => __xLodRootChunkCount << _maxLod + 5;

	public static int _yVoxelCount => __yLodRootChunkCount << _maxLod + 5;

	public static int _zVoxelCount => __zLodRootChunkCount << _maxLod + 5;

	public static void ResetRootChunkCount()
	{
		__xLodRootChunkCount = 8;
		__yLodRootChunkCount = 4;
		__zLodRootChunkCount = 8;
	}

	public static void ResetRootChunkCount(int xCnt)
	{
		__xLodRootChunkCount = xCnt;
		__yLodRootChunkCount = 4;
		__zLodRootChunkCount = 8;
	}

	public static void ResetRootChunkCount(int xCnt, int yCnt)
	{
		__xLodRootChunkCount = xCnt;
		__yLodRootChunkCount = yCnt;
		__zLodRootChunkCount = 8;
	}

	public static void ResetRootChunkCount(int xCnt, int yCnt, int zCnt)
	{
		__xLodRootChunkCount = xCnt;
		__yLodRootChunkCount = yCnt;
		__zLodRootChunkCount = zCnt;
	}
}

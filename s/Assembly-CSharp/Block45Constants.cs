public class Block45Constants
{
	public const int MAX_LOD = 4;

	public const int _xLodRootChunkCount = 8;

	public const int _yLodRootChunkCount = 4;

	public const int _zLodRootChunkCount = 8;

	public const int _shift = 3;

	public const int _mask = 7;

	public const int _numVoxelsPerAxis = 8;

	public const int _numVoxelsPrefix = 1;

	public const int _numVoxelsPostfix = 1;

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

	public const int MaxMaterialCount = 256;

	public static int _maxLod = 3;

	public static int _xChunkCount => 8 << _maxLod;

	public static int _yChunkCount => 4 << _maxLod;

	public static int _zChunkCount => 8 << _maxLod;

	public static int _xVoxelCount => 8 << _maxLod + 3;

	public static int _yVoxelCount => 4 << _maxLod + 3;

	public static int _zVoxelCount => 8 << _maxLod + 3;
}

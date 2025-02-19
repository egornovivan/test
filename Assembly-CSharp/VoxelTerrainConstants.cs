public class VoxelTerrainConstants
{
	public const int _shift = 5;

	public const int _mask = 31;

	public const int _nmask = -32;

	public const int _numVoxelsPerAxis = 32;

	public const int _numVoxelsPrefix = 1;

	public const int _numVoxelsPostfix = 2;

	public const float _scale = 1f;

	public const float _isolevel = 0.5f;

	public const byte _isolevelInByte = 128;

	public const int _mapFileWidth = 6144;

	public const int _mapFileHeight = 2944;

	public const int _mapFileCountX = 3;

	public const int _mapFileCountZ = 3;

	public const int _mapPieceChunkShift = 2;

	public const int _mapPieceShift = 7;

	public const int _mapPieceSize = 128;

	public const int _mapPieceCountXorZ = 48;

	public const int _mapPieceCountY = 23;

	public const int _mapPieceCountXYZ = 52992;

	public const int _mapChunkCountXorZ = 192;

	public const int _mapChunkCountY = 92;

	public const int _mapChunkCountXYZ = 3391488;

	public const float _normalHiltPivotY = 0.1f;

	public const float _spHiltPivotY = 0.12f;

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

	public const int VOXEL_NUM_PER_PIECE = 2744000;

	public const int _worldSideLenX = 18432;

	public const int _worldSideLenY = 2944;

	public const int _worldSideLenZ = 18432;

	public const int _worldMaxCX = 576;

	public const int _worldMaxCY = 92;

	public const int _worldMaxCZ = 576;

	public static int WorldMaxCY(int lod)
	{
		int num = 2 + lod;
		return 92 >> num << num;
	}
}

using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using VoxelPaintXML;

public class VFDataRTGen : IVxDataLoader
{
	private class RTGenChunkReq
	{
		public VFVoxelChunkData terraChunk;

		public VFVoxelChunkData waterChunk;

		public int terraStamp;

		public int waterStamp;

		public bool TerraOutOfDate => terraChunk == null || !terraChunk.IsStampIdentical(terraStamp);

		public bool WaterOutOfDate => waterChunk == null || !waterChunk.IsStampIdentical(waterStamp);
	}

	public const byte FLOOR_TYPE = 45;

	public const byte BLOCK_TOWN = 46;

	public const byte BlOCK_TOWN_CONNECTION = 71;

	private const float HalfPiTo1000 = 2000f / (float)Math.PI;

	private const byte BLOCK_AIR = 0;

	private const byte BLOCK_STONE = 1;

	private const float TAN45 = 0.727f;

	private const int c_noMonsterRadius = 96;

	private const int TownConnectionDepth = 4;

	private const int BiomaDepth = 8;

	public const int GRASS_REGION = 0;

	public const int FOREST_REGION = 1;

	public const int DESERT_REGION = 2;

	public const int REDSTONE_REGION = 3;

	public const int RAINFOREST_REGION = 4;

	public const int TERRAIN_REGION_CNT = 6;

	private const float c_fTerTypeMin = -0.39f;

	private const float c_seasideStart = -0.32f;

	private const float c_plainStart = 5f / 64f;

	private const float c_hillStart = 5f / 32f;

	private const float c_highlandStart = 0.8f;

	private const float c_fTerTypeMax = 1.15f;

	private const float c_plainLakeStartTerType = 0.07f;

	private const float c_topMountainStart = 75f / 128f;

	private const float c_topLakeStartTerType = 0.8f;

	private const float c_topLakeBankStartTerType = 0.82f;

	private const int GRASS_REGION_CNT = 5;

	public const int c_hillThickness = 256;

	private const float TerBaseNoisePara = 0.08f;

	private const float _rockyPara = 0.1f;

	private const float _terTypeCoef = 1f;

	private const float _terDecreaseLarge = 1.2f;

	private const float _terDecreaseSmall = 1f;

	private const int SeasideStartIndex = 0;

	private const int PlainStartIndex = 1;

	private const int HillStartIndex = 2;

	private const int MountainStartIndex = 3;

	private const int MountainEndIndex = 4;

	private const int HASMidBottom = 5;

	private const int HASMidTop = 256;

	private const int HASMinMin = 5;

	private const int HASMaxMax = 256;

	private const float islandStandard = 80f;

	private const float islandMin = 60f;

	private const float islandMax = 100f;

	private const float flatMidBottom = 0.05f;

	private const float flatMidMid = 0.5f;

	private const float flatMidTop = 3f;

	private const float flatMinMin = 0.03f;

	private const float flatMaxMax = 3.5f;

	private const int DensityClampFilter01 = 0;

	private const int FTerTypeIndex = 1;

	private const int DensityClampFilter02 = 2;

	private const int HillTerIndex01 = 3;

	private const int FTerTypeIndex01 = 4;

	private const int MountainParamIndex01 = 5;

	private const int MountainParamIndex011 = 6;

	private const int SierraIndex = 7;

	private const int HillTerIndex02 = 8;

	private const int FlatIndex = 9;

	private const int TownChangeIndex = 10;

	private const int Volume3DNoiseIndex = 11;

	private const int MountainParamIndex02 = 12;

	private const int MountainParamIndex022 = 13;

	private const int FTerTypeIndex02 = 14;

	private const int HASFilterIndex = 15;

	private const int TopCorrectionIndex = 16;

	private const int HASChangeIndex = 17;

	private const float TownChangeFrequency = 0.25f;

	private const float TownChangeFactor = 2f;

	private const float BiomaTerrainChangeRadius = 48f;

	private const float BiomaTerrainTopRadius = 32f;

	private const float BiomaTerrainChangeFrequency = 0.25f;

	private const float BiomaTerrainChangeFactor = 2f;

	public const int TEMP_WATER_PLUS_MIN = 5;

	public const int TEMP_WATER_PLUS_MAX = 20;

	public const int WET_WATER_PLUS_MIN = 50;

	public const int WET_WATER_PLUS_MAX = 80;

	public const float FILL_VOLUME = 160f;

	private const float bridgeStart = 0.98f;

	private const float bridgeEdge = 0.93f;

	private const float bridgeEnd = 0.5f;

	private const int RiverIndex = 0;

	private const int LakeIndex = 1;

	private const int LakeBottomHeightIndex = 2;

	private const int ContinentBoundIndex = 3;

	private const int BridgeIndex = 4;

	private const int RiverBottomChangeIndex = 5;

	private const int River2DChangeIndex = 6;

	private const int LakeChangeIndex = 7;

	private const float terTypeChangeDist = 64f;

	private const int MAX_TILES_IN_CACHE = 30;

	private const float CaveXZFrequency = 0.125f;

	private const float CaveHeightFrequency = 0.125f;

	private const float CaveThicknessFrequency = 1f;

	private const int CaveHeightMin = 5;

	private const int CaveThicknessMax = 30;

	private const float CaveXZThreshold = 0.05f;

	private const int CaveHeightTerValue = 80;

	private const int CaveThicknessTerValue = 20;

	private const float CaveThresholdTerValue = 0.2f;

	private const float CaveHillFrequency = 0.125f;

	private const float CaveHillHeightFrequency = 0.125f;

	private const float CaveHillThicknessFrequency = 0.25f;

	private const int CaveHillThicknessMax = 25;

	private const float CaveHillThreshold = 0.08f;

	private const float CaveHillFloorPer = 0.5f;

	private const float MineFrequency = 0.5f;

	private const float MineChance = 0.5f;

	private const int MineThickness = 50;

	private const int MineStartNoiseIndex = 6;

	private const int HEIGHT_INDEX = 3;

	private const int THICKNESS_INDEX = 4;

	private const int QUANTITY_INDEX = 5;

	private const int HeightOffsetMax = 30;

	private const int ThicknessOffsetMax = 50;

	private const int JumpCount = 2;

	private const byte GenVolumeThreshold = 168;

	private const int GrasslandIndex = 0;

	private const int ForestIndex = 1;

	private const int DesertIndex = 2;

	private const int RedstoneIndex = 3;

	private const int ChangeIndex = 4;

	private const int RainforestIndex = 5;

	private const int c_radiusPlants = 10;

	private const float TownHillDistance = 64f;

	private const float TownHillChangeFactor = 0f;

	private const float TownWaterDistance = 48f;

	private const float TownConnectionWidth = 4f;

	private const float TownConnectionHillDistance = 64f;

	private const float TownConnectionWaterDistance = 48f;

	private const float TownConnectionFlatDistance = 4f;

	private const float NoTownStartDistance = 96f;

	private const int coalIndex = 0;

	private const int ironIndex = 1;

	private const int copperIndex = 2;

	private const int aluminumIndex = 3;

	private const int silverIndex = 4;

	private const int goldIndex = 5;

	private const int oilIndex = 6;

	private const int zincIndex = 7;

	private const int lithiumIndex = 8;

	private const int sulfurIndex = 9;

	private const int diamondIndex = 10;

	private const int titaniumIndex = 11;

	public static IntVector2 noTownStartPos;

	public static List<RandomMapTypePoint> BiomaDistList = new List<RandomMapTypePoint>();

	private static RegionDescArrayCLS[] regionArray;

	public static ClimateType sceneClimate;

	private static readonly float[] GRASS_REGION_THRESHOLD = new float[6]
	{
		-0.39f,
		-0.32f,
		5f / 64f,
		5f / 32f,
		0.8f,
		1.15f
	};

	private static readonly int[][] S_VoxelIndex = new int[4][]
	{
		new int[35]
		{
			0, 1, 2, 3, 4, 5, 6, 7, 8, 9,
			10, 11, 12, 13, 14, 15, 16, 17, 18, 19,
			20, 21, 22, 23, 24, 25, 26, 27, 28, 29,
			30, 31, 32, 33, 34
		},
		new int[35]
		{
			0, 1, 3, 5, 7, 9, 11, 13, 15, 17,
			19, 21, 23, 25, 27, 29, 31, 257, 259, 261,
			263, 265, 267, 269, 271, 273, 275, 277, 279, 281,
			283, 285, 287, 289, 290
		},
		new int[35]
		{
			0, 1, 5, 9, 13, 17, 21, 25, 29, 257,
			261, 265, 269, 273, 277, 281, 285, 513, 517, 521,
			525, 529, 533, 537, 541, 769, 773, 777, 781, 785,
			789, 793, 797, 801, 802
		},
		new int[35]
		{
			0, 1, 9, 17, 25, 257, 265, 273, 281, 513,
			521, 529, 537, 769, 777, 785, 793, 1025, 1033, 1041,
			1049, 1281, 1289, 1297, 1305, 1537, 1545, 1553, 1561, 1793,
			1801, 1809, 1817, 1825, 1826
		}
	};

	private static int TEST_REGION_CNT;

	private static float[] TEST_REGION_THRESHOLD;

	public static int s_noiseHeight = 128;

	public static int s_seaDepth = 5;

	public float PlainThickness;

	public static int MountainThickness = 256;

	private static int s_noiseWidth = 128;

	public static float s_detailScale = 1f / (float)s_noiseWidth;

	private static float DensityDelta = 0.02f;

	private static float DensityDeltaHalf255Reci = 127.5f / DensityDelta;

	private static float DensityThreshold = -0.215f;

	private static float DensityThresholdMinusDelta = DensityThreshold - DensityDelta;

	private static float DensityThresholdPlusDelta = DensityThreshold + DensityDelta;

	private static int GrasslandChance = 25;

	private static int ForestChance = 25;

	private static int DesertChance = 25;

	private static int RedstoneChance = 25;

	private static RandomMapType CurrentMapType;

	private static int MapTypeOffset = 5;

	private static int[] MapTypeChance;

	private static int[] MapTypeValue;

	private static List<int> mapTypeList;

	private static float mapTypeFrequency0 = 0.04f;

	private static float mapTypeFrequencyX = 0.04f;

	private static float mapTypeFrequencyZ = 0.04f;

	private static float changeMapTypeFrequency = 16f;

	private static float terrainFrequency0 = 0.035f;

	private static float terrainFrequencyX = 0.035f;

	private static float terrainFrequencyZ = 0.035f;

	private static float MountainFrequencyFactor = 1f;

	private static float SierraFrequencyFactor = 0.125f;

	private static float HASEnd;

	private static float HASEnd2;

	private static float HASFilterFrequency = terrainFrequencyX;

	private static float HAS2FilterFrequency = HASFilterFrequency * 2f;

	public static int HASMin = 5;

	public static int HASMid = 30;

	public static int HASMax = 192;

	private static float HASChangeValue = 0.15f;

	private static float HASChangeValueSierra = 0.3f;

	private static float islandFactor = 80f;

	public static float flatMin = 0.03f;

	public static float flatMid = 0.25f;

	public static float flatMax = 2f;

	public static float flatFrequency = 1f / 128f;

	public static int waterSeed = 1;

	public static float waterHeight;

	private static float riverFrequency1 = 1f / 128f;

	private static float riverFrequency100 = 0.25f;

	private static float riverFrequencyNow;

	private static float riverFrequencyX = 0.0625f;

	private static float riverFrequencyZ = 0.0625f;

	private static float riverBottomPercent1 = 0.1f;

	private static float riverBottomPercent100 = 0.7f;

	private static float riverBottomPercentNow;

	private static float riverThreshold1 = 1f / 32f;

	private static float riverThreshold100 = 0.125f;

	private static float riverWidth1 = riverThreshold1 / riverFrequency1;

	private static float riverWidth100 = riverThreshold100 / riverFrequency1;

	private static float riverWidthNow;

	private static float bridgeFrequency1 = 0.125f;

	private static float bridgeFrequency100 = 0.00390625f;

	private static float bridgeFrequencyX = riverFrequency1 + (riverFrequency100 - riverFrequency1) * 0.125f;

	private static float bridgeFrequencyZ = bridgeFrequencyX;

	private static float bridgeThreshold1 = 0.0375f;

	private static float bridgeThreshold100 = 3f / 160f;

	private static float bridgeThreshold = 0.15f;

	private static float bridgeCof = 1f;

	private static float bridgeMaxHeight = 0.7f;

	private static float bridgeTopThreshold = 0.5f;

	private static float riverThreshold = riverThreshold1;

	private static float riverBankPercent = 0.9f;

	private static int densityMin = 1;

	private static int densityMax = 100;

	private static int widthMin = 1;

	private static int widthMax = 100;

	private static float continentBoundFrequency = 2f;

	private static float lakeChangeFrequency = 1f / 3f;

	private static float lakeFrequency = 0.0625f;

	private static float lakeThreshold = 0.875f;

	private static float lakeBankPercent = 0.05f;

	private static float selectedBiomaPlus = 0.6f;

	private static float climateDryPlus = 0.3f;

	private static float changeBiomaThreshold = 0.15f;

	public static float changeBiomaDiff = 24f;

	public static float biomaChangeFTerTypeDiff = 30f;

	private static float disturbFrequency = 12f;

	private static List<float[]> terTypeChanceIncList;

	private static float regionMineChance;

	private static bool isTownTile;

	private static double[][] dTileNoiseBuf;

	private static float[][] fTileHeightBuf;

	private static float[][] fTileGradTanBuf;

	private static RandomMapType[][] tileMapType;

	private static float[] fTerDensityClamp;

	private static float[] fTerDensityClampBase;

	private static float[] fTerNoiseHeight;

	private static readonly byte[] SolidBottomVoxel = new byte[2] { 255, 10 };

	private static readonly byte[] NoiseSeedsPlus = new byte[20]
	{
		0, 1, 2, 3, 4, 5, 6, 7, 8, 9,
		100, 101, 102, 103, 104, 105, 106, 107, 108, 109
	};

	private static readonly byte[] CaveSeedsPlus = new byte[6] { 90, 91, 92, 93, 94, 95 };

	private static readonly byte[] MineSeedsPlus = new byte[42]
	{
		10, 11, 12, 13, 14, 15, 16, 17, 18, 19,
		20, 21, 22, 23, 24, 25, 26, 27, 28, 29,
		30, 31, 32, 33, 34, 35, 36, 37, 38, 39,
		40, 41, 42, 43, 44, 45, 46, 47, 48, 49,
		50, 51
	};

	private static readonly byte[] RiverSeedsPlus = new byte[8] { 64, 65, 66, 67, 68, 69, 70, 71 };

	private static readonly byte[] BiomaSeedsPlus = new byte[6] { 80, 81, 82, 83, 84, 85 };

	private static SimplexNoise[] myNoise = null;

	private static SimplexNoise[] myCaveNoise = null;

	private static SimplexNoise[] myMineNoise = null;

	private static SimplexNoise[] myRiverNoise = null;

	private static SimplexNoise[] myBiomaNoise = null;

	private static int staticSeed = -1;

	private static int[] MineStartHeightList;

	private static double minePerturbanceFrequency0 = 0.25;

	private static double minePerturbanceFrequency1 = 0.0625;

	private static bool metalReduceSwitch = true;

	private static int metalReduceArea = 3000;

	public static float mineHeightFrequency = 0.5f;

	public static float mineThicknessFrequency = 2f;

	public static float mineQuantityFrequency = 8f;

	private static int HeightOffsetTer = 64;

	private static float DensityClampPlainTopValue = 0.3f;

	private static float DensityClampAllTopValue = 2.3f;

	private static float DensityClampBaseBlendFactor = 0.2f;

	private static float DensityClampLinerTopValue = 2.3f;

	private static float DensityClampLinerBlendSeaSide = 0.05f;

	private static float DensityClampLinerBlend = 0.5f;

	private byte[] _tmpInterTerraVoxels1;

	private byte[] _tmpInterTerraVoxels2;

	private byte[] _tmpInterWaterVoxels1;

	private byte[] _tmpInterWaterVoxels2;

	private byte[] _tmpBridgeVoxels;

	private VFTile _curTile;

	private VFTile _lodTile;

	private VFDataRTGenFileCache _tileFileCache;

	private byte[] _lrbfVol = new byte[4];

	private byte[] _lrbfVolDn = new byte[4];

	private IntVector2 _tmpVec2 = new IntVector2();

	private IntVector3 _tmpVec3 = new IntVector3();

	private IntVector4 _tmpVec4 = new IntVector4();

	private IntVector2 _tmpTileIndex = new IntVector2();

	private List<int> _tmpTownIdList = new List<int>();

	private List<VArtifactUnit> _tmpNewTowns = new List<VArtifactUnit>();

	private bool bImmMode;

	private static VoxelPaintXMLParser paintConfig;

	public static Dictionary<IntVector2, List<TreeInfo>> s_dicTreeInfoList = new Dictionary<IntVector2, List<TreeInfo>>();

	public static Dictionary<IntVector2, List<VoxelGrassInstance>> s_dicGrassInstList = new Dictionary<IntVector2, List<VoxelGrassInstance>>();

	public static Dictionary<IntVector3, byte> townFloorVoxelByte = new Dictionary<IntVector3, byte>();

	public System.Random TownRand = new System.Random();

	private static int renderedCount = 0;

	private static long sumTick = 0L;

	private static readonly int[] s_ofsToCheckPlants = new int[16]
	{
		-10, -10, -10, 0, -10, 10, 0, -10, 0, 10,
		10, -10, 10, 0, 10, 10
	};

	private Dictionary<IntVector4, RTGenChunkReq> _chunkReqList = new Dictionary<IntVector4, RTGenChunkReq>();

	private static float mineGenChanceFactor = 1.5f;

	public static int HillMax => (s_seaDepth + 256 <= s_noiseHeight - 1) ? (s_seaDepth + 256) : (s_noiseHeight - 1);

	public static int MountainMax => (s_seaDepth + MountainThickness <= s_noiseHeight - 1) ? (s_seaDepth + MountainThickness) : (s_noiseHeight - 1);

	private static float HeightScale => 1f / (float)s_noiseHeight;

	private static float HeightScale_1 => 0.32f / (float)s_noiseHeight;

	private static int HeightScalePivot => s_noiseHeight / 2;

	private static int DensityClampPivot => s_noiseHeight - 8;

	private static float HASStart => s_seaDepth;

	private static float HASChangeFrequency => terrainFrequencyX * 2f;

	public static float TownConnectionAreaWidth => (4f + Mathf.Max(64f, 48f)) * 3f;

	public static float TownConnectionAreaTypeWidth => 12f;

	public static float TownChangeMaxDistance => Mathf.Max(64f, 48f);

	public static float TownChangeMaxFactor => 3f;

	public static float WaterHeightBase => 3.5f + (float)s_seaDepth;

	private static float bridgeTopValue => 0.9f;

	private static float lakeBottomHeightMax0 => s_seaDepth + 5;

	private static int lakeAddedWaterTop => s_noiseHeight - 100;

	private static int rockyStart2 => HillMax;

	private static int HillTopCorrection => HillMax - 2;

	private static int MountainTopCorrection => s_noiseHeight;

	private static int CaveHillHeightMax => s_noiseHeight - 100;

	public static double MineFrequency0
	{
		get
		{
			return minePerturbanceFrequency0;
		}
		set
		{
			minePerturbanceFrequency0 = value;
		}
	}

	public static double MineFrequency1
	{
		get
		{
			return minePerturbanceFrequency1;
		}
		set
		{
			minePerturbanceFrequency1 = value;
		}
	}

	public static bool MetalReduceSwitch
	{
		get
		{
			return metalReduceSwitch;
		}
		set
		{
			metalReduceSwitch = value;
		}
	}

	public static int MetalReduceArea
	{
		get
		{
			return metalReduceArea;
		}
		set
		{
			metalReduceArea = value;
		}
	}

	public bool IsIdle => _chunkReqList.Count == 0;

	public bool ImmMode
	{
		get
		{
			return bImmMode;
		}
		set
		{
			bImmMode = value;
		}
	}

	public static bool townAvailable { get; set; }

	public static int MaxTownHeight => s_seaDepth + 256 + 40;

	private static float TownConnectionFlatMin => 1f;

	public VFDataRTGen(int seed)
	{
		staticSeed = seed;
		string[] array = new string[33];
		array[0] = RandomMapConfig.GetModeInt().ToString();
		array[1] = "|";
		int randomMapID = (int)RandomMapConfig.RandomMapID;
		array[2] = randomMapID.ToString();
		array[3] = "|";
		int vegetationId = (int)RandomMapConfig.vegetationId;
		array[4] = vegetationId.ToString();
		array[5] = "|";
		int scenceClimate = (int)RandomMapConfig.ScenceClimate;
		array[6] = scenceClimate.ToString();
		array[7] = "|";
		array[8] = RandomMapConfig.mapSize.ToString();
		array[9] = "|";
		array[10] = RandomMapConfig.TerrainHeight.ToString();
		array[11] = "|";
		array[12] = RandomMapConfig.riverWidth.ToString();
		array[13] = "|";
		array[14] = RandomMapConfig.riverDensity.ToString();
		array[15] = "|";
		array[16] = RandomMapConfig.plainHeight.ToString();
		array[17] = "|";
		array[18] = RandomMapConfig.flatness.ToString();
		array[19] = "|";
		array[20] = RandomMapConfig.bridgeMaxHeight.ToString();
		array[21] = "|";
		array[22] = RandomMapConfig.allyCount.ToString();
		array[23] = "|";
		array[24] = RandomMapConfig.mirror.ToString();
		array[25] = "|";
		array[26] = RandomMapConfig.rotation.ToString();
		array[27] = "|";
		array[28] = RandomMapConfig.pickedLineIndex.ToString();
		array[29] = "|";
		array[30] = RandomMapConfig.pickedLevelIndex.ToString();
		array[31] = "|";
		array[32] = staticSeed.ToString();
		string strSeed = string.Concat(array);
		string cacheFilePathName = VFDataRTGenFileCache.GetCacheFilePathName(strSeed);
		_tileFileCache = new VFDataRTGenFileCache(cacheFilePathName + ".terra");
		_curTile = new VFTile(0, s_noiseHeight);
		_lodTile = new VFTile(1, s_noiseHeight);
		int num = (s_noiseHeight + 1) * 2;
		_tmpBridgeVoxels = new byte[num];
		_tmpInterTerraVoxels1 = new byte[num];
		_tmpInterTerraVoxels2 = new byte[num];
		_tmpInterWaterVoxels1 = new byte[num];
		_tmpInterWaterVoxels2 = new byte[num];
		if (myNoise == null)
		{
			InitStaticParam(seed);
		}
		s_dicTreeInfoList.Clear();
		s_dicGrassInstList.Clear();
	}

	public static float PlainMax(float plainThickness)
	{
		return (!((float)s_seaDepth + plainThickness > (float)(s_noiseHeight - 1))) ? ((float)s_seaDepth + plainThickness) : ((float)(s_noiseHeight - 1));
	}

	public float SpawnHeight(float PlainThickness)
	{
		return (!(PlainMax(PlainThickness) + 20f > (float)(s_noiseHeight - 1))) ? (PlainMax(PlainThickness) + 20f) : ((float)(s_noiseHeight - 1));
	}

	public static void SetMapTypeFrequency(float scale)
	{
		mapTypeFrequencyX = mapTypeFrequency0 * scale;
		mapTypeFrequencyZ = mapTypeFrequency0 * scale;
	}

	public static void SetTerrainFrequency(float scale)
	{
		terrainFrequencyX = terrainFrequency0 * scale;
		terrainFrequencyZ = terrainFrequency0 * scale;
	}

	public static void SetPlainThickness(float plainHeight)
	{
		int num = ((s_noiseHeight < 256) ? 100 : ((s_noiseHeight >= 512) ? 256 : 200));
		HASMid = 5 + Mathf.RoundToInt((float)(num - 5) * (plainHeight - 1f) / 99f);
	}

	public static void SetIslandSize(int scale)
	{
		float num = (float)scale / 100f;
		islandFactor = num * 40f + 60f;
	}

	public static void SetFlatness(float flatness)
	{
		flatMid = GetSlideBarValue(101f - flatness, 0.05f, 0.5f, 3f);
	}

	public static void SetFlatMin(float value)
	{
		flatMin = (flatMid - 0.03f) * value + 0.03f;
	}

	public static void SetFlatMax(float value)
	{
		flatMax = (3.5f - flatMid) * value + flatMid;
	}

	public static void SetRiverDensity(int riverDensity)
	{
		if (riverDensity < densityMin)
		{
			riverDensity = densityMin;
		}
		if (riverDensity > densityMax)
		{
			riverDensity = densityMax;
		}
		riverFrequencyZ = (riverFrequencyX = (riverFrequencyNow = GetSlideBarValue(riverDensity, riverFrequency1, riverFrequency100)));
	}

	public static void SetRiverWidth(int riverWidth)
	{
		if (riverWidth < widthMin)
		{
			riverWidth = widthMin;
		}
		if (riverWidth > widthMax)
		{
			riverWidth = widthMax;
		}
		riverWidthNow = riverWidth1 + (riverWidth100 - riverWidth1) * (float)(riverWidth - widthMin) / (float)(widthMax - widthMin);
		riverThreshold = riverWidthNow * riverFrequencyX;
		riverBottomPercentNow = riverBottomPercent1 + (riverBottomPercent100 - riverBottomPercent1) * (riverWidthNow - riverWidth1) / (riverWidth100 - riverWidth1);
		float num = (bridgeFrequencyZ = (bridgeFrequencyX = bridgeFrequency1 + (bridgeFrequency100 - bridgeFrequency1) * (float)(riverWidth - widthMin) / (float)(widthMax - widthMin)));
		bridgeThreshold = bridgeThreshold1 + (bridgeThreshold100 - bridgeThreshold1) * (float)(riverWidth - widthMin) / (float)(widthMax - widthMin);
		if (Application.isEditor)
		{
			Debug.Log("<color=red>" + riverWidthNow + "</color>");
			Debug.Log("<color=red>" + riverThreshold + "</color>");
			Debug.Log("<color=red>riverBottomPercentNow:" + riverBottomPercentNow + "</color>");
			Debug.Log("<color=red>bridgeFrequencyNow:" + num + "</color>");
		}
	}

	public static void SetBridgeMaxHeight(int bridgeValue)
	{
		bridgeMaxHeight = (float)bridgeValue / 100f;
	}

	private static int rockyStart0(float PlainThickness)
	{
		return Mathf.FloorToInt(PlainMax(PlainThickness) - 20f);
	}

	private static int rockyStart1(float PlainThickness)
	{
		return Mathf.FloorToInt(PlainMax(PlainThickness));
	}

	private float CaveHeightMax(float PlainThickness)
	{
		return PlainMax(PlainThickness);
	}

	private float CaveHillHeightMin(int PlainThickness)
	{
		return PlainMax(PlainThickness) - 20f;
	}

	private float MineMaxHeight(float PlainThickness)
	{
		return PlainMax(PlainThickness);
	}

	private static int PlainTopHeight(float PlainThickness)
	{
		return Mathf.CeilToInt((!(PlainMax(PlainThickness) + 64f > (float)(s_noiseHeight - 1))) ? (PlainMax(PlainThickness) + 64f) : ((float)(s_noiseHeight - 1)));
	}

	private static int HillBottomHeight(float PlainThickness)
	{
		return Mathf.CeilToInt((!(PlainMax(PlainThickness) + 128f > (float)(s_noiseHeight - 1))) ? (PlainMax(PlainThickness) + 128f) : ((float)(s_noiseHeight - 1)));
	}

	private static int HillTopHeight(float PlainThickness)
	{
		return Mathf.CeilToInt((!(PlainMax(PlainThickness) + 256f > (float)(s_noiseHeight - 1))) ? (PlainMax(PlainThickness) + 256f) : ((float)(s_noiseHeight - 1)));
	}

	public static void InitStaticParam(int seed)
	{
		staticSeed = seed;
		NoiseSeedsPlus[0] = (byte)RandomMapConfig.RandomMapID;
		int num = NoiseSeedsPlus.Length;
		myNoise = new SimplexNoise[num];
		for (int i = 0; i < num; i++)
		{
			myNoise[i] = new SimplexNoise(staticSeed + NoiseSeedsPlus[i]);
		}
		int num2 = CaveSeedsPlus.Length;
		myCaveNoise = new SimplexNoise[num2];
		for (int j = 0; j < num2; j++)
		{
			myCaveNoise[j] = new SimplexNoise(staticSeed + CaveSeedsPlus[j]);
		}
		int num3 = MineSeedsPlus.Length;
		myMineNoise = new SimplexNoise[num3];
		for (int k = 0; k < num3; k++)
		{
			myMineNoise[k] = new SimplexNoise(staticSeed + MineSeedsPlus[k]);
		}
		if (s_noiseHeight == 512)
		{
			MineStartHeightList = new int[12]
			{
				130, 130, 120, 110, 100, 90, 80, 70, 70, 60,
				50, 40
			};
			HeightOffsetTer = 64;
		}
		else
		{
			MineStartHeightList = new int[12]
			{
				28, 28, 25, 25, 20, 18, 18, 15, 15, 11,
				8, 6
			};
			HeightOffsetTer = 0;
		}
		int num4 = RiverSeedsPlus.Length;
		myRiverNoise = new SimplexNoise[num4];
		for (int l = 0; l < num4; l++)
		{
			myRiverNoise[l] = new SimplexNoise(staticSeed + RiverSeedsPlus[l]);
		}
		int num5 = BiomaSeedsPlus.Length;
		myBiomaNoise = new SimplexNoise[num5];
		for (int m = 0; m < num5; m++)
		{
			myBiomaNoise[m] = new SimplexNoise(staticSeed + BiomaSeedsPlus[m]);
		}
		isTownTile = false;
		dTileNoiseBuf = new double[35][];
		fTileHeightBuf = new float[35][];
		fTileGradTanBuf = new float[35][];
		tileMapType = new RandomMapType[35][];
		for (int n = 0; n < 35; n++)
		{
			dTileNoiseBuf[n] = new double[35];
			fTileHeightBuf[n] = new float[35];
			fTileGradTanBuf[n] = new float[35];
			tileMapType[n] = new RandomMapType[35];
		}
		InitPlanetParam();
		InitDensityClamp();
		InitNoiseHeight();
		LoadPaintConfig();
	}

	public static void InitPlanetParam()
	{
		System.Random random = new System.Random(RandomMapConfig.MineGenSeed);
		mineGenChanceFactor = (float)random.NextDouble() * 1.2f + 1f;
	}

	public static void LoadPaintConfig()
	{
		BiomaDistList = VATownGenerator.Instance.InitBiomaPos();
		noTownStartPos = BiomaDistList[(int)(RandomMapConfig.RandomMapID - 1)].posList[0];
		paintConfig = new VoxelPaintXMLParser();
		paintConfig.LoadXMLInResources("RandomMapXML/tmpPaintVxMat", "RandomMapXML/", 79);
		regionArray = paintConfig.prms.RegionDescArrayValues;
		SetTerrainParam();
		lock (s_dicTreeInfoList)
		{
			s_dicTreeInfoList = new Dictionary<IntVector2, List<TreeInfo>>();
		}
		lock (s_dicGrassInstList)
		{
			s_dicGrassInstList = new Dictionary<IntVector2, List<VoxelGrassInstance>>();
		}
	}

	private static void InitDensityClamp()
	{
		fTerDensityClamp = new float[s_noiseHeight];
		fTerDensityClampBase = new float[s_noiseHeight];
		float topValue = 2.3f;
		float bottomValue = 0f;
		for (int i = 1; i < s_noiseHeight; i++)
		{
			if (i < s_seaDepth - 2)
			{
				fTerDensityClampBase[i] = 0f;
				continue;
			}
			float densityClampValue = GetDensityClampValue(s_noiseHeight - 1, s_seaDepth - 2, topValue, bottomValue, i);
			fTerDensityClampBase[i] = densityClampValue;
		}
	}

	private static float GetDensityClampValue(float top, float bottom, float topValue, float bottomValue, float vy)
	{
		float num = top - bottom;
		float num2 = (vy - bottom) / num;
		float f = 1f - num2;
		float num3 = Mathf.Asin(f);
		return ((float)Math.PI / 2f - num3) / ((float)Math.PI / 2f) * (topValue - bottomValue) + bottomValue;
	}

	private void ReGenDensityClamp(int x, int z)
	{
		PlainThickness = HASMid;
		float num = ((float)myNoise[0].Noise((float)x * s_detailScale * HASFilterFrequency, (float)z * s_detailScale * HASFilterFrequency) + 1f) * 0.5f;
		float num2 = ((float)myNoise[2].Noise((float)x * s_detailScale * HAS2FilterFrequency, (float)z * s_detailScale * HAS2FilterFrequency) + 1f) * 0.5f;
		HASEnd = PlainMax(PlainThickness);
		HASEnd2 = PlainMax(PlainThickness / 2f);
		fTerDensityClamp[0] = 0f;
		for (int i = 1; i < s_noiseHeight; i++)
		{
			if (i < s_seaDepth - 2)
			{
				fTerDensityClamp[i] = fTerDensityClampBase[i];
			}
			else if ((float)i <= HASEnd2)
			{
				float densityClampValue = GetDensityClampValue(HASEnd, s_seaDepth - 2, DensityClampPlainTopValue, 0f, i);
				float densityClampValue2 = GetDensityClampValue(HASEnd2, s_seaDepth - 2, DensityClampPlainTopValue, 0f, i);
				fTerDensityClamp[i] = densityClampValue * num2 + densityClampValue2 * (1f - num2);
			}
			else if ((float)i <= HASEnd)
			{
				float densityClampValue = GetDensityClampValue(HASEnd, s_seaDepth - 2, DensityClampPlainTopValue, 0f, i);
				float densityClampValue2 = GetDensityClampValue(s_noiseHeight - 1, HASEnd2, DensityClampAllTopValue, DensityClampPlainTopValue, i);
				fTerDensityClamp[i] = densityClampValue * num2 + densityClampValue2 * (1f - num2);
			}
			else if ((float)i > HASEnd)
			{
				float linerValue = GetLinerValue(i, HASEnd, s_noiseHeight - 1, DensityClampPlainTopValue, DensityClampLinerTopValue);
				float densityClampValue = GetDensityClampValue(s_noiseHeight - 1, HASEnd, DensityClampAllTopValue, DensityClampPlainTopValue, i);
				fTerDensityClamp[i] = linerValue * DensityClampLinerBlend + densityClampValue * (1f - DensityClampLinerBlend);
				float densityClampValue2 = GetDensityClampValue(s_noiseHeight - 1, HASEnd2, DensityClampAllTopValue, DensityClampPlainTopValue, i);
				fTerDensityClamp[i] = fTerDensityClamp[i] * num2 + densityClampValue2 * (1f - num2);
			}
			fTerDensityClamp[i] = fTerDensityClamp[i] * (1f - num * DensityClampBaseBlendFactor) + fTerDensityClampBase[i] * num * DensityClampBaseBlendFactor;
		}
	}

	private static float GetLinerValue(float vy, float bottomX, float topX, float bottomValue, float topValue)
	{
		float num = 0f;
		return bottomValue + (vy - bottomX) / (topX - bottomX) * (topValue - bottomValue);
	}

	private static void ReGenDensityClampStatic(IntVector2 worldXZ, out float[] fTerDensityClamp)
	{
		fTerDensityClamp = new float[s_noiseHeight];
		float num = ((float)myNoise[0].Noise((float)worldXZ.x * s_detailScale * HASFilterFrequency, (float)worldXZ.y * s_detailScale * HASFilterFrequency) + 1f) * 0.5f;
		float num2 = ((float)myNoise[2].Noise((float)worldXZ.x * s_detailScale * HAS2FilterFrequency, (float)worldXZ.y * s_detailScale * HAS2FilterFrequency) + 1f) * 0.5f;
		float num3 = PlainMax(HASMid);
		float num4 = PlainMax(HASMid / 2);
		for (int i = 1; i < s_noiseHeight; i++)
		{
			if (i < s_seaDepth - 2)
			{
				fTerDensityClamp[i] = fTerDensityClampBase[i];
			}
			else if ((float)i <= num4)
			{
				float densityClampValue = GetDensityClampValue(num3, s_seaDepth - 2, DensityClampPlainTopValue, 0f, i);
				float densityClampValue2 = GetDensityClampValue(num4, s_seaDepth - 2, DensityClampPlainTopValue, 0f, i);
				fTerDensityClamp[i] = densityClampValue * num2 + densityClampValue2 * (1f - num2);
			}
			else if ((float)i <= num3)
			{
				float densityClampValue = GetDensityClampValue(num3, s_seaDepth - 2, DensityClampPlainTopValue, 0f, i);
				float densityClampValue2 = GetDensityClampValue(s_noiseHeight - 1, num4, DensityClampAllTopValue, DensityClampPlainTopValue, i);
				fTerDensityClamp[i] = densityClampValue * num2 + densityClampValue2 * (1f - num2);
			}
			else if ((float)i > num3)
			{
				float linerValue = GetLinerValue(i, num3, s_noiseHeight - 1, DensityClampPlainTopValue, DensityClampLinerTopValue);
				float densityClampValue = GetDensityClampValue(s_noiseHeight - 1, num3, DensityClampAllTopValue, DensityClampPlainTopValue, i);
				fTerDensityClamp[i] = linerValue * DensityClampLinerBlend + densityClampValue * (1f - DensityClampLinerBlend);
				float densityClampValue2 = GetDensityClampValue(s_noiseHeight - 1, num4, DensityClampAllTopValue, DensityClampPlainTopValue, i);
				fTerDensityClamp[i] = fTerDensityClamp[i] * num2 + densityClampValue2 * (1f - num2);
			}
			fTerDensityClamp[i] = fTerDensityClamp[i] * (1f - num * DensityClampBaseBlendFactor) + fTerDensityClampBase[i] * num * DensityClampBaseBlendFactor;
		}
	}

	private static void InitNoiseHeight()
	{
		fTerNoiseHeight = new float[s_noiseHeight];
		float num = (float)(s_noiseHeight - 2) * HeightScale;
		for (int i = 1; i < s_noiseHeight; i++)
		{
			fTerNoiseHeight[i] = num;
			num -= ((i < HeightScalePivot) ? HeightScale : HeightScale_1);
		}
	}

	private void FillTileDataWithNoise(VFTile tile)
	{
		townFloorVoxelByte.Clear();
		bool flag = false;
		List<VArtifactUnit> list = new List<VArtifactUnit>();
		long ticks = DateTime.Now.Ticks;
		int tileX = tile.tileX;
		int tileZ = tile.tileZ;
		int tileL = tile.tileL;
		int[] array = S_VoxelIndex[tileL];
		_tmpTileIndex.x = tileX;
		_tmpTileIndex.y = tileZ;
		long ticks2 = DateTime.Now.Ticks;
		if (tileL <= 2 && VArtifactTownManager.Instance != null)
		{
			int num = 1 << tileL;
			for (int i = 0; i < num; i++)
			{
				_tmpVec2.x = tileX + i;
				for (int j = 0; j < num; j++)
				{
					_tmpVec2.y = tileZ + j;
					if (VArtifactTownManager.Instance.TileContainsTown(_tmpVec2) && VArtifactTownManager.Instance.GetTileTown(_tmpVec2) != null && !VArtifactTownManager.Instance.GetTileTown(_tmpVec2).isEmpty)
					{
						flag = true;
						list = VArtifactTownManager.Instance.OutputTownData(_tmpVec2);
						break;
					}
				}
				if (flag)
				{
					break;
				}
			}
		}
		long ticks3 = DateTime.Now.Ticks;
		isTownTile = false;
		float num2 = waterHeight - Mathf.Floor(waterHeight);
		int num3 = (int)Mathf.Floor(waterHeight + 0.5f);
		byte volWaterHeight = (byte)((!(num2 >= 0.5f)) ? (128f / (1f - num2)) : (255f - 127.5f / num2));
		int num4 = (tileX << 5) - 1;
		int num5 = (tileZ << 5) - 1;
		for (int k = 0; k < 35; k++)
		{
			int num6 = num5 + (array[k] & 0xFF) + (array[k] >> 8 << 5);
			float num7 = (float)num6 * s_detailScale;
			for (int l = 0; l < 35; l++)
			{
				if (k > 1 && k < 33 && l > 1 && l < 33)
				{
					if (k % 3 == 2)
					{
						if (l % 3 == 0)
						{
							l += 2;
						}
					}
					else
					{
						l = 33;
					}
				}
				byte[] array2 = tile.terraVoxels[k][l];
				byte[] array3 = tile.waterVoxels[k][l];
				Array.Clear(array2, 0, array2.Length);
				Array.Clear(array3, 0, array3.Length);
				int num8 = num4 + (array[l] & 0xFF) + (array[l] >> 8 << 5);
				IntVector2 intVector = new IntVector2(num8, num6);
				ReGenDensityClamp(num8, num6);
				float num9 = (float)num8 * s_detailScale;
				int tileH = tile.tileH;
				bool flag2 = false;
				if (flag && townAvailable && tileL <= 0)
				{
					_tmpTownIdList.Clear();
					_tmpTownIdList.Add(list[0].vat.townId);
					for (int m = 1; m < list.Count; m++)
					{
						if (!_tmpTownIdList.Contains(list[m].vat.townId))
						{
							_tmpTownIdList.Add(list[m].vat.townId);
						}
					}
					for (int n = 0; n < _tmpTownIdList.Count; n++)
					{
						VArtifactTownManager.Instance.RandomArtifactTown(_tmpTownIdList[n]);
					}
					for (int num10 = 0; num10 < list.Count; num10++)
					{
						if (!list[num10].isAddedToRender)
						{
							VArtifactTownManager.Instance.ArtifactAddToRender(list[num10], _tmpTileIndex);
						}
					}
				}
				bool caveEnable = true;
				bool riverArea = false;
				bool lakeArea = false;
				float fTerTypeBridge;
				float riverValue;
				float bridgeValue;
				float bridge2dFactor;
				RandomMapType mapType;
				float[] terTypeInc;
				float finalFterType = GetFinalFterType(intVector, out fTerTypeBridge, out caveEnable, out riverArea, out lakeArea, out riverValue, out bridgeValue, out bridge2dFactor, out mapType, out terTypeInc);
				tileMapType[k][l] = mapType;
				float fTerType = finalFterType;
				float num11 = finalFterType / riverValue;
				float fTerNoise = (fTerType * 2f - 1f) * 0.05f;
				GetFTerTypeAndZnTerType(ref fTerType, out var nTerType, terTypeInc);
				float num12 = fTerType;
				ComputeTerNoise(ref fTerNoise, nTerType, fTerType);
				float factor = (float)myNoise[9].Noise(num9 * flatFrequency, num7 * flatFrequency);
				float flatParamFromFactor = GetFlatParamFromFactor(factor, intVector.x, intVector.y, mapType);
				float num13 = s_seaDepth + 1;
				bool flag3 = IsHillTerrain(nTerType);
				if (!IsWaterTerrain(nTerType))
				{
					float num14;
					if (IsHillTerrain(nTerType))
					{
						num14 = s_noiseHeight;
						tileH = s_noiseHeight;
					}
					else if (!IsSeaSideTerrain(nTerType))
					{
						num14 = (float)PlainTopHeight(PlainThickness) + (finalFterType - terTypeInc[1]) / (terTypeInc[2] - terTypeInc[1]) * (float)(HillBottomHeight(PlainThickness) - PlainTopHeight(PlainThickness));
						tileH = Mathf.FloorToInt(num14);
					}
					else
					{
						float num15 = terTypeInc[0] / 16f;
						float num16 = terTypeInc[0] * 17f / 16f;
						float num17 = 1f;
						if (finalFterType < num16)
						{
							num17 = (Mathf.Sin(((finalFterType - num15) / (num16 - num15) - 0.5f) * (float)Math.PI) + 1f) / 2f;
						}
						num13 = (float)(s_seaDepth + 1) * num17;
						num14 = PlainTopHeight(PlainThickness);
						tileH = PlainTopHeight(PlainThickness);
					}
					OptimiseMaxHeight(ref tileH, num9, num7, flatParamFromFactor, fTerType, nTerType, fTerNoise, num13, fTerNoiseHeight, fTerDensityClamp, PlainThickness);
					int num18 = tileH - 1;
					int num19 = Mathf.FloorToInt(num13 + 1f);
					int num20 = 1;
					if (IsSeaSideTerrain(nTerType))
					{
						num20 = 2;
					}
					float fNoiseXZ = -10f;
					int num21 = num18;
					int num22 = num21 * 2;
					while ((float)num21 > num13)
					{
						if (GenTileVoxelOnly(num9, fTerNoiseHeight[num21], num7, flatParamFromFactor, fTerDensityClamp[num21], fTerType, nTerType, fTerNoise, ref array2[num22], ref array2[num22 + 1], PlainThickness))
						{
							num19 = num21;
							break;
						}
						num21 -= num20;
						num22 -= 2 * num20;
					}
					int num23 = num19 - 1;
					int num24 = num23 * 2;
					while (num23 > 0)
					{
						GenTileVoxelWithHeightMap(num9, fTerNoiseHeight[num23], num7, fTerNoise, ref fNoiseXZ, ref array2[num24], ref array2[num24 + 1], MountainTopCorrection, num23);
						num23--;
						num24 -= 2;
					}
					if (num20 > 1)
					{
						int num25 = num18 - 1;
						int num26 = num25 * 2;
						while (num25 >= num19)
						{
							float num27 = (int)array2[num26 + 2];
							float num28 = ((!((float)(num25 - 1) > num13)) ? 255f : ((float)(int)array2[num26 - 2]));
							array2[num26] = (byte)(num27 / 2f + num28 / 2f);
							if (array2[num26] > 0)
							{
								array2[num26 + 1] = 1;
							}
							else
							{
								array2[num26 + 1] = 0;
							}
							num25 -= num20;
							num26 -= 2 * num20;
						}
					}
					if (num14 - (float)tileH < 1f && array2[(tileH - 1) * 2] > 175 && array2[(tileH - 1) * 2 + 1] == 1)
					{
						array2[tileH * 2] = (byte)((num14 - (float)tileH) * 255f);
						array2[tileH * 2 + 1] = 1;
						tileH++;
					}
					if (num20 != 1 && num19 == Mathf.FloorToInt(num13) && num13 - (float)num19 > 0f && num19 > 0)
					{
						array2[num19 * 2] = (byte)((num13 - (float)num19) * 255f);
						array2[num19 * 2 + 1] = 1;
						array2[(num19 - 1) * 2] = byte.MaxValue;
					}
				}
				else
				{
					float num29 = terTypeInc[0] / 16f;
					float num30 = terTypeInc[0] * 17f / 16f;
					float num31;
					if (finalFterType < num29)
					{
						num31 = 0f;
					}
					else
					{
						float num32 = (Mathf.Sin(((finalFterType - num29) / (num30 - num29) - 0.5f) * (float)Math.PI) + 1f) / 2f;
						num31 = num32 * (float)(s_seaDepth + 1);
					}
					int num33 = Mathf.FloorToInt(num31);
					tileH = num33 + 1;
					byte b = (array2[(num33 + 1) * 2] = (byte)Mathf.RoundToInt((num31 - (float)num33) * 255f));
					array2[(num33 + 1) * 2 + 1] = 1;
					byte b2 = b;
					int num34 = num33;
					int num35 = num34 * 2;
					while (num34 > 0)
					{
						array2[num35] = byte.MaxValue;
						array2[num35 + 1] = 1;
						num34--;
						num35 -= 2;
					}
				}
				if (true && fTerTypeBridge != -1f)
				{
					Array.Clear(_tmpBridgeVoxels, 0, _tmpBridgeVoxels.Length);
					fTerType = fTerTypeBridge;
					if (fTerType < 0f)
					{
						fTerType = 0f;
					}
					fTerNoise = (fTerTypeBridge * 2f - 1f) * 0.05f;
					GetFTerTypeAndZnTerType(ref fTerType, out nTerType, terTypeInc);
					ComputeTerNoise(ref fTerNoise, nTerType, fTerType);
					if (!IsWaterTerrain(nTerType))
					{
						float num14;
						if (IsHillTerrain(nTerType))
						{
							num14 = s_noiseHeight;
							tileH = s_noiseHeight;
						}
						else if (!IsSeaSideTerrain(nTerType))
						{
							num14 = (float)PlainTopHeight(PlainThickness) + (fTerTypeBridge - terTypeInc[1]) / (terTypeInc[2] - terTypeInc[1]) * (float)(HillBottomHeight(PlainThickness) - PlainTopHeight(PlainThickness));
							tileH = Mathf.FloorToInt(num14);
						}
						else
						{
							float num36 = terTypeInc[0] / 16f;
							float num37 = terTypeInc[0] * 17f / 16f;
							float num38 = 1f;
							if (finalFterType < num37)
							{
								num38 = (Mathf.Sin(((finalFterType - num36) / (num37 - num36) - 0.5f) * (float)Math.PI) + 1f) / 2f;
							}
							num13 = (float)(s_seaDepth + 1) * num38;
							num14 = PlainTopHeight(PlainThickness);
							tileH = PlainTopHeight(PlainThickness);
						}
						OptimiseMaxHeight(ref tileH, num9, num7, flatParamFromFactor, fTerType, nTerType, fTerNoise, num13, fTerNoiseHeight, fTerDensityClamp, PlainThickness);
						int num39 = tileH - 1;
						int num40 = Mathf.FloorToInt(num13 + 1f);
						int num41 = 1;
						if (IsSeaSideTerrain(nTerType))
						{
							num41 = 2;
						}
						float fNoiseXZ2 = -10f;
						int num42 = num39;
						int num43 = num42 * 2;
						while ((float)num42 > num13)
						{
							if (GenTileVoxelOnly(num9, fTerNoiseHeight[num42], num7, flatParamFromFactor, fTerDensityClamp[num42], fTerType, nTerType, fTerNoise, ref _tmpBridgeVoxels[num43], ref _tmpBridgeVoxels[num43 + 1], PlainThickness))
							{
								num40 = num42;
								break;
							}
							num42 -= num41;
							num43 -= 2 * num41;
						}
						int num44 = num40 - 1;
						int num45 = num44 * 2;
						while (num44 > 0)
						{
							GenTileVoxelWithHeightMap(num9, fTerNoiseHeight[num44], num7, fTerNoise, ref fNoiseXZ2, ref _tmpBridgeVoxels[num45], ref _tmpBridgeVoxels[num45 + 1], MountainTopCorrection, num44);
							num44--;
							num45 -= 2;
						}
						if (num41 > 1)
						{
							int num46 = num39 - 1;
							int num47 = num46 * 2;
							while (num46 >= num40)
							{
								float num48 = (int)_tmpBridgeVoxels[num47 + 2];
								float num49 = ((!((float)(num46 - 1) > num13)) ? 255f : ((float)(int)_tmpBridgeVoxels[num47 - 2]));
								_tmpBridgeVoxels[num47] = (byte)(num48 / 2f + num49 / 2f);
								if (_tmpBridgeVoxels[num47] > 0)
								{
									_tmpBridgeVoxels[num47 + 1] = 1;
								}
								else
								{
									_tmpBridgeVoxels[num47 + 1] = 0;
								}
								num46 -= num41;
								num47 -= 2 * num41;
							}
						}
						if (num14 - (float)tileH < 1f && _tmpBridgeVoxels[(tileH - 1) * 2] > 175 && _tmpBridgeVoxels[(tileH - 1) * 2 + 1] == 1)
						{
							_tmpBridgeVoxels[tileH * 2] = (byte)((num14 - (float)tileH) * 255f);
							_tmpBridgeVoxels[tileH * 2 + 1] = 1;
							tileH++;
						}
						if (num41 != 1 && num40 == Mathf.FloorToInt(num13) && num13 - (float)num40 > 0f && num40 > 0)
						{
							_tmpBridgeVoxels[num40 * 2] = (byte)((num13 - (float)num40) * 255f);
							_tmpBridgeVoxels[num40 * 2 + 1] = 1;
							_tmpBridgeVoxels[(num40 - 1) * 2] = byte.MaxValue;
						}
					}
					else
					{
						float num50 = terTypeInc[0] / 16f;
						float num51 = terTypeInc[0] * 17f / 16f;
						float num52;
						if (fTerTypeBridge < num50)
						{
							num52 = 0f;
						}
						else
						{
							float num53 = (Mathf.Sin(((fTerTypeBridge - num50) / (num51 - num50) - 0.5f) * (float)Math.PI) + 1f) / 2f;
							num52 = num53 * (float)(s_seaDepth + 1);
						}
						int num54 = Mathf.FloorToInt(num52);
						tileH = num54 + 1;
						byte b3 = (byte)Mathf.RoundToInt((num52 - (float)num54) * 255f);
						_tmpBridgeVoxels[(num54 + 1) * 2] = b3;
						_tmpBridgeVoxels[(num54 + 1) * 2 + 1] = 1;
						byte b4 = b3;
						int num55 = num54;
						int num56 = num55 * 2;
						while (num55 > 0)
						{
							_tmpBridgeVoxels[num56] = byte.MaxValue;
							_tmpBridgeVoxels[num56 + 1] = 1;
							num55--;
							num56 -= 2;
						}
					}
					float f = 1f - bridge2dFactor;
					int num57 = 40;
					int num58 = ((fTerTypeBridge <= terTypeInc[1]) ? Mathf.RoundToInt((float)num57 * riverValue * riverValue * (1f - num11 * 0.5f) * Mathf.Pow(f, 0.5f)) : ((!(fTerTypeBridge <= terTypeInc[2])) ? Mathf.RoundToInt((float)num57 * riverValue * riverValue * (1f - num11 * 0.5f) * Mathf.Pow(f, 0.5f)) : Mathf.RoundToInt((float)num57 * riverValue * riverValue * (1f - num11 * 0.5f) * Mathf.Pow(f, 0.5f))));
					int num59 = 0;
					if (num58 >= 0)
					{
						for (int num60 = tileH; num60 > 0; num60--)
						{
							int num61 = num60 * 2;
							if ((double)(int)_tmpBridgeVoxels[num61] > 127.5)
							{
								num59 = num60;
								break;
							}
						}
						int num62 = 0;
						int num63 = Math.Min(tileH, array2.Length >> 1) - 1;
						int num64 = num63;
						int num65 = num64 * 2;
						while (num64 > 0)
						{
							if ((double)(int)array2[num65] > 127.5)
							{
								num62 = num64;
								break;
							}
							num64--;
							num65 -= 2;
						}
						float num66 = ((!(num11 > terTypeInc[1])) ? (PlainThickness / 2f * (num11 - terTypeInc[0]) / (terTypeInc[1] - terTypeInc[0]) + (float)s_seaDepth) : PlainMax(PlainThickness / 2f));
						if (num59 != 0)
						{
							int num67 = num63;
							int num68 = num67 * 2;
							while ((float)num67 > num66 - (float)num58)
							{
								if (num67 >= 0)
								{
									array2[num68] = _tmpBridgeVoxels[num68];
									array2[num68 + 1] = _tmpBridgeVoxels[num68 + 1];
								}
								num67--;
								num68 -= 2;
							}
						}
					}
				}
				array2[0] = SolidBottomVoxel[0];
				array2[1] = SolidBottomVoxel[1];
				FillWaterVoxels(array3, num3, array2, tileH, volWaterHeight, fTerType, riverArea, lakeArea);
				if (caveEnable)
				{
					float f2 = (float)myCaveNoise[0].Noise(num9 * 0.125f, num7 * 0.125f);
					float num69 = Mathf.Abs(f2);
					float num70 = (float)myCaveNoise[3].Noise(num9 * 0.125f * 3f, num7 * 0.125f * 3f);
					float num71 = (num70 + 1f) * 0.75f;
					float num72 = (0.05f + 0.2f * (1.15f - num12)) * num71;
					if (num69 < num72)
					{
						float num73 = (float)myCaveNoise[1].Noise(num9 * 0.125f, num7 * 0.125f);
						float num74 = CaveHeightMax(PlainThickness) + 80f * num12;
						float caveHeight = (num73 + 1f) * 0.5f * (num74 - 5f) + 5f;
						float factor2 = (float)myCaveNoise[2].Noise(num9 * 1f, num7 * 1f);
						float num75 = 30f + 20f * (1.15f - num12);
						float num76 = GetThicknessParamFromFactor(factor2) * num75;
						float num77 = (num72 - num69) / 0.05f;
						float num78 = ((!(num77 > 0.3f)) ? Mathf.Pow(num77 / 0.3f, 0.5f) : 1f);
						num76 *= num78;
						if (num76 > 0f)
						{
							GenCave(caveHeight, num76, array2);
						}
					}
				}
				float f3 = (float)myMineNoise[0].Noise(num9 * 0.5f, num7 * 0.5f);
				float num79 = Mathf.Abs(f3);
				if (num79 > 0.5f)
				{
					float mineChanceFactor = (num79 - 0.5f) / 0.5f;
					GeneralMineral(array2, num9, fTerNoiseHeight, num7, mineChanceFactor, fTerType, mapType);
				}
				array2[0] = SolidBottomVoxel[0];
				array2[1] = SolidBottomVoxel[1];
				tile.nTerraYLens[k][l] = (tileH + 1) * 2;
				tile.nWaterYLens[k][l] = (num3 + 1) * 2;
			}
		}
		long ticks4 = DateTime.Now.Ticks;
		for (int num80 = 2; num80 < 33; num80 += 3)
		{
			int num81 = num5 + (array[num80] & 0xFF) + (array[num80] >> 8 << 5);
			float scaledZ = (float)num81 * s_detailScale;
			byte[][] array4 = tile.terraVoxels[num80];
			byte[][] array5 = tile.waterVoxels[num80];
			for (int num82 = 3; num82 < 33; num82 += 3)
			{
				int num83 = num4 + (array[num82] & 0xFF) + (array[num82] >> 8 << 5);
				float scaledX = (float)num83 * s_detailScale;
				byte[] array6 = array4[num82 - 1];
				byte[] array7 = array4[num82 + 2];
				int num84 = tile.nTerraYLens[num80][num82 - 1];
				int num85 = tile.nTerraYLens[num80][num82 + 2];
				int tileH = (tile.nTerraYLens[num80][num82] = (tile.nTerraYLens[num80][num82 + 1] = num84));
				if (num85 > tileH)
				{
					tileH = (tile.nTerraYLens[num80][num82] = (tile.nTerraYLens[num80][num82 + 1] = num85));
				}
				Array.Clear(_tmpInterTerraVoxels1, 0, _tmpInterTerraVoxels1.Length);
				Array.Clear(_tmpInterTerraVoxels2, 0, _tmpInterTerraVoxels2.Length);
				for (int num86 = 0; num86 < tileH; num86 += 2)
				{
					int num87 = ((num84 > num86) ? array6[num86] : 0);
					int num88 = ((num85 > num86) ? array7[num86] : 0);
					_tmpInterTerraVoxels2[num86] = (byte)((num87 * 2 + num88 * 4) / 6);
					_tmpInterTerraVoxels1[num86] = (byte)((num87 * 4 + num88 * 2) / 6);
					byte b5 = (byte)((num87 != 0) ? array6[num86 + 1] : 0);
					_tmpInterTerraVoxels1[num86 + 1] = b5;
					_tmpInterTerraVoxels2[num86 + 1] = b5;
				}
				Array.Copy(_tmpInterTerraVoxels1, tile.terraVoxels[num80][num82], _tmpInterTerraVoxels1.Length);
				Array.Copy(_tmpInterTerraVoxels2, tile.terraVoxels[num80][num82 + 1], _tmpInterTerraVoxels2.Length);
				byte[] array8 = array5[num82 - 1];
				byte[] array9 = array5[num82 + 2];
				int num89 = tile.nWaterYLens[num80][num82 - 1];
				int num90 = tile.nWaterYLens[num80][num82 + 2];
				int num91 = (tile.nWaterYLens[num80][num82] = (tile.nWaterYLens[num80][num82 + 1] = num89));
				if (num90 > num91)
				{
					num91 = (tile.nWaterYLens[num80][num82] = (tile.nWaterYLens[num80][num82 + 1] = num90));
				}
				Array.Clear(_tmpInterWaterVoxels1, 0, _tmpInterWaterVoxels1.Length);
				Array.Clear(_tmpInterWaterVoxels2, 0, _tmpInterWaterVoxels2.Length);
				for (int num92 = 0; num92 < num91; num92 += 2)
				{
					int num87 = ((num89 > num92) ? array8[num92] : 0);
					int num88 = ((num90 > num92) ? array9[num92] : 0);
					_tmpInterWaterVoxels2[num92] = (byte)((num87 * 2 + num88 * 4) / 6);
					_tmpInterWaterVoxels1[num92] = (byte)((num87 * 4 + num88 * 2) / 6);
					byte b5 = (byte)((num87 != 0) ? array8[num92 + 1] : 0);
					_tmpInterWaterVoxels1[num92 + 1] = b5;
					_tmpInterWaterVoxels2[num92 + 1] = b5;
				}
				Array.Copy(_tmpInterWaterVoxels1, tile.waterVoxels[num80][num82], _tmpInterWaterVoxels1.Length);
				Array.Copy(_tmpInterWaterVoxels2, tile.waterVoxels[num80][num82 + 1], _tmpInterWaterVoxels2.Length);
				RandomMapType mapType = GetMapType(scaledX, scaledZ);
				tileMapType[num80][num82] = mapType;
				tileMapType[num80][num82 + 1] = mapType;
			}
		}
		for (int num93 = 3; num93 < 33; num93 += 3)
		{
			int num94 = num5 + (array[num93] & 0xFF) + (array[num93] >> 8 << 5);
			float scaledZ2 = (float)num94 * s_detailScale;
			byte[][] array10 = tile.terraVoxels[num93 - 1];
			byte[][] array11 = tile.terraVoxels[num93 + 2];
			byte[][] array12 = tile.waterVoxels[num93 - 1];
			byte[][] array13 = tile.waterVoxels[num93 + 2];
			for (int num95 = 2; num95 < 33; num95++)
			{
				int num96 = num4 + (array[num95] & 0xFF) + (array[num95] >> 8 << 5);
				float scaledX2 = (float)num96 * s_detailScale;
				byte[] array14 = array10[num95];
				byte[] array15 = array11[num95];
				int num97 = tile.nTerraYLens[num93 - 1][num95];
				int num98 = tile.nTerraYLens[num93 + 2][num95];
				int tileH = (tile.nTerraYLens[num93][num95] = (tile.nTerraYLens[num93 + 1][num95] = num97));
				if (num98 > tileH)
				{
					tileH = (tile.nTerraYLens[num93][num95] = (tile.nTerraYLens[num93 + 1][num95] = num98));
				}
				Array.Clear(_tmpInterTerraVoxels1, 0, _tmpInterTerraVoxels1.Length);
				Array.Clear(_tmpInterTerraVoxels2, 0, _tmpInterTerraVoxels2.Length);
				for (int num99 = 0; num99 < tileH; num99 += 2)
				{
					int num100 = ((num97 > num99) ? array14[num99] : 0);
					int num101 = ((num98 > num99) ? array15[num99] : 0);
					_tmpInterTerraVoxels1[num99] = (byte)((num100 * 2 + num101) / 3);
					_tmpInterTerraVoxels2[num99] = (byte)((num100 + num101 * 2) / 3);
					byte b6 = (byte)((num100 != 0) ? array14[num99 + 1] : 0);
					_tmpInterTerraVoxels1[num99 + 1] = b6;
					_tmpInterTerraVoxels2[num99 + 1] = b6;
				}
				Array.Copy(_tmpInterTerraVoxels1, tile.terraVoxels[num93][num95], _tmpInterTerraVoxels1.Length);
				Array.Copy(_tmpInterTerraVoxels2, tile.terraVoxels[num93 + 1][num95], _tmpInterTerraVoxels2.Length);
				byte[] array16 = array12[num95];
				byte[] array17 = array13[num95];
				int num102 = tile.nWaterYLens[num93 - 1][num95];
				int num103 = tile.nWaterYLens[num93 + 2][num95];
				int num91 = (tile.nWaterYLens[num93][num95] = (tile.nWaterYLens[num93 + 1][num95] = num102));
				if (num103 > num91)
				{
					num91 = (tile.nWaterYLens[num93][num95] = (tile.nWaterYLens[num93 + 1][num95] = num103));
				}
				Array.Clear(_tmpInterWaterVoxels1, 0, _tmpInterWaterVoxels1.Length);
				Array.Clear(_tmpInterWaterVoxels2, 0, _tmpInterWaterVoxels2.Length);
				for (int num104 = 0; num104 < num91; num104 += 2)
				{
					int num100 = ((num102 > num104) ? array16[num104] : 0);
					int num101 = ((num103 > num104) ? array17[num104] : 0);
					_tmpInterWaterVoxels1[num104] = (byte)((num100 * 2 + num101) / 3);
					_tmpInterWaterVoxels2[num104] = (byte)((num100 + num101 * 2) / 3);
					byte b6 = (byte)((num100 != 0) ? array16[num104 + 1] : 0);
					_tmpInterWaterVoxels1[num104 + 1] = b6;
					_tmpInterWaterVoxels2[num104 + 1] = b6;
				}
				Array.Copy(_tmpInterWaterVoxels1, tile.waterVoxels[num93][num95], _tmpInterWaterVoxels1.Length);
				Array.Copy(_tmpInterWaterVoxels2, tile.waterVoxels[num93 + 1][num95], _tmpInterWaterVoxels2.Length);
				RandomMapType mapType = GetMapType(scaledX2, scaledZ2);
				tileMapType[num93][num95] = mapType;
				tileMapType[num93 + 1][num95] = mapType;
			}
		}
		long ticks5 = DateTime.Now.Ticks;
		long ticks6 = DateTime.Now.Ticks;
		if (flag)
		{
			for (int num105 = 0; num105 < 35; num105++)
			{
				int num106 = num5 + (array[num105] & 0xFF) + (array[num105] >> 8 << 5);
				for (int num107 = 0; num107 < 35; num107++)
				{
					int num108 = num4 + (array[num107] & 0xFF) + (array[num107] >> 8 << 5);
					_tmpVec2.x = num108 >> 5;
					_tmpVec2.y = num106 >> 5;
					list = VArtifactTownManager.Instance.OutputTownData(_tmpVec2);
					if (list == null || list.Count == 0)
					{
						continue;
					}
					byte[] array2 = tile.terraVoxels[num105][num107];
					byte[] array3 = tile.waterVoxels[num105][num107];
					RandomMapType mapType = tileMapType[num105][num107];
					_tmpVec3.x = num108;
					_tmpVec3.z = num106;
					_tmpNewTowns.Clear();
					for (int num109 = 0; num109 < list.Count; num109++)
					{
						if (list[num109].IsInTown(num108, num106))
						{
							_tmpNewTowns.Add(list[num109]);
						}
					}
					int num110 = tile.nTerraYLens[num105][num107];
					for (int num111 = 0; num111 < _tmpNewTowns.Count; num111++)
					{
						VArtifactUnit vArtifactUnit = _tmpNewTowns[num111];
						Dictionary<IntVector3, VFVoxel> townVoxel = vArtifactUnit.townVoxel;
						int num112 = Mathf.FloorToInt(vArtifactUnit.worldPos.y + 10f);
						int num113 = Mathf.CeilToInt(vArtifactUnit.worldPos.y + (float)vArtifactUnit.vaSize.z);
						if ((num113 + 1) * 2 > num110)
						{
							num110 = (tile.nTerraYLens[num105][num107] = (num113 + 1) * 2);
						}
						bool flag4 = false;
						bool flag5 = false;
						int num114 = s_noiseHeight - 1;
						int num115 = num113 - 1;
						int num116 = num115 * 2;
						while (num115 >= 0)
						{
							_tmpVec3.y = num115;
							if (townVoxel.ContainsKey(_tmpVec3))
							{
								if (!flag5)
								{
									Array.Clear(array2, num112 * 2, array2.Length - num112 * 2);
									flag5 = true;
								}
								flag4 = true;
								num114 = num115;
								VFVoxel vFVoxel = townVoxel[_tmpVec3];
								if (array2[num116] + vFVoxel.Volume > 255)
								{
									array2[num116] = byte.MaxValue;
								}
								else
								{
									array2[num116] += vFVoxel.Volume;
								}
								byte b7;
								if (vFVoxel.Type < VArtifactUtil.isos[_tmpNewTowns[num111].isoGuId].m_Materials.Length)
								{
									b7 = (byte)VArtifactUtil.isos[_tmpNewTowns[num111].isoGuId].m_Materials[vFVoxel.Type].m_Guid;
									if (b7 >= 240)
									{
										int num117 = b7 % 240;
										string[] array18 = VArtifactUtil.triplaner[(int)(mapType - 1)].Split(',');
										b7 = Convert.ToByte(array18[num117]);
									}
								}
								else
								{
									b7 = vFVoxel.Type;
								}
								townFloorVoxelByte[new IntVector3(num107, num115, num105)] = b7;
							}
							num115--;
							num116 -= 2;
						}
						if (!flag4)
						{
							continue;
						}
						int num118 = num114;
						int num119 = num118 * 2;
						while (num118 >= 0)
						{
							if (array2[num119] != byte.MaxValue)
							{
								array2[num119] = byte.MaxValue;
								if (array2[num119 + 1] == 0)
								{
									array2[num119 + 1] = 1;
								}
							}
							num118--;
							num119 -= 2;
						}
						int num120 = num113 - 1;
						int num121 = num120 * 2;
						while (num120 >= 0)
						{
							if (num3 + 1 > num120)
							{
								array3[num121] = (byte)(255 - array2[num121]);
								if (array3[num121] < 128)
								{
									array3[num121 + 1] = 0;
								}
							}
							num120--;
							num121 -= 2;
						}
					}
				}
			}
		}
		long ticks7 = DateTime.Now.Ticks;
		SetVoxelType(tile, flag, tileL);
		long ticks8 = DateTime.Now.Ticks;
		if (!flag)
		{
			return;
		}
		foreach (KeyValuePair<IntVector3, byte> item in townFloorVoxelByte)
		{
			IntVector3 key = item.Key;
			byte value = item.Value;
			tile.terraVoxels[key.z][key.x][key.y * 2 + 1] = item.Value;
		}
	}

	private static float GetContinentValue(int worldx, int worldz)
	{
		float num = RandomMapConfig.Instance.BoudaryEdgeDistance - RandomMapConfig.Instance.boundOffset;
		float num2 = (float)RandomMapConfig.Instance.BoudaryEdgeDistance - RandomMapConfig.Instance.boundStart;
		float f = Mathf.Atan2(Mathf.Abs(worldz), Mathf.Abs(worldx));
		float num3 = Mathf.Sqrt(Mathf.Pow(worldx, 2f) + Mathf.Pow(worldz, 2f));
		float num4 = (float)(myRiverNoise[3].Noise((float)worldx / num3 * continentBoundFrequency, (float)worldz / num3 * continentBoundFrequency) + 1.0) * 0.5f;
		float num5 = RandomMapConfig.Instance.BoudaryEdgeDistance;
		float num6 = num - num4 * (float)RandomMapConfig.Instance.boundChange;
		float num7 = num2 - num4 * (float)RandomMapConfig.Instance.boundChange;
		if (Mathf.Abs(worldz) < Mathf.Abs(worldx))
		{
			num5 /= Mathf.Cos(f);
			num6 /= Mathf.Cos(f);
			num7 /= Mathf.Cos(f);
		}
		else
		{
			num5 /= Mathf.Sin(f);
			num6 /= Mathf.Sin(f);
			num7 /= Mathf.Sin(f);
		}
		if (num3 > num5)
		{
			return 2f;
		}
		if (num3 > num6)
		{
			return 1f + (num3 - num6) / (num5 - num6);
		}
		if (num3 < num7)
		{
			return 0f;
		}
		return (num3 - num7) / (num6 - num7);
	}

	private static float BlendContinentBound(float origin, float bound, float[] terTypeInc)
	{
		float num = -1f;
		float num2 = 0f;
		float num3 = 0f;
		float num4 = terTypeInc[0];
		float num5 = 0.4f;
		float num6 = terTypeInc[1];
		float num7 = 0.45f;
		float num8 = terTypeInc[2];
		float num9 = 1f;
		float num10 = 1f - bound;
		num10 = ((num10 > num7) ? ((num10 - num7) / (1f - num7) * (1f - num8) + num8) : ((num10 > num5) ? ((num10 - num5) / (num7 - num5) * (num8 - num6) + num6) : ((!(num10 > num3)) ? ((num10 - num) / (num3 - num) * (num4 - num2) + num2) : ((num10 - num3) / (num5 - num3) * (num6 - num4) + num4))));
		if (origin > num10)
		{
			return num10;
		}
		return origin;
	}

	private static void RoundToSquare(ref float maxDistance, ref float startDistance, float angle)
	{
		float num = Mathf.Abs(angle);
		if (num > 0f && num <= (float)Math.PI / 4f)
		{
			maxDistance /= Mathf.Cos(num);
			startDistance /= Mathf.Cos(num);
		}
		else if (num > (float)Math.PI / 4f && num < (float)Math.PI / 2f)
		{
			float f = (float)Math.PI / 2f - num;
			maxDistance /= Mathf.Cos(f);
			startDistance /= Mathf.Cos(f);
		}
		else if (num > (float)Math.PI / 2f && num <= (float)Math.PI * 3f / 4f)
		{
			float f2 = num - (float)Math.PI / 2f;
			maxDistance /= Mathf.Cos(f2);
			startDistance /= Mathf.Cos(f2);
		}
		else if (num > (float)Math.PI * 3f / 4f)
		{
			float f3 = (float)Math.PI - num;
			maxDistance /= Mathf.Cos(f3);
			startDistance /= Mathf.Cos(f3);
		}
	}

	private static float ResetLakeBottomWidth(float lakeThreshold, float fTerType)
	{
		return 1f - (1f - lakeThreshold) * lakeBankPercent;
	}

	private static float ResetLakeThreshold(float lakeThreshold, float fTerType, float scaledX, float scaledZ)
	{
		float num = 0f;
		float num2 = (float)myRiverNoise[7].Noise(scaledX * lakeChangeFrequency, scaledZ * lakeChangeFrequency);
		float num3 = (num2 + 1f) * 0.75f;
		return 1f - (1f - lakeThreshold) * num3;
	}

	private void FillWater(int vx, int vx2, int nWaterHeight, int fillWaterHeight, byte volWaterHeight, byte[][] yxVoxels, byte[][] yxWaterVoxels)
	{
		int num = fillWaterHeight;
		if (num > 0)
		{
			if (yxVoxels[num + 1][vx2] < 128 || yxVoxels[num][vx2] < 128)
			{
				byte[] array = yxWaterVoxels[num];
				array[vx2] = volWaterHeight;
				array[vx2 + 1] = 128;
				num--;
				array = yxWaterVoxels[num];
				array[vx2] = byte.MaxValue;
				array[vx2 + 1] = 128;
				num--;
			}
			else
			{
				byte[] array = yxWaterVoxels[num];
				array[vx2] = 0;
				array[vx2 + 1] = 0;
				num--;
			}
		}
		while (num > nWaterHeight - 2)
		{
			byte[] array = yxWaterVoxels[num];
			if (yxVoxels[num + 1][vx2] < 128 || yxVoxels[num][vx2] < 160)
			{
				array[vx2] = byte.MaxValue;
				array[vx2 + 1] = 128;
			}
			else
			{
				array[vx2] = 0;
				array[vx2 + 1] = 0;
			}
			num--;
		}
	}

	public void FillWaterVoxels(byte[] yWaterVoxels, int nWaterHeight, byte[] yTerraVoxels, int nTerraHeight, byte volWaterHeight, float fTerType, bool riverArea, bool lakeArea)
	{
		int num = yTerraVoxels.Length;
		int num2 = nWaterHeight * 2;
		int num3 = 15;
		int num4 = nTerraHeight;
		int num5 = num4;
		int num6 = num5 * 2;
		while (num5 > 0)
		{
			num4 = num5;
			if ((double)(int)yTerraVoxels[num6] > 127.5)
			{
				break;
			}
			num5--;
			num6 -= 2;
		}
		int num7 = nWaterHeight + 1;
		int num8 = num7;
		int num9 = num8 * 2;
		while (num8 > 0)
		{
			num7 = num8;
			if (num9 + 2 < num && yTerraVoxels[num9 + 2] > 128 && yTerraVoxels[num9] > 128)
			{
				break;
			}
			num8--;
			num9 -= 2;
		}
		if (num7 <= nWaterHeight)
		{
			yWaterVoxels[num2] = volWaterHeight;
			yWaterVoxels[num2 + 1] = 128;
			num2 -= 2;
			yWaterVoxels[num2] = byte.MaxValue;
			yWaterVoxels[num2 + 1] = 128;
			for (num2 -= 2; num2 > num7 * 2; num2 -= 2)
			{
				yWaterVoxels[num2] = byte.MaxValue;
				yWaterVoxels[num2 + 1] = 128;
			}
			int num10 = 0;
			while (num2 > 0 && num10 < num3)
			{
				yWaterVoxels[num2] = byte.MaxValue;
				yWaterVoxels[num2 + 1] = 128;
				num10++;
				num2 -= 2;
			}
		}
		else
		{
			yWaterVoxels[num2] = 0;
			yWaterVoxels[num2 + 1] = 0;
			num2 -= 2;
		}
		if (lakeArea || riverArea)
		{
			while (num2 >= 0)
			{
				yWaterVoxels[num2] = byte.MaxValue;
				yWaterVoxels[num2 + 1] = 128;
				num2 -= 2;
			}
			return;
		}
		while (num2 >= 0)
		{
			if (num2 + 2 >= num || yTerraVoxels[num2 + 2] < 128 || (float)(int)yTerraVoxels[num2] < 160f)
			{
				yWaterVoxels[num2] = byte.MaxValue;
				yWaterVoxels[num2 + 1] = 128;
			}
			else
			{
				yWaterVoxels[num2] = 0;
				yWaterVoxels[num2 + 1] = 0;
			}
			num2 -= 2;
		}
	}

	private void PaintTile_Simple(VFTile terTile)
	{
		for (int i = 1; i < 34; i++)
		{
			byte[][] array = terTile.terraVoxels[i + 1];
			byte[][] array2 = terTile.terraVoxels[i - 1];
			byte[][] array3 = terTile.terraVoxels[i];
			for (int j = 1; j < terTile.tileH - 1; j++)
			{
				byte[] array4 = array[j];
				byte[] array5 = array2[j];
				byte[] array6 = array[j - 1];
				byte[] array7 = array2[j - 1];
				byte[] array8 = array[j + 1];
				byte[] array9 = array2[j + 1];
				byte[] array10 = array3[j - 1];
				byte[] array11 = array3[j + 1];
				byte[] array12 = array3[j];
				int num = 2;
				for (int k = 1; k < 34; k++)
				{
					if (array12[num] < 128 && array10[num] >= 128)
					{
						int val = ((array12[num + 2] == 0) ? (array12[num] + 255 - array10[num + 2]) : ((array12[num + 2] != byte.MaxValue) ? Math.Abs(array12[num + 2] - array12[num]) : (array11[num + 2] + 255 - array12[num])));
						int val2 = ((array12[num - 2] == 0) ? (array12[num] + 255 - array10[num - 2]) : ((array12[num - 2] != byte.MaxValue) ? Math.Abs(array12[num - 2] - array12[num]) : (array11[num - 2] + 255 - array12[num])));
						int val3 = ((array4[num] == 0) ? (array12[num] + 255 - array6[num]) : ((array4[num] != byte.MaxValue) ? Math.Abs(array4[num] - array12[num]) : (array8[num] + 255 - array12[num])));
						int num2 = Math.Max(val2: Math.Max(val3, (array5[num] == 0) ? (array12[num] + 255 - array7[num]) : ((array5[num] != byte.MaxValue) ? Math.Abs(array5[num] - array12[num]) : (array9[num] + 255 - array12[num]))), val1: Math.Max(val, val2));
						if (num2 < 40)
						{
							array10[num + 1] = 3;
						}
					}
					num += 2;
				}
			}
		}
	}

	private void SetVoxelType(VFTile tile, bool isTownTile, int lod = 0)
	{
		int[] array = S_VoxelIndex[lod];
		int num = 34;
		int num2 = (tile.tileX << 5) - 1;
		int num3 = (tile.tileZ << 5) - 1;
		for (int i = 0; i < num; i++)
		{
			int num4 = num3 + (array[i] & 0xFF) + (array[i] >> 8 << 5);
			double[] array2 = dTileNoiseBuf[i];
			float[] array3 = fTileHeightBuf[i];
			float[] array4 = fTileGradTanBuf[i];
			byte[][] array5 = tile.terraVoxels[i];
			for (int j = 0; j < num; j++)
			{
				int num5 = num2 + (array[j] & 0xFF) + (array[j] >> 8 << 5);
				int idxRegion = VoxelPaintXMLParser.MapTypeToRegionId(tileMapType[i][j]);
				bool flag = false;
				double noise = (array2[j] = paintConfig.GetNoise(num5, num4));
				array4[j] = 0f;
				array3[j] = 0f;
				byte[] array6 = array5[j];
				int num6 = tile.nTerraYLens[i][j];
				int num7 = num6 >> 1;
				int num8 = num6;
				while (num7 > 1)
				{
					int num9 = num8 - 2;
					byte b = array6[num8];
					byte b2 = array6[num9];
					if (b < 128 && b2 >= 128)
					{
						float num10 = b - b2;
						float num11 = (float)(128 - b2) / num10;
						if (i == 0 || j == 0)
						{
							if (!flag)
							{
								array3[j] = (float)(num7 - 1) + num11;
								flag = true;
							}
						}
						else
						{
							byte[][] array7 = tile.terraVoxels[i + 1];
							byte[][] array8 = tile.terraVoxels[i - 1];
							byte[] array9 = array7[j];
							byte[] array10 = array8[j];
							_lrbfVol[0] = array5[j - 1][num8];
							_lrbfVol[1] = array5[j + 1][num8];
							_lrbfVol[2] = array9[num8];
							_lrbfVol[3] = array10[num8];
							_lrbfVolDn[0] = array5[j - 1][num9];
							_lrbfVolDn[1] = array5[j + 1][num9];
							_lrbfVolDn[2] = array9[num9];
							_lrbfVolDn[3] = array10[num9];
							float num12 = 0f;
							for (int k = 0; k < 4; k++)
							{
								float num13 = ((_lrbfVolDn[k] < 128) ? ((float)(_lrbfVolDn[k] - b2) / num10) : ((_lrbfVol[k] >= 128) ? ((1f - num11) * (float)(_lrbfVol[k] - b) / (128f - (float)(int)b)) : Math.Abs(num11 - (float)(128 - _lrbfVolDn[k]) / (float)(_lrbfVol[k] - _lrbfVolDn[k]))));
								if (num12 < num13)
								{
									num12 = num13;
								}
							}
							if (IsTownConnectionType(num5, num4))
							{
								array6[num8 - 2 + 1] = 71;
								if (num7 > 4)
								{
									for (int num14 = num7 - 1; num14 > num7 - 4; num14--)
									{
										array6[(num14 - 1 << 1) + 1] = 71;
									}
								}
								else
								{
									for (int num15 = num7 - 4; num15 > 1; num15--)
									{
										array6[(num15 - 1 << 1) + 1] = 71;
									}
								}
							}
							else
							{
								array6[num8 - 2 + 1] = paintConfig.GetVoxelType(num12, num7, noise, idxRegion);
								if (num7 > 8)
								{
									for (int num16 = num7 - 1; num16 > num7 - 8; num16--)
									{
										array6[(num16 - 1 << 1) + 1] = paintConfig.GetVoxelType(num12, num16, noise, idxRegion);
									}
								}
								else
								{
									for (int num17 = num7 - 1; num17 > 1; num17--)
									{
										array6[(num17 - 1 << 1) + 1] = paintConfig.GetVoxelType(num12, num17, noise, idxRegion);
									}
								}
							}
							if (!flag)
							{
								array4[j] = num12;
								array3[j] = (float)(num7 - 1) + num11;
								flag = true;
							}
						}
					}
					else if (i != 0 && j != 0 && b >= 128 && b2 < 128 && (array6[num8 + 2] >= 128 || (b >= 128 && b2 < 128 && num7 == tile.tileH - 1)))
					{
						array6[num8 + 1] = paintConfig.GetVoxelType(999f, num7, noise, idxRegion);
					}
					else if (i != 0 && j != 0 && b >= 128 && b2 >= 128 && (array6[num8 + 2] >= 128 || (b >= 128 && b2 < 128 && num7 == tile.tileH - 1)))
					{
						byte[][] array11 = tile.terraVoxels[i + 1];
						byte[][] array12 = tile.terraVoxels[i - 1];
						byte[] array13 = array11[j];
						byte[] array14 = array12[j];
						_lrbfVol[0] = array5[j - 1][num8];
						_lrbfVol[1] = array5[j + 1][num8];
						_lrbfVol[2] = array13[num8];
						_lrbfVol[3] = array14[num8];
						for (int l = 0; l < 4; l++)
						{
							if (_lrbfVol[l] < 128)
							{
								array6[num8 + 1] = paintConfig.GetVoxelType(999f, num7, noise, idxRegion);
								break;
							}
						}
					}
					num7--;
					num8 -= 2;
				}
			}
		}
	}

	private void GenTreesOnTile(VFTile terTile, int szCell)
	{
		if (SystemSettingData.Instance.RandomTerrainLevel > 0 && terTile.tileL != 1)
		{
			return;
		}
		if (terTile.tileL == 0)
		{
			if ((terTile.tileX & 1) != 0 || (terTile.tileZ & 1) != 0)
			{
				return;
			}
			FillTileData(terTile.tileX, terTile.tileZ, 1, _lodTile);
			terTile = _lodTile;
		}
		_tmpVec2.x = terTile.tileX;
		_tmpVec2.y = terTile.tileZ;
		lock (s_dicTreeInfoList)
		{
			if (s_dicTreeInfoList.ContainsKey(_tmpVec2))
			{
				return;
			}
		}
		paintConfig.RandSeed = 79;
		List<TreeInfo> list = new List<TreeInfo>();
		paintConfig.PlantTrees(terTile, dTileNoiseBuf, fTileHeightBuf, fTileGradTanBuf, list, tileMapType, szCell);
		int num = s_ofsToCheckPlants.Length;
		lock (s_dicTreeInfoList)
		{
			IntVector2 intVector = new IntVector2();
			for (int i = 0; i < num; i += 2)
			{
				intVector.x = terTile.tileX + (s_ofsToCheckPlants[i] << terTile.tileL);
				intVector.y = terTile.tileZ + (s_ofsToCheckPlants[i + 1] << terTile.tileL);
				if (s_dicTreeInfoList.TryGetValue(intVector, out var value))
				{
					s_dicTreeInfoList.Remove(intVector);
					TreeInfo.FreeTIs(value);
					break;
				}
			}
			intVector.x = terTile.tileX;
			intVector.y = terTile.tileZ;
			s_dicTreeInfoList.Add(intVector, list);
		}
	}

	private void GenGrassOnTile(VFTile terTile, int szCell)
	{
		if (terTile.tileL != 0)
		{
			return;
		}
		_tmpVec2.x = terTile.tileX;
		_tmpVec2.y = terTile.tileZ;
		lock (s_dicGrassInstList)
		{
			if (s_dicGrassInstList.ContainsKey(_tmpVec2))
			{
				return;
			}
		}
		paintConfig.RandSeed = 79;
		List<VoxelGrassInstance> list = new List<VoxelGrassInstance>();
		paintConfig.PlantGrass(terTile, dTileNoiseBuf, fTileHeightBuf, fTileGradTanBuf, list, tileMapType, szCell);
		int num = s_ofsToCheckPlants.Length;
		lock (s_dicGrassInstList)
		{
			IntVector2 intVector = new IntVector2();
			for (int i = 0; i < num; i += 2)
			{
				intVector.x = terTile.tileX + (s_ofsToCheckPlants[i] << terTile.tileL);
				intVector.y = terTile.tileZ + (s_ofsToCheckPlants[i + 1] << terTile.tileL);
				if (s_dicGrassInstList.Remove(intVector))
				{
					break;
				}
			}
			intVector.x = terTile.tileX;
			intVector.y = terTile.tileZ;
			s_dicGrassInstList.Add(intVector, list);
		}
	}

	private void FillChunkDataByTile0(VFVoxelChunkData chunk, VFTile terTile, int vyStart, bool bWater)
	{
		int num = 0;
		byte[] array = VFVoxelChunkData.s_ChunkDataPool.Get();
		Array.Clear(array, 0, array.Length);
		vyStart--;
		int num2 = vyStart + 35;
		if (num2 > terTile.tileH)
		{
			num2 = terTile.tileH;
		}
		if (vyStart < 0)
		{
			vyStart = 0;
			num += 70;
		}
		int num3 = num2 * 2;
		int num4 = vyStart * 2;
		byte[][][] array2 = ((!bWater) ? terTile.terraVoxels : terTile.waterVoxels);
		int num5 = 0;
		while (num5 < 35)
		{
			int num6 = 0;
			int num7 = 0;
			while (num6 < 35)
			{
				byte[] array3 = array2[num5][num6];
				int num8 = num + num7;
				int num9 = num4;
				while (num9 < num3)
				{
					array[num8] = array3[num9];
					array[num8 + 1] = array3[num9 + 1];
					num9 += 2;
					num8 += 70;
				}
				num6++;
				num7 += 2;
			}
			num5++;
			num += 2450;
		}
		chunk.OnDataLoaded(array, bFromPool: true);
	}

	private void FillTerraChunkDataByTileLOD(VFVoxelChunkData chunk, VFTile tile, int vyStart)
	{
		int tileL = tile.tileL;
		if (tileL > S_VoxelIndex.Length - 1)
		{
			Debug.LogWarning("[VFDataRTGen]Error: Unexpected lod " + tileL);
			return;
		}
		if (tileL == 0)
		{
			FillChunkDataByTile0(chunk, tile, vyStart, bWater: false);
			return;
		}
		int[] array = S_VoxelIndex[tileL];
		int num = 0;
		byte[] array2 = VFVoxelChunkData.s_ChunkDataPool.Get();
		Array.Clear(array2, 0, array2.Length);
		vyStart--;
		for (int i = 0; i < 35; i++)
		{
			int num2 = num;
			int num3 = 0;
			while (num3 < 35)
			{
				int num4 = num2;
				byte[] array3 = tile.terraVoxels[i][num3];
				int num5 = array3.Length >> 1;
				for (int j = 0; j < 35; j++)
				{
					int num6 = vyStart + (array[j] & 0xFF) + (array[j] >> 8 << 5);
					if (num6 >= 0 && num6 < num5)
					{
						int num7 = num6 << 1;
						array2[num4] = array3[num7];
						array2[num4 + 1] = array3[num7 + 1];
					}
					num4 += 70;
				}
				num3++;
				num2 += 2;
			}
			num += 2450;
		}
		chunk.OnDataLoaded(array2, bFromPool: true);
	}

	private void FillWaterChunkDataByTileLOD(VFVoxelChunkData chunk, VFTile tile, int vyStart)
	{
		int tileL = tile.tileL;
		if (tileL > S_VoxelIndex.Length - 1)
		{
			Debug.LogWarning("[VFDataRTGen]Error: Unexpected lod " + tileL);
			return;
		}
		if (tileL == 0)
		{
			FillChunkDataByTile0(chunk, tile, vyStart, bWater: true);
			return;
		}
		byte[] array = new byte[2];
		if (waterHeight < (float)vyStart)
		{
			array[0] = 0;
			array[1] = 0;
		}
		else if (waterHeight >= (float)(vyStart + (1 << 5 + tileL)))
		{
			array[0] = byte.MaxValue;
			array[1] = 128;
		}
		else
		{
			array[0] = 128;
			array[1] = 128;
		}
		if ((object)VFVoxelWater.self != null)
		{
			VFVoxelWater.self.OnWaterDataLoad(chunk, array, bFromPool: false);
		}
	}

	private int FillTileData(int tileX, int tileZ, int lod, VFTile tile)
	{
		_tmpVec4.x = tileX;
		_tmpVec4.y = tileZ;
		_tmpVec4.z = lod;
		_tmpVec4.w = s_noiseHeight;
		VFTerTileCacheDesc vFTerTileCacheDesc = _tileFileCache.FillTileDataWithFileCache(_tmpVec4, tile, dTileNoiseBuf, fTileHeightBuf, fTileGradTanBuf, tileMapType);
		if (vFTerTileCacheDesc != null)
		{
			isTownTile = 0 != (vFTerTileCacheDesc.bitMask & 1);
			if (lod == 0 && VArtifactTownManager.Instance != null)
			{
				_tmpTileIndex.x = tileX;
				_tmpTileIndex.y = tileZ;
				VArtifactTownManager.Instance.GenTownFromTileIndex(_tmpTileIndex);
			}
			return 1;
		}
		tile.tileX = tileX;
		tile.tileZ = tileZ;
		tile.tileL = lod;
		FillTileDataWithNoise(tile);
		int bitMask = (isTownTile ? 1 : 0) | ((lod > 1) ? 2 : 62);
		_tileFileCache.SaveDataToFileCaches(bitMask, tile, dTileNoiseBuf, fTileHeightBuf, fTileGradTanBuf, tileMapType);
		return 0;
	}

	public void Close()
	{
		_tileFileCache.Close();
	}

	public void AddRequest(VFVoxelChunkData chunkData)
	{
		int num = chunkData.ChunkPosLod.y << 5;
		if (num < 0 || (float)num >= (float)s_noiseHeight + 0.5f)
		{
			chunkData.OnDataLoaded(VFVoxelChunkData.S_ChunkDataAir);
			return;
		}
		bool flag = chunkData.SigOfType == 1;
		RTGenChunkReq value = null;
		if (!_chunkReqList.TryGetValue(chunkData.ChunkPosLod, out value))
		{
			value = new RTGenChunkReq();
			_chunkReqList[chunkData.ChunkPosLod] = value;
		}
		if (flag)
		{
			value.waterChunk = chunkData;
			value.waterStamp = chunkData.StampOfUpdating;
		}
		else
		{
			value.terraChunk = chunkData;
			value.terraStamp = chunkData.StampOfUpdating;
		}
	}

	private int CompareChunkPosLodXZ(IntVector4 cpos0, IntVector4 cpos1)
	{
		if (cpos0.w != cpos1.w)
		{
			return cpos0.w - cpos1.w;
		}
		Vector3 lastRefreshPos = LODOctreeMan.self.LastRefreshPos;
		float num = lastRefreshPos.x - (float)cpos0.x;
		float num2 = lastRefreshPos.z - (float)cpos0.z;
		float num3 = lastRefreshPos.x - (float)cpos1.x;
		float num4 = lastRefreshPos.z - (float)cpos1.z;
		float num5 = num * num + num2 * num2;
		float num6 = num3 * num3 + num4 * num4;
		return (int)(num5 - num6);
	}

	public void ProcessReqs()
	{
		if (_chunkReqList.Count <= 0)
		{
			return;
		}
		IntVector4 intVector = null;
		List<IntVector4> list = _chunkReqList.Keys.ToList();
		list.Sort(CompareChunkPosLodXZ);
		for (int i = 0; i < list.Count; i++)
		{
			intVector = list[i];
			RTGenChunkReq rTGenChunkReq = _chunkReqList[intVector];
			if (!rTGenChunkReq.TerraOutOfDate || !rTGenChunkReq.WaterOutOfDate)
			{
				VFTile curTile = _curTile;
				if (curTile.tileX != intVector.x || curTile.tileZ != intVector.z || curTile.tileL != intVector.w)
				{
					FillTileData(intVector.x, intVector.z, intVector.w, curTile);
					GenGrassOnTile(curTile, 1);
					GenTreesOnTile(curTile, 1);
				}
				int vyStart = intVector.y << 5;
				if (!rTGenChunkReq.TerraOutOfDate)
				{
					FillTerraChunkDataByTileLOD(rTGenChunkReq.terraChunk, curTile, vyStart);
				}
				if (!rTGenChunkReq.WaterOutOfDate)
				{
					FillWaterChunkDataByTileLOD(rTGenChunkReq.waterChunk, curTile, vyStart);
				}
			}
			_chunkReqList.Remove(intVector);
		}
	}

	public static void SetTerrainParam()
	{
		terTypeChanceIncList = new List<float[]>();
		for (int i = 0; i < regionArray.Length; i++)
		{
			RegionDescArrayCLS regionDescArrayCLS = regionArray[i];
			float[] array = new float[5];
			float[] array2 = new float[5];
			for (int j = 0; j < 5; j++)
			{
				array[j] = regionDescArrayCLS.TerrainDescArrayValues[j].terChance;
				if (j == 0)
				{
					array2[j] = array[j];
				}
				else
				{
					array2[j] = array[j] + array2[j - 1];
				}
			}
			TEST_REGION_CNT = 5;
			TEST_REGION_THRESHOLD = GRASS_REGION_THRESHOLD;
			terTypeChanceIncList.Add(array2);
		}
	}

	public static void ComputeTerNoise(ref float fTerNoise, int nTerType, float fTerType)
	{
		switch (nTerType)
		{
		default:
			fTerNoise += 0.8f - (-0.32f - fTerType) * 0.376766f;
			break;
		case 0:
		case 4:
			fTerNoise += 0.65f;
			break;
		case 1:
			fTerNoise += 0.8f - (-0.32f - fTerType) * 0.376766f;
			break;
		case 3:
			fTerNoise += 0.8f - (fTerType - 5f / 32f) * 0.3491f;
			break;
		case 2:
			fTerNoise += 0.9f - Math.Abs(fTerType - 15f / 128f) * 2.56f;
			break;
		}
	}

	public static RandomMapType GetXZMapType(int x, int z)
	{
		if (myNoise == null)
		{
			LogManager.Error("The Class VFDataRtGen.cs not initialized!!!");
			return RandomMapType.GrassLand;
		}
		float scaledX = (float)x * s_detailScale;
		float scaledZ = (float)z * s_detailScale;
		return GetMapType(scaledX, scaledZ);
	}

	public static RandomMapType GetXZMapType(IntVector2 worldXZ)
	{
		int x = worldXZ.x;
		int y = worldXZ.y;
		return GetXZMapType(x, y);
	}

	public static float GetfNoise12D1ten(int worldx, int worldz)
	{
		float scaledX = (float)worldx * s_detailScale;
		float scaledZ = (float)worldz * s_detailScale;
		return GetfNoise12D1ten(scaledX, scaledZ);
	}

	public static float GetfNoise12D1ten(float scaledX, float scaledZ)
	{
		float num = ((float)myNoise[1].Noise(scaledX * terrainFrequencyX, scaledZ * terrainFrequencyZ) + 1f) * 50f;
		float num2 = ((float)myNoise[4].Noise(scaledX * terrainFrequencyX, scaledZ * terrainFrequencyZ) + 1f) * 50f;
		float num3 = ((float)myNoise[14].Noise(scaledX * terrainFrequencyX * 3f, scaledZ * terrainFrequencyZ * 3f) + 1f) * 50f;
		float num4 = 10f;
		float num5 = 50f;
		float num6 = num5 / 2f;
		if (num > num5)
		{
			num = (num - num5) * ((islandFactor - num5) / (100f - num5)) + num5;
		}
		if (num2 > num5)
		{
			num2 = (num2 - num5) * ((islandFactor - num5) / (100f - num5)) + num5;
		}
		num3 = num3 * (num5 / 100f) + (75f - num5) / 2f;
		num3 = Mathf.Clamp(num3, 0f, 100f);
		float num7 = Mathf.Max(num, num2);
		float num8 = Mathf.Min(num, num2);
		float num10;
		if (num7 <= num5)
		{
			if (num7 <= num6)
			{
				if (num3 > num7)
				{
					float num9 = num6 - num7;
					num10 = ((!(num9 < num4)) ? num3 : (num7 * (1f - num9 / num4) + num3 * num9 / num4));
				}
				else
				{
					num10 = num7;
				}
			}
			else
			{
				num10 = num7;
			}
		}
		else
		{
			num10 = num7;
		}
		float num11 = 6f;
		float num12 = (float)myNoise[17].Noise(scaledX * terrainFrequencyX, scaledZ * terrainFrequencyZ);
		float num13 = num12 * num11 + 2f;
		num10 += num13;
		float num14 = 20f;
		float num15 = (float)myNoise[3].Noise(scaledX * terrainFrequencyX * 6f, scaledZ * terrainFrequencyZ * 6f);
		float num16 = (float)myNoise[8].Noise(scaledX * terrainFrequencyX * 4f, scaledZ * terrainFrequencyZ * 4f);
		float num17 = num15;
		float num18 = (num16 + 1f) * 0.5f;
		float num19 = num17 * num18 * num14 + 4f;
		num10 += num19;
		float mountain = GetMountain(scaledX, scaledZ, 5, 30f, 1f);
		float mountain2 = GetMountain(scaledX, scaledZ, 12, 20f, 2f);
		mountain2 *= Mathf.Pow(num10 / num5, 2f);
		num10 += Mathf.Max(mountain, mountain2);
		float finalSierra = GetFinalSierra(scaledX, scaledZ);
		return num10 + finalSierra;
	}

	private void GenCave(float caveHeight, float caveThickness, byte[] yVoxels)
	{
		byte b = 225;
		byte b2 = 165;
		if (caveHeight < 0f)
		{
			caveHeight = 0f;
		}
		int num = Mathf.FloorToInt(caveHeight);
		int num2 = Mathf.FloorToInt(caveHeight);
		int num3 = num2 * 2;
		while (num2 < Mathf.CeilToInt(caveHeight + caveThickness))
		{
			if (num3 < yVoxels.Length && yVoxels[num3] > b)
			{
				num++;
				num2++;
				num3 += 2;
				continue;
			}
			num = num2;
			break;
		}
		int num4 = Mathf.CeilToInt(caveHeight + caveThickness);
		int num5 = num4;
		int num6 = num5 * 2;
		while (num5 > Mathf.FloorToInt(caveHeight))
		{
			if (num6 < yVoxels.Length && yVoxels[num6] > b2)
			{
				num4 = num5;
				break;
			}
			num4--;
			num5--;
			num6 -= 2;
		}
		int num7 = yVoxels.Length / 2 - 1;
		int num8 = num7;
		int num9 = num8 * 2;
		while (num8 > 0)
		{
			num7 = num8;
			if (yVoxels[num9] > b2)
			{
				break;
			}
			num8--;
			num9 -= 2;
		}
		int num10 = num7 - 17;
		int num11 = Mathf.Min(num, num4, num10);
		int num12 = Mathf.CeilToInt(caveHeight);
		int num13 = num12 * 2;
		while (num12 < num11)
		{
			if ((float)num12 > caveHeight + 1f)
			{
				yVoxels[num13] = 0;
			}
			if (num12 == Mathf.CeilToInt(caveHeight))
			{
				yVoxels[num13] = (byte)Mathf.RoundToInt(255f * (caveHeight - (float)Mathf.FloorToInt(caveHeight)));
			}
			if ((float)(int)yVoxels[num13] < 127.5f)
			{
				yVoxels[num13 + 1] = 0;
			}
			num12++;
			num13 += 2;
		}
	}

	private void GenTileVoxelWithHeightMap(float voxelX, float voxelY, float voxelZ, float fTerNoise, ref float fNoiseXZ, ref byte volume, ref byte type, int topCorrection, int vy)
	{
		float num = DensityThresholdMinusDelta + voxelY / 10f;
		volume = byte.MaxValue;
		type = 1;
		if ((double)(int)volume > 127.5 && voxelY < fTerNoiseHeight[topCorrection - 1])
		{
			volume = 0;
		}
		else if ((double)(int)volume > 127.5 && voxelY < fTerNoiseHeight[topCorrection - 2])
		{
			float num2 = (float)myNoise[16].Noise2DFBM(voxelX * disturbFrequency, voxelZ * disturbFrequency, 2);
			volume = (byte)((double)(int)volume * ((double)num2 * 0.5 + 0.5));
		}
	}

	private static bool GenTileVoxelOnly(float voxelX, float voxelY, float voxelZ, float flatParam, float fDensityClamp, float fTerType, int nTerType, float fTerNoise, ref byte volume, ref byte type, float PlainThickness)
	{
		int nOctaves = GetnOcvtaves(nTerType);
		float hASChangeFactor = GetHASChangeFactor(fTerType);
		float num = (float)myNoise[11].Noise3DFBM(voxelX * flatParam, voxelY * flatParam, voxelZ * flatParam, 1);
		float num2 = 0f - fDensityClamp + Mathf.Abs(num * 0.08f) + (1f + 1.2f * num + 1f * (float)myNoise[11].Noise3DFBM(voxelX * 2f * flatParam, voxelY * 2f * flatParam, voxelZ * 2f * flatParam, nOctaves) * 0.5f) * fTerType + GetHASValue(voxelX, voxelZ, hASChangeFactor) + Mathf.Pow(GetFinalSierraNoise(voxelX, voxelZ), 2f) * HASChangeValueSierra * hASChangeFactor;
		if (num2 <= DensityThresholdMinusDelta)
		{
			volume = 0;
			type = 0;
		}
		else
		{
			volume = ((!(num2 > DensityThresholdPlusDelta)) ? ((byte)((num2 - DensityThresholdMinusDelta) * DensityDeltaHalf255Reci * fTerNoise)) : byte.MaxValue);
			type = 1;
			if ((double)(int)volume > 127.5 && voxelY < fTerNoiseHeight[MountainTopCorrection - 1])
			{
				volume = 0;
			}
			else if ((double)(int)volume > 127.5 && voxelY < fTerNoiseHeight[MountainTopCorrection - 2])
			{
				float num3 = (float)myNoise[16].Noise2DFBM(voxelX * disturbFrequency, voxelZ * disturbFrequency, 2);
				volume = (byte)((double)(int)volume * ((double)num3 * 0.5 + 0.5));
			}
			if (volume > 168)
			{
				return true;
			}
		}
		return false;
	}

	private void GenTileVoxel(float voxelX, float voxelY, float voxelZ, float flatParam, float fDensityClamp, float fTerType, int nTerType, float fTerNoise, ref float fNoiseXZ, ref byte volume, ref byte type, bool hillNotTop, int topCorrection, float PlainThickness)
	{
		int nOctaves = GetnOcvtaves(nTerType);
		float hASChangeFactor = GetHASChangeFactor(fTerType);
		float num = (float)myNoise[11].Noise3DFBM(voxelX * flatParam, voxelY * flatParam * 0.1f, voxelZ * flatParam, 1);
		float num2 = 0f - fDensityClamp + Mathf.Abs(num * 0.08f) + (1f + 1.2f * num + 1f * (float)myNoise[11].Noise3DFBM(voxelX * 2f * flatParam, voxelY * 2f * flatParam, voxelZ * 2f * flatParam, nOctaves) * 0.5f) * fTerType + GetHASValue(voxelX, voxelZ, hASChangeFactor) + Mathf.Pow(GetFinalSierraNoise(voxelX, voxelZ), 2f) * HASChangeValueSierra * hASChangeFactor;
		if (num2 <= DensityThresholdMinusDelta)
		{
			volume = 0;
			type = 0;
			return;
		}
		volume = ((!(num2 > DensityThresholdPlusDelta)) ? ((byte)((num2 - DensityThresholdMinusDelta) * DensityDeltaHalf255Reci * fTerNoise)) : byte.MaxValue);
		type = 1;
		if ((double)(int)volume > 127.5 && voxelY < fTerNoiseHeight[topCorrection - 1])
		{
			volume = 0;
		}
		else if ((double)(int)volume > 127.5 && voxelY < fTerNoiseHeight[topCorrection - 2])
		{
			float num3 = (float)myNoise[16].Noise2DFBM(voxelX * disturbFrequency, voxelZ * disturbFrequency, 2);
			volume = (byte)((double)(int)volume * ((double)num3 * 0.5 + 0.5));
		}
	}

	private static bool GenTileVoxelVolume(float voxelX, float voxelY, float voxelZ, float flatParam, float fDensityClamp, float fTerType, int nTerType, float fTerNoise, float PlainThickness)
	{
		int nOctaves = GetnOcvtaves(nTerType);
		float hASChangeFactor = GetHASChangeFactor(fTerType);
		byte b = 0;
		float num = (float)myNoise[11].Noise3DFBM(voxelX * flatParam, voxelY * flatParam, voxelZ * flatParam, 1);
		float num2 = 0f - fDensityClamp + Mathf.Abs(num * 0.08f) + (1f + 1.2f * num + 1f * (float)myNoise[11].Noise3DFBM(voxelX * 2f * flatParam, voxelY * 2f * flatParam, voxelZ * 2f * flatParam, nOctaves) * 0.5f) * fTerType + GetHASValue(voxelX, voxelZ, hASChangeFactor) + Mathf.Pow(GetFinalSierraNoise(voxelX, voxelZ), 2f) * HASChangeValueSierra * hASChangeFactor;
		if (num2 <= DensityThresholdMinusDelta)
		{
			b = 0;
		}
		else
		{
			b = ((!(num2 > DensityThresholdPlusDelta)) ? ((byte)((num2 - DensityThresholdMinusDelta) * DensityDeltaHalf255Reci * fTerNoise)) : byte.MaxValue);
			if ((double)(int)b > 127.5)
			{
				return true;
			}
		}
		return false;
	}

	private static int GetnOcvtaves(int nTerType)
	{
		if (IsHillTerrain(nTerType))
		{
			return 7;
		}
		if (IsPlainTerrain(nTerType))
		{
			return 3;
		}
		if (IsSeaSideTerrain(nTerType))
		{
			return 2;
		}
		if (IsWaterTerrain(nTerType))
		{
			return 1;
		}
		return 3;
	}

	private static float GetHASChangeFactor(float fTerType)
	{
		if (fTerType < -0.32f)
		{
			return 0f;
		}
		if (fTerType > 5f / 64f)
		{
			return 1f;
		}
		return Mathf.Pow((fTerType - -0.32f) / 0.398125f, 2f);
	}

	private static float GetHASValue(float scaledX, float scaledZ, float HASChangeFactor)
	{
		return (float)myNoise[15].Noise(scaledX * HASChangeFrequency, scaledZ * HASChangeFrequency) * HASChangeValue * HASChangeFactor;
	}

	private static float GetMountain(float scaledX, float scaledZ, int startNoiseIndex, float topValue, float frequencyF)
	{
		float f = (float)myNoise[startNoiseIndex + 1].Noise(scaledX * terrainFrequencyX * MountainFrequencyFactor * frequencyF, scaledZ * terrainFrequencyZ * MountainFrequencyFactor * frequencyF);
		float num = (float)myNoise[startNoiseIndex + 2].Noise(scaledX * terrainFrequencyX * MountainFrequencyFactor * 0.375f * frequencyF, scaledZ * terrainFrequencyZ * MountainFrequencyFactor * 0.375f * frequencyF);
		float num2 = 1f - Mathf.Abs(f);
		float num3 = Mathf.Sqrt((num + 1f) * 0.5f);
		float num4 = num2 * num3 * topValue;
		if (num4 < topValue / 10f)
		{
			return 0f;
		}
		return (num4 - topValue / 10f) * 10f / 9f;
	}

	private static float GetFinalSierra(float scaledX, float scaledZ)
	{
		float num = 30f;
		float finalSierraNoise = GetFinalSierraNoise(scaledX, scaledZ);
		float num2 = Mathf.Sqrt(finalSierraNoise) * num;
		if (num2 < num / 16f * 15f)
		{
			return 0f;
		}
		return (num2 - num / 16f * 15f) * 16f;
	}

	private static float GetFinalSierraNoise(float scaledX, float scaledZ)
	{
		float num = (float)myNoise[7].Noise(scaledX * terrainFrequencyX * SierraFrequencyFactor, scaledZ * terrainFrequencyZ * SierraFrequencyFactor);
		return (num + 1f) * 0.5f;
	}

	private static float TownFTerTypeTop(float[] terTypeInc)
	{
		return terTypeInc[2];
	}

	private static float TownFTerTypeBottom(float[] terTypeInc)
	{
		return terTypeInc[1] * 3f / 4f + terTypeInc[0] * 1f / 4f;
	}

	public static float GetOriginalFterType(IntVector2 worldXZ, out float fTerTypeBridge, out bool caveEnable, out bool riverArea, out bool lakeArea, out float riverValue, out float bridgeValue, out float bridge2dFactor, out RandomMapType mapType, out float[] terTypeInc)
	{
		float scaledX = (float)worldXZ.x * s_detailScale;
		float scaledZ = (float)worldXZ.y * s_detailScale;
		mapType = GetMapTypeAndTerInc(scaledX, scaledZ, out terTypeInc);
		float num = GetfNoise12D1ten(scaledX, scaledZ);
		float continentValue = GetContinentValue(worldXZ.x, worldXZ.y);
		int num2 = 0;
		float origin = num / 100f;
		origin = BlendContinentBound(origin, continentValue, terTypeInc);
		if (origin < 0f)
		{
			origin = 0f;
		}
		caveEnable = true;
		riverArea = false;
		lakeArea = false;
		riverValue = GetRiverValue(worldXZ.x, worldXZ.y, ref origin, ref caveEnable, ref lakeArea, out bridgeValue, out bridge2dFactor, terTypeInc);
		if (riverValue < 0.85f)
		{
			riverArea = true;
		}
		float result = origin * riverValue;
		fTerTypeBridge = -1f;
		if (bridgeValue != -1f)
		{
			fTerTypeBridge = origin * bridgeValue;
		}
		return result;
	}

	public static float GetOriginalFterType(IntVector2 worldXZ, out float fTerTypeBridge, out float[] terTypeInc)
	{
		float scaledX = (float)worldXZ.x * s_detailScale;
		float scaledZ = (float)worldXZ.y * s_detailScale;
		GetMapTypeAndTerInc(scaledX, scaledZ, out terTypeInc);
		float num = GetfNoise12D1ten(scaledX, scaledZ);
		float continentValue = GetContinentValue(worldXZ.x, worldXZ.y);
		int num2 = 0;
		float origin = num / 100f;
		origin = BlendContinentBound(origin, continentValue, terTypeInc);
		if (origin < 0f)
		{
			origin = 0f;
		}
		bool caveEnable = true;
		bool lakeArea = false;
		bool flag = false;
		float bridgeValue;
		float bridge2dFactor;
		float riverValue = GetRiverValue(worldXZ.x, worldXZ.y, ref origin, ref caveEnable, ref lakeArea, out bridgeValue, out bridge2dFactor, terTypeInc);
		if (riverValue < 0.85f)
		{
			flag = true;
		}
		float result = origin * riverValue;
		fTerTypeBridge = -1f;
		if (bridgeValue != -1f)
		{
			fTerTypeBridge = origin * bridgeValue;
		}
		return result;
	}

	public static float GetFinalFterType(IntVector2 worldXZ, out float fTerTypeBridge, out bool caveEnable, out bool riverArea, out bool lakeArea, out float riverValue, out float bridgeValue, out float bridge2dFactor, out RandomMapType mapType, out float[] terTypeInc)
	{
		return GetOriginalFterType(worldXZ, out fTerTypeBridge, out caveEnable, out riverArea, out lakeArea, out riverValue, out bridgeValue, out bridge2dFactor, out mapType, out terTypeInc);
	}

	public static float GetFinalFterType(IntVector2 worldXZ, out float fTerTypeBridge, out float[] terTypeInc)
	{
		return GetOriginalFterType(worldXZ, out fTerTypeBridge, out terTypeInc);
	}

	private static float GetBiomaChangeValue(float finalFTerType, IntVector2 worldXZ)
	{
		return finalFTerType;
	}

	private static float GetTownChangeValue(float finalFTerType, IntVector2 worldXZ, float[] terTypeInc)
	{
		if (VArtifactUtil.HasTown())
		{
			if (finalFTerType > TownFTerTypeTop(terTypeInc))
			{
				float num = 1f;
				VArtifactTown vaTown;
				float areaTownDistance = VATownGenerator.Instance.GetAreaTownDistance(worldXZ.x, worldXZ.y, out vaTown);
				float num2 = (float)worldXZ.x * s_detailScale;
				float num3 = (float)worldXZ.y * s_detailScale;
				float num4 = Mathf.Abs((float)myNoise[10].Noise(num2 * 0.25f, num3 * 0.25f));
				float num5 = 1f + num4 * 2f;
				if (vaTown != null)
				{
					if (areaTownDistance < (float)vaTown.SmallRadius * num5)
					{
						num = 0f;
					}
					else if (areaTownDistance < ((float)vaTown.SmallRadius + 64f) * num5)
					{
						num = (areaTownDistance - (float)vaTown.SmallRadius * num5) / (64f * num5);
					}
				}
				if (num > 0f)
				{
					float townConnectionFactor = GetTownConnectionFactor(worldXZ.x, worldXZ.y, 64f, num5);
					if (num > townConnectionFactor)
					{
						num = townConnectionFactor;
					}
				}
				finalFTerType = num * (finalFTerType - TownFTerTypeTop(terTypeInc)) + TownFTerTypeTop(terTypeInc);
			}
			else if (finalFTerType < TownFTerTypeBottom(terTypeInc))
			{
				float num6 = 1f;
				VArtifactTown vaTown2;
				float areaTownDistance2 = VATownGenerator.Instance.GetAreaTownDistance(worldXZ.x, worldXZ.y, out vaTown2);
				float num7 = (float)worldXZ.x * s_detailScale;
				float num8 = (float)worldXZ.y * s_detailScale;
				float num9 = ((float)myNoise[10].Noise(num7 * 0.25f, num8 * 0.25f) + 1f) / 2f;
				float num10 = 1f + num9 * 2f;
				if (vaTown2 != null)
				{
					if (areaTownDistance2 < (float)vaTown2.MiddleRadius * num10)
					{
						num6 = 0f;
					}
					else if (areaTownDistance2 < ((float)vaTown2.MiddleRadius + 48f) * num10)
					{
						num6 = (areaTownDistance2 - (float)vaTown2.MiddleRadius * num10) / (48f * num10);
					}
				}
				if (num6 > 0f)
				{
					float townConnectionFactor2 = GetTownConnectionFactor(worldXZ.x, worldXZ.y, 48f, num10);
					if (num6 > townConnectionFactor2)
					{
						num6 = townConnectionFactor2;
					}
				}
				finalFTerType = num6 * (finalFTerType - TownFTerTypeBottom(terTypeInc)) + TownFTerTypeBottom(terTypeInc);
			}
		}
		else
		{
			int num11 = IntVector2.SqrMagnitude(worldXZ - noTownStartPos);
			if ((float)num11 < 82944f)
			{
				float num12 = ((sceneClimate != ClimateType.CT_Wet) ? terTypeInc[2] : terTypeInc[3]);
				if (finalFTerType < num12)
				{
					float num13 = Mathf.Sqrt(num11);
					float num14 = (float)worldXZ.x * s_detailScale;
					float num15 = (float)worldXZ.y * s_detailScale;
					float num16 = ((float)myNoise[10].Noise(num14 * 0.25f, num15 * 0.25f) + 1f) / 2f;
					float num17 = 1f + num16 * 2f;
					float num18 = 1f;
					if (num13 < 96f * num17)
					{
						num18 = num13 / (96f * num17);
					}
					finalFTerType = num18 * (finalFTerType - num12) + num12;
				}
			}
		}
		return finalFTerType;
	}

	public static bool IsTownConnectionType(int x, int z)
	{
		float num = (float)x * s_detailScale;
		float num2 = (float)z * s_detailScale;
		float num3 = ((float)myNoise[10].Noise(num * 0.25f, num2 * 0.25f) + 1f) / 2f;
		float num4 = 1f + num3 * 2f;
		float connectionLineDistance = VATownGenerator.Instance.GetConnectionLineDistance(new IntVector2(x, z), onConnection: true);
		if (connectionLineDistance < 4f * num4)
		{
			return true;
		}
		return false;
	}

	public static float GetTownConnectionFactor(int x, int z, float distance, float changeFactor = -1f)
	{
		if (changeFactor == -1f)
		{
			float num = (float)x * s_detailScale;
			float num2 = (float)z * s_detailScale;
			float num3 = Mathf.Abs((float)myNoise[10].Noise(num * 0.25f, num2 * 0.25f));
			changeFactor = 1f + num3 * 2f;
		}
		float result = 1f;
		float connectionLineDistance = VATownGenerator.Instance.GetConnectionLineDistance(new IntVector2(x, z));
		if (connectionLineDistance < 4f * changeFactor)
		{
			result = 0f;
		}
		else if (connectionLineDistance < (4f + distance) * changeFactor)
		{
			result = (connectionLineDistance - 4f * changeFactor) / (distance * changeFactor);
		}
		return result;
	}

	public static int GetPosHeight(float x, float z, bool inWater = false)
	{
		IntVector2 worldXZ = new IntVector2(Mathf.RoundToInt(x), Mathf.RoundToInt(z));
		return GetPosHeight(worldXZ, inWater);
	}

	public static int GetPosHeight(IntVector2 worldXZ, bool inWater = false)
	{
		ReGenDensityClampStatic(worldXZ, out var array);
		float num = (float)worldXZ.x * s_detailScale;
		float num2 = (float)worldXZ.y * s_detailScale;
		float fTerTypeBridge;
		float[] terTypeInc;
		float finalFterType = GetFinalFterType(worldXZ, out fTerTypeBridge, out terTypeInc);
		float num3 = Mathf.Max(finalFterType, fTerTypeBridge);
		float fTerType = num3;
		float fTerNoise = (fTerType * 2f - 1f) * 0.05f;
		GetFTerTypeAndZnTerType(ref fTerType, out var nTerType, terTypeInc);
		ComputeTerNoise(ref fTerNoise, nTerType, fTerType);
		float factor = (float)myNoise[9].Noise(num * flatFrequency, num2 * flatFrequency);
		float flatParamFromFactor = GetFlatParamFromFactor(factor, worldXZ.x, worldXZ.y);
		int num4 = s_noiseHeight - 1;
		float num5 = s_seaDepth + 1;
		int num6 = num4;
		if (!IsWaterTerrain(nTerType))
		{
			if (IsHillTerrain(nTerType))
			{
				float num7 = s_noiseHeight;
				num4 = s_noiseHeight;
			}
			else if (!IsSeaSideTerrain(nTerType))
			{
				float num7 = (float)PlainTopHeight(HASMid) + (num3 - terTypeInc[1]) / (terTypeInc[2] - terTypeInc[1]) * (float)(HillBottomHeight(HASMid) - PlainTopHeight(HASMid));
				num4 = Mathf.FloorToInt(num7);
			}
			else
			{
				float num8 = terTypeInc[0] / 16f;
				float num9 = terTypeInc[0] * 17f / 16f;
				float num10 = 1f;
				if (finalFterType < num9)
				{
					num10 = (Mathf.Sin(((finalFterType - num8) / (num9 - num8) - 0.5f) * (float)Math.PI) + 1f) / 2f;
				}
				num5 = (float)(s_seaDepth + 1) * num10;
				float num7 = PlainTopHeight(HASMid);
				num4 = PlainTopHeight(HASMid);
			}
			OptimiseMaxHeight(ref num4, num, num2, flatParamFromFactor, fTerType, nTerType, fTerNoise, num5, fTerNoiseHeight, array, HASMid, 1);
			num6 = Mathf.Clamp(num4, 0, s_noiseHeight - 1);
			while ((float)num6 > num5 && !GenTileVoxelVolume(num, fTerNoiseHeight[num6], num2, flatParamFromFactor, array[num6], fTerType, nTerType, fTerNoise, HASMid))
			{
				num6--;
			}
		}
		else
		{
			float num11 = terTypeInc[0] / 16f;
			float num12 = terTypeInc[0] * 17f / 16f;
			float f;
			if (num3 < num11)
			{
				f = 0f;
			}
			else
			{
				float num13 = (Mathf.Sin(((num3 - num11) / (num12 - num11) - 0.5f) * (float)Math.PI) + 1f) / 2f;
				f = num13 * (float)(s_seaDepth + 1);
			}
			num4 = Mathf.CeilToInt(f);
			num6 = Mathf.FloorToInt(f);
		}
		if ((float)num6 < waterHeight + 1f && !inWater)
		{
			return Mathf.CeilToInt(waterHeight + 1f);
		}
		return num6;
	}

	public static float GetPosTop(IntVector2 worldPosXZ, out bool canGenNpc)
	{
		int num = worldPosXZ.x >> 5;
		int num2 = worldPosXZ.y >> 5;
		float num3 = GetPosHeight(worldPosXZ, inWater: true);
		float num4 = VArtifactUtil.IsInTown(worldPosXZ);
		if (num4 != 0f)
		{
			if (num3 < num4)
			{
				num3 = num4;
			}
			canGenNpc = false;
			return num3 + 1.5f;
		}
		if (IsSea(Mathf.FloorToInt(num3)))
		{
			canGenNpc = false;
			return num3 + 1.5f;
		}
		canGenNpc = true;
		return num3 + 1.5f;
	}

	public static float GetPosTop(IntVector2 worldPosXZ)
	{
		int num = worldPosXZ.x >> 5;
		int num2 = worldPosXZ.y >> 5;
		float num3 = (float)GetPosHeight(worldPosXZ) + 1.5f;
		float num4 = VArtifactUtil.IsInTown(worldPosXZ);
		if (num4 != 0f)
		{
			if (num3 < num4)
			{
				num3 = num4 + 1.5f;
			}
			return num3;
		}
		return num3;
	}

	public static float GetPosHeightWithTown(IntVector2 worldPosXZ, bool InWater = false)
	{
		float num = (float)GetPosHeight(worldPosXZ, InWater) + 1.5f;
		float num2 = VArtifactUtil.IsInTown(worldPosXZ);
		if (num2 != 0f && num < num2)
		{
			num = num2;
		}
		return num;
	}

	public static bool IsSpawnAvailable(IntVector2 worldXZ)
	{
		int x = worldXZ.x;
		int y = worldXZ.y;
		int num = RandomMapConfig.Instance.mapRadius - RandomMapConfig.Instance.boundOffset;
		if (x >= num || y >= num)
		{
			return true;
		}
		if (IsSea(x, y))
		{
			return false;
		}
		return true;
	}

	public static bool IsTownAvailable(int x, int z)
	{
		IntVector2 worldXZ = new IntVector2(x, z);
		float fTerTypeBridge;
		float[] terTypeInc;
		float originalFterType = GetOriginalFterType(worldXZ, out fTerTypeBridge, out terTypeInc);
		float fTerType = Mathf.Max(originalFterType, fTerTypeBridge);
		if (RandomMapConfig.ScenceClimate == ClimateType.CT_Wet)
		{
			if (fTerType < terTypeInc[1] + (terTypeInc[2] - terTypeInc[1]) * 0.85f && PlainMax(HASMid) < (float)(s_seaDepth + 80 + 10))
			{
				return false;
			}
		}
		else if (fTerType > terTypeInc[1] + (terTypeInc[2] - terTypeInc[1]) * 0.85f)
		{
			return false;
		}
		float num = (fTerType * 2f - 1f) * 0.05f;
		GetFTerTypeAndZnTerType(ref fTerType, out var nTerType, terTypeInc);
		if (RandomMapConfig.ScenceClimate != ClimateType.CT_Wet && IsHillTerrain(nTerType))
		{
			return false;
		}
		if (RandomMapConfig.ScenceClimate != ClimateType.CT_Wet && IsSea(x, z))
		{
			return false;
		}
		return true;
	}

	public static bool IsDungeonEntranceAvailable(IntVector2 genPos)
	{
		float fTerTypeBridge;
		float[] terTypeInc;
		float finalFterType = GetFinalFterType(genPos, out fTerTypeBridge, out terTypeInc);
		float fTerType = Mathf.Max(finalFterType, fTerTypeBridge);
		float num = (fTerType * 2f - 1f) * 0.05f;
		GetFTerTypeAndZnTerType(ref fTerType, out var nTerType, terTypeInc);
		if (IsHillTerrain(nTerType))
		{
			return false;
		}
		return true;
	}

	private static void GetFTerTypeAndZnTerType(ref float fTerType, out int nTerType, float[] terTypeInc)
	{
		if (fTerType >= terTypeInc[4])
		{
			fTerType = TEST_REGION_THRESHOLD[TEST_REGION_CNT];
			nTerType = TEST_REGION_CNT - 1;
			return;
		}
		for (nTerType = 0; nTerType < TEST_REGION_CNT; nTerType++)
		{
			if (nTerType == 0)
			{
				if (fTerType < terTypeInc[nTerType])
				{
					fTerType = (TEST_REGION_THRESHOLD[nTerType + 1] - TEST_REGION_THRESHOLD[nTerType]) / terTypeInc[nTerType] * fTerType + TEST_REGION_THRESHOLD[nTerType];
					break;
				}
			}
			else if (fTerType < terTypeInc[nTerType])
			{
				fTerType = (TEST_REGION_THRESHOLD[nTerType + 1] - TEST_REGION_THRESHOLD[nTerType]) / (terTypeInc[nTerType] - terTypeInc[nTerType - 1]) * (fTerType - terTypeInc[nTerType - 1]) + TEST_REGION_THRESHOLD[nTerType];
				break;
			}
		}
	}

	public static bool IsSea(int x, int z)
	{
		float posHeightWithTown = GetPosHeightWithTown(new IntVector2(x, z), InWater: true);
		return posHeightWithTown < waterHeight;
	}

	public static bool IsSea(int posHeight)
	{
		return (float)posHeight < waterHeight;
	}

	public static float GetBaseTerHeight(IntVector2 worldXZ)
	{
		float fTerTypeBridge;
		float[] terTypeInc;
		float finalFterType = GetFinalFterType(worldXZ, out fTerTypeBridge, out terTypeInc);
		float num = Mathf.Max(finalFterType, fTerTypeBridge);
		float num2 = num * (float)s_noiseHeight + 128f;
		return Mathf.Clamp(num2, waterHeight, num2);
	}

	private static float GetRiverValue(int worldX, int worldZ, ref float fTerType, ref bool caveEnable, ref bool lakeArea, out float bridgeValue, out float bridge2dFactor, float[] terTypeInc)
	{
		bridgeValue = -1f;
		bridge2dFactor = -1f;
		float num = 1f;
		float num2 = (float)worldX * s_detailScale;
		float num3 = (float)worldZ * s_detailScale;
		float num4 = num2 * riverFrequencyX;
		float num5 = num3 * riverFrequencyZ;
		float river2D = (float)myRiverNoise[0].Noise(num4, num5);
		float num6 = ResetThreshold(riverThreshold, fTerType, num2, num3);
		float num7 = 0.5f;
		river2D = RiverDisturb(river2D, num2, num3);
		if (Mathf.Abs(river2D) < num6 + 0.01f)
		{
			caveEnable = false;
		}
		if (Mathf.Abs(river2D) < num6)
		{
			num7 = num6 * (riverBottomPercentNow - Mathf.Pow(fTerType, 2f) * 0.1f);
			float num8 = (float)myRiverNoise[5].Noise(num2 * terrainFrequencyX * 0.25f, num3 * terrainFrequencyZ * 0.25f);
			num8 = (num8 + 1f) * 0.5f;
			num8 = 1f - num8 * num8 * num8 * num8 * num8;
			num8 = terTypeInc[0] * num8 * 1.5f / terTypeInc[4];
			float num9 = num8;
			if (Mathf.Abs(river2D) < num7)
			{
				num = num9;
			}
			else
			{
				float num10 = (Mathf.Abs(river2D) - num7) / (num6 - num7);
				float p = 1f;
				if (fTerType > terTypeInc[2])
				{
					p = 1f / (Mathf.Pow((fTerType - terTypeInc[2]) / (terTypeInc[4] - terTypeInc[2]), 0.25f) * 8f + 1f);
				}
				Mathf.Pow(num10, p);
				num = num10 * (1f - num9) + num9;
			}
		}
		int num11 = 0;
		float num12 = 0f;
		float num13 = 0f;
		float num14 = 0f;
		float num15 = 0f;
		float num16 = 1f;
		num15 = (float)myRiverNoise[1].Noise(num2 * lakeFrequency, num3 * lakeFrequency);
		num14 = ResetLakeThreshold(lakeThreshold, fTerType, num2, num3);
		if (Mathf.Abs(num15) > num14 - 0.01f)
		{
			caveEnable = false;
		}
		if (Mathf.Abs(num15) > num14 - 0.008f)
		{
			lakeArea = true;
		}
		if (Mathf.Abs(num15) > num14)
		{
			float num17 = ResetLakeBottomWidth(num14, fTerType);
			float num18 = (float)myRiverNoise[2].Noise(num2 * lakeFrequency * 0.1f, num3 * lakeFrequency * 0.1f);
			num18 = (num18 + 1f) * 0.5f;
			num18 = 1f - num18 * num18;
			num18 = terTypeInc[0] * num18 / terTypeInc[3];
			float num19 = num18;
			if (Mathf.Abs(num15) > num17)
			{
				num16 = num19;
			}
			else
			{
				float num20 = (Mathf.Abs(num15) - num17) / (num14 - num17);
				float p2 = 1f;
				if (fTerType > terTypeInc[2])
				{
					p2 = 1f / (Mathf.Pow((fTerType - terTypeInc[2]) / (terTypeInc[4] - terTypeInc[2]), 0.25f) * 8f + 1f);
				}
				Mathf.Pow(num20, p2);
				num16 = num20 * (1f - num19) + num19;
			}
		}
		if (num16 < num)
		{
			num = num16;
		}
		float finalFTerType = fTerType * num;
		float townChangeValue = GetTownChangeValue(finalFTerType, new IntVector2(worldX, worldZ), terTypeInc);
		if (townChangeValue >= fTerType)
		{
			num = 1f;
			fTerType = townChangeValue;
		}
		else if (num < 1f)
		{
			num = townChangeValue / fTerType;
		}
		else
		{
			fTerType = townChangeValue;
		}
		if (bridgeMaxHeight > float.Epsilon)
		{
			float num21 = terTypeInc[2] + (terTypeInc[4] - terTypeInc[2]) * bridgeMaxHeight;
			if (num < 0.98f)
			{
				float f = (float)myRiverNoise[4].Noise(num2 * bridgeFrequencyX * bridgeCof, num3 * bridgeFrequencyZ * bridgeCof);
				float num22 = ResetBridgeThreshold(bridgeThreshold, fTerType, terTypeInc, 0f, 0f);
				float num23 = riverFrequencyX * 2f;
				if (Mathf.Abs(f) < num22)
				{
					float num24 = 1f;
					num24 = ((!(Mathf.Abs(f) <= num22 / 2f)) ? ((Mathf.Abs(f) - num22 / 2f) / (num22 / 2f)) : 0f);
					float num25 = 0.98f;
					bridgeValue = num25 - (num25 - num) * num24;
					bridge2dFactor = Mathf.Abs(f) / num22;
					if (num * fTerType < num21 && bridgeValue * fTerType > num21)
					{
						float num26 = 1f;
						num26 = ((!(num * fTerType < terTypeInc[2])) ? ((num * fTerType - terTypeInc[2]) / (num21 - terTypeInc[2])) : 0f);
						bridgeValue = bridgeValue * num26 + num21 / fTerType * (1f - num26);
					}
				}
			}
		}
		return num;
	}

	private static float RiverDisturb(float river2D, float scaledX, float scaledZ)
	{
		float p = 1f;
		float f = 1f - (riverWidthNow - riverWidth1) / (riverWidth100 - riverWidth1);
		float num = riverThreshold1 / 2f * Mathf.Pow(f, 5f);
		float num2 = 0.75f;
		return river2D + num * (Mathf.Sin(scaledX * num2 * 2f) * 2f + Mathf.Sin(scaledZ * num2 * 2f) * 2f + Mathf.Sin(scaledX * num2 * 3.4f) + Mathf.Sin(scaledZ * num2 * 3.4f)) * Mathf.Pow(riverFrequency1 / riverFrequencyNow, p);
	}

	private float RiverWidthCorrection(float threshold)
	{
		return threshold * (1f + 3f * (1f - riverWidthNow / riverWidth100) * (1f - riverFrequencyNow / riverFrequency100));
	}

	private static float ResetBottomHeight(float riverBottomMax, float fTerType)
	{
		float num = (fTerType - -0.39f) / 1.54f;
		return riverBottomMax * num;
	}

	private static float ResetBottomWidth(float threshold, float fTerType)
	{
		float num = (fTerType - -0.39f) / 1.54f;
		return threshold * riverBankPercent * (1f - num);
	}

	private static float ResetThreshold(float riverThreshold, float fTerType, float scaledX = 0f, float scaledZ = 0f)
	{
		float num = (fTerType - -0.39f) / 1.54f;
		float num2 = 8.3045f;
		float num3 = ((!((double)num < 0.15)) ? (num2 * Mathf.Pow(num - 0.575f, 2f) + 0.5f) : 2f);
		float num4 = riverThreshold * num3;
		return num4 * (1f + 0.2f * (Mathf.Sin(scaledX * 2f) + Mathf.Sin(scaledZ * 2f) + Mathf.Sin(scaledX * 3.4f) + Mathf.Sin(scaledZ * 3.4f)));
	}

	private static float ResetBridgeThreshold(float riverThreshold, float fTerType, float[] terTypeInc, float scaledX = 0f, float scaledZ = 0f)
	{
		float num = (fTerType - -0.39f) / 1.54f;
		float num2 = 8f;
		float num3 = num2 * Mathf.Pow(num - 0.2f, 2f) + 1f;
		float num4 = riverThreshold * num3;
		num4 *= 1f + 0.1f * (Mathf.Sin(scaledX * 2f) + Mathf.Sin(scaledZ * 2f) + Mathf.Sin(scaledX * 3.4f) + Mathf.Sin(scaledZ * 3.4f));
		if (fTerType > terTypeInc[2])
		{
			num4 *= 1f + (fTerType - terTypeInc[2]) / (1f - terTypeInc[2]) * 2f;
		}
		return num4;
	}

	private void DebuffRiverBankTerrain(ref byte volume, ref byte type, float bottomHeight, float noise2D, float fTerType, IntVector2 worldxz, int vy)
	{
		if (vy > Mathf.CeilToInt(bottomHeight))
		{
			volume = 0;
			type = 0;
		}
		else if (vy == Mathf.CeilToInt(bottomHeight))
		{
			float num = bottomHeight % 1f;
			if (num == 0f)
			{
				num = 1f;
			}
			volume = TerrainUtil.HeightToVolume(num);
		}
		else if (vy == Mathf.CeilToInt(bottomHeight) - 1)
		{
			volume = byte.MaxValue;
		}
	}

	private static RandomMapType GetMapType(float scaledX, float scaledZ, out RandomMapType firstType, out RandomMapType secondType, out float diffValue)
	{
		IntVector2 worldPos = new IntVector2(Mathf.RoundToInt(scaledX / s_detailScale), Mathf.RoundToInt(scaledZ / s_detailScale));
		return GetMapTypeNew(worldPos, out firstType, out secondType, out diffValue);
	}

	private static RandomMapType GetMapType(float scaledX, float scaledZ)
	{
		IntVector2 worldPos = new IntVector2(Mathf.RoundToInt(scaledX / s_detailScale), Mathf.RoundToInt(scaledZ / s_detailScale));
		RandomMapType firstType;
		RandomMapType secondType;
		float diffValue;
		return GetMapTypeNew(worldPos, out firstType, out secondType, out diffValue);
	}

	private static RandomMapType GetMapTypeNew(IntVector2 worldPos, out RandomMapType firstType, out RandomMapType secondType, out float diffValue)
	{
		List<RandomMapTypeDist> list = new List<RandomMapTypeDist>();
		for (int i = 0; i < BiomaDistList.Count; i++)
		{
			list.Add(new RandomMapTypeDist(BiomaDistList[i].type, BiomaDistList[i].GetDistance(worldPos)));
		}
		list.Sort();
		firstType = list[0].type;
		secondType = list[1].type;
		diffValue = list[1].distance - list[0].distance;
		float num = (float)worldPos.x * s_detailScale;
		float num2 = (float)worldPos.y * s_detailScale;
		if (diffValue < changeBiomaDiff)
		{
			float num3 = (float)myBiomaNoise[4].Noise(num * changeMapTypeFrequency, num2 * changeMapTypeFrequency);
			num3 = num3 * 0.5f + 0.5f;
			float num4 = (1f - diffValue / changeBiomaDiff) / 2f;
			if (firstType < secondType)
			{
				return (!(num3 < num4)) ? firstType : secondType;
			}
			return (!(num3 < 1f - num4)) ? secondType : firstType;
		}
		return firstType;
	}

	private static RandomMapType GetMapTypeAndTerInc(float scaledX, float scaledZ, out float[] terTypeInc)
	{
		IntVector2 worldPos = new IntVector2(Mathf.RoundToInt(scaledX / s_detailScale), Mathf.RoundToInt(scaledZ / s_detailScale));
		RandomMapType firstType;
		RandomMapType secondType;
		float diffValue;
		return GetMapTypeAndTerInc(worldPos, out firstType, out secondType, out diffValue, out terTypeInc);
	}

	private static RandomMapType GetMapTypeAndTerInc(IntVector2 worldPos, out RandomMapType firstType, out RandomMapType secondType, out float diffValue, out float[] terTypeInc)
	{
		List<RandomMapTypeDist> list = new List<RandomMapTypeDist>();
		for (int i = 0; i < BiomaDistList.Count; i++)
		{
			list.Add(new RandomMapTypeDist(BiomaDistList[i].type, BiomaDistList[i].GetDistance(worldPos)));
		}
		list.Sort();
		firstType = list[0].type;
		secondType = list[1].type;
		diffValue = list[1].distance - list[0].distance;
		List<float[]> list2 = new List<float[]>();
		List<float> list3 = new List<float>();
		for (int j = 0; j < list.Count; j++)
		{
			if (list[j].distance - list[0].distance < 64f)
			{
				list2.Add(terTypeChanceIncList[(int)(list[j].type - 1)]);
				list3.Add(list[j].distance - list[0].distance);
			}
		}
		terTypeInc = GetTerTypeInc(list2, list3);
		float num = (float)worldPos.x * s_detailScale;
		float num2 = (float)worldPos.y * s_detailScale;
		if (diffValue < changeBiomaDiff)
		{
			float num3 = (float)myBiomaNoise[4].Noise(num * changeMapTypeFrequency, num2 * changeMapTypeFrequency);
			num3 = num3 * 0.5f + 0.5f;
			float num4 = (1f - diffValue / changeBiomaDiff) / 2f;
			if (firstType < secondType)
			{
				return (!(num3 < num4)) ? firstType : secondType;
			}
			return (!(num3 < 1f - num4)) ? secondType : firstType;
		}
		return firstType;
	}

	private static float[] GetTerTypeInc(List<float[]> terTypeList, List<float> distList)
	{
		if (terTypeList.Count == 1)
		{
			return terTypeList[0];
		}
		float num = 0f;
		for (int i = 0; i < distList.Count; i++)
		{
			distList[i] = 64f - distList[i];
			num += distList[i];
		}
		float[] array = new float[5];
		for (int j = 0; j < array.Length; j++)
		{
			array[j] = 0f;
			for (int k = 0; k < terTypeList.Count; k++)
			{
				array[j] += terTypeList[k][j] * distList[k] / num;
			}
		}
		return array;
	}

	public static bool IsNoPlantType(int vType)
	{
		return vType == 45 || vType == 46 || vType == 71;
	}

	private void GeneralMineral(byte[] yVoxels, float voxelX, float[] voxelYs, float voxelZ, float mineChanceFactor, float ftertype, RandomMapType maptype)
	{
		MineChanceArrayCLS[] mineChanceArrayValues = regionArray[(int)(maptype - 1)].MineChanceArrayValues;
		float mineChance = regionArray[(int)(maptype - 1)].mineChance;
		int num = mineChanceArrayValues.Length;
		int num2 = yVoxels.Length >> 1;
		for (int i = 0; i < num; i++)
		{
			float num3 = mineChance * mineGenChanceFactor;
			int num4 = i;
			int num5 = num4 + 6;
			float num6 = (float)myMineNoise[num5].Noise((double)voxelX * minePerturbanceFrequency0, (double)voxelZ * minePerturbanceFrequency0);
			float num7 = ((!(num6 < 0f)) ? num6 : (0f - num6));
			if (!(num7 < num3))
			{
				continue;
			}
			float num8 = 1f;
			float num9 = (float)myMineNoise[num5 + num].Noise((double)voxelX * minePerturbanceFrequency1, (double)voxelZ * minePerturbanceFrequency1);
			float num10 = num9 * 100f;
			if (num9 < 0f)
			{
				num10 *= -1f;
			}
			float num11 = mineChanceArrayValues[num4].perc;
			float num12 = num11 * num8;
			if (!(num10 < num12))
			{
				continue;
			}
			float num13 = (float)myMineNoise[3].Noise(voxelX * mineHeightFrequency, voxelZ * mineHeightFrequency);
			float num14 = (float)myMineNoise[4].Noise(voxelX * mineThicknessFrequency, voxelZ * mineThicknessFrequency);
			float num15 = (float)myMineNoise[5].Noise(voxelX * mineQuantityFrequency, voxelZ * mineQuantityFrequency);
			float num16 = (num3 - num7) / num3 * (num12 - num10) / num12 * mineChanceFactor;
			int num17 = Mathf.RoundToInt((float)MineStartHeightList[num4] * PlainMax(PlainThickness) / (float)(s_seaDepth + 40) + num13 * 30f + (float)HeightOffsetTer * ftertype * ftertype);
			float num18 = 50f + 50f * num14;
			float num19 = 1f / ((num15 + 1f) * 0.3f);
			float num20 = ((!(num16 > 0.3f)) ? Mathf.Sqrt(num16 / 0.3f) : 1f);
			num18 *= num20;
			int num21 = Mathf.Clamp(Mathf.RoundToInt((float)num17 - num18 / 2f), 0, 511);
			int num22 = num21 + (int)(num18 / 2f) + 1;
			if (num22 > num2)
			{
				num22 = num2;
			}
			float num23 = num21;
			for (int num24 = num21; num24 < num22; num24 = Mathf.RoundToInt(num23))
			{
				int num25 = num24 * 2;
				if ((float)(int)yVoxels[num25] > 127.5f)
				{
					yVoxels[num25 + 1] = (byte)mineChanceArrayValues[num4].type;
				}
				num23 += num19;
			}
			break;
		}
	}

	public static void OptimiseMaxHeight(ref int maxHeight, float voxelX, float voxelZ, float flatParam, float fTerType, int nTerType, float fTerNoise, float bottomHeight, float[] fTerNoiseHeight, float[] fTerDensityClamp, float PlainThickness, int minHeight = 5)
	{
		int num = 0;
		byte b = 64;
		float num2;
		for (num2 = (float)maxHeight - bottomHeight; num2 > (float)minHeight; num2 /= 2f)
		{
			int num3 = Mathf.Clamp(Mathf.RoundToInt(bottomHeight + num2 / 2f) - 1, 0, fTerNoiseHeight.Count() - 1);
			byte volume = 0;
			byte type = 0;
			GenTileVoxelOnly(voxelX, fTerNoiseHeight[num3], voxelZ, flatParam, fTerDensityClamp[num3], fTerType, nTerType, fTerNoise, ref volume, ref type, PlainThickness);
			if (volume >= b)
			{
				bottomHeight += num2 / 2f;
			}
		}
		maxHeight = Mathf.Clamp(Mathf.RoundToInt(bottomHeight + num2), 1, s_noiseHeight - 1);
	}

	private static float GetFlatParamFromFactor(float factor, int x, int y, RandomMapType firstType = RandomMapType.GrassLand)
	{
		float num = factor * factor * factor;
		float num2 = ((!(num < 0f)) ? (flatMid + (flatMax - flatMid) * num) : (flatMin + (flatMid - flatMin) * (1f + num)));
		if (num2 > TownConnectionFlatMin)
		{
			float townConnectionFactor = GetTownConnectionFactor(x, y, 4f, -1f);
			if (townConnectionFactor < 1f)
			{
				num2 = TownConnectionFlatMin;
			}
		}
		return num2;
	}

	private static float GetThicknessParamFromFactor(float factor)
	{
		return (factor * factor * factor + 1f) * 0.5f;
	}

	private static float GetHASParamFromFactor(float factor)
	{
		factor = (factor + 1f) * 0.5f;
		if (factor >= 0.9f)
		{
			return 1f;
		}
		float f = factor / 0.9f * (float)Math.PI - (float)Math.PI / 2f;
		float num = Mathf.Sin(f);
		float num2 = num * num * num;
		if (num > 0f)
		{
			num2 = num2 * num * num;
		}
		return num2;
	}

	private static float GetSlideBarValue(float value, float mid)
	{
		float num = mid / 4f;
		float num2 = mid / 2f;
		float num3 = mid * 2f;
		float num4 = mid * 4f;
		if (value <= 25f)
		{
			return (value - 1f) / 24f * (num2 - num) + num;
		}
		if (value <= 50f)
		{
			return (value - 25f) / 25f * (mid - num2) + num2;
		}
		if (value <= 75f)
		{
			return (value - 50f) / 25f * (num3 - mid) + mid;
		}
		if (value <= 100f)
		{
			return (value - 75f) / 25f * (num4 - num3) + num3;
		}
		return value;
	}

	private static float GetSlideBarValue(float value, float min, float max)
	{
		float num = Mathf.Pow(max / min, 0.25f);
		float num2 = min * num;
		float num3 = num2 * num;
		float num4 = num3 * num;
		if (value <= 25f)
		{
			return (value - 1f) / 24f * (num2 - min) + min;
		}
		if (value <= 50f)
		{
			return (value - 25f) / 25f * (num3 - num2) + num2;
		}
		if (value <= 75f)
		{
			return (value - 50f) / 25f * (num4 - num3) + num3;
		}
		if (value <= 100f)
		{
			return (value - 75f) / 25f * (max - num4) + num4;
		}
		return value;
	}

	private static float GetSlideBarValue(float value, float mid, float min, float max)
	{
		float num = Mathf.Sqrt(mid / min);
		float num2 = Mathf.Sqrt(max / mid);
		float num3 = min * num;
		float num4 = mid * num2;
		if (value <= 25f)
		{
			return (value - 1f) / 24f * (num3 - min) + min;
		}
		if (value <= 50f)
		{
			return (value - 25f) / 25f * (mid - num3) + num3;
		}
		if (value <= 75f)
		{
			return (value - 50f) / 25f * (num4 - mid) + mid;
		}
		if (value <= 100f)
		{
			return (value - 75f) / 25f * (max - num4) + num4;
		}
		return value;
	}

	private static bool IsHillTerrain(int nTerType)
	{
		if (nTerType == 3 || nTerType == 4)
		{
			return true;
		}
		return false;
	}

	private static bool IsMountainTop(int nTerType)
	{
		if (nTerType == 4)
		{
			return true;
		}
		return false;
	}

	private static bool IsWaterTerrain(int nTerType)
	{
		if (nTerType == 0)
		{
			return true;
		}
		return false;
	}

	private static bool IsSeaSideTerrain(int nTerType)
	{
		if (nTerType == 1)
		{
			return true;
		}
		return false;
	}

	private static bool IsPlainTerrain(int nTerType)
	{
		if (nTerType == 2)
		{
			return true;
		}
		return false;
	}
}

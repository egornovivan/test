using System;
using Pathea;
using UnityEngine;
using UnityStandardAssets.ImageEffects;

public class RandomMapConfig
{
	private static RandomMapConfig mInstance;

	public static RSceneMode mSceneMode;

	public static RGameType mGameType;

	public static int cacheModeInt;

	public static RandomMapType RandomMapID = RandomMapType.GrassLand;

	public static string SeedString = string.Empty;

	public static int RandSeed = 666;

	public static RandomMapType vegetationId = RandomMapType.GrassLand;

	public static ClimateType ScenceClimate;

	public static int mapSize;

	public static int riverDensity;

	public static int riverWidth;

	public static int plainHeight = 50;

	public static int flatness = 50;

	public static int bridgeMaxHeight = 50;

	public static bool useSkillTree;

	public static bool openAllScripts;

	public static bool mirror;

	public static int rotation;

	public static int pickedLineIndex;

	public static int pickedLevelIndex;

	public int boundaryWest = -20000;

	public int boundaryEast = 20000;

	public int boundarySouth = -20000;

	public int boundaryNorth = 20000;

	public int mapRadius = 20000;

	public float boundStart = 450f;

	public int boundOffset = 150;

	public int boundChange = 500;

	public int BorderOffset = 300;

	public static int terrainHeight = 128;

	public static int allyCount = 8;

	public static RandomMapConfig Instance
	{
		get
		{
			if (mInstance == null)
			{
				mInstance = new RandomMapConfig();
			}
			return mInstance;
		}
	}

	public static int RandomMapTypeCount => Enum.GetValues(typeof(RandomMapType)).Length;

	public static int TownGenInitSeed => (int)(RandSeed + RandomMapID + mapSize + riverDensity + riverWidth + plainHeight + flatness + bridgeMaxHeight);

	public static int BiomaInitSeed => TownGenInitSeed + 1;

	public static int TownGenSeed => TownGenInitSeed + 2;

	public static int AllyGenSeed => TownGenInitSeed + 3;

	public static int MineGenSeed => RandSeed + 1;

	public int BoudaryEdgeDistance => mapRadius + BorderOffset;

	public static int TerrainHeight
	{
		get
		{
			return terrainHeight;
		}
		set
		{
			terrainHeight = value;
		}
	}

	public Vector2 MapSize => new Vector2(BoudaryEdgeDistance * 2, BoudaryEdgeDistance * 2);

	public void SetMapParam()
	{
		WeatherConfig.SetClimateType(ScenceClimate, vegetationId);
		SetTerrainHeight(terrainHeight, ScenceClimate);
		VFDataRTGen.SetRiverDensity(riverDensity);
		VFDataRTGen.SetRiverWidth(riverWidth);
		System.Random random = new System.Random(RandSeed);
		VFDataRTGen.SetPlainThickness(plainHeight);
		VFDataRTGen.SetFlatness(flatness);
		VFDataRTGen.SetFlatMin((float)random.NextDouble());
		VFDataRTGen.SetFlatMax((float)random.NextDouble());
		VFDataRTGen.SetBridgeMaxHeight(bridgeMaxHeight);
		switch (mapSize)
		{
		case 0:
			SetBoundary(-20000, 20000, -20000, 20000, 200);
			VFDataRTGen.MetalReduceSwitch = false;
			VFDataRTGen.MetalReduceArea = 4000;
			VFDataRTGen.SetMapTypeFrequency(1f);
			break;
		case 1:
			SetBoundary(-10000, 10000, -10000, 10000, 100);
			VFDataRTGen.MetalReduceSwitch = false;
			VFDataRTGen.MetalReduceArea = 2000;
			VFDataRTGen.SetMapTypeFrequency(1f);
			break;
		case 2:
			SetBoundary(-4000, 4000, -4000, 4000, 40);
			VFDataRTGen.MetalReduceSwitch = false;
			VFDataRTGen.SetMapTypeFrequency(1.5f);
			break;
		case 3:
			SetBoundary(-2000, 2000, -2000, 2000, 20);
			VFDataRTGen.MetalReduceSwitch = false;
			VFDataRTGen.SetMapTypeFrequency(3f);
			break;
		case 4:
			SetBoundary(-1000, 1000, -1000, 1000, 10);
			VFDataRTGen.MetalReduceSwitch = false;
			VFDataRTGen.SetMapTypeFrequency(3f);
			break;
		}
		Debug.Log("<color=red>SeedString:" + SeedString + "terrainHeight:" + terrainHeight + "mapsize: " + mapSize + ", riverdensity: " + riverDensity + ", riverwidth: " + riverWidth + "plainHeight:" + plainHeight + "flatness:" + flatness + "bridgemaxheight:" + bridgeMaxHeight + "allyCount" + allyCount + "</color>");
		VFVoxelWater.c_fWaterLvl = VFDataRTGen.WaterHeightBase;
		if (ScenceClimate == ClimateType.CT_Wet)
		{
			System.Random random2 = new System.Random(RandSeed);
			int num = random2.Next(50, 80);
			VFVoxelWater.c_fWaterLvl += num;
		}
		else if (ScenceClimate == ClimateType.CT_Temperate)
		{
			System.Random random3 = new System.Random(RandSeed);
			int num2 = random3.Next(5, 20);
			VFVoxelWater.c_fWaterLvl += num2;
		}
		VFDataRTGen.sceneClimate = ScenceClimate;
		VFDataRTGen.waterHeight = VFVoxelWater.c_fWaterLvl;
		VFDataRTGen.InitStaticParam(RandSeed);
		SetGlobalFogHeight(VFDataRTGen.waterHeight);
	}

	public void SetBoundary(int west, int east, int south, int north, int offset)
	{
		boundaryWest = west;
		boundaryEast = east;
		boundarySouth = south;
		boundaryNorth = north;
		mapRadius = east;
		boundOffset = offset;
	}

	public static void InitTownAreaPara()
	{
		System.Random random = new System.Random(TownGenSeed);
		mirror = random.NextDouble() >= 0.5;
		rotation = random.Next(4);
		pickedLineIndex = random.Next(TownGenData.GenerationLine.Length);
		pickedLevelIndex = random.Next(TownGenData.AreaLevel.Length);
	}

	private void SetTerrainHeight(int terrainHeight, ClimateType scenceClimate)
	{
		if (terrainHeight < 256)
		{
			TerrainHeight = 128;
			VFDataRTGen.s_noiseHeight = 128;
			VFDataRTGen.s_seaDepth = 5;
		}
		else if (terrainHeight < 512)
		{
			TerrainHeight = 256;
			VFDataRTGen.s_noiseHeight = 256;
			VFDataRTGen.s_seaDepth = 5;
		}
		else
		{
			TerrainHeight = 512;
			VFDataRTGen.s_noiseHeight = 512;
			VFDataRTGen.s_seaDepth = 128;
		}
	}

	public static int GetModeInt()
	{
		if (PeGameMgr.IsSingleAdventure)
		{
			return 1;
		}
		if (PeGameMgr.IsMultiAdventure)
		{
			return 2;
		}
		return 3;
	}

	public static void SetGlobalFogHeight(float height)
	{
		GlobalFog component = Camera.main.GetComponent<GlobalFog>();
		if (component != null)
		{
			component.height = height;
		}
	}

	public static void SetGlobalFogHeight()
	{
		GlobalFog component = Camera.main.GetComponent<GlobalFog>();
		if (component != null)
		{
			component.height = VFDataRTGen.waterHeight;
		}
	}
}

using System;
using CameraForge;
using GraphMapping;
using NovaEnv;
using Pathea;
using PETools;
using UnityEngine;
using UnityEngine.Rendering;

public static class PeEnv
{
	private static int water_opt = 0;

	private static float ControlRain = 1f;

	private static float BaseRain = 0f;

	private static float rainSwitch = 1f;

	private static float wetcoef_multiplier = 1f;

	private static float wetcoef_offset = 0f;

	private static float _nearSeaTarget = 1f;

	private static float _nearSeaCurrent = 1f;

	private static SimplexNoiseEx s_envNoise = new SimplexNoiseEx(100000017L);

	public static Executor Nova;

	public static Settings NovaSettings;

	public static Output NovaOutput;

	public static bool isRain => null != Nova && Nova.WetCoef > 0.55f;

	public static void SetControlRain(float maxValue)
	{
		ControlRain = maxValue;
	}

	public static void SetBaseRain(float baseValue)
	{
		BaseRain = baseValue;
	}

	public static void CanRain(bool canRain)
	{
		if (canRain)
		{
			rainSwitch = 1f;
		}
		else
		{
			rainSwitch = 0f;
		}
	}

	public static void AlterNearSea(bool nearSea)
	{
		_nearSeaTarget = ((!nearSea) ? 0f : 1f);
	}

	public static void Init()
	{
		water_opt = 0;
		GameObject gameObject = Resources.Load("Nova Environment") as GameObject;
		if (gameObject != null)
		{
			GameObject gameObject2 = UnityEngine.Object.Instantiate(gameObject);
			gameObject2.name = gameObject.name;
			Nova = gameObject2.GetComponent<Executor>();
			NovaSettings = Nova.Settings;
			NovaOutput = Nova.Output;
			NovaSettings.MainCamera = Camera.main;
			Nova.OnExecutorCreate += OnExecutorCreate;
			Nova.OnExecutorDestroy += OnExecutorDestroy;
			if (PeGameMgr.IsMulti)
			{
				Nova.SunSettings.Obliquity = 30f;
				Nova.SunSettings.Period = 10000000000.0;
				Nova.SunSettings.Phi = 2500000000.0;
				Nova.Settings.LocalLatitude = -37f;
			}
		}
		else
		{
			Debug.LogError("Cannot find NovaEnv prefab");
		}
	}

	public static void OnExecutorCreate()
	{
		CameraController.AfterControllerPlay += AfterCamera;
		NovaSettings.SeaHeight = VFVoxelWater.c_fWaterLvl;
		if (SystemInfo.graphicsDeviceType != GraphicsDeviceType.Direct3D9)
		{
			NovaOutput.SunlightIntensityBase = 0.001f;
		}
	}

	public static void OnExecutorDestroy()
	{
		CameraController.AfterControllerPlay -= AfterCamera;
		water_opt = 0;
		Nova = null;
		NovaSettings = null;
		NovaOutput = null;
	}

	public static void Update()
	{
		if (!(Nova != null))
		{
			return;
		}
		if (SingleGameStory.curType == SingleGameStory.StoryScene.PajaShip)
		{
			Nova.LocalTime = 0.0;
		}
		else
		{
			Nova.LocalTime = GameTime.Timer.Second;
		}
		if (PeGameMgr.IsMulti && PlayerNetwork.mainPlayer != null && PlayerNetwork.mainPlayer._curSceneId == 4)
		{
			Nova.LocalTime = 0.0;
		}
		Nova.Settings.TimeElapseSpeed = GameTime.Timer.ElapseSpeed;
		Nova.Settings.SoundVolume = SystemSettingData.Instance.SoundVolume * SystemSettingData.Instance.EffectVolume;
		Nova.WetCoef = (float)Math.Pow(Math.Max((s_envNoise.Noise(Nova.LocalDay * 1.2) + s_envNoise.Noise(Nova.LocalDay * 2.4) * 0.5 + s_envNoise.Noise(Nova.LocalDay * 4.8) * 0.25 + s_envNoise.Noise(Nova.LocalDay * 9.6) * 0.125) * 0.38 + 0.45, 0.0), 3.0);
		if (Nova.WetCoef > 0.55f)
		{
			Nova.WetCoef -= 0.55f;
			Nova.WetCoef *= 2.2f;
			Nova.WetCoef = (float)Math.Pow(Nova.WetCoef, 2.0);
			Nova.WetCoef /= 2.2f;
			Nova.WetCoef += 0.55f;
		}
		Nova.WetCoef = (Mathf.Clamp01(Nova.WetCoef) * ControlRain + BaseRain) * rainSwitch;
		Debug.DrawLine(new Vector3((float)Nova.LocalDay, Nova.WetCoef, 0f), new Vector3((float)Nova.LocalDay + 0.01f, Nova.WetCoef, 0f), Color.white, 1000f);
		bool waterDepth = SystemSettingData.Instance.WaterDepth;
		bool waterRefraction = SystemSettingData.Instance.WaterRefraction;
		int num = 0;
		num = ((waterDepth && waterRefraction) ? 3 : ((!waterDepth && !waterRefraction) ? 1 : 2));
		if (water_opt != num)
		{
			Material original = null;
			switch (num)
			{
			case 3:
				original = Resources.Load<Material>("PEWater_High");
				break;
			case 2:
				original = Resources.Load<Material>("PEWater_Medium");
				break;
			case 1:
				original = Resources.Load<Material>("PEWater_Low");
				break;
			}
			if (VFVoxelWater.self != null && PEWaveSystem.Self != null)
			{
				Material material = UnityEngine.Object.Instantiate(original);
				VFVoxelWater.self.WaterMat = material;
				NovaSettings.WaterMaterial = material;
				water_opt = num;
			}
		}
		RenderSettings.fog = !VCEditor.s_Ready;
		if (PeGameMgr.sceneMode == PeGameMgr.ESceneMode.Story)
		{
			if (PeMappingMgr.inited)
			{
				switch (PeSingleton<PeMappingMgr>.Instance.Biome)
				{
				case EBiome.Sea:
					Nova.BiomoIndex = 0;
					break;
				case EBiome.Marsh:
					Nova.BiomoIndex = 4;
					break;
				case EBiome.Jungle:
					Nova.BiomoIndex = 2;
					break;
				case EBiome.Forest:
					Nova.BiomoIndex = 1;
					break;
				case EBiome.Desert:
					Nova.BiomoIndex = 0;
					break;
				case EBiome.Canyon:
					Nova.BiomoIndex = 0;
					break;
				case EBiome.Volcano:
					Nova.BiomoIndex = 6;
					break;
				case EBiome.Grassland:
					Nova.BiomoIndex = 0;
					break;
				case EBiome.Mountainous:
					Nova.BiomoIndex = 3;
					break;
				default:
					Nova.BiomoIndex = 0;
					break;
				}
				float num2 = 1f;
				float num3 = 0f;
				switch (PeSingleton<PeMappingMgr>.Instance.Biome)
				{
				case EBiome.Desert:
					num2 = 0.3f;
					num3 = 0f;
					break;
				case EBiome.Volcano:
					num2 = 0.3f;
					num3 = 0.45f;
					break;
				default:
					num2 = 1f;
					num3 = 0f;
					break;
				}
				wetcoef_multiplier = Mathf.Lerp(wetcoef_multiplier, num2, 0.01f);
				wetcoef_offset = Mathf.Lerp(wetcoef_offset, num3, 0.01f);
				Nova.WetCoef *= wetcoef_multiplier;
				Nova.WetCoef += wetcoef_offset;
			}
			else
			{
				Nova.BiomoIndex = 0;
			}
		}
		else
		{
			Nova.BiomoIndex = 0;
		}
		if (RandomDungenMgrData.InDungeon)
		{
			Nova.BiomoIndex = 5;
		}
		else if ((PeGameMgr.IsAdventure || PeGameMgr.IsBuild) && PeSingleton<PeCreature>.Instance.mainPlayer != null)
		{
			switch (VFDataRTGen.GetXZMapType(Mathf.RoundToInt(PeSingleton<PeCreature>.Instance.mainPlayer.position.x), Mathf.RoundToInt(PeSingleton<PeCreature>.Instance.mainPlayer.position.z)))
			{
			case RandomMapType.Swamp:
				Nova.BiomoIndex = 4;
				break;
			case RandomMapType.Rainforest:
				Nova.BiomoIndex = 2;
				break;
			case RandomMapType.Forest:
				Nova.BiomoIndex = 1;
				break;
			case RandomMapType.Desert:
				Nova.BiomoIndex = 0;
				break;
			case RandomMapType.Redstone:
				Nova.BiomoIndex = 0;
				break;
			case RandomMapType.Crater:
				Nova.BiomoIndex = 6;
				break;
			case RandomMapType.GrassLand:
				Nova.BiomoIndex = 0;
				break;
			case RandomMapType.Mountain:
				Nova.BiomoIndex = 3;
				break;
			default:
				Nova.BiomoIndex = 0;
				break;
			}
		}
		if (PeGameMgr.sceneMode == PeGameMgr.ESceneMode.Story || PeGameMgr.sceneMode == PeGameMgr.ESceneMode.Custom)
		{
			switch (SystemSettingData.Instance.TerrainLevel)
			{
			case 0:
				Nova.Settings.MaxFogEndDistance = 200f;
				break;
			case 1:
				Nova.Settings.MaxFogEndDistance = 340f;
				break;
			case 2:
				Nova.Settings.MaxFogEndDistance = 700f;
				break;
			case 3:
				Nova.Settings.MaxFogEndDistance = 1300f;
				break;
			}
		}
		else
		{
			Nova.Settings.MaxFogEndDistance = 550f;
		}
		AlterNearSea(WaterReflection.ReqRefl());
		if (Mathf.Abs(_nearSeaTarget - _nearSeaCurrent) > 0.0001f)
		{
			_nearSeaCurrent = Mathf.Lerp(_nearSeaCurrent, _nearSeaTarget, 0.04f);
		}
		if (_nearSeaCurrent < 0.001f)
		{
			WaterReflection.DisableRefl();
		}
		else
		{
			WaterReflection.EnableRefl();
		}
		Nova.WaterReflectionMasterBlend = _nearSeaCurrent;
	}

	private static void AfterCamera(CameraController camc)
	{
		if (!(Nova != null))
		{
			return;
		}
		Vector3 position = camc.pose.position;
		bool flag = false;
		flag = ((!(position.x < 0f) && !(position.z < 0f)) || !PeGameMgr.IsStory) && ((position.y < 0f && position.y > -100f) || PEUtil.CheckPositionUnderWater(position));
		float num = ((!flag) ? 0f : 50f);
		if (Physics.Raycast(new Ray(camc.pose.position + Vector3.up * 200f, Vector3.down), out var hitInfo, 200f, 16) && !Physics.Raycast(new Ray(camc.pose.position, Vector3.up), out var _, 200f, 16) && (!PeGameMgr.IsMultiStory || !(PlayerNetwork.mainPlayer != null) || PlayerNetwork.mainPlayer._curSceneId == -1 || PlayerNetwork.mainPlayer._curSceneId == 0))
		{
			num = 200f - hitInfo.distance;
		}
		if (!flag && num > 2f)
		{
			num = 0f;
		}
		if (Nova.Underwater > 0f != flag)
		{
			if (!flag)
			{
				Nova.Underwater = 0f;
			}
			else
			{
				Nova.Underwater = 0.01f;
			}
			Nova.Output.Apply();
		}
		Nova.Underwater = Mathf.Lerp(Nova.Underwater, num, 0.2f);
	}
}

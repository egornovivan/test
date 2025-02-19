using System;
using UnityEngine;

namespace NovaEnv;

public class Executor : MonoBehaviour
{
	public delegate void DNotify();

	[HideInInspector]
	public Settings Settings;

	[HideInInspector]
	public Output Output;

	private Transform UniverseGroup;

	private Transform WeatherGroup;

	private Transform CloudGroup;

	[HideInInspector]
	public Texture2D NoiseTexture;

	private GameObject SkySphere;

	private GameObject FogSphere;

	private GameObject UnderwaterMask;

	[HideInInspector]
	public Material SkySphereMat;

	[HideInInspector]
	public Material FogSphereMat;

	[HideInInspector]
	public Sky Sky;

	[HideInInspector]
	public Sun Sun;

	[HideInInspector]
	public Moon[] Moons = new Moon[0];

	[HideInInspector]
	public CloudLayer SunCloudLayer;

	[HideInInspector]
	public CloudLayer RainCloudLayer;

	[HideInInspector]
	public Storm Storm;

	[HideInInspector]
	public ScreenDropsEmitter DropsEmitter;

	[HideInInspector]
	public Thunder Thunder;

	[HideInInspector]
	public WindSimulator Wind;

	public double LocalTime;

	public SunParam SunSettings = new SunParam();

	public MoonParam[] MoonsSettings = new MoonParam[0];

	public bool DoRefreshUniverse;

	public BiomoTheme[] BiomoThemes = new BiomoTheme[0];

	public WeatherTheme WeatherTheme = new WeatherTheme();

	public CaveTheme CaveTheme = new CaveTheme();

	public int BiomoIndex;

	public float WetCoef;

	public float Temperature = 20f;

	public float CaveCoef;

	public float Underwater;

	public float WaterReflectionMasterBlend = 1f;

	public double LocalDay => LocalTime / Settings.SecondsPerDay;

	public double SunTime
	{
		get
		{
			double num = Settings.LocalLongitude - Settings.LocalTimeZone * 15f;
			return num / 360.0 * Settings.SecondsPerDay + LocalTime;
		}
	}

	public double UTC => LocalTime - (double)(Settings.LocalTimeZone / 24f) * Settings.SecondsPerDay;

	public double SunDay => SunTime / Settings.SecondsPerDay;

	public double FracSunDay
	{
		get
		{
			double num = SunTime / Settings.SecondsPerDay;
			return num - Math.Floor(num);
		}
	}

	public double SunYear => (LocalTime - SunSettings.Phi) / SunSettings.Period;

	public double FracSunYear
	{
		get
		{
			double num = (LocalTime - SunSettings.Phi) / SunSettings.Period;
			return num - Math.Floor(num);
		}
	}

	public Vector3 SunDirection => Sun.Direction;

	public Vector3 Moon0Direction => Moons[0].transform.forward;

	public float UnderwaterDensity => (float)(Math.Log((double)Underwater + 1.0, Math.E) + 1.0);

	public event DNotify OnExecutorCreate;

	public event DNotify OnExecutorDestroy;

	private void Awake()
	{
		Init();
	}

	private void Start()
	{
		if (this.OnExecutorCreate != null)
		{
			this.OnExecutorCreate();
		}
	}

	private void OnDestroy()
	{
		if (this.OnExecutorDestroy != null)
		{
			this.OnExecutorDestroy();
		}
		Free();
	}

	private void Update()
	{
		UpdateEditor();
		UpdateTime();
	}

	private void LateUpdate()
	{
		UpdateTransform();
		UpdateSunAndMoons();
		UpdateCave();
		UpdateWeather();
		UpdateThemes();
	}

	public Vector3 MoonDirection(int moonindex)
	{
		return Moons[moonindex].transform.forward;
	}

	private void Init()
	{
		Settings = GetComponent<Settings>();
		Output = GetComponent<Output>();
		Output.Executor = this;
		base.gameObject.layer = LayerMask.NameToLayer(Settings.EnvironmentLayer);
		CreateObjects();
	}

	private void Free()
	{
		UnityEngine.Object.Destroy(NoiseTexture);
		UnityEngine.Object.Destroy(SkySphereMat);
		UnityEngine.Object.Destroy(UnderwaterMask);
	}

	private void CreateObjects()
	{
		CreateStructure();
		GenerateNoiseTexture();
		CreateUniverse();
		CreateWeather();
		CreateCameraEffects();
	}

	private void CreateStructure()
	{
		UniverseGroup = Utils.CreateGameObject(null, "Universe", base.transform).transform;
		WeatherGroup = Utils.CreateGameObject(null, "Weather", base.transform).transform;
		WeatherGroup.SetAsLastSibling();
	}

	private void CreateUniverse()
	{
		CreateSkySphere();
		CreateFogSphere();
		RefreshUniverse();
	}

	private void CreateWeather()
	{
		CreateWind();
		CreateCloud();
		CreateStorm();
		CreateThunder();
	}

	private void CreateCameraEffects()
	{
		CreateUnderwaterMask();
	}

	private void CreateSkySphere()
	{
		SkySphereMat = new Material(Settings.SkySphereShader);
		SkySphereMat.SetTexture("_StarNoiseTexture", NoiseTexture);
		SkySphereMat.SetTexture("_UniverseTexture", Settings.UniverseCloud);
		SkySphere = Utils.CreateGameObject(PrimitiveType.Sphere, "SkySphere", UniverseGroup);
		SkySphere.transform.localScale = Settings.SkySize * 0.99f * Vector3.one * 2f;
		SkySphere.GetComponent<Renderer>().material = SkySphereMat;
		Sky = SkySphere.AddComponent<Sky>();
		Sky.Executor = this;
	}

	private void CreateFogSphere()
	{
		FogSphereMat = new Material(Settings.FogSphereShader);
		FogSphere = Utils.CreateGameObject(PrimitiveType.Sphere, "FogSphere", SkySphere.transform);
		FogSphere.transform.localScale = Vector3.one * 0.99f;
		FogSphere.GetComponent<Renderer>().material = FogSphereMat;
	}

	private void CreateUnderwaterMask()
	{
		UnderwaterMask = Utils.CreateGameObject(Resources.Load("Underwater/UnderwaterMask") as GameObject, "UnderwaterMask", Camera.main.transform);
		UnderwaterMask.transform.localScale = Vector3.one * 5f;
		UnderwaterMask.transform.localPosition = Vector3.forward;
	}

	private void CreateWind()
	{
		Wind = Utils.CreateGameObject<WindSimulator>(null, "Wind Simulator", WeatherGroup);
		Wind.Executor = this;
	}

	private void CreateCloud()
	{
		CloudGroup = Utils.CreateGameObject(null, "Cloud Layers", WeatherGroup).transform;
		SunCloudLayer = CreateCloudLayer("Sun Cloud", 0);
		RainCloudLayer = CreateCloudLayer("Rain Cloud", -10);
	}

	private void CreateStorm()
	{
		Storm = Utils.CreateGameObject(Settings.StormPrefab, "Storm", WeatherGroup).GetComponent<Storm>();
		Storm.Executor = this;
		Storm.gameObject.SetActive(value: false);
		DropsEmitter = Utils.CreateGameObject(Settings.DropsEmitterPrefab, "Screen Drops Emitter", WeatherGroup).GetComponent<ScreenDropsEmitter>();
		DropsEmitter.Executor = this;
	}

	private void CreateThunder()
	{
		Thunder = Utils.CreateGameObject(Settings.ThunderPrefab, "Thunder", WeatherGroup).GetComponent<Thunder>();
		Thunder.Executor = this;
		Thunder.gameObject.SetActive(value: true);
	}

	private CloudLayer CreateCloudLayer(string name, int layerIndex)
	{
		CloudLayer cloudLayer = Utils.CreateGameObject(null, name, CloudGroup).AddComponent<CloudLayer>();
		cloudLayer.Executor = this;
		cloudLayer.LayerIndex = layerIndex;
		return cloudLayer;
	}

	private void CreateSun()
	{
		if (Sun == null)
		{
			Sun = Utils.CreateGameObject(null, "Sun", UniverseGroup).AddComponent<Sun>();
		}
		Sun.Executor = this;
	}

	private void CreateMoons()
	{
		Moon[] array = new Moon[MoonsSettings.Length];
		Array.Copy(Moons, array, (Moons.Length >= array.Length) ? array.Length : Moons.Length);
		for (int i = array.Length; i < Moons.Length; i++)
		{
			UnityEngine.Object.Destroy(Moons[i].gameObject);
		}
		for (int j = 0; j < array.Length; j++)
		{
			string value = MoonsSettings[j].Name;
			if (string.IsNullOrEmpty(value))
			{
				value = "Moon " + (j + 1);
			}
			if (array[j] == null)
			{
				array[j] = Utils.CreateGameObject(null, value, UniverseGroup).AddComponent<Moon>();
			}
			array[j].Index = j;
		}
		Moons = array;
		Moon[] moons = Moons;
		foreach (Moon moon in moons)
		{
			moon.Executor = this;
		}
	}

	public void RefreshUniverse()
	{
		CreateSun();
		CreateMoons();
	}

	private void GenerateNoiseTexture()
	{
		NoiseTexture = new Texture2D(256, 256, TextureFormat.ARGB32, mipmap: false);
		NoiseTexture.filterMode = FilterMode.Point;
		Color[] array = new Color[65536];
		for (int i = 0; i < 256; i++)
		{
			for (int j = 0; j < 256; j++)
			{
				int num = i * 256 + j;
				array[num].r = UnityEngine.Random.Range(0f, 1f);
				array[num].g = UnityEngine.Random.Range(0f, 1f);
				array[num].b = UnityEngine.Random.Range(0f, 1f);
				array[num].a = UnityEngine.Random.Range(0f, 1f);
			}
		}
		NoiseTexture.SetPixels(array);
		NoiseTexture.Apply();
	}

	private void UpdateEditor()
	{
		if (Application.isEditor && Application.isPlaying && DoRefreshUniverse)
		{
			DoRefreshUniverse = false;
			RefreshUniverse();
		}
	}

	private void UpdateTime()
	{
		if (Settings.ManageTimeElapse)
		{
			LocalTime += Settings.TimeElapseSpeed * (double)Time.deltaTime;
		}
	}

	private void UpdateTransform()
	{
		if (Settings.MainCamera != null)
		{
			UniverseGroup.transform.position = Settings.MainCamera.transform.position;
			WeatherGroup.transform.position = Settings.MainCamera.transform.position;
			FogSphere.transform.localPosition = Vector3.zero;
			Vector3 position = FogSphere.transform.position;
			position.y = Settings.SeaHeight;
			FogSphere.transform.position = position;
			FogSphere.transform.localScale = Vector3.one * ((!(Underwater > 0f)) ? 0.99f : 0.3f);
		}
	}

	private void UpdateSunAndMoons()
	{
		Sun.Tick();
		Moon[] moons = Moons;
		foreach (Moon moon in moons)
		{
			moon.Tick();
		}
	}

	public void UpdateCameraEffects()
	{
		UnderwaterMask.SetActive(Underwater > 0f);
	}

	private void UpdateThemes()
	{
		if (BiomoThemes.Length > 0)
		{
			BiomoIndex %= BiomoThemes.Length;
			Output.Normalize(0f);
			for (int i = 0; i < BiomoThemes.Length; i++)
			{
				BiomoThemes[i].Weight = Mathf.Lerp(BiomoThemes[i].Weight, (BiomoIndex != i) ? 0f : 1f, BiomoThemes[BiomoIndex].FadeInSpeed);
				BiomoThemes[i].Execute(this);
			}
			Output.Normalize(1f);
			WeatherTheme.Execute(this);
			CaveTheme.Execute(this);
			Output.Apply();
		}
	}

	private void UpdateWeather()
	{
		WeatherTheme.Weight = WetCoef;
		if (WetCoef > 0.5f)
		{
			Storm.gameObject.SetActive(value: true);
			Storm.Strength = WetCoef * 2f - 1f;
			Vector3 forward = Vector3.forward;
			Vector3 up = Vector3.up;
			if (Settings.MainCamera != null)
			{
				Storm.transform.position = Settings.MainCamera.transform.position;
				forward = Settings.MainCamera.transform.forward;
				forward.y = 0f;
				forward.Normalize();
			}
			Vector3 windDirection = Wind.WindDirection;
			windDirection.y = 0f;
			up -= windDirection * Storm.WindTiltCoef;
			up.Normalize();
			Storm.transform.rotation = Quaternion.LookRotation(forward, up);
		}
		else
		{
			Storm.Strength = 0f;
			if (Storm.RainDropsAudio.volume < 0.01f)
			{
				Storm.gameObject.SetActive(value: false);
			}
		}
		if (Settings.MainCamera != null)
		{
			DropsEmitter.transform.position = Settings.MainCamera.transform.position + Settings.MainCamera.transform.forward * 1.5f;
			DropsEmitter.transform.rotation = Settings.MainCamera.transform.rotation;
		}
	}

	private void UpdateCave()
	{
		CaveTheme.Weight = CaveCoef;
	}
}

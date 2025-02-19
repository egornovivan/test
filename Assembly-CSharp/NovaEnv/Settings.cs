using UnityEngine;

namespace NovaEnv;

public class Settings : MonoBehaviour
{
	public string EnvironmentLayer = "Default";

	public Camera MainCamera;

	public float SoundVolume = 1f;

	public float LocalLongitude = 106.3f;

	public float LocalLatitude = 29.3f;

	public float LocalTimeZone = 8f;

	public double TimeElapseSpeed = 1.0;

	public double SecondsPerDay = 86400.0;

	public float SeaHeight;

	public float SkySize = 3000f;

	public float CloudHeight = 600f;

	public float CloudArea = 1500f;

	public float MaxFogEndDistance = 1024f;

	public float MaxRainParticleEmissiveRate = 500f;

	public Shader SkySphereShader;

	public Shader FogSphereShader;

	public Shader CloudShader;

	public Shader MoonBodyShader;

	public Material WaterMaterial;

	public string WaterDepthColorProp;

	public string WaterReflectionColorProp;

	public string WaterFresnelColorProp;

	public string WaterSpecularColorProp;

	public string WaterFoamColorProp;

	public string WaterDepthDensityProp;

	public string WaterReflectionBlendProp;

	public string WaterSunLightDirProp;

	public GameObject CloudLayerModel;

	public GameObject StormPrefab;

	public GameObject DropsEmitterPrefab;

	public GameObject ThunderPrefab;

	public Texture2D UniverseCloud;

	public bool ManageTimeElapse = true;

	public bool MoonLightEnable;

	public LayerMask LightCullingMask = -1;
}

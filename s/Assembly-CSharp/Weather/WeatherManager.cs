using uLink;
using UnityEngine;

namespace Weather;

public class WeatherManager : UnityEngine.MonoBehaviour
{
	private const int TimeOfDay = 93600;

	private static WeatherManager mInstance;

	public float RainTimeMin = 3600f;

	public float RainTimeMax = 7200f;

	public float SunTimeMin = 5000f;

	public float SunTimeMax = 15000f;

	public float MaxRainEmission = 1000f;

	private static double mWeatherChangeTime;

	private static UniSkyWeather mNextWeather;

	[SerializeField]
	private UniSkyWeather mCurrentWeather;

	public float mRainCoefBase;

	private float clearChance = WeatherConfig.ClearChance;

	private float partlyCloudyChance = WeatherConfig.PartlyCloudyChance;

	private float mostlyCloudChance = WeatherConfig.MostlyCloudChance;

	private float sprinkleRainChance = WeatherConfig.SprinkleRainChance;

	private float TorrentialRainChance;

	public static WeatherManager Instance => mInstance;

	public UniSkyWeather CurrentWeather => mCurrentWeather;

	private void Init()
	{
		InitWeatherParm();
	}

	private void Awake()
	{
		mInstance = this;
	}

	private void Start()
	{
		Init();
		if (!uLinkNetwork.HasServerPrepared)
		{
			base.enabled = false;
		}
	}

	private void Update()
	{
		RandWeather();
	}

	private void RandWeather()
	{
		if (GameTime.Timer == null || !(GameTime.Timer.Second > mWeatherChangeTime))
		{
			return;
		}
		UniSkyWeather uniSkyWeather = mNextWeather;
		if (uniSkyWeather == UniSkyWeather.USW_Random)
		{
			int num = Random.Range(0, 100);
			if ((float)num < clearChance)
			{
				mCurrentWeather = UniSkyWeather.USW_Clear;
			}
			else if ((float)num < clearChance + partlyCloudyChance)
			{
				mCurrentWeather = UniSkyWeather.USW_PartlyCloudy;
			}
			else if ((float)num < clearChance + partlyCloudyChance + mostlyCloudChance)
			{
				mCurrentWeather = UniSkyWeather.USW_MostlyCloud;
			}
			else if ((float)num < clearChance + partlyCloudyChance + mostlyCloudChance + sprinkleRainChance)
			{
				mCurrentWeather = UniSkyWeather.USW_SprinkleRain;
			}
			else
			{
				mCurrentWeather = UniSkyWeather.USW_TorrentialRain;
			}
		}
		else
		{
			mCurrentWeather = mNextWeather;
		}
		mNextWeather = UniSkyWeather.USW_Random;
		switch (mCurrentWeather)
		{
		case UniSkyWeather.USW_SprinkleRain:
			mWeatherChangeTime = GameTime.Timer.Second + (double)Random.Range(RainTimeMin, RainTimeMax);
			break;
		case UniSkyWeather.USW_TorrentialRain:
			mWeatherChangeTime = GameTime.Timer.Second + (double)Random.Range(RainTimeMin, RainTimeMax);
			break;
		case UniSkyWeather.USW_PartlyCloudy:
			mWeatherChangeTime = GameTime.Timer.Second + (double)Random.Range(RainTimeMin, RainTimeMax);
			break;
		case UniSkyWeather.USW_MostlyCloud:
			mWeatherChangeTime = GameTime.Timer.Second + (double)Random.Range(RainTimeMin, RainTimeMax);
			break;
		default:
			mWeatherChangeTime = GameTime.Timer.Second + (double)Random.Range(SunTimeMin, SunTimeMax);
			break;
		}
		SyncWeather();
		if (LogFilter.logDebug)
		{
			Debug.Log("changeWeather: " + CurrentWeather);
		}
	}

	public void GMRandWeather()
	{
		UniSkyWeather uniSkyWeather = mNextWeather;
		if (uniSkyWeather == UniSkyWeather.USW_Random)
		{
			int num = Random.Range(0, 100);
			if ((float)num < clearChance)
			{
				mCurrentWeather = UniSkyWeather.USW_Clear;
			}
			else if ((float)num < clearChance + partlyCloudyChance)
			{
				mCurrentWeather = UniSkyWeather.USW_PartlyCloudy;
			}
			else if ((float)num < clearChance + partlyCloudyChance + mostlyCloudChance)
			{
				mCurrentWeather = UniSkyWeather.USW_MostlyCloud;
			}
			else if ((float)num < clearChance + partlyCloudyChance + mostlyCloudChance + sprinkleRainChance)
			{
				mCurrentWeather = UniSkyWeather.USW_SprinkleRain;
			}
			else
			{
				mCurrentWeather = UniSkyWeather.USW_TorrentialRain;
			}
		}
		else
		{
			mCurrentWeather = mNextWeather;
		}
		mNextWeather = UniSkyWeather.USW_Random;
		switch (mCurrentWeather)
		{
		case UniSkyWeather.USW_SprinkleRain:
			mWeatherChangeTime = GameTime.Timer.Second + (double)Random.Range(RainTimeMin, RainTimeMax);
			break;
		case UniSkyWeather.USW_TorrentialRain:
			mWeatherChangeTime = GameTime.Timer.Second + (double)Random.Range(RainTimeMin, RainTimeMax);
			break;
		case UniSkyWeather.USW_PartlyCloudy:
			mWeatherChangeTime = GameTime.Timer.Second + (double)Random.Range(RainTimeMin, RainTimeMax);
			break;
		case UniSkyWeather.USW_MostlyCloud:
			mWeatherChangeTime = GameTime.Timer.Second + (double)Random.Range(RainTimeMin, RainTimeMax);
			break;
		default:
			mWeatherChangeTime = GameTime.Timer.Second + (double)Random.Range(SunTimeMin, SunTimeMax);
			break;
		}
		SyncWeather();
		if (LogFilter.logDebug)
		{
			Debug.Log("GMRandWeather: " + CurrentWeather);
		}
	}

	public static void SyncWeather(uLink.NetworkPlayer peer)
	{
		NetworkManager.SyncPeer(peer, EPacketType.PT_Common_WeatherChange, (int)Instance.CurrentWeather);
	}

	public static void SyncWeather()
	{
		NetworkManager.SyncProxy(EPacketType.PT_Common_WeatherChange, (int)Instance.CurrentWeather);
	}

	private void InitWeatherParm()
	{
		clearChance = WeatherConfig.ClearChance;
		partlyCloudyChance = WeatherConfig.PartlyCloudyChance;
		mostlyCloudChance = WeatherConfig.MostlyCloudChance;
		sprinkleRainChance = WeatherConfig.SprinkleRainChance;
		mNextWeather = UniSkyWeather.USW_Random;
	}
}

using UnityEngine;

public class WeatherConfig
{
	public const float NO_RAIN = 0.5f;

	public const float LESS_RAIN = 1f;

	public const float FULL_RAIN = 1f;

	public const float Rain_Threshold = 0.55f;

	public const float HeavyRain_Threshold = 0.7f;

	private WeatherConfig mInstance;

	public static ClimateType climate;

	public static float ClearChance = 80f;

	public static float PartlyCloudyChance = 10f;

	public static float MostlyCloudChance = 10f;

	public static float SprinkleRainChance;

	public static float TorrentialRainChance;

	public WeatherConfig Instance
	{
		get
		{
			if (mInstance == null)
			{
				mInstance = new WeatherConfig();
			}
			return mInstance;
		}
	}

	public static bool IsRaining => PeEnv.Nova.WetCoef > 0.55f;

	public static bool IsRainingHeavily => PeEnv.Nova.WetCoef > 0.7f;

	public static void SetClimateType(ClimateType climate, RandomMapType mapType)
	{
		switch (climate)
		{
		case ClimateType.CT_Dry:
			PeEnv.SetControlRain(0.5f);
			break;
		case ClimateType.CT_Temperate:
			PeEnv.SetControlRain(1f);
			break;
		case ClimateType.CT_Wet:
			PeEnv.SetControlRain(1f);
			break;
		case ClimateType.CT_Random:
			switch ((int)Time.time % 3)
			{
			case 0:
				SetClimateType(ClimateType.CT_Dry, mapType);
				break;
			case 1:
				SetClimateType(ClimateType.CT_Temperate, mapType);
				break;
			case 2:
				SetClimateType(ClimateType.CT_Wet, mapType);
				break;
			}
			return;
		}
		if (mapType == RandomMapType.Desert)
		{
			PeEnv.SetControlRain(0.5f);
		}
		WeatherConfig.climate = climate;
		RandomMapConfig.ScenceClimate = WeatherConfig.climate;
	}
}

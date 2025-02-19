using System;

namespace Weather;

public class WeatherConfig
{
	private WeatherConfig mInstance;

	public static EClimateType climate;

	public static float ClearChance = 80f;

	public static float PartlyCloudyChance = 10f;

	public static float MostlyCloudChance = 7f;

	public static float SprinkleRainChance = 1f;

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

	public static void SetClimateType(EClimateType climate)
	{
		switch (climate)
		{
		case EClimateType.CT_Dry:
			ClearChance = 80f;
			PartlyCloudyChance = 10f;
			MostlyCloudChance = 7f;
			SprinkleRainChance = 3f;
			break;
		case EClimateType.CT_Temperate:
			ClearChance = 60f;
			PartlyCloudyChance = 14f;
			MostlyCloudChance = 12f;
			SprinkleRainChance = 8f;
			break;
		case EClimateType.CT_Wet:
			ClearChance = 60f;
			PartlyCloudyChance = 10f;
			MostlyCloudChance = 5f;
			SprinkleRainChance = 1f;
			break;
		case EClimateType.CT_Random:
			switch (DateTime.Now.Second % 3)
			{
			case 0:
				SetClimateType(EClimateType.CT_Dry);
				break;
			case 1:
				SetClimateType(EClimateType.CT_Temperate);
				break;
			case 2:
				SetClimateType(EClimateType.CT_Wet);
				break;
			}
			return;
		}
		WeatherConfig.climate = climate;
		ServerConfig.ClimateType = climate;
	}
}

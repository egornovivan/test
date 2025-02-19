using UnityEngine;

public class StatsUserPrefs
{
	public static void InitIntKey(string statName, int value = 0)
	{
		PlayerPrefs.SetInt(statName, value);
		PlayerPrefs.Save();
	}

	public static bool SaveIntValue(string statName, int value = 0)
	{
		PlayerPrefs.SetInt(statName, value);
		PlayerPrefs.Save();
		return true;
	}

	public static bool GetIntValue(string statName, out int value)
	{
		value = 0;
		if (!PlayerPrefs.HasKey(statName))
		{
			return false;
		}
		value = PlayerPrefs.GetInt(statName);
		return true;
	}
}

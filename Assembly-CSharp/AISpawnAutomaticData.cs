using UnityEngine;

public class AISpawnAutomaticData
{
	public float delayTime;

	public AISpawnAutomatic data;

	public static AISpawnAutomaticData CreateAutomaticData(float delayTime, int id)
	{
		AISpawnAutomaticData aISpawnAutomaticData = new AISpawnAutomaticData();
		aISpawnAutomaticData.delayTime = delayTime;
		aISpawnAutomaticData.data = AISpawnAutomatic.GetAutomatic(id);
		if (aISpawnAutomaticData.data == null)
		{
			Debug.LogError("Can't find Automatic from ID :" + id);
		}
		return aISpawnAutomaticData;
	}

	public AISpawnWaveData GetWaveData(int index)
	{
		if (data == null || data.data == null)
		{
			return null;
		}
		return data.data.Find((AISpawnWaveData ret) => ret != null && ret.index == index);
	}
}

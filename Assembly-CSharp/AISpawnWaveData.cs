using System;

public class AISpawnWaveData
{
	public int index;

	public float delayTime;

	public AISpawnWave data;

	public static AISpawnWaveData ToWaveData(string value, int index)
	{
		AISpawnWaveData aISpawnWaveData = new AISpawnWaveData();
		string[] array = AiUtil.Split(value, '_');
		if (array.Length == 2)
		{
			aISpawnWaveData.index = index;
			aISpawnWaveData.delayTime = Convert.ToInt32(array[0]);
			aISpawnWaveData.data = AISpawnWave.GetWaveData(Convert.ToInt32(array[1]));
		}
		return aISpawnWaveData;
	}
}

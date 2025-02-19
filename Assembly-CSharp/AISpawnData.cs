using System;

public class AISpawnData
{
	public int spID;

	public int minCount;

	public int maxCount;

	public int minAngle;

	public int maxAngle;

	public bool isPath;

	public static AISpawnData ToSpawnData(string value, bool isPath)
	{
		AISpawnData aISpawnData = new AISpawnData();
		aISpawnData.isPath = isPath;
		string[] array = AiUtil.Split(value, '_');
		if (array.Length == 5)
		{
			aISpawnData.spID = Convert.ToInt32(array[0]);
			aISpawnData.minCount = Convert.ToInt32(array[1]);
			aISpawnData.maxCount = Convert.ToInt32(array[2]);
			aISpawnData.minAngle = Convert.ToInt32(array[3]);
			aISpawnData.maxAngle = Convert.ToInt32(array[4]);
		}
		return aISpawnData;
	}
}

using UnityEngine;

public struct RandIntDb
{
	private int minTimes;

	private int maxTimes;

	public RandIntDb(int min, int max)
	{
		minTimes = min;
		maxTimes = max;
	}

	public int Random()
	{
		return UnityEngine.Random.Range(minTimes, maxTimes);
	}
}

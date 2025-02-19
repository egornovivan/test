using UnityEngine;

public struct TimeSlot
{
	public float MaxTime;

	public float MinTime;

	public TimeSlot(float _MinTime, float _MaxTime)
	{
		MaxTime = _MaxTime + Random.Range(0.1f, 0.2f);
		MinTime = _MinTime + Random.Range(0.1f, 0.2f);
	}

	public bool InTheRange(float slot)
	{
		return slot >= MinTime && slot <= MaxTime;
	}
}

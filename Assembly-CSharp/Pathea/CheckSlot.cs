namespace Pathea;

public class CheckSlot
{
	public float minTime;

	public float maxTime;

	public CheckSlot(float min, float max)
	{
		minTime = min;
		maxTime = max;
	}

	public bool InSlot(float time)
	{
		return (minTime > maxTime) ? (time > minTime || time < maxTime) : (time > minTime && time < maxTime);
	}
}

using System.Collections.Generic;

public class RandomMapTypePoint
{
	public RandomMapType type;

	public List<IntVector2> posList = new List<IntVector2>();

	public RandomMapTypePoint(RandomMapType type)
	{
		this.type = type;
	}

	public RandomMapTypePoint(RandomMapType type, IntVector2 pos)
	{
		this.type = type;
		posList.Add(pos);
	}

	public void AddPos(IntVector2 pos)
	{
		posList.Add(pos);
	}

	public float GetDistance(IntVector2 pos)
	{
		float num = float.MaxValue;
		foreach (IntVector2 pos2 in posList)
		{
			float num2 = pos2.Distance(pos);
			if (num2 < num)
			{
				num = num2;
			}
		}
		return num;
	}
}

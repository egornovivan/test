using System;

public struct RandomMapTypeDist : IComparable
{
	public RandomMapType type;

	public float distance;

	public RandomMapTypeDist(RandomMapType type, float dist)
	{
		this.type = type;
		distance = dist;
	}

	public int CompareTo(object obj)
	{
		try
		{
			RandomMapTypeDist randomMapTypeDist = (RandomMapTypeDist)obj;
			if (distance < randomMapTypeDist.distance)
			{
				return -1;
			}
			if (distance > randomMapTypeDist.distance)
			{
				return 1;
			}
			if (type < randomMapTypeDist.type)
			{
				return -1;
			}
			return 1;
		}
		catch (Exception ex)
		{
			throw new Exception(ex.Message);
		}
	}
}

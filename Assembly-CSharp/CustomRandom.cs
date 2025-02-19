using System;

public class CustomRandom
{
	private const int _MaxRandomValue = 1024;

	private float[] _Value = new float[1024];

	private int _Index;

	public float Value => _Value[_Index++ & 0x3FF];

	private CustomRandom()
	{
	}

	public CustomRandom(int seed)
	{
		Random random = new Random(seed);
		for (int i = 0; i < 1024; i++)
		{
			_Value[i] = (float)random.NextDouble();
		}
	}

	public void Reset()
	{
		_Index = 0;
	}

	public float Range(float min, float max)
	{
		return (max - min) * Value + min;
	}

	public int Range(int min, int max)
	{
		return (int)((float)(max - min) * Value + (float)min);
	}
}

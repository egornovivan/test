using System.Collections.Generic;
using UnityEngine;

public class CustomRandom
{
	private const int _MaxRandomValue = 1024;

	private static Dictionary<int, float[]> _Randoms = new Dictionary<int, float[]>();

	private float[] _Values;

	private int _Index;

	public float Value => _Values[_Index++ & 0x3FF];

	private CustomRandom()
	{
	}

	public CustomRandom(int seed)
	{
		if (!_Randoms.ContainsKey(seed))
		{
			float[] array = new float[1024];
			Random.seed = seed;
			for (int i = 0; i < 1024; i++)
			{
				array[i] = Random.value;
			}
			_Randoms.Add(seed, array);
		}
		_Values = _Randoms[seed];
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
		return Mathf.RoundToInt((float)(max - min) * Value + (float)min);
	}
}

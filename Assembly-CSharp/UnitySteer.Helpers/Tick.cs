using System;
using UnityEngine;

namespace UnitySteer.Helpers;

[Serializable]
public class Tick
{
	private float _nextTick;

	[SerializeField]
	private float _randomRangeMin;

	[SerializeField]
	private float _randomRangeMax;

	[SerializeField]
	private float _tickLapse = 0.1f;

	public float Priority;

	public float NextTick => _nextTick;

	public float RandomRangeMax
	{
		get
		{
			return _randomRangeMax;
		}
		set
		{
			_randomRangeMax = Mathf.Max(value, _randomRangeMin);
		}
	}

	public float RandomRangeMin
	{
		get
		{
			return _randomRangeMin;
		}
		set
		{
			_randomRangeMin = Mathf.Min(value, _randomRangeMax);
		}
	}

	public float TickLapse
	{
		get
		{
			return _tickLapse;
		}
		set
		{
			_tickLapse = Mathf.Max(value, 0f);
		}
	}

	public Tick()
		: this(0.1f)
	{
	}

	public Tick(float tickLapse)
	{
		TickLapse = tickLapse;
	}

	public bool ShouldTick()
	{
		return ShouldTick(TickLapse);
	}

	public bool ShouldTick(float nextTickLapseOverride)
	{
		float time = Time.time;
		bool flag = _nextTick < time;
		if (flag)
		{
			_nextTick = time + nextTickLapseOverride + UnityEngine.Random.Range(_randomRangeMin, _randomRangeMax);
		}
		return flag;
	}
}

using System;
using UnityEngine;

namespace WhiteCat;

public struct Interpolation
{
	public static float Linear(float t)
	{
		return t;
	}

	public static float Square(float t)
	{
		return (!(t < 0.5f)) ? 1f : 0f;
	}

	public static float Random(float t)
	{
		return UnityEngine.Random.Range(0f, 1f);
	}

	public static float EaseIn(float t)
	{
		return t * t;
	}

	public static float EaseOut(float t)
	{
		return t * (2f - t);
	}

	public static float EaseInEaseOut(float t)
	{
		return (3f - t - t) * t * t;
	}

	public static float EaseInStrong(float t)
	{
		return t * t * t;
	}

	public static float EaseOutStrong(float t)
	{
		t = 1f - t;
		return 1f - t * t * t;
	}

	public static float EaseInEaseOutStrong(float t)
	{
		float num = t * t;
		return (6f * num - 15f * t + 10f) * num * t;
	}

	public static float BackInEaseOut(float t)
	{
		float num = t * t;
		return (-7f * num + 12f * t - 4f) * num;
	}

	public static float EaseInBackOut(float t)
	{
		float num = t * t;
		return (7f * num - 16f * t + 10f) * num;
	}

	public static float BackInBackOut(float t)
	{
		float num = t * t;
		return ((24f * t - 60f) * num + 46f * t - 9f) * num;
	}

	public static float Triangle(float t)
	{
		return (!(t < 0.5f)) ? (2f - t - t) : (t + t);
	}

	public static float Parabolic(float t)
	{
		return 4f * t * (1f - t);
	}

	public static float Bell(float t)
	{
		float num = 1f - t;
		num *= t;
		return 16f * num * num;
	}

	public static float Sine(float t)
	{
		return Mathf.Sin((t + t + 1.5f) * (float)Math.PI) * 0.5f + 0.5f;
	}
}

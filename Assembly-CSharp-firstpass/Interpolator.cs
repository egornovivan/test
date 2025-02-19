using System;
using UnityEngine;

[Serializable]
public class Interpolator
{
	public float[][] samples;

	public Interpolator(float[][] samplePoints)
	{
		samples = samplePoints;
	}

	public static float SqrMagnitude(float[] a)
	{
		float num = 0f;
		for (int i = 0; i < a.Length; i++)
		{
			num += Mathf.Pow(a[i], 2f);
		}
		return num;
	}

	public static float Magnitude(float[] a)
	{
		return Mathf.Sqrt(SqrMagnitude(a));
	}

	public static float SqrDistance(float[] a, float[] b)
	{
		float num = 0f;
		for (int i = 0; i < a.Length; i++)
		{
			num += Mathf.Pow(a[i] - b[i], 2f);
		}
		return num;
	}

	public static float Distance(float[] a, float[] b)
	{
		return Mathf.Sqrt(SqrDistance(a, b));
	}

	public static float[] Normalized(float[] a)
	{
		return Multiply(a, 1f / Magnitude(a));
	}

	public static bool Equals(float[] a, float[] b)
	{
		return SqrDistance(a, b) == 0f;
	}

	public static float[] Multiply(float[] a, float m)
	{
		float[] array = new float[a.Length];
		for (int i = 0; i < a.Length; i++)
		{
			array[i] = a[i] * m;
		}
		return array;
	}

	public static float Dot(float[] a, float[] b)
	{
		float num = 0f;
		for (int i = 0; i < a.Length; i++)
		{
			num += a[i] * b[i];
		}
		return num;
	}

	public static float Angle(float[] a, float[] b)
	{
		float num = Magnitude(a) * Magnitude(b);
		if (num == 0f)
		{
			return 0f;
		}
		return Mathf.Acos(Mathf.Clamp(Dot(a, b) / num, -1f, 1f));
	}

	public static float ClockwiseAngle(float[] a, float[] b)
	{
		float num = Angle(a, b);
		if (a[1] * b[0] - a[0] * b[1] > 0f)
		{
			num = (float)Math.PI * 2f - num;
		}
		return num;
	}

	public static float[] Add(float[] a, float[] b)
	{
		float[] array = new float[a.Length];
		for (int i = 0; i < a.Length; i++)
		{
			array[i] = a[i] + b[i];
		}
		return array;
	}

	public float[] Subtract(float[] a, float[] b)
	{
		return Add(a, Multiply(b, -1f));
	}

	public virtual float[] Interpolate(float[] output)
	{
		return Interpolate(output, normalize: true);
	}

	public virtual float[] Interpolate(float[] output, bool normalize)
	{
		throw new NotImplementedException();
	}

	public float[] BasicChecks(float[] output)
	{
		if (samples.Length == 1)
		{
			return new float[1] { 1f };
		}
		for (int i = 0; i < samples.Length; i++)
		{
			if (Equals(output, samples[i]))
			{
				float[] array = new float[samples.Length];
				array[i] = 1f;
				return array;
			}
		}
		return null;
	}
}

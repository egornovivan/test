using System;
using UnityEngine;

public class Bezier
{
	private float tangentLengths;

	private Vector3[] points;

	private float time;

	public Bezier(float argTangentLengths = 5f)
	{
		time = 0f;
		points = new Vector3[0];
		tangentLengths = argTangentLengths;
	}

	public Bezier(Vector3[] argPoints, float argTangentLengths = 5f)
	{
		time = 0f;
		points = argPoints;
		tangentLengths = argTangentLengths;
	}

	private Vector3 CubicBezier(Vector3 p0, Vector3 p1, Vector3 p2, Vector3 p3, float t)
	{
		t = Mathf.Clamp01(t);
		float num = 1f - t;
		return num * num * num * p0 + 3f * num * num * t * p1 + 3f * num * t * t * p2 + t * t * t * p3;
	}

	public void SetLength(float length)
	{
		tangentLengths = length;
	}

	public void Insert(Vector3 v)
	{
		Array.Resize(ref points, points.Length + 1);
		points[points.Length - 1] = v;
	}

	public void Refill(Vector3[] ps)
	{
		points = ps;
	}

	public float GetTime(Vector3 pos, float speed)
	{
		float num = time;
		float num2 = time + 1f;
		while (num2 - num > 0.0001f)
		{
			float num3 = (num + num2) / 2f;
			Vector3 vector = Plot(num3);
			if ((vector - pos).sqrMagnitude > speed * Time.deltaTime * (speed * Time.deltaTime))
			{
				num2 = num3;
			}
			else
			{
				num = num3;
			}
		}
		time = (num + num2) / 2f;
		return time;
	}

	public Vector3 Plot(float t)
	{
		int num = points.Length;
		int num2 = Mathf.FloorToInt(t);
		Vector3 normalized = ((points[(num2 + 1) % num] - points[num2 % num]).normalized - (points[(num2 - 1 + num) % num] - points[num2 % num]).normalized).normalized;
		Vector3 normalized2 = ((points[(num2 + 2) % num] - points[(num2 + 1) % num]).normalized - (points[(num2 + num) % num] - points[(num2 + 1) % num]).normalized).normalized;
		return CubicBezier(points[num2 % num], points[num2 % num] + normalized * tangentLengths, points[(num2 + 1) % num] - normalized2 * tangentLengths, points[(num2 + 1) % num], t - (float)num2);
	}

	public void OnDrawGizmos()
	{
		if (points.Length < 3)
		{
			return;
		}
		for (int i = 0; i < points.Length; i++)
		{
			int num = points.Length;
			Vector3 normalized = ((points[(i + 1) % num] - points[i]).normalized - (points[(i - 1 + num) % num] - points[i]).normalized).normalized;
			Vector3 normalized2 = ((points[(i + 2) % num] - points[(i + 1) % num]).normalized - (points[(i + num) % num] - points[(i + 1) % num]).normalized).normalized;
			Vector3 from = points[i];
			for (int j = 1; j <= 100; j++)
			{
				Vector3 vector = CubicBezier(points[i], points[i] + normalized * tangentLengths, points[(i + 1) % num] - normalized2 * tangentLengths, points[(i + 1) % num], (float)j / 100f);
				Gizmos.DrawLine(from, vector);
				from = vector;
			}
		}
	}
}

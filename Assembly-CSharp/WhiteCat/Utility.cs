using System;
using System.Collections;
using UnityEngine;

namespace WhiteCat;

public struct Utility
{
	public const float sqrt2 = 1.414214f;

	public const float sqrt3 = 1.732051f;

	public static readonly Matrix4x4 identityMatrix = Matrix4x4.identity;

	public static void Swap<T>(ref T a, ref T b)
	{
		T val = a;
		a = b;
		b = val;
	}

	public static bool IsNullOrEmpty(ICollection collection)
	{
		return collection == null || collection.Count == 0;
	}

	public static float Interpolate(float from, float to, float t, Func<float, float> interpolate)
	{
		return interpolate(Mathf.Clamp01(t)) * (to - from) + from;
	}

	public static Vector3 Interpolate(Vector3 from, Vector3 to, float t, Func<float, float> interpolate)
	{
		return interpolate(Mathf.Clamp01(t)) * (to - from) + from;
	}

	public static float InterpolateInThreePhases(float t, float t1, float t2, Func<float, float> interpolate)
	{
		if (t2 < t1)
		{
			Swap(ref t1, ref t2);
		}
		if (t < t1)
		{
			return interpolate(t / t1 * 0.5f);
		}
		if (t > t2)
		{
			return interpolate((t - t2) / (1f - t2) * 0.5f + 0.5f);
		}
		return interpolate(0.5f);
	}

	public static float CardinalSpline(float p0, float p1, float p2, float p3, float t, float tension = 0.5f)
	{
		return p1 + (p2 - p0) * tension * t + ((p2 - p1) * 3f - (p3 - p1) * tension - (p2 - p0) * tension * 2f) * t * t + ((p3 - p1) * tension - (p2 - p1) * 2f + (p2 - p0) * tension) * t * t * t;
	}

	public static void SetTimeScaleAndFixedFrequency(float timeScale, float fixedFrequency)
	{
		Time.timeScale = timeScale;
		Time.fixedDeltaTime = timeScale / fixedFrequency;
	}

	public static Color IntRGBAToColor(int rgba)
	{
		return new Color((float)(rgba >> 24) / 255f, (float)((rgba >> 16) & 0xFF) / 255f, (float)((rgba >> 8) & 0xFF) / 255f, (float)(rgba & 0xFF) / 255f);
	}

	public static Color IntRGBToColor(int rgb)
	{
		return new Color((float)((rgb >> 16) & 0xFF) / 255f, (float)((rgb >> 8) & 0xFF) / 255f, (float)(rgb & 0xFF) / 255f);
	}

	public static Vector3 ClosestPoint(Vector3 start, Vector3 end, Vector3 point)
	{
		Vector3 vector = end - start;
		float sqrMagnitude = vector.sqrMagnitude;
		if (sqrMagnitude < 1E-06f)
		{
			return start;
		}
		sqrMagnitude = Mathf.Clamp01(Vector3.Dot(point - start, vector) / sqrMagnitude);
		return start + vector * sqrMagnitude;
	}

	public static void ClosestPoint(Vector3 startA, Vector3 endA, Vector3 startB, Vector3 endB, out Vector3 pointA, out Vector3 pointB)
	{
		Vector3 vector = endA - startA;
		Vector3 vector2 = endB - startB;
		float num = Vector3.Dot(vector, vector2);
		float sqrMagnitude = vector.sqrMagnitude;
		float num2 = Vector3.Dot(startA - startB, vector);
		float sqrMagnitude2 = vector2.sqrMagnitude;
		float num3 = Vector3.Dot(startA - startB, vector2);
		float num4 = sqrMagnitude2 * sqrMagnitude - num * num;
		float num5 = (num * num3 - sqrMagnitude2 * num2) / num4;
		float num6 = (sqrMagnitude * num3 - num * num2) / num4;
		if (float.IsNaN(num5) || float.IsNaN(num6))
		{
			pointB = ClosestPoint(startB, endB, startA);
			pointA = ClosestPoint(startB, endB, endA);
			if ((pointB - startA).sqrMagnitude < (pointA - endA).sqrMagnitude)
			{
				pointA = startA;
				return;
			}
			pointB = pointA;
			pointA = endA;
		}
		else if (num5 < 0f)
		{
			if (num6 < 0f)
			{
				pointA = ClosestPoint(startA, endA, startB);
				pointB = ClosestPoint(startB, endB, startA);
				if ((pointA - startB).sqrMagnitude < (pointB - startA).sqrMagnitude)
				{
					pointB = startB;
				}
				else
				{
					pointA = startA;
				}
			}
			else if (num6 > 1f)
			{
				pointA = ClosestPoint(startA, endA, endB);
				pointB = ClosestPoint(startB, endB, startA);
				if ((pointA - endB).sqrMagnitude < (pointB - startA).sqrMagnitude)
				{
					pointB = endB;
				}
				else
				{
					pointA = startA;
				}
			}
			else
			{
				pointA = startA;
				pointB = ClosestPoint(startB, endB, startA);
			}
		}
		else if (num5 > 1f)
		{
			if (num6 < 0f)
			{
				pointA = ClosestPoint(startA, endA, startB);
				pointB = ClosestPoint(startB, endB, endA);
				if ((pointA - startB).sqrMagnitude < (pointB - endA).sqrMagnitude)
				{
					pointB = startB;
				}
				else
				{
					pointA = endA;
				}
			}
			else if (num6 > 1f)
			{
				pointA = ClosestPoint(startA, endA, endB);
				pointB = ClosestPoint(startB, endB, endA);
				if ((pointA - endB).sqrMagnitude < (pointB - endA).sqrMagnitude)
				{
					pointB = endB;
				}
				else
				{
					pointA = endA;
				}
			}
			else
			{
				pointA = endA;
				pointB = ClosestPoint(startB, endB, endA);
			}
		}
		else if (num6 < 0f)
		{
			pointB = startB;
			pointA = ClosestPoint(startA, endA, startB);
		}
		else if (num6 > 1f)
		{
			pointB = endB;
			pointA = ClosestPoint(startA, endA, endB);
		}
		else
		{
			pointA = startA + num5 * vector;
			pointB = startB + num6 * vector2;
		}
	}

	public static Vector3 GetPositionOfMatrix(ref Matrix4x4 matrix)
	{
		return new Vector3(matrix.m03, matrix.m13, matrix.m23);
	}

	public static Quaternion GetRotationOfMatrix(ref Matrix4x4 matrix)
	{
		return Quaternion.LookRotation(new Vector3(matrix.m02, matrix.m12, matrix.m22), new Vector3(matrix.m01, matrix.m11, matrix.m21));
	}

	public static Vector3 GetScaleOfMatrix(ref Matrix4x4 matrix)
	{
		return new Vector3(new Vector4(matrix.m00, matrix.m10, matrix.m20, matrix.m30).magnitude, new Vector4(matrix.m01, matrix.m11, matrix.m21, matrix.m31).magnitude, new Vector4(matrix.m02, matrix.m12, matrix.m22, matrix.m32).magnitude);
	}
}

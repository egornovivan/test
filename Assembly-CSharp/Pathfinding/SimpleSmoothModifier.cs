using System;
using System.Collections.Generic;
using Pathfinding.Util;
using UnityEngine;

namespace Pathfinding;

[Serializable]
[AddComponentMenu("Pathfinding/Modifiers/Simple Smooth")]
public class SimpleSmoothModifier : MonoModifier
{
	public enum SmoothType
	{
		Simple,
		Bezier,
		OffsetSimple,
		CurvedNonuniform
	}

	public SmoothType smoothType;

	public int subdivisions = 2;

	public int iterations = 2;

	public float strength = 0.5f;

	public bool uniformLength = true;

	public float maxSegmentLength = 2f;

	public float bezierTangentLength = 0.4f;

	public float offset = 0.2f;

	public float factor = 0.1f;

	public override ModifierData input => ModifierData.All;

	public override ModifierData output
	{
		get
		{
			ModifierData modifierData = ModifierData.VectorPath;
			if (iterations == 0 && smoothType == SmoothType.Simple && !uniformLength)
			{
				modifierData |= ModifierData.StrictVectorPath;
			}
			return modifierData;
		}
	}

	public override void Apply(Path p, ModifierData source)
	{
		if (p.vectorPath == null)
		{
			Debug.LogWarning("Can't process NULL path (has another modifier logged an error?)");
			return;
		}
		List<Vector3> list = null;
		switch (smoothType)
		{
		case SmoothType.Simple:
			list = SmoothSimple(p.vectorPath);
			break;
		case SmoothType.Bezier:
			list = SmoothBezier(p.vectorPath);
			break;
		case SmoothType.OffsetSimple:
			list = SmoothOffsetSimple(p.vectorPath);
			break;
		case SmoothType.CurvedNonuniform:
			list = CurvedNonuniform(p.vectorPath);
			break;
		}
		if (list != p.vectorPath)
		{
			ListPool<Vector3>.Release(p.vectorPath);
			p.vectorPath = list;
		}
	}

	public List<Vector3> CurvedNonuniform(List<Vector3> path)
	{
		if (maxSegmentLength <= 0f)
		{
			Debug.LogWarning("Max Segment Length is <= 0 which would cause DivByZero-exception or other nasty errors (avoid this)");
			return path;
		}
		int num = 0;
		for (int i = 0; i < path.Count - 1; i++)
		{
			float magnitude = (path[i] - path[i + 1]).magnitude;
			for (float num2 = 0f; num2 <= magnitude; num2 += maxSegmentLength)
			{
				num++;
			}
		}
		List<Vector3> list = ListPool<Vector3>.Claim(num);
		Vector3 vector = (path[1] - path[0]).normalized;
		for (int j = 0; j < path.Count - 1; j++)
		{
			float magnitude2 = (path[j] - path[j + 1]).magnitude;
			Vector3 vector2 = vector;
			Vector3 vector3 = ((j >= path.Count - 2) ? (path[j + 1] - path[j]).normalized : ((path[j + 2] - path[j + 1]).normalized - (path[j] - path[j + 1]).normalized).normalized);
			Vector3 tan = vector2 * magnitude2 * factor;
			Vector3 tan2 = vector3 * magnitude2 * factor;
			Vector3 a = path[j];
			Vector3 b = path[j + 1];
			float num3 = 1f / magnitude2;
			for (float num4 = 0f; num4 <= magnitude2; num4 += maxSegmentLength)
			{
				float t = num4 * num3;
				list.Add(GetPointOnCubic(a, b, tan, tan2, t));
			}
			vector = vector3;
		}
		list[list.Count - 1] = path[path.Count - 1];
		return list;
	}

	public static Vector3 GetPointOnCubic(Vector3 a, Vector3 b, Vector3 tan1, Vector3 tan2, float t)
	{
		float num = t * t;
		float num2 = num * t;
		float num3 = 2f * num2 - 3f * num + 1f;
		float num4 = -2f * num2 + 3f * num;
		float num5 = num2 - 2f * num + t;
		float num6 = num2 - num;
		return num3 * a + num4 * b + num5 * tan1 + num6 * tan2;
	}

	public List<Vector3> SmoothOffsetSimple(List<Vector3> path)
	{
		if (path.Count <= 2 || iterations <= 0)
		{
			return path;
		}
		if (iterations > 12)
		{
			Debug.LogWarning("A very high iteration count was passed, won't let this one through");
			return path;
		}
		int num = (path.Count - 2) * (int)Mathf.Pow(2f, iterations) + 2;
		List<Vector3> list = ListPool<Vector3>.Claim(num);
		List<Vector3> list2 = ListPool<Vector3>.Claim(num);
		for (int i = 0; i < num; i++)
		{
			list.Add(Vector3.zero);
			list2.Add(Vector3.zero);
		}
		for (int j = 0; j < path.Count; j++)
		{
			list[j] = path[j];
		}
		for (int k = 0; k < iterations; k++)
		{
			int num2 = (path.Count - 2) * (int)Mathf.Pow(2f, k) + 2;
			List<Vector3> list3 = list;
			list = list2;
			list2 = list3;
			float num3 = 1f;
			for (int l = 0; l < num2 - 1; l++)
			{
				Vector3 vector = list2[l];
				Vector3 vector2 = list2[l + 1];
				Vector3 normalized = Vector3.Cross(vector2 - vector, Vector3.up).normalized;
				bool flag = false;
				bool flag2 = false;
				bool flag3 = false;
				bool flag4 = false;
				if (l != 0 && !Polygon.IsColinear(vector, vector2, list2[l - 1]))
				{
					flag3 = true;
					flag = Polygon.Left(vector, vector2, list2[l - 1]);
				}
				if (l < num2 - 1 && !Polygon.IsColinear(vector, vector2, list2[l + 2]))
				{
					flag4 = true;
					flag2 = Polygon.Left(vector, vector2, list2[l + 2]);
				}
				if (flag3)
				{
					list[l * 2] = vector + ((!flag) ? (-normalized * offset * num3) : (normalized * offset * num3));
				}
				else
				{
					list[l * 2] = vector;
				}
				if (flag4)
				{
					list[l * 2 + 1] = vector2 + ((!flag2) ? (-normalized * offset * num3) : (normalized * offset * num3));
				}
				else
				{
					list[l * 2 + 1] = vector2;
				}
			}
			list[(path.Count - 2) * (int)Mathf.Pow(2f, k + 1) + 2 - 1] = list2[num2 - 1];
		}
		ListPool<Vector3>.Release(list2);
		return list;
	}

	public List<Vector3> SmoothSimple(List<Vector3> path)
	{
		if (path.Count < 2)
		{
			return path;
		}
		if (uniformLength)
		{
			int num = 0;
			maxSegmentLength = ((!(maxSegmentLength < 0.005f)) ? maxSegmentLength : 0.005f);
			for (int i = 0; i < path.Count - 1; i++)
			{
				float num2 = Vector3.Distance(path[i], path[i + 1]);
				num += Mathf.FloorToInt(num2 / maxSegmentLength);
			}
			List<Vector3> list = ListPool<Vector3>.Claim(num + 1);
			int num3 = 0;
			float num4 = 0f;
			for (int j = 0; j < path.Count - 1; j++)
			{
				float num5 = Vector3.Distance(path[j], path[j + 1]);
				int num6 = Mathf.FloorToInt((num5 + num4) / maxSegmentLength);
				float num7 = num4 / num5;
				Vector3 vector = path[j + 1] - path[j];
				for (int k = 0; k < num6; k++)
				{
					list.Add(vector * Math.Max(0f, (float)k / (float)num6 - num7) + path[j]);
					num3++;
				}
				num4 = (num5 + num4) % maxSegmentLength;
			}
			list.Add(path[path.Count - 1]);
			if (strength != 0f)
			{
				for (int l = 0; l < iterations; l++)
				{
					Vector3 vector2 = list[0];
					for (int m = 1; m < list.Count - 1; m++)
					{
						Vector3 vector3 = list[m];
						list[m] = Vector3.Lerp(vector3, (vector2 + list[m + 1]) / 2f, strength);
						vector2 = vector3;
					}
				}
			}
			return list;
		}
		List<Vector3> list2 = ListPool<Vector3>.Claim();
		if (subdivisions < 0)
		{
			subdivisions = 0;
		}
		int num8 = 1 << subdivisions;
		for (int n = 0; n < path.Count - 1; n++)
		{
			for (int num9 = 0; num9 < num8; num9++)
			{
				list2.Add(Vector3.Lerp(path[n], path[n + 1], (float)num9 / (float)num8));
			}
		}
		for (int num10 = 0; num10 < iterations; num10++)
		{
			Vector3 vector4 = list2[0];
			for (int num11 = 1; num11 < list2.Count - 1; num11++)
			{
				Vector3 vector5 = list2[num11];
				list2[num11] = Vector3.Lerp(vector5, (vector4 + list2[num11 + 1]) / 2f, strength);
				vector4 = vector5;
			}
		}
		return list2;
	}

	public List<Vector3> SmoothBezier(List<Vector3> path)
	{
		if (subdivisions < 0)
		{
			subdivisions = 0;
		}
		int num = 1 << subdivisions;
		List<Vector3> list = ListPool<Vector3>.Claim();
		for (int i = 0; i < path.Count - 1; i++)
		{
			Vector3 zero = Vector3.zero;
			Vector3 zero2 = Vector3.zero;
			zero = ((i != 0) ? (path[i + 1] - path[i - 1]) : (path[i + 1] - path[i]));
			zero2 = ((i != path.Count - 2) ? (path[i] - path[i + 2]) : (path[i] - path[i + 1]));
			zero *= bezierTangentLength;
			zero2 *= bezierTangentLength;
			Vector3 vector = path[i];
			Vector3 p = vector + zero;
			Vector3 vector2 = path[i + 1];
			Vector3 p2 = vector2 + zero2;
			for (int j = 0; j < num; j++)
			{
				list.Add(AstarMath.CubicBezier(vector, p, p2, vector2, (float)j / (float)num));
			}
		}
		list.Add(path[path.Count - 1]);
		return list;
	}
}

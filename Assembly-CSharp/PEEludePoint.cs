using System.Collections.Generic;
using PETools;
using UnityEngine;

public class PEEludePoint : MonoBehaviour
{
	private static List<EludePoint> s_Points = new List<EludePoint>();

	public Transform[] points;

	public static EludePoint GetEludePoint(Vector3 pos, Vector3 targetPos)
	{
		EludePoint result = null;
		float num = 16384f;
		for (int i = 0; i < s_Points.Count; i++)
		{
			if (!s_Points[i].Dirty && s_Points[i].CanElude(targetPos))
			{
				float num2 = PEUtil.SqrMagnitudeH(pos, s_Points[i].Position);
				if (num2 < num)
				{
					num = num2;
					result = s_Points[i];
				}
			}
		}
		return result;
	}

	public static void RegisterPoint(EludePoint point)
	{
		if (!s_Points.Contains(point))
		{
			s_Points.Add(point);
		}
	}

	public static void RemovePoint(EludePoint point)
	{
		if (s_Points.Contains(point))
		{
			s_Points.Remove(point);
		}
	}

	private void Awake()
	{
		for (int i = 0; i < points.Length; i++)
		{
			if (points[i] != null)
			{
				RegisterPoint(new EludePoint(points[i].position, points[i].forward, points[i].position - base.transform.position));
			}
		}
	}
}

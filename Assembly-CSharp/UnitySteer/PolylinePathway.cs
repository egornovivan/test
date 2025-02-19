using System;
using UnityEngine;

namespace UnitySteer;

public class PolylinePathway : Pathway
{
	private int pointCount;

	private Vector3[] points;

	private float radius;

	private float segmentLength;

	private float segmentProjection;

	private Vector3 chosen;

	private Vector3 segmentNormal;

	private float[] lengths;

	private Vector3[] normals;

	private float totalPathLength;

	private PolylinePathway()
	{
	}

	private PolylinePathway(Vector3[] _points, float _radius, bool _cyclic)
	{
		initialize(_points, _radius, _cyclic);
	}

	private void initialize(Vector3[] _points, float _radius, bool _cyclic)
	{
		radius = _radius;
		isCyclic = _cyclic;
		pointCount = _points.Length;
		totalPathLength = 0f;
		if (isCyclic)
		{
			pointCount++;
		}
		lengths = new float[pointCount];
		points = new Vector3[pointCount];
		normals = new Vector3[pointCount];
		for (int i = 0; i < _points.Length; i++)
		{
			int num = ((!isCyclic || i != pointCount - 1) ? i : 0);
			ref Vector3 reference = ref points[i];
			reference = _points[num];
			if (i > 0)
			{
				ref Vector3 reference2 = ref normals[i];
				reference2 = points[i] - points[i - 1];
				lengths[i] = normals[i].magnitude;
				normals[i] *= 1f / lengths[i];
				totalPathLength += lengths[i];
			}
		}
	}

	protected override float GetTotalPathLength()
	{
		return totalPathLength;
	}

	public override Vector3 mapPointToPath(Vector3 point, ref mapReturnStruct tStruct)
	{
		float num = float.MaxValue;
		Vector3 zero = Vector3.zero;
		for (int i = 1; i < pointCount; i++)
		{
			segmentLength = lengths[i];
			segmentNormal = normals[i];
			float num2 = pointToSegmentDistance(point, points[i - 1], points[i]);
			if (num2 < num)
			{
				num = num2;
				zero = chosen;
				tStruct.tangent = segmentNormal;
			}
		}
		tStruct.outside = (zero - point).magnitude - radius;
		return zero;
	}

	public override float mapPointToPathDistance(Vector3 point)
	{
		float num = float.MaxValue;
		float num2 = 0f;
		float result = 0f;
		for (int i = 1; i < pointCount; i++)
		{
			segmentLength = lengths[i];
			segmentNormal = normals[i];
			float num3 = pointToSegmentDistance(point, points[i - 1], points[i]);
			if (num3 < num)
			{
				num = num3;
				result = num2 + segmentProjection;
			}
			num2 += segmentLength;
		}
		return result;
	}

	public override Vector3 mapPathDistanceToPoint(float pathDistance)
	{
		float num = pathDistance;
		if (isCyclic)
		{
			num = (float)Math.IEEERemainder(pathDistance, totalPathLength);
		}
		else
		{
			if (pathDistance < 0f)
			{
				return points[0];
			}
			if (pathDistance >= totalPathLength)
			{
				return points[pointCount - 1];
			}
		}
		Vector3 result = Vector3.zero;
		for (int i = 1; i < pointCount; i++)
		{
			segmentLength = lengths[i];
			if (segmentLength < num)
			{
				num -= segmentLength;
				continue;
			}
			float t = num / segmentLength;
			result = Vector3.Lerp(points[i - 1], points[i], t);
			break;
		}
		return result;
	}

	private float pointToSegmentDistance(Vector3 point, Vector3 ep0, Vector3 ep1)
	{
		Vector3 rhs = point - ep0;
		segmentProjection = Vector3.Dot(segmentNormal, rhs);
		if (segmentProjection < 0f)
		{
			chosen = ep0;
			segmentProjection = 0f;
			return (point - ep0).magnitude;
		}
		if (segmentProjection > segmentLength)
		{
			chosen = ep1;
			segmentProjection = segmentLength;
			return (point - ep1).magnitude;
		}
		chosen = segmentNormal * segmentProjection;
		chosen += ep0;
		return Vector3.Distance(point, chosen);
	}
}

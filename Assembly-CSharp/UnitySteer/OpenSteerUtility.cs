using System;
using UnityEngine;

namespace UnitySteer;

public class OpenSteerUtility
{
	public static Vector3 RandomUnitVectorOnXZPlane()
	{
		Vector3 insideUnitSphere = UnityEngine.Random.insideUnitSphere;
		insideUnitSphere.y = 0f;
		insideUnitSphere.Normalize();
		return insideUnitSphere;
	}

	public static Vector3 limitMaxDeviationAngle(Vector3 source, float cosineOfConeAngle, Vector3 basis)
	{
		return vecLimitDeviationAngleUtility(insideOrOutside: true, source, cosineOfConeAngle, basis);
	}

	public static Vector3 vecLimitDeviationAngleUtility(bool insideOrOutside, Vector3 source, float cosineOfConeAngle, Vector3 basis)
	{
		float magnitude = source.magnitude;
		if (magnitude == 0f)
		{
			return source;
		}
		Vector3 lhs = source / magnitude;
		float num = Vector3.Dot(lhs, basis);
		if (insideOrOutside)
		{
			if (num >= cosineOfConeAngle)
			{
				return source;
			}
		}
		else if (num <= cosineOfConeAngle)
		{
			return source;
		}
		Vector3 vector = perpendicularComponent(source, basis);
		float num2 = (float)Math.Sqrt(1f - cosineOfConeAngle * cosineOfConeAngle);
		Vector3 vector2 = basis * cosineOfConeAngle;
		Vector3 vector3 = vector.normalized * num2;
		return (vector2 + vector3) * magnitude;
	}

	public static Vector3 parallelComponent(Vector3 source, Vector3 unitBasis)
	{
		float num = Vector3.Dot(source, unitBasis);
		return unitBasis * num;
	}

	public static Vector3 perpendicularComponent(Vector3 source, Vector3 unitBasis)
	{
		return source - parallelComponent(source, unitBasis);
	}

	public static Vector3 blendIntoAccumulator(float smoothRate, Vector3 newValue, Vector3 smoothedAccumulator)
	{
		return Vector3.Lerp(smoothedAccumulator, newValue, Mathf.Clamp(smoothRate, 0f, 1f));
	}

	public static float blendIntoAccumulator(float smoothRate, float newValue, float smoothedAccumulator)
	{
		return Mathf.Lerp(smoothedAccumulator, newValue, Mathf.Clamp(smoothRate, 0f, 1f));
	}

	public static Vector3 sphericalWrapAround(Vector3 source, Vector3 center, float radius)
	{
		Vector3 vector = source - center;
		float magnitude = vector.magnitude;
		if (magnitude > radius)
		{
			return source + vector / magnitude * radius * -2f;
		}
		return source;
	}

	public static float scalarRandomWalk(float initial, float walkspeed, float min, float max)
	{
		float value = initial + (UnityEngine.Random.value * 2f - 1f) * walkspeed;
		return Mathf.Clamp(value, min, max);
	}

	public static int intervalComparison(float x, float lowerBound, float upperBound)
	{
		if (x < lowerBound)
		{
			return -1;
		}
		if (x > upperBound)
		{
			return 1;
		}
		return 0;
	}

	public static float PointToSegmentDistance(Vector3 point, Vector3 ep0, Vector3 ep1, ref float segmentProjection)
	{
		Vector3 chosenPoint = Vector3.zero;
		return PointToSegmentDistance(point, ep0, ep1, ref chosenPoint, ref segmentProjection);
	}

	public static float PointToSegmentDistance(Vector3 point, Vector3 ep0, Vector3 ep1, ref Vector3 chosenPoint)
	{
		float segmentProjection = 0f;
		return PointToSegmentDistance(point, ep0, ep1, ref chosenPoint, ref segmentProjection);
	}

	public static float PointToSegmentDistance(Vector3 point, Vector3 ep0, Vector3 ep1, ref Vector3 chosenPoint, ref float segmentProjection)
	{
		Vector3 segmentNormal = ep1 - ep0;
		float magnitude = segmentNormal.magnitude;
		segmentNormal *= 1f / magnitude;
		return PointToSegmentDistance(point, ep0, ep1, segmentNormal, magnitude, ref chosenPoint, ref segmentProjection);
	}

	public static float PointToSegmentDistance(Vector3 point, Vector3 ep0, Vector3 ep1, Vector3 segmentNormal, float segmentLength, ref float segmentProjection)
	{
		Vector3 chosenPoint = Vector3.zero;
		return PointToSegmentDistance(point, ep0, ep1, segmentNormal, segmentLength, ref chosenPoint, ref segmentProjection);
	}

	public static float PointToSegmentDistance(Vector3 point, Vector3 ep0, Vector3 ep1, Vector3 segmentNormal, float segmentLength, ref Vector3 chosenPoint)
	{
		float segmentProjection = 0f;
		return PointToSegmentDistance(point, ep0, ep1, segmentNormal, segmentLength, ref chosenPoint, ref segmentProjection);
	}

	public static float PointToSegmentDistance(Vector3 point, Vector3 ep0, Vector3 ep1, Vector3 segmentNormal, float segmentLength, ref Vector3 chosenPoint, ref float segmentProjection)
	{
		Vector3 rhs = point - ep0;
		segmentProjection = Vector3.Dot(segmentNormal, rhs);
		if (segmentProjection < 0f)
		{
			chosenPoint = ep0;
			segmentProjection = 0f;
			return (point - ep0).magnitude;
		}
		if (segmentProjection > segmentLength)
		{
			chosenPoint = ep1;
			segmentProjection = segmentLength;
			return (point - ep1).magnitude;
		}
		chosenPoint = segmentNormal * segmentProjection;
		chosenPoint += ep0;
		return Vector3.Distance(point, chosenPoint);
	}

	public static float CosFromDegrees(float angle)
	{
		return Mathf.Cos(angle * ((float)Math.PI / 180f));
	}

	public static float DegreesFromCos(float cos)
	{
		return 57.29578f * Mathf.Acos(cos);
	}
}

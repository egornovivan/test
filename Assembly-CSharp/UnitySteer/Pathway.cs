using UnityEngine;

namespace UnitySteer;

public class Pathway
{
	protected bool isCyclic;

	public bool IsCyclic => isCyclic;

	public float TotalPathLength => GetTotalPathLength();

	public Vector3 FirstPoint => GetFirstPoint();

	public Vector3 LastPoint => GetLastPoint();

	protected virtual float GetTotalPathLength()
	{
		return 0f;
	}

	protected virtual Vector3 GetFirstPoint()
	{
		return Vector3.zero;
	}

	protected virtual Vector3 GetLastPoint()
	{
		return Vector3.zero;
	}

	public virtual Vector3 mapPointToPath(Vector3 point, ref mapReturnStruct tStruct)
	{
		return Vector3.zero;
	}

	public virtual Vector3 mapPathDistanceToPoint(float pathDistance)
	{
		return Vector3.zero;
	}

	public virtual float mapPointToPathDistance(Vector3 point)
	{
		return 0f;
	}

	public bool isInsidePath(Vector3 point)
	{
		mapReturnStruct tStruct = default(mapReturnStruct);
		mapPointToPath(point, ref tStruct);
		return tStruct.outside < 0f;
	}

	public float howFarOutsidePath(Vector3 point)
	{
		mapReturnStruct tStruct = default(mapReturnStruct);
		mapPointToPath(point, ref tStruct);
		return tStruct.outside;
	}
}

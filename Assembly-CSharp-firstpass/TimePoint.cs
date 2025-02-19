using UnityEngine;

public struct TimePoint
{
	public float time;

	public Vector3 point;

	public TimePoint(float time, Vector3 point)
	{
		this.time = time;
		this.point = point;
	}
}

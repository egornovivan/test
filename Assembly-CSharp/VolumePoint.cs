using UnityEngine;

public struct VolumePoint
{
	public Vector3 localPosition;

	public float pos_volume;

	public float neg_volume;

	public VolumePoint(Vector3 lpos, float pv, float nv)
	{
		localPosition = lpos;
		pos_volume = pv;
		neg_volume = nv;
	}
}

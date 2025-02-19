using UnityEngine;

internal class TrailLattice
{
	public Vector3 startPos;

	public Vector3 endPos;

	public float time;

	public TrailLattice(Vector3 s, Vector3 e, float t)
	{
		startPos = s;
		endPos = e;
		time = t;
	}
}

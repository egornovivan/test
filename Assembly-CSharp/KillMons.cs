using UnityEngine;

public struct KillMons
{
	public enum Type
	{
		protoTypeId,
		fixedId,
		max
	}

	public int id;

	public Type type;

	public Vector3 center;

	public float radius;

	public int monId;

	public int reviveTime;
}

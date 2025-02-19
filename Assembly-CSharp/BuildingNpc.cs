using UnityEngine;

public class BuildingNpc
{
	public int templateId;

	public Vector3 pos;

	public float rotY;

	public bool isStand;

	public BuildingNpc(int id, Vector3 pos, float rotY, bool isStand)
	{
		templateId = id;
		this.pos = pos;
		this.rotY = rotY;
		this.isStand = isStand;
	}
}

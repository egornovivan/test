using UnityEngine;

public class DetectedTown
{
	public int campId;

	public Vector3 pos;

	public string name;

	public IntVector2 PosCenter => new IntVector2(Mathf.RoundToInt(pos.x), Mathf.RoundToInt(pos.z));

	public DetectedTown(Vector3 pos, string name, int campId)
	{
		this.pos = pos;
		this.name = name;
		this.campId = campId;
	}
}

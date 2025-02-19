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

	public DetectedTown(VArtifactTown vat)
	{
		pos = new Vector3(vat.PosCenter.x, 0f, vat.PosCenter.y);
		name = PELocalization.GetString(vat.townNameId);
		campId = vat.templateId;
	}

	public static IntVector2 GetPosCenter(Vector3 pos)
	{
		return new IntVector2(Mathf.RoundToInt(pos.x), Mathf.RoundToInt(pos.z));
	}
}

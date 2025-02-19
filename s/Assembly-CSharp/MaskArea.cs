using UnityEngine;

public class MaskArea
{
	public byte Index;

	public int IconID;

	public int TeamId;

	public Vector3 Pos;

	public string Description;

	public MaskArea()
	{
	}

	public MaskArea(int iconID, Vector3 pos, string description)
	{
		Pos = pos;
		IconID = iconID;
		Description = description;
	}
}

using UnityEngine;

public class SpawnPoint
{
	public int ID;

	public int EntityID = -1;

	public Vector3 Position;

	public Quaternion Rotation;

	public Vector3 Scale;

	public string Name = "No Name";

	public int Prototype;

	public int PlayerIndex;

	public bool IsTarget;

	public bool Visible;

	public bool isDead;

	public Vector3 entityPos;

	public bool Enable = true;

	public SpawnPoint()
	{
	}

	public SpawnPoint(WEEntity ety)
	{
		ID = ety.ID;
		Name = ety.ObjectName;
		Position = ety.Position;
		Rotation = ety.Rotation;
		Scale = ety.Scale;
		Prototype = ety.Prototype;
		PlayerIndex = ety.PlayerIndex;
		IsTarget = ety.IsTarget;
		Visible = ety.Visible;
	}

	public SpawnPoint(SpawnPoint sp)
	{
		ID = sp.ID;
		EntityID = sp.EntityID;
		Name = sp.Name;
		Position = sp.Position;
		Rotation = sp.Rotation;
		Scale = sp.Scale;
		Prototype = sp.Prototype;
		PlayerIndex = sp.PlayerIndex;
		IsTarget = sp.IsTarget;
		Visible = sp.Visible;
	}
}

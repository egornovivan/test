using System.IO;
using UnityEngine;

namespace PeCustom;

public class SpawnPoint
{
	public int ID;

	public int EntityID = -1;

	public Vector3 spawnPos;

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
		spawnPos = ety.Position;
		Rotation = ety.Rotation;
		Scale = ety.Scale;
		Prototype = ety.Prototype;
		PlayerIndex = ety.PlayerIndex;
		IsTarget = ety.IsTarget;
		Visible = ety.Visible;
		entityPos = ety.Position;
	}

	public SpawnPoint(SpawnPoint sp)
	{
		ID = sp.ID;
		EntityID = sp.EntityID;
		Name = sp.Name;
		spawnPos = sp.spawnPos;
		Rotation = sp.Rotation;
		Scale = sp.Scale;
		Prototype = sp.Prototype;
		PlayerIndex = sp.PlayerIndex;
		IsTarget = sp.IsTarget;
		Visible = sp.Visible;
		entityPos = sp.entityPos;
	}

	public virtual void Serialize(BinaryWriter bw)
	{
		bw.Write(ID);
		bw.Write(EntityID);
		bw.Write(Name);
		bw.Write(spawnPos.x);
		bw.Write(spawnPos.y);
		bw.Write(spawnPos.z);
		bw.Write(Rotation.x);
		bw.Write(Rotation.y);
		bw.Write(Rotation.z);
		bw.Write(Rotation.w);
		bw.Write(Scale.x);
		bw.Write(Scale.y);
		bw.Write(Scale.z);
		bw.Write(Prototype);
		bw.Write(PlayerIndex);
		bw.Write(IsTarget);
		bw.Write(Visible);
		bw.Write(isDead);
		bw.Write(entityPos.x);
		bw.Write(entityPos.y);
		bw.Write(entityPos.z);
		bw.Write(Enable);
	}

	public virtual void Deserialize(int version, BinaryReader br)
	{
		switch (version)
		{
		case 1:
		case 2:
			ID = br.ReadInt32();
			EntityID = br.ReadInt32();
			spawnPos.x = br.ReadSingle();
			spawnPos.y = br.ReadSingle();
			spawnPos.z = br.ReadSingle();
			Rotation.x = br.ReadSingle();
			Rotation.y = br.ReadSingle();
			Rotation.z = br.ReadSingle();
			Rotation.w = br.ReadSingle();
			Scale.x = br.ReadSingle();
			Scale.y = br.ReadSingle();
			Scale.z = br.ReadSingle();
			Prototype = br.ReadInt32();
			PlayerIndex = br.ReadInt32();
			IsTarget = br.ReadBoolean();
			Visible = br.ReadBoolean();
			isDead = br.ReadBoolean();
			break;
		case 3:
			ID = br.ReadInt32();
			EntityID = br.ReadInt32();
			Name = br.ReadString();
			spawnPos.x = br.ReadSingle();
			spawnPos.y = br.ReadSingle();
			spawnPos.z = br.ReadSingle();
			Rotation.x = br.ReadSingle();
			Rotation.y = br.ReadSingle();
			Rotation.z = br.ReadSingle();
			Rotation.w = br.ReadSingle();
			Scale.x = br.ReadSingle();
			Scale.y = br.ReadSingle();
			Scale.z = br.ReadSingle();
			Prototype = br.ReadInt32();
			PlayerIndex = br.ReadInt32();
			IsTarget = br.ReadBoolean();
			Visible = br.ReadBoolean();
			isDead = br.ReadBoolean();
			break;
		case 4:
			ID = br.ReadInt32();
			EntityID = br.ReadInt32();
			Name = br.ReadString();
			spawnPos.x = br.ReadSingle();
			spawnPos.y = br.ReadSingle();
			spawnPos.z = br.ReadSingle();
			Rotation.x = br.ReadSingle();
			Rotation.y = br.ReadSingle();
			Rotation.z = br.ReadSingle();
			Rotation.w = br.ReadSingle();
			Scale.x = br.ReadSingle();
			Scale.y = br.ReadSingle();
			Scale.z = br.ReadSingle();
			Prototype = br.ReadInt32();
			PlayerIndex = br.ReadInt32();
			IsTarget = br.ReadBoolean();
			Visible = br.ReadBoolean();
			isDead = br.ReadBoolean();
			entityPos.x = br.ReadSingle();
			entityPos.y = br.ReadSingle();
			entityPos.z = br.ReadSingle();
			Enable = br.ReadBoolean();
			break;
		}
	}
}

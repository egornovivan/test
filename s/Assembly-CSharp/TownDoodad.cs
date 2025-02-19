using UnityEngine;

public class TownDoodad
{
	private int mProtoId;

	private int mWorldId;

	private Vector3 mPosition;

	public TownDoodad(int id, int worldId, Vector3 pos)
	{
		mProtoId = id;
		mWorldId = worldId;
		mPosition = pos;
	}

	public override bool Equals(object obj)
	{
		if (obj is TownDoodad)
		{
			TownDoodad townDoodad = (TownDoodad)obj;
			return townDoodad.mProtoId == mProtoId && mWorldId == townDoodad.mWorldId && Mathf.Abs(townDoodad.mPosition.x - mPosition.x) <= 2f && Mathf.Abs(townDoodad.mPosition.y - mPosition.y) <= 2f && Mathf.Abs(townDoodad.mPosition.z - mPosition.z) <= 2f;
		}
		return false;
	}

	public override int GetHashCode()
	{
		return mProtoId;
	}

	public static bool operator ==(TownDoodad doodad1, TownDoodad doodad2)
	{
		return doodad1.mProtoId == doodad2.mProtoId && doodad1.mWorldId == doodad2.mWorldId && Mathf.Abs(doodad1.mPosition.x - doodad2.mPosition.x) <= 2f && Mathf.Abs(doodad1.mPosition.y - doodad2.mPosition.y) <= 2f && Mathf.Abs(doodad1.mPosition.z - doodad2.mPosition.z) <= 2f;
	}

	public static bool operator !=(TownDoodad doodad1, TownDoodad doodad2)
	{
		return doodad1.mProtoId != doodad2.mProtoId || doodad1.mWorldId != doodad2.mWorldId || Mathf.Abs(doodad1.mPosition.x - doodad2.mPosition.x) > 2f || Mathf.Abs(doodad1.mPosition.y - doodad2.mPosition.y) > 2f || Mathf.Abs(doodad1.mPosition.z - doodad2.mPosition.z) > 2f;
	}
}

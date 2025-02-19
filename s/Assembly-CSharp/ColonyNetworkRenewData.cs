using UnityEngine;

public struct ColonyNetworkRenewData
{
	public int id;

	public int externId;

	public int teamId;

	public Vector3 pos;

	public Quaternion rot;

	public int ownerId;

	public int worldId;

	public ColonyNetworkRenewData(int id, int externId, int teamId, Vector3 pos, Quaternion rot, int ownerId, int worldId)
	{
		this.id = id;
		this.externId = externId;
		this.teamId = teamId;
		this.pos = pos;
		this.rot = rot;
		this.ownerId = ownerId;
		this.worldId = worldId;
	}
}

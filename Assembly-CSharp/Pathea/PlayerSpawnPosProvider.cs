using UnityEngine;

namespace Pathea;

public class PlayerSpawnPosProvider : PeSingleton<PlayerSpawnPosProvider>
{
	private Vector3 mPos;

	public Vector3 GetPos()
	{
		return mPos;
	}

	public void SetPos(Vector3 pos)
	{
		Debug.Log(string.Concat("<color=yellow>set player spawn pos:", pos, "</color>"));
		mPos = pos;
	}
}

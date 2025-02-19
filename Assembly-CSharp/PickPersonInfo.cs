using UnityEngine;

public class PickPersonInfo
{
	public int _entityId;

	public Vector3 _PickPos;

	public PickPersonInfo(int id, Vector3 pos)
	{
		_entityId = id;
		_PickPos = pos;
	}
}

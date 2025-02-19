using UnityEngine;

namespace TownData;

public class NativePointInfo
{
	public IntVector2 index;

	public Vector3 position;

	public int id;

	public int townId;

	public int ID => id;

	public float PosY => position.y;

	public NativePointInfo(IntVector2 index, int id)
	{
		this.index = index;
		this.id = id;
		position = new Vector3(index.x, -1f, index.y);
	}

	public NativePointInfo(Vector3 pos, int id)
	{
		index = new IntVector2(Mathf.RoundToInt(pos.x), Mathf.RoundToInt(pos.z));
		this.id = id;
		position = pos;
	}

	public void SetPosY(float height)
	{
		position.y = height;
	}
}

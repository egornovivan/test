using uLink;
using UnityEngine;

namespace TownData;

public class TownNpcInfo
{
	private IntVector2 index;

	private Vector3 position;

	private int id;

	public float PosY
	{
		get
		{
			return position.y;
		}
		set
		{
			position.y = value;
		}
	}

	public TownNpcInfo(IntVector2 index, int id)
	{
		this.index = index;
		this.id = id;
		position = new Vector3(index.x, -1f, index.y);
	}

	public Vector3 getPos()
	{
		return position;
	}

	public int getId()
	{
		return id;
	}

	public void setPosY(float y)
	{
		position.y = y;
	}

	public float getPosY()
	{
		return position.y;
	}

	internal static object DeserializeInfo(uLink.BitStream stream, params object[] codecOptions)
	{
		IntVector2 intVector = stream.Read<IntVector2>(new object[0]);
		int num = stream.Read<int>(new object[0]);
		return new TownNpcInfo(intVector, num);
	}

	internal static void SerializeInfo(uLink.BitStream stream, object value, params object[] codecOptions)
	{
		TownNpcInfo townNpcInfo = value as TownNpcInfo;
		stream.Write(townNpcInfo.index);
		stream.Write(townNpcInfo.id);
	}
}

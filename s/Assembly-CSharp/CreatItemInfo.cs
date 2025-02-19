using uLink;
using UnityEngine;

public class CreatItemInfo
{
	public int mItemId;

	public Vector3 mPos;

	public Quaternion mRotation;

	internal static object DeserializeItemInfo(uLink.BitStream stream, params object[] codecOptions)
	{
		CreatItemInfo creatItemInfo = new CreatItemInfo();
		creatItemInfo.mItemId = stream.Read<int>(new object[0]);
		creatItemInfo.mPos = stream.Read<Vector3>(new object[0]);
		creatItemInfo.mRotation = stream.Read<Quaternion>(new object[0]);
		return creatItemInfo;
	}

	internal static void SerializeItemInfo(uLink.BitStream stream, object value, params object[] codecOptions)
	{
		CreatItemInfo creatItemInfo = value as CreatItemInfo;
		stream.Write(creatItemInfo.mItemId);
		stream.Write(creatItemInfo.mPos);
		stream.Write(creatItemInfo.mRotation);
	}
}

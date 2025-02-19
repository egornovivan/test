using uLink;
using UnityEngine;

namespace CustomData;

public class MapObj
{
	public Vector3 pos;

	public int objID;

	internal static object Deserialize(uLink.BitStream stream, params object[] codecOptions)
	{
		MapObj mapObj = new MapObj();
		mapObj.pos = stream.Read<Vector3>(new object[0]);
		mapObj.objID = stream.Read<int>(new object[0]);
		return mapObj;
	}

	internal static void Serialize(uLink.BitStream stream, object value, params object[] codecOptions)
	{
		MapObj mapObj = (MapObj)value;
		stream.Write(mapObj.pos);
		stream.Write(mapObj.objID);
	}
}

using System.IO;
using PETools;
using UnityEngine;

public class PassengerInfo
{
	public enum Course
	{
		before,
		on,
		latter
	}

	public int npcID;

	public int startRouteID;

	public int startIndexID;

	public int endRouteID;

	public int endIndexID;

	public Vector3 dest;

	public Course type;

	public byte[] Export()
	{
		return Serialize.Export(delegate(BinaryWriter w)
		{
			w.Write(npcID);
			w.Write(startRouteID);
			w.Write(startIndexID);
			w.Write(endRouteID);
			w.Write(endIndexID);
			w.Write(dest.x);
			w.Write(dest.y);
			w.Write(dest.z);
			w.Write((int)type);
		});
	}

	public void Import(byte[] data)
	{
		Serialize.Import(data, delegate(BinaryReader r)
		{
			npcID = r.ReadInt32();
			startRouteID = r.ReadInt32();
			startIndexID = r.ReadInt32();
			endRouteID = r.ReadInt32();
			endIndexID = r.ReadInt32();
			dest = new Vector3(r.ReadSingle(), r.ReadSingle(), r.ReadSingle());
			type = (Course)r.ReadInt32();
		});
	}
}

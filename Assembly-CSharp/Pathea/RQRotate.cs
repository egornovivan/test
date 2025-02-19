using System.IO;
using UnityEngine;

namespace Pathea;

public class RQRotate : Request
{
	private const int Version_001 = 0;

	private const int Version_Current = 0;

	public Quaternion rotation = Quaternion.identity;

	public override EReqType Type => EReqType.Rotate;

	public RQRotate()
	{
	}

	public RQRotate(params object[] objs)
	{
		rotation = (Quaternion)objs[0];
	}

	public override void Serialize(BinaryWriter w)
	{
		w.Write(0);
		w.Write(rotation.x);
		w.Write(rotation.y);
		w.Write(rotation.z);
		w.Write(rotation.w);
	}

	public override void Deserialize(BinaryReader r)
	{
		r.ReadInt32();
		rotation.x = r.ReadSingle();
		rotation.y = r.ReadSingle();
		rotation.z = r.ReadSingle();
		rotation.w = r.ReadSingle();
	}
}

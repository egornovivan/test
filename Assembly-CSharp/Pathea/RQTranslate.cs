using System.IO;
using UnityEngine;

namespace Pathea;

public class RQTranslate : Request
{
	private const int Version_001 = 0;

	private const int Version_Current = 0;

	public Vector3 position = Vector3.zero;

	public bool adjust = true;

	public override EReqType Type => EReqType.Translate;

	public RQTranslate()
	{
	}

	public RQTranslate(params object[] objs)
	{
		position = (Vector3)objs[0];
		adjust = (bool)objs[1];
	}

	public override void Serialize(BinaryWriter w)
	{
		w.Write(0);
		w.Write(position.x);
		w.Write(position.y);
		w.Write(position.z);
		w.Write(adjust);
	}

	public override void Deserialize(BinaryReader r)
	{
		r.ReadInt32();
		position.x = r.ReadSingle();
		position.y = r.ReadSingle();
		position.z = r.ReadSingle();
		adjust = r.ReadBoolean();
	}
}

using System.IO;
using UnityEngine;

namespace Pathea;

public class RQSalvation : Request
{
	private const int Version_001 = 0;

	private const int Version_Current = 0;

	public int id;

	public bool carry;

	public Vector3 m_Direction;

	public override EReqType Type => EReqType.Salvation;

	public RQSalvation()
	{
	}

	public RQSalvation(params object[] objs)
	{
		id = (int)objs[0];
		carry = (bool)objs[1];
	}

	public override void Serialize(BinaryWriter w)
	{
		w.Write(0);
		w.Write(id);
		w.Write(carry);
	}

	public override void Deserialize(BinaryReader r)
	{
		r.ReadInt32();
		id = r.ReadInt32();
		carry = r.ReadBoolean();
	}
}

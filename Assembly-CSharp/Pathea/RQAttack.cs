using System.IO;

namespace Pathea;

public class RQAttack : Request
{
	private const int Version_001 = 0;

	private const int Version_Current = 0;

	public override EReqType Type => EReqType.Attack;

	public override void Serialize(BinaryWriter w)
	{
		w.Write(0);
	}

	public override void Deserialize(BinaryReader r)
	{
		r.ReadInt32();
	}
}

using System.IO;

namespace Pathea;

public class RQUseSkill : Request
{
	private const int Version_001 = 0;

	private const int Version_Current = 0;

	public override EReqType Type => EReqType.UseSkill;

	public override void Serialize(BinaryWriter w)
	{
		w.Write(0);
	}

	public override void Deserialize(BinaryReader r)
	{
		int num = r.ReadInt32();
	}
}

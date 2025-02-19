using System.IO;

namespace Pathea;

public class RQPauseAll : Request
{
	private const int Version_001 = 0;

	private const int Version_Current = 0;

	public override EReqType Type => EReqType.PauseAll;

	public override void Serialize(BinaryWriter w)
	{
		w.Write(0);
	}

	public override void Deserialize(BinaryReader r)
	{
		int num = r.ReadInt32();
	}
}

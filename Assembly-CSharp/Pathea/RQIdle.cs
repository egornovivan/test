using System.IO;

namespace Pathea;

public class RQIdle : Request
{
	public enum RQidleType
	{
		Idle,
		InjuredSit,
		InjuredRest,
		BeCarry,
		InjuredSitEX,
		Lie,
		Max
	}

	private const int Version_001 = 0;

	private const int Version_Current = 0;

	public string state = string.Empty;

	public override EReqType Type => EReqType.Idle;

	public RQIdle()
	{
	}

	public RQIdle(params object[] objs)
	{
		state = (string)objs[0];
	}

	public override void Serialize(BinaryWriter w)
	{
		w.Write(0);
		w.Write(state);
	}

	public override void Deserialize(BinaryReader r)
	{
		int num = r.ReadInt32();
		state = r.ReadString();
	}
}

using System.IO;

namespace Pathea;

public class RQState : Request
{
	private const int Version_001 = 0;

	private const int Version_Current = 0;

	public string animName = string.Empty;

	public override EReqType Type => EReqType.Animation;

	public RQState()
	{
	}

	public RQState(params object[] objs)
	{
		animName = (string)objs[0];
	}

	public override void Serialize(BinaryWriter w)
	{
		w.Write(0);
		w.Write(animName);
	}

	public override void Deserialize(BinaryReader r)
	{
		r.ReadInt32();
		animName = r.ReadString();
	}
}

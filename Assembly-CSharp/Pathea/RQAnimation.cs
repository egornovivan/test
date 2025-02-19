using System.IO;

namespace Pathea;

public class RQAnimation : Request
{
	private const int Version_001 = 0;

	private const int Version_002 = 1;

	private const int Version_Current = 1;

	public string animName = string.Empty;

	public float animTime;

	public bool play = true;

	public override EReqType Type => EReqType.Animation;

	public RQAnimation()
	{
	}

	public RQAnimation(params object[] objs)
	{
		animName = (string)objs[0];
		animTime = (float)objs[1];
		if (objs.Length >= 3)
		{
			play = (bool)objs[2];
		}
	}

	public override void Serialize(BinaryWriter w)
	{
		w.Write(1);
		w.Write(animName);
		w.Write(animTime);
		w.Write(play);
	}

	public override void Deserialize(BinaryReader r)
	{
		int num = r.ReadInt32();
		animName = r.ReadString();
		animTime = r.ReadSingle();
		if (num >= 1)
		{
			play = r.ReadBoolean();
		}
	}
}

using PatheaScript;

namespace PatheaScriptExt;

public class ENpcDead : Event
{
	private VarRef mNpcId;

	public override bool Parse()
	{
		if (!base.Parse())
		{
			return false;
		}
		mNpcId = PeType.GetNpcId(mInfo, mTrigger);
		return true;
	}

	public override bool Filter(params object[] param)
	{
		if (param.Length != 1)
		{
			return false;
		}
		return false;
	}

	public override string ToString()
	{
		return $"ENpcDead:{mNpcId}]";
	}
}

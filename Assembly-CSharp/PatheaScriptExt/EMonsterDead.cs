using PatheaScript;

namespace PatheaScriptExt;

public class EMonsterDead : Event
{
	private VarRef mMosterId;

	public override bool Parse()
	{
		if (!base.Parse())
		{
			return false;
		}
		mMosterId = PeType.GetMonsterId(mInfo, mTrigger);
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
		return $"EPlayerDead:{mMosterId}]";
	}
}

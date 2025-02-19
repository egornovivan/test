using PatheaScript;

namespace PatheaScriptExt;

public class EPlayerDead : Event
{
	private VarRef mPlayerId;

	public override bool Parse()
	{
		if (!base.Parse())
		{
			return false;
		}
		mPlayerId = PeType.GetPlayerId(mInfo, mTrigger);
		return true;
	}

	public override bool Filter(params object[] param)
	{
		if (param.Length != 1)
		{
			return false;
		}
		if (0 == mPlayerId.Value)
		{
		}
		return false;
	}

	public override string ToString()
	{
		return $"EPlayerDead:{mPlayerId}]";
	}
}

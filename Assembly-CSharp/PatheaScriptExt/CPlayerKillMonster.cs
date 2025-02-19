using PatheaScript;
using PeEvent;

namespace PatheaScriptExt;

public class CPlayerKillMonster : Condition
{
	private VarRef mCount;

	private VarRef mMonsterId;

	private Compare mCompare;

	private VarValue mKillCount = new VarValue(0);

	private void Handler(object sender, KillEventArg arg)
	{
		if (arg.victim.Id == mMonsterId.Value)
		{
			mKillCount += (VarValue)1;
		}
	}

	public override bool Parse()
	{
		mCompare = mFactory.GetCompare(mInfo, "compare");
		mCount = PatheaScript.Util.GetVarRefOrValue(mInfo, "count", VarValue.EType.Int, mTrigger);
		mMonsterId = PeType.GetMonsterId(mInfo, mTrigger);
		Globle.kill.Subscribe(Handler);
		return true;
	}

	public override bool Do()
	{
		if (!base.Do())
		{
			return false;
		}
		return mCompare.Do(mCount.Value, mKillCount);
	}
}

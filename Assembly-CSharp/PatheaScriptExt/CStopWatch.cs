using PatheaScript;

namespace PatheaScriptExt;

public class CStopWatch : Condition
{
	private VarRef mTimerName;

	private Compare mCompare;

	private VarRef mRef;

	public override bool Parse()
	{
		mTimerName = PatheaScript.Util.GetVarRefOrValue(mInfo, "id", VarValue.EType.String, mTrigger);
		mCompare = mFactory.GetCompare(mInfo, "compare");
		mRef = PatheaScript.Util.GetVarRefOrValue(mInfo, "sec", VarValue.EType.Float, mTrigger);
		return true;
	}

	public override bool Do()
	{
		if (!base.Do())
		{
			return false;
		}
		PETimer pETimer = PeTimerMgr.Instance.Get((string)mTimerName.Value);
		if (pETimer == null)
		{
			return false;
		}
		VarValue lhs = new VarValue((float)pETimer.Second);
		return mCompare.Do(lhs, mRef.Value);
	}
}

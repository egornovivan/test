namespace PatheaScript;

public class ConditionSwitch : Condition
{
	private VarRef mVarRef;

	private bool mRefVar;

	public override bool Parse()
	{
		mRefVar = Util.GetBool(mInfo, "value");
		mVarRef = Util.GetVarRef(mInfo, "switch", mTrigger);
		return true;
	}

	public override bool Do()
	{
		return mVarRef.Value == mRefVar;
	}

	public override string ToString()
	{
		return $"Condition[Switch:{mVarRef} == {mRefVar}]";
	}
}

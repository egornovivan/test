namespace PatheaScript;

public class ConditionCheckVar : Condition
{
	private VarRef mVarRef;

	private Compare mCompare;

	private VarRef mRefVar;

	public override bool Parse()
	{
		mCompare = mFactory.GetCompare(mInfo, "compare");
		mRefVar = Util.GetVarRefOrValue(mInfo, "ref", VarValue.EType.Var, mTrigger);
		mVarRef = Util.GetVarRef(mInfo, "name", mTrigger);
		return true;
	}

	public override bool Do()
	{
		return mCompare.Do(mVarRef.Value, mRefVar.Value);
	}

	public override string ToString()
	{
		return $"Condition[CheckVar:{mVarRef} {mCompare} {mRefVar}]";
	}
}

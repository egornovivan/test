namespace PatheaScript;

public class ActionLoadScript : ActionImmediate
{
	protected override bool Exec()
	{
		VarRef varRefOrValue = Util.GetVarRefOrValue(mInfo, "mission", VarValue.EType.Int, mTrigger);
		if ((int)varRefOrValue.Value < 0)
		{
			Debug.LogError("error script id:" + varRefOrValue);
			return false;
		}
		mTrigger.Parent.Parent.AddToLoadList((int)varRefOrValue.Value);
		return true;
	}
}

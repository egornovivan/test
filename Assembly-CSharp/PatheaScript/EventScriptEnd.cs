namespace PatheaScript;

public class EventScriptEnd : Event
{
	private VarRef mScriptIdRef;

	private PsScript.EResult mResult = PsScript.EResult.Max;

	public override bool Parse()
	{
		if (!base.Parse())
		{
			return false;
		}
		mScriptIdRef = Util.GetVarRefOrValue(mInfo, "mission", VarValue.EType.Int, mTrigger);
		mResult = Util.GetScriptResult(mInfo);
		return true;
	}

	public override bool Filter(params object[] param)
	{
		if (param.Length != 1)
		{
			return false;
		}
		if (!(param[0] is PsScript psScript))
		{
			return false;
		}
		int num = (int)mScriptIdRef.Value;
		if (num == 0)
		{
			num = mTrigger.Parent.Id;
		}
		if (psScript.Id != num)
		{
			return false;
		}
		if (psScript.Result != mResult)
		{
			return false;
		}
		return true;
	}
}

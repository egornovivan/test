namespace PatheaScript;

public class EventScriptBegin : Event
{
	private VarRef mScriptIdRef;

	public override bool Parse()
	{
		if (!base.Parse())
		{
			return false;
		}
		mScriptIdRef = Util.GetVarRefOrValue(mInfo, "mission", VarValue.EType.Int, mTrigger);
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
		return true;
	}
}

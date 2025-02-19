using PatheaScript;

namespace PatheaScriptExt;

public class EUiClicked : Event
{
	private VarRef mVarRef;

	public override bool Parse()
	{
		if (!base.Parse())
		{
			return false;
		}
		mVarRef = PeType.GetUIId(mInfo, mTrigger);
		return true;
	}

	public override bool Filter(params object[] param)
	{
		if (param.Length != 1)
		{
			return false;
		}
		int num = (int)param[0];
		if (num == -1)
		{
			return true;
		}
		if (mVarRef.Value == num)
		{
			return true;
		}
		return false;
	}

	public override string ToString()
	{
		return $"Event[Ui Clicked:{mVarRef}]";
	}
}

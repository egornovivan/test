using PatheaScript;

namespace PatheaScriptExt;

public class ActionKillStopWatch : ActionImmediate
{
	protected override bool Exec()
	{
		VarRef varRefOrValue = PatheaScript.Util.GetVarRefOrValue(mInfo, "id", VarValue.EType.String, mTrigger);
		PeTimerMgr.Instance.Remove((string)varRefOrValue.Value);
		return true;
	}
}

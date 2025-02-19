using PatheaScript;

namespace PatheaScriptExt;

public class ActionStopWatch : ActionImmediate
{
	protected override bool Exec()
	{
		VarRef varRefOrValue = PatheaScript.Util.GetVarRefOrValue(mInfo, "id", VarValue.EType.String, mTrigger);
		Functor functor = mFactory.GetFunctor(mInfo, "set1");
		VarRef varRefOrValue2 = PatheaScript.Util.GetVarRefOrValue(mInfo, "sec", VarValue.EType.Float, mTrigger);
		Functor functor2 = mFactory.GetFunctor(mInfo, "set2");
		VarRef varRefOrValue3 = PatheaScript.Util.GetVarRefOrValue(mInfo, "speed", VarValue.EType.Float, mTrigger);
		functor.Set(new Variable(), varRefOrValue2.Var);
		functor.Do();
		functor2.Set(new Variable(), varRefOrValue3.Var);
		functor2.Do();
		PETimer pETimer = new PETimer();
		pETimer.Second = (float)functor.Target.Value;
		pETimer.ElapseSpeed = (float)functor2.Target.Value;
		PeTimerMgr.Instance.Add((string)varRefOrValue.Value, pETimer);
		return true;
	}
}

namespace PatheaScript;

public class ActionSetVar : ActionImmediate
{
	protected override bool Exec()
	{
		Functor functor = mFactory.GetFunctor(mInfo, "set");
		VarRef varRefOrValue = Util.GetVarRefOrValue(mInfo, "value", VarValue.EType.Var, mTrigger);
		Variable.EScope varScope = Util.GetVarScope(mInfo);
		string @string = Util.GetString(mInfo, "name");
		Variable target = mTrigger.AddVar(@string, varScope);
		functor.Set(target, varRefOrValue.Var);
		functor.Do();
		Debug.Log("execute result:" + functor);
		return true;
	}
}

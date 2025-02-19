namespace PatheaScript;

public class ActionSetSwitch : ActionImmediate
{
	protected override bool Exec()
	{
		Functor functor = mFactory.GetFunctor(mInfo, "set");
		bool @bool = Util.GetBool(mInfo, "value");
		Variable.EScope varScope = Util.GetVarScope(mInfo);
		string @string = Util.GetString(mInfo, "name");
		Variable target = mTrigger.AddVar(@string, varScope);
		functor.Set(target, new Variable(@bool));
		functor.Do();
		Debug.Log("execute result:" + functor);
		return true;
	}
}

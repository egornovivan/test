using ScenarioRTL;

namespace PeCustom;

[Statement("SET VARIABLE", true)]
public class SetVariableAction : Action
{
	private string varname = string.Empty;

	private EScope varscope;

	private EFunc func;

	private Var value;

	protected override void OnCreate()
	{
		varname = Utility.ToVarname(base.parameters["var"]);
		varscope = (EScope)Utility.ToEnumInt(base.parameters["scope"]);
		func = Utility.ToFunc(base.parameters["func"]);
		value = Utility.ToVar(base.missionVars, base.parameters["value"]);
	}

	public override bool Logic()
	{
		VarScope varScope = null;
		if (varscope == EScope.Global)
		{
			varScope = base.scenarioVars;
		}
		else if (varscope == EScope.Mission)
		{
			varScope = base.missionVars;
		}
		if (varScope != null)
		{
			if (varScope.VarDeclared(varname))
			{
				varScope[varname] = Utility.FunctionVar(varScope[varname], value, func);
			}
			else
			{
				varScope[varname] = Utility.FunctionVar(Var.zero, value, func);
			}
		}
		return true;
	}
}

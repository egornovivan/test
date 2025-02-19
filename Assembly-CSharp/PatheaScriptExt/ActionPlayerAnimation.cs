using PatheaScript;

namespace PatheaScriptExt;

public class ActionPlayerAnimation : ActionImmediate
{
	protected override bool Exec()
	{
		return true;
	}

	public override string ToString()
	{
		return $"ActionPlayerAnimation";
	}
}

namespace PatheaScript;

public class NotEqual : Compare
{
	public override bool Do(VarValue lhs, VarValue rhs)
	{
		return lhs != rhs;
	}
}

namespace PatheaScript;

public class Greater : Compare
{
	public override bool Do(VarValue lhs, VarValue rhs)
	{
		return lhs > rhs;
	}
}

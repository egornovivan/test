namespace PatheaScript;

public class GreaterEqual : Compare
{
	public override bool Do(VarValue lhs, VarValue rhs)
	{
		return lhs > rhs || lhs == rhs;
	}
}

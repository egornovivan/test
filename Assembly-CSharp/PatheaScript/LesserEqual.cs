namespace PatheaScript;

public class LesserEqual : Compare
{
	public override bool Do(VarValue lhs, VarValue rhs)
	{
		return lhs < rhs || lhs == rhs;
	}
}

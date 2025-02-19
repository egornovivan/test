namespace PatheaScript;

public class Equal : Compare
{
	public override bool Do(VarValue lhs, VarValue rhs)
	{
		return lhs == rhs;
	}
}

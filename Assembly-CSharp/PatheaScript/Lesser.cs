namespace PatheaScript;

public class Lesser : Compare
{
	public override bool Do(VarValue lhs, VarValue rhs)
	{
		return lhs < rhs;
	}
}

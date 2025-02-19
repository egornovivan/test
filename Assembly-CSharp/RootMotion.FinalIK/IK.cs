namespace RootMotion.FinalIK;

public abstract class IK : SolverManager
{
	public abstract IKSolver GetIKSolver();

	protected override void UpdateSolver()
	{
		IKSolver iKSolver = GetIKSolver();
		if (!iKSolver.initiated)
		{
			InitiateSolver();
		}
		if (iKSolver.initiated)
		{
			iKSolver.Update();
		}
	}

	protected override void InitiateSolver()
	{
		if (!GetIKSolver().initiated)
		{
			GetIKSolver().Initiate(base.transform);
		}
	}

	protected override void FixTransforms()
	{
		if (GetIKSolver().initiated)
		{
			GetIKSolver().FixTransforms();
		}
	}

	protected abstract void OpenUserManual();

	protected abstract void OpenScriptReference();
}

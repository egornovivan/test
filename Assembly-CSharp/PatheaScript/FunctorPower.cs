namespace PatheaScript;

public class FunctorPower : Functor
{
	public override void Do()
	{
		mTarget.Value = VarValue.Power(mTarget.Value, mArg.Value);
	}
}

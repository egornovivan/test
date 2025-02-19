namespace PatheaScript;

public class FunctorDivide : Functor
{
	public override void Do()
	{
		mTarget.Value /= mArg.Value;
	}
}

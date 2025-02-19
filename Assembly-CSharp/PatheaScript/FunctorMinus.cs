namespace PatheaScript;

public class FunctorMinus : Functor
{
	public override void Do()
	{
		mTarget.Value -= mArg.Value;
	}
}

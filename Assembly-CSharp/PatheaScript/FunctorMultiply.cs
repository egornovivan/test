namespace PatheaScript;

public class FunctorMultiply : Functor
{
	public override void Do()
	{
		mTarget.Value *= mArg.Value;
	}
}

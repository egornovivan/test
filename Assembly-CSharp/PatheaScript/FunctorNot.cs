namespace PatheaScript;

public class FunctorNot : Functor
{
	public override void Do()
	{
		mTarget.Value ^= mArg.Value;
	}
}

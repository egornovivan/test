namespace PatheaScript;

public class FunctorMod : Functor
{
	public override void Do()
	{
		mTarget.Value %= mArg.Value;
	}
}

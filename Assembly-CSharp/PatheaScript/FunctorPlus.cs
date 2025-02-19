namespace PatheaScript;

public class FunctorPlus : Functor
{
	public override void Do()
	{
		mTarget.Value += mArg.Value;
	}
}

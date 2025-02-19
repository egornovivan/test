namespace PatheaScript;

public class FunctorSet : Functor
{
	public override void Do()
	{
		mTarget.Value = mArg.Value;
	}
}

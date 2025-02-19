using System;

namespace PatheaScript;

public abstract class Functor
{
	protected Variable mTarget;

	protected Variable mArg;

	public Variable Target => mTarget;

	public void Set(Variable target, Variable arg)
	{
		mTarget = target;
		mArg = arg;
	}

	public virtual void Do()
	{
		throw new NotImplementedException(ToString());
	}

	public override string ToString()
	{
		return $"Func:{GetType().ToString()}, Target:{mTarget}, Arg:{mArg}";
	}
}

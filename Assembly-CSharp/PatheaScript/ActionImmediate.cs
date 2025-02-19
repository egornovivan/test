namespace PatheaScript;

public abstract class ActionImmediate : Action
{
	public override bool Parse()
	{
		return true;
	}

	protected override TickResult OnTick()
	{
		if (base.OnTick() == TickResult.Finished)
		{
			return TickResult.Finished;
		}
		Exec();
		return TickResult.Finished;
	}

	protected abstract bool Exec();
}

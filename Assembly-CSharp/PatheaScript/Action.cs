using System.IO;

namespace PatheaScript;

public abstract class Action : TriggerChild, Storeable
{
	private enum EStep
	{
		Init,
		Running,
		Finished,
		Max
	}

	private EStep mStep;

	public Action()
	{
		mStep = EStep.Init;
	}

	protected virtual bool OnInit()
	{
		return true;
	}

	protected virtual TickResult OnTick()
	{
		return TickResult.Running;
	}

	protected virtual void OnReset()
	{
	}

	public TickResult Tick()
	{
		if (mStep == EStep.Init)
		{
			if (!OnInit())
			{
				return TickResult.Finished;
			}
			mStep = EStep.Running;
		}
		if (mStep == EStep.Running)
		{
			if (OnTick() != TickResult.Finished)
			{
				return TickResult.Running;
			}
			OnReset();
			mStep = EStep.Finished;
		}
		return TickResult.Finished;
	}

	public bool Init()
	{
		mStep = EStep.Init;
		return true;
	}

	public virtual void Store(BinaryWriter w)
	{
		w.Write((sbyte)mStep);
	}

	public virtual void Restore(BinaryReader r)
	{
		mStep = (EStep)r.ReadSByte();
	}
}

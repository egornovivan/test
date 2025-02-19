using System.IO;
using PatheaScript;
using UnityEngine;

namespace PatheaScriptExt;

public class ActionWait : Action
{
	private VarRef mTimeMs;

	private float mBeginTime;

	public override bool Parse()
	{
		mTimeMs = PatheaScript.Util.GetVarRefOrValue(mInfo, "ms", VarValue.EType.Int, mTrigger);
		return true;
	}

	protected override bool OnInit()
	{
		if (!base.OnInit())
		{
			return false;
		}
		mBeginTime = Time.time;
		return true;
	}

	protected override TickResult OnTick()
	{
		if (base.OnTick() == TickResult.Finished)
		{
			return TickResult.Finished;
		}
		if ((Time.time - mBeginTime) * 1000f > (float)(int)mTimeMs.Value)
		{
			return TickResult.Finished;
		}
		return TickResult.Running;
	}

	public override void Store(BinaryWriter w)
	{
		base.Store(w);
		w.Write(Time.time - mBeginTime);
	}

	public override void Restore(BinaryReader r)
	{
		base.Restore(r);
		float num = r.ReadSingle();
		mBeginTime = Time.time - num;
	}
}

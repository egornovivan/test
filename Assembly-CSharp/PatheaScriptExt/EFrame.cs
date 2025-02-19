using PatheaScript;
using UnityEngine;

namespace PatheaScriptExt;

public class EFrame : PatheaScript.Event
{
	private VarRef mT;

	private VarRef mQ;

	public override bool Parse()
	{
		if (!base.Parse())
		{
			return false;
		}
		mT = PatheaScript.Util.GetVarRefOrValue(mInfo, "t", VarValue.EType.Int, mTrigger);
		mQ = PatheaScript.Util.GetVarRefOrValue(mInfo, "q", VarValue.EType.Int, mTrigger);
		return true;
	}

	public override bool Filter(params object[] param)
	{
		if (mQ.Value == Time.frameCount % mT.Value)
		{
			return true;
		}
		return false;
	}

	public override string ToString()
	{
		return $"Event[Frame:mod{mT}={mQ}]";
	}
}

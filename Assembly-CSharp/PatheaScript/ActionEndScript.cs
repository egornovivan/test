using System.Collections.Generic;

namespace PatheaScript;

public class ActionEndScript : ActionImmediate
{
	protected override bool Exec()
	{
		VarRef varRefOrValue = Util.GetVarRefOrValue(mInfo, "mission", VarValue.EType.Int, mTrigger);
		int num = (int)varRefOrValue.Value;
		PsScript.EResult scriptResult = Util.GetScriptResult(mInfo);
		if (num >= 0)
		{
			PsScript psScript = null;
			psScript = ((num != 0) ? mTrigger.Parent.Parent.FindScriptById(num) : mTrigger.Parent);
			if (psScript != null)
			{
				psScript.RequireStop(scriptResult);
				return true;
			}
		}
		else
		{
			List<PsScript> list = new List<PsScript>(mTrigger.Parent.Parent.CurScript);
			if (num == -2)
			{
				list.Remove(mTrigger.Parent);
			}
			if (list.Count > 0)
			{
				foreach (PsScript item in list)
				{
					item.RequireStop(scriptResult);
				}
			}
		}
		return false;
	}
}

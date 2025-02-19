using ScenarioRTL;
using UnityEngine;

namespace PeCustom;

[Statement("SET ITEM GOAL", true)]
public class SetItemGoalAction : Action
{
	private int goalId;

	private string text;

	private int missionId;

	private OBJECT item;

	private ECompare comp;

	private int amt;

	protected override void OnCreate()
	{
		goalId = Utility.ToInt(base.missionVars, base.parameters["id"]);
		text = Utility.ToText(base.missionVars, base.parameters["text"]);
		missionId = Utility.ToEnumInt(base.parameters["mission"]);
		item = Utility.ToObject(base.parameters["item"]);
		comp = Utility.ToCompare(base.parameters["compare"]);
		amt = Utility.ToInt(base.missionVars, base.parameters["amount"]);
		if (missionId == 0)
		{
			missionId = base.mission.dataId;
		}
	}

	public override bool Logic()
	{
		if (PeCustomScene.Self != null && PeCustomScene.Self.scenario != null && PeCustomScene.Self.scenario.missionMgr != null)
		{
			PeCustomScene.Self.scenario.missionMgr.SetItemGoal(goalId, text, missionId, item, comp, amt);
		}
		else
		{
			Debug.LogError("MissionMgr is not ready");
		}
		return true;
	}
}

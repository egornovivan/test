using ScenarioRTL;
using UnityEngine;

namespace PeCustom;

[Statement("BEGIN CHOICE GROUP", true)]
public class BeginChoiceGroupAction : Action
{
	protected override void OnCreate()
	{
	}

	public override bool Logic()
	{
		if (PeCustomScene.Self != null && PeCustomScene.Self.scenario != null)
		{
			if (!PeCustomScene.Self.scenario.dialogMgr.BeginChooseGroup())
			{
				Debug.LogWarning("Begin choose group erro!");
			}
		}
		else
		{
			Debug.LogWarning("The PeCustomScene is not exist");
		}
		return true;
	}
}

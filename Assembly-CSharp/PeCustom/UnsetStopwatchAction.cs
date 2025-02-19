using ScenarioRTL;
using UnityEngine;

namespace PeCustom;

[Statement("UNSET STOPWATCH", true)]
public class UnsetStopwatchAction : Action
{
	private int id;

	protected override void OnCreate()
	{
		id = Utility.ToInt(base.missionVars, base.parameters["id"]);
	}

	public override bool Logic()
	{
		if (PeCustomScene.Self != null && PeCustomScene.Self.scenario != null && PeCustomScene.Self.scenario.stopwatchMgr != null)
		{
			PeCustomScene.Self.scenario.stopwatchMgr.UnsetStopwatch(id);
		}
		else
		{
			Debug.LogError("UnsetStopwatch - target is null");
		}
		return true;
	}
}

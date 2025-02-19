using ScenarioRTL;
using UnityEngine;

namespace PeCustom;

[Statement("TIME OUT")]
public class TimeoutListener : EventListener
{
	private int id;

	protected override void OnCreate()
	{
		id = Utility.ToInt(base.missionVars, base.parameters["id"]);
	}

	public override void Listen()
	{
		if (PeCustomScene.Self != null && PeCustomScene.Self.scenario != null && PeCustomScene.Self.scenario.stopwatchMgr != null)
		{
			PeCustomScene.Self.scenario.stopwatchMgr.OnTimeout += OnResponse;
		}
	}

	public override void Close()
	{
		if (PeCustomScene.Self != null && PeCustomScene.Self.scenario != null && PeCustomScene.Self.scenario.stopwatchMgr != null)
		{
			PeCustomScene.Self.scenario.stopwatchMgr.OnTimeout -= OnResponse;
		}
		else
		{
			Debug.LogError("Try to close eventlistener, but source has been destroyed");
		}
	}

	private void OnResponse(int _id)
	{
		if (id == _id)
		{
			Post();
		}
	}
}

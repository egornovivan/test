using Pathea;
using ScenarioRTL;
using UnityEngine;

namespace PeCustom;

[Statement("TICK CYCLE")]
public class TickListener : EventListener
{
	private int n;

	protected override void OnCreate()
	{
		n = Utility.ToInt(base.missionVars, base.parameters["n"]);
		if (n < 1)
		{
			n = 1;
		}
	}

	public override void Listen()
	{
		if (PeCustomScene.Self != null && PeCustomScene.Self.scenario != null && PeCustomScene.Self.scenario.tickMgr != null)
		{
			PeCustomScene.Self.scenario.tickMgr.OnTick += OnResponse;
		}
	}

	public override void Close()
	{
		if (PeCustomScene.Self != null && PeCustomScene.Self.scenario != null && PeCustomScene.Self.scenario.tickMgr != null)
		{
			PeCustomScene.Self.scenario.tickMgr.OnTick -= OnResponse;
		}
		else
		{
			Debug.LogError("Try to close eventlistener, but source has been destroyed");
		}
	}

	private void OnResponse(int tick)
	{
		if (PeGameMgr.IsMulti)
		{
			tick /= 4;
		}
		if (tick % n == 0)
		{
			Post();
		}
	}
}

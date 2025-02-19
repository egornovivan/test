using ScenarioRTL;
using UnityEngine;

namespace PeCustom;

[Statement("CUSTOM UI EVENT")]
public class CustomUIEventListener : EventListener
{
	private string eventname = string.Empty;

	protected override void OnCreate()
	{
		eventname = Utility.ToVarname(base.parameters["eventname"]);
	}

	public override void Listen()
	{
		if (PeCustomScene.Self != null && PeCustomScene.Self.scenario != null && PeCustomScene.Self.scenario.guiMgr != null)
		{
			PeCustomScene.Self.scenario.guiMgr.OnGUIResponse += OnResponse;
		}
	}

	public override void Close()
	{
		if (PeCustomScene.Self != null && PeCustomScene.Self.scenario != null && PeCustomScene.Self.scenario.guiMgr != null)
		{
			PeCustomScene.Self.scenario.guiMgr.OnGUIResponse -= OnResponse;
		}
		else
		{
			Debug.LogError("Try to close eventlistener, but source has been destroyed");
		}
	}

	private void OnResponse(string evtname)
	{
		if (eventname == evtname)
		{
			Post();
		}
	}
}

using ScenarioRTL;
using UnityEngine;

namespace PeCustom;

[Statement("CUSTOM UI")]
public class CustomUIListener : EventListener
{
	protected override void OnCreate()
	{
	}

	public override void Listen()
	{
		if (PeCustomScene.Self != null && PeCustomScene.Self.scenario != null && PeCustomScene.Self.scenario.guiMgr != null)
		{
			PeCustomScene.Self.scenario.guiMgr.OnGUIDrawing += base.Post;
		}
	}

	public override void Close()
	{
		if (PeCustomScene.Self != null && PeCustomScene.Self.scenario != null && PeCustomScene.Self.scenario.guiMgr != null)
		{
			PeCustomScene.Self.scenario.guiMgr.OnGUIDrawing -= base.Post;
		}
		else
		{
			Debug.LogError("Try to close eventlistener, but source has been destroyed");
		}
	}
}

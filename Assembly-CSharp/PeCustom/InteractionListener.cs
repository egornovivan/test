using Pathea;
using ScenarioRTL;
using UnityEngine;

namespace PeCustom;

[Statement("INTERACTION")]
public class InteractionListener : EventListener
{
	private OBJECT obj;

	protected override void OnCreate()
	{
		obj = Utility.ToObject(base.parameters["object"]);
	}

	public override void Listen()
	{
		if (PeSingleton<EntityMgr>.Instance != null && PeSingleton<EntityMgr>.Instance.eventor != null)
		{
			PeSingleton<EntityMgr>.Instance.eventor.Subscribe(OnResponse);
		}
	}

	public override void Close()
	{
		if (PeSingleton<EntityMgr>.Instance != null && PeSingleton<EntityMgr>.Instance.eventor != null)
		{
			PeSingleton<EntityMgr>.Instance.eventor.Unsubscribe(OnResponse);
		}
		else
		{
			Debug.LogError("Try to close eventlistener, but source has been destroyed");
		}
	}

	private void OnResponse(object sender, EntityMgr.RMouseClickEntityEvent e)
	{
		if (!(e.entity == null) && PeScenarioUtility.IsObjectContainEntity(obj, e.entity))
		{
			Post();
		}
	}
}

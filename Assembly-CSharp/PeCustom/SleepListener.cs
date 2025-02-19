using Pathea;
using ScenarioRTL;
using UnityEngine;

namespace PeCustom;

[Statement("SLEEP")]
public class SleepListener : EventListener
{
	protected override void OnCreate()
	{
	}

	public override void Listen()
	{
		PeEntity mainPlayer = PeSingleton<PeCreature>.Instance.mainPlayer;
		if (mainPlayer != null)
		{
			Action_Sleep action = mainPlayer.motionMgr.GetAction<Action_Sleep>();
			if (action != null)
			{
				action.startSleepEvt += OnResponse;
			}
		}
		else
		{
			Debug.LogError("main player is null");
		}
	}

	public override void Close()
	{
		PeEntity mainPlayer = PeSingleton<PeCreature>.Instance.mainPlayer;
		if (mainPlayer != null)
		{
			Action_Sleep action = mainPlayer.motionMgr.GetAction<Action_Sleep>();
			if (action != null)
			{
				action.startSleepEvt -= OnResponse;
			}
		}
		else
		{
			Debug.LogError("main player is null");
		}
	}

	private void OnResponse(int obj)
	{
		Post();
	}
}

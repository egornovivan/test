using Pathea;
using ScenarioRTL;
using UnityEngine;

namespace PeCustom;

[Statement("TAKE UP EQUIP")]
public class TakeUpEquipListener : EventListener
{
	protected override void OnCreate()
	{
	}

	public override void Listen()
	{
		PeEntity mainPlayer = PeSingleton<PeCreature>.Instance.mainPlayer;
		if (mainPlayer != null)
		{
			mainPlayer.motionEquipment.OnActiveWeapon += base.Post;
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
			mainPlayer.motionEquipment.OnActiveWeapon -= base.Post;
		}
		else
		{
			Debug.LogError("main player is null");
		}
	}
}

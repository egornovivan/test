using Pathea;
using ScenarioRTL;
using UnityEngine;

namespace PeCustom;

[Statement("CHANGE EQUIP")]
public class ChangeEquipListener : EventListener
{
	protected override void OnCreate()
	{
	}

	public override void Listen()
	{
		PeEntity mainPlayer = PeSingleton<PeCreature>.Instance.mainPlayer;
		if (mainPlayer != null)
		{
			mainPlayer.equipmentCmpt.OnEquipmentChange += base.Post;
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
			mainPlayer.equipmentCmpt.OnEquipmentChange -= base.Post;
		}
		else
		{
			Debug.LogError("main player is null");
		}
	}
}

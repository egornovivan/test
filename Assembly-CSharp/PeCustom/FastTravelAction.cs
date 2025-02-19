using System.IO;
using Pathea;
using ScenarioRTL;
using UnityEngine;

namespace PeCustom;

[Statement("FAST TRAVEL")]
public class FastTravelAction : Action
{
	private int worldId;

	private Vector3 pos;

	private Vector3 rot;

	private bool isFirst = true;

	protected override void OnCreate()
	{
		worldId = Utility.ToInt(base.missionVars, base.parameters["id"]);
		pos = Utility.ToVector(base.missionVars, base.parameters["point"]);
		rot = Utility.ToVector(base.missionVars, base.parameters["euler"]);
	}

	public override bool Logic()
	{
		if (PeGameMgr.IsMulti)
		{
			PlayerNetwork.RequestServer(EPacketType.PT_Custom_FastTravel, worldId, pos, rot);
		}
		else
		{
			if (FastTravel.bTraveling)
			{
				return false;
			}
			if (isFirst)
			{
				PeEntity mainPlayer = PeSingleton<PeCreature>.Instance.mainPlayer;
				if (mainPlayer != null)
				{
					if (rot != Vector3.zero && mainPlayer.peTrans != null)
					{
						Quaternion rotation = default(Quaternion);
						rotation.eulerAngles = rot;
						mainPlayer.peTrans.rotation = rotation;
					}
					if (pos != Vector3.zero)
					{
						PeSingleton<FastTravelMgr>.Instance.TravelTo(worldId, pos);
					}
				}
				isFirst = false;
				return false;
			}
			isFirst = true;
		}
		return true;
	}

	public override void RestoreState(BinaryReader r)
	{
	}

	public override void StoreState(BinaryWriter w)
	{
	}
}

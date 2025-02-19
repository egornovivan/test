using System.IO;
using Pathea;
using PETools;
using ScenarioRTL;
using UnityEngine;

namespace PeCustom;

[Statement("DISABLE SPAWN", true)]
public class DisableSpawnAction : Action
{
	private OBJECT spawn;

	protected override void OnCreate()
	{
		spawn = Utility.ToObject(base.parameters["spawnpoint"]);
	}

	public override bool Logic()
	{
		if (PeGameMgr.IsMulti)
		{
			byte[] array = Serialize.Export(delegate(BinaryWriter w)
			{
				BufferHelper.Serialize(w, spawn);
			});
			PlayerNetwork.RequestServer(EPacketType.PT_Custom_DisableSpawn, array);
		}
		else if (!PeScenarioUtility.EnableSpawnPoint(spawn, enable: false))
		{
			Debug.LogWarning("Diable spawn point error");
		}
		return true;
	}
}

using System.IO;
using Pathea;
using PETools;
using ScenarioRTL;
using UnityEngine;

namespace PeCustom;

[Statement("ENABLE SPAWN", true)]
public class EnableSpawnAction : Action
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
			PlayerNetwork.RequestServer(EPacketType.PT_Custom_EnableSpawn, array);
		}
		else if (!PeScenarioUtility.EnableSpawnPoint(spawn, enable: true))
		{
			Debug.LogWarning("Enable spawn point error");
		}
		return true;
	}
}

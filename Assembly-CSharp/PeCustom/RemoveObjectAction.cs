using System.IO;
using Pathea;
using PETools;
using ScenarioRTL;
using UnityEngine;

namespace PeCustom;

[Statement("REMOVE OBJECT", true)]
public class RemoveObjectAction : Action
{
	private OBJECT obj;

	private RANGE range;

	protected override void OnCreate()
	{
		obj = Utility.ToObject(base.parameters["object"]);
		range = Utility.ToRange(base.missionVars, base.parameters["range"]);
	}

	public override bool Logic()
	{
		if (PeGameMgr.IsMulti)
		{
			byte[] array = Serialize.Export(delegate(BinaryWriter w)
			{
				BufferHelper.Serialize(w, obj);
				BufferHelper.Serialize(w, range);
			});
			PlayerNetwork.RequestServer(EPacketType.PT_Custom_RemoveObject, array);
		}
		else if (PeScenarioUtility.RemoveObjects(obj, range))
		{
			Debug.LogWarning("Remove objects faild!");
		}
		return true;
	}
}

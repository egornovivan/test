using System.IO;
using Pathea;
using PETools;
using ScenarioRTL;
using UnityEngine;

namespace PeCustom;

[Statement("CREATE OBJECT", true)]
public class CreateObjectAction : Action
{
	private int amt;

	private OBJECT obj;

	private RANGE range;

	protected override void OnCreate()
	{
		amt = Utility.ToInt(base.missionVars, base.parameters["amount"]);
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
				w.Write(amt);
			});
			PlayerNetwork.RequestServer(EPacketType.PT_Custom_CreateObject, array);
		}
		else if (!PeScenarioUtility.CreateObjects(amt, obj, range))
		{
			Debug.LogWarning("Create objects faild!");
		}
		return true;
	}
}

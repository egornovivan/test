using System.IO;
using Pathea;
using PETools;
using ScenarioRTL;
using UnityEngine;

namespace PeCustom;

[Statement("REMOVE SPECIFIC OBJECT", true)]
public class RemoveSpecObjectAction : Action
{
	private OBJECT obj;

	protected override void OnCreate()
	{
		obj = Utility.ToObject(base.parameters["object"]);
	}

	public override bool Logic()
	{
		if (PeGameMgr.IsMulti)
		{
			byte[] array = Serialize.Export(delegate(BinaryWriter w)
			{
				BufferHelper.Serialize(w, obj);
			});
			PlayerNetwork.RequestServer(EPacketType.PT_Custom_RemoveSpecObject, array);
		}
		else if (PeScenarioUtility.RemoveObject(obj))
		{
			Debug.LogWarning("Remove Spec Object Error!");
		}
		return true;
	}
}

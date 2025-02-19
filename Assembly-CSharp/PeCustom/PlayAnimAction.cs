using System.IO;
using Pathea;
using PETools;
using ScenarioRTL;

namespace PeCustom;

[Statement("PLAY ANIMATION", true)]
public class PlayAnimAction : Action
{
	private string name;

	private OBJECT obj;

	public static bool playerAniming;

	protected override void OnCreate()
	{
		name = Utility.ToVarname(base.parameters["name"]);
		obj = Utility.ToObject(base.parameters["object"]);
	}

	public override bool Logic()
	{
		if (PeGameMgr.IsMulti)
		{
			byte[] array = Serialize.Export(delegate(BinaryWriter w)
			{
				BufferHelper.Serialize(w, obj);
				w.Write(name);
			});
			PlayerNetwork.RequestServer(EPacketType.PT_Custom_StartAnimation, array);
		}
		else
		{
			PeEntity entity = PeScenarioUtility.GetEntity(obj);
			if (null == entity)
			{
				return true;
			}
			if (obj.isCurrentPlayer)
			{
				playerAniming = true;
				entity.animCmpt.AnimEvtString += OnAnimString;
			}
			string text = name.Split('_')[name.Split('_').Length - 1];
			if (text == "Once")
			{
				entity.animCmpt.SetTrigger(name);
			}
			else if (text == "Muti")
			{
				entity.animCmpt.SetBool(name, value: true);
			}
		}
		return true;
	}

	private void OnAnimString(string param)
	{
		if (param == "OnCustomAniEnd" && obj.isCurrentPlayer)
		{
			playerAniming = false;
		}
	}
}

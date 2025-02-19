using System.IO;
using Pathea;
using PETools;
using ScenarioRTL;

namespace PeCustom;

[Statement("REMOVE QUEST", true)]
public class RemoveQuestAction : Action
{
	private int id;

	private OBJECT obj;

	protected override void OnCreate()
	{
		id = Utility.ToInt(base.missionVars, base.parameters["id"]);
		obj = Utility.ToObject(base.parameters["object"]);
	}

	public override bool Logic()
	{
		if (PeGameMgr.IsMulti)
		{
			byte[] array = Serialize.Export(delegate(BinaryWriter w)
			{
				BufferHelper.Serialize(w, obj);
				BufferHelper.Serialize(w, id);
			});
			PlayerNetwork.RequestServer(EPacketType.PT_Custom_RemoveQuest, array);
		}
		else if (PeCustomScene.Self != null && PeCustomScene.Self.scenario != null && PeCustomScene.Self.scenario.dialogMgr != null)
		{
			PeCustomScene.Self.scenario.dialogMgr.RemoveQuest(obj.Group, obj.Id, id);
		}
		return true;
	}
}

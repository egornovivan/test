using System.IO;
using Pathea;
using PETools;
using ScenarioRTL;

namespace PeCustom;

[Statement("ADD QUEST", true)]
public class AddQuestAction : Action
{
	private int id;

	private string text;

	private OBJECT obj;

	protected override void OnCreate()
	{
		id = Utility.ToInt(base.missionVars, base.parameters["id"]);
		text = Utility.ToText(base.missionVars, base.parameters["text"]);
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
				BufferHelper.Serialize(w, text);
			});
			PlayerNetwork.RequestServer(EPacketType.PT_Custom_AddQuest, array);
		}
		else if (PeCustomScene.Self != null && PeCustomScene.Self.scenario != null && PeCustomScene.Self.scenario.dialogMgr != null)
		{
			PeCustomScene.Self.scenario.dialogMgr.SetQuest(obj.Group, obj.Id, id, text);
		}
		return true;
	}
}

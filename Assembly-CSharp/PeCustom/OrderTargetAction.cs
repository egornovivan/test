using System.IO;
using Pathea;
using PETools;
using ScenarioRTL;

namespace PeCustom;

[Statement("ORDER TARGET", true)]
public class OrderTargetAction : Action
{
	private OBJECT obj;

	private ECommand cmd;

	private OBJECT tar;

	protected override void OnCreate()
	{
		obj = Utility.ToObject(base.parameters["object"]);
		cmd = (ECommand)Utility.ToEnumInt(base.parameters["command"]);
		tar = Utility.ToObject(base.parameters["target"]);
	}

	public override bool Logic()
	{
		if (PeGameMgr.IsMulti)
		{
			byte[] array = Serialize.Export(delegate(BinaryWriter w)
			{
				BufferHelper.Serialize(w, obj);
				BufferHelper.Serialize(w, tar);
				w.Write((byte)cmd);
			});
			PlayerNetwork.RequestServer(EPacketType.PT_Custom_OrderTarget, array);
		}
		else
		{
			PeEntity entity = PeScenarioUtility.GetEntity(obj);
			PeEntity entity2 = PeScenarioUtility.GetEntity(tar);
			if (entity != null && entity2 != null && (entity.proto == EEntityProto.Npc || entity.proto == EEntityProto.RandomNpc || entity.proto == EEntityProto.Monster) && entity.requestCmpt != null)
			{
				if (cmd == ECommand.MoveTo)
				{
					entity.requestCmpt.Register(EReqType.FollowTarget, entity2.Id);
				}
				else if (cmd == ECommand.FaceAt)
				{
					entity.requestCmpt.Register(EReqType.Dialogue, string.Empty, entity2.peTrans);
				}
			}
		}
		return true;
	}
}

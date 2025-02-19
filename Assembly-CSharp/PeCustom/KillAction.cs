using System.IO;
using Pathea;
using PETools;
using ScenarioRTL;

namespace PeCustom;

[Statement("KILL", true)]
public class KillAction : Action
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
			PlayerNetwork.RequestServer(EPacketType.PT_Custom_Kill, array);
		}
		else if (obj.isPrototype)
		{
			if (obj.type == OBJECT.OBJECTTYPE.MonsterProto)
			{
				foreach (PeEntity item in PeSingleton<EntityMgr>.Instance.All)
				{
					if (item.commonCmpt.entityProto.proto == EEntityProto.Monster && PeScenarioUtility.IsObjectContainEntity(obj, item) && range.Contains(item.position))
					{
						item.SetAttribute(AttribType.Hp, 0f, offEvent: false);
					}
				}
			}
		}
		else
		{
			PeEntity entity = PeScenarioUtility.GetEntity(obj);
			if (null != entity && range.Contains(entity.position))
			{
				entity.SetAttribute(AttribType.Hp, 0f, offEvent: false);
			}
		}
		return true;
	}
}

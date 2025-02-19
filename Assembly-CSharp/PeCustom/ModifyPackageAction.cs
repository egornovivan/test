using System.IO;
using ItemAsset;
using Pathea;
using PETools;
using ScenarioRTL;
using UnityEngine;

namespace PeCustom;

[Statement("MODIFY PACKAGE", true)]
public class ModifyPackageAction : Action
{
	private OBJECT item;

	private EFunc func;

	private int count;

	private OBJECT player;

	protected override void OnCreate()
	{
		item = Utility.ToObject(base.parameters["item"]);
		func = Utility.ToFunc(base.parameters["func"]);
		count = Utility.ToInt(base.missionVars, base.parameters["count"]);
		player = Utility.ToObject(base.parameters["player"]);
	}

	public override bool Logic()
	{
		if (PeGameMgr.IsMulti)
		{
			byte[] array = Serialize.Export(delegate(BinaryWriter w)
			{
				BufferHelper.Serialize(w, player);
				BufferHelper.Serialize(w, item);
				w.Write(count);
				w.Write((byte)func);
			});
			PlayerNetwork.RequestServer(EPacketType.PT_Custom_ModifyPackage, array);
		}
		else if (player.type == OBJECT.OBJECTTYPE.Player && item.type == OBJECT.OBJECTTYPE.ItemProto && item.isSpecificPrototype)
		{
			PeEntity entity = PeScenarioUtility.GetEntity(player);
			if (entity != null && entity.packageCmpt != null)
			{
				int itemCount = entity.packageCmpt.GetItemCount(item.Id);
				int num = Utility.Function(itemCount, count, func);
				if (num > 0)
				{
					entity.packageCmpt.Set(item.Id, num);
				}
				else
				{
					entity.packageCmpt.Destory(item.Id, itemCount);
					if (num < 0)
					{
						Debug.LogWarning($"Items whose protoID is {item.Id} are not enough.");
					}
				}
				if (num - itemCount > 0)
				{
					ItemProto itemProto = PeSingleton<ItemProto.Mgr>.Instance.Get(item.Id);
					if (itemProto == null)
					{
						return true;
					}
					string content = itemProto.GetName() + " X " + (num - itemCount);
					new PeTipMsg(content, itemProto.icon[0], PeTipMsg.EMsgLevel.Norm);
				}
			}
		}
		return true;
	}
}

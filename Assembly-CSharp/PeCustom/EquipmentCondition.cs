using System.Collections.Generic;
using System.IO;
using ItemAsset;
using Pathea;
using ScenarioRTL;

namespace PeCustom;

[Statement("EQUIPMENT")]
public class EquipmentCondition : Condition
{
	private OBJECT player;

	private OBJECT equip;

	protected override void OnCreate()
	{
		player = Utility.ToObject(base.parameters["player"]);
		equip = Utility.ToObject(base.parameters["equipment"]);
	}

	public override bool? Check()
	{
		if (player.type == OBJECT.OBJECTTYPE.Player && equip.type == OBJECT.OBJECTTYPE.ItemProto)
		{
			PeEntity entity = PeScenarioUtility.GetEntity(player);
			if (entity != null && entity.equipmentCmpt != null)
			{
				List<ItemObject> itemList = entity.equipmentCmpt._ItemList;
				for (int i = 0; i < itemList.Count; i++)
				{
					if (equip.isAnyPrototype)
					{
						return true;
					}
					if (equip.isAnyPrototypeInCategory && itemList[i].protoData.editorTypeId == equip.Group)
					{
						return true;
					}
					if (itemList[i].protoId == equip.Id)
					{
						return true;
					}
				}
			}
		}
		return false;
	}

	protected override void SendReq()
	{
		byte[] array = BufferHelper.Export(delegate(BinaryWriter w)
		{
			w.Write(reqId);
			BufferHelper.Serialize(w, player);
			BufferHelper.Serialize(w, equip);
		});
		PlayerNetwork.RequestServer(EPacketType.PT_Custom_CheckEquip, array);
	}
}

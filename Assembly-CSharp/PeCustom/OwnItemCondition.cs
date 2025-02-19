using System.IO;
using Pathea;
using ScenarioRTL;

namespace PeCustom;

[Statement("OWN ITEM")]
public class OwnItemCondition : Condition
{
	private OBJECT player;

	private ECompare comp;

	private int count;

	private OBJECT item;

	protected override void OnCreate()
	{
		player = Utility.ToObject(base.parameters["player"]);
		comp = Utility.ToCompare(base.parameters["compare"]);
		count = Utility.ToInt(base.missionVars, base.parameters["count"]);
		item = Utility.ToObject(base.parameters["item"]);
	}

	public override bool? Check()
	{
		if (player.type == OBJECT.OBJECTTYPE.Player && item.type == OBJECT.OBJECTTYPE.ItemProto)
		{
			PeEntity entity = PeScenarioUtility.GetEntity(player);
			if (entity != null && entity.packageCmpt != null)
			{
				int lhs = 0;
				if (item.isSpecificPrototype)
				{
					lhs = entity.packageCmpt.GetItemCount(item.Id);
				}
				else if (item.isAnyPrototypeInCategory)
				{
					lhs = entity.packageCmpt.GetCountByEditorType(item.Group);
				}
				else if (item.isAnyPrototype)
				{
					lhs = entity.packageCmpt.GetAllItemsCount();
				}
				if (Utility.Compare(lhs, count, comp))
				{
					return true;
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
			BufferHelper.Serialize(w, item);
			w.Write(count);
			w.Write((byte)comp);
		});
		PlayerNetwork.RequestServer(EPacketType.PT_Custom_CheckOwerItem, array);
	}
}

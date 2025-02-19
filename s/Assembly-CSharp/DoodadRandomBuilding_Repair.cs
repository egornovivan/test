using System.Collections.Generic;
using ItemAsset;
using Pathea;
using uLink;

public class DoodadRandomBuilding_Repair : DoodadRepair
{
	private int townId;

	private int _campId;

	private int _damageId;

	private int _dPlayerId;

	private List<ItemSample> DropItemID = new List<ItemSample>();

	public override void DropItem(NetInterface caster)
	{
		DoodadProtoDb.Item item = DoodadProtoDb.Get(_protoTypeId);
		if (item == null)
		{
			return;
		}
		DropItemID.Clear();
		List<ItemSample> dropItems = ItemDropData.GetDropItems(item.dropItemId);
		if (dropItems != null && dropItems.Count != 0)
		{
			List<ItemSample> list = GetSpecialItem.MonsterItemAdd(_protoTypeId, caster);
			if (list != null && list.Count > 0)
			{
				DropItemID.AddRange(list);
			}
			DropItemID.AddRange(dropItems);
			CreateDropScenes(DropItemID);
		}
	}

	public override void Create(MapObjNetwork net, NetworkMessageInfo info)
	{
		base.Create(net, info);
		_param = info.networkView.initialData.Read<string>(new object[0]);
		DoodadRandomBuilding.ExtractParam(_param, out townId, out _campId, out _damageId, out _dPlayerId);
	}

	public override void InitAttr()
	{
		_net.SetAttribute(AttribType.CampID, _campId);
		_net.SetAttribute(AttribType.DamageID, _damageId);
		_net.SetAttribute(AttribType.DefaultPlayerID, _dPlayerId);
	}

	public override void DeathDestroyNet()
	{
		BuildingInfoManager.Instance.OnBuildingDeath(townId, _entityId, _worldId);
		_net.DestroyMapObj();
	}
}

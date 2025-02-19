using System.Collections.Generic;
using ItemAsset;
using Pathea;
using uLink;
using UnityEngine;

public class NativeTowerNetwork : AiMonsterNetwork
{
	private int mTownId;

	private int mCampId;

	private int mDamageId;

	private Vector3 mScale;

	protected override void OnPEInstantiate(uLink.NetworkMessageInfo info)
	{
		SetParent("NativeTowerNetMgr");
		_id = info.networkView.initialData.Read<int>(new object[0]);
		_externId = info.networkView.initialData.Read<int>(new object[0]);
		_worldId = info.networkView.initialData.Read<int>(new object[0]);
		mTownId = info.networkView.initialData.Read<int>(new object[0]);
		mCampId = info.networkView.initialData.Read<int>(new object[0]);
		mDamageId = info.networkView.initialData.Read<int>(new object[0]);
		mScale = info.networkView.initialData.Read<Vector3>(new object[0]);
		Add(this);
		InitializeData();
		AddSkEntity();
	}

	protected override void InitializeData()
	{
		InitCmpt();
	}

	public override void DropItem(NetInterface caster)
	{
		DoodadProtoDb.Item item = DoodadProtoDb.Get(base.ExternId);
		if (item == null)
		{
			return;
		}
		base.DropItemID.Clear();
		List<ItemSample> dropItems = ItemDropData.GetDropItems(item.dropItemId);
		if (dropItems != null && dropItems.Count != 0)
		{
			List<ItemSample> list = GetSpecialItem.MonsterItemAdd(base.ExternId, caster);
			if (list != null && list.Count > 0)
			{
				base.DropItemID.AddRange(list);
			}
			base.DropItemID.AddRange(dropItems);
			CreateDropScenes(base.DropItemID);
		}
	}
}

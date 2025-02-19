using System;
using System.Collections.Generic;
using Pathea;
using UnityEngine;

public class ClickPEentityLootItem : MousePickableChildCollider
{
	private struct ItemSampleInfo
	{
		public int protoId;

		public int num;
	}

	public Action<int> onClick;

	private PeEntity entity;

	private List<ItemSampleInfo> items = new List<ItemSampleInfo>();

	private bool clicked;

	protected override void OnStart()
	{
		base.OnStart();
		entity = GetComponent<PeEntity>();
		MousePickablePeEntity component = entity.GetComponent<MousePickablePeEntity>();
		if (null != component)
		{
			UnityEngine.Object.Destroy(component);
		}
		operateDistance = 5f;
	}

	private void OnClickPeEntity()
	{
		if (!clicked)
		{
			if (onClick != null)
			{
				onClick(entity.Id);
			}
			for (int i = 0; i < items.Count; i++)
			{
				PeSingleton<LootItemMgr>.Instance.AddLootItem(entity.position, items[i].protoId, items[i].num);
			}
			Singleton<PeLogicGlobal>.Instance.DestroyEntity(entity.skEntity);
			clicked = true;
			UnityEngine.Object.Destroy(this);
		}
	}

	public void AddItem(int protoId, int num)
	{
		ItemSampleInfo itemSampleInfo = default(ItemSampleInfo);
		itemSampleInfo.protoId = protoId;
		itemSampleInfo.num = num;
		ItemSampleInfo item = itemSampleInfo;
		items.Add(item);
	}

	protected override void CheckOperate()
	{
		if (PeInput.Get(PeInput.LogicFunction.PickBody))
		{
			OnClickPeEntity();
		}
	}
}

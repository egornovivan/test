using System.Collections.Generic;
using Pathea;
using Pathea.Effect;
using UnityEngine;

public class MousePickableRandomItem : MousePickableChildCollider
{
	private List<ItemIdCount> allItems = new List<ItemIdCount>();

	private List<int> allItemObjs = new List<int>();

	private List<ItemIdCount> undefineItem = new List<ItemIdCount>();

	public Vector3 genPos;

	private bool clicked;

	private bool isOpen => undefineItem == null || undefineItem.Count == 0;

	public void RefreshItem(int[] items, List<ItemIdCount> items2, List<int> itemObjList)
	{
		allItems = new List<ItemIdCount>();
		for (int i = 0; i < items.Length; i += 2)
		{
			allItems.Add(new ItemIdCount(items[i], items[i + 1]));
		}
		undefineItem = items2;
		allItemObjs = itemObjList;
	}

	protected override void OnStart()
	{
		base.OnStart();
		base.priority = MousePicker.EPriority.Level3;
		operateDistance = 7f;
	}

	protected override void CheckOperate()
	{
		if (PeInput.Get(PeInput.LogicFunction.PickBody) && !clicked)
		{
			if (PeGameMgr.IsSingle)
			{
				OnClickRandomItem();
			}
			else
			{
				PlayerNetwork.mainPlayer.RPCServer(EPacketType.PT_InGame_RandomItemClicked, genPos, base.transform.position);
			}
			clicked = true;
		}
	}

	private void OnClickRandomItem()
	{
		for (int i = 0; i < allItems.Count; i++)
		{
			PeSingleton<LootItemMgr>.Instance.AddLootItem(base.transform.position, allItems[i].protoId, allItems[i].count);
		}
		for (int j = 0; j < undefineItem.Count; j++)
		{
			PeSingleton<LootItemMgr>.Instance.AddLootItem(base.transform.position, undefineItem[j].protoId, undefineItem[j].count);
		}
		for (int k = 0; k < allItemObjs.Count; k++)
		{
			PeSingleton<LootItemMgr>.Instance.AddLootItem(base.transform.position, allItemObjs[k]);
		}
		DestroySelf();
	}

	private void DestroySelf()
	{
		if (!(base.gameObject != null))
		{
			return;
		}
		RandomItemObj randomItemObj = RandomItemMgr.Instance.GetRandomItemObj(genPos);
		if (randomItemObj != null)
		{
			if (randomItemObj.type == RandomObjType.MonsterFeces)
			{
				Singleton<EffectBuilder>.Instance.Register(220, null, base.transform.position, Quaternion.identity);
			}
			RandomItemMgr.Instance.RemoveRandomItemObj(randomItemObj);
			SceneMan.RemoveSceneObj(randomItemObj);
		}
		Object.Destroy(base.gameObject);
	}
}

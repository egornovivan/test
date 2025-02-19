using System.Collections.Generic;
using ItemAsset;
using Pathea;
using UnityEngine;

public class ItemDropPeEntity : ItemDrop
{
	private bool _itemListsUpdated;

	private SkAliveEntity _skAlive;

	private List<IDroppableItemList> _itemLists = new List<IDroppableItemList>();

	private void Awake()
	{
		IniDropCmpts();
	}

	private void Start()
	{
		MousePickablePeEntity mousePickablePeEntity = GetComponent<MousePickablePeEntity>();
		if (mousePickablePeEntity == null)
		{
			mousePickablePeEntity = base.gameObject.AddComponent<MousePickablePeEntity>();
			mousePickablePeEntity.CollectColliders();
		}
		CreateDroppableItemList();
		mousePickablePeEntity.eventor.Subscribe(delegate(object sender, MousePickable.RMouseClickEvent e)
		{
			PeTrans component = e.mousePickable.GetComponent<PeTrans>();
			OpenGui(component.position);
		});
	}

	private void CreateDroppableItemList()
	{
		if (_itemListsUpdated)
		{
			return;
		}
		_itemListsUpdated = true;
		PeEntity component = GetComponent<PeEntity>();
		if (null == component)
		{
			return;
		}
		_skAlive = component.GetCmpt<SkAliveEntity>();
		if (_skAlive == null || !_skAlive.isDead || PeGameMgr.IsMulti)
		{
			return;
		}
		CommonCmpt cmpt = component.GetCmpt<CommonCmpt>();
		if (!(cmpt != null))
		{
			return;
		}
		List<ItemSample> list = ItemDropData.GetDropItems(cmpt.ItemDropId);
		if (cmpt.entityProto.proto == EEntityProto.Monster)
		{
			if (list == null)
			{
				list = GetSpecialItem.MonsterItemAdd(cmpt.entityProto.protoId);
			}
			else
			{
				list.AddRange(GetSpecialItem.MonsterItemAdd(cmpt.entityProto.protoId));
			}
		}
		if (list == null)
		{
			return;
		}
		foreach (ItemSample item in list)
		{
			AddDroppableItem(item);
		}
	}

	private void IniDropCmpts()
	{
		_itemLists.Clear();
		Component[] components = GetComponents(typeof(IDroppableItemList));
		Component[] array = components;
		foreach (Component component in array)
		{
			_itemLists.Add(component as IDroppableItemList);
		}
	}

	private new bool CanFetch(int index)
	{
		if (index < 0 || index >= _itemLists.Count)
		{
			return false;
		}
		return base.playerPkg.package.CanAdd(_itemList[index]);
	}

	private new bool CanFetchAll()
	{
		MaterialItem[] array = new MaterialItem[_itemList.Count];
		for (int i = 0; i < _itemList.Count; i++)
		{
			array[i] = new MaterialItem
			{
				protoId = _itemList[i].protoId,
				count = _itemList[i].stackCount
			};
		}
		return base.playerPkg.package.CanAdd(array);
	}

	private bool ConvertIndex(int gIdx, out int n, out int lIdx)
	{
		n = 0;
		lIdx = gIdx;
		foreach (IDroppableItemList itemList in _itemLists)
		{
			if (lIdx < itemList.DroppableItemCount)
			{
				return true;
			}
			n++;
			lIdx -= itemList.DroppableItemCount;
		}
		return false;
	}

	public override ItemSample Get(int index)
	{
		if (!ConvertIndex(index, out var n, out var lIdx))
		{
			return null;
		}
		return _itemLists[n].GetDroppableItemAt(lIdx);
	}

	public override int GetCount()
	{
		int num = 0;
		foreach (IDroppableItemList itemList in _itemLists)
		{
			num += itemList.DroppableItemCount;
		}
		return num;
	}

	public override void Fetch(int index)
	{
		if (!ConvertIndex(index, out var n, out var lIdx))
		{
			return;
		}
		if (GameConfig.IsMultiClient)
		{
			ItemSample droppableItemAt = _itemLists[n].GetDroppableItemAt(lIdx);
			if (null != PlayerNetwork.mainPlayer && null != _skAlive && null != _skAlive.Entity)
			{
				PlayerNetwork.mainPlayer.RequestDeadObjItem(_skAlive.Entity.Id, lIdx, droppableItemAt.protoId);
			}
			return;
		}
		PeEntity component = base.gameObject.GetComponent<PeEntity>();
		if (component != null && (component.entityProto.proto == EEntityProto.RandomNpc || component.entityProto.proto == EEntityProto.Npc))
		{
			OwnerData.deadNPC = new OwnerData();
			OwnerData.deadNPC.npcID = base.gameObject.GetComponent<PeEntity>().Id;
			OwnerData.deadNPC.npcName = base.gameObject.GetComponent<PeEntity>().name;
		}
		ItemSample droppableItemAt2 = _itemLists[n].GetDroppableItemAt(lIdx);
		if (base.playerPkg.package.CanAdd(droppableItemAt2))
		{
			base.playerPkg.Add(droppableItemAt2.protoId, droppableItemAt2.stackCount);
			_itemLists[n].RemoveDroppableItem(droppableItemAt2);
		}
		if (_skAlive != null && GetCount() == 0)
		{
			Singleton<PeEventGlobal>.Instance.PickupEvent.Invoke(_skAlive);
		}
	}

	public override void FetchAll()
	{
		if (GameConfig.IsMultiClient)
		{
			if (null != PlayerNetwork.mainPlayer && null != _skAlive && null != _skAlive.Entity)
			{
				PlayerNetwork.mainPlayer.RequestDeadObjAllItems(_skAlive.Entity.Id);
			}
			return;
		}
		List<MaterialItem> list = new List<MaterialItem>();
		foreach (IDroppableItemList itemList in _itemLists)
		{
			int droppableItemCount = itemList.DroppableItemCount;
			for (int i = 0; i < droppableItemCount; i++)
			{
				ItemSample droppableItemAt = itemList.GetDroppableItemAt(i);
				list.Add(new MaterialItem
				{
					protoId = droppableItemAt.protoId,
					count = droppableItemAt.stackCount
				});
			}
		}
		if (!base.playerPkg.package.CanAdd(list.ToArray()))
		{
			return;
		}
		PeEntity component = base.gameObject.GetComponent<PeEntity>();
		if (component != null && (component.entityProto.proto == EEntityProto.RandomNpc || component.entityProto.proto == EEntityProto.Npc))
		{
			OwnerData.deadNPC = new OwnerData();
			OwnerData.deadNPC.npcID = base.gameObject.GetComponent<PeEntity>().Id;
			OwnerData.deadNPC.npcName = base.gameObject.GetComponent<PeEntity>().name;
		}
		foreach (MaterialItem item in list)
		{
			base.playerPkg.Add(item.protoId, item.count);
		}
		foreach (IDroppableItemList itemList2 in _itemLists)
		{
			itemList2.RemoveDroppableItemAll();
		}
		if (_skAlive != null && GetCount() == 0)
		{
			Singleton<PeEventGlobal>.Instance.PickupEvent.Invoke(_skAlive);
		}
	}

	public bool NpcCanFetchAll(NpcPackageCmpt npcPackage)
	{
		List<MaterialItem> list = new List<MaterialItem>();
		foreach (IDroppableItemList itemList in _itemLists)
		{
			int droppableItemCount = itemList.DroppableItemCount;
			for (int i = 0; i < droppableItemCount; i++)
			{
				ItemSample droppableItemAt = itemList.GetDroppableItemAt(i);
				list.Add(new MaterialItem
				{
					protoId = droppableItemAt.protoId,
					count = droppableItemAt.stackCount
				});
			}
		}
		return npcPackage.CanAddItemList(list);
	}

	public void NpcFetchAll(NpcPackageCmpt npcPackage)
	{
		List<MaterialItem> list = new List<MaterialItem>();
		foreach (IDroppableItemList itemList in _itemLists)
		{
			int droppableItemCount = itemList.DroppableItemCount;
			for (int i = 0; i < droppableItemCount; i++)
			{
				ItemSample droppableItemAt = itemList.GetDroppableItemAt(i);
				list.Add(new MaterialItem
				{
					protoId = droppableItemAt.protoId,
					count = droppableItemAt.stackCount
				});
			}
		}
		if (!npcPackage.CanAddItemList(list))
		{
			return;
		}
		foreach (MaterialItem item in list)
		{
			npcPackage.Add(item.protoId, item.count);
		}
		foreach (IDroppableItemList itemList2 in _itemLists)
		{
			itemList2.RemoveDroppableItemAll();
		}
		if (_skAlive != null && GetCount() == 0)
		{
			Singleton<PeEventGlobal>.Instance.PickupEvent.Invoke(_skAlive);
		}
	}
}

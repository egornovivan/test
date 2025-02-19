using System.Collections.Generic;
using System.IO;
using ItemAsset;
using ItemAsset.SlotListHelper;
using PETools;

namespace Pathea;

public class NpcPackageCmpt : PackageCmpt
{
	private const int PkgCapacity = 15;

	private const int PrivateCapacity = 30;

	private const int HandinCapacity = 10;

	private SlotList mPrivateSlotList = new SlotList(30);

	private SlotList mSlotList = new SlotList(15);

	private SlotList mHandinList = new SlotList(10);

	public AutoIncreaseMoney mAutoIncreaseMoney;

	private List<ItemObject> _mEquipObjs = new List<ItemObject>();

	public void InitAutoIncreaseMoney(int max, int valuePerDay)
	{
		mAutoIncreaseMoney = new AutoIncreaseMoney(base.money, max, valuePerDay, added: true);
	}

	public SlotList GetPrivateSlotList()
	{
		return mPrivateSlotList;
	}

	public SlotList GetSlotList()
	{
		return mSlotList;
	}

	public SlotList GetHandinList()
	{
		return mHandinList;
	}

	public bool IsFull()
	{
		return mSlotList.GetVacancyCount() == 0 && mHandinList.GetVacancyCount() == 0;
	}

	public bool HandinIsFull()
	{
		return mHandinList.GetVacancyCount() == 0;
	}

	public override bool Add(ItemObject item, bool isNew = false)
	{
		if (IsFull())
		{
			return mHandinList.Add(item, isNew);
		}
		return mSlotList.Add(item, isNew);
	}

	public bool AddToPrivate(ItemObject item)
	{
		return mPrivateSlotList.Add(item);
	}

	public bool AddToNetHandin(ItemObject item)
	{
		return mHandinList.Add(item, isNew: true);
	}

	public bool AddToNet(ItemObject item)
	{
		return mSlotList.Add(item, isNew: true);
	}

	public bool AddToHandin(ItemObject item)
	{
		if (!HandinIsFull())
		{
			return mHandinList.Add(item, isNew: true);
		}
		return mSlotList.Add(item, isNew: true);
	}

	public override bool Remove(ItemObject item)
	{
		int num = mSlotList.FindItemIndexById(item.instanceId);
		if (num != -1)
		{
			mSlotList[num] = null;
		}
		num = mHandinList.FindItemIndexById(item.instanceId);
		if (num == -1)
		{
			return false;
		}
		mHandinList[num] = null;
		return true;
	}

	public override bool Contain(ItemObject item)
	{
		int num = mSlotList.FindItemIndexById(item.instanceId);
		if (num != -1)
		{
			return true;
		}
		num = mHandinList.FindItemIndexById(item.instanceId);
		if (num != -1)
		{
			return true;
		}
		return false;
	}

	public bool CanAddHandinItemList(List<ItemObject> items)
	{
		return mHandinList.GetVacancyCount() >= items.Count;
	}

	public bool CanAddPrivateItemList(List<ItemObject> items)
	{
		return mPrivateSlotList.GetVacancyCount() >= items.Count;
	}

	public bool CanAddItemList(List<MaterialItem> items)
	{
		return mSlotList.GetVacancyCount() + mHandinList.GetVacancyCount() >= items.Count;
	}

	public bool CanAddHandinItemList(List<MaterialItem> items)
	{
		return mHandinList.GetVacancyCount() >= items.Count;
	}

	public bool AddPrivateItemList(List<ItemObject> items)
	{
		if (!CanAddPrivateItemList(items))
		{
			return false;
		}
		foreach (ItemObject item in items)
		{
			if (!AddToPrivate(item))
			{
				return false;
			}
		}
		return true;
	}

	public bool AddHandinItemList(List<ItemObject> items)
	{
		if (!CanAddHandinItemList(items))
		{
			return false;
		}
		foreach (ItemObject item in items)
		{
			if (!AddToHandin(item))
			{
				return false;
			}
		}
		return true;
	}

	public void ClearHandin()
	{
		mHandinList.Clear();
	}

	public void Clear()
	{
		mSlotList.Clear();
	}

	public override bool CanAddItemList(List<ItemObject> items)
	{
		return mSlotList.GetVacancyCount() + mHandinList.GetVacancyCount() >= items.Count;
	}

	public override bool AddItemList(List<ItemObject> items)
	{
		if (!CanAddItemList(items))
		{
			return false;
		}
		foreach (ItemObject item in items)
		{
			if (!Add(item))
			{
				return false;
			}
		}
		return true;
	}

	public override bool AdditemFromEquip(List<ItemObject> items)
	{
		return AddItemList(items);
	}

	public override int GetItemCount(int protoId)
	{
		return mSlotList.GetCount(protoId) + mHandinList.GetCount(protoId);
	}

	public override bool ContainsItem(int protoId)
	{
		return mSlotList.ConatinsItem(protoId) || mHandinList.ConatinsItem(protoId);
	}

	public override int GetCountByEditorType(int editorType)
	{
		return mSlotList.GetCountByEditorType(editorType) + mHandinList.GetCountByEditorType(editorType);
	}

	public override int GetAllItemsCount()
	{
		return mSlotList.GetAllItemsCount() + mHandinList.GetAllItemsCount();
	}

	public override bool Destory(int protoId, int count)
	{
		return mSlotList.Destroy(protoId, count) || mHandinList.Destroy(protoId, count);
	}

	public override bool DestroyItem(int instanceId, int count)
	{
		return mSlotList.DestroyItem(instanceId, count) || mHandinList.DestroyItem(instanceId, count);
	}

	public override bool DestroyItem(ItemObject item, int count)
	{
		return mSlotList.DestroyItem(item.instanceId, count) || mHandinList.DestroyItem(item.instanceId, count);
	}

	public override bool Add(int protoId, int count)
	{
		if (IsFull())
		{
			return mHandinList.Add(protoId, count);
		}
		return mSlotList.Add(protoId, count);
	}

	public bool AddToHandin(int protoId, int count)
	{
		if (!HandinIsFull())
		{
			return mHandinList.Add(protoId, count, isNew: true);
		}
		return mSlotList.Add(protoId, count, isNew: true);
	}

	public List<ItemObject> GetEquipItemObjs(EeqSelect selet)
	{
		List<ItemObject> list = new List<ItemObject>();
		list.AddRange(mSlotList.mEquipItem.GetEquipItemObjs(selet));
		list.AddRange(mHandinList.mEquipItem.GetEquipItemObjs(selet));
		return list;
	}

	public List<ItemObject> GetAtkEquipItemObjs(AttackType Atktype)
	{
		List<ItemObject> list = new List<ItemObject>();
		list.AddRange(mSlotList.mEquipItem.GetAtkEquips(Atktype));
		list.AddRange(mHandinList.mEquipItem.GetAtkEquips(Atktype));
		return list;
	}

	public bool HasEq(EeqSelect Select)
	{
		return GetEquipItemObjs(Select).Count > 0;
	}

	public bool HasAtkEquip(AttackType type)
	{
		return mSlotList.mEquipItem.hasAtkEquip(type) || mHandinList.mEquipItem.hasAtkEquip(type);
	}

	public override bool Set(int protoId, int count)
	{
		return mSlotList.Set(protoId, count);
	}

	public override void OnUpdate()
	{
		base.OnUpdate();
		if (mAutoIncreaseMoney != null)
		{
			mAutoIncreaseMoney.Update();
		}
	}

	public override void Serialize(BinaryWriter w)
	{
		base.Serialize(w);
		PETools.Serialize.WriteBytes(mSlotList.Export(), w);
		PETools.Serialize.WriteBytes(mPrivateSlotList.Export(), w);
		if (mAutoIncreaseMoney != null)
		{
			PETools.Serialize.WriteBytes(mAutoIncreaseMoney.Export(), w);
		}
		else
		{
			PETools.Serialize.WriteBytes(null, w);
		}
		if (mHandinList != null)
		{
			PETools.Serialize.WriteBytes(mHandinList.Export(), w);
		}
		else
		{
			PETools.Serialize.WriteBytes(null, w);
		}
	}

	public override void Deserialize(BinaryReader r)
	{
		base.Deserialize(r);
		byte[] buffer = PETools.Serialize.ReadBytes(r);
		byte[] buffer2 = PETools.Serialize.ReadBytes(r);
		byte[] array = PETools.Serialize.ReadBytes(r);
		byte[] array2 = PETools.Serialize.ReadBytes(r);
		mSlotList.Import(buffer);
		mPrivateSlotList.Import(buffer2);
		if (array != null && array.Length > 0)
		{
			mAutoIncreaseMoney = new AutoIncreaseMoney(base.money);
			mAutoIncreaseMoney.Import(array);
		}
		else
		{
			mAutoIncreaseMoney = null;
		}
		if (array2 != null && array2.Length > 0)
		{
			mHandinList.Import(array2);
		}
	}
}

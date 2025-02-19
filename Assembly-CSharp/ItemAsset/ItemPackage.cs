using System.Collections.Generic;
using System.IO;
using ItemAsset.SlotListHelper;
using PeEvent;
using PETools;

namespace ItemAsset;

public class ItemPackage
{
	public enum ESlotType
	{
		None = -1,
		Item,
		Equipment,
		Resource,
		Armor,
		Max
	}

	public class EventArg : PeEvent.EventArg
	{
		public enum Op
		{
			Put,
			Reset,
			Clear,
			Update,
			Max
		}

		public Op op;

		public ItemObject itemObj;
	}

	private Event<EventArg> mEventor = new Event<EventArg>();

	private SlotList[] mSlotListArray = new SlotList[4];

	public static int InvalidIndex = -1;

	public Event<EventArg> changeEventor => mEventor;

	public ItemPackage()
	{
	}

	public ItemPackage(int capacity)
	{
		ExtendPackage(capacity, capacity, capacity, capacity);
	}

	private void Resend(ESlotType slotType, SlotList.ChangeEvent arg)
	{
		switch (arg.op)
		{
		case SlotList.ChangeEvent.Op.Set:
			changeEventor.Dispatch(new EventArg
			{
				op = EventArg.Op.Put,
				itemObj = GetItem(slotType, arg.index)
			});
			break;
		case SlotList.ChangeEvent.Op.Reset:
			changeEventor.Dispatch(new EventArg
			{
				op = EventArg.Op.Reset,
				itemObj = GetItem(slotType, arg.index)
			});
			break;
		case SlotList.ChangeEvent.Op.Update:
			changeEventor.Dispatch(new EventArg
			{
				op = EventArg.Op.Update,
				itemObj = null
			});
			break;
		case SlotList.ChangeEvent.Op.Clear:
			break;
		case SlotList.ChangeEvent.Op.Sort:
			break;
		}
	}

	private void ItemSlotListMsgHandler(object sender, SlotList.ChangeEvent arg)
	{
		Resend(ESlotType.Item, arg);
	}

	private void EquipmentSlotListMsgHandler(object sender, SlotList.ChangeEvent arg)
	{
		Resend(ESlotType.Equipment, arg);
	}

	private void ResourceSlotListMsgHandler(object sender, SlotList.ChangeEvent arg)
	{
		Resend(ESlotType.Resource, arg);
	}

	private void ArmorSlotListMsgHandler(object sender, SlotList.ChangeEvent arg)
	{
		Resend(ESlotType.Armor, arg);
	}

	private void RegisterSlotEvent(ESlotType slotType)
	{
		SlotList slotList = GetSlotList(slotType);
		switch (slotType)
		{
		case ESlotType.Item:
			slotList?.eventor.Subscribe(ItemSlotListMsgHandler);
			break;
		case ESlotType.Equipment:
			slotList?.eventor.Subscribe(EquipmentSlotListMsgHandler);
			break;
		case ESlotType.Resource:
			slotList?.eventor.Subscribe(ResourceSlotListMsgHandler);
			break;
		case ESlotType.Armor:
			slotList?.eventor.Subscribe(ArmorSlotListMsgHandler);
			break;
		}
	}

	public void ExtendPackage(int itemMax, int equipmentMax, int recourceMax, int armorMax)
	{
		SetSlotList(ESlotType.Item, SlotList.ResetCapacity(GetSlotList(), itemMax));
		SetSlotList(ESlotType.Equipment, SlotList.ResetCapacity(GetSlotList(ESlotType.Equipment), equipmentMax));
		SetSlotList(ESlotType.Resource, SlotList.ResetCapacity(GetSlotList(ESlotType.Resource), recourceMax));
		SetSlotList(ESlotType.Armor, SlotList.ResetCapacity(GetSlotList(ESlotType.Armor), armorMax));
	}

	public void UpdateNewFlag(float deltaTime)
	{
		for (int i = 0; i < mSlotListArray.Length; i++)
		{
			SlotList slotList = mSlotListArray[i];
			slotList.UpdateNewFlag(deltaTime);
		}
	}

	public void Clear(ESlotType type = ESlotType.Max)
	{
		if (type == ESlotType.Max)
		{
			GetSlotList().Clear();
			GetSlotList(ESlotType.Equipment).Clear();
			GetSlotList(ESlotType.Resource).Clear();
			GetSlotList(ESlotType.Armor).Clear();
		}
		else
		{
			GetSlotList(type).Clear();
		}
		changeEventor.Dispatch(new EventArg
		{
			op = EventArg.Op.Clear,
			itemObj = null
		});
	}

	private bool SetSlotList(ESlotType itemClass, SlotList list)
	{
		if (itemClass == ESlotType.Max)
		{
			return false;
		}
		mSlotListArray[(int)itemClass] = list;
		RegisterSlotEvent(itemClass);
		return true;
	}

	public SlotList GetSlotList(int prototypeId)
	{
		return GetSlotList(GetSlotType(prototypeId));
	}

	public SlotList GetSlotList(ESlotType itemClass = ESlotType.Item)
	{
		if (itemClass == ESlotType.Max)
		{
			return null;
		}
		return mSlotListArray[(int)itemClass];
	}

	public bool CanAddItemList(IEnumerable<ItemObject> items)
	{
		int[] array = new int[4];
		foreach (ItemObject item in items)
		{
			array[item.protoData.tabIndex]++;
		}
		for (int i = 0; i < 4; i++)
		{
			SlotList slotList = GetSlotList((ESlotType)i);
			if (slotList.GetVacancyCount() < array[i])
			{
				return false;
			}
		}
		return true;
	}

	public bool AddItemList(IEnumerable<ItemObject> items)
	{
		foreach (ItemObject item in items)
		{
			AddItem(item);
		}
		return true;
	}

	public static ESlotType GetSlotType(int prototypeId)
	{
		return (ESlotType)(ItemProto.GetItemData(prototypeId)?.tabIndex ?? 4);
	}

	public ItemObject FindItemByProtoId(int protoId)
	{
		return GetSlotList(protoId)?.FindItemByProtoId(protoId);
	}

	public int FindItemIndexByProtoId(int protoId)
	{
		ESlotType slotType = GetSlotType(protoId);
		SlotList slotList = GetSlotList(slotType);
		if (slotList == null)
		{
			return -1;
		}
		int slotIndex = slotList.FindItemIndexByProtoId(protoId);
		return CodeIndex(slotType, slotIndex);
	}

	private void SetItem(SlotList slotList, ItemObject item, int index, bool isNew = false)
	{
		slotList[index] = item;
		if (isNew)
		{
			slotList.newFlagMgr.Add(index);
		}
		else
		{
			slotList.newFlagMgr.Remove(index);
		}
		EventArg eventArg = new EventArg();
		eventArg.op = ((item == null) ? EventArg.Op.Reset : EventArg.Op.Put);
		eventArg.itemObj = item;
		EventArg arg = eventArg;
		changeEventor.Dispatch(arg);
	}

	public int AddItem(ItemObject itemObject, bool isNew = false)
	{
		ESlotType slotType = GetSlotType(itemObject.protoId);
		SlotList slotList = GetSlotList(slotType);
		int num = slotList.VacancyIndex();
		if (num == -1)
		{
			return InvalidIndex;
		}
		SetItem(slotList, itemObject, num, isNew);
		return CodeIndex(slotType, num);
	}

	public int PutItem(ItemObject item, int slotIndex, ESlotType slotType)
	{
		if (slotType == ESlotType.Max)
		{
			slotType = (ESlotType)item.protoData.tabIndex;
		}
		SlotList slotList = GetSlotList(slotType);
		SetItem(slotList, item, slotIndex);
		return CodeIndex(slotType, slotIndex);
	}

	public void PutItem(ItemObject item, int codedIndex)
	{
		if (DecodeIndex(codedIndex, out var type, out var slotIndex))
		{
			PutItem(item, slotIndex, type);
		}
	}

	public ItemObject GetItem(int codeIndex)
	{
		if (!DecodeIndex(codeIndex, out var type, out var slotIndex))
		{
			return null;
		}
		return GetItem(type, slotIndex);
	}

	public ItemObject GetItem(ESlotType slotType, int index)
	{
		return GetSlotList(slotType)?[index];
	}

	public bool RemoveItem(int codedIndex)
	{
		if (!DecodeIndex(codedIndex, out var type, out var slotIndex))
		{
			return false;
		}
		SlotList slotList = GetSlotList(type);
		if (slotList == null)
		{
			return false;
		}
		SetItem(slotList, null, slotIndex);
		return true;
	}

	public static int CodeIndex(ESlotType type, int slotIndex)
	{
		if (type == ESlotType.Max || slotIndex < 0)
		{
			return InvalidIndex;
		}
		return slotIndex | ((int)type << 24);
	}

	public static bool DecodeIndex(int index, out ESlotType type, out int slotIndex)
	{
		type = (ESlotType)(index >> 24);
		slotIndex = index & 0xFFFFFF;
		if (index == InvalidIndex)
		{
			return false;
		}
		return true;
	}

	public int GetItemIndexById(int instanceId)
	{
		for (int i = 0; i < 4; i++)
		{
			SlotList slotList = mSlotListArray[i];
			if (slotList != null)
			{
				int num = slotList.FindItemIndexById(instanceId);
				if (num != -1)
				{
					return CodeIndex((ESlotType)i, num);
				}
			}
		}
		return InvalidIndex;
	}

	public bool RemoveItemById(int instanceId)
	{
		int itemIndexById = GetItemIndexById(instanceId);
		return RemoveItem(itemIndexById);
	}

	public bool RemoveItem(ItemObject item)
	{
		return RemoveItemById(item.instanceId);
	}

	public bool HasItemObj(ItemObject itemObject)
	{
		if (itemObject == null)
		{
			return false;
		}
		return GetSlotList(itemObject.protoId)?.HasItem(itemObject.instanceId) ?? false;
	}

	public int GetVacancySlotIndex(int slotType = 0)
	{
		return GetVacancySlotIndex((ESlotType)slotType);
	}

	public int GetVacancySlotIndex(ESlotType slotType)
	{
		return GetSlotList(slotType)?.VacancyIndex() ?? (-1);
	}

	public void Export(BinaryWriter w)
	{
		for (int i = 0; i < 4; i++)
		{
			byte[] buff = null;
			SlotList slotList = GetSlotList((ESlotType)i);
			if (slotList != null)
			{
				buff = slotList.Export();
			}
			Serialize.WriteBytes(buff, w);
		}
	}

	public void Import(byte[] buffer)
	{
		using MemoryStream input = new MemoryStream(buffer, writable: false);
		using BinaryReader r = new BinaryReader(input);
		for (int i = 0; i < 4; i++)
		{
			byte[] array = Serialize.ReadBytes(r);
			if (array != null && array.Length > 0)
			{
				SlotList slotList = new SlotList();
				slotList.Import(array);
				SetSlotList((ESlotType)i, slotList);
			}
		}
	}

	public void Sort(ESlotType type)
	{
		SlotList slotList = GetSlotList(type);
		if (slotList != null)
		{
			slotList.Reduce();
			slotList.Sort();
		}
	}
}

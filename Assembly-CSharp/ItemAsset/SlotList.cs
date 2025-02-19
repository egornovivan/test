using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Pathea;
using PeEvent;
using UnityEngine;

namespace ItemAsset;

public class SlotList : IEnumerable, IEnumerable<ItemObject>
{
	public class ChangeEvent : EventArg
	{
		public enum Op
		{
			Set,
			Reset,
			Clear,
			Sort,
			Update,
			Max
		}

		public Op op;

		public int index;
	}

	private const int VERSION_0000 = 0;

	private const int CURRENT_VERSION = 0;

	private ItemObject[] mSlots;

	private NewFlagMgr mNewFlagMgr;

	private EquipItemMgr _equipItem;

	private Event<ChangeEvent> mEventor = new Event<ChangeEvent>();

	public NewFlagMgr newFlagMgr
	{
		get
		{
			if (mNewFlagMgr == null)
			{
				mNewFlagMgr = new NewFlagMgr();
			}
			return mNewFlagMgr;
		}
	}

	public EquipItemMgr mEquipItem
	{
		get
		{
			if (_equipItem == null)
			{
				_equipItem = new EquipItemMgr();
			}
			return _equipItem;
		}
	}

	public Event<ChangeEvent> eventor => mEventor;

	public ItemObject this[int index]
	{
		get
		{
			if (index < 0 || index >= Length)
			{
				return null;
			}
			return mSlots[index];
		}
		set
		{
			if (index >= 0 && index < Length)
			{
				if (mSlots[index] == null && value != null)
				{
					mEquipItem.Add(value);
				}
				if (mSlots[index] != null && value == null)
				{
					mEquipItem.ReMove(mSlots[index]);
				}
				if (mSlots[index] != null && value != null)
				{
					mEquipItem.ReMove(mSlots[index]);
					mEquipItem.Add(value);
				}
				mSlots[index] = value;
				eventor.Dispatch(new ChangeEvent
				{
					op = ((value == null) ? ChangeEvent.Op.Reset : ChangeEvent.Op.Set),
					index = index
				});
				if (mSlots[index] == null)
				{
					newFlagMgr.Remove(index);
				}
			}
		}
	}

	public int vacancyCount => Length - GetItemCount();

	public int Count => Length;

	public int Length => mSlots.Length;

	public SlotList(int capacity)
	{
		mSlots = new ItemObject[capacity];
		_equipItem = new EquipItemMgr();
	}

	public SlotList()
	{
	}

	IEnumerator<ItemObject> IEnumerable<ItemObject>.GetEnumerator()
	{
		return mSlots.AsEnumerable().GetEnumerator();
	}

	IEnumerator IEnumerable.GetEnumerator()
	{
		return mSlots.GetEnumerator();
	}

	public void SendUpdateEvent()
	{
		eventor.Dispatch(new ChangeEvent
		{
			op = ChangeEvent.Op.Update,
			index = -1
		});
	}

	private void AddEquipItem(ItemObject obj)
	{
		mEquipItem.Add(obj);
	}

	public void Clear()
	{
		for (int i = 0; i < mSlots.Length; i++)
		{
			mSlots[i] = null;
		}
		mEquipItem.Clear();
		eventor.Dispatch(new ChangeEvent
		{
			op = ChangeEvent.Op.Clear,
			index = -1
		});
	}

	public void Sort()
	{
		List<ItemObject> list = new List<ItemObject>(mSlots);
		list.RemoveAll((ItemObject item) => item == null);
		list.Sort(delegate(ItemObject item1, ItemObject item2)
		{
			if (item1.protoData.sortLabel > item2.protoData.sortLabel)
			{
				return 1;
			}
			if (item1.protoData.sortLabel < item2.protoData.sortLabel)
			{
				return -1;
			}
			if (item1.protoId > item2.protoId)
			{
				return 1;
			}
			if (item1.protoId < item2.protoId)
			{
				return -1;
			}
			if (item1.stackCount > item2.stackCount)
			{
				return 1;
			}
			return (item1.stackCount < item2.stackCount) ? (-1) : 0;
		});
		for (int i = 0; i < mSlots.Length; i++)
		{
			if (i >= list.Count)
			{
				mSlots[i] = null;
			}
			else
			{
				this[i] = list[i];
			}
		}
		eventor.Dispatch(new ChangeEvent
		{
			op = ChangeEvent.Op.Sort,
			index = -1
		});
		newFlagMgr.RemoveAll();
	}

	public void UpdateNewFlag(float deltaTime)
	{
		newFlagMgr.Update(deltaTime);
	}

	public bool Add(ItemObject itemObject, bool isNew = false)
	{
		if (itemObject == null)
		{
			return false;
		}
		int num = VacancyIndex();
		if (num == -1)
		{
			return false;
		}
		if (isNew)
		{
			newFlagMgr.Add(num);
		}
		this[num] = itemObject;
		if (itemObject.GetCmpt<OwnerData>() != null && OwnerData.deadNPC != null)
		{
			itemObject.GetCmpt<OwnerData>().npcID = OwnerData.deadNPC.npcID;
			itemObject.GetCmpt<OwnerData>().npcName = OwnerData.deadNPC.npcName;
		}
		return true;
	}

	public int VacancyIndex()
	{
		for (int i = 0; i < Length; i++)
		{
			if (this[i] == null)
			{
				return i;
			}
		}
		return -1;
	}

	public int GetVacancyCount()
	{
		return Length - GetItemCount();
	}

	public int GetItemCount()
	{
		int num = 0;
		for (int i = 0; i < mSlots.Length; i++)
		{
			if (mSlots[i] != null)
			{
				num++;
			}
		}
		return num;
	}

	public void OderByIsNull()
	{
		mSlots = mSlots.OrderBy((ItemObject a) => a == null).ToArray();
	}

	public bool Swap(int index1, int index2)
	{
		if (index1 >= Length || index2 > Length || index1 < 0 || index2 < 0)
		{
			return false;
		}
		ItemObject value = this[index1];
		this[index1] = this[index2];
		this[index2] = value;
		newFlagMgr.Remove(index1);
		newFlagMgr.Remove(index2);
		return true;
	}

	public List<ItemObject> ToList()
	{
		List<ItemObject> list = new List<ItemObject>(mSlots);
		list.RemoveAll((ItemObject item) => item == null);
		return list;
	}

	public int FindItemIndexByProtoId(int protoId)
	{
		for (int i = 0; i < Length; i++)
		{
			if (mSlots[i] != null && protoId == mSlots[i].protoId)
			{
				return i;
			}
		}
		return -1;
	}

	public ItemObject FindItemByProtoId(int protoId)
	{
		return this[FindItemIndexByProtoId(protoId)];
	}

	public int FindItemIndexById(int instanceId)
	{
		for (int i = 0; i < Length; i++)
		{
			if (mSlots[i] != null && instanceId == mSlots[i].instanceId)
			{
				return i;
			}
		}
		return -1;
	}

	public bool HasItem(int instanceId)
	{
		return -1 != FindItemIndexById(instanceId);
	}

	public static SlotList ResetCapacity(SlotList origin, int capacity)
	{
		SlotList slotList = new SlotList(capacity);
		if (origin == null)
		{
			return slotList;
		}
		Array.Copy(origin.mSlots, slotList.mSlots, Mathf.Min(origin.Length, capacity));
		return slotList;
	}

	public byte[] Export()
	{
		using MemoryStream memoryStream = new MemoryStream();
		using (BinaryWriter binaryWriter = new BinaryWriter(memoryStream))
		{
			binaryWriter.Write(0);
			binaryWriter.Write(Length);
			for (int i = 0; i < Length; i++)
			{
				ItemObject itemObject = this[i];
				if (itemObject != null)
				{
					binaryWriter.Write(itemObject.instanceId);
				}
				else
				{
					binaryWriter.Write(-1);
				}
			}
		}
		return memoryStream.ToArray();
	}

	public void Import(byte[] buffer)
	{
		using MemoryStream input = new MemoryStream(buffer, writable: false);
		using BinaryReader binaryReader = new BinaryReader(input);
		int num = binaryReader.ReadInt32();
		if (num > 0)
		{
			Debug.LogError("error version:" + num);
		}
		int num2 = binaryReader.ReadInt32();
		if (mSlots == null || mSlots.Length < num2)
		{
			mSlots = new ItemObject[num2];
		}
		else
		{
			Array.Clear(mSlots, 0, mSlots.Length);
		}
		for (int i = 0; i < num2; i++)
		{
			int id = binaryReader.ReadInt32();
			ItemObject itemObject = PeSingleton<ItemMgr>.Instance.Get(id);
			mSlots[i] = itemObject;
			AddEquipItem(itemObject);
		}
	}
}

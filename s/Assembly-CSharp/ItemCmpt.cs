using System.Collections.Generic;
using System.IO;
using System.Linq;
using ItemAsset;
using UnityEngine;

public class ItemCmpt : DataCmpt
{
	private int m_MaxCapacity;

	protected List<ItemObject> mItems = new List<ItemObject>();

	public int ItemCount => mItems.Count;

	public int Capacity => m_MaxCapacity;

	public int EmptyCount => m_MaxCapacity - ItemCount;

	public ItemObject this[int id] => mItems.Find((ItemObject iter) => iter.instanceId == id);

	public ItemCmpt()
	{
		mType = ECmptType.Item;
	}

	public override void Export(BinaryWriter w)
	{
		base.Export(w);
		BufferHelper.Serialize(w, mItems.Count);
		foreach (ItemObject mItem in mItems)
		{
			byte[] value = mItem.Export();
			BufferHelper.Serialize(w, value);
		}
	}

	public override void Import(BinaryReader r)
	{
		base.Import(r);
		int num = BufferHelper.ReadInt32(r);
		for (int i = 0; i < num; i++)
		{
			byte[] array = BufferHelper.ReadBytes(r);
			if (array != null && array.Length > 0)
			{
				ItemObject itemObject = ItemObject.Deserialize(array);
				if (itemObject != null)
				{
					mItems.Add(itemObject);
				}
			}
		}
	}

	public void Extend(int capacity)
	{
		m_MaxCapacity = capacity;
	}

	public void AddItem(ItemObject item)
	{
		if (item != null && !mItems.Contains(item))
		{
			mItems.Add(item);
		}
	}

	public void AddItem(IEnumerable<ItemObject> items)
	{
		foreach (ItemObject item in items)
		{
			mItems.Add(item);
		}
	}

	public void AddItem(int protoId, int num, ref List<ItemObject> effItems)
	{
		ItemManager.CreateItems(protoId, num, ref effItems);
		AddItem(effItems);
	}

	public void DelItem(int id)
	{
		mItems.RemoveAll((ItemObject iter) => iter.instanceId == id);
	}

	public void DelItem(ItemObject item)
	{
		if (item != null && mItems.Contains(item))
		{
			mItems.Remove(item);
		}
	}

	public void DelItem(ItemSample item)
	{
		mItems.RemoveAll((ItemObject iter) => iter.protoId == item.protoId);
	}

	public int DelItem(int protoId, int num, ref List<ItemObject> effItems)
	{
		int num2 = 0;
		for (int i = 0; i < ItemCount; i++)
		{
			if (num == 0)
			{
				break;
			}
			if (mItems[i].protoId == protoId)
			{
				int num3 = Mathf.Min(num, mItems[i].stackCount);
				if (!mItems[i].CountDown(num3))
				{
					ItemManager.RemoveItem(mItems[i].instanceId);
					mItems.RemoveAt(i);
				}
				else
				{
					effItems.Add(mItems[i]);
				}
				num2 += num3;
				num -= num3;
			}
		}
		return num2;
	}

	public IEnumerable<int> GetItemIds()
	{
		foreach (ItemObject item in mItems)
		{
			yield return item.instanceId;
		}
	}

	public ItemObject[] GetItems()
	{
		return mItems.ToArray();
	}

	public ItemObject[] GetItems(int protoId)
	{
		return mItems.Where((ItemObject iter) => iter.protoId == protoId).ToArray();
	}

	public bool HasItem(ItemObject item)
	{
		return mItems.Contains(item);
	}

	public bool HasItem(int id)
	{
		return mItems.Exists((ItemObject iter) => iter.instanceId == id);
	}

	public bool CanAdd(int num)
	{
		return mItems.Count + num <= m_MaxCapacity;
	}

	public int ItemIndex(int id)
	{
		return mItems.FindIndex((ItemObject iter) => iter.instanceId == id);
	}

	public ItemObject IndexItem(int index)
	{
		return mItems[index];
	}

	public void Clear()
	{
		mItems.Clear();
	}

	public void Resort()
	{
		for (int i = 0; i < mItems.Count; i++)
		{
			if (mItems[i].MaxStackNum == 1 || mItems[i].MaxStackNum == 0)
			{
				continue;
			}
			int num = i + 1;
			int num2 = mItems[i].MaxStackNum - mItems[i].stackCount;
			while (num != mItems.Count)
			{
				if (mItems[i].protoId == mItems[num].protoId && mItems[num].MaxStackNum != mItems[num].stackCount)
				{
					if (num2 < mItems[num].stackCount)
					{
						mItems[i].CountUp(num2);
						mItems[num].CountDown(num2);
						break;
					}
					mItems[i].CountUp(mItems[num].stackCount);
					num2 -= mItems[num].stackCount;
					mItems.RemoveAt(num);
				}
				else
				{
					num++;
				}
			}
		}
		mItems.Sort(delegate(ItemObject l, ItemObject r)
		{
			int num3 = l.protoId.CompareTo(r.protoId);
			return (num3 != 0) ? num3 : l.stackCount.CompareTo(r.stackCount);
		});
	}
}

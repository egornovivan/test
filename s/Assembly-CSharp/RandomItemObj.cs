using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class RandomItemObj
{
	public int boxId;

	public Vector3 position;

	public int id;

	public Quaternion rot;

	public string path;

	public List<int> rareItemInstance = new List<int>();

	public List<ItemIdCount> rareItemProto = new List<ItemIdCount>();

	public int[] items;

	public GameObject gameObj;

	public int Id
	{
		get
		{
			return id;
		}
		set
		{
			id = value;
		}
	}

	public GameObject Go => gameObj;

	public Vector3 Pos => position;

	public bool IgnoreY => false;

	public bool IsStatic => true;

	public RandomItemObj(int templateId, Vector3 pos, Quaternion rot, List<int> itemIdNum, string path, int id = 0)
	{
		boxId = templateId;
		position = pos;
		this.rot = rot;
		this.id = id;
		items = new int[itemIdNum.Count];
		this.path = path;
		for (int i = 0; i < itemIdNum.Count; i++)
		{
			items[i] = itemIdNum[i];
		}
	}

	public RandomItemObj(Vector3 pos, Quaternion rot, int[] itemIdNum, string path = "Prefab/RandomItems/random_box01", int id = 0)
	{
		position = pos;
		this.rot = rot;
		this.id = id;
		items = itemIdNum;
		this.path = path;
	}

	public RandomItemObj(int[] itemIdNum, Vector3 pos, Quaternion rot)
	{
		position = pos;
		this.rot = rot;
		id = 0;
		items = itemIdNum;
	}

	public void AddRareProto(int id, int count)
	{
		rareItemProto.Add(new ItemIdCount(id, count));
	}

	public void AddRareInstance(int id)
	{
		rareItemInstance.Add(id);
		if (rareItemProto.Count > 0)
		{
			rareItemProto.RemoveAt(0);
		}
	}

	public bool CanFetch(int index, int protoId, int count, out int removeIndex)
	{
		removeIndex = 0;
		int num = index * 2;
		if (num + 1 >= items.Count() || items[num] != protoId || items[num + 1] != count)
		{
			for (int i = 0; i < items.Count() && i + 1 < items.Count(); i += 2)
			{
				if (items[i] == protoId && items[i + 1] == count)
				{
					removeIndex = i / 2;
					return true;
				}
			}
			return false;
		}
		removeIndex = index;
		return true;
	}

	public bool TryFetch(int index, int protoId, int count)
	{
		if (CanFetch(index, protoId, count, out var removeIndex))
		{
			List<ItemIdCount> list = GetItems();
			list.RemoveAt(removeIndex);
			SaveItems(list);
			if (list.Count == 0)
			{
				RandomItemMgr.Instance.RemoveRandomItemObj(this);
			}
			return true;
		}
		return false;
	}

	public List<ItemIdCount> TryFetchAll()
	{
		List<ItemIdCount> result = GetItems();
		SaveItems(new List<ItemIdCount>());
		RandomItemMgr.Instance.RemoveRandomItemObj(this);
		return result;
	}

	public List<ItemIdCount> GetItems()
	{
		List<ItemIdCount> list = new List<ItemIdCount>();
		for (int i = 0; i < items.Count(); i += 2)
		{
			if (items[i + 1] > 0)
			{
				list.Add(new ItemIdCount(items[i], items[i + 1]));
			}
		}
		return list;
	}

	public void SaveItems(List<ItemIdCount> itemlist)
	{
		items = new int[itemlist.Count * 2];
		int num = 0;
		foreach (ItemIdCount item in itemlist)
		{
			items[num++] = item.protoId;
			items[num++] = item.count;
		}
	}
}

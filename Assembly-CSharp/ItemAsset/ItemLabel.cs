using System;
using System.Collections.Generic;
using Mono.Data.SqliteClient;
using UnityEngine;

namespace ItemAsset;

public class ItemLabel
{
	public enum Root
	{
		all = 0,
		weapon = 1,
		equipment = 2,
		tool = 3,
		turret = 4,
		consumables = 5,
		resoure = 6,
		part = 7,
		decoration = 8,
		ISO = 100
	}

	private class LabelItem
	{
		public int typeId;

		public Root rootParent;

		public int directParent;

		public string typeName;
	}

	private static Dictionary<int, LabelItem> dicItems;

	public static void Clear()
	{
		dicItems.Clear();
	}

	public static void LoadData()
	{
		dicItems = new Dictionary<int, LabelItem>(22);
		SqliteDataReader sqliteDataReader = LocalDatabase.Instance.ReadFullTable("itemtype");
		while (sqliteDataReader.Read())
		{
			int num = Convert.ToByte(sqliteDataReader.GetString(0));
			string @string = PELocalization.GetString(Convert.ToInt32(sqliteDataReader.GetString(1)));
			int directParent = Convert.ToInt32(sqliteDataReader.GetString(2));
			LabelItem labelItem = new LabelItem();
			labelItem.typeId = num;
			labelItem.typeName = @string;
			labelItem.directParent = directParent;
			LabelItem value = labelItem;
			dicItems.Add(num, value);
		}
		CalculateRootParent();
	}

	public static int GetParent(int typeId)
	{
		LabelItem labelItem = dicItems[typeId];
		if (labelItem == null)
		{
			Debug.LogError("not exist typeId:" + typeId);
			return -2;
		}
		return labelItem.directParent;
	}

	private static int GetRootParentRecursive(int typeId)
	{
		int parent = GetParent(typeId);
		return parent switch
		{
			-1 => typeId, 
			-2 => -2, 
			_ => GetRootParentRecursive(parent), 
		};
	}

	private static void CalculateRootParent()
	{
		foreach (LabelItem value in dicItems.Values)
		{
			int rootParentRecursive = GetRootParentRecursive(value.typeId);
			if (rootParentRecursive != -2)
			{
				value.rootParent = (Root)rootParentRecursive;
			}
		}
	}

	public static Root GetRootParent(int typeId)
	{
		if (!dicItems.ContainsKey(typeId))
		{
			return Root.all;
		}
		return dicItems[typeId].rootParent;
	}

	public static string GetName(int typeId)
	{
		if (!dicItems.ContainsKey(typeId))
		{
			return null;
		}
		return dicItems[typeId].typeName;
	}

	private static LabelItem[] GetDirectChildren(int typeId)
	{
		List<LabelItem> list = new List<LabelItem>(10);
		foreach (LabelItem value in dicItems.Values)
		{
			if (value.directParent == typeId)
			{
				list.Add(value);
			}
		}
		return list.ToArray();
	}

	public static string[] GetDirectChildrenName(int typeId)
	{
		return Array.ConvertAll(GetDirectChildren(typeId), (LabelItem item) => item.typeName);
	}

	public static int GetItemTypeByName(string name)
	{
		foreach (LabelItem value in dicItems.Values)
		{
			if (value.typeName == name)
			{
				return value.typeId;
			}
		}
		return -1;
	}
}

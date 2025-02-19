using System;
using ItemAsset;
using UnityEngine;

public class ItemScript_ItemList : ItemScript
{
	[Serializable]
	public class Item : MaterialItem
	{
	}

	[SerializeField]
	private Item[] m_items;

	public Item[] GetItems()
	{
		return m_items;
	}
}

using System.Collections.Generic;
using System.IO;

public class AccountItems
{
	public string account;

	public int balance;

	public Dictionary<int, int> itemList = new Dictionary<int, int>();

	public void ImportData(byte[] data)
	{
		itemList.Clear();
		if (data.Length == 0)
		{
			return;
		}
		using MemoryStream input = new MemoryStream(data);
		using BinaryReader reader = new BinaryReader(input);
		int num = BufferHelper.ReadInt32(reader);
		for (int i = 0; i < num; i++)
		{
			itemList[BufferHelper.ReadInt32(reader)] = BufferHelper.ReadInt32(reader);
		}
	}

	public byte[] ExportData()
	{
		using MemoryStream memoryStream = new MemoryStream();
		using BinaryWriter writer = new BinaryWriter(memoryStream);
		BufferHelper.Serialize(writer, itemList.Count);
		foreach (KeyValuePair<int, int> item in itemList)
		{
			BufferHelper.Serialize(writer, item.Key);
			BufferHelper.Serialize(writer, item.Value);
		}
		return memoryStream.ToArray();
	}

	private void AddItems(int itemType, int amount)
	{
		if (itemList.ContainsKey(itemType))
		{
			Dictionary<int, int> dictionary;
			Dictionary<int, int> dictionary2 = (dictionary = itemList);
			int key;
			int key2 = (key = itemType);
			key = dictionary[key];
			dictionary2[key2] = key + amount;
		}
		else
		{
			itemList[itemType] = amount;
		}
	}

	public bool DeleteItems(int itemType, int amount)
	{
		if (itemList.ContainsKey(itemType))
		{
			if (itemList[itemType] > amount)
			{
				Dictionary<int, int> dictionary;
				Dictionary<int, int> dictionary2 = (dictionary = itemList);
				int key;
				int key2 = (key = itemType);
				key = dictionary[key];
				dictionary2[key2] = key - amount;
			}
			else
			{
				if (itemList[itemType] != amount)
				{
					return false;
				}
				itemList.Remove(itemType);
			}
			return true;
		}
		return false;
	}
}

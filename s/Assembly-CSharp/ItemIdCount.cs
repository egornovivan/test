using System;
using System.Collections.Generic;
using uLink;

public class ItemIdCount
{
	public int protoId;

	public int count;

	public ItemIdCount()
	{
	}

	public ItemIdCount(int protoId)
	{
		this.protoId = protoId;
		count = 0;
	}

	public ItemIdCount(int protoId, int count)
	{
		this.protoId = protoId;
		this.count = count;
	}

	public static List<ItemIdCount> ParseStringToList(string itemsStr)
	{
		List<ItemIdCount> list = new List<ItemIdCount>();
		if (itemsStr == "0")
		{
			return list;
		}
		string[] array = itemsStr.Split(';');
		string[] array2 = array;
		foreach (string text in array2)
		{
			string[] array3 = text.Split(',');
			int num = Convert.ToInt32(array3[0]);
			int num2 = Convert.ToInt32(array3[1]);
			list.Add(new ItemIdCount(num, num2));
		}
		return list;
	}

	public static object Deserialize(BitStream stream, params object[] codecOptions)
	{
		int num = stream.ReadInt32();
		int num2 = stream.ReadInt32();
		return new ItemIdCount(num, num2);
	}

	public static void Serialize(BitStream stream, object value, params object[] codecOptions)
	{
		ItemIdCount itemIdCount = value as ItemIdCount;
		stream.WriteInt32(itemIdCount.protoId);
		stream.WriteInt32(itemIdCount.count);
	}
}

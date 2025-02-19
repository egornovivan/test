using System.Collections.Generic;

public class StoreData
{
	public int storenpcid;

	public List<int> _itemList = new List<int>();

	public List<int> itemListstory = new List<int>();

	public List<int> itemListadvensingle = new List<int>();

	public List<int> itemListadvencoop = new List<int>();

	public List<int> itemListadvenvs = new List<int>();

	public List<int> itemListadvenfree = new List<int>();

	public string iconname;

	public List<int> ItemList
	{
		get
		{
			if (ServerConfig.IsCooperation)
			{
				return itemListadvencoop;
			}
			if (ServerConfig.IsVS)
			{
				return itemListadvenvs;
			}
			return itemListadvenfree;
		}
	}
}

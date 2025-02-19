using System.Collections.Generic;
using Pathea;

public class StoreData
{
	public int npcid;

	public string iconname;

	public List<int> itemListstory = new List<int>();

	public List<int> itemListadvensingle = new List<int>();

	public List<int> itemListadvencoop = new List<int>();

	public List<int> itemListadvenvs = new List<int>();

	public List<int> itemListadvenfree = new List<int>();

	public List<int> itemList
	{
		get
		{
			if (PeGameMgr.IsStory)
			{
				return itemListstory;
			}
			if (PeGameMgr.IsSingleAdventure)
			{
				return itemListadvensingle;
			}
			if (PeGameMgr.IsCooperation)
			{
				return itemListadvencoop;
			}
			if (PeGameMgr.IsVS)
			{
				return itemListadvenvs;
			}
			return itemListadvenfree;
		}
	}
}

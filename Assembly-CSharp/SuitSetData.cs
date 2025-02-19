using System.Collections.Generic;
using ItemAsset;
using Mono.Data.SqliteClient;
using Pathea;
using PETools;

public class SuitSetData
{
	public struct MatchData
	{
		public string name;

		public List<string> itemNames;

		public List<bool> activeIndex;

		public int[] tips;

		public int activeTipsIndex;

		public List<int> itemProtoList;
	}

	public string suitSetName;

	public List<int> itemProtoList;

	public List<string> itemNames;

	public int[][] setBuffs;

	public int[] tips;

	public static SuitSetData[] g_SuitSetDatas;

	public static void LoadData()
	{
		List<SuitSetData> list = new List<SuitSetData>();
		SqliteDataReader sqliteDataReader = LocalDatabase.Instance.ReadFullTable("SuitSet");
		int num = (sqliteDataReader.FieldCount - 3) / 2;
		while (sqliteDataReader.Read())
		{
			SuitSetData suitSetData = new SuitSetData();
			suitSetData.suitSetName = PELocalization.GetString(Db.GetInt(sqliteDataReader, "NameID"));
			suitSetData.itemProtoList = new List<int>();
			suitSetData.itemProtoList.AddRange(Db.GetIntArray(sqliteDataReader, "IdList"));
			suitSetData.itemNames = GetEquipName(suitSetData.itemProtoList);
			suitSetData.setBuffs = new int[num][];
			suitSetData.tips = new int[num];
			for (int i = 0; i < num; i++)
			{
				suitSetData.setBuffs[i] = Db.GetIntArray(sqliteDataReader, "set" + (i + 2));
				suitSetData.tips[i] = Db.GetInt(sqliteDataReader, "Tips" + (i + 2));
			}
			list.Add(suitSetData);
		}
		g_SuitSetDatas = list.ToArray();
	}

	private static List<string> GetEquipName(List<int> itemProtoIDs)
	{
		List<string> list = new List<string>();
		if (itemProtoIDs != null)
		{
			for (int i = 0; i < itemProtoIDs.Count; i++)
			{
				ItemProto itemProto = PeSingleton<ItemProto.Mgr>.Instance.Get(itemProtoIDs[i]);
				list.Add((itemProto == null) ? string.Empty : itemProto.GetName());
			}
		}
		return list;
	}

	public static SuitSetData GetData(int protoID)
	{
		if (g_SuitSetDatas == null)
		{
			return null;
		}
		for (int i = 0; i < g_SuitSetDatas.Length; i++)
		{
			SuitSetData suitSetData = g_SuitSetDatas[i];
			if (suitSetData.itemProtoList != null && suitSetData.itemProtoList.Contains(protoID))
			{
				return suitSetData;
			}
		}
		return null;
	}

	public static void GetSuitSetEffect(List<ItemObject> equipList, ref List<int> buffList, ref List<MatchData> matchList)
	{
		if (buffList == null || equipList == null || matchList == null || g_SuitSetDatas == null)
		{
			return;
		}
		List<bool> list = null;
		int num = 0;
		for (int i = 0; i < g_SuitSetDatas.Length; i++)
		{
			SuitSetData suitSetData = g_SuitSetDatas[i];
			if (suitSetData.itemProtoList == null)
			{
				continue;
			}
			if (list == null)
			{
				list = new List<bool>();
			}
			else
			{
				list.Clear();
			}
			num = 0;
			for (int j = 0; j < suitSetData.itemProtoList.Count; j++)
			{
				list.Add(item: false);
				for (int k = 0; k < equipList.Count; k++)
				{
					if (equipList[k] != null && suitSetData.itemProtoList[j] == equipList[k].protoId)
					{
						num++;
						list[j] = true;
						break;
					}
				}
			}
			if (0 >= num)
			{
				continue;
			}
			MatchData item = default(MatchData);
			item.itemProtoList = suitSetData.itemProtoList;
			item.name = suitSetData.suitSetName;
			item.itemNames = suitSetData.itemNames;
			item.activeIndex = list;
			item.tips = suitSetData.tips;
			item.activeTipsIndex = ((1 >= num) ? (-1) : (num - 2));
			matchList.Add(item);
			list = null;
			for (int l = 0; l <= item.activeTipsIndex; l++)
			{
				if (suitSetData.setBuffs[l] != null)
				{
					buffList.AddRange(suitSetData.setBuffs[l]);
				}
			}
		}
	}
}

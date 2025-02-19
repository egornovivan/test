using System;
using System.Collections.Generic;
using ItemAsset;
using Mono.Data.SqliteClient;

public class ItemDropData
{
	private struct MeatData
	{
		public int lower;

		public int upper;
	}

	private struct DropData
	{
		public int id;

		public float pro;
	}

	private int _cnt;

	private List<DropData> _dropList = new List<DropData>();

	private MeatData _meatData = default(MeatData);

	public static Dictionary<int, ItemDropData> s_ItemDropDataTbl;

	public static List<ItemSample> GetDropItems(int id)
	{
		ItemDropData value = null;
		if (!s_ItemDropDataTbl.TryGetValue(id, out value))
		{
			return null;
		}
		List<ItemSample> list = new List<ItemSample>();
		Random random = new Random();
		int num = random.Next(value._meatData.lower, value._meatData.upper);
		if (num > 0)
		{
			list.Add(new ItemSample(229, num));
		}
		for (int i = 0; i < value._cnt; i++)
		{
			DropData dat;
			foreach (DropData drop in value._dropList)
			{
				dat = drop;
				float num2 = (float)random.NextDouble();
				if (!(num2 < dat.pro))
				{
					continue;
				}
				if (dat.id > 0)
				{
					ItemSample itemSample = list.Find((ItemSample it) => it.protoId == dat.id);
					if (itemSample == null)
					{
						list.Add(new ItemSample(dat.id));
					}
					else
					{
						itemSample.stackCount++;
					}
				}
				break;
			}
		}
		return list;
	}

	public static void LoadData()
	{
		if (s_ItemDropDataTbl != null)
		{
			return;
		}
		s_ItemDropDataTbl = new Dictionary<int, ItemDropData>();
		SqliteDataReader sqliteDataReader = LocalDatabase.Instance.ReadFullTable("loot");
		while (sqliteDataReader.Read())
		{
			string @string = sqliteDataReader.GetString(sqliteDataReader.GetOrdinal("id"));
			int key = Convert.ToInt32(@string);
			bool flag = false;
			ItemDropData itemDropData = new ItemDropData();
			string string2 = sqliteDataReader.GetString(sqliteDataReader.GetOrdinal("meat"));
			string[] array = string2.Split(';');
			if (array.Length == 2)
			{
				itemDropData._meatData.lower = Convert.ToInt32(array[0]);
				itemDropData._meatData.upper = Convert.ToInt32(array[1]);
				flag = true;
			}
			string string3 = sqliteDataReader.GetString(sqliteDataReader.GetOrdinal("loot"));
			string[] array2 = string3.Split(';');
			if (array2.Length == 2)
			{
				int num = Convert.ToInt32(array2[0]);
				string[] array3 = array2[1].Split(',');
				if (num > 0 && array3.Length > 0)
				{
					List<DropData> list = new List<DropData>();
					for (int i = 0; i < array3.Length; i++)
					{
						string[] array4 = array3[i].Split('_');
						if (array4.Length == 2)
						{
							DropData item = default(DropData);
							item.id = Convert.ToInt32(array4[0]);
							item.pro = Convert.ToSingle(array4[1]);
							list.Add(item);
						}
					}
					itemDropData._cnt = num;
					itemDropData._dropList = list;
				}
				flag = true;
			}
			if (flag)
			{
				s_ItemDropDataTbl[key] = itemDropData;
			}
		}
	}
}

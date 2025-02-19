using System;
using System.Collections.Generic;
using Mono.Data.SqliteClient;
using SkillSystem;
using UnityEngine;

namespace Pathea;

public class AttPlusNPCData
{
	public class AttrPlus
	{
		public struct RandomInt
		{
			public int m_Min;

			public int m_Max;

			public int Random()
			{
				return UnityEngine.Random.Range(m_Min, m_Max);
			}

			public static RandomInt Load(string text)
			{
				RandomInt result = default(RandomInt);
				string[] array = text.Split('_');
				if (array.Length != 2)
				{
					Debug.LogError("load RandomInt error:" + text);
				}
				else
				{
					result.m_Min = Convert.ToInt32(array[0]);
					result.m_Max = Convert.ToInt32(array[1]);
				}
				return result;
			}
		}

		public struct Data
		{
			public int AttrID;

			public RandomInt PlusValue;
		}

		public List<Data> mList = new List<Data>();

		public static AttrPlus LoadFromText(string tmp)
		{
			if (!string.IsNullOrEmpty(tmp))
			{
				AttrPlus attrPlus = new AttrPlus();
				string[] array = tmp.Split(';');
				for (int i = 0; i < array.Length; i++)
				{
					string[] array2 = array[i].Split(',');
					if (array2.Length % 2 != 0)
					{
						Debug.LogError("load RandomInt error:" + tmp);
						continue;
					}
					Data item = default(Data);
					item.AttrID = Convert.ToInt32(array2[0]);
					item.PlusValue = RandomInt.Load(array2[1]);
					attrPlus.mList.Add(item);
				}
				return attrPlus;
			}
			return null;
		}

		public new List<AttribType> GetType()
		{
			List<AttribType> list = new List<AttribType>();
			foreach (Data m in mList)
			{
				list.Add((AttribType)m.AttrID);
			}
			return list;
		}

		public Data GetPlusRandom(AttribType type)
		{
			return mList.Find((Data item) => (item.AttrID == (int)type) ? true : false);
		}
	}

	public class Item
	{
		public int _id;

		public AttrPlus AttPlus;

		public int PlusCount;

		public string attPlus
		{
			set
			{
				AttPlus = AttrPlus.LoadFromText(value);
			}
		}
	}

	private static List<Item> sList;

	public static void Load()
	{
		sList = new List<Item>();
		SqliteDataReader sqliteDataReader = LocalDatabase.Instance.ReadFullTable("AttPlusNPC");
		while (sqliteDataReader.Read())
		{
			Item item = new Item();
			item._id = sqliteDataReader.GetInt32(sqliteDataReader.GetOrdinal("NPC"));
			item.PlusCount = sqliteDataReader.GetInt32(sqliteDataReader.GetOrdinal("PlusCount"));
			item.attPlus = sqliteDataReader.GetString(sqliteDataReader.GetOrdinal("AttPlus"));
			sList.Add(item);
		}
	}

	public static void Release()
	{
		sList = null;
	}

	public static Item Get(int id)
	{
		return sList.Find((Item item) => (item._id == id) ? true : false);
	}

	public static bool ComparePlusCout(int NpcId, int curCout)
	{
		Item item = Get(NpcId);
		if (item == null || item.AttPlus == null)
		{
			return false;
		}
		return curCout < item.PlusCount;
	}

	public static int GetPlusCount(int NpcId)
	{
		Item item = Get(NpcId);
		if (item == null || item.AttPlus == null)
		{
			return -1;
		}
		return item.PlusCount;
	}

	public static bool GetRandom(int npcId, AttribType type, out AttrPlus.RandomInt Rand)
	{
		Rand = default(AttrPlus.RandomInt);
		Item item = Get(npcId);
		if (item == null || item.AttPlus == null)
		{
			return false;
		}
		Rand = item.AttPlus.GetPlusRandom(type).PlusValue;
		return true;
	}

	public static AttribType GetRandMaxAttribute(int npcId, SkEntity peSkentity)
	{
		Item item = Get(npcId);
		if (item == null || item.AttPlus == null)
		{
			return AttribType.Max;
		}
		return GetRandMaxAttr(npcId, peSkentity, item.AttPlus.GetType().ToArray());
	}

	public static AttribType GetProtoMaxAttribute(int npcId, SkEntity peSkentity)
	{
		Item item = Get(npcId);
		if (item == null || item.AttPlus == null)
		{
			return AttribType.Max;
		}
		return GetProtoMaxAttr(npcId, peSkentity, item.AttPlus.GetType().ToArray());
	}

	private static AttribType GetRandMaxAttr(int npcId, SkEntity entity, AttribType[] ChangeAbleAttr)
	{
		float num = 0f;
		AttribType result = ChangeAbleAttr[0];
		RandomNpcDb.Item item = RandomNpcDb.Get(npcId);
		RandomNpcDb.RandomInt randomInt = default(RandomNpcDb.RandomInt);
		for (int i = 0; i < ChangeAbleAttr.Length; i++)
		{
			if (item.TryGetAttrRandom(ChangeAbleAttr[i], out randomInt) && randomInt.m_Max != 0)
			{
				float num2 = (entity.GetAttribute(ChangeAbleAttr[i], isBase: true) - (float)randomInt.m_Min) / (float)(randomInt.m_Max - randomInt.m_Min);
				if (num2 > num)
				{
					result = ChangeAbleAttr[i];
					num = num2;
				}
			}
		}
		return result;
	}

	private static AttribType GetProtoMaxAttr(int npcId, SkEntity entity, AttribType[] ChangeAbleAttr)
	{
		return ChangeAbleAttr[0];
	}
}

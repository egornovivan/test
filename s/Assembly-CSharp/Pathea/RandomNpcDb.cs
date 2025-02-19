using System;
using System.Collections.Generic;
using Mono.Data.SqliteClient;
using UnityEngine;

namespace Pathea;

public class RandomNpcDb
{
	public class ItemcoutDb
	{
		public int protoId;

		public int count;

		public ItemcoutDb(int _portoId, int _cnt)
		{
			protoId = _portoId;
			count = _cnt;
		}

		public static List<ItemcoutDb> LoadItemcout(string item)
		{
			List<ItemcoutDb> list = null;
			if (!item.Equals("0"))
			{
				string[] array = item.Split(',', ';');
				if (array.Length > 1)
				{
					for (int i = 0; i < array.Length / 2; i++)
					{
						if (list == null)
						{
							list = new List<ItemcoutDb>();
						}
						list.Add(new ItemcoutDb(Convert.ToInt32(array[i * 2]), Convert.ToInt32(array[i * 2 + 1])));
					}
				}
			}
			return list;
		}
	}

	public class NpcMoney
	{
		public RandomInt initValue;

		public RandomInt incValue;

		public int max;

		public static NpcMoney LoadFromText(string text)
		{
			string[] array = text.Split(';');
			if (array.Length != 3)
			{
				return null;
			}
			string[] array2 = array[0].Split(',');
			if (array2.Length != 2)
			{
				return null;
			}
			NpcMoney npcMoney = new NpcMoney();
			if (!int.TryParse(array2[0], out npcMoney.initValue.m_Min))
			{
				return null;
			}
			if (!int.TryParse(array2[1], out npcMoney.initValue.m_Max))
			{
				return null;
			}
			array2 = array[1].Split(',');
			if (array2.Length != 2)
			{
				return null;
			}
			if (!int.TryParse(array2[0], out npcMoney.incValue.m_Min))
			{
				return null;
			}
			if (!int.TryParse(array2[1], out npcMoney.incValue.m_Max))
			{
				return null;
			}
			if (!int.TryParse(array[2], out npcMoney.max))
			{
				return null;
			}
			return npcMoney;
		}
	}

	public class RandomSkill
	{
		private class SkillData
		{
			public int id;

			public int weight;

			public string label;
		}

		private class SkillCount
		{
			public int count;

			public int weight;
		}

		private List<SkillCount> skillCountList = new List<SkillCount>(2);

		private List<SkillData> skillDataList = new List<SkillData>(5);

		public void AddSkill(int skillid, int weight, string label)
		{
			if (weight <= 0)
			{
				Debug.LogError("skill weight error. fixed to 1");
				weight = 1;
			}
			SkillData skillData = new SkillData();
			skillData.id = skillid;
			skillData.weight = weight;
			skillData.label = label;
			skillDataList.Add(skillData);
		}

		public void AddSkillCount(int count, int weight)
		{
			if (count < 0)
			{
				Debug.LogError("count error. fixed to 1");
				count = 0;
			}
			if (weight <= 0)
			{
				Debug.LogError("count weight error. fixed to 1");
				weight = 1;
			}
			SkillCount skillCount = new SkillCount();
			skillCount.count = count;
			skillCount.weight = weight;
			skillCountList.Add(skillCount);
		}

		private int GetSkillCount()
		{
			int num = 0;
			foreach (SkillCount skillCount in skillCountList)
			{
				num += skillCount.weight;
			}
			int num2 = UnityEngine.Random.Range(0, num);
			int num3 = 0;
			foreach (SkillCount skillCount2 in skillCountList)
			{
				if (num2 < num3 + skillCount2.weight)
				{
					return skillCount2.count;
				}
				num3 += skillCount2.weight;
			}
			return 0;
		}

		public int[] GetSkill()
		{
			List<int> list = new List<int>(2);
			int skillCount = GetSkillCount();
			if (skillCount <= 0 || skillCount > skillDataList.Count)
			{
				return list.ToArray();
			}
			List<SkillData> collection = skillDataList;
			SkillData lastSelected = null;
			for (int i = 0; i < skillCount; i++)
			{
				List<SkillData> list2 = new List<SkillData>(collection);
				if (lastSelected != null)
				{
					list2.RemoveAll((SkillData skillData) => (skillData.label == lastSelected.label) ? true : false);
				}
				SkillData skill = GetSkill(list2);
				if (skill != null)
				{
					list.Add(skill.id);
					collection = list2;
					lastSelected = skill;
					continue;
				}
				break;
			}
			return list.ToArray();
		}

		private SkillData GetSkill(List<SkillData> list)
		{
			if (list.Count <= 0)
			{
				Debug.LogError("Get Random Skill error. no skill.");
				return null;
			}
			int num = 0;
			foreach (SkillData item in list)
			{
				num += item.weight;
			}
			if (num <= 0)
			{
				Debug.LogError("Get Random Skill error. all skill weight is 0.");
				return null;
			}
			int num2 = UnityEngine.Random.Range(0, num);
			int num3 = 0;
			foreach (SkillData item2 in list)
			{
				if (num2 < num3 + item2.weight)
				{
					return item2;
				}
				num3 += item2.weight;
			}
			Debug.LogError("Get Random Skill error.");
			return null;
		}
	}

	public struct RandomInt
	{
		public int m_Min;

		public int m_Max;

		public int Random()
		{
			return UnityEngine.Random.Range(m_Min, m_Max);
		}
	}

	public class Item
	{
		public int id;

		public RandomInt hpMax;

		public RandomInt atk;

		public int resDamage;

		public float atkRange;

		public RandomInt def;

		public int reviveTime;

		public int[] initEquipment;

		public List<ItemcoutDb> initItems;

		public RandomSkill mSkillRandom;

		public NpcMoney npcMoney;

		public bool TryGetAttrRandom(AttribType type, out RandomInt randomInt)
		{
			switch (type)
			{
			case AttribType.HpMax:
				randomInt = hpMax;
				return true;
			case AttribType.Atk:
				randomInt = atk;
				return true;
			case AttribType.Def:
				randomInt = def;
				return true;
			default:
				randomInt = default(RandomInt);
				return false;
			}
		}
	}

	private static List<Item> sList;

	public static void Load()
	{
		sList = new List<Item>(50);
		SqliteDataReader sqliteDataReader = LocalDatabase.Instance.ReadFullTable("RandNPC");
		while (sqliteDataReader.Read())
		{
			Item item = new Item();
			item.id = Convert.ToInt32(sqliteDataReader.GetString(sqliteDataReader.GetOrdinal("ID")));
			string @string = sqliteDataReader.GetString(sqliteDataReader.GetOrdinal(AttribType.HpMax.ToString()));
			string[] array = @string.Split(',');
			if (array.Length == 2)
			{
				item.hpMax = default(RandomInt);
				item.hpMax.m_Min = Convert.ToInt32(array[0]);
				item.hpMax.m_Max = Convert.ToInt32(array[1]);
			}
			@string = sqliteDataReader.GetString(sqliteDataReader.GetOrdinal(AttribType.Atk.ToString()));
			array = @string.Split(',');
			if (array.Length == 2)
			{
				item.atk = default(RandomInt);
				item.atk.m_Min = Convert.ToInt32(array[0]);
				item.atk.m_Max = Convert.ToInt32(array[1]);
			}
			item.resDamage = Convert.ToInt32(sqliteDataReader.GetString(sqliteDataReader.GetOrdinal(AttribType.ResDamage.ToString())));
			item.atkRange = Convert.ToSingle(sqliteDataReader.GetString(sqliteDataReader.GetOrdinal(AttribType.AtkRange.ToString())));
			@string = sqliteDataReader.GetString(sqliteDataReader.GetOrdinal(AttribType.Def.ToString()));
			array = @string.Split(',');
			if (array.Length == 2)
			{
				item.def = default(RandomInt);
				item.def.m_Min = Convert.ToInt32(array[0]);
				item.def.m_Max = Convert.ToInt32(array[1]);
			}
			item.reviveTime = Convert.ToInt32(sqliteDataReader.GetString(sqliteDataReader.GetOrdinal("revive")));
			@string = sqliteDataReader.GetString(sqliteDataReader.GetOrdinal("equipment"));
			if (!string.IsNullOrEmpty(@string) && @string != "0")
			{
				array = @string.Split(',');
				item.initEquipment = new int[array.Length];
				for (int i = 0; i < array.Length; i++)
				{
					item.initEquipment[i] = Convert.ToInt32(array[i]);
				}
			}
			@string = sqliteDataReader.GetString(sqliteDataReader.GetOrdinal("item"));
			item.initItems = ItemcoutDb.LoadItemcout(@string);
			@string = sqliteDataReader.GetString(sqliteDataReader.GetOrdinal("skill"));
			if (!string.IsNullOrEmpty(@string) && @string != "0")
			{
				item.mSkillRandom = new RandomSkill();
				array = @string.Split(';');
				for (int j = 0; j < array.Length; j++)
				{
					if (array[j] != "0")
					{
						string[] array2 = array[j].Split(',');
						if (array2.Length == 3)
						{
							item.mSkillRandom.AddSkill(Convert.ToInt32(array2[0]), Convert.ToInt32(array2[1]), array2[2]);
						}
						else
						{
							Debug.LogError("string[" + array[j] + "] cant be splited by ',' to 3 parts.");
						}
					}
				}
				@string = sqliteDataReader.GetString(sqliteDataReader.GetOrdinal("skillnum"));
				array = @string.Split(';');
				for (int k = 0; k < array.Length; k++)
				{
					if (array[k] != "0")
					{
						string[] array3 = array[k].Split(',');
						if (array3.Length == 2)
						{
							item.mSkillRandom.AddSkillCount(Convert.ToInt32(array3[0]), Convert.ToInt32(array3[1]));
						}
						else
						{
							Debug.LogError("string[" + array[k] + "] cant be splited by ',' to 2 parts.");
						}
					}
				}
			}
			string string2 = sqliteDataReader.GetString(sqliteDataReader.GetOrdinal("money"));
			item.npcMoney = NpcMoney.LoadFromText(string2);
			sList.Add(item);
		}
	}

	public static void Release()
	{
		sList = null;
	}

	public static Item Get(int id)
	{
		return sList.Find((Item item) => (item.id == id) ? true : false);
	}
}

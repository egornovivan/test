using System;
using System.Collections.Generic;
using Behave.Runtime;
using Mono.Data.SqliteClient;
using PETools;
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

	public class RandomAbility
	{
		private class Data
		{
			public int id;

			public int weight;

			public string label;
		}

		private class Count
		{
			public int count;

			public int weight;
		}

		private List<Count> countList = new List<Count>(2);

		private List<Data> dataList = new List<Data>(5);

		public void AddData(int skillid, int weight, string label)
		{
			if (weight <= 0)
			{
				Debug.LogError("skill weight error. fixed to 1");
				weight = 1;
			}
			Data data = new Data();
			data.id = skillid;
			data.weight = weight;
			data.label = label;
			dataList.Add(data);
		}

		public void AddCount(int count, int weight)
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
			Count count2 = new Count();
			count2.count = count;
			count2.weight = weight;
			countList.Add(count2);
		}

		private int GetCount()
		{
			int num = 0;
			foreach (Count count in countList)
			{
				num += count.weight;
			}
			int num2 = UnityEngine.Random.Range(0, num);
			int num3 = 0;
			foreach (Count count2 in countList)
			{
				if (num2 < num3 + count2.weight)
				{
					return count2.count;
				}
				num3 += count2.weight;
			}
			return 0;
		}

		public Ablities Get()
		{
			List<int> list = new List<int>(2);
			int count = GetCount();
			if (count <= 0 || count > dataList.Count)
			{
				return NpcAblitycmpt.CompareSkillType(list);
			}
			List<Data> collection = dataList;
			Data lastSelected = null;
			for (int i = 0; i < count; i++)
			{
				List<Data> list2 = new List<Data>(collection);
				if (lastSelected != null)
				{
					list2.RemoveAll((Data skillData) => (skillData.label == lastSelected.label) ? true : false);
				}
				Data data = GetData(list2);
				if (data != null)
				{
					list.Add(data.id);
					collection = list2;
					lastSelected = data;
					continue;
				}
				break;
			}
			return NpcAblitycmpt.CompareSkillType(list);
		}

		private Data GetData(List<Data> list)
		{
			if (list.Count <= 0)
			{
				Debug.LogError("Get Random Skill error. no skill.");
				return null;
			}
			int num = 0;
			foreach (Data item in list)
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
			foreach (Data item2 in list)
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

		public void LoadSkill(string tmp)
		{
			if (string.IsNullOrEmpty(tmp) || !(tmp != "0"))
			{
				return;
			}
			string[] array = tmp.Split(';');
			for (int i = 0; i < array.Length; i++)
			{
				if (array[i] != "0")
				{
					string[] array2 = array[i].Split(',');
					if (array2.Length == 3)
					{
						AddData(Convert.ToInt32(array2[0]), Convert.ToInt32(array2[1]), array2[2]);
					}
					else
					{
						Debug.LogError("string[" + array[i] + "] cant be splited by ',' to 3 parts.");
					}
				}
			}
		}

		public void LoadSkillNum(string tmp)
		{
			if (string.IsNullOrEmpty(tmp) || !(tmp != "0"))
			{
				return;
			}
			string[] array = tmp.Split(';');
			for (int i = 0; i < array.Length; i++)
			{
				if (array[i] != "0")
				{
					string[] array2 = array[i].Split(',');
					if (array2.Length == 2)
					{
						AddCount(Convert.ToInt32(array2[0]), Convert.ToInt32(array2[1]));
					}
					else
					{
						Debug.LogError("string[" + array[i] + "] cant be splited by ',' to 2 parts.");
					}
				}
			}
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

		public static RandomInt Load(string text)
		{
			RandomInt result = default(RandomInt);
			string[] array = text.Split(',');
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

	public class VoiceMatch
	{
		public List<int> womanVoice;

		public List<int> manVoice;

		public VoiceMatch()
		{
			womanVoice = new List<int>();
			manVoice = new List<int>();
		}

		public int GetRandomVoice(PeSex sex)
		{
			return sex switch
			{
				PeSex.Female => womanVoice[UnityEngine.Random.Range(0, womanVoice.Count)], 
				PeSex.Male => manVoice[UnityEngine.Random.Range(0, manVoice.Count)], 
				_ => -1, 
			};
		}

		public static VoiceMatch LoadData(string tmp)
		{
			if (!string.IsNullOrEmpty(tmp) && tmp != "0")
			{
				string[] array = tmp.Split(';');
				if (array.Length != 2)
				{
					return null;
				}
				VoiceMatch voiceMatch = new VoiceMatch();
				string[] array2 = array[0].Split(',');
				for (int i = 0; i < array2.Length; i++)
				{
					voiceMatch.womanVoice.Add(Convert.ToInt32(array2[i]));
				}
				string[] array3 = array[1].Split(',');
				for (int j = 0; j < array3.Length; j++)
				{
					voiceMatch.manVoice.Add(Convert.ToInt32(array3[j]));
				}
				return voiceMatch;
			}
			return null;
		}
	}

	public class Item
	{
		[DbReader.DbField("ID", false)]
		public int id;

		public RandomInt hpMax;

		public RandomInt atk;

		public RandomInt def;

		public NpcMoney npcMoney;

		public VoiceMatch voiveMatch;

		public RandomAbility randomAbility;

		[DbReader.DbField("ResDamage", false)]
		public int resDamage;

		[DbReader.DbField("AtkRange", false)]
		public float atkRange;

		[DbReader.DbField("revive", false)]
		public int reviveTime;

		[DbReader.DbField("equipment", false)]
		public int[] initEquipment;

		public List<ItemcoutDb> initItems;

		[DbReader.DbField("AiPath", false)]
		public string behaveDataPath;

		[DbReader.DbField("HpMax", false)]
		private string hpMaxStr
		{
			set
			{
				hpMax = RandomInt.Load(value);
			}
		}

		[DbReader.DbField("Atk", false)]
		private string atkStr
		{
			set
			{
				atk = RandomInt.Load(value);
			}
		}

		[DbReader.DbField("Def", false)]
		private string defStr
		{
			set
			{
				def = RandomInt.Load(value);
			}
		}

		[DbReader.DbField("money", false)]
		private string moneyStr
		{
			set
			{
				npcMoney = NpcMoney.LoadFromText(value);
			}
		}

		[DbReader.DbField("VoiceType", false)]
		private string voiceStr
		{
			set
			{
				voiveMatch = VoiceMatch.LoadData(value);
			}
		}

		[DbReader.DbField("skill", false)]
		private string abilityStr
		{
			set
			{
				if (randomAbility == null)
				{
					randomAbility = new RandomAbility();
				}
				randomAbility.LoadSkill(value);
			}
		}

		[DbReader.DbField("skillnum", false)]
		private string abilityNumStr
		{
			set
			{
				if (randomAbility == null)
				{
					randomAbility = new RandomAbility();
				}
				randomAbility.LoadSkillNum(value);
			}
		}

		[DbReader.DbField("item", false)]
		private string initItem
		{
			set
			{
				initItems = ItemcoutDb.LoadItemcout(value);
			}
		}

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
			Item item = DbReader.ReadItem<Item>(sqliteDataReader);
			BTResolver.RegisterToCache(item.behaveDataPath);
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

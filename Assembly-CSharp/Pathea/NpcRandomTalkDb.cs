using System;
using System.Collections.Generic;
using Mono.Data.SqliteClient;
using PETools;
using UnityEngine;

namespace Pathea;

public class NpcRandomTalkDb
{
	public class RandomCase
	{
		public List<int> listCases;

		public RandomCase()
		{
			listCases = new List<int>();
		}

		public int RandCase()
		{
			return listCases[UnityEngine.Random.Range(0, listCases.Count)];
		}

		public static RandomCase Load(string text)
		{
			if (!string.IsNullOrEmpty(text))
			{
				RandomCase randomCase = new RandomCase();
				string[] array = text.Split(',');
				for (int i = 0; i < array.Length; i++)
				{
					randomCase.listCases.Add(Convert.ToInt32(array[i]));
				}
				return randomCase;
			}
			Debug.LogError("load RandomInt error:" + text);
			return null;
		}
	}

	private class RandomAttrChoce
	{
		private List<AttribType> lists;

		public RandomAttrChoce()
		{
			lists = new List<AttribType>();
			lists.Add(AttribType.Hunger);
			lists.Add(AttribType.Hp);
			lists.Add(AttribType.Comfort);
		}

		public AttribType RandType()
		{
			return lists[UnityEngine.Random.Range(0, lists.Count)];
		}
	}

	public class Item
	{
		[DbReader.DbField("ID", false)]
		public int _id;

		[DbReader.DbField("value", false)]
		public float _value;

		[DbReader.DbField("probability", false)]
		public float _probability;

		public float _interval;

		public RandomCase Scenario;

		public AttribType Type;

		public AttribType TypeMax;

		public ETalkLevel Level;

		public ENpcTalkType TalkType => (ENpcTalkType)_id;

		[DbReader.DbField("interval", false)]
		private float interval
		{
			set
			{
				_interval = value * 60f;
			}
		}

		[DbReader.DbField("scenario", false)]
		private string _scenario
		{
			set
			{
				Scenario = RandomCase.Load(value);
			}
		}
	}

	private static List<Item> sList;

	public static void Load()
	{
		sList = new List<Item>();
		SqliteDataReader sqliteDataReader = LocalDatabase.Instance.ReadFullTable("Npc_randomtalk");
		while (sqliteDataReader.Read())
		{
			Item item = DbReader.ReadItem<Item>(sqliteDataReader);
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

	public static Item Get(ENpcTalkType type)
	{
		return Get((int)type);
	}

	public static int GetTalkCase(int id)
	{
		Item item = Get(id);
		if (item == null)
		{
			return -1;
		}
		if (UnityEngine.Random.value <= item._probability)
		{
			return item.Scenario.RandCase();
		}
		return -1;
	}

	public static int GetTalkCase(int id, out float time)
	{
		time = 0f;
		Item item = Get(id);
		if (item == null)
		{
			return -1;
		}
		if (UnityEngine.Random.value <= item._probability)
		{
			time = item._interval;
			return item.Scenario.RandCase();
		}
		return -1;
	}

	public static List<Item> GetTalkItems(PeEntity entity)
	{
		if (entity == null)
		{
			return null;
		}
		List<Item> list = new List<Item>();
		Item talkItemByType = GetTalkItemByType(entity, AttribType.Hp, AttribType.HpMax);
		if (talkItemByType != null)
		{
			list.Add(talkItemByType);
		}
		talkItemByType = GetTalkItemByType(entity, AttribType.Hunger, AttribType.HungerMax);
		if (talkItemByType != null)
		{
			list.Add(talkItemByType);
		}
		talkItemByType = GetTalkItemByType(entity, AttribType.Comfort, AttribType.ComfortMax);
		if (talkItemByType != null)
		{
			list.Add(talkItemByType);
		}
		return list;
	}

	public static Item GetTalkItemByType(PeEntity entity, AttribType _type, AttribType _typeMax)
	{
		Item item = null;
		float attribute = entity.GetAttribute(_type);
		ETalkLevel eTalkLevel = SwichAtrrLevel(_type, attribute, entity);
		if (eTalkLevel != ETalkLevel.Max)
		{
			item = GetTalkItem(_type, eTalkLevel);
			item.Type = _type;
			item.TypeMax = _typeMax;
			item.Level = eTalkLevel;
		}
		return item;
	}

	public static List<Item> GetPlyerTalkItems(PeEntity entity)
	{
		if (entity == null)
		{
			return null;
		}
		List<Item> list = new List<Item>();
		float attribute = entity.GetAttribute(AttribType.Hp);
		ETalkLevel eTalkLevel = SwichAtrrLevel(AttribType.Hp, attribute, entity);
		if (eTalkLevel != ETalkLevel.Max)
		{
			Item plyerTalkItem = GetPlyerTalkItem(AttribType.Hp, eTalkLevel);
			plyerTalkItem.Type = AttribType.Hp;
			plyerTalkItem.Level = eTalkLevel;
			list.Add(plyerTalkItem);
		}
		float attribute2 = entity.GetAttribute(AttribType.Hunger);
		ETalkLevel eTalkLevel2 = SwichAtrrLevel(AttribType.Hunger, attribute2, entity);
		if (eTalkLevel2 != ETalkLevel.Max)
		{
			Item plyerTalkItem = GetPlyerTalkItem(AttribType.Hunger, eTalkLevel2);
			plyerTalkItem.Type = AttribType.Hunger;
			plyerTalkItem.Level = eTalkLevel2;
			list.Add(plyerTalkItem);
		}
		float attribute3 = entity.GetAttribute(AttribType.Comfort);
		ETalkLevel eTalkLevel3 = SwichAtrrLevel(AttribType.Comfort, attribute3, entity);
		if (eTalkLevel3 != ETalkLevel.Max)
		{
			Item plyerTalkItem = GetPlyerTalkItem(AttribType.Comfort, eTalkLevel3);
			plyerTalkItem.Type = AttribType.Comfort;
			plyerTalkItem.Level = eTalkLevel3;
			list.Add(plyerTalkItem);
		}
		return list;
	}

	private static Dictionary<ETalkLevel, Item> SwichItems(AttribType type)
	{
		Dictionary<ETalkLevel, Item> result = null;
		switch (type)
		{
		case AttribType.Hp:
			result = GetHealthCase();
			break;
		case AttribType.Hunger:
			result = GetHungerCase();
			break;
		case AttribType.Comfort:
			result = GetComfortCase();
			break;
		}
		return result;
	}

	private static Dictionary<ETalkLevel, Item> SwichMainPlyerItems(AttribType type)
	{
		Dictionary<ETalkLevel, Item> result = null;
		switch (type)
		{
		case AttribType.Hp:
			result = GetPlyerHealthCase();
			break;
		case AttribType.Hunger:
			result = GetPlyerHungerCase();
			break;
		case AttribType.Comfort:
			result = GetPlyerComfortCase();
			break;
		}
		return result;
	}

	private static Item GetTalkItem(AttribType type, ETalkLevel level)
	{
		Dictionary<ETalkLevel, Item> dictionary = SwichItems(type);
		return dictionary[level];
	}

	private static Item GetPlyerTalkItem(AttribType type, ETalkLevel level)
	{
		Dictionary<ETalkLevel, Item> dictionary = SwichMainPlyerItems(type);
		return dictionary[level];
	}

	private static ETalkLevel SwichAtrrLevel(AttribType type, float attr, PeEntity entity)
	{
		return type switch
		{
			AttribType.Hp => SwichHealthLevel(attr, entity), 
			AttribType.Hunger => SwichHungerLevel(attr, entity), 
			AttribType.Comfort => SwichComfortLevel(attr, entity), 
			_ => ETalkLevel.Max, 
		};
	}

	public static int CheckAttrbCase(PESkEntity peskentity)
	{
		RandomAttrChoce randomAttrChoce = new RandomAttrChoce();
		AttribType attribType = randomAttrChoce.RandType();
		float attribute = peskentity.GetAttribute(attribType);
		switch (attribType)
		{
		case AttribType.Hunger:
		{
			float attribute4 = peskentity.GetAttribute(AttribType.HungerMax);
			if (attribute <= attribute4 * Get(10)._value && attribute > attribute4 * Get(11)._value)
			{
				return Get(10).Scenario.RandCase();
			}
			if (attribute <= attribute4 * Get(13)._value)
			{
				return Get(11).Scenario.RandCase();
			}
			return -1;
		}
		case AttribType.Hp:
		{
			float attribute3 = peskentity.GetAttribute(AttribType.HpMax);
			if (attribute <= attribute3 * Get(12)._value && attribute > attribute3 * Get(13)._value)
			{
				return Get(12).Scenario.RandCase();
			}
			if (attribute <= attribute3 * Get(13)._value)
			{
				return Get(13).Scenario.RandCase();
			}
			return -1;
		}
		case AttribType.Comfort:
		{
			float attribute2 = peskentity.GetAttribute(AttribType.ComfortMax);
			if (attribute <= attribute2 * Get(7)._value && attribute > attribute2 * Get(8)._value)
			{
				return Get(7).Scenario.RandCase();
			}
			if (attribute <= attribute2 * Get(8)._value && attribute > attribute2 * Get(9)._value)
			{
				return Get(8).Scenario.RandCase();
			}
			if (attribute <= attribute2 * Get(9)._value)
			{
				return Get(9).Scenario.RandCase();
			}
			return -1;
		}
		default:
			return -1;
		}
	}

	private static ETalkLevel SwichHealthLevel(float attr, PeEntity entity)
	{
		Dictionary<ETalkLevel, Item> healthCase = GetHealthCase();
		if (attr <= healthCase[ETalkLevel.Medium]._value * entity.GetAttribute(AttribType.HpMax) && attr > healthCase[ETalkLevel.Low]._value * entity.GetAttribute(AttribType.HpMax))
		{
			return ETalkLevel.Medium;
		}
		if (attr <= healthCase[ETalkLevel.Low]._value * entity.GetAttribute(AttribType.HpMax))
		{
			return ETalkLevel.Low;
		}
		return ETalkLevel.Max;
	}

	private static ETalkLevel SwichHungerLevel(float attr, PeEntity entity)
	{
		Dictionary<ETalkLevel, Item> hungerCase = GetHungerCase();
		if (attr <= hungerCase[ETalkLevel.Medium]._value * entity.GetAttribute(AttribType.HungerMax) && attr > hungerCase[ETalkLevel.Low]._value * entity.GetAttribute(AttribType.HungerMax))
		{
			return ETalkLevel.Medium;
		}
		if (attr <= hungerCase[ETalkLevel.Low]._value * entity.GetAttribute(AttribType.HungerMax))
		{
			return ETalkLevel.Low;
		}
		return ETalkLevel.Max;
	}

	private static ETalkLevel SwichComfortLevel(float attr, PeEntity entity)
	{
		Dictionary<ETalkLevel, Item> comfortCase = GetComfortCase();
		if (attr <= comfortCase[ETalkLevel.Common]._value * entity.GetAttribute(AttribType.ComfortMax) && attr > comfortCase[ETalkLevel.Medium]._value * entity.GetAttribute(AttribType.ComfortMax))
		{
			return ETalkLevel.Common;
		}
		if (attr <= comfortCase[ETalkLevel.Medium]._value * entity.GetAttribute(AttribType.ComfortMax) && attr > comfortCase[ETalkLevel.Low]._value * entity.GetAttribute(AttribType.ComfortMax))
		{
			return ETalkLevel.Medium;
		}
		if (attr <= comfortCase[ETalkLevel.Low]._value * entity.GetAttribute(AttribType.ComfortMax))
		{
			return ETalkLevel.Low;
		}
		return ETalkLevel.Max;
	}

	private static Dictionary<ETalkLevel, Item> GetHungerCase()
	{
		Dictionary<ETalkLevel, Item> dictionary = new Dictionary<ETalkLevel, Item>();
		dictionary[ETalkLevel.Medium] = Get(10);
		dictionary[ETalkLevel.Low] = Get(11);
		return dictionary;
	}

	private static Dictionary<ETalkLevel, Item> GetPlyerHungerCase()
	{
		Dictionary<ETalkLevel, Item> dictionary = new Dictionary<ETalkLevel, Item>();
		dictionary[ETalkLevel.Medium] = Get(17);
		dictionary[ETalkLevel.Low] = Get(18);
		return dictionary;
	}

	private static Dictionary<ETalkLevel, Item> GetHealthCase()
	{
		Dictionary<ETalkLevel, Item> dictionary = new Dictionary<ETalkLevel, Item>();
		dictionary[ETalkLevel.Medium] = Get(12);
		dictionary[ETalkLevel.Low] = Get(13);
		return dictionary;
	}

	private static Dictionary<ETalkLevel, Item> GetPlyerHealthCase()
	{
		Dictionary<ETalkLevel, Item> dictionary = new Dictionary<ETalkLevel, Item>();
		dictionary[ETalkLevel.Medium] = Get(19);
		dictionary[ETalkLevel.Low] = Get(20);
		return dictionary;
	}

	private static Dictionary<ETalkLevel, Item> GetComfortCase()
	{
		Dictionary<ETalkLevel, Item> dictionary = new Dictionary<ETalkLevel, Item>();
		dictionary[ETalkLevel.Common] = Get(7);
		dictionary[ETalkLevel.Medium] = Get(8);
		dictionary[ETalkLevel.Low] = Get(9);
		return dictionary;
	}

	private static Dictionary<ETalkLevel, Item> GetPlyerComfortCase()
	{
		Dictionary<ETalkLevel, Item> dictionary = new Dictionary<ETalkLevel, Item>();
		dictionary[ETalkLevel.Common] = Get(14);
		dictionary[ETalkLevel.Medium] = Get(15);
		dictionary[ETalkLevel.Low] = Get(16);
		return dictionary;
	}
}

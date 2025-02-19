using System;
using System.Collections.Generic;
using System.Linq;
using ItemAsset;
using Mono.Data.SqliteClient;
using UnityEngine;

public class NpcAbility
{
	public int id;

	public string icon;

	public int desc;

	public int skillId;

	public int buffId;

	public float Percent;

	public float learnTime;

	public List<int> ProtoIds;

	private ItemProto.Bundle itemBundle;

	public List<List<string>> Cond;

	public static List<NpcAbility> mList = new List<NpcAbility>();

	private AblityType m_type;

	private int m_level;

	private float m_SkillRange;

	private float m_SkillPerCent;

	private float m_Correctrate;

	public AblityType Type => m_type;

	public int type
	{
		set
		{
			m_type = (AblityType)value;
		}
	}

	public SkillLevel Level => (SkillLevel)m_level;

	public int level
	{
		get
		{
			return m_level;
		}
		set
		{
			m_level = value;
		}
	}

	public float SkillRange => m_SkillRange;

	public float SkillPerCent => m_SkillPerCent;

	public float Correctrate => m_Correctrate;

	public List<MaterialItem> GetItem(float factor)
	{
		factor = Mathf.Clamp01(factor);
		if (itemBundle == null)
		{
			return null;
		}
		List<MaterialItem> items = itemBundle.GetItems();
		if (items == null)
		{
			return null;
		}
		foreach (MaterialItem item in items)
		{
			item.count = (int)((float)item.count * factor);
		}
		return items;
	}

	public bool Parse(string line)
	{
		if (string.IsNullOrEmpty(line))
		{
			return false;
		}
		string[] array = line.Split(';');
		for (int i = 0; i < array.Length; i++)
		{
			if (Cond == null)
			{
				Cond = new List<List<string>>();
			}
			string[] collection = array[i].Split(',');
			if (Cond.Count <= i)
			{
				Cond.Add(new List<string>());
			}
			Cond[i].AddRange(collection);
		}
		return true;
	}

	public bool Parse_value(string line)
	{
		if (string.IsNullOrEmpty(line))
		{
			return false;
		}
		string[] array = line.Split(',');
		for (int i = 0; i < array.Length; i++)
		{
			float num = float.Parse(array[i]);
			if (num < 1f)
			{
				Percent = num;
			}
			if (num > 1f)
			{
				if (ProtoIds == null)
				{
					ProtoIds = new List<int>();
				}
				ProtoIds.Add((int)num);
			}
		}
		return true;
	}

	public bool Isskill()
	{
		return skillId != 0;
	}

	public bool IsBuff()
	{
		return buffId != 0;
	}

	public bool IsTalent()
	{
		if (!Isskill() && !IsBuff() && !IsGetItem())
		{
			return true;
		}
		return false;
	}

	public bool IsGetItem()
	{
		return GetItem(1f) != null;
	}

	public bool CalculateCondtion()
	{
		foreach (List<string> item in Cond)
		{
			if (item != null && item.Count > 1)
			{
				if (item[0] == "range")
				{
					m_SkillRange = int.Parse(item[1]);
				}
				if (item[0] == "hppct")
				{
					m_SkillPerCent = float.Parse(item[1]);
				}
				if (item[0] == "HIT")
				{
					m_Correctrate = float.Parse(item[1]);
				}
			}
		}
		return true;
	}

	public static void Load()
	{
		SqliteDataReader sqliteDataReader = LocalDatabase.Instance.ReadFullTable("npcSkill");
		while (sqliteDataReader.Read())
		{
			NpcAbility npcAbility = new NpcAbility();
			npcAbility.id = Convert.ToInt32(sqliteDataReader.GetString(sqliteDataReader.GetOrdinal("id")));
			npcAbility.icon = sqliteDataReader.GetString(sqliteDataReader.GetOrdinal("icon"));
			npcAbility.desc = Convert.ToInt32(sqliteDataReader.GetString(sqliteDataReader.GetOrdinal("desc")));
			npcAbility.skillId = Convert.ToInt32(sqliteDataReader.GetString(sqliteDataReader.GetOrdinal("skill")));
			npcAbility.buffId = Convert.ToInt32(sqliteDataReader.GetString(sqliteDataReader.GetOrdinal("buff")));
			npcAbility.itemBundle = ItemProto.Bundle.Load(sqliteDataReader.GetString(sqliteDataReader.GetOrdinal("getItem")));
			npcAbility.level = Convert.ToInt32(sqliteDataReader.GetString(sqliteDataReader.GetOrdinal("level")));
			npcAbility.type = Convert.ToInt32(sqliteDataReader.GetString(sqliteDataReader.GetOrdinal("type")));
			npcAbility.learnTime = Convert.ToInt32(sqliteDataReader.GetString(sqliteDataReader.GetOrdinal("time")));
			string @string = sqliteDataReader.GetString(sqliteDataReader.GetOrdinal("cond"));
			npcAbility.Parse(@string);
			string string2 = sqliteDataReader.GetString(sqliteDataReader.GetOrdinal("value"));
			npcAbility.Parse_value(string2);
			mList.Add(npcAbility);
		}
	}

	public static NpcAbility FindNpcAbility(int id)
	{
		return mList.Find((NpcAbility iter) => iter.id == id);
	}

	public static NpcAbility FindNpcAbility(AblityType type, int level)
	{
		return mList.Find((NpcAbility iter) => iter.Type == type && iter.level == level);
	}

	public static NpcAbility FindNpcAblityBySkillId(int skillId)
	{
		return mList.Find((NpcAbility iter) => iter.skillId == skillId);
	}

	public static IEnumerable<NpcAbility> FindNpcAbilities(AblityType type)
	{
		return mList.Where((NpcAbility iter) => iter.Type == type);
	}

	public static IEnumerable<NpcAbility> FindNpcAbilities(IEnumerable<int> abilityIds)
	{
		return mList.Where(delegate(NpcAbility iter)
		{
			using (IEnumerator<int> enumerator = abilityIds.GetEnumerator())
			{
				if (enumerator.MoveNext())
				{
					int current = enumerator.Current;
					return iter.id == current;
				}
			}
			return false;
		});
	}

	public static IEnumerable<int> FindCoverAbilityId(int ablityId)
	{
		IEnumerable<NpcAbility> abilities = CoverAbility(ablityId);
		foreach (NpcAbility ability in abilities)
		{
			yield return ability.id;
		}
	}

	private static IEnumerable<NpcAbility> CoverAbility(int abiliyId)
	{
		NpcAbility ability = FindNpcAbility(abiliyId);
		if (ability == null)
		{
			yield break;
		}
		IEnumerable<NpcAbility> all = FindNpcAbilities(ability.Type);
		foreach (NpcAbility info in all)
		{
			if (ability.level > info.level)
			{
				yield return info;
			}
		}
	}

	public static NpcAbility FindNpcAbility(AblityType type, SkillLevel level)
	{
		return mList.Find((NpcAbility iter) => iter.Type == type && iter.Level == level);
	}

	private static NpcAbility CanAddBuff(int Id)
	{
		for (int i = 0; i < mList.Count; i++)
		{
			if (mList[i].id == Id)
			{
				if (mList[i].IsBuff())
				{
					return mList[i];
				}
				return null;
			}
		}
		return null;
	}

	public static float GetLearnTime(int abilityid)
	{
		return FindNpcAbility(abilityid)?.learnTime ?? 0f;
	}
}

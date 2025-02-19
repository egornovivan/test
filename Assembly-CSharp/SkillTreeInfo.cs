using System;
using System.Collections.Generic;
using Mono.Data.SqliteClient;

public class SkillTreeInfo
{
	public delegate void RefreshUIEvent(int mainType);

	private static Dictionary<int, Dictionary<int, Dictionary<int, SkillTreeUnit>>> _skillTreeInfo;

	private static Dictionary<int, List<SkillMainType>> _skillMainTypeInfo;

	private static RefreshUIEvent CallBackRefresh;

	public static Dictionary<int, List<SkillMainType>> SkillMainTypeInfo => _skillMainTypeInfo;

	public static void LoadData()
	{
		_skillTreeInfo = new Dictionary<int, Dictionary<int, Dictionary<int, SkillTreeUnit>>>();
		SqliteDataReader sqliteDataReader = LocalDatabase.Instance.ReadFullTable("skilltree");
		while (sqliteDataReader.Read())
		{
			int id = Convert.ToInt32(sqliteDataReader.GetString(sqliteDataReader.GetOrdinal("id")));
			int num = Convert.ToInt32(sqliteDataReader.GetString(sqliteDataReader.GetOrdinal("maintype")));
			int num2 = Convert.ToInt32(sqliteDataReader.GetString(sqliteDataReader.GetOrdinal("skilltype")));
			int skillGroup = Convert.ToInt32(sqliteDataReader.GetString(sqliteDataReader.GetOrdinal("skillgroup")));
			int skillGrade = Convert.ToInt32(sqliteDataReader.GetString(sqliteDataReader.GetOrdinal("skillgrade")));
			int num3 = Convert.ToInt32(sqliteDataReader.GetString(sqliteDataReader.GetOrdinal("level")));
			ulong exp = Convert.ToUInt64(sqliteDataReader.GetString(sqliteDataReader.GetOrdinal("exp")));
			string @string = sqliteDataReader.GetString(sqliteDataReader.GetOrdinal("parent"));
			string string2 = sqliteDataReader.GetString(sqliteDataReader.GetOrdinal("value"));
			string string3 = sqliteDataReader.GetString(sqliteDataReader.GetOrdinal("sprname"));
			int num4 = Convert.ToInt32(sqliteDataReader.GetString(sqliteDataReader.GetOrdinal("desc")));
			if (!_skillTreeInfo.ContainsKey(num))
			{
				_skillTreeInfo[num] = new Dictionary<int, Dictionary<int, SkillTreeUnit>>();
			}
			if (!_skillTreeInfo[num].ContainsKey(num2))
			{
				_skillTreeInfo[num][num2] = new Dictionary<int, SkillTreeUnit>();
			}
			SkillTreeUnit skillTreeUnit = new SkillTreeUnit();
			skillTreeUnit._id = id;
			skillTreeUnit._mainType = num;
			skillTreeUnit._skillType = num2;
			skillTreeUnit._skillGroup = skillGroup;
			skillTreeUnit._skillGrade = skillGrade;
			skillTreeUnit._level = num3;
			skillTreeUnit._exp = exp;
			skillTreeUnit._parent = @string;
			skillTreeUnit._value = string2;
			skillTreeUnit._sprName = string3;
			skillTreeUnit._descIndex = num4;
			skillTreeUnit._desc = PELocalization.GetString(num4);
			skillTreeUnit.Parse(string2);
			_skillTreeInfo[num][num2][num3] = skillTreeUnit;
		}
		foreach (KeyValuePair<int, Dictionary<int, Dictionary<int, SkillTreeUnit>>> item in _skillTreeInfo)
		{
			foreach (KeyValuePair<int, Dictionary<int, SkillTreeUnit>> item2 in item.Value)
			{
				foreach (KeyValuePair<int, SkillTreeUnit> item3 in item2.Value)
				{
					SkillTreeUnit maxLevelSkillByType = GetMaxLevelSkillByType(item3.Value._skillType);
					if (maxLevelSkillByType != null)
					{
						item3.Value._maxLevel = maxLevelSkillByType._level;
					}
				}
			}
		}
		LoadMainTypeData();
	}

	public static ulong GetEpx(int maintype, int skilltype, int level)
	{
		if (_skillTreeInfo.ContainsKey(maintype) && _skillTreeInfo[maintype].ContainsKey(skilltype) && _skillTreeInfo[maintype][skilltype].ContainsKey(level))
		{
			return _skillTreeInfo[maintype][skilltype][level]._exp;
		}
		return 0uL;
	}

	public static string GetParentSkill(int maintype, int skilltype, int level)
	{
		if (_skillTreeInfo.ContainsKey(maintype) && _skillTreeInfo[maintype].ContainsKey(skilltype) && _skillTreeInfo[maintype][skilltype].ContainsKey(level))
		{
			return _skillTreeInfo[maintype][skilltype][level]._parent;
		}
		return string.Empty;
	}

	public static string GetValue(int maintype, int skilltype, int level)
	{
		if (_skillTreeInfo.ContainsKey(maintype) && _skillTreeInfo[maintype].ContainsKey(skilltype) && _skillTreeInfo[maintype][skilltype].ContainsKey(level))
		{
			return _skillTreeInfo[maintype][skilltype][level]._value;
		}
		return null;
	}

	public static SkillTreeUnit GetSkillUnit(int id)
	{
		foreach (KeyValuePair<int, Dictionary<int, Dictionary<int, SkillTreeUnit>>> item in _skillTreeInfo)
		{
			foreach (KeyValuePair<int, Dictionary<int, SkillTreeUnit>> item2 in item.Value)
			{
				foreach (KeyValuePair<int, SkillTreeUnit> item3 in item2.Value)
				{
					if (item3.Value._id == id)
					{
						return item3.Value;
					}
				}
			}
		}
		return null;
	}

	public static SkillTreeUnit GetMinLevelSkillByType(int type)
	{
		SkillTreeUnit skillTreeUnit = null;
		foreach (KeyValuePair<int, Dictionary<int, Dictionary<int, SkillTreeUnit>>> item in _skillTreeInfo)
		{
			foreach (KeyValuePair<int, Dictionary<int, SkillTreeUnit>> item2 in item.Value)
			{
				foreach (KeyValuePair<int, SkillTreeUnit> item3 in item2.Value)
				{
					if (item3.Value._skillType == type)
					{
						if (skillTreeUnit == null)
						{
							skillTreeUnit = item3.Value;
						}
						else if (skillTreeUnit._level > item3.Value._level)
						{
							skillTreeUnit = item3.Value;
						}
					}
				}
			}
		}
		return skillTreeUnit;
	}

	public static SkillTreeUnit GetMaxLevelSkillByType(int type)
	{
		SkillTreeUnit skillTreeUnit = null;
		foreach (KeyValuePair<int, Dictionary<int, Dictionary<int, SkillTreeUnit>>> item in _skillTreeInfo)
		{
			foreach (KeyValuePair<int, Dictionary<int, SkillTreeUnit>> item2 in item.Value)
			{
				foreach (KeyValuePair<int, SkillTreeUnit> item3 in item2.Value)
				{
					if (item3.Value._skillType == type)
					{
						if (skillTreeUnit == null)
						{
							skillTreeUnit = item3.Value;
						}
						else if (skillTreeUnit._level < item3.Value._level)
						{
							skillTreeUnit = item3.Value;
						}
					}
				}
			}
		}
		return skillTreeUnit;
	}

	public static List<SkillTreeUnit> GetMinLevelByMainType(int mainType)
	{
		List<SkillTreeUnit> list = new List<SkillTreeUnit>();
		if (_skillTreeInfo.ContainsKey(mainType))
		{
			foreach (KeyValuePair<int, Dictionary<int, SkillTreeUnit>> item in _skillTreeInfo[mainType])
			{
				SkillTreeUnit skillTreeUnit = null;
				foreach (KeyValuePair<int, SkillTreeUnit> item2 in item.Value)
				{
					if (skillTreeUnit == null)
					{
						skillTreeUnit = item2.Value;
					}
					if (skillTreeUnit != null && item2.Value != null && skillTreeUnit._level > item2.Value._level)
					{
						skillTreeUnit = item2.Value;
					}
				}
				if (skillTreeUnit != null)
				{
					list.Add(skillTreeUnit);
				}
			}
		}
		list?.Sort(delegate(SkillTreeUnit left, SkillTreeUnit right)
		{
			if (left._skillGrade > right._skillGrade)
			{
				return 1;
			}
			return (left._skillGrade != right._skillGrade) ? (-1) : 0;
		});
		return list;
	}

	public static SkillTreeUnit GetSkillUnit(int type, int level)
	{
		foreach (KeyValuePair<int, Dictionary<int, Dictionary<int, SkillTreeUnit>>> item in _skillTreeInfo)
		{
			foreach (KeyValuePair<int, Dictionary<int, SkillTreeUnit>> item2 in item.Value)
			{
				foreach (KeyValuePair<int, SkillTreeUnit> item3 in item2.Value)
				{
					if (item3.Value._skillType == type && item3.Value._level == level)
					{
						return item3.Value;
					}
				}
			}
		}
		return null;
	}

	private static void LoadMainTypeData()
	{
		_skillMainTypeInfo = new Dictionary<int, List<SkillMainType>>();
		SqliteDataReader sqliteDataReader = LocalDatabase.Instance.ReadFullTable("skilltreemt");
		while (sqliteDataReader.Read())
		{
			SkillMainType skillMainType = new SkillMainType();
			skillMainType._mainType = Convert.ToInt32(sqliteDataReader.GetString(sqliteDataReader.GetOrdinal("typeid")));
			string @string = sqliteDataReader.GetString(sqliteDataReader.GetOrdinal("icon"));
			string[] collection = @string.Split(',');
			skillMainType._icon.AddRange(collection);
			skillMainType._desc = PELocalization.GetString(Convert.ToInt32(sqliteDataReader.GetString(sqliteDataReader.GetOrdinal("desc"))));
			skillMainType._skillGroup = Convert.ToInt32(sqliteDataReader.GetString(sqliteDataReader.GetOrdinal("skillgroup")));
			if (!_skillMainTypeInfo.ContainsKey(skillMainType._skillGroup))
			{
				_skillMainTypeInfo[skillMainType._skillGroup] = new List<SkillMainType>();
			}
			_skillMainTypeInfo[skillMainType._skillGroup].Add(skillMainType);
		}
	}

	public static void RefreshUI(int mainType)
	{
		if (CallBackRefresh != null)
		{
			CallBackRefresh(mainType);
		}
	}

	private static void FindChindren(List<string> parentSkills, List<SkillTreeUnit> allSkills, Dictionary<string, List<SkillTreeUnit>> outSkills)
	{
		if (outSkills == null)
		{
			outSkills = new Dictionary<string, List<SkillTreeUnit>>();
		}
		Dictionary<string, List<SkillTreeUnit>> dictionary = new Dictionary<string, List<SkillTreeUnit>>();
		if (parentSkills == null || parentSkills.Count == 0)
		{
			for (int i = 0; i < allSkills.Count; i++)
			{
				if (allSkills[i]._parent == "0")
				{
					if (!dictionary.ContainsKey("0"))
					{
						dictionary["0"] = new List<SkillTreeUnit>();
					}
					dictionary["0"].Add(allSkills[i]);
					allSkills.RemoveAt(i);
					i--;
				}
			}
		}
		else
		{
			for (int j = 0; j < allSkills.Count; j++)
			{
				string[] array = allSkills[j]._parent.Split(';');
				if (array.Length <= 0)
				{
					continue;
				}
				bool flag = false;
				for (int k = 0; k < array.Length; k++)
				{
					bool flag2 = false;
					for (int l = 0; l < parentSkills.Count; l++)
					{
						if (parentSkills[l] == array[k])
						{
							flag2 = true;
							break;
						}
					}
					if (!flag2)
					{
						break;
					}
					if (k == array.Length - 1)
					{
						flag = true;
					}
				}
				if (flag)
				{
					if (!dictionary.ContainsKey(allSkills[j]._parent))
					{
						dictionary[allSkills[j]._parent] = new List<SkillTreeUnit>();
					}
					dictionary[allSkills[j]._parent].Add(allSkills[j]);
					allSkills.RemoveAt(j);
					j--;
				}
			}
		}
		if (dictionary.Count <= 0 || allSkills.Count == 0)
		{
			return;
		}
		foreach (KeyValuePair<string, List<SkillTreeUnit>> item in dictionary)
		{
			if (!outSkills.ContainsKey(item.Key))
			{
				outSkills[item.Key] = new List<SkillTreeUnit>();
			}
			outSkills[item.Key].AddRange(item.Value);
			List<string> list = new List<string>();
			list.AddRange(item.Key.Split(';'));
			FindChindren(list, allSkills, outSkills);
		}
	}

	public static void SetUICallBack(RefreshUIEvent callBack)
	{
		CallBackRefresh = callBack;
	}

	public static Dictionary<int, List<SkillTreeUnit>> GetUIShowList(int mainType, SkillTreeUnitMgr mgr)
	{
		List<SkillTreeUnit> minLevelByMainType = GetMinLevelByMainType(mainType);
		List<SkillTreeUnit> skillsByMainType = mgr.GetSkillsByMainType(mainType);
		Dictionary<int, List<SkillTreeUnit>> dictionary = new Dictionary<int, List<SkillTreeUnit>>();
		for (int i = 0; i < minLevelByMainType.Count; i++)
		{
			for (int j = 0; j < skillsByMainType.Count; j++)
			{
				if (skillsByMainType[j]._skillType == minLevelByMainType[i]._skillType)
				{
					minLevelByMainType[i] = skillsByMainType[j];
					break;
				}
			}
			mgr.ChangeSkillState(minLevelByMainType[i]);
			if (!dictionary.ContainsKey(minLevelByMainType[i]._skillGrade))
			{
				dictionary[minLevelByMainType[i]._skillGrade] = new List<SkillTreeUnit>();
			}
			dictionary[minLevelByMainType[i]._skillGrade].Add(minLevelByMainType[i]);
		}
		foreach (KeyValuePair<int, List<SkillTreeUnit>> item in dictionary)
		{
			if (item.Value == null || item.Value.Count <= 1)
			{
				continue;
			}
			item.Value.Sort(delegate(SkillTreeUnit left, SkillTreeUnit right)
			{
				if (left._descIndex > right._descIndex)
				{
					return 1;
				}
				return (left._descIndex != right._descIndex) ? (-1) : 0;
			});
		}
		return dictionary;
	}
}

using System;
using System.Collections.Generic;
using SkillSystem;

public class SkillTreeUnit
{
	public int _id;

	public int _mainType;

	public int _skillType;

	public int _skillGroup;

	public int _skillGrade;

	public int _level;

	public ulong _exp;

	public string _parent;

	public string _value;

	public string _sprName;

	public SkillState _state;

	public string _desc;

	public int _maxLevel;

	private List<List<string>> cmd = new List<List<string>>();

	public SkillTreeUnit()
	{
		_state = SkillState.Lock;
	}

	public bool CheckEquipEnable(int type, int level)
	{
		foreach (List<string> item in cmd)
		{
			if (item == null || item.Count <= 1 || !(item[0] == "equiplimit"))
			{
				continue;
			}
			for (int i = 1; i < item.Count; i += 2)
			{
				if (Convert.ToInt32(item[i]) == type && Convert.ToInt32(item[i + 1]) <= level)
				{
					return true;
				}
			}
		}
		return false;
	}

	public bool CheckDriveEnable(int type, int level)
	{
		foreach (List<string> item in cmd)
		{
			if (item == null || item.Count <= 1 || !(item[0] == "drivelimit"))
			{
				continue;
			}
			for (int i = 1; i < item.Count; i += 2)
			{
				if (Convert.ToInt32(item[i]) == type && Convert.ToInt32(item[i + 1]) <= level)
				{
					return true;
				}
			}
		}
		return false;
	}

	public bool CheckMinerGetRare()
	{
		foreach (List<string> item in cmd)
		{
			if (item.Count <= 1 || !(item[0] == "minergetrare"))
			{
				continue;
			}
			for (int num = 1; num < item.Count; num = num++)
			{
				if (Convert.ToInt32(item[num]) == 1)
				{
					return true;
				}
			}
		}
		return false;
	}

	public bool CheckCutterGetRare()
	{
		foreach (List<string> item in cmd)
		{
			if (item.Count <= 1 || !(item[0] == "cuttergetrare"))
			{
				continue;
			}
			for (int num = 1; num < item.Count; num = num++)
			{
				if (Convert.ToInt32(item[num]) == 1)
				{
					return true;
				}
			}
		}
		return false;
	}

	public bool CheckHunterGetRare()
	{
		foreach (List<string> item in cmd)
		{
			if (item != null && item.Count == 1 && item[0] == "huntergetrare")
			{
				return true;
			}
		}
		return false;
	}

	public bool CheckUnlockColony(int colonytype)
	{
		foreach (List<string> item in cmd)
		{
			if (item == null || item.Count != 1 || !(item[0] == "unlockcolony"))
			{
				continue;
			}
			for (int num = 1; num < item.Count; num = num++)
			{
				if (Convert.ToInt32(item[num]) == colonytype)
				{
					return true;
				}
			}
		}
		return false;
	}

	public bool CheckUnlockProductItemType(int unlocktype)
	{
		foreach (List<string> item in cmd)
		{
			if (item == null || item.Count <= 1 || !(item[0] == "unlockitem"))
			{
				continue;
			}
			for (int num = 1; num < item.Count; num = num++)
			{
				if (Convert.ToInt32(item[num]) == unlocktype)
				{
					return true;
				}
			}
		}
		return false;
	}

	public bool RefreshBuff(SkEntity ske)
	{
		return true;
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
			if (cmd == null)
			{
				cmd = new List<List<string>>();
			}
			string[] collection = array[i].Split(',');
			if (cmd.Count <= i)
			{
				cmd.Add(new List<string>());
			}
			cmd[i].AddRange(collection);
		}
		return true;
	}
}

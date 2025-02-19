using System;
using System.Collections.Generic;
using Pathea;
using SkillSystem;

public class SkillTreeUnit
{
	public const bool ENABLESKTSYSTEM = false;

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

	public int _descIndex;

	public int _maxLevel;

	private List<List<string>> cmd = new List<List<string>>();

	public SkillTreeUnit()
	{
		_state = SkillState.Lock;
	}

	public bool CheckLockItemType(int locktype)
	{
		foreach (List<string> item in cmd)
		{
			if (item == null || item.Count <= 1 || !(item[0] == "lockitem"))
			{
				continue;
			}
			for (int i = 1; i < item.Count; i++)
			{
				if (Convert.ToInt32(item[i]) == locktype)
				{
					return true;
				}
			}
		}
		return false;
	}

	public bool CheckLockColonyType(int locktype)
	{
		foreach (List<string> item in cmd)
		{
			if (item == null || item.Count <= 1 || !(item[0] == "lockcolony"))
			{
				continue;
			}
			for (int i = 1; i < item.Count; i++)
			{
				if (Convert.ToInt32(item[i]) == locktype)
				{
					return true;
				}
			}
		}
		return false;
	}

	private bool CheckLockProduceItemType(int locktype)
	{
		foreach (List<string> item in cmd)
		{
			if (item == null || item.Count <= 1 || !(item[0] == "lockpitem"))
			{
				continue;
			}
			for (int i = 1; i < item.Count; i++)
			{
				if (Convert.ToInt32(item[i]) == locktype)
				{
					return true;
				}
			}
		}
		return false;
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
				if (Convert.ToInt32(item[i]) == type && Convert.ToInt32(item[i + 1]) >= level)
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
				if (Convert.ToInt32(item[i]) == type && Convert.ToInt32(item[i + 1]) >= level)
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
			for (int i = 1; i < item.Count; i++)
			{
				if (Convert.ToInt32(item[i]) == 1)
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
			for (int i = 1; i < item.Count; i++)
			{
				if (Convert.ToInt32(item[i]) == 1)
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
			if (item == null || item.Count <= 1 || !(item[0] == "unlockcolony"))
			{
				continue;
			}
			for (int i = 1; i < item.Count; i++)
			{
				if (Convert.ToInt32(item[i]) == colonytype)
				{
					return true;
				}
			}
		}
		return false;
	}

	public bool CheckUnlockProductItemLevel(int unlocklevel)
	{
		foreach (List<string> item in cmd)
		{
			if (item != null && item.Count > 1 && item[0] == "unlockproductlevel" && unlocklevel <= Convert.ToInt32(item[1]))
			{
				return true;
			}
		}
		return false;
	}

	public bool CheckUnlockProductItemType(int unlocktype)
	{
		if (CheckLockProduceItemType(unlocktype))
		{
			foreach (List<string> item in cmd)
			{
				if (item == null || item.Count <= 1 || !(item[0] == "unlockproducttype"))
				{
					continue;
				}
				for (int i = 1; i < item.Count; i++)
				{
					if (Convert.ToInt32(item[i]) == unlocktype)
					{
						return true;
					}
				}
			}
			return false;
		}
		return true;
	}

	public float CheckReduceTime(float srcTime)
	{
		foreach (List<string> item in cmd)
		{
			if (item != null && item.Count > 1 && item[0] == "reducetime")
			{
				srcTime -= srcTime * Convert.ToSingle(item[1]);
			}
		}
		return srcTime;
	}

	public bool CheckBuildShape(int index)
	{
		foreach (List<string> item in cmd)
		{
			if (item != null && item.Count > 1 && item[0] == "buildshape" && index <= Convert.ToInt32(item[1]))
			{
				return true;
			}
		}
		return false;
	}

	public bool CheckBuildBlockLevel(int level)
	{
		foreach (List<string> item in cmd)
		{
			if (item != null && item.Count > 1 && item[0] == "blocklevel" && level <= Convert.ToInt32(item[1]))
			{
				return true;
			}
		}
		return false;
	}

	public bool CheckUnlockBuildBlockBevel()
	{
		foreach (List<string> item in cmd)
		{
			if (item != null && item.Count > 1 && item[0] == "ulblockbevel" && Convert.ToInt32(item[1]) == 1)
			{
				return true;
			}
		}
		return false;
	}

	public bool CheckUnlockBuildBlockIso()
	{
		foreach (List<string> item in cmd)
		{
			if (item != null && item.Count > 1 && item[0] == "unlockbiso" && Convert.ToInt32(item[1]) == 1)
			{
				return true;
			}
		}
		return false;
	}

	public bool CheckUnlockBuildBlockVoxel()
	{
		foreach (List<string> item in cmd)
		{
			if (item != null && item.Count > 1 && item[0] == "unlockbvoxel" && Convert.ToInt32(item[1]) == 1)
			{
				return true;
			}
		}
		return false;
	}

	public bool CheckUnlockCusProduct(int unlocktype)
	{
		foreach (List<string> item in cmd)
		{
			if (item == null || item.Count <= 1 || !(item[0] == "ulcusproduct"))
			{
				continue;
			}
			for (int i = 1; i < item.Count; i++)
			{
				if (Convert.ToInt32(item[i]) == unlocktype)
				{
					return true;
				}
			}
		}
		return false;
	}

	public bool RefreshBuff(SkAliveEntity ske)
	{
		if (ske == null)
		{
			return false;
		}
		foreach (List<string> item in cmd)
		{
			if (item == null || item.Count <= 1)
			{
				continue;
			}
			if (item[0] == "buff")
			{
				int buffId = Convert.ToInt32(item[1]);
				List<int> list = new List<int>();
				List<float> list2 = new List<float>();
				for (int i = 2; i < item.Count; i += 2)
				{
					list.Add(Convert.ToInt32(item[i]));
					list2.Add(Convert.ToSingle(item[i + 1]));
				}
				if (list.Count > 0 && list2.Count > 0)
				{
					SkEntity.UnmountBuff(ske, buffId);
					SkEntity.MountBuff(ske, buffId, list, list2);
				}
			}
			else if (item[0] == "scanradius")
			{
				int radius = Convert.ToInt32(item[1]);
				MSScan.Instance.radius = radius;
			}
			else if (item[0] == "scanmat")
			{
				for (int j = 1; j < item.Count; j++)
				{
					MetalScanData.AddMetalScan(Convert.ToInt32(item[j]));
				}
			}
		}
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

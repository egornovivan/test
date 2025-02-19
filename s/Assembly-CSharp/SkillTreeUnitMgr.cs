using System.Collections.Generic;
using UnityEngine;

public class SkillTreeUnitMgr : ISkillTree
{
	[HideInInspector]
	private List<SkillTreeUnit> _learntSkills = new List<SkillTreeUnit>();

	public List<int> ExportLearntIDs()
	{
		List<int> list = new List<int>();
		foreach (SkillTreeUnit learntSkill in _learntSkills)
		{
			if (learntSkill != null)
			{
				list.Add(learntSkill._id);
			}
		}
		return list;
	}

	public void ImportLearntSkillIDs(List<int> learntSkillsID)
	{
		foreach (int item in learntSkillsID)
		{
			SkillTreeUnit skillUnit = SkillTreeInfo.GetSkillUnit(item);
			if (skillUnit != null)
			{
				_learntSkills.Add(skillUnit);
			}
		}
	}

	public SkillTreeUnit FindSkillUnit(int SkillType)
	{
		foreach (SkillTreeUnit learntSkill in _learntSkills)
		{
			if (learntSkill._skillType == SkillType)
			{
				return learntSkill;
			}
		}
		return null;
	}

	public void RemoveSkillUnit(SkillTreeUnit unit)
	{
		_learntSkills.Remove(unit);
	}

	public void AddSkillUnit(SkillTreeUnit unit)
	{
		if (!_learntSkills.Contains(unit))
		{
			_learntSkills.Add(unit);
		}
	}

	public SkillTreeUnit FindSkillUnitByID(int skillid)
	{
		foreach (SkillTreeUnit learntSkill in _learntSkills)
		{
			if (learntSkill._id == skillid)
			{
				return learntSkill;
			}
		}
		return null;
	}

	public List<SkillTreeUnit> GetSkillsByMainType(int mainType)
	{
		if (_learntSkills == null)
		{
			return null;
		}
		List<SkillTreeUnit> list = new List<SkillTreeUnit>();
		foreach (SkillTreeUnit learntSkill in _learntSkills)
		{
			if (learntSkill._mainType == mainType)
			{
				list.Add(learntSkill);
			}
		}
		return list;
	}

	public void ChangeSkillState(SkillTreeUnit skill)
	{
		if (_learntSkills.Contains(skill))
		{
			skill._state = SkillState.learnt;
			return;
		}
		string[] array = skill._parent.Split(';');
		for (int i = 0; i < array.Length; i++)
		{
			SkillTreeUnit skillUnit = SkillTreeInfo.GetSkillUnit(int.Parse(array[i]));
			if (skillUnit == null)
			{
				if (array[i] == "0")
				{
					skill._state = SkillState.unLock;
					break;
				}
				skill._state = SkillState.Lock;
				if (LogFilter.logDebug)
				{
					Debug.LogError("parent skill is not exsit");
				}
				break;
			}
			SkillTreeUnit skillTreeUnit = FindSkillUnit(skillUnit._skillType);
			if (skillTreeUnit != null)
			{
				if (skillTreeUnit._level < skillUnit._level)
				{
					skill._state = SkillState.Lock;
					break;
				}
				if (i == array.Length - 1)
				{
					skill._state = SkillState.unLock;
				}
			}
		}
	}

	public bool CheckEquipEnable(int type, int level)
	{
		foreach (SkillTreeUnit learntSkill in _learntSkills)
		{
			if (learntSkill != null && learntSkill.CheckEquipEnable(type, level))
			{
				return true;
			}
		}
		return false;
	}

	public bool CheckDriveEnable(int type, int level)
	{
		foreach (SkillTreeUnit learntSkill in _learntSkills)
		{
			if (learntSkill != null && learntSkill.CheckDriveEnable(type, level))
			{
				return true;
			}
		}
		return false;
	}

	public bool CheckMinerGetRare()
	{
		foreach (SkillTreeUnit learntSkill in _learntSkills)
		{
			if (learntSkill != null && learntSkill.CheckMinerGetRare())
			{
				return true;
			}
		}
		return false;
	}

	public bool CheckCutterGetRare()
	{
		foreach (SkillTreeUnit learntSkill in _learntSkills)
		{
			if (learntSkill != null && learntSkill.CheckCutterGetRare())
			{
				return true;
			}
		}
		return false;
	}

	public bool CheckHunterGetRare()
	{
		foreach (SkillTreeUnit learntSkill in _learntSkills)
		{
			if (learntSkill != null && learntSkill.CheckHunterGetRare())
			{
				return true;
			}
		}
		return false;
	}

	public bool CheckUnlockProductItemType(int colonytype)
	{
		foreach (SkillTreeUnit learntSkill in _learntSkills)
		{
			if (learntSkill != null && learntSkill.CheckUnlockProductItemType(colonytype))
			{
				return true;
			}
		}
		return false;
	}

	public bool CheckUnlockColony(int itemclasstype)
	{
		foreach (SkillTreeUnit learntSkill in _learntSkills)
		{
			if (learntSkill != null && learntSkill.CheckUnlockColony(itemclasstype))
			{
				return true;
			}
		}
		return false;
	}

	public void RefreshAllBuffs()
	{
	}
}

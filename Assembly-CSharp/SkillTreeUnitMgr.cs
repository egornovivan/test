using System.Collections.Generic;
using System.IO;
using Pathea;
using UnityEngine;

public class SkillTreeUnitMgr : PeCmpt, ISkillTree
{
	[HideInInspector]
	public NetworkInterface _net;

	private List<SkillTreeUnit> _learntSkills = new List<SkillTreeUnit>();

	private SkAliveEntity _ske;

	public static int _defaultSkillType;

	public override void Start()
	{
		base.Start();
		_ske = base.gameObject.GetComponent<SkAliveEntity>();
		InitDefaultSkill();
		RefreshAllBuffs();
	}

	public void InitDefaultSkill()
	{
		if (RandomMapConfig.useSkillTree && _learntSkills.Find((SkillTreeUnit iter) => iter._skillType == _defaultSkillType) == null)
		{
			SKTLearn(_defaultSkillType);
		}
	}

	public void SetNet(NetworkInterface net)
	{
		_net = net;
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
			unit.RefreshBuff(_ske);
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
		skill._state = SkillState.Lock;
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
				Debug.LogError("parent skill is not exsit");
				break;
			}
			SkillTreeUnit skillTreeUnit = FindSkillUnit(skillUnit._skillType);
			if (skillTreeUnit != null)
			{
				if (skillTreeUnit._level >= skillUnit._level)
				{
					if (i == array.Length - 1)
					{
						skill._state = SkillState.unLock;
					}
					continue;
				}
				skill._state = SkillState.Lock;
				break;
			}
			skill._state = SkillState.Lock;
			break;
		}
	}

	private bool CheckCommon(int level = 1)
	{
		if (!PeGameMgr.IsAdventure)
		{
			return true;
		}
		if (!RandomMapConfig.useSkillTree)
		{
			return true;
		}
		if (level == 0)
		{
			return true;
		}
		return false;
	}

	public bool CheckEquipEnable(int type, int level)
	{
		if (CheckCommon(level))
		{
			return true;
		}
		bool flag = false;
		foreach (SkillTreeUnit learntSkill in _learntSkills)
		{
			if (learntSkill != null && learntSkill.CheckLockItemType(type))
			{
				flag = true;
			}
		}
		if (flag)
		{
			foreach (SkillTreeUnit learntSkill2 in _learntSkills)
			{
				if (learntSkill2 != null && learntSkill2.CheckEquipEnable(type, level))
				{
					return true;
				}
			}
			new PeTipMsg(PELocalization.GetString(8000854), PeTipMsg.EMsgLevel.Norm);
			return false;
		}
		return true;
	}

	public bool CheckDriveEnable(int type, int level)
	{
		if (CheckCommon(level))
		{
			return true;
		}
		foreach (SkillTreeUnit learntSkill in _learntSkills)
		{
			if (learntSkill != null && learntSkill.CheckDriveEnable(type, level))
			{
				return true;
			}
		}
		new PeTipMsg(PELocalization.GetString(8000854), PeTipMsg.EMsgLevel.Norm);
		return false;
	}

	public bool CheckMinerGetRare()
	{
		if (CheckCommon())
		{
			return true;
		}
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
		if (CheckCommon())
		{
			return true;
		}
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
		if (CheckCommon())
		{
			return true;
		}
		foreach (SkillTreeUnit learntSkill in _learntSkills)
		{
			if (learntSkill != null && learntSkill.CheckHunterGetRare())
			{
				return true;
			}
		}
		return false;
	}

	public bool CheckUnlockColony(int itemclasstype)
	{
		if (CheckCommon())
		{
			return true;
		}
		bool flag = false;
		foreach (SkillTreeUnit learntSkill in _learntSkills)
		{
			if (learntSkill != null && learntSkill.CheckLockColonyType(itemclasstype))
			{
				flag = true;
			}
		}
		if (flag)
		{
			foreach (SkillTreeUnit learntSkill2 in _learntSkills)
			{
				if (learntSkill2 != null && learntSkill2.CheckUnlockColony(itemclasstype))
				{
					return true;
				}
			}
			new PeTipMsg(PELocalization.GetString(8000854), PeTipMsg.EMsgLevel.Norm);
			return false;
		}
		return true;
	}

	public bool CheckUnlockProductItemLevel(int unlocklevel)
	{
		if (CheckCommon())
		{
			return true;
		}
		foreach (SkillTreeUnit learntSkill in _learntSkills)
		{
			if (learntSkill != null && learntSkill.CheckUnlockProductItemLevel(unlocklevel))
			{
				return true;
			}
		}
		return false;
	}

	public bool CheckUnlockProductItemType(int unlocktype)
	{
		if (CheckCommon())
		{
			return true;
		}
		foreach (SkillTreeUnit learntSkill in _learntSkills)
		{
			if (learntSkill != null && learntSkill.CheckUnlockProductItemType(unlocktype))
			{
				return true;
			}
		}
		return false;
	}

	public float CheckReduceTime(float srcTime)
	{
		if (CheckCommon())
		{
			return srcTime;
		}
		foreach (SkillTreeUnit learntSkill in _learntSkills)
		{
			learntSkill?.CheckReduceTime(srcTime);
		}
		return srcTime;
	}

	public bool CheckBuildShape(int index)
	{
		if (CheckCommon())
		{
			return true;
		}
		foreach (SkillTreeUnit learntSkill in _learntSkills)
		{
			if (learntSkill != null && learntSkill.CheckBuildShape(index))
			{
				return true;
			}
		}
		return false;
	}

	public bool CheckBuildBlockLevel(int level)
	{
		if (CheckCommon())
		{
			return true;
		}
		foreach (SkillTreeUnit learntSkill in _learntSkills)
		{
			if (learntSkill != null && learntSkill.CheckBuildBlockLevel(level))
			{
				return true;
			}
		}
		return false;
	}

	public bool CheckUnlockBuildBlockBevel()
	{
		if (CheckCommon())
		{
			return true;
		}
		foreach (SkillTreeUnit learntSkill in _learntSkills)
		{
			if (learntSkill != null && learntSkill.CheckUnlockBuildBlockBevel())
			{
				return true;
			}
		}
		return false;
	}

	public bool CheckUnlockBuildBlockIso()
	{
		if (CheckCommon())
		{
			return true;
		}
		foreach (SkillTreeUnit learntSkill in _learntSkills)
		{
			if (learntSkill != null && learntSkill.CheckUnlockBuildBlockIso())
			{
				return true;
			}
		}
		return false;
	}

	public bool CheckUnlockBuildBlockVoxel()
	{
		if (CheckCommon())
		{
			return true;
		}
		foreach (SkillTreeUnit learntSkill in _learntSkills)
		{
			if (learntSkill != null && learntSkill.CheckUnlockBuildBlockVoxel())
			{
				return true;
			}
		}
		return false;
	}

	public bool CheckUnlockCusProduct(int unlocktype)
	{
		if (CheckCommon())
		{
			return true;
		}
		foreach (SkillTreeUnit learntSkill in _learntSkills)
		{
			if (learntSkill != null && learntSkill.CheckUnlockCusProduct(unlocktype))
			{
				return true;
			}
		}
		return false;
	}

	public void RefreshAllBuffs()
	{
		foreach (SkillTreeUnit learntSkill in _learntSkills)
		{
			learntSkill?.RefreshBuff(_ske);
		}
	}

	private bool DecExp(ulong needExp)
	{
		if (_ske == null)
		{
			return false;
		}
		if (_ske.GetAttribute(74) < (float)needExp)
		{
			return false;
		}
		_ske.SetAttribute(74, _ske.GetAttribute(74) - (float)needExp);
		return true;
	}

	private SKTLearnResult CheckLevelUpCondition(SkillTreeUnit skill)
	{
		if (_ske == null)
		{
			return SKTLearnResult.SKTLearnResult_SkAliveEntityIsNULL;
		}
		if (skill != null)
		{
			if (_ske.GetAttribute(74) < (float)skill._exp)
			{
				return SKTLearnResult.SKTLearnResult_DontHaveEnoughExp;
			}
			if (skill._parent != "0")
			{
				string[] array = skill._parent.Split(';');
				for (int i = 0; i < array.Length; i++)
				{
					SkillTreeUnit skillUnit = SkillTreeInfo.GetSkillUnit(int.Parse(array[i]));
					if (skillUnit == null)
					{
						return SKTLearnResult.SKTLearnResult_DataError;
					}
					SkillTreeUnit skillTreeUnit = FindSkillUnit(skillUnit._skillType);
					if (skillTreeUnit != null)
					{
						if (skillTreeUnit._level < skillUnit._level)
						{
							return SKTLearnResult.SKTLearnResult_NeedLearntParentSkill;
						}
						if (i == array.Length - 1)
						{
							return SKTLearnResult.SKTLearnResult_Success;
						}
					}
				}
				return SKTLearnResult.SKTLearnResult_DataError;
			}
			return SKTLearnResult.SKTLearnResult_Success;
		}
		return SKTLearnResult.SKTLearnResult_DataError;
	}

	public SKTLearnResult SKTLearn(int skillType)
	{
		SkillTreeUnit skillTreeUnit = FindSkillUnit(skillType);
		SkillTreeUnit skillTreeUnit2;
		if (skillTreeUnit != null)
		{
			int level = skillTreeUnit._level + 1;
			skillTreeUnit2 = SkillTreeInfo.GetSkillUnit(skillType, level);
			if (skillTreeUnit2 == null)
			{
				return SKTLearnResult.SKTLearnResult_DataError;
			}
			SKTLearnResult sKTLearnResult = CheckLevelUpCondition(skillTreeUnit2);
			if (sKTLearnResult != 0)
			{
				return sKTLearnResult;
			}
			if (!DecExp(skillTreeUnit2._exp))
			{
				return SKTLearnResult.SKTLearnResult_DontHaveEnoughExp;
			}
			RemoveSkillUnit(skillTreeUnit);
			AddSkillUnit(skillTreeUnit2);
		}
		else
		{
			skillTreeUnit2 = SkillTreeInfo.GetMinLevelSkillByType(skillType);
			if (skillTreeUnit2 == null)
			{
				return SKTLearnResult.SKTLearnResult_DataError;
			}
			SKTLearnResult sKTLearnResult2 = CheckLevelUpCondition(skillTreeUnit2);
			if (sKTLearnResult2 != 0)
			{
				return sKTLearnResult2;
			}
			if (!DecExp(skillTreeUnit2._exp))
			{
				return SKTLearnResult.SKTLearnResult_DontHaveEnoughExp;
			}
			AddSkillUnit(skillTreeUnit2);
		}
		SkillTreeInfo.RefreshUI(skillTreeUnit2._mainType);
		if (GameConfig.IsMultiMode && _net != null)
		{
			_net.RPCServer(EPacketType.PT_InGame_SKTLevelUp, skillType, PlayerNetwork.mainPlayerId);
		}
		return SKTLearnResult.SKTLearnResult_Success;
	}

	public ulong GetNextExpBySkillType(int skillType)
	{
		SkillTreeUnit skillTreeUnit = FindSkillUnit(skillType);
		if (skillTreeUnit != null)
		{
			int level = skillTreeUnit._level + 1;
			return SkillTreeInfo.GetSkillUnit(skillType, level)?._exp ?? 0;
		}
		return SkillTreeInfo.GetMinLevelSkillByType(skillType)?._exp ?? 0;
	}

	public override void Serialize(BinaryWriter _out)
	{
		if (_net != null)
		{
			return;
		}
		_out.Write(_learntSkills.Count);
		foreach (SkillTreeUnit learntSkill in _learntSkills)
		{
			_out.Write(learntSkill._id);
		}
	}

	public override void Deserialize(BinaryReader _in)
	{
		if (_net != null)
		{
			return;
		}
		int num = _in.ReadInt32();
		for (int i = 0; i < num; i++)
		{
			int id = _in.ReadInt32();
			SkillTreeUnit skillUnit = SkillTreeInfo.GetSkillUnit(id);
			if (skillUnit != null)
			{
				AddSkillUnit(skillUnit);
			}
		}
	}
}

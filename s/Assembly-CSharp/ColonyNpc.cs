using System.Collections.Generic;
using Pathea;
using SkillSystem;
using UnityEngine;

public class ColonyNpc
{
	public AiAdNpcNetwork _refNpc;

	public int _npcID = -1;

	public CSPersonnelData _data;

	public Vector3 Pos
	{
		get
		{
			if (_refNpc == null)
			{
				return Vector3.zero;
			}
			return _refNpc.transform.position;
		}
	}

	public int TeamId
	{
		get
		{
			if (_refNpc != null)
			{
				return _refNpc.TeamId;
			}
			return -1;
		}
	}

	public int m_State
	{
		get
		{
			return _data.m_State;
		}
		set
		{
			_data.m_State = value;
		}
	}

	public int m_WorkRoomID
	{
		get
		{
			return _data.m_WorkRoomID;
		}
		set
		{
			_updateWorkRoom(_data.m_WorkRoomID, value);
		}
	}

	public int m_DwellingsID
	{
		get
		{
			return _data.m_DwellingsID;
		}
		set
		{
			_data.m_DwellingsID = value;
		}
	}

	public int m_Occupation
	{
		get
		{
			return _data.m_Occupation;
		}
		set
		{
			_updateOccupation(_data.m_Occupation, value);
		}
	}

	public int m_WorkMode
	{
		get
		{
			return _data.m_WorkMode;
		}
		set
		{
			_data.m_WorkMode = value;
		}
	}

	public Vector3 m_GuardPos
	{
		get
		{
			return _data.m_GuardPos;
		}
		set
		{
			_data.m_GuardPos = value;
		}
	}

	public int m_ProcessingIndex
	{
		get
		{
			return _data.m_ProcessingIndex;
		}
		set
		{
			_data.m_ProcessingIndex = value;
		}
	}

	public bool m_IsProcessing
	{
		get
		{
			return _data.m_IsProcessing;
		}
		set
		{
			_data.m_IsProcessing = value;
		}
	}

	public ETrainerType trainerType
	{
		get
		{
			return (ETrainerType)_data.m_TrainerType;
		}
		set
		{
			_data.m_TrainerType = (int)value;
		}
	}

	public ETrainingType trainingType
	{
		get
		{
			return (ETrainingType)_data.m_TrainingType;
		}
		set
		{
			_data.m_TrainingType = (int)value;
		}
	}

	public bool IsTraining
	{
		get
		{
			return _data.m_IsTraining;
		}
		set
		{
			_data.m_IsTraining = value;
		}
	}

	public float GetEnhanceSkill => _refNpc.Npcskillcmpt.GetTalentPercent(AblityType.Reinforce);

	public float GetRecycleSkill => _refNpc.Npcskillcmpt.GetTalentPercent(AblityType.Disassembly);

	public float GetRepairSkill => _refNpc.Npcskillcmpt.GetTalentPercent(AblityType.Repair);

	public float GetCompoundSkill => _refNpc.Npcskillcmpt.GetTalentPercent(AblityType.Arts);

	public float GetDiagnoseTimeSkill => _refNpc.Npcskillcmpt.GetTalentPercent(AblityType.Diagnose);

	public float GetDiagnoseChanceSkill => _refNpc.Npcskillcmpt.GetCorrectRate(AblityType.Diagnose);

	public float GetTreatTimeSkill => _refNpc.Npcskillcmpt.GetTalentPercent(AblityType.Medical);

	public float GetTreatChanceSkill => _refNpc.Npcskillcmpt.GetCorrectRate(AblityType.Medical);

	public float GetTentTimeSkill => _refNpc.Npcskillcmpt.GetTalentPercent(AblityType.Nurse);

	public float GetProcessingTimeSkill => _refNpc.Npcskillcmpt.GetTalentPercent(AblityType.Explore);

	public float GetFarmingSkill => _refNpc.Npcskillcmpt.GetTalentPercent(AblityType.Farming);

	public float GetHarvestSkill => _refNpc.Npcskillcmpt.GetTalentPercent(AblityType.Harvest);

	public List<int> GetNpcAllSkill
	{
		get
		{
			return _refNpc.Abilities;
		}
		set
		{
			_refNpc.Abilities = value;
		}
	}

	public bool IsRandomNpc => _refNpc.IsRandomNpc;

	public int ProtoId => _refNpc.ProtoId;

	public SkEntity _skEntity => (!(_refNpc == null)) ? _refNpc._skEntity : null;

	public int UpgradeTimes => _refNpc.Npcskillcmpt.curAttributeUpTimes;

	public ColonyNpc()
	{
		_data = new CSPersonnelData();
		_data.m_State = 2;
	}

	public void _updateOccupation(int old_occupa, int new_occupa)
	{
		if (old_occupa == new_occupa)
		{
			return;
		}
		switch (new_occupa)
		{
		case 0:
			if (_refNpc != null && _refNpc.IsFollower())
			{
				_refNpc.SetFollower(bFlag: false);
			}
			m_WorkRoomID = -1;
			m_WorkMode = 0;
			break;
		case 1:
			if (_refNpc != null && _refNpc.IsFollower())
			{
				_refNpc.SetFollower(bFlag: false);
			}
			m_WorkRoomID = -1;
			m_WorkMode = 1;
			break;
		case 3:
		{
			if (_refNpc != null && _refNpc.IsFollower())
			{
				_refNpc.SetFollower(bFlag: false);
			}
			ColonyBase oneColonyItemIsWorkingPrefered2 = ColonyMgr._Instance.GetOneColonyItemIsWorkingPrefered(TeamId, 1134);
			if (oneColonyItemIsWorkingPrefered2 != null)
			{
				m_WorkRoomID = oneColonyItemIsWorkingPrefered2.Id;
				oneColonyItemIsWorkingPrefered2.AddWorker(this);
			}
			else
			{
				m_WorkRoomID = -1;
			}
			m_WorkMode = 4;
			break;
		}
		case 2:
			if (_refNpc != null && _refNpc.IsFollower())
			{
				_refNpc.SetFollower(bFlag: false);
			}
			m_WorkRoomID = -1;
			m_WorkMode = 7;
			break;
		case 4:
			m_WorkRoomID = -1;
			break;
		case 5:
		{
			if (_refNpc != null && _refNpc.IsFollower())
			{
				_refNpc.SetFollower(bFlag: false);
			}
			m_IsProcessing = false;
			m_ProcessingIndex = -1;
			ColonyBase oneColonyItemIsWorkingPrefered3 = ColonyMgr._Instance.GetOneColonyItemIsWorkingPrefered(TeamId, 1356);
			if (oneColonyItemIsWorkingPrefered3 != null)
			{
				m_WorkRoomID = oneColonyItemIsWorkingPrefered3.Id;
				oneColonyItemIsWorkingPrefered3.AddWorker(this);
			}
			else
			{
				m_WorkRoomID = -1;
			}
			m_WorkMode = 0;
			break;
		}
		case 6:
			if (_refNpc != null && _refNpc.IsFollower())
			{
				_refNpc.SetFollower(bFlag: false);
			}
			m_WorkRoomID = -1;
			m_WorkMode = 0;
			break;
		case 7:
		{
			if (_refNpc != null && _refNpc.IsFollower())
			{
				_refNpc.SetFollower(bFlag: false);
			}
			ColonyBase oneColonyItemIsWorkingPrefered = ColonyMgr._Instance.GetOneColonyItemIsWorkingPrefered(TeamId, 1423);
			if (oneColonyItemIsWorkingPrefered != null)
			{
				m_WorkRoomID = oneColonyItemIsWorkingPrefered.Id;
				oneColonyItemIsWorkingPrefered.AddWorker(this);
			}
			else
			{
				m_WorkRoomID = -1;
			}
			m_WorkMode = 0;
			break;
		}
		}
		_data.m_Occupation = new_occupa;
	}

	public void _updateWorkRoom(int old_workroom, int new_workroom)
	{
		if (old_workroom != new_workroom)
		{
			if (old_workroom > 0)
			{
				ColonyMgr.GetColonyItemByObjId(old_workroom)?.RemoveWorker(this);
			}
			_data.m_WorkRoomID = new_workroom;
		}
	}

	public void SetIsProcessing(bool isProcessing)
	{
		m_IsProcessing = isProcessing;
		Save();
	}

	public void SetProcessingIndex(int index)
	{
		m_ProcessingIndex = index;
		Save();
	}

	public void AttributeUpgrade(AttribType type, float value)
	{
		if (_refNpc != null)
		{
			_refNpc.Npcskillcmpt.AttributeUpgrade(type);
			_refNpc.ChangeAttribute(type, value);
			if (null != _skEntity)
			{
				_skEntity.SaveData();
			}
		}
	}

	public bool CanAttributeUp()
	{
		if (_refNpc != null)
		{
			return _refNpc.Npcskillcmpt.CanAttributeUp();
		}
		return false;
	}

	public void Save()
	{
		ColonyNpcData colonyNpcData = new ColonyNpcData();
		colonyNpcData.ExportData(this);
		AsyncSqlite.AddRecord(colonyNpcData);
	}

	public void Delete()
	{
		ColonyNpcMgr.Delete(_npcID);
	}
}

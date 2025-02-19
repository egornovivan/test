using System;
using Pathea;
using Pathea.PeEntityExt;
using UnityEngine;

public class SceneEntityMissionPointTowerDefence : ISceneEntityMissionPoint
{
	private TypeTowerDefendsData _towerData;

	private EntityMonsterBeacon _entityBcn;

	private int _entityBcnId = -1;

	private int _idxTarId = -1;

	private int _leftCntToFin;

	private static PlayerMission playerMission => MissionManager.Instance.m_PlayerMission;

	public int MissionId { get; set; }

	public int TargetId { get; set; }

	public SceneEntityMissionPointTowerDefence(int monsterBeaconId = -1)
	{
		_entityBcnId = monsterBeaconId;
	}

	public void Stop()
	{
		if (_entityBcn != null)
		{
			PeSingleton<PeCreature>.Instance.Destory(_entityBcn.Id);
		}
	}

	public bool Start()
	{
		if (SceneEntityCreator.self.PlayerTrans == null)
		{
			return false;
		}
		MissionCommonData missionCommonData = MissionManager.GetMissionCommonData(MissionId);
		for (int i = 0; i < missionCommonData.m_TargetIDList.Count; i++)
		{
			if (missionCommonData.m_TargetIDList[i] == TargetId)
			{
				_idxTarId = i;
				break;
			}
		}
		if (_idxTarId == -1)
		{
			return false;
		}
		_towerData = MissionManager.GetTypeTowerDefendsData(TargetId);
		if (_towerData == null)
		{
			return false;
		}
		playerMission.m_TowerUIData.MaxCount = _towerData.m_Count;
		playerMission.m_TowerUIData.MissionID = MissionId;
		playerMission.m_TowerUIData.CurCount = 0;
		if (_towerData.m_TdInfoId != 0)
		{
			_entityBcn = EntityMonsterBeacon.CreateMonsterBeaconByTDID(_towerData.m_TdInfoId, SceneEntityCreator.self.PlayerTrans, playerMission.m_TowerUIData, _entityBcnId, _towerData, missionCommonData.m_iNpc);
		}
		else
		{
			if (_towerData.m_SweepId.Count <= 0)
			{
				return false;
			}
			_entityBcn = EntityMonsterBeacon.CreateMonsterBeaconBySweepID(_towerData.m_SweepId, SceneEntityCreator.self.PlayerTrans, playerMission.m_TowerUIData, _towerData.m_Time, _entityBcnId, _towerData, missionCommonData.m_iNpc);
		}
		if (_towerData.m_tolTime != 0)
		{
			MissionManager.Instance.SetTowerMissionStartTime(TargetId);
		}
		if (_entityBcn == null)
		{
			return false;
		}
		_entityBcnId = _entityBcn.Id;
		EntityMonsterBeacon entityBcn = _entityBcn;
		entityBcn.handlerNewWave = (Action<AISpawnTDWavesData.TDWaveSpData, int>)Delegate.Combine(entityBcn.handlerNewWave, new Action<AISpawnTDWavesData.TDWaveSpData, int>(OnNewWave));
		EntityMonsterBeacon entityBcn2 = _entityBcn;
		entityBcn2.handlerOneDeath = (Action)Delegate.Combine(entityBcn2.handlerOneDeath, new Action(OnOneDeath));
		PeEntity peEntity = null;
		for (int j = 0; j < _towerData.m_iNpcList.Count; j++)
		{
			peEntity = PeSingleton<EntityMgr>.Instance.Get(_towerData.m_iNpcList[j]);
			if (!(peEntity == null))
			{
				peEntity.SetInvincible(value: false);
			}
		}
		_leftCntToFin = _towerData.m_Count;
		string missionValue = "0_0_0_0_0";
		playerMission.ModifyQuestVariable(MissionId, PlayerMission.MissionFlagTDMonster + _idxTarId, missionValue);
		return true;
	}

	private void OnNewWave(AISpawnTDWavesData.TDWaveSpData spData, int idxWave)
	{
		MissionManager.mTowerCurWave = (idxWave + 1).ToString();
		MissionManager.mTowerTotalWave = spData._waveDatas.Count.ToString();
		MissionManager.Instance.UpdateMissionTrack(MissionId);
	}

	private void OnOneDeath()
	{
		if (!PeGameMgr.IsMulti)
		{
			string questVariable = playerMission.GetQuestVariable(MissionId, PlayerMission.MissionFlagTDMonster + _idxTarId);
			string[] array = questVariable.Split('_');
			if (array.Length < 2)
			{
				Debug.LogError("[TaskTowerDef]:Wrong Quest Var:" + questVariable);
			}
			int num = ((array.Length >= 2) ? Convert.ToInt32(array[1]) : 0);
			num++;
			if (num < _towerData.m_Count)
			{
				questVariable = "0_" + num + "_0_1_0";
				playerMission.ModifyQuestVariable(MissionId, PlayerMission.MissionFlagTDMonster + _idxTarId, questVariable);
			}
			else if (num == _towerData.m_Count)
			{
				questVariable = "0_" + num + "_0_1_1";
				playerMission.ModifyQuestVariable(MissionId, PlayerMission.MissionFlagTDMonster + _idxTarId, questVariable);
				MissionManager.Instance.CompleteTarget(TargetId, MissionId, forceComplete: true);
			}
		}
	}
}

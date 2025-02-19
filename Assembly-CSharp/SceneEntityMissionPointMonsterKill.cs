using System;
using System.Collections.Generic;
using Pathea;
using Pathea.Projectile;
using SkillSystem;
using UnityEngine;

public class SceneEntityMissionPointMonsterKill : ISceneEntityMissionPoint
{
	private int _idxTarId = -1;

	private TypeMonsterData _data;

	private List<ISceneObjAgent> _agents = new List<ISceneObjAgent>();

	private static PlayerMission playerMission => MissionManager.Instance.m_PlayerMission;

	public int MissionId { get; set; }

	public int TargetId { get; set; }

	public bool GenMonsterInMission { get; set; }

	public bool Start()
	{
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
		_data = MissionManager.GetTypeMonsterData(TargetId);
		if (_data == null)
		{
			return false;
		}
		Vector3 pos;
		switch (_data.m_mr.refertoType)
		{
		case ReferToType.None:
			pos = PeSingleton<PeCreature>.Instance.mainPlayer.position;
			break;
		case ReferToType.Player:
			pos = PeSingleton<PeCreature>.Instance.mainPlayer.position;
			break;
		case ReferToType.Town:
			VArtifactUtil.GetTownPos(_data.m_mr.referToID, out pos);
			break;
		case ReferToType.Npc:
			pos = PeSingleton<EntityMgr>.Instance.Get(MissionManager.Instance.m_PlayerMission.adId_entityId[_data.m_mr.referToID]).position;
			break;
		default:
			pos = PeSingleton<PeCreature>.Instance.mainPlayer.position;
			break;
		}
		if (pos == Vector3.zero)
		{
			return false;
		}
		if (PeGameMgr.IsSingle || PeGameMgr.IsTutorial)
		{
			if (_data.type == 2)
			{
				DoodadEntityCreator.commonDeathEvent += OnMonsterDeath;
			}
			else
			{
				MonsterEntityCreator.commonDeathEvent += OnMonsterDeath;
			}
		}
		GenMonsterInMission = !PeGameMgr.IsStory;
		if (GenMonsterInMission)
		{
			Vector2 vector = UnityEngine.Random.insideUnitCircle.normalized * _data.m_mr.radius1;
			Vector3 center = pos + new Vector3(vector.x, 0f, vector.y);
			for (int j = 0; j < _data.m_CreMonList.Count; j++)
			{
				for (int k = 0; k < _data.m_CreMonList[j].monNum; k++)
				{
					Vector3 randomPosition = AiUtil.GetRandomPosition(center, 0f, _data.m_mr.radius2);
					randomPosition.y = 0f;
					int num = _data.m_CreMonList[j].monID;
					if (_data.m_CreMonList[j].type == 1)
					{
						num |= 0x40000000;
					}
					SceneEntityPosAgent sceneEntityPosAgent = MonsterEntityCreator.CreateAgent(randomPosition, num);
					sceneEntityPosAgent.spInfo = new MonsterEntityCreator.AgentInfo(EntityMonsterBeacon.GetSpBeacon4MonsterKillTask());
					sceneEntityPosAgent.canRide = false;
					_agents.Add(sceneEntityPosAgent);
					SceneMan.AddSceneObj(sceneEntityPosAgent);
				}
			}
		}
		return true;
	}

	public void Stop()
	{
		SceneMan.RemoveSceneObjs(_agents);
		_agents.Clear();
		MonsterEntityCreator.commonDeathEvent -= OnMonsterDeath;
		DoodadEntityCreator.commonDeathEvent -= OnMonsterDeath;
	}

	private void OnMonsterDeath(SkEntity cur, SkEntity caster)
	{
		if (_data.m_mustByPlayer && caster is SkProjectile)
		{
			GameObject gameObject = ((SkProjectile)caster).Caster();
			if (gameObject != null)
			{
				CommonCmpt component = gameObject.GetComponent<CommonCmpt>();
				if (component != null && component.entityProto.proto != 0)
				{
					return;
				}
			}
		}
		SkAliveEntity skAliveEntity = cur as SkAliveEntity;
		if (!(skAliveEntity != null) || _data == null)
		{
			return;
		}
		CommonCmpt commonCmpt = skAliveEntity.Entity.commonCmpt;
		if (commonCmpt == null)
		{
			return;
		}
		int protoId = commonCmpt.entityProto.protoId;
		bool flag = true;
		for (int i = 0; i < _data.m_MonsterList.Count; i++)
		{
			int num = _idxTarId * 10 + i;
			string missionFlag = PlayerMission.MissionFlagMonster + num;
			string questVariable = playerMission.GetQuestVariable(MissionId, missionFlag);
			string[] array = questVariable.Split('_');
			if (array.Length < 2)
			{
				Debug.LogError("[TaskMonsterKill]:Wrong Quest Var:" + questVariable);
			}
			int num2 = ((array.Length >= 2) ? Convert.ToInt32(array[1]) : 0);
			if (_data.m_MonsterList[i].npcs.Contains(protoId))
			{
				num2++;
				questVariable = array[0] + "_" + num2;
				playerMission.ModifyQuestVariable(MissionId, missionFlag, questVariable);
			}
			if (num2 < _data.m_MonsterList[i].type)
			{
				flag = false;
			}
		}
		if (flag)
		{
			MissionManager.Instance.CompleteTarget(TargetId, MissionId, forceComplete: true);
		}
	}
}

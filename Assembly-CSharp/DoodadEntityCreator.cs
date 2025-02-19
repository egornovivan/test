using System;
using System.Collections.Generic;
using Pathea;
using SkillSystem;
using UnityEngine;

public class DoodadEntityCreator
{
	public class AgentInfo : SceneEntityPosAgent.AgentInfo
	{
		public new static AgentInfo s_defAgentInfo = new AgentInfo();

		public int _id;

		public bool _isShown;

		public int _campId;

		public int _damageId;

		public int _playerId;

		public AgentInfo(int id = -1, bool isShown = true, int campId = 28, int damageId = 28, int playerId = -1)
		{
			_id = id;
			_isShown = isShown;
			_campId = campId;
			_damageId = damageId;
			_playerId = playerId;
		}
	}

	public static event Action<SkEntity, SkEntity> commonDeathEvent;

	public static void Init()
	{
		DoodadEntityCreator.commonDeathEvent = null;
		DoodadEntityCreator.commonDeathEvent = (Action<SkEntity, SkEntity>)Delegate.Combine(DoodadEntityCreator.commonDeathEvent, new Action<SkEntity, SkEntity>(RepProcessor.OnDoodadDeath));
	}

	public static SceneEntityPosAgent CreateAgent(Vector3 pos, int protoId, Vector3 scl, Quaternion rot)
	{
		SceneEntityPosAgent sceneEntityPosAgent = new SceneEntityPosAgent(pos, scl, rot, EntityType.EntityType_Doodad, protoId);
		sceneEntityPosAgent.spInfo = AgentInfo.s_defAgentInfo;
		return sceneEntityPosAgent;
	}

	public static SceneEntityPosAgent CreateAgent(Vector3 pos, int protoId = -1)
	{
		SceneEntityPosAgent sceneEntityPosAgent = new SceneEntityPosAgent(pos, Vector3.one, Quaternion.identity, EntityType.EntityType_Doodad, protoId);
		sceneEntityPosAgent.spInfo = AgentInfo.s_defAgentInfo;
		return sceneEntityPosAgent;
	}

	public static PeEntity CreateDoodad(int protoId, Vector3 pos)
	{
		SceneEntityPosAgent sceneEntityPosAgent = CreateAgent(pos, protoId);
		CreateDoodad(sceneEntityPosAgent);
		return sceneEntityPosAgent.entity;
	}

	public static PeEntity CreateDoodad(int protoId, Vector3 pos, Vector3 scl, Quaternion rot)
	{
		SceneEntityPosAgent sceneEntityPosAgent = CreateAgent(pos, protoId, scl, rot);
		CreateDoodad(sceneEntityPosAgent);
		return sceneEntityPosAgent.entity;
	}

	public static void CreateDoodad(SceneEntityPosAgent agent)
	{
		agent.entity = CreateDoodad(bSerializable: false, agent.spInfo as AgentInfo, agent.protoId, agent.Pos, agent.Scl, agent.Rot, agent.Id);
	}

	public static void CreateStoryDoodads(bool bNew)
	{
		if (PeGameMgr.IsMulti)
		{
			return;
		}
		foreach (KeyValuePair<int, SceneDoodadDesc> s_dicDoodadDatum in StoryDoodadMap.s_dicDoodadData)
		{
			bool flag = s_dicDoodadDatum.Value._type > 0;
			if (bNew || !flag)
			{
				AgentInfo spInfo = new AgentInfo(s_dicDoodadDatum.Value._id, s_dicDoodadDatum.Value._isShown, s_dicDoodadDatum.Value._campId, s_dicDoodadDatum.Value._damageId);
				if (!flag)
				{
					SceneEntityPosAgent sceneEntityPosAgent = CreateAgent(s_dicDoodadDatum.Value._pos, s_dicDoodadDatum.Value._protoId, s_dicDoodadDatum.Value._scl, s_dicDoodadDatum.Value._rot);
					sceneEntityPosAgent.spInfo = spInfo;
					SceneMan.AddSceneObj(sceneEntityPosAgent);
				}
				else
				{
					CreateDoodad(bSerializable: true, spInfo, s_dicDoodadDatum.Value._protoId, s_dicDoodadDatum.Value._pos, s_dicDoodadDatum.Value._scl, s_dicDoodadDatum.Value._rot);
				}
			}
		}
	}

	public static PeEntity CreateRandTerDoodad(int protoId, Vector3 pos, Vector3 scl, Quaternion rot, int townId = -1, int campId = 28, int damageId = 28, int playerId = -1)
	{
		return CreateDoodad(bSerializable: true, new AgentInfo(townId, isShown: true, campId, damageId, playerId), protoId, pos, scl, rot);
	}

	public static PeEntity CreateNetRandTerDoodad(int entityId, int protoId, Vector3 pos, Vector3 scl, Quaternion rot, int townId = -1, int campId = 28, int damageId = 28, int playerId = -1)
	{
		return CreateDoodad(bSerializable: true, new AgentInfo(townId, isShown: true, campId, damageId, playerId), protoId, pos, scl, rot, entityId);
	}

	public static PeEntity CreateStoryDoodadNet(int assetId, int entityId)
	{
		SceneDoodadDesc sceneDoodadDesc = StoryDoodadMap.Get(assetId);
		if (sceneDoodadDesc == null)
		{
			return null;
		}
		AgentInfo spInfo = new AgentInfo(sceneDoodadDesc._id, sceneDoodadDesc._isShown, sceneDoodadDesc._campId, sceneDoodadDesc._damageId);
		return CreateDoodad(bSerializable: true, spInfo, sceneDoodadDesc._protoId, sceneDoodadDesc._pos, sceneDoodadDesc._scl, sceneDoodadDesc._rot, entityId);
	}

	public static PeEntity CreateDoodadNet(int protoId, Vector3 pos, Vector3 scl, Quaternion rot, int id = -1, int campId = 28, int damageId = 28)
	{
		AgentInfo agentInfo = new AgentInfo(-1, isShown: true, campId, damageId);
		PeEntity peEntity = CreateDoodad(bSerializable: true, agentInfo, protoId, pos, scl, rot, id);
		if (null != peEntity)
		{
			agentInfo._damageId = (int)peEntity.GetAttribute(AttribType.DamageID);
		}
		return peEntity;
	}

	private static PeEntity CreateDoodad(bool bSerializable, AgentInfo spInfo, int protoId, Vector3 pos, Vector3 scl, Quaternion rot, int id = -1)
	{
		PeEntity peEntity = null;
		peEntity = ((!PeGameMgr.IsMulti || id == -1) ? ((!bSerializable) ? PeSingleton<PeEntityCreator>.Instance.CreateDoodad(PeSingleton<WorldInfoMgr>.Instance.FetchNonRecordAutoId(), protoId, pos, rot, scl) : PeSingleton<PeCreature>.Instance.CreateDoodad(PeSingleton<WorldInfoMgr>.Instance.FetchRecordAutoId(), protoId, pos, rot, scl)) : PeSingleton<PeEntityCreator>.Instance.CreateDoodad(id, protoId, pos, rot, scl));
		if (null == peEntity)
		{
			Debug.LogError("Failed to create doodad!");
			return null;
		}
		if (spInfo != null)
		{
			SceneDoodadLodCmpt cmpt = peEntity.GetCmpt<SceneDoodadLodCmpt>();
			if (cmpt != null)
			{
				cmpt.Index = spInfo._id;
				cmpt.IsShown = spInfo._isShown;
				cmpt.SetDamagable(spInfo._campId, spInfo._damageId, spInfo._playerId);
			}
		}
		return peEntity;
	}

	public static void OnDoodadDeath(SkEntity a, SkEntity b)
	{
		if (DoodadEntityCreator.commonDeathEvent != null)
		{
			DoodadEntityCreator.commonDeathEvent(a, b);
		}
		if (!PeGameMgr.IsAdventure)
		{
			return;
		}
		SceneDoodadLodCmpt component = a.gameObject.GetComponent<SceneDoodadLodCmpt>();
		if (component != null && component.Index >= 0)
		{
			PeEntity component2 = a.gameObject.GetComponent<PeEntity>();
			if (component2 != null)
			{
				bool isSignalTower = a.GetComponentInChildren<MonsterSummoner>() != null;
				VArtifactTownManager.Instance.OnBuildingDeath(component.Index, component2.Id, isSignalTower);
			}
		}
	}
}

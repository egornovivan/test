using System;
using System.Collections.Generic;
using PeEvent;
using PETools;
using UnityEngine;

namespace Pathea;

public class EntityMgr : MonoLikeSingleton<EntityMgr>
{
	public class RMouseClickEntityEvent : EventArg
	{
		public PeEntity entity;
	}

	public class NPCTalkEvent : EventArg
	{
		public PeEntity entity;
	}

	private Event<RMouseClickEntityEvent> mEventor = new Event<RMouseClickEntityEvent>();

	private Event<NPCTalkEvent> mNPCTalkEventor = new Event<NPCTalkEvent>();

	public Dictionary<int, PeEntity> mDicEntity = new Dictionary<int, PeEntity>(100);

	public List<PeEntity> m_Entities = new List<PeEntity>();

	public List<PeEntity> m_Tmp = new List<PeEntity>();

	private Transform mEntityRoot;

	private Transform mNpcEntityRoot;

	private List<PeEntity> _entitiesWithView = new List<PeEntity>();

	private int _frmUpdateEntitiesWithView = -1;

	public Event<RMouseClickEntityEvent> eventor => mEventor;

	public Event<NPCTalkEvent> npcTalkEventor => mNPCTalkEventor;

	private Transform EntityRoot
	{
		get
		{
			if (null == mEntityRoot)
			{
				GameObject gameObject = new GameObject("EntityRoot");
				mEntityRoot = gameObject.transform;
				GameObject gameObject2 = new GameObject("NpcRoot");
				gameObject2.AddComponent<PeNpcGroup>();
				gameObject2.AddComponent<NpcHatreTargets>();
				gameObject2.transform.parent = mEntityRoot;
				mNpcEntityRoot = gameObject2.transform;
			}
			return mEntityRoot;
		}
	}

	public Transform npcEntityRoot
	{
		get
		{
			if (null == mEntityRoot)
			{
				GameObject gameObject = new GameObject("EntityRoot");
				mEntityRoot = gameObject.transform;
				GameObject gameObject2 = new GameObject("NpcRoot");
				gameObject2.AddComponent<PeNpcGroup>();
				gameObject2.AddComponent<NpcHatreTargets>();
				gameObject2.transform.parent = mEntityRoot;
				mNpcEntityRoot = gameObject2.transform;
			}
			return (!(mNpcEntityRoot != null)) ? null : mNpcEntityRoot;
		}
	}

	public IEnumerable<PeEntity> All => mDicEntity.Values;

	public bool AddAfterAssignId(PeEntity entity, int entityId)
	{
		entity.SetId(entityId);
		return Add(entity);
	}

	private bool MatchInjured(PeEntity entity, Bounds bounds)
	{
		return entity != null && entity.IntersectsExtend(bounds);
	}

	private bool MatchInjured(PeEntity entity, Ray ray)
	{
		return entity != null && entity.IntersectRayExtend(ray);
	}

	private bool MatchInjured(PeEntity entity, Vector3 pos)
	{
		return entity != null && entity.ContainsPointExtend(pos);
	}

	private bool MatchProtoIDs(PeEntity entity, Vector3 pos, float radius, int[] prototIDs)
	{
		if (entity == null)
		{
			return false;
		}
		if (Array.IndexOf(prototIDs, entity.ProtoID) >= 0 && PEUtil.SqrMagnitudeH(entity.position, pos) <= radius * radius)
		{
			return true;
		}
		return false;
	}

	private bool Match(PeEntity entity, Vector3 pos, float radius, int playerID, bool isDeath, PeEntity exclude = null)
	{
		if (entity == null || entity.Equals(exclude) || entity.IsDeath() != isDeath)
		{
			return false;
		}
		int dstPlayerID = (int)entity.GetAttribute(AttribType.DefaultPlayerID);
		if (Singleton<ForceSetting>.Instance.AllyPlayer(playerID, dstPlayerID) && PEUtil.SqrMagnitudeH(entity.position, pos) <= radius * radius)
		{
			return true;
		}
		return false;
	}

	private bool MatchFriendly(PeEntity entity, Vector3 pos, float radius, int playerID, int prototID, bool isDeath, PeEntity exclude = null)
	{
		if (entity == null || entity.Equals(exclude) || entity.IsDeath() != isDeath)
		{
			return false;
		}
		int num = (int)entity.GetAttribute(AttribType.DefaultPlayerID);
		if (playerID == num && (playerID != 4 || prototID == entity.ProtoID) && PEUtil.SqrMagnitudeH(entity.position, pos) <= radius * radius)
		{
			return true;
		}
		return false;
	}

	private bool Match(PeEntity entity, Vector3 pos, float radius)
	{
		if (entity != null)
		{
			return PEUtil.SqrMagnitudeH(entity.position, pos) <= (radius + entity.maxRadius) * (radius + entity.maxRadius);
		}
		return false;
	}

	private bool Match(PeEntity entity, Vector3 pos, float radius, bool isDeath)
	{
		if (entity != null)
		{
			return PEUtil.SqrMagnitudeH(entity.position, pos) <= (radius + entity.maxRadius) * (radius + entity.maxRadius) && entity.IsDeath() == isDeath;
		}
		return false;
	}

	public List<PeEntity> GetEntities(Vector3 pos, float radius)
	{
		m_Tmp.Clear();
		int count = m_Entities.Count;
		for (int i = 0; i < count; i++)
		{
			PeEntity peEntity = m_Entities[i];
			float num = radius + peEntity.maxRadius;
			if (peEntity != null && PEUtil.SqrMagnitudeH(peEntity.position, pos) <= num * num)
			{
				m_Tmp.Add(m_Entities[i]);
			}
		}
		return m_Tmp;
	}

	public List<PeEntity> GetEntities(Vector3 pos, float radius, bool isDeath)
	{
		m_Tmp.Clear();
		int count = m_Entities.Count;
		for (int i = 0; i < count; i++)
		{
			PeEntity peEntity = m_Entities[i];
			float num = radius + peEntity.maxRadius;
			if (peEntity != null && peEntity.IsDeath() == isDeath && PEUtil.SqrMagnitudeH(peEntity.position, pos) <= num * num)
			{
				m_Tmp.Add(m_Entities[i]);
			}
		}
		return m_Tmp;
	}

	public List<PeEntity> GetTowerEntities(Vector3 pos, float radius, bool isDeath)
	{
		m_Tmp.Clear();
		int count = m_Entities.Count;
		for (int i = 0; i < count; i++)
		{
			PeEntity peEntity = m_Entities[i];
			float num = radius + peEntity.maxRadius;
			if (peEntity != null && peEntity.Tower != null && peEntity.IsDeath() == isDeath && PEUtil.SqrMagnitudeH(peEntity.position, pos) <= num * num)
			{
				m_Tmp.Add(m_Entities[i]);
			}
		}
		return m_Tmp;
	}

	public List<PeEntity> GetEntities(Vector3 pos, float radius, int playerID, bool isDeath, PeEntity exclude = null)
	{
		m_Tmp.Clear();
		int count = m_Entities.Count;
		for (int i = 0; i < count; i++)
		{
			PeEntity peEntity = m_Entities[i];
			if (!(peEntity == null) && !peEntity.Equals(exclude) && peEntity.IsDeath() == isDeath)
			{
				int num = (int)peEntity.GetAttribute(AttribType.DefaultPlayerID);
				if ((PEUtil.SqrMagnitudeH(peEntity.position, pos) <= radius * radius && Singleton<ForceSetting>.Instance.AllyPlayer(playerID, num)) || PEUtil.CanCordialReputation(playerID, num))
				{
					m_Tmp.Add(m_Entities[i]);
				}
			}
		}
		return m_Tmp;
	}

	public bool NearEntityModel(Vector3 pos, float radius, int playerID, bool isDeath, PeEntity exclude = null)
	{
		int count = m_Entities.Count;
		float num = radius * radius;
		for (int i = 0; i < count; i++)
		{
			PeEntity peEntity = m_Entities[i];
			if (!(peEntity == null) && !peEntity.Equals(exclude) && peEntity.hasView && peEntity.IsDeath() == isDeath && (NpcRobotDb.Instance == null || peEntity.Id != NpcRobotDb.Instance.mID))
			{
				int dstPlayerID = (int)peEntity.GetAttribute(AttribType.DefaultPlayerID);
				if (PEUtil.SqrMagnitudeH(peEntity.position, pos) <= num && Singleton<ForceSetting>.Instance.AllyPlayer(playerID, dstPlayerID))
				{
					return true;
				}
			}
		}
		return false;
	}

	public List<PeEntity> GetEntitiesFriendly(Vector3 pos, float radius, int playerID, int protoID, bool isDeath, PeEntity exclude = null)
	{
		m_Tmp.Clear();
		int count = m_Entities.Count;
		for (int i = 0; i < count; i++)
		{
			PeEntity peEntity = m_Entities[i];
			if (!(peEntity == null) && !peEntity.Equals(exclude) && peEntity.IsDeath() == isDeath)
			{
				int num = (int)peEntity.GetAttribute(AttribType.DefaultPlayerID);
				if (playerID == num && (playerID != 4 || protoID == peEntity.ProtoID) && PEUtil.SqrMagnitudeH(peEntity.position, pos) <= radius * radius)
				{
					m_Tmp.Add(m_Entities[i]);
				}
			}
		}
		return m_Tmp;
	}

	public PeEntity[] GetEntitiesByProtoIDs(Vector3 pos, float radius, int[] protoIDs)
	{
		m_Tmp.Clear();
		int count = m_Entities.Count;
		for (int i = 0; i < count; i++)
		{
			if (MatchProtoIDs(m_Entities[i], pos, radius, protoIDs))
			{
				m_Tmp.Add(m_Entities[i]);
			}
		}
		return m_Tmp.ToArray();
	}

	public List<PeEntity> GetEntitiesInjured(Vector3 pos)
	{
		m_Tmp.Clear();
		int count = m_Entities.Count;
		for (int i = 0; i < count; i++)
		{
			if (m_Entities[i] != null && m_Entities[i].ContainsPointExtend(pos))
			{
				m_Tmp.Add(m_Entities[i]);
			}
		}
		return m_Tmp;
	}

	public PeEntity[] GetEntitiesInjured(Ray ray)
	{
		m_Tmp.Clear();
		int count = m_Entities.Count;
		for (int i = 0; i < count; i++)
		{
			if (MatchInjured(m_Entities[i], ray))
			{
				m_Tmp.Add(m_Entities[i]);
			}
		}
		return m_Tmp.ToArray();
	}

	public List<PeEntity> GetEntitiesInjured(Bounds bounds)
	{
		m_Tmp.Clear();
		int count = m_Entities.Count;
		for (int i = 0; i < count; i++)
		{
			if (m_Entities[i] != null && m_Entities[i].IntersectsExtend(bounds))
			{
				m_Tmp.Add(m_Entities[i]);
			}
		}
		return m_Tmp;
	}

	private bool MatchHatred(PeEntity entity, Vector3 pos, float radius, int monsterProtoID)
	{
		if (entity != null && entity.entityProto != null && Vector3.Distance(pos, entity.position) <= radius && entity.proto == EEntityProto.Monster && entity.entityProto.protoId == monsterProtoID)
		{
			return true;
		}
		return false;
	}

	public PeEntity[] GetHatredEntities(Vector3 pos, float radius, int monsterProtoID)
	{
		m_Tmp.Clear();
		for (int i = 0; i < m_Entities.Count; i++)
		{
			if (MatchHatred(m_Entities[i], pos, radius, monsterProtoID))
			{
				m_Tmp.Add(m_Entities[i]);
			}
		}
		return m_Tmp.ToArray();
	}

	private bool MatchStoryAssetId(PeEntity entity, int storyAssetId)
	{
		if (entity != null)
		{
			if (entity.proto != EEntityProto.Doodad)
			{
				return false;
			}
			if (entity.GetComponent<SceneDoodadLodCmpt>().Index == storyAssetId)
			{
				return true;
			}
		}
		return false;
	}

	public PeEntity[] GetDoodadEntities(int storyAssetId)
	{
		m_Tmp.Clear();
		for (int i = 0; i < m_Entities.Count; i++)
		{
			if (MatchStoryAssetId(m_Entities[i], storyAssetId))
			{
				m_Tmp.Add(m_Entities[i]);
			}
		}
		return m_Tmp.ToArray();
	}

	public PeEntity[] GetDoodadEntitiesByProtoId(int prototypeID)
	{
		List<PeEntity> list = new List<PeEntity>(mDicEntity.Values);
		return list.FindAll(delegate(PeEntity e)
		{
			if (e != null)
			{
				if (e.proto != EEntityProto.Doodad)
				{
					return false;
				}
				if (!StoryDoodadMap.s_dicDoodadData.ContainsKey(e.GetComponent<SceneDoodadLodCmpt>().Index))
				{
					return false;
				}
				if (StoryDoodadMap.s_dicDoodadData[e.GetComponent<SceneDoodadLodCmpt>().Index]._protoId == prototypeID)
				{
					return true;
				}
			}
			return false;
		}).ToArray();
	}

	public List<PeEntity> GetEntitiesWithView()
	{
		if (Time.frameCount != _frmUpdateEntitiesWithView)
		{
			_entitiesWithView.Clear();
			for (int i = 0; i < m_Entities.Count; i++)
			{
				if (m_Entities[i].hasView)
				{
					_entitiesWithView.Add(m_Entities[i]);
				}
			}
			_frmUpdateEntitiesWithView = Time.frameCount;
		}
		return _entitiesWithView;
	}

	public PeEntity InitEntity(int entityId, GameObject obj)
	{
		if (null != PeSingleton<EntityMgr>.Instance.Get(entityId))
		{
			Debug.LogError("existed entity with id:" + entityId);
			return null;
		}
		PeEntity peEntity = obj.GetComponent<PeEntity>();
		if (peEntity == null)
		{
			peEntity = obj.AddComponent<PeEntity>();
		}
		AddAfterAssignId(peEntity, entityId);
		return peEntity;
	}

	public PeEntity Create(int entityId, string path, Vector3 pos, Quaternion rot, Vector3 scl, bool isnpc = false)
	{
		if (entityId == -1)
		{
			Debug.LogError("[CreateEntity]Failed to create entity : Invalid entity id " + entityId);
			return null;
		}
		if (null != PeSingleton<EntityMgr>.Instance.Get(entityId))
		{
			Debug.LogError("[CreateEntity]Failed to create entity : Existed entity with id:" + entityId);
			return null;
		}
		PeEntity peEntity = PeEntity.Create(path, pos, rot, scl);
		if (null == peEntity)
		{
			Debug.LogError("[CreateEntity]Failed to create entity!");
			return null;
		}
		bool flag = peEntity.GetComponent<MainPlayerCmpt>() != null;
		Transform parent = ((!(peEntity.NpcCmpt == null) && !flag) ? npcEntityRoot : EntityRoot);
		peEntity.transform.parent = parent;
		InitEntity(entityId, peEntity.gameObject);
		return peEntity;
	}

	public bool Destroy(int entityId)
	{
		if (entityId == -1)
		{
			Debug.Log("<color=green>Invalid entity id:" + entityId + "</color>");
			return false;
		}
		PeEntity peEntity = PeSingleton<EntityMgr>.Instance.Get(entityId);
		if (null == peEntity)
		{
			return false;
		}
		Remove(entityId);
		return PeEntity.Destroy(peEntity);
	}

	public bool Add(PeEntity entity)
	{
		if (mDicEntity.ContainsKey(entity.Id))
		{
			Debug.LogError("exist entity id: " + entity.Id);
			return false;
		}
		mDicEntity.Add(entity.Id, entity);
		m_Entities.Add(entity);
		return true;
	}

	public bool Remove(int id)
	{
		if (mDicEntity.ContainsKey(id))
		{
			m_Entities.Remove(mDicEntity[id]);
			mDicEntity.Remove(id);
			return true;
		}
		return false;
	}

	public PeEntity Get(int entityId)
	{
		if (mDicEntity.ContainsKey(entityId))
		{
			return mDicEntity[entityId];
		}
		return null;
	}

	public PeEntity Get(string entityName)
	{
		string value = entityName.ToLower();
		foreach (PeEntity value2 in mDicEntity.Values)
		{
			if (null != value2 && value2.ToString().ToLower().Contains(value))
			{
				return value2;
			}
		}
		return null;
	}

	public PeEntity GetByScenarioId(int scenarioId)
	{
		foreach (KeyValuePair<int, PeEntity> item in mDicEntity)
		{
			if (null != item.Value && item.Value.scenarioId == scenarioId)
			{
				return item.Value;
			}
		}
		return null;
	}

	public void Clear()
	{
		m_Entities.Clear();
		mDicEntity.Clear();
	}
}

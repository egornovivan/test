using System;
using System.Collections;
using System.Collections.Generic;
using Pathea;
using PETools;
using UnityEngine;

public class MonsterSiege : MonoBehaviour
{
	private bool m_SpawnFinished;

	private CSMgCreator m_Creator;

	private EntityMonsterBeacon m_Beacon;

	private TowerInfoUIData m_UIData;

	private List<SiegeAgent> m_Agents;

	private List<PeEntity> m_Npcs;

	private List<PeEntity> m_Towers;

	private List<PeEntity> m_Buildings;

	private List<PeEntity> m_Defences;

	private List<PeEntity> m_Entities;

	private bool m_IsReady;

	private int m_KillCount;

	private int m_MaxCount;

	private float m_ElapsedTime;

	private float m_RemainTime = -1f;

	public float assemblyRadius => m_Creator.Assembly.Radius;

	public Vector3 assemblyPosition => m_Creator.Assembly.Position;

	public void SetCreator(CSMgCreator creator, TowerInfoUIData uiData)
	{
		m_Creator = creator;
		m_UIData = uiData;
	}

	public PeEntity GetClosestEntity(SceneEntityPosAgent agent)
	{
		PeEntity result = null;
		float num = float.PositiveInfinity;
		for (int i = 0; i < m_Entities.Count; i++)
		{
			if (m_Entities[i] != null && !m_Entities[i].IsDeath())
			{
				float num2 = PEUtil.Magnitude(m_Entities[i].position, agent.Pos);
				if (num2 < num)
				{
					num = num2;
					result = m_Entities[i];
				}
			}
		}
		return result;
	}

	public PeEntity GetRandomEntity()
	{
		if (m_Entities.Count > 0)
		{
			return m_Entities[UnityEngine.Random.Range(0, m_Entities.Count)];
		}
		return null;
	}

	public PeEntity GetRandomEntityView()
	{
		if (m_Entities.Count > 0)
		{
			return m_Entities[UnityEngine.Random.Range(0, m_Entities.Count)];
		}
		return null;
	}

	private void OnEntitySpawned(SceneEntityPosAgent agent)
	{
		m_MaxCount++;
		if (EntityMonsterBeacon.IsBcnMonsterProtoId(agent.protoId))
		{
			EntityMonsterBeacon.DecodeBcnMonsterProtoId(agent.protoId, out var spType, out var dif, out var spawnType);
			int areaType = ((!PeGameMgr.IsStory) ? AiUtil.GetMapID(agent.Pos) : PeSingleton<PeMappingMgr>.Instance.GetAiSpawnMapId(new Vector2(agent.Pos.x, agent.Pos.z)));
			AISpawnTDWavesData.TDMonsterSpData monsterSpData = AISpawnTDWavesData.GetMonsterSpData(bRandTer: false, spType, dif, spawnType, areaType);
			float hp = ((monsterSpData == null) ? 200f : ((float)monsterSpData._rhp));
			float atk = ((monsterSpData == null) ? 50f : ((float)monsterSpData._dps));
			SiegeAgent siegeAgent = new SiegeAgent(this, agent, hp, atk);
			agent.spInfo = new SiegeAgent.AgentInfo(m_Beacon, siegeAgent);
			m_Agents.Add(siegeAgent);
		}
	}

	private void OnNewWave(AISpawnTDWavesData.TDWaveSpData spData, int idxWave)
	{
		m_IsReady = false;
		m_SpawnFinished = idxWave >= spData._waveDatas.Count - 1;
		m_Npcs.Clear();
		m_Towers.Clear();
		m_Buildings.Clear();
		m_Defences.Clear();
		m_Entities.Clear();
		List<PeEntity> cSNpcs = CSMain.GetCSNpcs(m_Creator);
		for (int i = 0; i < cSNpcs.Count; i++)
		{
			if (cSNpcs[i] != null && PEUtil.SqrMagnitude(cSNpcs[i].position, assemblyPosition) < assemblyRadius * assemblyRadius)
			{
				m_Npcs.Add(cSNpcs[i]);
			}
		}
		m_Towers = PeSingleton<EntityMgr>.Instance.GetTowerEntities(assemblyPosition, assemblyRadius, isDeath: false);
		m_Buildings = CSMain.GetCSBuildings(m_Creator);
		m_Defences.AddRange(m_Npcs);
		m_Defences.AddRange(m_Towers);
		m_Entities.AddRange(m_Defences);
		m_Entities.AddRange(m_Buildings);
	}

	private IEnumerator Defence()
	{
		while (true)
		{
			if (m_Beacon != null)
			{
				List<SiegeAgent> agents = m_Agents.FindAll((SiegeAgent ret) => !ret.hasView && !ret.death);
				for (int i = 0; i < m_Defences.Count; i++)
				{
					if (!m_Defences[i].IsDeath() && !m_Defences[i].hasView)
					{
						List<SiegeAgent> tmpAgents = agents.FindAll((SiegeAgent ret) => PEUtil.SqrMagnitude(ret.position, assemblyPosition, is3D: false) <= assemblyRadius * assemblyRadius);
						if (tmpAgents.Count > 0)
						{
							float atk = m_Defences[i].GetAttribute(AttribType.Atk);
							tmpAgents[UnityEngine.Random.Range(0, tmpAgents.Count)].ApplyDamage(atk);
						}
					}
				}
			}
			yield return new WaitForSeconds(UnityEngine.Random.Range(5f, 10f));
		}
	}

	private IEnumerator WarningOne(int count, float intervals, float delayTime)
	{
		for (int i = 0; i < count; i++)
		{
			string content = (m_IsReady ? PELocalization.GetString(8000190) : PELocalization.GetString(8000189));
			PeTipMsg.Register(content, PeTipMsg.EMsgLevel.Warning, PeTipMsg.EMsgType.Colony, 800);
			yield return new WaitForSeconds(intervals);
		}
		yield return new WaitForSeconds(delayTime);
	}

	private IEnumerator Warning()
	{
		yield return StartCoroutine(WarningOne(5, 12f, 60f));
		while (true)
		{
			if (m_Creator != null && m_Creator.Assembly != null && PeSingleton<PeCreature>.Instance.mainPlayer != null)
			{
				Vector3 v1 = m_Creator.Assembly.Position;
				Vector3 v2 = PeSingleton<PeCreature>.Instance.mainPlayer.position;
				float radius = m_Creator.Assembly.Radius;
				if (PEUtil.SqrMagnitude(v1, v2, is3D: false) < radius * radius)
				{
					yield return StartCoroutine(WarningOne(5, 12f, 60f));
				}
			}
			yield return new WaitForSeconds(5f);
		}
	}

	private void OnDeath(SiegeAgent agent)
	{
		m_Agents.Remove(agent);
		if (m_SpawnFinished && m_Agents.Count <= 0)
		{
			UnityEngine.Object.Destroy(base.gameObject);
		}
		m_KillCount++;
	}

	private void Awake()
	{
		m_IsReady = true;
		m_Beacon = GetComponent<EntityMonsterBeacon>();
		EntityMonsterBeacon beacon = m_Beacon;
		beacon.handlerNewWave = (Action<AISpawnTDWavesData.TDWaveSpData, int>)Delegate.Combine(beacon.handlerNewWave, new Action<AISpawnTDWavesData.TDWaveSpData, int>(OnNewWave));
		EntityMonsterBeacon beacon2 = m_Beacon;
		beacon2.handerNewEntity = (Action<SceneEntityPosAgent>)Delegate.Combine(beacon2.handerNewEntity, new Action<SceneEntityPosAgent>(OnEntitySpawned));
		if (m_Creator != null)
		{
			m_Creator.SetSiege(value: true);
		}
		m_Agents = new List<SiegeAgent>();
		m_Npcs = new List<PeEntity>();
		m_Towers = new List<PeEntity>();
		m_Buildings = new List<PeEntity>();
		m_Defences = new List<PeEntity>();
		m_Entities = new List<PeEntity>();
		SiegeAgent.DeathEvent = (Action<SiegeAgent>)Delegate.Combine(SiegeAgent.DeathEvent, new Action<SiegeAgent>(OnDeath));
		StartCoroutine(Defence());
		StartCoroutine(Warning());
	}

	private void Update()
	{
		m_Entities = m_Entities.FindAll((PeEntity ret) => ret != null && !ret.IsDeath());
		if (!m_IsReady)
		{
			m_ElapsedTime += Time.deltaTime;
		}
		if (m_Beacon != null && (float)m_Beacon.SpData._timeToDelete > float.Epsilon)
		{
			m_RemainTime = (float)m_Beacon.SpData._timeToDelete - m_ElapsedTime;
			if (m_RemainTime <= float.Epsilon)
			{
				m_Beacon.Delete();
			}
		}
		if (m_UIData != null)
		{
			m_UIData.CurCount = m_KillCount;
			m_UIData.MaxCount = m_MaxCount;
			if (m_RemainTime > -1E-45f)
			{
				m_UIData.RemainTime = m_RemainTime;
			}
		}
	}

	private void OnDestroy()
	{
		for (int i = 0; i < m_Agents.Count; i++)
		{
			m_Agents[i].Clear();
		}
		if (m_Beacon != null)
		{
			EntityMonsterBeacon beacon = m_Beacon;
			beacon.handlerNewWave = (Action<AISpawnTDWavesData.TDWaveSpData, int>)Delegate.Remove(beacon.handlerNewWave, new Action<AISpawnTDWavesData.TDWaveSpData, int>(OnNewWave));
			EntityMonsterBeacon beacon2 = m_Beacon;
			beacon2.handerNewEntity = (Action<SceneEntityPosAgent>)Delegate.Remove(beacon2.handerNewEntity, new Action<SceneEntityPosAgent>(OnEntitySpawned));
		}
		if (m_Creator != null)
		{
			m_Creator.SetSiege(value: false);
		}
		m_Agents.Clear();
		m_Npcs.Clear();
		m_Towers.Clear();
		m_Buildings.Clear();
		m_Defences.Clear();
		m_Entities.Clear();
		SiegeAgent.DeathEvent = (Action<SiegeAgent>)Delegate.Remove(SiegeAgent.DeathEvent, new Action<SiegeAgent>(OnDeath));
	}
}

using System;
using Pathea;
using UnityEngine;

public class MonsterSiege_Town : MonoBehaviour
{
	public static MonsterSiege_Town Instance;

	public float minHour;

	public float maxHour;

	public float perCheckTime;

	public float probability;

	private int m_CurSiegeID;

	private EntityMonsterBeacon m_MonsterSiege;

	private VArtifactTown m_Town;

	private TowerInfoUIData m_UIData;

	private bool m_SpawnFinished;

	private bool m_IsReady;

	private float m_ElapsedTime;

	private VArtifactTown m_RecordTown;

	public void OnNewTown(VArtifactTown town)
	{
		if (town != null && town.ms_id > 0)
		{
			m_RecordTown = town;
		}
	}

	private void SetSiegeID(int id)
	{
		if (m_CurSiegeID == 0 && m_MonsterSiege == null)
		{
			m_Town.SetMsId(id);
			m_CurSiegeID = id;
			m_IsReady = false;
			m_SpawnFinished = false;
			m_ElapsedTime = 0f;
			m_UIData = new TowerInfoUIData();
			m_MonsterSiege = EntityMonsterBeacon.CreateMonsterBeaconByTDID(m_CurSiegeID, null, m_UIData);
			m_MonsterSiege.transform.position = m_Town.TransPos;
			m_MonsterSiege.TargetPosition = m_Town.TransPos;
			EntityMonsterBeacon monsterSiege = m_MonsterSiege;
			monsterSiege.handlerOneDeath = (Action)Delegate.Combine(monsterSiege.handlerOneDeath, new Action(OnMemberDeath));
			EntityMonsterBeacon monsterSiege2 = m_MonsterSiege;
			monsterSiege2.handerNewEntity = (Action<SceneEntityPosAgent>)Delegate.Combine(monsterSiege2.handerNewEntity, new Action<SceneEntityPosAgent>(OnMemberCreated));
			EntityMonsterBeacon monsterSiege3 = m_MonsterSiege;
			monsterSiege3.handlerNewWave = (Action<AISpawnTDWavesData.TDWaveSpData, int>)Delegate.Combine(monsterSiege3.handlerNewWave, new Action<AISpawnTDWavesData.TDWaveSpData, int>(OnNewWave));
		}
	}

	private int GetLevel(VArtifactTown town)
	{
		return Mathf.Clamp(town.level, 1, 5);
	}

	private void OnMemberDeath()
	{
	}

	private void OnMemberCreated(SceneEntityPosAgent agent)
	{
		m_UIData.MaxCount++;
	}

	private void OnNewWave(AISpawnTDWavesData.TDWaveSpData data, int idxWave)
	{
		m_IsReady = true;
		m_SpawnFinished = idxWave >= data._waveDatas.Count - 1;
	}

	private void Awake()
	{
		Instance = this;
	}

	private void Update()
	{
		if (PeSingleton<PeCreature>.Instance.mainPlayer == null || UITowerInfo.Instance == null)
		{
			return;
		}
		if (m_RecordTown != null)
		{
			m_Town = m_RecordTown;
			SetSiegeID(m_RecordTown.ms_id);
			m_RecordTown = null;
		}
		if (m_MonsterSiege != null && m_SpawnFinished && m_UIData.CurCount == m_UIData.MaxCount)
		{
			UnityEngine.Object.DestroyImmediate(m_MonsterSiege.gameObject);
		}
		if (m_CurSiegeID > 0 && m_MonsterSiege == null)
		{
			m_CurSiegeID = 0;
			m_SpawnFinished = false;
			if (m_Town != null)
			{
				m_Town.SetMsId(0);
			}
		}
		if (m_IsReady)
		{
			m_ElapsedTime += Time.deltaTime;
		}
		if (m_MonsterSiege != null && (float)m_MonsterSiege.SpData._timeToDelete > float.Epsilon)
		{
			float num = Mathf.Max(0f, (float)m_MonsterSiege.SpData._timeToDelete - m_ElapsedTime);
			if (m_UIData != null)
			{
				m_UIData.RemainTime = num;
			}
			if (num <= float.Epsilon)
			{
				m_MonsterSiege.Delete();
			}
		}
		if (EntityMonsterBeacon.IsRunning() || m_CurSiegeID != 0 || !(m_MonsterSiege == null))
		{
			return;
		}
		m_Town = VArtifactTown.GetStandTown(PeSingleton<PeCreature>.Instance.mainPlayer.position);
		if (m_Town == null)
		{
			return;
		}
		if (m_Town.lastHour < 1.401298464324817E-45 || m_Town.nextHour < 1.401298464324817E-45)
		{
			m_Town.RandomSiege(minHour, maxHour);
		}
		if (GameTime.Timer.Hour - m_Town.lastHour >= m_Town.nextHour && Time.time - m_Town.lastCheckTime >= perCheckTime)
		{
			if (UnityEngine.Random.value < probability)
			{
				SetSiegeID(GetLevel(m_Town));
				m_Town.RandomSiege(minHour, maxHour);
			}
			m_Town.lastCheckTime = Time.time;
		}
	}
}

using System;
using Pathea;
using UnityEngine;

public class MonsterSiege_Camp : MonoBehaviour
{
	public float minHour;

	public float maxHour;

	public float minMinute;

	public float maxMinute;

	public float probability;

	public float maxFailedCount;

	private float m_CurHour;

	private float m_LastHour;

	private float m_RandTime;

	private float m_LastRandomTime;

	private int m_FailedCout;

	private EntityMonsterBeacon m_Beacon;

	private void TryCreateMonsterSiege()
	{
		Camp camp = Camp.GetCamp(PeSingleton<PeCreature>.Instance.mainPlayer.position);
		if (m_Beacon == null && camp != null && Time.time - m_LastRandomTime > m_RandTime * 60f)
		{
			m_LastRandomTime = Time.time;
			m_RandTime = UnityEngine.Random.Range(minMinute, maxMinute);
			if (UnityEngine.Random.value > probability)
			{
				m_FailedCout++;
				return;
			}
			m_Beacon = EntityMonsterBeacon.CreateMonsterBeaconByTDID(1, null, null);
			m_Beacon.TargetPosition = camp.Pos;
			m_Beacon.transform.position = camp.Pos;
			EntityMonsterBeacon beacon = m_Beacon;
			beacon.handlerNewWave = (Action<AISpawnTDWavesData.TDWaveSpData, int>)Delegate.Combine(beacon.handlerNewWave, new Action<AISpawnTDWavesData.TDWaveSpData, int>(OnNewWave));
			m_FailedCout = 0;
		}
	}

	private void OnNewWave(AISpawnTDWavesData.TDWaveSpData spData, int idxWave)
	{
		if (idxWave == spData._waveDatas.Count - 1 && m_Beacon != null)
		{
			EntityMonsterBeacon beacon = m_Beacon;
			beacon.handlerNewWave = (Action<AISpawnTDWavesData.TDWaveSpData, int>)Delegate.Remove(beacon.handlerNewWave, new Action<AISpawnTDWavesData.TDWaveSpData, int>(OnNewWave));
			UnityEngine.Object.Destroy(m_Beacon.gameObject);
		}
	}

	private void Awake()
	{
		m_LastHour = (float)GameTime.Timer.Hour;
		m_CurHour = UnityEngine.Random.Range(minHour, maxHour);
		m_LastRandomTime = Time.time;
		m_RandTime = UnityEngine.Random.Range(minMinute, maxMinute);
	}

	private void Update()
	{
		if (!(PeSingleton<PeCreature>.Instance.mainPlayer == null) && !(GameTime.Timer.Hour - (double)m_LastHour < (double)m_CurHour))
		{
			if (m_Beacon != null || (float)m_FailedCout >= maxFailedCount)
			{
				m_LastHour = (float)GameTime.Timer.Hour;
				m_CurHour = UnityEngine.Random.Range(minHour, maxHour);
			}
			else
			{
				TryCreateMonsterSiege();
			}
		}
	}
}

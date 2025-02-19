using System;
using Pathea;
using UnityEngine;

public class ItemScript_MonsterBeacon : ItemScript
{
	[SerializeField]
	private int m_monsterBeaconId;

	private EntityMonsterBeacon _entityBcn;

	private DragItemAgent _agent;

	public int MBId => m_monsterBeaconId;

	private void DestroySelf()
	{
		DragItemAgent.Destory(_agent);
	}

	private void OnNewWave(AISpawnTDWavesData.TDWaveSpData tdData, int wave)
	{
		if (wave == tdData._waveDatas.Count - 1)
		{
			Invoke("DestroySelf", 10f);
		}
	}

	public override void OnActivate()
	{
		base.OnActivate();
		if (_entityBcn != null)
		{
			Debug.LogError("[MonsterBeaconItem]:MonsterBeacon has existed.");
			return;
		}
		int entityId = ((!GameConfig.IsMultiMode) ? (-1) : id);
		_entityBcn = EntityMonsterBeacon.CreateMonsterBeaconByTDID(m_monsterBeaconId, base.transform, new TowerInfoUIData(), entityId, null, -1, bOnlyMonster: true);
		if (_entityBcn != null)
		{
			EntityMonsterBeacon entityBcn = _entityBcn;
			entityBcn.handlerNewWave = (Action<AISpawnTDWavesData.TDWaveSpData, int>)Delegate.Combine(entityBcn.handlerNewWave, new Action<AISpawnTDWavesData.TDWaveSpData, int>(OnNewWave));
			_agent = DragItemAgent.GetById(id);
			if (_agent != null)
			{
				SceneMan.RemoveSceneObj(_agent);
			}
		}
	}

	private void OnDestroy()
	{
		if (!GameConfig.IsMultiMode && _entityBcn != null)
		{
			PeSingleton<PeCreature>.Instance.Destory(_entityBcn.Id);
		}
	}
}

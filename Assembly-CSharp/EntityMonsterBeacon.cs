using System;
using System.Collections;
using System.Collections.Generic;
using Pathea;
using PeMap;
using UnityEngine;

public class EntityMonsterBeacon : PeEntity
{
	public const int TowerDefenseSpType_Beg = 500;

	private const int MonsterBeaconPlayerID = 8;

	private const int MonsterBeaconCampID = 26;

	private const int MonsterKillBeaconID = -2;

	private const float TimeStep = 1f;

	protected int _idxWave;

	protected float _preTime;

	protected List<ISceneObjAgent> _agents = new List<ISceneObjAgent>();

	protected int _campCol = -1;

	protected int _spDataId;

	protected Vector3 _position;

	protected Vector3 _forward;

	protected TowerInfoUIData _uiData;

	protected AISpawnTDWavesData.TDWaveSpData _spData;

	public Action handlerOneDeath;

	public Action<SceneEntityPosAgent> handerNewEntity;

	public Action<AISpawnTDWavesData.TDWaveSpData, int> handlerNewWave;

	private static EntityMonsterBeacon s_spBeacon = null;

	private static List<EntityMonsterBeacon> s_Beacons = new List<EntityMonsterBeacon>();

	private bool isSweep;

	private MonsterAirborne _airborne;

	private MonsterBeaconMark m_Mark;

	private static List<int> s_spTypes0 = new List<int> { 0 };

	private static List<int> s_spTypes01 = new List<int> { 0, 1 };

	private static List<int> s_spTypes02 = new List<int> { 0, 2 };

	private static List<int> s_spTypes03 = new List<int> { 0, 3 };

	protected float PreTime
	{
		get
		{
			return _preTime;
		}
		set
		{
			_preTime = value;
			if (_uiData != null)
			{
				_uiData.PreTime = _preTime;
			}
		}
	}

	public Vector3 TargetPosition
	{
		set
		{
			_position = value;
		}
	}

	public int CampColor => _campCol;

	public bool IsMonsterKill => base.Id == -2;

	public AISpawnTDWavesData.TDWaveSpData SpData => _spData;

	public static bool IsRunning()
	{
		return s_Beacons.Count > 0;
	}

	public static bool IsBcnMonsterProtoId(int code)
	{
		return (code & 0x20000000) != 0;
	}

	public static int EncodeBcnMonsterProtoId(int spType, int dif, int spawnType)
	{
		spawnType++;
		if (spawnType < 0)
		{
			spawnType = 0;
		}
		return 0x20000000 | ((dif << 14) & 0xFFC000) | ((spType << 4) & 0x3FF0) | (spawnType & 0xF);
	}

	public static void DecodeBcnMonsterProtoId(int code, out int spType, out int dif, out int spawnType)
	{
		spawnType = (code & 0xF) - 1;
		spType = (code & 0x3FF0) >> 4;
		dif = (code & 0xFFC000) >> 14;
		if (spType >= 500)
		{
			dif = -1;
			spawnType = -1;
		}
	}

	public static EntityMonsterBeacon GetSpBeacon4MonsterKillTask()
	{
		if (s_spBeacon == null)
		{
			GameObject gameObject = new GameObject("SpBeacon4MK");
			s_spBeacon = gameObject.AddComponent<EntityMonsterBeacon4Kill>();
			PeSingleton<EntityMgr>.Instance.AddAfterAssignId(s_spBeacon, -2);
		}
		return s_spBeacon;
	}

	private static List<int> GetSpawnTypeMask(bool bOnlyMonster, out int campCol)
	{
		campCol = -1;
		if (bOnlyMonster)
		{
			return s_spTypes0;
		}
		if (PeGameMgr.IsStory)
		{
			int playerID = ((PeSingleton<PeCreature>.Instance.mainPlayer != null) ? ((int)PeSingleton<PeCreature>.Instance.mainPlayer.GetAttribute(AttribType.DefaultPlayerID)) : 0);
			bool flag = PeSingleton<ReputationSystem>.Instance.GetReputationLevel(playerID, 5) > ReputationSystem.ReputationLevel.Neutral;
			bool flag2 = PeSingleton<ReputationSystem>.Instance.GetReputationLevel(playerID, 6) > ReputationSystem.ReputationLevel.Neutral;
			if (flag)
			{
				return s_spTypes02;
			}
			if (flag2)
			{
				return s_spTypes01;
			}
			return (!(UnityEngine.Random.value > 0.5f)) ? s_spTypes02 : s_spTypes01;
		}
		return VATownGenerator.Instance.GetRandomExistEnemyType(out campCol) switch
		{
			AllyType.Puja => s_spTypes01, 
			AllyType.Paja => s_spTypes02, 
			AllyType.Npc => s_spTypes03, 
			_ => s_spTypes0, 
		};
	}

	public static EntityMonsterBeacon CreateMonsterBeaconByTDID(int spDataId, Transform targetTrans, TowerInfoUIData uiData, int entityId = -1, TypeTowerDefendsData data = null, int releaseNpcid = -1, bool bOnlyMonster = false)
	{
		int campCol = -1;
		List<int> spawnTypeMask = GetSpawnTypeMask(bOnlyMonster, out campCol);
		AISpawnTDWavesData.TDWaveSpData waveSpData = AISpawnTDWavesData.GetWaveSpData(spDataId, UnityEngine.Random.value, spawnTypeMask);
		if (waveSpData == null)
		{
			return null;
		}
		GameObject gameObject = new GameObject("MonsterBeacon");
		Vector3 pos = default(Vector3);
		if (data != null)
		{
			switch (data.m_Pos.type)
			{
			case TypeTowerDefendsData.PosType.getPos:
				pos = PeSingleton<PeCreature>.Instance.mainPlayer.position;
				break;
			case TypeTowerDefendsData.PosType.pos:
				pos = data.m_Pos.pos;
				break;
			case TypeTowerDefendsData.PosType.npcPos:
				pos = PeSingleton<EntityMgr>.Instance.Get(data.m_Pos.id).position;
				break;
			case TypeTowerDefendsData.PosType.doodadPos:
				pos = PeSingleton<EntityMgr>.Instance.GetDoodadEntities(data.m_Pos.id)[0].position;
				break;
			case TypeTowerDefendsData.PosType.conoly:
				if (!CSMain.GetAssemblyPos(out pos))
				{
					pos = PeSingleton<PeCreature>.Instance.mainPlayer.position;
				}
				break;
			case TypeTowerDefendsData.PosType.camp:
				if (!VArtifactUtil.GetTownPos(data.m_Pos.id, out pos))
				{
					pos = PeSingleton<PeCreature>.Instance.mainPlayer.position;
				}
				break;
			}
			data.finallyPos = pos;
			gameObject.transform.position = pos;
			gameObject.transform.rotation = Quaternion.identity;
		}
		else if (targetTrans != null)
		{
			pos = targetTrans.position;
			gameObject.transform.position = targetTrans.position;
			gameObject.transform.rotation = targetTrans.rotation;
		}
		EntityMonsterBeacon bcn = gameObject.AddComponent<EntityMonsterBeacon>();
		PeSingleton<EntityMgr>.Instance.AddAfterAssignId(bcn, (entityId == -1) ? PeSingleton<WorldInfoMgr>.Instance.FetchNonRecordAutoId() : entityId);
		bcn._campCol = campCol;
		bcn._uiData = uiData;
		bcn._spData = waveSpData;
		bcn._spDataId = spDataId;
		bcn._position = gameObject.transform.position;
		bcn._forward = gameObject.transform.forward;
		bcn.PreTime = waveSpData._timeToStart + waveSpData._waveDatas[0]._delayTime;
		if (UITowerInfo.Instance != null && uiData != null)
		{
			UITowerInfo.Instance.SetInfo(uiData);
			UITowerInfo.Instance.Show();
			UITowerInfo.Instance.e_BtnReady += delegate
			{
				bcn.PreTime = 0f;
			};
		}
		bcn.StartCoroutine(bcn.RefreshTowerMission());
		return bcn;
	}

	public static bool IsController()
	{
		if (PeGameMgr.IsSingle || (AiTowerDefense.mInstance != null && AiTowerDefense.mInstance.hasOwnerAuth))
		{
			return true;
		}
		return false;
	}

	public static EntityMonsterBeacon CreateMonsterBeaconBySweepID(List<int> sweepDataId, Transform targetTrans, TowerInfoUIData uiData, int preTime, int entityId = -1, TypeTowerDefendsData data = null, int releaseNpcid = -1)
	{
		GameObject gameObject = new GameObject("MonsterBeacon");
		Vector3 pos = default(Vector3);
		if (data != null)
		{
			switch (data.m_Pos.type)
			{
			case TypeTowerDefendsData.PosType.getPos:
				pos = PeSingleton<PeCreature>.Instance.mainPlayer.position;
				break;
			case TypeTowerDefendsData.PosType.pos:
				pos = data.m_Pos.pos;
				break;
			case TypeTowerDefendsData.PosType.npcPos:
				pos = PeSingleton<EntityMgr>.Instance.Get(data.m_Pos.id).position;
				break;
			case TypeTowerDefendsData.PosType.doodadPos:
				pos = PeSingleton<EntityMgr>.Instance.GetDoodadEntities(data.m_Pos.id)[0].position;
				break;
			case TypeTowerDefendsData.PosType.conoly:
				if (!CSMain.GetAssemblyPos(out pos))
				{
					pos = PeSingleton<PeCreature>.Instance.mainPlayer.position;
				}
				break;
			case TypeTowerDefendsData.PosType.camp:
				if (VArtifactUtil.GetTownPos(data.m_Pos.id, out pos))
				{
					pos = PeSingleton<PeCreature>.Instance.mainPlayer.position;
				}
				break;
			}
			data.finallyPos = pos;
		}
		gameObject.transform.position = pos;
		AISpawnTDWavesData.TDWaveSpData waveSpData = MonsterSweepData.GetWaveSpData(sweepDataId, pos);
		if (waveSpData == null)
		{
			return null;
		}
		EntityMonsterBeacon bcn = gameObject.AddComponent<EntityMonsterBeacon>();
		bcn.isSweep = true;
		PeSingleton<EntityMgr>.Instance.AddAfterAssignId(bcn, (entityId == -1) ? PeSingleton<WorldInfoMgr>.Instance.FetchNonRecordAutoId() : entityId);
		bcn._uiData = uiData;
		bcn._spData = waveSpData;
		bcn._position = pos;
		bcn._forward = Vector3.forward;
		bcn.PreTime = preTime + waveSpData._waveDatas[0]._delayTime;
		if (UITowerInfo.Instance != null && uiData != null)
		{
			UITowerInfo.Instance.SetInfo(uiData);
			UITowerInfo.Instance.Show();
			UITowerInfo.Instance.e_BtnReady += delegate
			{
				bcn.PreTime = 0f;
			};
		}
		bcn.StartCoroutine(bcn.RefreshTowerMission());
		return bcn;
	}

	private void Start()
	{
		if (!(this is EntityMonsterBeacon4Kill))
		{
			s_Beacons.Add(this);
		}
		if (!PeGameMgr.IsMulti && !IsMonsterKill)
		{
			m_Mark = new MonsterBeaconMark();
			m_Mark.Position = _position;
			m_Mark.IsMonsterSiege = true;
			PeSingleton<LabelMgr>.Instance.Add(m_Mark);
		}
	}

	private void OnDestroy()
	{
		if (_airborne != null)
		{
			MonsterAirborne.DestroyAirborne(_airborne);
		}
		if (!isSweep)
		{
			for (int i = 0; i < _agents.Count; i++)
			{
				MonEscape(_agents[i] as SceneEntityPosAgent, base.transform.position);
			}
		}
		SceneMan.RemoveSceneObjs(_agents);
		_agents.Clear();
		if (UITowerInfo.Instance != null && _uiData != null)
		{
			UITowerInfo.Instance.Hide();
		}
		s_Beacons.Remove(this);
		if (!PeGameMgr.IsMulti && m_Mark != null)
		{
			PeSingleton<LabelMgr>.Instance.Remove(m_Mark);
			m_Mark = null;
		}
		if (!(null != CSMain.Instance))
		{
			return;
		}
		List<CSAssembly> allAssemblies = CSMain.Instance.GetAllAssemblies();
		if (0 >= allAssemblies.Count)
		{
			return;
		}
		for (int j = 0; j < allAssemblies.Count; j++)
		{
			CSAssembly cSAssembly = allAssemblies[j];
			if (cSAssembly != null && cSAssembly.InRange(_position))
			{
				DigTerrainManager.ClearColonyBlockInfo(cSAssembly);
			}
		}
	}

	private void MonEscape(SceneEntityPosAgent agent, Vector3 center)
	{
		if (!(agent.entity == null))
		{
			Vector3 vector = agent.entity.position - center;
			vector.Normalize();
			if (agent.entity.monster != null)
			{
				Request request = agent.entity.monster.Req_MoveToPosition(agent.entity.position + vector * 50f, 1f, isForce: true, SpeedState.Run);
				request.AddRelation(EReqType.Attack, EReqRelation.Block);
			}
			Singleton<PeLogicGlobal>.Instance.DestroyEntity(agent.entity.aliveEntity, 10f);
		}
	}

	private IEnumerator RefreshTowerMission()
	{
		_idxWave = 0;
		_uiData.CurWavesRemaining = _spData._waveDatas.Count;
		_uiData.TotalWaves = _spData._waveDatas.Count;
		while (_idxWave < _spData._waveDatas.Count)
		{
			while (PreTime > 0f)
			{
				yield return new WaitForSeconds(1f);
				PreTime -= 1f;
			}
			PreTime = 0f;
			Vector3 dir = _forward;
			Vector3 center = _position;
			int m = _idxWave;
			AISpawnTDWavesData.TDWaveData wd = _spData._waveDatas[m];
			if (PeGameMgr.IsStory)
			{
				StroyManager.Instance.PushStoryList(wd._plotID);
			}
			int nMonsterTypes = wd._monsterTypes.Count;
			for (int n = 0; n < nMonsterTypes; n++)
			{
				int spType = wd._monsterTypes[n];
				int minAngle = wd._minDegs[n];
				int maxAngle = wd._maxDegs[n];
				int spCount = UnityEngine.Random.Range(wd._minNums[n], wd._maxNums[n]);
				for (int i = 0; i < spCount; i++)
				{
					Vector3 pos;
					if (spType == 520 || spType == 521)
					{
						pos = center;
					}
					else
					{
						if (isSweep)
						{
							pos = AiUtil.GetRandomPosition(center, 80f, 100f, dir, minAngle, maxAngle);
							base.transform.position = center + (center - pos) * 1000f;
						}
						else
						{
							pos = AiUtil.GetRandomPosition(center, 20f, 80f, dir, minAngle, maxAngle);
						}
						pos.y = 0f;
					}
					SceneEntityPosAgent agent = MonsterEntityCreator.CreateAgent(pos, EncodeBcnMonsterProtoId(spType, _spData._dif, _spData._spawnType));
					agent.spInfo = new MonsterEntityCreator.AgentInfo(this);
					agent.canRide = false;
					if (handerNewEntity != null)
					{
						handerNewEntity(agent);
					}
					_agents.Add(agent);
					SceneMan.AddSceneObj(agent);
				}
			}
			if (handlerNewWave != null)
			{
				handlerNewWave(_spData, _idxWave);
			}
			_uiData.CurWavesRemaining--;
			_idxWave++;
			if (_idxWave < _spData._waveDatas.Count)
			{
				for (int cdTime = _spData._timeToCool; cdTime > 0; cdTime--)
				{
					yield return new WaitForSeconds(1f);
				}
				PreTime = _spData._waveDatas[_idxWave]._delayTime;
			}
		}
	}

	public virtual void OnMonsterCreated(PeEntity e)
	{
		if (!(e != null))
		{
			return;
		}
		CommonCmpt cmpt = e.GetCmpt<CommonCmpt>();
		if (cmpt != null)
		{
			cmpt.TDObj = base.gameObject;
			cmpt.TDpos = base.gameObject.transform.position;
		}
		SkAliveEntity cmpt2 = e.GetCmpt<SkAliveEntity>();
		if (!(cmpt2 != null))
		{
			return;
		}
		cmpt2.deathEvent += delegate
		{
			OnMonsterDeath(e);
		};
		cmpt2.SetAttribute(AttribType.DefaultPlayerID, 8f);
		cmpt2.SetAttribute(AttribType.CampID, 26f);
		LodCmpt lc = e.lodCmpt;
		if (e.lodCmpt != null && PeGameMgr.IsSingle && isSweep)
		{
			e.lodCmpt.onDestroyView = delegate
			{
				OnMonsterEdge(lc);
			};
		}
	}

	private void OnMonsterEdge(LodCmpt lc)
	{
		lc.OnDestroy();
		if (_uiData != null)
		{
			_uiData.CurCount++;
		}
		if (handlerOneDeath != null)
		{
			handlerOneDeath();
		}
	}

	private void OnMonsterDeath(PeEntity e)
	{
		if (_uiData != null)
		{
			_uiData.CurCount++;
		}
		if (handlerOneDeath != null)
		{
			handlerOneDeath();
		}
	}

	public void Delete()
	{
		UnityEngine.Object.DestroyImmediate(base.gameObject);
	}

	public void UpdateUI(int missionId, int totalCount, int totalWave, float preTime)
	{
		if (_uiData == null)
		{
			return;
		}
		_uiData.MaxCount = totalCount;
		_uiData.TotalWaves = totalWave;
		_uiData.MissionID = missionId;
		_uiData.PreTime = preTime;
		if (UITowerInfo.Instance != null)
		{
			UITowerInfo.Instance.SetInfo(_uiData);
			UITowerInfo.Instance.Show();
			UITowerInfo.Instance.e_BtnReady += delegate
			{
				PreTime = 0f;
			};
		}
	}

	public void AddAirborneReq(SceneEntityPosAgent agent)
	{
		if (_airborne == null)
		{
			Vector3 point = _position;
			int layerMask = 79872;
			if (Physics.Raycast(point + 500f * Vector3.up, Vector3.down, out var hitInfo, 1000f, layerMask))
			{
				point = hitInfo.point;
			}
			MonsterAirborne.Type type = (((agent.protoId & 0x8000000) == 0) ? MonsterAirborne.Type.Paja : MonsterAirborne.Type.Puja);
			_airborne = MonsterAirborne.CreateAirborne(point, type);
		}
		_airborne.AddAirborneReq(agent);
	}
}

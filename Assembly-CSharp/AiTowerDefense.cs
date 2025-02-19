using System;
using System.Collections;
using ItemAsset;
using Pathea;
using PeMap;
using uLink;
using UnityEngine;

public class AiTowerDefense : AiCommonTD
{
	private static AiTowerDefense mInstance;

	private EntityMonsterBeacon _mbEntity;

	private int _missionId;

	private int _targetId;

	private MonsterBeaconMark m_Mark;

	protected bool isStart;

	public int MissionId => _missionId;

	public int TargetId => _targetId;

	protected override void OnPEAwake()
	{
		base.OnPEAwake();
		mInstance = this;
	}

	protected override void OnPEInstantiate(uLink.NetworkMessageInfo info)
	{
		_id = info.networkView.initialData.Read<int>(new object[0]);
		_missionId = info.networkView.initialData.Read<int>(new object[0]);
		_targetId = info.networkView.initialData.Read<int>(new object[0]);
		base.authId = info.networkView.initialData.Read<int>(new object[0]);
		_teamId = info.networkView.initialData.Read<int>(new object[0]);
		base._pos = base.transform.position;
		base.rot = base.transform.rotation;
		StartCoroutine(WaitForMainPlayer());
	}

	protected override void OnPEStart()
	{
		base.OnPEStart();
		BindAction(EPacketType.PT_InGame_TDStartInfo, RPC_S2C_TDStartInfo);
		BindAction(EPacketType.PT_InGame_TDInitData, RPC_S2C_ResponseInitData);
		BindAction(EPacketType.PT_InGame_TDInfo, RPC_S2C_TDInfo);
		BindAction(EPacketType.PT_InGame_TDMonsterDeath, RPC_S2C_MonsterDeath);
		if (IsAuth())
		{
			m_Mark = new MonsterBeaconMark();
			m_Mark.Position = GetAuthPos();
			PeSingleton<LabelMgr>.Instance.Add(m_Mark);
		}
	}

	protected override void OnPEDestroy()
	{
		base.OnPEDestroy();
		mInstance = null;
		PeSingleton<PeCreature>.Instance.Destory(base.Id);
		if (m_Mark != null)
		{
			PeSingleton<LabelMgr>.Instance.Remove(m_Mark);
		}
		if (null != UITowerInfo.Instance)
		{
			UITowerInfo.Instance.isShow = false;
		}
	}

	public static void OnMonsterAdd(int id, AiNetwork ai, PeEntity entity)
	{
		EntityMonsterBeacon entityMonsterBeacon = PeSingleton<EntityMgr>.Instance.Get(id) as EntityMonsterBeacon;
		if (null != entityMonsterBeacon)
		{
			entityMonsterBeacon.OnMonsterCreated(entity);
		}
		else
		{
			if (!(null != entity))
			{
				return;
			}
			CommonCmpt cmpt = entity.GetCmpt<CommonCmpt>();
			if (cmpt != null)
			{
				AiTowerDefense aiTowerDefense = NetworkInterface.Get<AiTowerDefense>(id);
				if (null != aiTowerDefense)
				{
					cmpt.TDpos = aiTowerDefense._pos;
				}
			}
			SkAliveEntity cmpt2 = entity.GetCmpt<SkAliveEntity>();
			if (cmpt2 != null)
			{
				cmpt2.SetAttribute(AttribType.DefaultPlayerID, 8f);
				cmpt2.SetAttribute(AttribType.CampID, 26f);
			}
		}
	}

	public static bool IsAuth()
	{
		return !(null == mInstance) && mInstance.hasOwnerAuth;
	}

	public static Vector3 GetAuthPos()
	{
		PlayerNetwork player = PlayerNetwork.GetPlayer(mInstance.authId);
		if (null == player)
		{
			return Vector3.zero;
		}
		return player._pos;
	}

	private IEnumerator WaitForMainPlayer()
	{
		while (null == PlayerNetwork.mainPlayer)
		{
			yield return null;
		}
		if (base.hasOwnerAuth)
		{
			if (_missionId == -1 && _targetId == -1)
			{
				ItemObject item = PeSingleton<ItemMgr>.Instance.Get(base.Id);
				if (item != null)
				{
					Drag drag = item.GetCmpt<Drag>();
					if (drag != null)
					{
						DragArticleAgent.Create(drag, base._pos, Vector3.one, Quaternion.identity, base.Id, this);
						StartCoroutine(WaitForActivate());
					}
				}
			}
			else
			{
				SceneEntityCreator.self.AddMissionPoint(_missionId, _targetId, base.Id);
				StartCoroutine(WaitForActivate());
			}
		}
		else
		{
			RPCServer(EPacketType.PT_InGame_TDInitData);
		}
	}

	private void SyncTDInfo(int totalCount, int waveIndex, float preTime, float coolTime)
	{
		RPCServer(EPacketType.PT_InGame_TDInfo, totalCount, waveIndex, preTime, coolTime);
	}

	private void SyncTDStartInfo(int totalWave, float preTime)
	{
		RPCServer(EPacketType.PT_InGame_TDStartInfo, totalWave, preTime);
	}

	private void OnActivate()
	{
		if (null != _mbEntity)
		{
			isStart = false;
			EntityMonsterBeacon mbEntity = _mbEntity;
			mbEntity.handlerNewWave = (Action<AISpawnTDWavesData.TDWaveSpData, int>)Delegate.Combine(mbEntity.handlerNewWave, new Action<AISpawnTDWavesData.TDWaveSpData, int>(OnNewWave));
			EntityMonsterBeacon mbEntity2 = _mbEntity;
			mbEntity2.handerNewEntity = (Action<SceneEntityPosAgent>)Delegate.Combine(mbEntity2.handerNewEntity, new Action<SceneEntityPosAgent>(OnNewEntity));
			if (MissionId == -1 && TargetId == -1)
			{
				Vector3 pos = base._pos;
			}
			else
			{
				GetTdGenPos(out var pos);
				Vector3 vector = pos;
				_mbEntity.TargetPosition = vector;
				vector = vector;
				base.transform.position = vector;
				base._pos = vector;
			}
			if (_mbEntity.SpData != null)
			{
				float num = _mbEntity.SpData._timeToStart + _mbEntity.SpData._waveDatas[0]._delayTime;
				totalWave = _mbEntity.SpData._waveDatas.Count;
				_mbEntity.UpdateUI(_missionId, 0, totalWave, num);
				SyncTDStartInfo(totalWave, num);
			}
		}
	}

	public static Vector3 GetTdGenPos(int targetId)
	{
		Vector3 pos = Vector3.zero;
		TypeTowerDefendsData typeTowerDefendsData = MissionManager.GetTypeTowerDefendsData(targetId);
		if (typeTowerDefendsData != null)
		{
			switch (typeTowerDefendsData.m_Pos.type)
			{
			case TypeTowerDefendsData.PosType.getPos:
				pos = PeSingleton<PeCreature>.Instance.mainPlayer.position;
				break;
			case TypeTowerDefendsData.PosType.pos:
				pos = typeTowerDefendsData.m_Pos.pos;
				break;
			case TypeTowerDefendsData.PosType.npcPos:
				pos = PeSingleton<EntityMgr>.Instance.Get(typeTowerDefendsData.m_Pos.id).position;
				break;
			case TypeTowerDefendsData.PosType.doodadPos:
				pos = PeSingleton<EntityMgr>.Instance.GetDoodadEntities(typeTowerDefendsData.m_Pos.id)[0].position;
				break;
			case TypeTowerDefendsData.PosType.conoly:
				if (!CSMain.GetAssemblyPos(out pos))
				{
					pos = PeSingleton<PeCreature>.Instance.mainPlayer.position;
				}
				break;
			case TypeTowerDefendsData.PosType.camp:
				if (!VArtifactUtil.GetTownPos(typeTowerDefendsData.m_Pos.id, out pos))
				{
					pos = PeSingleton<PeCreature>.Instance.mainPlayer.position;
				}
				break;
			}
		}
		return pos;
	}

	private void GetTdGenPos(out Vector3 pos)
	{
		pos = Vector3.zero;
		TypeTowerDefendsData typeTowerDefendsData = MissionManager.GetTypeTowerDefendsData(TargetId);
		if (typeTowerDefendsData == null)
		{
			return;
		}
		switch (typeTowerDefendsData.m_Pos.type)
		{
		case TypeTowerDefendsData.PosType.getPos:
			pos = PeSingleton<PeCreature>.Instance.mainPlayer.position;
			break;
		case TypeTowerDefendsData.PosType.pos:
			pos = typeTowerDefendsData.m_Pos.pos;
			break;
		case TypeTowerDefendsData.PosType.npcPos:
			pos = PeSingleton<EntityMgr>.Instance.Get(typeTowerDefendsData.m_Pos.id).position;
			break;
		case TypeTowerDefendsData.PosType.doodadPos:
			pos = PeSingleton<EntityMgr>.Instance.GetDoodadEntities(typeTowerDefendsData.m_Pos.id)[0].position;
			break;
		case TypeTowerDefendsData.PosType.conoly:
			if (!CSMain.GetAssemblyPos(out pos))
			{
				pos = PeSingleton<PeCreature>.Instance.mainPlayer.position;
			}
			break;
		case TypeTowerDefendsData.PosType.camp:
			if (!VArtifactUtil.GetTownPos(typeTowerDefendsData.m_Pos.id, out pos))
			{
				pos = PeSingleton<PeCreature>.Instance.mainPlayer.position;
			}
			break;
		}
		typeTowerDefendsData.finallyPos = pos;
	}

	private IEnumerator WaitForActivate()
	{
		do
		{
			yield return null;
			_mbEntity = PeSingleton<EntityMgr>.Instance.Get(base.Id) as EntityMonsterBeacon;
		}
		while (!(null != _mbEntity));
		OnActivate();
	}

	private void OnNewWave(AISpawnTDWavesData.TDWaveSpData tdData, int wave)
	{
		wave++;
		if (tdData != null)
		{
			float num = 0f;
			float num2 = 0f;
			if (tdData._waveDatas.Count != wave)
			{
				num = tdData._timeToCool;
				num2 = tdData._waveDatas[wave]._delayTime;
			}
			SyncTDInfo(totalCount, wave, num2, num);
			if (null != _mbEntity)
			{
				_mbEntity.UpdateUI(_missionId, totalCount, totalWave, num2);
			}
		}
	}

	private void OnNewEntity(SceneEntityPosAgent agent)
	{
		totalCount++;
	}

	private IEnumerator CouterCoroutine()
	{
		while (true)
		{
			if (coolTime != 0f)
			{
				yield return new WaitForSeconds(1f);
				coolTime -= 1f;
			}
			else
			{
				MissionManager.Instance.m_PlayerMission.m_TowerUIData.PreTime = Mathf.Clamp(preTime, 0f, 1000f);
				yield return new WaitForSeconds(1f);
				preTime -= 1f;
			}
		}
	}

	protected void RPC_S2C_TDStartInfo(uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
		if (!base.hasOwnerAuth && !(null == PlayerNetwork.mainPlayer) && !isStart)
		{
			isStart = true;
			totalWave = stream.Read<int>(new object[0]);
			deathCount = stream.Read<int>(new object[0]);
			int num = stream.Read<int>(new object[0]);
			preTime = stream.Read<float>(new object[0]);
			coolTime = 0f;
			MissionManager.Instance.m_PlayerMission.m_TowerUIData.TotalWaves = totalWave;
			MissionManager.Instance.m_PlayerMission.m_TowerUIData.CurWavesRemaining = totalWave - num;
			MissionManager.Instance.m_PlayerMission.m_TowerUIData.bRefurbish = true;
			MissionManager.Instance.m_PlayerMission.m_TowerUIData.MissionID = MissionId;
			MissionManager.Instance.m_PlayerMission.m_TowerUIData.CurCount = deathCount;
			MissionManager.Instance.m_PlayerMission.m_TowerUIData.PreTime = preTime;
			if (UITowerInfo.Instance != null)
			{
				UITowerInfo.Instance.SetInfo(MissionManager.Instance.m_PlayerMission.m_TowerUIData);
				UITowerInfo.Instance.Show();
				StartCoroutine(CouterCoroutine());
			}
		}
	}

	protected void RPC_S2C_ResponseInitData(uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
		if (!base.hasOwnerAuth && !(null == PlayerNetwork.mainPlayer) && !isStart)
		{
			isStart = true;
			totalWave = stream.Read<int>(new object[0]);
			deathCount = stream.Read<int>(new object[0]);
			int num = stream.Read<int>(new object[0]);
			preTime = stream.Read<float>(new object[0]);
			coolTime = stream.Read<float>(new object[0]);
			MissionManager.Instance.m_PlayerMission.m_TowerUIData.TotalWaves = totalWave;
			MissionManager.Instance.m_PlayerMission.m_TowerUIData.CurWavesRemaining = totalWave - num;
			MissionManager.Instance.m_PlayerMission.m_TowerUIData.bRefurbish = true;
			MissionManager.Instance.m_PlayerMission.m_TowerUIData.MissionID = MissionId;
			MissionManager.Instance.m_PlayerMission.m_TowerUIData.CurCount = deathCount;
			MissionManager.Instance.m_PlayerMission.m_TowerUIData.PreTime = preTime;
			if (UITowerInfo.Instance != null)
			{
				UITowerInfo.Instance.SetInfo(MissionManager.Instance.m_PlayerMission.m_TowerUIData);
				UITowerInfo.Instance.Show();
				StartCoroutine(CouterCoroutine());
			}
		}
	}

	protected void RPC_S2C_TDInfo(uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
		if (!base.hasOwnerAuth && !(null == PlayerNetwork.mainPlayer))
		{
			totalCount = stream.Read<int>(new object[0]);
			deathCount = stream.Read<int>(new object[0]);
			int num = stream.Read<int>(new object[0]);
			preTime = stream.Read<float>(new object[0]);
			coolTime = stream.Read<float>(new object[0]);
			MissionManager.Instance.m_PlayerMission.m_TowerUIData.bRefurbish = true;
			MissionManager.Instance.m_PlayerMission.m_TowerUIData.CurWavesRemaining = totalWave - num;
			MissionManager.Instance.m_PlayerMission.m_TowerUIData.MaxCount = totalCount;
			MissionManager.Instance.m_PlayerMission.m_TowerUIData.CurCount = deathCount;
			MissionManager.Instance.m_PlayerMission.m_TowerUIData.PreTime = preTime;
		}
	}

	private void RPC_S2C_MonsterDeath(uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
		if (!base.hasOwnerAuth && !(null == PlayerNetwork.mainPlayer))
		{
			deathCount = stream.Read<int>(new object[0]);
			MissionManager.Instance.m_PlayerMission.m_TowerUIData.CurCount = deathCount;
		}
	}
}

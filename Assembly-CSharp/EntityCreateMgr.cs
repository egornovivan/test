using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using Mono.Data.SqliteClient;
using Pathea;
using Pathea.PeEntityExt;
using Pathea.PeEntityExtTrans;
using PETools;
using SkillSystem;
using UnityEngine;

public class EntityCreateMgr : MonoBehaviour
{
	public struct Min_Max_Int
	{
		public int m_Min;

		public int m_Max;

		public int Random()
		{
			return UnityEngine.Random.Range(m_Min, m_Max);
		}
	}

	public class StoryEntityMgr
	{
		public IntVec2 m_IdxPos;

		public List<int> m_FixCreateID;

		public List<IntVec3> m_RandomCreatePosMap;

		public Dictionary<EntityType, Dictionary<IntVec3, EntityPosAgent>> m_CreatedEntityMap;

		public StoryEntityMgr()
		{
			m_FixCreateID = new List<int>();
			m_RandomCreatePosMap = new List<IntVec3>();
			m_CreatedEntityMap = new Dictionary<EntityType, Dictionary<IntVec3, EntityPosAgent>>();
		}

		public void Clear()
		{
			m_FixCreateID.Clear();
			m_RandomCreatePosMap.Clear();
			foreach (KeyValuePair<EntityType, Dictionary<IntVec3, EntityPosAgent>> item in m_CreatedEntityMap)
			{
				foreach (KeyValuePair<IntVec3, EntityPosAgent> item2 in item.Value)
				{
					SceneMan.RemoveSceneObj(item2.Value);
				}
			}
		}
	}

	public const bool DbgUseLegacyCode = false;

	public const int CreateNpcNum = 3;

	public const int CreateMonsterNum = 5;

	public const int CreateNpcRadius = 128;

	public const int TowerDefencePlayerID = 8;

	public const int TowerDefenceCampID = 26;

	private int TowerMissionID;

	private int TowerStep;

	private int TowerIdxI;

	private AISpawnWaveData TowerAiwd;

	private bool bTowerStarted = true;

	public Dictionary<int, PeEntity> m_TowerDefineMonsterMap;

	public Dictionary<IntVec2, StoryEntityMgr> m_StoryEntityMgr;

	public bool npcLoaded;

	private static EntityCreateMgr mInstance;

	private PeTrans m_PlayerTrans;

	public static EntityCreateMgr Instance => mInstance;

	public Transform mPlayerTrans
	{
		get
		{
			if (m_PlayerTrans == null)
			{
				m_PlayerTrans = PeSingleton<PeCreature>.Instance.mainPlayer.peTrans;
			}
			return m_PlayerTrans.trans;
		}
	}

	public void RemoveStoryEntityMgr(IntVec2 key)
	{
		if (key != null && m_StoryEntityMgr.TryGetValue(key, out var value))
		{
			value.Clear();
			m_StoryEntityMgr.Remove(key);
		}
	}

	private void Awake()
	{
		m_StoryEntityMgr = new Dictionary<IntVec2, StoryEntityMgr>();
		m_TowerDefineMonsterMap = new Dictionary<int, PeEntity>();
		mInstance = this;
	}

	private void Update()
	{
	}

	public void New()
	{
	}

	private IEnumerator InitNpc()
	{
		while (PeSingleton<PeCreature>.Instance.mainPlayer == null)
		{
			yield return new WaitForSeconds(0.1f);
		}
		if (PeGameMgr.IsSingleStory)
		{
			InitDefaultNpc();
		}
		InitRandomNpc();
		yield return 2;
		npcLoaded = true;
	}

	private void InitRandomNpc()
	{
		foreach (KeyValuePair<int, NpcMissionData> dicMissionDatum in NpcMissionDataRepository.dicMissionData)
		{
			if (dicMissionDatum.Value.m_Rnpc_ID != -1 && !PeGameMgr.IsMulti)
			{
				PeEntity peEntity = PeSingleton<PeCreature>.Instance.CreateRandomNpc(dicMissionDatum.Value.m_Rnpc_ID, dicMissionDatum.Key, Vector3.zero, Quaternion.identity, Vector3.one);
				if (null == peEntity)
				{
					break;
				}
				PeTrans peTrans = peEntity.peTrans;
				if (null == peTrans)
				{
					Debug.LogError("entity has no ViewCmpt");
					break;
				}
				peTrans.position = dicMissionDatum.Value.m_Pos;
				peEntity.SetUserData(dicMissionDatum.Value);
				peEntity.SetBirthPos(dicMissionDatum.Value.m_Pos);
				SetNpcShopIcon(peEntity);
			}
		}
	}

	private void InitDefaultNpc()
	{
		SqliteDataReader sqliteDataReader = LocalDatabase.Instance.ReadFullTable("NPC");
		while (sqliteDataReader.Read())
		{
			int id = Convert.ToInt32(sqliteDataReader.GetString(sqliteDataReader.GetOrdinal("NPC_ID")));
			int protoId = Convert.ToInt32(sqliteDataReader.GetString(sqliteDataReader.GetOrdinal("PrototypeNPC")));
			if (!PeGameMgr.IsMulti)
			{
				PeEntity peEntity = PeSingleton<PeCreature>.Instance.CreateNpc(id, protoId, Vector3.zero, Quaternion.identity, Vector3.one);
				if (!(peEntity == null))
				{
					InitNpcWithDb(peEntity, sqliteDataReader);
					NpcMissionData missionData = NpcMissionDataRepository.GetMissionData(peEntity.Id);
					peEntity.SetUserData(missionData);
					SetNpcShopIcon(peEntity);
				}
			}
		}
	}

	private bool InitNpcWithDb(PeEntity entity, SqliteDataReader reader)
	{
		string text = reader.GetString(reader.GetOrdinal("startpoint"));
		if (PeGameMgr.playerType == PeGameMgr.EPlayerType.Tutorial)
		{
			text = reader.GetString(reader.GetOrdinal("training_pos"));
			if (text == "0,0,0")
			{
				text = "50000,0,0";
			}
		}
		string[] array = text.Split(',');
		if (array.Length < 3)
		{
			Debug.LogError("Npc's StartPoint is Error");
		}
		else
		{
			float x = Convert.ToSingle(array[0]);
			float y = Convert.ToSingle(array[1]);
			float z = Convert.ToSingle(array[2]);
			PeTrans peTrans = entity.peTrans;
			if (null != peTrans)
			{
				peTrans.position = new Vector3(x, y, z);
			}
			NpcCmpt npcCmpt = entity.NpcCmpt;
			if (null != npcCmpt)
			{
				npcCmpt.FixedPointPos = new Vector3(x, y, z);
			}
		}
		SetNpcMoney(entity, reader.GetString(reader.GetOrdinal("money")));
		return true;
	}

	public void CreateEntityFromCustom(int id, int proid, string name, int Count, int spawnNum, float scale, int re)
	{
	}

	public void CreateEntity(int NpcNum, int monsterNum, int Radius)
	{
		List<IntVec2> idxPos = new List<IntVec2>();
		if (!GetMapCreateCenterPosList(NpcNum + monsterNum, Radius, ref idxPos))
		{
			return;
		}
		if (NetworkInterface.IsClient && null != PlayerNetwork.mainPlayer)
		{
			byte[] binPos = Serialize.Export(delegate(BinaryWriter w)
			{
				w.Write(NpcNum);
				w.Write(monsterNum);
				w.Write(idxPos.Count);
				foreach (IntVec2 item in idxPos)
				{
					int num = item.x << 16;
					num |= item.y;
					w.Write(num);
				}
			});
			PlayerNetwork.mainPlayer.SyncSpawnPos(binPos);
		}
		for (int i = 0; i < idxPos.Count; i++)
		{
			if (!m_StoryEntityMgr.ContainsKey(idxPos[i]))
			{
				continue;
			}
			StoryEntityMgr storyEntityMgr = m_StoryEntityMgr[idxPos[i]];
			if (storyEntityMgr == null)
			{
				continue;
			}
			List<int> adRandListByWild = NpcMissionDataRepository.GetAdRandListByWild(1);
			int j = 0;
			int count = storyEntityMgr.m_RandomCreatePosMap.Count;
			for (; j < NpcNum; j++)
			{
				int index = UnityEngine.Random.Range(1, adRandListByWild.Count);
				CreateAdRandomNpcMgr(adRandListByWild[index], storyEntityMgr.m_RandomCreatePosMap[j], storyEntityMgr);
			}
			for (; j < count; j++)
			{
				CreateMonsterMgr(storyEntityMgr.m_RandomCreatePosMap[j], storyEntityMgr);
			}
			if (!PeGameMgr.IsStory)
			{
				continue;
			}
			foreach (int item2 in storyEntityMgr.m_FixCreateID)
			{
				AISpawnPoint aISpawnPoint = AISpawnPoint.s_spawnPointData[item2];
				CreateFixPosMonsterMgr(aISpawnPoint.resId, new IntVec3(aISpawnPoint.Position), storyEntityMgr);
			}
		}
	}

	public bool GetMapCreateCenterPosList(int MaxEntityNum, int Radius, ref List<IntVec2> indexPos)
	{
		if (PeSingleton<PeCreature>.Instance == null)
		{
			return false;
		}
		if (PeSingleton<PeCreature>.Instance.mainPlayer == null)
		{
			return false;
		}
		Vector3 vector = PeSingleton<PeCreature>.Instance.mainPlayer.ExtGetPos();
		int num = ((!(vector.x > 0f)) ? (Radius * -1) : Radius);
		int num2 = (int)(vector.x + (float)num) / (Radius * 2);
		num = ((!(vector.z > 0f)) ? (Radius * -1) : Radius);
		int num3 = (int)(vector.z + (float)num) / (Radius * 2);
		int num4 = num2;
		int num5 = num3;
		StoryEntityMgr storyEntityMgr = null;
		for (int i = -1; i < 2; i++)
		{
			num2 = num4 + i;
			for (int j = -1; j < 2; j++)
			{
				num3 = num5 + j;
				IntVec2 intVec = new IntVec2(num2, num3);
				Vector3 center = new Vector3(num2 * (Radius * 2), 0f, num3 * (Radius * 2));
				if (m_StoryEntityMgr.ContainsKey(intVec))
				{
					continue;
				}
				storyEntityMgr = new StoryEntityMgr();
				storyEntityMgr.m_IdxPos = intVec;
				for (int k = 0; k < MaxEntityNum; k++)
				{
					IntVec3 patrolPoint;
					if (!PeGameMgr.IsStory)
					{
						patrolPoint = GetPatrolPoint(center, bCheck: false);
						storyEntityMgr.m_RandomCreatePosMap.Add(patrolPoint);
						continue;
					}
					Vector3 randomPosition = AiUtil.GetRandomPosition(center, -400f, 400f);
					patrolPoint = new IntVec3(randomPosition.x, randomPosition.y, randomPosition.z);
					if (AIErodeMap.IsInErodeArea2D(randomPosition) == null)
					{
						storyEntityMgr.m_RandomCreatePosMap.Add(patrolPoint);
					}
				}
				storyEntityMgr.m_FixCreateID = AISpawnPoint.Find(center.x - (float)Radius, center.z - (float)Radius, center.x + (float)Radius, center.z + (float)Radius);
				m_StoryEntityMgr.Add(intVec, storyEntityMgr);
				if (!indexPos.Contains(intVec))
				{
					indexPos.Add(intVec);
				}
			}
		}
		return indexPos.Count > 0;
	}

	private bool CreateAdRandomNpcMgr(int adnpcid, IntVec3 npcPos, StoryEntityMgr sem)
	{
		EntityPosAgent entityPosAgent = new EntityPosAgent();
		entityPosAgent.idx = sem.m_IdxPos;
		entityPosAgent.proid = adnpcid;
		entityPosAgent.entitytype = EntityType.EntityType_Npc;
		entityPosAgent.position = new Vector3(npcPos.x, npcPos.y, npcPos.z);
		if (sem.m_CreatedEntityMap.ContainsKey(EntityType.EntityType_Npc))
		{
			if (!sem.m_CreatedEntityMap[EntityType.EntityType_Npc].ContainsKey(npcPos))
			{
				sem.m_CreatedEntityMap[EntityType.EntityType_Npc][npcPos] = entityPosAgent;
				SceneMan.AddSceneObj(entityPosAgent);
				return true;
			}
			return false;
		}
		Dictionary<IntVec3, EntityPosAgent> dictionary = new Dictionary<IntVec3, EntityPosAgent>();
		dictionary.Add(npcPos, entityPosAgent);
		sem.m_CreatedEntityMap.Add(EntityType.EntityType_Npc, dictionary);
		SceneMan.AddSceneObj(entityPosAgent);
		return true;
	}

	private void CreateFixPosMonsterMgr(int mid, IntVec3 pos, StoryEntityMgr sem)
	{
		EntityPosAgent entityPosAgent = new EntityPosAgent();
		entityPosAgent.idx = sem.m_IdxPos;
		entityPosAgent.proid = mid;
		entityPosAgent.entitytype = EntityType.EntityType_Monster;
		entityPosAgent.position = new Vector3(pos.x, pos.y, pos.z);
		SceneMan.AddSceneObj(entityPosAgent);
		if (sem.m_CreatedEntityMap.ContainsKey(EntityType.EntityType_Monster))
		{
			if (!sem.m_CreatedEntityMap[EntityType.EntityType_Monster].ContainsKey(pos))
			{
				sem.m_CreatedEntityMap[EntityType.EntityType_Monster][pos] = entityPosAgent;
			}
		}
		else
		{
			Dictionary<IntVec3, EntityPosAgent> dictionary = new Dictionary<IntVec3, EntityPosAgent>();
			dictionary.Add(pos, entityPosAgent);
			sem.m_CreatedEntityMap.Add(EntityType.EntityType_Monster, dictionary);
		}
	}

	public PeEntity CreateRandomNpc(int proID, Vector3 pos)
	{
		AdNpcData adNpcData = NpcMissionDataRepository.GetAdNpcData(proID);
		if (adNpcData == null)
		{
			return null;
		}
		if (NetworkInterface.IsClient && !PeGameMgr.IsMultiStory)
		{
			if (proID > 100)
			{
				NetworkManager.WaitCoroutine(PlayerNetwork.RequestCreateAdNpc(proID, pos));
			}
			else
			{
				NetworkManager.WaitCoroutine(PlayerNetwork.RequestCreateAdMainNpc(proID, pos));
			}
			return null;
		}
		int id = PeSingleton<WorldInfoMgr>.Instance.FetchRecordAutoId();
		PeEntity peEntity = PeSingleton<PeCreature>.Instance.CreateRandomNpc(adNpcData.mRnpc_ID, id, Vector3.zero, Quaternion.identity, Vector3.one);
		if (null == peEntity)
		{
			return null;
		}
		NpcMissionData npcMissionData = new NpcMissionData();
		npcMissionData.m_bRandomNpc = true;
		npcMissionData.m_Rnpc_ID = adNpcData.mRnpc_ID;
		npcMissionData.m_QCID = adNpcData.mQC_ID;
		PeTrans peTrans = peEntity.peTrans;
		if (null == peTrans)
		{
			Debug.LogError("entity has no ViewCmpt");
			return null;
		}
		peTrans.position = pos;
		peEntity.SetBirthPos(pos);
		int randomMission = AdRMRepository.GetRandomMission(npcMissionData.m_QCID, npcMissionData.m_CurMissionGroup);
		if (randomMission != 0)
		{
			npcMissionData.m_RandomMission = randomMission;
		}
		for (int i = 0; i < adNpcData.m_CSRecruitMissionList.Count; i++)
		{
			npcMissionData.m_CSRecruitMissionList.Add(adNpcData.m_CSRecruitMissionList[i]);
		}
		NpcMissionDataRepository.AddMissionData(adNpcData.mID, npcMissionData);
		peEntity.SetUserData(npcMissionData);
		return peEntity;
	}

	private void CreateMonsterMgr(IntVec3 pos, StoryEntityMgr sem)
	{
		EntityPosAgent entityPosAgent = new EntityPosAgent();
		entityPosAgent.idx = sem.m_IdxPos;
		entityPosAgent.proid = -1;
		entityPosAgent.entitytype = EntityType.EntityType_Monster;
		entityPosAgent.position = new Vector3(pos.x, pos.y, pos.z);
		SceneMan.AddSceneObj(entityPosAgent);
		if (sem.m_CreatedEntityMap.ContainsKey(EntityType.EntityType_Monster))
		{
			sem.m_CreatedEntityMap[EntityType.EntityType_Monster][pos] = entityPosAgent;
			return;
		}
		Dictionary<IntVec3, EntityPosAgent> dictionary = new Dictionary<IntVec3, EntityPosAgent>();
		dictionary.Add(pos, entityPosAgent);
		sem.m_CreatedEntityMap.Add(EntityType.EntityType_Monster, dictionary);
	}

	private int GetMonsterProtoID(Vector3 pos, EntityPosAgent epa)
	{
		int result = 0;
		int num = 0;
		num = (int)AiUtil.GetPointType(pos);
		if (PeGameMgr.IsStory)
		{
			int aiSpawnMapId = PeSingleton<PeMappingMgr>.Instance.GetAiSpawnMapId(new Vector2(pos.x, pos.z));
			result = AISpeciesData.GetRandomAI(AISpawnDataStory.GetAiSpawnData(aiSpawnMapId, num));
		}
		else if (PeGameMgr.IsAdventure)
		{
			int mapID = AiUtil.GetMapID(pos);
			int areaID = AiUtil.GetAreaID(pos);
			result = AISpawnDataAdvSingle.GetPathID(mapID, areaID, num);
		}
		return result;
	}

	public PeEntity CreateMonsterInst(Vector3 pos, int proid, EntityPosAgent epa)
	{
		bool flag = false;
		if (proid < 0)
		{
			proid = GetMonsterProtoID(pos, epa);
		}
		if (NetworkInterface.IsClient)
		{
			NetworkManager.WaitCoroutine(PlayerNetwork.RequestCreateAi(proid, pos, -1, -1, -1));
			return null;
		}
		PeEntity peEntity = null;
		if (flag)
		{
			int id = PeSingleton<WorldInfoMgr>.Instance.FetchRecordAutoId();
			peEntity = PeSingleton<PeCreature>.Instance.CreateMonster(id, proid, Vector3.zero, Quaternion.identity, Vector3.one);
		}
		else
		{
			int id2 = PeSingleton<WorldInfoMgr>.Instance.FetchNonRecordAutoId();
			peEntity = PeSingleton<PeEntityCreator>.Instance.CreateMonster(id2, proid, Vector3.zero, Quaternion.identity, Vector3.one);
		}
		if (epa.entitytype == EntityType.EntityType_MonsterTD)
		{
			SkAliveEntity cmpt = peEntity.GetCmpt<SkAliveEntity>();
			if (cmpt == null)
			{
				return null;
			}
			CommonCmpt cmpt2 = peEntity.GetCmpt<CommonCmpt>();
			if (cmpt2 != null)
			{
				cmpt2.TDObj = GameObject.Find("TowerMission");
			}
			cmpt.SetAttribute(AttribType.DefaultPlayerID, 8f);
			cmpt.SetAttribute(AttribType.CampID, 26f);
			if (!m_TowerDefineMonsterMap.ContainsKey(peEntity.Id))
			{
				m_TowerDefineMonsterMap.Add(peEntity.Id, peEntity);
			}
			peEntity.GetGameObject().name = "MisMonster";
		}
		if (null == peEntity)
		{
			Debug.LogError("create monster error");
			return null;
		}
		epa.createdid = peEntity.Id;
		LodCmpt lodCmpt = peEntity.lodCmpt;
		if (lodCmpt != null)
		{
			lodCmpt.onDeactivate = delegate
			{
				epa.DestroyEntity();
			};
		}
		PeTrans peTrans = peEntity.peTrans;
		if (null == peTrans)
		{
			Debug.LogError("entity has no ViewCmpt");
			return null;
		}
		SkAliveEntity cmpt3 = peEntity.GetCmpt<SkAliveEntity>();
		if (cmpt3 == null)
		{
			Debug.LogError("entity has no SkAliveEntity");
			return null;
		}
		cmpt3.deathEvent += MonsterDeath;
		pos.y += MonsterProtoDb.Get(proid).hOffset;
		peTrans.position = pos;
		return peEntity;
	}

	public void MonsterDeath(SkEntity self, SkEntity caster)
	{
		SkAliveEntity skAliveEntity = self as SkAliveEntity;
		CommonCmpt cmpt = skAliveEntity.Entity.GetCmpt<CommonCmpt>();
		if (!(cmpt == null))
		{
			Debug.Log(cmpt.entityProto.protoId);
			if (!(MissionManager.Instance == null))
			{
				MissionManager.Instance.ProcessMonsterDead(cmpt.entityProto.protoId, skAliveEntity.Entity.Id);
			}
		}
	}

	public bool CreateMisMonster(Vector3 center, float radius, int proid, int num)
	{
		for (int i = 0; i < num; i++)
		{
			EntityPosAgent entityPosAgent = new EntityPosAgent();
			entityPosAgent.entitytype = EntityType.EntityType_Monster;
			entityPosAgent.position = AiUtil.GetRandomPosition(center, 0f, radius);
			entityPosAgent.bMission = true;
			entityPosAgent.proid = proid;
			SceneMan.AddSceneObj(entityPosAgent);
		}
		return true;
	}

	public void StartTowerMission(int MissionID, int step, TypeTowerDefendsData towerData, float time = 0f)
	{
		MissionCommonData missionCommonData = MissionManager.GetMissionCommonData(MissionID);
		int num = -1;
		for (int i = 0; i < missionCommonData.m_TargetIDList.Count; i++)
		{
			if (missionCommonData.m_TargetIDList[i] == towerData.m_TargetID)
			{
				num = i;
				break;
			}
		}
		if (num == -1)
		{
			return;
		}
		AISpawnAutomatic automatic = AISpawnAutomatic.GetAutomatic(towerData.m_TdInfoId);
		if (automatic == null || automatic.data.Count <= step)
		{
			return;
		}
		AISpawnWaveData aISpawnWaveData = automatic.data[step];
		if (aISpawnWaveData == null || aISpawnWaveData.data.data.Count == 0)
		{
			return;
		}
		int num2 = num * 100 + step * 10;
		string missionFlag = PlayerMission.MissionFlagTDMonster + num2;
		string questVariable = MissionManager.Instance.GetQuestVariable(MissionID, missionFlag);
		string[] array = questVariable.Split('_');
		if (array.Length != 5)
		{
			return;
		}
		int num3 = Convert.ToInt32(array[3]);
		if (num3 != 1)
		{
			MissionManager.mTowerCurWave = (step + 1).ToString();
			MissionManager.mTowerTotalWave = automatic.data.Count.ToString();
			float num4 = aISpawnWaveData.delayTime;
			if (time > 0f)
			{
				num4 = time;
			}
			MissionManager.Instance.m_PlayerMission.m_TowerUIData.PreTime = num4;
			TowerMissionID = MissionID;
			TowerStep = step;
			TowerIdxI = num;
			TowerAiwd = aISpawnWaveData;
			bTowerStarted = false;
			StartCoroutine(WaitingTowerStart(MissionID, num4, step, num, aISpawnWaveData, bCom: false));
			UITowerInfo.Instance.Show();
			UITowerInfo.Instance.e_BtnReady += ImmediatelyStartTower;
		}
	}

	public IEnumerator WaitingTowerStart(int MissionID, float time, int step, int idxI, AISpawnWaveData aiwd, bool bCom)
	{
		float delaytime = 0f;
		while (time > delaytime && !bTowerStarted)
		{
			yield return new WaitForSeconds(1f);
			delaytime += 1f;
			MissionManager.Instance.m_PlayerMission.m_TowerUIData.PreTime -= 1f;
			if (MissionManager.Instance.m_PlayerMission.m_TowerUIData.PreTime < 0f)
			{
				MissionManager.Instance.m_PlayerMission.m_TowerUIData.PreTime = 0f;
			}
		}
		CreateTowerDefineMonster(MissionID, step, idxI, aiwd, bCom);
	}

	public void ImmediatelyStartTower()
	{
		if (!bTowerStarted)
		{
			CreateTowerDefineMonster(TowerMissionID, TowerStep, TowerIdxI, TowerAiwd, bCom: false);
		}
	}

	public void CreateTowerDefineMonster(int MissionID, int step, int idxI, AISpawnWaveData aiwd, bool bCom)
	{
		if (bTowerStarted)
		{
			return;
		}
		MissionManager.Instance.m_PlayerMission.m_TowerUIData.PreTime = 0f;
		Vector3 playerPos = GetPlayerPos();
		Vector3 playerDir = GetPlayerDir();
		for (int i = 0; i < aiwd.data.data.Count; i++)
		{
			AISpawnData aISpawnData = aiwd.data.data[i];
			if (aISpawnData != null)
			{
				int num = idxI * 100 + step * 10 + i;
				string questVariable = MissionManager.Instance.GetQuestVariable(MissionID, PlayerMission.MissionFlagTDMonster + num);
				string[] array = questVariable.Split('_');
				if (array.Length != 5)
				{
					return;
				}
				int num2 = Convert.ToInt32(array[2]);
				for (int j = 0; j < num2; j++)
				{
					EntityPosAgent entityPosAgent = new EntityPosAgent();
					entityPosAgent.entitytype = EntityType.EntityType_MonsterTD;
					entityPosAgent.position = AiUtil.GetRandomPosition(playerPos, 0f, 45f, playerDir, aISpawnData.minAngle, aISpawnData.maxAngle);
					entityPosAgent.bMission = true;
					entityPosAgent.proid = aISpawnData.spID;
					SceneMan.AddSceneObj(entityPosAgent);
				}
				if (!bCom)
				{
					string text = "_1";
					questVariable = array[0] + "_" + array[1] + "_" + array[2] + text + "_" + array[4];
					MissionManager.Instance.ModifyQuestVariable(MissionID, PlayerMission.MissionFlagTDMonster + num, questVariable);
				}
			}
		}
		bTowerStarted = true;
	}

	public void SetNpcShopIcon(PeEntity npc)
	{
		string storeNpcIcon = StoreRepository.GetStoreNpcIcon(npc.Id);
		if (!(storeNpcIcon == "0"))
		{
			npc.SetShopIcon(storeNpcIcon);
		}
	}

	public void SetNpcMoney(PeEntity entity, string text)
	{
		NpcPackageCmpt cmpt = entity.GetCmpt<NpcPackageCmpt>();
		string[] array = text.Split(';');
		if (array.Length != 3)
		{
			return;
		}
		string[] array2 = array[0].Split(',');
		Min_Max_Int min_Max_Int = default(Min_Max_Int);
		if (array2.Length != 2 || !int.TryParse(array2[0], out min_Max_Int.m_Min) || !int.TryParse(array2[1], out min_Max_Int.m_Max))
		{
			return;
		}
		array2 = array[1].Split(',');
		Min_Max_Int min_Max_Int2 = default(Min_Max_Int);
		if (array2.Length == 2 && int.TryParse(array2[0], out min_Max_Int2.m_Min) && int.TryParse(array2[1], out min_Max_Int2.m_Max))
		{
			int result = 0;
			if (int.TryParse(array[2], out result))
			{
				cmpt.InitAutoIncreaseMoney(result, min_Max_Int2.Random());
				cmpt.money.current = min_Max_Int.Random();
			}
		}
	}

	public void SaveEntityCreated(BinaryWriter bw)
	{
		bw.Write(m_StoryEntityMgr.Count);
		foreach (KeyValuePair<IntVec2, StoryEntityMgr> item in m_StoryEntityMgr)
		{
			StoryEntityMgr value = item.Value;
			if (value == null)
			{
				continue;
			}
			bw.Write(item.Key.x);
			bw.Write(item.Key.y);
			bw.Write(value.m_CreatedEntityMap.Count);
			foreach (KeyValuePair<EntityType, Dictionary<IntVec3, EntityPosAgent>> item2 in value.m_CreatedEntityMap)
			{
				bw.Write((int)item2.Key);
				bw.Write(item2.Value.Count);
				foreach (KeyValuePair<IntVec3, EntityPosAgent> item3 in item2.Value)
				{
					EntityPosAgent value2 = item3.Value;
					if (value2 != null)
					{
						if (value2.createdid == 0)
						{
							bw.Write(value: false);
							continue;
						}
						bw.Write(value: true);
						bw.Write(item3.Key.x);
						bw.Write(item3.Key.y);
						bw.Write(item3.Key.z);
						bw.Write(value2.proid);
						Serialize.WriteVector3(bw, value2.position);
					}
				}
			}
		}
	}

	public void ReadEntityCreated(byte[] buffer)
	{
		if (buffer == null || buffer.Length == 0)
		{
			return;
		}
		MemoryStream memoryStream = new MemoryStream(buffer);
		BinaryReader binaryReader = new BinaryReader(memoryStream);
		foreach (KeyValuePair<IntVec2, StoryEntityMgr> item in m_StoryEntityMgr)
		{
			item.Value.Clear();
		}
		m_StoryEntityMgr.Clear();
		int num = binaryReader.ReadInt32();
		for (int i = 0; i < num; i++)
		{
			int x = binaryReader.ReadInt32();
			int y = binaryReader.ReadInt32();
			IntVec2 intVec = new IntVec2(x, y);
			int num2 = binaryReader.ReadInt32();
			for (int j = 0; j < num2; j++)
			{
				EntityType entityType = (EntityType)binaryReader.ReadInt32();
				int num3 = binaryReader.ReadInt32();
				for (int k = 0; k < num3; k++)
				{
					if (binaryReader.ReadBoolean())
					{
						x = binaryReader.ReadInt32();
						y = binaryReader.ReadInt32();
						int num4 = binaryReader.ReadInt32();
						IntVec3 key = new IntVec3(x, y, num4);
						EntityPosAgent entityPosAgent = new EntityPosAgent();
						entityPosAgent.idx = intVec;
						entityPosAgent.proid = binaryReader.ReadInt32();
						entityPosAgent.entitytype = entityType;
						entityPosAgent.position = Serialize.ReadVector3(binaryReader);
						Dictionary<IntVec3, EntityPosAgent> dictionary = new Dictionary<IntVec3, EntityPosAgent>();
						dictionary.Add(key, entityPosAgent);
						StoryEntityMgr storyEntityMgr = new StoryEntityMgr();
						storyEntityMgr.m_IdxPos = intVec;
						storyEntityMgr.m_CreatedEntityMap.Add(entityType, dictionary);
					}
				}
			}
		}
		binaryReader.Close();
		memoryStream.Close();
	}

	private void SaveNpcMissionData(int npcID, NpcMissionData data, BinaryWriter _out)
	{
		if (data == null)
		{
			_out.Write(-1);
			_out.Write(npcID);
			return;
		}
		_out.Write(npcID);
		_out.Write(data.m_Rnpc_ID);
		_out.Write(data.m_QCID);
		Serialize.WriteVector3(_out, data.m_Pos);
		_out.Write(data.m_CurMissionGroup);
		_out.Write(data.m_CurGroupTimes);
		_out.Write(data.mCurComMisNum);
		_out.Write(data.mCompletedMissionCount);
		_out.Write(data.m_RandomMission);
		_out.Write(data.m_RecruitMissionNum);
		_out.Write(data.m_bRandomNpc);
		_out.Write(data.m_bColonyOrder);
		_out.Write(data.mInFollowMission);
		_out.Write(data.m_MissionList.Count);
		for (int i = 0; i < data.m_MissionList.Count; i++)
		{
			_out.Write(data.m_MissionList[i]);
		}
		_out.Write(data.m_MissionListReply.Count);
		for (int j = 0; j < data.m_MissionListReply.Count; j++)
		{
			_out.Write(data.m_MissionListReply[j]);
		}
		_out.Write(data.m_RecruitMissionList.Count);
		for (int k = 0; k < data.m_RecruitMissionList.Count; k++)
		{
			_out.Write(data.m_RecruitMissionList[k]);
		}
		_out.Write(data.m_CSRecruitMissionList.Count);
		for (int l = 0; l < data.m_CSRecruitMissionList.Count; l++)
		{
			_out.Write(data.m_CSRecruitMissionList[l]);
		}
	}

	public void Export(BinaryWriter bw)
	{
		int value = 0;
		bw.Write(value);
		bw.Write(NpcMissionDataRepository.dicMissionData.Count);
		foreach (KeyValuePair<int, NpcMissionData> dicMissionDatum in NpcMissionDataRepository.dicMissionData)
		{
			if (dicMissionDatum.Value.m_Rnpc_ID == -1)
			{
				bw.Write(-2);
				bw.Write(dicMissionDatum.Key);
				bw.Write(dicMissionDatum.Value.m_MissionList.Count);
				for (int i = 0; i < dicMissionDatum.Value.m_MissionList.Count; i++)
				{
					bw.Write(dicMissionDatum.Value.m_MissionList[i]);
				}
				bw.Write(dicMissionDatum.Value.m_MissionListReply.Count);
				for (int j = 0; j < dicMissionDatum.Value.m_MissionListReply.Count; j++)
				{
					bw.Write(dicMissionDatum.Value.m_MissionListReply[j]);
				}
				bw.Write(dicMissionDatum.Value.m_RecruitMissionList.Count);
				for (int k = 0; k < dicMissionDatum.Value.m_RecruitMissionList.Count; k++)
				{
					bw.Write(dicMissionDatum.Value.m_RecruitMissionList[k]);
				}
				bw.Write(dicMissionDatum.Value.m_CSRecruitMissionList.Count);
				for (int l = 0; l < dicMissionDatum.Value.m_CSRecruitMissionList.Count; l++)
				{
					bw.Write(dicMissionDatum.Value.m_CSRecruitMissionList[l]);
				}
			}
			else
			{
				SaveNpcMissionData(dicMissionDatum.Key, dicMissionDatum.Value, bw);
			}
		}
	}

	private void ReadNpcMissionData(NpcMissionData data, BinaryReader _in)
	{
		data.m_MissionList.Clear();
		data.m_MissionListReply.Clear();
		data.m_CSRecruitMissionList.Clear();
		data.m_RecruitMissionList.Clear();
		data.m_Rnpc_ID = _in.ReadInt32();
		if (data.m_Rnpc_ID != -1)
		{
			data.m_QCID = _in.ReadInt32();
			data.m_Pos = Serialize.ReadVector3(_in);
			data.m_CurMissionGroup = _in.ReadInt32();
			data.m_CurGroupTimes = _in.ReadInt32();
			data.mCurComMisNum = _in.ReadByte();
			data.mCompletedMissionCount = _in.ReadInt32();
			data.m_RandomMission = _in.ReadInt32();
			data.m_RecruitMissionNum = _in.ReadInt32();
			data.m_bRandomNpc = _in.ReadBoolean();
			data.m_bColonyOrder = _in.ReadBoolean();
			data.mInFollowMission = _in.ReadBoolean();
			int num = _in.ReadInt32();
			for (int i = 0; i < num; i++)
			{
				data.m_MissionList.Add(_in.ReadInt32());
			}
			num = _in.ReadInt32();
			for (int j = 0; j < num; j++)
			{
				data.m_MissionListReply.Add(_in.ReadInt32());
			}
			num = _in.ReadInt32();
			for (int k = 0; k < num; k++)
			{
				data.m_RecruitMissionList.Add(_in.ReadInt32());
			}
			num = _in.ReadInt32();
			for (int l = 0; l < num; l++)
			{
				data.m_CSRecruitMissionList.Add(_in.ReadInt32());
			}
		}
	}

	public void Import(byte[] buffer)
	{
		if (buffer == null || buffer.Length == 0)
		{
			return;
		}
		MemoryStream memoryStream = new MemoryStream(buffer);
		BinaryReader binaryReader = new BinaryReader(memoryStream);
		int curArvhiveVersion = PeSingleton<ArchiveMgr>.Instance.GetCurArvhiveVersion();
		if (curArvhiveVersion <= 0)
		{
			int num = binaryReader.ReadInt32();
			int num2 = ((num >= NpcMissionDataRepository.dicMissionData.Count) ? NpcMissionDataRepository.dicMissionData.Count : num);
			PeEntity peEntity = null;
			foreach (KeyValuePair<int, NpcMissionData> dicMissionDatum in NpcMissionDataRepository.dicMissionData)
			{
				num2--;
				if (num2 <= -1)
				{
					break;
				}
				if (dicMissionDatum.Value.m_Rnpc_ID == -1)
				{
					peEntity = PeSingleton<EntityMgr>.Instance.Get(dicMissionDatum.Key);
				}
				else
				{
					ReadNpcMissionData(dicMissionDatum.Value, binaryReader);
					peEntity = PeSingleton<EntityMgr>.Instance.Get(dicMissionDatum.Key);
				}
				if (!(peEntity == null))
				{
					peEntity.SetUserData(dicMissionDatum.Value);
				}
			}
		}
		else
		{
			binaryReader.ReadInt32();
			int num3 = binaryReader.ReadInt32();
			NpcMissionDataRepository.dicMissionData.Clear();
			PeEntity peEntity2 = null;
			NpcMissionData npcMissionData = null;
			for (int i = 0; i < num3; i++)
			{
				int num4 = binaryReader.ReadInt32();
				switch (num4)
				{
				case -2:
				{
					npcMissionData = new NpcMissionData();
					npcMissionData.m_Rnpc_ID = -1;
					int num6 = binaryReader.ReadInt32();
					peEntity2 = PeSingleton<EntityMgr>.Instance.Get(num6);
					int num7 = binaryReader.ReadInt32();
					for (int j = 0; j < num7; j++)
					{
						npcMissionData.m_MissionList.Add(binaryReader.ReadInt32());
					}
					num7 = binaryReader.ReadInt32();
					for (int k = 0; k < num7; k++)
					{
						npcMissionData.m_MissionListReply.Add(binaryReader.ReadInt32());
					}
					num7 = binaryReader.ReadInt32();
					for (int l = 0; l < num7; l++)
					{
						npcMissionData.m_RecruitMissionList.Add(binaryReader.ReadInt32());
					}
					num7 = binaryReader.ReadInt32();
					for (int m = 0; m < num7; m++)
					{
						npcMissionData.m_CSRecruitMissionList.Add(binaryReader.ReadInt32());
					}
					NpcMissionDataRepository.AddMissionData(num6, npcMissionData);
					break;
				}
				case -1:
				{
					int npcId = binaryReader.ReadInt32();
					NpcMissionDataRepository.AddMissionData(npcId, null);
					continue;
				}
				default:
				{
					npcMissionData = new NpcMissionData();
					int num5 = num4;
					ReadNpcMissionData(npcMissionData, binaryReader);
					NpcMissionDataRepository.AddMissionData(num5, npcMissionData);
					peEntity2 = PeSingleton<EntityMgr>.Instance.Get(num5);
					break;
				}
				}
				if (!(peEntity2 == null))
				{
					peEntity2.SetUserData(npcMissionData);
				}
			}
		}
		binaryReader.Close();
		memoryStream.Close();
	}

	public void NpcMouseEventHandler(object sender, EntityMgr.RMouseClickEntityEvent e)
	{
		PeEntity entity = e.entity;
		if (entity == null)
		{
			return;
		}
		float num = Vector3.Distance(entity.position, GetPlayerPos());
		if (!(num > 4.5f) && IsRandomNpc(entity) && entity.IsDead() && entity.Id != 9203 && entity.Id != 9204 && ((entity.Id != 9214 && entity.Id != 9215) || MissionManager.Instance.HasMission(242)))
		{
			if (GameConfig.IsMultiMode)
			{
			}
			if (entity.IsRecruited())
			{
				GameUI.Instance.mRevive.ShowServantRevive(entity);
			}
		}
	}

	public void NpcTalkRequest(object sender, EntityMgr.NPCTalkEvent e)
	{
		PeEntity entity = e.entity;
		if (entity == null || (entity.proto != EEntityProto.Npc && entity.proto != EEntityProto.RandomNpc))
		{
			return;
		}
		float num = Vector3.Distance(entity.position, GetPlayerPos());
		if (num > 9f || (IsRandomNpc(entity) && entity.IsDead()) || (IsRandomNpc(entity) && entity.IsFollower()) || !entity.GetTalkEnable())
		{
			return;
		}
		if (IsRandomNpc(entity) && !entity.IsDead() && entity.GetUserData() is NpcMissionData { m_RandomMission: not 0 } npcMissionData && !MissionManager.Instance.HasMission(npcMissionData.m_RandomMission))
		{
			if (PeGameMgr.IsSingleStory)
			{
				RMRepository.CreateRandomMission(npcMissionData.m_RandomMission, npcMissionData.mCurComMisNum);
			}
			else if (PeGameMgr.IsMultiAdventure || PeGameMgr.IsMultiBuild || PeGameMgr.IsMultiStory)
			{
				PlayerNetwork.mainPlayer.RPCServer(EPacketType.PT_InGame_NewMission, npcMissionData.m_RandomMission, entity.Id);
			}
			else
			{
				AdRMRepository.CreateRandomMission(npcMissionData.m_RandomMission);
			}
		}
		if (!entity.IsDead() && entity.NpcCmpt.CanTalk && !GameUI.Instance.mNPCTalk.isPlayingTalk)
		{
			if (GameUI.Instance.mNpcWnd.IsOpen())
			{
				StroyManager.Instance.RemoveReq(GameUI.Instance.mNpcWnd.m_CurSelNpc, EReqType.Dialogue);
			}
			GameUI.Instance.mNpcWnd.SetCurSelNpc(entity, sayHalo: true);
			GameUI.Instance.mNpcWnd.Show();
		}
	}

	public IntVec3 GetPatrolPoint(Vector3 center, bool bCheck = true)
	{
		Vector2 vector = UnityEngine.Random.insideUnitCircle;
		Vector3 vector2;
		bool canGenNpc;
		do
		{
			vector = vector.normalized * UnityEngine.Random.Range(-400, 400);
			vector2 = center + new Vector3(vector.x, 0f, vector.y);
			IntVector2 worldPosXZ = new IntVector2((int)vector2.x, (int)vector2.z);
			canGenNpc = false;
			vector2.y = VFDataRTGen.GetPosTop(worldPosXZ, out canGenNpc);
		}
		while ((vector2.y > -1.01f && vector2.y < -0.99f) || !canGenNpc);
		return new IntVec3(vector2.x, vector2.y, vector2.z);
	}

	public Vector3 GetPlayerPos()
	{
		if (mPlayerTrans == null)
		{
			return Vector3.zero;
		}
		return m_PlayerTrans.position;
	}

	public Vector3 GetPlayerDir()
	{
		if (mPlayerTrans == null)
		{
			return Vector3.zero;
		}
		return m_PlayerTrans.forward;
	}

	public Transform GetPlayerTrans()
	{
		return mPlayerTrans;
	}

	public bool IsRandomNpc(PeEntity npc)
	{
		if (npc == null)
		{
			return false;
		}
		if (!(npc.GetUserData() is NpcMissionData npcMissionData))
		{
			return false;
		}
		return npcMissionData.m_bRandomNpc;
	}
}

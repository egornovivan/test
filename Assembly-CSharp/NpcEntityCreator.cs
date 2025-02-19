using System;
using System.Collections.Generic;
using ItemAsset;
using Mono.Data.SqliteClient;
using Pathea;
using Pathea.PeEntityExt;
using Pathea.PeEntityExtTrans;
using UnityEngine;

public class NpcEntityCreator
{
	public class AgentInfo : SceneEntityPosAgent.AgentInfo
	{
		public new static AgentInfo s_defAgentInfo = new AgentInfo();

		public override void OnSuceededToCreate(SceneEntityPosAgent agent)
		{
			SceneMan.RemoveSceneObj(agent);
		}
	}

	public static void Init()
	{
	}

	public static SceneEntityPosAgent CreateAgent(Vector3 pos, int protoId, Vector3 scl, Quaternion rot)
	{
		SceneEntityPosAgent sceneEntityPosAgent = new SceneEntityPosAgent(pos, scl, rot, EntityType.EntityType_Npc, protoId);
		sceneEntityPosAgent.spInfo = AgentInfo.s_defAgentInfo;
		return sceneEntityPosAgent;
	}

	public static SceneEntityPosAgent CreateAgent(Vector3 pos, int protoId = -1)
	{
		SceneEntityPosAgent sceneEntityPosAgent = new SceneEntityPosAgent(pos, Vector3.one, Quaternion.identity, EntityType.EntityType_Npc, protoId);
		sceneEntityPosAgent.spInfo = AgentInfo.s_defAgentInfo;
		return sceneEntityPosAgent;
	}

	public static PeEntity CreateNpc(int protoId, Vector3 pos)
	{
		SceneEntityPosAgent sceneEntityPosAgent = CreateAgent(pos, protoId);
		CreateNpc(sceneEntityPosAgent);
		return sceneEntityPosAgent.entity;
	}

	public static PeEntity CreateNpc(int protoId, Vector3 pos, Vector3 scl, Quaternion rot)
	{
		SceneEntityPosAgent sceneEntityPosAgent = CreateAgent(pos, protoId, scl, rot);
		CreateNpc(sceneEntityPosAgent);
		return sceneEntityPosAgent.entity;
	}

	public static void CreateNpc(SceneEntityPosAgent agent)
	{
		agent.entity = null;
		if (agent.protoId < 0)
		{
			List<int> adRandListByWild = NpcMissionDataRepository.GetAdRandListByWild(1);
			int index = UnityEngine.Random.Range(0, adRandListByWild.Count);
			agent.protoId = adRandListByWild[index];
		}
		AdNpcData adNpcData = NpcMissionDataRepository.GetAdNpcData(agent.protoId);
		if (adNpcData == null)
		{
			return;
		}
		if (NetworkInterface.IsClient && !PeGameMgr.IsMultiStory)
		{
			if (agent.protoId > 100)
			{
				NetworkManager.WaitCoroutine(PlayerNetwork.RequestCreateAdNpc(agent.protoId, agent.Pos));
			}
			else
			{
				NetworkManager.WaitCoroutine(PlayerNetwork.RequestCreateAdMainNpc(agent.protoId, agent.Pos));
			}
			return;
		}
		int id = PeSingleton<WorldInfoMgr>.Instance.FetchRecordAutoId();
		agent.entity = PeSingleton<PeCreature>.Instance.CreateRandomNpc(adNpcData.mRnpc_ID, id, agent.Pos, agent.Rot, agent.Scl);
		if (null == agent.entity)
		{
			Debug.LogError("[SceneEntityCreator]Failed to create npc:" + agent.protoId);
			return;
		}
		if ((bool)MissionManager.Instance && agent.protoId < 100 && !MissionManager.Instance.m_PlayerMission.adId_entityId.ContainsKey(agent.protoId))
		{
			MissionManager.Instance.m_PlayerMission.adId_entityId[agent.protoId] = agent.entity.Id;
		}
		agent.entity.SetBirthPos(agent.Pos);
		NpcMissionData npcMissionData = new NpcMissionData();
		npcMissionData.m_bRandomNpc = true;
		npcMissionData.m_Rnpc_ID = adNpcData.mRnpc_ID;
		npcMissionData.m_QCID = adNpcData.mQC_ID;
		int randomMission = AdRMRepository.GetRandomMission(npcMissionData.m_QCID, npcMissionData.m_CurMissionGroup);
		if (randomMission != 0)
		{
			npcMissionData.m_RandomMission = randomMission;
		}
		for (int i = 0; i < adNpcData.m_CSRecruitMissionList.Count; i++)
		{
			npcMissionData.m_CSRecruitMissionList.Add(adNpcData.m_CSRecruitMissionList[i]);
		}
		NpcMissionDataRepository.AddMissionData(agent.entity.Id, npcMissionData);
		agent.entity.SetUserData(npcMissionData);
	}

	public static void CreateStoryRandNpc()
	{
		foreach (KeyValuePair<int, NpcMissionData> dicMissionDatum in NpcMissionDataRepository.dicMissionData)
		{
			if (dicMissionDatum.Value.m_Rnpc_ID != -1)
			{
				if (PeGameMgr.IsMulti)
				{
					break;
				}
				PeEntity peEntity = PeSingleton<PeCreature>.Instance.CreateRandomNpc(dicMissionDatum.Value.m_Rnpc_ID, dicMissionDatum.Key, dicMissionDatum.Value.m_Pos, Quaternion.identity, Vector3.one);
				if (null == peEntity)
				{
					break;
				}
				peEntity.SetUserData(dicMissionDatum.Value);
				peEntity.SetBirthPos(dicMissionDatum.Value.m_Pos);
				SetNpcShopIcon(peEntity);
			}
		}
	}

	public static void CreateStoryLineNpcFromID(int npcID, Vector3 position)
	{
		SqliteDataReader sqliteDataReader = LocalDatabase.Instance.ReadFullTable("NPC");
		NpcMissionDataRepository.Reset();
		while (sqliteDataReader.Read())
		{
			int num = Convert.ToInt32(sqliteDataReader.GetString(sqliteDataReader.GetOrdinal("NPC_ID")));
			int protoId = Convert.ToInt32(sqliteDataReader.GetString(sqliteDataReader.GetOrdinal("PrototypeNPC")));
			if (num == npcID)
			{
				PeEntity peEntity = PeSingleton<PeCreature>.Instance.CreateNpc(num, protoId, Vector3.zero, Quaternion.identity, Vector3.one);
				if (!(peEntity == null))
				{
					InitNpcWithDb(peEntity, sqliteDataReader);
					NpcMissionData missionData = NpcMissionDataRepository.GetMissionData(peEntity.Id);
					peEntity.SetUserData(missionData);
					SetNpcShopIcon(peEntity);
					peEntity.position = position;
					break;
				}
			}
		}
	}

	public static void CreateStoryLineNpc()
	{
		SqliteDataReader sqliteDataReader = LocalDatabase.Instance.ReadFullTable("NPC");
		NpcMissionDataRepository.Reset();
		while (sqliteDataReader.Read())
		{
			int id = Convert.ToInt32(sqliteDataReader.GetString(sqliteDataReader.GetOrdinal("NPC_ID")));
			int protoId = Convert.ToInt32(sqliteDataReader.GetString(sqliteDataReader.GetOrdinal("PrototypeNPC")));
			if (PeGameMgr.IsMultiStory)
			{
				break;
			}
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

	public static void CreateTutorialLineNpc()
	{
		SqliteDataReader sqliteDataReader = LocalDatabase.Instance.ReadFullTable("NPC");
		while (sqliteDataReader.Read())
		{
			int num = Convert.ToInt32(sqliteDataReader.GetString(sqliteDataReader.GetOrdinal("NPC_ID")));
			int protoId = Convert.ToInt32(sqliteDataReader.GetString(sqliteDataReader.GetOrdinal("PrototypeNPC")));
			string @string = sqliteDataReader.GetString(sqliteDataReader.GetOrdinal("training_pos"));
			if (@string != "0,0,0")
			{
				string[] array = @string.Split(',');
				Vector3 zero = Vector3.zero;
				if (array.Length < 3)
				{
					Debug.LogError("Npc's StartPoint is Error at NPC_ID=" + num);
				}
				else
				{
					zero.x = Convert.ToSingle(array[0]);
					zero.y = Convert.ToSingle(array[1]);
					zero.z = Convert.ToSingle(array[2]);
				}
				PeEntity peEntity = PeSingleton<PeCreature>.Instance.CreateNpc(num, protoId, zero, Quaternion.identity, Vector3.one);
				if (!(peEntity == null))
				{
					SetNpcMoney(peEntity, sqliteDataReader.GetString(sqliteDataReader.GetOrdinal("money")));
					AddWeaponItem(peEntity, sqliteDataReader.GetString(sqliteDataReader.GetOrdinal("weapon")), sqliteDataReader.GetString(sqliteDataReader.GetOrdinal("item")));
					SetNpcAbility(peEntity, sqliteDataReader.GetString(sqliteDataReader.GetOrdinal("speciality")));
					NpcMissionData missionData = NpcMissionDataRepository.GetMissionData(peEntity.Id);
					peEntity.SetUserData(missionData);
					SetNpcShopIcon(peEntity);
				}
			}
		}
	}

	private static bool InitNpcWithDb(PeEntity entity, SqliteDataReader reader)
	{
		string @string = reader.GetString(reader.GetOrdinal("startpoint"));
		string[] array = @string.Split(',');
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
		AddWeaponItem(entity, reader.GetString(reader.GetOrdinal("weapon")), reader.GetString(reader.GetOrdinal("item")));
		SetNpcAbility(entity, reader.GetString(reader.GetOrdinal("speciality")));
		return true;
	}

	private static void AddWeaponItem(PeEntity npc, string weapon, string item)
	{
		if (!weapon.Equals("0"))
		{
			SetNpcWeapon(npc, Convert.ToInt32(weapon));
		}
		if (item.Equals("0"))
		{
			return;
		}
		string[] array = item.Split(',', ';');
		if (array.Length > 1)
		{
			for (int i = 0; i < array.Length / 2; i++)
			{
				SetNpcPackageItem(npc, Convert.ToInt32(array[i * 2]), Convert.ToInt32(array[i * 2 + 1]));
			}
		}
	}

	private static void SetNpcWeapon(PeEntity npc, int weaponID)
	{
		if (weaponID != 0)
		{
			npc.GetComponent<EquipmentCmpt>().AddInitEquipment(PeSingleton<ItemMgr>.Instance.CreateItem(weaponID));
		}
	}

	private static void SetNpcPackageItem(PeEntity npc, int itemID, int num)
	{
		NpcPackageCmpt cmpt = npc.GetCmpt<NpcPackageCmpt>();
		cmpt.Add(itemID, num);
	}

	private static void SetNpcShopIcon(PeEntity npc)
	{
		string storeNpcIcon = StoreRepository.GetStoreNpcIcon(npc.Id);
		if (!(storeNpcIcon == "0"))
		{
			npc.SetShopIcon(storeNpcIcon);
		}
	}

	private static void SetNpcMoney(PeEntity entity, string text)
	{
		NpcPackageCmpt cmpt = entity.GetCmpt<NpcPackageCmpt>();
		string[] array = text.Split(';');
		if (array.Length != 3)
		{
			return;
		}
		string[] array2 = array[0].Split(',');
		if (array2.Length != 2 || !int.TryParse(array2[0], out var result) || !int.TryParse(array2[1], out var result2))
		{
			return;
		}
		array2 = array[1].Split(',');
		if (array2.Length == 2 && int.TryParse(array2[0], out var result3) && int.TryParse(array2[1], out var result4))
		{
			int result5 = 0;
			if (int.TryParse(array[2], out result5))
			{
				cmpt.InitAutoIncreaseMoney(result5, UnityEngine.Random.Range(result3, result4));
				cmpt.money.current = UnityEngine.Random.Range(result, result2);
			}
		}
	}

	private static void SetNpcAbility(PeEntity entity, string text)
	{
		if (!string.IsNullOrEmpty(text) && !(text == "0"))
		{
			Ablities ablities = new Ablities();
			string[] array = text.Split(',');
			for (int i = 0; i < array.Length; i++)
			{
				ablities.Add(Convert.ToInt32(array[i]));
			}
			NpcCmpt cmpt = entity.GetCmpt<NpcCmpt>();
			if (cmpt != null)
			{
				cmpt.SetAbilityIDs(ablities);
			}
		}
	}
}

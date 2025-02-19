using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using CustomCharactor;
using CustomData;
using ItemAsset;
using Mono.Data.SqliteClient;
using Pathea;
using PETools;
using uLink;
using UnityEngine;

public class AiAdNpcNetwork : AiObject
{
	public enum EReqType
	{
		Idle,
		Animation,
		MoveToPoint,
		FollowPath,
		FollowTarget,
		Salvation,
		Dialogue,
		Translate,
		Rotate,
		Attack,
		PauseAll,
		UseSkill,
		TalkMove,
		Hand,
		Remove,
		MAX
	}

	protected int workState;

	private int _mountId;

	protected double workRestTime;

	internal NpcMissionData mission = new NpcMissionData();

	protected int _missionState = 3;

	protected int mProtoId;

	protected int originTeam;

	protected float rotY;

	protected string mCustomName;

	protected bool hasRecord;

	protected bool isStand;

	protected bool bDesTroy;

	public bool bForcedServant;

	private Vector3 gerdyPutDownPos = new Vector3(12246.42f, 193.1f, 6528.76f);

	protected int mAutoReviveTime;

	protected CharacterName mCharacterName;

	protected DigitalMoney mNpcMoney = new DigitalMoney();

	public AutoIncreaseMoney mAutoIncreaseMoney;

	protected float deadStartTime;

	protected bool inFollowMission;

	protected bool bCanCarrier;

	protected bool bRecruited;

	public static int TotalRescueGameTime = 120;

	protected int m_npcType;

	protected int nMissionFlag;

	protected AttackMode attackMode;

	protected CustomCharactor.CustomData _customData;

	protected MissionNpcInfo mni = new MissionNpcInfo();

	public Player lordPlayer;

	protected ItemCmpt m_ItemModule;

	protected NpcAbilityCmpt m_Npcskillcmpt;

	protected ItemCmpt m_ServantItemModule;

	public int ProtoId => mProtoId;

	public bool IsRandomNpc => base.Id > 9200;

	public string NpcName => mCharacterName.FullName;

	public string ShowName => mCharacterName.GivenName;

	public DigitalMoney Money => mNpcMoney;

	public float DeadStartTime
	{
		get
		{
			return deadStartTime;
		}
		set
		{
			deadStartTime = value;
		}
	}

	public bool IsInFollowMission
	{
		get
		{
			return inFollowMission;
		}
		set
		{
			inFollowMission = value;
		}
	}

	public bool CanCarrier
	{
		get
		{
			return bCanCarrier;
		}
		set
		{
			if (bCanCarrier != value)
			{
				bCanCarrier = value;
				if (value)
				{
					GetOnCarrier();
				}
			}
		}
	}

	public virtual bool Recruited
	{
		get
		{
			return bRecruited;
		}
		set
		{
			bRecruited = value;
		}
	}

	public AttackMode AttackMode
	{
		get
		{
			return attackMode;
		}
		set
		{
			attackMode = value;
		}
	}

	public int IntMissionFlag
	{
		get
		{
			return nMissionFlag;
		}
		set
		{
			nMissionFlag = value;
		}
	}

	public List<int> Abilities
	{
		get
		{
			return Npcskillcmpt.SkillAbilityIds;
		}
		set
		{
			Npcskillcmpt.SkillAbilityIds = value;
		}
	}

	public CustomCharactor.CustomData CustomizedData => _customData;

	public NpcAbilityCmpt Npcskillcmpt => m_Npcskillcmpt;

	public ItemCmpt ItemModule => m_ItemModule;

	public ItemCmpt ServantItemModule => m_ServantItemModule;

	protected override void OnPEInstantiate(uLink.NetworkMessageInfo info)
	{
		SetParent("NpcNetMgr");
		_objType = AiObjectType.AiObjectType_Npc;
		_id = info.networkView.initialData.Read<int>(new object[0]);
		m_scale = info.networkView.initialData.Read<float>(new object[0]);
		_externId = info.networkView.initialData.Read<int>(new object[0]);
		m_npcType = info.networkView.initialData.Read<int>(new object[0]);
		base.authId = info.networkView.initialData.Read<int>(new object[0]);
		isStand = info.networkView.initialData.Read<bool>(new object[0]);
		rotY = info.networkView.initialData.Read<float>(new object[0]);
		bForcedServant = info.networkView.initialData.Read<bool>(new object[0]);
		mCustomName = info.networkView.initialData.Read<string>(new object[0]);
		mProtoId = -1;
		_teamId = (originTeam = -1);
		spawnPos = base.transform.position;
		_worldId = info.networkView.group;
		Add(this);
		InitializeData();
		Player.OnHeartBeatTimeoutEvent += base.OnPlayerDisconnect;
		Player.PlayerDisconnected += OnPlayerDisconnect;
		StartCoroutine(AutoSave());
		AddSkEntity();
		base.gameObject.name = $"{NpcName}, TemplateId:{base.ExternId}, Id:{base.Id}";
		base.transform.rotation = Quaternion.Euler(0f, rotY, 0f);
	}

	public override void SkCreater()
	{
		if (_skEntity == null || _skEntity._attribs == null)
		{
			return;
		}
		if (ServerConfig.IsStory)
		{
			NpcMissionData missionData = NpcMissionDataRepository.GetMissionData(base.Id);
			if (missionData != null)
			{
				if (base.Id >= 9200)
				{
					SKAttribute.InitPlayerBaseAttrs(_skEntity._attribs, out _skEntity._baseAttribs);
				}
				else
				{
					SKAttribute.InitNpcBaseAttrs(_skEntity._attribs, base.ExternId, out _skEntity._baseAttribs);
				}
			}
			return;
		}
		if (ServerConfig.IsCustom)
		{
			SKAttribute.InitNpcBaseAttrs(_skEntity._attribs, base.ExternId, out _skEntity._baseAttribs);
		}
		else
		{
			SKAttribute.InitPlayerBaseAttrs(_skEntity._attribs, out _skEntity._baseAttribs);
		}
		AdNpcData adNpcData = NpcMissionDataRepository.GetAdNpcData(base.ExternId);
		if (adNpcData != null)
		{
			InitNpcData(adNpcData.mRnpc_ID);
			ResetDefaultPlayerId();
		}
	}

	protected override void OnPEStart()
	{
		base.OnPEStart();
		BindAction(EPacketType.PT_NPC_InitData, RPC_C2S_InitData);
		BindAction(EPacketType.PT_NPC_WorkState, RPC_C2S_SetWorkState);
		BindAction(EPacketType.PT_NPC_WorkItemFetch, RPC_C2S_CreateItemFetch);
		BindAction(EPacketType.PT_NPC_Move, RPC_C2S_NPCMove);
		BindAction(EPacketType.PT_NPC_ForceMove, RPC_S2C_ForceMove);
		BindAction(EPacketType.PT_NPC_RotY, RPC_C2S_NPCRotY);
		BindAction(EPacketType.PT_NPC_Equips, RPC_C2S_NpcEquips);
		BindAction(EPacketType.PT_NPC_Items, RPC_C2S_NpcItems);
		BindAction(EPacketType.PT_NPC_Destroy, RPC_C2S_Destroy);
		BindAction(EPacketType.PT_NPC_MissionState, RPC_C2S_MissionState);
		BindAction(EPacketType.PT_NPC_RequestAiOp, RPC_C2S_RequestAiOp);
		BindAction(EPacketType.PT_NPC_Mount, RPC_S2C_Mount);
		BindAction(EPacketType.PT_NPC_UpdateCampsite, RPC_S2C_UpdateCampsite);
		BindAction(EPacketType.PT_NPC_State, RPC_S2C_State);
		BindAction(EPacketType.PT_InGame_GetOnVehicle, RPC_C2S_GetOnVehicle);
		BindAction(EPacketType.PT_InGame_GetOffVehicle, RPC_C2S_GetOffVehicle);
		BindAction(EPacketType.PT_NPC_ExternData, RPC_C2S_ExternData);
		BindAction(EPacketType.PT_NPC_Skill, RPC_C2S_NpcSkill);
		BindAction(EPacketType.PT_NPC_SelfUseItem, RPC_C2S_SelfUseItem);
		BindAction(EPacketType.PT_CL_CLN_InitData, RPC_C2S_CLN_InitData);
		BindAction(EPacketType.PT_CL_CLN_SetState, RPC_C2S_CLN_SetState);
		BindAction(EPacketType.PT_CL_CLN_SetDwellingsID, RPC_C2S_CLN_SetDwellingsID);
		BindAction(EPacketType.PT_CL_CLN_SetWorkRoomID, RPC_C2S_CLN_SetWorkRoomID);
		BindAction(EPacketType.PT_CL_CLN_RemoveNpc, RPC_C2S_CLN_RemoveNpc);
		BindAction(EPacketType.PT_CL_CLN_SetOccupation, RPC_C2S_CLN_SetOccupation);
		BindAction(EPacketType.PT_CL_CLN_SetWorkMode, RPC_C2S_CLN_SetWorkMode);
		BindAction(EPacketType.PT_CL_CLN_PlantGetBack, RPC_C2S_CLN_PlantGetBack);
		BindAction(EPacketType.PT_CL_CLN_PlantPutOut, RPC_C2S_CLN_PlantPutOut);
		BindAction(EPacketType.PT_CL_CLN_PlantWater, RPC_C2S_CLN_PlantWater);
		BindAction(EPacketType.PT_CL_CLN_PlantClean, RPC_C2S_CLN_PlantClean);
		BindAction(EPacketType.PT_CL_CLN_PlantClear, RPC_C2S_CLN_PlantClear);
		BindAction(EPacketType.PT_CL_CLN_SetProcessingIndex, RPC_C2S_CLN_SetProcessingIndex);
		BindAction(EPacketType.PT_NPC_ResetPosition, base.RPC_C2S_ResetPosition);
		BindAction(EPacketType.PT_NPC_ForcedServant, RPC_C2S_ForcedServant);
		BindAction(EPacketType.PT_NPC_AddEnemyLock, RPC_C2S_AddEnemyLock);
		BindAction(EPacketType.PT_NPC_RemoveEnemyLock, RPC_C2S_RemoveEnemyLock);
		BindAction(EPacketType.PT_NPC_ClearEnemyLocked, RPC_C2S_ClearEnemyLocked);
		if (m_npcType == 1 && IsRandomNpc)
		{
			CheckValidDist(Player.ValidDistance, InvalidDistEvent, 30f);
		}
	}

	protected override void OnPEDestroy()
	{
		base.OnPEDestroy();
		Player.OnHeartBeatTimeoutEvent -= base.OnPlayerDisconnect;
		Player.PlayerDisconnected -= OnPlayerDisconnect;
		if (!AsyncSqlite.Disposed && !bDesTroy)
		{
			UpdateNpc();
			if (null != _skEntity)
			{
				_skEntity.SaveData();
			}
		}
	}

	protected override void InitializeData()
	{
		hasRecord = false;
		InitCmpt();
		Load();
		if (!hasRecord)
		{
			switch (ServerConfig.SceneMode)
			{
			case ESceneMode.Story:
				InitDefaultStoryNpc();
				break;
			case ESceneMode.Custom:
				InitDefaultCustomNpc();
				break;
			default:
				InitDefaultRandomNpc();
				break;
			}
		}
		Vector3 pos = base.transform.position;
		CommonHelper.AdjustPos(ref pos);
		base.transform.position = pos;
		if (!hasRecord)
		{
			Save();
		}
	}

	protected override void InitCmpt()
	{
		base.InitCmpt();
		m_ItemModule = (ItemCmpt)AddCmpt(ECmptType.Item);
		m_ServantItemModule = (ItemCmpt)AddCmpt(ECmptType.ServantItem);
		m_Npcskillcmpt = (NpcAbilityCmpt)AddCmpt(ECmptType.NpcSkillAbility);
		ItemModule.Extend(15);
		ServantItemModule.Extend(10);
	}

	private void InitDefaultStoryNpc()
	{
		base.sex = (PeSex)Random.Range(1, 3);
		_customData = CustomCharactor.CustomData.CreateCustomData(base.sex);
		int race = Random.Range(1, 5);
		NpcMissionData missionData = NpcMissionDataRepository.GetMissionData(base.Id);
		if (missionData == null)
		{
			if (LogFilter.logDebug)
			{
				Debug.LogErrorFormat("NPCID is error id:{0}", base.Id);
			}
			NetInterface.NetDestroy(this);
			return;
		}
		if (base.Id >= 9200)
		{
			GenerateRandomNpcData(base.sex, race);
			InitNpcEquipments(missionData.m_Rnpc_ID);
			InitPackage(missionData.m_Rnpc_ID);
			InitSkill(missionData.m_Rnpc_ID);
		}
		else if (base.Id > 9000 && base.Id < 9200)
		{
			GenerateStoryNpcData(missionData.m_Rnpc_ID);
			InitNpcEquipments(base.Id);
			InitPackage(base.Id);
			InitSkill(base.Id);
		}
		mProtoId = missionData.m_Rnpc_ID;
		_customData.charactorName = NpcName;
		mission = missionData;
	}

	private void InitDefaultCustomNpc()
	{
		NpcProtoDb.Item item = NpcProtoDb.Get(base.ExternId);
		if (item == null)
		{
			NetInterface.NetDestroy(this);
			if (LogFilter.logDebug)
			{
				Debug.LogErrorFormat("rand npc id:{0} does not exist.", base.ExternId);
			}
			return;
		}
		base.sex = item.sex;
		_customData = CustomCharactor.CustomData.CreateCustomData(base.sex);
		GenerateRandomNpcData(base.sex, -1);
		_customData.charactorName = NpcName;
		InitNpcEquipments(item.sort);
		InitPackage(item.sort);
		InitSkill(item.sort);
		NpcMissionData missionData = NpcMissionDataRepository.GetMissionData(item.sort);
		if (missionData != null)
		{
			mProtoId = missionData.m_Rnpc_ID;
			mission = missionData;
			NpcMissionDataRepository.AddMissionData(base.Id, mission);
		}
		else if (LogFilter.logDebug)
		{
			Debug.LogErrorFormat("NPC:{0} with id:{1} has no mission id:{2}", NpcName, base.ExternId, item.sort);
		}
	}

	private void InitDefaultRandomNpc()
	{
		base.sex = (PeSex)Random.Range(1, 3);
		_customData = CustomCharactor.CustomData.CreateCustomData(base.sex);
		int race = Random.Range(1, 5);
		GenerateRandomNpcData(base.sex, race);
		_customData.charactorName = NpcName;
		AdNpcData adNpcData = NpcMissionDataRepository.GetAdNpcData(base.ExternId);
		if (adNpcData == null)
		{
			NetInterface.NetDestroy(this);
			if (LogFilter.logDebug)
			{
				Debug.LogErrorFormat("rand npc id:{0} does not exist.", base.ExternId);
			}
			return;
		}
		InitNpcEquipments(adNpcData.mRnpc_ID);
		InitPackage(adNpcData.mRnpc_ID);
		InitSkill(adNpcData.mRnpc_ID);
		mProtoId = adNpcData.mRnpc_ID;
		mission = new NpcMissionData();
		mission.m_Rnpc_ID = adNpcData.mRnpc_ID;
		mission.m_QCID = adNpcData.mQC_ID;
		int randomMission = AdRMRepository.GetRandomMission(adNpcData.mQC_ID, mission.m_CurMissionGroup);
		if (randomMission != 0)
		{
			mission.m_RandomMission = randomMission;
		}
		for (int i = 0; i < adNpcData.m_CSRecruitMissionList.Count; i++)
		{
			mission.m_CSRecruitMissionList.Add(adNpcData.m_CSRecruitMissionList[i]);
		}
		NpcMissionDataRepository.AddMissionData(base.Id, mission);
	}

	private void InvalidDistEvent()
	{
		if (MissionManager.InvalidNpc(this) && !ColonyNpcMgr.IsColonyNpc(base.Id))
		{
			SPTerrainEvent.OnNpcDestroyed(this);
			NetInterface.NetDestroy(this);
			DeleteNpc(base.Id);
		}
	}

	private IEnumerator AutoSave()
	{
		while (true)
		{
			yield return new WaitForSeconds(300f);
			UpdateNpc();
			if (mAutoIncreaseMoney != null)
			{
				mAutoIncreaseMoney.Update();
				SyncMoney();
			}
			if (null != _skEntity)
			{
				_skEntity.SaveData();
			}
		}
	}

	private void UpdateNpc()
	{
		NpcData npcData = new NpcData();
		npcData.UpdateData(this);
		AsyncSqlite.AddRecord(npcData);
	}

	private void Save()
	{
		NpcData npcData = new NpcData();
		npcData.ExportData(this);
		AsyncSqlite.AddRecord(npcData);
	}

	public static void SaveAll()
	{
		foreach (KeyValuePair<int, ObjNetInterface> netObj in ObjNetInterface._netObjs)
		{
			if (netObj.Value is AiAdNpcNetwork)
			{
				AiAdNpcNetwork aiAdNpcNetwork = (AiAdNpcNetwork)netObj.Value;
				aiAdNpcNetwork.UpdateNpc();
				if (null != aiAdNpcNetwork._skEntity)
				{
					aiAdNpcNetwork._skEntity.SaveData();
				}
			}
		}
	}

	public void ExportSpawnInfo(BinaryWriter w)
	{
		BufferHelper.Serialize(w, base.ExternId);
		BufferHelper.Serialize(w, m_npcType);
		BufferHelper.Serialize(w, base.Scale);
		if (ServerConfig.IsStory)
		{
			BufferHelper.Serialize(w, base.transform.position);
		}
		else
		{
			BufferHelper.Serialize(w, base.SpawnPos);
		}
		BufferHelper.Serialize(w, bForcedServant);
		BufferHelper.Serialize(w, mCustomName);
	}

	public void ExportBaseInfo(BinaryWriter w)
	{
		BufferHelper.Serialize(w, mCharacterName.GivenName);
		BufferHelper.Serialize(w, mCharacterName.FamilyName);
		BufferHelper.Serialize(w, mAutoReviveTime);
		BufferHelper.Serialize(w, isStand);
		BufferHelper.Serialize(w, rotY);
		BufferHelper.Serialize(w, _missionState);
		Money.Export(w);
		BufferHelper.Serialize(w, mProtoId);
	}

	public void ImportBaseInfo(BinaryReader r)
	{
		string givenName = BufferHelper.ReadString(r);
		string familyName = BufferHelper.ReadString(r);
		mAutoReviveTime = BufferHelper.ReadInt32(r);
		isStand = BufferHelper.ReadBoolean(r);
		rotY = BufferHelper.ReadSingle(r);
		_missionState = BufferHelper.ReadInt32(r);
		Money.Import(r);
		mProtoId = BufferHelper.ReadInt32(r);
		mCharacterName = new CharacterName(givenName, familyName);
	}

	public void Load()
	{
		PEDbOp pEDbOp = AsyncSqlite.NewReadOp();
		if (pEDbOp != null)
		{
			pEDbOp.SetCmdText("SELECT ver,customdata,missiondata,externdata FROM npcdata WHERE id=@id;");
			pEDbOp.BindParam("@id", base.Id);
			pEDbOp.BindReaderHandler(LoadComplete);
			pEDbOp.Exec();
			pEDbOp = null;
		}
	}

	public void DeleteNpc(int npcId)
	{
		NpcData npcData = new NpcData();
		npcData.DeleteData(this);
		AsyncSqlite.AddRecord(npcData);
	}

	private void LoadComplete(SqliteDataReader reader)
	{
		if (reader.Read())
		{
			reader.GetInt32(reader.GetOrdinal("ver"));
			byte[] data = (byte[])reader.GetValue(reader.GetOrdinal("customdata"));
			_customData = new CustomCharactor.CustomData();
			_customData.Deserialize(data);
			byte[] buffer = (byte[])reader.GetValue(reader.GetOrdinal("missiondata"));
			mission = new NpcMissionData();
			mission.Deserialize(buffer);
			NpcMissionDataRepository.AddMissionData(base.Id, mission);
			byte[] buff = (byte[])reader.GetValue(reader.GetOrdinal("externdata"));
			Serialize.Import(buff, delegate(BinaryReader r)
			{
				ImportBaseInfo(r);
				ImportCmpt(r);
			});
			hasRecord = true;
		}
	}

	protected override void OnPlayerDisconnect(Player player)
	{
		if (base.authId == -1 || null == player)
		{
			return;
		}
		int id = player.Id;
		if (player.Equals(lordPlayer))
		{
			player.GetOffTrainWhenDisconnect(2, base.Id);
			DismissByPlayer();
			if (bForcedServant)
			{
				int teamId = player.TeamId;
				Player teamPlayer = Player.GetTeamPlayer(teamId, id);
				if (teamPlayer != null)
				{
					ForcedServant(teamPlayer);
				}
			}
		}
		base.OnPlayerDisconnect(player);
	}

	public void Update()
	{
		if (!ServerConfig.IsStory || base.Id != 9008)
		{
			return;
		}
		Player player = Player.GetPlayer(base.authId);
		if (!(player != null))
		{
			return;
		}
		PlayerMission curTeamMission = MissionManager.Manager.GetCurTeamMission(player.Id);
		if (curTeamMission != null && curTeamMission.HadCompleteMission(18, player) && !curTeamMission.HadCompleteMission(27, player))
		{
			bool flag = false;
			if (base.transform.rotation.eulerAngles.y != 270f)
			{
				base.transform.rotation = Quaternion.Euler(0f, 270f, 0f);
				rotY = 270f;
				flag = true;
			}
			if (Vector3.Distance(base.transform.position, gerdyPutDownPos) > 0.2f)
			{
				base.transform.position = gerdyPutDownPos;
				flag = true;
			}
			if (flag)
			{
				RPCOthers(EPacketType.PT_NPC_ForceMove, base.transform.position, (byte)0, (int)rotY, GameTime.Timer.Second);
			}
		}
	}

	public void ResetDefaultPlayerId()
	{
		int num = ((!(null == lordPlayer)) ? lordPlayer.Id : (ServerConfig.IsCooperation ? 1 : 2));
		_skEntity.SetAllAttribute(AttribType.DefaultPlayerID, num);
	}

	internal override void OnDamage(int casterId, float damage)
	{
		if (casterId != 0)
		{
			ObjNetInterface objNetInterface = ObjNetInterface.Get(casterId);
			if (!(objNetInterface == null) && objNetInterface is SkNetworkInterface)
			{
				base.OnDamage(casterId, damage);
				IncreaseHatred(objNetInterface.gameObject, (int)damage);
			}
		}
	}

	internal override void OnDeath(int casterId = 0)
	{
		base.OnDeath(casterId);
		if (LogFilter.logDebug)
		{
			Debug.LogWarningFormat("Npc:[{0}] die", NpcName);
		}
		DeadStartTime = (float)GameTime.PlayTime.Second;
		_bDeath = true;
		if (IntMissionFlag == MISSIONFlag.Mission_Follow || MISSIONFlag.Mission_Dif == IntMissionFlag)
		{
			SetMissionFlag(MISSIONFlag.Mission_No, -1, -1, -1, 0);
		}
		NpcAutoRevive();
	}

	public void NpcAutoRevive()
	{
		if (!base.IsDead || (Recruited && !bForcedServant))
		{
			return;
		}
		if (ServerConfig.IsStory)
		{
			NpcMissionData missionData = NpcMissionDataRepository.GetMissionData(base.Id);
			if (missionData == null)
			{
				return;
			}
			if (missionData.m_Rnpc_ID != -1 && base.Id >= 9200)
			{
				RandomNpcDb.Item item = RandomNpcDb.Get(missionData.m_Rnpc_ID);
				if (item != null)
				{
					if (item.reviveTime == -1)
					{
						DeleteNpc(base.Id);
					}
					else
					{
						Invoke("NpcRevive", item.reviveTime);
					}
				}
				return;
			}
			StoryNpc npc = StoryNpcMgr.GetNpc(base.Id);
			if (npc != null)
			{
				if (npc._revive == -1)
				{
					DeleteNpc(base.Id);
				}
				else
				{
					Invoke("NpcRevive", npc._revive);
				}
			}
		}
		else
		{
			AdNpcData adNpcData = NpcMissionDataRepository.GetAdNpcData(base.ExternId);
			if (adNpcData != null)
			{
				RandomNpcDb.Item item2 = RandomNpcDb.Get(adNpcData.mRnpc_ID);
				Invoke("NpcRevive", item2.reviveTime);
			}
		}
	}

	protected override IEnumerator DestroyAiObjectCoroutine()
	{
		yield break;
	}

	public override void DropItem(NetInterface caster)
	{
		if (ServantItemModule != null)
		{
		}
	}

	public void SyncWorkState(uLink.NetworkMessageInfo info, Vector3 pos, int type)
	{
		switch (type)
		{
		case 0:
			base.transform.position = pos;
			RPCProxy(EPacketType.PT_NPC_WorkState, workState, pos, base.transform.rotation.eulerAngles.y);
			break;
		case 1:
			RPCProxy(EPacketType.PT_NPC_WorkState, workState, pos, base.transform.rotation.eulerAngles.y);
			break;
		}
	}

	public void SyncVehicleStatus(uLink.NetworkPlayer peer)
	{
		if (base._OnCar && null != base._Creation && base._SeatIndex != -2)
		{
			RPCPeer(peer, EPacketType.PT_InGame_GetOnVehicle, base._Creation.Id, base._SeatIndex);
		}
	}

	internal void RecruitByPlayer(Player player, bool force = false)
	{
		if (null != lordPlayer || null == player)
		{
			return;
		}
		if (force)
		{
			if (!player.AddForceServant(this))
			{
				return;
			}
		}
		else if (!player.AddServant(this))
		{
			if (LogFilter.logDebug)
			{
				Debug.LogWarningFormat("Player[{0}] has too many servants.", player.roleName);
			}
			return;
		}
		Recruited = true;
		lordPlayer = player;
		base.authId = player.Id;
		_teamId = player.TeamId;
		if (!force)
		{
			spawnPos = base.transform.position;
		}
		RPCProxy(EPacketType.PT_NPC_Recruit, base.authId, base.TeamId, force);
		ResetDefaultPlayerId();
	}

	internal void DismissByPlayer()
	{
		if (null != lordPlayer)
		{
			if (bForcedServant)
			{
				spawnPos = lordPlayer.transform.position;
			}
			else
			{
				base.transform.position = base.SpawnPos;
			}
			lordPlayer.DismissServant(this);
		}
		Recruited = false;
		lordPlayer = null;
		if (!ColonyNpcMgr.IsColonyNpc(base.Id))
		{
			_teamId = originTeam;
		}
		if (base.IsDead)
		{
			NpcRevive();
		}
		RPCProxy(EPacketType.PT_NPC_Dismiss, base.SpawnPos, base.TeamId);
		ResetDefaultPlayerId();
	}

	public void ForcedServant(Player p)
	{
		if (null == lordPlayer && p != null)
		{
			base.transform.position = p.transform.position;
			RPCOthers(EPacketType.PT_NPC_ResetPosition, base.transform.position);
			RecruitByPlayer(p, force: true);
			RPCOthers(EPacketType.PT_NPC_ForcedServant, true, p.Id);
		}
	}

	public void RemoveForcedServant(Player p)
	{
		if (null != lordPlayer && p != null)
		{
			DismissByPlayer();
			RPCOthers(EPacketType.PT_NPC_ForcedServant, false, p.Id);
		}
	}

	public void GetOnVehicle(CreationNetwork creation, int index)
	{
		if (!(null == creation) && creation.GetOnSide(this, ref index) != 0)
		{
			base._OnCar = true;
			base._SeatIndex = index;
			base._Creation = creation;
			RPCOthers(EPacketType.PT_InGame_GetOnVehicle, creation.Id, index);
		}
	}

	public void GetOffVehicle(Vector3 outPos)
	{
		if (base._OnCar)
		{
			base.transform.position = outPos;
			if (null != base._Creation)
			{
				RPCOthers(EPacketType.PT_InGame_GetOffVehicle, outPos);
				base._Creation.GetOff(this);
			}
			base._OnCar = false;
			base._SeatIndex = -2;
			base._Creation = null;
		}
	}

	public void TransToSpawn()
	{
		base.transform.position = spawnPos;
		mAiSynAttribute.mv3Postion = base.transform.position;
		RPCOthers(EPacketType.PT_NPC_RequestAiOp, EReqType.Translate, base.transform.position);
	}

	internal int[] GetItemIDs(int tabIndex)
	{
		if (tabIndex == 0)
		{
			return ItemModule.GetItemIds().ToArray();
		}
		return ServantItemModule.GetItemIds().ToArray();
	}

	internal int[] GetEquipIDs()
	{
		return base.EquipModule.EquipIds;
	}

	protected void InitNpcEquipments(int randId)
	{
		if (base.Id > 9000 && base.Id < 9200)
		{
			StoryNpc npc = StoryNpcMgr.GetNpc(randId);
			if (npc != null)
			{
				InitEquipments(npc._equipments);
				InitAutoIncreaseMoney(npc.npcMoney.max, npc.npcMoney.incValue.Random());
				Money.Current = npc.npcMoney.initValue.Random();
			}
		}
		else
		{
			RandomNpcDb.Item item = RandomNpcDb.Get(randId);
			if (item != null)
			{
				InitEquipments(item.initEquipment);
				InitAutoIncreaseMoney(item.npcMoney.max, item.npcMoney.incValue.Random());
				Money.Current = item.npcMoney.initValue.Random();
			}
		}
	}

	public void InitPackage(int randId)
	{
		if (base.Id > 9000 && base.Id < 9200)
		{
			StoryNpc npc = StoryNpcMgr.GetNpc(randId);
			if (npc == null || npc._items == null || npc._items.Count <= 0 || ServantItemModule == null)
			{
				return;
			}
			{
				foreach (StoryNpc.NpcHadItems item2 in npc._items)
				{
					ItemObject itemObject = ItemManager.CreateItem(item2._itemId, item2._itemNum);
					if (itemObject != null)
					{
						ServantItemModule.AddItem(itemObject);
					}
				}
				return;
			}
		}
		RandomNpcDb.Item item = RandomNpcDb.Get(randId);
		if (item == null || ServantItemModule == null || item.initItems == null || item.initItems.Count == 0)
		{
			return;
		}
		foreach (RandomNpcDb.ItemcoutDb initItem in item.initItems)
		{
			List<ItemObject> effItems = new List<ItemObject>(item.initItems.Count);
			ServantItemModule.AddItem(initItem.protoId, initItem.count, ref effItems);
		}
	}

	public void InitSkill(int randId)
	{
		if (base.Id > 9000 && base.Id < 9200)
		{
			StoryNpc npc = StoryNpcMgr.GetNpc(randId);
			if (npc != null && npc._npcSkillIds != null && npc._npcSkillIds.Length != 0)
			{
				int[] npcSkillIds = npc._npcSkillIds;
				foreach (int id in npcSkillIds)
				{
					AddAbility(id);
				}
			}
			return;
		}
		RandomNpcDb.Item item = RandomNpcDb.Get(randId);
		if (item == null || item.mSkillRandom == null)
		{
			return;
		}
		int[] skill = item.mSkillRandom.GetSkill();
		if (skill != null && skill.Length != 0)
		{
			int[] array = skill;
			foreach (int id2 in array)
			{
				AddAbility(id2);
			}
		}
	}

	public void SyncPackageIndex(int tabIndex)
	{
		int[] itemIDs = GetItemIDs(tabIndex);
		int num = itemIDs.Length;
		if (num == 0)
		{
			RPCProxy(EPacketType.PT_NPC_PacageIndex, tabIndex, num);
		}
		else
		{
			RPCProxy(EPacketType.PT_NPC_PacageIndex, tabIndex, num, itemIDs.ToArray());
		}
	}

	public void SyncPutOnEquip(ItemObject equip)
	{
		RPCProxy(EPacketType.PT_InGame_PutOnEquipment, equip);
	}

	public void SyncTakeOffEquip(int objId)
	{
		RPCProxy(EPacketType.PT_InGame_TakeOffEquipment, objId);
	}

	public void SyncMoney()
	{
		RPCProxy(EPacketType.PT_NPC_Money, Money.Current);
	}

	public void SyncTeamId()
	{
		RPCProxy(EPacketType.PT_NPC_SyncTeamID, base.TeamId);
	}

	private void RPC_C2S_InitData(uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
		byte[] array = _customData.Serialize();
		byte[] array2 = mission.Serialize();
		RPCPeer(info.sender, EPacketType.PT_NPC_InitData, base.transform.position, base.transform.rotation, array, array2, base.IsDead, base.authId, _missionState, _boolName, _boolValue, bForcedServant);
		InitClnNpcData(base.TeamId);
		if (bForcedServant && null == lordPlayer)
		{
			ForcedServant(Player.GetPlayer(info.sender));
		}
		SyncVehicleStatus(info.sender);
		if (_mountId != 0)
		{
			RPCOthers(EPacketType.PT_NPC_Mount, _mountId);
		}
		SyncScenarioId(info.sender);
	}

	protected override void RPC_C2S_SetController(uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
		if (null != lordPlayer)
		{
			base.authId = lordPlayer.Id;
			RPCPeer(info.sender, EPacketType.PT_NPC_Recruit, lordPlayer.Id, lordPlayer.TeamId, bForcedServant);
		}
		else
		{
			base.RPC_C2S_SetController(stream, info);
		}
	}

	public static void RPC_C2S_NpcLoseItem(uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
		int num = stream.Read<int>(new object[0]);
		int id = stream.Read<int>(new object[0]);
		int id2 = stream.Read<int>(new object[0]);
		stream.Read<int>(new object[0]);
		Player player = Player.GetPlayer(info.sender);
		if (null == player)
		{
			return;
		}
		AiAdNpcNetwork aiAdNpcNetwork = ObjNetInterface.Get<AiAdNpcNetwork>(id);
		if (null == aiAdNpcNetwork)
		{
			return;
		}
		ItemCmpt itemCmpt = ((num != 0) ? aiAdNpcNetwork.ServantItemModule : aiAdNpcNetwork.ItemModule);
		ItemObject itemObject = itemCmpt[id2];
		if (itemObject == null)
		{
			if (LogFilter.logDebug)
			{
				Debug.LogWarning("Invalid item.");
			}
		}
		else if (player.Package.GetEmptyGridCount(itemObject.protoData) <= 0)
		{
			if (LogFilter.logDebug)
			{
				Debug.LogWarningFormat("player[{0}] has not enough space.", player.roleName);
			}
		}
		else
		{
			player.Package.AddItem(itemObject);
			player.SyncPackageIndex();
			itemCmpt.DelItem(id2);
			aiAdNpcNetwork.SyncPackageIndex(num);
		}
	}

	public static void RPC_C2S_NpcPackageSort(uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
		int id = stream.Read<int>(new object[0]);
		int num = stream.Read<int>(new object[0]);
		AiAdNpcNetwork aiAdNpcNetwork = ObjNetInterface.Get<AiAdNpcNetwork>(id);
		if (!(null == aiAdNpcNetwork))
		{
			ItemCmpt itemCmpt = ((num != 0) ? aiAdNpcNetwork.ServantItemModule : aiAdNpcNetwork.ItemModule);
			itemCmpt.Resort();
			aiAdNpcNetwork.RPCPeer(info.sender, EPacketType.PT_NPC_Items, num, itemCmpt.ItemCount, itemCmpt.GetItems(), false);
		}
	}

	public static void RPC_C2S_NpcLoseAllItems(uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
		int id = stream.Read<int>(new object[0]);
		int num = stream.Read<int>(new object[0]);
		Player player = Player.GetPlayer(info.sender);
		if (null == player)
		{
			return;
		}
		AiAdNpcNetwork aiAdNpcNetwork = ObjNetInterface.Get<AiAdNpcNetwork>(id);
		if (!(null == aiAdNpcNetwork))
		{
			ItemCmpt itemCmpt = ((num != 0) ? aiAdNpcNetwork.ServantItemModule : aiAdNpcNetwork.ItemModule);
			ItemObject[] items = itemCmpt.GetItems();
			if (player.Package.CanAdd(items))
			{
				player.Package.AddItemList(items);
				player.SyncPackageIndex();
				itemCmpt.Clear();
				aiAdNpcNetwork.SyncPackageIndex(num);
			}
		}
	}

	public static void RPC_C2S_NpcGetItem(uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
		int num = stream.Read<int>(new object[0]);
		int num2 = stream.Read<int>(new object[0]);
		int num3 = stream.Read<int>(new object[0]);
		int num4 = stream.Read<int>(new object[0]);
		Player player = Player.GetPlayer(info.sender);
		if (null == player)
		{
			return;
		}
		AiAdNpcNetwork aiAdNpcNetwork = ObjNetInterface.Get<AiAdNpcNetwork>(num2);
		if (null == aiAdNpcNetwork)
		{
			return;
		}
		ItemPlaceType itemPlaceType = (ItemPlaceType)num;
		switch ((ItemPlaceType)num4)
		{
		case ItemPlaceType.IPT_Bag:
		{
			ItemObject itemById = player.Package.GetItemById(num3);
			if (itemById == null)
			{
				if (LogFilter.logDebug)
				{
					Debug.LogWarningFormat("Item not in player[{0}] package.", player.roleName);
				}
				break;
			}
			ItemCmpt itemCmpt2 = null;
			int tabIndex2 = 0;
			switch (itemPlaceType)
			{
			case ItemPlaceType.IPT_ServantInteraction:
			case ItemPlaceType.IPT_ColonyServantInteractionPersonel:
				itemCmpt2 = aiAdNpcNetwork.ItemModule;
				tabIndex2 = 0;
				break;
			case ItemPlaceType.IPT_ServantInteraction2:
			case ItemPlaceType.IPT_ColonyServantInteraction2Personel:
				itemCmpt2 = aiAdNpcNetwork.ServantItemModule;
				tabIndex2 = 1;
				break;
			}
			if (!itemCmpt2.CanAdd(1))
			{
				if (LogFilter.logDebug)
				{
					Debug.LogWarningFormat("Npc[{0}] has not enough space in package.", num2);
				}
			}
			else
			{
				player.Package.RemoveItem(itemById);
				player.SyncPackageIndex();
				itemCmpt2.AddItem(itemById);
				aiAdNpcNetwork.SyncPackageIndex(tabIndex2);
			}
			break;
		}
		case ItemPlaceType.IPT_ServantInteraction:
		case ItemPlaceType.IPT_ColonyServantInteractionPersonel:
		{
			if (itemPlaceType == ItemPlaceType.IPT_ServantInteraction || itemPlaceType == ItemPlaceType.IPT_ColonyServantInteractionPersonel)
			{
				break;
			}
			ItemObject itemObject3 = aiAdNpcNetwork.ItemModule[num3];
			if (itemObject3 == null)
			{
				if (LogFilter.logDebug)
				{
					Debug.LogWarningFormat("Item not in npc[{0}] package.", num2);
				}
			}
			else if (!aiAdNpcNetwork.ServantItemModule.CanAdd(1))
			{
				if (LogFilter.logDebug)
				{
					Debug.LogWarningFormat("Npc[{0}] has not enough space in package.", num2);
				}
			}
			else
			{
				aiAdNpcNetwork.ItemModule.DelItem(num3);
				aiAdNpcNetwork.ServantItemModule.AddItem(itemObject3);
				aiAdNpcNetwork.SyncPackageIndex(0);
				aiAdNpcNetwork.SyncPackageIndex(1);
			}
			break;
		}
		case ItemPlaceType.IPT_ServantInteraction2:
		case ItemPlaceType.IPT_ColonyServantInteraction2Personel:
		{
			if (itemPlaceType == ItemPlaceType.IPT_ServantInteraction2 || itemPlaceType == ItemPlaceType.IPT_ColonyServantInteraction2Personel)
			{
				break;
			}
			ItemObject itemObject2 = aiAdNpcNetwork.ServantItemModule[num3];
			if (itemObject2 == null)
			{
				if (LogFilter.logDebug)
				{
					Debug.LogWarningFormat("Item not in npc[{0}] package.", num2);
				}
			}
			else if (!aiAdNpcNetwork.ItemModule.CanAdd(1))
			{
				if (LogFilter.logDebug)
				{
					Debug.LogWarningFormat("Npc[{0}] has not enough space in package.", num2);
				}
			}
			else
			{
				aiAdNpcNetwork.ServantItemModule.DelItem(num3);
				aiAdNpcNetwork.ItemModule.AddItem(itemObject2);
				aiAdNpcNetwork.SyncPackageIndex(0);
				aiAdNpcNetwork.SyncPackageIndex(1);
			}
			break;
		}
		case ItemPlaceType.IPT_ServantEqu:
		case ItemPlaceType.IPT_ConolyServantEquPersonel:
		{
			ItemObject itemObject = aiAdNpcNetwork.EquipModule[num3];
			if (itemObject == null)
			{
				if (LogFilter.logDebug)
				{
					Debug.LogWarningFormat("Item not in npc[{0}] package.", num2);
				}
				break;
			}
			ItemCmpt itemCmpt = null;
			int tabIndex = 0;
			switch (itemPlaceType)
			{
			case ItemPlaceType.IPT_ServantInteraction:
			case ItemPlaceType.IPT_ColonyServantInteractionPersonel:
				itemCmpt = aiAdNpcNetwork.ItemModule;
				tabIndex = 0;
				break;
			case ItemPlaceType.IPT_ServantInteraction2:
			case ItemPlaceType.IPT_ColonyServantInteraction2Personel:
				itemCmpt = aiAdNpcNetwork.ServantItemModule;
				tabIndex = 1;
				break;
			}
			int num5 = aiAdNpcNetwork.EquipModule.FindEffectEquipCount(itemObject);
			if (!itemCmpt.CanAdd(num5))
			{
				if (LogFilter.logDebug)
				{
					Debug.LogWarningFormat("Npc[{0}] has not enough space in package.", num2);
				}
				break;
			}
			List<ItemObject> effEquips = new List<ItemObject>();
			if (aiAdNpcNetwork.EquipModule.TakeOffEquip(itemObject, ref effEquips))
			{
				itemCmpt.AddItem(effEquips);
				aiAdNpcNetwork.SyncTakeOffEquip(itemObject.instanceId);
				aiAdNpcNetwork.SyncPackageIndex(tabIndex);
			}
			break;
		}
		case ItemPlaceType.IPT_HotKeyBar:
		case ItemPlaceType.IPT_Equipment:
		case ItemPlaceType.IPT_ServantSkill:
			break;
		}
	}

	private void RPC_C2S_SetWorkState(uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
		int num = stream.Read<int>(new object[0]);
		Vector3 vector = stream.Read<Vector3>(new object[0]);
		float y = stream.Read<float>(new object[0]);
		workState = num;
		vector.y += 1f;
		base.transform.position = vector;
		base.transform.rotation = Quaternion.Euler(0f, y, 0f);
		SyncWorkState(info, vector, 0);
	}

	private void RPC_C2S_CreateItemFetch(uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
		if (!Recruited)
		{
			return;
		}
		workState = 0;
		if (Abilities.Count <= 0)
		{
			if (LogFilter.logDebug)
			{
				Debug.LogWarning(" random npc not randskill ! ID : " + base.ExternId);
			}
			return;
		}
		Vector3 vector = stream.Read<Vector3>(new object[0]);
		int num = stream.Read<int>(new object[0]);
		if (!((float)num / 3600f <= 0f))
		{
			workRestTime = GameTime.PlayTime.Second;
			Vector3 vector2 = Random.onUnitSphere * 3f;
			Vector3 vector3 = new Vector3(vector2.x, 0f, vector2.y);
			Vector3 pos = vector + vector3;
			NetInterface.Instantiate(PrefabManager.Self.ItemFetchNetworkSeed, pos, Quaternion.identity, info.networkView.group, Abilities.ToArray(), num);
		}
	}

	private void RPC_C2S_NPCMove(uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
		if (base.authId == Player.GetPlayerId(info.sender))
		{
			Vector3 vector = stream.Read<Vector3>(new object[0]);
			byte b = stream.Read<byte>(new object[0]);
			int num = stream.Read<int>(new object[0]);
			double num2 = stream.Read<double>(new object[0]);
			base.transform.position = vector;
			mAiSynAttribute.mv3Postion = vector;
			mAiSynAttribute.rotEuler = num;
			Vector3 euler = PEUtil.UncompressEulerAngle(num);
			base.transform.rotation = Quaternion.Euler(euler);
			URPCOthers(EPacketType.PT_NPC_Move, vector, b, num, num2);
		}
	}

	private void RPC_S2C_ForceMove(uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
	}

	private void RPC_C2S_NPCRotY(uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
		int num = stream.Read<int>(new object[0]);
		mAiSynAttribute.rotEuler = num;
		Vector3 euler = PEUtil.UncompressEulerAngle(num);
		base.transform.rotation = Quaternion.Euler(euler);
		URPCOthers(EPacketType.PT_NPC_RotY, num);
	}

	protected override void RPC_C2S_ExternData(uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
		byte[] array = Serialize.Export(delegate(BinaryWriter w)
		{
			ExportBaseInfo(w);
		});
		RPCPeer(info.sender, EPacketType.PT_NPC_ExternData, array);
	}

	private void RPC_C2S_NpcSkill(uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
		int[] array = Abilities.ToArray();
		if (array.Length == 0)
		{
			RPCPeer(info.sender, EPacketType.PT_NPC_Skill, 0);
		}
		else
		{
			RPCPeer(info.sender, EPacketType.PT_NPC_Skill, array.Length, array);
		}
	}

	private void RPC_C2S_SelfUseItem(uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
		int num = stream.Read<int>(new object[0]);
		ItemObject itemByID = ItemManager.GetItemByID(num);
		if (itemByID == null)
		{
			if (LogFilter.logDebug)
			{
				Debug.LogWarning("Invalid item.");
			}
			return;
		}
		ItemCmpt itemCmpt = ItemModule;
		int num2 = 0;
		if (!itemCmpt.HasItem(itemByID))
		{
			itemCmpt = ServantItemModule;
			num2 = 1;
			if (!itemCmpt.HasItem(itemByID))
			{
				itemCmpt = null;
			}
		}
		if (itemCmpt == null)
		{
			return;
		}
		Consume cmpt = itemByID.GetCmpt<Consume>();
		if (cmpt != null)
		{
			if (!ServerConfig.UnlimitedRes)
			{
				if (!itemByID.CountDown(1))
				{
					itemCmpt.DelItem(num);
					SyncPackageIndex(num2);
				}
				else
				{
					RPCPeer(info.sender, EPacketType.PT_NPC_Items, num2, itemCmpt.ItemCount, itemCmpt.GetItems(), false);
				}
			}
			RPCOthers(EPacketType.PT_NPC_SelfUseItem, num);
		}
		Equip cmpt2 = itemByID.GetCmpt<Equip>();
		if (cmpt2 == null)
		{
			return;
		}
		int num3 = base.EquipModule.FindEffectEquipCount(itemByID.protoData.equipPos);
		if (itemCmpt.ItemCount + num3 > itemCmpt.Capacity)
		{
			if (LogFilter.logDebug)
			{
				Debug.LogWarning("Not enough space in npc package.");
			}
			return;
		}
		int num4 = itemCmpt.ItemIndex(num);
		if (num4 == -1)
		{
			if (LogFilter.logDebug)
			{
				Debug.LogWarningFormat("Npc does not have this item[{0}].", num);
			}
			return;
		}
		List<ItemObject> effEquips = new List<ItemObject>();
		if (base.EquipModule.PutOnEquip(itemByID, ref effEquips))
		{
			itemCmpt.DelItem(num);
			itemCmpt.AddItem(effEquips);
			SyncPackageIndex(num2);
			SyncPutOnEquip(itemByID);
		}
	}

	private void RPC_C2S_NpcEquips(uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
		if (base.EquipModule.EquipCount <= 0)
		{
			RPCPeer(info.sender, EPacketType.PT_NPC_Equips, 0);
		}
		else
		{
			RPCPeer(info.sender, EPacketType.PT_NPC_Equips, base.EquipModule.EquipCount, base.EquipModule.EquipItems, false);
		}
	}

	private void RPC_C2S_NpcItems(uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
		if (ItemModule == null || ItemModule.ItemCount <= 0)
		{
			RPCPeer(info.sender, EPacketType.PT_NPC_Items, 0, 0);
		}
		else
		{
			RPCPeer(info.sender, EPacketType.PT_NPC_Items, 0, ItemModule.ItemCount, ItemModule.GetItems(), false);
		}
		if (ServantItemModule == null || ServantItemModule.ItemCount <= 0)
		{
			RPCPeer(info.sender, EPacketType.PT_NPC_Items, 1, 0);
		}
		else
		{
			RPCPeer(info.sender, EPacketType.PT_NPC_Items, 1, ServantItemModule.ItemCount, ServantItemModule.GetItems(), false);
		}
	}

	private void RPC_C2S_Destroy(uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
		if (!bDesTroy)
		{
			bDesTroy = true;
			DeleteNpc(base.Id);
			StartCoroutine(DestroyCoroutine());
		}
	}

	private void RPC_C2S_MissionState(uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
		_missionState = stream.Read<int>(new object[0]);
	}

	private void RPC_C2S_RequestAiOp(uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
		if (Player.GetPlayer(info.sender) == null)
		{
			return;
		}
		int num = stream.Read<int>(new object[0]);
		if (base.authId != -1 && ObjNetInterface.Get(base.authId) != null)
		{
			Player player = ObjNetInterface.Get(base.authId) as Player;
			switch ((EReqType)num)
			{
			case EReqType.Animation:
				RPCPeer(player.OwnerView.owner, EPacketType.PT_NPC_RequestAiOp, num, stream.Read<string>(new object[0]), stream.Read<float>(new object[0]), stream.Read<bool>(new object[0]));
				break;
			case EReqType.FollowPath:
				RPCOthers(EPacketType.PT_NPC_RequestAiOp, num, stream.Read<Vector3[]>(new object[0]), stream.Read<bool>(new object[0]));
				break;
			case EReqType.FollowTarget:
			{
				if (ServerConfig.IsSurvive)
				{
					player = Player.GetPlayer(info.sender);
					ForceGetController(player.Id);
				}
				else if (Player.GetNearestPlayer(player.TeamId, base.transform.position) != player)
				{
					player = Player.GetNearestPlayer(player.TeamId, base.transform.position);
					ForceGetController(player.Id);
				}
				int num5 = stream.Read<int>(new object[0]);
				Player player2 = ObjNetInterface.Get(num5) as Player;
				if (player2 != null)
				{
					num5 = player.Id;
				}
				RPCPeer(player.OwnerView.owner, EPacketType.PT_NPC_RequestAiOp, num, num5, stream.Read<Vector3>(new object[0]), stream.Read<int>(new object[0]), stream.Read<float>(new object[0]));
				break;
			}
			case EReqType.MoveToPoint:
			case EReqType.TalkMove:
				if (base.authId == Player.GetPlayerId(info.sender))
				{
					RPCOthers(EPacketType.PT_NPC_RequestAiOp, num, stream.Read<Vector3>(new object[0]), stream.Read<float>(new object[0]), stream.Read<bool>(new object[0]), stream.Read<int>(new object[0]));
				}
				break;
			case EReqType.Salvation:
			{
				int num4 = stream.Read<int>(new object[0]);
				bool flag = stream.Read<bool>(new object[0]);
				ObjNetInterface objNetInterface = ObjNetInterface.Get(num4);
				if (!(objNetInterface as AiAdNpcNetwork == null) && (objNetInterface as AiAdNpcNetwork).authId != base.authId)
				{
					if (Player.GetPlayerId(info.sender) != base.authId)
					{
						ForceGetController(Player.GetPlayerId(info.sender));
					}
					if ((objNetInterface as AiAdNpcNetwork).authId == base.authId || (objNetInterface as AiAdNpcNetwork).ForceGetController(Player.GetPlayerId(info.sender)))
					{
						RPCPeer(player.OwnerView.owner, EPacketType.PT_NPC_RequestAiOp, num, num4, flag);
					}
				}
				break;
			}
			case EReqType.Translate:
				base.transform.position = stream.Read<Vector3>(new object[0]);
				mAiSynAttribute.mv3Postion = base.transform.position;
				RPCOthers(EPacketType.PT_NPC_RequestAiOp, num, base.transform.position, stream.Read<bool>(new object[0]));
				base.authId = -1;
				if (stream.Read<bool>(new object[0]))
				{
					RPCProxy(EPacketType.PT_InGame_LostController);
				}
				break;
			case EReqType.UseSkill:
				RPCPeer(player.OwnerView.owner, EPacketType.PT_NPC_RequestAiOp, num);
				break;
			case EReqType.Hand:
			{
				int num3 = stream.Read<int>(new object[0]);
				if (num3 != player.Id)
				{
					ForceGetController(num3);
				}
				break;
			}
			case EReqType.Remove:
			{
				int num2 = stream.Read<int>(new object[0]);
				switch (num2)
				{
				case 4:
					RPCOthers(EPacketType.PT_NPC_RequestAiOp, num, num2);
					break;
				case 3:
					RPCOthers(EPacketType.PT_NPC_RequestAiOp, num, num2, stream.Read<Vector3[]>(new object[0]));
					break;
				}
				break;
			}
			case EReqType.Dialogue:
			case EReqType.Rotate:
			case EReqType.Attack:
			case EReqType.PauseAll:
				break;
			}
			return;
		}
		Player player3 = Player.GetPlayer(info.sender);
		switch ((EReqType)num)
		{
		case EReqType.Animation:
			ForceGetController(Player.GetPlayerId(info.sender));
			RPCPeer(info.sender, EPacketType.PT_NPC_RequestAiOp, num, stream.Read<string>(new object[0]), stream.Read<float>(new object[0]), stream.Read<bool>(new object[0]));
			break;
		case EReqType.FollowPath:
			ForceGetController(Player.GetPlayerId(info.sender));
			RPCPeer(info.sender, EPacketType.PT_NPC_RequestAiOp, num, stream.Read<Vector3[]>(new object[0]), stream.Read<bool>(new object[0]));
			break;
		case EReqType.FollowTarget:
			if (!ServerConfig.IsSurvive && Player.GetNearestPlayer(player3.TeamId, base.transform.position) != player3)
			{
				player3 = Player.GetNearestPlayer(player3.TeamId, base.transform.position);
			}
			base.transform.position = player3.transform.position;
			mAiSynAttribute.mv3Postion = base.transform.position;
			RPCOthers(EPacketType.PT_NPC_Move, base.transform.position, (byte)0, 0, GameTime.Timer.Second);
			ForceGetController(player3.Id);
			RPCPeer(info.sender, EPacketType.PT_NPC_RequestAiOp, num, stream.Read<int>(new object[0]), stream.Read<Vector3>(new object[0]), stream.Read<int>(new object[0]), stream.Read<float>(new object[0]));
			break;
		case EReqType.MoveToPoint:
		case EReqType.TalkMove:
			base.transform.position = stream.Read<Vector3>(new object[0]);
			mAiSynAttribute.mv3Postion = base.transform.position;
			RPCOthers(EPacketType.PT_NPC_RequestAiOp, EReqType.Translate, base.transform.position, true);
			break;
		case EReqType.Rotate:
			ForceGetController(Player.GetPlayerId(info.sender));
			RPCPeer(info.sender, EPacketType.PT_NPC_RequestAiOp, num, stream.Read<Quaternion>(new object[0]));
			break;
		case EReqType.Salvation:
		{
			ForceGetController(Player.GetPlayerId(info.sender));
			int num6 = stream.Read<int>(new object[0]);
			ObjNetInterface objNetInterface2 = ObjNetInterface.Get(num6);
			if (!(objNetInterface2 is AiAdNpcNetwork))
			{
				break;
			}
			if ((objNetInterface2 as AiAdNpcNetwork).authId != -1)
			{
				Player player4 = ObjNetInterface.Get((objNetInterface2 as AiAdNpcNetwork).authId) as Player;
				if (player4 != null)
				{
					ForceGetController(player4.Id);
				}
			}
			else
			{
				(objNetInterface2 as AiAdNpcNetwork).ForceGetController(Player.GetPlayerId(info.sender));
			}
			bool flag2 = stream.Read<bool>(new object[0]);
			RPCPeer(info.sender, EPacketType.PT_NPC_RequestAiOp, num, num6, flag2);
			break;
		}
		case EReqType.Translate:
			base.transform.position = stream.Read<Vector3>(new object[0]);
			mAiSynAttribute.mv3Postion = base.transform.position;
			RPCOthers(EPacketType.PT_NPC_RequestAiOp, num, base.transform.position, stream.Read<bool>(new object[0]));
			break;
		case EReqType.UseSkill:
			ForceGetController(Player.GetPlayerId(info.sender));
			RPCPeer(info.sender, EPacketType.PT_NPC_RequestAiOp, num);
			break;
		case EReqType.Remove:
			RPCOthers(EPacketType.PT_NPC_RequestAiOp, num, stream.Read<int>(new object[0]), stream.Read<Vector3[]>(new object[0]));
			break;
		case EReqType.Dialogue:
		case EReqType.Attack:
		case EReqType.PauseAll:
		case EReqType.Hand:
			break;
		}
	}

	private void RPC_S2C_Mount(uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
		int num = (_mountId = stream.Read<int>(new object[0]));
		RPCOthers(EPacketType.PT_NPC_Mount, num);
	}

	private void RPC_S2C_UpdateCampsite(uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
		bool flag = stream.Read<bool>(new object[0]);
		RPCOthers(EPacketType.PT_NPC_UpdateCampsite, flag);
	}

	private void RPC_S2C_State(uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
		int num = stream.Read<int>(new object[0]);
		RPCOthers(EPacketType.PT_NPC_State, num);
	}

	private void RPC_C2S_GetOnVehicle(uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
		if (!(null != base._Creation))
		{
			int id = stream.Read<int>(new object[0]);
			int index = stream.Read<int>(new object[0]);
			CreationNetwork creationNetwork = ObjNetInterface.Get<CreationNetwork>(id);
			if (!(null == creationNetwork))
			{
				GetOnVehicle(creationNetwork, index);
			}
		}
	}

	private void RPC_C2S_GetOffVehicle(uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
		if (base._OnCar)
		{
			Vector3 outPos = stream.Read<Vector3>(new object[0]);
			GetOffVehicle(outPos);
		}
	}

	private void RPC_C2S_ForcedServant(uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
		bool flag = stream.Read<bool>(new object[0]);
		if (bForcedServant != flag || !flag || (base.authId != Player.GetPlayer(info.sender).Id && !(ObjNetInterface.Get(base.authId) != null)))
		{
			if (flag)
			{
				ForcedServant(Player.GetPlayer(info.sender));
			}
			else
			{
				RemoveForcedServant(Player.GetPlayer(info.sender));
			}
			bForcedServant = flag;
		}
	}

	public bool ForceGetController(int playerId)
	{
		Player player = ObjNetInterface.Get(playerId) as Player;
		if (null != player && player is Player)
		{
			base.authId = player.Id;
			RPCOthers(EPacketType.PT_InGame_SetController, base.authId);
			return true;
		}
		return false;
	}

	private void RPC_C2S_AddEnemyLock(uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
		int num = stream.Read<int>(new object[0]);
		RPCProxy(EPacketType.PT_NPC_AddEnemyLock, num);
	}

	private void RPC_C2S_RemoveEnemyLock(uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
		int num = stream.Read<int>(new object[0]);
		RPCProxy(EPacketType.PT_NPC_RemoveEnemyLock, num);
	}

	private void RPC_C2S_ClearEnemyLocked(uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
		RPCProxy(EPacketType.PT_NPC_ClearEnemyLocked);
	}

	protected IEnumerator DestroyCoroutine()
	{
		yield return new WaitForSeconds(2f);
		NetInterface.NetDestroy(this);
	}

	private void InitClnNpcData(int teamID)
	{
		_teamId = teamID;
		ColonyNpc npcByID = ColonyNpcMgr.GetNpcByID(base.Id);
		if (npcByID != null)
		{
			SyncTeamId();
			RPCOthers(EPacketType.PT_CL_CLN_InitData, npcByID.m_DwellingsID, npcByID.m_GuardPos, npcByID.m_Occupation, npcByID.m_State, npcByID.m_WorkMode, npcByID.m_WorkRoomID, npcByID.m_IsProcessing, npcByID.m_ProcessingIndex, (int)npcByID.trainerType, (int)npcByID.trainingType, npcByID.IsTraining);
		}
	}

	private void RPC_C2S_CLN_InitData(uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
		Player player = Player.GetPlayer(info.sender);
		if (player != null)
		{
			InitClnNpcData(player.TeamId);
		}
	}

	private void RPC_C2S_CLN_SetState(uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
		ColonyNpc npcByID = ColonyNpcMgr.GetNpcByID(base.Id);
		if (npcByID != null)
		{
			int num = stream.Read<int>(new object[0]);
			npcByID.m_State = stream.Read<int>(new object[0]);
			RPCOthers(EPacketType.PT_CL_CLN_SetState, npcByID.m_State, num);
		}
	}

	private void RPC_C2S_CLN_SetDwellingsID(uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
		int dwellingsID = stream.Read<int>(new object[0]);
		ColonyNpc npcByID = ColonyNpcMgr.GetNpcByID(base.Id);
		if (npcByID != null)
		{
			if (npcByID.m_DwellingsID != 0)
			{
				((ColonyDwellings)ColonyMgr.GetColonyItemByObjId(npcByID.m_DwellingsID))?.RemoveNpcs(npcByID._npcID);
			}
			npcByID.m_DwellingsID = dwellingsID;
			npcByID.Save();
			ColonyDwellings colonyDwellings = (ColonyDwellings)ColonyMgr.GetColonyItemByObjId(npcByID.m_DwellingsID);
			if (colonyDwellings != null)
			{
				colonyDwellings.AddNpcs(npcByID._npcID);
				RPCOthers(EPacketType.PT_CL_CLN_SetDwellingsID, npcByID.m_DwellingsID);
			}
		}
	}

	private void RPC_C2S_CLN_SetWorkRoomID(uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
		int num = stream.Read<int>(new object[0]);
		ColonyNpc npcByID = ColonyNpcMgr.GetNpcByID(base.Id);
		if (npcByID != null)
		{
			ColonyBase colonyItemByObjId = ColonyMgr.GetColonyItemByObjId(num);
			if (colonyItemByObjId != null && colonyItemByObjId.AddWorker(npcByID))
			{
				npcByID.m_WorkRoomID = num;
				npcByID.Save();
				RPCOthers(EPacketType.PT_CL_CLN_SetWorkRoomID, npcByID.m_WorkRoomID);
			}
		}
	}

	private void RPC_C2S_CLN_RemoveNpc(uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
		ColonyNpc npcByID = ColonyNpcMgr.GetNpcByID(base.Id);
		if (npcByID != null)
		{
			ColonyMgr.GetColonyItemByObjId(npcByID.m_WorkRoomID)?.RemoveWorker(npcByID);
			((ColonyDwellings)ColonyMgr.GetColonyItemByObjId(npcByID.m_DwellingsID))?.RemoveNpcs(npcByID._npcID);
			ColonyNpcMgr.RemoveAt(npcByID.TeamId, npcByID._npcID);
			RPCOthers(EPacketType.PT_CL_CLN_RemoveNpc);
			AiAdNpcNetwork aiAdNpcNetwork = ObjNetInterface.Get<AiAdNpcNetwork>(npcByID._npcID);
			if (aiAdNpcNetwork != null)
			{
				aiAdNpcNetwork.DismissByPlayer();
			}
		}
	}

	private void RPC_C2S_CLN_SetOccupation(uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
		int num = stream.Read<int>(new object[0]);
		ColonyNpc npcByID = ColonyNpcMgr.GetNpcByID(base.Id);
		if (npcByID == null)
		{
			return;
		}
		if (num == 3)
		{
			List<ColonyNpc> teamNpcs = ColonyNpcMgr.GetTeamNpcs(base.TeamId);
			if (teamNpcs.FindAll((ColonyNpc it) => it.m_Occupation == 3).Count >= 8)
			{
				return;
			}
		}
		if (num == 5)
		{
			List<ColonyNpc> teamNpcs2 = ColonyNpcMgr.GetTeamNpcs(base.TeamId);
			if (teamNpcs2.FindAll((ColonyNpc it) => it.m_Occupation == 5).Count >= 5)
			{
				return;
			}
		}
		npcByID.m_Occupation = num;
		npcByID.Save();
		RPCOthers(EPacketType.PT_CL_CLN_SetOccupation, npcByID.m_Occupation, npcByID.m_WorkMode, npcByID.m_WorkRoomID);
	}

	private void RPC_C2S_CLN_SetWorkMode(uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
		int workMode = stream.Read<int>(new object[0]);
		ColonyNpc npcByID = ColonyNpcMgr.GetNpcByID(base.Id);
		if (npcByID != null)
		{
			npcByID.m_WorkMode = workMode;
			npcByID.Save();
			RPCOthers(EPacketType.PT_CL_CLN_SetWorkMode, npcByID.m_WorkMode);
		}
	}

	private void RPC_C2S_CLN_PlantGetBack(uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
		int num = stream.Read<int>(new object[0]);
		int objId = stream.Read<int>(new object[0]);
		ColonyNpc npcByID = ColonyNpcMgr.GetNpcByID(base.Id);
		if (npcByID == null)
		{
			if (LogFilter.logDebug)
			{
				Debug.LogError("npc is null: RPC_C2S_CLN_PlantGetBack");
			}
			return;
		}
		FarmPlantLogic plantByItemObjID = FarmManager.Instance.GetPlantByItemObjID(num);
		if (plantByItemObjID == null)
		{
			return;
		}
		ColonyFarm colonyFarm = (ColonyFarm)ColonyMgr.GetColonyItemByObjId(objId);
		if (colonyFarm == null)
		{
			return;
		}
		float num2 = 1f + npcByID.GetHarvestSkill;
		int num3 = (int)((float)((int)(plantByItemObjID.mLife / 20.0) + 1) * 0.2f * (float)plantByItemObjID.mPlantInfo.mItemGetNum * num2);
		Dictionary<int, int> dictionary = new Dictionary<int, int>();
		for (int i = 0; i < num3; i++)
		{
			float num4 = Random.Range(0f, 1f);
			for (int j = 0; j < plantByItemObjID.mPlantInfo.mItemGetPro.Count; j++)
			{
				if (num4 < plantByItemObjID.mPlantInfo.mItemGetPro[j].m_probablity)
				{
					if (!dictionary.ContainsKey(plantByItemObjID.mPlantInfo.mItemGetPro[j].m_id))
					{
						dictionary[plantByItemObjID.mPlantInfo.mItemGetPro[j].m_id] = 0;
					}
					Dictionary<int, int> dictionary2;
					Dictionary<int, int> dictionary3 = (dictionary2 = dictionary);
					int id;
					int key = (id = plantByItemObjID.mPlantInfo.mItemGetPro[j].m_id);
					id = dictionary2[id];
					dictionary3[key] = id + 1;
				}
			}
		}
		List<ColonyBase> colonyItemsByItemId = ColonyMgr._Instance.GetColonyItemsByItemId(base.TeamId, 1129);
		if (colonyItemsByItemId != null && colonyItemsByItemId.Count > 0)
		{
			List<ItemIdCount> list = new List<ItemIdCount>();
			foreach (int key2 in dictionary.Keys)
			{
				list.Add(new ItemIdCount(key2, dictionary[key2]));
			}
			if (CSUtils.CanAddListToStorage(list, base.TeamId))
			{
				CSUtils.AddItemListToStorage(list, base.TeamId);
				FarmManager.Instance.RemovePlant(num);
				RPCOthers(EPacketType.PT_CL_CLN_PlantGetBack, num);
				return;
			}
			ColonyMgr._Instance.GetColonyAssembly(base.TeamId)?.ShowTips(ETipType.storage_full);
		}
		RPCPeer(info.sender, EPacketType.PT_CL_FARM_RestoreGetBack, plantByItemObjID);
	}

	private void RPC_C2S_CLN_PlantPutOut(uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
		Vector3 vector = stream.Read<Vector3>(new object[0]);
		int objId = stream.Read<int>(new object[0]);
		byte b = stream.Read<byte>(new object[0]);
		ColonyFarm colonyFarm = (ColonyFarm)ColonyMgr.GetColonyItemByObjId(objId);
		if (colonyFarm == null)
		{
			return;
		}
		int num = colonyFarm.NpcGetPlantSeedId();
		if (num <= 0)
		{
			return;
		}
		ItemObject itemByID = ItemManager.GetItemByID(num);
		if (itemByID == null)
		{
			if (LogFilter.logDebug)
			{
				Debug.LogError("Item does not exsit.");
			}
			return;
		}
		ItemObject itemObject = itemByID;
		if (!itemObject.CountDown(1))
		{
			colonyFarm.DeleteSeed(num);
		}
		else
		{
			ItemObject itemObject2 = ItemManager.CreateFromItem(itemByID.protoId, 1, itemByID);
			if (itemObject2 == null)
			{
				return;
			}
			itemObject = itemObject2;
			ChannelNetwork.SyncItemList(base.WorldId, new ItemObject[2] { itemObject2, itemByID });
			num = itemObject2.instanceId;
		}
		FarmPlantLogic farmPlantLogic = FarmManager.Instance.GetPlantByItemObjID(num);
		if (farmPlantLogic == null)
		{
			farmPlantLogic = FarmManager.Instance.CreatePlant(base.WorldId, num, PlantInfo.GetPlantInfoByItemId(itemObject.protoId).mTypeID, vector, Quaternion.Euler(0f, 0f, 0f), b);
		}
		farmPlantLogic.mPos = vector;
		RPCOthers(EPacketType.PT_CL_CLN_PlantPutOut, vector, Quaternion.Euler(0f, 0f, 0f), num, b, farmPlantLogic);
	}

	private void RPC_C2S_CLN_PlantWater(uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
		int itemObjID = stream.Read<int>(new object[0]);
		int objId = stream.Read<int>(new object[0]);
		FarmPlantLogic plantByItemObjID = FarmManager.Instance.GetPlantByItemObjID(itemObjID);
		if (plantByItemObjID == null)
		{
			return;
		}
		int b = (int)(((double)plantByItemObjID.mPlantInfo.mWaterLevel[1] - plantByItemObjID.mWater) / 30.0);
		ColonyFarm colonyFarm = (ColonyFarm)ColonyMgr.GetColonyItemByObjId(objId);
		if (colonyFarm == null)
		{
			return;
		}
		int toolId;
		int waterCount = colonyFarm.GetWaterCount(out toolId);
		if (waterCount > 0)
		{
			plantByItemObjID.Watering(Mathf.Min(waterCount, b));
			colonyFarm.DeleteItemWithItemObjID(toolId, Mathf.Min(waterCount, b), 1);
			if (ItemManager.GetItemByID(toolId) == null)
			{
				colonyFarm.DeletePlantTool(toolId);
			}
			FarmManager.Instance.SyncPlant(plantByItemObjID);
		}
		else
		{
			colonyFarm._Network.RPCPeer(info.sender, EPacketType.PT_CL_FARM_RestoreWater, plantByItemObjID);
		}
	}

	private void RPC_C2S_CLN_PlantClean(uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
		int itemObjID = stream.Read<int>(new object[0]);
		int objId = stream.Read<int>(new object[0]);
		FarmPlantLogic plantByItemObjID = FarmManager.Instance.GetPlantByItemObjID(itemObjID);
		if (plantByItemObjID == null)
		{
			return;
		}
		int b = (int)(((double)plantByItemObjID.mPlantInfo.mCleanLevel[1] - plantByItemObjID.mClean) / 30.0);
		ColonyFarm colonyFarm = (ColonyFarm)ColonyMgr.GetColonyItemByObjId(objId);
		if (colonyFarm == null)
		{
			return;
		}
		int toolId;
		int cleanCount = colonyFarm.GetCleanCount(out toolId);
		if (cleanCount > 0)
		{
			plantByItemObjID.Cleaning(Mathf.Min(cleanCount, b));
			colonyFarm.DeleteItemWithItemObjID(toolId, Mathf.Min(cleanCount, b), 1);
			if (ItemManager.GetItemByID(toolId) == null)
			{
				colonyFarm.DeletePlantTool(toolId);
			}
			FarmManager.Instance.SyncPlant(plantByItemObjID);
		}
		else
		{
			colonyFarm._Network.RPCPeer(info.sender, EPacketType.PT_CL_FARM_RestoreClean, plantByItemObjID);
		}
	}

	private void RPC_C2S_CLN_PlantClear(uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
		int num = stream.Read<int>(new object[0]);
		FarmPlantLogic plantByItemObjID = FarmManager.Instance.GetPlantByItemObjID(num);
		if (plantByItemObjID != null)
		{
			FarmManager.Instance.RemovePlant(num);
			ItemManager.RemoveItem(num);
			RPCOthers(EPacketType.PT_CL_CLN_PlantClear, num);
		}
	}

	private void RPC_C2S_CLN_SetProcessingIndex(uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
		int taskIndex = stream.Read<int>(new object[0]);
		ColonyNpc npcByID = ColonyNpcMgr.GetNpcByID(base.Id);
		if (npcByID != null)
		{
			ColonyBase colonyItemByObjId = ColonyMgr.GetColonyItemByObjId(npcByID.m_WorkRoomID);
			if (colonyItemByObjId is ColonyProcessing colonyProcessing)
			{
				colonyProcessing.TrySetNpcProcessingIndex(npcByID, taskIndex);
			}
		}
	}

	private void RPC_C2S_CLN_SetIsProcessing(uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
	}

	public void SetIsProcessing(bool isProcessing)
	{
		RPCOthers(EPacketType.PT_CL_CLN_SetIsProcessing, isProcessing);
	}

	public void InitAutoIncreaseMoney(int max, int valuePerDay)
	{
		mAutoIncreaseMoney = new AutoIncreaseMoney(mNpcMoney, max, valuePerDay, added: true);
	}

	public void GetOnCarrier()
	{
	}

	private Vector3 GetOffPostion()
	{
		return new Vector3(0f, 0f, 0f);
	}

	public override void WeaponReload(int objId, int oldProtoId, int newProtoId, float magazineSize)
	{
		ItemObject itemByID = ItemManager.GetItemByID(objId);
		if (itemByID == null)
		{
			return;
		}
		GunAmmo cmpt = itemByID.GetCmpt<GunAmmo>();
		if (cmpt != null && ItemModule != null)
		{
			List<ItemObject> effItems = new List<ItemObject>();
			if (oldProtoId != -1 && cmpt.count != 0)
			{
				ItemModule.AddItem(oldProtoId, cmpt.count, ref effItems);
				cmpt.count = 0;
			}
			ItemModule.DelItem(newProtoId, (int)magazineSize, ref effItems);
			cmpt.count += (int)magazineSize;
			cmpt.index = newProtoId;
			effItems.Add(itemByID);
			ChannelNetwork.SyncItemList(base.WorldId, effItems);
			SyncPackageIndex(0);
		}
	}

	public override void ItemAttrChange(int itemObjId, float num)
	{
		ItemObject itemObject = base.EquipModule[itemObjId];
		if (itemObject != null)
		{
			GunAmmo cmpt = itemObject.GetCmpt<GunAmmo>();
			if (cmpt != null && !((float)cmpt.count < num))
			{
				cmpt.count -= (int)num;
				ChannelNetwork.SyncItem(base.WorldId, itemObject);
			}
		}
	}

	public override void EquipItemCost(int itemObjId, float num)
	{
		ItemObject itemObject = base.EquipModule[itemObjId];
		if (itemObject != null)
		{
			if (!itemObject.CountDown((int)num))
			{
				base.EquipModule.Remove(itemObject);
				SyncTakeOffEquip(itemObjId);
			}
			else
			{
				ChannelNetwork.SyncItem(base.WorldId, itemObject);
			}
		}
	}

	public override void PackageItemCost(int itemObjId, float num)
	{
	}

	public void AddAbility(int id)
	{
		Npcskillcmpt.AddNpcSkillAbility(id);
	}

	public bool RemoveAbility(int id)
	{
		Npcskillcmpt.RemoveNpcSkillAbiliy(id);
		return true;
	}

	public void LevelUp(AblityType type, SkillLevel oldlevel, SkillLevel newlevel)
	{
		NpcAbility npcAbility = NpcAbility.FindNpcAbility(type, oldlevel);
		NpcAbility npcAbility2 = NpcAbility.FindNpcAbility(type, newlevel);
		if (npcAbility != null)
		{
			RemoveAbility(npcAbility.id);
		}
		if (npcAbility2 != null)
		{
			AddAbility(npcAbility2.id);
		}
	}

	public List<int> GetSkllIds()
	{
		if (Npcskillcmpt == null)
		{
			return null;
		}
		return Npcskillcmpt.GetSkillIDs();
	}

	public float GetNpcSkillRange(int skillId)
	{
		if (Npcskillcmpt == null)
		{
			return 0f;
		}
		return Npcskillcmpt.GetCmptSkillRange(skillId);
	}

	public bool TryGetItemSkill(Vector3 pos, float percent = 1f)
	{
		if (Npcskillcmpt != null)
		{
			return Npcskillcmpt.TryGetItemskill(pos, percent) != null;
		}
		return false;
	}

	public float GetHpPerChange()
	{
		List<int> skllIds = GetSkllIds();
		if (skllIds == null)
		{
			return 0f;
		}
		using (List<int>.Enumerator enumerator = skllIds.GetEnumerator())
		{
			if (enumerator.MoveNext())
			{
				int current = enumerator.Current;
				return GetNpcChange_Hp(current);
			}
		}
		return 0f;
	}

	public float GetNpcChange_Hp(int SkillId)
	{
		if (Npcskillcmpt == null)
		{
			return 0f;
		}
		return Npcskillcmpt.GetChangeHpPer(SkillId);
	}

	public bool IsFollower()
	{
		return Recruited;
	}

	public bool SetFollower(bool bFlag)
	{
		if (!bFlag)
		{
			if (null == lordPlayer || !Equals(lordPlayer) || bForcedServant)
			{
				return false;
			}
			DismissByPlayer();
			return true;
		}
		return true;
	}

	protected void InitNpcData(byte[] data)
	{
		Serialize.Import(data, delegate
		{
		});
	}

	protected void InitNpcData(int randNpcId)
	{
		RandomNpcDb.Item item = RandomNpcDb.Get(randNpcId);
		if (item == null)
		{
			if (LogFilter.logDebug)
			{
				Debug.LogError("no npc random data found, randId:" + randNpcId);
			}
			return;
		}
		mAutoReviveTime = item.reviveTime;
		int num = item.hpMax.Random();
		int num2 = item.atk.Random();
		int resDamage = item.resDamage;
		float atkRange = item.atkRange;
		int num3 = item.def.Random();
		_skEntity.SetAllAttribute(AttribType.HpMax, num);
		_skEntity.SetAllAttribute(AttribType.Atk, num2);
		_skEntity.SetAllAttribute(AttribType.ResDamage, resDamage);
		_skEntity.SetAllAttribute(AttribType.AtkRange, atkRange);
		_skEntity.SetAllAttribute(AttribType.Def, num3);
		_skEntity.SetAllAttribute(AttribType.Hp, num);
	}

	protected void GenerateRandomNpcData(PeSex sex, int race)
	{
		int npcType = m_npcType;
		if (npcType == 6)
		{
			mCharacterName = new CharacterName(mCustomName);
		}
		else
		{
			mCharacterName = NameGenerater.Fetch(sex, race);
		}
	}

	protected void GenerateStoryNpcData(int protoId)
	{
		if (NpcProtoDb.Get(protoId) != null)
		{
			string[] array = NpcProtoDb.Get(protoId).defaultName.Split(' ');
			if (array.Length > 1)
			{
				mCharacterName = new CharacterName(NpcProtoDb.Get(protoId).defaultName, NpcProtoDb.Get(protoId).defaultShowName, array[1]);
			}
			else
			{
				mCharacterName = new CharacterName(NpcProtoDb.Get(protoId).defaultName, NpcProtoDb.Get(protoId).defaultShowName, NpcProtoDb.Get(protoId).defaultName);
			}
		}
	}

	public void SetMissionFlag(int flag, int nMissionID, int nTeam, int casterId, int type)
	{
		IntMissionFlag = flag;
		if (IntMissionFlag == MISSIONFlag.Mission_No)
		{
			if (mni.nMissionID != -1 && mni.nTeam != -1)
			{
				PlayerMission curPlayerMissionByMissionId = MissionManager.Manager.GetCurPlayerMissionByMissionId(casterId, mni.nMissionID);
				if (curPlayerMissionByMissionId != null && curPlayerMissionByMissionId.HasMission(mni.nMissionID))
				{
					Player player = Player.GetPlayer(casterId);
					if (null == player)
					{
						return;
					}
					curPlayerMissionByMissionId.FailureMission(player, mni.nMissionID);
					SetMissionFlagByDel();
				}
			}
		}
		else
		{
			Player player2 = Player.GetPlayer(casterId);
			if (null == player2)
			{
				return;
			}
			mni.nMissionID = nMissionID;
			mni.nTeam = nTeam;
			mni.name = base.name;
			base.authId = player2.Id;
			if (IntMissionFlag == MISSIONFlag.Mission_Follow || IntMissionFlag == MISSIONFlag.Mission_Dif)
			{
			}
			RPCProxy(EPacketType.PT_InGame_SetController, base.authId);
		}
		SyncMissionFlag();
	}

	public void SyncMission()
	{
		RPCProxy(EPacketType.PT_NPC_Mission, mission.Serialize());
	}

	public void SyncMissionFlag()
	{
		RPCProxy(EPacketType.PT_NPC_MissionFlag, mni.name, IntMissionFlag, mni.nMissionID);
	}

	public void SetMissionFlagByDel()
	{
		IntMissionFlag = MISSIONFlag.Mission_No;
		base.authId = -1;
		mni.nMissionID = -1;
		mni.nTeam = -1;
		mni.name = string.Empty;
	}

	public void NpcRevive()
	{
		_bDeath = false;
		float attribute = _skEntity.GetAttribute(AttribType.HpMax);
		_skEntity.SetAllAttribute(AttribType.Hp, attribute);
		base.transform.position += Vector3.up;
		CancelEEState(PEActionType.Death);
		RPCProxy(EPacketType.PT_NPC_Revive, attribute, base.transform.position);
	}

	public override bool GetDeadObjItem(Player player)
	{
		if (null == player)
		{
			return false;
		}
		if (base.DropItemID.Count <= 0)
		{
			return false;
		}
		if (!player.Package.CanAdd(base.DropItemID))
		{
			return false;
		}
		ItemObject[] array = player.Package.AddSameItems(base.DropItemID);
		if (array == null)
		{
			return false;
		}
		base.DropItemID.Clear();
		if (ItemModule != null)
		{
			ItemModule.Clear();
		}
		player.SyncItemList(array);
		player.SyncPackageIndex();
		player.SyncNewItem(base.DropItemID);
		return true;
	}

	public override bool GetDeadObjItem(Player player, int index, int itemID)
	{
		if (null == player)
		{
			return false;
		}
		if (index <= -1 || index >= base.DropItemID.Count)
		{
			return false;
		}
		if (base.DropItemID[index].protoId != itemID)
		{
			return false;
		}
		if (!player.Package.CanAdd(base.DropItemID[index]))
		{
			return false;
		}
		List<ItemObject> effItems = new List<ItemObject>();
		player.Package.AddSameItems(base.DropItemID[index], ref effItems);
		base.DropItemID.RemoveAt(index);
		ItemModule.DelItem(base.DropItemID[index]);
		player.SyncPackageIndex();
		player.SyncNewItem(base.DropItemID[itemID]);
		return true;
	}

	public bool CanRevie()
	{
		return base.IsDead && GameTime.PlayTime.Second - (double)DeadStartTime >= (double)TotalRescueGameTime;
	}

	public void SetMissionNpcInfo(string name, int nMissionID, int MissionFlag, bool Inv)
	{
		mni.name = name;
		mni.nMissionID = nMissionID;
		SyncMissionFlag();
	}
}

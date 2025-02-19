using System.Collections;
using System.Collections.Generic;
using System.IO;
using CSRecord;
using CustomCharactor;
using ItemAsset;
using Pathea;
using Pathea.PeEntityExt;
using Pathea.PeEntityExtTrans;
using PETools;
using uLink;
using UnityEngine;

public class AiAdNpcNetwork : AiNetwork
{
	protected int npcType;

	protected int originTeam = -1;

	protected int mLordPlayerId = -1;

	protected int mColonyLordPlayerId = -1;

	protected float rotY;

	public NpcMissionData useData;

	protected NpcCmpt _npcCmpt;

	protected Vector3 spawnPos;

	protected bool isStand;

	public bool _npcMissionInited;

	public bool bForcedServant;

	public int _mountId;

	private string customName;

	private bool _op;

	internal CreationNetwork DriveCreation { get; set; }

	internal int SeatIndex { get; set; }

	public int LordPlayerId => mLordPlayerId;

	public NpcCmpt npcCmpt
	{
		get
		{
			if (_npcCmpt == null)
			{
				PeEntity peEntity = PeSingleton<EntityMgr>.Instance.Get(base.Id);
				if (peEntity != null)
				{
					_npcCmpt = peEntity.GetCmpt<NpcCmpt>();
				}
			}
			return _npcCmpt;
		}
	}

	private void InitClnNpcData()
	{
		RPCServer(EPacketType.PT_CL_CLN_InitData);
	}

	public void SetClnState(int state)
	{
		RPCServer(EPacketType.PT_CL_CLN_SetState, state);
	}

	public void SetClnDwellingsID(int dwellingsID)
	{
		RPCServer(EPacketType.PT_CL_CLN_SetDwellingsID, dwellingsID);
	}

	public void SetClnWorkRoomID(int workRoomID)
	{
		RPCServer(EPacketType.PT_CL_CLN_SetWorkRoomID, workRoomID);
	}

	public void SetClnOccupation(int occupation)
	{
		RPCServer(EPacketType.PT_CL_CLN_SetOccupation, occupation);
	}

	public void SetClnWorkMode(int workMode)
	{
		RPCServer(EPacketType.PT_CL_CLN_SetWorkMode, workMode);
	}

	public void PlantGetBack(int objId, int farmId)
	{
		RPCServer(EPacketType.PT_CL_CLN_PlantGetBack, objId, farmId);
	}

	public void PlantPutOut(Vector3 pos, int farmId, byte terrainType)
	{
		RPCServer(EPacketType.PT_CL_CLN_PlantPutOut, pos, farmId, terrainType);
	}

	public void PlantWater(int objID, int farmId)
	{
		RPCServer(EPacketType.PT_CL_CLN_PlantWater, objID, farmId);
	}

	public void PlantClean(int objID, int farmId)
	{
		RPCServer(EPacketType.PT_CL_CLN_PlantClean, objID, farmId);
	}

	public void PlantClear(int objID)
	{
		RPCServer(EPacketType.PT_CL_CLN_PlantClear, objID);
	}

	private void RPC_S2C_CLN_InitData(uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
		int dwellingsID = stream.Read<int>(new object[0]);
		Vector3 guardPos = stream.Read<Vector3>(new object[0]);
		int occupation = stream.Read<int>(new object[0]);
		int state = stream.Read<int>(new object[0]);
		int workMode = stream.Read<int>(new object[0]);
		int workRoomID = stream.Read<int>(new object[0]);
		bool isProcessing = stream.Read<bool>(new object[0]);
		int processingIndex = stream.Read<int>(new object[0]);
		int trainerType = stream.Read<int>(new object[0]);
		int trainingType = stream.Read<int>(new object[0]);
		bool isTraining = stream.Read<bool>(new object[0]);
		CSPersonnelData cSPersonnelData = new CSPersonnelData();
		cSPersonnelData.ID = base.Id;
		cSPersonnelData.dType = 50;
		cSPersonnelData.m_State = state;
		cSPersonnelData.m_Occupation = occupation;
		cSPersonnelData.m_WorkMode = workMode;
		cSPersonnelData.m_DwellingsID = dwellingsID;
		cSPersonnelData.m_GuardPos = guardPos;
		cSPersonnelData.m_WorkRoomID = workRoomID;
		cSPersonnelData.m_IsProcessing = isProcessing;
		cSPersonnelData.m_ProcessingIndex = processingIndex;
		cSPersonnelData.m_TrainerType = trainerType;
		cSPersonnelData.m_TrainingType = trainingType;
		cSPersonnelData.m_IsTraining = isTraining;
		StartCoroutine(InitDataRoutine(cSPersonnelData));
	}

	private IEnumerator InitDataRoutine(CSPersonnelData cspd)
	{
		PeEntity npc = null;
		while (true)
		{
			npc = PeSingleton<EntityMgr>.Instance.Get(base.Id);
			if (npc == null)
			{
				yield return new WaitForSeconds(0.5f);
				continue;
			}
			break;
		}
		CSCreator creator = null;
		while (true)
		{
			creator = MultiColonyManager.GetCreator(base.TeamId);
			if (creator == null)
			{
				yield return new WaitForSeconds(0.5f);
				continue;
			}
			break;
		}
		((CSMgCreator)creator).AddNpc(npc, cspd, bSetPos: true);
	}

	private void RPC_S2C_CLN_SetState(uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
	}

	private void RPC_S2C_CLN_SetDwellingsID(uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
		int iD = stream.Read<int>(new object[0]);
		CSMgCreator creator = MultiColonyManager.GetCreator(base.TeamId, createNewIfNone: false);
		if (!(creator != null))
		{
			return;
		}
		CSPersonnel[] npcs = creator.GetNpcs();
		CSPersonnel[] array = npcs;
		foreach (CSPersonnel cSPersonnel in array)
		{
			if (cSPersonnel != null && cSPersonnel.m_Npc != null && base.Id == cSPersonnel.m_Npc.Id)
			{
				CSDwellings cSDwellings = creator.GetCommonEntity(iD) as CSDwellings;
				cSDwellings.AddNpcs(cSPersonnel);
			}
		}
	}

	private void RPC_S2C_CLN_SetWorkRoomID(uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
		int num = stream.Read<int>(new object[0]);
		CSCreator creator = MultiColonyManager.GetCreator(base.TeamId, createNewIfNone: false);
		if (!(creator != null))
		{
			return;
		}
		CSPersonnel[] npcs = creator.GetNpcs();
		CSPersonnel[] array = npcs;
		foreach (CSPersonnel cSPersonnel in array)
		{
			if (cSPersonnel == null || !(cSPersonnel.m_Npc != null) || base.Id != cSPersonnel.m_Npc.Id)
			{
				continue;
			}
			if (num == 0)
			{
				cSPersonnel.WorkRoom = null;
				break;
			}
			Dictionary<int, CSCommon> commonEntities = creator.GetCommonEntities();
			foreach (KeyValuePair<int, CSCommon> item in commonEntities)
			{
				if (item.Value.Assembly != null && item.Value.WorkerMaxCount > 0 && item.Value.m_Type != 7 && item.Value.ID == num)
				{
					cSPersonnel.WorkRoom = item.Value;
					break;
				}
			}
		}
	}

	private void RPC_S2C_CLN_SetOccupation(uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
		int occupation = stream.Read<int>(new object[0]);
		int workMode = stream.Read<int>(new object[0]);
		int iD = stream.Read<int>(new object[0]);
		CSCreator creator = MultiColonyManager.GetCreator(base.TeamId, createNewIfNone: false);
		if (!(creator != null))
		{
			return;
		}
		CSPersonnel[] npcs = creator.GetNpcs();
		CSPersonnel[] array = npcs;
		foreach (CSPersonnel cSPersonnel in array)
		{
			if (cSPersonnel != null && cSPersonnel.m_Npc != null && base.Id == cSPersonnel.m_Npc.Id)
			{
				cSPersonnel.m_Occupation = occupation;
				cSPersonnel.m_WorkMode = workMode;
				cSPersonnel.WorkRoom = creator.GetCommonEntity(iD);
			}
		}
	}

	private void RPC_S2C_CLN_SetWorkMode(uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
		int workMode = stream.Read<int>(new object[0]);
		CSCreator creator = MultiColonyManager.GetCreator(base.TeamId, createNewIfNone: false);
		if (!(creator != null))
		{
			return;
		}
		CSPersonnel[] npcs = creator.GetNpcs();
		CSPersonnel[] array = npcs;
		foreach (CSPersonnel cSPersonnel in array)
		{
			if (cSPersonnel != null && cSPersonnel.m_Npc != null && base.Id == cSPersonnel.m_Npc.Id)
			{
				cSPersonnel.m_WorkMode = workMode;
			}
		}
	}

	private void RPC_S2C_CLN_PlantGetBack(uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
		int num = stream.Read<int>(new object[0]);
		if (num != -1)
		{
			FarmManager.Instance.RemovePlant(num);
			DragArticleAgent.Destory(num);
			PeSingleton<ItemMgr>.Instance.DestroyItem(num);
			return;
		}
		FarmPlantLogic plant = stream.Read<FarmPlantLogic>(new object[0]);
		CSMgCreator creator = MultiColonyManager.GetCreator(base.TeamId);
		if (!(creator == null) && creator.Assembly != null)
		{
			creator.Assembly.Farm?.RestoreWateringPlant(plant);
		}
	}

	private void RPC_S2C_CLN_RemoveNpc(uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
		CSCreator creator = MultiColonyManager.GetCreator(base.TeamId);
		PeEntity peEntity = PeSingleton<EntityMgr>.Instance.Get(base.Id);
		creator.RemoveNpc(peEntity);
		peEntity.Dismiss();
	}

	private void RPC_S2C_CLN_PlantPutOut(uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
		Vector3 pos = stream.Read<Vector3>(new object[0]);
		Quaternion quaternion = stream.Read<Quaternion>(new object[0]);
		int id = stream.Read<int>(new object[0]);
		stream.Read<byte>(new object[0]);
		ItemObject itemObject = PeSingleton<ItemMgr>.Instance.Get(id);
		DragArticleAgent dragArticleAgent = DragArticleAgent.Create(itemObject.GetCmpt<Drag>(), pos, Vector3.one, quaternion, id);
		FarmPlantLogic farmPlantLogic = dragArticleAgent.itemLogic as FarmPlantLogic;
		farmPlantLogic.InitInMultiMode();
		stream.Read<FarmPlantLogic>(new object[0]);
		farmPlantLogic.UpdateInMultiMode();
	}

	private void RPC_S2C_CLN_PlantUpdateInfo(uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
		int itemObjID = stream.Read<int>(new object[0]);
		double mLife = stream.Read<double>(new object[0]);
		double mWater = stream.Read<double>(new object[0]);
		double mClean = stream.Read<double>(new object[0]);
		FarmPlantLogic plantByItemObjID = FarmManager.Instance.GetPlantByItemObjID(itemObjID);
		if (plantByItemObjID != null)
		{
			plantByItemObjID.mLife = mLife;
			plantByItemObjID.mWater = mWater;
			plantByItemObjID.mClean = mClean;
		}
	}

	private void RPC_S2C_CLN_PlantClear(uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
		int num = stream.Read<int>(new object[0]);
		FarmPlantLogic plantByItemObjID = FarmManager.Instance.GetPlantByItemObjID(num);
		if (plantByItemObjID != null)
		{
			FarmManager.Instance.RemovePlant(num);
			DragArticleAgent.Destory(num);
			PeSingleton<ItemMgr>.Instance.DestroyItem(num);
		}
	}

	private void RPC_S2C_CLN_PlantWater(uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
		stream.Read<int>(new object[0]);
		FarmPlantLogic plant = stream.Read<FarmPlantLogic>(new object[0]);
		CSMgCreator creator = MultiColonyManager.GetCreator(base.TeamId);
		if (!(creator == null) && creator.Assembly != null)
		{
			CSFarm farm = creator.Assembly.Farm;
			farm.RestoreWateringPlant(plant);
		}
	}

	private void RPC_S2C_CLN_PlantClean(uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
		stream.Read<int>(new object[0]);
		FarmPlantLogic plant = stream.Read<FarmPlantLogic>(new object[0]);
		CSMgCreator creator = MultiColonyManager.GetCreator(base.TeamId);
		if (!(creator == null) && creator.Assembly != null)
		{
			CSFarm farm = creator.Assembly.Farm;
			farm.RestoreCleaningPlant(plant);
		}
	}

	private void RPC_S2C_CLN_SetProcessingIndex(uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
		int processingIndex = stream.Read<int>(new object[0]);
		CSMgCreator creator = MultiColonyManager.GetCreator(base.TeamId, createNewIfNone: false);
		if (!(creator != null))
		{
			return;
		}
		CSPersonnel[] npcs = creator.GetNpcs();
		CSPersonnel[] array = npcs;
		foreach (CSPersonnel cSPersonnel in array)
		{
			if (cSPersonnel != null && cSPersonnel.m_Npc != null && base.Id == cSPersonnel.m_Npc.Id)
			{
				cSPersonnel.ProcessingIndex = processingIndex;
			}
		}
	}

	private void RPC_S2C_CLN_SetIsProcessing(uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
		bool isProcessing = stream.Read<bool>(new object[0]);
		CSMgCreator creator = MultiColonyManager.GetCreator(base.TeamId, createNewIfNone: false);
		if (!(creator != null))
		{
			return;
		}
		CSPersonnel[] npcs = creator.GetNpcs();
		CSPersonnel[] array = npcs;
		foreach (CSPersonnel cSPersonnel in array)
		{
			if (cSPersonnel != null && cSPersonnel.m_Npc != null && base.Id == cSPersonnel.m_Npc.Id)
			{
				cSPersonnel.IsProcessing = isProcessing;
			}
		}
	}

	protected override void OnPEInstantiate(uLink.NetworkMessageInfo info)
	{
		_id = info.networkView.initialData.Read<int>(new object[0]);
		_scale = info.networkView.initialData.Read<float>(new object[0]);
		_externId = info.networkView.initialData.Read<int>(new object[0]);
		npcType = info.networkView.initialData.Read<int>(new object[0]);
		base.authId = info.networkView.initialData.Read<int>(new object[0]);
		isStand = info.networkView.initialData.Read<bool>(new object[0]);
		rotY = info.networkView.initialData.Read<float>(new object[0]);
		bForcedServant = info.networkView.initialData.Read<bool>(new object[0]);
		customName = info.networkView.initialData.Read<string>(new object[0]);
		base._pos = (spawnPos = base.transform.position);
		originTeam = (_teamId = -1);
		mLordPlayerId = -1;
	}

	protected override void OnPEStart()
	{
		BindSkAction();
		BindAction(EPacketType.PT_NPC_InitData, RPC_S2C_InitData);
		BindAction(EPacketType.PT_InGame_SetController, RPC_S2C_SetController);
		BindAction(EPacketType.PT_InGame_LostController, RPC_S2C_LostController);
		BindAction(EPacketType.PT_NPC_HPChange, RPC_S2C_ApplyHpChange);
		BindAction(EPacketType.PT_NPC_Death, RPC_S2C_Death);
		BindAction(EPacketType.PT_NPC_Turn, RPC_S2C_Turn);
		BindAction(EPacketType.PT_NPC_Revive, RPC_S2C_NpcRevive);
		BindAction(EPacketType.PT_NPC_Dismiss, RPC_S2C_DismissByPlayer);
		BindAction(EPacketType.PT_NPC_Recruit, RPC_S2C_RecruitByPlayer);
		BindAction(EPacketType.PT_NPC_WorkState, RPC_S2C_SyncWorkState);
		BindAction(EPacketType.PT_NPC_Move, RPC_NPCMove);
		BindAction(EPacketType.PT_NPC_ForceMove, NPC_ForceMove);
		BindAction(EPacketType.PT_NPC_RotY, base.RPC_S2C_NetRotation);
		BindAction(EPacketType.PT_NPC_MissionFlag, RPC_MissionFlag);
		BindAction(EPacketType.PT_NPC_PacageIndex, RPC_PackageIndex);
		BindAction(EPacketType.PT_NPC_Money, RPC_S2C_SyncMoney);
		BindAction(EPacketType.PT_NPC_SyncTeamID, RPC_S2C_SyncTeamID);
		BindAction(EPacketType.PT_NPC_Equips, RPC_S2C_NpcEquips);
		BindAction(EPacketType.PT_NPC_Items, RPC_S2C_NpcItems);
		BindAction(EPacketType.PT_NPC_Mission, RPC_S2C_NpcMisstion);
		BindAction(EPacketType.PT_NPC_MissionState, RPC_S2C_MissionState);
		BindAction(EPacketType.PT_NPC_RequestAiOp, RPC_S2C_RequestAiOp);
		BindAction(EPacketType.PT_NPC_Mount, RPC_S2C_Mount);
		BindAction(EPacketType.PT_NPC_UpdateCampsite, RPC_S2C_UpdateCampsite);
		BindAction(EPacketType.PT_NPC_State, RPC_S2C_State);
		BindAction(EPacketType.PT_InGame_GetOnVehicle, RPC_S2C_GetOn);
		BindAction(EPacketType.PT_InGame_GetOffVehicle, RPC_S2C_GetOff);
		BindAction(EPacketType.PT_NPC_ExternData, RPC_S2C_ExternData);
		BindAction(EPacketType.PT_NPC_Skill, RPC_S2C_NpcSkill);
		BindAction(EPacketType.PT_AI_RifleAim, base.RPC_S2C_RifleAim);
		BindAction(EPacketType.PT_AI_IKPosWeight, base.RPC_S2C_SetIKPositionWeight);
		BindAction(EPacketType.PT_AI_IKPosition, base.RPC_S2C_SetIKPosition);
		BindAction(EPacketType.PT_AI_IKRotWeight, base.RPC_S2C_SetIKRotationWeight);
		BindAction(EPacketType.PT_AI_IKRotation, base.RPC_S2C_SetIKRotation);
		BindAction(EPacketType.PT_AI_BoolString, RPC_S2C_SetBool_String);
		BindAction(EPacketType.PT_AI_BoolInt, RPC_S2C_SetBool_Int);
		BindAction(EPacketType.PT_AI_VectorString, RPC_S2C_SetVector_String);
		BindAction(EPacketType.PT_AI_VectorInt, RPC_S2C_SetVector_Int);
		BindAction(EPacketType.PT_AI_IntString, RPC_S2C_SetInteger_String);
		BindAction(EPacketType.PT_AI_IntInt, RPC_S2C_SetInteger_Int);
		BindAction(EPacketType.PT_AI_LayerWeight, RPC_S2C_SetLayerWeight);
		BindAction(EPacketType.PT_AI_LookAtWeight, RPC_S2C_SetLookAtWeight);
		BindAction(EPacketType.PT_AI_LookAtPos, RPC_S2C_SetLookAtPosition);
		BindAction(EPacketType.PT_CL_CLN_InitData, RPC_S2C_CLN_InitData);
		BindAction(EPacketType.PT_CL_CLN_SetState, RPC_S2C_CLN_SetState);
		BindAction(EPacketType.PT_CL_CLN_SetDwellingsID, RPC_S2C_CLN_SetDwellingsID);
		BindAction(EPacketType.PT_CL_CLN_SetWorkRoomID, RPC_S2C_CLN_SetWorkRoomID);
		BindAction(EPacketType.PT_CL_CLN_SetOccupation, RPC_S2C_CLN_SetOccupation);
		BindAction(EPacketType.PT_CL_CLN_SetWorkMode, RPC_S2C_CLN_SetWorkMode);
		BindAction(EPacketType.PT_CL_CLN_PlantGetBack, RPC_S2C_CLN_PlantGetBack);
		BindAction(EPacketType.PT_CL_CLN_RemoveNpc, RPC_S2C_CLN_RemoveNpc);
		BindAction(EPacketType.PT_CL_CLN_PlantPutOut, RPC_S2C_CLN_PlantPutOut);
		BindAction(EPacketType.PT_CL_CLN_PlantUpdateInfo, RPC_S2C_CLN_PlantUpdateInfo);
		BindAction(EPacketType.PT_CL_CLN_PlantClear, RPC_S2C_CLN_PlantClear);
		BindAction(EPacketType.PT_CL_CLN_PlantWater, RPC_S2C_CLN_PlantWater);
		BindAction(EPacketType.PT_CL_CLN_PlantClean, RPC_S2C_CLN_PlantClean);
		BindAction(EPacketType.PT_CL_CLN_SetProcessingIndex, RPC_S2C_CLN_SetProcessingIndex);
		BindAction(EPacketType.PT_CL_CLN_SetIsProcessing, RPC_S2C_CLN_SetIsProcessing);
		BindAction(EPacketType.PT_InGame_PutOnEquipment, RPC_S2C_PutOnEquipment);
		BindAction(EPacketType.PT_InGame_TakeOffEquipment, RPC_S2C_TakeOffEquipment);
		BindAction(EPacketType.PT_InGame_DeadObjItem, base.RPC_C2S_ResponseDeadObjItem);
		BindAction(EPacketType.PT_NPC_ResetPosition, base.RPC_S2C_ResetPosition);
		BindAction(EPacketType.PT_NPC_ForcedServant, RPC_S2C_ForcedServant);
		BindAction(EPacketType.PT_AI_SetBool, RPC_S2C_SetBool);
		BindAction(EPacketType.PT_AI_SetTrigger, RPC_S2C_SetTrigger);
		BindAction(EPacketType.PT_AI_SetMoveMode, RPC_S2C_SetMoveMode);
		BindAction(EPacketType.PT_AI_HoldWeapon, RPC_S2C_HoldWeapon);
		BindAction(EPacketType.PT_AI_SwitchHoldWeapon, RPC_S2C_SwitchHoldWeapon);
		BindAction(EPacketType.PT_AI_SwordAttack, RPC_S2C_SwordAttack);
		BindAction(EPacketType.PT_AI_TwoHandWeaponAttack, RPC_S2C_TwoHandWeaponAttack);
		BindAction(EPacketType.PT_AI_SetIKAim, RPC_S2C_SetIKAim);
		BindAction(EPacketType.PT_AI_Fadein, RPC_S2C_Fadein);
		BindAction(EPacketType.PT_AI_Fadeout, RPC_S2C_Fadeout);
		BindAction(EPacketType.PT_Common_ScenarioId, base.RPC_S2C_ScenarioId);
		BindAction(EPacketType.PT_NPC_SelfUseItem, RPC_S2C_SelfUseItem);
		BindAction(EPacketType.PT_NPC_AddEnemyLock, RPC_S2C_AddEnemyLock);
		BindAction(EPacketType.PT_NPC_RemoveEnemyLock, RPC_S2C_RemoveEnemyLock);
		BindAction(EPacketType.PT_NPC_ClearEnemyLocked, RPC_S2C_ClearEnemyLocked);
		RPCServer(EPacketType.PT_NPC_InitData);
	}

	protected override void OnPEDestroy()
	{
		if (npcType == 2 || npcType == 3)
		{
		}
		if (null != _entity)
		{
			_entity.NpcCmpt.OnAddEnemyLock -= OnAddEnemyLock;
			_entity.NpcCmpt.OnRemoveEnemyLock -= OnRemoveEnemyLock;
			_entity.NpcCmpt.OnClearEnemyLocked -= OnClearEnemyLocked;
		}
		base.OnPEDestroy();
	}

	internal void CreateAdNpc(byte[] customData, byte[] missionData, int missionState)
	{
		if (!NpcMissionDataRepository.m_AdRandMisNpcData.ContainsKey(base.ExternId))
		{
			return;
		}
		AdNpcData adNpcData = NpcMissionDataRepository.m_AdRandMisNpcData[base.ExternId];
		if (adNpcData == null)
		{
			return;
		}
		_entity = PeSingleton<PeEntityCreator>.Instance.CreateRandomNpcForNet(adNpcData.mRnpc_ID, base.Id, Vector3.zero, Quaternion.identity, Vector3.one);
		if (null == _entity)
		{
			return;
		}
		CustomCharactor.CustomData customData2 = new CustomCharactor.CustomData();
		customData2.Deserialize(customData);
		PeEntityCreator.ApplyCustomCharactorData(_entity, customData2);
		_viewTrans = _entity.peTrans;
		if (null == _viewTrans)
		{
			Debug.LogError("entity has no ViewCmpt");
			return;
		}
		_viewTrans.position = base.transform.position;
		_viewTrans.rotation = base.transform.rotation;
		_entity.SetBirthPos(spawnPos);
		_move = _entity.GetCmpt<Motion_Move>();
		NetCmpt netCmpt = _entity.GetCmpt<NetCmpt>();
		if (null == netCmpt)
		{
			netCmpt = _entity.Add<NetCmpt>();
		}
		netCmpt.network = this;
		EntityInfoCmpt cmpt = _entity.GetCmpt<EntityInfoCmpt>();
		if (null != cmpt)
		{
			base.gameObject.name = $"{cmpt.characterName}, TemplateId:{base.ExternId}, Id:{base.Id}";
		}
		useData = new NpcMissionData();
		useData.Deserialize(missionData);
		for (int i = 0; i < adNpcData.m_CSRecruitMissionList.Count; i++)
		{
			useData.m_CSRecruitMissionList.Add(adNpcData.m_CSRecruitMissionList[i]);
		}
		NpcMissionDataRepository.AddMissionData(base.Id, useData);
		_entity.SetUserData(useData);
		if (npcType == 2 || npcType == 3)
		{
		}
		if (isStand)
		{
			VArtifactUtil.SetNpcStandRot(_entity, rotY, isStand: true);
		}
		OnSpawned(_entity.GetGameObject());
	}

	private void CreateCustomNpc(byte[] customData, byte[] missionData, int missionState)
	{
		NpcProtoDb.Item item = NpcProtoDb.Get(base.ExternId);
		if (item == null || !NpcMissionDataRepository.dicMissionData.ContainsKey(item.sort))
		{
			return;
		}
		NpcMissionData npcMissionData = NpcMissionDataRepository.dicMissionData[item.sort];
		if (npcMissionData == null)
		{
			return;
		}
		_entity = PeSingleton<PeEntityCreator>.Instance.CreateNpcForNet(base.Id, base.ExternId, Vector3.zero, Quaternion.identity, Vector3.one);
		if (null == _entity)
		{
			return;
		}
		_entity.ExtSetName(new CharacterName(customName));
		_viewTrans = _entity.peTrans;
		if (null == _viewTrans)
		{
			Debug.LogError("entity has no ViewCmpt");
			return;
		}
		_viewTrans.position = base.transform.position;
		_viewTrans.rotation = base.transform.rotation;
		_entity.SetBirthPos(spawnPos);
		_move = _entity.GetCmpt<Motion_Move>();
		NetCmpt netCmpt = _entity.GetCmpt<NetCmpt>();
		if (null == netCmpt)
		{
			netCmpt = _entity.Add<NetCmpt>();
		}
		netCmpt.network = this;
		EntityInfoCmpt cmpt = _entity.GetCmpt<EntityInfoCmpt>();
		if (null != cmpt)
		{
			base.gameObject.name = $"{cmpt.characterName}, TemplateId:{base.ExternId}, Id:{base.Id}";
		}
		useData = new NpcMissionData();
		useData.Deserialize(missionData);
		for (int i = 0; i < npcMissionData.m_CSRecruitMissionList.Count; i++)
		{
			useData.m_CSRecruitMissionList.Add(npcMissionData.m_CSRecruitMissionList[i]);
		}
		NpcMissionDataRepository.AddMissionData(base.Id, useData);
		_entity.SetUserData(useData);
		if (npcType == 2 || npcType == 3)
		{
		}
		if (isStand)
		{
			VArtifactUtil.SetNpcStandRot(_entity, rotY, isStand: true);
		}
		OnSpawned(_entity.GetGameObject());
	}

	internal void CreateStoryNpc(byte[] customData, byte[] missionData, int missionState)
	{
		if (!NpcMissionDataRepository.dicMissionData.ContainsKey(base.Id))
		{
			return;
		}
		NpcMissionData npcMissionData = NpcMissionDataRepository.dicMissionData[base.Id];
		if (npcMissionData == null)
		{
			return;
		}
		_entity = PeSingleton<PeEntityCreator>.Instance.CreateNpcForNet(base.Id, base.ExternId, Vector3.zero, Quaternion.identity, Vector3.one);
		if (null == _entity)
		{
			return;
		}
		_viewTrans = _entity.peTrans;
		if (null == _viewTrans)
		{
			Debug.LogError("entity has no ViewCmpt");
			return;
		}
		_viewTrans.position = base.transform.position;
		_viewTrans.rotation = base.transform.rotation;
		_move = _entity.GetCmpt<Motion_Move>();
		NetCmpt netCmpt = _entity.GetCmpt<NetCmpt>();
		if (null == netCmpt)
		{
			netCmpt = _entity.Add<NetCmpt>();
		}
		netCmpt.network = this;
		EntityInfoCmpt cmpt = _entity.GetCmpt<EntityInfoCmpt>();
		if (null != cmpt)
		{
			base.gameObject.name = $"{cmpt.characterName}, TemplateId:{base.ExternId}, Id:{base.Id}";
		}
		useData = new NpcMissionData();
		useData.Deserialize(missionData);
		for (int i = 0; i < npcMissionData.m_CSRecruitMissionList.Count; i++)
		{
			useData.m_CSRecruitMissionList.Add(npcMissionData.m_CSRecruitMissionList[i]);
		}
		NpcMissionDataRepository.AddMissionData(base.Id, useData);
		_entity.SetUserData(useData);
		if (npcType == 2 || npcType == 3)
		{
		}
		if (isStand)
		{
			VArtifactUtil.SetNpcStandRot(_entity, rotY, isStand: true);
		}
		OnSpawned(_entity.GetGameObject());
	}

	internal void CreateRdNpc(byte[] customData, byte[] missionData, int missionState)
	{
		if (!NpcMissionDataRepository.dicMissionData.ContainsKey(base.Id))
		{
			return;
		}
		NpcMissionData npcMissionData = NpcMissionDataRepository.dicMissionData[base.Id];
		if (npcMissionData == null)
		{
			return;
		}
		_entity = PeSingleton<PeEntityCreator>.Instance.CreateRandomNpcForNet(npcMissionData.m_Rnpc_ID, base.Id, Vector3.zero, Quaternion.identity, Vector3.one);
		if (null == _entity)
		{
			return;
		}
		CustomCharactor.CustomData customData2 = new CustomCharactor.CustomData();
		customData2.Deserialize(customData);
		PeEntityCreator.ApplyCustomCharactorData(_entity, customData2);
		_viewTrans = _entity.peTrans;
		if (null == _viewTrans)
		{
			Debug.LogError("entity has no ViewCmpt");
			return;
		}
		_viewTrans.position = base.transform.position;
		_viewTrans.rotation = base.transform.rotation;
		_move = _entity.GetCmpt<Motion_Move>();
		NetCmpt netCmpt = _entity.GetCmpt<NetCmpt>();
		if (null == netCmpt)
		{
			netCmpt = _entity.Add<NetCmpt>();
		}
		netCmpt.network = this;
		EntityInfoCmpt cmpt = _entity.GetCmpt<EntityInfoCmpt>();
		if (null != cmpt)
		{
			base.gameObject.name = $"{cmpt.characterName}, TemplateId:{base.ExternId}, Id:{base.Id}";
		}
		useData = new NpcMissionData();
		useData.Deserialize(missionData);
		for (int i = 0; i < npcMissionData.m_CSRecruitMissionList.Count; i++)
		{
			useData.m_CSRecruitMissionList.Add(npcMissionData.m_CSRecruitMissionList[i]);
		}
		NpcMissionDataRepository.AddMissionData(base.Id, useData);
		_entity.SetUserData(useData);
		if (npcType == 2 || npcType == 3)
		{
		}
		if (isStand)
		{
			VArtifactUtil.SetNpcStandRot(_entity, rotY, isStand: true);
		}
		OnSpawned(_entity.GetGameObject());
	}

	public override void OnSpawned(GameObject obj)
	{
		base.OnSpawned(obj);
		_entity.NpcCmpt.OnAddEnemyLock += OnAddEnemyLock;
		_entity.NpcCmpt.OnRemoveEnemyLock += OnRemoveEnemyLock;
		_entity.NpcCmpt.OnClearEnemyLocked += OnClearEnemyLocked;
	}

	private void InitExternData(byte[] externData)
	{
		Serialize.Import(externData, delegate(BinaryReader r)
		{
			string givenName = BufferHelper.ReadString(r);
			string familyName = BufferHelper.ReadString(r);
			if (null != _entity)
			{
				_entity.ExtSetName(new CharacterName(givenName, familyName));
			}
			BufferHelper.ReadInt32(r);
			BufferHelper.ReadBoolean(r);
			BufferHelper.ReadSingle(r);
			BufferHelper.ReadInt32(r);
			BufferHelper.ReadInt32(r);
			BufferHelper.ReadInt32(r);
		});
	}

	protected override IEnumerator SyncMove()
	{
		base._pos = base.transform.position;
		base.rot = base.transform.rotation;
		while (base.hasOwnerAuth)
		{
			NpcMove();
			yield return new WaitForSeconds(0f);
		}
	}

	protected override void CheckAuthority()
	{
		if (!base.hasOwnerAuth || (!(null == npcCmpt) && !npcCmpt.IsFollower))
		{
			base.CheckAuthority();
		}
	}

	public void NpcMove()
	{
		base._pos = base.transform.position;
		base.rot = base.transform.rotation;
		if (null != _viewTrans)
		{
			Vector3 position = _viewTrans.position;
			base.transform.position = position;
			base._pos = position;
			Quaternion rotation = _viewTrans.rotation;
			base.transform.rotation = rotation;
			base.rot = rotation;
		}
		if (!(null == _move) && (Vector3.SqrMagnitude(_move.velocity - _syncAttr.Speed) > 0.1f || Mathf.Abs(_syncAttr.Pos.x - base._pos.x) > 0.1f || Mathf.Abs(_syncAttr.Pos.y - base._pos.y) > 0.1f || Mathf.Abs(_syncAttr.Pos.z - base._pos.z) > 0.1f || Mathf.Abs(_syncAttr.EulerY - base.rot.eulerAngles.y) > 0.1f))
		{
			int num = VCUtils.CompressEulerAngle(base.rot.eulerAngles);
			URPCServer(EPacketType.PT_NPC_Move, base._pos, (byte)_move.speed, num, GameTime.Timer.Second);
			_syncAttr.Pos = base._pos;
			_syncAttr.Speed = _move.velocity;
			_syncAttr.EulerY = base.rot.eulerAngles.y;
			if (_move is Motion_Move_Human)
			{
				(_move as Motion_Move_Human).NetMovePos = base._pos;
			}
		}
	}

	public override void InitForceData()
	{
		if (!PeGameMgr.IsCustom && null != _entity)
		{
			if (mLordPlayerId != -1)
			{
				_entity.SetAttribute(AttribType.DefaultPlayerID, mLordPlayerId, offEvent: false);
			}
			else
			{
				mColonyLordPlayerId = ((mColonyLordPlayerId != -1) ? mColonyLordPlayerId : ((base.TeamId != PlayerNetwork.mainPlayer.TeamId && (base.TeamId == -1 || Singleton<ForceSetting>.Instance.Conflict(base.TeamId, PlayerNetwork.mainPlayer.Id))) ? (-1) : PlayerNetwork.mainPlayer.Id));
				if (mColonyLordPlayerId != -1)
				{
					_entity.SetAttribute(AttribType.DefaultPlayerID, mColonyLordPlayerId, offEvent: false);
				}
				else
				{
					_entity.SetAttribute(AttribType.DefaultPlayerID, 2f, offEvent: false);
				}
			}
			if (PeGameMgr.IsAdventure)
			{
				_entity.SetAttribute(AttribType.CampID, 5f, offEvent: false);
				_entity.SetAttribute(AttribType.DamageID, 5f, offEvent: false);
			}
		}
		ResetTarget();
	}

	private void OnAddEnemyLock(int id)
	{
		if (base.hasOwnerAuth)
		{
			RPCServer(EPacketType.PT_NPC_AddEnemyLock, id);
		}
	}

	private void OnRemoveEnemyLock(int id)
	{
		if (base.hasOwnerAuth)
		{
			RPCServer(EPacketType.PT_NPC_RemoveEnemyLock, id);
		}
	}

	private void OnClearEnemyLocked()
	{
		if (base.hasOwnerAuth)
		{
			RPCServer(EPacketType.PT_NPC_ClearEnemyLocked);
		}
	}

	private void ResetTarget()
	{
		if (base.hasOwnerAuth && null != _entity)
		{
			TargetCmpt cmpt = _entity.GetCmpt<TargetCmpt>();
			if (null != cmpt)
			{
				cmpt.ClearEnemy();
			}
		}
	}

	private IEnumerator GetOnVehicle(int id)
	{
		while (true)
		{
			DriveCreation = NetworkInterface.Get<CreationNetwork>(id);
			if (null != DriveCreation)
			{
				if (DriveCreation.GetOn(base.Runner, SeatIndex))
				{
					DriveCreation.AddPassanger(this);
					break;
				}
				yield return null;
				continue;
			}
			break;
		}
	}

	public bool WaitForInitNpc()
	{
		if (!PlayerNetwork.mainPlayer._initOk || StroyManager.Instance == null)
		{
			return false;
		}
		StroyManager.Instance.InitMission(base.Id);
		_npcMissionInited = true;
		if (null != _viewTrans && PeGameMgr.IsMultiStory)
		{
			_viewTrans.rotation = base.transform.rotation;
		}
		MissionManager.Instance.m_PlayerMission.UpdateNpcMissionTex(_entity);
		return true;
	}

	public bool WaitForMountNpc()
	{
		if (PeSingleton<EntityMgr>.Instance.Get(_mountId) == null || npcCmpt == null)
		{
			return false;
		}
		PeTrans component = PeSingleton<EntityMgr>.Instance.Get(_mountId).GetComponent<PeTrans>();
		if (component == null)
		{
			return false;
		}
		Transform child = PEUtil.GetChild(component.existent, "CarryUp");
		if (child == null)
		{
			return false;
		}
		npcCmpt.MountID = _mountId;
		return true;
	}

	public bool WaitForInitNpcMission()
	{
		if (null == PlayerNetwork.mainPlayer || !PlayerNetwork.mainPlayer._initOk)
		{
			return false;
		}
		_npcMissionInited = true;
		MissionManager.Instance.m_PlayerMission.UpdateNpcMissionTex(_entity);
		return true;
	}

	public void GetOffVehicle(Vector3 pos)
	{
		if (null != DriveCreation)
		{
			DriveCreation.GetOff(pos);
			DriveCreation = null;
		}
	}

	private void GetData(byte[] buffer)
	{
		if (buffer == null)
		{
			return;
		}
		using MemoryStream input = new MemoryStream(buffer);
		using BinaryReader reader = new BinaryReader(input);
		BufferHelper.ReadInt32(reader);
		BufferHelper.ReadInt32(reader);
		BufferHelper.ReadInt32(reader);
		BufferHelper.ReadInt32(reader);
		BufferHelper.ReadSingle(reader);
		BufferHelper.ReadSingle(reader);
		BufferHelper.ReadSingle(reader);
		BufferHelper.ReadInt32(reader);
		int num = BufferHelper.ReadInt32(reader);
		for (int i = 0; i < num; i++)
		{
			BufferHelper.ReadInt32(reader);
		}
		num = BufferHelper.ReadInt32(reader);
		for (int j = 0; j < num; j++)
		{
			BufferHelper.ReadInt32(reader);
		}
		num = BufferHelper.ReadInt32(reader);
		for (int k = 0; k < num; k++)
		{
			BufferHelper.ReadInt32(reader);
		}
		BufferHelper.ReadString(reader);
		BufferHelper.ReadString(reader);
		num = BufferHelper.ReadInt32(reader);
		for (int l = 0; l < num; l++)
		{
			BufferHelper.ReadInt32(reader);
		}
		BufferHelper.ReadColor(reader, out var _value);
		BufferHelper.ReadColor(reader, out _value);
		BufferHelper.ReadColor(reader, out _value);
		BufferHelper.ReadSingle(reader);
		BufferHelper.ReadSingle(reader);
		useData = new NpcMissionData();
		useData.mCurComMisNum = BufferHelper.ReadByte(reader);
		useData.mCompletedMissionCount = BufferHelper.ReadInt32(reader);
		useData.m_RandomMission = BufferHelper.ReadInt32(reader);
		useData.m_RecruitMissionNum = BufferHelper.ReadInt32(reader);
		useData.m_Rnpc_ID = BufferHelper.ReadInt32(reader);
		useData.m_CurMissionGroup = BufferHelper.ReadInt32(reader);
		useData.m_CurGroupTimes = BufferHelper.ReadInt32(reader);
		useData.m_QCID = BufferHelper.ReadInt32(reader);
		BufferHelper.ReadVector3(reader, out useData.m_Pos);
		num = BufferHelper.ReadInt32(reader);
		for (int m = 0; m < num; m++)
		{
			int item = BufferHelper.ReadInt32(reader);
			useData.m_MissionList.Add(item);
		}
		num = BufferHelper.ReadInt32(reader);
		for (int n = 0; n < num; n++)
		{
			int item2 = BufferHelper.ReadInt32(reader);
			useData.m_MissionListReply.Add(item2);
		}
		num = BufferHelper.ReadInt32(reader);
		for (int num2 = 0; num2 < num; num2++)
		{
			int item3 = BufferHelper.ReadInt32(reader);
			useData.m_RecruitMissionList.Add(item3);
		}
	}

	private void RequestNpcEquips()
	{
		RPCServer(EPacketType.PT_NPC_Equips);
	}

	private void RequestNpcItems()
	{
		RPCServer(EPacketType.PT_NPC_Items);
	}

	private void RequestExternData()
	{
		RPCServer(EPacketType.PT_NPC_ExternData);
	}

	private void RequestNpcSkill()
	{
		RPCServer(EPacketType.PT_NPC_Skill);
	}

	public void RequestNpcUseItem(int objId)
	{
		RPCServer(EPacketType.PT_NPC_SelfUseItem, objId);
	}

	public void RequestResetPosition(Vector3 pos)
	{
		RPCServer(EPacketType.PT_NPC_ResetPosition, pos);
	}

	public void RequestMount(Transform trans)
	{
		RPCServer(EPacketType.PT_NPC_Mount, trans.position, trans.rotation.y);
	}

	public void RequestUpdateCampsite(bool val)
	{
		RPCServer(EPacketType.PT_NPC_UpdateCampsite, val);
	}

	public void RequestState(int state)
	{
		RPCServer(EPacketType.PT_NPC_State, state);
	}

	public void RequestGetOn(int creationId, int index)
	{
		RPCServer(EPacketType.PT_InGame_GetOnVehicle, creationId, index);
	}

	public void RequestGetOff()
	{
		RPCServer(EPacketType.PT_InGame_GetOffVehicle);
	}

	protected override void RPC_S2C_InitData(uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
		Vector3 vector = stream.Read<Vector3>(new object[0]);
		base.transform.position = vector;
		base._pos = vector;
		Quaternion rotation = stream.Read<Quaternion>(new object[0]);
		base.transform.rotation = rotation;
		base.rot = rotation;
		byte[] customData = stream.Read<byte[]>(new object[0]);
		byte[] missionData = stream.Read<byte[]>(new object[0]);
		stream.Read<bool>(new object[0]);
		base.authId = stream.Read<int>(new object[0]);
		int missionState = stream.Read<int>(new object[0]);
		int num = stream.Read<int>(new object[0]);
		bool value = stream.Read<bool>(new object[0]);
		bForcedServant = stream.Read<bool>(new object[0]);
		_syncAttr.Pos = base._pos;
		_syncAttr.EulerY = base.rot.eulerAngles.y;
		if (PeGameMgr.IsMultiStory)
		{
			if (base.Id >= 9200)
			{
				CreateRdNpc(customData, missionData, missionState);
			}
			else
			{
				CreateStoryNpc(customData, missionData, missionState);
			}
			GlobalBehaviour.RegisterEvent(WaitForInitNpc);
		}
		else if (PeGameMgr.IsCustom)
		{
			CreateCustomNpc(customData, missionData, missionState);
		}
		else
		{
			CreateAdNpc(customData, missionData, missionState);
			GlobalBehaviour.RegisterEvent(WaitForInitNpcMission);
		}
		if (_move is Motion_Move_Human)
		{
			(_move as Motion_Move_Human).NetMovePos = base._pos;
		}
		RequestNpcEquips();
		base.OnSkAttrInitEvent += InitForceData;
		if (num != 0 && base.animatorCmpt != null)
		{
			base.animatorCmpt.SetBool(num, value);
		}
		if (!base.hasOwnerAuth && _move != null)
		{
			_move.NetMoveTo(base._pos, Vector3.zero, immediately: true);
		}
		if (base.Id < 9000 && !PeGameMgr.IsMultiCustom)
		{
			MissionManager.Instance.m_PlayerMission.adId_entityId[base.Id] = base.Id;
		}
	}

	protected override void RPC_S2C_ApplyHpChange(uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
		float damage = stream.Read<float>(new object[0]);
		int lifeLeft = stream.Read<int>(new object[0]);
		int id = stream.Read<int>(new object[0]);
		CommonInterface caster = null;
		NetworkInterface networkInterface = NetworkInterface.Get(id);
		if (null != networkInterface)
		{
			caster = networkInterface.Runner;
		}
		if (null != base.Runner)
		{
			base.Runner.NetworkApplyDamage(caster, damage, lifeLeft);
		}
	}

	protected override void RPC_S2C_Turn(uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
		float y = stream.Read<float>(new object[0]);
		Quaternion rotation = Quaternion.Euler(0f, y, 0f);
		base.transform.rotation = rotation;
		base.rot = rotation;
		if (null != _viewTrans)
		{
			_viewTrans.rotation = base.transform.rotation;
		}
	}

	private void RPC_S2C_NpcRevive(uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
		float attrValue = stream.Read<float>(new object[0]);
		Vector3 vector = stream.Read<Vector3>(new object[0]);
		base.transform.position = vector;
		base._pos = vector;
		death = false;
		if (!(null == _entity))
		{
			_entity.SetAttribute(AttribType.Hp, attrValue);
			if (null != _move)
			{
				_move.NetMoveTo(base.transform.position, Vector3.zero, immediately: true);
			}
			MotionMgrCmpt cmpt = base.Runner.SkEntityPE.Entity.GetCmpt<MotionMgrCmpt>();
			if (cmpt != null)
			{
				PEActionParamB param = PEActionParamB.param;
				param.b = true;
				cmpt.DoActionImmediately(PEActionType.Revive, param);
			}
		}
	}

	private void RPC_S2C_DismissByPlayer(uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
		Vector3 vector = stream.Read<Vector3>(new object[0]);
		base.transform.position = vector;
		base._pos = vector;
		int teamId = stream.Read<int>(new object[0]);
		PeEntity peEntity = PeSingleton<EntityMgr>.Instance.Get(base.Id);
		if (null != peEntity)
		{
			if (null != PeSingleton<PeCreature>.Instance.mainPlayer && mLordPlayerId == PeSingleton<PeCreature>.Instance.mainPlayer.Id)
			{
				peEntity.SetFollower(bFlag: false);
			}
			else
			{
				NpcCmpt cmpt = peEntity.GetCmpt<NpcCmpt>();
				if (null != cmpt)
				{
					cmpt.SetServantLeader(null);
				}
			}
			peEntity.Dismiss();
			if (base._pos.x > 10f && PeGameMgr.IsMultiStory)
			{
				peEntity.NpcCmpt.FixedPointPos = base._pos;
			}
		}
		peEntity.NpcCmpt.Req_MoveToPosition(base._pos, 1f, isForce: true, SpeedState.Run);
		_teamId = teamId;
		mLordPlayerId = -1;
		InitForceData();
	}

	private void RPC_S2C_RecruitByPlayer(uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
		int num2 = (base.authId = stream.Read<int>(new object[0]));
		mLordPlayerId = num2;
		_teamId = stream.Read<int>(new object[0]);
		bool flag = stream.Read<bool>(new object[0]);
		ResetContorller();
		PeEntity peEntity = PeSingleton<EntityMgr>.Instance.Get(base.Id);
		if (!(null != peEntity))
		{
			return;
		}
		if (null != PeSingleton<PeCreature>.Instance.mainPlayer && mLordPlayerId == PeSingleton<PeCreature>.Instance.mainPlayer.Id)
		{
			if (!flag)
			{
				peEntity.SetFollower(bFlag: true);
			}
		}
		else
		{
			PeEntity peEntity2 = PeSingleton<EntityMgr>.Instance.Get(mLordPlayerId);
			if (null != peEntity2)
			{
				ServantLeaderCmpt cmpt = peEntity2.GetCmpt<ServantLeaderCmpt>();
				NpcCmpt cmpt2 = peEntity.GetCmpt<NpcCmpt>();
				if (null != cmpt2)
				{
					cmpt2.SetServantLeader(cmpt);
				}
			}
		}
		peEntity.SetBirthPos(peEntity.position);
		peEntity.CmdStopTalk();
		StroyManager.Instance.RemoveReq(peEntity, EReqType.Dialogue);
		peEntity.Recruit();
		peEntity.SetShopIcon(null);
	}

	private void RPC_S2C_SyncWorkState(uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
		if (!(null == base.Runner))
		{
			stream.TryRead<int>(out var _);
			stream.TryRead<Vector3>(out var _);
			stream.TryRead<float>(out var _);
			if (base.hasOwnerAuth)
			{
			}
		}
	}

	private void RPC_NPCMove(uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
		Vector3 vector = stream.Read<Vector3>(new object[0]);
		base.transform.position = vector;
		base._pos = vector;
		byte speed = stream.Read<byte>(new object[0]);
		int data = stream.Read<int>(new object[0]);
		double controllerTime = stream.Read<double>(new object[0]);
		Vector3 euler = VCUtils.UncompressEulerAngle(data);
		Quaternion rotation = Quaternion.Euler(euler);
		base.transform.rotation = rotation;
		base.rot = rotation;
		if (!base.hasOwnerAuth)
		{
			if (null != _move)
			{
				_move.AddNetTransInfo(base._pos, base.transform.rotation.eulerAngles, (SpeedState)speed, controllerTime);
			}
			if (null != npcCmpt && npcCmpt.Req_GetRequest(EReqType.FollowPath) is RQFollowPath rQFollowPath && IsReached(rQFollowPath.path[rQFollowPath.path.Length - 1], base._pos, Is3D: true) && !rQFollowPath.isLoop)
			{
				npcCmpt.Req_Remove(EReqType.FollowPath);
			}
		}
	}

	private void NPC_ForceMove(uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
		Vector3 vector = stream.Read<Vector3>(new object[0]);
		base.transform.position = vector;
		base._pos = vector;
		stream.Read<byte>(new object[0]);
		int data = stream.Read<int>(new object[0]);
		stream.Read<double>(new object[0]);
		Vector3 euler = VCUtils.UncompressEulerAngle(data);
		Quaternion rotation = Quaternion.Euler(euler);
		base.transform.rotation = rotation;
		base.rot = rotation;
		if (_entity.peTrans.position == base._pos)
		{
			_entity.peTrans.position = base._pos;
		}
		if (_entity.peTrans.rotation == base.rot)
		{
			_entity.peTrans.rotation = base.rot;
		}
	}

	internal bool IsReached(Vector3 pos, Vector3 targetPos, bool Is3D = false, float radiu = 2f)
	{
		float num = PEUtil.Magnitude(pos, targetPos, Is3D);
		return num < radiu;
	}

	private void RPC_S2C_NpcMisstion(uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
		byte[] buffer = stream.Read<byte[]>(new object[0]);
		AdNpcData adNpcData = NpcMissionDataRepository.m_AdRandMisNpcData[base.ExternId];
		if (adNpcData != null)
		{
			useData.Deserialize(buffer);
		}
	}

	private void RPC_S2C_MissionState(uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
		PeEntity peEntity = PeSingleton<EntityMgr>.Instance.Get(base.Id);
		if (!(null == peEntity))
		{
			peEntity.SetState((NpcMissionState)stream.Read<int>(new object[0]));
		}
	}

	private void RPC_S2C_RequestAiOp(uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
		int num = stream.Read<int>(new object[0]);
		if (npcCmpt == null)
		{
			return;
		}
		if (base.hasOwnerAuth)
		{
			switch ((EReqType)num)
			{
			case EReqType.Animation:
				npcCmpt.Req_PlayAnimation(stream.Read<string>(new object[0]), stream.Read<float>(new object[0]), stream.Read<bool>(new object[0]));
				break;
			case EReqType.FollowPath:
				npcCmpt.Req_FollowPath(stream.Read<Vector3[]>(new object[0]), stream.Read<bool>(new object[0]), SpeedState.Run, fromnet: true);
				break;
			case EReqType.FollowTarget:
				npcCmpt.Req_FollowTarget(stream.Read<int>(new object[0]), stream.Read<Vector3>(new object[0]), stream.Read<int>(new object[0]), stream.Read<float>(new object[0]), bNet: true);
				break;
			case EReqType.MoveToPoint:
				npcCmpt.Req_MoveToPosition(stream.Read<Vector3>(new object[0]), stream.Read<float>(new object[0]), stream.Read<bool>(new object[0]), (SpeedState)stream.Read<int>(new object[0]));
				if (PeGameMgr.IsMultiStory)
				{
					npcCmpt.FixedPointPos = base._pos;
				}
				break;
			case EReqType.TalkMove:
				npcCmpt.Req_TalkMoveToPosition(stream.Read<Vector3>(new object[0]), stream.Read<float>(new object[0]), stream.Read<bool>(new object[0]), (SpeedState)stream.Read<int>(new object[0]));
				break;
			case EReqType.Rotate:
				npcCmpt.Req_Rotation(stream.Read<Quaternion>(new object[0]));
				break;
			case EReqType.Salvation:
				npcCmpt.Req_Salvation(stream.Read<int>(new object[0]), stream.Read<bool>(new object[0]));
				break;
			case EReqType.Translate:
			{
				Vector3 vector = stream.Read<Vector3>(new object[0]);
				base.transform.position = vector;
				base._pos = vector;
				stream.Read<bool>(new object[0]);
				if (PeGameMgr.IsMultiStory)
				{
					npcCmpt.FixedPointPos = base._pos;
				}
				base.transform.position = base._pos;
				if (_move != null)
				{
					_move.NetMoveTo(base._pos, Vector3.zero, immediately: true);
				}
				break;
			}
			case EReqType.UseSkill:
				npcCmpt.Req_UseSkill();
				break;
			case EReqType.Remove:
				break;
			case EReqType.Dialogue:
			case EReqType.Attack:
			case EReqType.PauseAll:
			case EReqType.Hand:
				break;
			}
			return;
		}
		switch ((EReqType)num)
		{
		case EReqType.Animation:
			break;
		case EReqType.FollowPath:
			npcCmpt.Req_FollowPath(stream.Read<Vector3[]>(new object[0]), stream.Read<bool>(new object[0]), SpeedState.Run, fromnet: true);
			break;
		case EReqType.FollowTarget:
			break;
		case EReqType.MoveToPoint:
			if (PeGameMgr.IsMultiStory)
			{
				npcCmpt.FixedPointPos = base._pos;
			}
			break;
		case EReqType.TalkMove:
			break;
		case EReqType.Rotate:
			break;
		case EReqType.Salvation:
			break;
		case EReqType.Translate:
		{
			Vector3 vector = stream.Read<Vector3>(new object[0]);
			base.transform.position = vector;
			base._pos = vector;
			stream.Read<bool>(new object[0]);
			if (PeGameMgr.IsMultiStory)
			{
				npcCmpt.FixedPointPos = base._pos;
			}
			if (_move != null)
			{
				_move.NetMoveTo(base._pos, Vector3.zero, immediately: true);
			}
			StroyManager.Instance.EntityReach(_entity, trigger: true, fromNet: true);
			break;
		}
		case EReqType.UseSkill:
			break;
		case EReqType.Remove:
		{
			EReqType eReqType = (EReqType)stream.Read<int>(new object[0]);
			switch (eReqType)
			{
			case EReqType.FollowPath:
			{
				Vector3[] pos = stream.Read<Vector3[]>(new object[0]);
				if (npcCmpt.Req_GetRequest(eReqType) is RQFollowPath rQFollowPath && rQFollowPath.Equal(pos))
				{
					npcCmpt.Req_Remove(eReqType);
				}
				break;
			}
			case EReqType.FollowTarget:
				if (npcCmpt.Req_GetRequest(eReqType) is RQFollowTarget)
				{
					npcCmpt.Req_Remove(eReqType);
				}
				break;
			}
			break;
		}
		case EReqType.Dialogue:
		case EReqType.Attack:
		case EReqType.PauseAll:
		case EReqType.Hand:
			break;
		}
	}

	private void RPC_S2C_Mount(uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
		if (base.hasOwnerAuth)
		{
			return;
		}
		_mountId = stream.Read<int>(new object[0]);
		if (npcCmpt != null)
		{
			if (_mountId == 0)
			{
				npcCmpt.MountID = _mountId;
			}
			else if (PeSingleton<EntityMgr>.Instance.Get(_mountId) != null)
			{
				npcCmpt.MountID = _mountId;
			}
			else
			{
				GlobalBehaviour.RegisterEvent(WaitForMountNpc);
			}
		}
		else
		{
			Debug.LogError("npccmpt is null,mount failed");
		}
	}

	private void RPC_S2C_UpdateCampsite(uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
		if (!base.hasOwnerAuth)
		{
			bool updateCampsite = stream.Read<bool>(new object[0]);
			if (npcCmpt != null)
			{
				npcCmpt.UpdateCampsite = updateCampsite;
			}
		}
	}

	private void RPC_S2C_State(uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
		if (!base.hasOwnerAuth)
		{
			int state = stream.Read<int>(new object[0]);
			if (npcCmpt != null)
			{
				npcCmpt.State = (ENpcState)state;
			}
		}
	}

	private void RPC_S2C_GetOn(uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
		int id = stream.Read<int>(new object[0]);
		SeatIndex = stream.Read<int>(new object[0]);
		StartCoroutine(GetOnVehicle(id));
	}

	private void RPC_S2C_GetOff(uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
		Vector3 vector = stream.Read<Vector3>(new object[0]);
		base.transform.position = vector;
		base._pos = vector;
		GetOffVehicle(base._pos);
		if (null != base.MtCmpt)
		{
			base.MtCmpt.EndAction(PEActionType.Drive);
		}
		if (null != base.Trans)
		{
			base.Trans.position = base._pos;
		}
	}

	private void RPC_MissionFlag(uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
		if (!(null == base.Runner))
		{
			stream.TryRead<string>(out var _);
			stream.TryRead<int>(out var _);
			stream.TryRead<int>(out var _);
		}
	}

	private void RPC_PackageIndex(uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
		int num = stream.Read<int>(new object[0]);
		int num2 = stream.Read<int>(new object[0]);
		PeEntity peEntity = PeSingleton<EntityMgr>.Instance.Get(base.Id);
		if (null == peEntity)
		{
			return;
		}
		NpcPackageCmpt cmpt = peEntity.GetCmpt<NpcPackageCmpt>();
		if (null == cmpt)
		{
			return;
		}
		if (num == 0)
		{
			cmpt.Clear();
			if (num2 == 0)
			{
				return;
			}
			int[] array = stream.Read<int[]>(new object[0]);
			int[] array2 = array;
			foreach (int id in array2)
			{
				ItemObject itemObject = PeSingleton<ItemMgr>.Instance.Get(id);
				if (itemObject != null)
				{
					cmpt.AddToNet(itemObject);
				}
			}
			return;
		}
		cmpt.ClearHandin();
		if (num2 == 0)
		{
			return;
		}
		int[] array3 = stream.Read<int[]>(new object[0]);
		int[] array4 = array3;
		foreach (int id2 in array4)
		{
			ItemObject itemObject2 = PeSingleton<ItemMgr>.Instance.Get(id2);
			if (itemObject2 != null)
			{
				cmpt.AddToNetHandin(itemObject2);
			}
		}
	}

	private void RPC_S2C_SyncMoney(uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
		int current = stream.Read<int>(new object[0]);
		PeEntity peEntity = PeSingleton<EntityMgr>.Instance.Get(base.Id);
		if (null == peEntity)
		{
			return;
		}
		PackageCmpt cmpt = peEntity.GetCmpt<PackageCmpt>();
		if (!(null == cmpt))
		{
			cmpt.money.current = current;
			if (null != GameUI.Instance.mShopWnd)
			{
				GameUI.Instance.mShopWnd.ResetItem();
			}
		}
	}

	private void RPC_S2C_SyncTeamID(uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
		_teamId = stream.Read<int>(new object[0]);
		ResetTarget();
	}

	private void RPC_S2C_PutOnEquipment(uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
		ItemObject itemObj = stream.Read<ItemObject>(new object[0]);
		PeEntity peEntity = PeSingleton<EntityMgr>.Instance.Get(base.Id);
		if (!(null == peEntity))
		{
			EquipmentCmpt cmpt = peEntity.GetCmpt<EquipmentCmpt>();
			if (null != cmpt)
			{
				cmpt.PutOnEquipment(itemObj, addToReceiver: false, null, netRequest: true);
			}
		}
	}

	private void RPC_S2C_TakeOffEquipment(uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
		int id = stream.Read<int>(new object[0]);
		ItemObject itemObject = PeSingleton<ItemMgr>.Instance.Get(id);
		if (itemObject == null)
		{
			return;
		}
		PeEntity peEntity = PeSingleton<EntityMgr>.Instance.Get(base.Id);
		if (!(null == peEntity))
		{
			EquipmentCmpt cmpt = peEntity.GetCmpt<EquipmentCmpt>();
			if (null != cmpt)
			{
				cmpt.TakeOffEquipment(itemObject, addToReceiver: false, null, netRequest: true);
			}
		}
	}

	private void RPC_S2C_ExternData(uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
		byte[] externData = stream.Read<byte[]>(new object[0]);
		InitExternData(externData);
		RequestNpcSkill();
	}

	private void RPC_S2C_NpcEquips(uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
		if (stream.Read<int>(new object[0]) != 0)
		{
			ItemObject[] array = stream.Read<ItemObject[]>(new object[0]);
			PeEntity peEntity = PeSingleton<EntityMgr>.Instance.Get(base.Id);
			if (null == peEntity)
			{
				return;
			}
			EquipmentCmpt cmpt = peEntity.GetCmpt<EquipmentCmpt>();
			if (null != cmpt && array != null)
			{
				cmpt.ApplyEquipment(array);
			}
		}
		RequestNpcItems();
	}

	private void RPC_S2C_NpcItems(uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
		int num = stream.Read<int>(new object[0]);
		if (stream.Read<int>(new object[0]) != 0)
		{
			ItemObject[] array = stream.Read<ItemObject[]>(new object[0]);
			PeEntity peEntity = PeSingleton<EntityMgr>.Instance.Get(base.Id);
			if (null != peEntity)
			{
				NpcPackageCmpt cmpt = peEntity.GetCmpt<NpcPackageCmpt>();
				if (null != cmpt)
				{
					if (num == 0)
					{
						cmpt.Clear();
					}
					else
					{
						cmpt.ClearHandin();
					}
					ItemObject[] array2 = array;
					foreach (ItemObject item in array2)
					{
						if (num == 0)
						{
							cmpt.AddToNet(item);
						}
						else
						{
							cmpt.AddToNetHandin(item);
						}
					}
				}
			}
		}
		RequestExternData();
	}

	private void RPC_S2C_NpcSkill(uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
		if (stream.Read<int>(new object[0]) != 0)
		{
			int[] array = stream.Read<int[]>(new object[0]);
			PeEntity peEntity = PeSingleton<EntityMgr>.Instance.Get(base.Id);
			if (null == peEntity)
			{
				return;
			}
			NpcCmpt cmpt = peEntity.GetCmpt<NpcCmpt>();
			for (int i = 0; i < array.Length; i++)
			{
				cmpt.AddAbility(array[i]);
			}
		}
		RequestAbnormalCondition();
	}

	private void RPC_S2C_ForcedServant(uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
		_op = stream.Read<bool>(new object[0]);
		int num = stream.Read<int>(new object[0]);
		bForcedServant = _op;
		if (num == PlayerNetwork.mainPlayerId)
		{
			StartCoroutine(WaitForMainPlayer());
		}
	}

	protected void RPC_S2C_SelfUseItem(uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
		int id = stream.Read<int>(new object[0]);
		ItemObject itemObject = PeSingleton<ItemMgr>.Instance.Get(id);
		if (itemObject != null)
		{
			UseItemCmpt cmpt = _entity.GetCmpt<UseItemCmpt>();
			if (cmpt != null)
			{
				cmpt.UseFromNet(itemObject);
			}
		}
	}

	protected override void RPC_S2C_LostController(uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
		base.RPC_S2C_LostController(stream, info);
		if (lastAuthId == PlayerNetwork.mainPlayerId && _entity != null)
		{
			_entity.requestCmpt.RequsetProtect();
			PeSingleton<PeCreature>.Instance.mainPlayer.GetCmpt<ServantLeaderCmpt>().SevantLostController(PeSingleton<PeCreature>.Instance.mainPlayer.peTrans);
			NpcMgr.ColonyNpcLostController(_entity);
			lastAuthId = base.authId;
		}
	}

	private void RPC_S2C_AddEnemyLock(uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
		int entityId = stream.Read<int>(new object[0]);
		PeEntity peEntity = PeSingleton<EntityMgr>.Instance.Get(entityId);
		if (!(null == peEntity) && !base.hasOwnerAuth && null != _entity)
		{
			_entity.NpcCmpt.AddEnemyLocked(peEntity);
		}
	}

	private void RPC_S2C_RemoveEnemyLock(uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
		int entityId = stream.Read<int>(new object[0]);
		PeEntity peEntity = PeSingleton<EntityMgr>.Instance.Get(entityId);
		if (!(null == peEntity) && !base.hasOwnerAuth && null != _entity)
		{
			_entity.NpcCmpt.RemoveEnemyLocked(peEntity);
		}
	}

	private void RPC_S2C_ClearEnemyLocked(uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
		if (!base.hasOwnerAuth && null != _entity)
		{
			_entity.NpcCmpt.ClearLockedEnemies();
		}
	}

	private IEnumerator WaitForMainPlayer()
	{
		while (null == PeSingleton<PeCreature>.Instance.mainPlayer)
		{
			yield return null;
		}
		ServantLeaderCmpt leader = PeSingleton<PeCreature>.Instance.mainPlayer.GetCmpt<ServantLeaderCmpt>();
		if (_op)
		{
			leader.AddForcedServant(npcCmpt, isMove: true);
		}
		else
		{
			leader.RemoveForcedServant(npcCmpt);
		}
	}
}

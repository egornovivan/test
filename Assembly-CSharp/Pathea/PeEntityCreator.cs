using System;
using System.Collections.Generic;
using CustomCharactor;
using ItemAsset;
using Pathea.PeEntityExt;
using Pathea.PeEntityExtTrans;
using PETools;
using SkillSystem;
using UnityEngine;

namespace Pathea;

public class PeEntityCreator : PeSingleton<PeEntityCreator>
{
	public const string PlayerPrefabPath = "EntityPlayer";

	public const string NpcPrefabPath = "EntityNpc";

	public const string NpcPrefabNativePath = "EntityNpcNative";

	public const string TowerPrefabPath = "EntityTower";

	public const string DoodadPrefabPath = "EntityDoodad";

	public const string MonsterPrefabPath = "EntityMonster";

	public const string MonsterNpcPrefabPath = "EntityMonster_Npc";

	public const string GroupPrefabPath = "EntityGroup";

	private const int EasyBuffID = 30200168;

	public const int HumanMonsterMask = 10000;

	public PeEntity CreateDoodad(int id, int protoId, Vector3 pos, Quaternion rot, Vector3 scl)
	{
		DoodadProtoDb.Item item = DoodadProtoDb.Get(protoId);
		if (item == null)
		{
			Debug.LogError("cant find doodad proto by id:" + protoId);
			return null;
		}
		PeEntity peEntity = PeSingleton<EntityMgr>.Instance.Create(id, "EntityDoodad", pos, rot, scl);
		if (peEntity == null)
		{
			return null;
		}
		peEntity.ExtSetName(new CharacterName(item.name));
		peEntity.SetViewModelPath(item.modelPath);
		InitAttrs(peEntity, item.dbAttr, null);
		InitProto(peEntity, EEntityProto.Doodad, protoId);
		CommonCmpt cmpt = peEntity.GetCmpt<CommonCmpt>();
		if (cmpt != null)
		{
			cmpt.ItemDropId = item.dropItemId;
		}
		return peEntity;
	}

	public PeEntity CreateTower(int id, int protoId, Vector3 pos, Quaternion rot, Vector3 scl)
	{
		TowerProtoDb.Item item = TowerProtoDb.Get(protoId);
		if (item == null)
		{
			return null;
		}
		PeEntity peEntity = PeSingleton<EntityMgr>.Instance.Create(id, "EntityTower", pos, rot, scl);
		if (peEntity == null)
		{
			return null;
		}
		peEntity.ExtSetName(new CharacterName(item.name));
		peEntity.SetViewModelPath(item.modelPath);
		InitBehaveData(peEntity, item.behaveDataPath);
		InitAttrs(peEntity, item.dbAttr, null);
		InitProto(peEntity, EEntityProto.Tower, protoId);
		InitIdentity(peEntity, item.eId, item.eRace, bBoss: false);
		InitTowerBulletData(peEntity, item.bulletData);
		InitBattle(peEntity);
		return peEntity;
	}

	public PeEntity CreateMonster(int id, int protoId, Vector3 pos, Quaternion rot, Vector3 scl, float exScale = -1f, int colorType = -1, int buffId = 0)
	{
		MonsterProtoDb.Item item = MonsterProtoDb.Get(protoId);
		if (item == null)
		{
			return null;
		}
		float num = ((!(exScale > 0f)) ? UnityEngine.Random.Range(item.fScaleMinMax[0], item.fScaleMinMax[1]) : exScale);
		PeEntity peEntity = null;
		peEntity = ((protoId >= 10000) ? PeSingleton<EntityMgr>.Instance.Create(id, "EntityMonster_Npc", pos, rot, scl * num) : PeSingleton<EntityMgr>.Instance.Create(id, "EntityMonster", pos, rot, scl * num));
		if (peEntity == null)
		{
			return null;
		}
		peEntity.ExtSetName(new CharacterName(item.name));
		peEntity.ExtSetFaceIcon(item.icon);
		peEntity.SetViewModelPath(item.modelPath);
		InitBehaveData(peEntity, item.behaveDataPath);
		initMonsterEscape(peEntity, item.injuredState, item.escapeProb);
		InitProto(peEntity, EEntityProto.Monster, protoId);
		InitIdentity(peEntity, item.eId, item.eRace, item.isBoss);
		if (protoId < 10000)
		{
			InitAttrsWithScale(peEntity, item.dbAttr, item.initBuff, num);
			InitEquipment(peEntity, item.initEquip);
			if ((peEntity.Race == ERace.Puja || peEntity.Race == ERace.Paja) && colorType >= 0 && colorType <= 7)
			{
				peEntity.biologyViewCmpt.SetColorID(colorType);
			}
		}
		else
		{
			ApplyCustomCharactorData(peEntity, CreateCustomData());
			InitMonsterNpc(peEntity, item.npcProtoID);
			InitAttrsWithScale(peEntity, item.dbAttr, item.initBuff, num, item.npcProtoID);
			if (peEntity.Race == ERace.Mankind)
			{
				if (colorType >= 0 && colorType <= 7)
				{
					InitEquipment(peEntity, MonsterRandomDb.GetEquipments(colorType));
					InitWeapon(peEntity, MonsterRandomDb.GetWeapon(colorType));
				}
				else
				{
					RandomNpcDb.Item item2 = RandomNpcDb.Get(item.npcProtoID);
					if (item2 != null)
					{
						InitEquipment(peEntity, item2.initEquipment);
					}
				}
			}
		}
		Motion_Move_Motor cmpt = peEntity.GetCmpt<Motion_Move_Motor>();
		if (cmpt != null)
		{
			cmpt.Field = item.movementField;
		}
		CommonCmpt cmpt2 = peEntity.GetCmpt<CommonCmpt>();
		if (cmpt2 != null)
		{
			cmpt2.ItemDropId = item.dropItemId;
		}
		MonsterCmpt cmpt3 = peEntity.GetCmpt<MonsterCmpt>();
		if (cmpt3 != null)
		{
			cmpt3.InjuredLevel = item.injuredLevel;
		}
		if (PeGameMgr.gameLevel == PeGameMgr.EGameLevel.Easy && peEntity.skEntity != null)
		{
			SkEntity.MountBuff(peEntity.skEntity, 30200168, new List<int>(), new List<float>());
		}
		if (buffId > 0 && peEntity.skEntity != null)
		{
			SkEntity.MountBuff(peEntity.skEntity, buffId, new List<int>(), new List<float>());
		}
		MonsterEntityCreator.AttachMonsterDeathEvent(peEntity);
		return peEntity;
	}

	public PeEntity CreateMonsterNet(int id, int protoId, Vector3 pos, Quaternion rot, Vector3 scl, float exScale = -1f, int buffId = 0)
	{
		MonsterProtoDb.Item item = MonsterProtoDb.Get(protoId);
		if (item == null)
		{
			return null;
		}
		float num = ((!(exScale > 0f)) ? UnityEngine.Random.Range(item.fScaleMinMax[0], item.fScaleMinMax[1]) : exScale);
		PeEntity peEntity = null;
		peEntity = ((protoId >= 10000) ? PeSingleton<EntityMgr>.Instance.Create(id, "EntityMonster_Npc", pos, rot, scl * num) : PeSingleton<EntityMgr>.Instance.Create(id, "EntityMonster", pos, rot, scl * num));
		if (peEntity == null)
		{
			return null;
		}
		peEntity.ExtSetName(new CharacterName(item.name));
		peEntity.ExtSetFaceIcon(item.icon);
		peEntity.SetViewModelPath(item.modelPath);
		InitBehaveData(peEntity, item.behaveDataPath);
		InitAttrsWithScale(peEntity, item.dbAttr, item.initBuff, num);
		InitProto(peEntity, EEntityProto.Monster, protoId);
		InitIdentity(peEntity, item.eId, item.eRace, item.isBoss);
		InitEquipment(peEntity, item.initEquip);
		initMonsterEscape(peEntity, item.injuredState, item.escapeProb);
		Motion_Move_Motor cmpt = peEntity.GetCmpt<Motion_Move_Motor>();
		if (cmpt != null)
		{
			cmpt.Field = item.movementField;
		}
		CommonCmpt cmpt2 = peEntity.GetCmpt<CommonCmpt>();
		if (cmpt2 != null)
		{
			cmpt2.ItemDropId = item.dropItemId;
		}
		MonsterCmpt cmpt3 = peEntity.GetCmpt<MonsterCmpt>();
		if (cmpt3 != null)
		{
			cmpt3.InjuredLevel = item.injuredLevel;
		}
		if (PeGameMgr.gameLevel == PeGameMgr.EGameLevel.Easy && peEntity.skEntity != null)
		{
			SkEntity.MountBuff(peEntity.skEntity, 30200168, new List<int>(), new List<float>());
		}
		if (buffId > 0 && peEntity.skEntity != null)
		{
			SkEntity.MountBuff(peEntity.skEntity, buffId, new List<int>(), new List<float>());
		}
		MonsterEntityCreator.AttachMonsterDeathEvent(peEntity);
		return peEntity;
	}

	public PeEntity CreateNpcRobot(int id, int protoId, Vector3 pos, Quaternion rot, Vector3 scl, float exScale = -1f)
	{
		if (id != NpcRobotDb.Instance.mFollowID)
		{
			return null;
		}
		PeEntity peEntity = PeSingleton<EntityMgr>.Instance.Get(id);
		if (peEntity == null || peEntity.transform == null)
		{
			return null;
		}
		NpcProtoDb.Item item = NpcProtoDb.Get(protoId);
		if (item == null)
		{
			Debug.LogError("cant find doodad proto by id:" + protoId);
			return null;
		}
		PeEntity peEntity2 = PeSingleton<EntityMgr>.Instance.Create(NpcRobotDb.Instance.mID, "EntityMonster", pos, rot, scl);
		if (peEntity2 == null)
		{
			return null;
		}
		peEntity2.ExtSetName(new CharacterName(NpcRobotDb.Instance.mName));
		peEntity2.SetViewModelPath(NpcRobotDb.Instance.robotModelPath);
		InitBehaveData(peEntity2, NpcRobotDb.Instance.behaveDataPath);
		InitProto(peEntity2, EEntityProto.NpcRobot, protoId);
		InitAttrs(peEntity2, item.dbAttr, item.InFeildBuff);
		InitRobotInfo(peEntity2, peEntity);
		peEntity2.peTrans.position = NpcRobotDb.Instance.startPos;
		return peEntity2;
	}

	public PeEntity CreatePlayer(int id, Vector3 pos, Quaternion rot, Vector3 scl, CustomCharactor.CustomData data = null)
	{
		PeEntity peEntity = PeSingleton<EntityMgr>.Instance.Create(id, "EntityPlayer", pos, rot, scl);
		ApplyCustomCharactorData(peEntity, data);
		if (SingleGameStory.curType == SingleGameStory.StoryScene.TrainingShip && PeGameMgr.IsTutorial)
		{
			peEntity.enityInfoCmpt.characterName = new CharacterName("Tutorial");
		}
		PlayerProtoDb.Item item = PlayerProtoDb.Get();
		InitAttrs(peEntity, item.dbAttr, item.initBuff);
		InitProto(peEntity, EEntityProto.Player, -1);
		InitIdentity(peEntity, EIdentity.Player, ERace.Mankind, bBoss: false);
		CommonCmpt cmpt = peEntity.GetCmpt<CommonCmpt>();
		if (null != cmpt)
		{
			cmpt.OwnerID = 1;
		}
		return peEntity;
	}

	public PeEntity CreateNpc(int id, int protoId, Vector3 pos, Quaternion rot, Vector3 scl)
	{
		NpcProtoDb.Item item = NpcProtoDb.Get(protoId);
		if (item == null)
		{
			Debug.LogError("cant find npc proto by id:" + protoId);
			return null;
		}
		string path = "EntityNpc";
		if (item.race == "Puja" || item.race == "Paja")
		{
			path = "EntityNpcNative";
		}
		PeEntity peEntity = PeSingleton<EntityMgr>.Instance.Create(id, path, pos, rot, scl);
		if (peEntity == null)
		{
			return null;
		}
		peEntity.ExtSetName(new CharacterName().InitStoryNpcName(item.name, item.showName));
		peEntity.ExtSetFaceIcon(item.icon);
		peEntity.ExtSetFaceIconBig(item.iconBig);
		peEntity.ExtSetVoiceType(item.voiceType);
		if (item.modelObj != null)
		{
			peEntity.SetViewModelPath(item.modelPrefabPath);
		}
		else if (!string.IsNullOrEmpty(item.modelAssetPath))
		{
			peEntity.SetViewModelPath(item.modelAssetPath);
		}
		else
		{
			peEntity.SetAvatarNpcModelPath(item.avatarModelPath);
		}
		peEntity.ExtSetSex(item.sex);
		peEntity.ExtSetVoiceType(item.voiceType);
		InitBehaveData(peEntity, item.behaveDataPath);
		InitAttrs(peEntity, item.dbAttr, item.InFeildBuff);
		InitProto(peEntity, EEntityProto.Npc, protoId);
		InitIdentity(peEntity, EIdentity.Npc, ERace.Mankind, bBoss: false);
		InitBattle(peEntity);
		CommonCmpt cmpt = peEntity.GetCmpt<CommonCmpt>();
		if (null != cmpt)
		{
			cmpt.OwnerID = 1;
		}
		return peEntity;
	}

	public PeEntity CreateNpcForNet(int id, int protoId, Vector3 pos, Quaternion rot, Vector3 scl)
	{
		NpcProtoDb.Item item = NpcProtoDb.Get(protoId);
		if (item == null)
		{
			Debug.LogError("cant find npc proto by id:" + protoId);
			return null;
		}
		if (PeSingleton<EntityMgr>.Instance == null)
		{
			return null;
		}
		string path = "EntityNpc";
		if (item.race == "Puja" || item.race == "Paja")
		{
			path = "EntityNpcNative";
		}
		PeEntity peEntity = PeSingleton<EntityMgr>.Instance.Create(id, path, pos, rot, scl);
		if (peEntity == null)
		{
			return null;
		}
		peEntity.ExtSetName(new CharacterName().InitStoryNpcName(item.name, item.showName));
		peEntity.ExtSetFaceIcon(item.icon);
		peEntity.ExtSetFaceIconBig(item.iconBig);
		if (item.modelObj != null)
		{
			peEntity.SetViewModelPath(item.modelPrefabPath);
		}
		else if (!string.IsNullOrEmpty(item.modelAssetPath))
		{
			peEntity.SetViewModelPath(item.modelAssetPath);
		}
		else
		{
			peEntity.SetAvatarNpcModelPath(item.avatarModelPath);
		}
		peEntity.ExtSetSex(item.sex);
		peEntity.ExtSetVoiceType(item.voiceType);
		InitBehaveData(peEntity, item.behaveDataPath);
		InitAttrs(peEntity, item.dbAttr, item.InFeildBuff);
		InitProto(peEntity, EEntityProto.Npc, protoId);
		InitIdentity(peEntity, EIdentity.Npc, ERace.Mankind, bBoss: false);
		InitBattle(peEntity);
		CommonCmpt cmpt = peEntity.GetCmpt<CommonCmpt>();
		if (null != cmpt)
		{
			cmpt.OwnerID = 1;
		}
		return peEntity;
	}

	public PeEntity CreateRandomNpcForNet(int templateId, int id, Vector3 pos, Quaternion rot, Vector3 scl)
	{
		PeEntity peEntity = PeSingleton<EntityMgr>.Instance.Create(id, "EntityNpc", pos, rot, scl);
		if (null == peEntity)
		{
			return null;
		}
		RandomNpcDb.Item item = RandomNpcDb.Get(templateId);
		if (item == null)
		{
			Debug.LogError("no npc random data found with templateId:" + templateId);
			return peEntity;
		}
		InitRandomNpcAttrForNet(peEntity, item);
		InitBehaveData(peEntity, item.behaveDataPath);
		InitProto(peEntity, EEntityProto.RandomNpc, templateId);
		InitIdentity(peEntity, EIdentity.Npc, ERace.Mankind, bBoss: false);
		CommonCmpt cmpt = peEntity.GetCmpt<CommonCmpt>();
		if (null != cmpt)
		{
			cmpt.OwnerID = 1;
		}
		return peEntity;
	}

	public PeEntity CreateRandomNpc(int templateId, int id, Vector3 pos, Quaternion rot, Vector3 scl)
	{
		PeEntity peEntity = PeSingleton<EntityMgr>.Instance.Create(id, "EntityNpc", pos, rot, scl);
		if (null == peEntity)
		{
			return null;
		}
		ApplyCustomCharactorData(peEntity, CreateCustomData());
		InitRandomNpc(peEntity, templateId);
		InitProto(peEntity, EEntityProto.RandomNpc, templateId);
		InitIdentity(peEntity, EIdentity.Npc, ERace.Mankind, bBoss: false);
		InitBattle(peEntity);
		CommonCmpt cmpt = peEntity.GetCmpt<CommonCmpt>();
		if (null != cmpt)
		{
			cmpt.OwnerID = 1;
		}
		return peEntity;
	}

	public PeEntity CreateMountsMonster(MountMonsterData data)
	{
		int id = PeSingleton<WorldInfoMgr>.Instance.FetchNonRecordAutoId();
		PeEntity peEntity = CreateMonster(id, data._protoId, data._curPostion, data._rotation, data._scale, 1f);
		if (peEntity == null)
		{
			return null;
		}
		peEntity.SetAttribute(AttribType.CampID, data._mountsForce._campID);
		peEntity.SetAttribute(AttribType.DamageID, data._mountsForce._damageID);
		peEntity.SetAttribute(AttribType.DefaultPlayerID, data._mountsForce._defaultPlyerID);
		peEntity.monstermountCtrl.m_MountsForceDb = data._mountsForce.Copy();
		peEntity.monstermountCtrl.ResetMountsSkill(data._mountsSkill);
		peEntity.monstermountCtrl.SetctrlType(data._eCtrltype);
		float attribute = peEntity.GetAttribute(AttribType.HpMax);
		peEntity.SetAttribute(AttribType.Hp, data._hp * attribute);
		return peEntity;
	}

	public static void initMonsterEscape(PeEntity entity, float escapeBase, float escapeProb)
	{
		TargetCmpt cmpt = entity.GetCmpt<TargetCmpt>();
		if (!(cmpt == null))
		{
			cmpt.EscapeBase = escapeBase;
			cmpt.EscapeProp = escapeProb;
		}
	}

	public static void InitTowerBulletData(PeEntity entity, TowerProtoDb.BulletData bulletData)
	{
		TowerCmpt cmpt = entity.GetCmpt<TowerCmpt>();
		if (!(cmpt == null))
		{
			cmpt.NeedVoxel = bulletData.needBlock;
			cmpt.CostType = (ECostType)bulletData.bulletType;
			cmpt.ConsumeItem = bulletData.bulletId;
			cmpt.ConsumeCost = bulletData.bulletCost;
			cmpt.ConsumeCountMax = bulletData.bulletMax;
			cmpt.ConsumeEnergyCost = bulletData.energyCost;
			cmpt.ConsumeEnergyMax = bulletData.energyMax;
			cmpt.SkillID = bulletData.skillId;
		}
	}

	public static void InitBehaveData(PeEntity entity, string behaveDataPath)
	{
		BehaveCmpt cmpt = entity.GetCmpt<BehaveCmpt>();
		if (cmpt != null)
		{
			cmpt.SetAssetPath(behaveDataPath);
		}
	}

	public static void InitProto(PeEntity entity, EEntityProto prototype, int prototypeId)
	{
		entity.entityProto = new EntityProto
		{
			proto = prototype,
			protoId = prototypeId
		};
	}

	public static void InitIdentity(PeEntity entity, EIdentity eId, ERace eRace, bool bBoss)
	{
		CommonCmpt cmpt = entity.GetCmpt<CommonCmpt>();
		if (null == cmpt)
		{
			Debug.LogError("cant find common cmpt");
			return;
		}
		cmpt.Identity = eId;
		cmpt.Race = eRace;
		cmpt.IsBoss = bBoss;
	}

	public static void InitAttrs(PeEntity entity, DbAttr dbAttr, int[] initBuff)
	{
		SkAliveEntity cmpt = entity.GetCmpt<SkAliveEntity>();
		if (null != cmpt)
		{
			cmpt.m_InitBuffList = initBuff;
			cmpt.m_Attrs = dbAttr.ToAliveAttr();
			cmpt.InitSkEntity();
		}
	}

	public static void InitAttrsWithScale(PeEntity entity, DbAttr dbAttr, int[] initBuff, float fScale)
	{
		SkAliveEntity cmpt = entity.GetCmpt<SkAliveEntity>();
		if (null != cmpt)
		{
			cmpt.m_InitBuffList = initBuff;
			cmpt.m_Attrs = dbAttr.ToAliveAttrWithScale(fScale);
			cmpt.InitSkEntity();
		}
	}

	public static void InitAttrsWithScale(PeEntity entity, DbAttr dbAttr, int[] initBuff, float fScale, int npcRandID)
	{
		SkAliveEntity cmpt = entity.GetCmpt<SkAliveEntity>();
		if (null != cmpt)
		{
			cmpt.m_InitBuffList = initBuff;
			cmpt.m_Attrs = dbAttr.ToAliveAttrWithScale(fScale);
			RandomNpcDb.Item item = RandomNpcDb.Get(npcRandID);
			if (item != null)
			{
				int num = cmpt.m_Attrs.Length;
				Array.Resize(ref cmpt.m_Attrs, num + 4);
				cmpt.m_Attrs[num] = new PESkEntity.Attr
				{
					m_Type = AttribType.HpMax,
					m_Value = item.hpMax.Random()
				};
				cmpt.m_Attrs[num + 1] = new PESkEntity.Attr
				{
					m_Type = AttribType.Atk,
					m_Value = item.atk.Random()
				};
				cmpt.m_Attrs[num + 2] = new PESkEntity.Attr
				{
					m_Type = AttribType.Def,
					m_Value = item.def.Random()
				};
				cmpt.m_Attrs[num + 3] = new PESkEntity.Attr
				{
					m_Type = AttribType.Hp,
					m_Value = cmpt.m_Attrs[num].m_Value
				};
			}
			cmpt.InitSkEntity();
		}
	}

	public static void InitAttrs(PeEntity entity, PESkEntity.Attr[] dbAttr, int[] initBuff)
	{
		SkAliveEntity cmpt = entity.GetCmpt<SkAliveEntity>();
		if (null != cmpt)
		{
			cmpt.m_InitBuffList = initBuff;
			cmpt.m_Attrs = dbAttr;
			cmpt.InitSkEntity();
		}
	}

	public static void InitBattle(PeEntity entity)
	{
		if (!(entity == null))
		{
			NpcCmpt cmpt = entity.GetCmpt<NpcCmpt>();
			if (!(cmpt == null))
			{
				cmpt.Battle = ENpcBattle.Defence;
			}
		}
	}

	public static void ApplyCustomCharactorData(PeEntity entity, CustomCharactor.CustomData data)
	{
		if (data == null)
		{
			data = CustomCharactor.CustomData.DefaultFemale();
		}
		if (!string.IsNullOrEmpty(data.charactorName))
		{
			entity.ExtSetName(new CharacterName(data.charactorName));
		}
		entity.ExtSetSex(PeGender.Convert(data.sex));
		entity.SetAvatarData(data.appearData, data.nudeAvatarData);
	}

	public static CustomCharactor.CustomData CreateCustomData()
	{
		CustomCharactor.CustomData customData = null;
		string excludeHead = null;
		if (CustomDataMgr.Instance.Current != null)
		{
			excludeHead = CustomDataMgr.Instance.Current.nudeAvatarData[AvatarData.ESlot.Head];
		}
		PeSex peSex = PeGender.Random();
		customData = ((peSex != PeSex.Female) ? CustomCharactor.CustomData.RandomMale(excludeHead) : CustomCharactor.CustomData.RandomFemale(excludeHead));
		customData.appearData.Random();
		customData.charactorName = null;
		return customData;
	}

	public static void InitRandomNpcAttr(PeEntity entity, RandomNpcDb.Item item)
	{
		PlayerProtoDb.Item randomNpc = PlayerProtoDb.GetRandomNpc();
		DbAttr dbAttr = randomNpc.dbAttr.Clone();
		dbAttr.attributeArray[0] = item.hpMax.Random();
		dbAttr.attributeArray[25] = item.atk.Random();
		dbAttr.attributeArray[21] = item.resDamage;
		dbAttr.attributeArray[24] = item.atkRange;
		dbAttr.attributeArray[27] = item.def.Random();
		dbAttr.attributeArray[1] = dbAttr.attributeArray[0];
		dbAttr.attributeArray[16] = dbAttr.attributeArray[15];
		InitAttrs(entity, dbAttr, randomNpc.InFeildBuff);
	}

	public static void InitRandomNpcAttrForNet(PeEntity entity, RandomNpcDb.Item item)
	{
		PlayerProtoDb.Item randomNpc = PlayerProtoDb.GetRandomNpc();
		DbAttr dbAttr = randomNpc.dbAttr.Clone();
		InitAttrs(entity, dbAttr, randomNpc.InFeildBuff);
	}

	public static void InitMonsterSkinRandom(PeEntity entity, int playerId)
	{
		switch (entity.Race)
		{
		case ERace.Mankind:
			InitEquipment(entity, MonsterRandomDb.GetEquipments(playerId));
			InitWeapon(entity, MonsterRandomDb.GetWeapon(playerId));
			break;
		case ERace.Puja:
			entity.biologyViewCmpt.SetColorID(playerId);
			break;
		case ERace.Paja:
			entity.biologyViewCmpt.SetColorID(playerId);
			break;
		case ERace.Monster:
			break;
		}
	}

	public static void InitMonsterSkinRandomNet(PeEntity entity, int playerId)
	{
		switch (entity.Race)
		{
		case ERace.Puja:
			entity.biologyViewCmpt.SetColorID(playerId);
			break;
		case ERace.Paja:
			entity.biologyViewCmpt.SetColorID(playerId);
			break;
		}
	}

	public static void InitMonsterNpc(PeEntity entity, int npcProtoID)
	{
		if (!(null == entity))
		{
			PeSex sex = entity.ExtGetSex();
			int race = UnityEngine.Random.Range(1, 5);
			entity.ExtSetName(PeSingleton<WorldInfoMgr>.Instance.FetchName(sex, race));
		}
	}

	public static void InitRandomNpc(PeEntity entity, int templateId)
	{
		if (null == entity)
		{
			return;
		}
		PeSex sex = entity.ExtGetSex();
		int race = UnityEngine.Random.Range(1, 5);
		entity.ExtSetName(PeSingleton<WorldInfoMgr>.Instance.FetchName(sex, race));
		RandomNpcDb.Item item = RandomNpcDb.Get(templateId);
		if (item == null)
		{
			Debug.LogError("no npc random data found");
			return;
		}
		NpcCmpt cmpt = entity.GetCmpt<NpcCmpt>();
		if (cmpt != null)
		{
			cmpt.ReviveTime = item.reviveTime;
		}
		InitRandomNpcAttr(entity, item);
		InitEquipment(entity, item.initEquipment);
		InitPackage(entity, item.initItems);
		InitNpcMoney(entity, item.npcMoney);
		InitNpcAbility(entity, item.randomAbility);
		InitBehaveData(entity, item.behaveDataPath);
		entity.ExtSetVoiceType(item.voiveMatch.GetRandomVoice(sex));
	}

	public static void InitNpcAbility(PeEntity entity, RandomNpcDb.RandomAbility ability)
	{
		if (ability != null)
		{
			NpcCmpt cmpt = entity.GetCmpt<NpcCmpt>();
			if (!(cmpt == null))
			{
				cmpt.SetAbilityIDs(ability.Get());
			}
		}
	}

	public static void InitPackage(PeEntity entity, IEnumerable<int> itemProtoIds)
	{
		PackageCmpt packageCmpt = entity.packageCmpt;
		if (null == packageCmpt || itemProtoIds == null)
		{
			return;
		}
		foreach (int itemProtoId in itemProtoIds)
		{
			ItemObject itemObject = PeSingleton<ItemMgr>.Instance.CreateItem(itemProtoId);
			if (itemObject != null)
			{
				packageCmpt.Add(itemObject);
			}
		}
	}

	public static void InitPackage(PeEntity entity, List<RandomNpcDb.ItemcoutDb> items)
	{
		if (!(entity.packageCmpt == null) && items != null)
		{
			for (int i = 0; i < items.Count; i++)
			{
				entity.packageCmpt.Add(items[i].protoId, items[i].count);
			}
		}
	}

	public static void InitNpcMoney(PeEntity entity, RandomNpcDb.NpcMoney npcMoney)
	{
		NpcPackageCmpt cmpt = entity.GetCmpt<NpcPackageCmpt>();
		if (cmpt != null)
		{
			cmpt.money.current = npcMoney.initValue.Random();
			cmpt.InitAutoIncreaseMoney(npcMoney.max, npcMoney.incValue.Random());
		}
	}

	public static void InitWeapon(PeEntity entity, int weaponId)
	{
		if (null == entity.equipmentCmpt)
		{
			Debug.LogError("no equipment cmpt");
			return;
		}
		ItemObject itemObject = PeSingleton<ItemMgr>.Instance.CreateItem(weaponId);
		if (itemObject != null)
		{
			entity.equipmentCmpt.AddInitEquipment(itemObject);
		}
	}

	public static void InitEquipment(PeEntity entity, IEnumerable<int> equipmentItemProtoIds)
	{
		if (equipmentItemProtoIds == null)
		{
			return;
		}
		EquipmentCmpt cmpt = entity.GetCmpt<EquipmentCmpt>();
		if (null == cmpt)
		{
			Debug.LogError("no equipment cmpt");
			return;
		}
		PeSex require = entity.ExtGetSex();
		foreach (int equipmentItemProtoId in equipmentItemProtoIds)
		{
			ItemProto itemProto = PeSingleton<ItemProto.Mgr>.Instance.Get(equipmentItemProtoId);
			if (itemProto != null && PeGender.IsMatch(itemProto.equipSex, require))
			{
				ItemObject itemObject = PeSingleton<ItemMgr>.Instance.CreateItem(equipmentItemProtoId);
				if (itemObject != null)
				{
					cmpt.AddInitEquipment(itemObject);
				}
			}
		}
	}

	public static void InitRobot()
	{
		PeEntity peEntity = PeSingleton<EntityMgr>.Instance.Get(NpcRobotDb.Instance.mID);
		if (!(peEntity == null))
		{
			PeEntity peEntity2 = PeSingleton<EntityMgr>.Instance.Get(NpcRobotDb.Instance.mFollowID);
			if (!(peEntity2 == null) && !(peEntity2.transform == null))
			{
				InitRobotInfo(peEntity, peEntity2);
			}
		}
	}

	public static void InitRobotInfo(PeEntity robot, PeEntity ownerEntity)
	{
		if (!(robot == null) && !(ownerEntity == null))
		{
			if (robot.robotCmpt == null)
			{
				robot.gameObject.AddComponent<RobotCmpt>();
			}
			Motion_Move_Motor motion_Move_Motor = robot.motionMove as Motion_Move_Motor;
			if (motion_Move_Motor != null)
			{
				motion_Move_Motor.Field = MovementField.Sky;
			}
			if (ownerEntity.NpcCmpt != null)
			{
				ownerEntity.NpcCmpt.AddFollowRobot(robot);
			}
			robot.SetAttribute(AttribType.DamageID, 27f);
			robot.SetAttribute(AttribType.CampID, 0f);
		}
	}

	public static void RecruitMainNpc(PeEntity entity)
	{
		NpcProtoDb.Item item = NpcProtoDb.Get(entity.entityProto.protoId);
		if (item == null)
		{
			Debug.LogError("cant find npc proto by id:" + entity.entityProto.protoId);
			return;
		}
		SkAliveEntity cmpt = entity.GetCmpt<SkAliveEntity>();
		if (!(null == cmpt))
		{
			for (int i = 0; i < item.RecruitBuff.Length; i++)
			{
				SkEntity.MountBuff(cmpt, item.RecruitBuff[i], new List<int>(), new List<float>());
			}
			NpcCmpt npcCmpt = entity.NpcCmpt;
			if (npcCmpt != null)
			{
				npcCmpt.UpdateCampsite = false;
			}
		}
	}

	public static void ExileMainNpc(PeEntity entity)
	{
		if (entity.IsDeath())
		{
			if (entity.NpcCmpt != null && entity.NpcCmpt.FixedPointPos != Vector3.zero)
			{
				float f = PEUtil.Magnitude(entity.position, entity.NpcCmpt.FixedPointPos);
				if (Mathf.Abs(f) >= 256f)
				{
					PEUtil.RagdollTranlate(entity, entity.NpcCmpt.FixedPointPos);
				}
			}
			return;
		}
		NpcProtoDb.Item item = NpcProtoDb.Get(entity.entityProto.protoId);
		if (item == null)
		{
			Debug.LogError("cant find npc proto by id:" + entity.entityProto.protoId);
			return;
		}
		SkAliveEntity cmpt = entity.GetCmpt<SkAliveEntity>();
		if (!(null == cmpt))
		{
			for (int i = 0; i < item.RecruitBuff.Length; i++)
			{
				cmpt.CancelBuffById(item.RecruitBuff[i]);
			}
			NpcCmpt npcCmpt = entity.NpcCmpt;
			if (npcCmpt != null)
			{
				npcCmpt.UpdateCampsite = true;
			}
		}
	}

	public static void RecruitRandomNpc(PeEntity entity)
	{
		SkAliveEntity cmpt = entity.GetCmpt<SkAliveEntity>();
		if (!(null == cmpt))
		{
			PlayerProtoDb.Item randomNpc = PlayerProtoDb.GetRandomNpc();
			randomNpc.dbAttr.Clone();
			for (int i = 0; i < randomNpc.RecruitBuff.Length; i++)
			{
				SkEntity.MountBuff(cmpt, randomNpc.RecruitBuff[i], new List<int>(), new List<float>());
			}
			NpcCmpt npcCmpt = entity.NpcCmpt;
			if (npcCmpt != null)
			{
				npcCmpt.UpdateCampsite = false;
			}
		}
	}

	public static void ExileRandomNpc(PeEntity entity)
	{
		if (entity.IsDeath())
		{
			if (entity.NpcCmpt != null && entity.NpcCmpt.FixedPointPos != Vector3.zero)
			{
				float f = PEUtil.Magnitude(entity.position, entity.NpcCmpt.FixedPointPos);
				if (Mathf.Abs(f) >= 256f)
				{
					entity.lodCmpt.DestroyView();
					entity.ExtSetPos(entity.NpcCmpt.FixedPointPos);
					SceneMan.SetDirty(entity.lodCmpt);
				}
			}
			return;
		}
		SkAliveEntity aliveEntity = entity.aliveEntity;
		if (!(null == aliveEntity))
		{
			PlayerProtoDb.Item randomNpc = PlayerProtoDb.GetRandomNpc();
			randomNpc.dbAttr.Clone();
			for (int i = 0; i < randomNpc.RecruitBuff.Length; i++)
			{
				aliveEntity.CancelBuffById(randomNpc.RecruitBuff[i]);
			}
			NpcCmpt npcCmpt = entity.NpcCmpt;
			if (npcCmpt != null && npcCmpt.FixedPointPos != Vector3.zero && !(npcCmpt.Creater != null) && !npcCmpt.IsServant)
			{
				npcCmpt.Req_MoveToPosition(npcCmpt.FixedPointPos, 1f, isForce: true, SpeedState.Run);
			}
			if (npcCmpt != null)
			{
				npcCmpt.UpdateCampsite = true;
			}
		}
	}
}

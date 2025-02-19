using System.Collections;
using System.Collections.Generic;
using System.IO;
using CustomCharactor;
using ItemAsset;
using Pathea;
using PETools;
using uLink;
using UnityEngine;

public class AiObject : SkNetworkInterface
{
	private const int c_cntAttribType = 97;

	public float[] attributeArray = new float[97];

	private static readonly int[] c_scalableAttrIndices = new int[6] { 37, 36, 0, 1, 25, 27 };

	protected int _externId;

	protected AiSynAttribute mAiSynAttribute = new AiSynAttribute();

	protected Dictionary<string, bool> animatorState = new Dictionary<string, bool>();

	protected int _boolName;

	protected bool rifleState;

	protected bool _boolValue;

	protected EquipmentCmpt m_EquipModule;

	protected AiObjectType _objType;

	protected float m_radius;

	protected float m_scale = 1f;

	protected int m_fixId = -1;

	protected Vector3 spawnPos;

	protected Dictionary<GameObject, int> m_hatredlist = new Dictionary<GameObject, int>();

	protected List<ItemSample> m_DropItem = new List<ItemSample>();

	public EquipmentCmpt EquipModule => m_EquipModule;

	public int ExternId => _externId;

	public int colorType { get; protected set; }

	public CustomCharactor.CustomData customData { get; protected set; }

	public PeSex sex { get; protected set; }

	public float radius => m_radius;

	public float Scale => m_scale;

	public int FixId => m_fixId;

	public bool IsDead => _bDeath;

	public Vector3 SpawnPos
	{
		get
		{
			return spawnPos;
		}
		set
		{
			spawnPos = value;
		}
	}

	public List<ItemSample> DropItemID => m_DropItem;

	protected override void OnPEInstantiate(uLink.NetworkMessageInfo info)
	{
		_objType = AiObjectType.AiObjectType_Monster;
		_id = info.networkView.initialData.Read<int>(new object[0]);
		_externId = info.networkView.initialData.Read<int>(new object[0]);
		m_scale = info.networkView.initialData.Read<float>(new object[0]);
		base.authId = info.networkView.initialData.Read<int>(new object[0]);
		_worldId = info.networkView.group;
		Add(this);
		spawnPos = base.transform.position;
		Player.PlayerDisconnected += OnPlayerDisconnect;
		Player.OnHeartBeatTimeoutEvent += OnPlayerDisconnect;
	}

	public override void SkCreater()
	{
		if (_skEntity == null || _skEntity._attribs == null)
		{
			return;
		}
		if (ExternId < 10000)
		{
			SKAttribute.InitMonsterBaseAttrs(_skEntity._attribs, ExternId, out _skEntity._baseAttribs);
			ScaleAttrs();
			return;
		}
		MonsterProtoDb.Item item = MonsterProtoDb.Get(ExternId);
		if (item != null)
		{
			RandomNpcDb.Item item2 = RandomNpcDb.Get(item.npcProtoID);
			if (item2 != null)
			{
				SKAttribute.InitPlayerBaseAttrs(_skEntity._attribs, out _skEntity._baseAttribs);
				SetAllAttribute(AttribType.HpMax, item2.hpMax.Random());
				SetAllAttribute(AttribType.Atk, item2.atk.Random());
				SetAllAttribute(AttribType.ResDamage, item2.resDamage);
				SetAllAttribute(AttribType.AtkRange, item2.atkRange);
				SetAllAttribute(AttribType.Def, item2.def.Random());
				SetAllAttribute(AttribType.Hp, _skEntity.GetAttribute(AttribType.HpMax));
				SetAllAttribute(AttribType.Hunger, _skEntity.GetAttribute(AttribType.HungerMax));
			}
		}
	}

	private void ScaleAttrs()
	{
		for (int i = 0; i < c_scalableAttrIndices.Length; i++)
		{
			int type = c_scalableAttrIndices[i];
			_skEntity._attribs.NumAttribs.SetAllAttribute((AttribType)type, _skEntity._attribs.NumAttribs.GetAttribute((AttribType)type) * Scale);
		}
	}

	protected override void OnPEStart()
	{
		base.OnPEStart();
		BindAction(EPacketType.PT_AI_InitData, RPC_C2S_InitData);
		BindAction(EPacketType.PT_AI_AnimatorState, RPC_C2S_RequestAnimatorState);
		BindAction(EPacketType.PT_InGame_SetController, RPC_C2S_SetController);
		BindAction(EPacketType.PT_InGame_LostController, RPC_C2S_LostController);
		BindAction(EPacketType.PT_AI_SetRadius, RPC_C2S_InitRadius);
		BindAction(EPacketType.PT_AI_Move, RPC_C2S_AiNetworkMovePostion);
		BindAction(EPacketType.PT_AI_RotY, RPC_C2S_AiNetworkMoveRotationY);
		BindAction(EPacketType.PT_AI_IKTarget, RPC_C2S_AiNetworkIKTarget);
		BindAction(EPacketType.PT_AI_Animation, RPC_C2S_PlayAnimation);
		BindAction(EPacketType.PT_AI_RifleAim, RPC_C2S_RifleAim);
		BindAction(EPacketType.PT_AI_IKPosWeight, RPC_C2S_SetIKPositionWeight);
		BindAction(EPacketType.PT_AI_IKPosition, RPC_C2S_SetIKPosition);
		BindAction(EPacketType.PT_AI_IKRotWeight, RPC_C2S_SetIKRotationWeight);
		BindAction(EPacketType.PT_AI_IKRotation, RPC_C2S_SetIKRotation);
		BindAction(EPacketType.PT_AI_BoolString, RPC_C2S_SetBool_String);
		BindAction(EPacketType.PT_AI_BoolInt, RPC_C2S_SetBool_Int);
		BindAction(EPacketType.PT_AI_VectorString, RPC_C2S_SetVector_String);
		BindAction(EPacketType.PT_AI_VectorInt, RPC_C2S_SetVector_Int);
		BindAction(EPacketType.PT_AI_IntString, RPC_C2S_SetInteger_String);
		BindAction(EPacketType.PT_AI_IntInt, RPC_C2S_SetInteger_Int);
		BindAction(EPacketType.PT_AI_LayerWeight, RPC_C2S_SetLayerWeight);
		BindAction(EPacketType.PT_AI_LookAtWeight, RPC_C2S_SetLookAtWeight);
		BindAction(EPacketType.PT_AI_LookAtPos, RPC_C2S_SetLookAtPosition);
		BindAction(EPacketType.PT_AI_SetBool, RPC_C2S_SetBool);
		BindAction(EPacketType.PT_AI_SetTrigger, RPC_C2S_SetTrigger);
		BindAction(EPacketType.PT_AI_SetMoveMode, RPC_C2S_SetMoveMode);
		BindAction(EPacketType.PT_AI_HoldWeapon, RPC_C2S_HoldWeapon);
		BindAction(EPacketType.PT_AI_SwitchHoldWeapon, RPC_C2S_SwitchHoldWeapon);
		BindAction(EPacketType.PT_AI_SwordAttack, RPC_C2S_SwordAttack);
		BindAction(EPacketType.PT_AI_TwoHandWeaponAttack, RPC_C2S_TwoHandWeaponAttack);
		BindAction(EPacketType.PT_AI_SetIKAim, RPC_C2S_SetIKAim);
		BindAction(EPacketType.PT_AI_Fadein, RPC_C2S_Fadein);
		BindAction(EPacketType.PT_AI_Fadeout, RPC_C2S_Fadeout);
		BindAction(EPacketType.PT_AI_ExternData, RPC_C2S_ExternData);
		SPTerrainEvent.InitCustomData(base.Id, _skEntity);
		CheckAuth();
	}

	protected override void OnPEDestroy()
	{
		base.OnPEDestroy();
		Player.OnHeartBeatTimeoutEvent -= OnPlayerDisconnect;
		Player.PlayerDisconnected -= OnPlayerDisconnect;
	}

	public void SyncDeadItems(uLink.NetworkPlayer peer)
	{
		if (DropItemID.Count == 0)
		{
			RPCPeer(peer, EPacketType.PT_InGame_DeadObjItem, DropItemID.Count);
		}
		else
		{
			RPCPeer(peer, EPacketType.PT_InGame_DeadObjItem, DropItemID.Count, DropItemID.ToArray(), false);
		}
	}

	public void SyncDeadItems()
	{
		if (DropItemID.Count == 0)
		{
			RPCOthers(EPacketType.PT_InGame_DeadObjItem, DropItemID.Count);
		}
		else
		{
			RPCOthers(EPacketType.PT_InGame_DeadObjItem, DropItemID.Count, DropItemID.ToArray(), false);
		}
	}

	public void SyncEquip(uLink.NetworkPlayer peer)
	{
		RPCPeer(peer, EPacketType.PT_AI_Equipment, EquipModule.EquipIds);
	}

	private void SyncInitData(uLink.NetworkPlayer peer)
	{
		RPCPeer(peer, EPacketType.PT_AI_InitData, base.transform.position, base.transform.rotation, base.authId);
	}

	private void SyncAnimatorState(uLink.NetworkPlayer peer)
	{
		byte[] array = Serialize.Export(delegate(BinaryWriter w)
		{
			BufferHelper.Serialize(w, animatorState.Count);
			foreach (KeyValuePair<string, bool> item in animatorState)
			{
				BufferHelper.Serialize(w, item.Key);
				BufferHelper.Serialize(w, item.Value);
			}
		});
		RPCPeer(peer, EPacketType.PT_AI_AnimatorState, rifleState, array);
	}

	protected void SyncScenarioId(uLink.NetworkPlayer peer)
	{
		int customId = SPTerrainEvent.GetCustomId(base.Id);
		if (customId != -1)
		{
			RPCPeer(peer, EPacketType.PT_Common_ScenarioId, customId);
		}
	}

	private void RPC_C2S_InitData(uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
		SyncInitData(info.sender);
		SyncScenarioId(info.sender);
		if (ExternId >= 10000)
		{
			byte[] array = customData.Serialize();
			RPCPeer(info.sender, EPacketType.PT_AI_AvatarData, array);
		}
		else
		{
			RPCPeer(info.sender, EPacketType.PT_AI_AvatarData);
		}
	}

	protected virtual void RPC_C2S_ExternData(uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
		if (EquipModule != null && EquipModule.EquipCount != 0)
		{
			ChannelNetwork.SyncItemList(info.sender, EquipModule.EquipItems);
			SyncEquip(info.sender);
		}
		if (IsDead)
		{
			SyncDeadItems(info.sender);
			RPCPeer(info.sender, EPacketType.PT_AI_Death, -1);
		}
	}

	private void RPC_C2S_RequestAnimatorState(uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
		SyncAnimatorState(info.sender);
	}

	protected override void RPC_C2S_SetController(uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
		if (base.authId <= 0 || !Player.ValidPlayer(base.authId))
		{
			base.authId = Player.GetPlayerId(info.sender);
			RPCProxy(EPacketType.PT_InGame_SetController, base.authId);
		}
		else
		{
			RPCPeer(info.sender, EPacketType.PT_InGame_SetController, base.authId);
		}
	}

	protected void RPC_C2S_InitRadius(uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
		m_radius = stream.Read<float>(new object[0]);
	}

	protected virtual void RPC_C2S_AiNetworkMovePostion(uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
		Vector3 vector = stream.Read<Vector3>(new object[0]);
		AiSynAttribute aiSynAttribute = mAiSynAttribute;
		Vector3 vector2 = vector;
		base.transform.position = vector2;
		aiSynAttribute.mv3Postion = vector2;
		URPCOthers(EPacketType.PT_AI_Move, vector);
	}

	private void RPC_C2S_AiNetworkMoveRotationY(uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
		int num = stream.Read<int>(new object[0]);
		mAiSynAttribute.rotEuler = num;
		Vector3 euler = PEUtil.UncompressEulerAngle(num);
		base.transform.rotation = Quaternion.Euler(euler);
		URPCOthers(EPacketType.PT_AI_RotY, num);
	}

	private void RPC_C2S_AiNetworkIKTarget(uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
		if (!IsDead && stream.TryRead<Vector3>(out var value))
		{
			URPCOthers(EPacketType.PT_AI_IKTarget, value);
		}
	}

	protected override void RPC_C2S_LostController(uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
		int playerId = Player.GetPlayerId(info.sender);
		if (playerId != -1 && base.authId == playerId)
		{
			base.authId = -1;
			RPCProxy(EPacketType.PT_InGame_LostController);
		}
	}

	private void RPC_C2S_PlayAnimation(uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
		string text = stream.Read<string>(new object[0]);
		RPCOthers(EPacketType.PT_AI_Animation, text);
	}

	private void RPC_C2S_RifleAim(uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
		rifleState = stream.Read<bool>(new object[0]);
		RPCProxy(EPacketType.PT_AI_RifleAim, rifleState);
	}

	protected virtual void RPC_C2S_SetIKPositionWeight(uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
		AvatarIKGoal avatarIKGoal = stream.Read<AvatarIKGoal>(new object[0]);
		float num = stream.Read<float>(new object[0]);
		RPCOthers(EPacketType.PT_AI_IKPosWeight, avatarIKGoal, num);
	}

	protected virtual void RPC_C2S_SetIKPosition(uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
		AvatarIKGoal avatarIKGoal = stream.Read<AvatarIKGoal>(new object[0]);
		Vector3 vector = stream.Read<Vector3>(new object[0]);
		RPCOthers(EPacketType.PT_AI_IKPosition, avatarIKGoal, vector);
	}

	protected virtual void RPC_C2S_SetIKRotation(uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
		AvatarIKGoal avatarIKGoal = stream.Read<AvatarIKGoal>(new object[0]);
		Quaternion quaternion = stream.Read<Quaternion>(new object[0]);
		RPCOthers(EPacketType.PT_AI_IKRotation, avatarIKGoal, quaternion);
	}

	protected virtual void RPC_C2S_SetIKRotationWeight(uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
		AvatarIKGoal avatarIKGoal = stream.Read<AvatarIKGoal>(new object[0]);
		float num = stream.Read<float>(new object[0]);
		RPCOthers(EPacketType.PT_AI_IKRotWeight, avatarIKGoal, num);
	}

	protected virtual void RPC_C2S_SetBool_String(uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
		string text = stream.Read<string>(new object[0]);
		bool flag = stream.Read<bool>(new object[0]);
		animatorState[text] = flag;
		RPCOthers(EPacketType.PT_AI_BoolString, text, flag);
	}

	protected virtual void RPC_C2S_SetBool_Int(uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
		int num = stream.Read<int>(new object[0]);
		bool flag = stream.Read<bool>(new object[0]);
		RPCOthers(EPacketType.PT_AI_BoolInt, num, flag);
	}

	protected virtual void RPC_C2S_SetVector_String(uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
		string text = stream.Read<string>(new object[0]);
		Vector3 vector = stream.Read<Vector3>(new object[0]);
		RPCOthers(EPacketType.PT_AI_VectorString, text, vector);
	}

	protected virtual void RPC_C2S_SetVector_Int(uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
		int num = stream.Read<int>(new object[0]);
		Vector3 vector = stream.Read<Vector3>(new object[0]);
		RPCOthers(EPacketType.PT_AI_VectorInt, num, vector);
	}

	protected virtual void RPC_C2S_SetInteger_String(uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
		string text = stream.Read<string>(new object[0]);
		int num = stream.Read<int>(new object[0]);
		RPCOthers(EPacketType.PT_AI_IntString, text, num);
	}

	protected virtual void RPC_C2S_SetInteger_Int(uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
		int num = stream.Read<int>(new object[0]);
		int num2 = stream.Read<int>(new object[0]);
		RPCOthers(EPacketType.PT_AI_IntInt, num, num2);
	}

	protected virtual void RPC_C2S_SetLayerWeight(uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
		int num = stream.Read<int>(new object[0]);
		float num2 = stream.Read<float>(new object[0]);
		RPCOthers(EPacketType.PT_AI_LayerWeight, num, num2);
	}

	protected virtual void RPC_C2S_SetLookAtWeight(uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
		float num = stream.Read<float>(new object[0]);
		RPCOthers(EPacketType.PT_AI_LookAtWeight, num);
	}

	protected virtual void RPC_C2S_SetLookAtPosition(uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
		Vector3 vector = stream.Read<Vector3>(new object[0]);
		RPCOthers(EPacketType.PT_AI_LookAtPos, vector);
	}

	private void RPC_C2S_SetBool(uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
		int num = stream.Read<int>(new object[0]);
		bool flag = stream.Read<bool>(new object[0]);
		_boolName = num;
		_boolValue = flag;
		RPCOthers(EPacketType.PT_AI_SetBool, num, flag);
	}

	private void RPC_C2S_SetTrigger(uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
		string text = stream.Read<string>(new object[0]);
		RPCOthers(EPacketType.PT_AI_SetTrigger, text);
	}

	private void RPC_C2S_SetMoveMode(uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
		int num = stream.Read<int>(new object[0]);
		RPCOthers(EPacketType.PT_AI_SetMoveMode, num);
	}

	private void RPC_C2S_HoldWeapon(uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
	}

	private void RPC_C2S_SwitchHoldWeapon(uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
	}

	private void RPC_C2S_SwordAttack(uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
		Vector3 vector = stream.Read<Vector3>(new object[0]);
		RPCOthers(EPacketType.PT_AI_SwordAttack, vector);
	}

	private void RPC_C2S_TwoHandWeaponAttack(uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
		Vector3 vector = stream.Read<Vector3>(new object[0]);
		int num = stream.Read<int>(new object[0]);
		int num2 = stream.Read<int>(new object[0]);
		RPCOthers(EPacketType.PT_AI_TwoHandWeaponAttack, vector, num, num2);
	}

	private void RPC_C2S_SetIKAim(uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
	}

	private void RPC_C2S_Fadein(uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
		float num = stream.Read<float>(new object[0]);
		RPCOthers(EPacketType.PT_AI_Fadein, num);
	}

	private void RPC_C2S_Fadeout(uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
		float num = stream.Read<float>(new object[0]);
		RPCOthers(EPacketType.PT_AI_Fadeout, num);
	}

	protected void RPC_C2S_ResetPosition(uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
		Vector3 vector = stream.Read<Vector3>(new object[0]);
		base.transform.position = vector;
		RPCOthers(EPacketType.PT_NPC_ResetPosition, vector);
	}

	protected virtual void InitializeData()
	{
		InitCmpt();
		sex = PeSex.Max;
		if (ExternId < 10000)
		{
			if (_objType == AiObjectType.AiObjectType_Monster)
			{
				MonsterProtoDb.Item item = MonsterProtoDb.Get(ExternId);
				if (item != null)
				{
					InitEquipments(item.initEquip);
				}
			}
			return;
		}
		InitCustomData();
		bool flag = false;
		if (colorType != -1)
		{
			int[] equipments = MonsterRandomDb.GetEquipments(colorType);
			int weapon = MonsterRandomDb.GetWeapon(colorType);
			List<int> list = new List<int>();
			if (weapon != -1)
			{
				list.Add(weapon);
			}
			if (equipments.Length != 0)
			{
				list.AddRange(equipments);
			}
			if (list.Count != 0)
			{
				InitEquipments(list.ToArray());
				flag = true;
			}
		}
		if (flag)
		{
			return;
		}
		MonsterProtoDb.Item item2 = MonsterProtoDb.Get(ExternId);
		if (item2 != null)
		{
			RandomNpcDb.Item item3 = RandomNpcDb.Get(item2.npcProtoID);
			if (item3 != null && item3.initEquipment != null && item3.initEquipment.Length != 0)
			{
				InitEquipments(item3.initEquipment);
			}
		}
	}

	protected override void InitCmpt()
	{
		base.InitCmpt();
		m_EquipModule = (EquipmentCmpt)AddCmpt(ECmptType.Equipment);
	}

	protected virtual void InitCustomData()
	{
		sex = (PeSex)Random.Range(1, 3);
		customData = CustomCharactor.CustomData.CreateCustomData(sex);
		int race = Random.Range(1, 5);
		customData.charactorName = NameGenerater.Fetch(sex, race).FullName;
	}

	protected virtual void InitEquipments(int[] initEquips)
	{
		EquipModule.Clear();
		if (initEquips == null || initEquips.Length == 0)
		{
			return;
		}
		for (int i = 0; i < initEquips.Length; i++)
		{
			ItemProto itemData = ItemProto.GetItemData(initEquips[i]);
			if (itemData != null && (itemData.equipSex == PeSex.Undefined || itemData.equipSex == sex))
			{
				ItemObject itemObject = ItemManager.CreateItem(initEquips[i], 1);
				if (itemObject != null)
				{
					EquipModule.PutOnEquip(itemObject);
				}
			}
		}
	}

	internal override void OnDamage(int casterId, float damage)
	{
		base.OnDamage(casterId, damage);
		ObjNetInterface objNetInterface = ObjNetInterface.Get(casterId);
		if (!(objNetInterface == null) && objNetInterface is SkNetworkInterface)
		{
			IncreaseHatred(objNetInterface.gameObject, (int)damage);
		}
	}

	public virtual void IncreaseHatred(GameObject obj, int hatredValue)
	{
		if (!(obj == null) && hatredValue > 0)
		{
			if (!m_hatredlist.ContainsKey(obj))
			{
				m_hatredlist.Add(obj, hatredValue);
				return;
			}
			Dictionary<GameObject, int> hatredlist;
			Dictionary<GameObject, int> dictionary = (hatredlist = m_hatredlist);
			GameObject key;
			GameObject key2 = (key = obj);
			int num = hatredlist[key];
			dictionary[key2] = num + hatredValue;
		}
	}

	internal override void OnDeath(int casterId = 0)
	{
		base.OnDeath(casterId);
		ObjNetInterface objNetInterface = ObjNetInterface.Get(casterId);
		CheckMonsterMission((SkNetworkInterface)objNetInterface);
		if (ServerConfig.IsStory && this is AiMonsterNetwork && objNetInterface is Player)
		{
			MonsterHandbookData.AddMhByKilledMonsterID(ExternId);
			(objNetInterface as Player).SyncMonsterBook(objNetInterface as Player, ower: false, ExternId);
		}
		_bDeath = true;
		m_hatredlist.Clear();
		DropItem(objNetInterface);
		RPCOthers(EPacketType.PT_AI_Death, casterId);
		StartCoroutine(DestroyAiObjectCoroutine());
		AISpawnPoint.DeleteSpawnPoint(m_fixId);
	}

	protected virtual IEnumerator DestroyAiObjectCoroutine()
	{
		yield return new WaitForSeconds(30f);
		NetInterface.NetDestroy(this);
	}

	public virtual void DropItem(NetInterface caster)
	{
	}

	public void CreateDropItems(List<ItemSample> items, ref List<ItemObject> effItems)
	{
		ItemManager.CreateItems(items, ref effItems);
		if (effItems.Count > 0)
		{
			ChannelNetwork.SyncItemList(base.WorldId, effItems);
		}
	}

	public void CreateDropScenes(List<ItemSample> items)
	{
		List<ItemObject> effItems = new List<ItemObject>();
		CreateDropItems(items, ref effItems);
		Player randomPlayer = Player.GetRandomPlayer();
		if (randomPlayer != null)
		{
			SceneDropItem.CreateDropItems(randomPlayer.WorldId, effItems, base.transform.position, Vector3.zero, base.transform.rotation);
		}
	}

	public virtual bool GetDeadObjItem(Player player)
	{
		return false;
	}

	public virtual bool GetDeadObjItem(Player player, int index, int objID)
	{
		return false;
	}

	private void CheckMonsterMission(SkNetworkInterface caster)
	{
		Player player = null;
		if (caster is Player)
		{
			player = caster as Player;
		}
		else if (caster is AiAdNpcNetwork)
		{
			AiAdNpcNetwork aiAdNpcNetwork = caster as AiAdNpcNetwork;
			if (null != aiAdNpcNetwork && aiAdNpcNetwork.Recruited)
			{
				player = aiAdNpcNetwork.lordPlayer;
			}
		}
		else if (caster is AiTowerNetwork)
		{
			AiTowerNetwork aiTowerNetwork = caster as AiTowerNetwork;
			if (null != aiTowerNetwork && aiTowerNetwork.OwnerId != -1 && ObjNetInterface.Get(aiTowerNetwork.OwnerId) != null && ObjNetInterface.Get(aiTowerNetwork.OwnerId) is Player)
			{
				player = ObjNetInterface.Get(aiTowerNetwork.OwnerId) as Player;
			}
		}
		if (player == null)
		{
			if (ServerConfig.IsStory)
			{
				player = Player.GetRandomPlayer();
				if (player == null)
				{
					return;
				}
			}
			else
			{
				player = Player.GetNearestPlayer(Pos);
				if (player == null)
				{
					return;
				}
			}
		}
		player.ProcessMonsterDead(ExternId);
	}
}

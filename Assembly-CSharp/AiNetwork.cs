using System.Collections;
using System.Collections.Generic;
using System.IO;
using CustomCharactor;
using ItemAsset;
using Pathea;
using uLink;
using UnityEngine;

public class AiNetwork : SkNetworkInterface
{
	protected int _externId;

	protected float _scale;

	protected float lastRotY;

	[SerializeField]
	public PeEntity _entity;

	protected PeTrans _viewTrans;

	protected Vector3 lastPosition;

	protected BaseSyncAttr _syncAttr;

	protected List<ItemObject> equipList = new List<ItemObject>();

	protected ItemSample[] dropItems;

	protected int _groupId;

	protected int _tdId;

	protected int _dungeonId;

	protected int _colorType;

	protected int _playerId;

	protected int _fixId = -1;

	protected int _buffId;

	protected bool _canride = true;

	protected bool death;

	private AnimatorCmpt _animatorCmpt;

	private Motion_Equip _equipment;

	private IKAimCtrl _iKAim;

	private BiologyViewCmpt _view;

	public int ExternId => _externId;

	public float Scale => _scale;

	internal AnimatorCmpt animatorCmpt
	{
		get
		{
			if (_animatorCmpt == null)
			{
				if (null == base.Runner || null == base.Runner.SkEntityPE)
				{
					return null;
				}
				_animatorCmpt = base.Runner.SkEntityPE.gameObject.GetComponent<AnimatorCmpt>();
			}
			return _animatorCmpt;
		}
	}

	private Motion_Equip Equipment
	{
		get
		{
			if (_equipment == null)
			{
				if (null == base.Runner || null == base.Runner.SkEntityPE)
				{
					return null;
				}
				_equipment = base.Runner.SkEntityPE.gameObject.GetComponent<Motion_Equip>();
			}
			return _equipment;
		}
	}

	private IKAimCtrl IKAim
	{
		get
		{
			if (_iKAim == null)
			{
				if (null == base.Runner || null == base.Runner.SkEntityPE)
				{
					return null;
				}
				_iKAim = base.Runner.SkEntityPE.gameObject.GetComponent<IKAimCtrl>();
			}
			return _iKAim;
		}
	}

	private BiologyViewCmpt View
	{
		get
		{
			if (_view == null)
			{
				if (null == base.Runner || null == base.Runner.SkEntityPE)
				{
					return null;
				}
				_view = base.Runner.SkEntityPE.gameObject.GetComponent<BiologyViewCmpt>();
			}
			return _view;
		}
	}

	protected override void OnPEInstantiate(uLink.NetworkMessageInfo info)
	{
		_id = info.networkView.initialData.Read<int>(new object[0]);
		_externId = info.networkView.initialData.Read<int>(new object[0]);
		_scale = info.networkView.initialData.Read<float>(new object[0]);
		base.authId = info.networkView.initialData.Read<int>(new object[0]);
		_groupId = info.networkView.initialData.Read<int>(new object[0]);
		_tdId = info.networkView.initialData.Read<int>(new object[0]);
		_dungeonId = info.networkView.initialData.Read<int>(new object[0]);
		_colorType = info.networkView.initialData.Read<int>(new object[0]);
		_playerId = info.networkView.initialData.Read<int>(new object[0]);
		_fixId = info.networkView.initialData.Read<int>(new object[0]);
		_buffId = info.networkView.initialData.Read<int>(new object[0]);
		_canride = info.networkView.initialData.Read<bool>(new object[0]);
		death = false;
		base._pos = base.transform.position;
		base.rot = base.transform.rotation;
		_syncAttr.Pos = base._pos;
		_syncAttr.EulerY = base.rot.eulerAngles.y;
	}

	private void OnAlterAttribs(int type)
	{
	}

	protected override void OnPEStart()
	{
		BindSkAction();
		BindAction(EPacketType.PT_AI_InitData, RPC_S2C_InitData);
		BindAction(EPacketType.PT_AI_AnimatorState, RPC_S2C_ResponseAnimatorState);
		BindAction(EPacketType.PT_InGame_SetController, RPC_S2C_SetController);
		BindAction(EPacketType.PT_AI_Move, RPC_S2C_AiNetworkMovePostion);
		BindAction(EPacketType.PT_AI_RotY, RPC_S2C_NetRotation);
		BindAction(EPacketType.PT_AI_IKTarget, RPC_S2C_AiNetworkIKTarget);
		BindAction(EPacketType.PT_InGame_LostController, RPC_S2C_LostController);
		BindAction(EPacketType.PT_AI_Animation, RPC_S2C_PlayAnimation);
		BindAction(EPacketType.PT_AI_Equipment, RPC_S2C_SyncEquips);
		BindAction(EPacketType.PT_AI_ApplyHpChange, RPC_S2C_ApplyHpChange);
		BindAction(EPacketType.PT_AI_Death, RPC_S2C_Death);
		BindAction(EPacketType.PT_AI_Turn, RPC_S2C_Turn);
		BindAction(EPacketType.PT_AI_RifleAim, RPC_S2C_RifleAim);
		BindAction(EPacketType.PT_AI_IKPosWeight, RPC_S2C_SetIKPositionWeight);
		BindAction(EPacketType.PT_AI_IKPosition, RPC_S2C_SetIKPosition);
		BindAction(EPacketType.PT_AI_IKRotWeight, RPC_S2C_SetIKRotationWeight);
		BindAction(EPacketType.PT_AI_IKRotation, RPC_S2C_SetIKRotation);
		BindAction(EPacketType.PT_AI_BoolString, RPC_S2C_SetBool_String);
		BindAction(EPacketType.PT_AI_BoolInt, RPC_S2C_SetBool_Int);
		BindAction(EPacketType.PT_AI_VectorString, RPC_S2C_SetVector_String);
		BindAction(EPacketType.PT_AI_VectorInt, RPC_S2C_SetVector_Int);
		BindAction(EPacketType.PT_AI_IntString, RPC_S2C_SetInteger_String);
		BindAction(EPacketType.PT_AI_IntInt, RPC_S2C_SetInteger_Int);
		BindAction(EPacketType.PT_AI_LayerWeight, RPC_S2C_SetLayerWeight);
		BindAction(EPacketType.PT_AI_LookAtWeight, RPC_S2C_SetLookAtWeight);
		BindAction(EPacketType.PT_AI_LookAtPos, RPC_S2C_SetLookAtPosition);
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
		BindAction(EPacketType.PT_InGame_DeadObjItem, RPC_C2S_ResponseDeadObjItem);
		BindAction(EPacketType.PT_Common_ScenarioId, RPC_S2C_ScenarioId);
		BindAction(EPacketType.PT_AI_AvatarData, RPC_S2C_AvatarData);
		RPCServer(EPacketType.PT_AI_InitData);
	}

	protected virtual IEnumerator SyncMove()
	{
		base._pos = base.transform.position;
		base.rot = base.transform.rotation;
		while (base.hasOwnerAuth)
		{
			if (null != _viewTrans)
			{
				AiNetwork aiNetwork = this;
				Vector3 position = _viewTrans.position;
				base.transform.position = position;
				aiNetwork._pos = position;
				AiNetwork aiNetwork2 = this;
				Quaternion rotation = _viewTrans.rotation;
				base.transform.rotation = rotation;
				aiNetwork2.rot = rotation;
			}
			if (Mathf.Abs(_syncAttr.Pos.x - base._pos.x) > 0.1f || Mathf.Abs(_syncAttr.Pos.y - base._pos.y) > 0.1f || Mathf.Abs(_syncAttr.Pos.z - base._pos.z) > 0.1f)
			{
				_syncAttr.Pos = base._pos;
				URPCServer(EPacketType.PT_AI_Move, base._pos);
				if (null != _move && null != _entity)
				{
					if (_entity.Race == ERace.Mankind && _entity.proto == EEntityProto.Monster)
					{
						_move.AddNetTransInfo(base._pos, base.rot.eulerAngles, _move.speed, GameTime.Timer.Second);
					}
					else
					{
						_move.NetMoveTo(base._pos, Vector3.zero);
					}
				}
			}
			if (null != _entity && (_entity.proto != EEntityProto.Monster || _entity.Race != ERace.Mankind) && Mathf.Abs(_syncAttr.EulerY - base.rot.eulerAngles.y) > 0.1f)
			{
				_syncAttr.EulerY = base.rot.eulerAngles.y;
				int rotEuler = VCUtils.CompressEulerAngle(base.rot.eulerAngles);
				URPCServer(EPacketType.PT_AI_RotY, rotEuler);
			}
			yield return new WaitForSeconds(1f / uLink.Network.sendRate);
		}
	}

	public override void OnSpawned(GameObject obj)
	{
		base.OnSpawned(obj);
		if (null == _entity.netCmpt)
		{
			_entity.netCmpt = _entity.Add<NetCmpt>();
		}
		_entity.netCmpt.network = this;
		StartCoroutine(AuthorityCheckCoroutine());
	}

	protected override void ResetContorller()
	{
		base.ResetContorller();
		if (base.hasOwnerAuth)
		{
			StartCoroutine(SyncMove());
		}
	}

	internal void CreateAi()
	{
		_entity = PeSingleton<PeEntityCreator>.Instance.CreateMonsterNet(base.Id, ExternId, Vector3.zero, Quaternion.identity, Vector3.one, Scale, _buffId);
		if (null == _entity)
		{
			return;
		}
		if (_fixId != -1)
		{
			PeSingleton<SceneEntityCreatorArchiver>.Instance.SetEntityByFixedSpId(_fixId, _entity);
		}
		_entity.monster.Ride(_canride);
		_viewTrans = _entity.GetCmpt<PeTrans>();
		if (null == _viewTrans)
		{
			Debug.LogError("entity has no ViewCmpt");
			return;
		}
		_viewTrans.position = base.transform.position;
		_move = _entity.GetCmpt<Motion_Move>();
		NetCmpt netCmpt = _entity.GetCmpt<NetCmpt>();
		if (null == netCmpt)
		{
			netCmpt = _entity.Add<NetCmpt>();
		}
		netCmpt.network = this;
		MonsterProtoDb.Item item = MonsterProtoDb.Get(ExternId);
		if (item == null)
		{
			base.gameObject.name = $"TemplateId:{ExternId}, Id:{base.Id}";
		}
		else
		{
			base.gameObject.name = $"{item.name}, TemplateId:{ExternId}, Id:{base.Id}";
		}
		if (_groupId != -1)
		{
			AIGroupNetWork.OnMonsterAdd(_groupId, this, _entity);
		}
		if (_tdId != -1)
		{
			AiTowerDefense.OnMonsterAdd(_tdId, this, _entity);
			if ((bool)_entity.monster)
			{
				_entity.monster.Ride(value: false);
			}
		}
		if (_dungeonId != -1)
		{
			_entity.SetAttribute(AttribType.CampID, 26f);
			_entity.SetAttribute(AttribType.DamageID, 26f);
			if ((bool)_entity.monster)
			{
				_entity.monster.Ride(value: false);
			}
		}
		if (_colorType != -1)
		{
			PeEntityCreator.InitMonsterSkinRandomNet(_entity, _colorType);
		}
		if (_playerId != -1)
		{
			_entity.SetAttribute(AttribType.DefaultPlayerID, _playerId);
		}
		OnSpawned(_entity.GetGameObject());
		if (_entity.Race == ERace.Mankind && _entity.proto == EEntityProto.Monster)
		{
			_move.AddNetTransInfo(base.transform.position, base.rot.eulerAngles, _move.speed, GameTime.Timer.Second);
		}
		else
		{
			_move.NetMoveTo(base.transform.position, Vector3.zero);
		}
	}

	private void InitDeadItems(ItemDropPeEntity dropEntity, ItemSample[] items)
	{
		if (!(null != dropEntity))
		{
			return;
		}
		dropEntity.RemoveDroppableItemAll();
		if (items != null)
		{
			foreach (ItemSample item in items)
			{
				dropEntity.AddDroppableItem(item);
			}
		}
	}

	public void RequestSetBool(int str, bool b)
	{
		RPCServer(EPacketType.PT_AI_SetBool, str, b);
	}

	public void RequestSetBool(string str, bool b)
	{
		RPCServer(EPacketType.PT_AI_SetBool, str, b);
	}

	public void RequestSetTrigger(string str)
	{
		RPCServer(EPacketType.PT_AI_SetTrigger, str);
	}

	public void RequestSetMoveMode(int mode)
	{
		RPCServer(EPacketType.PT_AI_SetMoveMode, mode);
	}

	public void RequestSwordAttack(Vector3 dir)
	{
		RPCServer(EPacketType.PT_AI_SwordAttack, dir);
	}

	public void RequestTwoHandWeaponAttack(Vector3 dir, int handType = 0, int time = 0)
	{
		RPCServer(EPacketType.PT_AI_TwoHandWeaponAttack, dir, handType, time);
	}

	public void RequestFadein(float time)
	{
		RPCServer(EPacketType.PT_AI_Fadein, time);
	}

	public void RequestFadeout(float time)
	{
		RPCServer(EPacketType.PT_AI_Fadeout, time);
	}

	protected virtual void RPC_S2C_InitData(uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
		Vector3 vector = stream.Read<Vector3>(new object[0]);
		base.transform.position = vector;
		base._pos = vector;
		Quaternion rotation = stream.Read<Quaternion>(new object[0]);
		base.transform.rotation = rotation;
		base.rot = rotation;
		base.authId = stream.Read<int>(new object[0]);
	}

	protected virtual void RPC_S2C_ResponseAnimatorState(uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
		if (base.hasOwnerAuth)
		{
			return;
		}
		AiObject aiObject = base.Runner as AiObject;
		if (aiObject == null)
		{
			return;
		}
		stream.Read<bool>(new object[0]);
		byte[] buffer = stream.Read<byte[]>(new object[0]);
		using MemoryStream input = new MemoryStream(buffer);
		using BinaryReader reader = new BinaryReader(input);
		int num = BufferHelper.ReadInt32(reader);
		for (int i = 0; i < num; i++)
		{
			string text = BufferHelper.ReadString(reader);
			bool value = BufferHelper.ReadBoolean(reader);
			aiObject.SetBool(text, value);
		}
	}

	protected override void RPC_S2C_SetController(uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
		base.authId = stream.Read<int>(new object[0]);
		lastAuthId = base.authId;
		ResetContorller();
	}

	protected void RPC_S2C_AiNetworkMovePostion(uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
		Vector3 vector = stream.Read<Vector3>(new object[0]);
		base.transform.position = vector;
		base._pos = vector;
		if (!base.hasOwnerAuth && null != _move)
		{
			if (null != _entity && _entity.Race == ERace.Mankind && _entity.proto == EEntityProto.Monster)
			{
				_move.AddNetTransInfo(base._pos, base.rot.eulerAngles, _move.speed, GameTime.Timer.Second);
			}
			else if (null != _entity && !_entity.hasView && _entity.IsMount)
			{
				_move.NetMoveTo(base._pos, Vector3.zero, immediately: true);
			}
			else
			{
				_move.NetMoveTo(base._pos, Vector3.zero);
			}
		}
	}

	protected void RPC_S2C_NetRotation(uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
		int data = stream.Read<int>(new object[0]);
		Vector3 vector = VCUtils.UncompressEulerAngle(data);
		Quaternion rotation = Quaternion.Euler(vector);
		base.transform.rotation = rotation;
		base.rot = rotation;
		if (!base.hasOwnerAuth && null != _move)
		{
			_move.NetRotateTo(vector);
		}
	}

	protected virtual void RPC_S2C_AiNetworkIKTarget(uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
		if (!base.hasOwnerAuth && stream.TryRead<Vector3>(out var _) && !(null == base.Runner))
		{
		}
	}

	protected override void RPC_S2C_LostController(uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
		base.authId = -1;
		ResetContorller();
		if (base.canGetAuth)
		{
			RPCServer(EPacketType.PT_InGame_SetController);
		}
	}

	protected virtual void RPC_S2C_PlayAnimation(uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
		stream.Read<string>(new object[0]);
		if (!base.hasOwnerAuth)
		{
		}
	}

	protected virtual void RPC_S2C_SyncEquips(uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
		int[] array = stream.Read<int[]>(new object[0]);
		if (null == _entity)
		{
			return;
		}
		EquipmentCmpt cmpt = _entity.GetCmpt<EquipmentCmpt>();
		if (null == cmpt)
		{
			return;
		}
		int[] array2 = array;
		foreach (int num in array2)
		{
			if (num != -1)
			{
				ItemObject itemObject = PeSingleton<ItemMgr>.Instance.Get(num);
				if (itemObject != null)
				{
					cmpt.PutOnEquipment(itemObject);
				}
			}
		}
	}

	protected virtual void RPC_S2C_ApplyHpChange(uLink.BitStream stream, uLink.NetworkMessageInfo info)
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

	protected virtual void RPC_S2C_Death(uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
		stream.Read<int>(new object[0]);
		death = true;
		if (!(null == base.Runner) && !(null == base.Runner.SkEntityPE))
		{
			ItemDropPeEntity itemDropPeEntity = base.Runner.SkEntityPE.gameObject.GetComponent<ItemDropPeEntity>();
			if (null == itemDropPeEntity)
			{
				itemDropPeEntity = base.Runner.SkEntityPE.gameObject.AddComponent<ItemDropPeEntity>();
			}
			InitDeadItems(itemDropPeEntity, dropItems);
		}
	}

	protected virtual void RPC_S2C_Turn(uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
		float y = stream.Read<float>(new object[0]);
		base.transform.rotation = Quaternion.Euler(0f, y, 0f);
		if (null != base.Runner)
		{
			_viewTrans.rotation = base.transform.rotation;
		}
	}

	protected void RPC_S2C_RifleAim(uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
		if (!base.hasOwnerAuth && !(null == base.Runner))
		{
			stream.Read<bool>(new object[0]);
		}
	}

	protected void RPC_S2C_SetIKPositionWeight(uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
		if (base.hasOwnerAuth || null == base.Runner)
		{
			return;
		}
		AvatarIKGoal avatarIKGoal = stream.Read<AvatarIKGoal>(new object[0]);
		float num = stream.Read<float>(new object[0]);
		AiObject aiObject = base.Runner as AiObject;
		if (aiObject != null)
		{
			switch (avatarIKGoal)
			{
			case AvatarIKGoal.LeftFoot:
				aiObject.SetLeftFootIKWeight(num);
				break;
			case AvatarIKGoal.LeftHand:
				aiObject.SetLeftHandIKWeight(num);
				break;
			case AvatarIKGoal.RightFoot:
				aiObject.SetRightFootIKWeight(num);
				break;
			case AvatarIKGoal.RightHand:
				aiObject.SetRightHandIKWeight(num);
				break;
			}
		}
	}

	protected void RPC_S2C_SetIKPosition(uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
		if (base.hasOwnerAuth || null == base.Runner)
		{
			return;
		}
		AvatarIKGoal avatarIKGoal = stream.Read<AvatarIKGoal>(new object[0]);
		Vector3 vector = stream.Read<Vector3>(new object[0]);
		AiObject aiObject = base.Runner as AiObject;
		if (aiObject != null)
		{
			switch (avatarIKGoal)
			{
			case AvatarIKGoal.LeftFoot:
				aiObject.SetLeftFootIKPosition(vector);
				break;
			case AvatarIKGoal.LeftHand:
				aiObject.SetLeftHandIKPosition(vector);
				break;
			case AvatarIKGoal.RightFoot:
				aiObject.SetRightFootIKPosition(vector);
				break;
			case AvatarIKGoal.RightHand:
				aiObject.SetRightHandIKPosition(vector);
				break;
			}
		}
	}

	protected void RPC_S2C_SetIKRotationWeight(uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
		if (!base.hasOwnerAuth && !(null == base.Runner))
		{
			AvatarIKGoal goal = stream.Read<AvatarIKGoal>(new object[0]);
			float value = stream.Read<float>(new object[0]);
			AiObject aiObject = base.Runner as AiObject;
			if (aiObject != null)
			{
				aiObject.SetIKRotationWeight(goal, value);
			}
		}
	}

	protected void RPC_S2C_SetIKRotation(uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
		if (!base.hasOwnerAuth && !(null == base.Runner))
		{
			AvatarIKGoal goal = stream.Read<AvatarIKGoal>(new object[0]);
			Quaternion goalPosition = stream.Read<Quaternion>(new object[0]);
			AiObject aiObject = base.Runner as AiObject;
			if (aiObject != null)
			{
				aiObject.SetIKRotation(goal, goalPosition);
			}
		}
	}

	protected virtual void RPC_S2C_SetBool_String(uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
		if (!base.hasOwnerAuth && !(null == base.Runner))
		{
			string text = stream.Read<string>(new object[0]);
			bool value = stream.Read<bool>(new object[0]);
			AiObject aiObject = base.Runner as AiObject;
			if (aiObject != null)
			{
				aiObject.SetBool(text, value);
			}
		}
	}

	protected virtual void RPC_S2C_SetBool_Int(uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
		if (!base.hasOwnerAuth && !(null == base.Runner))
		{
			int id = stream.Read<int>(new object[0]);
			bool value = stream.Read<bool>(new object[0]);
			AiObject aiObject = base.Runner as AiObject;
			if (aiObject != null)
			{
				aiObject.SetBool(id, value);
			}
		}
	}

	protected virtual void RPC_S2C_SetVector_String(uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
		if (!base.hasOwnerAuth && !(null == base.Runner))
		{
			string text = stream.Read<string>(new object[0]);
			Vector3 value = stream.Read<Vector3>(new object[0]);
			AiObject aiObject = base.Runner as AiObject;
			if (aiObject != null)
			{
				aiObject.SetVector(text, value);
			}
		}
	}

	protected virtual void RPC_S2C_SetVector_Int(uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
		if (!base.hasOwnerAuth && !(null == base.Runner))
		{
			int id = stream.Read<int>(new object[0]);
			Vector3 value = stream.Read<Vector3>(new object[0]);
			AiObject aiObject = base.Runner as AiObject;
			if (aiObject != null)
			{
				aiObject.SetVector(id, value);
			}
		}
	}

	protected virtual void RPC_S2C_SetInteger_String(uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
		if (!base.hasOwnerAuth && !(null == base.Runner))
		{
			string text = stream.Read<string>(new object[0]);
			int value = stream.Read<int>(new object[0]);
			AiObject aiObject = base.Runner as AiObject;
			if (aiObject != null)
			{
				aiObject.SetInteger(text, value);
			}
		}
	}

	protected virtual void RPC_S2C_SetInteger_Int(uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
		if (!base.hasOwnerAuth && !(null == base.Runner))
		{
			int id = stream.Read<int>(new object[0]);
			int value = stream.Read<int>(new object[0]);
			AiObject aiObject = base.Runner as AiObject;
			if (aiObject != null)
			{
				aiObject.SetInteger(id, value);
			}
		}
	}

	protected virtual void RPC_S2C_SetLayerWeight(uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
		if (!base.hasOwnerAuth && !(null == base.Runner))
		{
			int layerIndex = stream.Read<int>(new object[0]);
			float weight = stream.Read<float>(new object[0]);
			AiObject aiObject = base.Runner as AiObject;
			if (aiObject != null)
			{
				aiObject.SetLayerWeight(layerIndex, weight);
			}
		}
	}

	protected virtual void RPC_S2C_SetLookAtWeight(uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
		if (!base.hasOwnerAuth && !(null == base.Runner))
		{
			float value = stream.Read<float>(new object[0]);
			AiObject aiObject = base.Runner as AiObject;
			if (aiObject != null)
			{
				aiObject.LookAtWeight(value);
			}
		}
	}

	protected virtual void RPC_S2C_SetLookAtPosition(uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
		if (!base.hasOwnerAuth && !(null == base.Runner))
		{
			Vector3 vector = stream.Read<Vector3>(new object[0]);
			AiObject aiObject = base.Runner as AiObject;
			if (aiObject != null && vector != Vector3.zero)
			{
				aiObject.LookAtPosition(vector);
			}
		}
	}

	protected virtual void RPC_S2C_SetBool(uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
		if (!base.hasOwnerAuth && !(null == base.Runner))
		{
			int num = stream.Read<int>(new object[0]);
			bool value = stream.Read<bool>(new object[0]);
			if (animatorCmpt != null)
			{
				animatorCmpt.SetBool(num, value);
			}
		}
	}

	protected virtual void RPC_S2C_SetTrigger(uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
		if (!base.hasOwnerAuth && !(null == base.Runner))
		{
			string trigger = stream.Read<string>(new object[0]);
			if (animatorCmpt != null)
			{
				animatorCmpt.SetTrigger(trigger);
			}
		}
	}

	protected virtual void RPC_S2C_SetMoveMode(uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
		if (!base.hasOwnerAuth && !(null == base.Runner))
		{
			int mode = stream.Read<int>(new object[0]);
			if (_move != null)
			{
				_move.mode = (MoveMode)mode;
			}
		}
	}

	protected virtual void RPC_S2C_HoldWeapon(uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
		if (!base.hasOwnerAuth && !(null == base.Runner))
		{
		}
	}

	protected virtual void RPC_S2C_SwitchHoldWeapon(uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
		if (!base.hasOwnerAuth && !(null == base.Runner))
		{
		}
	}

	protected virtual void RPC_S2C_SwordAttack(uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
		if (!base.hasOwnerAuth && !(null == base.Runner))
		{
			Vector3 dir = stream.Read<Vector3>(new object[0]);
			if (Equipment != null)
			{
				Equipment.SwordAttack(dir);
			}
		}
	}

	protected virtual void RPC_S2C_TwoHandWeaponAttack(uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
		if (!base.hasOwnerAuth && !(null == base.Runner))
		{
			Vector3 dir = stream.Read<Vector3>(new object[0]);
			int attackModeIndex = stream.Read<int>(new object[0]);
			int time = stream.Read<int>(new object[0]);
			if (Equipment != null)
			{
				Equipment.TwoHandWeaponAttack(dir, attackModeIndex, time);
			}
		}
	}

	protected virtual void RPC_S2C_SetIKAim(uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
		if (!base.hasOwnerAuth && !(null == base.Runner))
		{
		}
	}

	protected virtual void RPC_S2C_Fadein(uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
		if (!base.hasOwnerAuth && !(null == base.Runner))
		{
			float time = stream.Read<float>(new object[0]);
			if (View != null)
			{
				View.Fadein(time);
			}
		}
	}

	protected virtual void RPC_S2C_Fadeout(uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
		if (!base.hasOwnerAuth && !(null == base.Runner))
		{
			float time = stream.Read<float>(new object[0]);
			if (View != null)
			{
				View.Fadeout(time);
			}
		}
	}

	protected void RPC_C2S_ResponseDeadObjItem(uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
		if (stream.Read<int>(new object[0]) != 0)
		{
			dropItems = stream.Read<ItemSample[]>(new object[0]);
		}
	}

	protected void RPC_S2C_ResetPosition(uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
		Vector3 vector = stream.Read<Vector3>(new object[0]);
		base.transform.position = vector;
		base._pos = vector;
		if (!(null == _move))
		{
			if (_entity.Race == ERace.Mankind && _entity.proto == EEntityProto.Monster)
			{
				_move.AddNetTransInfo(base._pos, base.rot.eulerAngles, _move.speed, GameTime.Timer.Second);
			}
			else
			{
				_move.NetMoveTo(base._pos, Vector3.zero, immediately: true);
			}
			if (this is AiAdNpcNetwork && PeGameMgr.IsMultiStory)
			{
				(this as AiAdNpcNetwork).npcCmpt.FixedPointPos = base._pos;
			}
		}
	}

	protected void RPC_S2C_ScenarioId(uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
		int scenarioId = stream.Read<int>(new object[0]);
		PeEntity peEntity = PeSingleton<EntityMgr>.Instance.Get(base.Id);
		if (null != peEntity)
		{
			peEntity.scenarioId = scenarioId;
		}
	}

	protected void RPC_S2C_AvatarData(uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
		CreateAi();
		if (ExternId >= 10000)
		{
			byte[] data = stream.Read<byte[]>(new object[0]);
			CustomCharactor.CustomData customData = new CustomCharactor.CustomData();
			customData.Deserialize(data);
			if (null != _entity)
			{
				PeEntityCreator.ApplyCustomCharactorData(_entity, customData);
			}
		}
		RPCServer(EPacketType.PT_AI_ExternData);
	}
}

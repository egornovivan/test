using System.Collections;
using System.Collections.Generic;
using ItemAsset;
using Pathea;
using SkillAsset;
using uLink;
using UnityEngine;
using WhiteCat;

public class CreationNetwork : SkNetworkInterface
{
	protected int objectID;

	private int _ownerId = -1;

	private BehaviourController cc;

	internal DestroyHandler OnDestroyHandle;

	internal List<SkNetworkInterface> Passangers = new List<SkNetworkInterface>();

	private DragArticleAgent drawItem;

	private PeTrans _viewTrans;

	private CreationSkEntity _entity;

	private bool _bLock;

	private bool _physicsEnabled;

	public int ObjectID => objectID;

	internal float HP { get; set; }

	internal float MaxHP { get; set; }

	internal float Fuel { get; set; }

	internal float MaxFuel { get; set; }

	internal PlayerNetwork Driver { get; set; }

	protected override void OnPEInstantiate(uLink.NetworkMessageInfo info)
	{
		_id = info.networkView.initialData.Read<int>(new object[0]);
		_ownerId = info.networkView.initialData.Read<int>(new object[0]);
		_teamId = info.networkView.initialData.Read<int>(new object[0]);
		base._pos = base.transform.position;
		base.rot = base.transform.rotation;
	}

	protected override void OnPEStart()
	{
		BindSkAction();
		BindAction(EPacketType.PT_CR_InitData, RPC_S2C_InitData);
		BindAction(EPacketType.PT_InGame_SetController, RPC_S2C_SetController);
		BindAction(EPacketType.PT_InGame_LostController, RPC_S2C_LostController);
		BindAction(EPacketType.PT_CR_ChargeFuel, RPC_S2C_ChargeFuel);
		BindAction(EPacketType.PT_CR_SkillCast, RPC_S2C_SkillCast);
		BindAction(EPacketType.PT_CR_SkillCastShoot, RPC_S2C_SkillCastShoot);
		BindAction(EPacketType.PT_CR_ApplyHpChange, RPC_S2C_ApplyHpChange);
		BindAction(EPacketType.PT_CR_Death, RPC_S2C_Death);
		BindAction(EPacketType.PT_CR_SyncEnergyDelta, RPC_S2C_SyncEnergyDelta);
		BindAction(EPacketType.PT_CR_SyncPos, RPC_S2C_SyncPos);
		BindAction(EPacketType.PT_InGame_MissionMoveAircraft, RPC_S2C_MissionMoveAircraft);
		RPCServer(EPacketType.PT_CR_InitData);
	}

	public override void OnSpawned(GameObject go)
	{
		base.OnSpawned(go);
		cc = go.GetComponent<BehaviourController>();
		_viewTrans = go.GetComponent<PeTrans>();
		_viewTrans.position = base.transform.position;
		StartCoroutine(AuthorityCheckCoroutine());
	}

	protected override void CheckAuthority()
	{
		if (!base.hasOwnerAuth || !(Driver != null) || Driver.Id != base.authId)
		{
			base.CheckAuthority();
		}
	}

	private IEnumerator SyncMove()
	{
		base._pos = base.transform.position;
		base.rot = base.transform.rotation;
		while (true)
		{
			if (_bLock)
			{
				yield return new WaitForSeconds(1f / uLink.Network.sendRate);
				continue;
			}
			if (base.hasOwnerAuth)
			{
				if (null != base.Runner)
				{
					CreationNetwork creationNetwork = this;
					Vector3 position = _viewTrans.position;
					base.transform.position = position;
					creationNetwork._pos = position;
					CreationNetwork creationNetwork2 = this;
					Quaternion rotation = _viewTrans.rotation;
					base.transform.rotation = rotation;
					creationNetwork2.rot = rotation;
				}
				if (!(null != cc))
				{
					break;
				}
				if (cc.enabled)
				{
					byte[] data = cc.C2S_GetData();
					if (data != null)
					{
						RPCServer(EPacketType.PT_CR_SyncPos, data);
					}
				}
			}
			if (cc != null)
			{
				float energyDelta = 0f;
				cc.GetAndResetDeltaEnergy(ref energyDelta);
				if (energyDelta != 0f)
				{
					RPCServer(EPacketType.PT_CR_SyncEnergyDelta, energyDelta);
				}
			}
			yield return new WaitForSeconds(1f / uLink.Network.sendRate);
		}
		RPCServer(EPacketType.PT_CR_Death);
	}

	protected override void ResetContorller()
	{
		base.ResetContorller();
		if (base.hasOwnerAuth)
		{
			if (cc != null)
			{
				cc.ResetHost(PeSingleton<PeCreature>.Instance.mainPlayer.Id);
			}
		}
		else if (cc != null)
		{
			cc.ResetHost(-1);
		}
	}

	public override void InitForceData()
	{
		if (null != base.Runner && null != base.Runner.SkEntityBase && cc is CarrierController)
		{
			if (null == Driver)
			{
				base.Runner.SkEntityBase.SetAttribute(91, 99f);
			}
			else
			{
				base.Runner.SkEntityBase.SetAttribute(91, Driver.Id);
			}
		}
	}

	private void SendBaseAttr()
	{
		if (base.hasOwnerAuth && null != base.Runner && null != base.Runner.SkEntityBase)
		{
			byte[] array = base.Runner.SkEntityBase.Export();
			if (array != null && array.Length > 0)
			{
				RPCServer(EPacketType.PT_InGame_SKSendBaseAttrs, array);
			}
		}
	}

	public bool GetOn(CommonInterface runner, int seatIndex)
	{
		if (null != base.Runner && null != base.Runner.SkEntityBase && runner != null)
		{
			PeEntity component = base.Runner.SkEntityBase.GetComponent<PeEntity>();
			if (null != component && null != runner.SkEntityPE && null != runner.SkEntityPE.Entity)
			{
				PassengerCmpt cmpt = runner.SkEntityPE.Entity.GetCmpt<PassengerCmpt>();
				CarrierController component2 = component.GetComponent<CarrierController>();
				if (null != cmpt && null != component2)
				{
					cmpt.GetOn(component2, seatIndex, checkState: false);
					return true;
				}
			}
		}
		return false;
	}

	public void GetOff(Vector3 pos)
	{
		Passangers.Remove(this);
		foreach (SkNetworkInterface passanger in Passangers)
		{
			if (passanger != null && !passanger.hasOwnerAuth)
			{
				passanger._move.NetMoveTo(pos, Vector3.zero);
			}
		}
	}

	public void GetOff(Vector3 pos, EVCComponent seatType)
	{
		if (seatType != EVCComponent.cpSideSeat)
		{
			if (Driver != null && !Driver.hasOwnerAuth)
			{
				Driver._move.NetMoveTo(pos, Vector3.zero);
			}
			Driver = null;
		}
		else
		{
			GetOff(pos);
		}
	}

	public void AddPassanger(SkNetworkInterface pass)
	{
		if (!Passangers.Contains(pass))
		{
			Passangers.Add(pass);
		}
	}

	private void RPC_S2C_InitData(uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
		ItemObject itemObject = stream.Read<ItemObject>(new object[0]);
		Vector3 vector = stream.Read<Vector3>(new object[0]);
		base.transform.position = vector;
		base._pos = vector;
		Quaternion rotation = stream.Read<Quaternion>(new object[0]);
		base.transform.rotation = rotation;
		base.rot = rotation;
		HP = stream.Read<float>(new object[0]);
		MaxHP = stream.Read<float>(new object[0]);
		Fuel = stream.Read<float>(new object[0]);
		MaxFuel = stream.Read<float>(new object[0]);
		base.authId = stream.Read<int>(new object[0]);
		_ownerId = stream.Read<int>(new object[0]);
		if (itemObject == null)
		{
			Debug.LogWarning("CreationNetwork RPC_S2C_InitData null item.");
			return;
		}
		DragArticleAgent dragArticleAgent = DragArticleAgent.Create(DragArticleAgent.CreateItemDrag(itemObject.protoId), base._pos, Vector3.one, base.rot, _id, this, isCreation: true);
		if (dragArticleAgent != null && null != dragArticleAgent.itemLogic && null != dragArticleAgent.itemLogic.gameObject)
		{
			OnSpawned(dragArticleAgent.itemLogic.gameObject);
			_entity = dragArticleAgent.itemLogic.gameObject.GetComponent<CreationSkEntity>();
			if (_entity != null)
			{
				NetCmpt netCmpt = _entity.GetComponent<NetCmpt>();
				if (null == netCmpt)
				{
					netCmpt = _entity.gameObject.AddComponent<NetCmpt>();
				}
				netCmpt.network = this;
			}
		}
		else
		{
			Debug.LogWarningFormat("CreationNetwork RPC_S2C_InitData invalide agent:{0}", itemObject.protoId);
		}
		base.OnSkAttrInitEvent += SendBaseAttr;
		StartCoroutine(SyncMove());
	}

	protected override void RPC_S2C_SetController(uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
		base.authId = stream.Read<int>(new object[0]);
		_teamId = stream.Read<int>(new object[0]);
		ResetContorller();
		SendBaseAttr();
		if (!(_entity == null))
		{
			AIBehaviourController component = _entity.GetComponent<AIBehaviourController>();
			if ((bool)component)
			{
				component.SetCreater(_ownerId);
			}
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

	private void RPC_S2C_SyncEnergyDelta(uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
		if (stream.TryRead<float>(out var value) && cc != null)
		{
			cc.SetEnergy(value);
		}
	}

	private void RPC_S2C_SyncPos(uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
		if (base.hasOwnerAuth || _bLock || !(cc != null) || !stream.TryRead<byte[]>(out var value))
		{
			return;
		}
		cc.S2C_SetData(value);
		Vector3 position = cc.transform.position;
		base.transform.position = position;
		base._pos = position;
		Quaternion rotation = cc.transform.rotation;
		base.transform.rotation = rotation;
		base.rot = rotation;
		if (null != Driver)
		{
			Driver.UpdateDriverStatus(this);
		}
		foreach (SkNetworkInterface passanger in Passangers)
		{
			if (passanger != null)
			{
				passanger.UpdateDriverStatus(this);
			}
		}
	}

	private void RPC_S2C_ChargeFuel(uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
		float energy = stream.Read<float>(new object[0]);
		if (null != cc)
		{
			cc.SetEnergy(energy);
		}
	}

	protected virtual void RPC_S2C_SkillCast(uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
	}

	protected virtual void RPC_S2C_SkillCastShoot(uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
		int skillId = stream.Read<int>(new object[0]);
		Vector3 position = stream.Read<Vector3>(new object[0]);
		DefaultPosTarget target = new DefaultPosTarget(position);
		SkillRunner skillRunner = base.Runner as SkillRunner;
		if (null != skillRunner && !skillRunner.IsController)
		{
			skillRunner.RunEffOnProxy(skillId, target);
		}
	}

	protected void RPC_S2C_ApplyHpChange(uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
		float damage = stream.Read<float>(new object[0]);
		int lifeLeft = stream.Read<int>(new object[0]);
		uLink.NetworkViewID viewID = stream.Read<uLink.NetworkViewID>(new object[0]);
		CommonInterface caster = null;
		uLink.NetworkView networkView = uLink.NetworkView.Find(viewID);
		if (null != networkView)
		{
			NetworkInterface component = networkView.GetComponent<NetworkInterface>();
			if (null != component && null != component.Runner)
			{
				caster = component.Runner;
			}
		}
		if (null != base.Runner)
		{
			base.Runner.NetworkApplyDamage(caster, damage, lifeLeft);
		}
	}

	protected void RPC_S2C_Death(uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
	}

	protected void RPC_S2C_MissionMoveAircraft(uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
		bool flag = stream.Read<bool>(new object[0]);
		int num = stream.Read<int>(new object[0]);
		if (flag)
		{
			_bLock = true;
			cc.networkEnabled = false;
			Object @object = Resources.Load("Cutscene Clips/PathClip" + (num + 1));
			if (!(@object == null))
			{
				GameObject pathObj = Object.Instantiate(@object) as GameObject;
				MoveByPath moveByPath = _entity.gameObject.AddComponent<MoveByPath>();
				moveByPath.SetDurationDelay(15f, 0f);
				moveByPath.StartMove(pathObj, RotationMode.ConstantUp);
				_physicsEnabled = cc.physicsEnabled;
				cc.physicsEnabled = false;
				cc.creationController.visible = true;
			}
			return;
		}
		Vector3 vector = stream.Read<Vector3>(new object[0]);
		_bLock = true;
		GameObject gameObject = _entity.gameObject;
		if (gameObject == null)
		{
			return;
		}
		Object object2 = Resources.Load("Cutscene Clips/PathClip" + (num + 5));
		if (object2 == null)
		{
			return;
		}
		GameObject pathObj2 = Object.Instantiate(object2) as GameObject;
		MoveByPath moveByPath2 = gameObject.AddComponent<MoveByPath>();
		moveByPath2.SetDurationDelay(15f, 0f);
		moveByPath2.AddEndListener(delegate
		{
			if (_physicsEnabled)
			{
				cc.physicsEnabled = true;
			}
			_bLock = false;
			cc.networkEnabled = true;
		});
		moveByPath2.StartMove(pathObj2, RotationMode.ConstantUp);
	}
}

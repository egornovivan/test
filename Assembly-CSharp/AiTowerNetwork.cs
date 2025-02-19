using System.Collections;
using ItemAsset;
using Pathea;
using PeMap;
using uLink;
using UnityEngine;

public class AiTowerNetwork : AiNetwork
{
	private AiTowerSyncData _syncData;

	private TowerCmpt _towerCmpt;

	public int ownerId { get; protected set; }

	public int aimId { get; protected set; }

	protected override void OnPEInstantiate(uLink.NetworkMessageInfo info)
	{
		_id = info.networkView.initialData.Read<int>(new object[0]);
		_scale = info.networkView.initialData.Read<float>(new object[0]);
		ownerId = info.networkView.initialData.Read<int>(new object[0]);
		_teamId = info.networkView.initialData.Read<int>(new object[0]);
		death = false;
		base.authId = ownerId;
		base._pos = base.transform.position;
		base.rot = base.transform.rotation;
	}

	protected override void OnPEStart()
	{
		PlayerNetwork.OnTeamChangedEventHandler += InitMapInfo;
		BindSkAction();
		BindAction(EPacketType.PT_Tower_InitData, RPC_Tower_InitData);
		BindAction(EPacketType.PT_Tower_Move, RPC_Tower_Move);
		BindAction(EPacketType.PT_Tower_Target, RPC_Tower_Target);
		BindAction(EPacketType.PT_Tower_AimPosition, RPC_S2C_AimPosition);
		BindAction(EPacketType.PT_Tower_Fire, RPC_S2C_Fire);
		BindAction(EPacketType.PT_Tower_LostEnemy, RPC_S2C_LostEnemy);
		BindAction(EPacketType.PT_InGame_SetController, RPC_S2C_SetController);
		BindAction(EPacketType.PT_InGame_LostController, RPC_S2C_LostController);
		BindAction(EPacketType.PT_InGame_InitData, RPC_S2C_InitData);
		RPCServer(EPacketType.PT_Tower_InitData);
	}

	protected override void OnPEDestroy()
	{
		StopAllCoroutines();
		PlayerNetwork.OnTeamChangedEventHandler -= InitMapInfo;
		RemoveMapInfo();
		DragArticleAgent.Destory(base.Id);
		if (null != base.Runner)
		{
			base.Runner.InitNetworkLayer(null);
			Object.Destroy(base.Runner.gameObject);
		}
	}

	public override void OnSpawned(GameObject obj)
	{
		base.OnSpawned(obj);
		_towerCmpt = _entity.GetCmpt<TowerCmpt>();
		if (null == _towerCmpt)
		{
			LogManager.Error("error tower cmpt:" + base.Id);
		}
		RPCServer(EPacketType.PT_InGame_InitData);
	}

	private void InitMapInfo()
	{
		if (!(null == Singleton<ForceSetting>.Instance))
		{
			if (Singleton<ForceSetting>.Instance.Conflict(base.TeamId, PlayerNetwork.mainPlayerId))
			{
				RemoveMapInfo();
			}
			else
			{
				AddMapInfo();
			}
		}
	}

	private void AddMapInfo()
	{
		TowerMark towerMark = new TowerMark();
		towerMark.position = base._pos;
		towerMark.ID = base.Id;
		ItemObject itemObject = PeSingleton<ItemMgr>.Instance.Get(base.Id);
		if (itemObject != null)
		{
			towerMark.text = itemObject.protoData.GetName();
		}
		PeEntity peEntity = PeSingleton<EntityMgr>.Instance.Get(base.Id);
		if (null != peEntity)
		{
			towerMark.campId = Mathf.RoundToInt(peEntity.GetAttribute(AttribType.CampID));
		}
		PeSingleton<LabelMgr>.Instance.Add(towerMark);
		PeSingleton<TowerMark.Mgr>.Instance.Add(towerMark);
	}

	private void RemoveMapInfo()
	{
		TowerMark towerMark = PeSingleton<TowerMark.Mgr>.Instance.Find((TowerMark tower) => base.Id == tower.ID);
		if (towerMark != null)
		{
			PeSingleton<LabelMgr>.Instance.Remove(towerMark);
			PeSingleton<TowerMark.Mgr>.Instance.Remove(towerMark);
		}
	}

	protected override IEnumerator SyncMove()
	{
		while (base.hasOwnerAuth)
		{
			if (null != _towerCmpt && !death && (_towerCmpt.ChassisY != _syncData.ChassisY || !_towerCmpt.PitchEuler.Equals(_syncData.PitchEuler)))
			{
				_syncData.ChassisY = _towerCmpt.ChassisY;
				_syncData.PitchEuler = _towerCmpt.PitchEuler;
			}
			yield return new WaitForSeconds(1f / uLink.Network.sendRate);
		}
	}

	private IEnumerator WaitForTarget()
	{
		yield return new WaitForSeconds(3f);
		int oldId = -1;
		while (true)
		{
			if (oldId != aimId)
			{
				PeEntity tarEntity = PeSingleton<EntityMgr>.Instance.Get(aimId);
				if (null != tarEntity && tarEntity.hasView && null != _towerCmpt)
				{
					_towerCmpt.Target = tarEntity.centerBone;
					oldId = aimId;
				}
			}
			yield return new WaitForSeconds(1f);
		}
	}

	public override void InitForceData()
	{
		if (base.Runner == null)
		{
			return;
		}
		if (PeGameMgr.IsMultiStory)
		{
			if (null != base.Runner && null != base.Runner.SkEntityBase)
			{
				base.Runner.SkEntityBase.SetAttribute(91, 1f);
			}
		}
		else if (base.TeamId != -1 && null != base.Runner && null != base.Runner.SkEntityBase)
		{
			base.Runner.SkEntityBase.SetAttribute(91, base.TeamId);
			base.Runner.SkEntityBase.SetAttribute(95, base.TeamId);
			base.Runner.SkEntityBase.SetAttribute(92, base.TeamId);
		}
	}

	public void RequestAimTarget(int skEntityId)
	{
		RPCServer(EPacketType.PT_Tower_AimPosition, skEntityId);
	}

	public void RequestFire(int skEntityId)
	{
		RPCServer(EPacketType.PT_Tower_Fire, skEntityId);
	}

	public void RequestEnemyLost()
	{
		RPCServer(EPacketType.PT_Tower_LostEnemy);
	}

	private void RPC_Tower_InitData(uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
		Vector3 vector = stream.Read<Vector3>(new object[0]);
		base.transform.position = vector;
		base._pos = vector;
		Quaternion rotation = stream.Read<Quaternion>(new object[0]);
		base.transform.rotation = rotation;
		base.rot = rotation;
		ItemObject itemObject = stream.Read<ItemObject>(new object[0]);
		if (itemObject == null)
		{
			LogManager.Error("Invalid tower item");
			return;
		}
		Drag cmpt = itemObject.GetCmpt<Drag>();
		if (cmpt == null)
		{
			return;
		}
		DragTowerAgent dragTowerAgent = new DragTowerAgent(cmpt, base.transform.position, Vector3.one, base.transform.rotation, base.Id, this);
		dragTowerAgent.Create();
		SceneMan.AddSceneObj(dragTowerAgent);
		_entity = PeSingleton<EntityMgr>.Instance.Get(base.Id);
		if (!(null == _entity))
		{
			TowerProtoDb.Item item = TowerProtoDb.Get(itemObject.protoData.towerEntityId);
			if (item != null)
			{
				base.gameObject.name = item.name + "_" + base.Id;
			}
			OnSpawned(_entity.GetGameObject());
			InitMapInfo();
		}
	}

	private void RPC_Tower_Move(uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
	}

	private void RPC_Tower_Target(uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
		if (!base.hasOwnerAuth && !(null == _towerCmpt) && stream.TryRead<float>(out var value) && stream.TryRead<Vector3>(out var value2))
		{
			_towerCmpt.ApplyChassis(value);
			_towerCmpt.ApplyPitchEuler(value2);
		}
	}

	private void RPC_S2C_AimPosition(uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
		if (!base.hasOwnerAuth)
		{
			aimId = stream.Read<int>(new object[0]);
			PeEntity peEntity = PeSingleton<EntityMgr>.Instance.Get(aimId);
			if (!(null == peEntity) && null != _entity && null != _towerCmpt)
			{
				_towerCmpt.Target = peEntity.centerBone;
			}
		}
	}

	private void RPC_S2C_Fire(uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
		if (!base.hasOwnerAuth)
		{
			int entityId = stream.Read<int>(new object[0]);
			PeEntity peEntity = PeSingleton<EntityMgr>.Instance.Get(entityId);
			if (!(null == peEntity) && !(peEntity.skEntity == null) && null != _entity && null != _towerCmpt)
			{
				_towerCmpt.Fire(peEntity.skEntity);
			}
		}
	}

	private void RPC_S2C_LostEnemy(uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
		aimId = -1;
		if (null != _entity && null != _towerCmpt)
		{
			_towerCmpt.Target = null;
		}
	}

	protected override void RPC_S2C_InitData(uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
		aimId = stream.Read<int>(new object[0]);
		StartCoroutine(WaitForTarget());
	}
}

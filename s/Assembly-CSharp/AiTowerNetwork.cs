using System.Collections;
using System.Collections.Generic;
using ItemAsset;
using Pathea;
using uLink;
using UnityEngine;

public class AiTowerNetwork : AiObject
{
	protected int batteryPowerCostNum = 140;

	protected int AmmoCostNum = 1;

	protected int _ownerId = -1;

	protected ItemObject towerItem;

	protected LifeLimit towerLifeLimit;

	protected Durability towerDurability;

	protected bool _hasRecord;

	public int OwnerId => _ownerId;

	public int aimTarget { get; protected set; }

	protected override void OnPEInstantiate(uLink.NetworkMessageInfo info)
	{
		SetParent("TowerNetMgr");
		_objType = AiObjectType.AiObjectType_Tower;
		_id = info.networkView.initialData.Read<int>(new object[0]);
		m_scale = info.networkView.initialData.Read<float>(new object[0]);
		_ownerId = info.networkView.initialData.Read<int>(new object[0]);
		_teamId = info.networkView.initialData.Read<int>(new object[0]);
		_worldId = info.networkView.group;
		base.authId = OwnerId;
		towerItem = ItemManager.GetItemByID(base.Id);
		if (towerItem != null)
		{
			towerLifeLimit = towerItem.GetCmpt<LifeLimit>();
			towerDurability = towerItem.GetCmpt<Durability>();
		}
		Add(this);
		InitializeData();
		AddSkEntity();
		base.PEInstantiateEvent += DropItemManager.OnInitialized;
		base.PEDestroyEvent += DropItemManager.OnDestroy;
		Player.PlayerDisconnected += OnPlayerDisconnect;
		Player.OnHeartBeatTimeoutEvent += OnPlayerDisconnect;
	}

	protected override void OnPEStart()
	{
		base.OnPEStart();
		BindAction(EPacketType.PT_Tower_InitData, RPC_Tower_InitData);
		BindAction(EPacketType.PT_Tower_Move, RPC_Tower_Move);
		BindAction(EPacketType.PT_Tower_Target, RPC_Tower_Target);
		BindAction(EPacketType.PT_Tower_AimPosition, RPC_C2S_AimPosition);
		BindAction(EPacketType.PT_Tower_Fire, RPC_C2S_Fire);
		BindAction(EPacketType.PT_Tower_LostEnemy, RPC_C2S_LostEnemy);
		BindAction(EPacketType.PT_InGame_InitData, RPC_C2S_InitData);
	}

	protected override void OnPEDestroy()
	{
		base.OnPEDestroy();
		base.PEInstantiateEvent -= DropItemManager.OnInitialized;
		base.PEDestroyEvent -= DropItemManager.OnDestroy;
		Player.PlayerDisconnected -= OnPlayerDisconnect;
		Player.OnHeartBeatTimeoutEvent -= OnPlayerDisconnect;
	}

	protected override void InitializeData()
	{
		InitCmpt();
		if (towerItem != null && towerItem.protoData != null)
		{
			TowerProtoDb.Item item = TowerProtoDb.Get(towerItem.protoData.towerEntityId);
			if (item != null)
			{
				base.gameObject.name = item.name + "_" + base.Id;
			}
		}
		if (!DropItemManager.HasRecord(base.Id))
		{
			SyncSave();
		}
	}

	public override void SkCreater()
	{
		if (!(_skEntity == null) && _skEntity._attribs != null)
		{
			if (towerItem != null && towerItem.protoData != null)
			{
				SKAttribute.InitTowerBaseAttrs(_skEntity._attribs, towerItem.protoData.towerEntityId, out _skEntity._baseAttribs);
			}
			if (ServerConfig.IsCooperation)
			{
				_skEntity.SetAllAttribute(AttribType.DefaultPlayerID, 1f);
				return;
			}
			_skEntity.SetAllAttribute(AttribType.DefaultPlayerID, base.TeamId);
			_skEntity.SetAllAttribute(AttribType.DamageID, base.TeamId);
			_skEntity.SetAllAttribute(AttribType.CampID, base.TeamId);
		}
	}

	protected override IEnumerator DestroyAiObjectCoroutine()
	{
		yield return new WaitForSeconds(30f);
		ItemManager.RemoveItem(base.Id);
		NetInterface.NetDestroy(this);
	}

	internal override void OnDeath(int casterId = 0)
	{
		base.OnDeath(casterId);
		Delete();
	}

	internal override void OnDamage(int casterId, float damage)
	{
		base.OnDamage(casterId, damage);
		if (null != _skEntity && towerItem != null)
		{
			float attribute = _skEntity.GetAttribute(AttribType.Hp);
			if (towerLifeLimit != null)
			{
				towerLifeLimit.SetValue(attribute);
			}
			if (towerDurability != null)
			{
				towerDurability.SetValue(attribute);
			}
		}
	}

	public void Delete()
	{
		NetObjectData netObjectData = new NetObjectData();
		netObjectData.DeleteData(base.Id);
		AsyncSqlite.AddRecord(netObjectData);
	}

	public void SyncSave()
	{
		TowerData towerData = new TowerData();
		towerData.ExportData(this);
		AsyncSqlite.AddRecord(towerData);
	}

	public override void ItemAttrChange(int itemObjId, float num)
	{
		if (towerItem == null || towerItem.protoData == null)
		{
			return;
		}
		TowerProtoDb.Item item = TowerProtoDb.Get(towerItem.protoData.towerEntityId);
		if (item == null || item.bulletData == null)
		{
			return;
		}
		switch (item.bulletData.bulletType)
		{
		case 1:
		{
			Tower cmpt2 = towerItem.GetCmpt<Tower>();
			if (cmpt2 != null && (float)cmpt2.curCostValue >= num)
			{
				cmpt2.curCostValue -= (int)num;
			}
			break;
		}
		case 2:
		{
			Energy cmpt = towerItem.GetCmpt<Energy>();
			if (cmpt != null && cmpt.energy.current >= num)
			{
				cmpt.energy.current -= (int)num;
			}
			break;
		}
		}
		ChannelNetwork.SyncItem(base.WorldId, towerItem);
	}

	public override void WeaponReload(Player player, int objId, int oldProtoId, int newProtoId, float magazineSize)
	{
		ItemObject itemByID = ItemManager.GetItemByID(objId);
		if (itemByID == null)
		{
			return;
		}
		Tower cmpt = itemByID.GetCmpt<Tower>();
		if (cmpt != null)
		{
			List<ItemObject> effItems = new List<ItemObject>();
			if (cmpt.curCostValue != 0)
			{
				player.Package.AddSameItems(oldProtoId, cmpt.curCostValue, ref effItems);
				cmpt.curCostValue = 0;
			}
			player.Package.RemoveItem(newProtoId, (int)magazineSize, ref effItems);
			cmpt.curCostValue += (int)magazineSize;
			effItems.Add(itemByID);
			ChannelNetwork.SyncItemList(base.WorldId, effItems);
			player.SyncPackageIndex();
		}
	}

	private void RPC_Tower_InitData(uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
		ItemObject itemByID = ItemManager.GetItemByID(base.Id);
		if (itemByID != null)
		{
			RPCPeer(info.sender, EPacketType.PT_Tower_InitData, base.transform.position, base.transform.rotation, itemByID);
		}
	}

	private void RPC_Tower_Move(uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
		Vector3 vector = stream.Read<Vector3>(new object[0]);
		base.transform.position = vector;
		URPCProxy(EPacketType.PT_Tower_Move, vector);
	}

	private void RPC_Tower_Target(uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
		float num = stream.Read<float>(new object[0]);
		Vector3 vector = stream.Read<Vector3>(new object[0]);
		URPCProxy(EPacketType.PT_Tower_Target, num, vector);
	}

	private void RPC_C2S_AimPosition(uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
		int num2 = (aimTarget = stream.Read<int>(new object[0]));
		RPCOthers(EPacketType.PT_Tower_AimPosition, num2);
	}

	private void RPC_C2S_Fire(uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
		int num = stream.Read<int>(new object[0]);
		RPCOthers(EPacketType.PT_Tower_Fire, num);
	}

	private void RPC_C2S_LostEnemy(uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
		RPCOthers(EPacketType.PT_Tower_LostEnemy);
	}

	private void RPC_C2S_InitData(uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
		RPCPeer(info.sender, EPacketType.PT_InGame_InitData, aimTarget);
	}

	public void cost(ItemObject itemObj, ItemProperty prop, int costNum)
	{
	}
}

using System.Collections.Generic;
using ItemAsset;
using Pathea;
using uLink;
using UnityEngine;

public class AiMonsterNetwork : AiObject
{
	protected int _groupId;

	protected int _tdId;

	protected int _dungeonId;

	protected int _buffId;

	public int _backupTeamId { get; private set; }

	public int GroupId => _groupId;

	public int TdId => _tdId;

	public int DungeonId => _dungeonId;

	public Player LordPlayer { get; private set; }

	protected override void OnPEInstantiate(uLink.NetworkMessageInfo info)
	{
		SetParent("MonsterNetMgr");
		base.OnPEInstantiate(info);
		_groupId = info.networkView.initialData.Read<int>(new object[0]);
		_tdId = info.networkView.initialData.Read<int>(new object[0]);
		_dungeonId = info.networkView.initialData.Read<int>(new object[0]);
		base.colorType = info.networkView.initialData.Read<int>(new object[0]);
		info.networkView.initialData.Read<int>(new object[0]);
		m_fixId = info.networkView.initialData.Read<int>(new object[0]);
		_buffId = info.networkView.initialData.Read<int>(new object[0]);
		if (_tdId != -1)
		{
			AiTowerDefense.OnTDMonsterInstantiate(_tdId, this);
		}
		if (_groupId != -1)
		{
			AIGroupNetWork.OnGroupMonsterInstantiate(_groupId, this);
		}
		InitializeData();
		AddSkEntity();
		base.gameObject.name = $"TemplateId:{base.ExternId}, Id:{base.Id}, tdId:{TdId}, gpId:{GroupId}";
	}

	protected override void OnPEStart()
	{
		base.OnPEStart();
		if (m_fixId == -1 && _tdId == -1 && !ServerConfig.IsCustom && _dungeonId == -1)
		{
			CheckValidDist(Player.ValidDistance, InvalidDistEvent);
		}
	}

	protected override void OnPEDestroy()
	{
		base.OnPEDestroy();
		SPTerrainEvent.OnMonsterDeath(this);
	}

	public override void DropItem(NetInterface caster)
	{
		MonsterProtoDb.Item item = MonsterProtoDb.Get(base.ExternId);
		if (item == null)
		{
			return;
		}
		base.DropItemID.Clear();
		List<ItemSample> dropItems = ItemDropData.GetDropItems(item.dropItemId);
		if (dropItems != null && dropItems.Count != 0)
		{
			List<ItemSample> list = GetSpecialItem.MonsterItemAdd(base.ExternId, caster);
			if (list != null && list.Count > 0)
			{
				base.DropItemID.AddRange(list);
			}
			base.DropItemID.AddRange(dropItems);
			CreateDropScenes(base.DropItemID);
		}
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
		player.SyncItemList(array);
		player.SyncPackageIndex();
		player.SyncNewItem(base.DropItemID);
		base.DropItemID.Clear();
		NetInterface.NetDestroy(this);
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
		List<ItemObject> effItems = new List<ItemObject>(1);
		player.Package.AddSameItems(base.DropItemID[index], ref effItems);
		player.SyncItemList(effItems);
		player.SyncPackageIndex();
		player.SyncNewItem(base.DropItemID[index]);
		base.DropItemID.RemoveAt(index);
		if (base.DropItemID.Count <= 0)
		{
			NetInterface.NetDestroy(this);
		}
		return true;
	}

	protected override void RPC_C2S_SetController(uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
		if (null != LordPlayer)
		{
			LordPlayer.RPCPeer(info.sender, EPacketType.PT_Mount_ReqMonsterCtrl, LordPlayer.Id, base.Id);
			LordPlayer.RPCProxy(EPacketType.PT_Mount_AddMountMonster, LordPlayer.Id, base.Id);
			RPCPeer(info.sender, EPacketType.PT_InGame_SetController, base.authId);
		}
		else
		{
			base.RPC_C2S_SetController(stream, info);
		}
	}

	protected override void RPC_C2S_AiNetworkMovePostion(uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
		Vector3 vector = stream.Read<Vector3>(new object[0]);
		AiSynAttribute aiSynAttribute = mAiSynAttribute;
		Vector3 vector2 = vector;
		base.transform.position = vector2;
		aiSynAttribute.mv3Postion = vector2;
		if ((bool)LordPlayer && null != LordPlayer && LordPlayer._OnRide)
		{
			LordPlayer.SetPosition(base.transform.position);
			LordPlayer.SyncPosArea(base.transform.position);
		}
		URPCOthers(EPacketType.PT_AI_Move, vector);
	}

	public bool ForceGetController(Player player)
	{
		if (null != player)
		{
			base.authId = player.Id;
			RPCOthers(EPacketType.PT_InGame_SetController, base.authId);
			return true;
		}
		return false;
	}

	public bool MountByPlayer(Player player)
	{
		if (null != LordPlayer || null == player)
		{
			return false;
		}
		if (player.SetMount(this))
		{
			_backupTeamId = _teamId;
			LordPlayer = player;
			base.authId = player.Id;
			_teamId = player.TeamId;
			return true;
		}
		return false;
	}

	public bool DeMountByPlayer(Player player)
	{
		if (null != LordPlayer && LordPlayer.Id == player.Id && player.DelMount(this))
		{
			LordPlayer = null;
			base.authId = -1;
			_teamId = _backupTeamId;
			return true;
		}
		return false;
	}

	private void InvalidDistEvent()
	{
		NetInterface.NetDestroy(this);
	}
}

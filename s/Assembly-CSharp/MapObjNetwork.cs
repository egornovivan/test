using System.Collections;
using System.Collections.Generic;
using uLink;
using UnityEngine;

public class MapObjNetwork : SkNetworkInterface
{
	public IDoodad _doodad;

	public int _doodaProtoId;

	protected int _objType;

	protected int _defaultPlayerId;

	public Player _owner;

	private List<int> _itemList = new List<int>();

	private bool _hasRecord;

	private static List<MapObjNetwork> _mapObjNetworkMgr = new List<MapObjNetwork>();

	internal int ObjType => _objType;

	public int DefaultPlayerId => _defaultPlayerId;

	public int aimTarget { get; protected set; }

	protected override void OnPEStart()
	{
		base.OnPEStart();
		BindAction(EPacketType.PT_MO_InsertItemList, RPC_C2S_InsertItemList);
		BindAction(EPacketType.PT_MO_RequestItemList, RPC_C2S_RequestItemList);
		BindAction(EPacketType.PT_MO_GetAllItem, RPC_C2S_GetAllItem);
		BindAction(EPacketType.PT_MO_GetItem, RPC_C2S_GetItem);
		BindAction(EPacketType.PT_MO_StartRepair, RPC_C2S_StartRepair);
		BindAction(EPacketType.PT_MO_StopRepair, RPC_C2S_StopRepair);
		BindAction(EPacketType.PT_MO_SyncRepairTime, RPC_C2S_SyncRepairTime);
		BindAction(EPacketType.PT_MO_Destroy, RPC_C2S_Destroy);
		BindAction(EPacketType.PT_Common_ScenarioId, RPC_C2S_ScenarioId);
		BindAction(EPacketType.PT_InGame_SetController, RPC_C2S_SetController);
		BindAction(EPacketType.PT_InGame_LostController, RPC_C2S_LostController);
		BindAction(EPacketType.PT_Tower_Target, RPC_Tower_Target);
		BindAction(EPacketType.PT_Tower_Fire, RPC_C2S_Fire);
		BindAction(EPacketType.PT_Tower_AimPosition, RPC_C2S_AimPosition);
		BindAction(EPacketType.PT_Tower_LostEnemy, RPC_C2S_LostEnemy);
		BindAction(EPacketType.PT_InGame_InitData, RPC_C2S_InitData);
		SPTerrainEvent.InitCustomData(base.Id, _skEntity);
		CheckAuth();
	}

	public override void SkCreater()
	{
		if (!(_skEntity == null) && _skEntity._attribs != null)
		{
			SKAttribute.InitDoodadBaseAttrs(_skEntity._attribs, _doodad._protoTypeId, out _skEntity._baseAttribs);
		}
	}

	internal override void OnDeath(int casterId = 0)
	{
		base.OnDeath(casterId);
		ObjNetInterface objNetInterface = ObjNetInterface.Get(casterId);
		if (objNetInterface is CreationNetwork)
		{
			objNetInterface = (objNetInterface as CreationNetwork).Controller;
		}
		CheckMonsterMission((SkNetworkInterface)objNetInterface);
		_bDeath = true;
		_doodad.DropItem(objNetInterface);
		_doodad.DeleteData();
		_doodad.DeathDestroyNet();
	}

	public void DestroyByTimeOut()
	{
		Invoke("TimeOut", 300f);
	}

	public void AddToItemlist(int objID)
	{
		if (_doodad != null)
		{
			_doodad.AddToItemlist(objID);
		}
	}

	public void AddToItemlist(int objID, int index)
	{
		if (_doodad != null)
		{
			_doodad.AddToItemlist(objID, index);
		}
	}

	public static int Vector2Index(Vector3 pos)
	{
		int num = 0;
		int num2 = 0;
		int num3 = 0;
		num = ((!(pos.x > 0f)) ? (-10) : 10);
		num2 = (int)(pos.x + (float)num) / 20;
		num = ((!(pos.z > 0f)) ? (-10) : 10);
		num3 = (int)(pos.z + (float)num) / 20;
		return (num2 << 16) | num3;
	}

	public static bool HadCreate(int boxid, int type)
	{
		if (boxid != -1)
		{
			foreach (MapObjNetwork item in _mapObjNetworkMgr)
			{
				if (item._doodad._assetId == boxid && item._objType == type)
				{
					return true;
				}
			}
		}
		return false;
	}

	public static bool HadCreate(Vector3 pos)
	{
		foreach (MapObjNetwork item in _mapObjNetworkMgr)
		{
			if (item.transform.position == pos)
			{
				return true;
			}
		}
		return false;
	}

	public void DestroyMapObj(float waitTime = 2f)
	{
		StartCoroutine(DestroyMapObjCoroutine(waitTime));
	}

	protected override void OnPEInstantiate(uLink.NetworkMessageInfo info)
	{
		SetParent("DoodadNetMgr");
		_objType = info.networkView.initialData.Read<int>(new object[0]);
		base.authId = info.networkView.initialData.Read<int>(new object[0]);
		_teamId = info.networkView.initialData.Read<int>(new object[0]);
		_worldId = info.networkView.group.id;
		_defaultPlayerId = base.authId;
		_owner = Player.GetPlayer(base.authId);
		Player.PlayerDisconnected += OnPlayerDisconnect;
		if (_objType == 1)
		{
			_doodad = new DoodadDropBox();
		}
		else if (_objType == 2)
		{
			_doodad = new DoodadDeadDrop();
		}
		else if (_objType == 3)
		{
			_doodad = new DoodadBox();
		}
		else if (_objType == 4)
		{
			_doodad = new DoodadItem();
		}
		else if (_objType == 5)
		{
			_doodad = new DoodadRepair();
		}
		else if (_objType == 6)
		{
			_doodad = new DoodadPower();
		}
		else if (_objType == 7)
		{
			_doodad = new DoodadRandomBuilding();
		}
		else if (_objType == 8)
		{
			_doodad = new DoodadRandomBuilding_Repair();
		}
		else if (_objType == 9)
		{
			_doodad = new DoodadRandomBuilding_Power();
		}
		else
		{
			_doodad = new IDoodad();
		}
		_doodad.Create(this, info);
		_id = _doodad._entityId;
		_doodaProtoId = _doodad._protoTypeId;
		_mapObjNetworkMgr.Add(this);
		AddSkEntity();
		_doodad.InitAttr();
		base.gameObject.name = $"Mapobj assetId:{_doodad._assetId}, protoTypeId:{_doodaProtoId}, objType:{_objType}, entityId:{_id}";
	}

	public void TimeOut()
	{
		StartCoroutine(DestroyMapObjCoroutine());
	}

	internal IEnumerator DestroyMapObjCoroutine(float waitTime = 2f)
	{
		yield return new WaitForSeconds(waitTime);
		NetInterface.NetDestroy(this);
	}

	protected override void OnPEDestroy()
	{
		base.OnPEDestroy();
		_doodad.OnDestroy();
		_mapObjNetworkMgr.Remove(this);
		Player.PlayerDisconnected -= OnPlayerDisconnect;
	}

	protected IEnumerator DestroyCoroutine()
	{
		yield return new WaitForSeconds(2f);
		NetInterface.NetDestroy(this);
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
			if (player == null)
			{
				player = Player.GetNearestPlayer(aiAdNpcNetwork.transform.position);
			}
		}
		else if (caster is AiTowerNetwork)
		{
			AiTowerNetwork aiTowerNetwork = caster as AiTowerNetwork;
			if (null != aiTowerNetwork && aiTowerNetwork.OwnerId != -1 && ObjNetInterface.Get(aiTowerNetwork.OwnerId) != null && ObjNetInterface.Get(aiTowerNetwork.OwnerId) is Player)
			{
				player = ObjNetInterface.Get(aiTowerNetwork.OwnerId) as Player;
			}
			if (player == null)
			{
				player = Player.GetNearestPlayer(aiTowerNetwork.transform.position);
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
		player.ProcessMonsterDead(_doodad._protoTypeId);
	}

	private void SyncScenarioId(uLink.NetworkPlayer peer)
	{
		int customId = SPTerrainEvent.GetCustomId(base.Id);
		if (customId != -1)
		{
			RPCPeer(peer, EPacketType.PT_Common_ScenarioId, customId);
		}
	}

	public void RPC_C2S_InsertItemList(uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
		_doodad.Insert(stream, info);
	}

	public void RPC_C2S_RequestItemList(uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
		_doodad.RequestItemList(stream, info);
	}

	public void RPC_C2S_GetAllItem(uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
		_doodad.GetAllItems(stream, info);
	}

	public void RPC_C2S_GetItem(uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
		_doodad.GetItem(stream, info);
	}

	public void RPC_C2S_StartRepair(uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
		if (_doodad is DoodadRepair)
		{
			(_doodad as DoodadRepair).StartRepair(stream, info);
		}
	}

	public void RPC_C2S_StopRepair(uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
		if (_doodad is DoodadRepair)
		{
			(_doodad as DoodadRepair).StopRepair(stream, info);
		}
	}

	public void RPC_C2S_SyncRepairTime(uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
		if (_doodad is DoodadRepair)
		{
			(_doodad as DoodadRepair).SyncRepairTime();
		}
	}

	public void RPC_C2S_Destroy(uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
		StartCoroutine(DestroyCoroutine());
	}

	private void RPC_C2S_ScenarioId(uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
		SyncScenarioId(info.sender);
	}

	protected override void RPC_C2S_SetController(uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
		if (base.authId == -1 || !Player.ValidPlayer(base.authId))
		{
			base.authId = Player.GetPlayerId(info.sender);
			RPCProxy(EPacketType.PT_InGame_SetController, base.authId);
		}
		else
		{
			RPCPeer(info.sender, EPacketType.PT_InGame_SetController, base.authId);
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

	private void RPC_Tower_Target(uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
		float num = stream.Read<float>(new object[0]);
		Vector3 vector = stream.Read<Vector3>(new object[0]);
		URPCProxy(EPacketType.PT_Tower_Target, num, vector);
	}

	private void RPC_C2S_Fire(uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
		int num = stream.Read<int>(new object[0]);
		RPCOthers(EPacketType.PT_Tower_Fire, num);
	}

	private void RPC_C2S_AimPosition(uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
		int num2 = (aimTarget = stream.Read<int>(new object[0]));
		RPCOthers(EPacketType.PT_Tower_AimPosition, num2);
	}

	private void RPC_C2S_LostEnemy(uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
		aimTarget = -1;
		RPCOthers(EPacketType.PT_Tower_LostEnemy);
	}

	private void RPC_C2S_InitData(uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
		RPCPeer(info.sender, EPacketType.PT_InGame_InitData, aimTarget);
	}
}

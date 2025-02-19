using System.Collections;
using System.Collections.Generic;
using ItemAsset;
using Pathea;
using uLink;
using UnityEngine;
using WhiteCat;

public class CreationNetwork : SkNetworkInterface
{
	internal CreationSeat CreationCockpit = new CreationSeat();

	internal List<CreationSeat> Seats = new List<CreationSeat>();

	private int _ownerId = -1;

	public Vector3 _recordPos;

	private static List<int> lockObjs = new List<int>();

	internal Player Controller { get; set; }

	internal float HP { get; set; }

	internal float MaxHP { get; set; }

	internal float Fuel { get; set; }

	internal float MaxFuel { get; set; }

	internal bool HasEmptySeat => Seats.Exists((CreationSeat iter) => iter.CSPassanger == null);

	internal bool ValidCockpit => Seats.Exists((CreationSeat iter) => iter.CSType != EVCComponent.cpSideSeat && null == iter.CSPassanger);

	protected override void OnPEInstantiate(uLink.NetworkMessageInfo info)
	{
		SetParent("CreationNetMgr");
		_id = info.networkView.initialData.Read<int>(new object[0]);
		_ownerId = info.networkView.initialData.Read<int>(new object[0]);
		_teamId = info.networkView.initialData.Read<int>(new object[0]);
		_worldId = info.networkView.group;
		Add(this);
		Player player = Player.GetPlayer(_ownerId);
		base.authId = -1;
		if (null != player)
		{
			base.authId = player.Id;
		}
		base.gameObject.name = base.Id.ToString();
		InitializeData();
		AddSkEntity();
		SyncSave();
		base.PEInstantiateEvent += DropItemManager.OnInitialized;
		base.PEDestroyEvent += DropItemManager.OnDestroy;
		Player.PlayerDisconnected += OnPlayerDisconnect;
	}

	protected override void OnPEStart()
	{
		base.OnPEStart();
		BindAction(EPacketType.PT_CR_InitData, RPC_C2S_InitData);
		BindAction(EPacketType.PT_InGame_SetController, RPC_C2S_SetController);
		BindAction(EPacketType.PT_InGame_LostController, RPC_C2S_LostController);
		BindAction(EPacketType.PT_CR_Death, RPC_C2S_Death);
		BindAction(EPacketType.PT_CR_SkillCast, RPC_C2S_SkillCast);
		BindAction(EPacketType.PT_CR_Turn, RPC_C2S_Turn);
		BindAction(EPacketType.PT_CR_SyncEnergyDelta, RPC_C2S_SyncEnergyDelta);
		BindAction(EPacketType.PT_CR_SyncPos, RPC_C2S_SyncPos);
		CheckAuth();
	}

	protected override void OnPEDestroy()
	{
		base.OnPEDestroy();
		Player.PlayerDisconnected -= OnPlayerDisconnect;
	}

	protected override void OnPlayerDisconnect(Player player)
	{
		base.OnPlayerDisconnect(player);
		if (base.authId != -1 && base.authId == player.Id && !_bDeath)
		{
			SyncSave();
			SaveCreation();
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
		CreationNetData creationNetData = new CreationNetData();
		creationNetData.ExportData(this);
		AsyncSqlite.AddRecord(creationNetData);
	}

	public void SaveCreation()
	{
		CreationOriginData creationData = SteamWorks.GetCreationData(base.Id);
		if (creationData != null)
		{
			UGCData uGCData = new UGCData();
			uGCData.UpdateData(this);
			AsyncSqlite.AddRecord(uGCData);
		}
	}

	private void OnCreationExplode()
	{
		foreach (CreationSeat seat in Seats)
		{
			if (seat != null && seat.CSPassanger != null)
			{
				if (seat.CSPassanger is Player)
				{
					Player player = (Player)seat.CSPassanger;
					player.GetOffVehicle(base.transform.position);
					player.KillPlayer();
				}
				else
				{
					AiAdNpcNetwork aiAdNpcNetwork = (AiAdNpcNetwork)seat.CSPassanger;
					aiAdNpcNetwork.GetOffVehicle(base.transform.position);
				}
			}
		}
		if (null != Controller)
		{
			Controller.GetOffVehicle(base.transform.position);
		}
	}

	internal void PassangerGetOff(Vector3 outPos)
	{
		foreach (CreationSeat seat in Seats)
		{
			if (seat != null && seat.CSPassanger != null)
			{
				if (seat.CSPassanger is Player)
				{
					Player player = (Player)seat.CSPassanger;
					player.GetOffVehicle(outPos);
				}
				else
				{
					AiAdNpcNetwork aiAdNpcNetwork = (AiAdNpcNetwork)seat.CSPassanger;
					aiAdNpcNetwork.GetOffVehicle(outPos);
				}
			}
		}
	}

	internal void SetDriver(NetInterface driver)
	{
		if (null != driver)
		{
			Controller = (Player)driver;
			base.authId = Controller.Id;
			RPCOthers(EPacketType.PT_InGame_SetController, base.authId, base.TeamId);
		}
	}

	internal void ResetDriver(Vector3 pos)
	{
		PassangerGetOff(pos);
		Controller = null;
		base.authId = -1;
		RPCOthers(EPacketType.PT_InGame_LostController);
	}

	private IEnumerator AutoSave()
	{
		while (true)
		{
			yield return new WaitForSeconds(60f);
			if (_bDeath)
			{
				break;
			}
			SyncSave();
			SaveCreation();
		}
	}

	internal override void OnDamage(int casterId, float damage = 0f)
	{
		base.OnDamage(casterId, damage);
		HP = _skEntity.GetAttribute(AttribType.Hp);
		ItemObject itemByID = ItemManager.GetItemByID(base.Id);
		if (itemByID != null)
		{
			LifeLimit cmpt = itemByID.GetCmpt<LifeLimit>();
			if (cmpt != null && cmpt.SetValue(HP))
			{
				ChannelNetwork.SyncItem(base.WorldId, itemByID);
			}
		}
	}

	private void RPC_C2S_InitData(uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
		ItemObject itemByID = ItemManager.GetItemByID(base.Id);
		if (itemByID != null)
		{
			int num = ((!(null != Controller)) ? (-1) : Controller.Id);
			RPCPeer(info.sender, EPacketType.PT_CR_InitData, itemByID, base.transform.position, base.transform.rotation, HP, MaxHP, Fuel, MaxFuel, num, _ownerId);
			StartCoroutine(AutoSave());
		}
	}

	protected override void RPC_C2S_SetController(uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
		if (base.authId == -1)
		{
			base.authId = Player.GetPlayerId(info.sender);
			RPCProxy(EPacketType.PT_InGame_SetController, base.authId, base.TeamId);
		}
		else
		{
			RPCPeer(info.sender, EPacketType.PT_InGame_SetController, base.authId, base.TeamId);
		}
	}

	protected override void RPC_C2S_LostController(uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
		Player player = Player.GetPlayer(info.sender);
		OnPlayerDisconnect(player);
	}

	private void RPC_C2S_SyncPos(uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
		if (base.authId != Player.GetPlayerId(info.sender) || !stream.TryRead<byte[]>(out var value))
		{
			return;
		}
		UnionValue unionValue = default(UnionValue);
		int offset = 0;
		unionValue.ReadByteFrom(value, ref offset);
		byte byteValue = unionValue.byteValue;
		byte b = 1;
		if ((byteValue & b) != 0)
		{
			base.transform.position = Kit.ReadVector3FromBuffer(value, ref offset);
		}
		b <<= 1;
		if ((byteValue & b) != 0)
		{
			base.transform.rotation = Kit.ReadQuaternionFromBuffer(value, ref offset);
		}
		if (null != Controller && Controller._OnCar)
		{
			Controller.SetPosition(base.transform.position);
			Controller.SyncPosArea(base.transform.position);
		}
		foreach (CreationSeat seat in Seats)
		{
			if (!(null == seat.CSPassanger) && seat.CSPassanger is Player)
			{
				Player player = (Player)seat.CSPassanger;
				player.SetPosition(base.transform.position);
				player.SyncPosArea(base.transform.position);
			}
		}
		URPCOthers(EPacketType.PT_CR_SyncPos, value);
	}

	private void RPC_C2S_SyncEnergyDelta(uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
		if (!stream.TryRead<float>(out var value))
		{
			return;
		}
		ItemObject itemByID = ItemManager.GetItemByID(base.Id);
		if (itemByID != null)
		{
			Energy cmpt = itemByID.GetCmpt<Energy>();
			if (cmpt != null)
			{
				cmpt.ChangeValue(value);
				URPCOthers(EPacketType.PT_CR_SyncEnergyDelta, cmpt.floatValue.current);
			}
		}
	}

	private void RPC_C2S_Death(uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
	}

	protected virtual void RPC_C2S_SkillCast(uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
		if (!_bDeath)
		{
			byte[] remainingBytes = stream.GetRemainingBytes();
			RPCOthers(EPacketType.PT_CR_SkillCast, remainingBytes);
		}
	}

	private void RPC_C2S_Turn(uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
		base.transform.Rotate(Vector3.up, 90f, Space.World);
		RPCOthers(EPacketType.PT_CR_Turn, base.transform.rotation.eulerAngles.y);
		SyncSave();
	}

	internal void InitializeData()
	{
		CreationOriginData creationData = SteamWorks.GetCreationData(base.Id);
		if (creationData == null)
		{
			return;
		}
		HP = creationData.HP;
		MaxHP = creationData.MaxHP;
		Fuel = creationData.Fuel;
		MaxFuel = creationData.MaxFuel;
		RegisteredISO registerISOByHash = SteamWorks.GetRegisterISOByHash(creationData.HashCode);
		if (registerISOByHash == null)
		{
			return;
		}
		foreach (EVCComponent component in registerISOByHash.Components)
		{
			if (component == EVCComponent.cpSideSeat || component == EVCComponent.cpVehicleCockpit || component == EVCComponent.cpVtolCockpit || component == EVCComponent.cpShipCockpit)
			{
				if (component != EVCComponent.cpSideSeat)
				{
					CreationCockpit.CSPassanger = null;
					CreationCockpit.CSType = component;
				}
				else
				{
					CreationSeat creationSeat = new CreationSeat();
					creationSeat.CSType = component;
					Seats.Add(creationSeat);
				}
			}
		}
		base.gameObject.name = registerISOByHash.IsoName;
	}

	internal EVCComponent GetOn(NetInterface passanger, ref int index)
	{
		index = -1;
		if (null == passanger)
		{
			return EVCComponent.cpAbstract;
		}
		if (CreationCockpit.CSPassanger == null)
		{
			index = -1;
			CreationCockpit.CSPassanger = passanger;
			return CreationCockpit.CSType;
		}
		for (int i = 0; i < Seats.Count; i++)
		{
			if (null == Seats[i].CSPassanger)
			{
				index = i;
				Seats[index].CSPassanger = passanger;
				return Seats[i].CSType;
			}
		}
		return EVCComponent.cpAbstract;
	}

	internal EVCComponent GetOnSide(NetInterface passanger, ref int index)
	{
		index = -1;
		if (null == passanger)
		{
			return EVCComponent.cpAbstract;
		}
		for (int i = 0; i < Seats.Count; i++)
		{
			if (null == Seats[i].CSPassanger && Seats[i].CSType == EVCComponent.cpSideSeat)
			{
				index = i;
				Seats[index].CSPassanger = passanger;
				return Seats[i].CSType;
			}
		}
		return EVCComponent.cpAbstract;
	}

	internal EVCComponent GetOff(NetInterface passanger)
	{
		if (null == passanger)
		{
			return EVCComponent.cpAbstract;
		}
		if (passanger.Equals(CreationCockpit.CSPassanger))
		{
			CreationCockpit.CSPassanger = null;
			return CreationCockpit.CSType;
		}
		for (int i = 0; i < Seats.Count; i++)
		{
			if (passanger.Equals(Seats[i].CSPassanger))
			{
				Seats[i].CSPassanger = null;
				return Seats[i].CSType;
			}
		}
		return EVCComponent.cpAbstract;
	}

	public bool IsFullHP()
	{
		return HP == MaxHP;
	}

	public bool IsFullFuel()
	{
		return Fuel == MaxFuel;
	}

	internal override void OnDeath(int casterId)
	{
		base.OnDeath(casterId);
		HP = 0f;
		_bDeath = true;
		base.enabled = false;
		CreationOriginData creationData = SteamWorks.GetCreationData(base.Id);
		if (creationData != null)
		{
			ItemManager.RemoveItem(base.Id);
		}
		RPCOthers(EPacketType.PT_CR_Death);
		OnCreationExplode();
		base.authId = -1;
		StartCoroutine(DestroyAiObjectCoroutine());
		Delete();
	}

	protected IEnumerator DestroyAiObjectCoroutine()
	{
		yield return new WaitForSeconds(30f);
		NetInterface.NetDestroy(this);
	}

	protected virtual int DefenceType()
	{
		return 5;
	}

	public static bool IsLock(Player player, int objId)
	{
		PlayerMission curPlayerMission = MissionManager.Manager.GetCurPlayerMission(player.Id);
		if (curPlayerMission != null && (curPlayerMission.HasMission(822) || (curPlayerMission.HadCompleteMission(822, player) && !curPlayerMission.HadCompleteMission(826, player))) && lockObjs.Contains(objId))
		{
			return true;
		}
		curPlayerMission = MissionManager.Manager.GetCurTeamMission(player.Id);
		if (curPlayerMission != null && (curPlayerMission.HasMission(822) || (curPlayerMission.HadCompleteMission(822, player) && !curPlayerMission.HadCompleteMission(826, player))) && lockObjs.Contains(objId))
		{
			return true;
		}
		return false;
	}

	public static void AddLock(Player player, int objId)
	{
		PlayerMission curPlayerMission = MissionManager.Manager.GetCurPlayerMission(player.Id);
		if (curPlayerMission != null && (curPlayerMission.HasMission(822) || (curPlayerMission.HadCompleteMission(822, player) && !curPlayerMission.HadCompleteMission(826, player))) && lockObjs.Count <= 4)
		{
			lockObjs.Add(objId);
		}
		curPlayerMission = MissionManager.Manager.GetCurTeamMission(player.Id);
		if (curPlayerMission != null && (curPlayerMission.HasMission(822) || (curPlayerMission.HadCompleteMission(822, player) && !curPlayerMission.HadCompleteMission(826, player))) && lockObjs.Count <= 4)
		{
			lockObjs.Add(objId);
		}
	}

	public static void MoveAircraft()
	{
		int num = 0;
		foreach (int lockObj in lockObjs)
		{
			CreationNetwork creationNetwork = ObjNetInterface.Get(lockObj) as CreationNetwork;
			if (creationNetwork != null)
			{
				if (creationNetwork._recordPos == creationNetwork.Pos)
				{
					break;
				}
				creationNetwork._recordPos = creationNetwork.Pos;
				creationNetwork.RPCOthers(EPacketType.PT_InGame_MissionMoveAircraft, true, num);
				num++;
			}
		}
	}

	public static void ReturnAircraft()
	{
		int num = 0;
		foreach (int lockObj in lockObjs)
		{
			CreationNetwork creationNetwork = ObjNetInterface.Get(lockObj) as CreationNetwork;
			if (creationNetwork != null && !(creationNetwork._recordPos == Vector3.zero))
			{
				creationNetwork.RPCOthers(EPacketType.PT_InGame_MissionMoveAircraft, false, num, creationNetwork._recordPos);
				num++;
			}
		}
	}
}

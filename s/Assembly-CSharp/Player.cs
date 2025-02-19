using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using CustomData;
using ItemAsset;
using Mono.Data.SqliteClient;
using Pathea;
using PETools;
using Railway;
using uLink;
using UnityEngine;
using Weather;

public class Player : SkNetworkInterface
{
	public const int MaxServant = 2;

	public const int WRAP_Type_TOWN = 0;

	public const int WRAP_Type_FLAG = 1;

	public const int WRAP_Type_Base = 2;

	public const float repairHp = 20000f;

	public const float chargeFuel = 36000f;

	public const double VarPerOp = 30.0;

	private bool isGm;

	private float _lobbyExpIncrement;

	protected static Dictionary<uLink.NetworkPlayer, int> PlayerPeers = new Dictionary<uLink.NetworkPlayer, int>();

	protected static Dictionary<int, uLink.NetworkPlayer> PlayerIds = new Dictionary<int, uLink.NetworkPlayer>();

	protected static Dictionary<int, Player> PlayerDic = new Dictionary<int, Player>();

	protected static List<Player> Players = new List<Player>();

	protected int _gameMoney;

	protected BattleInfo _battleInfo;

	protected PlayerSynAttribute mPlayerSynAttribute;

	protected CharacterArmor _charcterArmor;

	protected List<AiAdNpcNetwork> _servantList;

	protected List<AiAdNpcNetwork> _forceServantList;

	protected Dictionary<int, int> _shortcutKeys;

	protected List<int> _learntSkillsID;

	protected List<int> _metalScanList;

	protected List<int> voxelAreaSent;

	protected List<int> blockAreaSent;

	protected SkillTreeUnitMgr _learntSkills;

	protected AiMonsterNetwork _mount;

	public List<Replicator.KnownFormula> _forumlaList;

	protected List<ItemObject> _exceptItemList;

	protected Dictionary<GroupNetInterface, DamageData> _harmList;

	protected Dictionary<int, List<ItemObject>> _repurchaseItems;

	protected float lastHeartBeatTime;

	protected bool _hasRecord;

	protected bool _onDamage;

	protected int _curSceneId;

	protected EquipmentCmpt m_EquipModule;

	protected PlayerPackageCmpt m_PackageCmpt;

	public List<FollowData> followdata = new List<FollowData>();

	private Vector3 enterPos;

	public bool IsGM => isGm;

	public static float lastSortTime { get; protected set; }

	public List<int> MetalScanList
	{
		get
		{
			if (ServerConfig.IsStory)
			{
				return PublicData.Self._storyMetalScan;
			}
			return _metalScanList;
		}
		set
		{
			if (ServerConfig.IsStory)
			{
				PublicData.Self._storyMetalScan = value;
			}
			else
			{
				_metalScanList = value;
			}
		}
	}

	public List<Replicator.KnownFormula> ForumlaList
	{
		get
		{
			if (ServerConfig.IsStory)
			{
				return PublicData.Self._storyForumlaList;
			}
			return _forumlaList;
		}
		set
		{
			if (ServerConfig.IsStory)
			{
				PublicData.Self._storyForumlaList = value;
			}
			else
			{
				_forumlaList = value;
			}
		}
	}

	public int RoomIndex { get; set; }

	public int GameMoney
	{
		get
		{
			if (ServerConfig.MoneyType == EMoneyType.Meat)
			{
				return Package.GetItemCount(229);
			}
			return _gameMoney;
		}
	}

	public EVCComponent _SeatType { get; set; }

	public string account { get; protected set; }

	public ulong steamId { get; protected set; }

	public string roleName { get; protected set; }

	public byte sex { get; protected set; }

	public BattleInfo Battle => _battleInfo;

	public int originTeamId { get; private set; }

	public bool isOriginTeam => _teamId == originTeamId && GroupNetwork.IsEmpty(originTeamId);

	public bool isOriginLeader => _teamId == originTeamId && !GroupNetwork.IsEmpty(originTeamId);

	public EquipmentCmpt EquipModule => m_EquipModule;

	public PlayerPackageCmpt Package => m_PackageCmpt;

	private event Action<int> KillEvent;

	private event Action<int, bool> DeathEvent;

	private event Action<int> OccupyEvent;

	private event Action<int, int> MoneyEvent;

	private event Action<int, float> PointEvent;

	public static event Action<Player> PlayerDisconnected;

	public static event Action<Player> OnHeartBeatTimeoutEvent;

	public event Action<int> OnUseItemEventHandler;

	public event Action<int> OnPutOutItemEventHandler;

	private int CountDropAmount()
	{
		int num = ServerConfig.DropDeadPercent;
		if (num > 100 || num < 0)
		{
			num = 100;
		}
		if (num == 0)
		{
			return 0;
		}
		int num2 = EquipNotBindCount() + Package.ItemNotBindCount();
		return num2 * num / 100;
	}

	public List<ItemObject> GetDropItems(int dropCount)
	{
		List<ItemObject> list = new List<ItemObject>();
		if (dropCount == 0)
		{
			return list;
		}
		List<ItemObject> list2 = new List<ItemObject>();
		for (int i = 0; i < dropCount; i++)
		{
			int num = EquipNotBindCount();
			int num2 = Package.ItemNotBindCount();
			int max = num + num2;
			int num3 = UnityEngine.Random.Range(1, max);
			if (num3 > num2)
			{
				ItemObject item = RandGetEquipment();
				list.Add(item);
				continue;
			}
			ItemObject itemObject = RandGetValidItemFromPackage(_exceptItemList);
			if (itemObject != null)
			{
				ItemObject itemObject2 = itemObject;
				itemObject2 = RandGetStackItem(itemObject);
				if (itemObject2 != null)
				{
					list2.Add(itemObject2);
					list.Add(itemObject2);
				}
				else
				{
					list.Add(itemObject);
				}
				_exceptItemList.Add(itemObject);
			}
		}
		SyncEquipedItems(EquipModule.EquipItems);
		if (list2.Count > 0)
		{
			SyncItemList(list2.ToArray());
		}
		SyncPackageIndex();
		return list;
	}

	private ItemObject RandGetEquipment()
	{
		ItemObject[] notBindEquips = EquipModule.GetNotBindEquips();
		if (notBindEquips == null || notBindEquips.Length <= 0)
		{
			return null;
		}
		int num = UnityEngine.Random.Range(1, notBindEquips.Length);
		ItemObject itemObject = notBindEquips[num - 1];
		RemoveEquipment(itemObject);
		return itemObject;
	}

	private ItemObject RandGetStackItem(ItemObject itemObj)
	{
		if (itemObj == null)
		{
			return null;
		}
		int stackCount = itemObj.stackCount;
		int num = UnityEngine.Random.Range(1, stackCount);
		if (num >= stackCount)
		{
			Package.RemoveItem(itemObj);
			SyncPackageIndex();
			return null;
		}
		ItemObject itemObject = CreateItem(itemObj.protoId, num);
		if (itemObject != null)
		{
			ItemCountDown(itemObj, num);
		}
		return itemObject;
	}

	public float GetHP()
	{
		return _skEntity.GetAttribute(AttribType.Hp);
	}

	public float GetComfort()
	{
		return _skEntity.GetAttribute(AttribType.Comfort);
	}

	public float GetHunger()
	{
		return _skEntity.GetAttribute(AttribType.Hunger);
	}

	internal override void OnDeath(int casterId = 0)
	{
		base.OnDeath(casterId);
		_bDeath = true;
		_harmList.Clear();
		RPCOthers(EPacketType.PT_InGame_PlayerDeath, casterId);
		SceneDropItem.CreateDropItems(base.WorldId, GetDropItems(CountDropAmount()), base.transform.position, Vector3.zero, base.transform.rotation);
		Invoke("PlayerReset", 5f);
	}

	private void PlayerReset()
	{
		RPCOwner(EPacketType.PT_InGame_PlayerReset);
	}

	private void PlayerRevive()
	{
		_skEntity.SetAttribute(AttribType.Hp, _skEntity.GetAttribute(AttribType.HpMax));
		_skEntity.SetAttribute(AttribType.Oxygen, _skEntity.GetAttribute(AttribType.OxygenMax));
		_skEntity.SetAttribute(AttribType.Stamina, _skEntity.GetAttribute(AttribType.StaminaMax));
		_bDeath = false;
		RPCOthers(EPacketType.PT_InGame_PlayerRevive, _skEntity.GetAttribute(AttribType.Hp), _skEntity.GetAttribute(AttribType.Oxygen), _skEntity.GetAttribute(AttribType.Stamina), base.transform.position, base.transform.rotation);
		SyncEquipedItems(EquipModule.EquipItems);
		SyncPackageIndex(1);
	}

	public override void SetTeamId(int teamId)
	{
		base.SetTeamId(teamId);
		ForceSetting.AddPlayer(base.Id, base.TeamId, EPlayerType.Human, base.name);
		if (!ServerConfig.IsSurvive)
		{
			return;
		}
		if (_servantList != null)
		{
			for (int i = 0; i < _servantList.Count; i++)
			{
				_servantList[i].SetTeamId(base.TeamId);
				_servantList[i].SyncTeamId();
			}
		}
		if (_forceServantList != null)
		{
			for (int j = 0; j < _forceServantList.Count; j++)
			{
				_forceServantList[j].SetTeamId(base.TeamId);
				_forceServantList[j].SyncTeamId();
			}
		}
	}

	public void ResetTeamId()
	{
		SetTeamId(originTeamId);
		GroupNetwork.AddToTeam(base.Id, base.OwnerView.owner, originTeamId);
	}

	public void SyncGetDeadObjAllItems(int netObjId)
	{
		RPCOthers(EPacketType.PT_InGame_GetAllDeadObjItem, netObjId);
	}

	public void SyncGetDeadObjItem(int netObjId, int index, int itemId)
	{
		RPCOthers(EPacketType.PT_InGame_GetDeadObjItem, netObjId, index, itemId);
	}

	private void RPC_C2S_PlayerReset(uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
		Vector3 pos = stream.Read<Vector3>(new object[0]);
		CommonHelper.AdjustPos(ref pos);
		SetPosition(pos);
		SyncPosArea(pos);
		base.transform.rotation = Quaternion.identity;
		PlayerRevive();
		RPCOthers(EPacketType.PT_InGame_FastTransfer, pos);
	}

	private void RPC_C2S_PlayerRevive(uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
		Vector3 pos = stream.Read<Vector3>(new object[0]);
		if (GetItemNum(937) <= 0)
		{
			CommonHelper.AdjustPos(ref pos);
			SetPosition(pos);
			SyncPosArea(pos);
			base.transform.rotation = Quaternion.identity;
			PlayerRevive();
			RPCOthers(EPacketType.PT_InGame_FastTransfer, pos);
		}
		else
		{
			List<ItemObject> effItems = new List<ItemObject>(10);
			Package.RemoveItem(937, 1, ref effItems);
			SyncItemList(effItems);
			SyncPackageIndex();
			ProcessItemMission(937);
			PlayerRevive();
		}
	}

	private void RPC_C2S_SendMsg(uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
		EMsgType eMsgType = stream.Read<EMsgType>(new object[0]);
		string text = stream.Read<string>(new object[0]);
		string text2 = text.Replace("[lang:cn]", string.Empty);
		text2 = text2.Replace("[lang:other]", string.Empty);
		if (text2 == "/peroot")
		{
			isGm = true;
		}
		else
		{
			if (GMCommand.Self.ParsingCMD(this, text2))
			{
				return;
			}
			switch (eMsgType)
			{
			case EMsgType.ToTeam:
				if (ServerConfig.IsVS)
				{
					SyncGroupData(EPacketType.PT_InGame_SendMsg, eMsgType, text);
				}
				break;
			case EMsgType.ToAll:
				RPCOthers(EPacketType.PT_InGame_SendMsg, eMsgType, text);
				break;
			case EMsgType.ToOne:
				break;
			}
		}
	}

	private void RPC_C2S_ApplyDamage(uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
		float num = stream.Read<float>(new object[0]);
	}

	private void RPC_C2S_ApplyComfort(uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
		stream.TryRead<float>(out var _);
	}

	private void RPC_C2S_ApplySatiation(uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
		stream.TryRead<float>(out var _);
	}

	private void RPC_C2S_CreateNewTeam(uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
	}

	private void RPC_C2S_JoinTeam(uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
		if (!ServerConfig.IsSurvive)
		{
			return;
		}
		int num = stream.Read<int>(new object[0]);
		if (!isOriginTeam || base.TeamId == num)
		{
			return;
		}
		if (!GroupNetwork.CanJoin(num))
		{
			SyncErrorMsg("Cannot join team");
			return;
		}
		int leaderID = GroupNetwork.GetLeaderID(num);
		if (leaderID != -1)
		{
			Player player = GetPlayer(leaderID);
			if (null != player)
			{
				player.RPCOwner(EPacketType.PT_InGame_JoinTeam, base.Id, roleName);
			}
		}
	}

	private void RPC_C2S_ApproveJoin(uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
		if (!ServerConfig.IsSurvive)
		{
			return;
		}
		int num = stream.Read<int>(new object[0]);
		if (base.Id == num || !GroupNetwork.CanJoin(base.TeamId) || !GroupNetwork.IsLeaderID(base.TeamId, base.Id) || GroupNetwork.IsMember(base.TeamId, num))
		{
			return;
		}
		uLink.NetworkPlayer playerPeer = GetPlayerPeer(num);
		if (playerPeer.isUnassigned)
		{
			return;
		}
		Player player = GetPlayer(num);
		if (!(null == player))
		{
			int num2 = GroupNetwork.AddToTeam(num, playerPeer, base.TeamId);
			if (num2 != -1)
			{
				int teamId = player.TeamId;
				player.SetTeamId(base.TeamId);
				GroupNetwork.RemoveFromTeam(teamId, num);
				player.SyncTeamInfo();
				player.RPCOthers(EPacketType.PT_InGame_ApproveJoin, base.TeamId);
			}
		}
	}

	private void RPC_C2S_DenyJoin(uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
		if (!ServerConfig.IsSurvive)
		{
			return;
		}
		int num = stream.Read<int>(new object[0]);
		if (base.Id != num && GroupNetwork.IsLeaderID(base.TeamId, base.Id))
		{
			Player player = GetPlayer(num);
			if (null != player)
			{
				player.RPCOwner(EPacketType.PT_InGame_DenyJoin, base.Id);
			}
		}
	}

	private void RPC_C2S_Invitation(uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
		if (!ServerConfig.IsSurvive)
		{
			return;
		}
		int num = stream.Read<int>(new object[0]);
		if (base.Id != num && GroupNetwork.CanJoin(base.TeamId) && GroupNetwork.IsLeaderID(base.TeamId, base.Id))
		{
			Player player = GetPlayer(num);
			if (!(null == player) && player.isOriginTeam)
			{
				player.RPCOwner(EPacketType.PT_InGame_Invitation, base.Id);
			}
		}
	}

	private void RPC_C2S_KickSB(uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
		if (!ServerConfig.IsSurvive)
		{
			return;
		}
		int num = stream.Read<int>(new object[0]);
		if (base.Id != num && GroupNetwork.IsLeaderID(base.TeamId, base.Id) && GroupNetwork.IsMember(base.TeamId, num))
		{
			Player player = GetPlayer(num);
			if (!(null == player))
			{
				player.ResetTeamId();
				GroupNetwork.RemoveFromTeam(base.TeamId, num);
				player.SyncTeamInfo();
				RPCOthers(EPacketType.PT_InGame_KickSB, num);
			}
		}
	}

	private void RPC_C2S_LeaderDeliver(uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
		if (ServerConfig.IsSurvive)
		{
			int num = stream.Read<int>(new object[0]);
			if (base.Id != num && GroupNetwork.IsMember(base.TeamId, num) && GroupNetwork.IsLeaderID(base.TeamId, base.Id))
			{
				GroupNetwork.LeaderDeliver(base.TeamId, num);
				RPCOthers(EPacketType.PT_InGame_LeaderDeliver, num);
			}
		}
	}

	private void RPC_C2S_QuitTeam(uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
		if (ServerConfig.IsSurvive && GroupNetwork.IsTeamExisted(base.TeamId) && !GroupNetwork.IsLeaderID(base.TeamId, base.Id))
		{
			int teamId = base.TeamId;
			ResetTeamId();
			GroupNetwork.RemoveFromTeam(teamId, base.Id);
			SyncTeamInfo();
			RPCOthers(EPacketType.PT_InGame_QuitTeam);
		}
	}

	private void RPC_C2S_DissolveTeam(uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
		if (ServerConfig.IsSurvive && GroupNetwork.IsTeamExisted(base.TeamId) && GroupNetwork.IsLeaderID(base.TeamId, base.Id))
		{
			GroupNetwork.DissolveTeam(base.TeamId);
			RPCOthers(EPacketType.PT_InGame_DissolveTeam);
		}
	}

	private void RPC_C2S_AcceptJoinTeam(uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
		if (!ServerConfig.IsSurvive)
		{
			return;
		}
		int id = stream.Read<int>(new object[0]);
		int num = stream.Read<int>(new object[0]);
		if (GroupNetwork.IsLeaderID(num, id) && isOriginTeam)
		{
			num = GroupNetwork.AddToTeam(base.Id, base.OwnerView.owner, num);
			if (num != -1)
			{
				int teamId = base.TeamId;
				SetTeamId(num);
				GroupNetwork.RemoveFromTeam(teamId, base.Id);
				SyncTeamInfo();
				RPCOthers(EPacketType.PT_InGame_AcceptJoinTeam, base.TeamId);
			}
		}
	}

	private void RPC_C2S_ServantRevive(uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
		int id = stream.Read<int>(new object[0]);
		Vector3 position = stream.Read<Vector3>(new object[0]);
		if (GetItemNum(937) <= 0)
		{
			return;
		}
		AiAdNpcNetwork aiAdNpcNetwork = ObjNetInterface.Get<AiAdNpcNetwork>(id);
		if (null == aiAdNpcNetwork || !aiAdNpcNetwork.IsDead || !aiAdNpcNetwork.Recruited || null == aiAdNpcNetwork.lordPlayer || base.Id != aiAdNpcNetwork.lordPlayer.Id)
		{
			return;
		}
		ItemCmpt itemCmpt = null;
		int tabIndex = -1;
		if (aiAdNpcNetwork.ItemModule.HasItem(937))
		{
			itemCmpt = aiAdNpcNetwork.ItemModule;
			tabIndex = 0;
		}
		else if (aiAdNpcNetwork.ServantItemModule.HasItem(937))
		{
			itemCmpt = aiAdNpcNetwork.ServantItemModule;
			tabIndex = 1;
		}
		if (itemCmpt != null)
		{
			List<ItemObject> effItems = new List<ItemObject>();
			itemCmpt.DelItem(937, 1, ref effItems);
			SyncItemList(effItems);
			aiAdNpcNetwork.SyncPackageIndex(tabIndex);
		}
		else
		{
			if (GetItemNum(937) <= 0)
			{
				return;
			}
			List<ItemObject> effItems2 = new List<ItemObject>();
			Package.RemoveItem(937, 1, ref effItems2);
			SyncItemList(effItems2);
			SyncPackageIndex();
		}
		aiAdNpcNetwork.transform.position = position;
		aiAdNpcNetwork.NpcRevive();
	}

	private void RPC_C2S_ServantAutoRevive(uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
		int id = stream.Read<int>(new object[0]);
		Vector3 position = stream.Read<Vector3>(new object[0]);
		AiAdNpcNetwork aiAdNpcNetwork = ObjNetInterface.Get<AiAdNpcNetwork>(id);
		if (!(null == aiAdNpcNetwork) && aiAdNpcNetwork.IsDead && aiAdNpcNetwork.Recruited && !(null == aiAdNpcNetwork.lordPlayer) && base.Id == aiAdNpcNetwork.lordPlayer.Id)
		{
			ItemCmpt itemCmpt = null;
			int tabIndex = -1;
			if (aiAdNpcNetwork.ItemModule.HasItem(937))
			{
				itemCmpt = aiAdNpcNetwork.ItemModule;
				tabIndex = 0;
			}
			else if (aiAdNpcNetwork.ServantItemModule.HasItem(937))
			{
				itemCmpt = aiAdNpcNetwork.ServantItemModule;
				tabIndex = 1;
			}
			if (itemCmpt != null)
			{
				List<ItemObject> effItems = new List<ItemObject>();
				itemCmpt.DelItem(937, 1, ref effItems);
				SyncItemList(effItems);
				aiAdNpcNetwork.SyncPackageIndex(tabIndex);
				aiAdNpcNetwork.transform.position = position;
				aiAdNpcNetwork.NpcRevive();
			}
		}
	}

	private void RPC_C2S_CurSceneId(uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
		_curSceneId = stream.Read<int>(new object[0]);
		RPCOthers(EPacketType.PT_InGame_CurSceneId, _curSceneId);
	}

	private void RPC_C2S_NpcRecruit(uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
		int id = stream.Read<int>(new object[0]);
		bool flag = stream.Read<bool>(new object[0]);
		AiAdNpcNetwork aiAdNpcNetwork = ObjNetInterface.Get<AiAdNpcNetwork>(id);
		if (aiAdNpcNetwork.Recruited && null == aiAdNpcNetwork.lordPlayer)
		{
			aiAdNpcNetwork.DismissByPlayer();
		}
		if (!(null == aiAdNpcNetwork) && !aiAdNpcNetwork.IsDead && !aiAdNpcNetwork.Recruited && !(null != aiAdNpcNetwork.lordPlayer))
		{
			Player player = this;
			if (flag)
			{
				player = GetNearestPlayer(base.TeamId, aiAdNpcNetwork.transform.position);
			}
			aiAdNpcNetwork.RecruitByPlayer(player);
		}
	}

	private void RPC_C2S_SwitchScene(uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
		if (ServerConfig.IsCustom)
		{
			NetInterface.RemovePlayerFromGroup(info.sender, base.WorldId);
			NetInterface.RemovePlayerFromGroup(info.sender, 101);
		}
	}

	private void RPC_C2S_LoginScene(uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
		string yirdName = stream.Read<string>(new object[0]);
		if (ServerConfig.IsCustom)
		{
			int gameWorldId = CustomGameData.Mgr.GetGameWorldId(yirdName);
			_worldId = GameWorld.LoginWorld(gameWorldId);
			NetInterface.AddPlayerToGroup(info.sender, base.WorldId);
		}
	}

	private void RPC_C2S_CheckPosition(uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
		byte[] buffer = stream.Read<byte[]>(new object[0]);
		BufferHelper.Import(buffer, delegate(BinaryReader r)
		{
			int num = r.ReadInt32();
			BufferHelper.ReadObject(r, out var obj);
			BufferHelper.ReadRange(r, out var obj2);
			bool flag = false;
			ISceneObject customObj = SPTerrainEvent.GetCustomObj(base.WorldId, obj, this);
			flag = ((!object.Equals(customObj, null)) ? obj2.Contains(customObj.Pos) : (obj2.type == RANGE.RANGETYPE.Anywhere));
			RPCOwner(EPacketType.PT_Custom_CheckResult, num, flag);
		});
	}

	private void RPC_C2S_CheckRotation(uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
		byte[] buffer = stream.Read<byte[]>(new object[0]);
		BufferHelper.Import(buffer, delegate(BinaryReader r)
		{
			int num = r.ReadInt32();
			BufferHelper.ReadObject(r, out var obj);
			BufferHelper.ReadDirRange(r, out var obj2);
			EAxis eAxis = (EAxis)r.ReadByte();
			bool flag = false;
			ISceneObject customObj = SPTerrainEvent.GetCustomObj(base.WorldId, obj, this);
			if (object.Equals(customObj, null))
			{
				flag = obj2.type == DIRRANGE.DIRRANGETYPE.Anydirection;
			}
			else if (customObj is SkNetworkInterface)
			{
				Transform transform = ((SkNetworkInterface)customObj).transform;
				switch (eAxis)
				{
				case EAxis.Left:
					flag = obj2.Contains(-transform.right);
					break;
				case EAxis.Right:
					flag = obj2.Contains(transform.right);
					break;
				case EAxis.Down:
					flag = obj2.Contains(-transform.up);
					break;
				case EAxis.Up:
					flag = obj2.Contains(transform.up);
					break;
				case EAxis.Backward:
					flag = obj2.Contains(-transform.forward);
					break;
				case EAxis.Forward:
					flag = obj2.Contains(transform.forward);
					break;
				}
			}
			RPCOwner(EPacketType.PT_Custom_CheckResult, num, flag);
		});
	}

	private void RPC_C2S_CheckDistance3D(uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
		byte[] buffer = stream.Read<byte[]>(new object[0]);
		BufferHelper.Import(buffer, delegate(BinaryReader r)
		{
			int num = r.ReadInt32();
			BufferHelper.ReadObject(r, out var obj);
			BufferHelper.ReadObject(r, out var obj2);
			ISceneObject customObj = SPTerrainEvent.GetCustomObj(base.WorldId, obj, this);
			ISceneObject customObj2 = SPTerrainEvent.GetCustomObj(base.WorldId, obj2, this);
			ECompare comp = (ECompare)r.ReadByte();
			float rhs = r.ReadSingle();
			bool flag = false;
			if (!object.Equals(customObj, null) && !object.Equals(customObj2, null))
			{
				float lhs = Vector3.Distance(customObj.Pos, customObj2.Pos);
				flag = Utility.Compare(lhs, rhs, comp);
			}
			RPCOwner(EPacketType.PT_Custom_CheckResult, num, flag);
		});
	}

	private void RPC_C2S_CheckDistance2D(uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
		byte[] buffer = stream.Read<byte[]>(new object[0]);
		BufferHelper.Import(buffer, delegate(BinaryReader r)
		{
			int num = r.ReadInt32();
			BufferHelper.ReadObject(r, out var obj);
			BufferHelper.ReadObject(r, out var obj2);
			ISceneObject customObj = SPTerrainEvent.GetCustomObj(base.WorldId, obj, this);
			ISceneObject customObj2 = SPTerrainEvent.GetCustomObj(base.WorldId, obj2, this);
			ECompare comp = (ECompare)r.ReadByte();
			float rhs = r.ReadSingle();
			bool flag = false;
			if (!object.Equals(customObj, null) && !object.Equals(customObj2, null))
			{
				Vector3 pos = customObj.Pos;
				Vector3 pos2 = customObj2.Pos;
				pos.y = (pos2.y = 0f);
				float lhs = Vector3.Distance(pos, pos2);
				flag = Utility.Compare(lhs, rhs, comp);
			}
			RPCOwner(EPacketType.PT_Custom_CheckResult, num, flag);
		});
	}

	private void RPC_C2S_CheckLookAt3D(uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
		byte[] buffer = stream.Read<byte[]>(new object[0]);
		BufferHelper.Import(buffer, delegate(BinaryReader r)
		{
			int num = r.ReadInt32();
			BufferHelper.ReadObject(r, out var obj);
			BufferHelper.ReadObject(r, out var obj2);
			float rhs = r.ReadSingle();
			EAxis eAxis = (EAxis)r.ReadByte();
			ECompare comp = (ECompare)r.ReadByte();
			ISceneObject customObj = SPTerrainEvent.GetCustomObj(base.WorldId, obj, this);
			ISceneObject customObj2 = SPTerrainEvent.GetCustomObj(base.WorldId, obj2, this);
			bool flag = false;
			if (!object.Equals(customObj, null) && !object.Equals(customObj2, null) && customObj is SkNetworkInterface && customObj2 is SkNetworkInterface)
			{
				Transform transform = ((SkNetworkInterface)customObj).transform;
				Transform transform2 = ((SkNetworkInterface)customObj2).transform;
				Vector3 to = transform2.position - transform.position;
				switch (eAxis)
				{
				case EAxis.Left:
					flag = Utility.Compare(Vector3.Angle(-transform.right, to), rhs, comp);
					break;
				case EAxis.Right:
					flag = Utility.Compare(Vector3.Angle(transform.right, to), rhs, comp);
					break;
				case EAxis.Down:
					flag = Utility.Compare(Vector3.Angle(-transform.up, to), rhs, comp);
					break;
				case EAxis.Up:
					flag = Utility.Compare(Vector3.Angle(transform.up, to), rhs, comp);
					break;
				case EAxis.Backward:
					flag = Utility.Compare(Vector3.Angle(-transform.forward, to), rhs, comp);
					break;
				case EAxis.Forward:
					flag = Utility.Compare(Vector3.Angle(transform.forward, to), rhs, comp);
					break;
				}
			}
			RPCOwner(EPacketType.PT_Custom_CheckResult, num, flag);
		});
	}

	private void RPC_C2S_CheckLookAt2D(uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
		byte[] buffer = stream.Read<byte[]>(new object[0]);
		BufferHelper.Import(buffer, delegate(BinaryReader r)
		{
			int num = r.ReadInt32();
			BufferHelper.ReadObject(r, out var obj);
			BufferHelper.ReadObject(r, out var obj2);
			float rhs = r.ReadSingle();
			EAxis eAxis = (EAxis)r.ReadByte();
			ECompare comp = (ECompare)r.ReadByte();
			ISceneObject customObj = SPTerrainEvent.GetCustomObj(base.WorldId, obj, this);
			ISceneObject customObj2 = SPTerrainEvent.GetCustomObj(base.WorldId, obj2, this);
			bool flag = false;
			if (!object.Equals(customObj, null) && !object.Equals(customObj2, null) && customObj is SkNetworkInterface && customObj2 is SkNetworkInterface)
			{
				Transform transform = ((SkNetworkInterface)customObj).transform;
				Transform transform2 = ((SkNetworkInterface)customObj2).transform;
				Vector3 to = transform2.position - transform.position;
				Vector3 forward = transform.forward;
				Vector3 right = transform.right;
				to.y = (forward.y = (right.y = 0f));
				switch (eAxis)
				{
				case EAxis.Left:
					flag = Utility.Compare(Vector3.Angle(-right, to), rhs, comp);
					break;
				case EAxis.Right:
					flag = Utility.Compare(Vector3.Angle(right, to), rhs, comp);
					break;
				case EAxis.Down:
					flag = true;
					break;
				case EAxis.Up:
					flag = true;
					break;
				case EAxis.Backward:
					flag = Utility.Compare(Vector3.Angle(-forward, to), rhs, comp);
					break;
				case EAxis.Forward:
					flag = Utility.Compare(Vector3.Angle(forward, to), rhs, comp);
					break;
				}
			}
			RPCOwner(EPacketType.PT_Custom_CheckResult, num, flag);
		});
	}

	private void RPC_C2S_CheckOwerItem(uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
		byte[] buffer = stream.Read<byte[]>(new object[0]);
		BufferHelper.Import(buffer, delegate(BinaryReader r)
		{
			int num = r.ReadInt32();
			BufferHelper.ReadObject(r, out var obj);
			BufferHelper.ReadObject(r, out var obj2);
			int rhs = r.ReadInt32();
			ECompare comp = (ECompare)r.ReadByte();
			bool flag = false;
			if (obj.type == OBJECTTYPE.Player && obj2.type == OBJECTTYPE.ItemProto)
			{
				Player player = (Player)SPTerrainEvent.GetCustomObj(base.WorldId, obj, this);
				if (null != player)
				{
					int lhs = 0;
					if (obj2.isSpecificPrototype)
					{
						lhs = player.Package.GetItemCount(obj2.Id);
					}
					else if (obj2.isAnyPrototypeInCategory)
					{
						lhs = player.Package.GetCountByEditorType(obj2.Group);
					}
					else if (obj2.isAnyPrototype)
					{
						lhs = player.Package.GetAllItemsCount();
					}
					if (Utility.Compare(lhs, rhs, comp))
					{
						flag = true;
					}
				}
			}
			RPCOwner(EPacketType.PT_Custom_CheckResult, num, flag);
		});
	}

	private void RPC_C2S_CheckAttribute(uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
		byte[] buffer = stream.Read<byte[]>(new object[0]);
		BufferHelper.Import(buffer, delegate(BinaryReader r)
		{
			int num = r.ReadInt32();
			BufferHelper.ReadObject(r, out var obj);
			float rhs = r.ReadSingle();
			AttribType type = (AttribType)r.ReadByte();
			ECompare comp = (ECompare)r.ReadByte();
			bool flag = false;
			SkNetworkInterface skNetworkInterface = (SkNetworkInterface)SPTerrainEvent.GetCustomObj(base.WorldId, obj, this);
			if (!object.Equals(null, skNetworkInterface) && Utility.Compare(skNetworkInterface._skEntity.GetAttribute(type), rhs, comp))
			{
				flag = true;
			}
			RPCOwner(EPacketType.PT_Custom_CheckResult, num, flag);
		});
	}

	private void RPC_C2S_CheckEquipment(uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
		byte[] buffer = stream.Read<byte[]>(new object[0]);
		BufferHelper.Import(buffer, delegate(BinaryReader r)
		{
			int num = r.ReadInt32();
			BufferHelper.ReadObject(r, out var obj);
			BufferHelper.ReadObject(r, out var obj2);
			bool flag = false;
			if (obj.type == OBJECTTYPE.Player && obj2.type == OBJECTTYPE.ItemProto)
			{
				Player player = (Player)SPTerrainEvent.GetCustomObj(base.WorldId, obj, this);
				if (null != player)
				{
					ItemObject[] equipItems = player.EquipModule.EquipItems;
					ItemObject[] array = equipItems;
					foreach (ItemObject itemObject in array)
					{
						if (obj2.isAnyPrototype)
						{
							flag = true;
						}
						if (obj2.isAnyPrototypeInCategory && itemObject.protoData.editorTypeId == obj2.Group)
						{
							flag = true;
						}
						if (itemObject.protoId == obj2.Id)
						{
							flag = true;
						}
					}
				}
			}
			RPCOwner(EPacketType.PT_Custom_CheckResult, num, flag);
		});
	}

	private void RPC_C2S_CheckWorld(uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
		byte[] buffer = stream.Read<byte[]>(new object[0]);
		BufferHelper.Import(buffer, delegate(BinaryReader r)
		{
			int num = r.ReadInt32();
			BufferHelper.ReadObject(r, out var obj);
			int num2 = r.ReadInt32();
			ISceneObject customObj = SPTerrainEvent.GetCustomObj(num2, obj, this);
			bool flag = !object.Equals(null, customObj) && customObj.WorldId == num2;
			RPCOwner(EPacketType.PT_Custom_CheckResult, num, flag);
		});
	}

	private void RPC_C2S_AddQuest(uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
		byte[] data = stream.Read<byte[]>(new object[0]);
		BufferHelper.Import(data, delegate(BinaryReader r)
		{
			BufferHelper.ReadObject(r, out var obj);
			int quest_id = r.ReadInt32();
			string text = r.ReadString();
			if (CustomGameData.Mgr.dialogMgr.SetQuest(obj.Group, obj.Id, quest_id, text))
			{
				RPCOthers(EPacketType.PT_Custom_AddQuest, data);
			}
		});
	}

	private void RPC_C2S_RemoveQuest(uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
		byte[] data = stream.Read<byte[]>(new object[0]);
		BufferHelper.Import(data, delegate(BinaryReader r)
		{
			BufferHelper.ReadObject(r, out var obj);
			int quest_id = r.ReadInt32();
			if (CustomGameData.Mgr.dialogMgr.RemoveQuest(obj.Group, obj.Id, quest_id))
			{
				RPCOthers(EPacketType.PT_Custom_RemoveQuest, data);
			}
		});
	}

	private void RPC_C2S_PlayerAnimation(uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
		byte[] array = stream.Read<byte[]>(new object[0]);
		RPCOthers(EPacketType.PT_Custom_StartAnimation, array);
	}

	private void RPC_C2S_StopAnimation(uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
		byte[] array = stream.Read<byte[]>(new object[0]);
		RPCOthers(EPacketType.PT_Custom_StopAnimation, array);
	}

	private void RPC_C2S_SetPose(uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
		int num = stream.Read<int>(new object[0]);
	}

	private void RPC_C2S_ModifyPackage(uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
		byte[] buffer = stream.Read<byte[]>(new object[0]);
		BufferHelper.Import(buffer, delegate(BinaryReader r)
		{
			BufferHelper.ReadObject(r, out var obj);
			BufferHelper.ReadObject(r, out var obj2);
			int rhs = r.ReadInt32();
			EFunc func = (EFunc)r.ReadByte();
			if (obj.type == OBJECTTYPE.Player && obj2.type == OBJECTTYPE.ItemProto && obj2.isSpecificPrototype)
			{
				Player player = (Player)SPTerrainEvent.GetCustomObj(base.WorldId, obj, this);
				if (null != player)
				{
					int itemCount = player.Package.GetItemCount(obj2.Id);
					int num = Utility.Function(itemCount, rhs, func);
					List<ItemObject> effItems = new List<ItemObject>();
					if (num > itemCount)
					{
						player.Package.AddSameItems(obj2.Id, num - itemCount, ref effItems);
						player.SyncNewItem(new ItemSample(obj2.Id, num - itemCount));
					}
					else
					{
						player.Package.RemoveItem(obj2.Id, itemCount - num, ref effItems);
					}
					player.SyncItemList(effItems);
					player.SyncPackageIndex();
				}
			}
		});
	}

	private void RPC_C2S_SetVariable(uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
		int num = stream.Read<int>(new object[0]);
	}

	private void RPC_C2S_ModifyStat(uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
		byte[] buffer = stream.Read<byte[]>(new object[0]);
		BufferHelper.Import(buffer, delegate(BinaryReader r)
		{
			BufferHelper.ReadObject(r, out var obj);
			float rhs = r.ReadSingle();
			AttribType type = (AttribType)r.ReadByte();
			EFunc func = (EFunc)r.ReadByte();
			SkNetworkInterface skNetworkInterface = (SkNetworkInterface)SPTerrainEvent.GetCustomObj(base.WorldId, obj, this);
			if (null != skNetworkInterface && null != skNetworkInterface._skEntity)
			{
				float attribute = skNetworkInterface._skEntity.GetAttribute(type);
				float value = Utility.Function(attribute, rhs, func);
				skNetworkInterface.SetAttribute(type, value, isBase: true);
			}
		});
	}

	private void RPC_C2S_Kill(uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
		byte[] buffer = stream.Read<byte[]>(new object[0]);
		BufferHelper.Import(buffer, delegate(BinaryReader r)
		{
			BufferHelper.ReadObject(r, out var obj);
			BufferHelper.ReadRange(r, out var obj2);
			if (obj.isPrototype)
			{
				if (obj.type == OBJECTTYPE.MonsterProto)
				{
					List<AiMonsterNetwork> list = ObjNetInterface.Get<AiMonsterNetwork>();
					for (int i = 0; i != list.Count; i++)
					{
						if (obj2.Contains(list[i].Pos) && null != list[i]._skEntity)
						{
							list[i].SetAttribute(AttribType.Hp, 0f, isBase: true);
						}
					}
				}
			}
			else
			{
				SkNetworkInterface skNetworkInterface = (SkNetworkInterface)SPTerrainEvent.GetCustomObj(base.WorldId, obj, this);
				if (null != skNetworkInterface && null != skNetworkInterface._skEntity && obj2.Contains(skNetworkInterface.Pos))
				{
					skNetworkInterface.SetAttribute(AttribType.Hp, 0f, isBase: true);
				}
			}
		});
	}

	public void KillPlayer()
	{
		SetAttribute(AttribType.Hp, 0f, isBase: false);
		RPCOthers(EPacketType.PT_InGame_SKSyncAttr, (byte)1, _skEntity.GetAttribute(AttribType.Hp), false, -1);
	}

	private void RPC_C2S_CreateObject(uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
		byte[] buffer = stream.Read<byte[]>(new object[0]);
		BufferHelper.Import(buffer, delegate(BinaryReader r)
		{
			BufferHelper.ReadObject(r, out var obj);
			BufferHelper.ReadRange(r, out var obj2);
			int amout = r.ReadInt32();
			SPTerrainEvent.CreateObjects(obj, obj2, amout, base.WorldId, this);
		});
	}

	private void RPC_C2S_RemoveObject(uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
		byte[] buffer = stream.Read<byte[]>(new object[0]);
		BufferHelper.Import(buffer, delegate(BinaryReader r)
		{
			BufferHelper.ReadObject(r, out var obj);
			BufferHelper.ReadRange(r, out var obj2);
			SPTerrainEvent.RemoveObjects(obj, obj2, base.WorldId, this);
		});
	}

	private void RPC_C2S_RemoveSpecificObject(uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
		byte[] buffer = stream.Read<byte[]>(new object[0]);
		BufferHelper.Import(buffer, delegate(BinaryReader r)
		{
			BufferHelper.ReadObject(r, out var obj);
			SPTerrainEvent.RemoveSpecObject(obj, base.WorldId, this);
		});
	}

	private void RPC_C2S_EnableSpawn(uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
		byte[] array = stream.Read<byte[]>(new object[0]);
		RPCOthers(EPacketType.PT_Custom_EnableSpawn, array);
	}

	private void RPC_C2S_DisableSpawn(uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
		byte[] array = stream.Read<byte[]>(new object[0]);
		RPCOthers(EPacketType.PT_Custom_DisableSpawn, array);
	}

	private void RPC_C2S_OrderVector(uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
		int num = stream.Read<int>(new object[0]);
		Vector3 vector = stream.Read<Vector3>(new object[0]);
	}

	private void RPC_C2S_OrderTarget(uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
		byte[] array = stream.Read<byte[]>(new object[0]);
		RPCOthers(EPacketType.PT_Custom_OrderTarget, array);
	}

	private void RPC_C2S_CancelOrder(uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
		byte[] array = stream.Read<byte[]>(new object[0]);
		RPCOthers(EPacketType.PT_Custom_CancelOrder, array);
	}

	private void RPC_C2S_FastTravel(uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
		int num = stream.Read<int>(new object[0]);
		Vector3 pos = stream.Read<Vector3>(new object[0]);
		Vector3 vector = stream.Read<Vector3>(new object[0]);
		if (CustomGameData.Mgr.data.WorldNames.Length > num)
		{
			string yirdName = CustomGameData.Mgr.data.WorldNames[num];
			int gameWorldId = CustomGameData.Mgr.GetGameWorldId(yirdName);
			if (gameWorldId == base.WorldId)
			{
				CommonHelper.AdjustPos(ref pos);
				SetPosition(pos);
				SyncPosArea(pos);
				RPCOthers(EPacketType.PT_InGame_FastTransfer, pos);
			}
			else
			{
				int teamId = base.TeamId;
				ResetTeamId();
				GroupNetwork.RemoveFromTeam(teamId, base.Id);
				NetInterface.RemovePlayerFromGroup(info.sender, base.WorldId);
				NetInterface.RemovePlayerFromGroup(info.sender, 101);
				RPCOwner(EPacketType.PT_InGame_SwitchScene, num, pos);
			}
		}
	}

	private void RPC_C2S_AddChoice(uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
		int num = stream.Read<int>(new object[0]);
		string text = stream.Read<string>(new object[0]);
		if (CustomGameData.Mgr.dialogMgr.AddChoose(num, text))
		{
			RPCOthers(EPacketType.PT_Custom_AddChoice, num, text);
		}
	}

	internal float GetDurAtk()
	{
		return _skEntity.GetAttribute(AttribType.ResDamage);
	}

	internal float GetResBouns()
	{
		return _skEntity.GetAttribute(AttribType.ResBouns);
	}

	public void SyncErrorMsg(string estr)
	{
		if (estr.Length > 0)
		{
			RPCOwner(EPacketType.PT_Common_ErrorMsg, estr);
		}
	}

	public void SyncErrorMsgBox(string estr)
	{
		if (estr.Length > 0)
		{
			RPCOwner(EPacketType.PT_Common_ErrorMsgBox, estr);
		}
	}

	public void SyncErrorMsg(int estr)
	{
		if (estr > 0)
		{
			RPCOwner(EPacketType.PT_Common_ErrorMsgCode, estr);
		}
	}

	public void AddLobbyExp(float exp)
	{
		_lobbyExpIncrement += exp;
	}

	private void SendLobbyExpToLobby()
	{
		uLobbyNetwork.AddAccountLobbyExp(steamId, _lobbyExpIncrement);
		_lobbyExpIncrement = 0f;
	}

	public void CreateAccountItem(int itemType, int amount)
	{
		RPCOwner(EPacketType.PT_InGame_AccItems_CreateItem, itemType, amount);
		ItemObject itemObject = CreateItem(itemType, amount, syn: true);
		if (itemObject != null)
		{
			Package.AddItem(itemObject);
			SyncPackageIndex();
		}
	}

	private void RPC_AccItems_CreateItem(uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
		int itemType = stream.ReadInt32();
		int amount = stream.ReadInt32();
		BaseNetwork baseNetwork = BaseNetwork.GetBaseNetwork(base.Id);
		if (baseNetwork != null)
		{
			ulong num = baseNetwork.steamId;
			uLobbyNetwork.CreateAccountItems(itemType, amount, num);
		}
	}

	private void InitPlayerBattle()
	{
		this.KillEvent = (Action<int>)Delegate.Combine(this.KillEvent, new Action<int>(BattleManager.OnKill));
		this.DeathEvent = (Action<int, bool>)Delegate.Combine(this.DeathEvent, new Action<int, bool>(BattleManager.OnDeath));
		this.OccupyEvent = (Action<int>)Delegate.Combine(this.OccupyEvent, new Action<int>(BattleManager.OnOccupy));
		this.MoneyEvent = (Action<int, int>)Delegate.Combine(this.MoneyEvent, new Action<int, int>(BattleManager.OnBattleMoney));
		this.PointEvent = (Action<int, float>)Delegate.Combine(this.PointEvent, new Action<int, float>(BattleManager.OnBattlePoint));
	}

	internal void GetPoint(float point)
	{
		_battleInfo.Point += point;
		BattleManager.OnGetPoint(base.TeamId, point);
	}

	public void AddHarmer(GroupNetInterface harmer, float damage)
	{
		if (_harmList.ContainsKey(harmer))
		{
			DamageData damageData = _harmList[harmer];
			damageData._Damage += damage;
			damageData._DamageTime = (float)uLink.Network.time;
		}
		else
		{
			DamageData value = default(DamageData);
			value._Damage = damage;
			value._DamageTime = (float)uLink.Network.time;
			_harmList.Add(harmer, value);
		}
	}

	public void AddKill()
	{
		_battleInfo.KillCount++;
		if (this.KillEvent != null)
		{
			this.KillEvent(base.TeamId);
		}
	}

	public void GetSite()
	{
		_battleInfo.SiteCount++;
		if (this.OccupyEvent != null)
		{
			this.OccupyEvent(base.TeamId);
		}
		SyncPlayerBattleInfo();
	}

	public void OnBattleDeath(SkNetworkInterface caster)
	{
		_battleInfo.DeathCount++;
		if (this.DeathEvent != null)
		{
			this.DeathEvent(base.TeamId, arg2: true);
		}
		foreach (KeyValuePair<GroupNetInterface, DamageData> harm in _harmList)
		{
			if (harm.Key.TeamId != caster.TeamId)
			{
				continue;
			}
			Player player = harm.Key as Player;
			if (!(null == player))
			{
				if (caster.Equals(player))
				{
					player.AddKill();
					player.GetBattlePoint(BattleConstData.Instance._points_kill);
					player.GetBattleMoney(BattleConstData.Instance._meat_kill);
					player.SyncPlayerBattleInfo();
				}
				else if (uLink.Network.time - (double)harm.Value._DamageTime <= (double)DamageData.ValidTime)
				{
					player.GetBattlePoint(BattleConstData.Instance._points_assist);
					player.GetBattleMoney(BattleConstData.Instance._meat_assist);
					player.SyncPlayerBattleInfo();
				}
			}
		}
		BattleManager.SyncBattleInfos();
		_harmList.Clear();
	}

	public void GetBattlePoint(float point)
	{
		_battleInfo.Point += point;
		if (this.PointEvent != null)
		{
			this.PointEvent(base.TeamId, point);
		}
	}

	public void GetBattleMoney(int money)
	{
		_battleInfo.Money += money;
		AddMoney(money);
		if (this.MoneyEvent != null)
		{
			this.MoneyEvent(base.TeamId, money);
		}
	}

	public void SyncPlayerBattleInfo(uLink.NetworkPlayer peer)
	{
		RPCPeer(peer, EPacketType.PT_InGame_PlayerBattleInfo, Battle);
	}

	public void SyncPlayerBattleInfo()
	{
		RPCOthers(EPacketType.PT_InGame_PlayerBattleInfo, Battle);
	}

	public void SyncLoadData()
	{
		PEDbOp pEDbOp = AsyncSqlite.NewReadOp();
		if (pEDbOp != null)
		{
			pEDbOp.SetCmdText("SELECT ver,pos,teamid,playerdata FROM player WHERE roleid=@roleid AND steamid=@steamid;");
			pEDbOp.BindParam("@roleid", base.Id);
			pEDbOp.BindParam("@steamid", (long)steamId);
			pEDbOp.BindReaderHandler(LoadDataComplete);
			pEDbOp.Exec();
			pEDbOp = null;
		}
	}

	private void LoadDataComplete(SqliteDataReader reader)
	{
		if (!reader.Read())
		{
			return;
		}
		int @int = reader.GetInt32(reader.GetOrdinal("teamid"));
		if (ServerConfig.IsSurvive || @int == base.TeamId)
		{
			byte[] buff = (byte[])reader.GetValue(reader.GetOrdinal("playerdata"));
			Serialize.Import(buff, delegate(BinaryReader r)
			{
				ReadSkill(r);
				ReadShortcut(r);
				ReadArmorInfo(r);
				_gameMoney = BufferHelper.ReadInt32(r);
				_curSceneId = BufferHelper.ReadInt32(r);
				ImportCmpt(r);
			});
			_hasRecord = true;
		}
	}

	public void LoadRecordData()
	{
		_hasRecord = false;
		SyncLoadData();
		if (_hasRecord)
		{
			return;
		}
		InitPackage();
		InitEquip();
		ValidateArmorData();
		int centerIndex = AreaHelper.Vector2Int(base.transform.position);
		List<int> neighborIndex = AreaHelper.GetNeighborIndex(centerIndex);
		foreach (int item in neighborIndex)
		{
			GameWorld.AddExploredArea(base.Id, base.WorldId, base.TeamId, item);
		}
		Save();
	}

	public void SyncSave()
	{
		PlayerData playerData = new PlayerData();
		byte[] pos = Serialize.Export(delegate(BinaryWriter w)
		{
			if (base.transform.position.y < -100f)
			{
				BufferHelper.Serialize(w, enterPos);
			}
			else
			{
				BufferHelper.Serialize(w, base.transform.position);
			}
		});
		byte[] data = Serialize.Export(delegate(BinaryWriter w)
		{
			WriteSkill(w);
			WriteShortcut(w);
			WriteArmorInfo(w);
			BufferHelper.Serialize(w, GameMoney);
			BufferHelper.Serialize(w, _curSceneId);
			ExportCmpt(w);
		});
		playerData.ExportData(base.Id, roleName, (long)steamId, base.TeamId, base.WorldId, pos, data);
		AsyncSqlite.AddRecord(playerData);
	}

	public void SyncSaveGameWorld()
	{
		byte[] data = Serialize.Export(delegate(BinaryWriter w)
		{
			GameWorld.Serialize(base.Id, base.WorldId, w);
		});
		PlayerWorldData playerWorldData = new PlayerWorldData();
		playerWorldData.ExportData(base.Id, base.WorldId, data);
		AsyncSqlite.AddRecord(playerWorldData);
	}

	public void Save()
	{
		SyncSave();
		SyncSaveGameWorld();
	}

	private void AddGameMoney(int num)
	{
		if (ServerConfig.MoneyType == EMoneyType.Meat)
		{
			List<ItemObject> effItems = new List<ItemObject>();
			Package.AddSameItems(229, num, ref effItems);
			SyncItemList(effItems);
			SyncPackageIndex();
		}
		else
		{
			int value = _gameMoney + num;
			_gameMoney = Mathf.Clamp(value, 0, 268435455);
		}
	}

	private void SubGameMoney(int num)
	{
		if (ServerConfig.MoneyType == EMoneyType.Meat)
		{
			List<ItemObject> effItems = new List<ItemObject>();
			Package.RemoveItem(229, num, ref effItems);
			SyncItemList(effItems);
			SyncPackageIndex();
		}
		else
		{
			int value = _gameMoney - num;
			_gameMoney = Mathf.Clamp(value, 0, 268435455);
		}
	}

	public void SetShortcutKey(int objId, int srcIndex, int destIndex, ItemPlaceType place)
	{
		if (objId == -1)
		{
			_shortcutKeys.Remove(srcIndex);
			SyncSetShortCut(srcIndex, -1, -1, -1);
			return;
		}
		switch (place)
		{
		case ItemPlaceType.IPT_Bag:
			_shortcutKeys[destIndex] = objId;
			SyncSetShortCut(-1, -1, destIndex, objId);
			break;
		case ItemPlaceType.IPT_HotKeyBar:
			if (_shortcutKeys.ContainsKey(srcIndex) && _shortcutKeys[srcIndex] == objId)
			{
				int srcObjId = -1;
				if (_shortcutKeys.ContainsKey(destIndex))
				{
					int num = _shortcutKeys[destIndex];
					_shortcutKeys[srcIndex] = num;
					srcObjId = num;
				}
				else
				{
					_shortcutKeys.Remove(srcIndex);
				}
				_shortcutKeys[destIndex] = objId;
				SyncSetShortCut(srcIndex, srcObjId, destIndex, objId);
			}
			break;
		}
	}

	public void GetOnVehicle(CreationNetwork creation)
	{
		if (null == creation)
		{
			return;
		}
		int index = -1;
		_SeatType = creation.GetOn(this, ref index);
		if (_SeatType == EVCComponent.cpAbstract)
		{
			if (LogFilter.logDebug)
			{
				Debug.LogWarning("Can not geton.");
			}
			return;
		}
		base._OnCar = true;
		base._SeatIndex = index;
		base._Creation = creation;
		RPCOthers(EPacketType.PT_InGame_GetOnVehicle, creation.Id, index);
		if (_SeatType != EVCComponent.cpSideSeat)
		{
			creation.SetDriver(this);
		}
	}

	public void GetOffVehicle(Vector3 outPos)
	{
		SetPosition(outPos);
		if (null != base._Creation)
		{
			RPCOthers(EPacketType.PT_InGame_GetOffVehicle, base.transform.position, _SeatType);
			EVCComponent off = base._Creation.GetOff(this);
			if (off != EVCComponent.cpSideSeat && off != 0)
			{
				base._Creation.ResetDriver(outPos);
			}
		}
		base._OnCar = false;
		base._SeatIndex = -2;
		_SeatType = EVCComponent.cpAbstract;
		base._Creation = null;
	}

	public static ObjType GetPutOutClass(ItemObject item)
	{
		switch ((EItemClass)item.protoData.itemClassId)
		{
		case EItemClass.COLONY_CORE:
			return ObjType.Colony;
		case EItemClass.FLAGS:
			return ObjType.Flag;
		case EItemClass.TOWER_FOUR:
		case EItemClass.TOWER:
		case EItemClass.ENERGYTOWER:
			return ObjType.Tower;
		case EItemClass.MONSTERBEACON:
			return ObjType.AiBeacon;
		case EItemClass.MOTOBIKE:
		case EItemClass.CAR:
		case EItemClass.CART:
		case EItemClass.SHIPS:
		case EItemClass.AIRPLANE:
		case EItemClass.AIRCRAFT:
		case EItemClass.ROBOT:
		case EItemClass.AITURRET:
			return ObjType.Creation;
		case EItemClass.PLANT_SEED:
			return ObjType.PlantSeed;
		case EItemClass.DOODADBOX:
			return ObjType.DoodadBox;
		default:
			return ObjType.Item;
		}
	}

	public bool RepairVehicle(CreationNetwork creation)
	{
		if (null == creation)
		{
			return false;
		}
		if (creation.IsFullHP())
		{
			if (LogFilter.logDebug)
			{
				Debug.LogWarning("No need to repair it.");
			}
			return false;
		}
		ItemObject itemByItemID = Package.GetItemByItemID(1030);
		if (itemByItemID == null)
		{
			if (LogFilter.logDebug)
			{
				Debug.LogWarning("Need a 'Repair kit'!");
			}
			return false;
		}
		if (!itemByItemID.CountDown(1))
		{
			Package.DeleteItem(itemByItemID);
			SyncPackageIndex();
		}
		else
		{
			SyncItem(itemByItemID);
		}
		return true;
	}

	public void ChargeVehicle(CreationNetwork creation)
	{
		if (null == creation || creation.IsFullFuel())
		{
			return;
		}
		ItemObject itemByItemID = Package.GetItemByItemID(1029);
		if (itemByItemID == null)
		{
			if (LogFilter.logDebug)
			{
				Debug.LogWarning("Need an 'Energe packet'!");
			}
			return;
		}
		if (!itemByItemID.CountDown(1))
		{
			Package.DeleteItem(itemByItemID);
			SyncPackageIndex();
		}
		else
		{
			SyncItem(itemByItemID);
		}
		creation.Fuel += 36000f;
		creation.Fuel = Mathf.Clamp(creation.Fuel, 0f, creation.MaxFuel);
		creation.RPCOthers(EPacketType.PT_CR_ChargeFuel, creation.Fuel);
	}

	public int ApplyItemCost(Dictionary<IntVector3, B45Block> addList, Dictionary<IntVector3, B45Block> removeList)
	{
		Dictionary<int, int> dictionary = new Dictionary<int, int>();
		int num = 0;
		if (removeList != null)
		{
			foreach (B45Block value in removeList.Values)
			{
				if (value.blockType >> 2 > 0)
				{
					int blockItemProtoID = BSBlockMatMap.GetBlockItemProtoID(value.materialType);
					if (dictionary.ContainsKey(blockItemProtoID))
					{
						Dictionary<int, int> dictionary2;
						Dictionary<int, int> dictionary3 = (dictionary2 = dictionary);
						int key;
						int key2 = (key = blockItemProtoID);
						key = dictionary2[key];
						dictionary3[key2] = key - 1;
					}
					else
					{
						dictionary[blockItemProtoID] = -1;
					}
					num--;
				}
			}
		}
		if (addList != null)
		{
			foreach (B45Block value2 in addList.Values)
			{
				if (value2.blockType >> 2 > 0)
				{
					int blockItemProtoID2 = BSBlockMatMap.GetBlockItemProtoID(value2.materialType);
					if (dictionary.ContainsKey(blockItemProtoID2))
					{
						Dictionary<int, int> dictionary4;
						Dictionary<int, int> dictionary5 = (dictionary4 = dictionary);
						int key;
						int key3 = (key = blockItemProtoID2);
						key = dictionary4[key];
						dictionary5[key3] = key + 1;
					}
					else
					{
						dictionary[blockItemProtoID2] = 1;
					}
				}
				num++;
			}
		}
		List<ItemObject> effItems = new List<ItemObject>(10);
		List<ItemSample> list = new List<ItemSample>();
		foreach (KeyValuePair<int, int> item in dictionary)
		{
			int key4 = item.Key;
			if (item.Value == 0)
			{
				continue;
			}
			if (item.Value >= 1)
			{
				int num2 = Mathf.CeilToInt(1f * (float)item.Value / (float)GameWorld.BlockNumPerItem);
				if (num2 > 0)
				{
					Package.RemoveItem(key4, num2, ref effItems);
				}
				continue;
			}
			int num3 = Mathf.CeilToInt(1f * (float)Mathf.Abs(item.Value) / (float)GameWorld.BlockNumPerItem);
			if (num3 > 0)
			{
				Package.AddSameItems(key4, num3, ref effItems);
				list.Add(new ItemSample(key4, num3));
			}
		}
		SyncItemList(effItems);
		SyncNewItem(list);
		SyncPackageIndex();
		return num;
	}

	public override void WeaponReload(int objId, int oldProtoId, int newProtoId, float magazineSize)
	{
		ItemObject itemByID = ItemManager.GetItemByID(objId);
		if (itemByID == null)
		{
			return;
		}
		GunAmmo cmpt = itemByID.GetCmpt<GunAmmo>();
		if (cmpt != null)
		{
			List<ItemObject> effItems = new List<ItemObject>();
			if (oldProtoId != -1 && cmpt.count != 0)
			{
				Package.AddSameItems(oldProtoId, cmpt.count, ref effItems);
				cmpt.count = 0;
			}
			Package.RemoveItem(newProtoId, (int)magazineSize, ref effItems);
			cmpt.count += (int)magazineSize;
			cmpt.index = newProtoId;
			effItems.Add(itemByID);
			ChannelNetwork.SyncItemList(base.WorldId, effItems);
			SyncPackageIndex();
		}
	}

	public override bool GunEnergyReload(ItemObject item, float num)
	{
		if (item == null)
		{
			return false;
		}
		GunAmmo cmpt = item.GetCmpt<GunAmmo>();
		if (cmpt == null)
		{
			return false;
		}
		cmpt.count = (int)num;
		return true;
	}

	public override bool BatteryEnergyReload(ItemObject item, float num)
	{
		if (item == null)
		{
			return false;
		}
		return item.GetCmpt<Energy>()?.SetValue(num) ?? false;
	}

	public override void JetPackEnergyReload(ItemObject item, float num)
	{
		if (item != null)
		{
			JetPkg cmpt = item.GetCmpt<JetPkg>();
			if (cmpt != null)
			{
				cmpt.energy = num;
			}
		}
	}

	public override void ItemAttrChange(int itemObjId, float num)
	{
		ItemObject itemObject = EquipModule[itemObjId];
		if (itemObject == null)
		{
			return;
		}
		if (itemObject.protoId == 68 || itemObject.protoId == 69)
		{
			int num2 = ((itemObject.protoId != 68) ? 53 : 51);
			int itemCount = Package.GetItemCount(num2);
			if (itemCount >= Mathf.RoundToInt(num))
			{
				List<ItemObject> effItems = new List<ItemObject>();
				Package.RemoveItem(num2, Mathf.RoundToInt(num), ref effItems);
				ChannelNetwork.SyncItemList(base.WorldId, effItems);
				SyncPackageIndex();
			}
		}
		else
		{
			GunAmmo cmpt = itemObject.GetCmpt<GunAmmo>();
			if (cmpt != null && !((float)cmpt.count < num))
			{
				cmpt.count -= (int)num;
				ChannelNetwork.SyncItem(base.WorldId, itemObject);
			}
		}
	}

	public override void EquipItemCost(int itemObjId, float num)
	{
		ItemObject itemObject = EquipModule[itemObjId];
		if (itemObject != null)
		{
			if (!itemObject.CountDown((int)num))
			{
				EquipModule.Remove(itemObject);
				SyncTakeOffEquipment(new int[1] { itemObjId });
			}
			else
			{
				ChannelNetwork.SyncItem(base.WorldId, itemObject);
			}
		}
	}

	public override void PackageItemCost(int itemObjId, float num)
	{
		ItemObject itemById = Package.GetItemById(itemObjId);
		if (itemById != null)
		{
			if (itemById.CountDown((int)num))
			{
				SyncItem(itemById);
				SyncPackageIndex();
			}
			else
			{
				Package.RemoveItem(itemById);
				SyncPackageIndex();
			}
		}
	}

	public override void WeaponDurabilityChange(ItemObject item)
	{
		item?.GetCmpt<Equip>()?.ExpendAttackDurability();
	}

	public override float ArmorDurabilityChange(int itemObjId, float damage)
	{
		ItemObject itemByID = ItemManager.GetItemByID(itemObjId);
		if (itemByID == null)
		{
			return 0f;
		}
		Equip cmpt = itemByID.GetCmpt<Equip>();
		if (cmpt != null)
		{
			cmpt.ExpendDefenceDurability(damage);
			return cmpt.GetDurability();
		}
		Durability cmpt2 = itemByID.GetCmpt<Durability>();
		if (cmpt2 == null)
		{
			return 0f;
		}
		cmpt2.ChangeValue(0f - damage);
		return cmpt2.floatValue.current;
	}

	internal void SyncItem(ItemObject item)
	{
		ChannelNetwork.SyncItem(base.OwnerView.owner, item);
	}

	internal void SyncItemList(IEnumerable<ItemObject> items)
	{
		SyncItemList(items.ToArray());
	}

	internal void SyncItemList(ItemObject[] items)
	{
		if (items.Length >= 1)
		{
			ChannelNetwork.SyncItemList(base.OwnerView.owner, items);
		}
	}

	internal void SyncNewItem(IEnumerable<ItemSample> items)
	{
		SyncNewItem(items.ToArray());
	}

	internal void SyncNewItem(ItemSample[] items)
	{
		if (items != null && items.Length >= 1)
		{
			RPCOwner(EPacketType.PT_InGame_NewItemList, items, false);
		}
	}

	internal void SyncNewItem(ItemSample item)
	{
		SyncNewItem(new ItemSample[1] { item });
	}

	public void SyncSplitItemFromPackage(int objId, int index)
	{
		RPCOwner(EPacketType.PT_InGame_PackageSplit, objId, index);
	}

	public void SyncInitPackage()
	{
		RPCOwner(EPacketType.PT_InGame_InitPackage, Package.MaxItemNum, Package.MaxEquipNum, Package.MaxResourceNum, Package.MaxArmorNum);
	}

	public void SyncPackageItems(int tab)
	{
		List<ItemObject> validItemList = Package.GetValidItemList(tab);
		SyncItemList(validItemList.ToArray());
	}

	public void SyncPackageIndex(int tab)
	{
		byte[] array2 = Serialize.Export(delegate(BinaryWriter w)
		{
			int[] array = Package.GetItemObjIDs(tab).ToArray();
			BufferHelper.Serialize(w, array.Length);
			for (int i = 0; i < array.Length; i++)
			{
				int num = tab;
				num <<= 16;
				num += i;
				BufferHelper.Serialize(w, num);
				BufferHelper.Serialize(w, array[i]);
			}
		});
		RPCOwner(EPacketType.PT_InGame_PackageIndex, array2);
	}

	public void SyncInitShortCut()
	{
		byte[] array = Serialize.Export(delegate(BinaryWriter w)
		{
			WriteShortcut(w);
		});
		RPCOwner(EPacketType.PT_InGame_InitShortcut, array);
	}

	public void SyncSetShortCut(int srcIndex, int srcObjId, int destIndex, int destObjId)
	{
		RPCOwner(EPacketType.PT_InGame_SetShortcut, srcIndex, srcObjId, destIndex, destObjId);
	}

	public void SyncInitLearntSkills()
	{
		RPCOwner(EPacketType.PT_InGame_InitLearntSkills, ExportLearntIDs().ToArray());
	}

	public void SyncPackageIndex()
	{
		byte[] changedIndex = Package.GetChangedIndex();
		if (changedIndex != null)
		{
			RPCOwner(EPacketType.PT_InGame_PackageIndex, changedIndex);
		}
		Package.SyncMissionPackageIndex();
	}

	private void createBuildingBlock(Vector3 root, int id, int rotation)
	{
		GameWorld gameWorld = GameWorld.GetGameWorld(base.WorldId);
		if (gameWorld != null)
		{
			if (LogFilter.logDebug)
			{
				Debug.Log(string.Concat("BuildBuilding", root, " ", id, " ", rotation));
			}
			Dictionary<IntVector3, B45Block> dictionary = GameWorld.BuildBuilding(root, id, rotation);
			if (LogFilter.logDebug)
			{
				Debug.Log("BuildBuildingBlock" + dictionary.Count);
			}
			gameWorld.BuildBuildingBlock(dictionary);
		}
	}

	private IEnumerator createBuildingItems(List<ItemObject> buildingItemList, List<Vector3> pos, List<Quaternion> rot)
	{
		GameWorld world = GameWorld.GetGameWorld(base.WorldId);
		if (world == null)
		{
			yield break;
		}
		for (int i = 0; i < buildingItemList.Count(); i++)
		{
			if (!world.ExistedObj(buildingItemList[i].instanceId))
			{
				SceneItem item = SceneObjMgr.Create<SceneItem>();
				item.Init(buildingItemList[i].instanceId, buildingItemList[i].protoId, pos[i], Vector3.one, rot[i], base.WorldId);
				item.SetItem(buildingItemList[i]);
				GameWorld.AddSceneObj(item, base.WorldId);
				SceneObjMgr.Save(item);
				ChannelNetwork.SyncItem(base.WorldId, buildingItemList[i]);
				SyncSceneObject(item);
				yield return null;
			}
		}
	}

	public ItemObject CreateItem(int itemId, int num)
	{
		return CreateItem(itemId, num, syn: false);
	}

	public ItemObject CreateItem(int itemId, int num, bool syn)
	{
		ItemObject itemObject = ItemManager.CreateItem(itemId, num);
		if (itemObject != null && syn)
		{
			SyncItem(itemObject);
		}
		return itemObject;
	}

	public void ItemCountDown(ItemObject itemObj, int num)
	{
		if (itemObj != null)
		{
			itemObj.CountDown(num);
			SyncItem(itemObj);
		}
	}

	private void ValidateArmorData()
	{
		_charcterArmor.ValidateData();
	}

	private void InitArmor()
	{
		_charcterArmor.Init(Package.ItemPack);
	}

	private void InitPackage()
	{
		List<int> list = new List<int>();
		int item = 1292;
		if (ServerConfig.IsStory)
		{
			item = 1358;
		}
		else if (ServerConfig.IsAdventure)
		{
			if (ServerConfig.IsCooperation)
			{
				item = 1290;
			}
			else if (ServerConfig.IsVS)
			{
				item = 1300;
			}
			else if (ServerConfig.IsSurvive)
			{
				item = ((!ServerConfig.UnlimitedRes) ? 1301 : 1305);
			}
		}
		if (!ServerConfig.IsCustom)
		{
			list.Add(item);
		}
		if (list.Count != 0)
		{
			foreach (int item2 in list)
			{
				ItemObject itemObject = ItemObject.Create(item2, -1);
				if (itemObject == null)
				{
					continue;
				}
				Bundle cmpt = itemObject.GetCmpt<Bundle>();
				if (cmpt != null)
				{
					List<ItemSample> list2 = cmpt.Extract();
					if (list2.Count != 0)
					{
						List<ItemObject> effItems = new List<ItemObject>(list2.Count);
						foreach (ItemSample item3 in list2)
						{
							ItemManager.CreateItems(item3.protoId, item3.stackCount, ref effItems);
						}
						Package.AddItemList(effItems);
					}
				}
				if (ServerConfig.ScriptsAvailable)
				{
					ReplicatorFormula cmpt2 = itemObject.GetCmpt<ReplicatorFormula>();
					if (cmpt2 != null)
					{
						AddFormula(cmpt2);
					}
				}
				MetalScan cmpt3 = itemObject.GetCmpt<MetalScan>();
				if (cmpt3 != null)
				{
					ApplyMetalScan(cmpt3.metalIds);
				}
			}
		}
		_gameMoney = 1000;
	}

	private bool GetColonyBack(ItemObject itemObj)
	{
		ObjNetInterface objNetInterface = ObjNetInterface.Get(itemObj.instanceId);
		if (null == objNetInterface)
		{
			return false;
		}
		ColonyNetwork colonyNetwork = objNetInterface as ColonyNetwork;
		if (!colonyNetwork.GetBack())
		{
			SyncErrorMsg(8000084);
			return false;
		}
		itemObj.GetCmpt<Durability>()?.SetValue(colonyNetwork.runner.Durability);
		itemObj.GetCmpt<LifeLimit>()?.SetValue(colonyNetwork.runner.Durability);
		colonyNetwork.RemoveEntity();
		DropItemManager.DeleteNetworkObj(itemObj.instanceId);
		NetInterface.NetDestroy(objNetInterface);
		return true;
	}

	private void OnUseItemEvent(int instanceId)
	{
		if (this.OnUseItemEventHandler != null)
		{
			this.OnUseItemEventHandler(instanceId);
		}
	}

	private void OnUseItem(int instanceId)
	{
		int customId = SPTerrainEvent.GetCustomId(base.Id);
		ChannelNetwork.SyncChannel(base.WorldId, EPacketType.PT_CustomEvent_UseItem, base.Id, customId, instanceId);
	}

	private void OnPutOutItemEvent(int instanceId)
	{
		if (this.OnPutOutItemEventHandler != null)
		{
			this.OnPutOutItemEventHandler(instanceId);
		}
	}

	private void OnPutOutItem(int instanceId)
	{
		int customId = SPTerrainEvent.GetCustomId(base.Id);
		ChannelNetwork.SyncChannel(base.WorldId, EPacketType.PT_CustomEvent_PutoutItem, base.Id, customId, instanceId);
	}

	private void RPC_C2S_SetShortCut(uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
		int objId = stream.Read<int>(new object[0]);
		int srcIndex = stream.Read<int>(new object[0]);
		int destIndex = stream.Read<int>(new object[0]);
		ItemPlaceType place = stream.Read<ItemPlaceType>(new object[0]);
		SetShortcutKey(objId, srcIndex, destIndex, place);
	}

	private void RPC_C2S_GetAllItem(uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
		int num = stream.Read<int>(new object[0]);
		AiObject aiObject = ObjNetInterface.Get<AiObject>(num);
		if (!(null != aiObject))
		{
			return;
		}
		if (aiObject is AiAdNpcNetwork)
		{
			AiAdNpcNetwork aiAdNpcNetwork = aiObject as AiAdNpcNetwork;
			if (null != aiAdNpcNetwork && aiAdNpcNetwork.IsDead && aiAdNpcNetwork.GetDeadObjItem(this))
			{
				aiAdNpcNetwork.SyncPackageIndex(0);
				SyncGetDeadObjAllItems(num);
			}
		}
		else if (aiObject is AiMonsterNetwork)
		{
			AiMonsterNetwork aiMonsterNetwork = aiObject as AiMonsterNetwork;
			if (null != aiMonsterNetwork && aiMonsterNetwork.IsDead)
			{
				aiMonsterNetwork.GetDeadObjItem(this);
				SyncGetDeadObjAllItems(num);
			}
		}
	}

	private void RPC_C2S_GetItem(uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
		int num = stream.Read<int>(new object[0]);
		int index = stream.Read<int>(new object[0]);
		int num2 = stream.Read<int>(new object[0]);
		AiObject aiObject = ObjNetInterface.Get<AiObject>(num);
		if (!(null != aiObject))
		{
			return;
		}
		if (aiObject is AiAdNpcNetwork)
		{
			AiAdNpcNetwork aiAdNpcNetwork = aiObject as AiAdNpcNetwork;
			if (null != aiAdNpcNetwork && aiAdNpcNetwork.IsDead && aiAdNpcNetwork.GetDeadObjItem(this, index, num2))
			{
				aiAdNpcNetwork.SyncPackageIndex(0);
				SyncGetDeadObjItem(num, index, num2);
			}
		}
		else if (aiObject is AiMonsterNetwork)
		{
			AiMonsterNetwork aiMonsterNetwork = aiObject as AiMonsterNetwork;
			if (null != aiMonsterNetwork && aiMonsterNetwork.IsDead && aiMonsterNetwork.GetDeadObjItem(this, index, num2))
			{
				SyncGetDeadObjItem(num, index, num2);
			}
		}
	}

	private void PlayerDropItem(int objId, int count)
	{
		ItemObject itemById = Package.GetItemById(objId);
		if (itemById == null && LogFilter.logDebug)
		{
			Debug.LogError("Invalid item");
		}
		ItemObject itemObject = itemById;
		if (itemById.stackCount == count)
		{
			itemObject = itemById;
			Package.RemoveItem(itemById);
			SyncPackageIndex();
			SyncItem(itemById);
		}
		else if (itemById.stackCount > count)
		{
			itemById.CountDown(1);
			itemObject = ItemManager.CreateFromItem(itemById.protoId, 1, itemById);
			SyncItemList(new ItemObject[2] { itemById, itemObject });
		}
	}

	private void RPC_C2S_PutOutItem(uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
		int num = stream.Read<int>(new object[0]);
		Vector3 vector = stream.Read<Vector3>(new object[0]);
		Vector3 vector2 = stream.Read<Vector3>(new object[0]);
		Quaternion quaternion = stream.Read<Quaternion>(new object[0]);
		ItemObject itemById = Package.GetItemById(num);
		if (itemById == null)
		{
			if (LogFilter.logDebug)
			{
				Debug.LogWarning("Invalid item.");
			}
			return;
		}
		ObjType putOutClass = GetPutOutClass(itemById);
		switch (putOutClass)
		{
		case ObjType.Colony:
			if (!ColonyMgr._Instance.CheckMax(originTeamId, itemById.protoId))
			{
				if (LogFilter.logDebug)
				{
					Debug.LogWarning("Out of limit of number.");
				}
				SyncErrorMsg("Out of limit of number.");
				return;
			}
			if (itemById.protoId == 1127 && !GameWorld.CheckArea(base.WorldId, originTeamId, vector, 377))
			{
				if (LogFilter.logDebug)
				{
					Debug.LogWarning("No authority.");
				}
				SyncErrorMsg("No authority.");
				return;
			}
			break;
		case ObjType.None:
			return;
		case ObjType.AiBeacon:
			if (AiTowerDefense.IsOnlyOneLimit(base.TeamId))
			{
				if (LogFilter.logDebug)
				{
					Debug.LogWarning("One tower defense limited.");
				}
				SyncErrorMsg("One tower defense limited.");
				return;
			}
			break;
		}
		ProcessItemMission(itemById.GetItemProtoId(), itemById.instanceId);
		ItemObject itemObject = itemById;
		if ((ServerConfig.UnlimitedRes || ServerConfig.IsBuild) && !ItemID.IsCreation(num))
		{
			itemObject = ItemManager.CreateFromItem(itemById.protoId, 1, itemById);
			SceneObjMgr.AddItem(itemObject);
		}
		else if (itemById.stackCount == 1)
		{
			itemObject = itemById;
			Package.RemoveItem(itemById);
			SyncPackageIndex();
			ChannelNetwork.SyncItem(101, itemById);
		}
		else
		{
			itemById.CountDown(1);
			itemObject = ItemManager.CreateFromItem(itemById.protoId, 1, itemById);
			ChannelNetwork.SyncItemList(101, new ItemObject[2] { itemById, itemObject });
		}
		OnPutOutItemEvent(num);
		switch (putOutClass)
		{
		case ObjType.Colony:
			NetInterface.Instantiate(PrefabManager.Self.ColonyNetwork, vector, quaternion, base.WorldId, itemObject.instanceId, itemObject.protoId, base.Id, originTeamId);
			GetSite();
			break;
		case ObjType.Creation:
			CreationNetwork.AddLock(this, itemObject.instanceId);
			NetInterface.Instantiate(PrefabManager.Self.CreationNetwork, vector, quaternion, base.WorldId, itemObject.instanceId, base.Id, originTeamId);
			break;
		case ObjType.AiBeacon:
			NetInterface.Instantiate(PrefabManager.Self.AiTowerDefense, vector, quaternion, base.WorldId, itemObject.instanceId, -1, -1, base.Id, base.TeamId);
			break;
		case ObjType.Item:
			if (!GameWorld.CheckArea(base.WorldId, originTeamId, vector, itemById.protoId))
			{
				if (LogFilter.logDebug)
				{
					Debug.LogWarning("No authority.");
				}
				SyncErrorMsg("No authority.");
			}
			else
			{
				SceneItem sceneItem = SceneObjMgr.Create<SceneItem>();
				sceneItem.Init(itemObject.instanceId, itemObject.protoId, vector, vector2, quaternion, base.WorldId);
				sceneItem.SetItem(itemObject);
				GameWorld.AddSceneObj(sceneItem, base.WorldId);
				SceneObjMgr.Save(sceneItem);
				SyncSceneObject(sceneItem);
			}
			break;
		case ObjType.PlantSeed:
		{
			byte terrainType = stream.Read<byte>(new object[0]);
			FarmPlantLogic farmPlantLogic = FarmManager.Instance.GetPlantByItemObjID(itemObject.instanceId);
			if (farmPlantLogic == null)
			{
				farmPlantLogic = FarmManager.Instance.CreatePlant(base.WorldId, itemObject.instanceId, PlantInfo.GetPlantInfoByItemId(itemObject.protoId).mTypeID, vector, quaternion, terrainType);
			}
			farmPlantLogic.mPos = vector;
			RPCOthers(EPacketType.PT_InGame_Plant_PutOut, vector, quaternion, vector2, itemObject.instanceId, farmPlantLogic);
			break;
		}
		case ObjType.DoodadBox:
			DoodadMgr.CreateDoodadWithoutLimit(base.Id, originTeamId, base.WorldId, itemById.instanceId, itemById.instanceId, 201, vector, 3, "0", vector2);
			break;
		case ObjType.Flag:
			break;
		}
	}

	private void RPC_C2S_PutOutTower(uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
		int num = stream.Read<int>(new object[0]);
		Vector3 pos = stream.Read<Vector3>(new object[0]);
		Quaternion rot = stream.Read<Quaternion>(new object[0]);
		ItemObject itemById = Package.GetItemById(num);
		if (itemById == null)
		{
			if (LogFilter.logDebug)
			{
				Debug.LogWarning("Invalid item.");
			}
			return;
		}
		if (!GameWorld.CheckArea(base.WorldId, originTeamId, pos, itemById.protoId))
		{
			if (LogFilter.logDebug)
			{
				Debug.LogWarning("No authority.");
			}
			SyncErrorMsg("No authority.");
			return;
		}
		ItemObject itemObject = itemById;
		ProcessItemMission(itemById.protoId);
		if ((ServerConfig.UnlimitedRes || ServerConfig.IsBuild) && !ItemID.IsCreation(num))
		{
			itemObject = ItemManager.CreateFromItem(itemById.protoId, 1, itemById);
			SceneObjMgr.AddItem(itemObject);
		}
		else if (itemById.stackCount == 1)
		{
			itemObject = itemById;
			Package.RemoveItem(itemById);
			SyncPackageIndex();
			SyncItem(itemById);
		}
		else
		{
			itemById.CountDown(1);
			itemObject = ItemManager.CreateFromItem(itemById.protoId, 1, itemById);
			SyncItemList(new ItemObject[2] { itemById, itemObject });
		}
		OnPutOutItemEvent(num);
		NetInterface.Instantiate(PrefabManager.Self.AiTowerNetworkSeed, pos, rot, base.WorldId, itemObject.instanceId, 1f, base.Id, originTeamId);
	}

	private void RPC_C2S_PutOutFlag(uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
		int num = stream.Read<int>(new object[0]);
		Vector3 pos = stream.Read<Vector3>(new object[0]);
		Quaternion rot = stream.Read<Quaternion>(new object[0]);
		ItemObject itemById = Package.GetItemById(num);
		if (itemById == null)
		{
			if (LogFilter.logDebug)
			{
				Debug.LogWarning("Invalid item.");
			}
			return;
		}
		if (!GameWorld.CheckArea(base.WorldId, originTeamId, pos, itemById.protoId))
		{
			if (LogFilter.logDebug)
			{
				Debug.LogWarning("No authority.");
			}
			SyncErrorMsg("No authority.");
			return;
		}
		ItemObject itemObject = itemById;
		if ((ServerConfig.UnlimitedRes || ServerConfig.IsBuild) && !ItemID.IsCreation(num))
		{
			itemObject = ItemManager.CreateFromItem(itemById.protoId, 1, itemById);
			SceneObjMgr.AddItem(itemObject);
		}
		else if (itemById.stackCount == 1)
		{
			itemObject = itemById;
			Package.RemoveItem(itemById);
			SyncPackageIndex();
			SyncItem(itemById);
		}
		else
		{
			itemById.CountDown(1);
			itemObject = ItemManager.CreateFromItem(itemById.protoId, 1, itemById);
			SyncItemList(new ItemObject[2] { itemById, itemObject });
		}
		OnPutOutItemEvent(num);
		NetInterface.Instantiate(PrefabManager.Self.AiFlagNetwork, pos, rot, base.WorldId, itemObject.instanceId, originTeamId, base.Id, itemObject.protoId);
		GetSite();
		BattleManager.SyncBattleInfo(originTeamId);
	}

	private void RPC_C2S_ProcessItemMission(uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
		stream.TryRead<int>(out var value);
		stream.TryRead<IntVector3[]>(out var value2);
		MissionManager.Manager.GetCurPlayerMission(base.Id)?.ProcessItemMission(value, this, value2.ToList());
		MissionManager.Manager.GetCurTeamMission(base.Id)?.ProcessItemMission(value, this, value2.ToList());
	}

	private void RPC_C2S_UseItem(uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
		int num = stream.Read<int>(new object[0]);
		if (_bDeath)
		{
			return;
		}
		ItemObject itemById = Package.GetItemById(num);
		if (itemById == null)
		{
			if (LogFilter.logDebug)
			{
				Debug.LogWarning("Invalid item.");
			}
			return;
		}
		bool flag = false;
		Bundle cmpt = itemById.GetCmpt<Bundle>();
		if (cmpt != null)
		{
			List<ItemSample> list = cmpt.Extract();
			if (list.Count >= 1)
			{
				if (!Package.CanAdd(list))
				{
					return;
				}
				List<ItemObject> effItems = new List<ItemObject>(list.Count);
				foreach (ItemSample item in list)
				{
					ItemManager.CreateItems(item.protoId, item.stackCount, ref effItems);
				}
				Package.AddItemList(effItems);
				SyncItemList(effItems);
				SyncNewItem(list);
			}
			flag = true;
		}
		Consume cmpt2 = itemById.GetCmpt<Consume>();
		if (cmpt2 != null)
		{
			flag = true;
			RPCOthers(EPacketType.PT_InGame_UseItem, num);
		}
		ReplicatorFormula cmpt3 = itemById.GetCmpt<ReplicatorFormula>();
		if (cmpt3 != null)
		{
			if (AddFormula(cmpt3))
			{
				if (ServerConfig.IsStory)
				{
					SyncTeamFormulaId();
				}
				else
				{
					SyncFormulaId();
				}
			}
			flag = true;
		}
		Equip cmpt4 = itemById.GetCmpt<Equip>();
		if (cmpt4 != null)
		{
			if (!PutOnEquipment(itemById))
			{
				return;
			}
			Package.RemoveItem(itemById);
			SyncPutOnEquipment(new ItemObject[1] { itemById });
		}
		MetalScan cmpt5 = itemById.GetCmpt<MetalScan>();
		if (cmpt5 != null && ApplyMetalScan(cmpt5.metalIds))
		{
			if (ServerConfig.IsStory)
			{
				SyncTeamMetalScan();
			}
			else
			{
				SyncMetalScan();
			}
			flag = true;
		}
		if (flag)
		{
			if (!ServerConfig.UnlimitedRes)
			{
				if (!itemById.CountDown(1))
				{
					Package.DeleteItem(itemById);
				}
				else
				{
					SyncItem(itemById);
				}
			}
			OnUseItemEvent(num);
		}
		SyncPackageIndex();
		ProcessItemMission(itemById.protoId);
	}

	public void ProcessItemMission(int protoId, int objId = 0)
	{
		PlayerMission curPlayerMission = MissionManager.Manager.GetCurPlayerMission(base.Id);
		if (curPlayerMission != null)
		{
			List<IntVector3> list = new List<IntVector3>();
			IntVector3 item = new IntVector3(base.transform.position);
			list.Add(item);
			curPlayerMission.ProcessItemMission(protoId, this, list, objId);
		}
		curPlayerMission = MissionManager.Manager.GetCurTeamMission(base.Id);
		if (curPlayerMission != null)
		{
			List<IntVector3> list2 = new List<IntVector3>();
			IntVector3 item2 = new IntVector3(base.transform.position);
			list2.Add(item2);
			curPlayerMission.ProcessItemMission(protoId, this, list2, objId);
		}
	}

	public void ProcessMonsterDead(int protoId)
	{
		MissionManager.Manager.GetCurPlayerMission(base.Id)?.ProcessMonsterDead(protoId, this);
		MissionManager.Manager.GetCurTeamMission(base.Id)?.ProcessMonsterDead(protoId, this);
	}

	public bool HasMission(int missionId)
	{
		PlayerMission curPlayerMission = MissionManager.Manager.GetCurPlayerMission(base.Id);
		if (curPlayerMission != null && curPlayerMission.HasMission(missionId))
		{
			return true;
		}
		curPlayerMission = MissionManager.Manager.GetCurTeamMission(base.Id);
		if (curPlayerMission != null && curPlayerMission.HasMission(missionId))
		{
			return true;
		}
		return false;
	}

	public bool HadCompleteMission(int missionId)
	{
		PlayerMission curPlayerMission = MissionManager.Manager.GetCurPlayerMission(base.Id);
		if (curPlayerMission != null && curPlayerMission.HadCompleteMission(missionId, this))
		{
			return true;
		}
		curPlayerMission = MissionManager.Manager.GetCurTeamMission(base.Id);
		if (curPlayerMission != null && curPlayerMission.HadCompleteMission(missionId, this))
		{
			return true;
		}
		return false;
	}

	private void RPC_C2S_GetItemListBack(uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
		int num = stream.Read<int>(new object[0]);
		ItemSample[] items = stream.Read<ItemSample[]>(new object[0]);
		ItemObject itemByID = ItemManager.GetItemByID(num);
		if (itemByID == null || itemByID.protoData == null)
		{
			if (LogFilter.logDebug)
			{
				Debug.LogWarningFormat("Item[{0}] does not exist.", num);
			}
			return;
		}
		ObjType putOutClass = GetPutOutClass(itemByID);
		ObjType objType = putOutClass;
		if (objType != ObjType.Item || GameWorld.DelSceneObj(num, base.WorldId))
		{
			ItemManager.RemoveItem(num);
			ItemObject[] array = Package.AddSameItems(items);
			if (array != null)
			{
				SyncItemList(array);
				SyncNewItem(items);
			}
			SyncPackageIndex();
			RPCOthers(EPacketType.PT_InGame_GetItemBack, num);
		}
	}

	private void RPC_C2S_GetLootItemBack(uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
		int num = stream.Read<int>(new object[0]);
		bool flag = stream.Read<bool>(new object[0]);
		ItemObject itemByID = ItemManager.GetItemByID(num);
		if (itemByID == null || itemByID.protoData == null)
		{
			if (LogFilter.logDebug)
			{
				Debug.LogWarningFormat("Item[{0}] does not exist.", num);
			}
		}
		else
		{
			if (!GameWorld.DelSceneObj(num, base.WorldId))
			{
				return;
			}
			if (!flag)
			{
				if (itemByID.protoData.maxStackNum != 1 && !ItemID.IsCreation(num))
				{
					List<ItemObject> effItems = new List<ItemObject>();
					Package.AddSameItems(itemByID.protoId, itemByID.stackCount, ref effItems);
					SyncItemList(effItems.ToArray());
					ItemManager.RemoveItem(num);
				}
				else
				{
					Package.AddItem(itemByID);
				}
				SyncPackageIndex();
			}
			SyncNewItem(itemByID);
			RPCOthers(EPacketType.PT_InGame_GetLootItemBack, num);
		}
	}

	private void RPC_C2S_PreGetItemBack(uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
		int num = stream.Read<int>(new object[0]);
		SkNetworkInterface skNetworkInterface = ObjNetInterface.Get<SkNetworkInterface>(num);
		if (!(null == skNetworkInterface) && !ForceSetting.Ally(skNetworkInterface.TeamId, base.TeamId))
		{
			RPCOthers(EPacketType.PT_InGame_PreGetItemBack, num);
		}
	}

	private void RPC_C2S_GetItemBack(uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
		int num = stream.Read<int>(new object[0]);
		ItemObject itemByID = ItemManager.GetItemByID(num);
		if (itemByID == null || itemByID.protoData == null)
		{
			if (LogFilter.logDebug)
			{
				Debug.LogWarningFormat("Item[{0}] does not exist.", num);
			}
			return;
		}
		switch (GetPutOutClass(itemByID))
		{
		case ObjType.Item:
			if (!GameWorld.DelSceneObj(num, base.WorldId))
			{
				return;
			}
			break;
		case ObjType.Colony:
			if (!GetColonyBack(itemByID))
			{
				return;
			}
			break;
		case ObjType.Creation:
		{
			CreationNetwork creationNetwork = ObjNetInterface.Get<CreationNetwork>(num);
			if (null == creationNetwork || creationNetwork.Controller != null)
			{
				return;
			}
			if (ForceSetting.Conflict(creationNetwork.TeamId, base.Id))
			{
				SyncErrorMsg(82209000);
				return;
			}
			if (CreationNetwork.IsLock(this, num))
			{
				return;
			}
			DropItemManager.DeleteNetworkObj(num);
			NetInterface.NetDestroy(creationNetwork);
			break;
		}
		case ObjType.Tower:
		{
			AiTowerNetwork aiTowerNetwork = ObjNetInterface.Get<AiTowerNetwork>(num);
			if (null == aiTowerNetwork)
			{
				return;
			}
			DropItemManager.DeleteNetworkObj(num);
			NetInterface.NetDestroy(aiTowerNetwork);
			break;
		}
		case ObjType.Flag:
		{
			AiFlagNetwork aiFlagNetwork = ObjNetInterface.Get<AiFlagNetwork>(num);
			if (null == aiFlagNetwork)
			{
				return;
			}
			aiFlagNetwork.RemoveFlag();
			DropItemManager.DeleteNetworkObj(num);
			NetInterface.NetDestroy(aiFlagNetwork);
			break;
		}
		case ObjType.AiBeacon:
		{
			AiTowerDefense aiTowerDefense = ObjNetInterface.Get<AiTowerDefense>(num);
			if (null == aiTowerDefense)
			{
				return;
			}
			NetInterface.NetDestroy(aiTowerDefense);
			break;
		}
		}
		if ((ServerConfig.UnlimitedRes || ServerConfig.IsBuild) && !ItemID.IsCreation(num))
		{
			ItemManager.RemoveItem(itemByID.instanceId);
		}
		else
		{
			Package.AddItem(itemByID);
			SyncPackageIndex();
		}
		RPCOthers(EPacketType.PT_InGame_GetItemBack, num);
	}

	private void RPC_C2S_Turn(uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
		int num = stream.Read<int>(new object[0]);
		ItemObject itemByID = ItemManager.GetItemByID(num);
		if (itemByID == null)
		{
			return;
		}
		Quaternion quaternion = Quaternion.Euler(0f, 0f, 0f);
		switch (GetPutOutClass(itemByID))
		{
		case ObjType.Item:
		{
			SceneObject sceneObj = GameWorld.GetSceneObj(num, base.WorldId);
			if (sceneObj == null)
			{
				return;
			}
			sceneObj.Rot *= Quaternion.Euler(0f, 90f, 0f);
			quaternion = sceneObj.Rot;
			SceneObjMgr.Save(sceneObj);
			break;
		}
		case ObjType.Colony:
		{
			ColonyNetwork colonyNetwork = ObjNetInterface.Get<ColonyNetwork>(num);
			if (!(null == colonyNetwork))
			{
				colonyNetwork.transform.rotation *= Quaternion.Euler(0f, 90f, 0f);
				colonyNetwork.SyncSave();
				colonyNetwork.RPCOthers(EPacketType.PT_CL_Turn, colonyNetwork.transform.rotation);
			}
			return;
		}
		case ObjType.Creation:
		{
			CreationNetwork creationNetwork = ObjNetInterface.Get<CreationNetwork>(num);
			if (null == creationNetwork)
			{
				return;
			}
			creationNetwork.transform.rotation *= Quaternion.Euler(0f, 90f, 0f);
			quaternion = creationNetwork.transform.rotation;
			creationNetwork.SyncSave();
			break;
		}
		case ObjType.Tower:
		{
			AiTowerNetwork aiTowerNetwork = ObjNetInterface.Get<AiTowerNetwork>(num);
			if (null == aiTowerNetwork)
			{
				return;
			}
			aiTowerNetwork.transform.rotation *= Quaternion.Euler(0f, 90f, 0f);
			quaternion = aiTowerNetwork.transform.rotation;
			aiTowerNetwork.SyncSave();
			break;
		}
		case ObjType.Flag:
		{
			AiFlagNetwork aiFlagNetwork = ObjNetInterface.Get<AiFlagNetwork>(num);
			if (null == aiFlagNetwork)
			{
				return;
			}
			aiFlagNetwork.transform.rotation *= Quaternion.Euler(0f, 90f, 0f);
			quaternion = aiFlagNetwork.transform.rotation;
			aiFlagNetwork.SyncSave();
			break;
		}
		case ObjType.PlantSeed:
		{
			FarmPlantLogic plantByItemObjID = FarmManager.Instance.GetPlantByItemObjID(num);
			if (plantByItemObjID == null)
			{
				return;
			}
			plantByItemObjID.mRot *= Quaternion.Euler(0f, 90f, 0f);
			quaternion = plantByItemObjID.mRot;
			break;
		}
		}
		RPCOthers(EPacketType.PT_InGame_Turn, num, quaternion);
	}

	[Obsolete]
	private void RPC_C2S_GetColonyBack(uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
	}

	private void RPC_C2S_GetOnVehicle(uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
		int id = stream.Read<int>(new object[0]);
		if (!(null != base._Creation))
		{
			CreationNetwork creationNetwork = ObjNetInterface.Get<CreationNetwork>(id);
			if (!(null == creationNetwork) && !ForceSetting.Conflict(creationNetwork.TeamId, base.Id))
			{
				GetOnVehicle(creationNetwork);
			}
		}
	}

	private void RPC_C2S_GetOffVehicle(uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
		if (base._OnCar)
		{
			Vector3 outPos = stream.Read<Vector3>(new object[0]);
			GetOffVehicle(outPos);
		}
	}

	private void RPC_C2S_RepairVehicle(uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
		int id = stream.Read<int>(new object[0]);
		if (null != base._Creation)
		{
			return;
		}
		CreationNetwork creationNetwork = ObjNetInterface.Get<CreationNetwork>(id);
		if (null == creationNetwork || ForceSetting.Conflict(creationNetwork.TeamId, base.Id))
		{
			return;
		}
		if (Vector3.SqrMagnitude(base.transform.position - creationNetwork.transform.position) > 25f)
		{
			if (LogFilter.logDebug)
			{
				Debug.LogWarning("Too far.");
			}
			SyncErrorMsg("Too far.");
		}
		else if (RepairVehicle(creationNetwork))
		{
			RPCPeer(info.sender, EPacketType.PT_InGame_RepairVehicle, creationNetwork.Id);
		}
	}

	private void RPC_C2S_ChargeVehicle(uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
		int id = stream.Read<int>(new object[0]);
		if (null != base._Creation)
		{
			return;
		}
		CreationNetwork creationNetwork = ObjNetInterface.Get<CreationNetwork>(id);
		if (null == creationNetwork || ForceSetting.Conflict(creationNetwork.TeamId, base.Id))
		{
			return;
		}
		if (Vector3.SqrMagnitude(base.transform.position - creationNetwork.transform.position) > 25f)
		{
			if (LogFilter.logDebug)
			{
				Debug.LogWarning("Too far.");
			}
			SyncErrorMsg("Too far.");
		}
		else
		{
			ChargeVehicle(creationNetwork);
		}
	}

	private void RPC_C2S_TowerRefill(uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
		stream.TryRead<int>(out var value);
		stream.TryRead<int>(out var value2);
		stream.TryRead<int>(out var value3);
		ItemObject itemByID = ItemManager.GetItemByID(value);
		List<ItemObject> effItems = new List<ItemObject>(10);
		effItems.Add(itemByID);
		Package.RemoveItem(value3, value2, ref effItems);
	}

	private void RPC_C2S_BuildBlockRedo(uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
		GameWorld gameWorld = GameWorld.GetGameWorld(base.WorldId);
		if (gameWorld == null)
		{
			return;
		}
		if (ServerAdministrator.IsBuildLock(base.Id))
		{
			if (LogFilter.logDebug)
			{
				Debug.LogWarning("You are denied to build.");
			}
			SyncErrorMsg("You are denied to build.");
			return;
		}
		byte[] buff = stream.Read<byte[]>(new object[0]);
		Dictionary<IntVector3, BSVoxel> oldVoxels = new Dictionary<IntVector3, BSVoxel>();
		Dictionary<IntVector3, BSVoxel> newVoxels = new Dictionary<IntVector3, BSVoxel>();
		Dictionary<int, int> dictionary = new Dictionary<int, int>();
		int opType = 0;
		int mode = 0;
		int dsType = 0;
		float scale = 0f;
		Serialize.Import(buff, delegate(BinaryReader r)
		{
			opType = BufferHelper.ReadInt32(r);
			mode = BufferHelper.ReadInt32(r);
			dsType = BufferHelper.ReadInt32(r);
			scale = BufferHelper.ReadSingle(r);
			int num = BufferHelper.ReadInt32(r);
			for (int i = 0; i < num; i++)
			{
				IntVector3 key = BufferHelper.ReadIntVector3(r);
				BufferHelper.ReadBSVoxel(r, out var _value);
				BufferHelper.ReadBSVoxel(r, out var _value2);
				oldVoxels[key] = _value;
				newVoxels[key] = _value2;
			}
		});
		bool flag = true;
		int num2 = 1;
		if (mode == 0)
		{
			foreach (KeyValuePair<IntVector3, BSVoxel> item3 in newVoxels)
			{
				int num3 = 0;
				if (dsType == 0)
				{
					num3 = BSVoxelMatMap.GetItemID(item3.Value.materialType);
				}
				else if (dsType == 1)
				{
					if (item3.Value.IsExtendable())
					{
						if (!item3.Value.IsExtendableRoot())
						{
							num3 = BSBlockMatMap.GetBlockItemProtoID((byte)(item3.Value.materialType >> 2));
							num2 = 1;
						}
						else
						{
							num2 = 0;
						}
					}
					else
					{
						num3 = BSBlockMatMap.GetBlockItemProtoID(item3.Value.materialType);
					}
				}
				if (num3 <= 0)
				{
					continue;
				}
				if (num3 != 0)
				{
					if (dictionary.ContainsKey(num3))
					{
						Dictionary<int, int> dictionary2;
						Dictionary<int, int> dictionary3 = (dictionary2 = dictionary);
						int key2;
						int key3 = (key2 = num3);
						key2 = dictionary2[key2];
						dictionary3[key3] = key2 + num2;
					}
					else
					{
						dictionary.Add(num3, num2);
					}
				}
				PlayerMission curPlayerMission = MissionManager.Manager.GetCurPlayerMission(base.Id);
				if (curPlayerMission != null)
				{
					List<IntVector3> list = new List<IntVector3>();
					IntVector3 item = new IntVector3((float)item3.Key.x * scale, (float)item3.Key.y * scale, (float)item3.Key.z * scale);
					list.Add(item);
					curPlayerMission.ProcessItemMission(BSBlockMatMap.GetBlockItemProtoID(item3.Value.materialType), this, list.ToList());
				}
				curPlayerMission = MissionManager.Manager.GetCurTeamMission(base.Id);
				if (curPlayerMission != null)
				{
					List<IntVector3> list2 = new List<IntVector3>();
					IntVector3 item2 = new IntVector3((float)item3.Key.x * scale, (float)item3.Key.y * scale, (float)item3.Key.z * scale);
					list2.Add(item2);
					curPlayerMission.ProcessItemMission(BSBlockMatMap.GetBlockItemProtoID(item3.Value.materialType), this, list2.ToList());
				}
			}
			if (!ServerConfig.UnlimitedRes)
			{
				float num4 = 1f;
				if (dsType == 1)
				{
					num4 = 4f;
				}
				foreach (KeyValuePair<int, int> item4 in dictionary)
				{
					if (Package.GetItemCount(item4.Key) < Mathf.CeilToInt((float)item4.Value / num4))
					{
						flag = false;
					}
				}
				if (flag)
				{
					List<ItemObject> effItems = new List<ItemObject>();
					foreach (KeyValuePair<int, int> item5 in dictionary)
					{
						Package.RemoveItem(item5.Key, Mathf.CeilToInt((float)item5.Value / num4), ref effItems);
					}
					SyncItemList(effItems);
					SyncPackageIndex();
				}
			}
		}
		else if (opType == 0 || opType == 1)
		{
			foreach (BSVoxel value in oldVoxels.Values)
			{
				int num5 = 0;
				if (dsType == 0)
				{
					if (!BSBlockMatMap.VoxelIsZero(value, 1f))
					{
						num5 = BSVoxelMatMap.GetItemID(value.materialType);
					}
				}
				else if (dsType == 1)
				{
					if (value.IsExtendable())
					{
						if (!value.IsExtendableRoot())
						{
							num5 = BSBlockMatMap.GetBlockItemProtoID((byte)(value.materialType >> 2));
							num2 = 1;
						}
						else
						{
							num2 = 0;
						}
					}
					else if (!BSBlockMatMap.BlockIsZero(value, 1f))
					{
						num5 = BSBlockMatMap.GetBlockItemProtoID(value.materialType);
					}
				}
				if (num5 > 0 && num5 != 0)
				{
					if (dictionary.ContainsKey(num5))
					{
						Dictionary<int, int> dictionary4;
						Dictionary<int, int> dictionary5 = (dictionary4 = dictionary);
						int key2;
						int key4 = (key2 = num5);
						key2 = dictionary4[key2];
						dictionary5[key4] = key2 + num2;
					}
					else
					{
						dictionary.Add(num5, num2);
					}
				}
			}
			if (!ServerConfig.UnlimitedRes)
			{
				float divisor = 1f;
				if (dsType == 1)
				{
					divisor = 4f;
				}
				ItemSample[] items = dictionary.Select((KeyValuePair<int, int> iter) => new ItemSample(iter.Key, Mathf.FloorToInt((float)iter.Value / divisor))).ToArray();
				flag = Package.CanAdd(items);
				if (flag)
				{
					ItemObject[] items2 = Package.AddSameItems(items);
					SyncItemList(items2);
					SyncPackageIndex();
					SyncNewItem(items);
				}
			}
		}
		if (flag)
		{
			gameWorld.ApplyData(dsType, newVoxels);
		}
	}

	private void RPC_C2S_AddItem(uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
		stream.TryRead<int>(out var value);
		stream.TryRead<int>(out var value2);
		List<ItemObject> effItems = new List<ItemObject>(10);
		Package.AddSameItems(value, value2, ref effItems);
		SyncItemList(effItems);
		SyncPackageIndex();
		SyncNewItem(new ItemSample[1]
		{
			new ItemSample(value, value2)
		});
	}

	private void RPC_C2S_MoveNpc(uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
		stream.TryRead<int>(out var value);
		stream.TryRead<Vector3>(out var value2);
		AiAdNpcNetwork aiAdNpcNetwork = ObjNetInterface.Get<AiAdNpcNetwork>(value);
		if (!(aiAdNpcNetwork == null))
		{
			aiAdNpcNetwork.transform.position = value2;
		}
	}

	private void RPC_C2S_AddTestEquipment(uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
		stream.TryRead<int>(out var value);
		RPCOthers(EPacketType.PT_Test_PutOnEquipment, value);
	}

	private void RPC_C2S_SplitItem(uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
		int objId = stream.Read<int>(new object[0]);
		int num = stream.Read<int>(new object[0]);
		ItemObject itemById = Package.GetItemById(objId);
		if (num >= itemById.stackCount)
		{
			return;
		}
		if (Package.GetEmptyGridCount(itemById.protoData) <= 0)
		{
			if (LogFilter.logDebug)
			{
				Debug.LogWarningFormat("Player[{0}] has not enough empty place.", roleName);
			}
		}
		else
		{
			itemById.CountDown(num);
			ItemObject itemObject = ItemManager.CreateFromItem(itemById.protoId, num, itemById);
			int index = Package.AddItem(itemObject);
			SyncItemList(new ItemObject[2] { itemById, itemObject });
			SyncSplitItemFromPackage(itemObject.instanceId, index);
		}
	}

	private void RPC_C2S_DeleteItem(uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
		stream.TryRead<int>(out var value);
		stream.TryRead<int>(out var _);
		stream.TryRead<int>(out var _);
		ItemObject itemById = Package.GetItemById(value);
		if (itemById != null)
		{
			Package.DeleteItem(itemById);
			SyncPackageIndex();
		}
	}

	private void RPC_C2S_ExchangeItem(uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
		int objId = stream.Read<int>(new object[0]);
		int num = stream.Read<int>(new object[0]);
		int num2 = stream.Read<int>(new object[0]);
		ItemObject itemById = Package.GetItemById(objId);
		if (itemById == null)
		{
			return;
		}
		int itemIndex = Package.GetItemIndex(itemById);
		if (itemIndex == num)
		{
			int num3 = -1;
			ItemObject itemByIndex = Package.GetItemByIndex(num2, itemById.protoData);
			if (itemByIndex == null)
			{
				Package.SetItem(itemById, num2, itemById.protoData.tabIndex, itemById.protoData.category);
				Package.SetItem(null, num, itemById.protoData.tabIndex, itemById.protoData.category);
			}
			else if (itemById.protoId != itemByIndex.protoId)
			{
				Package.SetItem(itemById, num2, itemById.protoData.tabIndex, itemById.protoData.category);
				Package.SetItem(itemByIndex, num, itemById.protoData.tabIndex, itemById.protoData.category);
				num3 = itemByIndex.instanceId;
			}
			else if (itemByIndex.MaxStackNum >= itemByIndex.stackCount + itemById.stackCount)
			{
				itemByIndex.CountUp(itemById.stackCount);
				Package.RemoveItem(itemById);
				SyncItem(itemByIndex);
			}
			else
			{
				int num4 = itemByIndex.MaxStackNum - itemByIndex.stackCount;
				itemByIndex.CountUp(num4);
				itemById.CountDown(num4);
				SyncItemList(new ItemObject[2] { itemById, itemByIndex });
			}
			SyncPackageIndex();
		}
	}

	private void RPC_C2S_SortPackage(uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
		int num = stream.Read<int>(new object[0]);
		if (num != -1)
		{
			ItemObject[] items = Package.Sort(num);
			SyncItemList(items);
		}
		else
		{
			ItemObject[] items2 = Package.MissionPackageSort(0);
			SyncItemList(items2);
		}
		SyncPackageIndex();
	}

	private void RPC_C2S_RemoveNewFlag(uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
	}

	private void RPC_C2S_CreateBuildingWithItem(uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
		stream.TryRead<BuildingID>(out var value);
		stream.TryRead<CreatItemInfo[]>(out var value2);
		stream.TryRead<Vector3>(out var value3);
		stream.TryRead<int>(out var value4);
		stream.TryRead<int>(out var value5);
		if (LogFilter.logDebug)
		{
			Debug.Log("GetRPC:GreateBuilding:" + value);
		}
		if (BuildingInfoManager.Instance.GeneratedBuildings.ContainsKey(value))
		{
			if (LogFilter.logDebug)
			{
				Debug.Log("mCreatedNpcItemBuildingIndex containsKey! " + value);
			}
			return;
		}
		createBuildingBlock(value3, value4, value5);
		if (LogFilter.logDebug)
		{
			Debug.Log("CreateBuilding:" + value);
		}
		RPCOthers(EPacketType.PT_InGame_CreateBuilding, value, value3, value4, value5);
		List<ItemObject> list = new List<ItemObject>();
		List<Vector3> list2 = new List<Vector3>();
		List<Quaternion> list3 = new List<Quaternion>();
		CreatItemInfo[] array = value2;
		foreach (CreatItemInfo creatItemInfo in array)
		{
			ItemObject item = ItemManager.CreateItem(creatItemInfo.mItemId, 1);
			list.Add(item);
			list2.Add(creatItemInfo.mPos);
			list3.Add(creatItemInfo.mRotation);
		}
		SyncItemList(list.ToArray());
		StartCoroutine(createBuildingItems(list, list2, list3));
		BuildingInfoManager.Instance.GeneratedBuildings.Add(value, 0);
	}

	private void RPC_C2S_ItemAttrChanged(uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
		int id = stream.Read<int>(new object[0]);
		int itemObjId = stream.Read<int>(new object[0]);
		float num = stream.Read<float>(new object[0]);
		int protoId = stream.Read<int>(new object[0]);
		SkNetworkInterface skNetworkInterface = ObjNetInterface.Get<SkNetworkInterface>(id);
		if (null != skNetworkInterface)
		{
			skNetworkInterface.ItemAttrChange(itemObjId, num);
		}
		ProcessItemMission(protoId);
	}

	private static void RPC_C2S_EquipItemCost(uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
		int id = stream.Read<int>(new object[0]);
		int itemObjId = stream.Read<int>(new object[0]);
		float num = stream.Read<float>(new object[0]);
		SkNetworkInterface skNetworkInterface = ObjNetInterface.Get<SkNetworkInterface>(id);
		if (null != skNetworkInterface)
		{
			skNetworkInterface.EquipItemCost(itemObjId, num);
		}
	}

	private void RPC_C2S_WeaponReload(uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
		int id = stream.Read<int>(new object[0]);
		int objId = stream.Read<int>(new object[0]);
		int oldProtoId = stream.Read<int>(new object[0]);
		int newProtoId = stream.Read<int>(new object[0]);
		float magazineSize = stream.Read<float>(new object[0]);
		SkNetworkInterface skNetworkInterface = ObjNetInterface.Get<SkNetworkInterface>(id);
		if (null != skNetworkInterface)
		{
			if (skNetworkInterface is AiTowerNetwork)
			{
				(skNetworkInterface as AiTowerNetwork).WeaponReload(this, objId, oldProtoId, newProtoId, magazineSize);
			}
			else
			{
				skNetworkInterface.WeaponReload(objId, oldProtoId, newProtoId, magazineSize);
			}
		}
	}

	private void RPC_C2S_MountsItemCost(uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
		int id = stream.Read<int>(new object[0]);
		int num = stream.Read<int>(new object[0]);
		SkNetworkInterface skNetworkInterface = ObjNetInterface.Get<SkNetworkInterface>(id);
		if (!(null != skNetworkInterface))
		{
		}
	}

	private static void RPC_C2S_GunEnergyReload(uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
		int id = stream.Read<int>(new object[0]);
		int objectId = stream.Read<int>(new object[0]);
		float num = stream.Read<float>(new object[0]);
		SkNetworkInterface skNetworkInterface = ObjNetInterface.Get<SkNetworkInterface>(id);
		if (!(null != skNetworkInterface))
		{
			return;
		}
		ItemObject itemByID = ItemManager.GetItemByID(objectId);
		if (itemByID != null)
		{
			skNetworkInterface.GunEnergyReload(itemByID, num);
			if (itemByID.changedFlag)
			{
				ChannelNetwork.SyncItem(skNetworkInterface.WorldId, itemByID);
			}
		}
	}

	private static void RPC_C2S_BatteryEnergyReload(uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
		int id = stream.Read<int>(new object[0]);
		int objectId = stream.Read<int>(new object[0]);
		float num = stream.Read<float>(new object[0]);
		SkNetworkInterface skNetworkInterface = ObjNetInterface.Get<SkNetworkInterface>(id);
		if (null == skNetworkInterface)
		{
			return;
		}
		ItemObject itemByID = ItemManager.GetItemByID(objectId);
		if (itemByID != null)
		{
			skNetworkInterface.BatteryEnergyReload(itemByID, num);
			if (itemByID.changedFlag)
			{
				ChannelNetwork.SyncItem(skNetworkInterface.WorldId, itemByID);
			}
		}
	}

	private static void RPC_C2S_JetPackEnergyReload(uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
		int id = stream.Read<int>(new object[0]);
		int objectId = stream.Read<int>(new object[0]);
		float num = stream.Read<float>(new object[0]);
		SkNetworkInterface skNetworkInterface = ObjNetInterface.Get<SkNetworkInterface>(id);
		if (!(null != skNetworkInterface))
		{
			return;
		}
		ItemObject itemByID = ItemManager.GetItemByID(objectId);
		if (itemByID != null)
		{
			skNetworkInterface.JetPackEnergyReload(itemByID, num);
			if (itemByID.changedFlag)
			{
				ChannelNetwork.SyncItem(skNetworkInterface.WorldId, itemByID);
			}
		}
	}

	private static void RPC_C2S_PackageItemCost(uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
		int id = stream.Read<int>(new object[0]);
		int itemObjId = stream.Read<int>(new object[0]);
		float num = stream.Read<float>(new object[0]);
		SkNetworkInterface skNetworkInterface = ObjNetInterface.Get<SkNetworkInterface>(id);
		if (null != skNetworkInterface)
		{
			skNetworkInterface.PackageItemCost(itemObjId, num);
		}
	}

	private static void RPC_C2S_WeaponDurability(uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
		int id = stream.Read<int>(new object[0]);
		int num = stream.Read<int>(new object[0]);
		ItemObject itemByID = ItemManager.GetItemByID(num);
		if (itemByID != null)
		{
			SkNetworkInterface skNetworkInterface = ObjNetInterface.Get<SkNetworkInterface>(id);
			if (null != skNetworkInterface)
			{
				skNetworkInterface.WeaponDurabilityChange(itemByID);
				Durability cmpt = itemByID.GetCmpt<Durability>();
				ChannelNetwork.SyncChannel(skNetworkInterface.WorldId, EPacketType.PT_InGame_WeaponDurability, cmpt.floatValue.current, num);
			}
		}
	}

	private void RPC_C2S_ArmorDurability(uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
		int num = stream.Read<int>(new object[0]);
		int[] array = stream.Read<int[]>(new object[0]);
		float damage = stream.Read<float>(new object[0]);
		float[] array2 = new float[array.Length];
		SkNetworkInterface skNetworkInterface = ObjNetInterface.Get<SkNetworkInterface>(num);
		if (null != skNetworkInterface)
		{
			int num2 = 0;
			int[] array3 = array;
			foreach (int itemObjId in array3)
			{
				array2[num2] = skNetworkInterface.ArmorDurabilityChange(itemObjId, damage);
				num2++;
			}
			RPCOthers(EPacketType.PT_InGame_ArmorDurability, num, array, array2);
		}
	}

	private void RPC_C2S_MissionMoveAircraft(uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
		if (stream.Read<bool>(new object[0]))
		{
			CreationNetwork.MoveAircraft();
		}
		else
		{
			CreationNetwork.ReturnAircraft();
		}
	}

	private void InitEquip()
	{
		int[] array = (ServerConfig.IsStory ? ((sex != 1) ? new int[3] { 113, 149, 210 } : new int[3] { 95, 131, 192 }) : ((sex != 1) ? new int[4] { 33, 113, 149, 210 } : new int[4] { 33, 95, 131, 192 }));
		List<ItemObject> effItems = new List<ItemObject>(array.Length);
		ItemManager.CreateItems(array, ref effItems);
		foreach (ItemObject item in effItems)
		{
			PutOnEquipment(item);
		}
	}

	public int EquipNotBindCount()
	{
		return EquipModule.EquipItems.Count((ItemObject iter) => !iter.bind);
	}

	public void SyncFormulaId()
	{
		IEnumerable<int> source = ForumlaList.Select((Replicator.KnownFormula iter) => iter?.id ?? (-1));
		RPCOwner(EPacketType.PT_InGame_MergeSkillList, source.ToArray());
	}

	public void SyncTeamFormulaId()
	{
		IEnumerable<int> source = ForumlaList.Select((Replicator.KnownFormula iter) => iter?.id ?? (-1));
		SyncGroupData(EPacketType.PT_InGame_MergeSkillList, source.ToArray());
	}

	public void SyncPutOnEquipment(ItemObject[] equips)
	{
		if (equips.Length > 0)
		{
			RPCOthers(EPacketType.PT_InGame_PutOnEquipment, equips, false);
		}
	}

	public void SyncTakeOffEquipment(int[] equipIds)
	{
		if (equipIds.Length > 0)
		{
			RPCOthers(EPacketType.PT_InGame_TakeOffEquipment, equipIds);
		}
	}

	public void SyncExchangeItem(int srcId, int destIndex, int destId, int srcIndex)
	{
		RPCOwner(EPacketType.PT_InGame_ExchangeItem, srcId, destIndex, destId, srcIndex);
	}

	public void SyncEquipedItems(ItemObject[] items)
	{
		if (items.Length > 0)
		{
			RPCOthers(EPacketType.PT_InGame_EquipedItem, items, false);
		}
	}

	public void SyncEquipedItems(uLink.NetworkPlayer peer)
	{
		if (EquipModule.EquipCount >= 1)
		{
			RPCPeer(peer, EPacketType.PT_InGame_EquipedItem, EquipModule.EquipItems, false);
		}
	}

	internal int GetItemNum(int ItemID)
	{
		return Package.GetItemCount(ItemID);
	}

	public bool RemoveEquipment(ItemObject item)
	{
		return EquipModule.Remove(item);
	}

	public bool AddEquipment(ItemObject item)
	{
		return EquipModule.Add(item);
	}

	public bool PutOnEquipment(ItemObject item)
	{
		if (item == null)
		{
			return false;
		}
		Equip cmpt = item.GetCmpt<Equip>();
		if (cmpt == null)
		{
			return false;
		}
		if (cmpt.sex != 0 && cmpt.sex != (PeSex)sex)
		{
			return false;
		}
		int num = EquipModule.FindEffectEquipCount(item);
		if (Package.GetEmptyGridCount(item.protoData) < num)
		{
			return false;
		}
		List<ItemObject> effEquips = new List<ItemObject>();
		if (EquipModule.PutOnEquip(item, ref effEquips))
		{
			Package.AddItemList(effEquips);
		}
		return true;
	}

	public void TakeOffEquipment(ItemObject item)
	{
		if (item != null)
		{
			int num = EquipModule.FindEffectEquipCount(item);
			if (Package.GetEmptyGridCount(item.protoData) >= num)
			{
				List<ItemObject> effEquips = new List<ItemObject>();
				EquipModule.TakeOffEquip(item, ref effEquips);
				Package.AddItemList(effEquips);
			}
		}
	}

	private void WriteShortcut(BinaryWriter w)
	{
		w.Write(_shortcutKeys.Count);
		foreach (KeyValuePair<int, int> shortcutKey in _shortcutKeys)
		{
			BufferHelper.Serialize(w, shortcutKey.Key);
			BufferHelper.Serialize(w, shortcutKey.Value);
		}
	}

	private void WriteArmorInfo(BinaryWriter w)
	{
		if (_charcterArmor != null)
		{
			_charcterArmor.Serialize(w);
		}
	}

	private void ReadShortcut(BinaryReader r)
	{
		int num = BufferHelper.ReadInt32(r);
		for (int i = 0; i < num; i++)
		{
			int key = BufferHelper.ReadInt32(r);
			int value = BufferHelper.ReadInt32(r);
			_shortcutKeys[key] = value;
		}
	}

	private void ReadArmorInfo(BinaryReader r)
	{
		if (_charcterArmor != null)
		{
			_charcterArmor.Deserialize(r);
		}
	}

	private void RPC_C2S_TakeOffEquipment(uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
		int num = stream.Read<int>(new object[0]);
		ItemObject itemObject = EquipModule[num];
		if (itemObject != null)
		{
			TakeOffEquipment(itemObject);
			SyncTakeOffEquipment(new int[1] { num });
			SyncPackageIndex();
		}
	}

	private void RPC_C2S_PutOnEquipment(uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
		int objId = stream.Read<int>(new object[0]);
		int num = stream.Read<int>(new object[0]);
		ItemObject itemById = Package.GetItemById(objId);
		if (itemById != null)
		{
			int itemIndex = Package.GetItemIndex(itemById);
			if (itemIndex == num && PutOnEquipment(itemById))
			{
				Package.RemoveItem(itemById);
				SyncPutOnEquipment(new ItemObject[1] { itemById });
				SyncPackageIndex();
			}
		}
	}

	public void SyncExploredAreas()
	{
		List<ExploredArea> teamExploredAreas = GameWorld.GetTeamExploredAreas(base.TeamId, base.WorldId);
		if (teamExploredAreas.Count > 0)
		{
			IEnumerable<int> source = teamExploredAreas.Select((ExploredArea iter) => iter.Index);
			RPCOwner(EPacketType.PT_InGame_ExploredAreaArray, source.ToArray());
		}
	}

	public void SyncMaskAreas()
	{
		List<MaskArea> areas = GameWorld.GetTeamMaskAreas(base.WorldId, base.TeamId);
		if (areas.Count <= 0)
		{
			return;
		}
		byte[] array = Serialize.Export(delegate(BinaryWriter w)
		{
			BufferHelper.Serialize(w, areas.Count);
			foreach (MaskArea item in areas)
			{
				BufferHelper.Serialize(w, item.Index);
				BufferHelper.Serialize(w, item.IconID);
				BufferHelper.Serialize(w, item.Pos);
				BufferHelper.Serialize(w, item.Description);
			}
		});
		RPCOwner(EPacketType.PT_InGame_MaskAreaArray, array, false);
	}

	public void SyncTownAreas(uLink.NetworkPlayer peer)
	{
		Vector3[] townAreas = GameWorld.GetTownAreas(base.WorldId);
		if (townAreas == null || townAreas.Length > 0)
		{
			Vector3[] array = townAreas.Where((Vector3 iter) => GameWorld.IsTeamExploredArea(base.WorldId, base.TeamId, AreaHelper.Vector2Int(iter))).ToArray();
			if (array.Length >= 1)
			{
				RPCPeer(peer, EPacketType.PT_InGame_TownAreaArray, array);
			}
		}
	}

	public void SyncCampAreas(uLink.NetworkPlayer peer)
	{
		Vector3[] campAreas = GameWorld.GetCampAreas(base.WorldId);
		if (campAreas == null || campAreas.Length > 0)
		{
			Vector3[] array = campAreas.Where((Vector3 iter) => GameWorld.IsTeamExploredArea(base.WorldId, base.TeamId, AreaHelper.Vector2Int(iter))).ToArray();
			if (array.Length >= 1)
			{
				RPCPeer(peer, EPacketType.PT_InGame_CampAreaArray, array);
			}
		}
	}

	public void SyncExploredArea(int index)
	{
		RPCOthers(EPacketType.PT_InGame_ExploredArea, index);
	}

	private void RPC_C2S_MakeMask(uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
		byte b = stream.Read<byte>(new object[0]);
		Vector3 vector = stream.Read<Vector3>(new object[0]);
		int num = stream.Read<int>(new object[0]);
		string text = stream.Read<string>(new object[0]);
		if (b == byte.MaxValue)
		{
			int num2 = GameWorld.AddMaskArea(base.Id, base.WorldId, base.TeamId, num, vector, text);
			if (num2 != -1)
			{
				SyncGroupData(EPacketType.PT_InGame_MakeMask, (byte)num2, vector, num, text);
			}
			return;
		}
		MaskArea maskArea = GameWorld.GetMaskArea(base.Id, base.WorldId, base.TeamId, b);
		if (maskArea != null)
		{
			maskArea.IconID = num;
			maskArea.Description = text;
			SyncGroupData(EPacketType.PT_InGame_MakeMask, b, vector, num, text);
		}
	}

	private void RPC_C2S_RemoveMask(uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
		byte b = stream.Read<byte>(new object[0]);
		if (GameWorld.RemoveMaskArea(base.Id, base.WorldId, base.TeamId, b))
		{
			SyncGroupData(EPacketType.PT_InGame_RemoveMask, b);
		}
	}

	private void RPC_C2S_AddTownArea(uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
		Vector3 vector = stream.Read<Vector3>(new object[0]);
		GameWorld.AddExploredArea(base.Id, base.WorldId, base.TeamId, vector);
		GameWorld.AddTownArea(base.WorldId, vector);
		SyncGroupData(EPacketType.PT_InGame_TownArea, vector);
	}

	private void RPC_C2S_AddCampArea(uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
		Vector3 vector = stream.Read<Vector3>(new object[0]);
		GameWorld.AddExploredArea(base.Id, base.WorldId, base.TeamId, vector);
		GameWorld.AddCampArea(base.WorldId, vector);
		SyncGroupData(EPacketType.PT_InGame_CampArea, vector);
	}

	private void RPC_C2S_CreateMapObj(uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
		_exceptItemList.Clear();
		int num = stream.Read<int>(new object[0]);
		MapObj[] array = stream.Read<MapObj[]>(new object[0]);
		if (array == null || num == 2 || num != 1)
		{
			return;
		}
		MapObj[] array2 = array;
		foreach (MapObj mapObj in array2)
		{
			if (mapObj != null)
			{
				int objID = mapObj.objID;
				DoodadMgr.CreateDoodad(base.Id, base.TeamId, base.WorldId, -1, IdGenerator.NewDoodadId, -1, mapObj.pos, 1, string.Empty, Vector3.one);
			}
		}
	}

	public void RPC_C2S_CreateSceneBox(uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
		int num = stream.Read<int>(new object[0]);
		if (!MapObjNetwork.HadCreate(num, 3) && ServerConfig.IsStory)
		{
			StoryDoodadDesc storyDoodadDesc = StoryDoodadMap.Get(num);
			if (storyDoodadDesc != null)
			{
				DoodadMgr.CreateDoodad(base.Id, base.TeamId, base.WorldId, num, IdGenerator.NewDoodadId, -1, storyDoodadDesc._pos, 3, storyDoodadDesc._param, Vector3.one);
			}
		}
	}

	public void RPC_C2S_CreateSceneItem(uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
		string text = stream.Read<string>(new object[0]);
		Vector3 pos = stream.Read<Vector3>(new object[0]);
		string text2 = stream.Read<string>(new object[0]);
		int num = stream.Read<int>(new object[0]);
		bool flag = stream.Read<bool>(new object[0]);
		if (num == -1)
		{
			num = MapObjNetwork.Vector2Index(pos);
		}
		if (flag)
		{
			if (!MapObjNetwork.HadCreate(pos))
			{
				text2 = text2 + "|" + text;
				DoodadMgr.CreateDoodadWithoutLimit(base.Id, base.TeamId, base.WorldId, num, IdGenerator.NewDoodadId, -1, pos, 4, text2, Vector3.one);
			}
		}
		else if (!MapObjNetwork.HadCreate(num, 4))
		{
			text2 = text2 + "|" + text;
			DoodadMgr.CreateDoodad(base.Id, base.TeamId, base.WorldId, num, IdGenerator.NewDoodadId, -1, pos, 4, text2, Vector3.one);
		}
	}

	public void RPC_C2S_DestroySceneItem(uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
		Vector3 pos = stream.Read<Vector3>(new object[0]);
		DoodadMgr.DestroyDoodad(pos);
	}

	public IEnumerable<ItemObject> PutItemIntoMapObj(int[] itemList, MapObjNetwork mapObj)
	{
		if (itemList == null || itemList.Length <= 0 || mapObj == null)
		{
			yield break;
		}
		ItemObject[] itemlistVaile = Package.GetValidItemListNotBind(0).Union(Package.GetValidItemListNotBind(1)).Union(Package.GetValidItemListNotBind(2).Union(Package.GetValidItemListNotBind(3)))
			.ToArray();
		if (itemlistVaile.Length <= 0)
		{
			yield break;
		}
		foreach (int itemID in itemList)
		{
			for (int i2 = itemlistVaile.Length - 1; i2 >= 0; i2--)
			{
				ItemObject itemObj = itemlistVaile[i2];
				if (itemObj != null && itemObj.instanceId == itemID)
				{
					mapObj.AddToItemlist(itemID);
					Package.RemoveItem(itemObj);
					yield return itemObj;
					break;
				}
			}
		}
		SyncPackageIndex();
	}

	public ItemObject PutItemIntoMapObj(int itemId, int index, MapObjNetwork mapObj)
	{
		ItemObject itemByID = ItemManager.GetItemByID(itemId);
		if (itemByID == null || mapObj == null)
		{
			return null;
		}
		mapObj.AddToItemlist(itemId, index);
		Package.RemoveItem(itemByID);
		SyncPackageIndex();
		return itemByID;
	}

	public void DeadDropItem(MapObjNetwork mapObj, int dropCount)
	{
		if (mapObj == null)
		{
			return;
		}
		List<ItemObject> list = new List<ItemObject>();
		for (int i = 0; i < dropCount; i++)
		{
			int num = EquipNotBindCount();
			int num2 = Package.ItemNotBindCount();
			int max = num + num2;
			int num3 = UnityEngine.Random.Range(1, max);
			if (num3 > num2)
			{
				RandPutEquipmentIntoMapObj(mapObj);
				continue;
			}
			ItemObject itemObject = RandGetValidItemFromPackage(_exceptItemList);
			if (itemObject != null)
			{
				ItemObject itemObject2 = null;
				itemObject2 = PutItemIntoMapObj(mapObj, itemObject);
				if (itemObject2 != null)
				{
					list.Add(itemObject2);
				}
				_exceptItemList.Add(itemObject);
			}
		}
		SyncEquipedItems(EquipModule.EquipItems);
		if (list.Count > 0)
		{
			SyncItemList(list.ToArray());
		}
		SyncPackageIndex();
	}

	private ItemObject RandGetValidItemFromPackage(List<ItemObject> exceptItemList)
	{
		IEnumerable<ItemObject> source = Package.GetValidItemListNotBind(0).Union(Package.GetValidItemListNotBind(1)).Union(Package.GetValidItemListNotBind(2).Union(Package.GetValidItemListNotBind(3)));
		List<ItemObject> first = source.ToList();
		IEnumerable<ItemObject> source2 = first.Except(exceptItemList);
		int num = source2.Count();
		if (num <= 0)
		{
			return null;
		}
		int num2 = UnityEngine.Random.Range(1, num);
		return source2.ElementAt(num2 - 1);
	}

	private ItemObject PutItemIntoMapObj(MapObjNetwork mapObj, ItemObject itemObj)
	{
		if (mapObj == null || itemObj == null)
		{
			return null;
		}
		int stackCount = itemObj.stackCount;
		int num = UnityEngine.Random.Range(1, stackCount);
		if (num >= stackCount)
		{
			mapObj.AddToItemlist(itemObj.instanceId);
			Package.RemoveItem(itemObj);
			SyncPackageIndex();
			return null;
		}
		ItemObject itemObject = CreateItem(itemObj.protoId, num);
		if (itemObject != null)
		{
			mapObj.AddToItemlist(itemObject.instanceId);
			ItemCountDown(itemObj, num);
		}
		return itemObject;
	}

	private void RandPutEquipmentIntoMapObj(MapObjNetwork mapObj)
	{
		if (!(mapObj == null))
		{
			ItemObject[] notBindEquips = EquipModule.GetNotBindEquips();
			if (notBindEquips != null && notBindEquips.Length > 0)
			{
				int num = UnityEngine.Random.Range(1, notBindEquips.Length);
				ItemObject itemObject = notBindEquips[num - 1];
				RemoveEquipment(itemObject);
				mapObj.AddToItemlist(itemObject.instanceId);
			}
		}
	}

	private void RPC_C2S_MultiMergeSkill(uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
		int id = stream.Read<int>(new object[0]);
		int _productCount = stream.Read<int>(new object[0]);
		if (_productCount == 0)
		{
			return;
		}
		Replicator.Formula formula = Replicator.Formula.Mgr.Instance.Find(id);
		if (formula == null)
		{
			return;
		}
		List<ItemObject> effItems = new List<ItemObject>(10);
		if (!ServerConfig.UnlimitedRes)
		{
			foreach (Replicator.Formula.Material material in formula.materials)
			{
				if (GetItemNum(material.itemId) < material.itemCount * _productCount)
				{
					return;
				}
			}
			IEnumerable<ItemSample> items = formula.materials.Select((Replicator.Formula.Material iter) => new ItemSample(iter.itemId, iter.itemCount * _productCount));
			ItemObject[] collection = Package.RemoveItem(items);
			effItems.AddRange(collection);
		}
		int num = _productCount * formula.m_productItemCount;
		Package.AddSameItems(formula.productItemId, num, ref effItems);
		SyncItemList(effItems.ToArray());
		SyncPackageIndex();
		SyncNewItem(new ItemSample(formula.productItemId, num));
	}

	private void RPC_C2S_CancelMerge(uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
	}

	public static bool CheckRemoveMissionDun()
	{
		if (ServerConfig.IsAdventure)
		{
			List<Player> list = ObjNetInterface.Get<Player>();
			foreach (Player item in list)
			{
				if (item != null && !item.HadCompleteMission(9088))
				{
					return false;
				}
			}
		}
		return true;
	}

	public void SyncPlayerMission()
	{
		byte[] array3;
		byte[] array2;
		byte[] array;
		byte[] array4 = (array3 = (array2 = (array = null)));
		PlayerMission curPlayerMission = MissionManager.Manager.GetCurPlayerMission(base.Id);
		if (curPlayerMission != null)
		{
			array4 = ((curPlayerMission.m_nTeam <= -1) ? curPlayerMission.Export(0) : curPlayerMission.Export(1));
			array3 = ((!ServerConfig.IsStory) ? AdRMRepository.Export(base.Id, curPlayerMission) : RMRepository.Export(curPlayerMission, base.Id));
		}
		PlayerMission curTeamMission = MissionManager.Manager.GetCurTeamMission(base.Id);
		if (curTeamMission != null)
		{
			array2 = ((curTeamMission.m_nTeam <= -1) ? curTeamMission.Export(0) : curTeamMission.Export(1));
			array = ((!ServerConfig.IsStory) ? AdRMRepository.Export(base.Id, curTeamMission) : RMRepository.Export(curTeamMission, base.TeamId));
		}
		RPCOwner(EPacketType.PT_InGame_SyncMission, array4, array3, array2, array);
	}

	public void SyncTeamMission()
	{
	}

	public void SycTeamCollectItemID(int targetId, int itemId, int itemNum)
	{
		SyncGroupData(EPacketType.PT_InGame_SetCollectItemID, targetId, itemId, itemNum);
	}

	private void ProcessRandomMission(ref MissionCommonData data, AiAdNpcNetwork npcR)
	{
		data.m_iNpc = npcR.Id;
		NpcMissionData mission = npcR.mission;
		mission.m_MissionListReply.Clear();
		for (int i = 0; i < data.m_TargetIDList.Count; i++)
		{
			switch (MissionRepository.GetTargetType(data.m_TargetIDList[i]))
			{
			case TargetType.TargetType_Follow:
			{
				TypeFollowData typeFollowData = MissionManager.GetTypeFollowData(data.m_TargetIDList[i]);
				if (typeFollowData != null)
				{
					typeFollowData.m_iNpcList.Clear();
					typeFollowData.m_FailResetPos = npcR.transform.position;
					typeFollowData.m_iNpcList.Add(npcR.Id);
				}
				break;
			}
			case TargetType.TargetType_TowerDif:
			{
				TypeTowerDefendsData typeTowerDefendsData = MissionManager.GetTypeTowerDefendsData(data.m_TargetIDList[i]);
				if (typeTowerDefendsData != null)
				{
					typeTowerDefendsData.m_NpcList.Clear();
					typeTowerDefendsData.m_NpcList.Add(npcR.Id);
					typeTowerDefendsData.finallyPos = typeTowerDefendsData.m_Pos.pos;
				}
				break;
			}
			default:
				data.m_iReplyNpc = npcR.Id;
				if (!mission.m_MissionListReply.Contains(data.m_ID))
				{
					mission.m_MissionListReply.Add(data.m_ID);
				}
				break;
			case TargetType.TargetType_Discovery:
				break;
			}
		}
	}

	public int SetGetTakeMission(int missionID, int nNpcID, TakeMissionType type, bool bCheck)
	{
		int num = -1;
		MissionCommonData data = MissionManager.Manager.GetAdrmMissionCommonData(base.Id, missionID);
		if (data == null)
		{
			return -1;
		}
		if (!ServerConfig.IsStory && nNpcID == -1)
		{
			nNpcID = data.m_iNpc;
			if (ObjNetInterface.Get<AiAdNpcNetwork>(nNpcID) == null)
			{
				nNpcID = -1;
			}
		}
		if (nNpcID != -1)
		{
			AiAdNpcNetwork aiAdNpcNetwork = ObjNetInterface.Get<AiAdNpcNetwork>(nNpcID);
			if (null == aiAdNpcNetwork || aiAdNpcNetwork.mission == null || aiAdNpcNetwork.IntMissionFlag == MISSIONFlag.Mission_Follow || aiAdNpcNetwork.IntMissionFlag == MISSIONFlag.Mission_Dif)
			{
				return -1;
			}
			if (missionID == 191)
			{
				ColonyNpc npcByID = ColonyNpcMgr.GetNpcByID(aiAdNpcNetwork.Id);
				if (npcByID == null)
				{
					ColonyMgr._Instance.CanAddNpc(base.TeamId);
					ColonyDwellings oneBedSpace = ColonyMgr.GetOneBedSpace(base.TeamId);
					if (oneBedSpace == null)
					{
						return -1;
					}
					if (ColonyMgr._Instance.AddNpc(aiAdNpcNetwork, oneBedSpace, base.TeamId))
					{
						RPCOthers(EPacketType.PT_InGame_AddNpcToColony, nNpcID, base.TeamId, oneBedSpace.Id);
						return 2;
					}
					return -1;
				}
				return -1;
			}
			NpcMissionData mission = aiAdNpcNetwork.mission;
			if (mission == null)
			{
				return -1;
			}
			AdRandomGroup adRandomGroup = AdRMRepository.GetAdRandomGroup(mission.m_QCID);
			if (adRandomGroup == null)
			{
				return -1;
			}
			PlayerMission playerMission;
			if ((adRandomGroup.IsMultiMode && base.TeamId != -1) || (ServerConfig.IsAdventure && !ServerConfig.IsVS))
			{
				playerMission = MissionManager.Manager.GetTeamPlayerMission(base.TeamId);
				if (playerMission == null)
				{
					playerMission = new PlayerMission();
				}
				playerMission.m_nTeam = base.TeamId;
				MissionManager.Manager.AddTeamPlayerMission(base.TeamId, playerMission);
			}
			else
			{
				playerMission = MissionManager.Manager.GetCurPlayerMission(base.Id);
				if (playerMission == null)
				{
					playerMission = MissionManager.Manager.GetCurTeamMission(base.Id);
				}
			}
			playerMission.m_CurNpc = aiAdNpcNetwork;
			playerMission.m_playerName = roleName;
			if (MissionManager.HasRandomMission(missionID))
			{
				MissionCommonData adrmData = MissionManager.Manager.GetAdrmData(base.Id, missionID);
				if (adrmData == null && ServerConfig.IsAdventure && !ServerConfig.IsVS)
				{
					int idx = -1;
					int rewardIdx = -1;
					adrmData = AdRMRepository.CreateRandomMission(missionID, ref idx, ref rewardIdx, nNpcID);
					if (adrmData != null)
					{
						MissionManager.Manager.AddAdrmData(base.Id, adrmData);
						SyncGroupData(EPacketType.PT_InGame_NewMission, missionID, idx, rewardIdx);
						data = adrmData;
					}
				}
				ProcessRandomMission(ref data, aiAdNpcNetwork);
			}
			if (type == TakeMissionType.TakeMissionType_Complete)
			{
				playerMission.CompleteMission(missionID, this);
				return -1;
			}
			if (ServerConfig.IsStory)
			{
				if (bCheck && !playerMission.IsGetTakeMission(missionID, this) && !data.IsTalkMission())
				{
					return -1;
				}
			}
			else if (bCheck && !playerMission.IsGetTakeMission(missionID, this))
			{
				return -1;
			}
			playerMission.SetQuestVariable(missionID, "STEP", "0", this);
			if (!data.IsTalkMission())
			{
				for (int i = 0; i < data.m_TargetIDList.Count; i++)
				{
					switch (MissionRepository.GetTargetType(data.m_TargetIDList[i]))
					{
					case TargetType.TargetType_KillMonster:
					{
						TypeMonsterData typeMonsterData = MissionManager.GetTypeMonsterData(data.m_TargetIDList[i]);
						if (typeMonsterData != null)
						{
							for (int j = 0; j < typeMonsterData.m_MonsterList.Count; j++)
							{
								int num2 = i * 10 + j;
								string missionValue = typeMonsterData.m_MonsterList[j].id + "_0";
								playerMission.SetQuestVariable(missionID, "MONSTER" + num2, missionValue, this);
							}
						}
						break;
					}
					case TargetType.TargetType_UseItem:
					{
						TypeUseItemData typeUseItemData = MissionManager.GetTypeUseItemData(data.m_TargetIDList[i]);
						if (typeUseItemData != null)
						{
							string missionValue = typeUseItemData.m_ItemID + "_0";
							playerMission.SetQuestVariable(missionID, "ITEM" + i, missionValue, this);
						}
						break;
					}
					}
				}
			}
			switch (type)
			{
			case TakeMissionType.TakeMissionType_Get:
			{
				if (data.m_OPID == null)
				{
					break;
				}
				for (int l = 0; l < data.m_OPID.Count; l++)
				{
					if (MissionRepository.HaveTalkOP(data.m_OPID[l]))
					{
					}
				}
				break;
			}
			case TakeMissionType.TakeMissionType_In:
			{
				if (data.m_INID == null)
				{
					break;
				}
				for (int m = 0; m < data.m_INID.Count; m++)
				{
					if (playerMission.IsGetTakeMission(data.m_INID[m], this))
					{
						SetGetTakeMission(data.m_INID[m], nNpcID, TakeMissionType.TakeMissionType_Get, bCheck);
					}
				}
				break;
			}
			case TakeMissionType.TakeMissionType_Complete:
			{
				if (!playerMission.HadCompleteMission(missionID, this))
				{
					playerMission.CompleteMission(missionID, this);
				}
				if (data.m_EDID == null)
				{
					break;
				}
				for (int k = 0; k < data.m_EDID.Count; k++)
				{
					if (!MissionRepository.HaveTalkOP(data.m_EDID[k]) && playerMission.IsGetTakeMission(data.m_EDID[k], this))
					{
						SetGetTakeMission(data.m_EDID[k], nNpcID, TakeMissionType.TakeMissionType_Get, bCheck);
					}
				}
				break;
			}
			}
			if (adRandomGroup.IsMultiMode || (ServerConfig.IsAdventure && !ServerConfig.IsVS))
			{
				return 1;
			}
			return 0;
		}
		if (!ServerConfig.IsStory && LogFilter.logDebug)
		{
			Debug.LogError("mission npc is wrong ,missionid = " + missionID);
		}
		PlayerMission playerMission2 = MissionManager.Manager.GetTeamPlayerMission(base.TeamId);
		if (playerMission2 == null)
		{
			playerMission2 = new PlayerMission();
		}
		playerMission2.m_nTeam = base.TeamId;
		playerMission2.m_playerName = roleName;
		MissionManager.Manager.AddTeamPlayerMission(base.TeamId, playerMission2);
		if (type != TakeMissionType.TakeMissionType_Complete)
		{
			if (ServerConfig.IsStory)
			{
				if (bCheck && !playerMission2.IsGetTakeMission(missionID, this) && !data.IsTalkMission())
				{
					return -1;
				}
			}
			else if (bCheck && !playerMission2.IsGetTakeMission(missionID, this))
			{
				return -1;
			}
			playerMission2.SetQuestVariable(missionID, "STEP", "0", this);
			if (!data.IsTalkMission())
			{
				for (int n = 0; n < data.m_TargetIDList.Count; n++)
				{
					switch (MissionRepository.GetTargetType(data.m_TargetIDList[n]))
					{
					case TargetType.TargetType_KillMonster:
					{
						TypeMonsterData typeMonsterData2 = MissionManager.GetTypeMonsterData(data.m_TargetIDList[n]);
						if (typeMonsterData2 != null)
						{
							for (int num3 = 0; num3 < typeMonsterData2.m_MonsterList.Count; num3++)
							{
								int num4 = n * 10 + num3;
								string missionValue2 = typeMonsterData2.m_MonsterList[num3].id + "_0";
								playerMission2.SetQuestVariable(missionID, "MONSTER" + num4, missionValue2, this);
							}
						}
						break;
					}
					case TargetType.TargetType_UseItem:
					{
						TypeUseItemData typeUseItemData2 = MissionManager.GetTypeUseItemData(data.m_TargetIDList[n]);
						if (typeUseItemData2 != null)
						{
							string missionValue2 = typeUseItemData2.m_ItemID + "_0";
							playerMission2.SetQuestVariable(missionID, "ITEM" + n, missionValue2, this);
						}
						break;
					}
					}
				}
			}
			switch (type)
			{
			case TakeMissionType.TakeMissionType_Get:
			{
				if (data.m_OPID == null)
				{
					break;
				}
				for (int num6 = 0; num6 < data.m_OPID.Count; num6++)
				{
					if (MissionRepository.HaveTalkOP(data.m_OPID[num6]))
					{
					}
				}
				break;
			}
			case TakeMissionType.TakeMissionType_In:
			{
				if (data.m_INID == null)
				{
					break;
				}
				for (int num7 = 0; num7 < data.m_INID.Count; num7++)
				{
					if (playerMission2.IsGetTakeMission(data.m_INID[num7], this))
					{
						SetGetTakeMission(data.m_INID[num7], nNpcID, TakeMissionType.TakeMissionType_Get, bCheck);
					}
				}
				break;
			}
			case TakeMissionType.TakeMissionType_Complete:
			{
				if (!playerMission2.HadCompleteMission(missionID, this))
				{
					playerMission2.CompleteMission(missionID, this);
				}
				if (data.m_EDID == null)
				{
					break;
				}
				for (int num5 = 0; num5 < data.m_EDID.Count; num5++)
				{
					if (!MissionRepository.HaveTalkOP(data.m_EDID[num5]) && playerMission2.IsGetTakeMission(data.m_EDID[num5], this))
					{
						SetGetTakeMission(data.m_EDID[num5], nNpcID, TakeMissionType.TakeMissionType_Get, bCheck);
					}
				}
				break;
			}
			}
			return 1;
		}
		return -1;
	}

	public void RequestKillMonsterPos(Vector3 pos, float radius, List<MissionIDNum> monlist)
	{
		List<int> list = new List<int>();
		List<int> list2 = new List<int>();
		for (int i = 0; i < monlist.Count; i++)
		{
			MissionIDNum missionIDNum = monlist[i];
			list.Add(missionIDNum.id);
			list2.Add(missionIDNum.num);
		}
		int[] array = list.ToArray();
		int[] array2 = list2.ToArray();
		RPCOwner(EPacketType.PT_InGame_MissionMonsterPos, pos, radius, array, array2);
	}

	public void RequestFollow(float x, float y, int targetid, int missionId)
	{
		RPCOwner(EPacketType.PT_InGame_MissionFollowPos, x, y, targetid, missionId);
	}

	public void RequestDiscovery(float x, float y, int targetid, int missionId)
	{
		RPCOthers(EPacketType.PT_InGame_MissionDiscoveryPos, x, y, targetid, missionId);
	}

	public void SyncUseItemPos(int targetid, Vector3 pos, int missionId)
	{
		RPCOthers(EPacketType.PT_InGame_MissionItemUsePos, targetid, pos, missionId);
	}

	public void RequestModifyMissionFlag(int MissionID, string MissionFlag, string MissionValue)
	{
		PlayerMission curPlayerMissionByMissionId = MissionManager.Manager.GetCurPlayerMissionByMissionId(base.Id, MissionID);
		if (curPlayerMissionByMissionId != null)
		{
			if (curPlayerMissionByMissionId.m_nTeam == -1)
			{
				RPCOwner(EPacketType.PT_InGame_ModifyMissionFlag, MissionID, MissionFlag, MissionValue);
			}
			else
			{
				SyncGroupData(EPacketType.PT_InGame_ModifyMissionFlag, MissionID, MissionFlag, MissionValue);
			}
		}
	}

	public void CompleteTarget(int targetid, int missionid)
	{
		PlayerMission curPlayerMissionByMissionId = MissionManager.Manager.GetCurPlayerMissionByMissionId(base.Id, missionid);
		if (curPlayerMissionByMissionId != null && curPlayerMissionByMissionId.HasMission(missionid))
		{
			if (curPlayerMissionByMissionId.m_nTeam == -1)
			{
				RPCOwner(EPacketType.PT_InGame_CompleteTarget, targetid, missionid, base.Id);
			}
			else
			{
				SyncGroupData(EPacketType.PT_InGame_CompleteTarget, targetid, missionid, base.Id);
			}
		}
	}

	public void ReplyCompleteMission(int targetid, int missionid, bool bCheck = true, bool exceptme = false)
	{
		PlayerMission curPlayerMissionByMissionId = MissionManager.Manager.GetCurPlayerMissionByMissionId(base.Id, missionid);
		if (curPlayerMissionByMissionId != null)
		{
			if (curPlayerMissionByMissionId.m_nTeam == -1)
			{
				RPCOwner(EPacketType.PT_InGame_CompleteMission, targetid, missionid, bCheck);
			}
			else if (exceptme)
			{
				SyncGroupData(this, EPacketType.PT_InGame_CompleteMission, targetid, missionid, bCheck);
			}
			else
			{
				SyncGroupData(EPacketType.PT_InGame_CompleteMission, targetid, missionid, bCheck);
			}
		}
	}

	public void FailMission(int missionid)
	{
		PlayerMission curPlayerMissionByMissionId = MissionManager.Manager.GetCurPlayerMissionByMissionId(base.Id, missionid);
		if (curPlayerMissionByMissionId != null)
		{
			if (curPlayerMissionByMissionId.m_nTeam == -1)
			{
				RPCOwner(EPacketType.PT_InGame_MissionFailed, missionid);
			}
			else
			{
				SyncGroupData(EPacketType.PT_InGame_MissionFailed, missionid);
			}
		}
	}

	internal bool AddServant(AiAdNpcNetwork npc)
	{
		if (_servantList.Count >= 2)
		{
			return false;
		}
		if (_servantList.Contains(npc))
		{
			return false;
		}
		_servantList.Add(npc);
		return true;
	}

	internal bool AddForceServant(AiAdNpcNetwork npc)
	{
		if (_forceServantList.Count >= 2)
		{
			return false;
		}
		if (_forceServantList.Contains(npc))
		{
			return false;
		}
		_forceServantList.Add(npc);
		return true;
	}

	internal void DismissServant(int npcID)
	{
		_servantList.RemoveAll((AiAdNpcNetwork iter) => iter.Id == npcID);
		_forceServantList.RemoveAll((AiAdNpcNetwork iter) => iter.Id == npcID);
	}

	internal void DismissServant(AiAdNpcNetwork npc)
	{
		_servantList.Remove(npc);
		_forceServantList.Remove(npc);
	}

	private void SendAccessMission(int ret, int missionId, int npcId, TakeMissionType type, bool bCheck)
	{
		switch (ret)
		{
		case -1:
			return;
		case 0:
			RPCOwner(EPacketType.PT_InGame_AccessMission, missionId, npcId, bCheck);
			break;
		default:
		{
			PlayerMission playerMission = MissionManager.Manager.GetCurTeamMission(base.Id);
			if (playerMission == null)
			{
				playerMission = MissionManager.Manager.GetCurPlayerMission(base.Id);
				if (playerMission == null)
				{
					return;
				}
			}
			if (ServerConfig.IsStory)
			{
				byte[] array = RMRepository.Export(playerMission, base.TeamId);
				SyncGroupData(EPacketType.PT_InGame_AccessMission, missionId, npcId, bCheck, array);
				break;
			}
			byte[] array2 = AdRMRepository.Export(base.Id, playerMission);
			if (array2 == null)
			{
				return;
			}
			SyncGroupData(EPacketType.PT_InGame_AccessMission, missionId, npcId, bCheck, array2);
			break;
		}
		case 2:
			break;
		}
		followdata.Clear();
	}

	private void RPC_C2S_CreateMission(uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
		int num = stream.Read<int>(new object[0]);
		int npcId = stream.Read<int>(new object[0]);
		int idx = -1;
		int rewardIdx = -1;
		if (!HasMission(num))
		{
			MissionCommonData missionCommonData = null;
			missionCommonData = ((!ServerConfig.IsStory) ? AdRMRepository.CreateRandomMission(num, ref idx, ref rewardIdx, npcId) : RMRepository.CreateRandomMission(num, ref idx, ref rewardIdx));
			if (missionCommonData != null)
			{
				MissionManager.Manager.AddAdrmData(base.Id, missionCommonData);
				SyncGroupData(EPacketType.PT_InGame_NewMission, num, idx, rewardIdx);
			}
		}
	}

	private void RPC_C2S_AccessMission(uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
		int num = stream.Read<int>(new object[0]);
		int num2 = stream.Read<int>(new object[0]);
		byte type = stream.Read<byte>(new object[0]);
		bool flag = stream.Read<bool>(new object[0]);
		if ((!flag || !HasMission(num)) && !HadCompleteMission(num))
		{
			int num3 = SetGetTakeMission(num, num2, (TakeMissionType)type, flag);
			if (num3 != -1)
			{
				SendAccessMission(num3, num, num2, (TakeMissionType)type, flag);
			}
		}
	}

	private void RPC_C2S_InitMission(uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
		stream.TryRead<int>(out var value);
		TypeMonsterData typeMonsterData = MissionManager.GetTypeMonsterData(value);
		if (typeMonsterData != null)
		{
			Vector2 vector = UnityEngine.Random.insideUnitCircle.normalized * typeMonsterData.m_AdDist;
			Vector3 pos = base.transform.position + new Vector3(vector.x, 0f, vector.y);
			RequestKillMonsterPos(pos, typeMonsterData.m_AdRadius, typeMonsterData.m_CreateMonsterList);
		}
	}

	private void RPC_C2S_ResponseKillMonsterPos(uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
		Vector3[] array = stream.Read<Vector3[]>(new object[0]);
		int[] array2 = stream.Read<int[]>(new object[0]);
		int[] array3 = stream.Read<int[]>(new object[0]);
		int num = 0;
		for (int i = 0; i < array3.Length; i++)
		{
			for (int j = 0; j < array3[i]; j++)
			{
				num++;
			}
		}
	}

	private void RPC_C2S_ResponseFollowPos(uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
		Vector3 distPos = stream.Read<Vector3>(new object[0]);
		int targetID = stream.Read<int>(new object[0]);
		TypeFollowData typeFollowData = MissionManager.GetTypeFollowData(targetID);
		if (typeFollowData != null)
		{
			typeFollowData.m_DistPos = distPos;
		}
	}

	private void RPC_C2S_ResponseDiscoveryPos(uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
		stream.TryRead<Vector3>(out var value);
		stream.TryRead<int>(out var value2);
		TypeSearchData typeSearchData = MissionManager.GetTypeSearchData(value2);
		if (typeSearchData != null)
		{
			typeSearchData.m_DistPos = value;
		}
	}

	private void RPC_C2S_ReplyDeleteMission(uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
		stream.TryRead<int>(out var value);
		MissionCommonData adrmMissionCommonData = MissionManager.Manager.GetAdrmMissionCommonData(base.Id, value);
		if (adrmMissionCommonData == null)
		{
			return;
		}
		PlayerMission curPlayerMissionByMissionId = MissionManager.Manager.GetCurPlayerMissionByMissionId(base.Id, value);
		if (curPlayerMissionByMissionId == null || !curPlayerMissionByMissionId.HasMission(value))
		{
			return;
		}
		curPlayerMissionByMissionId.DelMissionInfo(value, this);
		for (int i = 0; i < adrmMissionCommonData.m_ResetID.Count; i++)
		{
			if (curPlayerMissionByMissionId.m_MissionState.ContainsKey(adrmMissionCommonData.m_ResetID[i]))
			{
				curPlayerMissionByMissionId.m_MissionState.Remove(adrmMissionCommonData.m_ResetID[i]);
				MissionCommonData missionCommonData = MissionManager.GetMissionCommonData(adrmMissionCommonData.m_ResetID[i]);
				for (int j = 0; j < missionCommonData.m_TargetIDList.Count; j++)
				{
					curPlayerMissionByMissionId.m_MissionTargetState.Remove(missionCommonData.m_TargetIDList[j]);
				}
			}
		}
		if (ServerConfig.IsStory)
		{
			for (int k = 0; k < adrmMissionCommonData.m_TargetIDList.Count; k++)
			{
				curPlayerMissionByMissionId.m_MissionTargetState.Remove(adrmMissionCommonData.m_TargetIDList[k]);
			}
		}
		AiTowerDefense.TowerDefenseFinish(value);
		SyncGroupData(EPacketType.PT_InGame_DeleteMission, value);
	}

	private void RPC_C2S_RequestCompleteTarget(uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
		stream.TryRead<int>(out var value);
		stream.TryRead<int>(out var value2);
		MissionManager.Manager.GetCurPlayerMissionByMissionId(base.Id, value2)?.CompleteTarget(value, value2, this);
	}

	private void RPC_C2S_RequestCompleteMission(uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
		stream.TryRead<int>(out var value);
		stream.TryRead<int>(out var value2);
		bool bCheck = stream.Read<bool>(new object[0]);
		PlayerMission playerMission = MissionManager.Manager.GetCurPlayerMissionByMissionId(base.Id, value2);
		if (playerMission == null)
		{
			MissionCommonData adrmMissionCommonData = MissionManager.Manager.GetAdrmMissionCommonData(base.Id, value2);
			if (adrmMissionCommonData == null || !adrmMissionCommonData.IsTalkMission())
			{
				return;
			}
			playerMission = MissionManager.Manager.GetCurTeamMission(base.Id);
			if (playerMission == null)
			{
				return;
			}
			playerMission.CompleteMission(value2, this);
		}
		playerMission.ProcessTargetByMission(value, value2, this, bCheck);
		AiTowerDefense.TowerDefenseFinish(value2);
	}

	private void RPC_C2S_ComMisTest(uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
		stream.TryRead<int>(out var value);
		PlayerMission curPlayerMissionByMissionId = MissionManager.Manager.GetCurPlayerMissionByMissionId(base.Id, value);
		if (curPlayerMissionByMissionId == null)
		{
			return;
		}
		Dictionary<string, string> missionFlagType = curPlayerMissionByMissionId.GetMissionFlagType(value);
		foreach (KeyValuePair<string, string> item in missionFlagType)
		{
			string key = item.Key;
			if (key.Contains("MONSTER"))
			{
				string[] array = item.Value.Split('_');
				if (array.Length == 2)
				{
					string missionValue = array[0] + "_5";
					curPlayerMissionByMissionId.ModifyQuestVariable(value, key, missionValue, this);
					break;
				}
			}
		}
	}

	private void RPC_C2S_MissionFailed(uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
		stream.TryRead<int>(out var value);
		PlayerMission curPlayerMissionByMissionId = MissionManager.Manager.GetCurPlayerMissionByMissionId(base.Id, value);
		if (curPlayerMissionByMissionId != null)
		{
			curPlayerMissionByMissionId.FailureMission(this, value);
			AiTowerDefense.TowerDefenseFinish(value);
		}
	}

	private void RPC_C2S_MissionUseItem(uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
		int protoId = stream.Read<int>(new object[0]);
		Vector3 vector = stream.Read<Vector3>(new object[0]);
		ProcessItemMission(protoId);
	}

	private void RPC_C2S_ModifyQuestVariable(uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
		int num = stream.Read<int>(new object[0]);
		string missionFlag = stream.Read<string>(new object[0]);
		int itemID = stream.Read<int>(new object[0]);
		int num2 = stream.Read<int>(new object[0]);
		MissionManager.Manager.GetCurPlayerMissionByMissionId(base.Id, num)?.ModifyQuestVariable(num, missionFlag, itemID, num2, this);
	}

	private void RPC_C2S_MissionTowerDefense(uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
		int num = stream.Read<int>(new object[0]);
		int num2 = stream.Read<int>(new object[0]);
		Vector3 pos = stream.Read<Vector3>(new object[0]);
		int newMonsterId = IdGenerator.NewMonsterId;
		int id = base.Id;
		if (ServerConfig.IsAdventure)
		{
			id = GetNearestPlayer(base.TeamId, pos).Id;
		}
		NetInterface.Instantiate(PrefabManager.Self.AiTowerDefense, Vector3.zero, Quaternion.identity, base.WorldId, newMonsterId, num, num2, id, base.TeamId);
	}

	private void RPC_C2S_MissionKillMonster(uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
		int num = stream.Read<int>(new object[0]);
		int num2 = stream.Read<int>(new object[0]);
		RPCPeer(info.sender, EPacketType.PT_InGame_MissionKillMonster, num, num2);
	}

	private void RPC_C2S_SetMission(uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
		int num = stream.Read<int>(new object[0]);
		MissionManager.Manager.GetCurPlayerMissionByMissionId(base.Id, num)?.SetMission(num, this);
		RPCProxy(info.sender, EPacketType.PT_InGame_SetMission, num);
	}

	private void RPC_C2S_EntityReach(uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
		RPCOthers(EPacketType.PT_InGame_EntityReach, stream.Read<int>(new object[0]), stream.Read<int>(new object[0]));
	}

	private void RPC_C2S_RequestAdMissionData(uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
		int num = stream.Read<int>(new object[0]);
		int num2 = stream.Read<int>(new object[0]);
		int missionId = stream.Read<int>(new object[0]);
		switch ((TargetType)num)
		{
		case TargetType.TargetType_Follow:
			if (!ServerConfig.IsStory)
			{
				TypeFollowData typeFollowData = MissionManager.GetTypeFollowData(num2);
				if (typeFollowData != null)
				{
					RequestFollow(typeFollowData.m_DistPos.x, typeFollowData.m_DistPos.z, num2, missionId);
				}
			}
			break;
		case TargetType.TargetType_Discovery:
		{
			TypeSearchData typeSearchData = MissionManager.GetTypeSearchData(num2);
			if (typeSearchData != null)
			{
				RequestDiscovery(typeSearchData.m_DistPos.x, typeSearchData.m_DistPos.z, num2, missionId);
			}
			break;
		}
		case TargetType.TargetType_UseItem:
		{
			TypeUseItemData typeUseItemData = MissionManager.GetTypeUseItemData(num2);
			if (typeUseItemData != null)
			{
				SyncUseItemPos(num2, typeUseItemData.m_Pos, missionId);
			}
			break;
		}
		case TargetType.TargetType_Messenger:
			break;
		case TargetType.TargetType_TowerDif:
			break;
		}
	}

	private void RPC_C2S_RequestAiOp(uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
		int num = stream.Read<int>(new object[0]);
		int num2 = stream.Read<int>(new object[0]);
		int[] array = stream.Read<int[]>(new object[0]);
		AiAdNpcNetwork.EReqType eReqType = (AiAdNpcNetwork.EReqType)num;
		if (eReqType != AiAdNpcNetwork.EReqType.FollowTarget)
		{
			return;
		}
		Player nearestPlayer = GetNearestPlayer(array, base.Id);
		if (nearestPlayer == null)
		{
			int[] array2 = array;
			foreach (int id in array2)
			{
				AiAdNpcNetwork aiAdNpcNetwork = ObjNetInterface.Get(id) as AiAdNpcNetwork;
				aiAdNpcNetwork.ForceGetController(base.Id);
			}
		}
	}

	private void RPC_C2S_ModifyMissionFlag(uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
		int num = stream.Read<int>(new object[0]);
		string missionFlag = stream.Read<string>(new object[0]);
		string missionValue = stream.Read<string>(new object[0]);
		MissionManager.Manager.GetCurPlayerMissionByMissionId(base.Id, num)?.ModifyQuestVariable(num, missionFlag, missionValue, this);
	}

	public void PlayerMoveInit()
	{
		base._OnCar = false;
		base._OnTrain = false;
		base._Creation = null;
	}

	public void SetPosition(Vector3 pos)
	{
		base.transform.position = pos;
		mPlayerSynAttribute.mv3Postion = pos;
	}

	public void SyncPosArea(Vector3 pos)
	{
		int index = AreaHelper.Vector2Int(pos);
		if (!ServerConfig.IsStory && !ServerConfig.IsCustom && GameWorld.AddExploredArea(base.Id, base.WorldId, base.TeamId, index))
		{
			SyncExploredArea(index);
		}
		RoomMgr.UpdateBroadcastSet(index, this);
	}

	private bool CanFastTravel(int areaIndex)
	{
		GameWorld gameWorld = GameWorld.GetGameWorld(base.WorldId);
		if (gameWorld == null)
		{
			return false;
		}
		return !gameWorld.IsOccupiedArea(base.TeamId, areaIndex);
	}

	private void RPC_C2S_FastTransfer(uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
		Vector3 pos = stream.Read<Vector3>(new object[0]);
		int num = stream.Read<int>(new object[0]);
		int num2 = stream.Read<int>(new object[0]);
		if (GameMoney < num || (ServerConfig.IsStory && _curSceneId != 0) || _bDeath)
		{
			return;
		}
		if (num2 == 1)
		{
			int areaIndex = AreaHelper.Vector2Int(pos);
			if (!CanFastTravel(areaIndex))
			{
				return;
			}
		}
		ReduceMoney(num);
		CommonHelper.AdjustPos(ref pos);
		SetPosition(pos);
		SyncPosArea(pos);
		if (_mount != null)
		{
			_mount.transform.position = pos;
		}
		RPCOthers(EPacketType.PT_InGame_FastTransfer, pos);
	}

	private void RPC_C2S_PlayerMovePosition(uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
		if (!base._OnCar && !base._OnTrain && !base._OnRide)
		{
			Vector3 vector = stream.Read<Vector3>(new object[0]);
			byte b = stream.Read<byte>(new object[0]);
			float num = stream.Read<float>(new object[0]);
			double num2 = stream.Read<double>(new object[0]);
			SetPosition(vector);
			SyncPosArea(vector);
			base.transform.rotation = Quaternion.Euler(0f, num, 0f);
			mPlayerSynAttribute.mfRotationY = num;
			mPlayerSynAttribute.mnPlayerState = (SpeedState)b;
			URPCProxy(EPacketType.PT_InGame_PlayerPosition, vector, b, num, num2);
		}
	}

	private void RPC_C2S_PlayerMoveRotationY(uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
		if (stream.TryRead<float>(out var value) && !base._OnCar && !base._OnTrain && !base._OnRide)
		{
			base.transform.rotation = Quaternion.Euler(0f, value, 0f);
			mPlayerSynAttribute.mfRotationY = value;
			URPCProxy(EPacketType.PT_InGame_PlayerRot, value);
		}
	}

	private void RPC_C2S_PlayerMovePlayerState(uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
		if (stream.TryRead<byte>(out var value))
		{
			mPlayerSynAttribute.mnPlayerState = (SpeedState)value;
			URPCProxy(EPacketType.PT_InGame_PlayerState, value);
		}
	}

	private void RPC_C2S_PlayerMoveGrounded(uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
		if (stream.TryRead<bool>(out var value))
		{
			mPlayerSynAttribute.mbGrounded = value;
			URPCProxy(EPacketType.PT_InGame_PlayerOnGround, value);
		}
	}

	private void RPC_C2S_PlayerMoveShootTarget(uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
		Vector3 vector = stream.Read<Vector3>(new object[0]);
		mPlayerSynAttribute.mv3shootTarget = vector;
		RPCProxy(EPacketType.PT_InGame_PlayerShootTarget, vector);
	}

	private void RPC_C2S_SyncGliderStatus(uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
		bool flag = stream.Read<bool>(new object[0]);
		RPCProxy(EPacketType.PT_InGame_GliderStatus, flag);
	}

	private void RPC_C2S_SyncParachuteStatus(uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
		bool flag = stream.Read<bool>(new object[0]);
		RPCProxy(EPacketType.PT_InGame_ParachuteStatus, flag);
	}

	private void RPC_C2S_SyncJetPackStatus(uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
		bool flag = stream.Read<bool>(new object[0]);
		RPCProxy(EPacketType.PT_InGame_JetPackStatus, flag);
	}

	public void SendFarmInfo()
	{
		ItemObject[] array = FarmManager.ExportItemObj(base.WorldId);
		if (array != null)
		{
			byte[] array2 = FarmManager.ExportToByte(base.WorldId);
			if (array2 != null && array2.Length > 0)
			{
				RPCOwner(EPacketType.PT_InGame_FarmInfo, array, array2, false);
			}
		}
	}

	private void RPC_C2S_Plant_GetBack(uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
		int num = stream.Read<int>(new object[0]);
		FarmPlantLogic plantByItemObjID = FarmManager.Instance.GetPlantByItemObjID(num);
		if (plantByItemObjID == null)
		{
			return;
		}
		int num2 = (int)((float)((int)(plantByItemObjID.mLife / 20.0) + 1) * 0.2f * (float)plantByItemObjID.mPlantInfo.mItemGetNum);
		Dictionary<int, int> dictionary = new Dictionary<int, int>();
		for (int i = 0; i < num2; i++)
		{
			float num3 = UnityEngine.Random.Range(0f, 1f);
			for (int j = 0; j < plantByItemObjID.mPlantInfo.mItemGetPro.Count; j++)
			{
				if (num3 < plantByItemObjID.mPlantInfo.mItemGetPro[j].m_probablity)
				{
					if (!dictionary.ContainsKey(plantByItemObjID.mPlantInfo.mItemGetPro[j].m_id))
					{
						dictionary[plantByItemObjID.mPlantInfo.mItemGetPro[j].m_id] = 0;
					}
					Dictionary<int, int> dictionary2;
					Dictionary<int, int> dictionary3 = (dictionary2 = dictionary);
					int id;
					int key = (id = plantByItemObjID.mPlantInfo.mItemGetPro[j].m_id);
					id = dictionary2[id];
					dictionary3[key] = id + 1;
				}
			}
		}
		List<ItemSample> list = new List<ItemSample>();
		List<ItemObject> list2 = new List<ItemObject>();
		foreach (int key2 in dictionary.Keys)
		{
			ItemObject itemObject = CreateItem(key2, dictionary[key2], syn: false);
			if (itemObject != null)
			{
				Package.AddItem(itemObject);
				list2.Add(itemObject);
				list.Add(new ItemSample(key2, dictionary[key2]));
			}
		}
		FarmManager.Instance.RemovePlant(num);
		if (list2.Count >= 1)
		{
			SyncItemList(list2.ToArray());
			SyncPackageIndex();
			SyncNewItem(list);
		}
		RPCOthers(EPacketType.PT_InGame_Plant_GetBack, num);
	}

	[Obsolete]
	private void RPC_C2S_Plant_PutOut(uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
		Vector3 vector = stream.Read<Vector3>(new object[0]);
		Quaternion quaternion = stream.Read<Quaternion>(new object[0]);
		int num = stream.Read<int>(new object[0]);
		int num2 = stream.Read<int>(new object[0]);
		int num3 = stream.Read<int>(new object[0]);
		Vector3 vector2 = stream.Read<Vector3>(new object[0]);
		int num4 = stream.Read<int>(new object[0]);
		byte b = stream.Read<byte>(new object[0]);
		ItemObject itemByID = ItemManager.GetItemByID(num);
		if (itemByID == null)
		{
			if (LogFilter.logDebug)
			{
				Debug.LogError("Item does not exsit.");
			}
			return;
		}
		if (!Package.ExistID(num))
		{
			if (LogFilter.logDebug)
			{
				Debug.LogError("Item does not exsit.");
			}
			return;
		}
		if (!GameWorld.CheckArea(base.WorldId, base.TeamId, vector, itemByID.protoId))
		{
			if (LogFilter.logDebug)
			{
				Debug.LogError("Put item disabled.");
			}
			return;
		}
		ItemObject itemObject = itemByID;
		if (itemByID.stackCount <= 1)
		{
			Package.RemoveItem(itemByID);
			SyncPackageIndex();
		}
		else
		{
			itemByID.CountDown(1);
			ItemObject itemObject2 = ItemManager.CreateFromItem(itemByID.protoId, 1, itemByID);
			if (itemObject2 == null)
			{
				return;
			}
			itemObject = itemObject2;
			ChannelNetwork.SyncItemList(101, new ItemObject[2] { itemByID, itemObject });
			num = itemObject2.instanceId;
		}
		FarmPlantLogic farmPlantLogic = FarmManager.Instance.GetPlantByItemObjID(num);
		if (farmPlantLogic == null)
		{
			farmPlantLogic = FarmManager.Instance.CreatePlant(base.WorldId, num, PlantInfo.GetPlantInfoByItemId(num4).mTypeID, vector2, quaternion, b);
		}
		farmPlantLogic.mPos = vector2;
		RPCOthers(EPacketType.PT_InGame_Plant_PutOut, vector, quaternion, num, num2, num3, vector2, num4, b);
	}

	private void RPC_C2S_Plant_VFTerrainTarget(uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
		IntVector3 intPos = stream.Read<IntVector3>(new object[0]);
		float radius = stream.Read<float>(new object[0]);
		byte targetType = stream.Read<byte>(new object[0]);
		byte[] array = stream.Read<byte[]>(new object[0]);
		GameWorld world = GameWorld.GetGameWorld(base.WorldId);
		if (world == null)
		{
			return;
		}
		Serialize.Import(array, delegate(BinaryReader _out)
		{
			int num = _out.ReadInt32();
			for (float num2 = 0f - radius; num2 <= radius; num2 += 1f)
			{
				for (float num3 = 0f - radius; num3 <= radius; num3 += 1f)
				{
					for (float num4 = 0f - radius; num4 <= radius; num4 += 1f)
					{
						IntVector3 pos = new IntVector3((float)intPos.x + num2, (float)intPos.y + num4, (float)intPos.z + num3);
						BufferHelper.ReadVFVoxel(_out, out var _value);
						world.ChangeTerrain(pos, targetType, _value);
					}
				}
			}
		});
		RPCOthers(EPacketType.PT_InGame_Plant_VFTerrainTarget, intPos, radius, targetType, array);
	}

	private void RPC_C2S_Plant_Water(uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
		int itemObjID = stream.Read<int>(new object[0]);
		FarmPlantLogic plantByItemObjID = FarmManager.Instance.GetPlantByItemObjID(itemObjID);
		if (plantByItemObjID != null)
		{
			int b = (int)(((double)plantByItemObjID.mPlantInfo.mWaterLevel[1] - plantByItemObjID.mWater) / 30.0);
			int itemNum = GetItemNum(1003);
			if (itemNum > 0)
			{
				plantByItemObjID.Watering(Mathf.Min(itemNum, b));
				List<ItemObject> effItems = new List<ItemObject>(10);
				Package.RemoveItem(1003, Mathf.Min(itemNum, b), ref effItems);
				SyncItemList(effItems);
				SyncPackageIndex();
				FarmManager.Instance.SyncPlant(plantByItemObjID);
			}
		}
	}

	private void RPC_C2S_Plant_Clean(uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
		int itemObjID = stream.Read<int>(new object[0]);
		FarmPlantLogic plantByItemObjID = FarmManager.Instance.GetPlantByItemObjID(itemObjID);
		if (plantByItemObjID != null)
		{
			int b = (int)(((double)plantByItemObjID.mPlantInfo.mCleanLevel[1] - plantByItemObjID.mClean) / 30.0);
			int itemNum = GetItemNum(1002);
			if (itemNum > 0)
			{
				plantByItemObjID.Cleaning(Mathf.Min(itemNum, b));
				List<ItemObject> effItems = new List<ItemObject>();
				Package.RemoveItem(1002, Mathf.Min(itemNum, b), ref effItems);
				SyncItemList(effItems);
				SyncPackageIndex();
				FarmManager.Instance.SyncPlant(plantByItemObjID);
			}
		}
	}

	private void RPC_C2S_Plant_Clear(uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
		int num = stream.Read<int>(new object[0]);
		FarmPlantLogic plantByItemObjID = FarmManager.Instance.GetPlantByItemObjID(num);
		if (plantByItemObjID != null)
		{
			FarmManager.Instance.RemovePlant(num);
			ItemManager.RemoveItem(num);
			RPCOthers(EPacketType.PT_InGame_Plant_Clear, num);
		}
	}

	public void GetOffTrainWhenDisconnect(int type, int passengerID)
	{
		int num = RailwayManager.Instance.DoRemovePassenger(type, passengerID);
		if (num != -1)
		{
			RPCProxy(EPacketType.PT_InGame_Railway_GetOffTrainEx, type, passengerID);
		}
	}

	public bool RequestAddPoint(int objID, Vector3 pos, Point.EType type, int prePointId = -1, int pointId = -1)
	{
		ItemObject itemByID = ItemManager.GetItemByID(objID);
		if (itemByID == null)
		{
			return false;
		}
		if (!Package.ExistID(itemByID))
		{
			return false;
		}
		if (!GameWorld.CheckArea(base.WorldId, base.TeamId, pos, itemByID.protoId))
		{
			return false;
		}
		ItemObject itemObject = itemByID;
		if (itemByID.stackCount <= 1)
		{
			Package.RemoveItem(itemByID);
			SyncPackageIndex();
		}
		else
		{
			itemByID.CountDown(1);
			ItemObject itemObject2 = ItemManager.CreateFromItem(itemByID.protoId, 1, itemByID);
			if (itemObject2 == null)
			{
				return false;
			}
			itemObject = itemObject2;
			SyncItemList(new ItemObject[2] { itemObject2, itemByID });
		}
		Point point = RailwayManager.Instance.GetPoint(prePointId);
		if (point != null && point.prePointId == -1 && point.nextPointId != -1)
		{
			Point.ReverseNext(point);
		}
		Point point2 = RailwayManager.Instance.AddPoint(pos, prePointId, type, pointId);
		if (point2 == null)
		{
			return false;
		}
		if (point2 != null)
		{
			point2.itemInstanceId = objID;
		}
		RailwayManager.Instance.SaveData();
		return true;
	}

	public void RequestChangePrePoint(int pointId, int preID)
	{
		Point point = RailwayManager.Instance.GetPoint(pointId);
		Point point2 = RailwayManager.Instance.GetPoint(preID);
		if (point2 != null && point2.prePointId == -1 && point2.nextPointId != -1)
		{
			List<Point> list = new List<Point>();
			list.Add(point);
			list.Insert(0, point2);
			while (point2.nextPointId != -1)
			{
				point2 = RailwayManager.Instance.GetPoint(point2.nextPointId);
				list.Insert(0, point2);
			}
			list[0].ChangePrePoint(-1);
			for (int i = 0; i < list.Count - 1; i++)
			{
				list[i].ChangeNextPoint(list[i + 1].id);
			}
		}
		else
		{
			point.ChangePrePoint(preID);
		}
		RailwayManager.Instance.SaveData();
	}

	public bool RequestRemovePoint(Player player, int pointID)
	{
		if (player == null)
		{
			return false;
		}
		Point point = RailwayManager.Instance.GetPoint(pointID);
		if (point != null)
		{
			ItemObject itemByID = ItemManager.GetItemByID(point.itemInstanceId);
			if (itemByID != null)
			{
				player.Package.AddItem(itemByID);
				RailwayManager.Instance.RemovePoint(player, pointID);
				RailwayManager.Instance.SaveData();
				return true;
			}
		}
		return false;
	}

	public void RequestChangeNextPoint(Point point, int nextID)
	{
		Point point2 = RailwayManager.Instance.GetPoint(nextID);
		if (point2 != null && point2.prePointId != -1 && point2.nextPointId == -1)
		{
			List<Point> list = new List<Point>();
			list.Add(point);
			list.Add(point2);
			while (point2.prePointId != -1)
			{
				point2 = RailwayManager.Instance.GetPoint(point2.prePointId);
				list.Add(point2);
			}
			for (int i = 0; i < list.Count - 1; i++)
			{
				list[i].ChangeNextPoint(list[i + 1].id);
			}
			list[list.Count - 1].ChangeNextPoint(-1);
		}
		else
		{
			point.ChangeNextPoint(nextID);
		}
		RailwayManager.Instance.SaveData();
	}

	public Route RequestCreateRoute(int pointId, string routeName)
	{
		if (!RailwayManager.Instance.IsPointInCompletedLine(pointId))
		{
			return null;
		}
		Point point = RailwayManager.Instance.GetPoint(pointId);
		if (point == null)
		{
			return null;
		}
		Point header = Point.GetHeader(point);
		if (header.pointType != Point.EType.End)
		{
			return null;
		}
		List<int> list = new List<int>();
		for (point = header; point != null; point = point.GetNextPoint())
		{
			list.Add(point.id);
		}
		Route result = RailwayManager.Instance.CreateRoute(routeName, list.ToArray());
		RailwayManager.Instance.SaveData();
		return result;
	}

	public bool RequestDeleteRoute(Player player, int routeId)
	{
		Route route = RailwayManager.Instance.GetRoute(routeId);
		if (route != null)
		{
			if (route.HasPassenger())
			{
				return false;
			}
			if (route.trainID != -1)
			{
				ItemObject itemByID = ItemManager.GetItemByID(route.trainID);
				if (itemByID != null && player.Package.GetEmptyIndex(itemByID.protoData) >= 0)
				{
					player.Package.AddItem(itemByID);
					player.SyncPackageIndex();
				}
			}
			bool result = RailwayManager.Instance.RemoveRoute(route.id);
			RailwayManager.Instance.SaveData();
			return result;
		}
		return false;
	}

	public bool RequestGetOnTrain(int routeId, int passengerID)
	{
		Route route = RailwayManager.Instance.GetRoute(routeId);
		if (route != null)
		{
			if (!route.AddPassenger(passengerID))
			{
				return false;
			}
			ObjNetInterface objNetInterface = ObjNetInterface.Get(passengerID);
			if (objNetInterface == null)
			{
				return false;
			}
			if ((objNetInterface as SkNetworkInterface)._OnTrain)
			{
				return false;
			}
			if (objNetInterface is Player)
			{
				((Player)objNetInterface)._OnTrain = true;
			}
			else if (objNetInterface is AiAdNpcNetwork)
			{
				((AiAdNpcNetwork)objNetInterface)._OnTrain = true;
			}
			RailwayManager.Instance.SaveData();
			return true;
		}
		return false;
	}

	public bool RequestGetOffTrain(int routeId, int passengerID, Vector3 pos)
	{
		Route route = RailwayManager.Instance.GetRoute(routeId);
		if (route != null)
		{
			RailwayTrain train = route.train;
			if (train == null)
			{
				return false;
			}
			ObjNetInterface objNetInterface = ObjNetInterface.Get(passengerID);
			if (objNetInterface == null)
			{
				return false;
			}
			if (!(objNetInterface as SkNetworkInterface)._OnTrain)
			{
				return false;
			}
			if (objNetInterface is Player)
			{
				((Player)objNetInterface)._OnTrain = false;
				((Player)objNetInterface).SetPosition(pos);
			}
			else if (objNetInterface is AiAdNpcNetwork)
			{
				((AiAdNpcNetwork)objNetInterface)._OnTrain = false;
			}
			bool result = route.RemovePassenger(passengerID);
			RailwayManager.Instance.SaveData();
			return result;
		}
		return false;
	}

	public bool RequestSetRouteTrain(Player player, int routeId, int objId)
	{
		ItemObject itemByID = ItemManager.GetItemByID(objId);
		if (itemByID == null || player == null)
		{
			return false;
		}
		Route route = RailwayManager.Instance.GetRoute(routeId);
		if (route != null)
		{
			if (route.HasPassenger())
			{
				return false;
			}
			ItemObject itemByID2 = ItemManager.GetItemByID(route.trainID);
			if (itemByID2 != null)
			{
				if (route.trainID == objId)
				{
					return false;
				}
				player.Package.AddItem(itemByID2);
			}
			route.trainID = objId;
			ItemObject itemByID3 = ItemManager.GetItemByID(objId);
			if (itemByID3 != null)
			{
				player.Package.RemoveItem(itemByID3);
				player.SyncPackageIndex();
				RailwayManager.Instance.SaveData();
				return true;
			}
			return false;
		}
		return false;
	}

	public bool RequestChangePointRot(int pointID, Vector3 rot)
	{
		Point point = RailwayManager.Instance.GetPoint(pointID);
		if (point != null)
		{
			point.rotation = rot;
			RailwayManager.Instance.SaveData();
			return true;
		}
		return false;
	}

	public bool RequestSetPointName(int pointID, string name)
	{
		Point point = RailwayManager.Instance.GetPoint(pointID);
		if (point != null)
		{
			point.name = name;
			return false;
		}
		RailwayManager.Instance.SaveData();
		return true;
	}

	public bool RequestSetRouteName(int routeID, string name)
	{
		Route route = RailwayManager.Instance.GetRoute(routeID);
		if (route != null)
		{
			route.name = name;
			RailwayManager.Instance.SaveData();
			return true;
		}
		return false;
	}

	public bool RequestSetPointStayTime(int pointID, float time)
	{
		Route routeByPointId = RailwayManager.Instance.GetRouteByPointId(pointID);
		if (routeByPointId == null)
		{
			return false;
		}
		routeByPointId.SetStayTime(pointID, time);
		RailwayManager.Instance.SaveData();
		return true;
	}

	public bool RequestRemoveTrain(Player player, int routeID)
	{
		Route route = RailwayManager.Instance.GetRoute(routeID);
		if (route != null && player != null)
		{
			if (route.trainID == -1 || route.HasPassenger())
			{
				return false;
			}
			ItemObject itemByID = ItemManager.GetItemByID(route.trainID);
			if (itemByID != null && player.Package.GetEmptyGridCount(itemByID.protoData) > 0)
			{
				player.Package.AddItem(itemByID);
				player.SyncPackageIndex();
				route.trainID = -1;
				RailwayManager.Instance.SaveData();
				return true;
			}
		}
		return false;
	}

	public void RequestSetTrainToStation(Player player, int routeId, int pointId)
	{
		RailwayManager.Instance.GetRoute(routeId)?.SetTrainToStation(pointId);
	}

	public bool RequestAutoCreateRoute(Player player, int pointID, int itemObjID)
	{
		Point point = RailwayManager.Instance.GetPoint(pointID);
		if (RequestCreateRoute(pointID, string.Empty) == null)
		{
			return false;
		}
		if (!RequestSetRouteTrain(player, point.routeId, itemObjID))
		{
			return false;
		}
		RequestSetTrainToStation(player, point.routeId, pointID);
		return true;
	}

	private void RPC_C2S_Railway_AddPoint(uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
		Vector3 vector = stream.Read<Vector3>(new object[0]);
		int num = stream.Read<int>(new object[0]);
		int num2 = stream.Read<int>(new object[0]);
		int objID = stream.Read<int>(new object[0]);
		if (RequestAddPoint(objID, vector, (Point.EType)num, num2))
		{
			RPCOthers(EPacketType.PT_InGame_Railway_AddPoint, vector, num, num2);
		}
	}

	private void RPC_C2S_Railway_PrePointChange(uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
		int num = stream.Read<int>(new object[0]);
		int num2 = stream.Read<int>(new object[0]);
		Point point = RailwayManager.Instance.GetPoint(num);
		if (point != null)
		{
			RequestChangePrePoint(num, num2);
			RPCOthers(EPacketType.PT_InGame_Railway_PrePointChange, num, num2);
		}
	}

	private void RPC_C2S_Railway_NextPointChange(uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
		int num = stream.Read<int>(new object[0]);
		int num2 = stream.Read<int>(new object[0]);
		Point point = RailwayManager.Instance.GetPoint(num);
		if (point != null)
		{
			RequestChangeNextPoint(point, num2);
			RPCOthers(EPacketType.PT_InGame_Railway_NextPointChange, num, num2);
		}
	}

	private void RPC_C2S_Railway_Route(uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
		int num = stream.Read<int>(new object[0]);
		string text = stream.Read<string>(new object[0]);
		if (RequestCreateRoute(num, text) != null)
		{
			RPCOthers(EPacketType.PT_InGame_Railway_Route, num, text);
		}
	}

	private void RPC_C2S_Railway_GetOnTrain(uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
		int num = stream.Read<int>(new object[0]);
		int num2 = stream.Read<int>(new object[0]);
		if (RequestGetOnTrain(num, num2))
		{
			RPCOthers(EPacketType.PT_InGame_Railway_GetOnTrain, num, num2);
		}
	}

	private void RPC_C2S_Railway_GetOffTrain(uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
		int num = stream.Read<int>(new object[0]);
		int num2 = stream.Read<int>(new object[0]);
		Vector3 vector = stream.Read<Vector3>(new object[0]);
		if (RequestGetOffTrain(num, num2, vector))
		{
			RPCOthers(EPacketType.PT_InGame_Railway_GetOffTrain, num, num2, vector);
		}
	}

	private void RPC_C2S_Railway_DeleteRoute(uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
		int num = stream.Read<int>(new object[0]);
		Route route = RailwayManager.Instance.GetRoute(num);
		if (route != null && !route.HasPassenger())
		{
			Player player = GetPlayer(info.sender);
			if (player != null && RequestDeleteRoute(player, num))
			{
				RPCOthers(EPacketType.PT_InGame_Railway_DeleteRoute, num);
			}
		}
	}

	private void RPC_C2S_Railway_Recycle(uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
		int num = stream.Read<int>(new object[0]);
		Player player = GetPlayer(info.sender);
		if (player != null && RequestRemovePoint(player, num))
		{
			RPCOthers(EPacketType.PT_InGame_Railway_Recycle, num);
		}
	}

	private void RPC_C2S_Railway_SetRouteTrain(uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
		int num = stream.Read<int>(new object[0]);
		int num2 = stream.Read<int>(new object[0]);
		Player player = GetPlayer(info.sender);
		if (player != null && RequestSetRouteTrain(player, num, num2))
		{
			RPCOthers(EPacketType.PT_InGame_Railway_SetRouteTrain, num, num2);
		}
	}

	private void RPC_C2S_Railway_ChangeStationRot(uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
		int num = stream.Read<int>(new object[0]);
		Vector3 vector = stream.Read<Vector3>(new object[0]);
		if (RequestChangePointRot(num, vector))
		{
			RPCOthers(EPacketType.PT_InGame_Railway_ChangeStationRot, num, vector);
		}
	}

	private void RPC_C2S_Railway_ResetPointName(uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
		int num = stream.Read<int>(new object[0]);
		string text = stream.Read<string>(new object[0]);
		if (RequestSetPointName(num, text))
		{
			RPCOthers(EPacketType.PT_InGame_Railway_ResetPointName, num, text);
		}
	}

	private void RPC_C2S_Railway_ResetRouteName(uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
		int num = stream.Read<int>(new object[0]);
		string text = stream.Read<string>(new object[0]);
		if (RequestSetRouteName(num, text))
		{
			RPCOthers(EPacketType.PT_InGame_Railway_ResetRouteName, num, text);
		}
	}

	private void RPC_C2S_Railway_ResetPointTime(uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
		int num = stream.Read<int>(new object[0]);
		float num2 = stream.Read<float>(new object[0]);
		if (RequestSetPointStayTime(num, num2))
		{
			RPCOthers(EPacketType.PT_InGame_Railway_ResetPointTime, num, num2);
		}
	}

	private void RPC_C2S_Railway_AutoCreateRoute(uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
		int num = stream.Read<int>(new object[0]);
		int num2 = stream.Read<int>(new object[0]);
		Player player = GetPlayer(info.sender);
		if (player != null && RequestAutoCreateRoute(player, num, num2))
		{
			RPCOthers(EPacketType.PT_InGame_Railway_AutoCreateRoute, num, num2);
		}
	}

	private void RPC_C2S_EnterDungeon(uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
		Vector3 entrancePos = stream.Read<Vector3>(new object[0]);
		enterPos = Pos;
		bool flag = false;
		if (RandomDunGenMgr.Instance.EnterDungeon(entrancePos, enterPos, out var genPos, out var seed, out var dungeonId, out var dungeonDataId))
		{
			flag = true;
			RPCOwner(EPacketType.PT_InGame_EnterDungeon, flag, genPos, seed, dungeonId, dungeonDataId);
		}
		else
		{
			RPCOwner(EPacketType.PT_InGame_EnterDungeon, flag);
		}
	}

	private void RPC_C2S_ExitDungeon(uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
		RandomDunGenMgr.Instance.ExitDungeon();
		RPCOwner(EPacketType.PT_InGame_ExitDungeon);
	}

	private void RPC_C2S_UploadDungeonSeed(uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
		Vector3 entrancePos = stream.Read<Vector3>(new object[0]);
		int seed = stream.Read<int>(new object[0]);
		RandomDunGenMgr.Instance.SetSeed(entrancePos, seed);
	}

	private void RPC_C2S_GenDunEntrance(uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
		Vector3 vector = stream.Read<Vector3>(new object[0]);
		int id = stream.Read<int>(new object[0]);
		DungeonBaseData dataFromId = RandomDungeonDataBase.GetDataFromId(id);
		if (dataFromId != null && RandomDunGenMgr.Instance.GenEntrance(vector, dataFromId))
		{
			RPCOthers(EPacketType.PT_InGame_GenDunEntrance, vector, dataFromId.id);
		}
	}

	public void SyncDungeonEntrance(int worldId)
	{
		if (worldId != RandomDunGenMgr.Instance.genWorld)
		{
			return;
		}
		List<Vector3> list = new List<Vector3>();
		List<int> list2 = new List<int>();
		foreach (RandomDungeonData value in RandomDunGenMgrData.allDungeons.Values)
		{
			list.Add(value.entrancePos);
			list2.Add(value.dungeonBaseDataId);
		}
		if (list.Count > 0)
		{
			RPCOwner(EPacketType.PT_InGame_GenDunEntranceList, list.ToArray(), list2.ToArray());
		}
	}

	public void SyncInitWhenSpawn()
	{
		RPCOwner(EPacketType.PT_InGame_InitWhenSpawn);
	}

	private void RPC_C2S_GenRandomItem(uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
		Vector3 vector = stream.Read<Vector3>(new object[0]);
		int num = stream.Read<int>(new object[0]);
		if (RandomItemMgr.Instance.ContainsPos(vector) || (vector.y >= 0f && (!RandomItemMgr.Instance.IsAreaAvalable(vector) || !RandomItemMgr.Instance.IsBoxNumAvailable(num))))
		{
			return;
		}
		System.Random random = new System.Random((int)(DateTime.UtcNow.Ticks + (int)(vector.x + vector.y + vector.z)));
		string path;
		List<ItemIdCount> list = RandomItemDataMgr.GenItemDicByBoxId(num, out path, random);
		if (list == null || list.Count <= 0)
		{
			if (LogFilter.logDebug)
			{
				Debug.LogError("boxId error: " + num);
			}
			return;
		}
		int[] array = new int[list.Count * 2];
		int num2 = 0;
		foreach (ItemIdCount item in list)
		{
			array[num2++] = item.protoId;
			array[num2++] = item.count;
		}
		Quaternion quaternion = Quaternion.Euler(0f, random.Next(360), 0f);
		RandomItemMgr.Instance.AddRandomItem(num, vector, quaternion, path, array.ToList());
		RPCOthers(EPacketType.PT_InGame_RandomItem, vector, quaternion, num, array, false);
	}

	private void RPC_C2S_GenRandomItemRare(uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
		Vector3 vector = stream.Read<Vector3>(new object[0]);
		int num = stream.Read<int>(new object[0]);
		if (RandomItemMgr.Instance.ContainsPos(vector) || (vector.y >= 0f && (!RandomItemMgr.Instance.IsAreaAvalable(vector) || !RandomItemMgr.Instance.IsBoxNumAvailable(num))))
		{
			return;
		}
		System.Random random = new System.Random((int)(DateTime.UtcNow.Ticks + (int)(vector.x + vector.y + vector.z)));
		string path;
		List<ItemIdCount> list = RandomItemDataMgr.GenItemDicByBoxId(num, out path, random);
		if (list == null || list.Count <= 0)
		{
			if (LogFilter.logDebug)
			{
				Debug.LogError("boxId error: " + num);
			}
			return;
		}
		int[] array = new int[list.Count * 2];
		int num2 = 0;
		foreach (ItemIdCount item in list)
		{
			array[num2++] = item.protoId;
			array[num2++] = item.count;
		}
		Quaternion quaternion = Quaternion.Euler(0f, random.Next(360), 0f);
		RandomItemMgr.Instance.AddRandomItem(num, vector, quaternion, path, array.ToList());
		RPCOthers(EPacketType.PT_InGame_RandomItemRare, vector, quaternion, num, array, false);
		RPCOwner(EPacketType.PT_InGame_RandomIsoCode, vector);
	}

	private void RPC_C2S_GenRandomRareAry(uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
		Vector3 entrancePos = stream.Read<Vector3>(new object[0]);
		int num = stream.Read<int>(new object[0]);
		string isoTag = stream.Read<string>(new object[0]);
		List<Vector3> list = stream.Read<Vector3[]>(new object[0]).ToList();
		List<int> list2 = stream.Read<int[]>(new object[0]).ToList();
		List<ItemIdCount> list3 = stream.Read<ItemIdCount[]>(new object[0]).ToList();
		RandomDungeonData dungeonData = RandomDunGenMgrData.GetDungeonData(entrancePos);
		List<Vector3> list4 = new List<Vector3>();
		List<Quaternion> list5 = new List<Quaternion>();
		List<int> list6 = new List<int>();
		List<int> list7 = new List<int>();
		List<int> list8 = new List<int>();
		int num2 = Mathf.Min(list.Count, list2.Count);
		for (int i = 0; i < num2; i++)
		{
			Vector3 vector = list[i];
			int num3 = list2[i];
			if (RandomItemMgr.Instance.ContainsPos(vector) || (vector.y >= 0f && (!RandomItemMgr.Instance.IsAreaAvalable(vector) || !RandomItemMgr.Instance.IsBoxNumAvailable(num3))))
			{
				continue;
			}
			System.Random random = new System.Random((int)(DateTime.UtcNow.Ticks + (int)(vector.x + vector.y + vector.z)));
			string path;
			List<ItemIdCount> list9 = RandomItemDataMgr.GenItemDicByBoxId(num3, out path, random);
			if (list9 == null || list9.Count <= 0)
			{
				if (LogFilter.logDebug)
				{
					Debug.LogError("boxId error: " + num3);
				}
				continue;
			}
			if (list3[0].protoId >= 0)
			{
				list9.AddRange(list3);
			}
			int[] array = new int[list9.Count * 2];
			int num4 = 0;
			foreach (ItemIdCount item in list9)
			{
				array[num4++] = item.protoId;
				array[num4++] = item.count;
			}
			Quaternion quaternion = Quaternion.Euler(0f, random.Next(360), 0f);
			RandomItemObj randomItemObj = RandomItemMgr.Instance.AddRandomItem(num3, vector, quaternion, path, array.ToList());
			randomItemObj.AddRareProto(1, 1);
			RandomDunGenMgrData.AddRareItem(dungeonData.id, randomItemObj);
			list4.Add(vector);
			list5.Add(quaternion);
			list6.Add(num3);
			list7.Add(array.Count());
			list8.AddRange(array);
		}
		if (list4.Count > 0)
		{
			RPCOthers(EPacketType.PT_InGame_RandomItemRareAry, list4.ToArray(), list5.ToArray(), list6.ToArray(), list7.ToArray(), list8.ToArray());
			SteamWorks.SendGetRandIsoIds(dungeonData.id, num, isoTag, this);
			if (LogFilter.logDebug)
			{
				Debug.LogError("SendGetRandIsoIds" + dungeonData.id + "," + num);
			}
		}
	}

	private void RPC_C2S_RandomItemFetch(uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
		Vector3 vector = stream.Read<Vector3>(new object[0]);
		int num = stream.Read<int>(new object[0]);
		int num2 = stream.Read<int>(new object[0]);
		int num3 = stream.Read<int>(new object[0]);
		RandomItemObj randomItemObj = RandomItemMgr.Instance.GetRandomItemObj(vector);
		if (randomItemObj != null && Package.CanAdd(num2, num3) && randomItemObj.TryFetch(num, num2, num3))
		{
			List<ItemObject> effItems = new List<ItemObject>();
			Package.AddSameItems(num2, num3, ref effItems);
			SyncItemList(effItems);
			SyncPackageIndex();
			SyncNewItem(new ItemSample(num2, num3));
			RPCOthers(EPacketType.PT_InGame_RandomItemFetch, vector, num, num2, num3);
		}
	}

	private void RPC_C2S_RandomItemFetchAll(uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
		Vector3 vector = stream.Read<Vector3>(new object[0]);
		RandomItemObj randomItemObj = RandomItemMgr.Instance.GetRandomItemObj(vector);
		if (randomItemObj == null)
		{
			return;
		}
		List<ItemIdCount> list = randomItemObj.TryFetchAll();
		if (list.Count == 0)
		{
			return;
		}
		List<ItemSample> list2 = new List<ItemSample>();
		foreach (ItemIdCount item in list)
		{
			list2.Add(new ItemSample(item.protoId, item.count));
		}
		if (Package.CanAdd(list2))
		{
			ItemObject[] items = Package.AddSameItems(list2);
			SyncItemList(items);
			SyncPackageIndex();
			SyncNewItem(list2);
			RPCOthers(EPacketType.PT_InGame_RandomItemFetchAll, vector);
		}
	}

	private void RPC_C2S_RandomFeces(uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
		Vector3 vector = stream.Read<Vector3>(new object[0]);
		if (!RandomItemMgr.Instance.ContainsPos(vector) && (!(vector.y >= 0f) || RandomItemMgr.Instance.IsAreaAvalableForFeces(vector)))
		{
			System.Random random = new System.Random();
			string modelPath;
			int[] array = RandomFecesDataMgr.GenFecesItemIdCount(out modelPath);
			Quaternion quaternion = Quaternion.Euler(0f, new System.Random().Next(360), 0f);
			if (RandomItemMgr.Instance.AddFeces(vector, quaternion, array))
			{
				RPCOthers(EPacketType.PT_InGame_RandomFeces, vector, quaternion, array);
			}
		}
	}

	private void RPC_C2S_RandomItemClicked(uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
		Vector3 vector = stream.Read<Vector3>(new object[0]);
		Vector3 pos = stream.Read<Vector3>(new object[0]);
		RandomItemObj randomItemObj = RandomItemMgr.Instance.GetRandomItemObj(vector);
		if (randomItemObj == null)
		{
			return;
		}
		int[] items = randomItemObj.items;
		RandomItemMgr.Instance.RemoveRandomItemObj(randomItemObj);
		List<ItemSample> list = new List<ItemSample>();
		List<ItemObject> effItems = new List<ItemObject>();
		for (int i = 0; i < items.Length; i += 2)
		{
			list.Add(new ItemSample(items[i], items[i + 1]));
		}
		ItemManager.CreateItems(list, ref effItems);
		foreach (int item in randomItemObj.rareItemInstance)
		{
			effItems.Add(ItemManager.GetItemByID(item));
		}
		if (effItems.Count > 0)
		{
			ChannelNetwork.SyncItemList(base.WorldId, effItems);
		}
		SceneDropItem.CreateDropItems(base.WorldId, effItems, pos, Vector3.zero, base.transform.rotation);
		RPCOthers(EPacketType.PT_InGame_RandomItemDestroy, vector);
	}

	public void SyncRandomItem(int worldId)
	{
		if (worldId != RandomDunGenMgr.Instance.genWorld)
		{
			return;
		}
		RandomItemObj[] allRandomItemObjs = RandomItemMgr.Instance.AllRandomItemObjs;
		if (allRandomItemObjs != null && allRandomItemObjs.Length > 0)
		{
			RandomItemObj[] array = allRandomItemObjs;
			foreach (RandomItemObj randomItemObj in array)
			{
				RPCOwner(EPacketType.PT_InGame_RandomItem, randomItemObj.Pos, randomItemObj.rot, randomItemObj.boxId, randomItemObj.items, false);
			}
		}
	}

	public void SyncDelRio(List<Vector3> posList)
	{
		RPCOthers(EPacketType.PT_InGame_RandomItemDestroyList, posList.ToArray());
	}

	public void SyncRoomInfo()
	{
		RPCOwner(EPacketType.PT_Common_InitAdminData, ServerAdministrator.SerializeAdminData());
	}

	private void RPC_C2S_AddBlackList(uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
		int num = stream.Read<int>(new object[0]);
		if (num == base.Id || !ServerAdministrator.IsAssistant(base.Id) || ServerAdministrator.IsBlack(num))
		{
			return;
		}
		if (ServerAdministrator.IsAssistant(num))
		{
			if (!ServerAdministrator.IsAdmin(base.Id))
			{
				return;
			}
			ServerAdministrator.DeleteAssistant(num);
		}
		ServerAdministrator.AddBlacklist(num);
		RPCOthers(EPacketType.PT_InGame_AddBlackList, num);
		Player player = GetPlayer(num);
		if (null != player)
		{
			NetInterface.CloseConnection(player.OwnerView.owner);
		}
	}

	private void RPC_C2S_DeleteBlackList(uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
		int num = stream.Read<int>(new object[0]);
		if (num != base.Id && ServerAdministrator.IsAssistant(base.Id) && ServerAdministrator.IsBlack(num))
		{
			ServerAdministrator.DeleteBlacklist(num);
			RPCOthers(EPacketType.PT_InGame_DelBlackList, num);
		}
	}

	private void RPC_C2S_ClearBlackList(uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
		if (ServerAdministrator.IsAssistant(base.Id))
		{
			ServerAdministrator.ClearBlacklist();
			RPCOthers(EPacketType.PT_InGame_ClearBlackList);
		}
	}

	private void RPC_C2S_AddAssistants(uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
		int num = stream.Read<int>(new object[0]);
		if (num != base.Id && ServerAdministrator.IsAdmin(base.Id) && !ServerAdministrator.IsAssistant(num))
		{
			ServerAdministrator.AddAssistant(num);
			RPCOthers(EPacketType.PT_InGame_AddAssistant, num);
		}
	}

	private void RPC_C2S_DeleteAssistants(uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
		int num = stream.Read<int>(new object[0]);
		if (num != base.Id && ServerAdministrator.IsAdmin(base.Id) && ServerAdministrator.IsAssistant(num))
		{
			ServerAdministrator.DeleteAssistant(num);
			RPCOthers(EPacketType.PT_InGame_DelAssistant, num);
		}
	}

	private void RPC_C2S_ClearAssistants(uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
		if (ServerAdministrator.IsAdmin(base.Id))
		{
			ServerAdministrator.ClearAssistant();
			RPCOthers(EPacketType.PT_InGame_ClearAssistant);
		}
	}

	private void RPC_C2S_BuildLock(uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
		int num = stream.Read<int>(new object[0]);
		if (num != base.Id && ServerAdministrator.IsAssistant(base.Id) && (!ServerAdministrator.IsAssistant(num) || ServerAdministrator.IsAdmin(base.Id)))
		{
			ServerAdministrator.LockBuild(num);
			RPCOthers(EPacketType.PT_InGame_BuildLock, num);
		}
	}

	private void RPC_C2S_BuildUnLock(uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
		int num = stream.Read<int>(new object[0]);
		if (num != base.Id && ServerAdministrator.IsAssistant(base.Id))
		{
			ServerAdministrator.BuildUnLock(num);
			RPCOthers(EPacketType.PT_InGame_BuildUnLock, num);
		}
	}

	private void RPC_C2S_ClearBuildLock(uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
		if (ServerAdministrator.IsAssistant(base.Id))
		{
			ServerAdministrator.ClearBuildLock();
			RPCOthers(EPacketType.PT_InGame_ClearBuildLock);
		}
	}

	private void RPC_C2S_ClearVoxelData(uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
		int num = stream.Read<int>(new object[0]);
		if (ServerAdministrator.IsAssistant(base.Id))
		{
			GameWorld gameWorld = GameWorld.GetGameWorld(base.WorldId);
			if (gameWorld != null)
			{
				gameWorld.Clear(num);
				RPCOthers(EPacketType.PT_InGame_ClearVoxel, num);
			}
		}
	}

	private void RPC_C2S_ClearAllVoxelData(uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
		if (ServerAdministrator.IsAssistant(base.Id))
		{
			GameWorld gameWorld = GameWorld.GetGameWorld(base.WorldId);
			if (gameWorld != null)
			{
				gameWorld.Clear();
				RPCOthers(EPacketType.PT_InGame_ClearAllVoxel);
			}
		}
	}

	private void RPC_C2S_LockArea(uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
		int num = stream.Read<int>(new object[0]);
		if (ServerAdministrator.IsAssistant(base.Id))
		{
			ServerAdministrator.LockArea(num);
			RPCOthers(EPacketType.PT_InGame_AreaLock, num);
		}
	}

	private void RPC_C2S_UnLockArea(uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
		int num = stream.Read<int>(new object[0]);
		if (ServerAdministrator.IsAssistant(base.Id))
		{
			ServerAdministrator.UnLockArea(num);
			RPCOthers(EPacketType.PT_InGame_AreaUnLock, num);
		}
	}

	private void RPC_C2S_BuildChunk(uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
		bool flag = stream.Read<bool>(new object[0]);
		if (ServerAdministrator.IsAdmin(base.Id))
		{
			ServerAdministrator.AllowModify = flag;
			RPCOthers(EPacketType.PT_InGame_BlockLock, flag);
		}
	}

	private void RPC_C2S_JoinGame(uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
		bool flag = stream.Read<bool>(new object[0]);
		if (ServerAdministrator.IsAdmin(base.Id))
		{
			ServerAdministrator.AllowJoin = flag;
			RPCOthers(EPacketType.PT_InGame_LoginBan, flag);
		}
	}

	private void RPC_C2S_KickPlayer(uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
		if (ServerAdministrator.IsAdmin(base.Id))
		{
			uLink.NetworkPlayer[] connections = uLink.Network.connections;
			foreach (uLink.NetworkPlayer peer in connections)
			{
				NetInterface.CloseConnection(peer);
			}
		}
	}

	public void ReduceMoney(int num)
	{
		SubGameMoney(num);
		SyncUserMoney();
	}

	public void AddMoney(int num)
	{
		AddGameMoney(num);
		SyncUserMoney();
	}

	public void SyncUserMoney()
	{
		RPCOwner(EPacketType.PT_InGame_PlayerMoney, GameMoney);
	}

	public void SyncMoneyType()
	{
		RPCOwner(EPacketType.PT_InGame_MoneyType, ServerConfig.MoneyType);
	}

	public void SyncCurSceneId()
	{
		RPCOwner(EPacketType.PT_InGame_CurSceneId, _curSceneId);
	}

	public void SyncRepurchaseItemIDs(int npcid)
	{
		List<ItemObject> source = _repurchaseItems[npcid];
		IEnumerable<int> source2 = source.Select((ItemObject iter) => iter.instanceId);
		RPCOwner(EPacketType.PT_InGame_RepurchaseIndex, source2.ToArray());
	}

	public void SyncShopItemIds(int npcId, int[] itemIds)
	{
		if (itemIds != null && itemIds.Length > 0)
		{
			RPCOwner(EPacketType.PT_InGame_InitShop, npcId, itemIds);
		}
	}

	private void RPC_C2S_GetShop(uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
		int num = stream.Read<int>(new object[0]);
		AiAdNpcNetwork aiAdNpcNetwork = ObjNetInterface.Get<AiAdNpcNetwork>(num);
		if (null == aiAdNpcNetwork)
		{
			return;
		}
		int num2 = -1;
		if (ServerConfig.IsStory)
		{
			NpcMissionData missionData = NpcMissionDataRepository.GetMissionData(aiAdNpcNetwork.Id);
			if (missionData == null)
			{
				return;
			}
			num2 = aiAdNpcNetwork.Id;
		}
		else
		{
			AdNpcData adNpcData = NpcMissionDataRepository.GetAdNpcData(aiAdNpcNetwork.ExternId);
			if (adNpcData == null)
			{
				return;
			}
			num2 = adNpcData.mRnpc_ID;
		}
		if (ShopManager.HasNpcShop(num))
		{
			List<ItemObject> items = ShopManager.RefreshNpcShop(num);
			SyncItemList(items);
		}
		else
		{
			ItemObject[] npcShopData = ShopManager.GetNpcShopData(num, num2);
			SyncItemList(npcShopData);
			aiAdNpcNetwork.SyncMoney();
		}
		ItemObject[] npcShopData2 = ShopManager.GetNpcShopData(num, num2);
		int[] itemIds = npcShopData2.Select((ItemObject iter) => iter.instanceId).ToArray();
		SyncShopItemIds(num, itemIds);
		List<int> npcStoreId = ShopManager.GetNpcStoreId(num, num2);
		ColonyMgr.AddStore(npcStoreId, base.TeamId);
	}

	private void RPC_C2S_BuyItemInMulti(uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
		int num = stream.Read<int>(new object[0]);
		int instanceId = stream.Read<int>(new object[0]);
		int num2 = stream.Read<int>(new object[0]);
		if (0 >= num2)
		{
			return;
		}
		AiAdNpcNetwork aiAdNpcNetwork = ObjNetInterface.Get<AiAdNpcNetwork>(num);
		if (null == aiAdNpcNetwork)
		{
			return;
		}
		int shopId;
		Record.stShopData npcShopItem = ShopManager.GetNpcShopItem(num, instanceId, out shopId);
		if (npcShopItem == null)
		{
			if (LogFilter.logDebug)
			{
				Debug.LogError("RPC_C2S_BuyItemInMulti: shopData null!");
			}
			return;
		}
		ItemObject itemByID = ItemManager.GetItemByID(npcShopItem.ItemObjID);
		if (itemByID == null || itemByID.protoData == null)
		{
			if (LogFilter.logDebug)
			{
				Debug.LogWarning("Sold out.");
			}
			SyncErrorMsg("Sold out.");
			return;
		}
		if (itemByID.stackCount <= 0)
		{
			if (LogFilter.logDebug)
			{
				Debug.LogWarning("Sold out.");
			}
			SyncErrorMsg("Sold out.");
			return;
		}
		if (!Package.CanAdd(itemByID.protoId, num2))
		{
			if (LogFilter.logDebug)
			{
				Debug.LogWarning("Not enough space.");
			}
			SyncErrorMsg("Not enough space.");
			return;
		}
		if (num2 > itemByID.stackCount)
		{
			if (LogFilter.logDebug)
			{
				Debug.LogWarning("Not enough count.");
			}
			SyncErrorMsg("Not enough count.");
			return;
		}
		ShopData shopData = ShopRespository.GetShopData(shopId);
		if (shopData == null)
		{
			return;
		}
		int num3 = shopData.m_Price * num2;
		if (GameMoney < num3)
		{
			if (LogFilter.logDebug)
			{
				Debug.LogWarning("Not enough money.");
			}
			SyncErrorMsg("Not enough money.");
			return;
		}
		List<ItemObject> effItems = new List<ItemObject>();
		if (itemByID.MaxStackNum == 1)
		{
			ItemManager.CreateFromItemList(itemByID.protoId, num2, itemByID, ref effItems);
			if (effItems.Count >= 1)
			{
				Package.AddItemList(effItems);
			}
		}
		else
		{
			Package.AddSameItems(itemByID.protoId, num2, ref effItems);
		}
		SyncItemList(effItems);
		if (num2 < itemByID.stackCount)
		{
			itemByID.CountDown(num2);
			ChannelNetwork.SyncItem(base.WorldId, itemByID);
		}
		else
		{
			ItemManager.RemoveItem(itemByID.instanceId);
			npcShopItem.ItemObjID = -1;
		}
		aiAdNpcNetwork.Money.Add(num3);
		aiAdNpcNetwork.SyncMoney();
		SyncPackageIndex();
		SyncNewItem(new ItemSample[1]
		{
			new ItemSample(itemByID.protoId, num2)
		});
		ReduceMoney(num3);
		List<ItemObject> list = ShopManager.RefreshNpcShop(num);
		SyncItemList(list);
		int[] itemIds = list.Select((ItemObject iter) => iter.instanceId).ToArray();
		SyncShopItemIds(num, itemIds);
	}

	private void RPC_C2S_Repurchase(uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
		int num = stream.Read<int>(new object[0]);
		int objId = stream.Read<int>(new object[0]);
		int num2 = stream.Read<int>(new object[0]);
		ItemObject itemByID = ItemManager.GetItemByID(objId);
		if (itemByID == null || itemByID.protoData == null || !_repurchaseItems.ContainsKey(num) || _repurchaseItems.Count((KeyValuePair<int, List<ItemObject>> iter) => iter.Value.Exists((ItemObject o) => o.instanceId == objId)) <= 0)
		{
			return;
		}
		if (num2 > itemByID.stackCount)
		{
			num2 = itemByID.stackCount;
		}
		int num3 = itemByID.GetSellPrice() * num2;
		if (GameMoney < num3)
		{
			if (LogFilter.logDebug)
			{
				Debug.LogWarning("Not enough money.");
			}
			SyncErrorMsg("Not enough money.");
			return;
		}
		if (!Package.CanAdd(itemByID.protoId, num2))
		{
			if (LogFilter.logDebug)
			{
				Debug.LogWarning("Not enough space.");
			}
			SyncErrorMsg("Not enough space.");
			return;
		}
		AiAdNpcNetwork aiAdNpcNetwork = ObjNetInterface.Get<AiAdNpcNetwork>(num);
		if (null != aiAdNpcNetwork)
		{
			aiAdNpcNetwork.Money.Add(num3);
			aiAdNpcNetwork.SyncMoney();
		}
		ReduceMoney(num3);
		if (itemByID.MaxStackNum == 1)
		{
			if (num2 == itemByID.stackCount)
			{
				Package.AddItem(itemByID);
				_repurchaseItems[num].Remove(itemByID);
			}
			else
			{
				List<ItemObject> effItems = new List<ItemObject>();
				Package.AddSameItems(itemByID.protoId, num2, ref effItems);
				itemByID.CountDown(num2);
				effItems.Add(itemByID);
				SyncItemList(effItems);
			}
		}
		else
		{
			if (num2 == itemByID.stackCount)
			{
				_repurchaseItems[num].Remove(itemByID);
			}
			List<ItemObject> effItems2 = new List<ItemObject>();
			Package.AddSameItems(itemByID.protoId, num2, ref effItems2);
			itemByID.CountDown(num2);
			effItems2.Add(itemByID);
			SyncItemList(effItems2);
		}
		SyncRepurchaseItemIDs(num);
		SyncPackageIndex();
	}

	private void RPC_C2S_ChangeCurrency(uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
		int num = stream.Read<int>(new object[0]);
		if (ServerConfig.MoneyType == (EMoneyType)num)
		{
			return;
		}
		foreach (KeyValuePair<int, ObjNetInterface> netObj in ObjNetInterface._netObjs)
		{
			if (netObj.Value is AiAdNpcNetwork)
			{
				(netObj.Value as AiAdNpcNetwork).Money.Current = (netObj.Value as AiAdNpcNetwork).Money.Current * 4;
			}
		}
		ServerConfig.MoneyType = (EMoneyType)num;
		RPCOthers(EPacketType.PT_InGame_ChangeCurrency, num);
	}

	private void RPC_C2S_SellItemInMulti(uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
		int num = stream.Read<int>(new object[0]);
		int objId = stream.Read<int>(new object[0]);
		int num2 = stream.Read<int>(new object[0]);
		ItemObject itemById = Package.GetItemById(objId);
		if (itemById == null)
		{
			return;
		}
		if (itemById.stackCount < num2)
		{
			num2 = itemById.stackCount;
		}
		int num3 = num2 * itemById.GetSellPrice();
		AiAdNpcNetwork aiAdNpcNetwork = ObjNetInterface.Get<AiAdNpcNetwork>(num);
		if (aiAdNpcNetwork == null)
		{
			return;
		}
		int current = aiAdNpcNetwork.Money.Current;
		if (current < num3)
		{
			if (LogFilter.logDebug)
			{
				Debug.LogWarning("Npc does not have enough money.");
			}
			SyncErrorMsg("Npc does not have enough money.");
			return;
		}
		if (!_repurchaseItems.ContainsKey(num))
		{
			_repurchaseItems.Add(num, new List<ItemObject>());
		}
		if (itemById.stackCount <= 1 || itemById.stackCount == num2)
		{
			_repurchaseItems[num].Add(itemById);
			Package.RemoveItem(itemById);
		}
		else
		{
			ItemObject itemObject = ItemManager.CreateFromItem(itemById.protoId, num2, itemById);
			_repurchaseItems[num].Add(itemObject);
			itemById.CountDown(num2);
			SyncItemList(new ItemObject[2] { itemObject, itemById });
		}
		aiAdNpcNetwork.Money.Remove(num3);
		aiAdNpcNetwork.SyncMoney();
		AddMoney(num3);
		SyncPackageIndex();
		SyncRepurchaseItemIDs(num);
	}

	public bool ApplyMetalScan(int[] ids)
	{
		bool result = false;
		PublicData.Self.bChanged = true;
		foreach (int item in ids)
		{
			if (!MetalScanList.Contains(item))
			{
				MetalScanList.Add(item);
				result = true;
			}
		}
		return result;
	}

	public void ApplyDurabilityReduce(int Type)
	{
		List<ItemObject> list = new List<ItemObject>();
		if (Type == 0)
		{
			return;
		}
		ItemObject[] equipItems = EquipModule.EquipItems;
		foreach (ItemObject itemObject in equipItems)
		{
			switch (itemObject.protoData.equipType)
			{
			}
		}
	}

	public void SyncMetalScan(bool openWnd = false)
	{
		RPCOwner(EPacketType.PT_InGame_MetalScanList, MetalScanList.ToArray(), openWnd);
	}

	public void SyncTeamMetalScan(bool openWnd = true)
	{
		if (MetalScanList != null)
		{
			SyncGroupData(EPacketType.PT_InGame_MetalScanList, MetalScanList.ToArray(), openWnd);
		}
	}

	private void RPC_C2S_SkillCast(uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
		int num = stream.Read<int>(new object[0]);
		uLink.NetworkViewID networkViewID = stream.Read<uLink.NetworkViewID>(new object[0]);
		RPCProxy(EPacketType.PT_InGame_SkillCast, num, networkViewID);
	}

	private void RPC_C2S_SkillCastShoot(uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
		int num = stream.Read<int>(new object[0]);
		Vector3 vector = stream.Read<Vector3>(new object[0]);
		RPCProxy(EPacketType.PT_InGame_SkillShoot, num, vector);
	}

	private void RPC_C2S_WaterPitcher(uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
		ItemProto itemData = ItemProto.GetItemData(1003);
		if (itemData != null)
		{
			List<ItemObject> effItems = new List<ItemObject>(10);
			Package.AddSameItems(1003, 10, ref effItems);
			SyncItemList(effItems);
			ItemSample itemSample = new ItemSample(1003, 10);
			SyncNewItem(new ItemSample[1] { itemSample });
			SyncPackageIndex();
		}
	}

	private void RPC_C2S_WaterPump(uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
		ItemProto itemData = ItemProto.GetItemData(1003);
		if (itemData != null)
		{
			List<ItemObject> effItems = new List<ItemObject>(10);
			Package.AddSameItems(1003, 10, ref effItems);
			SyncItemList(effItems);
			ItemSample itemSample = new ItemSample(1003, 10);
			SyncNewItem(new ItemSample[1] { itemSample });
			SyncPackageIndex();
		}
	}

	public List<int> ExportLearntIDs()
	{
		return _learntSkills.ExportLearntIDs();
	}

	public void ImportLearntSkillIDs()
	{
		if (_hasRecord)
		{
			_learntSkills.ImportLearntSkillIDs(_learntSkillsID);
		}
	}

	public SkillTreeUnit FindSkillUnit(int SkillType)
	{
		return _learntSkills.FindSkillUnit(SkillType);
	}

	public bool CheckMinerGetRare()
	{
		return _learntSkills.CheckMinerGetRare();
	}

	public bool CheckCutterGetRare()
	{
		return _learntSkills.CheckCutterGetRare();
	}

	public bool CheckHunterGetRare()
	{
		return _learntSkills.CheckHunterGetRare();
	}

	public SkillTreeUnit FindSkillUnitByID(int skillid)
	{
		return _learntSkills.FindSkillUnit(skillid);
	}

	public bool AddFormula(ReplicatorFormula formula)
	{
		bool result = false;
		if (formula == null)
		{
			result = false;
		}
		int[] formulaId = formula.formulaId;
		int id;
		for (int i = 0; i < formulaId.Length; i++)
		{
			id = formulaId[i];
			if (!ForumlaList.Exists((Replicator.KnownFormula iter) => iter.id == id))
			{
				PublicData.Self.bChanged = true;
				ForumlaList.Add(new Replicator.KnownFormula
				{
					id = id,
					flag = true
				});
				result = true;
			}
		}
		return result;
	}

	private void WriteSkill(BinaryWriter w)
	{
		_learntSkillsID = ExportLearntIDs();
		BufferHelper.Serialize(w, _learntSkillsID.Count);
		foreach (int item in _learntSkillsID)
		{
			BufferHelper.Serialize(w, item);
		}
		if (!ServerConfig.IsStory)
		{
			BufferHelper.Serialize(w, ForumlaList.Count);
			foreach (Replicator.KnownFormula forumla in ForumlaList)
			{
				BufferHelper.Serialize(w, forumla.id);
				BufferHelper.Serialize(w, forumla.flag);
			}
		}
		BufferHelper.Serialize(w, MetalScanList.Count);
		foreach (int metalScan in MetalScanList)
		{
			BufferHelper.Serialize(w, metalScan);
		}
	}

	private void ReadSkill(BinaryReader reader)
	{
		int num = BufferHelper.ReadInt32(reader);
		for (int i = 0; i < num; i++)
		{
			_learntSkillsID.Add(BufferHelper.ReadInt32(reader));
		}
		if (!ServerConfig.IsStory)
		{
			num = BufferHelper.ReadInt32(reader);
			for (int j = 0; j < num; j++)
			{
				int id = BufferHelper.ReadInt32(reader);
				bool flag = BufferHelper.ReadBoolean(reader);
				ForumlaList.Add(new Replicator.KnownFormula
				{
					id = id,
					flag = flag
				});
			}
		}
		num = BufferHelper.ReadInt32(reader);
		for (int k = 0; k < num; k++)
		{
			int item = BufferHelper.ReadInt32(reader);
			if (!MetalScanList.Contains(item))
			{
				MetalScanList.Add(item);
			}
		}
	}

	private void RPC_C2S_SKTLevelUp(uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
		int num = stream.Read<int>(new object[0]);
		int num2 = stream.Read<int>(new object[0]);
		SkillTreeUnit skillTreeUnit = FindSkillUnit(num);
		SkillTreeUnit skillTreeUnit2;
		if (skillTreeUnit != null)
		{
			int level = skillTreeUnit._level + 1;
			skillTreeUnit2 = SkillTreeInfo.GetSkillUnit(num, level);
			if (skillTreeUnit2 == null)
			{
				return;
			}
			_learntSkills.RemoveSkillUnit(skillTreeUnit);
			_learntSkills.AddSkillUnit(skillTreeUnit2);
			SyncSave();
		}
		else
		{
			skillTreeUnit2 = SkillTreeInfo.GetMinLevelSkillByType(num);
			if (skillTreeUnit2 == null)
			{
				return;
			}
			_learntSkills.AddSkillUnit(skillTreeUnit2);
			SyncSave();
		}
		RPCOthers(EPacketType.PT_InGame_SKTLevelUp, num, skillTreeUnit2._level, num2);
	}

	protected override void OnPEInstantiate(uLink.NetworkMessageInfo info)
	{
		SetParent("PlayerNetMgr");
		_id = info.networkView.initialData.Read<int>(new object[0]);
		_worldId = info.networkView.initialData.Read<int>(new object[0]);
		_teamId = info.networkView.initialData.Read<int>(new object[0]);
		AddSkEntity();
		AddArmorInfo();
		InitPlayerData();
		LoadRecordData();
		ImportLearntSkillIDs();
		if (!ServerConfig.IsStory)
		{
			StartCoroutine(ResAutoIncrease());
		}
		StartCoroutine(AutoSave());
		base.PEInstantiateEvent += ServerAdministrator.OnPlayerInitializedEvent;
		GameWorld.RegisterVoxelDataChangedEvent(base.WorldId, OnVoxelDataChanged);
		GameWorld.RegisterBlockDataChangedEvent(base.WorldId, OnBlockDataChanged);
		if (ServerConfig.IsCustom)
		{
			this.OnUseItemEventHandler = (Action<int>)Delegate.Combine(this.OnUseItemEventHandler, new Action<int>(OnUseItem));
			this.OnPutOutItemEventHandler = (Action<int>)Delegate.Combine(this.OnPutOutItemEventHandler, new Action<int>(OnPutOutItem));
		}
		InitArmor();
	}

	protected override void OnPEStart()
	{
		if (!AddPlayer())
		{
			base.enabled = false;
			return;
		}
		if (GMCommand.Self.IsForbid(steamId))
		{
			NetInterface.CloseConnection(base.OwnerView.owner);
		}
		base.OnPEStart();
		BindAction(EPacketType.PT_InGame_TerrainData, RPC_C2S_RequestTerrainData);
		BindAction(EPacketType.PT_InGame_RequestInit, RPC_C2S_RequestInit);
		BindAction(EPacketType.PT_InGame_RequestData, RPC_C2S_RequestData);
		BindAction(EPacketType.PT_InGame_MakeMask, RPC_C2S_MakeMask);
		BindAction(EPacketType.PT_InGame_RemoveMask, RPC_C2S_RemoveMask);
		BindAction(EPacketType.PT_InGame_SetShortcut, RPC_C2S_SetShortCut);
		BindAction(EPacketType.PT_InGame_FastTransfer, RPC_C2S_FastTransfer);
		BindAction(EPacketType.PT_InGame_PlayerPosition, RPC_C2S_PlayerMovePosition);
		BindAction(EPacketType.PT_InGame_PlayerRot, RPC_C2S_PlayerMoveRotationY);
		BindAction(EPacketType.PT_InGame_PlayerState, RPC_C2S_PlayerMovePlayerState);
		BindAction(EPacketType.PT_InGame_PlayerOnGround, RPC_C2S_PlayerMoveGrounded);
		BindAction(EPacketType.PT_InGame_PlayerShootTarget, RPC_C2S_PlayerMoveShootTarget);
		BindAction(EPacketType.PT_InGame_GliderStatus, RPC_C2S_SyncGliderStatus);
		BindAction(EPacketType.PT_InGame_ParachuteStatus, RPC_C2S_SyncParachuteStatus);
		BindAction(EPacketType.PT_InGame_JetPackStatus, RPC_C2S_SyncJetPackStatus);
		BindAction(EPacketType.PT_InGame_GetAllDeadObjItem, RPC_C2S_GetAllItem);
		BindAction(EPacketType.PT_InGame_GetDeadObjItem, RPC_C2S_GetItem);
		BindAction(EPacketType.PT_InGame_PutItem, RPC_C2S_PutOutItem);
		BindAction(EPacketType.PT_InGame_PutOutTower, RPC_C2S_PutOutTower);
		BindAction(EPacketType.PT_InGame_PutOutFlag, RPC_C2S_PutOutFlag);
		BindAction(EPacketType.PT_InGame_TakeOffEquipment, RPC_C2S_TakeOffEquipment);
		BindAction(EPacketType.PT_InGame_PutOnEquipment, RPC_C2S_PutOnEquipment);
		BindAction(EPacketType.PT_InGame_MissionItem, RPC_C2S_ProcessItemMission);
		BindAction(EPacketType.PT_InGame_UseItem, RPC_C2S_UseItem);
		BindAction(EPacketType.PT_InGame_GetItemListBack, RPC_C2S_GetItemListBack);
		BindAction(EPacketType.PT_InGame_GetItemBack, RPC_C2S_GetItemBack);
		BindAction(EPacketType.PT_InGame_PreGetItemBack, RPC_C2S_PreGetItemBack);
		BindAction(EPacketType.PT_InGame_GetLootItemBack, RPC_C2S_GetLootItemBack);
		BindAction(EPacketType.PT_InGame_Turn, RPC_C2S_Turn);
		BindAction(EPacketType.PT_InGame_GetColonyBack, RPC_C2S_GetColonyBack);
		BindAction(EPacketType.PT_InGame_GetOnVehicle, RPC_C2S_GetOnVehicle);
		BindAction(EPacketType.PT_InGame_GetOffVehicle, RPC_C2S_GetOffVehicle);
		BindAction(EPacketType.PT_InGame_RepairVehicle, RPC_C2S_RepairVehicle);
		BindAction(EPacketType.PT_InGame_ChargeVehicle, RPC_C2S_ChargeVehicle);
		BindAction(EPacketType.PT_InGame_BlockUndo, RPC_C2S_BuildBlockRedo);
		BindAction(EPacketType.PT_InGame_BlockRedo, RPC_C2S_BuildBlockRedo);
		BindAction(EPacketType.PT_InGame_TowerRefill, RPC_C2S_TowerRefill);
		BindAction(EPacketType.PT_InGame_PackageSplit, RPC_C2S_SplitItem);
		BindAction(EPacketType.PT_InGame_PackageDelete, RPC_C2S_DeleteItem);
		BindAction(EPacketType.PT_InGame_PackageSort, RPC_C2S_SortPackage);
		BindAction(EPacketType.PT_InGame_ExchangeItem, RPC_C2S_ExchangeItem);
		BindAction(EPacketType.PT_InGame_RemoveNewFlag, RPC_C2S_RemoveNewFlag);
		BindAction(EPacketType.PT_InGame_MissionMoveAircraft, RPC_C2S_MissionMoveAircraft);
		BindAction(EPacketType.PT_InGame_PublicStorageStore, RPC_C2S_PublicStorageStore);
		BindAction(EPacketType.PT_InGame_PublicStorageExchange, RPC_C2S_PublicStorageExchange);
		BindAction(EPacketType.PT_InGame_PublicStorageFetch, RPC_C2S_PublicStorageFetch);
		BindAction(EPacketType.PT_InGame_PublicStorageDelete, RPC_C2S_PublicStroageDelete);
		BindAction(EPacketType.PT_InGame_PublicStorageSplit, RPC_C2S_PublicStorageSplit);
		BindAction(EPacketType.PT_InGame_PublicStorageSort, RPC_C2S_PublicStorageSort);
		BindAction(EPacketType.PT_InGame_GameStarted, RPC_C2S_GameStarted);
		BindAction(EPacketType.PT_InGame_PersonalStorageStore, RPC_C2S_PersonalStorageStore);
		BindAction(EPacketType.PT_InGame_PersonalStroageDelete, RPC_C2S_PersonalStorageDelete);
		BindAction(EPacketType.PT_InGame_PersonalStorageFetch, RPC_C2S_PersonalStorageFetch);
		BindAction(EPacketType.PT_InGame_PersonalStorageSplit, RPC_C2S_PersonalStorageSplit);
		BindAction(EPacketType.PT_InGame_PersonalStorageExchange, RPC_C2S_PersonalStorageExchange);
		BindAction(EPacketType.PT_InGame_PersonalStorageSort, RPC_C2S_PersonalStorageSort);
		BindAction(EPacketType.PT_InGame_CreateBuilding, RPC_C2S_CreateBuildingWithItem);
		BindAction(EPacketType.PT_InGame_PlayerRevive, RPC_C2S_PlayerRevive);
		BindAction(EPacketType.PT_InGame_PlayerReset, RPC_C2S_PlayerReset);
		BindAction(EPacketType.PT_InGame_SendMsg, RPC_C2S_SendMsg);
		BindAction(EPacketType.PT_InGame_ApplyDamage, RPC_C2S_ApplyDamage);
		BindAction(EPacketType.PT_InGame_ApplyComfort, RPC_C2S_ApplyComfort);
		BindAction(EPacketType.PT_InGame_ApplySatiation, RPC_C2S_ApplySatiation);
		BindAction(EPacketType.PT_InGame_CreateTeam, RPC_C2S_CreateNewTeam);
		BindAction(EPacketType.PT_InGame_JoinTeam, RPC_C2S_JoinTeam);
		BindAction(EPacketType.PT_InGame_ApproveJoin, RPC_C2S_ApproveJoin);
		BindAction(EPacketType.PT_InGame_DenyJoin, RPC_C2S_DenyJoin);
		BindAction(EPacketType.PT_InGame_Invitation, RPC_C2S_Invitation);
		BindAction(EPacketType.PT_InGame_AcceptJoinTeam, RPC_C2S_AcceptJoinTeam);
		BindAction(EPacketType.PT_InGame_KickSB, RPC_C2S_KickSB);
		BindAction(EPacketType.PT_InGame_LeaderDeliver, RPC_C2S_LeaderDeliver);
		BindAction(EPacketType.PT_InGame_QuitTeam, RPC_C2S_QuitTeam);
		BindAction(EPacketType.PT_InGame_DissolveTeam, RPC_C2S_DissolveTeam);
		BindAction(EPacketType.PT_InGame_CreateMapObj, RPC_C2S_CreateMapObj);
		BindAction(EPacketType.PT_InGame_ItemAttrChanged, RPC_C2S_ItemAttrChanged);
		BindAction(EPacketType.PT_InGame_EquipItemCost, RPC_C2S_EquipItemCost);
		BindAction(EPacketType.PT_InGame_WeaponReload, RPC_C2S_WeaponReload);
		BindAction(EPacketType.PT_InGame_PackageItemCost, RPC_C2S_PackageItemCost);
		BindAction(EPacketType.PT_InGame_GunEnergyReload, RPC_C2S_GunEnergyReload);
		BindAction(EPacketType.PT_InGame_BatteryEnergyReload, RPC_C2S_BatteryEnergyReload);
		BindAction(EPacketType.PT_InGame_JetPackEnergyReload, RPC_C2S_JetPackEnergyReload);
		BindAction(EPacketType.PT_InGame_WeaponDurability, RPC_C2S_WeaponDurability);
		BindAction(EPacketType.PT_InGame_ArmorDurability, RPC_C2S_ArmorDurability);
		BindAction(EPacketType.PT_InGame_CreateSceneBox, RPC_C2S_CreateSceneBox);
		BindAction(EPacketType.PT_InGame_CreateSceneItem, RPC_C2S_CreateSceneItem);
		BindAction(EPacketType.PT_MO_Destroy, RPC_C2S_DestroySceneItem);
		BindAction(EPacketType.PT_AI_SpawnPos, SPTerrainEvent.RPC_C2S_SpawnPos);
		BindAction(EPacketType.PT_NPC_CreateAd, SPTerrainEvent.RPC_C2S_CreateAdNpc);
		BindAction(EPacketType.PT_NPC_CreateStRd, SPTerrainEvent.RPC_C2S_CreateStRdNpc);
		BindAction(EPacketType.PT_NPC_CreateAdMainNpc, SPTerrainEvent.PT_NPC_CreateAdMainNpc);
		BindAction(EPacketType.PT_NPC_CreateSt, SPTerrainEvent.RPC_C2S_CreateStNpc);
		BindAction(EPacketType.PT_AI_SpawnAI, SPTerrainEvent.RPC_C2S_SpawnAIAtPoint);
		BindAction(EPacketType.PT_AI_SetFixActive, SPTerrainEvent.RPC_C2S_SetFixActive);
		BindAction(EPacketType.PT_Common_NativeTowerDestroyed, RandomTownManager.RPC_C2S_NativeTowerDestroyed);
		BindAction(EPacketType.PT_Common_TownCreate, RandomTownManager.RPC_C2S_TownCreate);
		BindAction(EPacketType.PT_NPC_CreateTown, SPTerrainEvent.RPC_C2S_CreateAdNpcByIndex);
		BindAction(EPacketType.PT_AI_SpawnGroupAI, SPTerrainEvent.RPC_C2S_SpawnAIGroupAtPoint);
		BindAction(EPacketType.PT_AI_Gift, SPTerrainEvent.RPC_C2S_SpawnGift);
		BindAction(EPacketType.PT_AI_NativeStatic, SPTerrainEvent.RPC_C2S_CreateNativeStatic);
		BindAction(EPacketType.PT_NPC_GetItem, AiAdNpcNetwork.RPC_C2S_NpcGetItem);
		BindAction(EPacketType.PT_NPC_DeleteItem, AiAdNpcNetwork.RPC_C2S_NpcLoseItem);
		BindAction(EPacketType.PT_NPC_DeleteAllItem, AiAdNpcNetwork.RPC_C2S_NpcLoseAllItems);
		BindAction(EPacketType.PT_NPC_PutOnEquip, RPC_C2S_NpcPutOnEquip);
		BindAction(EPacketType.PT_NPC_TakeOffEquip, RPC_C2S_NpcTakeOffEquip);
		BindAction(EPacketType.PT_NPC_SortPackage, AiAdNpcNetwork.RPC_C2S_NpcPackageSort);
		BindAction(EPacketType.PT_NPC_Dismiss, RPC_C2S_DismissByPlayer);
		BindAction(EPacketType.PT_NPC_ServentRevive, RPC_C2S_ServantRevive);
		BindAction(EPacketType.PT_InGame_CurSceneId, RPC_C2S_CurSceneId);
		BindAction(EPacketType.PT_NPC_Recruit, RPC_C2S_NpcRecruit);
		BindAction(EPacketType.PT_NPC_Revive, RPC_C2S_ServantAutoRevive);
		BindAction(EPacketType.PT_InGame_NewMission, RPC_C2S_CreateMission);
		BindAction(EPacketType.PT_InGame_AccessMission, RPC_C2S_AccessMission);
		BindAction(EPacketType.PT_InGame_InitMission, RPC_C2S_InitMission);
		BindAction(EPacketType.PT_InGame_MissionMonsterPos, RPC_C2S_ResponseKillMonsterPos);
		BindAction(EPacketType.PT_InGame_MissionFollowPos, RPC_C2S_ResponseFollowPos);
		BindAction(EPacketType.PT_InGame_MissionDiscoveryPos, RPC_C2S_ResponseDiscoveryPos);
		BindAction(EPacketType.PT_InGame_DeleteMission, RPC_C2S_ReplyDeleteMission);
		BindAction(EPacketType.PT_InGame_CompleteTarget, RPC_C2S_RequestCompleteTarget);
		BindAction(EPacketType.PT_InGame_CompleteMission, RPC_C2S_RequestCompleteMission);
		BindAction(EPacketType.PT_InGame_ComMisTest, RPC_C2S_ComMisTest);
		BindAction(EPacketType.PT_InGame_MissionFailed, RPC_C2S_MissionFailed);
		BindAction(EPacketType.PT_InGame_MissionUseItem, RPC_C2S_MissionUseItem);
		BindAction(EPacketType.PT_InGame_ModifyQuestVariable, RPC_C2S_ModifyQuestVariable);
		BindAction(EPacketType.PT_InGame_MissionKillMonster, RPC_C2S_MissionKillMonster);
		BindAction(EPacketType.PT_InGame_MissionTowerDefense, RPC_C2S_MissionTowerDefense);
		BindAction(EPacketType.PT_InGame_SetMission, RPC_C2S_SetMission);
		BindAction(EPacketType.PT_InGame_EntityReach, RPC_C2S_EntityReach);
		BindAction(EPacketType.PT_InGame_RequestAdMissionData, RPC_C2S_RequestAdMissionData);
		BindAction(EPacketType.PT_InGame_ModifyMissionFlag, RPC_C2S_ModifyMissionFlag);
		BindAction(EPacketType.PT_InGame_AddBlackList, RPC_C2S_AddBlackList);
		BindAction(EPacketType.PT_InGame_DelBlackList, RPC_C2S_DeleteBlackList);
		BindAction(EPacketType.PT_InGame_ClearBlackList, RPC_C2S_ClearBlackList);
		BindAction(EPacketType.PT_InGame_AddAssistant, RPC_C2S_AddAssistants);
		BindAction(EPacketType.PT_InGame_DelAssistant, RPC_C2S_DeleteAssistants);
		BindAction(EPacketType.PT_InGame_ClearAssistant, RPC_C2S_ClearAssistants);
		BindAction(EPacketType.PT_InGame_BuildLock, RPC_C2S_BuildLock);
		BindAction(EPacketType.PT_InGame_BuildUnLock, RPC_C2S_BuildUnLock);
		BindAction(EPacketType.PT_InGame_ClearBuildLock, RPC_C2S_ClearBuildLock);
		BindAction(EPacketType.PT_InGame_ClearVoxel, RPC_C2S_ClearVoxelData);
		BindAction(EPacketType.PT_InGame_ClearAllVoxel, RPC_C2S_ClearAllVoxelData);
		BindAction(EPacketType.PT_InGame_AreaLock, RPC_C2S_LockArea);
		BindAction(EPacketType.PT_InGame_AreaUnLock, RPC_C2S_UnLockArea);
		BindAction(EPacketType.PT_InGame_BlockLock, RPC_C2S_BuildChunk);
		BindAction(EPacketType.PT_InGame_LoginBan, RPC_C2S_JoinGame);
		BindAction(EPacketType.PT_InGame_Kick, RPC_C2S_KickPlayer);
		BindAction(EPacketType.PT_InGame_InitShop, RPC_C2S_GetShop);
		BindAction(EPacketType.PT_InGame_Buy, RPC_C2S_BuyItemInMulti);
		BindAction(EPacketType.PT_InGame_Repurchase, RPC_C2S_Repurchase);
		BindAction(EPacketType.PT_InGame_Sell, RPC_C2S_SellItemInMulti);
		BindAction(EPacketType.PT_InGame_ChangeCurrency, RPC_C2S_ChangeCurrency);
		BindAction(EPacketType.PT_InGame_SkillCast, RPC_C2S_SkillCast);
		BindAction(EPacketType.PT_InGame_SkillShoot, RPC_C2S_SkillCastShoot);
		BindAction(EPacketType.PT_InGame_MergeSkill, RPC_C2S_MultiMergeSkill);
		BindAction(EPacketType.PT_InGame_CancelMerge, RPC_C2S_CancelMerge);
		BindAction(EPacketType.PT_InGame_WaterPitcher, RPC_C2S_WaterPitcher);
		BindAction(EPacketType.PT_InGame_WaterPump, RPC_C2S_WaterPump);
		BindAction(EPacketType.PT_InGame_TownArea, RPC_C2S_AddTownArea);
		BindAction(EPacketType.PT_InGame_CampArea, RPC_C2S_AddCampArea);
		BindAction(EPacketType.PT_Test_AddItem, RPC_C2S_AddItem);
		BindAction(EPacketType.PT_Test_AddEquipment, RPC_C2S_AddTestEquipment);
		BindAction(EPacketType.PT_Test_MoveNpc, RPC_C2S_MoveNpc);
		BindAction(EPacketType.PT_InGame_Plant_GetBack, RPC_C2S_Plant_GetBack);
		BindAction(EPacketType.PT_InGame_Plant_VFTerrainTarget, RPC_C2S_Plant_VFTerrainTarget);
		BindAction(EPacketType.PT_InGame_Plant_Water, RPC_C2S_Plant_Water);
		BindAction(EPacketType.PT_InGame_Plant_Clean, RPC_C2S_Plant_Clean);
		BindAction(EPacketType.PT_InGame_Plant_Clear, RPC_C2S_Plant_Clear);
		BindAction(EPacketType.PT_InGame_Railway_AddPoint, RPC_C2S_Railway_AddPoint);
		BindAction(EPacketType.PT_InGame_Railway_PrePointChange, RPC_C2S_Railway_PrePointChange);
		BindAction(EPacketType.PT_InGame_Railway_NextPointChange, RPC_C2S_Railway_NextPointChange);
		BindAction(EPacketType.PT_InGame_Railway_Route, RPC_C2S_Railway_Route);
		BindAction(EPacketType.PT_InGame_Railway_GetOnTrain, RPC_C2S_Railway_GetOnTrain);
		BindAction(EPacketType.PT_InGame_Railway_GetOffTrain, RPC_C2S_Railway_GetOffTrain);
		BindAction(EPacketType.PT_InGame_Railway_DeleteRoute, RPC_C2S_Railway_DeleteRoute);
		BindAction(EPacketType.PT_InGame_Railway_Recycle, RPC_C2S_Railway_Recycle);
		BindAction(EPacketType.PT_InGame_Railway_SetRouteTrain, RPC_C2S_Railway_SetRouteTrain);
		BindAction(EPacketType.PT_InGame_Railway_ChangeStationRot, RPC_C2S_Railway_ChangeStationRot);
		BindAction(EPacketType.PT_InGame_Railway_ResetPointName, RPC_C2S_Railway_ResetPointName);
		BindAction(EPacketType.PT_InGame_Railway_ResetRouteName, RPC_C2S_Railway_ResetRouteName);
		BindAction(EPacketType.PT_InGame_Railway_ResetPointTime, RPC_C2S_Railway_ResetPointTime);
		BindAction(EPacketType.PT_InGame_Railway_AutoCreateRoute, RPC_C2S_Railway_AutoCreateRoute);
		BindAction(EPacketType.PT_InGame_AccItems_CreateItem, RPC_AccItems_CreateItem);
		BindAction(EPacketType.PT_InGame_SKTLevelUp, RPC_C2S_SKTLevelUp);
		BindAction(EPacketType.PT_InGame_RandomItem, RPC_C2S_GenRandomItem);
		BindAction(EPacketType.PT_InGame_RandomItemRare, RPC_C2S_GenRandomItemRare);
		BindAction(EPacketType.PT_InGame_RandomItemRareAry, RPC_C2S_GenRandomRareAry);
		BindAction(EPacketType.PT_InGame_RandomItemFetch, RPC_C2S_RandomItemFetch);
		BindAction(EPacketType.PT_InGame_RandomItemFetchAll, RPC_C2S_RandomItemFetchAll);
		BindAction(EPacketType.PT_InGame_RandomFeces, RPC_C2S_RandomFeces);
		BindAction(EPacketType.PT_InGame_RandomItemClicked, RPC_C2S_RandomItemClicked);
		BindAction(EPacketType.PT_InGame_EnterDungeon, RPC_C2S_EnterDungeon);
		BindAction(EPacketType.PT_InGame_ExitDungeon, RPC_C2S_ExitDungeon);
		BindAction(EPacketType.PT_InGame_UploadDungeonSeed, RPC_C2S_UploadDungeonSeed);
		BindAction(EPacketType.PT_InGame_GenDunEntrance, RPC_C2S_GenDunEntrance);
		BindAction(EPacketType.PT_InGame_ReviveSB, RPC_C2S_ReviveSB);
		BindAction(EPacketType.PT_InGame_ApprovalRevive, RPC_C2S_ApprovalRevive);
		BindAction(EPacketType.PT_Common_FoundMapLable, RPC_C2S_FoundMapLable);
		BindAction(EPacketType.PT_InGame_SwitchArmorSuit, RPC_C2S_SwitchArmorSuit);
		BindAction(EPacketType.PT_InGame_EquipArmorPart, RPC_C2S_EquipArmorPart);
		BindAction(EPacketType.PT_InGame_RemoveArmorPart, RPC_C2S_RemoveArmorPart);
		BindAction(EPacketType.PT_InGame_SwitchArmorPartMirror, RPC_C2S_SwitchArmorPartMirror);
		BindAction(EPacketType.PT_InGame_SyncArmorPartPos, RPC_C2S_SyncArmorPartPos);
		BindAction(EPacketType.PT_InGame_SyncArmorPartRot, RPC_C2S_SyncArmorPartRot);
		BindAction(EPacketType.PT_InGame_SyncArmorPartScale, RPC_C2S_SyncArmorPartScale);
		BindAction(EPacketType.PT_Common_TownDoodad, SPTerrainEvent.RPC_C2S_TownDoodad);
		BindAction(EPacketType.PT_Common_NativeTowerCreate, SPTerrainEvent.RPC_C2S_NativeTowerCreate);
		BindAction(EPacketType.PT_Custom_CheckPos, RPC_C2S_CheckPosition);
		BindAction(EPacketType.PT_Custom_CheckRot, RPC_C2S_CheckRotation);
		BindAction(EPacketType.PT_Custom_CheckDist3D, RPC_C2S_CheckDistance3D);
		BindAction(EPacketType.PT_Custom_CheckDist2D, RPC_C2S_CheckDistance2D);
		BindAction(EPacketType.PT_Custom_CheckLookAt3D, RPC_C2S_CheckLookAt3D);
		BindAction(EPacketType.PT_Custom_CheckLookAt2D, RPC_C2S_CheckLookAt2D);
		BindAction(EPacketType.PT_Custom_CheckOwerItem, RPC_C2S_CheckOwerItem);
		BindAction(EPacketType.PT_Custom_CheckAttribute, RPC_C2S_CheckAttribute);
		BindAction(EPacketType.PT_Custom_CheckEquip, RPC_C2S_CheckEquipment);
		BindAction(EPacketType.PT_Custom_CheckWorld, RPC_C2S_CheckWorld);
		BindAction(EPacketType.PT_Custom_AddQuest, RPC_C2S_AddQuest);
		BindAction(EPacketType.PT_Custom_RemoveQuest, RPC_C2S_RemoveQuest);
		BindAction(EPacketType.PT_Custom_StartAnimation, RPC_C2S_PlayerAnimation);
		BindAction(EPacketType.PT_Custom_StopAnimation, RPC_C2S_StopAnimation);
		BindAction(EPacketType.PT_Custom_SetPos, RPC_C2S_SetPose);
		BindAction(EPacketType.PT_Custom_ModifyPackage, RPC_C2S_ModifyPackage);
		BindAction(EPacketType.PT_Custom_SetVariable, RPC_C2S_SetVariable);
		BindAction(EPacketType.PT_Custom_ModifyStat, RPC_C2S_ModifyStat);
		BindAction(EPacketType.PT_Custom_Kill, RPC_C2S_Kill);
		BindAction(EPacketType.PT_Custom_CreateObject, RPC_C2S_CreateObject);
		BindAction(EPacketType.PT_Custom_RemoveObject, RPC_C2S_RemoveObject);
		BindAction(EPacketType.PT_Custom_RemoveSpecObject, RPC_C2S_RemoveSpecificObject);
		BindAction(EPacketType.PT_Custom_EnableSpawn, RPC_C2S_EnableSpawn);
		BindAction(EPacketType.PT_Custom_DisableSpawn, RPC_C2S_DisableSpawn);
		BindAction(EPacketType.PT_Custom_OrderVector, RPC_C2S_OrderVector);
		BindAction(EPacketType.PT_Custom_OrderTarget, RPC_C2S_OrderTarget);
		BindAction(EPacketType.PT_Custom_CancelOrder, RPC_C2S_CancelOrder);
		BindAction(EPacketType.PT_Custom_FastTravel, RPC_C2S_FastTravel);
		BindAction(EPacketType.PT_Custom_AddChoice, RPC_C2S_AddChoice);
		BindAction(EPacketType.PT_NPC_RequestAiOp, RPC_C2S_RequestAiOp);
		BindAction(EPacketType.PT_InGame_HeartBeat, RPC_C2S_HeartBeat);
		BindAction(EPacketType.PT_Mount_ReqMonsterCtrl, RPC_C2S_ReqMonsterCtrl);
		BindAction(EPacketType.PT_Mount_AddMountMonster, RPC_C2S_AddRideMonster);
		BindAction(EPacketType.PT_Mount_DelMountMonster, RPC_C2S_DelRideMonster);
		BindAction(EPacketType.PT_Mount_SyncPlayerRot, RPC_C2S_SyncPlayerRot);
	}

	protected override void OnPEDestroy()
	{
		base.OnPEDestroy();
		if (Player.PlayerDisconnected != null)
		{
			Player.PlayerDisconnected(this);
		}
		GameWorld.UnregisterVoxelDataChangedEvent(base.WorldId, OnVoxelDataChanged);
		GameWorld.UnregisterBlockDataChangedEvent(base.WorldId, OnBlockDataChanged);
		base.PEInstantiateEvent -= ServerAdministrator.OnPlayerInitializedEvent;
		if (ServerConfig.IsCustom)
		{
			this.OnUseItemEventHandler = (Action<int>)Delegate.Remove(this.OnUseItemEventHandler, new Action<int>(OnUseItem));
			this.OnPutOutItemEventHandler = (Action<int>)Delegate.Remove(this.OnPutOutItemEventHandler, new Action<int>(OnPutOutItem));
		}
		int teamId = base.TeamId;
		SetTeamId(originTeamId);
		GroupNetwork.RemoveFromTeam(teamId, base.Id);
		GetOffTrainWhenDisconnect(1, base.Id);
		GetOffVehicle(base.transform.position);
		PlayerReviveForLoad();
		Save();
		DelPlayer();
		if (LogFilter.logDebug)
		{
			Debug.Log(roleName + " OnDestroy");
		}
	}

	public static int GetPlayerId(uLink.NetworkPlayer peer)
	{
		if (PlayerPeers.ContainsKey(peer))
		{
			return PlayerPeers[peer];
		}
		return -1;
	}

	public static uLink.NetworkPlayer GetPlayerPeer(int id)
	{
		if (PlayerIds.ContainsKey(id))
		{
			return PlayerIds[id];
		}
		return uLink.NetworkPlayer.unassigned;
	}

	public static Player GetPlayer(uLink.NetworkPlayer peer)
	{
		int playerId = GetPlayerId(peer);
		return GetPlayer(playerId);
	}

	public static Player GetPlayer(int id)
	{
		if (PlayerDic.ContainsKey(id))
		{
			return PlayerDic[id];
		}
		return null;
	}

	public static Player GetPlayerByName(string playername)
	{
		foreach (Player value in PlayerDic.Values)
		{
			if (value.roleName == playername)
			{
				return value;
			}
		}
		return null;
	}

	public static Player GetRandomPlayer()
	{
		using (List<Player>.Enumerator enumerator = Players.GetEnumerator())
		{
			if (enumerator.MoveNext())
			{
				return enumerator.Current;
			}
		}
		return null;
	}

	public static Player GetNearestPlayer(int team, Vector3 pos)
	{
		List<Player> teamPlayers = GetTeamPlayers(team);
		if (teamPlayers == null || teamPlayers.Count == 0 || teamPlayers[0] == null)
		{
			return null;
		}
		Player player = teamPlayers[0];
		foreach (Player item in teamPlayers)
		{
			if (item != null && Vector3.Distance(item.transform.position, pos) < Vector3.Distance(player.transform.position, pos))
			{
				player = item;
			}
		}
		return player;
	}

	public static Player GetNearestPlayer(Vector3 pos)
	{
		if (Players.Count == 0)
		{
			return null;
		}
		Player player = Players[0];
		foreach (Player player2 in Players)
		{
			if (player2 != null && Vector3.Distance(player2.transform.position, pos) < Vector3.Distance(player.transform.position, pos))
			{
				player = player2;
			}
		}
		return player;
	}

	public static Player GetNearestPlayer(int[] npcids, int playerId)
	{
		bool flag = true;
		int num = (ObjNetInterface.Get(npcids[0]) as AiAdNpcNetwork).authId;
		foreach (int id in npcids)
		{
			AiAdNpcNetwork aiAdNpcNetwork = ObjNetInterface.Get(id) as AiAdNpcNetwork;
			if (aiAdNpcNetwork != null)
			{
				if (aiAdNpcNetwork.authId <= 0)
				{
					flag = false;
				}
				else if (num != aiAdNpcNetwork.authId)
				{
					flag = false;
				}
			}
			num = aiAdNpcNetwork.authId;
		}
		if (flag)
		{
			return ObjNetInterface.Get(num) as Player;
		}
		return null;
	}

	public static int GetPlayerTeamId(int playerId)
	{
		if (PlayerDic.ContainsKey(playerId))
		{
			return PlayerDic[playerId].TeamId;
		}
		return -1;
	}

	public static Player GetPlayer(string roleName)
	{
		return Players.Find((Player iter) => iter.roleName == roleName);
	}

	public static Player GetTeamPlayer(int team, int except)
	{
		foreach (Player player in Players)
		{
			if (player != null && player.Id != except && player.TeamId == team)
			{
				return player;
			}
		}
		return null;
	}

	public static List<Player> GetTeamPlayers(int team)
	{
		List<Player> teamPlayers = new List<Player>();
		GroupNetwork.GetTeamData(team)?.MemberAction(delegate(MemberData md)
		{
			if (md != null && PlayerDic.ContainsKey(md.Id))
			{
				teamPlayers.Add(PlayerDic[md.Id]);
			}
		});
		return teamPlayers;
	}

	public static void SavePlayers()
	{
		foreach (Player player in Players)
		{
			if (null != player)
			{
				player.Save();
			}
		}
	}

	protected bool AddPlayer()
	{
		if (PlayerDic.ContainsKey(base.Id))
		{
			NetInterface.NetDestroy(PlayerDic[base.Id]);
			NetInterface.CloseConnection(base.OwnerView.owner);
			return false;
		}
		Add(this);
		PlayerDic.Add(base.Id, this);
		PlayerPeers.Add(base.OwnerView.owner, base.Id);
		PlayerIds.Add(base.Id, base.OwnerView.owner);
		Players.Add(this);
		return true;
	}

	protected void DelPlayer()
	{
		if (PlayerDic.ContainsKey(base.Id))
		{
			if (PlayerIds.ContainsKey(base.Id))
			{
				PlayerPeers.Remove(PlayerIds[base.Id]);
				PlayerIds.Remove(base.Id);
			}
			Players.Remove(PlayerDic[base.Id]);
			PlayerDic.Remove(base.Id);
		}
	}

	public static bool ValidPlayer(int id)
	{
		return PlayerDic.ContainsKey(id);
	}

	public static bool ValidDistance(ObjNetInterface obj)
	{
		if (null == obj)
		{
			return false;
		}
		foreach (Player player in Players)
		{
			if (Mathf.Abs(player.transform.position.x - obj.transform.position.x) <= 512f && Mathf.Abs(player.transform.position.y - obj.transform.position.y) <= 512f && Mathf.Abs(player.transform.position.z - obj.transform.position.z) <= 512f)
			{
				return true;
			}
		}
		return false;
	}

	private void InitPlayerData()
	{
		BaseNetwork baseNetwork = BaseNetwork.GetBaseNetwork(base.Id);
		if (!(null == baseNetwork))
		{
			account = baseNetwork.account;
			steamId = baseNetwork.steamId;
			roleName = baseNetwork.roleName;
			sex = baseNetwork.sex;
			originTeamId = baseNetwork.curTeamId;
			base.gameObject.tag = "Player";
			base.gameObject.name = roleName + "_" + base.Id;
			_battleInfo = new BattleInfo(base.TeamId);
			InitPlayerBattle();
			mPlayerSynAttribute = new PlayerSynAttribute();
			_shortcutKeys = new Dictionary<int, int>();
			_learntSkillsID = new List<int>(20);
			if (MetalScanList == null)
			{
				MetalScanList = new List<int>(20);
			}
			voxelAreaSent = new List<int>(20);
			blockAreaSent = new List<int>(20);
			_servantList = new List<AiAdNpcNetwork>(2);
			_forceServantList = new List<AiAdNpcNetwork>(2);
			_exceptItemList = new List<ItemObject>(200);
			_learntSkills = new SkillTreeUnitMgr();
			if (ForumlaList == null)
			{
				ForumlaList = new List<Replicator.KnownFormula>(20);
			}
			_harmList = new Dictionary<GroupNetInterface, DamageData>(20);
			_repurchaseItems = new Dictionary<int, List<ItemObject>>(200);
			InitCmpt();
			ForceSetting.AddPlayer(base.Id, base.TeamId, EPlayerType.Human, roleName);
			ForceSetting.AddPlayer(base.TeamId, base.TeamId, EPlayerType.Human, "Team" + base.TeamId);
			int playerDescId = baseNetwork.GetPlayerDescId();
			if (playerDescId == -1)
			{
				SPTerrainEvent.AddCustomId(base.Id, -base.Id, -1, base.Id);
			}
			else
			{
				SPTerrainEvent.AddCustomId(base.Id, playerDescId, -1, base.Id);
			}
		}
	}

	protected override void InitCmpt()
	{
		base.InitCmpt();
		m_EquipModule = (EquipmentCmpt)AddCmpt(ECmptType.Equipment);
		m_PackageCmpt = (PlayerPackageCmpt)AddCmpt(ECmptType.PlayerPackage);
		Package.InitPackage(420, 420, 420, 420);
	}

	public override void SkCreater()
	{
		SKAttribute.InitPlayerBaseAttrs(_skEntity._attribs, out _skEntity._baseAttribs);
		_skEntity.SetAllAttribute(AttribType.DefaultPlayerID, base.Id);
	}

	private void PlayerReviveForLoad()
	{
		if (_skEntity.GetAttribute(AttribType.Hp) <= Mathf.Epsilon || _bDeath)
		{
			_skEntity.SetAllAttribute(AttribType.Hp, 100f);
		}
	}

	private void AddArmorInfo()
	{
		_charcterArmor = base.gameObject.AddComponent<CharacterArmor>();
	}

	private void GetBattleReward()
	{
		GameWorld gameWorld = GameWorld.GetGameWorld(base.WorldId);
		if (gameWorld != null)
		{
			int occupiedAreaNum = gameWorld.GetOccupiedAreaNum(base.TeamId);
			if (occupiedAreaNum > 0)
			{
				int num = BattleConstData.Instance._meat_time * occupiedAreaNum;
				AddMoney(num);
			}
		}
	}

	private void OnHeartBeatTimeout(Player player)
	{
		if (Player.OnHeartBeatTimeoutEvent != null)
		{
			Player.OnHeartBeatTimeoutEvent(player);
		}
	}

	private IEnumerator AutoSave()
	{
		while (true)
		{
			yield return new WaitForSeconds(300f);
			Save();
		}
	}

	private IEnumerator CheckHeartBeat()
	{
		yield return new WaitForSeconds(30f);
		lastHeartBeatTime = Time.time;
		while (true)
		{
			yield return new WaitForSeconds(2f);
			if (lastHeartBeatTime + 8f < Time.time)
			{
				OnHeartBeatTimeout(this);
			}
		}
	}

	private IEnumerator ResAutoIncrease()
	{
		while (true)
		{
			yield return new WaitForSeconds(BattleConstData.Instance._site_interval);
			GetBattleReward();
		}
	}

	private IEnumerator SyncOwnerData(uLink.NetworkPlayer peer)
	{
		SyncRoomInfo();
		SyncInitLearntSkills();
		yield return null;
		SyncInitPackage();
		yield return null;
		SyncPackageItems(0);
		SyncPackageIndex(0);
		yield return null;
		SyncPackageItems(1);
		SyncPackageIndex(1);
		yield return null;
		SyncPackageItems(2);
		SyncPackageIndex(2);
		yield return null;
		SyncPackageItems(3);
		SyncPackageIndex(3);
		yield return null;
		Package.SyncPackageItems(0);
		Package.SyncPackageIndex(0);
		yield return null;
		Package.SyncPackageItems(1);
		Package.SyncPackageIndex(1);
		yield return null;
		Package.SyncPackageItems(2);
		Package.SyncPackageIndex(2);
		yield return null;
		Package.SyncPackageItems(3);
		Package.SyncPackageIndex(3);
		yield return null;
		SyncSceneObjects();
		yield return null;
		SyncMoneyType();
		SyncUserMoney();
		yield return null;
		SyncCurSceneId();
		SendFarmInfo();
		yield return null;
		WeatherManager.SyncWeather(peer);
		GameTime.SyncTimer(peer);
		yield return null;
		SyncPlayerBattleInfo(peer);
		yield return null;
		SyncEquipedItems(peer);
		yield return null;
		SyncInitShortCut();
		SyncRailwayData();
		yield return null;
		if (!ServerConfig.IsStory && !ServerConfig.IsCustom)
		{
			SyncExploredAreas();
			yield return null;
		}
		SyncFormulaId();
		SyncMetalScan();
		yield return null;
		SyncInitOK(peer);
		SyncTownAreas(peer);
		SyncCampAreas(peer);
		SyncMaskAreas();
		SyncMapLableIds(peer);
		yield return null;
		SyncTeamInfo(peer);
		SyncVehicleStatus(peer);
		SyncArmorData(peer);
		SyncArmorInfo(peer);
		yield return null;
		ColonyMgr.SyncData(this);
		yield return null;
		SyncPlayerMission();
		SyncTeamMission();
		yield return null;
		SyncRandomItem(base.WorldId);
		SyncDungeonEntrance(base.WorldId);
		SyncInitWhenSpawn();
		ReputationSystem.SyncData(this);
		SyncMonsterBook(this, ower: true);
		StartCoroutine(CheckHeartBeat());
	}

	public void SyncMonsterBook(Player player, bool ower, int protoId = 0)
	{
		if (ower)
		{
			player.RPCOwner(EPacketType.PT_InGame_MonsterBook, true, MonsterHandbookData.Serialize());
		}
		else
		{
			player.RPCOthers(EPacketType.PT_InGame_MonsterBook, false, protoId);
		}
	}

	private void SyncArmorInfo(uLink.NetworkPlayer peer)
	{
		byte[] array = Serialize.Export(delegate(BinaryWriter w)
		{
			WriteArmorInfo(w);
		});
		if (array.Length > 0)
		{
			RPCPeer(peer, EPacketType.PT_InGame_SyncArmorInfo, array.ToArray());
		}
	}

	private void SyncMapLableIds(uLink.NetworkPlayer peer)
	{
		if (ServerConfig.FoundMapLable.Count > 0)
		{
			RPCPeer(peer, EPacketType.PT_Common_FoundMapLable, ServerConfig.FoundMapLable.ToArray());
		}
	}

	private IEnumerator SyncProxyData(uLink.NetworkPlayer peer)
	{
		SyncPlayerBattleInfo(peer);
		yield return null;
		SyncEquipedItems(peer);
		yield return null;
		SyncInitStatus(peer);
		yield return null;
		SyncVehicleStatus(peer);
		yield return null;
		yield return null;
		SyncInitOK(peer);
		SyncTeamInfo(peer);
		SyncArmorData(peer);
		SyncArmorInfo(peer);
	}

	public void SyncVehicleStatus(uLink.NetworkPlayer peer)
	{
		if (base._OnCar && null != base._Creation && base._SeatIndex != -2)
		{
			RPCPeer(peer, EPacketType.PT_InGame_GetOnVehicle, base._Creation.Id, base._SeatIndex);
		}
	}

	public void SyncInitStatus(uLink.NetworkPlayer peer)
	{
		RPCPeer(peer, EPacketType.PT_InGame_InitStatus, mPlayerSynAttribute.mnPlayerState, mPlayerSynAttribute.mbGrounded, mPlayerSynAttribute.mbJumpFlag, mPlayerSynAttribute.mv3shootTarget);
	}

	public void SyncInitOK(uLink.NetworkPlayer peer)
	{
		RPCPeer(peer, EPacketType.PT_InGame_InitDataOK, ServerConfig.GameStarted);
	}

	public void SyncSceneObjects()
	{
		ItemObject[] items = SceneObjMgr.GetItems().ToArray();
		SyncItemList(items);
		SceneObject[] sceneObjs = GameWorld.GetSceneObjs(base.WorldId);
		if (sceneObjs != null && sceneObjs.Length > 0)
		{
			RPCOwner(EPacketType.PT_InGame_SceneObject, sceneObjs, false);
		}
	}

	public void SyncSceneObject(SceneObject obj)
	{
		RPCOthers(EPacketType.PT_InGame_SceneObject, new SceneObject[1] { obj }, false);
	}

	public void SyncSceneObjects(List<SceneObject> objs)
	{
		RPCOthers(EPacketType.PT_InGame_SceneObject, objs.ToArray(), false);
	}

	public void SyncDelSceneObjects(List<int> objIds)
	{
		RPCOthers(EPacketType.PT_InGame_DelSceneObjects, objIds.ToArray());
	}

	public void SyncDelSceneObjects(int objId)
	{
		RPCOthers(EPacketType.PT_InGame_DelSceneObjects, new int[1] { objId });
	}

	public void SyncGroupData(params object[] args)
	{
		if (!GroupNetwork.IsTeamExisted(base.TeamId))
		{
			return;
		}
		List<uLink.NetworkPlayer> members = GroupNetwork.GetMembers(base.TeamId);
		foreach (uLink.NetworkPlayer item in members)
		{
			Player player = GetPlayer(item);
			if (null != player)
			{
				player.RPCOwner(args);
			}
		}
	}

	public void SyncGroupData(NetInterface excludePeer, params object[] args)
	{
		if (GroupNetwork.IsTeamExisted(base.TeamId))
		{
			RPCProxy(excludePeer, args);
		}
		else
		{
			RPCOwner(args);
		}
	}

	public void SyncRailwayData()
	{
		byte[] array = RailwayManager.Instance.Export();
		if (array.Length > 0)
		{
			RPCOwner(EPacketType.PT_InGame_RailwayData, array);
		}
	}

	public void SyncReviveSB(int id)
	{
		RPCOwner(EPacketType.PT_InGame_ReviveSB, id);
	}

	public void SyncTeamInfo(uLink.NetworkPlayer peer)
	{
		if (base.TeamId != -1)
		{
			int leaderID = GroupNetwork.GetLeaderID(base.TeamId);
			RPCPeer(peer, EPacketType.PT_InGame_TeamInfo, base.TeamId, leaderID);
		}
	}

	public void SyncTeamInfo()
	{
		int leaderID = GroupNetwork.GetLeaderID(base.TeamId);
		RPCOthers(EPacketType.PT_InGame_TeamInfo, base.TeamId, leaderID);
	}

	public void SyncArmorData(uLink.NetworkPlayer peer)
	{
		List<ItemObject> list = new List<ItemObject>();
		_charcterArmor.ForEachSuitsItemIDs(delegate(int Id)
		{
			ItemObject itemByID = ItemManager.GetItemByID(Id);
			if (itemByID != null)
			{
				list.Add(itemByID);
			}
		});
		ChannelNetwork.SyncItemList(peer, list.ToArray());
	}

	protected void SyncScenarioId(uLink.NetworkPlayer peer)
	{
		int customId = SPTerrainEvent.GetCustomId(base.Id);
		if (customId != -1)
		{
			RPCPeer(peer, EPacketType.PT_Common_ScenarioId, customId);
		}
	}

	private IEnumerator SyncTerrainData()
	{
		int index = AreaHelper.Vector2Int(base.transform.position);
		RoomMgr.UpdateBroadcastSet(index, this);
		yield return null;
		RPCOwner(EPacketType.PT_InGame_TerrainDataOk);
	}

	private void OnVoxelDataChanged(int areaIndex)
	{
	}

	private void OnBlockDataChanged(int areaIndex)
	{
	}

	public bool SetMount(AiMonsterNetwork mount)
	{
		if (null == _mount)
		{
			_mount = mount;
			base._OnRide = true;
			return true;
		}
		return false;
	}

	public bool DelMount(AiMonsterNetwork mount)
	{
		if (null != mount && mount == _mount)
		{
			_mount = null;
			base._OnRide = false;
			return true;
		}
		return false;
	}

	private void RPC_C2S_RequestTerrainData(uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
		RPCPeer(info.sender, EPacketType.PT_InGame_CurTeamId, GroupNetwork.CurNewTeamId);
		StartCoroutine(SyncTerrainData());
	}

	private void RPC_C2S_RequestInit(uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
		RPCPeer(info.sender, EPacketType.PT_InGame_RequestInit, base.transform.position, base.transform.rotation.eulerAngles.y);
	}

	private void RPC_C2S_RequestData(uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
		bool flag = stream.Read<bool>(new object[0]);
		ulong num = stream.Read<ulong>(new object[0]);
		if (flag)
		{
			AddPlayerToGroup(base.WorldId);
			StartCoroutine(SyncOwnerData(info.sender));
		}
		else
		{
			StartCoroutine(SyncProxyData(info.sender));
		}
		SyncScenarioId(info.sender);
	}

	private void RPC_C2S_ReviveSB(uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
		int id = stream.Read<int>(new object[0]);
		if (GetItemNum(937) > 0)
		{
			Player player = GetPlayer(id);
			if (!(null == player))
			{
				player.SyncReviveSB(base.Id);
			}
		}
	}

	private void RPC_C2S_ApprovalRevive(uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
		int id = stream.Read<int>(new object[0]);
		Player player = GetPlayer(id);
		if (!(null == player) && player.GetItemNum(937) > 0)
		{
			List<ItemObject> effItems = new List<ItemObject>();
			player.Package.RemoveItem(937, 1, ref effItems);
			player.SyncItemList(effItems);
			player.SyncPackageIndex();
			PlayerRevive();
		}
	}

	private void RPC_C2S_GameStarted(uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
		ServerConfig.GameStarted = true;
	}

	private void RPC_C2S_NpcPutOnEquip(uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
		int num = stream.Read<int>(new object[0]);
		int num2 = stream.Read<int>(new object[0]);
		ItemPlaceType itemPlaceType = stream.Read<ItemPlaceType>(new object[0]);
		AiAdNpcNetwork aiAdNpcNetwork = ObjNetInterface.Get<AiAdNpcNetwork>(num);
		if (null == aiAdNpcNetwork)
		{
			return;
		}
		ItemObject itemByID = ItemManager.GetItemByID(num2);
		if (itemByID == null)
		{
			if (LogFilter.logDebug)
			{
				Debug.LogWarningFormat("Invalid item[{0}].", num2);
			}
			return;
		}
		Equip cmpt = itemByID.GetCmpt<Equip>();
		if (cmpt == null)
		{
			return;
		}
		int num3 = aiAdNpcNetwork.EquipModule.FindEffectEquipCount(itemByID.protoData.equipPos);
		switch (itemPlaceType)
		{
		default:
			return;
		case ItemPlaceType.IPT_Bag:
		{
			if (Package.GetEmptyGridCount(itemByID.protoData) < num3)
			{
				if (LogFilter.logDebug)
				{
					Debug.LogWarningFormat("Not enough space in player[{0}] package.", base.Id);
				}
				return;
			}
			if (!Package.ExistID(itemByID))
			{
				if (LogFilter.logDebug)
				{
					Debug.LogWarningFormat("Item[{0}] not in player[{1}] package.", num2, base.Id);
				}
				return;
			}
			List<ItemObject> effEquips2 = new List<ItemObject>();
			if (!aiAdNpcNetwork.EquipModule.PutOnEquip(itemByID, ref effEquips2))
			{
				return;
			}
			Package.AddItemList(effEquips2);
			Package.RemoveItem(itemByID);
			SyncPackageIndex();
			break;
		}
		case ItemPlaceType.IPT_ServantInteraction:
		case ItemPlaceType.IPT_ColonyServantInteractionPersonel:
		{
			if (!aiAdNpcNetwork.ItemModule.CanAdd(num3))
			{
				if (LogFilter.logDebug)
				{
					Debug.LogWarningFormat("Not enough space in npc[{0}] package.", num);
				}
				return;
			}
			if (!aiAdNpcNetwork.ItemModule.HasItem(itemByID))
			{
				if (LogFilter.logDebug)
				{
					Debug.LogWarningFormat("Item[{0}] not in npc[{1}] package.", num2, num);
				}
				return;
			}
			List<ItemObject> effEquips3 = new List<ItemObject>();
			if (!aiAdNpcNetwork.EquipModule.PutOnEquip(itemByID, ref effEquips3))
			{
				return;
			}
			aiAdNpcNetwork.ItemModule.AddItem(effEquips3);
			aiAdNpcNetwork.ItemModule.DelItem(num2);
			aiAdNpcNetwork.SyncPackageIndex(0);
			break;
		}
		case ItemPlaceType.IPT_ServantInteraction2:
		case ItemPlaceType.IPT_ColonyServantInteraction2Personel:
		{
			if (!aiAdNpcNetwork.ServantItemModule.CanAdd(num3))
			{
				if (LogFilter.logDebug)
				{
					Debug.LogWarningFormat("Not enough space in npc[{0}] package.", num);
				}
				return;
			}
			if (!aiAdNpcNetwork.ServantItemModule.HasItem(itemByID))
			{
				if (LogFilter.logDebug)
				{
					Debug.LogWarningFormat("Item[{0}] not in npc[{1}] package.", num2, num);
				}
				return;
			}
			List<ItemObject> effEquips = new List<ItemObject>();
			if (!aiAdNpcNetwork.EquipModule.PutOnEquip(itemByID, ref effEquips))
			{
				return;
			}
			aiAdNpcNetwork.ServantItemModule.AddItem(effEquips);
			aiAdNpcNetwork.ServantItemModule.DelItem(num2);
			aiAdNpcNetwork.SyncPackageIndex(1);
			break;
		}
		case ItemPlaceType.IPT_HotKeyBar:
		case ItemPlaceType.IPT_Equipment:
		case ItemPlaceType.IPT_ServantEqu:
		case ItemPlaceType.IPT_ServantSkill:
		case ItemPlaceType.IPT_ConolyServantEquPersonel:
			return;
		}
		aiAdNpcNetwork.SyncPutOnEquip(itemByID);
	}

	private void RPC_C2S_NpcTakeOffEquip(uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
		int num = stream.Read<int>(new object[0]);
		int num2 = stream.Read<int>(new object[0]);
		int num3 = stream.Read<int>(new object[0]);
		AiAdNpcNetwork aiAdNpcNetwork = ObjNetInterface.Get<AiAdNpcNetwork>(num);
		if (null == aiAdNpcNetwork)
		{
			return;
		}
		ItemObject itemObject = aiAdNpcNetwork.EquipModule[num2];
		if (itemObject == null)
		{
			if (LogFilter.logDebug)
			{
				Debug.LogWarningFormat("Invalid item[{0}].", num2);
			}
			return;
		}
		int num4 = aiAdNpcNetwork.EquipModule.FindEffectEquipCount(itemObject);
		if (!aiAdNpcNetwork.ItemModule.CanAdd(num4))
		{
			if (LogFilter.logDebug)
			{
				Debug.LogWarningFormat("Not enough space in npc[{0}] package.", num);
			}
			return;
		}
		List<ItemObject> effEquips = new List<ItemObject>();
		if (aiAdNpcNetwork.EquipModule.TakeOffEquip(itemObject, ref effEquips))
		{
			aiAdNpcNetwork.ItemModule.AddItem(effEquips);
			aiAdNpcNetwork.SyncPackageIndex(0);
			aiAdNpcNetwork.SyncTakeOffEquip(itemObject.instanceId);
		}
	}

	private void RPC_C2S_DismissByPlayer(uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
		int id = stream.Read<int>(new object[0]);
		AiAdNpcNetwork aiAdNpcNetwork = ObjNetInterface.Get<AiAdNpcNetwork>(id);
		if (!(null == aiAdNpcNetwork) && !(null == aiAdNpcNetwork.lordPlayer) && Equals(aiAdNpcNetwork.lordPlayer) && !aiAdNpcNetwork.bForcedServant)
		{
			aiAdNpcNetwork.DismissByPlayer();
		}
	}

	private void RPC_C2S_FoundMapLable(uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
		int num = stream.Read<int>(new object[0]);
		if (ServerConfig.AddFoundMapLable(num))
		{
			List<int> list = new List<int>();
			list.Add(num);
			RPCOthers(EPacketType.PT_Common_FoundMapLable, list.ToArray());
		}
	}

	private void RPC_C2S_SwitchArmorSuit(uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
		int num = stream.Read<int>(new object[0]);
		bool flag = _charcterArmor.SwitchArmorSuit(num);
		if (flag)
		{
			SyncPackageIndex();
		}
		RPCOthers(EPacketType.PT_InGame_SwitchArmorSuit, num, flag);
	}

	private void RPC_C2S_EquipArmorPart(uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
		int num = stream.Read<int>(new object[0]);
		int num2 = stream.Read<int>(new object[0]);
		int num3 = stream.Read<int>(new object[0]);
		int num4 = stream.Read<int>(new object[0]);
		bool flag = _charcterArmor.EquipArmorPartFromPackage(num, num2, num3, num4);
		if (flag)
		{
			ItemObject itemByID = ItemManager.GetItemByID(num);
			if (itemByID != null)
			{
				ChannelNetwork.SyncItem(base.WorldId, itemByID);
			}
			SyncPackageIndex();
		}
		RPCOthers(EPacketType.PT_InGame_EquipArmorPart, num, num2, num3, num4, flag);
	}

	private void RPC_C2S_RemoveArmorPart(uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
		int num = stream.Read<int>(new object[0]);
		int num2 = stream.Read<int>(new object[0]);
		bool flag = stream.Read<bool>(new object[0]);
		bool flag2 = _charcterArmor.RemoveArmorPart(num, num2, flag);
		if (flag2)
		{
			SyncPackageIndex();
		}
		RPCOthers(EPacketType.PT_InGame_RemoveArmorPart, num, num2, flag, flag2);
	}

	private void RPC_C2S_SwitchArmorPartMirror(uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
		int num = stream.Read<int>(new object[0]);
		int num2 = stream.Read<int>(new object[0]);
		bool flag = stream.Read<bool>(new object[0]);
		bool flag2 = _charcterArmor.SwitchArmorPartMirror(num, num2, flag);
		RPCOthers(EPacketType.PT_InGame_SwitchArmorPartMirror, num, num2, flag, flag2);
	}

	private void RPC_C2S_SyncArmorPartPos(uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
		int num = stream.Read<int>(new object[0]);
		int num2 = stream.Read<int>(new object[0]);
		bool flag = stream.Read<bool>(new object[0]);
		Vector3 vector = stream.Read<Vector3>(new object[0]);
		if (_charcterArmor.SetArmorPartPosition(num, num2, flag, vector))
		{
			RPCOthers(EPacketType.PT_InGame_SyncArmorPartPos, num, num2, flag, vector);
		}
	}

	private void RPC_C2S_SyncArmorPartRot(uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
		int num = stream.Read<int>(new object[0]);
		int num2 = stream.Read<int>(new object[0]);
		bool flag = stream.Read<bool>(new object[0]);
		Quaternion quaternion = stream.Read<Quaternion>(new object[0]);
		if (_charcterArmor.SetArmorPartRotation(num, num2, flag, quaternion))
		{
			RPCOthers(EPacketType.PT_InGame_SyncArmorPartRot, num, num2, flag, quaternion);
		}
	}

	private void RPC_C2S_SyncArmorPartScale(uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
		int num = stream.Read<int>(new object[0]);
		int num2 = stream.Read<int>(new object[0]);
		bool flag = stream.Read<bool>(new object[0]);
		Vector3 vector = stream.Read<Vector3>(new object[0]);
		if (_charcterArmor.SetArmorPartScale(num, num2, flag, vector))
		{
			RPCOthers(EPacketType.PT_InGame_SyncArmorPartScale, num, num2, flag, vector);
		}
	}

	private void RPC_C2S_HeartBeat(uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
		lastHeartBeatTime = Time.time;
	}

	private void RPC_C2S_ReqMonsterCtrl(uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
		int num = stream.Read<int>(new object[0]);
		AiMonsterNetwork aiMonsterNetwork = ObjNetInterface.Get<AiMonsterNetwork>(num);
		Player player = GetPlayer(info.sender);
		if ((bool)aiMonsterNetwork && (bool)player && aiMonsterNetwork.ForceGetController(player))
		{
			RPCPeer(info.sender, EPacketType.PT_Mount_ReqMonsterCtrl, player.Id, num);
		}
	}

	private void RPC_C2S_AddRideMonster(uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
		int num = stream.Read<int>(new object[0]);
		AiMonsterNetwork aiMonsterNetwork = ObjNetInterface.Get<AiMonsterNetwork>(num);
		Player player = GetPlayer(info.sender);
		if ((bool)aiMonsterNetwork && (bool)player && aiMonsterNetwork.MountByPlayer(player))
		{
			RPCProxy(EPacketType.PT_Mount_AddMountMonster, player.Id, num);
		}
	}

	private void RPC_C2S_DelRideMonster(uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
		int num = stream.Read<int>(new object[0]);
		AiMonsterNetwork aiMonsterNetwork = ObjNetInterface.Get<AiMonsterNetwork>(num);
		Player player = GetPlayer(info.sender);
		if ((bool)aiMonsterNetwork && (bool)player && aiMonsterNetwork.DeMountByPlayer(player))
		{
			RPCProxy(EPacketType.PT_Mount_DelMountMonster, player.Id, num, aiMonsterNetwork._backupTeamId);
		}
	}

	private void RPC_C2S_SyncPlayerRot(uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
		if (stream.TryRead<Vector3>(out var value))
		{
			base.transform.rotation = Quaternion.Euler(value);
		}
		URPCProxy(EPacketType.PT_Mount_SyncPlayerRot, value);
	}

	private void RPC_C2S_PersonalStorageStore(uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
		int objID = stream.Read<int>(new object[0]);
		int dstIndex = stream.Read<int>(new object[0]);
		StorageManager.Store(this, objID, dstIndex);
	}

	private void RPC_C2S_PersonalStorageDelete(uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
		int objID = stream.Read<int>(new object[0]);
		StorageManager.Delete(this, objID);
	}

	private void RPC_C2S_PersonalStorageFetch(uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
		int objID = stream.Read<int>(new object[0]);
		int dstIndex = stream.Read<int>(new object[0]);
		StorageManager.Fetch(this, objID, dstIndex);
	}

	private void RPC_C2S_PersonalStorageSplit(uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
		int objID = stream.Read<int>(new object[0]);
		int num = stream.Read<int>(new object[0]);
		StorageManager.Split(this, objID, num);
	}

	private void RPC_C2S_PersonalStorageExchange(uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
		int objID = stream.Read<int>(new object[0]);
		int originIndex = stream.Read<int>(new object[0]);
		int destIndex = stream.Read<int>(new object[0]);
		StorageManager.Exchange(this, objID, originIndex, destIndex);
	}

	private void RPC_C2S_PersonalStorageSort(uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
		int tabIndex = stream.Read<int>(new object[0]);
		StorageManager.Sort(this, tabIndex);
	}

	private void RPC_C2S_PublicStorageStore(uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
		int objectId = stream.Read<int>(new object[0]);
		int storageIndex = stream.Read<int>(new object[0]);
		ItemObject itemByID = ItemManager.GetItemByID(objectId);
		GroupNetwork.Store(this, itemByID, storageIndex);
	}

	private void RPC_C2S_PublicStorageExchange(uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
		int objectId = stream.Read<int>(new object[0]);
		int srcIndex = stream.Read<int>(new object[0]);
		int destIndex = stream.Read<int>(new object[0]);
		ItemObject itemByID = ItemManager.GetItemByID(objectId);
		GroupNetwork.Change(this, itemByID, srcIndex, destIndex);
	}

	private void RPC_C2S_PublicStorageFetch(uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
		int objectId = stream.Read<int>(new object[0]);
		int packageIndex = stream.Read<int>(new object[0]);
		ItemObject itemByID = ItemManager.GetItemByID(objectId);
		GroupNetwork.Fetch(this, itemByID, packageIndex);
	}

	private void RPC_C2S_PublicStroageDelete(uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
		int objectId = stream.Read<int>(new object[0]);
		ItemObject itemByID = ItemManager.GetItemByID(objectId);
		GroupNetwork.Delete(this, itemByID);
	}

	private void RPC_C2S_PublicStorageSplit(uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
		int objectId = stream.Read<int>(new object[0]);
		int num = stream.Read<int>(new object[0]);
		ItemObject itemByID = ItemManager.GetItemByID(objectId);
		GroupNetwork.Split(this, itemByID, num);
	}

	private void RPC_C2S_PublicStorageSort(uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
		int tab = stream.Read<int>(new object[0]);
		if (!(lastSortTime > Time.time))
		{
			lastSortTime = Time.time + 3f;
			GroupNetwork.Sort(this, tab);
		}
	}

	private void SyncWeather()
	{
		RPCOwner(EPacketType.PT_Common_InitWeather, (int)WeatherManager.Instance.CurrentWeather);
	}

	public bool IsDirtyVoxelArea(int index)
	{
		return !voxelAreaSent.Contains(index);
	}

	public bool IsDirtyBlockArea(int index)
	{
		return !blockAreaSent.Contains(index);
	}

	internal void SendVoxelData(int index)
	{
		GameWorld gameWorld = GameWorld.GetGameWorld(base.WorldId);
		if (gameWorld != null)
		{
			voxelAreaSent.Add(index);
			byte[] array = gameWorld.GenBSVoxelData(0, index);
			if (array != null)
			{
				ChannelNetwork.SyncChannelPeer(base.OwnerView.owner, EPacketType.PT_Common_VoxelData, index, array);
			}
		}
	}

	internal void SendBlockData(int index)
	{
		GameWorld gameWorld = GameWorld.GetGameWorld(base.WorldId);
		if (gameWorld != null)
		{
			blockAreaSent.Add(index);
			byte[] array = gameWorld.GenBSVoxelData(1, index);
			if (array != null)
			{
				ChannelNetwork.SyncChannelPeer(base.OwnerView.owner, EPacketType.PT_Common_BlockData, index, array);
			}
		}
	}
}

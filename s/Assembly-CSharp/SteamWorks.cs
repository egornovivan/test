using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using ItemAsset;
using Mono.Data.SqliteClient;
using NetworkHelper;
using uLink;
using UnityEngine;

public class SteamWorks : UnityEngine.MonoBehaviour
{
	private static SteamWorks _instance;

	public static Dictionary<string, FileSender> _FileSenderMgr = new Dictionary<string, FileSender>();

	private static List<CreationOriginData> _creationList = new List<CreationOriginData>();

	private static List<ItemProto> _creationItemData = new List<ItemProto>();

	private static List<RegisteredISO> _creationRegList = new List<RegisteredISO>();

	private static Dictionary<int, List<ulong>> _playerDic = new Dictionary<int, List<ulong>>();

	private void Awake()
	{
		_instance = this;
	}

	public static void RPC_C2S_RequestUGC(uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
		List<RegisteredISO> registerISO = GetRegisterISO();
		int count = registerISO.Count;
		if (count <= 0)
		{
			NetworkManager.SyncPeer(info.sender, EPacketType.PT_Common_RequestUGC, count);
		}
		else
		{
			NetworkManager.SyncPeer(info.sender, EPacketType.PT_Common_RequestUGC, count, registerISO.ToArray(), false);
		}
	}

	public static void RPC_C2S_WorkShopShared(uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
		ulong num = stream.Read<ulong>(new object[0]);
		string isoName = stream.Read<string>(new object[0]);
		ulong hashCode = stream.Read<ulong>(new object[0]);
		bool free = stream.Read<bool>(new object[0]);
		int instanceId = stream.Read<int>(new object[0]);
		RegisteredISO registeredISO = GetRegisterISO(num);
		if (registeredISO == null)
		{
			registeredISO = new RegisteredISO();
			registeredISO.HashCode = hashCode;
			registeredISO.IsoName = isoName;
			registeredISO.UGCHandle = num;
			AddRegisterISO(registeredISO);
			SaveRegISO2DB(registeredISO);
		}
		int objectID = ItemManager.CreateObjectID();
		int seed = UnityEngine.Random.Range(100, 10000000);
		CreationOriginData creationOriginData = new CreationOriginData();
		creationOriginData.HashCode = hashCode;
		creationOriginData.Seed = seed;
		creationOriginData.ObjectID = objectID;
		creationOriginData.free = free;
		creationOriginData.instanceId = instanceId;
		Player player = Player.GetPlayer(info.sender);
		if (player != null)
		{
			creationOriginData.SteamId = player.steamId;
		}
		AddRegisterCreation(creationOriginData);
		NetworkManager.SyncProxy(EPacketType.PT_Common_WorkshopShared, registeredISO);
	}

	public static void RPC_C2S_UGCDownLoadComplete(uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
		ulong handleID = stream.Read<ulong>(new object[0]);
		RegisteredISO registerISO = GetRegisterISO(handleID);
		if (registerISO != null)
		{
			int baseNetId = BaseNetwork.GetBaseNetId(info.sender);
			AddRegistered(baseNetId, registerISO.HashCode);
			IEnumerable<CreationOriginData> registerCreation = GetRegisterCreation(registerISO.HashCode);
			if (registerCreation.Count() >= 1)
			{
				NetworkManager.SyncPeer(info.sender, EPacketType.PT_Common_UGCList, registerCreation.ToArray(), false);
			}
			ulong sID = 0uL;
			Player player = Player.GetPlayer(info.sender);
			if (player != null)
			{
				sID = player.steamId;
			}
			IEnumerable<int> source = from CreationOriginData iter in registerCreation
				where iter.SteamId == sID && null == ItemProto.Mgr.Instance.Get(iter.ObjectID)
				select iter.ObjectID;
			if (source.Count() >= 1)
			{
				NetworkManager.SyncPeer(info.sender, EPacketType.PT_Common_UGCData, source.ToArray());
			}
		}
	}

	public static void RPC_C2S_UGCItemData(uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
		int num = stream.Read<int>(new object[0]);
		byte[] array = stream.Read<byte[]>(new object[0]);
		ItemProto itemData = ItemProto.GetItemData(num);
		if (itemData == null)
		{
			itemData = new ItemProto();
			itemData.GenItemData(num, array);
			AddItemData(num);
			SaveItem(num, array);
		}
	}

	public static void RPC_S2C_UGCSkillData(uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
		byte[] array = stream.Read<byte[]>(new object[0]);
	}

	public static void RPC_C2S_UGCData(uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
		_instance.StartCoroutine(UGCData(stream, info));
	}

	public static void RPC_C2S_DownTimeOut(uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
		ulong num = stream.Read<ulong>(new object[0]);
		ulong num2 = stream.Read<ulong>(new object[0]);
		string path = GetUserDataPath() + "/PlanetExplorers/VoxelCreationData/Creations/NetCache/" + num.ToString("X").PadLeft(16, '0') + ".~vcres";
		if (!File.Exists(path))
		{
			NetworkManager.SyncPeer(info.sender, EPacketType.PT_Common_DownTimeOut, num2);
			return;
		}
		byte[] array = File.ReadAllBytes(path);
		if (array != null)
		{
			string skey = info.networkView.viewID.id + num.ToString("X").PadLeft(16, '0');
			FileSender fileSender = new FileSender(num.ToString("X").PadLeft(16, '0'), array, num2, _FileSenderMgr, skey);
			NetworkManager.SyncPeer(info.sender, EPacketType.PT_Common_StartSendFile, fileSender.m_FileName, fileSender.m_IndexInList, array.Length, num2);
			int count = 0;
			byte[] array2 = fileSender.ReadData(ref count);
			NetworkManager.SyncPeer(info.sender, EPacketType.PT_Common_SendFile, array2, count, fileSender.m_IndexInList);
		}
	}

	public static void RPC_C2S_FileDontExists(uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
		ulong hashCode = stream.Read<ulong>(new object[0]);
		ulong num = stream.Read<ulong>(new object[0]);
		string path = GetUserDataPath() + "/PlanetExplorers/VoxelCreationData/Creations/NetCache/" + hashCode.ToString("X").PadLeft(16, '0') + ".~vcres";
		if (!File.Exists(path))
		{
			NetworkManager.SyncPeer(info.sender, EPacketType.PT_Common_InvalidUGC, num);
			RemoveRegisterISO(hashCode);
			return;
		}
		byte[] array = File.ReadAllBytes(path);
		if (array != null)
		{
			string skey = info.networkView.viewID.id + hashCode.ToString("X").PadLeft(16, '0');
			FileSender fileSender = new FileSender(hashCode.ToString("X").PadLeft(16, '0'), array, num, _FileSenderMgr, skey);
			NetworkManager.SyncPeer(info.sender, EPacketType.PT_Common_PreUpload, fileSender.m_FileName, fileSender.m_IndexInList, array.Length, num);
			int count = 0;
			byte[] array2 = fileSender.ReadData(ref count);
			NetworkManager.SyncPeer(info.sender, EPacketType.PT_Common_UGCUpload, array2, count, fileSender.m_IndexInList);
		}
	}

	public static void RPC_C2S_SendFile(uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
		string key = stream.Read<string>(new object[0]);
		int num = stream.Read<int>(new object[0]);
		if (stream.Read<bool>(new object[0]) && _FileSenderMgr.ContainsKey(key) && _FileSenderMgr[key] != null)
		{
			int count = 0;
			_FileSenderMgr[key].m_Sended += num;
			byte[] array = _FileSenderMgr[key].ReadData(ref count);
			if (array != null)
			{
				NetworkManager.SyncPeer(info.sender, EPacketType.PT_Common_UGCUpload, array, count, _FileSenderMgr[key].m_IndexInList);
			}
			else
			{
				_FileSenderMgr.Remove(key);
			}
		}
	}

	public static void Load()
	{
		LoadRegISO();
		LoadCreation();
		LoadCreationItem();
		LoadCreationSkill();
	}

	private static void LoadSkillComplete(SqliteDataReader reader)
	{
	}

	private static void LoadCreationSkill()
	{
		PEDbOp pEDbOp = AsyncSqlite.NewReadOp();
		if (pEDbOp != null)
		{
			pEDbOp.SetCmdText("SELECT * FROM skilldata;");
			pEDbOp.BindReaderHandler(LoadSkillComplete);
			pEDbOp.Exec();
			pEDbOp = null;
		}
	}

	private static void LoadItemComplete(SqliteDataReader reader)
	{
		while (reader.Read())
		{
			int @int = reader.GetInt32(reader.GetOrdinal("ver"));
			int int2 = reader.GetInt32(reader.GetOrdinal("itemid"));
			byte[] buffer = (byte[])reader.GetValue(reader.GetOrdinal("blobdata"));
			ItemProto item = new ItemProto();
			item.GenItemData(int2, buffer);
		}
	}

	private static void LoadCreationItem()
	{
		PEDbOp pEDbOp = AsyncSqlite.NewReadOp();
		if (pEDbOp != null)
		{
			pEDbOp.SetCmdText("SELECT * FROM itemdata;");
			pEDbOp.BindReaderHandler(LoadItemComplete);
			pEDbOp.Exec();
			pEDbOp = null;
		}
	}

	private static void LoadCreationComplete(SqliteDataReader reader)
	{
		while (reader.Read())
		{
			int @int = reader.GetInt32(reader.GetOrdinal("ver"));
			int objID = reader.GetInt32(reader.GetOrdinal("objid"));
			if (!_creationList.Exists((CreationOriginData iter) => iter.ObjectID == objID))
			{
				int int2 = reader.GetInt32(reader.GetOrdinal("seed"));
				float @float = reader.GetFloat(reader.GetOrdinal("hp"));
				float float2 = reader.GetFloat(reader.GetOrdinal("maxhp"));
				float float3 = reader.GetFloat(reader.GetOrdinal("fuel"));
				float float4 = reader.GetFloat(reader.GetOrdinal("maxfuel"));
				ulong int3 = (ulong)reader.GetInt64(reader.GetOrdinal("hash"));
				ulong int4 = (ulong)reader.GetInt64(reader.GetOrdinal("steamid"));
				CreationOriginData creationOriginData = new CreationOriginData();
				creationOriginData.ObjectID = objID;
				creationOriginData.Seed = int2;
				creationOriginData.HP = @float;
				creationOriginData.MaxHP = float2;
				creationOriginData.Fuel = float3;
				creationOriginData.MaxFuel = float4;
				creationOriginData.HashCode = int3;
				creationOriginData.SteamId = int4;
				AddRegisterCreation(creationOriginData);
			}
		}
	}

	private static void LoadCreation()
	{
		PEDbOp pEDbOp = AsyncSqlite.NewReadOp();
		if (pEDbOp != null)
		{
			pEDbOp.SetCmdText("SELECT * FROM creationdata;");
			pEDbOp.BindReaderHandler(LoadCreationComplete);
			pEDbOp.Exec();
			pEDbOp = null;
		}
	}

	private static void LoadIsoComplete(SqliteDataReader reader)
	{
		while (reader.Read())
		{
			int @int = reader.GetInt32(reader.GetOrdinal("ver"));
			RegisteredISO registeredISO = new RegisteredISO();
			registeredISO.HashCode = (ulong)reader.GetInt64(reader.GetOrdinal("hash"));
			registeredISO.UGCHandle = (ulong)reader.GetInt64(reader.GetOrdinal("handle"));
			registeredISO.IsoName = reader.GetString(reader.GetOrdinal("name"));
			byte[] array = (byte[])reader.GetValue(reader.GetOrdinal("blobdata"));
			if (array != null)
			{
				using MemoryStream input = new MemoryStream(array);
				using BinaryReader reader2 = new BinaryReader(input);
				int num = BufferHelper.ReadInt32(reader2);
				for (int i = 0; i < num; i++)
				{
					EVCComponent item = (EVCComponent)BufferHelper.ReadInt32(reader2);
					registeredISO.Components.Add(item);
				}
			}
			AddRegisterISO(registeredISO);
		}
	}

	private static void LoadRegISO()
	{
		PEDbOp pEDbOp = AsyncSqlite.NewReadOp();
		if (pEDbOp != null)
		{
			pEDbOp.SetCmdText("SELECT * FROM registerediso;");
			pEDbOp.BindReaderHandler(LoadIsoComplete);
			pEDbOp.Exec();
			pEDbOp = null;
		}
	}

	private static void DeleteRegISOFromDB(ulong hashCode)
	{
	}

	internal static bool DispatchComplete(ulong hashCode)
	{
		return BaseNetwork.ForeachActionAll((int id) => HasRegistered(id, hashCode) ? true : false);
	}

	internal static bool DispatchComplete(int id)
	{
		foreach (RegisteredISO creationReg in _creationRegList)
		{
			if (!HasRegistered(id, creationReg.HashCode))
			{
				return false;
			}
		}
		return true;
	}

	internal static void RemoveRegisteredPlayer(int id)
	{
		_playerDic.Remove(id);
	}

	internal static bool HasRegistered(int id, ulong hashCode)
	{
		if (!_playerDic.ContainsKey(id))
		{
			_playerDic[id] = new List<ulong>();
			return false;
		}
		return _playerDic[id].Contains(hashCode);
	}

	internal static void AddRegistered(int id, ulong hashCode)
	{
		if (!_playerDic.ContainsKey(id))
		{
			_playerDic[id] = new List<ulong>();
		}
		if (!_playerDic[id].Contains(hashCode))
		{
			_playerDic[id].Add(hashCode);
		}
	}

	internal static void AddItemData(ItemProto item)
	{
		if (item != null && !_creationItemData.Exists((ItemProto iter) => iter.id == item.id))
		{
			_creationItemData.Add(item);
		}
	}

	internal static bool HasItemData(int itemID)
	{
		return _creationItemData.Exists((ItemProto iter) => iter.id == itemID);
	}

	internal static void AddItemData(int itemID)
	{
		ItemProto itemData = ItemProto.GetItemData(itemID);
		AddItemData(itemData);
	}

	public static void SaveItem(int objId, byte[] itemData)
	{
		UGCItemDbData uGCItemDbData = new UGCItemDbData();
		uGCItemDbData.ExportData(objId, itemData);
		AsyncSqlite.AddRecord(uGCItemDbData);
	}

	internal static void AddRegisterISO(RegisteredISO iso)
	{
		if (!IsRegisteredISO(iso.UGCHandle))
		{
			_creationRegList.Add(iso);
		}
	}

	internal static List<RegisteredISO> GetRegisterISO()
	{
		return _creationRegList;
	}

	internal static RegisteredISO GetRegisterISO(ulong handleID)
	{
		return _creationRegList.Find((RegisteredISO iter) => iter.UGCHandle == handleID);
	}

	internal static RegisteredISO GetRegisterISOByHash(ulong hashCode)
	{
		return _creationRegList.Find((RegisteredISO iter) => iter.HashCode == hashCode);
	}

	internal static bool IsRegisteredISO(ulong handleID)
	{
		return _creationRegList.Exists((RegisteredISO iter) => iter.UGCHandle == handleID);
	}

	internal static void RemoveRegisterISO(ulong hashCode)
	{
		_creationRegList.RemoveAll((RegisteredISO iter) => iter.HashCode == hashCode);
	}

	public static void SaveRegISO2DB(RegisteredISO regISO)
	{
		RegISODbData regISODbData = new RegISODbData();
		regISODbData.ExportData((long)regISO.HashCode, (long)regISO.UGCHandle, regISO.IsoName, regISO.GetComponentsData());
		AsyncSqlite.AddRecord(regISODbData);
	}

	internal static void AddRegisterCreation(CreationOriginData creation)
	{
		if (!_creationList.Exists((CreationOriginData iter) => iter.ObjectID == creation.ObjectID))
		{
			_creationList.Add(creation);
		}
	}

	internal static IEnumerable<CreationOriginData> GetRegisterCreation(ulong hashCode)
	{
		return _creationList.FindAll((CreationOriginData iter) => iter.HashCode == hashCode);
	}

	internal static CreationOriginData GetCreationData(int objID)
	{
		return _creationList.Find((CreationOriginData iter) => iter.ObjectID == objID);
	}

	internal static void RemoveCreationData(int objID)
	{
		ulong num = 0uL;
		foreach (CreationOriginData creation in _creationList)
		{
			if (creation.ObjectID == objID)
			{
				num = creation.HashCode;
			}
		}
		if (num != 0L)
		{
			_creationList.RemoveAll((CreationOriginData iter) => iter.ObjectID == objID);
			DeleteCreationFromDB(objID);
			RemoveCDataIfAllDeath(num);
		}
	}

	internal static void RemoveCDataIfAllDeath(ulong HashCode)
	{
		if (_creationList.FindIndex((CreationOriginData iter) => iter.HashCode == HashCode) < 0)
		{
			_creationRegList.RemoveAll((RegisteredISO iter1) => iter1.HashCode == HashCode);
			DeleteCreationRegFromDB((long)HashCode);
		}
	}

	public static void DeleteCreationFromDB(int objID)
	{
		DeleteCreationDbData deleteCreationDbData = new DeleteCreationDbData();
		deleteCreationDbData.DeleteData(objID);
		AsyncSqlite.AddRecord(deleteCreationDbData);
	}

	public static void DeleteCreationRegFromDB(long hashCode)
	{
		DeleteCreationRgeDbData deleteCreationRgeDbData = new DeleteCreationRgeDbData();
		deleteCreationRgeDbData.DeleteData(hashCode);
		AsyncSqlite.AddRecord(deleteCreationRgeDbData);
	}

	private static IEnumerator UGCData(uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
		int objectID = stream.Read<int>(new object[0]);
		float hp = stream.Read<float>(new object[0]);
		float fuel = stream.Read<float>(new object[0]);
		int[] ids = stream.Read<int[]>(new object[0]);
		int[] nums = stream.Read<int[]>(new object[0]);
		bool hasSeat = stream.Read<bool>(new object[0]);
		int id = Player.GetPlayerId(info.sender);
		CreationOriginData data = GetCreationData(objectID);
		ulong sID = 0uL;
		Player player = Player.GetPlayer(id);
		if (player != null)
		{
			sID = player.steamId;
		}
		if (data == null || data.SteamId != sID)
		{
			if (LogFilter.logDebug)
			{
				Debug.LogError("Ivalid creation.");
			}
			yield break;
		}
		if (hasSeat)
		{
			RegisteredISO iso = GetRegisterISOByHash(data.HashCode);
			if (iso != null && iso.Components.Count <= 0)
			{
				int[] components = stream.Read<int[]>(new object[0]);
				int[] array = components;
				foreach (int component in array)
				{
					iso.Components.Add((EVCComponent)component);
				}
				SaveRegISO2DB(iso);
			}
		}
		data.HP = (data.MaxHP = hp);
		data.Fuel = (data.MaxFuel = fuel);
		while (!DispatchComplete(data.HashCode))
		{
			yield return null;
		}
		player = Player.GetPlayer(id);
		if (null == player)
		{
			if (LogFilter.logWarn)
			{
				Debug.LogWarningFormat("Ivalid owner, ignore the creation object");
			}
			RemoveCreationData(objectID);
			yield break;
		}
		CreationOriginData isodata = GetCreationData(objectID);
		PlayerPackageCmpt package = player.Package;
		List<ItemObject> effectItems = new List<ItemObject>();
		int dungeonId;
		bool isDungeonIso = DungeonIsos.IsDungeonIso(data.HashCode, out dungeonId);
		if (!ServerConfig.UnlimitedRes && !ServerConfig.IsBuild && !isDungeonIso && !data.free)
		{
			int count = Mathf.Min(ids.Length, nums.Length);
			for (int j = 0; j < count; j++)
			{
				int num = package.GetItemCount(ids[j]);
				if (num < nums[j])
				{
					RemoveCreationData(objectID);
					if (LogFilter.logDebug)
					{
						Debug.LogWarning("Not enough resources.");
					}
					NetworkManager.SyncPeer(player.OwnerView.owner, EPacketType.PT_Common_IsoExportSuccessed, isodata.instanceId, false);
					yield break;
				}
			}
			for (int k = 0; k < count; k++)
			{
				package.RemoveItem(ids[k], nums[k], ref effectItems);
			}
		}
		data.Save();
		ItemObject itemObject = ItemManager.CreateItem(objectID, 1);
		if (itemObject == null)
		{
			if (LogFilter.logWarn)
			{
				Debug.LogWarningFormat("Ivalid itemobj");
			}
			yield break;
		}
		if (!isDungeonIso)
		{
			package.AddItem(itemObject);
			effectItems.Add(itemObject);
			if (effectItems.Count > 0)
			{
				player.SyncItemList(effectItems.ToArray());
			}
			ItemSample[] newItems = new ItemSample[1]
			{
				new ItemSample(itemObject.protoId, itemObject.stackCount)
			};
			player.SyncPackageIndex();
			player.SyncNewItem(newItems);
			player.SyncErrorMsg("Item transfer completed!");
		}
		else
		{
			RandomDunGenMgr.Instance.ReceiveIsoObject(dungeonId, data.HashCode, itemObject.instanceId);
		}
		if (!data.free)
		{
			NetworkManager.SyncPeer(player.OwnerView.owner, EPacketType.PT_Common_IsoExportSuccessed, isodata.instanceId, true);
		}
	}

	private static string GetUserDataPath()
	{
		return Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
	}

	public static void SendGetRandIsoIds(int dungeonId, int amount, string isoTag, Player player)
	{
		if (DungeonIsos.AddItem(amount, dungeonId))
		{
			NetworkManager.SyncPeer(player.networkView.owner, EPacketType.PT_Common_GetRandIsoFileIds, dungeonId, amount, isoTag);
		}
	}

	public static void RPC_C2S_SendRandIsoFileIds(uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
		int num = stream.Read<int>(new object[0]);
		ulong[] array = stream.Read<ulong[]>(new object[0]);
		ulong[] array2 = stream.Read<ulong[]>(new object[0]);
		if (DungeonIsos.AddIsos(num, array, array2))
		{
			NetworkManager.SyncProxy(EPacketType.PT_Common_SendRandIsoFileIds, num, array, array2);
		}
	}

	public static void RPC_C2S_ExportRandIso(uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
		int num = stream.Read<int>(new object[0]);
		int num2 = stream.Read<int>(new object[0]);
		string text = stream.Read<string>(new object[0]);
		if (DungeonIsos.CheckExported(num, num2))
		{
			NetworkManager.SyncPeer(info.sender, EPacketType.PT_Common_ExportRandIso, num, num2, text);
			DungeonIsos.SetIsoState(num, num2, DungeonIsos.IsoState.IsoState_Exporting);
		}
	}

	public static void RPC_C2S_SendRandIsoHash(uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
		int dungeonId = stream.Read<int>(new object[0]);
		int index = stream.Read<int>(new object[0]);
		ulong hashcode = stream.Read<ulong>(new object[0]);
		DungeonIsos.SetHashCode(dungeonId, index, hashcode);
	}
}

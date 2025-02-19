using System.Collections.Generic;
using System.IO;
using ItemAsset;
using Mono.Data.SqliteClient;
using PETools;

public class DropItemManager
{
	private static DropItemManager _instance;

	private static List<int> _existIds = new List<int>();

	public static DropItemManager Instance
	{
		get
		{
			if (_instance == null)
			{
				_instance = new DropItemManager();
			}
			return _instance;
		}
	}

	public static bool HasRecord(int id)
	{
		return _existIds.Contains(id);
	}

	internal static void OnDestroy(NetInterface sender)
	{
		ObjNetInterface objNetInterface = sender as ObjNetInterface;
		if (!(null == objNetInterface))
		{
			_existIds.Remove(objNetInterface.Id);
		}
	}

	internal static void OnInitialized(NetInterface sender)
	{
		ObjNetInterface objNetInterface = sender as ObjNetInterface;
		if (!(null == objNetInterface) && !HasRecord(objNetInterface.Id))
		{
			_existIds.Add(objNetInterface.Id);
		}
	}

	public static void LoadComplete(SqliteDataReader reader)
	{
		List<ColonyNetworkRenewData> list = new List<ColonyNetworkRenewData>();
		while (reader.Read())
		{
			int @int = reader.GetInt32(reader.GetOrdinal("ver"));
			int int2 = reader.GetInt32(reader.GetOrdinal("id"));
			int int3 = reader.GetInt32(reader.GetOrdinal("externid"));
			int int4 = reader.GetInt32(reader.GetOrdinal("teamnum"));
			ObjType int5 = (ObjType)reader.GetInt32(reader.GetOrdinal("itemtype"));
			byte[] buffer = (byte[])reader.GetValue(reader.GetOrdinal("blobdata"));
			ItemObject itemByID = ItemManager.GetItemByID(int2);
			if (itemByID == null || HasRecord(int2))
			{
				continue;
			}
			_existIds.Add(int2);
			using MemoryStream input = new MemoryStream(buffer);
			using BinaryReader reader2 = new BinaryReader(input);
			BufferHelper.ReadVector3(reader2, out var _value);
			BufferHelper.ReadQuaternion(reader2, out var _value2);
			switch (int5)
			{
			case ObjType.Tower:
			{
				int num7 = BufferHelper.ReadInt32(reader2);
				int num8 = BufferHelper.ReadInt32(reader2);
				NetInterface.Instantiate(PrefabManager.Self.AiTowerNetworkSeed, _value, _value2, num8, int2, 1f, num7, int4);
				break;
			}
			case ObjType.Flag:
			{
				int num3 = BufferHelper.ReadInt32(reader2);
				int num4 = BufferHelper.ReadInt32(reader2);
				NetInterface.Instantiate(PrefabManager.Self.AiFlagNetwork, _value, _value2, num4, int2, int4, num3, int3);
				break;
			}
			case ObjType.SceneStatic:
			{
				int num5 = BufferHelper.ReadInt32(reader2);
				int num6 = BufferHelper.ReadInt32(reader2);
				NetInterface.Instantiate(PrefabManager.Self.AiSceneStaticObject, _value, _value2, num6, int2, int4, num5, int3);
				break;
			}
			case ObjType.Creation:
			{
				CommonHelper.AdjustPos(ref _value);
				int num9 = BufferHelper.ReadInt32(reader2);
				NetInterface.Instantiate(PrefabManager.Self.CreationNetwork, _value, _value2, num9, int2, -1, int4);
				break;
			}
			case ObjType.Colony:
			{
				int num = 3000000;
				if (VersionMgr.dropItemRecordVersion >= 2016011300)
				{
					num = BufferHelper.ReadInt32(reader2);
				}
				int num2 = BufferHelper.ReadInt32(reader2);
				NetInterface.Instantiate(PrefabManager.Self.ColonyNetwork, _value, _value2, num2, int2, int3, num, int4);
				if (VersionMgr.dropItemRecordVersion < 2016011300)
				{
					list.Add(new ColonyNetworkRenewData(int2, int3, int4, _value, _value2, num, num2));
				}
				break;
			}
			case ObjType.Item:
			case ObjType.AiBeacon:
			case ObjType.PlantSeed:
				break;
			}
		}
		if (list.Count > 0)
		{
			foreach (ColonyNetworkRenewData item in list)
			{
				ColonyNetwork.RenewSave(item);
			}
		}
		if (VersionMgr.dropItemRecordVersion < VersionMgr.currentDropItemVersion)
		{
			VersionMgr.SaveDropItemVersion();
		}
	}

	public static void LoadNetworkObj()
	{
		PEDbOp pEDbOp = AsyncSqlite.NewReadOp();
		if (pEDbOp != null)
		{
			pEDbOp.SetCmdText("SELECT * FROM networkobj;");
			pEDbOp.BindReaderHandler(LoadComplete);
			pEDbOp.Exec();
			pEDbOp = null;
		}
	}

	public static void DeleteNetworkObj(int id)
	{
		NetObjectData netObjectData = new NetObjectData();
		netObjectData.DeleteData(id);
		AsyncSqlite.AddRecord(netObjectData);
	}
}

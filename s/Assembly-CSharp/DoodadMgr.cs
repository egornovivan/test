using System;
using System.Collections.Generic;
using System.IO;
using ItemAsset;
using Mono.Data.SqliteClient;
using PETools;
using UnityEngine;

public class DoodadMgr
{
	private static Dictionary<int, IDoodad> _doodadMgr = new Dictionary<int, IDoodad>();

	public static void Add(IDoodad doodad)
	{
		if (!_doodadMgr.ContainsKey(doodad._assetId))
		{
			_doodadMgr[doodad._entityId] = doodad;
		}
	}

	public static void Remove(IDoodad doodad)
	{
		_doodadMgr.Remove(doodad._entityId);
	}

	public static List<int> DescToItems(string desc, List<ItemObject> effItems)
	{
		if (effItems == null)
		{
			return null;
		}
		if (desc != "0")
		{
			List<int> list = new List<int>();
			string[] array = desc.Split(';');
			for (int i = 0; i < array.Length; i++)
			{
				string[] array2 = array[i].Split(',');
				if (array2.Length == 2)
				{
					int itemId = Convert.ToInt32(array2[0]);
					int num = Convert.ToInt32(array2[1]);
					ItemObject itemObject = ItemManager.CreateItem(itemId, num);
					if (itemObject != null)
					{
						effItems.Add(itemObject);
						list.Add(itemObject.instanceId);
					}
				}
			}
			return list;
		}
		return null;
	}

	public static IDoodad GetDoodad(int id)
	{
		if (_doodadMgr.ContainsKey(id))
		{
			return _doodadMgr[id];
		}
		return null;
	}

	public static void CreateCustomDoodad(int worldId, int entityId, int protoTypeId, int ownerId, Vector3 pos, Vector3 scale, Quaternion rot)
	{
		if (!MapObjNetwork.HadCreate(entityId, 0))
		{
			CreateUnlimitedDoodad(-1, -1, worldId, -1, entityId, protoTypeId, 0, string.Empty, pos, scale, rot);
		}
	}

	public static void CreateDoodad(int playerId, int teamId, int worldId, int assetId, int entityId, int protoTypeId, Vector3 pos, int type, string param, Vector3 scl)
	{
		if (!MapObjNetwork.HadCreate(assetId, type))
		{
			CreateUnlimitedDoodad(playerId, teamId, worldId, assetId, entityId, protoTypeId, type, param, pos, scl, Quaternion.identity);
		}
	}

	public static void CreateDoodadWithoutLimit(int playerId, int teamId, int worldId, int assetId, int entityId, int protoTypeId, Vector3 pos, int type, string param, Vector3 scl)
	{
		CreateUnlimitedDoodad(playerId, teamId, worldId, assetId, entityId, protoTypeId, type, param, pos, scl, Quaternion.identity);
	}

	public static void CreateUnlimitedDoodad(int playerId, int teamId, int worldId, int assetId, int entityId, int protoTypeId, int type, string param, Vector3 pos, Vector3 scl, Quaternion rot)
	{
		NetInterface.Instantiate(PrefabManager.Self.MapObjNetwork, pos, rot, worldId, type, playerId, teamId, assetId, entityId, protoTypeId, scl, param);
	}

	public static void DestroyDoodad(Vector3 pos)
	{
		foreach (KeyValuePair<int, IDoodad> item in _doodadMgr)
		{
			if (item.Value._pos == pos)
			{
				item.Value._net.DestroyMapObj();
				break;
			}
		}
	}

	public static void LoadData()
	{
		PEDbOp pEDbOp = AsyncSqlite.NewReadOp();
		if (pEDbOp != null)
		{
			pEDbOp.SetCmdText("SELECT * FROM doodaddata;");
			pEDbOp.BindReaderHandler(LoadComplete);
			pEDbOp.Exec();
			pEDbOp = null;
		}
	}

	public static void LoadComplete(SqliteDataReader reader)
	{
		while (reader.Read())
		{
			int entityId = reader.GetInt32(reader.GetOrdinal("id"));
			int type = reader.GetInt32(reader.GetOrdinal("objtype"));
			byte[] buff = (byte[])reader.GetValue(reader.GetOrdinal("data"));
			Serialize.Import(buff, delegate(BinaryReader r)
			{
				int num = r.ReadInt32();
				for (int i = 0; i < num; i++)
				{
					r.ReadInt32();
				}
				int assetId = r.ReadInt32();
				int protoTypeId = r.ReadInt32();
				int worldId = r.ReadInt32();
				int teamId = r.ReadInt32();
				Vector3 pos = new Vector3(r.ReadSingle(), r.ReadSingle(), r.ReadSingle());
				Vector3 vector = new Vector3(r.ReadSingle(), r.ReadSingle(), r.ReadSingle());
				float y = r.ReadSingle();
				string param = r.ReadString();
				if (ServerConfig.IsCustom)
				{
					Quaternion rot = Quaternion.Euler(0f, y, 0f);
					int ownerId = r.ReadInt32();
					CreateCustomDoodad(worldId, entityId, protoTypeId, ownerId, pos, vector, rot);
				}
				else
				{
					CreateDoodad(-1, teamId, worldId, assetId, entityId, protoTypeId, pos, type, param, vector);
				}
			});
		}
	}

	public static void SaveAll()
	{
		if (_doodadMgr.Count == 0)
		{
			return;
		}
		bool flag = false;
		foreach (KeyValuePair<int, IDoodad> item in _doodadMgr)
		{
			if (!item.Value._hasRecord && item.Value.ObjType != 2 && item.Value.ObjType != 1)
			{
				flag = true;
				break;
			}
		}
		if (!flag)
		{
			return;
		}
		List<IDoodad> list = new List<IDoodad>();
		foreach (KeyValuePair<int, IDoodad> item2 in _doodadMgr)
		{
			if (!item2.Value._hasRecord)
			{
				list.Add(item2.Value);
				item2.Value._hasRecord = true;
			}
		}
		DoodadMgrData doodadMgrData = new DoodadMgrData();
		doodadMgrData.ExportData(list);
		AsyncSqlite.AddRecord(doodadMgrData);
	}

	public static void SyncDoodaItems(Player player)
	{
		List<ItemObject> list = new List<ItemObject>();
		foreach (KeyValuePair<int, IDoodad> item in _doodadMgr)
		{
			item.Value.SyncItemObjs(player.OwnerView.owner);
		}
	}
}

using System.Collections.Generic;
using System.IO;
using ItemAsset;
using Mono.Data.SqliteClient;
using PETools;
using UnityEngine;

public static class SceneObjMgr
{
	private static List<int> _sceneItems = new List<int>();

	private static bool _sceneItemChangeFlag = false;

	public static T Create<T>() where T : SceneObject, new()
	{
		T val = new T();
		return (T)val;
	}

	public static IEnumerable<ItemObject> GetItems()
	{
		foreach (int id in _sceneItems)
		{
			ItemObject item = ItemManager.GetItemByID(id);
			if (item != null)
			{
				yield return item;
			}
		}
	}

	public static void AddItem(int id)
	{
		AddItem(id, sync: true);
	}

	public static void AddItem(int id, bool sync)
	{
		if (id != -1)
		{
			ItemObject itemByID = ItemManager.GetItemByID(id);
			if (itemByID != null)
			{
				AddItem(itemByID, sync);
			}
		}
	}

	public static void AddItem(ItemObject item)
	{
		AddItem(item, sync: true);
	}

	public static void AddItem(ItemObject item, bool sync)
	{
		if (item != null && !_sceneItems.Contains(item.instanceId))
		{
			_sceneItemChangeFlag = true;
			_sceneItems.Add(item.instanceId);
			if (sync)
			{
				ChannelNetwork.SyncItem(101, item);
			}
		}
	}

	public static void RemoveItem(int id)
	{
		if (_sceneItems.Remove(id))
		{
			_sceneItemChangeFlag = true;
		}
	}

	public static void Delete(int id)
	{
		SceneObjectDbData sceneObjectDbData = new SceneObjectDbData();
		sceneObjectDbData.DeleteData(id);
		AsyncSqlite.AddRecord(sceneObjectDbData);
	}

	public static void SaveSceneIds()
	{
		if (!_sceneItemChangeFlag)
		{
			return;
		}
		byte[] data = Serialize.Export(delegate(BinaryWriter w)
		{
			BufferHelper.Serialize(w, _sceneItems.Count);
			foreach (int sceneItem in _sceneItems)
			{
				BufferHelper.Serialize(w, sceneItem);
			}
		});
		SceneIdDbData sceneIdDbData = new SceneIdDbData();
		sceneIdDbData.ExportData(ServerConfig.ServerName, data);
		AsyncSqlite.AddRecord(sceneIdDbData);
		_sceneItemChangeFlag = false;
	}

	public static void LoadSceneIds()
	{
		PEDbOp pEDbOp = AsyncSqlite.NewReadOp();
		if (pEDbOp != null)
		{
			pEDbOp.SetCmdText("SELECT ver,servername,data FROM sceneid;");
			pEDbOp.BindReaderHandler(LoadSceneIdComplete);
			pEDbOp.Exec();
			pEDbOp = null;
		}
	}

	private static void LoadSceneIdComplete(SqliteDataReader reader)
	{
		if (!reader.Read())
		{
			return;
		}
		int @int = reader.GetInt32(reader.GetOrdinal("ver"));
		byte[] buff = (byte[])reader.GetValue(reader.GetOrdinal("data"));
		Serialize.Import(buff, delegate(BinaryReader r)
		{
			int num = BufferHelper.ReadInt32(r);
			for (int i = 0; i < num; i++)
			{
				int item = BufferHelper.ReadInt32(r);
				if (!_sceneItems.Contains(item))
				{
					_sceneItems.Add(item);
				}
			}
		});
	}

	public static void Save(SceneObject obj)
	{
		SceneObjectDbData sceneObjectDbData = new SceneObjectDbData();
		sceneObjectDbData.ExportData(obj);
		AsyncSqlite.AddRecord(sceneObjectDbData);
	}

	private static void LoadComplete(SqliteDataReader reader)
	{
		while (reader.Read())
		{
			int @int = reader.GetInt32(reader.GetOrdinal("ver"));
			int id = reader.GetInt32(reader.GetOrdinal("id"));
			ESceneObjType type = (ESceneObjType)reader.GetInt32(reader.GetOrdinal("type"));
			byte[] buff = (byte[])reader.GetValue(reader.GetOrdinal("data"));
			SceneObject obj = null;
			Serialize.Import(buff, delegate(BinaryReader r)
			{
				BufferHelper.ReadVector3(r, out var _value);
				BufferHelper.ReadVector3(r, out var _value2);
				BufferHelper.ReadQuaternion(r, out var _value3);
				int worldId = BufferHelper.ReadInt32(r);
				int protoId = BufferHelper.ReadInt32(r);
				switch (type)
				{
				case ESceneObjType.ITEM:
				case ESceneObjType.DOODAD:
				{
					SceneItem sceneItem = Create<SceneItem>();
					sceneItem.Init(id, protoId, _value, _value2, _value3, worldId);
					ItemObject itemByID2 = ItemManager.GetItemByID(id);
					if (itemByID2 == null)
					{
						if (LogFilter.logDebug)
						{
							Debug.LogErrorFormat("Load SceneItem error with id:{0}", id);
						}
					}
					else
					{
						sceneItem.SetItem(itemByID2);
						sceneItem.SetType(type);
						GameWorld.AddSceneObj(sceneItem, worldId);
						AddItem(itemByID2, sync: false);
						obj = sceneItem;
					}
					break;
				}
				case ESceneObjType.EFFECT:
				{
					SceneObject sceneObject = Create<SceneObject>();
					sceneObject.Init(id, protoId, _value, _value2, _value3, worldId);
					sceneObject.SetType(ESceneObjType.EFFECT);
					GameWorld.AddSceneObj(sceneObject, worldId);
					obj = sceneObject;
					break;
				}
				case ESceneObjType.DROPITEM:
				{
					SceneDropItem sceneDropItem = Create<SceneDropItem>();
					sceneDropItem.Init(id, protoId, _value, _value2, _value3, worldId);
					ItemObject itemByID = ItemManager.GetItemByID(id);
					if (itemByID == null)
					{
						if (LogFilter.logDebug)
						{
							Debug.LogErrorFormat("Load SceneItem error with id:{0}", id);
						}
					}
					else
					{
						sceneDropItem.SetItem(itemByID);
						sceneDropItem.SetType(type);
						GameWorld.AddSceneObj(sceneDropItem, worldId);
						AddItem(itemByID, sync: false);
						obj = sceneDropItem;
					}
					break;
				}
				}
			});
		}
	}

	public static void Load()
	{
		PEDbOp pEDbOp = AsyncSqlite.NewReadOp();
		if (pEDbOp != null)
		{
			pEDbOp.SetCmdText("SELECT ver,id,type,data FROM sceneobj;");
			pEDbOp.BindReaderHandler(LoadComplete);
			pEDbOp.Exec();
			pEDbOp = null;
		}
	}
}

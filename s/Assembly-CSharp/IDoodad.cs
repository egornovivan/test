using System.Collections.Generic;
using System.IO;
using System.Linq;
using ItemAsset;
using Mono.Data.SqliteClient;
using PETools;
using uLink;
using UnityEngine;

public class IDoodad
{
	protected int _objType;

	public int _assetId;

	public int _entityId;

	public int _protoTypeId;

	public int _worldId;

	public int _teamId;

	public int _defaultPlayerId;

	public Vector3 _scale;

	public Vector3 _pos;

	public float _rotY;

	internal bool _hasRecord;

	internal MapObjNetwork _net;

	public string _param = string.Empty;

	protected List<int> _itemList = new List<int>();

	internal int ObjType => _objType;

	public virtual void Create(MapObjNetwork net, uLink.NetworkMessageInfo info)
	{
		_assetId = info.networkView.initialData.Read<int>(new object[0]);
		_entityId = info.networkView.initialData.Read<int>(new object[0]);
		_protoTypeId = info.networkView.initialData.Read<int>(new object[0]);
		_scale = info.networkView.initialData.Read<Vector3>(new object[0]);
		_worldId = net.WorldId;
		_teamId = net.TeamId;
		_pos = net.transform.position;
		_rotY = net.transform.rotation.y;
		_objType = net.ObjType;
		_defaultPlayerId = net.DefaultPlayerId;
		_net = net;
		_net._doodad = this;
		DoodadMgr.Add(this);
	}

	public virtual void InitAttr()
	{
	}

	public virtual void Insert(uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
	}

	public void SyncItemObjs(uLink.NetworkPlayer peer)
	{
		List<ItemObject> list = new List<ItemObject>();
		if (_itemList == null || _itemList.Count == 0)
		{
			return;
		}
		foreach (int item in _itemList)
		{
			ItemObject itemByID = ItemManager.GetItemByID(item);
			if (itemByID != null)
			{
				list.Add(itemByID);
			}
		}
		if (list != null && list.Count > 0)
		{
			ChannelNetwork.SyncItemList(peer, list.ToArray());
		}
	}

	public void RequestItemList(uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
		if (_itemList != null)
		{
			SyncItemObjs(info.sender);
			_net.RPCPeer(info.sender, EPacketType.PT_MO_RequestItemList, _itemList.ToArray());
		}
	}

	public virtual void AddToItemlist(int objID)
	{
		if (objID > 0 && _itemList != null && !_itemList.Contains(objID))
		{
			_itemList.Add(objID);
		}
	}

	public virtual void AddToItemlist(int objID, int index)
	{
	}

	public void GetAllItems(uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
		if (_itemList == null)
		{
			return;
		}
		if (_itemList.Count == 0)
		{
			TryToClearItemlist();
			_net.RPCOthers(EPacketType.PT_MO_RequestItemList, _itemList.ToArray());
			return;
		}
		Player player = Player.GetPlayer(info.sender);
		if (null != player)
		{
			int num = _itemList.Count();
			for (int num2 = num - 1; num2 >= 0; num2--)
			{
				if (_itemList[num2] != 0)
				{
					ItemObject itemByID = ItemManager.GetItemByID(_itemList[num2]);
					if (itemByID != null)
					{
						if (player.Package.GetEmptyGridCount(itemByID.protoData) == 0)
						{
							player.SyncErrorMsg("Package is full.");
							_net.RPCOthers(EPacketType.PT_MO_ModifyItemList, _itemList.ToArray());
							return;
						}
						player.Package.AddItem(itemByID);
						if (this is DoodadBox)
						{
							_itemList[num2] = -1;
						}
						else
						{
							_itemList.RemoveAt(num2);
						}
						if (itemByID.protoId == 1339 && _itemList.Count == 0 && this is DoodadItem)
						{
							_net.DestroyMapObj();
							DeleteData();
						}
					}
					else if (this is DoodadBox)
					{
						_itemList[num2] = -1;
					}
					else
					{
						_itemList.RemoveAt(num2);
					}
				}
			}
			ItemObject[] items = player.Package.Sort(0);
			player.SyncItemList(items);
			items = player.Package.Sort(1);
			player.SyncItemList(items);
			items = player.Package.Sort(2);
			player.SyncItemList(items);
			player.SyncPackageIndex();
		}
		TryToClearItemlist();
		_net.RPCOthers(EPacketType.PT_MO_ModifyItemList, _itemList.ToArray());
	}

	public virtual void GetItem(uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
		if (_itemList == null)
		{
			return;
		}
		if (_itemList.Count() == 0)
		{
			TryToClearItemlist();
			return;
		}
		int itemID = stream.Read<int>(new object[0]);
		int num = _itemList.FindIndex((int iter) => iter == itemID);
		if (num < 0)
		{
			return;
		}
		Player player = Player.GetPlayer(info.sender);
		if (!(null != player) || itemID == 0)
		{
			return;
		}
		ItemObject itemByID = ItemManager.GetItemByID(itemID);
		if (itemByID == null)
		{
			return;
		}
		if (player.Package.GetEmptyGridCount(itemByID.protoData) == 0)
		{
			player.SyncErrorMsg("Package is full.");
			return;
		}
		player.Package.AddItem(itemByID);
		if (this is DoodadBox)
		{
			_itemList[num] = -1;
		}
		else
		{
			_itemList.RemoveAt(num);
		}
		player.SyncItem(itemByID);
		player.SyncPackageIndex();
		if (itemByID.protoId == 1339 && _itemList.Count == 0 && this is DoodadItem)
		{
			_net.DestroyMapObj();
			DeleteData();
		}
		SaveData();
		_net.RPCOthers(EPacketType.PT_MO_RemoveItem, itemID);
		TryToClearItemlist();
	}

	public void TryToClearItemlist()
	{
		if ((this is DoodadDeadDrop || this is DoodadDropBox) && _itemList != null && _itemList.Count() == 0)
		{
			_itemList.Clear();
			_net.DestroyMapObj();
			DeleteData();
		}
	}

	public void DeleteData()
	{
		DoodadData doodadData = new DoodadData();
		doodadData.DeleteData(_entityId);
		AsyncSqlite.AddRecord(doodadData);
	}

	public virtual void DropItem(NetInterface caster)
	{
	}

	public virtual void DeathDestroyNet()
	{
	}

	public void CreateDropItems(List<ItemSample> items, ref List<ItemObject> effItems)
	{
		ItemManager.CreateItems(items, ref effItems);
		if (effItems.Count > 0)
		{
			ChannelNetwork.SyncItemList(_worldId, effItems);
		}
	}

	public void CreateDropScenes(List<ItemSample> items)
	{
		List<ItemObject> effItems = new List<ItemObject>();
		CreateDropItems(items, ref effItems);
		Player randomPlayer = Player.GetRandomPlayer();
		if (randomPlayer != null)
		{
			SceneDropItem.CreateDropItems(randomPlayer.WorldId, effItems, _pos, Vector3.zero, Quaternion.Euler(0f, _rotY, 0f));
		}
	}

	public void OnDestroy()
	{
		_itemList.Clear();
		DoodadMgr.Remove(this);
	}

	public virtual byte[] Export()
	{
		return Serialize.Export(delegate(BinaryWriter w)
		{
			w.Write(_itemList.Count);
			foreach (int item in _itemList)
			{
				w.Write(item);
			}
			w.Write(_assetId);
			w.Write(_protoTypeId);
			w.Write(_worldId);
			w.Write(_teamId);
			w.Write(_pos.x);
			w.Write(_pos.y);
			w.Write(_pos.z);
			w.Write(_scale.x);
			w.Write(_scale.y);
			w.Write(_scale.z);
			w.Write(_rotY);
			w.Write(_param);
			w.Write(_defaultPlayerId);
		});
	}

	public virtual void Import(byte[] data)
	{
		Serialize.Import(data, delegate(BinaryReader r)
		{
			_itemList.Clear();
			int num = r.ReadInt32();
			for (int i = 0; i < num; i++)
			{
				_itemList.Add(r.ReadInt32());
			}
			_assetId = r.ReadInt32();
			_protoTypeId = r.ReadInt32();
			_worldId = r.ReadInt32();
			_teamId = r.ReadInt32();
			_pos = new Vector3(r.ReadSingle(), r.ReadSingle(), r.ReadSingle());
			_scale = new Vector3(r.ReadSingle(), r.ReadSingle(), r.ReadSingle());
			_rotY = r.ReadSingle();
			_param = r.ReadString();
			_defaultPlayerId = r.ReadInt32();
		});
	}

	public void SaveData()
	{
		DoodadData doodadData = new DoodadData();
		doodadData.ExportData(this);
		AsyncSqlite.AddRecord(doodadData);
	}

	public void LoadData()
	{
		PEDbOp pEDbOp = AsyncSqlite.NewReadOp();
		if (pEDbOp != null)
		{
			pEDbOp.SetCmdText("SELECT * FROM doodaddata WHERE id = @id;");
			pEDbOp.BindParam("@id", _entityId);
			pEDbOp.BindReaderHandler(LoadComplete);
			pEDbOp.Exec();
			pEDbOp = null;
		}
	}

	public void LoadComplete(SqliteDataReader dataReader)
	{
		if (dataReader.Read())
		{
			int @int = dataReader.GetInt32(dataReader.GetOrdinal("ver"));
			_objType = dataReader.GetInt32(dataReader.GetOrdinal("objtype"));
			byte[] array = (byte[])dataReader.GetValue(dataReader.GetOrdinal("data"));
			if (array.Length > 0)
			{
				Import(array);
				_hasRecord = true;
			}
		}
	}
}

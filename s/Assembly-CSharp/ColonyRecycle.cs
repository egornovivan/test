using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using ItemAsset;
using UnityEngine;

public class ColonyRecycle : ColonyBase
{
	public const int MAX_WORKER_COUNT = 4;

	private CSRecycleData _MyData;

	private ItemObject _Item;

	public string RoleName = string.Empty;

	public override int MaxWorkerCount => 4;

	public ColonyRecycle(ColonyNetwork network)
	{
		SetNetwork(network);
		_RecordData = new CSRecycleData();
		_MyData = (CSRecycleData)_RecordData;
		LoadData();
		SetWorkerCountChangeEventHandler(OnWorkerCountChange);
	}

	public void OnWorkerCountChange()
	{
		if (_MyData.m_CurTime != -1f)
		{
			float num = _MyData.m_CurTime / _MyData.m_Time;
			_MyData.m_Time = FixFinalTime(CSRecycleInfo.m_BaseTime);
			_MyData.m_CurTime = _MyData.m_Time * num;
		}
	}

	public float GetWorkerParam()
	{
		float num = 1f;
		foreach (ColonyNpc value in _worker.Values)
		{
			if (value != null)
			{
				num *= 1f - value.GetRecycleSkill;
			}
		}
		return num;
	}

	private float FixFinalTime(float origin)
	{
		int getWorkingCount = base.GetWorkingCount;
		return origin * Mathf.Pow(0.82f, getWorkingCount) * GetWorkerParam();
	}

	public override void MyUpdate()
	{
		if (IsRecycling() && _MyData.m_CurTime != -1f)
		{
			_MyData.m_CurTime += 1f;
			if (_MyData.m_CurTime >= _MyData.m_Time)
			{
				OnRecycleEnd();
			}
		}
	}

	public override void CombomData(BinaryWriter writer)
	{
		BufferHelper.Serialize(writer, _MyData.m_ItemID);
		BufferHelper.Serialize(writer, _MyData.m_CurTime);
		BufferHelper.Serialize(writer, _MyData.m_Time);
		BufferHelper.Serialize(writer, _MyData.m_RecycleItems.Count);
		BufferHelper.Serialize(writer, RoleName);
		foreach (KeyValuePair<int, int> recycleItem in _MyData.m_RecycleItems)
		{
			BufferHelper.Serialize(writer, recycleItem.Key);
			BufferHelper.Serialize(writer, recycleItem.Value);
		}
	}

	public override void ParseData(byte[] data, int ver)
	{
		using MemoryStream input = new MemoryStream(data);
		using BinaryReader reader = new BinaryReader(input);
		_MyData.m_ItemID = BufferHelper.ReadInt32(reader);
		_MyData.m_CurTime = BufferHelper.ReadSingle(reader);
		_MyData.m_Time = BufferHelper.ReadSingle(reader);
		int num = BufferHelper.ReadInt32(reader);
		RoleName = BufferHelper.ReadString(reader);
		for (int i = 0; i < num; i++)
		{
			int key = BufferHelper.ReadInt32(reader);
			int value = BufferHelper.ReadInt32(reader);
			_MyData.m_RecycleItems[key] = value;
		}
		_Item = ItemManager.GetItemByID(_MyData.m_ItemID);
	}

	public override void InitMyData()
	{
		_MyData.m_ItemID = 0;
		_MyData.m_CurTime = -1f;
		_MyData.m_Time = -1f;
	}

	public bool FetchMaterial(int itemId, Player player)
	{
		if (!_MyData.m_RecycleItems.ContainsKey(itemId))
		{
			return false;
		}
		if (player == null)
		{
			return false;
		}
		ItemObject itemObject = player.CreateItem(itemId, _MyData.m_RecycleItems[itemId], syn: true);
		if (itemObject == null)
		{
			return false;
		}
		if (player.Package.GetEmptyGridCount(_Item.protoData) <= 0)
		{
			return false;
		}
		if (_MyData.m_RecycleItems[itemId] - itemObject.stackCount > 0)
		{
			_MyData.m_RecycleItems[itemId] = _MyData.m_RecycleItems[itemId] - itemObject.stackCount;
			SyncRecycleItem(itemId, _MyData.m_RecycleItems[itemId]);
		}
		else
		{
			_MyData.m_RecycleItems.Remove(itemId);
			SyncRecycleItem(itemId, 0);
		}
		player.Package.AddItem(itemObject);
		player.SyncPackageIndex();
		SyncSave();
		return true;
	}

	public bool FetchItem(Player player)
	{
		if (_Item == null)
		{
			return false;
		}
		if (IsRecycling())
		{
			return false;
		}
		if (player == null)
		{
			return false;
		}
		if (player.Package.GetEmptyGridCount(_Item.protoData) <= 0)
		{
			return false;
		}
		player.Package.AddItem(_Item);
		player.SyncPackageIndex();
		_Item = null;
		_MyData.m_ItemID = 0;
		SyncSave();
		return true;
	}

	public void SyncRecycleItem(int itemId, int amount)
	{
		_Network.RPCOthers(EPacketType.PT_CL_RCY_SyncRecycleItem, itemId, amount);
	}

	public bool SetItem(ItemObject item, Player player)
	{
		if (item == null)
		{
			return false;
		}
		if (IsRecycling())
		{
			return false;
		}
		if (_Item != null)
		{
			if (player.Package.GetEmptyGridCount(_Item.protoData) <= 0)
			{
				return false;
			}
			player.Package.AddItem(_Item);
			player.SyncPackageIndex();
			_Item = null;
			_MyData.m_ItemID = 0;
		}
		if (!player.RemoveEquipment(item))
		{
			if (player.Package.GetItemById(item.instanceId) == null)
			{
				return false;
			}
			player.Package.RemoveItem(item);
			player.SyncPackageIndex();
		}
		_Item = item;
		_MyData.m_ItemID = item.instanceId;
		RoleName = player.roleName;
		SyncSave();
		return true;
	}

	public bool StartRecycle()
	{
		if (_Item == null)
		{
			return false;
		}
		_MyData.m_Time = FixFinalTime(CSRecycleInfo.m_BaseTime);
		_MyData.m_CurTime = 0f;
		SyncSave();
		return true;
	}

	public bool StopRecycle()
	{
		if (_Item == null)
		{
			return false;
		}
		if (_MyData.m_CurTime == -1f)
		{
			return false;
		}
		_MyData.m_Time = -1f;
		_MyData.m_CurTime = -1f;
		SyncSave();
		return true;
	}

	public bool IsRecycling()
	{
		if (ColonyMgr._Instance.HasCoreAndPower(_Network.TeamId, _Network.transform.position))
		{
			return _MyData.m_CurTime != -1f;
		}
		return false;
	}

	public override bool IsWorking()
	{
		if (ColonyMgr._Instance.HasCoreAndPower(_Network.TeamId, _Network.transform.position))
		{
			return true;
		}
		return false;
	}

	public void OnRecycleEnd()
	{
		Player player = Player.GetPlayer(RoleName);
		EPacketType ePacketType = EPacketType.PT_NULL;
		Quaternion quaternion = Quaternion.identity;
		RandomItemObj randomItemObj = null;
		if (player != null)
		{
			Dictionary<int, int> recycleItems = GetRecycleItems();
			if (recycleItems != null && recycleItems.Count >= 1)
			{
				IEnumerable<ItemSample> items = recycleItems.Select((KeyValuePair<int, int> iter) => new ItemSample(iter.Key, iter.Value));
				if (player.Package.CanAdd(items))
				{
					ItemObject[] array = player.Package.AddSameItems(items);
					if (array != null)
					{
						ChannelNetwork.SyncItemList(_Network.WorldId, array);
					}
					player.SyncPackageIndex();
					ePacketType = EPacketType.PT_CL_RCY_End;
				}
				if (ePacketType == EPacketType.PT_NULL)
				{
					List<ItemIdCount> list = recycleItems.Select((KeyValuePair<int, int> iter) => new ItemIdCount(iter.Key, iter.Value)).ToList();
					if (CSUtils.CanAddListToStorage(list, base.TeamId))
					{
						CSUtils.AddItemListToStorage(list, base.TeamId);
						ePacketType = EPacketType.PT_CL_RCY_MatsToStorage;
					}
					if (ePacketType == EPacketType.PT_NULL)
					{
						int[] itemIdNum = CSUtils.ItemIdCountListToIntArray(list);
						System.Random random = new System.Random();
						Vector3 vector = _Network.transform.position + new Vector3(1f, 0.66f, 1f);
						quaternion = Quaternion.Euler(0f, random.Next(360), 0f);
						randomItemObj = new RandomItemObj(vector + new Vector3((float)random.NextDouble() * 1.2f, (float)random.NextDouble() * 0.1f, (float)random.NextDouble() * 1.2f), quaternion, itemIdNum);
						RandomItemMgr.Instance.AddItemForProcessing(randomItemObj);
						ePacketType = EPacketType.PT_CL_RCY_MatsToResult;
					}
				}
			}
		}
		if (ePacketType != 0)
		{
			ItemManager.RemoveItem(_Item.instanceId);
			_Item = null;
			_MyData.m_Time = -1f;
			_MyData.m_CurTime = -1f;
			_MyData.m_ItemID = 0;
			RoleName = string.Empty;
			SyncSave();
			switch (ePacketType)
			{
			case EPacketType.PT_CL_RCY_End:
			case EPacketType.PT_CL_RCY_MatsToStorage:
				_Network.RPCOthers(ePacketType);
				break;
			case EPacketType.PT_CL_RCY_MatsToResult:
				_Network.RPCOthers(EPacketType.PT_CL_RCY_MatsToResult, randomItemObj.position, quaternion, randomItemObj.items);
				break;
			}
		}
	}

	public Dictionary<int, int> GetRecycleItems()
	{
		Recycle cmpt = _Item.GetCmpt<Recycle>();
		if (cmpt == null)
		{
			return null;
		}
		Dictionary<int, int> dictionary = new Dictionary<int, int>();
		MaterialItem[] recycleItems = cmpt.GetRecycleItems();
		foreach (MaterialItem materialItem in recycleItems)
		{
			dictionary[materialItem.protoId] = materialItem.count;
		}
		return dictionary;
	}
}

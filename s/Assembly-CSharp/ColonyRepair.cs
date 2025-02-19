using System.Collections.Generic;
using System.IO;
using System.Linq;
using ItemAsset;
using UnityEngine;

public class ColonyRepair : ColonyBase
{
	public const int MAX_WORKER_COUNT = 4;

	private CSRepairData _MyData;

	private ItemObject _Item;

	public override int MaxWorkerCount => 4;

	public ColonyRepair(ColonyNetwork network)
	{
		SetNetwork(network);
		_RecordData = new CSRepairData();
		_MyData = (CSRepairData)_RecordData;
		LoadData();
		SetWorkerCountChangeEventHandler(OnWorkerCountChange);
	}

	public override void MyUpdate()
	{
		if (IsRepairing() && (_MyData.m_CurTime < _MyData.m_Time || _MyData.m_CurTime != -1f))
		{
			_MyData.m_CurTime += 1f;
			if (_MyData.m_CurTime >= _MyData.m_Time)
			{
				OnRepairEnd();
			}
		}
	}

	public void OnWorkerCountChange()
	{
		if (_MyData.m_CurTime < _MyData.m_Time || _MyData.m_CurTime != -1f)
		{
			float num = _MyData.m_CurTime / _MyData.m_Time;
			_MyData.m_Time = FixFinalTime(CSRepairInfo.m_BaseTime);
			_MyData.m_CurTime = _MyData.m_Time * num;
		}
	}

	public override void CombomData(BinaryWriter writer)
	{
		BufferHelper.Serialize(writer, _MyData.m_ItemID);
		BufferHelper.Serialize(writer, _MyData.m_CurTime);
		BufferHelper.Serialize(writer, _MyData.m_Time);
	}

	public override void ParseData(byte[] data, int ver)
	{
		using MemoryStream input = new MemoryStream(data);
		using BinaryReader reader = new BinaryReader(input);
		_MyData.m_ItemID = BufferHelper.ReadInt32(reader);
		_MyData.m_CurTime = BufferHelper.ReadSingle(reader);
		_MyData.m_Time = BufferHelper.ReadSingle(reader);
		_Item = ItemManager.GetItemByID(_MyData.m_ItemID);
	}

	public override void InitMyData()
	{
		_MyData.m_ItemID = 0;
		_MyData.m_CurTime = -1f;
		_MyData.m_Time = -1f;
	}

	public bool SetItem(int objId, Player player)
	{
		ItemObject itemByID = ItemManager.GetItemByID(objId);
		if (itemByID == null)
		{
			return false;
		}
		if (IsRepairing())
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
		if (!player.RemoveEquipment(itemByID))
		{
			if (player.Package.GetItemById(objId) == null)
			{
				return false;
			}
			player.Package.RemoveItem(itemByID);
			player.SyncPackageIndex();
		}
		_Item = itemByID;
		_MyData.m_ItemID = objId;
		SyncSave();
		return true;
	}

	public bool FetchItem(Player player)
	{
		if (!ColonyMgr._Instance.HasCoreAndPower(_Network.TeamId, _Network.transform.position))
		{
			return false;
		}
		if (_Item == null || player == null)
		{
			return false;
		}
		if (IsRepairing())
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

	public bool Start(Player player)
	{
		if (_Item == null || player == null)
		{
			return false;
		}
		if (!ColonyMgr._Instance.HasCoreAndPower(_Network.TeamId, _Network.transform.position))
		{
			return false;
		}
		Dictionary<int, int> costsItems = GetCostsItems();
		foreach (KeyValuePair<int, int> item in costsItems)
		{
			if (player.GetItemNum(item.Key) < item.Value)
			{
				return false;
			}
		}
		IEnumerable<ItemSample> items = costsItems.Select((KeyValuePair<int, int> iter) => new ItemSample(iter.Key, iter.Value));
		ItemObject[] items2 = player.Package.RemoveItem(items);
		ChannelNetwork.SyncItemList(_Network.WorldId, items2);
		player.SyncPackageIndex();
		_MyData.m_CurTime = 0f;
		_MyData.m_Time = FixFinalTime(CSRepairInfo.m_BaseTime);
		SyncSave();
		return true;
	}

	public float GetWorkerParam()
	{
		float num = 1f;
		foreach (ColonyNpc value in _worker.Values)
		{
			if (value != null)
			{
				num *= 1f - value.GetRepairSkill;
			}
		}
		return num;
	}

	private float FixFinalTime(float origin)
	{
		int getWorkingCount = base.GetWorkingCount;
		return origin * Mathf.Pow(0.82f, getWorkingCount) * GetWorkerParam();
	}

	public bool Stop()
	{
		if (_Item == null)
		{
			return false;
		}
		if (_MyData.m_CurTime == -1f)
		{
			return false;
		}
		_MyData.m_CurTime = -1f;
		_MyData.m_Time = -1f;
		SyncSave();
		return true;
	}

	public bool IsRepairing()
	{
		if ((_MyData.m_CurTime < _MyData.m_Time || _MyData.m_CurTime != -1f) && ColonyMgr._Instance.HasCoreAndPower(_Network.TeamId, _Network.transform.position))
		{
			return true;
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

	public Dictionary<int, int> GetCostsItems()
	{
		Dictionary<int, int> dictionary = new Dictionary<int, int>();
		Repair cmpt = _Item.GetCmpt<Repair>();
		if (cmpt == null || cmpt.GetValue().IsCurrentMax())
		{
			return dictionary;
		}
		foreach (MaterialItem requirement in cmpt.GetRequirements())
		{
			dictionary[requirement.protoId] = requirement.count;
		}
		return dictionary;
	}

	public void OnRepairEnd()
	{
		if (_Item != null)
		{
			if (_Item.instanceId >= 100000000)
			{
				CreationOriginData creationData = SteamWorks.GetCreationData(_Item.instanceId);
				creationData.HP = creationData.MaxHP;
				SyncCreationHP(_Item.instanceId, creationData.HP);
				_Item.GetCmpt<Repair>().Do();
				SyncItem(_Item);
			}
			else
			{
				_Item.GetCmpt<Repair>().Do();
				SyncItem(_Item);
			}
			_MyData.m_CurTime = -1f;
			_MyData.m_Time = -1f;
			SyncSave();
			_Network.RPCOthers(EPacketType.PT_CL_RPA_End);
		}
	}
}

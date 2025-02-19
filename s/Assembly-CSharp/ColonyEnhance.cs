using System.Collections.Generic;
using System.IO;
using System.Linq;
using ItemAsset;
using UnityEngine;

public class ColonyEnhance : ColonyBase
{
	public const int MAX_WORKER_COUNT = 4;

	private CSEnhanceData _MyData;

	private ItemObject _Item;

	public override int MaxWorkerCount => 4;

	public ColonyEnhance(ColonyNetwork network)
	{
		SetNetwork(network);
		_RecordData = new CSEnhanceData();
		_MyData = (CSEnhanceData)_RecordData;
		LoadData();
		SetWorkerCountChangeEventHandler(OnWorkerCountChange);
	}

	public void OnWorkerCountChange()
	{
		if (_MyData.m_CurTime < _MyData.m_Time || _MyData.m_CurTime != -1f)
		{
			float num = _MyData.m_CurTime / _MyData.m_Time;
			_MyData.m_Time = FixFinalTime(CSEnhanceInfo.m_BaseTime);
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
				num *= 1f - value.GetEnhanceSkill;
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
		if (IsEnhancing() && (_MyData.m_CurTime < _MyData.m_Time || _MyData.m_CurTime != -1f))
		{
			_MyData.m_CurTime += 1f;
			if (_MyData.m_CurTime >= _MyData.m_Time)
			{
				OnEnhanced();
				_MyData.m_CurTime = -1f;
				_MyData.m_Time = -1f;
			}
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
		if (IsEnhancing())
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
		if (_Item == null || player == null)
		{
			return false;
		}
		if (IsEnhancing())
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
		List<MaterialItem> costsItems = GetCostsItems();
		foreach (MaterialItem item in costsItems)
		{
			if (player.GetItemNum(item.protoId) < item.count)
			{
				return false;
			}
		}
		IEnumerable<ItemSample> items = costsItems.Select((MaterialItem iter) => new ItemSample(iter.protoId, iter.count));
		ItemObject[] items2 = player.Package.RemoveItem(items);
		_MyData.m_CurTime = 0f;
		_MyData.m_Time = FixFinalTime(CSEnhanceInfo.m_BaseTime);
		ChannelNetwork.SyncItemList(player.WorldId, items2);
		player.SyncPackageIndex();
		SyncSave();
		return true;
	}

	public bool Stop()
	{
		if (_Item == null)
		{
			return false;
		}
		if (!IsEnhancing())
		{
			return false;
		}
		_MyData.m_CurTime = -1f;
		_MyData.m_Time = -1f;
		return true;
	}

	public bool IsEnhancing()
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

	public List<MaterialItem> GetCostsItems()
	{
		if (_Item == null)
		{
			return null;
		}
		return _Item.GetCmpt<Strengthen>()?.GetMaterialItems().ToList();
	}

	private void OnEnhanced()
	{
		if (_Item == null)
		{
			if (LogFilter.logDebug)
			{
				Debug.LogWarning("The Enhance item is null, so cant be enhanced!");
			}
			return;
		}
		Strengthen cmpt = _Item.GetCmpt<Strengthen>();
		if (cmpt != null)
		{
			cmpt.LevelUp();
			_Network.RPCOthers(EPacketType.PT_CL_EHN_End);
			SyncItem(_Item);
			SyncSave();
		}
	}
}

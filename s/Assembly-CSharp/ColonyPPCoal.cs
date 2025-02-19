using System.Collections.Generic;
using System.IO;
using System.Linq;
using ItemAsset;
using UnityEngine;

public class ColonyPPCoal : ColonyPowerPlant
{
	private float autoPercent = 0.2f;

	private int autoCount = 15;

	protected CSPPCoalData _SubData;

	public virtual int FuelID => Info.m_WorkedTimeItemID[0];

	public virtual int FuelMaxCount => Info.m_WorkedTimeItemCnt[0];

	public virtual float AutoPercent => autoPercent;

	public virtual int AutoCount => autoCount;

	public float PowerRadius => Info.m_Radius;

	public ColonyPPCoal()
	{
	}

	public ColonyPPCoal(ColonyNetwork network)
	{
		SetNetwork(network);
		_RecordData = new CSPPCoalData();
		_MyData = (CSPPCoalData)_RecordData;
		_SubData = (CSPPCoalData)_MyData;
		LoadData();
	}

	public override void CombomData(BinaryWriter writer)
	{
		BufferHelper.Serialize(writer, _SubData.m_ChargingItems.Count);
		foreach (KeyValuePair<int, int> chargingItem in _SubData.m_ChargingItems)
		{
			BufferHelper.Serialize(writer, chargingItem.Key);
			BufferHelper.Serialize(writer, chargingItem.Value);
		}
		BufferHelper.Serialize(writer, _SubData.m_WorkedTime);
		BufferHelper.Serialize(writer, _SubData.m_CurWorkedTime);
	}

	public override void ParseData(byte[] data, int ver)
	{
		using MemoryStream input = new MemoryStream(data);
		using BinaryReader reader = new BinaryReader(input);
		int num = BufferHelper.ReadInt32(reader);
		for (int i = 0; i < num; i++)
		{
			int key = BufferHelper.ReadInt32(reader);
			int num2 = BufferHelper.ReadInt32(reader);
			_SubData.m_ChargingItems[key] = num2;
			if (ItemManager.GetItemByID(num2) != null)
			{
				_ChargeItems.Add(ItemManager.GetItemByID(num2));
			}
		}
		_SubData.m_WorkedTime = BufferHelper.ReadSingle(reader);
		_SubData.m_CurWorkedTime = BufferHelper.ReadSingle(reader);
	}

	public override bool IsWorking()
	{
		if (ColonyMgr._Instance.HaveCore(_Network.TeamId, _Network.transform.position) && _SubData.m_CurWorkedTime < _SubData.m_WorkedTime && _SubData.m_CurWorkedTime != -1f)
		{
			return true;
		}
		return false;
	}

	public override void MyUpdate()
	{
		if (!IsWorking())
		{
			return;
		}
		ChargingItem(Mathf.Clamp(GameTime.Timer.ElapseSpeed, 0f, 50f));
		_SubData.m_CurWorkedTime += 1f;
		if (_SubData.m_CurWorkedTime != _SubData.m_WorkedTime)
		{
			return;
		}
		bool flag = false;
		List<ColonyBase> colonyItemsByItemId = ColonyMgr._Instance.GetColonyItemsByItemId(_Network.TeamId, 1129);
		if (colonyItemsByItemId != null)
		{
			foreach (ColonyBase item in colonyItemsByItemId)
			{
				if (item != null && AddFuel((ColonyStorage)item))
				{
					flag = true;
					break;
				}
			}
		}
		if (!flag)
		{
			_SubData.m_CurWorkedTime = -1f;
			_SubData.m_WorkedTime = -1f;
			_Network.RPCOthers(EPacketType.PT_CL_PPC_NoPower);
		}
	}

	public override void InitMyData()
	{
		_SubData.m_WorkedTime = Info.m_WorkedTime;
		_SubData.m_CurWorkedTime = 0f;
	}

	public override bool AddFuel(Player player)
	{
		int num = -1;
		for (int i = 0; i < Info.m_WorkedTimeItemID.Count; i++)
		{
			int itemID = Info.m_WorkedTimeItemID[i];
			float num2 = Mathf.Max(_SubData.m_WorkedTime - _SubData.m_CurWorkedTime, 0f);
			float num3 = num2 / _SubData.m_WorkedTime;
			int num4 = Mathf.Max(1, Mathf.RoundToInt((float)Info.m_WorkedTimeItemCnt[i] * (1f - num3)));
			if (player.GetItemNum(itemID) >= num4)
			{
				num = i;
				break;
			}
		}
		if (num != -1)
		{
			int itemID2 = Info.m_WorkedTimeItemID[num];
			float num5 = Mathf.Max(_SubData.m_WorkedTime - _SubData.m_CurWorkedTime, 0f);
			float num6 = num5 / _SubData.m_WorkedTime;
			int num7 = Mathf.Max(1, Mathf.RoundToInt((float)Info.m_WorkedTimeItemCnt[num] * (1f - num6)));
			if (player.GetItemNum(itemID2) >= num7)
			{
				List<ItemObject> effItems = new List<ItemObject>(10);
				player.Package.RemoveItem(itemID2, num7, ref effItems);
				ChannelNetwork.SyncItemList(_Network.WorldId, effItems);
				player.SyncPackageIndex();
				_SubData.m_CurWorkedTime = 0f;
				_SubData.m_WorkedTime = Info.m_WorkedTime;
				SendWorkedTime();
				return true;
			}
			return false;
		}
		return false;
	}

	public bool AddFuel(ColonyStorage colonyObj)
	{
		int num = -1;
		for (int i = 0; i < Info.m_WorkedTimeItemID.Count; i++)
		{
			int itemId = Info.m_WorkedTimeItemID[i];
			float num2 = Mathf.Max(_SubData.m_WorkedTime - _SubData.m_CurWorkedTime, 0f);
			float num3 = num2 / _SubData.m_WorkedTime;
			int num4 = Mathf.Max(1, Mathf.RoundToInt((float)Info.m_WorkedTimeItemCnt[i] * (1f - num3)));
			ItemObject itemByID = ItemManager.GetItemByID(colonyObj.GetItemObjByItemId(itemId));
			if (itemByID != null && itemByID.stackCount >= num4)
			{
				num = i;
				break;
			}
		}
		if (num != -1)
		{
			int itemId2 = Info.m_WorkedTimeItemID[num];
			float num5 = Mathf.Max(_SubData.m_WorkedTime - _SubData.m_CurWorkedTime, 0f);
			float num6 = num5 / _SubData.m_WorkedTime;
			int num7 = Mathf.Max(1, Mathf.RoundToInt((float)Info.m_WorkedTimeItemCnt[num] * (1f - num6)));
			ItemObject itemByID2 = ItemManager.GetItemByID(colonyObj.GetItemObjByItemId(itemId2));
			if (itemByID2 != null && itemByID2.stackCount >= num7)
			{
				ItemObject[] items = colonyObj.DeleteItemWithItemID(itemByID2.protoData, num7).ToArray();
				ChannelNetwork.SyncItemList(_Network.WorldId, items);
				IEnumerable<int> itemObjIDs = colonyObj._Items.GetItemObjIDs(itemByID2.GetTabIndex());
				_SubData.m_CurWorkedTime = 0f;
				_SubData.m_WorkedTime = Info.m_WorkedTime;
				SendWorkedTime();
				return true;
			}
			return false;
		}
		return false;
	}

	public void SendWorkedTime()
	{
		_Network.RPCOthers(EPacketType.PT_CL_PPC_WorkedTime, _SubData.m_CurWorkedTime);
	}

	private void StartWorkingCounter(float curWorkedTime)
	{
		_SubData.m_WorkedTime = Info.m_WorkedTime;
		_SubData.m_CurWorkedTime = curWorkedTime;
	}

	public override List<ItemIdCount> GetRequirements()
	{
		List<ItemIdCount> list = new List<ItemIdCount>();
		float num = Mathf.Max(_SubData.m_WorkedTime - _SubData.m_CurWorkedTime, 0f);
		float num2 = num / Info.m_WorkedTime;
		if (num2 < AutoPercent)
		{
			list.Add(new ItemIdCount(FuelID, AutoCount));
		}
		return list;
	}

	public override bool MeetDemand(int protoId, int count)
	{
		if (count <= 0)
		{
			return true;
		}
		float num = 1f / (float)FuelMaxCount;
		float num2 = (float)count * num;
		float num3 = num2 * Info.m_WorkedTime;
		float curWorkedTime = Mathf.Max(0f, _SubData.m_CurWorkedTime - num3);
		StartWorkingCounter(curWorkedTime);
		SendWorkedTime();
		return true;
	}

	public override void MeetDemands(List<ItemIdCount> supplyItems)
	{
		int count = supplyItems[0].count;
		if (count > 0)
		{
			float num = 1f / (float)FuelMaxCount;
			float num2 = (float)count * num;
			float num3 = num2 * Info.m_WorkedTime;
			float curWorkedTime = Mathf.Max(0f, _SubData.m_CurWorkedTime - num3);
			StartWorkingCounter(curWorkedTime);
			SendWorkedTime();
		}
	}
}

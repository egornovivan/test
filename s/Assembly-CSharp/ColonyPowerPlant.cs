using System.Collections.Generic;
using System.IO;
using ItemAsset;
using UnityEngine;

public class ColonyPowerPlant : ColonyBase
{
	public const int MAX_WORKER_COUNT = 4;

	protected List<ItemObject> _ChargeItems = new List<ItemObject>();

	public CSPowerPlanetData _MyData;

	private int _ChargePro;

	public override int MaxWorkerCount => 4;

	public virtual CSPPCoalInfo Info => CSPPCoalInfo.ppCoalInfo;

	public ColonyPowerPlant()
	{
	}

	public ColonyPowerPlant(ColonyNetwork network)
	{
		SetNetwork(network);
		_RecordData = new CSPowerPlanetData();
		_MyData = (CSPowerPlanetData)_RecordData;
		LoadData();
	}

	public override void InitMyData()
	{
	}

	public override void CombomData(BinaryWriter writer)
	{
		BufferHelper.Serialize(writer, _MyData.m_ChargingItems.Count);
		foreach (KeyValuePair<int, int> chargingItem in _MyData.m_ChargingItems)
		{
			BufferHelper.Serialize(writer, chargingItem.Key);
			BufferHelper.Serialize(writer, chargingItem.Value);
		}
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
			_MyData.m_ChargingItems[key] = num2;
			if (ItemManager.GetItemByID(num2) != null)
			{
				_ChargeItems.Add(ItemManager.GetItemByID(num2));
			}
		}
	}

	public override bool IsWorking()
	{
		return false;
	}

	public virtual bool AddFuel(Player player)
	{
		return false;
	}

	public bool AddChargeItem(int index, int objId, Player player)
	{
		if (!IsWorking())
		{
			return false;
		}
		ItemObject itemByID = ItemManager.GetItemByID(objId);
		if (_MyData.m_ChargingItems.ContainsKey(index) && _MyData.m_ChargingItems[index] != 0)
		{
			ItemObject itemByID2 = ItemManager.GetItemByID(_MyData.m_ChargingItems[index]);
			if (player.Package.GetEmptyGridCount(itemByID2.protoData) <= 0)
			{
				return false;
			}
			if (!RemoveChargeItem(itemByID2, player))
			{
				return false;
			}
		}
		_MyData.m_ChargingItems[index] = objId;
		if (!_ChargeItems.Contains(itemByID) && itemByID != null)
		{
			_ChargeItems.Add(itemByID);
			if (!player.RemoveEquipment(itemByID))
			{
				player.Package.RemoveItem(itemByID);
				player.SyncPackageIndex();
			}
		}
		SyncSave();
		return true;
	}

	public bool RemoveChargeItem(ItemObject obj, Player player)
	{
		if (obj == null || player == null)
		{
			return false;
		}
		if (!IsWorking())
		{
			return false;
		}
		if (!_MyData.m_ChargingItems.ContainsValue(obj.instanceId) || !_ChargeItems.Contains(obj))
		{
			return false;
		}
		player.Package.AddItem(obj);
		player.SyncPackageIndex();
		foreach (KeyValuePair<int, int> chargingItem in _MyData.m_ChargingItems)
		{
			if (chargingItem.Value == obj.instanceId)
			{
				_MyData.m_ChargingItems.Remove(chargingItem.Key);
				break;
			}
		}
		_ChargeItems.Remove(obj);
		SyncItem(obj);
		SyncSave();
		return true;
	}

	protected void ChargingItem(float deltaTime)
	{
		_ChargePro++;
		foreach (ItemObject chargeItem in _ChargeItems)
		{
			if (chargeItem == null)
			{
				continue;
			}
			Energy cmpt = chargeItem.GetCmpt<Energy>();
			if (cmpt == null)
			{
				continue;
			}
			cmpt.energy.Change(deltaTime * Info.m_ChargingRate * 10000f / Time.deltaTime);
			cmpt.energy.ChangePercent(deltaTime * Info.m_ChargingRate);
			if (_ChargePro % 5 == 0 || cmpt.energy.current == cmpt.GetRawMax())
			{
				if (chargeItem.instanceId < 100000000)
				{
					ChannelNetwork.SyncItem(_Network.WorldId, chargeItem);
				}
				else
				{
					SyncCreationFuel(chargeItem.instanceId, cmpt.floatValue.current);
				}
			}
		}
	}
}

using System.Collections.Generic;
using CSRecord;
using ItemAsset;
using Pathea;

public class ColonyPowerPlant : ColonyBase
{
	private CSPowerPlanetData _MyData;

	public ColonyPowerPlant()
	{
	}

	public ColonyPowerPlant(ColonyNetwork network)
	{
		SetNetwork(network);
		_RecordData = new CSPowerPlanetData();
		_MyData = (CSPowerPlanetData)_RecordData;
	}

	public void AddChargeItem(int index, int objId)
	{
		ItemObject itemObject = PeSingleton<ItemMgr>.Instance.Get(objId);
		if (itemObject != null)
		{
			_MyData.m_ChargingItems[index] = objId;
		}
	}

	public void RemoveChargeItem(int objId)
	{
		foreach (KeyValuePair<int, int> chargingItem in _MyData.m_ChargingItems)
		{
			if (chargingItem.Value == objId)
			{
				_MyData.m_ChargingItems.Remove(chargingItem.Key);
				break;
			}
		}
	}

	public virtual bool IsWorking()
	{
		return false;
	}
}

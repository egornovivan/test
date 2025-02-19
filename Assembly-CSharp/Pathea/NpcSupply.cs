using System.Collections.Generic;
using ItemAsset;

namespace Pathea;

public class NpcSupply
{
	private static NpcSupplyAgent m_Agent = new NpcSupplyAgent();

	public static bool AgentSupply(PeEntity npc, CSAssembly csAssembly, ESupplyType type)
	{
		if (m_Agent == null)
		{
			m_Agent = new NpcSupplyAgent();
		}
		return m_Agent.Supply(type, npc, csAssembly);
	}

	public static bool CsStorageSupply(PeEntity npc, CSAssembly assembly, int protoId, int count)
	{
		if (assembly == null || assembly.Storages == null || npc.packageCmpt == null)
		{
			return false;
		}
		int itemCounFromFactoryAndAllStorage = CSUtils.GetItemCounFromFactoryAndAllStorage(protoId, assembly);
		if (itemCounFromFactoryAndAllStorage > count)
		{
			if (CSUtils.CountDownItemFromAllStorage(protoId, count, assembly))
			{
				return npc.packageCmpt.Add(protoId, count);
			}
		}
		else if (itemCounFromFactoryAndAllStorage > 0 && CSUtils.CountDownItemFromAllStorage(protoId, itemCounFromFactoryAndAllStorage, assembly))
		{
			return npc.packageCmpt.Add(protoId, itemCounFromFactoryAndAllStorage);
		}
		return false;
	}

	public static bool CsStrargeSupplyExchangeBattery(PeEntity npc, CSAssembly assembly, List<ItemObject> objs, int protoId, int count = 1)
	{
		if (assembly == null || assembly.Storages == null || npc.packageCmpt == null)
		{
			return false;
		}
		List<ItemObject> itemListInStorage = CSUtils.GetItemListInStorage(protoId, assembly);
		if (objs != null)
		{
			for (int i = 0; i < objs.Count; i++)
			{
				if (CSUtils.AddItemObjToStorage(objs[i].instanceId, assembly))
				{
					npc.packageCmpt.Remove(objs[i]);
				}
			}
		}
		if (itemListInStorage.Count <= 0)
		{
			return false;
		}
		for (int j = 0; j < itemListInStorage.Count; j++)
		{
			Energy cmpt = itemListInStorage[j].GetCmpt<Energy>();
			if (cmpt != null && cmpt.floatValue.percent > 0f && CSUtils.RemoveItemObjFromStorage(itemListInStorage[j].instanceId, assembly))
			{
				return npc.packageCmpt.Add(itemListInStorage[j]);
			}
		}
		return false;
	}

	public static void CsStorageSupplyNpcs(List<PeEntity> npcs, ESupplyType type)
	{
		if (npcs == null)
		{
			return;
		}
		for (int i = 0; i < npcs.Count; i++)
		{
			if (npcs[i].NpcCmpt != null && npcs[i].NpcCmpt.Creater != null && npcs[i].NpcCmpt.Creater.Assembly != null)
			{
				AgentSupply(npcs[i], npcs[i].NpcCmpt.Creater.Assembly, type);
			}
		}
	}

	public static void SupplyNpcsByCSAssembly(List<PeEntity> npcs, CSAssembly assembly, ESupplyType type)
	{
		if (npcs != null)
		{
			for (int i = 0; i < npcs.Count; i++)
			{
				AgentSupply(npcs[i], assembly, type);
			}
		}
	}
}

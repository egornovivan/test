using ItemAsset;

namespace Behave.Runtime;

[BehaveAction(typeof(BTWeaponry), "Weaponry")]
public class BTWeaponry : BTNormal
{
	private class Data
	{
	}

	private Data m_Data;

	private float startTime;

	private float waitTime = 3f;

	private ItemObject m_BetterAtkObj;

	private AttackMode m_Atkmode;

	private ItemObject m_BetterDefObj;

	private BehaveResult Tick(Tree sender)
	{
		if (base.entity.NpcCmpt.EqSelect.BetterAtkObj == null)
		{
			return BehaveResult.Success;
		}
		m_BetterAtkObj = base.entity.NpcCmpt.EqSelect.BetterAtkObj;
		m_Atkmode = base.entity.NpcCmpt.EqSelect.BetterAtkObj.protoData.weaponInfo.attackModes[0];
		if (m_BetterAtkObj != null && base.entity.motionEquipment.ActiveableEquipment == null)
		{
			SelectItem.EquipByObj(base.entity, m_BetterAtkObj);
			return BehaveResult.Running;
		}
		if (m_BetterAtkObj != null && base.entity.motionEquipment.ActiveableEquipment != null && base.entity.motionEquipment.ActiveableEquipment.m_ItemObj != m_BetterAtkObj)
		{
			SelectItem.EquipByObj(base.entity, m_BetterAtkObj);
			return BehaveResult.Running;
		}
		return BehaveResult.Success;
	}
}

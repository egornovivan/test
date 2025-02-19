using ItemAsset;

namespace Behave.Runtime;

[BehaveAction(typeof(BTIsOnlyGloves), "IsOnlyGloves")]
public class BTIsOnlyGloves : BTNormal
{
	private class Data
	{
	}

	private BehaveResult Tick(Tree sender)
	{
		if (base.entity.NpcCmpt.EqSelect.BetterAtkObj == null)
		{
			if (base.entity.motionEquipment.axe != null && base.entity.motionEquipment.axe is PEChainSaw)
			{
				SelectItem.TakeOffEquip(base.entity);
			}
			else if (base.entity.motionEquipment.digTool != null && base.entity.motionEquipment.digTool is PECrusher)
			{
				SelectItem.TakeOffEquip(base.entity);
			}
			return BehaveResult.Success;
		}
		return BehaveResult.Failure;
	}
}

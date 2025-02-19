using System.Collections.Generic;
using ItemAsset;
using Pathea.PeEntityExtNpcPackage;

namespace Pathea;

public class AmmoExpend_Battery : AmmoExpend
{
	private const float Energy_Percent_min = 0f;

	private const float Energy_Percent_Max = 0.5f;

	private const int Energy_num = 1;

	public override EAtkRangeExpend Type => EAtkRangeExpend.Battery;

	public override int costId => 228;

	public override bool useEnergy => true;

	public override bool SupplySth(PeEntity entity, CSAssembly asAssembly)
	{
		float attribute = entity.GetAttribute(AttribType.Energy);
		if (attribute > 0f)
		{
			return false;
		}
		List<ItemObject> equipObjs = entity.GetEquipObjs(EeqSelect.energy);
		if (equipObjs != null && equipObjs.Count > 0)
		{
			for (int i = 0; i < equipObjs.Count; i++)
			{
				Energy cmpt = equipObjs[i].GetCmpt<Energy>();
				if (cmpt != null && cmpt.floatValue.percent > 0f)
				{
					return false;
				}
			}
		}
		return NpcSupply.CsStrargeSupplyExchangeBattery(entity, asAssembly, equipObjs, costId);
	}
}

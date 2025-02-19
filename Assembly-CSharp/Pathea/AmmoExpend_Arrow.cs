using Pathea.PeEntityExtNpcPackage;

namespace Pathea;

public class AmmoExpend_Arrow : AmmoExpend
{
	private const int Arrow_Supply_min = 10;

	private const int Arrow_Supply_Once = 100;

	public override EAtkRangeExpend Type => EAtkRangeExpend.Arrow;

	public override int costId => 49;

	public override bool useEnergy => false;

	public override bool SupplySth(PeEntity entity, CSAssembly asAssembly)
	{
		int itemCount = entity.GetItemCount(costId);
		if (itemCount > 10)
		{
			return false;
		}
		return NpcSupply.CsStorageSupply(entity, asAssembly, costId, 100);
	}
}

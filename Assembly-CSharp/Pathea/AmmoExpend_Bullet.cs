using Pathea.PeEntityExtNpcPackage;

namespace Pathea;

public class AmmoExpend_Bullet : AmmoExpend
{
	private const int BULLET_Supply_once = 100;

	private const int BULLET_Supply_min = 20;

	public override EAtkRangeExpend Type => EAtkRangeExpend.Bullet;

	public override int costId => 50;

	public override bool useEnergy => false;

	public override bool SupplySth(PeEntity entity, CSAssembly asAssembly)
	{
		int itemCount = entity.GetItemCount(costId);
		if (itemCount > 20)
		{
			return false;
		}
		return NpcSupply.CsStorageSupply(entity, asAssembly, costId, 100);
	}
}

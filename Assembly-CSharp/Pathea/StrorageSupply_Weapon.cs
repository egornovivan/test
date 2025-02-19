using System;

namespace Pathea;

public class StrorageSupply_Weapon : StrorageSupply
{
	public override ESupplyType Type => ESupplyType.Weapon;

	public override bool DoSupply(PeEntity entity, CSAssembly CsAssembly)
	{
		throw new NotImplementedException();
	}
}

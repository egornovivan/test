namespace Pathea;

public abstract class StrorageSupply
{
	public abstract ESupplyType Type { get; }

	public abstract bool DoSupply(PeEntity entity, CSAssembly CsAssembly);
}

namespace Pathea;

public abstract class AmmoExpend
{
	public abstract bool useEnergy { get; }

	public abstract int costId { get; }

	public abstract EAtkRangeExpend Type { get; }

	public abstract bool SupplySth(PeEntity entity, CSAssembly asAssembly);

	public virtual bool MatchExpend(int costProtoId)
	{
		return costId == costProtoId;
	}
}

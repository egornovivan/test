namespace Pathea;

public class NpcSupplyAgent
{
	private StrorageSupply[] mCsSupplies;

	public NpcSupplyAgent()
	{
		mCsSupplies = new StrorageSupply[2];
		mCsSupplies[0] = new StorageSupply_Ammo();
		mCsSupplies[1] = new StrorageSupply_Weapon();
	}

	public bool Supply(ESupplyType type, PeEntity entity, CSAssembly assembly)
	{
		return mCsSupplies[(int)type] != null && mCsSupplies[(int)type].DoSupply(entity, assembly);
	}
}

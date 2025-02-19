using ItemAsset;

namespace Pathea;

public abstract class PackageCmpt_Equip
{
	protected SlotList mEqiupSlotList = new SlotList();

	public abstract SlotList GetEquipList();
}

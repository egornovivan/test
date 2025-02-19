using ItemAsset;
using Pathea;

public class PEAimAbleEquip : PEHoldAbleEquipment
{
	public PEAimAttr m_AimAttr = new PEAimAttr();

	protected IKCmpt m_IKCmpt;

	public override void InitEquipment(PeEntity entity, ItemObject itemObj)
	{
		base.InitEquipment(entity, itemObj);
		m_IKCmpt = m_Entity.GetCmpt<IKCmpt>();
	}
}

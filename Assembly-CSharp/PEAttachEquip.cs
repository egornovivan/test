using ItemAsset;
using Pathea;

public class PEAttachEquip : PEEquipment
{
	public string m_AttachBoneName = "mountMain";

	public override void InitEquipment(PeEntity entity, ItemObject itemObj)
	{
		base.InitEquipment(entity, itemObj);
		m_View.AttachObject(base.gameObject, m_AttachBoneName);
	}
}

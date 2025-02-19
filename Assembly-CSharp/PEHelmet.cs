using ItemAsset;
using Pathea;

public class PEHelmet : PEEquipment
{
	public string m_AttachBoneName = "Bip01 Head";

	public override void InitEquipment(PeEntity entity, ItemObject itemObj)
	{
		base.InitEquipment(entity, itemObj);
		m_View.AttachObject(base.gameObject, m_AttachBoneName);
	}
}

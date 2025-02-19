using ItemAsset;
using Pathea;

public class PEBattery : PEEquipment
{
	private const string AttachBone = "Bip01 L Thigh";

	public override void InitEquipment(PeEntity entity, ItemObject itemObj)
	{
		base.InitEquipment(entity, itemObj);
		BiologyViewCmpt biologyViewCmpt = m_Entity.biologyViewCmpt;
		if (null != biologyViewCmpt)
		{
			biologyViewCmpt.AttachObject(base.gameObject, "Bip01 L Thigh");
		}
	}
}

using ItemAsset;
using Pathea;

public class PEWaterPitcher : PECtrlAbleEquipment
{
	private const string AttachBone = "mountMain";

	public override void InitEquipment(PeEntity entity, ItemObject itemObj)
	{
		base.InitEquipment(entity, itemObj);
		m_View.AttachObject(base.gameObject, "mountMain");
	}
}

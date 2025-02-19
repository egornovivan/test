using ItemAsset;
using Pathea;

public class PEGloves : PeSword
{
	public override void InitEquipment(PeEntity entity, ItemObject itemObj)
	{
		m_Entity = entity;
		m_ItemObj = itemObj;
		m_View = m_Entity.biologyViewCmpt;
		m_MotionEquip = m_Entity.motionEquipment;
		m_MotionMgr = m_Entity.motionMgr;
		m_View.AttachObject(base.gameObject, m_HandChangeAttr.m_PutOffBone);
	}
}

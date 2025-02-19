using ItemAsset;
using Pathea;
using UnityEngine;

public class PEEnergySheild : PECtrlAbleEquipment
{
	public EnergySheildHandler m_Handler;

	public GameObject m_SubPart;

	public string attachBone = "Bip01 Spine3";

	public string subPartAttachBone = "Bip01 R Clavicle";

	private float m_LastNetValue;

	private UTimer m_Time;

	public override void InitEquipment(PeEntity entity, ItemObject itemObj)
	{
		base.InitEquipment(entity, itemObj);
		if (null != m_SubPart)
		{
			m_View.AttachObject(m_SubPart, subPartAttachBone);
		}
		m_View.AttachObject(base.gameObject, attachBone);
	}

	public override void RemoveEquipment()
	{
		base.RemoveEquipment();
		if (null != m_SubPart)
		{
			m_View.DetachObject(m_SubPart);
			Object.Destroy(m_SubPart);
		}
	}

	protected virtual void Update()
	{
		bool flag = null != m_Entity && m_Entity.GetAttribute(AttribType.Shield) > 0f;
		if (null != m_Handler)
		{
			m_Handler.gameObject.layer = 25;
			if (m_Handler.gameObject.activeSelf != flag)
			{
				m_Handler.gameObject.SetActive(flag);
			}
		}
	}
}

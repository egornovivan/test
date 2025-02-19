using ItemAsset;
using Pathea;
using UnityEngine;

public class PEWaterPump : PEAimAbleEquip
{
	public Transform mBackPack;

	private EquipmentActiveEffect m_Effect;

	public override void InitEquipment(PeEntity entity, ItemObject itemObj)
	{
		base.InitEquipment(entity, itemObj);
		if (null != mBackPack)
		{
			if (null != m_View)
			{
				m_View.AttachObject(mBackPack.gameObject, "Bow_box");
			}
			else
			{
				mBackPack.gameObject.SetActive(value: false);
			}
		}
		m_Effect = GetComponent<EquipmentActiveEffect>();
	}

	public override void RemoveEquipment()
	{
		base.RemoveEquipment();
		if (null != m_View && null != mBackPack)
		{
			m_View.DetachObject(mBackPack.gameObject);
			Object.Destroy(mBackPack.gameObject);
		}
	}

	protected override void UpdateHideState()
	{
		base.UpdateHideState();
		if (!(null == m_Entity))
		{
			MainPlayerCmpt component = m_Entity.GetComponent<MainPlayerCmpt>();
			if (null != component)
			{
				mBackPack.gameObject.SetActive(!component.firstPersonCtrl);
			}
		}
	}

	public override void SetActiveState(bool active)
	{
		base.SetActiveState(active);
		if (null != m_Effect)
		{
			m_Effect.SetActiveState(active);
		}
	}
}

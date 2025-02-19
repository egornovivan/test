using ItemAsset;
using Pathea;
using UnityEngine;

public class PEConversionGun : PEGun
{
	public Transform mBackPack;

	private PackageCmpt m_Pack;

	public override float magazineSize
	{
		get
		{
			if (null != m_Pack)
			{
				return 99999f;
			}
			return 0f;
		}
	}

	public override float magazineValue
	{
		get
		{
			if (null != m_Pack)
			{
				return m_Pack.GetItemCount(base.curItemID);
			}
			return 0f;
		}
		set
		{
			if (null != m_Pack)
			{
				int itemCount = m_Pack.GetItemCount(base.curItemID);
				if ((float)itemCount > value)
				{
					m_Pack.Destory(base.curItemID, Mathf.RoundToInt((float)itemCount - value));
				}
			}
		}
	}

	public override void InitEquipment(PeEntity entity, ItemObject itemObj)
	{
		base.InitEquipment(entity, itemObj);
		m_Pack = m_Entity.GetCmpt<PackageCmpt>();
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
}

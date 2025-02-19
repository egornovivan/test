using ItemAsset;
using Pathea;
using UnityEngine;

public class PEHoldAbleEquipment : PECtrlAbleEquipment
{
	[Header("HoldAbleAttr")]
	public ActiveAttr m_HandChangeAttr;

	public virtual bool canHoldEquipment => true;

	protected void InitLayer(PeEntity entity)
	{
		AnimatorCmpt component = entity.GetComponent<AnimatorCmpt>();
		if (!(component != null))
		{
			return;
		}
		string[] layers = m_HandChangeAttr.m_Layers;
		foreach (string text in layers)
		{
			if (!string.IsNullOrEmpty(text))
			{
				component.SetLayerWeight(text, 1f);
			}
		}
	}

	protected void ResetLayer(PeEntity entity)
	{
		AnimatorCmpt component = entity.GetComponent<AnimatorCmpt>();
		if (!(component != null))
		{
			return;
		}
		string[] layers = m_HandChangeAttr.m_Layers;
		foreach (string text in layers)
		{
			if (!string.IsNullOrEmpty(text))
			{
				component.SetLayerWeight(text, 0f);
			}
		}
	}

	public override void InitEquipment(PeEntity entity, ItemObject itemObj)
	{
		base.InitEquipment(entity, itemObj);
		InitLayer(entity);
		m_View.AttachObject(base.gameObject, m_HandChangeAttr.m_PutOffBone);
	}

	public override void RemoveEquipment()
	{
		base.RemoveEquipment();
		ResetLayer(m_Entity);
	}
}

using ItemAsset;
using Pathea;
using UnityEngine;

public class PEParachute : PECtrlAbleEquipment
{
	private const string AttachBone = "Bow_box";

	public float m_TurnOnSpeed = -10f;

	public float BalanceDownSpeed = -3f;

	public float m_HorizonalSpeed = 5f;

	private Animator m_Anim;

	public override void InitEquipment(PeEntity entity, ItemObject itemObj)
	{
		base.InitEquipment(entity, itemObj);
		m_Anim = GetComponentInChildren<Animator>();
		m_View.AttachObject(base.gameObject, "Bow_box");
		SetOpenState(open: false);
	}

	public void SetOpenState(bool open)
	{
		if (null != m_Anim)
		{
			m_Anim.SetBool("Open", open);
		}
	}
}

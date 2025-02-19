using ItemAsset;
using Pathea;
using UnityEngine;

public class PEGlider : PECtrlAbleEquipment
{
	private const string AttachBone = "Bow_box";

	private const float GravityAcc = 10f;

	public float m_TurnOnSpeed = -10f;

	public float m_RotateAcc = 5f;

	public float m_BoostPower = 2f;

	public float m_BalanceForwardSpeed = 20f;

	public float m_BalanceDownSpeed = 5f;

	public float m_AreaDragF = 0.004f;

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

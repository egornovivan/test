using Pathea;
using UnityEngine;

[RequireComponent(typeof(Animator))]
public class AnimatorCtrl : MonoBehaviour
{
	private Animator m_Anim;

	private AnimatorCmpt m_AnimCmpt;

	private void Start()
	{
		m_Anim = GetComponent<Animator>();
		PeEntity componentOrOnParent = VCUtils.GetComponentOrOnParent<PeEntity>(base.gameObject);
		if (null != componentOrOnParent)
		{
			m_AnimCmpt = componentOrOnParent.GetCmpt<AnimatorCmpt>();
		}
	}

	public void AnimEvent(string para)
	{
		if (null != m_AnimCmpt)
		{
			m_AnimCmpt.AnimEvent(para);
		}
	}

	private void OnAnimatorMove()
	{
		if (null != m_AnimCmpt)
		{
			m_AnimCmpt.m_LastRot = m_Anim.deltaRotation;
			m_AnimCmpt.m_LastMove = m_Anim.deltaPosition;
		}
	}
}

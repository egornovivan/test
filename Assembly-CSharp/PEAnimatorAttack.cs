using Pathea;
using UnityEngine;

public class PEAnimatorAttack : StateMachineBehaviour
{
	internal PeEntity m_Entity;

	public EAttackCheck AttackCheck;

	private bool m_Init;

	internal virtual void Init(Animator animator)
	{
	}

	private void InitAnimator(Animator animator)
	{
		if (!m_Init)
		{
			m_Entity = animator.GetComponentInParent<PeEntity>();
			Init(animator);
			m_Init = true;
		}
	}

	public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
	{
		base.OnStateEnter(animator, stateInfo, layerIndex);
		InitAnimator(animator);
		animator.SetBool("Attacking", value: true);
		if (m_Entity.animCmpt != null)
		{
			m_Entity.animCmpt.SetInteger("attackCheck", (int)AttackCheck);
		}
	}

	public override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
	{
		base.OnStateExit(animator, stateInfo, layerIndex);
		animator.SetBool("Attacking", value: false);
		if (m_Entity.animCmpt != null)
		{
			m_Entity.animCmpt.SetInteger("attackCheck", (int)AttackCheck);
		}
	}
}

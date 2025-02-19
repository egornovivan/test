using Pathea;
using UnityEngine;

public class PEAnimatorState : StateMachineBehaviour
{
	private bool m_Init;

	private PeEntity m_Entity;

	public PeEntity Entity => m_Entity;

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
	}
}

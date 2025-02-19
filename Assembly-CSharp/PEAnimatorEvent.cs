using Pathea;
using UnityEngine;

public abstract class PEAnimatorEvent : StateMachineBehaviour
{
	[Range(0f, 1f)]
	public float triggerTime;

	private bool m_Trigger;

	private PeEntity m_Entity;

	internal PeEntity Entity => m_Entity;

	internal abstract void OnTrigger();

	public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
	{
		base.OnStateEnter(animator, stateInfo, layerIndex);
		m_Trigger = false;
	}

	public override void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
	{
		base.OnStateUpdate(animator, stateInfo, layerIndex);
		if (m_Entity == null)
		{
			m_Entity = animator.GetComponentInParent<PeEntity>();
		}
		if (!m_Trigger && stateInfo.normalizedTime >= triggerTime)
		{
			OnTrigger();
			m_Trigger = true;
		}
	}

	public override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
	{
		base.OnStateExit(animator, stateInfo, layerIndex);
		m_Trigger = false;
	}
}

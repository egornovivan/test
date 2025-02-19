using System;
using Pathea;
using PETools;
using UnityEngine;

public class PEAnimatorAttackTrigger : PEAnimatorAttack
{
	public float startTime;

	public float endTime;

	public string[] bones;

	private PEAttackTrigger[] m_Triggers;

	internal override void Init(Animator animator)
	{
		base.Init(animator);
		m_Triggers = new PEAttackTrigger[0];
		string[] array = bones;
		foreach (string boneName in array)
		{
			Transform child = PEUtil.GetChild(animator.transform, boneName);
			if (child != null)
			{
				PEAttackTrigger component = child.GetComponent<PEAttackTrigger>();
				if (null != component)
				{
					Array.Resize(ref m_Triggers, m_Triggers.Length + 1);
					m_Triggers[m_Triggers.Length - 1] = component;
				}
			}
		}
	}

	private void ClearHitInfo()
	{
		for (int i = 0; i < m_Triggers.Length; i++)
		{
			if (m_Triggers[i] != null)
			{
				m_Triggers[i].ClearHitInfo();
			}
		}
	}

	private void ResetHitInfo()
	{
		for (int i = 0; i < m_Triggers.Length; i++)
		{
			if (m_Triggers[i] != null)
			{
				m_Triggers[i].ClearHitInfo();
			}
		}
	}

	private void ActivateTrigger(bool value)
	{
		for (int i = 0; i < m_Triggers.Length; i++)
		{
			if (m_Triggers[i] != null)
			{
				m_Triggers[i].active = value;
			}
		}
	}

	public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
	{
		base.OnStateEnter(animator, stateInfo, layerIndex);
		ClearHitInfo();
		ActivateTrigger(value: false);
	}

	public override void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
	{
		base.OnStateUpdate(animator, stateInfo, layerIndex);
		if (stateInfo.normalizedTime > endTime || stateInfo.normalizedTime < startTime)
		{
			ResetHitInfo();
			ActivateTrigger(value: false);
		}
		else
		{
			ActivateTrigger(value: true);
		}
	}

	public override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
	{
		base.OnStateExit(animator, stateInfo, layerIndex);
		ClearHitInfo();
		ActivateTrigger(value: false);
	}
}

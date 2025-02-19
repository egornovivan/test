using System;
using Pathea;
using PETools;
using RootMotion.FinalIK;
using UnityEngine;

public class PEAnimatorIK : StateMachineBehaviour
{
	public string[] ikBones;

	public AnimationCurve weightCurve;

	private AimIK[] m_IKs;

	private TargetCmpt m_Target;

	private void GetIKs(Transform root)
	{
		if (m_IKs != null || !(root != null))
		{
			return;
		}
		m_IKs = new AimIK[0];
		Transform child = PEUtil.GetChild(root, "GrounderIK");
		if (!(child != null))
		{
			return;
		}
		for (int i = 0; i < ikBones.Length; i++)
		{
			Transform child2 = PEUtil.GetChild(child, ikBones[i]);
			if (child2 != null)
			{
				AimIK component = child2.GetComponent<AimIK>();
				if (component != null)
				{
					Array.Resize(ref m_IKs, m_IKs.Length + 1);
					m_IKs[m_IKs.Length - 1] = component;
				}
			}
		}
	}

	public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
	{
		base.OnStateEnter(animator, stateInfo, layerIndex);
		if (m_Target == null)
		{
			m_Target = animator.GetComponentInParent<TargetCmpt>();
		}
		if (m_Target == null || m_Target.GetAttackEnemy() == null)
		{
			return;
		}
		if (m_IKs == null)
		{
			GetIKs(m_Target.transform);
		}
		if (m_IKs != null && m_IKs.Length > 0)
		{
			for (int i = 0; i < m_IKs.Length; i++)
			{
				m_IKs[i].solver.target = m_Target.GetAttackEnemy().CenterBone;
			}
		}
	}

	public override void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
	{
		base.OnStateUpdate(animator, stateInfo, layerIndex);
		if (m_IKs == null || m_IKs.Length <= 0)
		{
			return;
		}
		for (int i = 0; i < m_IKs.Length; i++)
		{
			if (m_IKs[i] != null)
			{
				m_IKs[i].solver.IKPositionWeight = weightCurve.Evaluate(stateInfo.normalizedTime);
			}
		}
	}

	public override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
	{
		base.OnStateExit(animator, stateInfo, layerIndex);
		if (m_IKs == null || m_IKs.Length <= 0)
		{
			return;
		}
		for (int i = 0; i < m_IKs.Length; i++)
		{
			if (m_IKs[i] != null)
			{
				m_IKs[i].solver.target = null;
				m_IKs[i].solver.IKPositionWeight = 0f;
			}
		}
	}
}

using System;
using Pathea;
using PETools;
using RootMotion.FinalIK;
using UnityEngine;

public class PEAnimatorAim : StateMachineBehaviour
{
	public string[] ikBones;

	public AnimationCurve weightCurve;

	private AimIK[] m_IKs;

	private TargetCmpt m_Target;

	private Vector3 m_Pos;

	private void GetIKs(Transform root)
	{
		if (m_IKs != null)
		{
			return;
		}
		m_IKs = new AimIK[0];
		if (!(root != null) || !(root.parent != null))
		{
			return;
		}
		Transform child = PEUtil.GetChild(root.parent, "GrounderIK");
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
		if (m_IKs == null)
		{
			GetIKs(animator.transform);
		}
		if (m_Target == null)
		{
			m_Target = animator.GetComponentInParent<TargetCmpt>();
		}
		if (!(m_Target == null) && m_Target.GetAttackEnemy() != null && m_IKs != null && m_IKs.Length > 0)
		{
			m_Pos = m_Target.GetAttackEnemy().centerPos;
		}
	}

	public override void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
	{
		base.OnStateUpdate(animator, stateInfo, layerIndex);
		for (int i = 0; i < m_IKs.Length; i++)
		{
			m_IKs[i].solver.IKPosition = m_Pos;
			m_IKs[i].solver.IKPositionWeight = weightCurve.Evaluate(stateInfo.normalizedTime);
		}
	}

	public override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
	{
		base.OnStateExit(animator, stateInfo, layerIndex);
		for (int i = 0; i < m_IKs.Length; i++)
		{
			m_IKs[i].solver.IKPosition = Vector3.zero;
		}
	}
}

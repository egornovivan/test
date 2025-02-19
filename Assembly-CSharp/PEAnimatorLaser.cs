using System;
using Pathea;
using PETools;
using RootMotion.FinalIK;
using UnityEngine;

public class PEAnimatorLaser : StateMachineBehaviour
{
	public float length;

	public string[] ikBones;

	public AnimationCurve weightCurve;

	public AnimationCurve posCurve;

	private AimIK[] m_IKs;

	private TargetCmpt m_Target;

	private PeTrans m_Trans;

	private Vector3 m_Start;

	private Vector3 m_End;

	private void GetPoint(Transform tr, Vector3 target)
	{
		Vector3 vector = Vector3.ProjectOnPlane(target - tr.position, Vector3.up);
		Vector3 onNormal = Vector3.ProjectOnPlane(tr.forward, Vector3.up);
		Vector3 vector2 = Vector3.Project(vector, onNormal);
		Vector3 vector3 = vector - vector2;
		m_Start = target + vector3.normalized * length;
		m_End = target - vector3.normalized * length;
	}

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
		if (m_Trans == null)
		{
			m_Trans = animator.GetComponentInParent<PeTrans>();
		}
		if (!(m_Target == null) && m_Target.GetAttackEnemy() != null && m_IKs != null && m_IKs.Length > 0 && m_Trans != null)
		{
			GetPoint(m_Trans.trans, m_Target.GetAttackEnemy().position);
		}
	}

	public override void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
	{
		base.OnStateUpdate(animator, stateInfo, layerIndex);
		float normalizedTime = stateInfo.normalizedTime;
		float iKPositionWeight = Mathf.Clamp01(weightCurve.Evaluate(normalizedTime));
		float t = Mathf.Clamp01(posCurve.Evaluate(normalizedTime));
		Vector3 vector = Vector3.Lerp(m_Start, m_End, t);
		for (int i = 0; i < m_IKs.Length; i++)
		{
			m_IKs[i].solver.IKPosition = vector;
			m_IKs[i].solver.IKPositionWeight = iKPositionWeight;
		}
		Debug.DrawLine(m_Start, vector, Color.cyan);
	}

	public override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
	{
		base.OnStateExit(animator, stateInfo, layerIndex);
		for (int i = 0; i < m_IKs.Length; i++)
		{
			m_IKs[i].solver.target = null;
		}
	}
}

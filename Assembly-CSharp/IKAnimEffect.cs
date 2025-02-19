using System;
using RootMotion.FinalIK;
using UnityEngine;

[Serializable]
public class IKAnimEffect
{
	[Serializable]
	public class EffectortLink
	{
		public string m_Name;

		public FullBodyBipedEffector m_Effector;

		public AnimationCurve m_ForceDir;

		public AnimationCurve m_UpDir;

		public bool m_PinWordPos;

		public void OnModifyOffset(IKSolverFullBodyBiped solver, Vector3 dir, float timeP, float weight, Vector3 deltaDir)
		{
			if (solver != null)
			{
				solver.GetEffector(m_Effector).positionOffset += (m_ForceDir.Evaluate(timeP) * dir + m_UpDir.Evaluate(timeP) * Vector3.up - ((!m_PinWordPos) ? Vector3.zero : deltaDir)) * weight;
			}
		}
	}

	public Vector3 m_EffectDir = Vector3.forward;

	public float m_Weight = 1f;

	[Range(0.01f, 10f)]
	public float m_StepTime = 2f;

	[Range(0.01f, 1f)]
	public float m_FadeTime = 0.1f;

	private Vector3 m_StartPos;

	private Vector3 m_DeltaDir;

	private float m_RemainingTime;

	public EffectortLink[] m_Effects;

	public bool isRunning => m_RemainingTime > 0f;

	public void DoEffect(IKSolverFullBodyBiped solver, Vector3 dir, float weight)
	{
		if (solver != null)
		{
			m_EffectDir = dir;
			m_Weight = weight;
			m_StartPos = solver.GetRoot().position;
			m_DeltaDir = Vector3.zero;
			m_RemainingTime = m_StepTime;
		}
	}

	public void EndEffect()
	{
		m_RemainingTime = 0f;
	}

	public void OnModifyOffset(IKSolverFullBodyBiped solver, float weight, float deltaTime)
	{
		if (solver == null)
		{
			m_RemainingTime = 0f;
		}
		else if (m_RemainingTime > 0f)
		{
			float num = m_StepTime - m_RemainingTime;
			float num2 = Mathf.Clamp01(num / m_FadeTime);
			Vector3 deltaDir = solver.GetRoot().position - m_StartPos;
			EffectortLink[] effects = m_Effects;
			foreach (EffectortLink effectortLink in effects)
			{
				effectortLink.OnModifyOffset(solver, m_EffectDir, num / m_StepTime, weight * num2 * m_Weight, deltaDir);
			}
			m_RemainingTime -= deltaTime;
		}
	}
}

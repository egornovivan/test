using System;
using System.Collections.Generic;
using RootMotion.FinalIK;
using UnityEngine;

namespace PEIK;

[Serializable]
public class IKHitReaction
{
	[Serializable]
	public class HitPart
	{
		public string m_Name;

		public List<Transform> m_PartTrans;

		public IKAnimEffect m_Effect;

		public void Hit(IKSolverFullBodyBiped solver, Vector3 dir, float weight, float effectTime, float fadeTime = 0.1f)
		{
			m_Effect.m_StepTime = effectTime;
			m_Effect.m_FadeTime = fadeTime;
			m_Effect.DoEffect(solver, dir, weight);
		}

		public void OnModifyOffset(IKSolverFullBodyBiped solver, float weight, float deltaTime)
		{
			m_Effect.OnModifyOffset(solver, weight, deltaTime);
		}
	}

	public float weightScale = 1f;

	public HitPart[] m_HitParts;

	public bool isRunning
	{
		get
		{
			HitPart[] hitParts = m_HitParts;
			foreach (HitPart hitPart in hitParts)
			{
				if (hitPart.m_Effect.isRunning)
				{
					return true;
				}
			}
			return false;
		}
	}

	public void OnHit(IKSolverFullBodyBiped solver, Transform trans, Vector3 dir, float weight, float effectTime)
	{
		HitPart[] hitParts = m_HitParts;
		foreach (HitPart hitPart in hitParts)
		{
			if (hitPart.m_PartTrans.Contains(trans))
			{
				hitPart.Hit(solver, dir, weight * weightScale, effectTime);
			}
		}
	}

	public void OnModifyOffset(IKSolverFullBodyBiped solver, float weight, float deltaTime)
	{
		HitPart[] hitParts = m_HitParts;
		foreach (HitPart hitPart in hitParts)
		{
			hitPart.OnModifyOffset(solver, weight * weightScale, deltaTime);
		}
	}
}

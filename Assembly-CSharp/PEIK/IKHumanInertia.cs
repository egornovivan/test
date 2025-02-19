using System;
using System.Collections;
using RootMotion.FinalIK;
using UnityEngine;

namespace PEIK;

public class IKHumanInertia : IKOffsetModifier
{
	[Serializable]
	public class InertiaEffect
	{
		[Serializable]
		public class InertiaBody
		{
			[Serializable]
			public class EffectorLink
			{
				public FullBodyBipedEffector m_Effector;

				public float m_Weight = 1f;
			}

			public EffectorLink[] m_EffectorLink;

			public Transform m_FollowTarget;

			[Range(0.1f, 100f)]
			public float m_InertiaAcc = 5f;

			public float m_MachVelocity = 0.2f;

			public AnimationCurve m_VelocityFacter;

			private Vector3 m_LastVelocity;

			private Vector3 m_LazyPoint;

			public void Init()
			{
				m_LastVelocity = Vector3.zero;
				m_LazyPoint = m_FollowTarget.position;
			}

			public void Update(IKSolverFullBodyBiped solver, float weight, float deltaTime, float velocity)
			{
				Vector3 vector = m_FollowTarget.position - m_LazyPoint;
				Vector3 vector2 = vector / deltaTime;
				Vector3 vector3 = vector2 - m_LastVelocity;
				float a = vector3.magnitude / m_InertiaAcc;
				a = Mathf.Min(a, deltaTime);
				m_LastVelocity += vector3.normalized * m_InertiaAcc * a;
				m_LazyPoint += m_LastVelocity * deltaTime;
				vector = m_FollowTarget.position - m_LazyPoint;
				m_LazyPoint += vector * m_MachVelocity;
				vector = m_LazyPoint - m_FollowTarget.position;
				EffectorLink[] effectorLink = m_EffectorLink;
				foreach (EffectorLink effectorLink2 in effectorLink)
				{
					solver.GetEffector(effectorLink2.m_Effector).positionOffset += vector * effectorLink2.m_Weight * weight * m_VelocityFacter.Evaluate(velocity);
				}
			}
		}

		public InertiaBody[] m_Bodies;

		public float m_Weight = 1f;

		public float m_FadeTime = 0.2f;

		private float m_FadeWeight = 1f;

		private Vector3 m_LastPos = Vector3.zero;

		public void Init()
		{
			InertiaBody[] bodies = m_Bodies;
			foreach (InertiaBody inertiaBody in bodies)
			{
				inertiaBody.Init();
			}
		}

		public IEnumerator UpdateFadeState(bool fadein)
		{
			if (fadein)
			{
				while (m_FadeWeight < 1f)
				{
					if (m_FadeTime > 0f)
					{
						m_FadeWeight = Mathf.Clamp01(m_FadeWeight + Time.deltaTime / m_FadeTime);
					}
					else
					{
						m_FadeWeight = 1f;
					}
					yield return null;
				}
				yield break;
			}
			while (m_FadeWeight > float.Epsilon)
			{
				if (m_FadeTime > 0f)
				{
					m_FadeWeight = Mathf.Clamp01(m_FadeWeight - Time.deltaTime / m_FadeTime);
				}
				else
				{
					m_FadeWeight = 0f;
				}
				yield return null;
			}
		}

		public void Update(IKSolverFullBodyBiped solver, float deltaTime)
		{
			float velocity = (solver.GetRoot().position - m_LastPos).magnitude / deltaTime;
			m_LastPos = solver.GetRoot().position;
			InertiaBody[] bodies = m_Bodies;
			foreach (InertiaBody inertiaBody in bodies)
			{
				inertiaBody.Update(solver, m_Weight * m_FadeWeight, deltaTime, velocity);
			}
		}
	}

	public enum InertiaType
	{
		Null,
		Move,
		Blow
	}

	public InertiaEffect m_MoveInertia;

	public InertiaEffect m_BlowInertia;

	public void SetType(InertiaType type)
	{
		StopAllCoroutines();
		StartCoroutine(m_MoveInertia.UpdateFadeState(type == InertiaType.Move));
		StartCoroutine(m_BlowInertia.UpdateFadeState(type == InertiaType.Blow));
	}

	protected override void OnModifyOffset()
	{
		m_MoveInertia.Update(m_FBBIK.solver, base.deltaTime);
		m_BlowInertia.Update(m_FBBIK.solver, base.deltaTime);
	}

	protected override void OnInit()
	{
		m_MoveInertia.Init();
		m_BlowInertia.Init();
		SetType(InertiaType.Move);
	}
}

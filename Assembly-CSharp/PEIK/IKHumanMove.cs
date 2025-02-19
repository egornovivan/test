using System;
using System.Collections;
using RootMotion.FinalIK;
using UnityEngine;

namespace PEIK;

public class IKHumanMove : IKOffsetModifier
{
	[Serializable]
	public class BodyTiltForward
	{
		[Serializable]
		public class EffectorLink
		{
			public FullBodyBipedEffector m_Effector;

			public Vector3 m_Offset;

			[Range(0.1f, 100f)]
			public float m_TileAcc = 5f;

			public AnimationCurve m_TileWeight;

			private float m_ProVelocity;

			public void Reset()
			{
				m_ProVelocity = 0f;
			}

			public void Apply(IKSolverFullBodyBiped solver, float m_Weight, Vector3 curentVelocity, float deltaTime)
			{
				Vector3 from = Vector3.Project(curentVelocity, solver.GetRoot().forward);
				float num = from.magnitude;
				if (Vector3.Angle(from, solver.GetRoot().forward) > 150f)
				{
					num *= -1f;
				}
				m_ProVelocity = Mathf.Lerp(m_ProVelocity, num, m_TileAcc * deltaTime);
				solver.GetEffector(m_Effector).positionOffset += solver.GetRoot().TransformDirection(m_Offset * m_TileWeight.Evaluate(m_ProVelocity)) * m_Weight;
			}
		}

		public EffectorLink[] m_EffectorLinks;

		private Vector3 m_LastPosition;

		private Vector3 m_Velocity;

		[Range(-10f, 10f)]
		public float m_SubForward;

		public void Reset(IKSolverFullBodyBiped solver)
		{
			m_LastPosition = solver.GetRoot().position;
			EffectorLink[] effectorLinks = m_EffectorLinks;
			foreach (EffectorLink effectorLink in effectorLinks)
			{
				effectorLink.Reset();
			}
		}

		public void Update(IKSolverFullBodyBiped solver, float m_Weight, float deltaTime)
		{
			m_Velocity = (solver.GetRoot().position - m_LastPosition) / deltaTime;
			EffectorLink[] effectorLinks = m_EffectorLinks;
			foreach (EffectorLink effectorLink in effectorLinks)
			{
				effectorLink.Apply(solver, m_Weight, m_Velocity + solver.GetRoot().forward * m_SubForward, deltaTime);
			}
			m_LastPosition = solver.GetRoot().position;
		}
	}

	[Serializable]
	public class BodyTiltSide
	{
		[Serializable]
		public class EffectorLink
		{
			public FullBodyBipedEffector m_Effector;

			public Vector3 m_Offset;

			public Vector3 m_Pin;

			public Vector3 m_PinWeight;

			public void Apply(IKSolverFullBodyBiped solver, float m_Weight, Quaternion rotation)
			{
				solver.GetEffector(m_Effector).positionOffset += rotation * m_Offset * m_Weight;
				Vector3 vector = solver.GetRoot().position + rotation * m_Pin;
				Vector3 vector2 = vector - solver.GetEffector(m_Effector).bone.position;
				Vector3 vector3 = m_PinWeight * Mathf.Abs(m_Weight);
				solver.GetEffector(m_Effector).positionOffset = new Vector3(Mathf.Lerp(solver.GetEffector(m_Effector).positionOffset.x, vector2.x, vector3.x), Mathf.Lerp(solver.GetEffector(m_Effector).positionOffset.y, vector2.y, vector3.y), Mathf.Lerp(solver.GetEffector(m_Effector).positionOffset.z, vector2.z, vector3.z));
			}
		}

		public EffectorLink[] m_TurnLeftEffectorLinks;

		public EffectorLink[] m_TurnRightEffectorLinks;

		public float m_TiltSpeed = 6f;

		public float m_TiltSensitivity = 0.07f;

		private float m_TiltAngle;

		private Vector3 m_LastForward;

		[Range(-1f, 1f)]
		public float testAngle;

		public void Reset(IKSolverFullBodyBiped solver)
		{
			m_LastForward = solver.GetRoot().forward;
		}

		public void Update(IKSolverFullBodyBiped solver, float m_Weight, float deltaTime)
		{
			Quaternion quaternion = Quaternion.FromToRotation(m_LastForward, solver.GetRoot().forward);
			float angle = 0f;
			Vector3 axis = Vector3.zero;
			quaternion.ToAngleAxis(out angle, out axis);
			if (axis.y > 0f)
			{
				angle = 0f - angle;
			}
			angle *= m_TiltSensitivity * 0.01f;
			angle /= deltaTime;
			angle = Mathf.Clamp(angle, -1f, 1f);
			m_TiltAngle = Mathf.Lerp(m_TiltAngle, angle, deltaTime * m_TiltSpeed);
			float weight = Mathf.Abs(m_TiltAngle) / 1f * m_Weight + Mathf.Abs(testAngle);
			if (m_TiltAngle - testAngle < 0f)
			{
				EffectorLink[] turnRightEffectorLinks = m_TurnRightEffectorLinks;
				foreach (EffectorLink effectorLink in turnRightEffectorLinks)
				{
					effectorLink.Apply(solver, weight, solver.GetRoot().rotation);
				}
			}
			else
			{
				EffectorLink[] turnLeftEffectorLinks = m_TurnLeftEffectorLinks;
				foreach (EffectorLink effectorLink2 in turnLeftEffectorLinks)
				{
					effectorLink2.Apply(solver, weight, solver.GetRoot().rotation);
				}
			}
			m_LastForward = solver.GetRoot().forward;
		}
	}

	public BodyTiltForward m_BodyTiltForward;

	public BodyTiltSide m_BodyTiltSide;

	public float m_FadeOutTime = 0.2f;

	private float m_FadeWeight = 1f;

	private IEnumerator FadeUpdate(bool active)
	{
		while (m_FadeWeight > 0f)
		{
			m_FadeWeight -= base.deltaTime / m_FadeOutTime;
			yield return null;
		}
		m_FadeWeight = 0f;
	}

	public void SetActive(bool active)
	{
		StartCoroutine(FadeUpdate(active));
	}

	protected override void OnModifyOffset()
	{
		if (m_FadeWeight > 0f)
		{
			m_BodyTiltForward.Update(m_FBBIK.solver, m_Weight * m_FadeWeight, base.deltaTime);
			m_BodyTiltSide.Update(m_FBBIK.solver, m_Weight * m_FadeWeight, base.deltaTime);
		}
	}

	protected override void OnInit()
	{
		m_BodyTiltForward.Reset(m_FBBIK.solver);
		m_BodyTiltSide.Reset(m_FBBIK.solver);
		m_FadeWeight = 1f;
	}
}

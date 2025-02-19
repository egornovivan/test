using System;
using RootMotion.FinalIK;
using UnityEngine;

namespace PEIK;

[Serializable]
public class IKMoveEffect
{
	public IKAnimEffect m_StartEffect;

	public IKAnimEffect m_LFootStopEffect;

	public IKAnimEffect m_RFootStopEffect;

	public float m_CampAngle = 60f;

	public AnimationCurve m_SpeedToWeight;

	public AnimationCurve m_SpeedToTime;

	public bool isRunning => m_StartEffect.isRunning || m_RFootStopEffect.isRunning || m_LFootStopEffect.isRunning;

	public void StartMove(IKSolverFullBodyBiped solver, Vector3 velocity)
	{
	}

	public void StopMove(IKSolverFullBodyBiped solver, Vector3 velocity)
	{
		velocity = Vector3.ProjectOnPlane(velocity, Vector3.up);
		StopMove(solver, velocity, ISRFootMove(solver, velocity));
	}

	public void StopMove(IKSolverFullBodyBiped solver, Vector3 velocity, bool rightFoot)
	{
		float weight = m_SpeedToWeight.Evaluate(velocity.magnitude);
		float stepTime = m_SpeedToTime.Evaluate(velocity.magnitude);
		if (rightFoot)
		{
			m_RFootStopEffect.m_StepTime = stepTime;
			m_RFootStopEffect.DoEffect(solver, velocity.normalized, weight);
		}
		else
		{
			m_LFootStopEffect.m_StepTime = stepTime;
			m_LFootStopEffect.DoEffect(solver, velocity.normalized, weight);
		}
	}

	public void EndEffect()
	{
		m_StartEffect.EndEffect();
		m_RFootStopEffect.EndEffect();
		m_LFootStopEffect.EndEffect();
	}

	private bool ISRFootMove(IKSolverFullBodyBiped solver, Vector3 velocity)
	{
		Vector3 to = Util.ProjectOntoPlane(velocity, Vector3.up);
		Vector3 from = solver.GetEffector(FullBodyBipedEffector.LeftFoot).bone.transform.position - solver.GetEffector(FullBodyBipedEffector.RightFoot).bone.transform.position;
		return Vector3.Angle(from, to) > 90f;
	}

	public void OnModifyOffset(IKSolverFullBodyBiped solver, float weight, float deltaTime)
	{
		m_StartEffect.OnModifyOffset(solver, weight, deltaTime);
		m_LFootStopEffect.OnModifyOffset(solver, weight, deltaTime);
		m_RFootStopEffect.OnModifyOffset(solver, weight, deltaTime);
	}
}

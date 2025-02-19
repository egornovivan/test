using System;
using UnityEngine;

namespace RootMotion.FinalIK;

[Serializable]
public class InteractionLookAt
{
	public LookAtIK ik;

	public float lerpSpeed = 5f;

	public float weightSpeed = 1f;

	private Transform lookAtTarget;

	private float stopLookTime;

	private float weight;

	private bool firstFBBIKSolve;

	public void Look(Transform target, float time)
	{
		if (!(ik == null))
		{
			if (ik.solver.IKPositionWeight <= 0f)
			{
				ik.solver.IKPosition = ik.solver.GetRoot().position + ik.solver.GetRoot().forward * 3f;
			}
			lookAtTarget = target;
			stopLookTime = time;
		}
	}

	public void Update()
	{
		if (ik == null)
		{
			return;
		}
		if (ik.enabled)
		{
			ik.Disable();
		}
		if (!(lookAtTarget == null))
		{
			float num = ((!(Time.time < stopLookTime)) ? (0f - weightSpeed) : weightSpeed);
			weight = Mathf.Clamp(weight + num * Time.deltaTime, 0f, 1f);
			ik.solver.IKPositionWeight = Interp.Float(weight, InterpolationMode.InOutQuintic);
			ik.solver.IKPosition = Vector3.Lerp(ik.solver.IKPosition, lookAtTarget.position, lerpSpeed * Time.deltaTime);
			if (weight <= 0f)
			{
				lookAtTarget = null;
			}
			firstFBBIKSolve = true;
		}
	}

	public void SolveSpine()
	{
		if (!(ik == null) && firstFBBIKSolve)
		{
			float headWeight = ik.solver.headWeight;
			float eyesWeight = ik.solver.eyesWeight;
			ik.solver.headWeight = 0f;
			ik.solver.eyesWeight = 0f;
			ik.solver.Update();
			ik.solver.headWeight = headWeight;
			ik.solver.eyesWeight = eyesWeight;
		}
	}

	public void SolveHead()
	{
		if (!(ik == null) && firstFBBIKSolve)
		{
			float bodyWeight = ik.solver.bodyWeight;
			ik.solver.bodyWeight = 0f;
			ik.solver.Update();
			ik.solver.bodyWeight = bodyWeight;
			firstFBBIKSolve = false;
		}
	}
}

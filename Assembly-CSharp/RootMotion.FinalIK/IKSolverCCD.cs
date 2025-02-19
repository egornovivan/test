using System;
using UnityEngine;

namespace RootMotion.FinalIK;

[Serializable]
public class IKSolverCCD : IKSolverHeuristic
{
	public IterationDelegate OnPreIteration;

	public void FadeOutBoneWeights()
	{
		if (bones.Length >= 2)
		{
			bones[0].weight = 1f;
			float num = 1f / (float)(bones.Length - 1);
			for (int i = 1; i < bones.Length; i++)
			{
				bones[i].weight = num * (float)(bones.Length - 1 - i);
			}
		}
	}

	protected override void OnInitiate()
	{
		if (firstInitiation || !Application.isPlaying)
		{
			IKPosition = bones[bones.Length - 1].transform.position;
		}
		InitiateBones();
	}

	protected override void OnUpdate()
	{
		if (IKPositionWeight <= 0f)
		{
			return;
		}
		IKPositionWeight = Mathf.Clamp(IKPositionWeight, 0f, 1f);
		if (target != null)
		{
			IKPosition = target.position;
		}
		Vector3 vector = ((maxIterations <= 1) ? Vector3.zero : GetSingularityOffset());
		for (int i = 0; i < maxIterations && (!(vector == Vector3.zero) || i < 1 || !(tolerance > 0f) || !(base.positionOffset < tolerance * tolerance)); i++)
		{
			lastLocalDirection = localDirection;
			if (OnPreIteration != null)
			{
				OnPreIteration(i);
			}
			Solve(IKPosition + ((i != 0) ? Vector3.zero : vector));
		}
		lastLocalDirection = localDirection;
	}

	private void Solve(Vector3 targetPosition)
	{
		for (int num = bones.Length - 2; num > -1; num--)
		{
			Vector3 fromDirection = bones[bones.Length - 1].transform.position - bones[num].transform.position;
			Vector3 toDirection = targetPosition - bones[num].transform.position;
			Quaternion quaternion = Quaternion.FromToRotation(fromDirection, toDirection) * bones[num].transform.rotation;
			float num2 = bones[num].weight * IKPositionWeight;
			if (num2 >= 1f)
			{
				bones[num].transform.rotation = quaternion;
			}
			else
			{
				bones[num].transform.rotation = Quaternion.Lerp(bones[num].transform.rotation, quaternion, num2);
			}
			if (useRotationLimits && bones[num].rotationLimit != null)
			{
				bones[num].rotationLimit.Apply();
			}
		}
	}
}

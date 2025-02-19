using System;
using System.Collections;
using UnityEngine;

namespace RootMotion.FinalIK.Demos;

public class MotionAbsorb : MonoBehaviour
{
	[Serializable]
	public class Absorber
	{
		public FullBodyBipedEffector effector;

		public float weight = 1f;

		public void SetToBone(IKSolverFullBodyBiped solver)
		{
			solver.GetEffector(effector).position = solver.GetEffector(effector).bone.position;
			solver.GetEffector(effector).rotation = solver.GetEffector(effector).bone.rotation;
		}

		public void SetEffectorWeights(IKSolverFullBodyBiped solver, float w)
		{
			solver.GetEffector(effector).positionWeight = w * weight;
			solver.GetEffector(effector).rotationWeight = w * weight;
		}
	}

	public FullBodyBipedIK ik;

	public Absorber[] absorbers;

	public float weight = 1f;

	public AnimationCurve falloff;

	public float falloffSpeed = 1f;

	private float timer;

	private void OnCollisionEnter()
	{
		if (!(timer > 0f))
		{
			StartCoroutine(AbsorbMotion());
		}
	}

	private IEnumerator AbsorbMotion()
	{
		timer = 1f;
		for (int i = 0; i < absorbers.Length; i++)
		{
			absorbers[i].SetToBone(ik.solver);
		}
		while (timer > 0f)
		{
			timer -= Time.deltaTime * falloffSpeed;
			float w = falloff.Evaluate(timer);
			for (int j = 0; j < absorbers.Length; j++)
			{
				absorbers[j].SetEffectorWeights(ik.solver, w * weight);
			}
			yield return null;
		}
		yield return null;
	}
}

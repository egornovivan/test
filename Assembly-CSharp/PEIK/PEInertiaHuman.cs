using System;
using RootMotion.FinalIK;
using RootMotion.FinalIK.Demos;
using UnityEngine;

namespace PEIK;

public class PEInertiaHuman : OffsetModifier
{
	[Serializable]
	public class Body
	{
		[Serializable]
		public class EffectorLink
		{
			public FullBodyBipedEffector effector;

			public float weight;
		}

		public Transform transform;

		public EffectorLink[] effectorLinks;

		[Range(0.01f, 100f)]
		public float acceleration = 5f;

		public float matchVelocity = 0.3f;

		private Vector3 delta;

		private Vector3 lazyPoint;

		private Vector3 direction;

		private bool firstUpdate = true;

		public void Reset()
		{
			if (!(transform == null))
			{
				lazyPoint = transform.position;
				direction = Vector3.zero;
			}
		}

		public void Update(IKSolverFullBodyBiped solver, float weight, float deltaTime)
		{
			if (!(transform == null))
			{
				if (firstUpdate)
				{
					Reset();
					firstUpdate = false;
				}
				delta = transform.position - lazyPoint;
				Vector3 vector = delta / deltaTime;
				Vector3 vector2 = vector - direction;
				float a = vector2.magnitude / acceleration;
				a = Mathf.Min(a, deltaTime);
				direction += vector2.normalized * acceleration * a;
				lazyPoint += direction * deltaTime;
				lazyPoint += delta * matchVelocity;
				EffectorLink[] array = effectorLinks;
				foreach (EffectorLink effectorLink in array)
				{
					solver.GetEffector(effectorLink.effector).positionOffset += (lazyPoint - transform.position) * effectorLink.weight * weight;
				}
			}
		}
	}

	public Body[] bodies;

	protected override void OnModifyOffset()
	{
		Body[] array = bodies;
		foreach (Body body in array)
		{
			body.Update(ik.solver, weight, base.deltaTime);
		}
	}
}

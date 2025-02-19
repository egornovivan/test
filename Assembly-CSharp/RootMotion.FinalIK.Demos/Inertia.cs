using System;
using UnityEngine;

namespace RootMotion.FinalIK.Demos;

public class Inertia : OffsetModifier
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

		public float speed = 10f;

		public float acceleration = 3f;

		public float matchVelocity;

		public float gravity;

		private Vector3 delta;

		private Vector3 lazyPoint;

		private Vector3 direction;

		private Vector3 lastPosition;

		private bool firstUpdate = true;

		public void Reset()
		{
			if (!(transform == null))
			{
				lazyPoint = transform.position;
				lastPosition = transform.position;
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
				direction = Vector3.Lerp(direction, (transform.position - lazyPoint) / deltaTime * 0.01f, deltaTime * acceleration);
				lazyPoint += direction * deltaTime * speed;
				delta = transform.position - lastPosition;
				lazyPoint += delta * matchVelocity;
				lazyPoint.y += gravity * deltaTime;
				EffectorLink[] array = effectorLinks;
				foreach (EffectorLink effectorLink in array)
				{
					solver.GetEffector(effectorLink.effector).positionOffset += (lazyPoint - transform.position) * effectorLink.weight * weight;
				}
				lastPosition = transform.position;
			}
		}
	}

	public Body[] bodies;

	public OffsetLimits[] limits;

	public void ResetBodies()
	{
		Body[] array = bodies;
		foreach (Body body in array)
		{
			body.Reset();
		}
	}

	protected override void OnModifyOffset()
	{
		Body[] array = bodies;
		foreach (Body body in array)
		{
			body.Update(ik.solver, weight, base.deltaTime);
		}
		ApplyLimits(limits);
	}
}

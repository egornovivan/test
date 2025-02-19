using System.Collections.Generic;
using UnityEngine;

namespace Steer3D;

public class Flees : SteeringBehaviour
{
	public class FleeTarget
	{
		public bool active = true;

		public Vector3 target;

		public Transform targetTrans;

		public float weight = 1f;

		public float affectRadius = 5f;

		public float fleeRadius = 2f;

		public float forbiddenRadius = 0.2f;
	}

	public List<FleeTarget> targets = new List<FleeTarget>();

	public override bool idle
	{
		get
		{
			if (!active)
			{
				return true;
			}
			if (targets == null)
			{
				return true;
			}
			foreach (FleeTarget target in targets)
			{
				if (target.active)
				{
					Vector3 vector = ((!(target.targetTrans != null)) ? target.target : target.targetTrans.position);
					if ((vector - base.position).sqrMagnitude <= target.affectRadius * target.affectRadius)
					{
						return false;
					}
				}
			}
			return true;
		}
	}

	public override void Behave()
	{
		if (targets == null)
		{
			return;
		}
		foreach (FleeTarget target in targets)
		{
			if (target.targetTrans != null)
			{
				target.target = target.targetTrans.position;
			}
			if (target.active)
			{
				Vector3 vector = target.target - base.position;
				if (vector.magnitude <= target.affectRadius)
				{
					Vector3 desired_vel = -vector.normalized;
					float num = 0f;
					num = ((!(target.affectRadius > target.fleeRadius)) ? ((!(vector.magnitude > target.fleeRadius)) ? 1f : 0f) : Mathf.Clamp01(Mathf.InverseLerp(target.affectRadius, target.fleeRadius, vector.magnitude)));
					desired_vel *= num;
					agent.AddDesiredVelocity(desired_vel, target.weight, 0.75f);
				}
			}
		}
	}
}

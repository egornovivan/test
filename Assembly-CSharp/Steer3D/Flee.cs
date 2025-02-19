using UnityEngine;

namespace Steer3D;

public class Flee : SteeringBehaviour
{
	public Vector3 target;

	public Transform targetTrans;

	public float weight = 1f;

	public float affectRadius = 5f;

	public float fleeRadius = 2f;

	public float forbiddenRadius = 0.2f;

	public override bool idle => !active || (target - base.position).sqrMagnitude >= affectRadius * affectRadius;

	public override void Behave()
	{
		if (targetTrans != null)
		{
			target = targetTrans.position;
		}
		if (!idle)
		{
			Vector3 vector = target - base.position;
			Vector3 desired_vel = -vector.normalized;
			float num = 0f;
			num = ((!(affectRadius > fleeRadius)) ? ((!(vector.magnitude > fleeRadius)) ? 1f : 0f) : Mathf.Clamp01(Mathf.InverseLerp(affectRadius, fleeRadius, vector.magnitude)));
			desired_vel *= num;
			agent.AddDesiredVelocity(desired_vel, weight, 0.75f);
		}
	}
}

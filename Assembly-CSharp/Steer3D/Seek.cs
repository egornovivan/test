using UnityEngine;

namespace Steer3D;

public class Seek : SteeringBehaviour
{
	public Vector3 target;

	public Transform targetTrans;

	public float weight = 1f;

	public float slowingRadius = 5f;

	public float arriveRadius = 0.2f;

	public override bool idle => !active || (target - base.position).sqrMagnitude < arriveRadius * arriveRadius;

	public override void Behave()
	{
		if (targetTrans != null)
		{
			target = targetTrans.position;
		}
		if (!idle)
		{
			Vector3 vector = target - base.position;
			Vector3 normalized = vector.normalized;
			float num = 1f;
			num = ((!(slowingRadius > arriveRadius)) ? ((!(vector.magnitude > arriveRadius)) ? 0f : 1f) : Mathf.Clamp01(Mathf.InverseLerp(arriveRadius, slowingRadius, vector.magnitude)));
			normalized *= num;
			agent.AddDesiredVelocity(normalized, weight, 0.75f);
		}
	}
}

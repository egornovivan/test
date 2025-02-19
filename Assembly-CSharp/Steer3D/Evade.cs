using UnityEngine;

namespace Steer3D;

public class Evade : SteeringBehaviour
{
	private const int history_count = 8;

	public Vector3 target;

	public Transform targetTrans;

	public float weight = 1f;

	public float affectRadius = 5f;

	public float fleeRadius = 2f;

	public float forbiddenRadius = 0.2f;

	private Vector3[] history_target = new Vector3[8];

	private float[] history_time = new float[8];

	private int history_cursor;

	public override bool idle => !active || (target - base.position).sqrMagnitude >= affectRadius * affectRadius;

	public override void Behave()
	{
		Vector3 vector = Vector3.zero;
		bool flag = false;
		if (targetTrans != null)
		{
			target = targetTrans.position;
			SteerAgent component = targetTrans.GetComponent<SteerAgent>();
			if (component != null)
			{
				flag = true;
				vector = component.velocity * component.maxSpeed;
			}
		}
		if (idle)
		{
			return;
		}
		if (!flag)
		{
			for (int num = 7; num > 0; num--)
			{
				ref Vector3 reference = ref history_target[num];
				reference = history_target[num - 1];
			}
			for (int num2 = 7; num2 > 0; num2--)
			{
				history_time[num2] = history_time[num2 - 1];
			}
			ref Vector3 reference2 = ref history_target[0];
			reference2 = target;
			history_time[0] = Time.time;
			history_cursor++;
			if (history_cursor > 8)
			{
				history_cursor = 8;
			}
			int num3 = 0;
			Vector3 zero = Vector3.zero;
			for (int i = 0; i < history_cursor - 1; i++)
			{
				Vector3 vector2 = history_target[i] - history_target[i + 1];
				float value = history_time[i] - history_time[i + 1];
				value = Mathf.Clamp(value, 0.001f, 0.2f);
				if (!(vector2.magnitude > 10f))
				{
					Vector3 vector3 = vector2 / value;
					zero += vector3;
					num3++;
				}
			}
			if (num3 > 0)
			{
				vector = zero / num3;
			}
		}
		Vector3 vector4 = target - base.position;
		Vector3 vector5 = target + vector * (vector4.magnitude / agent.maxSpeed);
		vector4 = vector5 - base.position;
		Vector3 desired_vel = -vector4.normalized;
		float num4 = 0f;
		num4 = ((!(affectRadius > fleeRadius)) ? ((!(vector4.magnitude > fleeRadius)) ? 1f : 0f) : Mathf.Clamp01(Mathf.InverseLerp(affectRadius, fleeRadius, vector4.magnitude)));
		desired_vel *= num4;
		agent.AddDesiredVelocity(desired_vel, weight, 0.75f);
	}
}

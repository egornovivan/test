using System;
using AiAsset;
using UnityEngine;

public class TraceSun : MonoBehaviour
{
	public Transform chassis;

	public Transform pitchPivot;

	private float pitchAngle = 60f;

	private Vector3 aimDirection = Vector3.zero;

	private Vector3 SunDirection()
	{
		return Vector3.up;
	}

	private void Update()
	{
		UpdateRotation();
	}

	private void UpdateRotation()
	{
		Vector3 vector = SunDirection();
		if (vector == Vector3.zero || vector.y < 0f)
		{
			return;
		}
		if (chassis != null)
		{
			Vector3 vector2 = vector;
			vector2.y = 0f;
			Vector3 b = vector2;
			aimDirection = Vector3.Slerp(aimDirection, b, 15f * Time.deltaTime);
			if (aimDirection != Vector3.zero)
			{
				Vector3 forward = chassis.forward;
				forward.y = 0f;
				Quaternion quaternion = Quaternion.FromToRotation(forward, aimDirection);
				chassis.rotation *= quaternion;
			}
		}
		if (pitchPivot != null)
		{
			Vector3 b2 = AiMath.ProjectOntoPlane(vector, pitchPivot.up);
			if (Vector3.Dot(b2.normalized, pitchPivot.forward) > Mathf.Cos((float)Math.PI / 180f * pitchAngle))
			{
				Vector3 toDirection = Vector3.Slerp(pitchPivot.forward, b2, 15f * Time.deltaTime);
				Quaternion quaternion2 = Quaternion.FromToRotation(pitchPivot.forward, toDirection);
				pitchPivot.rotation = quaternion2 * pitchPivot.rotation;
			}
		}
	}
}

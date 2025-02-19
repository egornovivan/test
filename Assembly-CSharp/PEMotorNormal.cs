using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class PEMotorNormal : PEMotor
{
	public float maxRotationSpeed = 270f;

	private bool firstframe = true;

	private void UpdateFacingDirection()
	{
		float magnitude = base.desiredFacingDirection.magnitude;
		Vector3 v = base.transform.rotation * base.desiredMovementDirection * (1f - magnitude) + base.desiredFacingDirection * magnitude;
		v = Util.ProjectOntoPlane(v, base.transform.up);
		v = alignCorrection * v;
		if (v.sqrMagnitude > 0.01f)
		{
			Vector3 v2 = Util.ConstantSlerp(base.transform.forward, v, maxRotationSpeed * Time.deltaTime);
			v2 = Util.ProjectOntoPlane(v2, base.transform.up);
			Quaternion rotation = default(Quaternion);
			rotation.SetLookRotation(v2, base.transform.up);
			base.transform.rotation = rotation;
		}
	}

	private void UpdateVelocity()
	{
		CharacterController characterController = GetComponent(typeof(CharacterController)) as CharacterController;
		Vector3 vector = characterController.velocity;
		if (firstframe)
		{
			vector = Vector3.zero;
			firstframe = false;
		}
		if (base.grounded)
		{
			vector = Util.ProjectOntoPlane(vector, base.transform.up);
		}
		Vector3 vector2 = vector;
		base.jumping = false;
		if (base.grounded)
		{
			Vector3 vector3 = base.desiredVelocity - vector;
			if (vector3.magnitude > maxVelocityChange)
			{
				vector3 = vector3.normalized * maxVelocityChange;
			}
			vector2 += vector3;
		}
		float num = 1f;
		AlignmentTracker component = GetComponent<AlignmentTracker>();
		if (Mathf.Abs(component.velocitySmoothed.y) > num)
		{
			vector2 *= Mathf.Max(0f, Mathf.Abs(num / component.velocitySmoothed.y));
		}
		vector2 += base.transform.up * (0f - gravity) * Time.deltaTime;
		if (base.jumping)
		{
			vector2 -= base.transform.up * (0f - gravity) * Time.deltaTime / 2f;
		}
		CollisionFlags collisionFlags = characterController.Move(vector2 * Time.deltaTime);
		base.grounded = (collisionFlags & CollisionFlags.Below) != 0;
	}

	private void Update()
	{
		if (Time.deltaTime != 0f && Time.timeScale != 0f)
		{
			UpdateFacingDirection();
			UpdateVelocity();
		}
	}
}

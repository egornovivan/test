using UnityEngine;

public class AiGunAimer : MonoBehaviour
{
	public Transform aimPivot;

	public Transform aimWeapon;

	public Transform aimTarget;

	public float heightOff;

	public float effect = 1f;

	private Vector3 aimDirection = Vector3.zero;

	private LayerMask mask;

	private float m_startTime;

	private float m_dstEffect;

	public float dstEffect
	{
		set
		{
			if (Mathf.Abs(m_dstEffect - value) > 0.1f)
			{
				m_startTime = Time.time;
			}
			m_dstEffect = value;
		}
	}

	private void Start()
	{
		mask = 1 << base.gameObject.layer;
		mask = (int)mask | (1 << (LayerMask.NameToLayer("Ignore Raycast") & 0x1F));
		mask = ~(int)mask;
	}

	protected void LateUpdate()
	{
		effect = Mathf.Lerp(1f - m_dstEffect, m_dstEffect, (Time.time - m_startTime) * 0.75f);
		if (!(effect <= float.Epsilon))
		{
			Vector3 position = aimWeapon.position;
			Quaternion rotation = aimWeapon.rotation;
			Vector3 position2 = aimPivot.position;
			Transform transform = base.transform;
			Vector3 vector = Quaternion.Inverse(transform.rotation) * (aimWeapon.position - position2);
			Vector3 zero = Vector3.zero;
			zero = (aimDirection = Vector3.Slerp(b: (!(aimTarget == null)) ? (Quaternion.Inverse(transform.rotation) * (aimTarget.position + Vector3.up * heightOff - position2)) : (Quaternion.Inverse(transform.rotation) * transform.forward), a: aimDirection, t: 10f * Time.deltaTime));
			Quaternion rotation2 = Quaternion.FromToRotation(vector, aimDirection);
			RotateTransformAroundPointInOtherTransformSpace(aimWeapon, rotation2, position2, transform);
			float num = 0f;
			Vector3 from = vector;
			Vector3 to = zero;
			from.y = 0f;
			to.y = 0f;
			float num2 = Vector3.Angle(from, to);
			num = 1f - num2 / 400f;
			aimWeapon.position = position2 + (aimWeapon.position - position2) * num;
			aimWeapon.rotation = Quaternion.FromToRotation(-aimWeapon.right, aimWeapon.position - position2) * aimWeapon.rotation;
			if (effect <= 1f)
			{
				aimWeapon.position = Vector3.Lerp(position, aimWeapon.position, effect);
				aimWeapon.rotation = Quaternion.Slerp(rotation, aimWeapon.rotation, effect);
			}
		}
	}

	private void RotateTransformAroundPointInOtherTransformSpace(Transform toRotate, Quaternion rotation, Vector3 pivot, Transform space)
	{
		Vector3 vector = toRotate.position - pivot;
		Vector3 vector2 = -vector + space.rotation * (rotation * (Quaternion.Inverse(space.rotation) * vector));
		toRotate.position += vector2;
		toRotate.rotation = space.rotation * rotation * Quaternion.Inverse(space.rotation) * toRotate.rotation;
	}
}

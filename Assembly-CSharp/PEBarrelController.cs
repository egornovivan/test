using System;
using PETools;
using UnityEngine;

public class PEBarrelController : MonoBehaviour
{
	[Serializable]
	public class Chassis
	{
		public Transform chassis;

		public Vector3 pivot = Vector3.forward;

		public float angle;
	}

	[Serializable]
	public class Pitch
	{
		public Transform pitch;

		public Vector3 pivot = Vector3.forward;

		public Vector3 pivotAxis = Vector3.right;

		public float minAngle;

		public float maxAngle;
	}

	public Transform emitter;

	public Vector3 axis = Vector3.forward;

	public Chassis chassis;

	public Pitch pitch;

	private bool m_AimChassis;

	private bool m_AimPitch;

	private int m_Layer;

	private float m_AimStartTime;

	private Transform m_AimTarget;

	private Bounds m_LocalBounds;

	private Vector3 m_AimPosition;

	public Transform AimTarget
	{
		get
		{
			return m_AimTarget;
		}
		set
		{
			if (m_AimTarget != value)
			{
				m_AimTarget = value;
				m_AimStartTime = Time.time;
			}
		}
	}

	public Vector3 AimPosition
	{
		get
		{
			return m_AimPosition;
		}
		set
		{
			m_AimPosition = value;
		}
	}

	public bool Aim
	{
		set
		{
			base.enabled = value;
		}
	}

	public float ChassisY => chassis.chassis.rotation.eulerAngles.y;

	public Vector3 PitchEuler => pitch.pitch.rotation.eulerAngles;

	public bool IsAimed => m_AimChassis && m_AimPitch;

	public void ApplyChassis(float rotY)
	{
		if (chassis != null && null != chassis.chassis)
		{
			Vector3 eulerAngles = chassis.chassis.rotation.eulerAngles;
			eulerAngles.y = rotY;
			chassis.chassis.rotation = Quaternion.Euler(eulerAngles);
		}
	}

	public void ApplyPitchEuler(Vector3 angleEuler)
	{
		if (pitch != null && null != pitch.pitch)
		{
			pitch.pitch.rotation = Quaternion.Euler(angleEuler);
		}
	}

	public bool Angle(Vector3 position, float angle)
	{
		if (chassis.chassis != null && position != Vector3.zero)
		{
			Vector3 from = chassis.chassis.TransformDirection(chassis.pivot);
			from.y = 0f;
			Vector3 to = position - chassis.chassis.position;
			to.y = 0f;
			return Vector3.Angle(from, to) <= angle;
		}
		return false;
	}

	public bool PitchAngle(Vector3 position, float angle)
	{
		if (pitch.pitch != null && position != Vector3.zero)
		{
			Vector3 from = new Vector3(0f, pitch.pitch.TransformDirection(pitch.pivot).y, 0f);
			Vector3 to = new Vector3(0f, (position - pitch.pitch.position).y, 0f);
			if (to.sqrMagnitude < 0.0025000002f)
			{
				return true;
			}
			return Vector3.Angle(from, to) <= angle;
		}
		return false;
	}

	public bool EstimatedAttack(Vector3 position, Transform target = null)
	{
		Vector3 vector = base.transform.TransformPoint(m_LocalBounds.center);
		float maxDistance = Vector3.Distance(vector, position);
		RaycastHit hitInfo;
		return !Physics.Raycast(vector, position - vector, out hitInfo, maxDistance, m_Layer) || target == null || hitInfo.transform.IsChildOf(target);
	}

	public bool CanAttack(Vector3 position, Transform target = null)
	{
		if (emitter == null)
		{
			return true;
		}
		float maxDistance = Vector3.Distance(emitter.position, position);
		Vector3 direction = emitter.TransformDirection(axis);
		if (!Physics.Raycast(emitter.position, direction, out var hitInfo, maxDistance, m_Layer))
		{
			return true;
		}
		if (target == null || hitInfo.transform.IsChildOf(target))
		{
			return true;
		}
		if (hitInfo.collider.gameObject.layer == 4)
		{
			return true;
		}
		return false;
	}

	public bool Evaluate(Vector3 position)
	{
		Quaternion quaternion = Quaternion.identity;
		Quaternion quaternion2 = Quaternion.identity;
		if (chassis.chassis != null)
		{
			Vector3 direction = position - chassis.chassis.position;
			Transform transform = chassis.chassis;
			Vector3 vector = transform.InverseTransformDirection(direction);
			Vector3 vector2 = transform.InverseTransformDirection(transform.TransformDirection(chassis.pivot));
			vector2 = Vector3.ProjectOnPlane(vector2, Vector3.up);
			vector = Vector3.ProjectOnPlane(vector, Vector3.up);
			quaternion = Quaternion.FromToRotation(vector2, vector);
			transform.rotation = quaternion * transform.rotation;
		}
		if (pitch.pitch != null)
		{
			Transform transform2 = pitch.pitch;
			Vector3 vector3 = position - pitch.pitch.position;
			Vector3 planeNormal = transform2.TransformDirection(pitch.pivotAxis);
			Vector3 fromDirection = transform2.TransformDirection(pitch.pivot);
			Vector3 toDirection = Vector3.ProjectOnPlane(vector3, planeNormal);
			quaternion2 = Quaternion.FromToRotation(fromDirection, toDirection);
			transform2.rotation = quaternion2 * transform2.rotation;
		}
		Vector3 position2 = emitter.position;
		Vector3 direction2 = emitter.TransformDirection(axis);
		float maxDistance = Vector3.Distance(position2, position);
		if (chassis.chassis != null)
		{
			chassis.chassis.rotation = Quaternion.Inverse(quaternion) * chassis.chassis.rotation;
		}
		if (pitch.pitch != null)
		{
			pitch.pitch.rotation = Quaternion.Inverse(quaternion2) * pitch.pitch.rotation;
		}
		return !Physics.Raycast(emitter.position, direction2, maxDistance, m_Layer);
	}

	private bool ChassisRotation()
	{
		if (chassis.chassis == null)
		{
			return true;
		}
		if (m_AimTarget == null && m_AimPosition == Vector3.zero)
		{
			return false;
		}
		Vector3 vector = ((!(m_AimTarget != null)) ? m_AimPosition : m_AimTarget.position);
		if (PEUtil.SqrMagnitude(vector, chassis.chassis.position, is3D: false) < 0.25f)
		{
			return false;
		}
		Vector3 direction = vector - chassis.chassis.position;
		Transform transform = chassis.chassis;
		Vector3 vector2 = transform.InverseTransformDirection(direction);
		Vector3 vector3 = transform.InverseTransformDirection(transform.TransformDirection(chassis.pivot));
		vector2 = Vector3.ProjectOnPlane(vector2, Vector3.up);
		vector3 = Vector3.ProjectOnPlane(vector3, Vector3.up);
		float num = Mathf.Lerp(90f, 270f, (Time.time - m_AimStartTime) * 0.5f);
		Vector3 toDirection = Util.ConstantSlerp(vector3, vector2, num * Time.deltaTime);
		transform.rotation = Quaternion.FromToRotation(vector3, toDirection) * transform.rotation;
		return Vector3.Angle(vector3, vector2) < 5f;
	}

	private bool PitchRotation()
	{
		if (pitch.pitch == null)
		{
			return true;
		}
		if (m_AimTarget == null && m_AimPosition == Vector3.zero)
		{
			return false;
		}
		Vector3 vector = ((!(m_AimTarget != null)) ? m_AimPosition : m_AimTarget.position);
		if (PEUtil.SqrMagnitude(vector, pitch.pitch.position, is3D: false) < 0.25f)
		{
			return false;
		}
		Vector3 vector2 = vector - pitch.pitch.position;
		Transform transform = pitch.pitch;
		Vector3 planeNormal = transform.TransformDirection(pitch.pivotAxis);
		Vector3 vector3 = transform.TransformDirection(pitch.pivot);
		Vector3 vector4 = Vector3.ProjectOnPlane(vector2, planeNormal);
		Vector3 toDirection = Vector3.Slerp(vector3, vector4, 270f * Time.deltaTime);
		transform.rotation = Quaternion.FromToRotation(vector3, toDirection) * transform.rotation;
		return Vector3.Angle(vector3, vector4) < 5f;
	}

	private void Awake()
	{
		m_Layer = 2177024;
		m_LocalBounds = PEUtil.GetLocalColliderBoundsInChildren(base.gameObject);
	}

	private void LateUpdate()
	{
		m_AimChassis = ChassisRotation();
		if (!m_AimChassis)
		{
			m_AimPitch = false;
		}
		else
		{
			m_AimPitch = PitchRotation();
		}
	}
}

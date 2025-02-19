using System;
using Pathea;
using UnityEngine;

public class Trajectory : MonoBehaviour
{
	[SerializeField]
	protected Transform m_Caster;

	[SerializeField]
	protected Transform m_Emitter;

	[SerializeField]
	protected Transform m_Target;

	[SerializeField]
	protected Vector3 m_TargetPosition;

	[SerializeField]
	private bool m_InitRot;

	protected int m_Index;

	private BiologyViewCmpt m_TargetView;

	private PeTrans m_TargetTrans;

	private PEDefenceTrigger m_DefenceTrigger;

	private Vector3 m_MoveVector;

	protected Vector3 m_Velocity;

	public bool rayCast;

	public Vector3 moveVector
	{
		get
		{
			return m_MoveVector;
		}
		set
		{
			m_MoveVector = value;
		}
	}

	public Transform target
	{
		set
		{
			m_Target = value;
		}
	}

	public bool isActive { get; set; }

	public virtual Vector3 Track(float deltaTime)
	{
		return Vector3.zero;
	}

	public virtual Quaternion Rotate(float deltaTime)
	{
		return base.transform.rotation;
	}

	public virtual void SetData(Transform caster, Transform emitter, Transform target, Vector3 targetPosition, int index = 0)
	{
		m_Caster = caster;
		m_Emitter = emitter;
		m_Target = target;
		m_TargetPosition = targetPosition;
		m_Index = index;
		if (m_Target != null)
		{
			m_TargetView = m_Target.GetComponentInParent<BiologyViewCmpt>();
			m_TargetTrans = m_Target.GetComponentInParent<PeTrans>();
			if (null != m_TargetView)
			{
				m_DefenceTrigger = m_TargetView.GetComponentInChildren<PEDefenceTrigger>();
			}
		}
		if (m_InitRot)
		{
			base.transform.rotation = Quaternion.identity;
		}
		isActive = true;
	}

	protected virtual bool Overlook(Collider c)
	{
		if (c == null || c.isTrigger)
		{
			return true;
		}
		if (c.tag == "WorldCollider")
		{
			return true;
		}
		if (c.transform.IsChildOf(base.transform))
		{
			return true;
		}
		if (m_Emitter != null && c.transform.IsChildOf(m_Emitter))
		{
			return true;
		}
		return false;
	}

	protected Vector3 GetPredictPosition(Transform target, Vector3 startPos, float startSpeed)
	{
		Vector3 targetCenter = GetTargetCenter(target);
		Vector3 targetVeloctiy = GetTargetVeloctiy(target);
		float sqrMagnitude = targetVeloctiy.sqrMagnitude;
		if (sqrMagnitude < 0.05f)
		{
			Vector3 from = startPos - targetCenter;
			float num = Mathf.Sqrt(startSpeed * startSpeed - sqrMagnitude);
			float num2 = Mathf.Cos(Vector3.Angle(from, targetVeloctiy) / 180f * (float)Math.PI);
			float num3 = from.sqrMagnitude * sqrMagnitude * num2 * num2;
			float num4 = ((!(Vector3.Angle(from, targetVeloctiy) <= 90f)) ? ((Mathf.Sqrt(from.sqrMagnitude + num3 / num / num) + Mathf.Sqrt(num3 / num / num)) / num) : ((Mathf.Sqrt(from.sqrMagnitude + num3 / num / num) - Mathf.Sqrt(num3 / num / num)) / num));
			return targetCenter + targetVeloctiy * num4;
		}
		return targetCenter;
	}

	protected Vector3 GetTargetCenter(Transform target = null)
	{
		if (m_Target != null)
		{
			if (null != m_DefenceTrigger && null != m_DefenceTrigger.centerBone)
			{
				return m_DefenceTrigger.centerBone.position;
			}
			if (m_TargetView != null && m_TargetView.centerTransform != null)
			{
				return m_TargetView.centerTransform.position;
			}
			if (m_TargetTrans != null)
			{
				return m_TargetTrans.center;
			}
			return m_Target.position;
		}
		return m_TargetPosition;
	}

	protected Vector3 GetTargetVeloctiy(Transform target)
	{
		if (target == null)
		{
			return Vector3.zero;
		}
		Motion_Move componentInParent = target.GetComponentInParent<Motion_Move>();
		if (componentInParent != null)
		{
			return componentInParent.velocity;
		}
		return Vector3.zero;
	}
}

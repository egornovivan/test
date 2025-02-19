using RootMotion.FinalIK;
using UnityEngine;

[RequireComponent(typeof(AimIK))]
public class IKFlashLight : MonoBehaviour
{
	private AimIK m_AimIK;

	public Transform m_Root;

	[Range(0f, 180f)]
	public float m_AngleThreshold = 40f;

	public bool m_Active;

	public float m_FadeTime = 0.5f;

	private float m_Radius = 10f;

	private Vector3 m_TargetPos = Vector3.zero;

	public Transform aimTrans
	{
		get
		{
			return m_AimIK.solver.transform;
		}
		set
		{
			m_AimIK.solver.transform = value;
		}
	}

	public Vector3 targetPos
	{
		get
		{
			return m_AimIK.solver.IKPosition;
		}
		set
		{
			m_TargetPos = value;
		}
	}

	private void Awake()
	{
		m_AimIK = GetComponent<AimIK>();
	}

	private void UpdateActiveState()
	{
		if (null == aimTrans)
		{
			m_AimIK.solver.IKPositionWeight = 0f;
		}
		else if (m_Active)
		{
			if (m_AimIK.solver.IKPositionWeight < 1f)
			{
				m_AimIK.solver.IKPositionWeight = Mathf.Clamp01(m_AimIK.solver.IKPositionWeight + Time.deltaTime / m_FadeTime);
			}
		}
		else if (m_AimIK.solver.IKPositionWeight > 0f)
		{
			m_AimIK.solver.IKPositionWeight = Mathf.Clamp01(m_AimIK.solver.IKPositionWeight - Time.deltaTime / m_FadeTime);
		}
	}

	private void UpdateIKTarget()
	{
		if (!m_Active || null == aimTrans)
		{
			return;
		}
		if (null != m_Root)
		{
			Vector3 vector = m_TargetPos - aimTrans.position;
			if (m_TargetPos == Vector3.zero)
			{
				vector = m_Root.forward;
			}
			vector.Normalize();
			float num = Vector3.Angle(vector, m_Root.forward);
			if (num > m_AngleThreshold)
			{
				vector = Vector3.Slerp(vector, m_Root.forward, (num - m_AngleThreshold) / num);
			}
			vector.Normalize();
			m_AimIK.solver.IKPosition = aimTrans.position + vector * m_Radius;
		}
		else
		{
			m_Active = false;
		}
	}

	private void Update()
	{
		UpdateActiveState();
		UpdateIKTarget();
	}
}

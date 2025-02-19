using RootMotion.FinalIK;
using UnityEngine;

[RequireComponent(typeof(AimIK))]
public class IKAimCtrl : MonoBehaviour
{
	public float m_Weight = 1f;

	public Transform m_Target;

	public Transform m_Root;

	public Transform m_DetectorCenter;

	private float m_DetectorRadius = 100f;

	private float m_MinDis = 5f;

	private Vector3 m_DefaultAxis;

	[Range(0f, 180f)]
	public float m_DetectorAngle = 80f;

	public float m_LerpSpeed = 3f;

	[Range(0.01f, 5f)]
	public float m_FadeInTime = 0.5f;

	public float m_FadeOutTime = 0.05f;

	private float m_FadeWeight;

	private AimIK m_AimIK;

	private FullBodyBipedIK m_FBBIK;

	private Vector3 m_LastDir;

	private Vector3 m_TargetDir;

	private Vector3 m_AimPos;

	private Vector3 m_UpdatedAimPos;

	public Vector3 m_TargetPosOffset;

	private Vector3 m_ScaledOffset;

	private bool m_Active;

	[Range(0.01f, 5f)]
	public float m_SyncAimAxieFadeTime = 0.15f;

	private float m_SyncAimAxieWeight;

	private bool m_SyncAimAxie;

	private Transform m_FollowTrans;

	private Transform m_ModelTran;

	private Quaternion m_LocalRot;

	private bool m_InRange;

	private bool m_UseSyncTarget;

	public float crossDir = 1f;

	public Vector3 targetPos
	{
		get
		{
			if (null != m_AimIK)
			{
				Vector3 vector = m_AimPos;
				if (null != m_Target)
				{
					vector = m_UpdatedAimPos;
				}
				return vector + m_ScaledOffset;
			}
			return base.transform.position + base.transform.forward;
		}
		set
		{
			m_AimPos = value;
		}
	}

	public bool active => m_Active;

	public bool aimed => m_FadeWeight < 0.9f || m_InRange;

	public Ray aimRay => new Ray(m_AimIK.solver.transform.position, targetPos - m_TargetPosOffset - m_AimIK.solver.transform.position);

	private void Awake()
	{
		m_AimIK = GetComponent<AimIK>();
		m_FBBIK = GetComponent<FullBodyBipedIK>();
		m_ModelTran = base.transform.parent.GetComponentInChildren<PEModelController>().transform;
		m_DetectorCenter = m_AimIK.solver.transform;
		m_DefaultAxis = m_AimIK.solver.axis;
	}

	private void Start()
	{
		if (null == m_Root)
		{
			PEModelController componentInChildren = base.transform.parent.GetComponentInChildren<PEModelController>();
			if (null != componentInChildren)
			{
				m_Root = componentInChildren.transform;
			}
		}
	}

	private void LateUpdate()
	{
		UpdateSyncAimAxie();
		UpdateIKPos();
	}

	public void SetTarget(Transform target)
	{
		m_Target = target;
	}

	public void SetActive(bool active)
	{
		m_Active = active;
	}

	public void SetSmoothMoveState(bool smoothMove)
	{
		if (null != m_AimIK && m_AimIK.solver != null)
		{
			m_AimIK.solver.clampSmoothing = (smoothMove ? 2 : 0);
		}
	}

	private void UpdateIKPos()
	{
		m_AimIK.solver.IKPositionWeight = m_Weight * m_FadeWeight;
		m_FadeWeight = Mathf.Clamp01(m_FadeWeight + ((!m_Active) ? (-1f) : 1f) * Time.deltaTime / m_FadeInTime);
		m_UpdatedAimPos = ((!(null == m_Target)) ? m_Target.position : m_AimPos);
		if (!m_Active || !(null != m_DetectorCenter))
		{
			return;
		}
		if (null == m_Target)
		{
			m_TargetDir = m_AimPos - m_DetectorCenter.position;
		}
		else
		{
			m_TargetDir = m_Target.position - m_DetectorCenter.position;
			m_UpdatedAimPos = m_Target.position;
		}
		if (Vector3.zero == m_TargetDir && null != m_Root)
		{
			m_TargetDir = m_Root.forward;
		}
		m_InRange = true;
		if (null != m_Root)
		{
			float num = Vector3.Angle(m_TargetDir, m_Root.forward);
			if (num > m_DetectorAngle)
			{
				m_TargetDir = Vector3.Slerp(m_TargetDir, m_Root.forward, (num - m_DetectorAngle) / num);
				m_InRange = false;
			}
		}
		float num2 = Mathf.Clamp(m_TargetDir.magnitude, m_MinDis, m_DetectorRadius);
		m_ScaledOffset = m_TargetPosOffset * num2 / m_DetectorRadius;
		m_TargetDir.Normalize();
		m_TargetDir = Vector3.Slerp(m_LastDir, m_TargetDir, m_LerpSpeed * Time.deltaTime);
		if (!m_UseSyncTarget)
		{
			m_AimIK.solver.IKPosition = m_DetectorCenter.position + m_TargetDir * num2 + m_ScaledOffset;
		}
		m_LastDir = m_TargetDir;
	}

	public void SetAimTran(Transform aimTran)
	{
		if (null != m_FollowTrans)
		{
			m_FollowTrans.localRotation = m_LocalRot;
			m_FollowTrans = null;
		}
		if (null == aimTran)
		{
			m_AimIK.solver.transform = m_DetectorCenter;
			m_AimIK.solver.axis = m_DefaultAxis;
			return;
		}
		m_AimIK.solver.transform = aimTran;
		m_AimIK.solver.axis = Vector3.forward;
		m_LocalRot = aimTran.localRotation;
		m_FollowTrans = aimTran;
	}

	public void StartSyncAimAxie()
	{
		m_SyncAimAxie = true;
	}

	public void EndSyncAimAxie()
	{
		m_SyncAimAxie = false;
	}

	private void UpdateSyncAimAxie()
	{
		m_UseSyncTarget = false;
		m_SyncAimAxieWeight = Mathf.Clamp01(m_SyncAimAxieWeight + ((!m_SyncAimAxie) ? (-1f) : 1f) * Time.deltaTime / m_SyncAimAxieFadeTime);
		if (m_SyncAimAxieWeight > 0f)
		{
			if (m_SyncAimAxie)
			{
				if (null != m_ModelTran)
				{
					if (null != m_FollowTrans)
					{
						m_FollowTrans.rotation = Quaternion.Slerp(m_FollowTrans.rotation, Quaternion.LookRotation(m_ModelTran.forward), m_SyncAimAxieWeight);
						return;
					}
					m_UseSyncTarget = true;
					float num = Vector3.Angle(m_TargetDir, Vector3.up);
					Vector3 vector = m_DetectorCenter.rotation * m_DefaultAxis;
					float angle = Vector3.Angle(vector, Vector3.up) + num - 90f;
					vector = Quaternion.AngleAxis(angle, crossDir * Vector3.Cross(Vector3.up, vector)) * Vector3.up;
					m_AimIK.solver.IKPosition = m_DetectorCenter.position + vector.normalized * m_MinDis;
				}
			}
			else if (null != m_FollowTrans)
			{
				m_FollowTrans.localRotation = Quaternion.Slerp(m_LocalRot, m_FollowTrans.localRotation, m_SyncAimAxieWeight);
			}
		}
		else if (null != m_FollowTrans)
		{
			m_AimIK.solver.axis = Vector3.forward;
		}
		else
		{
			m_AimIK.solver.axis = m_DefaultAxis;
		}
	}
}

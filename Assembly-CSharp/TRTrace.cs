using UnityEngine;

public class TRTrace : Trajectory
{
	public float lerpValue;

	public float speed;

	public float angle;

	public float deviation;

	public bool isTrace;

	private Vector3 m_CurMoveDir;

	private Vector3 m_OldCenter;

	private Vector3 m_Deviation;

	private bool m_CanTrace;

	private void Start()
	{
		m_CanTrace = true;
		m_OldCenter = GetTargetCenter();
		m_Deviation = Random.insideUnitSphere.normalized * Random.Range(0f, deviation);
		m_CurMoveDir = GetTargetPosition() - base.transform.position;
		base.transform.rotation = Quaternion.LookRotation(m_CurMoveDir);
		if (m_Index > 0)
		{
			m_CurMoveDir = Quaternion.AngleAxis(Random.Range(0f, 360f), base.transform.forward) * base.transform.right;
			Vector3 axis = Vector3.Cross(m_CurMoveDir, base.transform.forward);
			m_CurMoveDir = Quaternion.AngleAxis(Random.Range(0f, 90f), axis) * m_CurMoveDir;
		}
	}

	private Vector3 GetTargetPosition()
	{
		if (isTrace)
		{
			return GetTargetCenter() + m_Deviation;
		}
		return m_OldCenter + m_Deviation;
	}

	public override Vector3 Track(float deltaTime)
	{
		Vector3 vector = GetTargetPosition() - base.transform.position;
		if (Vector3.Angle(m_CurMoveDir, vector) <= angle)
		{
			m_CurMoveDir = Vector3.Slerp(m_CurMoveDir, vector, lerpValue * deltaTime);
		}
		return m_CurMoveDir.normalized * speed * deltaTime;
	}

	public override Quaternion Rotate(float deltaTime)
	{
		return Quaternion.LookRotation(m_CurMoveDir);
	}
}

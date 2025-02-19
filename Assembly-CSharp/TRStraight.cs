using UnityEngine;

public class TRStraight : Trajectory
{
	[SerializeField]
	private float speed;

	[SerializeField]
	private bool towardsTarget;

	public override void SetData(Transform caster, Transform emitter, Transform target, Vector3 targetPosition, int index)
	{
		base.SetData(caster, emitter, target, targetPosition, index);
		Emit();
	}

	public void Emit()
	{
		if (!towardsTarget && null != m_Emitter)
		{
			m_Velocity = m_Emitter.forward;
		}
		else
		{
			m_Velocity = ((!(null != m_Target)) ? m_TargetPosition : GetTargetCenter(m_Target)) - base.transform.position;
			m_Velocity.Normalize();
		}
		base.transform.rotation = Quaternion.LookRotation(m_Velocity, (!(null != m_Emitter)) ? Vector3.up : m_Emitter.up);
		m_Velocity *= speed;
	}

	public override Vector3 Track(float deltaTime)
	{
		return m_Velocity * deltaTime;
	}
}

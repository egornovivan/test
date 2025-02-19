using UnityEngine;

public class TRShotgun : Trajectory
{
	public float speed;

	public float maxAngle;

	private Vector3 vertical = Vector3.up;

	public override void SetData(Transform caster, Transform emitter, Transform target, Vector3 targetPosition, int index)
	{
		base.SetData(caster, emitter, target, targetPosition, index);
		Emit(GetTargetCenter() - base.transform.position);
	}

	protected void Emit(Vector3 fwd)
	{
		m_Velocity = fwd.normalized;
		Vector3 tangent = ((!(null != m_Emitter)) ? Vector3.up : m_Emitter.up);
		Vector3 binormal = Vector3.zero;
		Vector3.OrthoNormalize(ref m_Velocity, ref tangent, ref binormal);
		m_Velocity = Quaternion.AngleAxis((Random.value - 0.5f) * maxAngle, tangent) * Quaternion.AngleAxis((Random.value - 0.5f) * maxAngle, binormal) * m_Velocity;
		base.transform.rotation = Quaternion.LookRotation(m_Velocity, tangent);
		m_Velocity *= speed;
	}

	public override Vector3 Track(float deltaTime)
	{
		return m_Velocity * deltaTime;
	}
}

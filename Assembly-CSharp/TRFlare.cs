using UnityEngine;

public class TRFlare : Trajectory
{
	public float range;

	public float speed;

	private Vector3 direction;

	private float distance;

	private void Start()
	{
		if (m_Target != null)
		{
			Emit(GetTargetCenter(m_Target));
		}
		else
		{
			Emit(m_TargetPosition);
		}
	}

	public void Emit(Vector3 targetPos)
	{
		direction = ((!(null != m_Emitter)) ? Vector3.up : m_Emitter.forward);
	}

	public override Vector3 Track(float deltaTime)
	{
		distance += (1f - distance / range) * speed * deltaTime;
		return (1f - distance / range) * speed * deltaTime * direction.normalized;
	}
}

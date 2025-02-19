using UnityEngine;

public class TRImpulse : Trajectory
{
	public float speed;

	public float gravity;

	public float resist = 0.1f;

	public TRImpulseInitDirection initDirection;

	public bool followRotate;

	public override void SetData(Transform caster, Transform emitter, Transform target, Vector3 targetPosition, int index)
	{
		base.SetData(caster, emitter, target, targetPosition, index);
		if (null != m_Emitter)
		{
			Emit((!m_Target) ? targetPosition : GetTargetCenter(m_Target), m_Emitter, index);
		}
		else
		{
			Object.Destroy(base.gameObject);
		}
	}

	protected virtual void Emit(Vector3 targetPos, Transform emitTrans, int index)
	{
		switch (initDirection)
		{
		case TRImpulseInitDirection.PrecisionGuided:
		{
			float num = targetPos.y - base.transform.position.y;
			float num2 = Vector2.SqrMagnitude(new Vector2(targetPos.x - base.transform.position.x, targetPos.z - base.transform.position.z));
			float num3 = num - speed * speed / gravity;
			if (speed * speed * speed * speed - 2f * gravity * speed * speed * num - gravity * gravity * num2 > float.Epsilon)
			{
				num3 += Mathf.Sign(gravity) * Mathf.Sqrt((num - speed * speed / gravity) * (num - speed * speed / gravity) - num2 - num * num);
			}
			Vector3 vector = new Vector3(targetPos.x - base.transform.position.x, num - num3, targetPos.z - base.transform.position.z);
			m_Velocity = vector.normalized * speed;
			base.transform.rotation = emitTrans.rotation;
			break;
		}
		case TRImpulseInitDirection.TowardsTarget:
			m_Velocity = (targetPos - base.transform.position).normalized * speed;
			base.transform.rotation = Quaternion.FromToRotation(Vector3.forward, m_Velocity);
			break;
		case TRImpulseInitDirection.SelfDirection:
			m_Velocity = emitTrans.forward * speed;
			base.transform.rotation = emitTrans.rotation;
			break;
		}
	}

	public override Vector3 Track(float deltaTime)
	{
		m_Velocity += Vector3.down * gravity * deltaTime;
		m_Velocity += (0f - resist) * m_Velocity.sqrMagnitude * m_Velocity.normalized * Time.deltaTime;
		return m_Velocity * deltaTime;
	}

	public override Quaternion Rotate(float deltaTime)
	{
		if (followRotate)
		{
			return Quaternion.FromToRotation(Vector3.forward, m_Velocity);
		}
		return base.transform.rotation;
	}
}

using UnityEngine;

public class TRGatling : Trajectory
{
	public float speed;

	public float maxOffset;

	private Vector3 moveDirection;

	private Vector3 offset = Vector3.up;

	private void Start()
	{
		if (null != m_Emitter)
		{
			Emit(m_Emitter.forward);
		}
		else
		{
			Object.Destroy(base.gameObject);
		}
	}

	public void Emit(Vector3 fwd)
	{
		moveDirection = fwd;
		Vector3.OrthoNormalize(ref moveDirection, ref offset);
		base.transform.position += Quaternion.AngleAxis(Random.value * 360f, moveDirection) * (offset * maxOffset);
		base.transform.forward = moveDirection;
	}

	public override Vector3 Track(float deltaTime)
	{
		return moveDirection * speed * deltaTime;
	}
}

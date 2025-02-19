using UnityEngine;

public class TRRay : Trajectory
{
	public float speed;

	private Vector3 moveDirection;

	private Vector3 GetLocal()
	{
		if (m_Index == 0)
		{
			return new Vector3(0f, 0f, 1f);
		}
		if (m_Index == 1)
		{
			return new Vector3(1f, 0f, 0f);
		}
		if (m_Index == 2)
		{
			return new Vector3(0f, 0f, -1f);
		}
		if (m_Index == 3)
		{
			return new Vector3(-1f, 0f, 0f);
		}
		return new Vector3(0f, 0f, 1f);
	}

	private void Start()
	{
		moveDirection = base.transform.rotation * GetLocal();
		Debug.DrawRay(base.transform.position, moveDirection.normalized * 5f, Color.cyan);
	}

	public override Vector3 Track(float deltaTime)
	{
		if (moveDirection == Vector3.zero && m_Emitter != null)
		{
			moveDirection = m_Emitter.forward;
		}
		return moveDirection.normalized * speed * deltaTime;
	}

	public override Quaternion Rotate(float deltaTime)
	{
		return Quaternion.LookRotation(moveDirection);
	}
}

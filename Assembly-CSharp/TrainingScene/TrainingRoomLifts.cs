using UnityEngine;

namespace TrainingScene;

public class TrainingRoomLifts : MonoBehaviour
{
	[SerializeField]
	private Rigidbody liftA;

	[SerializeField]
	private Rigidbody liftB;

	[SerializeField]
	private float coolingTime = 5f;

	[SerializeField]
	private float minHeight = -7.75f;

	[SerializeField]
	private float maxHeight = -1.37f;

	[SerializeField]
	private float moveSpeed = 1f;

	private float m_ChangeTime;

	private bool m_AMoveUp;

	private void Start()
	{
		m_ChangeTime = Time.time + coolingTime;
		Vector3 localPosition = liftA.transform.localPosition;
		localPosition.y = minHeight;
		liftA.transform.localPosition = localPosition;
		localPosition = liftB.transform.localPosition;
		localPosition.y = maxHeight;
		liftB.transform.localPosition = localPosition;
	}

	private void FixedUpdate()
	{
		if (Time.time >= m_ChangeTime)
		{
			m_ChangeTime = Time.time + coolingTime;
			m_AMoveUp = !m_AMoveUp;
			return;
		}
		Vector3 localPosition = liftA.transform.localPosition;
		localPosition.y = Mathf.Clamp(localPosition.y + ((!m_AMoveUp) ? (-1f) : 1f) * Time.fixedDeltaTime * moveSpeed, minHeight, maxHeight);
		liftA.MovePosition(liftA.transform.parent.TransformPoint(localPosition));
		localPosition = liftB.transform.localPosition;
		localPosition.y = Mathf.Clamp(localPosition.y + (m_AMoveUp ? (-1f) : 1f) * Time.fixedDeltaTime * moveSpeed, minHeight, maxHeight);
		liftB.MovePosition(liftB.transform.parent.TransformPoint(localPosition));
	}
}

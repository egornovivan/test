using UnityEngine;

public class IKCombatLimit : MonoBehaviour
{
	private static int count = 100;

	public Transform rootBone;

	public Vector3 axis = Vector3.forward;

	[Range(0f, 180f)]
	public float limit;

	public float distance = 1f;

	private Vector3 m_Pivot = Vector3.zero;

	public Vector3 pivot => rootBone.TransformDirection(m_Pivot);

	private void Start()
	{
		m_Pivot = rootBone.InverseTransformDirection(base.transform.TransformDirection(axis));
	}

	private void OnDrawGizmosSelected()
	{
		Vector3 vector = base.transform.TransformDirection(axis);
		Vector3 vector2 = Vector3.ProjectOnPlane(Vector3.forward, vector);
		float num = 360f / (float)count;
		for (int i = 0; i < count; i++)
		{
			Vector3 rhs = Quaternion.AngleAxis(num * (float)i, vector) * vector2;
			Vector3 vector3 = Vector3.Cross(vector, rhs);
			Vector3 direction = Quaternion.AngleAxis(limit, vector3) * vector;
			Gizmos.DrawRay(base.transform.position, direction);
		}
	}
}

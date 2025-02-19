using UnityEngine;

public class PEBoneRotation : MonoBehaviour
{
	public Transform target;

	public Vector3 axisForward;

	public Vector3 axisUp;

	public bool rotateAuto;

	public float rotateSpeed;

	private Vector3 m_FaceDir;

	private void LateUpdate()
	{
		Vector3 vector = base.transform.rotation * axisUp;
		Vector3 vector2 = Vector3.ProjectOnPlane(m_FaceDir, vector);
		if (rotateAuto)
		{
			vector2 = Quaternion.AngleAxis(rotateSpeed * Time.deltaTime, vector) * vector2;
		}
		if (target != null)
		{
			vector2 = Vector3.ProjectOnPlane(target.position - base.transform.position, vector);
		}
		m_FaceDir = Util.ConstantSlerp(m_FaceDir, vector2, rotateSpeed * Time.deltaTime);
		m_FaceDir = Vector3.ProjectOnPlane(m_FaceDir, vector);
		base.transform.rotation = Quaternion.FromToRotation(base.transform.forward, m_FaceDir) * base.transform.rotation;
	}
}

using UnityEngine;

public class FixBoneForearmError : MonoBehaviour
{
	public Transform m_HandTrans;

	private Vector3 m_DefaultLocalPos;

	private Quaternion m_DefaultHandLocalRot;

	private void Start()
	{
		if (null != m_HandTrans)
		{
			m_DefaultLocalPos = base.transform.localPosition;
			m_DefaultHandLocalRot = base.transform.localRotation;
		}
	}

	private void LateUpdate()
	{
		if (null != m_HandTrans)
		{
			base.transform.localPosition = m_DefaultLocalPos;
			base.transform.localRotation = m_DefaultHandLocalRot;
		}
	}
}

using UnityEngine;

public class RopeMiddJoint : MonoBehaviour
{
	public Transform mConnectedBody;

	public Transform mConnectedBody2;

	public float mRopeLength = 0.12f;

	public float mForcepower = 100f;

	public float mFanpower = 0.01f;

	[HideInInspector]
	public Rigidbody m_Rigidbody;

	private void Awake()
	{
		m_Rigidbody = GetComponent<Rigidbody>();
	}

	private void FixedUpdate()
	{
		if (!(null == m_Rigidbody))
		{
			Vector3 vector = base.transform.position - mConnectedBody.transform.position;
			m_Rigidbody.AddForce(-vector.normalized * (vector.magnitude - mRopeLength) * mForcepower, ForceMode.Acceleration);
			if (null != mConnectedBody.gameObject.GetComponent<Rigidbody>())
			{
				mConnectedBody.gameObject.GetComponent<Rigidbody>().AddForce(vector.normalized * (vector.magnitude - mRopeLength) * mForcepower * mFanpower, ForceMode.Acceleration);
			}
			Vector3 vector2 = base.transform.position - mConnectedBody2.transform.position;
			m_Rigidbody.AddForce(-vector2.normalized * (vector2.magnitude - mRopeLength) * mForcepower, ForceMode.Acceleration);
			if (null != mConnectedBody2.gameObject.GetComponent<Rigidbody>())
			{
				mConnectedBody2.gameObject.GetComponent<Rigidbody>().AddForce(vector2.normalized * (vector2.magnitude - mRopeLength) * mForcepower * mFanpower, ForceMode.Acceleration);
			}
		}
	}
}

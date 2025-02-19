using UnityEngine;

public class RopeJoint : MonoBehaviour
{
	public Transform mConnectedBody;

	public float mRopeLength = 0.12f;

	public float mForcepower = 1f;

	public float mFanpower = 0.001f;

	[HideInInspector]
	public Rigidbody m_Rigidbody;

	private void Awake()
	{
		m_Rigidbody = GetComponent<Rigidbody>();
	}

	private void FixedUpdate()
	{
		if (null == m_Rigidbody)
		{
			return;
		}
		Vector3 vector = base.transform.position - mConnectedBody.position;
		if (vector.magnitude > mRopeLength)
		{
			base.transform.position = mConnectedBody.position + mRopeLength * vector.normalized;
			m_Rigidbody.AddForce(-vector.normalized * (vector.magnitude - mRopeLength) * mForcepower, ForceMode.Acceleration);
			if (null != mConnectedBody.gameObject.GetComponent<Rigidbody>())
			{
				mConnectedBody.gameObject.GetComponent<Rigidbody>().AddForce(vector.normalized * (vector.magnitude - mRopeLength) * mForcepower * mFanpower, ForceMode.Acceleration);
			}
		}
	}
}

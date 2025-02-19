using UnityEngine;

public class AircarftPutForce : MonoBehaviour
{
	[SerializeField]
	private float mMaxPower;

	public AircaraftTest mAircaraft;

	private float currutPower;

	private Vector3 zMove
	{
		get
		{
			if (mAircaraft.m_ForwardInput)
			{
				return mAircaraft.transform.forward;
			}
			if (mAircaraft.m_BackwardInput)
			{
				return -mAircaraft.transform.forward;
			}
			return Vector3.zero;
		}
	}

	private Vector3 xMove
	{
		get
		{
			Vector3 rhs = base.transform.position - mAircaraft.mRigidbody.worldCenterOfMass;
			if (mAircaraft.m_LeftInput)
			{
				return -Vector3.Cross(mAircaraft.transform.up, rhs).normalized;
			}
			if (mAircaraft.m_RightInput)
			{
				return Vector3.Cross(mAircaraft.transform.up, rhs).normalized;
			}
			return Vector3.zero;
		}
	}

	private void Start()
	{
	}

	private void Update()
	{
	}

	private void FixedUpdate()
	{
		if (mAircaraft != null)
		{
			float num = Mathf.Clamp01(Vector3.Dot(mAircaraft.transform.up, base.transform.up));
			float num2 = Mathf.Clamp01(Vector3.Dot(xMove + zMove, base.transform.up));
			Vector3 force = base.transform.up * currutPower;
			Vector3 position = Vector3.Lerp(base.transform.position, mAircaraft.mRigidbody.worldCenterOfMass, 0.97f);
			position.y = mAircaraft.mRigidbody.worldCenterOfMass.y;
			mAircaraft.mRigidbody.AddForceAtPosition(force, position);
		}
	}
}

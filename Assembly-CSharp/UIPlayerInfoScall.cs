using UnityEngine;

public class UIPlayerInfoScall : MonoBehaviour
{
	public GameObject mStage;

	public GameObject mPlayerMode;

	public int mSpeedCoeff;

	public float mObstruction;

	public float mScaleSpeed;

	public float mMaxSpeed;

	private BoxCollider mBoxClollider;

	private float starDis;

	private void Start()
	{
		mScaleSpeed = 0f;
		mBoxClollider = base.gameObject.GetComponent<BoxCollider>();
	}

	private void Update()
	{
		if (Mathf.Abs(mScaleSpeed) > mObstruction)
		{
			if (mScaleSpeed > 0f)
			{
				mScaleSpeed -= mObstruction;
			}
			else
			{
				mScaleSpeed += mObstruction;
			}
		}
		else
		{
			mScaleSpeed = 0f;
		}
		float num = mScaleSpeed * (float)mSpeedCoeff;
		if (mStage != null)
		{
			Vector3 localEulerAngles = mStage.transform.localEulerAngles;
			mStage.transform.localEulerAngles = new Vector3(localEulerAngles.x, localEulerAngles.y + num, localEulerAngles.z);
		}
		if (mPlayerMode != null)
		{
			Vector3 localEulerAngles2 = mPlayerMode.transform.localEulerAngles;
			mPlayerMode.transform.localEulerAngles = new Vector3(localEulerAngles2.x, localEulerAngles2.y + num, localEulerAngles2.z);
		}
	}

	private void OnScroll(float delta)
	{
		mScaleSpeed += delta;
		if (mScaleSpeed > mMaxSpeed)
		{
			mScaleSpeed = mMaxSpeed;
		}
		if (mScaleSpeed < 0f - mMaxSpeed)
		{
			mScaleSpeed = 0f - mMaxSpeed;
		}
	}
}

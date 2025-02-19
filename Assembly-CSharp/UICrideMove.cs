using UnityEngine;

public class UICrideMove : MonoBehaviour
{
	public float mMoveSpeed = 80f;

	public float mColorSpeed = 0.02f;

	public bool mCanMove = true;

	public int mOnceWaitTime_s = 10;

	public UITexture mBgTexcture;

	public UITexture mGuangTexture;

	public UITexture mDianTexture;

	private bool mStarMove;

	private bool mIsMove;

	private Color mGuangColor;

	private bool bColorAdd = true;

	private float mTime;

	private float mColorFlag;

	private void Start()
	{
	}

	private void Update()
	{
		if (!mCanMove)
		{
			return;
		}
		if (mIsMove)
		{
			if (mStarMove)
			{
				StarMove();
			}
			UpdateMove();
		}
		else
		{
			UpDateTime();
		}
	}

	private void UpdateMove()
	{
		Vector3 eulerAngles = base.gameObject.transform.localRotation.eulerAngles;
		eulerAngles.z -= mMoveSpeed * Time.deltaTime;
		base.gameObject.transform.localRotation = Quaternion.Euler(eulerAngles);
		if (bColorAdd)
		{
			mGuangColor.r += mColorSpeed;
			mGuangColor.g += mColorSpeed;
			mGuangColor.b += mColorSpeed;
			float num = 70f + 24f * mGuangColor.r;
			mBgTexcture.transform.localScale = new Vector3(num, num, 1f);
			if ((double)mGuangColor.r > 0.7)
			{
				bColorAdd = false;
			}
		}
		else
		{
			mGuangColor.r -= mColorSpeed;
			mGuangColor.g -= mColorSpeed;
			mGuangColor.b -= mColorSpeed;
			float num = 70f + 24f * mGuangColor.r;
			mBgTexcture.transform.localScale = new Vector3(num, num, 1f);
			if ((double)mGuangColor.r < 0.06)
			{
				EndMove();
			}
		}
		mBgTexcture.color = Color.Lerp(mBgTexcture.color, mGuangColor, Time.deltaTime);
		mGuangTexture.color = Color.Lerp(mGuangTexture.color, mGuangColor, Time.deltaTime);
		mDianTexture.color = Color.Lerp(mDianTexture.color, mGuangColor, Time.deltaTime);
	}

	private void StarMove()
	{
		mTime = 0f;
		mGuangColor = Color.black;
		bColorAdd = true;
		mStarMove = false;
		mBgTexcture.transform.localScale = new Vector3(70f, 70f, 1f);
	}

	private void EndMove()
	{
		mIsMove = false;
		mGuangColor = Color.black;
		mBgTexcture.transform.localScale = new Vector3(70f, 70f, 1f);
	}

	private void UpDateTime()
	{
		mTime += 1f * Time.deltaTime;
		if (mTime >= (float)mOnceWaitTime_s)
		{
			mIsMove = true;
			mStarMove = true;
		}
		if (mTime < (float)(mOnceWaitTime_s / 2))
		{
			mColorFlag = 1f - mTime / (float)mOnceWaitTime_s;
		}
		else
		{
			mColorFlag = mTime / (float)mOnceWaitTime_s;
		}
		mColorFlag *= 1f;
	}
}

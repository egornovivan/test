using UnityEngine;

public class UIHealthBar : MonoBehaviour
{
	[SerializeField]
	private float mValue = 1f;

	[SerializeField]
	private UISprite mSprBg;

	[SerializeField]
	private UIFilledSprite mSprBgNormal;

	[SerializeField]
	private UIFilledSprite mSprBgAdd;

	[SerializeField]
	private UIFilledSprite mSprBgMinus;

	[SerializeField]
	private UIHpSpecularHandler handler;

	public float MAXVALUE = 500f;

	public float mMaxValue;

	public float mCurValue;

	public float Value
	{
		get
		{
			return mValue;
		}
		set
		{
			mValue = value;
		}
	}

	private void Start()
	{
		mMaxValue = MAXVALUE;
		mCurValue = MAXVALUE;
		mSprBgNormal.fillAmount = 0f;
		mSprBgAdd.fillAmount = 0f;
		mSprBgMinus.fillAmount = 0f;
	}

	private void Update()
	{
		if (mValue > 1f)
		{
			mValue = 1f;
		}
		if (mValue < 0f)
		{
			mValue = 0f;
		}
		if (mMaxValue == MAXVALUE)
		{
			mSprBgAdd.fillAmount = 0f;
			mSprBgMinus.fillAmount = 0f;
			mSprBgNormal.fillAmount = mCurValue / mMaxValue;
		}
		else if (mMaxValue > MAXVALUE)
		{
			mSprBgMinus.fillAmount = 0f;
			if (mCurValue == MAXVALUE)
			{
				mSprBgAdd.fillAmount = 0f;
				mSprBgNormal.fillAmount = 1f;
			}
			else if (mCurValue < MAXVALUE)
			{
				mSprBgAdd.fillAmount = 0f;
				mSprBgNormal.fillAmount = mCurValue / MAXVALUE;
			}
			else if (mCurValue > MAXVALUE)
			{
				mSprBgNormal.fillAmount = 1f;
				mSprBgAdd.fillAmount = (mCurValue - MAXVALUE) / MAXVALUE;
			}
		}
		else if (mMaxValue < MAXVALUE)
		{
			mSprBgAdd.fillAmount = 0f;
			mSprBgMinus.fillAmount = (MAXVALUE - mMaxValue) / MAXVALUE;
			mSprBgNormal.fillAmount = mCurValue / mMaxValue * (1f - mSprBgMinus.fillAmount);
		}
	}
}

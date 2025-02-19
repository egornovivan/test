using System.Collections.Generic;
using UnityEngine;

public class GridSlider_N : MonoBehaviour
{
	public Color mStartCol;

	public Color mMidCol;

	public Color mEndCol;

	public Color mDisableCol;

	public float mLength = 10f;

	public int mNum;

	public UISprite mPerfab;

	public UISprite mIcon;

	public UILabel mText;

	private List<UISprite> mSprList;

	private float mSliderValue = 1f;

	public void Init()
	{
		mSprList = new List<UISprite>();
		for (int i = 0; i < mNum; i++)
		{
			UISprite uISprite = Object.Instantiate(mPerfab);
			uISprite.transform.parent = base.transform;
			uISprite.transform.localPosition = new Vector3(mLength * (float)(i + 1) + 4f, 0f, 0f);
			uISprite.MakePixelPerfect();
			mSprList.Add(uISprite);
		}
		ResetGrid();
	}

	public void SetIcon(string iconName)
	{
		if (null != mIcon)
		{
			mIcon.spriteName = iconName;
		}
	}

	private void ResetGrid()
	{
		for (int i = 0; i < mNum; i++)
		{
			if ((0.499f + (float)i) / (float)mNum < mSliderValue)
			{
				if (i < mNum / 2)
				{
					mSprList[i].color = Color.Lerp(mStartCol, mMidCol, 2f * (float)i / (float)mNum);
				}
				else
				{
					mSprList[i].color = Color.Lerp(mMidCol, mEndCol, 2f * (1f * (float)i / (float)mNum - 0.5f));
				}
			}
			else
			{
				mSprList[i].color = mDisableCol;
			}
		}
	}

	public void SetSliderValue(float SliderValue, string TrueValue = "")
	{
		if (null != mText)
		{
			mText.text = TrueValue;
		}
		if (mSliderValue != SliderValue)
		{
			mSliderValue = Mathf.Clamp01(SliderValue);
			ResetGrid();
		}
	}
}

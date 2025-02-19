using UnityEngine;

public class ServantTalkItem_N : MonoBehaviour
{
	public UILabel mName;

	public UILabel mContent;

	public UISlicedSprite mBG;

	private float mTimeCounter = 9f;

	private float mFadeTimeCounter;

	public void InitItem(string name, string content)
	{
		mName.text = name;
		mContent.text = content;
		if (null != mBG)
		{
			float num = mContent.font.CalculatePrintedSize(mContent.processedText, encoding: true, UIFont.SymbolStyle.None).y * (float)mContent.font.size;
			if (num + 30f > 88f)
			{
				Vector3 localScale = mBG.transform.localScale;
				localScale.y = num + 30f;
				mBG.transform.localScale = localScale;
			}
		}
	}

	private void Update()
	{
		if (mTimeCounter > 0f)
		{
			mTimeCounter -= Time.deltaTime;
			if (mTimeCounter <= 0f)
			{
				Hide();
			}
		}
		if (mFadeTimeCounter > 0f)
		{
			mFadeTimeCounter -= Time.deltaTime;
			if (mFadeTimeCounter <= 0f)
			{
				Object.Destroy(base.gameObject);
			}
		}
	}

	public void GoUp()
	{
		GetComponent<TweenPosition>().Play(forward: true);
	}

	public void Hide()
	{
		GetComponent<TweenScale>().Play(forward: false);
		mFadeTimeCounter = 0.5f;
	}
}

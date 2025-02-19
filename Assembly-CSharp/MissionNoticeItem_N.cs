using UnityEngine;

public class MissionNoticeItem_N : MonoBehaviour
{
	public UILabel mMissionName;

	public UILabel mMissionState;

	public UISprite mBg;

	private float mTimeCounter = 9f;

	private float mFadeTimeCounter;

	public void InitItem(int type, string name)
	{
		switch (type)
		{
		case 0:
			mBg.spriteName = "MissionNotice_b";
			mMissionState.text = "[70dcff]New Mission[-]";
			break;
		case 1:
			mBg.spriteName = "MissionNotice_y";
			mMissionState.text = "[feca7e]Mission Completed[-]";
			break;
		case 2:
			mBg.spriteName = "MissionNotice_r";
			mMissionState.text = "[ff8882]Mission Failed[-]";
			break;
		}
		mMissionName.text = name;
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

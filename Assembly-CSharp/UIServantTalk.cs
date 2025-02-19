using System.Collections.Generic;
using UnityEngine;

public class UIServantTalk : UIBaseWidget
{
	public ServantTalkItem_N mPrefab;

	private ServantTalkItem_N mCurrentItem;

	private ServantTalkItem_N mLastItem;

	private List<TalkData> mTalkList = new List<TalkData>();

	private float mCountTime;

	private void Update()
	{
		if (mCountTime >= 0f)
		{
			mCountTime -= Time.deltaTime;
		}
		if (mCountTime <= 0f && mTalkList.Count > 0)
		{
			AddOpBtn(mTalkList[0]);
			mTalkList.RemoveAt(0);
			mCountTime = 3f;
		}
	}

	public void AddTalk(int id, string name = null)
	{
		TalkData talkData = TalkRespository.GetTalkData(id);
		if (talkData == null)
		{
			if (Application.isEditor)
			{
				Debug.LogError("ServantTalk ID" + id + " can't find talk data");
			}
		}
		else
		{
			mTalkList.Add(talkData);
		}
	}

	private void AddOpBtn(TalkData talkdata)
	{
		if (mLastItem != null)
		{
			mLastItem.Hide();
		}
		if (mCurrentItem != null)
		{
			mCurrentItem.Hide();
			mLastItem = mCurrentItem;
		}
		if (talkdata != null)
		{
			string empty = string.Empty;
			string empty2 = string.Empty;
			mCurrentItem = Object.Instantiate(mPrefab);
			mCurrentItem.transform.parent = base.transform;
			mCurrentItem.transform.localPosition = Vector3.zero;
			mCurrentItem.transform.localRotation = Quaternion.identity;
			mCurrentItem.InitItem(empty2, empty);
		}
	}
}

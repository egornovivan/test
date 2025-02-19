using UnityEngine;

public class MissionStateTipItem : MonoBehaviour
{
	[SerializeField]
	private UILabel mState;

	[SerializeField]
	private UILabel mContent;

	[SerializeField]
	private UISprite mSprite;

	private float mTimer;

	private float mExistTime = 3.5f;

	private void Start()
	{
		mTimer = 0f;
	}

	private void Update()
	{
		mTimer += Time.deltaTime;
		if (mTimer >= mExistTime)
		{
			Object.Destroy(base.gameObject);
		}
	}

	public void SetContent(string _state, string _content)
	{
		mState.text = _state;
		mContent.text = _content;
		if (StateSplit(_state) == "New Mission: ")
		{
			mSprite.spriteName = "system_b";
		}
		else if (StateSplit(_state) == "Mission Failed: ")
		{
			mSprite.spriteName = "system_r";
		}
		else if (StateSplit(_state) == "Mission Completed: ")
		{
			mSprite.spriteName = "system_y";
		}
	}

	private string StateSplit(string splitedStr)
	{
		string empty = string.Empty;
		return splitedStr.Split(']')[1].Split('[')[0];
	}
}

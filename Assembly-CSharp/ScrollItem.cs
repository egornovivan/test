using System.Collections.Generic;
using UnityEngine;

public class ScrollItem : MonoBehaviour
{
	public UILabel mContent;

	public UIScrollBar mScroll;

	public List<string> mSelections;

	public bool UseLocalLocation = true;

	public int mIndex;

	private float lastValue;

	private string m_Description;

	private void Update()
	{
		if (base.gameObject.name != "Quality" && Mathf.Abs(lastValue - mScroll.scrollValue) > 0.01f)
		{
			lastValue = mScroll.scrollValue;
			UIOption.Instance.OnChange();
		}
		mIndex = (int)Mathf.Round(mScroll.scrollValue * (float)(mSelections.Count - 1));
		mContent.text = GetStrByCurIndex();
	}

	private string GetStrByCurIndex()
	{
		if (UseLocalLocation)
		{
			int result = 0;
			string text = mSelections[mIndex].Trim();
			if (text == string.Empty)
			{
				return string.Empty;
			}
			if (int.TryParse(text, out result))
			{
				return PELocalization.GetString(result);
			}
			Debug.Log("Not find localLization keyID:" + mSelections[mIndex]);
			return string.Empty;
		}
		return mSelections[mIndex].ToString();
	}

	public void SetIndex(int index)
	{
		if (index >= 0 && index < mSelections.Count)
		{
			mIndex = index;
		}
		else
		{
			mIndex = mSelections.Count - 1;
		}
		mContent.text = GetStrByCurIndex();
		mScroll.scrollValue = (float)mIndex / (float)(mSelections.Count - 1);
		lastValue = mScroll.scrollValue;
	}

	public void SetValue(float setValue)
	{
		float scrollValue = Mathf.Clamp01(setValue);
		mScroll.scrollValue = scrollValue;
		lastValue = scrollValue;
		mContent.text = string.Empty;
	}
}

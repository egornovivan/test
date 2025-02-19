using System.Collections.Generic;
using UnityEngine;

public class OptionItem_N : MonoBehaviour
{
	public UILabel mContent;

	public List<string> mSelections;

	public bool UseLocalization = true;

	public int mIndex;

	private string m_Description;

	private void LBtnDown()
	{
		if (--mIndex < 0)
		{
			mIndex = mSelections.Count - 1;
		}
		UpdateContent();
	}

	private void RBtnDown()
	{
		if (++mIndex > mSelections.Count - 1)
		{
			mIndex = 0;
		}
		UpdateContent();
	}

	private void UpdateContent()
	{
		mContent.text = GetStringByCurIndex();
		UIOption.Instance.OnChange();
	}

	private string GetStringByCurIndex()
	{
		if (UseLocalization)
		{
			string text = mSelections[mIndex].Trim();
			if (text == string.Empty)
			{
				return string.Empty;
			}
			if (int.TryParse(text, out var result))
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
		mContent.text = GetStringByCurIndex();
	}
}

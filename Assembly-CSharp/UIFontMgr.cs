using System.Collections.Generic;
using UnityEngine;

public class UIFontMgr : MonoBehaviour
{
	public enum LanguageType
	{
		English,
		Chinese
	}

	private static UIFontMgr mInstance;

	public LanguageType mLanguageType;

	public List<UIFont> mEnglish = new List<UIFont>();

	public List<UIFont> mChinese = new List<UIFont>();

	public static UIFontMgr Instance => mInstance;

	private void Awake()
	{
		mInstance = this;
		if (SystemSettingData.Instance != null)
		{
			if (SystemSettingData.Instance.IsChinese)
			{
				mLanguageType = LanguageType.Chinese;
			}
			else
			{
				mLanguageType = LanguageType.English;
			}
		}
	}

	public UIFont GetFontForLanguage(UIFont oldFont)
	{
		int num = mEnglish.IndexOf(oldFont);
		if (num == -1)
		{
			num = mChinese.IndexOf(oldFont);
		}
		if (num > -1)
		{
			if (mLanguageType == LanguageType.English && num < mEnglish.Count)
			{
				return mEnglish[num];
			}
			if (mLanguageType == LanguageType.Chinese && num < mChinese.Count)
			{
				return mChinese[num];
			}
		}
		return oldFont;
	}
}

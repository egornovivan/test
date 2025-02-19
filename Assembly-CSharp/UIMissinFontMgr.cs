using UnityEngine;

public class UIMissinFontMgr : MonoBehaviour
{
	public UIFont mEnglish;

	public UIFont mChinese;

	private UILabel mUIlable;

	private void Awake()
	{
		mUIlable = base.transform.GetComponent<UILabel>();
		if (SystemSettingData.Instance != null)
		{
			if (SystemSettingData.Instance.IsChinese)
			{
				mUIlable.font = mChinese;
			}
			else
			{
				mUIlable.font = mEnglish;
			}
		}
	}
}

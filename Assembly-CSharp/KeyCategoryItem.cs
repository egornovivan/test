using UnityEngine;

public class KeyCategoryItem : MonoBehaviour
{
	public int mStringID;

	private UILabel mLable;

	private bool mHasContent;

	private void Start()
	{
		mLable = GetComponentInChildren<UILabel>();
	}

	private void TryLocalize()
	{
		if ((bool)mLable && PELocalization.GetString(mStringID) != string.Empty)
		{
			mHasContent = true;
			mLable.text = PELocalization.GetString(mStringID);
		}
	}

	private void Update()
	{
		if (!mHasContent)
		{
			TryLocalize();
		}
	}
}

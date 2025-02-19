using UnityEngine;

public class PEUILocalize : MonoBehaviour
{
	public int mStringID;

	public bool mStrToUpper;

	public bool mStrToLower;

	private UILabel mLable;

	private void Start()
	{
		mLable = GetComponent<UILabel>();
		Localize();
	}

	public void Localize()
	{
		if ((bool)mLable)
		{
			string @string = PELocalization.GetString(mStringID);
			if (mStrToUpper)
			{
				mLable.text = @string.ToUpper();
			}
			else if (mStrToLower)
			{
				mLable.text = @string.ToLower();
			}
			else
			{
				mLable.text = @string;
			}
		}
	}
}

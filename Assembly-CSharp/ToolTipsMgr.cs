using UnityEngine;

public class ToolTipsMgr : MonoBehaviour
{
	private static ToolTipsMgr mInstance;

	public UILabel mContent;

	public UISlicedSprite mBg;

	public UISprite mMeat;

	public UISprite mLine;

	private void Awake()
	{
		mInstance = this;
	}

	private void SetText(string content)
	{
		if (content != null)
		{
			mContent.text = content;
			mLine.gameObject.SetActive(value: true);
			if (content.Contains("[meat]"))
			{
				mContent.text = content.Replace("[meat]", string.Empty);
			}
			UITooltip.ShowText(mContent.text);
		}
		else
		{
			mMeat.gameObject.SetActive(value: false);
			mLine.gameObject.SetActive(value: false);
			UITooltip.ShowText(content);
		}
	}

	public static void ShowText(string content)
	{
		if ((bool)mInstance)
		{
			mInstance.SetText(content);
		}
	}
}

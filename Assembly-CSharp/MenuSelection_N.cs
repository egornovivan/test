using UnityEngine;

public class MenuSelection_N : MonoBehaviour
{
	public UISlicedSprite mBgSpr;

	public UILabel mTextLabel;

	public UISprite mExpansionSpr;

	private UIButton mButton;

	public bool ExpansionEnable
	{
		set
		{
			mExpansionSpr.enabled = value;
			mButton.isEnabled = value;
			mTextLabel.color = ((!value) ? Color.gray : Color.white);
		}
	}

	public bool ShowExpansion
	{
		set
		{
			mExpansionSpr.enabled = value;
		}
	}

	public Vector3 Size => mBgSpr.transform.localScale;

	private void Awake()
	{
		mTextLabel.transform.localPosition = new Vector3(mBgSpr.transform.localScale.x / 2f, (0f - mBgSpr.transform.localScale.y) / 2f, 0f);
		mExpansionSpr.transform.localPosition = new Vector3(mBgSpr.transform.localScale.x - 2f, (0f - mBgSpr.transform.localScale.y) / 2f, 0f);
		BoxCollider component = GetComponent<BoxCollider>();
		component.size = mBgSpr.transform.localScale;
		component.center = new Vector3(component.size.x / 2f, (0f - component.size.y) / 2f, -1f);
		mButton = GetComponent<UIButton>();
	}

	public void SetInfo(string content, SimpleMenu_N menu, int index)
	{
		mTextLabel.text = content;
	}

	private void OnClick()
	{
	}
}

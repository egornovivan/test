using UnityEngine;

public class MessionListItem : MonoBehaviour
{
	public UICheckbox mCheckBoxTag;

	public UILabel mLbTitle;

	public UISprite mSpBg;

	public UISprite mSpState;

	public GameObject mTeewnContent;

	public object mData;

	private bool _IsExpand;

	public bool IsExpand => _IsExpand;

	private void Awake()
	{
		_IsExpand = mTeewnContent.activeSelf;
	}

	private void TeewnContentOnExpand()
	{
		if (_IsExpand)
		{
			_IsExpand = false;
		}
		else
		{
			_IsExpand = true;
		}
		mSpState.spriteName = ((!_IsExpand) ? "mission_closed" : "mission_open");
	}
}

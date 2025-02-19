using UnityEngine;

public class TutorialItem_N : MonoBehaviour
{
	public delegate void OnClickEvent(object sender);

	public UICheckbox mCheckBox;

	public UILabel mLabel;

	public int mID;

	public event OnClickEvent e_OnClick;

	public void SetItem(int ID, string content = "")
	{
		mID = ID;
		mLabel.text = content;
	}

	private void OnClick()
	{
		if (Input.GetMouseButtonUp(0) && mID != -1 && this.e_OnClick != null)
		{
			this.e_OnClick(this);
		}
	}
}

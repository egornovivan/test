using UnityEngine;

public class UIFriendItem : MonoBehaviour
{
	public delegate void ItemEvent(int index);

	[SerializeField]
	private UILabel mLbName;

	[SerializeField]
	private UITexture mTexIco;

	[HideInInspector]
	private int mIndex = -1;

	public bool mIsOnLine;

	public int Index => mIndex;

	public event ItemEvent e_ShowToolTip;

	public event ItemEvent e_ShowFrienMenu;

	public void SetFriendInfo(Texture2D texIco, string ifno, int index, bool isOnline)
	{
		mLbName.text = ifno;
		if (texIco != null)
		{
			mTexIco.mainTexture = texIco;
		}
		mIndex = index;
		mLbName.color = ((!isOnline) ? Color.gray : Color.white);
		mIsOnLine = isOnline;
	}

	private void OnMouseRightClick()
	{
		if (this.e_ShowFrienMenu != null)
		{
			this.e_ShowFrienMenu(mIndex);
		}
	}
}

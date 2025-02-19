using System.Collections.Generic;
using UnityEngine;

public class UIFriendWnd : UIStaticWnd
{
	public enum TabState
	{
		state_Friend,
		state_Palyer
	}

	public delegate void ItemEvent(int index);

	[SerializeField]
	private UITexture mTexIco;

	[SerializeField]
	private UILabel mLbName;

	[SerializeField]
	private GameObject mItemPrefab;

	[SerializeField]
	private UIGrid mGrid;

	[SerializeField]
	private TweenPosition mTweenPos;

	[SerializeField]
	private GameObject mOptionMenuPrefab;

	[SerializeField]
	private GameObject mInviteBoxPrefab;

	[SerializeField]
	private UICheckbox mTabSteamFriend;

	[SerializeField]
	private UICheckbox mTabRoomPlayer;

	[HideInInspector]
	public UIOptionMenu mOptionMenu;

	[HideInInspector]
	public UIInviteMsgbox mInviteBox;

	private List<UIFriendItem> mItemList = new List<UIFriendItem>();

	private bool isHide;

	[HideInInspector]
	public TabState mTabState;

	public event ItemEvent e_ShowToolTip;

	public event ItemEvent e_ShowFriendMenu;

	public event WndEvent e_TabChange;

	public void InitOptionMenu(Transform centerTs, Camera uiCamera)
	{
		GameObject gameObject = Object.Instantiate(mOptionMenuPrefab);
		gameObject.transform.parent = centerTs;
		gameObject.transform.localPosition = new Vector3(0f, 0f, -20f);
		gameObject.transform.localScale = Vector3.one;
		mOptionMenu = gameObject.GetComponent<UIOptionMenu>();
		mOptionMenu.Init(uiCamera);
		mOptionMenu.Hide();
	}

	public void InitInviteBox(Transform leftTopTs)
	{
		GameObject gameObject = Object.Instantiate(mInviteBoxPrefab);
		gameObject.transform.parent = leftTopTs;
		gameObject.transform.localPosition = new Vector3(-220f, -180f, -100f);
		gameObject.transform.localScale = Vector3.one;
		mInviteBox = gameObject.GetComponent<UIInviteMsgbox>();
		mInviteBox.Hide();
	}

	public void SetMyInfo(string name, Texture2D tex)
	{
		if (tex != null)
		{
			mTexIco.mainTexture = tex;
		}
		mLbName.text = name;
	}

	public void AddListItem(string name, Texture2D tex, int index, bool isOnline)
	{
		GameObject gameObject = Object.Instantiate(mItemPrefab);
		gameObject.transform.parent = mGrid.gameObject.transform;
		gameObject.transform.localScale = Vector3.one;
		gameObject.transform.localPosition = Vector3.zero;
		gameObject.SetActive(value: true);
		UIFriendItem component = gameObject.GetComponent<UIFriendItem>();
		component.SetFriendInfo(tex, name, index, isOnline);
		component.e_ShowToolTip += ShowToopTip;
		component.e_ShowFrienMenu += ShowFriendMenu;
		mItemList.Add(component);
	}

	public void ClearList()
	{
		foreach (UIFriendItem mItem in mItemList)
		{
			mItem.gameObject.transform.parent = null;
			Object.Destroy(mItem.gameObject);
		}
		mItemList.Clear();
	}

	private void SortListForOnlie()
	{
		foreach (UIFriendItem mItem in mItemList)
		{
			mItem.gameObject.name = ((!mItem.mIsOnLine) ? "1_Offline_FriendItem" : "0_Online_FriendItem");
		}
	}

	public void RepostionList()
	{
		SortListForOnlie();
		mGrid.repositionNow = true;
	}

	private void ShowToopTip(int index)
	{
		if (this.e_ShowToolTip != null)
		{
			this.e_ShowToolTip(index);
		}
	}

	private void ShowFriendMenu(int index)
	{
		if (this.e_ShowFriendMenu != null)
		{
			this.e_ShowFriendMenu(index);
		}
	}

	public override void Show()
	{
		base.gameObject.SetActive(value: true);
		isHide = false;
		mTweenPos.Play(forward: true);
	}

	protected override void OnHide()
	{
		isHide = true;
		mTweenPos.Play(forward: false);
	}

	private void MoveFinished()
	{
		if (isHide)
		{
			base.gameObject.SetActive(value: false);
		}
	}

	private void TabFriendOnActive(bool active)
	{
		if (active)
		{
			mTabState = TabState.state_Friend;
			if (this.e_TabChange != null)
			{
				this.e_TabChange();
			}
		}
	}

	private void TabPalyerOnActive(bool active)
	{
		if (active)
		{
			mTabState = TabState.state_Palyer;
			if (this.e_TabChange != null)
			{
				this.e_TabChange();
			}
		}
	}

	public void EnableTabRoomPalyer(bool enable)
	{
		mTabRoomPlayer.gameObject.SetActive(enable);
	}
}

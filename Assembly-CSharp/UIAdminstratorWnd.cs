using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIAdminstratorWnd : UIBaseWnd
{
	[SerializeField]
	private Transform Centent;

	[SerializeField]
	private GameObject UIAdminstratorItemPrefab;

	[HideInInspector]
	public UIAdminstratorItem mUIAdminstratorItem;

	public UICheckbox mBlackListCheckbox;

	public UICheckbox mPersonnelCheckbox;

	public GameObject mPersonnelBg;

	public GameObject mBlackListBg;

	[SerializeField]
	private UILabel mLbPage;

	[SerializeField]
	private UIGrid mGird;

	[SerializeField]
	private GameObject mForbidsBuildBtn;

	[SerializeField]
	private GameObject mForbidsNewPalyerBtn;

	[SerializeField]
	private GameObject mForbidsBuildBg;

	[SerializeField]
	private GameObject mForbidsNewPalyerBg;

	[SerializeField]
	private GameObject mBanBtn;

	[SerializeField]
	private GameObject mBanAllBtn;

	[SerializeField]
	private GameObject mReMoveBtn;

	[SerializeField]
	private GameObject mReMoveAllBtn;

	public UserAdmin mUserAdmin;

	private List<UIAdminstratorItem> mItemList = new List<UIAdminstratorItem>();

	private List<UserAdmin> mBPrivilegesInfoList = new List<UserAdmin>();

	private List<UserAdmin> mpitchAdminList = new List<UserAdmin>();

	private List<UserAdmin> UserAdminList;

	private int mCurrentPage = 1;

	private int mEndPage;

	private bool Ispersonnel = true;

	private ulong mMask = 2uL;

	private bool ForbidsBuild;

	private bool ForbidsNewPalyer;

	private GameObject AddUIPrefab(GameObject prefab, Transform parentTs)
	{
		GameObject gameObject = Object.Instantiate(prefab);
		gameObject.transform.parent = parentTs;
		gameObject.layer = parentTs.gameObject.layer;
		gameObject.transform.localPosition = Vector3.zero;
		gameObject.transform.localScale = Vector3.one;
		return gameObject;
	}

	private void Awake()
	{
		Instant();
	}

	private void Instant()
	{
		GameObject gameObject = AddUIPrefab(UIAdminstratorItemPrefab, Centent);
		mUIAdminstratorItem = gameObject.GetComponent<UIAdminstratorItem>();
		gameObject.SetActive(value: false);
	}

	private void Start()
	{
		UIAdminstratorctr.UpdatamPersonel();
		ServerAdministrator.PrivilegesChangedEvent = OnPrivilegesChanged;
		ServerAdministrator.LockAreaChangedEvent = OnLockAreaChanged;
		UpdataPage();
		Reflsh(UIAdminstratorctr.mUIPersonelInfoList);
		mPersonnelBg.SetActive(value: true);
	}

	private void Updata()
	{
	}

	private void OnDestroy()
	{
		ServerAdministrator.PrivilegesChangedEvent = null;
		ServerAdministrator.LockAreaChangedEvent = null;
	}

	private void OnPrivilegesChanged(UserAdmin ua)
	{
		UIAdminstratorctr.ChangeAssistant(ua);
		ShowPageLsit(UIAdminstratorctr.mUIPersonelInfoList, mCurrentPage, UIAdminstratorctr.UIArrayPersonnelAdmin);
		Reflsh(UIAdminstratorctr.mUIPersonelInfoList);
	}

	private void OnLockAreaChanged(int ifon, bool Lock)
	{
	}

	private void Test()
	{
		for (int i = 0; i < 35; i++)
		{
			UserAdmin userAdmin = new UserAdmin(i, "palyer" + i, 0uL);
			if (i == 0)
			{
				userAdmin.AddPrivileges(AdminMask.AdminRole);
			}
			ServerAdministrator.UserAdminList.Add(userAdmin);
		}
	}

	private void UpdataPage()
	{
		for (int i = (mCurrentPage - 1) * 8; i < UIAdminstratorctr.UIArrayPersonnelAdmin.Count && i < mCurrentPage * 8; i++)
		{
			UIAdminstratorctr.mUIPersonelInfoList.Add((UserAdmin)UIAdminstratorctr.UIArrayPersonnelAdmin[i]);
		}
		if (UIAdminstratorctr.UIArrayPersonnelAdmin.Count % 8 == 0)
		{
			mEndPage = UIAdminstratorctr.UIArrayPersonnelAdmin.Count / 8;
		}
		else
		{
			mEndPage = UIAdminstratorctr.UIArrayPersonnelAdmin.Count / 8 + 1;
		}
		mLbPage.text = mCurrentPage + "/" + mEndPage;
	}

	private void Move<T>(List<T> Fromlist1, T Info, List<T> Tolist2)
	{
		if (Info != null)
		{
			Fromlist1.Remove(Info);
			Tolist2.Add(Info);
		}
	}

	private void Reflsh(List<UserAdmin> refashList)
	{
		Clear();
		refashList.Sort(delegate(UserAdmin x, UserAdmin y)
		{
			if (PlayerNetwork.IsOnline(x.Id) && PlayerNetwork.IsOnline(y.Id))
			{
				return 0;
			}
			return (!PlayerNetwork.IsOnline(x.Id)) ? 1 : (-1);
		});
		foreach (UserAdmin refash in refashList)
		{
			AddAdminstItem(refash);
		}
		mGird.repositionNow = true;
	}

	private void ShowPageLsit(List<UserAdmin> PageList, int Page, ArrayList ArrayAdmin)
	{
		if (ArrayAdmin.Count % 8 == 0)
		{
			mEndPage = ArrayAdmin.Count / 8;
		}
		else
		{
			mEndPage = ArrayAdmin.Count / 8 + 1;
		}
		PageList.Clear();
		for (int i = (Page - 1) * 8; i < ArrayAdmin.Count && i < Page * 8; i++)
		{
			PageList.Add((UserAdmin)ArrayAdmin[i]);
		}
		mLbPage.text = Page + "/" + mEndPage;
	}

	private void AddAdminstItem(UserAdmin userAdmin)
	{
		GameObject gameObject = Object.Instantiate(UIAdminstratorItemPrefab);
		gameObject.transform.parent = mGird.transform;
		gameObject.transform.localScale = Vector3.one;
		gameObject.transform.localPosition = Vector3.zero;
		gameObject.SetActive(value: true);
		UIAdminstratorItem component = gameObject.GetComponent<UIAdminstratorItem>();
		if (PlayerNetwork.IsOnline(userAdmin.Id))
		{
			component.NameText = "[33FF00]" + userAdmin.RoleName + "[-]";
		}
		else
		{
			component.NameText = "[999999]" + userAdmin.RoleName + "[-]";
		}
		component.mUserAdmin = userAdmin;
		component.e_ItemAdminOnClick += ItemAdminOnClick;
		component.e_ItemAdminOnpitch += ItemAdminOnpitch;
		component.isForbiddenRelsh = true;
		UIAdminstratorctr.ShowAssistant(component);
		if (ServerAdministrator.IsAdmin(PlayerNetwork.mainPlayerId))
		{
			if (Ispersonnel)
			{
				component.mSetBtn.SetActive(value: true);
				component.mForbidenBtn.SetActive(value: true);
				mForbidsBuildBtn.SetActive(value: true);
				mForbidsNewPalyerBtn.SetActive(value: true);
				mBanBtn.SetActive(value: true);
				mBanAllBtn.SetActive(value: true);
			}
			else
			{
				mBanBtn.SetActive(value: false);
				mBanAllBtn.SetActive(value: false);
				mReMoveBtn.SetActive(value: true);
				mReMoveAllBtn.SetActive(value: true);
			}
		}
		else
		{
			mForbidsBuildBtn.SetActive(value: false);
			mForbidsNewPalyerBtn.SetActive(value: false);
		}
		mItemList.Add(component);
	}

	private void Clear()
	{
		foreach (UIAdminstratorItem mItem in mItemList)
		{
			if (mItem != null)
			{
				Object.Destroy(mItem.gameObject);
				mItem.gameObject.transform.parent = null;
			}
		}
		mItemList.Clear();
	}

	private void OnPersonnelBtn()
	{
		mPersonnelBg.SetActive(value: true);
		mBlackListBg.SetActive(value: false);
		mForbidsBuildBtn.SetActive(value: true);
		mForbidsNewPalyerBtn.SetActive(value: true);
		Ispersonnel = true;
		mCurrentPage = 1;
		ShowPageLsit(UIAdminstratorctr.mUIPersonelInfoList, mCurrentPage, UIAdminstratorctr.UIArrayPersonnelAdmin);
		if (UIAdminstratorctr.mUIPersonelInfoList.Count == 0)
		{
			MustShow();
		}
		Reflsh(UIAdminstratorctr.mUIPersonelInfoList);
	}

	private void OnBlackListBtn()
	{
		mPersonnelBg.SetActive(value: false);
		mBlackListBg.SetActive(value: true);
		mForbidsBuildBtn.SetActive(value: false);
		mForbidsNewPalyerBtn.SetActive(value: false);
		Ispersonnel = false;
		mCurrentPage = 1;
		ShowPageLsit(UIAdminstratorctr.mUIBalckInfoList, mCurrentPage, UIAdminstratorctr.UIArrayBlackAdmin);
		Reflsh(UIAdminstratorctr.mUIBalckInfoList);
	}

	private void OnActivate()
	{
		mPersonnelBg.SetActive(value: true);
	}

	private void ForbidsBuildBtn()
	{
		ForbidsBuild = !ForbidsBuild;
		mForbidsBuildBg.SetActive(ForbidsBuild);
		UIAdminstratorctr.FobidenAll(ForbidsBuild);
		Reflsh(UIAdminstratorctr.mUIPersonelInfoList);
	}

	private void ForbidsNewPalyerBtn()
	{
		ForbidsNewPalyer = !ForbidsNewPalyer;
		mForbidsNewPalyerBg.SetActive(ForbidsNewPalyer);
		ServerAdministrator.SetJoinGame(ForbidsNewPalyer);
		if (ForbidsNewPalyer)
		{
			Debug.Log("ForbidsNewPalyer!!");
		}
		else
		{
			Debug.Log("AllowNewPalyer!!");
		}
		Reflsh(UIAdminstratorctr.mUIPersonelInfoList);
	}

	private void SetBlackList(UserAdmin SetUser)
	{
		foreach (UIAdminstratorItem mItem in mItemList)
		{
			if (mItem.mUserAdmin.Id == SetUser.Id)
			{
				mItem.MoveToBlackList(SetUser.Id);
				break;
			}
		}
	}

	private void OnBanBtn()
	{
		foreach (UserAdmin mpitchAdmin in mpitchAdminList)
		{
			UIAdminstratorctr.UIAddBlacklist(mpitchAdmin);
		}
		mpitchAdminList.Clear();
	}

	private void MustShow()
	{
		UIAdminstratorctr.UIArrayPersonnelAdmin.Clear();
		UIAdminstratorctr.mUIPersonelInfoList.Clear();
		if (UIAdminstratorctr._mUserAdmin != null)
		{
			UIAdminstratorctr.mUIPersonelInfoList.Add(UIAdminstratorctr._mUserAdmin);
		}
		if (UIAdminstratorctr._mUserAdmin != null && UIAdminstratorctr._mSelfAdmin != null && UIAdminstratorctr._mUserAdmin != UIAdminstratorctr._mSelfAdmin)
		{
			UIAdminstratorctr.mUIPersonelInfoList.Add(UIAdminstratorctr._mSelfAdmin);
		}
	}

	private void OnBanAllBtn()
	{
		ArrayList uIArrayPersonnelAdmin = UIAdminstratorctr.UIArrayPersonnelAdmin;
		foreach (UserAdmin item in uIArrayPersonnelAdmin)
		{
			UIAdminstratorctr.UIAddBlacklist(item);
		}
		MustShow();
		Reflsh(UIAdminstratorctr.mUIPersonelInfoList);
		uIArrayPersonnelAdmin.Clear();
	}

	private void OnRemoveBtn()
	{
		foreach (UserAdmin mpitchAdmin in mpitchAdminList)
		{
			ServerAdministrator.RequestDeleteBlackList(mpitchAdmin.Id);
		}
		mpitchAdminList.Clear();
		ShowPageLsit(UIAdminstratorctr.mUIBalckInfoList, mCurrentPage, UIAdminstratorctr.UIArrayBlackAdmin);
		Reflsh(UIAdminstratorctr.mUIBalckInfoList);
	}

	private void OnRemoveAllBtn()
	{
		ServerAdministrator.RequestClearBlackList();
		UIAdminstratorctr.mUIBalckInfoList.Clear();
		UIAdminstratorctr.UIArrayBlackAdmin.Clear();
		mLbPage.text = "0/0";
		Reflsh(UIAdminstratorctr.mUIBalckInfoList);
	}

	private void OnRightBtn()
	{
		if (Ispersonnel)
		{
			if (mCurrentPage < mEndPage)
			{
				mCurrentPage++;
				ShowPageLsit(UIAdminstratorctr.mUIPersonelInfoList, mCurrentPage, UIAdminstratorctr.UIArrayPersonnelAdmin);
				Reflsh(UIAdminstratorctr.mUIPersonelInfoList);
			}
		}
		else if (mCurrentPage < mEndPage)
		{
			mCurrentPage++;
			ShowPageLsit(UIAdminstratorctr.mUIBalckInfoList, mCurrentPage, UIAdminstratorctr.UIArrayBlackAdmin);
			Reflsh(UIAdminstratorctr.mUIBalckInfoList);
		}
	}

	private void OnLeftBtn()
	{
		if (Ispersonnel)
		{
			if (mCurrentPage > 1)
			{
				mCurrentPage--;
				ShowPageLsit(UIAdminstratorctr.mUIPersonelInfoList, mCurrentPage, UIAdminstratorctr.UIArrayPersonnelAdmin);
				Reflsh(UIAdminstratorctr.mUIPersonelInfoList);
			}
		}
		else if (mCurrentPage > 1)
		{
			mCurrentPage--;
			ShowPageLsit(UIAdminstratorctr.mUIBalckInfoList, mCurrentPage, UIAdminstratorctr.UIArrayBlackAdmin);
			Reflsh(UIAdminstratorctr.mUIBalckInfoList);
		}
	}

	private void OnLeftEndBtn()
	{
		mCurrentPage = 1;
		if (Ispersonnel)
		{
			ShowPageLsit(UIAdminstratorctr.mUIPersonelInfoList, mCurrentPage, UIAdminstratorctr.UIArrayPersonnelAdmin);
			Reflsh(UIAdminstratorctr.mUIPersonelInfoList);
		}
		else
		{
			ShowPageLsit(UIAdminstratorctr.mUIBalckInfoList, mCurrentPage, UIAdminstratorctr.UIArrayBlackAdmin);
			Reflsh(UIAdminstratorctr.mUIBalckInfoList);
		}
	}

	private void OnRightEndBtn()
	{
		if (mEndPage <= 0)
		{
			mEndPage = 1;
		}
		mCurrentPage = mEndPage;
		if (Ispersonnel)
		{
			UIAdminstratorctr.mUIPersonelInfoList.Clear();
			for (int i = (mEndPage - 1) * 8; i < UIAdminstratorctr.UIArrayPersonnelAdmin.Count; i++)
			{
				UIAdminstratorctr.mUIPersonelInfoList.Add((UserAdmin)UIAdminstratorctr.UIArrayPersonnelAdmin[i]);
			}
			mLbPage.text = mCurrentPage + "/" + mEndPage;
			Reflsh(UIAdminstratorctr.mUIPersonelInfoList);
		}
		else
		{
			UIAdminstratorctr.mUIBalckInfoList.Clear();
			for (int j = (mEndPage - 1) * 8; j < UIAdminstratorctr.UIArrayBlackAdmin.Count; j++)
			{
				UIAdminstratorctr.mUIBalckInfoList.Add((UserAdmin)UIAdminstratorctr.UIArrayBlackAdmin[j]);
			}
			mLbPage.text = mCurrentPage + "/" + mEndPage;
			Reflsh(UIAdminstratorctr.mUIBalckInfoList);
		}
	}

	private void ItemAdminOnClick(object sender, UserAdmin userAdmin)
	{
		UIAdminstratorItem uIAdminstratorItem = sender as UIAdminstratorItem;
		if (uIAdminstratorItem != null && userAdmin != null)
		{
			Reflsh(UIAdminstratorctr.mUIPersonelInfoList);
		}
	}

	private void ItemAdminOnpitch(object sender, UserAdmin userAdmin, bool Ispitch)
	{
		UIAdminstratorItem uIAdminstratorItem = sender as UIAdminstratorItem;
		if (uIAdminstratorItem != null)
		{
			if (Ispitch)
			{
				mpitchAdminList.Add(userAdmin);
			}
			else
			{
				mpitchAdminList.Remove(userAdmin);
			}
		}
	}
}

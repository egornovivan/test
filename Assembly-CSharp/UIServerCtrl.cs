using System.Collections.Generic;
using Pathea;
using UnityEngine;

public class UIServerCtrl : MonoBehaviour
{
	public UIPageListCtrl mList;

	public GameObject mRoomWnd;

	public UILabel mSeverName;

	public UIInput mPassWord;

	public UILabel mGameType;

	public UILabel mGameMode;

	public UILabel mMonster;

	public UILabel mMapName;

	public UILabel mTeamNO;

	public UILabel mPlayerNo;

	public UILabel mSeedNo;

	public UILabel mBiome;

	public UILabel mClimate;

	public UICheckbox mCheckPrivate;

	public UICheckbox mCheckProxy;

	public UISprite mBtnTeamLeftBg;

	public UISprite mBtnTeamRightBg;

	public BoxCollider mBCTeamLeft;

	public BoxCollider mBCTeamRight;

	public UIHostCreateCtrl mHostCreateCtrl;

	private int mTeamCount = 1;

	private int mPlayerCount = 1;

	public event OnGuiBtnClicked StartFunc;

	public event OnGuiBtnClicked BtnStart;

	public event OnGuiBtnClicked BtnBack;

	public event OnGuiBtnClicked BtnRefresh;

	public event OnGuiBtnClicked BtnClose;

	public event OnGuiBtnClicked BtnCreate;

	public event OnGuiBtnClicked BtnDelete;

	public event OnGuiIndexBaseCallBack checkListItem;

	public void UpdateServerInfo(MyServer ms)
	{
		List<string> list = ms.ToServerInfo();
		if (list.Count >= 11)
		{
			mSeverName.text = list[0];
			mPassWord.text = list[1];
			mGameType.text = list[2];
			mGameMode.text = list[3];
			mMonster.text = list[4];
			mMapName.text = list[5];
			mTeamNO.text = list[6];
			mPlayerNo.text = list[7];
			mSeedNo.text = list[8];
			mBiome.text = list[9];
			mClimate.text = list[10];
			mCheckPrivate.isChecked = ms.isPrivate;
			mCheckProxy.isChecked = ms.proxyServer;
			if (PeGameMgr.IsVS || PeGameMgr.IsCustom)
			{
				SetTeamState(IsActive: true);
			}
			else
			{
				SetTeamState(IsActive: false);
			}
			int.TryParse(mTeamNO.text, out mTeamCount);
			int.TryParse(mPlayerNo.text, out mPlayerCount);
		}
	}

	public void GetMyServerInfo(MyServer ms)
	{
		if (ms != null)
		{
			ms.gamePassword = mPassWord.text;
			ms.teamNum = int.Parse(mTeamNO.text);
			ms.numPerTeam = int.Parse(mPlayerNo.text);
			ms.isPrivate = mCheckPrivate.isChecked;
			ms.proxyServer = mCheckProxy.isChecked;
		}
	}

	private void Awake()
	{
		mList.CheckItem += CheckListItem;
	}

	private void Start()
	{
		if (this.StartFunc != null)
		{
			this.StartFunc();
		}
	}

	private void Update()
	{
	}

	private void SetTeamState(bool IsActive)
	{
		float num = 1f;
		if (!IsActive)
		{
			num = 0.3f;
		}
		mBtnTeamLeftBg.color = new Color(num, num, num, 1f);
		mBtnTeamRightBg.color = new Color(num, num, num, 1f);
		mBCTeamLeft.enabled = IsActive;
		mBCTeamRight.enabled = IsActive;
	}

	private void BtnBackOnClick()
	{
		if (Input.GetMouseButtonUp(0))
		{
			base.gameObject.SetActive(value: false);
			mRoomWnd.SetActive(value: true);
			if (this.BtnBack != null)
			{
				this.BtnBack();
			}
		}
	}

	private void BtnCreateOnClick()
	{
		if (Input.GetMouseButtonUp(0))
		{
			if (this.BtnCreate != null)
			{
				this.BtnCreate();
			}
			mHostCreateCtrl.gameObject.SetActive(value: true);
			mHostCreateCtrl.InitMapInfo();
		}
	}

	private void BtnDeleteOnClick()
	{
		if (Input.GetMouseButtonUp(0) && mList.mSelectedIndex != -1 && mList.mSelectedIndex < mList.mItems.Count)
		{
			string text = mList.mItems[mList.mSelectedIndex].mData[0];
			string text2 = UIMsgBoxInfo.mCZ_DeleteSrever.GetString() + "'" + text + "'";
			MessageBox_N.ShowYNBox(text2, OnBtnDelete);
		}
	}

	private void OnBtnDelete()
	{
		if (this.BtnDelete != null)
		{
			this.BtnDelete();
		}
	}

	private void BtnStartOnClick()
	{
		if (Input.GetMouseButtonUp(0) && this.BtnStart != null)
		{
			this.BtnStart();
		}
	}

	private void BtnCloseOnClick()
	{
		if (Input.GetMouseButtonUp(0) && this.BtnClose != null)
		{
			this.BtnClose();
		}
	}

	private void CheckListItem(int index)
	{
		if (Input.GetMouseButtonUp(0) && this.checkListItem != null)
		{
			this.checkListItem(index);
		}
	}

	private void BtnRefreshOnClick()
	{
		if (this.BtnRefresh != null)
		{
			this.BtnRefresh();
		}
	}

	private void OnTeamNumSelectLeft()
	{
		if (Input.GetMouseButtonUp(0) && PeGameMgr.IsVS && mTeamCount >= 3)
		{
			mTeamCount--;
			mPlayerCount = 4;
			mTeamNO.text = mTeamCount.ToString();
			mPlayerNo.text = mPlayerCount.ToString();
		}
	}

	private void OnTeamNumSelectRight()
	{
		if (Input.GetMouseButtonUp(0) && PeGameMgr.IsVS && mTeamCount <= 3)
		{
			mTeamCount++;
			mPlayerCount = 4;
			mTeamNO.text = mTeamCount.ToString();
			mPlayerNo.text = mPlayerCount.ToString();
		}
	}

	private void OnPlayerNumSelectLeft()
	{
		if (!Input.GetMouseButtonUp(0))
		{
			return;
		}
		if (!PeGameMgr.IsSurvive)
		{
			if (mTeamCount * (mPlayerCount / 2) >= 1)
			{
				mPlayerCount /= 2;
				mPlayerNo.text = mPlayerCount.ToString();
			}
		}
		else
		{
			mPlayerCount = Mathf.Max(1, mPlayerCount / 2);
			mPlayerNo.text = mPlayerCount.ToString();
		}
	}

	private void OnPlayerNumSelectRight()
	{
		if (!Input.GetMouseButtonUp(0))
		{
			return;
		}
		if (!PeGameMgr.IsSurvive)
		{
			if (mTeamCount * (mPlayerCount * 2) <= 32)
			{
				mPlayerCount *= 2;
				mPlayerNo.text = mPlayerCount.ToString();
			}
		}
		else
		{
			mPlayerCount = Mathf.Min(32, mPlayerCount * 2);
			mPlayerNo.text = mPlayerCount.ToString();
		}
	}
}

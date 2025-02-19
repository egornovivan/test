using System;
using System.Collections.Generic;
using Pathea;
using Steamworks;
using UnityEngine;

public class PeSteamFriendMgr : MonoBehaviour
{
	internal class InviteInfo
	{
		public ulong inviteSteamId;

		public long serverUID;

		public string InviteName;

		public DateTime reciveTime;

		public InviteInfo(ulong steamId, long uid)
		{
			inviteSteamId = steamId;
			serverUID = uid;
			reciveTime = DateTime.Now;
		}
	}

	private static PeSteamFriendMgr mInstance;

	[HideInInspector]
	public UIFriendWnd mFriendWnd;

	private Dictionary<int, SteamFriendsData> mFriendsData;

	private SteamFriendsData mMyData;

	private Dictionary<int, BaseNetwork> mBaseNetWorkList;

	public long mMyServerUID = -1L;

	private int mCurrentIndex = -1;

	private bool mIsInvite;

	private Dictionary<long, InviteInfo> mInviteInfoMap = new Dictionary<long, InviteInfo>();

	private long inviteServerUID = -1L;

	private InviteInfo info;

	public static PeSteamFriendMgr Instance => mInstance;

	public bool IsInvite => mIsInvite;

	private void Awake()
	{
		mInstance = this;
		GameClientNetwork.OnDisconnectEvent += OnDisconnectServer;
	}

	private void OnDestroy()
	{
		GameClientNetwork.OnDisconnectEvent -= OnDisconnectServer;
	}

	public void Init(Transform tsTopLeftAnthor, Transform tsCenterAnthor, Camera uiCamera)
	{
		InitUIFriendWnd(tsTopLeftAnthor);
		SteamFriendPrcMgr.Instance.Init(CallBackGetFriends, CallBackRecvMsg, CallBackPersonStateChange);
		UIFriendWnd uIFriendWnd = mFriendWnd;
		uIFriendWnd.e_OnShow = (UIBaseWidget.WndEvent)Delegate.Combine(uIFriendWnd.e_OnShow, new UIBaseWidget.WndEvent(ReflashFriendWnd));
		mFriendWnd.e_TabChange += ReflashFriendWnd;
		RoomGui_N instance = RoomGui_N.Instance;
		instance.e_OnShow = (UIBaseWidget.WndEvent)Delegate.Combine(instance.e_OnShow, new UIBaseWidget.WndEvent(ReflashFriendWnd));
		mFriendWnd.e_ShowFriendMenu += ShowMenu;
		mFriendWnd.InitOptionMenu(tsCenterAnthor, uiCamera);
		mFriendWnd.InitInviteBox(tsTopLeftAnthor);
		GetFriends();
		mBaseNetWorkList = BaseNetwork.GetBaseNetworkList();
		mMyData = SteamFriendPrcMgr.Instance.GetMyInfo();
		if (mMyData != null)
		{
			mFriendWnd.SetMyInfo(mMyData._PlayerName, mMyData._avatar);
		}
		else
		{
			mFriendWnd.SetMyInfo(string.Empty, null);
		}
	}

	private void InitUIFriendWnd(Transform tsPartent)
	{
		GameObject gameObject = UnityEngine.Object.Instantiate(Resources.Load("Prefab/GameUI/FriendWnd")) as GameObject;
		mFriendWnd = gameObject.GetComponent<UIFriendWnd>();
		mFriendWnd.gameObject.transform.parent = tsPartent;
		mFriendWnd.transform.localScale = Vector3.one;
		mFriendWnd.transform.localPosition = new Vector3(-160f, -400f, -10f);
	}

	private void GetFriends()
	{
		SteamFriendPrcMgr.Instance.GetFriends();
	}

	private void CallBackGetFriends(Dictionary<int, SteamFriendsData> friendsList, bool bOK)
	{
		if (bOK)
		{
			mFriendsData = friendsList;
			if (null != mFriendWnd && mFriendWnd.isShow)
			{
				ReflashFriendWnd();
			}
		}
	}

	private void CallBackPersonStateChange(int index)
	{
		ReflashFriendWnd();
	}

	private void CallBackRecvMsg(int index, string text)
	{
	}

	private void ReflashFriendWnd(UIBaseWidget widget = null)
	{
		if (mFriendWnd == null || SteamFriendPrcMgr.Instance == null)
		{
			return;
		}
		mFriendWnd.EnableTabRoomPalyer(BaseNetwork.IsInRoom());
		mFriendWnd.ClearList();
		if (mFriendWnd.mTabState == UIFriendWnd.TabState.state_Friend)
		{
			if (mFriendsData != null)
			{
				foreach (KeyValuePair<int, SteamFriendsData> mFriendsDatum in mFriendsData)
				{
					mFriendWnd.AddListItem(GetFriendInfo(mFriendsDatum.Value), mFriendsDatum.Value._avatar, mFriendsDatum.Key, mFriendsDatum.Value._PlayerState != EPersonaState.k_EPersonaStateOffline);
				}
			}
		}
		else if (mFriendWnd.mTabState == UIFriendWnd.TabState.state_Palyer)
		{
			Dictionary<int, BaseNetwork> baseNetworkList = BaseNetwork.GetBaseNetworkList();
			if (baseNetworkList != null)
			{
				foreach (KeyValuePair<int, BaseNetwork> item in baseNetworkList)
				{
					mFriendWnd.AddListItem(GetPalyerInfo(item.Value), null, item.Key, isOnline: true);
				}
			}
		}
		mFriendWnd.RepostionList();
	}

	private string GetPalyerInfo(BaseNetwork _base)
	{
		return _base.name;
	}

	private string GetFriendInfo(SteamFriendsData data)
	{
		string text = data._PlayerName;
		int playerState = (int)data._PlayerState;
		if (data._PlayedGameName != null && data._PlayedGameName.Length > 0)
		{
			text = text + "[00ff00][Playing][-][ff9900]" + data._PlayedGameName + "[-]";
		}
		else
		{
			switch (playerState)
			{
			case 0:
				text += "[Off-line]";
				break;
			case 1:
			case 2:
			case 3:
			case 4:
			case 5:
			case 6:
				text += "[00ff00][Online][-]";
				break;
			}
		}
		return text;
	}

	private void ShowMenu(int index)
	{
		mFriendWnd.mOptionMenu.Clear();
		if (mFriendWnd.mTabState == UIFriendWnd.TabState.state_Friend)
		{
			mFriendWnd.mOptionMenu.AddOption("ChatTo", ChatTo);
			if (BaseNetwork.IsInRoom())
			{
				mFriendWnd.mOptionMenu.AddOption("Invite", Invite);
			}
			mFriendWnd.mOptionMenu.AddOption("Delete", FriendRemove);
		}
		else if (mFriendWnd.mTabState == UIFriendWnd.TabState.state_Palyer)
		{
			if (mBaseNetWorkList[index].SteamID.m_SteamID == mMyData._SteamID.m_SteamID)
			{
				return;
			}
			if (BaseNetwork.IsInRoom())
			{
				mFriendWnd.mOptionMenu.AddOption("Add Friend", AddFriend);
			}
		}
		mFriendWnd.mOptionMenu.Show();
		mCurrentIndex = index;
	}

	private void ChatTo(object sender)
	{
		mFriendWnd.mOptionMenu.Hide();
		if (mFriendsData.ContainsKey(mCurrentIndex))
		{
			SteamFriendPrcMgr.Instance.ChatTo(mFriendsData[mCurrentIndex]._SteamID.m_SteamID);
		}
	}

	private void Invite(object sender)
	{
		mFriendWnd.mOptionMenu.Hide();
		if (mFriendsData.ContainsKey(mCurrentIndex) && mMyServerUID != -1)
		{
			SteamFriendPrcMgr.Instance.Invite(mFriendsData[mCurrentIndex]._SteamID.m_SteamID, mMyServerUID);
		}
	}

	private void FriendRemove(object sender)
	{
		mFriendWnd.mOptionMenu.Hide();
		if (mFriendsData.ContainsKey(mCurrentIndex))
		{
			SteamFriendPrcMgr.Instance.FriendRemove(mFriendsData[mCurrentIndex]._SteamID.m_SteamID);
		}
	}

	private void AddFriend(object sender)
	{
		mFriendWnd.mOptionMenu.Hide();
		if (mBaseNetWorkList.ContainsKey(mCurrentIndex))
		{
			SteamFriendPrcMgr.Instance.FriendAdd(mBaseNetWorkList[mCurrentIndex].SteamID.m_SteamID);
		}
	}

	private void OnDisconnectServer()
	{
		mMyServerUID = -1L;
	}

	public void ReciveInvite(ulong inviteSteamId, long serverUID)
	{
		if (serverUID != mMyServerUID)
		{
			InviteInfo value = new InviteInfo(inviteSteamId, serverUID);
			mInviteInfoMap[serverUID] = value;
		}
	}

	private void Update()
	{
		if (!PeGameMgr.IsSingle && !(mFriendWnd == null) && UILobbyMainWndCtrl.Instance.bGetRoomInfo)
		{
			if (PeSingleton<PeFlowMgr>.Instance.curScene == PeFlowMgr.EPeScene.LobbyScene)
			{
				UpdateInviteState();
			}
			else if (PeSingleton<PeFlowMgr>.Instance.curScene == PeFlowMgr.EPeScene.GameScene)
			{
				UpdateInviteState();
			}
		}
	}

	private void UpdateInviteState()
	{
		if (!mFriendWnd.mInviteBox.isShow && mInviteInfoMap.Count > 0)
		{
			using (Dictionary<long, InviteInfo>.Enumerator enumerator = mInviteInfoMap.GetEnumerator())
			{
				if (enumerator.MoveNext())
				{
					info = enumerator.Current.Value;
					ShowInviteMsgBox();
				}
				return;
			}
		}
		if (inviteServerUID != -1 && mInviteInfoMap.Count == 0)
		{
			if (PeSingleton<PeFlowMgr>.Instance.curScene == PeFlowMgr.EPeScene.GameScene)
			{
				PeSceneCtrl.Instance.GotoLobbyScene();
			}
			else if (PeSingleton<PeFlowMgr>.Instance.curScene == PeFlowMgr.EPeScene.LobbyScene)
			{
				InviteJionRomm();
			}
		}
	}

	private void ShowInviteMsgBox()
	{
		string msg = info.InviteName + " " + UIMsgBoxInfo.mCZ_InvitePlayer.GetString() + "(" + info.reciveTime.ToString("yyyy-MM-dd HH:mm") + ")";
		mFriendWnd.mInviteBox.ShowMsg(msg, JionCallBack, CancelCallBack, IgnorAllCallBack, TimeOutCallBack);
	}

	private void JionCallBack()
	{
		inviteServerUID = info.serverUID;
		mInviteInfoMap.Remove(info.serverUID);
		mFriendWnd.mInviteBox.Hide();
	}

	private void CancelCallBack()
	{
		mInviteInfoMap.Remove(info.serverUID);
		inviteServerUID = -1L;
		mFriendWnd.mInviteBox.Hide();
	}

	private void IgnorAllCallBack()
	{
		mInviteInfoMap.Clear();
		inviteServerUID = -1L;
		mFriendWnd.mInviteBox.Hide();
	}

	private void TimeOutCallBack()
	{
		CancelCallBack();
	}

	private void InviteJionRomm()
	{
		if (!(UILobbyMainWndCtrl.Instance == null) && UILobbyMainWndCtrl.Instance.HaveServerID(inviteServerUID))
		{
			UILobbyMainWndCtrl.Instance.SetInviteServerData(inviteServerUID);
			UILobbyMainWndCtrl.Instance.JoinToServerByInvite();
			inviteServerUID = -1L;
		}
	}
}

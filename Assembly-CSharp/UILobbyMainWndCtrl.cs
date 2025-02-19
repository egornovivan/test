using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using CustomData;
using Pathea;
using uLink;
using uLobby;
using UnityEngine;

public class UILobbyMainWndCtrl : UIStaticWnd
{
	public enum SortType
	{
		mLock,
		mRoomNO,
		mPalyerNO
	}

	public delegate void LobbyUIStart();

	private static UILobbyMainWndCtrl mInstance;

	public Camera mUICamera;

	public GameObject mRoomWnd;

	public GameObject mButtomAuthor;

	public GameObject mTopLeftAuthor;

	public GameObject mUICenter;

	public GameObject mUIServerWnd;

	public GameObject mPassWordWnd;

	public GameObject mWorkShopPrfab;

	public GameObject mHostCreateWnd;

	public int mPlayerPageIndex;

	public UIPageListCtrl mPlayerList;

	public int mRoomListPage;

	public UIPageListCtrl mRoomList;

	private RoleInfo mRoleInfo;

	public UITexture mTxPlayerHerder;

	public UILabel mLbPlayerName;

	public UILabel mLbPlayerLv;

	public UIInput mRoomInput;

	public UIInput mPlayerInput;

	public UIInput mCheckPasswordInput;

	public UIInput mMsgText;

	public UITalkBoxCtrl mTalkBoxControl;

	private RecentRoomDataManager mRecentRoom_M;

	private List<ServerRegistered> _curServerList = new List<ServerRegistered>();

	private List<ServerRegistered> _serverListInter = new List<ServerRegistered>();

	private List<ServerRegistered> _serverListLan = new List<ServerRegistered>();

	internal ServerRegistered mSelectServerData;

	private UIWorkShopCtrl mWorkShopCtrl;

	internal ServerRegistered mInviteServerData;

	private PeLobbyLevel lobbyLevel;

	[HideInInspector]
	public bool bGetRoomInfo;

	private bool LockRoomList;

	private bool bSortDn = true;

	private SortType mSortType = SortType.mRoomNO;

	private long roomUID;

	private int checkIndex = -1;

	private string QueryRoomText = string.Empty;

	private string QueryPlayerText = string.Empty;

	private GameObject mLastWnd;

	[SerializeField]
	private GameObject mallWndPrefab;

	private UIMallWnd mMallWnd;

	public static UILobbyMainWndCtrl Instance => mInstance;

	public long RoomUID => roomUID;

	public event LobbyUIStart e_LobbyUIStart;

	private void Awake()
	{
		mInstance = this;
		PlayMusic();
		mPlayerPageIndex = 0;
		mRoomListPage = 0;
		mRoomList.CheckItem += roomListChickItem;
		mRoomList.DoubleClickItem += RoomListDoubleClickItem;
	}

	public override void Show()
	{
		mButtomAuthor.gameObject.SetActive(value: true);
		base.Show();
	}

	protected override void OnHide()
	{
		mButtomAuthor.gameObject.SetActive(value: false);
		base.OnHide();
	}

	public void SetInviteServerData(long serverUID)
	{
		mInviteServerData = null;
		if (mInviteServerData == null)
		{
			mInviteServerData = _serverListInter.Find((ServerRegistered sr) => sr.ServerUID == serverUID);
		}
		if (mInviteServerData == null)
		{
			mInviteServerData = _serverListLan.Find((ServerRegistered sr) => sr.ServerUID == serverUID);
		}
	}

	public bool HaveServerID(long serverUID)
	{
		foreach (ServerRegistered item in _serverListInter)
		{
			if (item.ServerUID == serverUID)
			{
				return true;
			}
		}
		foreach (ServerRegistered item2 in _serverListLan)
		{
			if (item2.ServerUID == serverUID)
			{
				return true;
			}
		}
		return false;
	}

	private void Start()
	{
		PeSteamFriendMgr.Instance.Init(mTopLeftAuthor.transform, mUICenter.transform, mUICamera);
		if (!(GameClientLobby.Self == null))
		{
			mRoleInfo = GameClientLobby.role;
			SetRoleInfo();
			mRecentRoom_M = new RecentRoomDataManager(mRoleInfo.name);
			mRecentRoom_M.LoadFromFile();
			StartCoroutine(UpdatePlayerInfo());
			StartCoroutine(UpdateRoomInfo());
			InitRoomListSort();
			if (this.e_LobbyUIStart != null)
			{
				this.e_LobbyUIStart();
			}
		}
	}

	private void InitRoomListSort()
	{
		for (int i = 0; i < mRoomList.mHeaderItems.Count; i++)
		{
			if (i == 0 || i == 3)
			{
				mRoomList.mHeaderItems[i].InitSort(CanSort: true);
				if (i == 0)
				{
					mRoomList.mHeaderItems[i].SetSortSatate(2);
				}
				else
				{
					mRoomList.mHeaderItems[i].SetSortSatate(0);
				}
				mRoomList.mHeaderItems[i].eSortOnClick += OnClickSort;
			}
			else
			{
				mRoomList.mHeaderItems[i].InitSort(CanSort: false);
			}
		}
	}

	private void SetRoleInfo()
	{
		if (mRoleInfo != null)
		{
			mLbPlayerName.text = mRoleInfo.name;
			ResetLevel();
			Texture2D texture = RoleHerderTexture.GetTexture();
			if (texture != null)
			{
				mTxPlayerHerder.mainTexture = texture;
			}
		}
	}

	private void Update()
	{
		UpdateLobbyLevel();
	}

	private void UpdateLobbyLevel()
	{
		if (lobbyLevel != null && mRoleInfo.lobbyExp >= (float)(lobbyLevel.exp + lobbyLevel.nextExp))
		{
			ResetLevel();
		}
	}

	private void ResetLevel()
	{
		lobbyLevel = PeSingleton<PeLobbyLevel.Mgr>.Instance.GetLevel(mRoleInfo.lobbyExp);
		mLbPlayerLv.text = ((lobbyLevel == null) ? "0" : lobbyLevel.level.ToString());
	}

	public void PlayMusic()
	{
	}

	public void AddTalk(string name, string content)
	{
		if (!(mTalkBoxControl == null))
		{
			if (GameClientLobby.role.name == name)
			{
				mTalkBoxControl.AddMsg(name, content, "99C68B");
			}
			else
			{
				mTalkBoxControl.AddMsg(name, content, "EDB1A6");
			}
		}
	}

	private IEnumerator UpdatePlayerInfo()
	{
		while (true)
		{
			RefreshPlayerList();
			yield return new WaitForSeconds(1f);
		}
	}

	public void RefreshPlayerList()
	{
		if (GameClientLobby.Self == null)
		{
			return;
		}
		if (mPlayerPageIndex == 0)
		{
			List<RoleInfoProxy> rolesInLobby = GameClientLobby.Self.m_RolesInLobby;
			mPlayerList.mItems.Clear();
			for (int i = 0; i < rolesInLobby.Count; i++)
			{
				List<string> list = new List<string>();
				list.Add(rolesInLobby[i].name);
				if (QueryPlayerText.Length > 0)
				{
					if (QueryItem(QueryPlayerText, list[0]))
					{
						mPlayerList.AddItem(list);
					}
				}
				else
				{
					mPlayerList.AddItem(list);
				}
			}
			mPlayerList.UpdateList();
		}
		else if (mPlayerPageIndex == 1)
		{
			mPlayerList.mItems.Clear();
			mPlayerList.UpdateList();
		}
	}

	private IEnumerator UpdateRoomInfo()
	{
		mRoomListPage = 0;
		uLink.MasterServer.ipAddress = ClientConfig.ProxyIP;
		uLink.MasterServer.port = ClientConfig.ProxyPort;
		uLink.MasterServer.password = "patheahaha";
		uLink.MasterServer.updateRate = 4f;
		uLink.MasterServer.RequestHostList("PatheaGame");
		uLink.MasterServer.DiscoverLocalHosts("PatheaGame", 9900, 9915);
		yield return new WaitForSeconds(3f);
		while (true)
		{
			_serverListInter.Clear();
			_serverListLan.Clear();
			if (Lobby.isConnected)
			{
				IEnumerable<ServerInfo> lobbySrvs = ServerRegistry.GetServers();
				foreach (ServerInfo server in lobbySrvs)
				{
					ServerRegistered reg = new ServerRegistered();
					reg.AnalyseServer(server);
					_serverListInter.Add(reg);
				}
			}
			uLink.HostData[] servers = uLink.MasterServer.PollHostList();
			uLink.HostData[] array = servers;
			foreach (uLink.HostData server2 in array)
			{
				ProxyServerRegistered reg2 = new ProxyServerRegistered();
				reg2.AnalyseServer(server2, isLan: false);
				_serverListInter.Add(reg2);
			}
			uLink.HostData[] datas = uLink.MasterServer.PollDiscoveredHosts();
			uLink.HostData[] array2 = datas;
			foreach (uLink.HostData data in array2)
			{
				ServerRegistered server3 = new ServerRegistered();
				server3.AnalyseServer(data, isLan: true);
				_serverListLan.Add(server3);
			}
			uLink.MasterServer.ClearHostList();
			uLink.MasterServer.ClearDiscoveredHosts();
			RefreshRoomList();
			uLink.MasterServer.RequestHostList("PatheaGame");
			uLink.MasterServer.DiscoverLocalHosts("PatheaGame", 9900, 9915);
			bGetRoomInfo = true;
			yield return new WaitForSeconds(5f);
		}
	}

	private void LockSortOnClick()
	{
		OnClickSort(100, mRoomList.mLockSortState);
	}

	private void OnClickSort(int index, int sortSatae)
	{
		int num = -1;
		switch (sortSatae)
		{
		case 0:
			num = 2;
			bSortDn = true;
			break;
		case 1:
			num = 2;
			bSortDn = true;
			break;
		case 2:
			num = 1;
			bSortDn = false;
			break;
		default:
			return;
		}
		switch (index)
		{
		case 0:
			mSortType = SortType.mRoomNO;
			mRoomList.mHeaderItems[3].SetSortSatate(0);
			mRoomList.mHeaderItems[0].SetSortSatate(num);
			mRoomList.SetLockUIState(0);
			break;
		case 3:
			mSortType = SortType.mPalyerNO;
			mRoomList.mHeaderItems[0].SetSortSatate(0);
			mRoomList.mHeaderItems[3].SetSortSatate(num);
			mRoomList.SetLockUIState(0);
			break;
		case 100:
			mSortType = SortType.mLock;
			mRoomList.mHeaderItems[0].SetSortSatate(0);
			mRoomList.mHeaderItems[3].SetSortSatate(0);
			mRoomList.SetLockUIState(num);
			break;
		default:
			return;
		}
		RefreshRoomList();
	}

	private void SortRoomList()
	{
		_curServerList.Sort(delegate(ServerRegistered _one, ServerRegistered _two)
		{
			if (object.ReferenceEquals(_one, null))
			{
				if (object.ReferenceEquals(_two, null))
				{
					return 0;
				}
				return -1;
			}
			if (_one.ServerID == _two.ServerID)
			{
				return 0;
			}
			return (_one.ServerID > _two.ServerID) ? 1 : (-1);
		});
		int returnValue = (bSortDn ? 1 : (-1));
		_curServerList.Sort(delegate(ServerRegistered _one, ServerRegistered _two)
		{
			if (object.ReferenceEquals(_one, null))
			{
				if (object.ReferenceEquals(_two, null))
				{
					return 0;
				}
				return -1;
			}
			if (mSortType == SortType.mLock)
			{
				if (_one.PasswordStatus == _two.PasswordStatus)
				{
					return 0;
				}
				if (_one.PasswordStatus > _two.PasswordStatus)
				{
					return returnValue;
				}
				return -returnValue;
			}
			if (mSortType == SortType.mPalyerNO)
			{
				if (_one.CurConn == _two.CurConn)
				{
					return 0;
				}
				if (_one.CurConn > _two.CurConn)
				{
					return returnValue;
				}
				return -returnValue;
			}
			if (_one.ServerID == _two.ServerID)
			{
				return 0;
			}
			return (_one.ServerID > _two.ServerID) ? returnValue : (-returnValue);
		});
	}

	private void roomListChickItem(int index)
	{
		if (mRoomListPage == 0 || mRoomListPage == 1)
		{
			if (index >= 0 && index < _curServerList.Count)
			{
				roomUID = _curServerList[index].ServerUID;
				mSelectServerData = _curServerList[index];
				checkIndex = index;
			}
			else
			{
				checkIndex = -1;
			}
		}
		else
		{
			if (mRoomListPage != 2)
			{
				return;
			}
			if (index >= 0 && index < mRecentRoom_M.mRecentRoomList.Count)
			{
				roomUID = mRecentRoom_M.mRecentRoomList[index].mUID;
				ServerRegistered serverRegistered = _curServerList.Find((ServerRegistered sr) => sr.ServerUID == roomUID);
				mSelectServerData = serverRegistered;
				checkIndex = index;
			}
			else
			{
				checkIndex = -1;
			}
		}
	}

	private void RoomListDoubleClickItem(int index)
	{
		BtnJoinOnClick();
	}

	private List<string> ServerDataToList(ServerRegistered mServerData)
	{
		List<string> list = new List<string>();
		if (mServerData.ServerID <= -1)
		{
			list.Add("[6666FF]OFFICIAL");
			list.Add("[6666FF]" + mServerData.ServerName);
		}
		else if (mServerData.UseProxy)
		{
			list.Add("[99CC00]Proxy");
			list.Add("[99CC00]" + mServerData.ServerName);
		}
		else
		{
			list.Add(mServerData.ServerID.ToString());
			list.Add(mServerData.ServerName);
		}
		list.Add(mServerData.ServerMasterName);
		list.Add(mServerData.CurConn + "/" + mServerData.LimitedConn);
		list.Add((PeGameMgr.EGameType)mServerData.GameType switch
		{
			PeGameMgr.EGameType.Cooperation => "Cooperation", 
			PeGameMgr.EGameType.VS => "VS", 
			PeGameMgr.EGameType.Survive => "Survive", 
			_ => "Cooperation", 
		});
		list.Add((PeGameMgr.ESceneMode)mServerData.GameMode switch
		{
			PeGameMgr.ESceneMode.Adventure => "Adventure", 
			PeGameMgr.ESceneMode.Build => "Build", 
			PeGameMgr.ESceneMode.Custom => "Custom", 
			PeGameMgr.ESceneMode.Story => "Story", 
			_ => "Adventure", 
		});
		list.Add(mServerData.Ping.ToString());
		string item = (((mServerData.ServerStatus & 1) != 1) ? "InProgress" : "Waiting");
		list.Add(item);
		list.Add(mServerData.ServerVersion);
		return list;
	}

	private void RefreshRoomList()
	{
		if (mRoomList == null)
		{
			return;
		}
		_curServerList.Clear();
		if (mRoomListPage == 0)
		{
			foreach (ServerRegistered item in _serverListInter)
			{
				if (QueryRoomText.Length > 0)
				{
					if (QueryItem(QueryRoomText, item.ServerID.ToString()) || QueryItem(QueryRoomText, item.ServerName))
					{
						_curServerList.Add(item);
					}
				}
				else
				{
					_curServerList.Add(item);
				}
			}
		}
		else if (mRoomListPage == 1)
		{
			foreach (ServerRegistered item2 in _serverListLan)
			{
				if (QueryRoomText.Length > 0)
				{
					if (QueryItem(QueryRoomText, item2.ServerID.ToString()) || QueryItem(QueryRoomText, item2.ServerName))
					{
						_curServerList.Add(item2);
					}
				}
				else
				{
					_curServerList.Add(item2);
				}
			}
		}
		else if (mRoomListPage == 2)
		{
			foreach (ServerRegistered item3 in _serverListLan)
			{
				_curServerList.Add(item3);
			}
			foreach (ServerRegistered item4 in _serverListInter)
			{
				_curServerList.Add(item4);
			}
		}
		SortRoomList();
		if (mRoomListPage == 0 || mRoomListPage == 1)
		{
			mRoomList.mItems.Clear();
			int mSelectedIndex = -1;
			for (int i = 0; i < _curServerList.Count; i++)
			{
				List<string> mData = ServerDataToList(_curServerList[i]);
				PageListItem pageListItem = new PageListItem();
				pageListItem.mData = mData;
				pageListItem.mColor = Color.white;
				pageListItem.mEanbleICon = _curServerList[i].PasswordStatus == 1;
				mRoomList.AddItem(pageListItem);
				if (roomUID == _curServerList[i].ServerUID)
				{
					mSelectedIndex = i;
				}
			}
			mRoomList.mSelectedIndex = mSelectedIndex;
			mRoomList.UpdateList();
		}
		else
		{
			if (mRoomListPage != 2)
			{
				return;
			}
			mRoomList.mItems.Clear();
			int mSelectedIndex2 = -1;
			for (int j = 0; j < mRecentRoom_M.mRecentRoomList.Count; j++)
			{
				long UID = mRecentRoom_M.mRecentRoomList[j].mUID;
				ServerRegistered serverRegistered = _curServerList.Find((ServerRegistered sr) => sr.ServerUID == UID);
				if (serverRegistered != null)
				{
					List<string> list = ServerDataToList(serverRegistered);
					PageListItem pageListItem2 = new PageListItem();
					pageListItem2.mData = list;
					pageListItem2.mColor = Color.white;
					pageListItem2.mEanbleICon = serverRegistered.PasswordStatus == 1;
					if (QueryRoomText.Length > 0 && list.Count >= 2)
					{
						if (QueryItem(QueryRoomText, list[0]) || QueryItem(QueryRoomText, list[1]))
						{
							mRoomList.AddItem(pageListItem2);
						}
					}
					else
					{
						mRoomList.AddItem(pageListItem2);
					}
				}
				else
				{
					List<string> list2 = new List<string>();
					list2.Add(string.Empty);
					list2.Add(mRecentRoom_M.mRecentRoomList[j].mRoomName);
					list2.Add(mRecentRoom_M.mRecentRoomList[j].mCreator);
					list2.Add(string.Empty);
					list2.Add(string.Empty);
					list2.Add(string.Empty);
					list2.Add(string.Empty);
					list2.Add(string.Empty);
					list2.Add(mRecentRoom_M.mRecentRoomList[j].mVersion);
					PageListItem pageListItem3 = new PageListItem();
					pageListItem3.mData = list2;
					pageListItem3.mColor = Color.gray;
					pageListItem3.mEanbleICon = false;
					if (QueryRoomText.Length > 0 && list2.Count >= 2)
					{
						if (QueryItem(QueryRoomText, list2[1]))
						{
							mRoomList.AddItem(pageListItem3);
						}
					}
					else
					{
						mRoomList.AddItem(pageListItem3);
					}
				}
				if (roomUID == UID)
				{
					mSelectedIndex2 = j;
				}
			}
			mRoomList.mSelectedIndex = mSelectedIndex2;
			mRoomList.UpdateList();
		}
	}

	private bool QueryItem(string text, string ItemName)
	{
		try
		{
			if (text.Trim().Length == 0)
			{
				return true;
			}
			ItemName = ItemName.ToLower();
			Regex regex = new Regex(text);
			Match match = regex.Match(ItemName);
			if (match.Success)
			{
				return true;
			}
			return false;
		}
		catch
		{
			return false;
		}
	}

	private void SendMsg()
	{
		string text = mMsgText.text;
		text += ((!SystemSettingData.Instance.IsChinese) ? "[lang:other]" : "[lang:cn]");
		LobbyInterface.LobbyRPC(ELobbyMsgType.SendMsg, EMsgType.ToAll, GameClientLobby.role.steamId, GameClientLobby.role.name, text);
		mMsgText.text = string.Empty;
		Invoke("GetInputFocus", 0.1f);
	}

	private void GetInputFocus()
	{
		mMsgText.selected = true;
	}

	private void BtnQueryRoomOnClick()
	{
		QueryRoomText = mRoomInput.text;
		QueryRoomText = QueryRoomText.Replace("*", string.Empty);
		QueryRoomText = QueryRoomText.Replace("$", string.Empty);
		QueryRoomText = QueryRoomText.Replace("(", string.Empty);
		QueryRoomText = QueryRoomText.Replace(")", string.Empty);
		QueryRoomText = QueryRoomText.Replace("@", string.Empty);
		QueryRoomText = QueryRoomText.Replace("^", string.Empty);
		QueryRoomText = QueryRoomText.Replace("[", string.Empty);
		QueryRoomText = QueryRoomText.Replace("]", string.Empty);
		QueryRoomText = QueryRoomText.Replace(" ", string.Empty);
		mRoomInput.text = QueryRoomText;
		QueryRoomText = QueryRoomText.ToLower();
		RefreshRoomList();
	}

	private void BtnClearQueryRoomOnClick()
	{
		QueryRoomText = string.Empty;
		mRoomInput.text = string.Empty;
		RefreshRoomList();
	}

	private void BtnSearchPlayerOnClick()
	{
		QueryPlayerText = mPlayerInput.text;
		QueryPlayerText = QueryPlayerText.Replace("*", string.Empty);
		QueryPlayerText = QueryPlayerText.Replace("$", string.Empty);
		QueryPlayerText = QueryPlayerText.Replace("(", string.Empty);
		QueryPlayerText = QueryPlayerText.Replace(")", string.Empty);
		QueryPlayerText = QueryPlayerText.Replace("@", string.Empty);
		QueryPlayerText = QueryPlayerText.Replace("^", string.Empty);
		QueryPlayerText = QueryPlayerText.Replace("[", string.Empty);
		QueryPlayerText = QueryPlayerText.Replace("]", string.Empty);
		QueryPlayerText = QueryPlayerText.Replace(" ", string.Empty);
		mPlayerInput.text = QueryPlayerText;
		QueryPlayerText = QueryPlayerText.ToLower();
		RefreshPlayerList();
	}

	private void BtnClearPlayerOnClick()
	{
		QueryPlayerText = string.Empty;
		mPlayerInput.text = string.Empty;
		RefreshPlayerList();
	}

	private void ListTitleAllOnActive(bool isActive)
	{
		if (isActive)
		{
			mPlayerPageIndex = 0;
			mPlayerList.UpdateList();
		}
	}

	private void ListTitleFriendsOnActive(bool isActive)
	{
		if (isActive)
		{
			mPlayerPageIndex = 1;
			mPlayerList.UpdateList();
		}
	}

	private void ListTitleInternetOnActive(bool isActive)
	{
		if (isActive)
		{
			mRoomListPage = 0;
			roomUID = 0L;
			mRoomList.ClearSelected();
			RefreshRoomList();
		}
	}

	private void ListTitleLanOnActive(bool isActive)
	{
		if (isActive)
		{
			mRoomListPage = 1;
			roomUID = 0L;
			mRoomList.ClearSelected();
			RefreshRoomList();
		}
	}

	private void ListTitleRecentOnActive(bool isActive)
	{
		if (isActive)
		{
			mRoomListPage = 2;
			roomUID = 0L;
			mRoomList.ClearSelected();
			RefreshRoomList();
		}
	}

	private void BtnCharacterOnClick()
	{
		if (Input.GetMouseButtonUp(0) && GameClientLobby.Self != null)
		{
			GameClientLobby.Self.BackToRole();
		}
	}

	private void BtnMainMenuOnClick()
	{
		if (Input.GetMouseButtonUp(0))
		{
			MessageBox_N.ShowYNBox(PELocalization.GetString(8000052), delegate
			{
				StopCoroutine(UpdatePlayerInfo());
				StopCoroutine(UpdateRoomInfo());
				PeSceneCtrl.Instance.GotoMainMenuScene();
			});
		}
	}

	private void BtnRefreshOnClick()
	{
	}

	private void BtnDeleteOnClick()
	{
		if (!Input.GetMouseButtonUp(0))
		{
			return;
		}
		if (mRoomListPage == 0 || mRoomListPage == 1)
		{
			if (mRoomList.mSelectedIndex > -1 && mRoomList.mSelectedIndex < _curServerList.Count)
			{
				if (!GameClientLobby.role.name.Equals(_curServerList[mRoomList.mSelectedIndex].ServerMasterName))
				{
					return;
				}
				LobbyInterface.LobbyRPC(ELobbyMsgType.CloseServer, _curServerList[mRoomList.mSelectedIndex].ServerID, SteamMgr.steamId.m_SteamID);
				MessageBox_N.ShowMaskBox(MsgInfoType.ServerDeleteMask, PELocalization.GetString(8000058), 15f);
			}
		}
		else if (mRoomListPage == 2 && roomUID != 0L)
		{
			mRecentRoom_M.DeleteItem(roomUID);
		}
		roomUID = 0L;
		RefreshRoomList();
		if (checkIndex != -1)
		{
			roomListChickItem(checkIndex);
		}
	}

	private void BtnHostOnClick()
	{
		if (Input.GetMouseButtonUp(0))
		{
			if (!UnityEngine.Network.HavePublicAddress())
			{
				MessageBox_N.ShowOkBox(PELocalization.GetString(8000057), ShowUIServerWnd);
			}
			else
			{
				ShowUIServerWnd();
			}
		}
	}

	private void ShowUIServerWnd()
	{
		mRoomWnd.SetActive(value: false);
		mUIServerWnd.SetActive(value: true);
	}

	private void BtnJoinOnClick()
	{
		if (!Input.GetMouseButtonUp(0) || roomUID == 0L)
		{
			return;
		}
		if (mSelectServerData == null)
		{
			MessageBox_N.ShowOkBox(UIMsgBoxInfo.mRoomIsClose.GetString());
			return;
		}
		if (mSelectServerData.PasswordStatus == 1)
		{
			mPassWordWnd.SetActive(value: true);
		}
		else
		{
			ConnectServer(needPasswold: false, mSelectServerData);
		}
		ServerRegistered serverRegistered = _curServerList.Find((ServerRegistered sr) => sr.ServerUID == roomUID);
		if (serverRegistered != null)
		{
			Debug.Log(serverRegistered.ServerUID);
			mRecentRoom_M.AddItem(serverRegistered.ServerUID, serverRegistered.ServerName, serverRegistered.ServerMasterName, serverRegistered.ServerVersion);
		}
	}

	public void JoinToServerByInvite()
	{
		if (mInviteServerData == null)
		{
			MessageBox_N.ShowOkBox(UIMsgBoxInfo.mRoomIsClose.GetString());
			return;
		}
		if (mInviteServerData.PasswordStatus == 1)
		{
			mPassWordWnd.SetActive(value: true);
		}
		else
		{
			ConnectServer(needPasswold: false, mInviteServerData);
		}
		ServerRegistered serverRegistered = _curServerList.Find((ServerRegistered sr) => sr.ServerUID == roomUID);
		if (serverRegistered != null)
		{
			mRecentRoom_M.AddItem(serverRegistered.ServerUID, serverRegistered.ServerName, serverRegistered.ServerMasterName, serverRegistered.ServerVersion);
		}
	}

	private void OnPasswordOkBtn()
	{
		if (mInviteServerData != null)
		{
			ConnectServer(needPasswold: true, mInviteServerData);
			SetInviteServerData(-1L);
		}
		else if (mSelectServerData != null)
		{
			ConnectServer(needPasswold: true, mSelectServerData);
		}
		mPassWordWnd.SetActive(value: false);
	}

	private void ConnectServer(bool needPasswold, ServerRegistered data)
	{
		if (data == null)
		{
			return;
		}
		if (data.GameMode == 4)
		{
			if (!string.IsNullOrEmpty(data.UID) && !string.IsNullOrEmpty(data.MapName))
			{
				string path = Path.Combine(GameConfig.CustomDataDir, data.MapName);
				PeSingleton<CustomGameData.Mgr>.Instance.curGameData = PeSingleton<CustomGameData.Mgr>.Instance.GetCustomData(data.UID, path);
				if (PeSingleton<CustomGameData.Mgr>.Instance.curGameData != null)
				{
					PeGameMgr.mapUID = data.UID;
					ScenarioIntegrityCheck check = ScenarioMapUtils.CheckIntegrityByPath(path);
					StartCoroutine(ProcessIntegrityCheck(check, needPasswold, data));
				}
			}
		}
		else
		{
			Connect(needPasswold, data);
		}
	}

	private void Connect(bool needPasswold, ServerRegistered data)
	{
		MyServerManager.LocalIp = data.IPAddress;
		MyServerManager.LocalPort = data.Port;
		MyServerManager.LocalPwd = ((!needPasswold) ? string.Empty : mCheckPasswordInput.text);
		MyServerManager.LocalHost = data;
		GameClientNetwork.Connect();
		PeSteamFriendMgr.Instance.mMyServerUID = data.ServerUID;
	}

	private IEnumerator ProcessIntegrityCheck(ScenarioIntegrityCheck check, bool needPasswold, ServerRegistered data)
	{
		while (true)
		{
			if (check.integrated == true)
			{
				Connect(needPasswold, data);
				yield break;
			}
			if (check.integrated == false)
			{
				break;
			}
			yield return null;
		}
		MessageBox_N.ShowOkBox(PELocalization.GetString(8000484));
	}

	private void OnPasswordCancelBtn()
	{
		mPassWordWnd.SetActive(value: false);
		SetInviteServerData(-1L);
	}

	private void BtnInputOnClick()
	{
		SendMsg();
	}

	private void OnSubmit(string inputString)
	{
		SendMsg();
	}

	private void BtnWorkShopsOnClick()
	{
		if (!(mWorkShopCtrl == null))
		{
			return;
		}
		GameObject gameObject = UnityEngine.Object.Instantiate(mWorkShopPrfab);
		gameObject.transform.transform.parent = mUICenter.transform;
		gameObject.transform.localPosition = new Vector3(0f, 40f, -20f);
		gameObject.transform.localScale = Vector3.one;
		mWorkShopCtrl = gameObject.GetComponent<UIWorkShopCtrl>();
		if (!(mWorkShopCtrl == null))
		{
			gameObject.SetActive(value: true);
			mWorkShopCtrl.e_BtnClose += WorkShopOnClose;
			if (mRoomWnd.activeSelf)
			{
				mRoomWnd.SetActive(value: false);
				mLastWnd = mRoomWnd;
			}
			if (mUIServerWnd.activeSelf)
			{
				mUIServerWnd.SetActive(value: false);
				mLastWnd = mUIServerWnd;
			}
			if (mHostCreateWnd.activeSelf)
			{
				mHostCreateWnd.SetActive(value: false);
				mLastWnd = mHostCreateWnd;
			}
			if (RoomGui_N.Instance.isShow)
			{
				RoomGui_N.Instance.gameObject.SetActive(value: false);
				mLastWnd = RoomGui_N.Instance.gameObject;
			}
			if (mMallWnd != null && mMallWnd.isShow)
			{
				mMallWnd.gameObject.SetActive(value: false);
				mLastWnd = mMallWnd.gameObject;
			}
		}
	}

	private void WorkShopOnClose()
	{
		if (!(mWorkShopCtrl == null))
		{
			if (mLastWnd != null && mLastWnd != mWorkShopCtrl.gameObject)
			{
				mLastWnd.SetActive(value: true);
			}
			else
			{
				mRoomWnd.SetActive(value: true);
			}
			UnityEngine.Object.Destroy(mWorkShopCtrl.gameObject);
			mWorkShopCtrl = null;
		}
	}

	private void BtnFriendsOnClick()
	{
		if (!PeSteamFriendMgr.Instance.mFriendWnd.isShow)
		{
			PeSteamFriendMgr.Instance.mFriendWnd.Show();
		}
		else
		{
			PeSteamFriendMgr.Instance.mFriendWnd.Hide();
		}
	}

	private void BtnMallOnClick()
	{
		if (mMallWnd == null)
		{
			GameObject gameObject = UnityEngine.Object.Instantiate(mallWndPrefab);
			gameObject.transform.transform.parent = mUICenter.transform;
			gameObject.transform.localPosition = new Vector3(0f, 40f, -10f);
			gameObject.transform.localScale = Vector3.one;
			mMallWnd = gameObject.GetComponent<UIMallWnd>();
			UIMallWnd uIMallWnd = mMallWnd;
			uIMallWnd.e_OnHide = (WndEvent)Delegate.Combine(uIMallWnd.e_OnHide, new WndEvent(MallWndOnClose));
		}
		if (mRoomWnd.activeSelf)
		{
			mRoomWnd.SetActive(value: false);
			mLastWnd = mRoomWnd;
		}
		if (mUIServerWnd.activeSelf)
		{
			mUIServerWnd.SetActive(value: false);
			mLastWnd = mUIServerWnd;
		}
		if (mHostCreateWnd.activeSelf)
		{
			mHostCreateWnd.SetActive(value: false);
			mLastWnd = mHostCreateWnd;
		}
		if (mWorkShopCtrl != null)
		{
			UnityEngine.Object.Destroy(mWorkShopCtrl.gameObject);
			mWorkShopCtrl = null;
			mLastWnd = mRoomWnd;
		}
		if (RoomGui_N.Instance.isShow)
		{
			RoomGui_N.Instance.gameObject.SetActive(value: false);
			mLastWnd = RoomGui_N.Instance.gameObject;
		}
		mMallWnd.gameObject.SetActive(value: true);
	}

	private void MallWndOnClose(UIBaseWidget widget = null)
	{
		if (mLastWnd != null && mLastWnd != mMallWnd.gameObject)
		{
			mLastWnd.SetActive(value: true);
		}
		else
		{
			mRoomWnd.SetActive(value: true);
		}
	}
}

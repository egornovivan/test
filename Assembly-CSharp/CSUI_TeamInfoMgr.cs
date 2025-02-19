using System.Collections.Generic;
using Pathea;
using UnityEngine;

public class CSUI_TeamInfoMgr : UIBaseWnd
{
	public enum MyType
	{
		Integration,
		Players,
		Troops
	}

	private class TeamInfo
	{
		public int _group;

		public int _killCount;

		public int _deathCount;

		public float _point;
	}

	public delegate void ReceiveDataDel();

	public delegate void CreatTeamDel();

	public delegate void OnInvitationDel(int id);

	public delegate void AcceptJoinTeamDel(int inviterId, int teamId);

	public delegate void JoinTeamDel(int _teamId, bool _freejoin);

	public delegate void KickTeamDel(PlayerNetwork _kicked);

	public delegate void OnAgreeJoinDel(bool _isAgree, PlayerNetwork _pnet);

	public delegate void OnDeliverToDel(int id);

	public delegate void OnDissolveToDel();

	public delegate void OnQuitTeamDel();

	private static CSUI_TeamInfoMgr m_Instance;

	[SerializeField]
	private GameObject mIntegrationPage;

	[SerializeField]
	private GameObject mPlayersPage;

	[SerializeField]
	private GameObject mTroopsPage;

	[SerializeField]
	private UIGrid mIntegrationGrid;

	[SerializeField]
	private UIGrid mPlayersGrid;

	[SerializeField]
	private UIGrid mTroopsGrid;

	[SerializeField]
	private GameObject mIntegrationPrefab;

	[SerializeField]
	private GameObject mPlayersPrefab;

	[SerializeField]
	private GameObject mTroopsPrefab;

	[SerializeField]
	private GameObject mTroopsBtn;

	[SerializeField]
	private N_ImageButton mKickBtn;

	[SerializeField]
	private N_ImageButton mReferredBtn;

	[SerializeField]
	private N_ImageButton mBreakBtn;

	[SerializeField]
	private N_ImageButton mQuitBtn;

	public GameObject mYesSpr;

	public GameObject mNoSpr;

	public UILabel mIntegrationPageCountText;

	public UILabel mPlayersPageCountText;

	public UILabel mTroopsPageCountText;

	public N_ImageButton _creatTeam;

	public N_ImageButton _invitation;

	public N_ImageButton _joinTeam;

	private bool m_FreeJoin;

	private MyType mBtnType;

	private MyGameType mGameType;

	private List<TeamInfo> mTotalTeamList = new List<TeamInfo>();

	private int IntegrationPageIndex;

	private int IntegrationPageCount;

	private int IntegrationPerPageCount = 12;

	private List<CSUI_TeamListItem> IntegrationPageList;

	private int PlayersPageIndex;

	private int PlayersPageCount;

	private int PlayersPerPageCount = 12;

	private List<CSUI_TeamListItem> PlayersPageList;

	private int TroopsPageIndex;

	private int TroopsPageCount;

	private int TroopsPerPageCount = 11;

	private List<CSUI_TeamListItem> TroopsPageList;

	private int _currentIndex = -1;

	public UISlicedSprite m_CheckBtnBg;

	private List<PlayerNetwork> m_AllPlayers;

	private PlayerNetwork _currentChosedPlayer;

	private List<PlayerNetwork> _MemberLis = new List<PlayerNetwork>();

	private PlayerNetwork mInviter;

	public static CSUI_TeamInfoMgr Intance => m_Instance;

	public MyType BtnType
	{
		get
		{
			return mBtnType;
		}
		set
		{
			mBtnType = value;
			switch (value)
			{
			case MyType.Integration:
				InfoSum();
				if (mTotalTeamList != null)
				{
					int num3 = mTotalTeamList.Count % IntegrationPerPageCount;
					int num4 = mTotalTeamList.Count / IntegrationPerPageCount;
					if (num3 == 0)
					{
						IntegrationPageCount = num4;
					}
					else
					{
						IntegrationPageCount = num4 + 1;
					}
					CreatIntegration(IntegrationPageIndex);
				}
				break;
			case MyType.Players:
				GetAllPlayers();
				if (AllPlayers != null)
				{
					int num = AllPlayers.Count % PlayersPerPageCount;
					int num2 = AllPlayers.Count / PlayersPerPageCount;
					if (num == 0)
					{
						PlayersPageCount = num2;
					}
					else
					{
						PlayersPageCount = num2 + 1;
					}
					CreatPlayers(PlayersPageIndex);
				}
				break;
			case MyType.Troops:
				RefreshTeamGrid(PlayerNetwork.mainPlayer.TeamId);
				break;
			}
		}
	}

	public MyGameType GameType
	{
		get
		{
			return mGameType;
		}
		set
		{
			mGameType = value;
			if (value == MyGameType.Survival)
			{
				_creatTeam.gameObject.SetActive(value: true);
				_joinTeam.gameObject.SetActive(value: true);
			}
			else
			{
				_creatTeam.gameObject.SetActive(value: true);
				_joinTeam.gameObject.SetActive(value: true);
			}
		}
	}

	public List<PlayerNetwork> AllPlayers
	{
		get
		{
			return m_AllPlayers;
		}
		set
		{
			m_AllPlayers = value;
		}
	}

	public static event CreatTeamDel CreatTeamEvent;

	public static event OnInvitationDel OnInvitationEvent;

	public static event AcceptJoinTeamDel AcceptJoinTeamEvent;

	public static event JoinTeamDel JoinTeamEvent;

	public static event KickTeamDel KickTeamEvent;

	public static event OnAgreeJoinDel OnAgreeJoinEvent;

	public static event OnDeliverToDel OnDeliverToEvent;

	public static event OnDissolveToDel OnDissolveEvent;

	public static event OnQuitTeamDel OnMemberQuitTeamEvent;

	private void InitGrid()
	{
		IntegrationPageList = new List<CSUI_TeamListItem>();
		for (int i = 0; i < IntegrationPerPageCount; i++)
		{
			GameObject newGameObject = GetNewGameObject(mIntegrationPrefab, mIntegrationGrid.transform);
			CSUI_TeamListItem component = newGameObject.GetComponent<CSUI_TeamListItem>();
			component.mType = MyItemType.One;
			component.mIndex = i;
			component.ItemChecked += UICheckItem;
			IntegrationPageList.Add(component);
		}
		mIntegrationGrid.repositionNow = true;
		PlayersPageList = new List<CSUI_TeamListItem>();
		for (int j = 0; j < PlayersPerPageCount; j++)
		{
			GameObject newGameObject2 = GetNewGameObject(mPlayersPrefab, mPlayersGrid.transform);
			CSUI_TeamListItem component2 = newGameObject2.GetComponent<CSUI_TeamListItem>();
			component2.mType = MyItemType.Two;
			component2.mIndex = j;
			component2.ItemChecked += UICheckItem;
			component2.ItemCheckedPlayer += UICheckItemPlayer;
			PlayersPageList.Add(component2);
		}
		mPlayersGrid.repositionNow = true;
		TroopsPageList = new List<CSUI_TeamListItem>();
		for (int k = 0; k < TroopsPerPageCount; k++)
		{
			GameObject newGameObject3 = GetNewGameObject(mTroopsPrefab, mTroopsGrid.transform);
			CSUI_TeamListItem component3 = newGameObject3.GetComponent<CSUI_TeamListItem>();
			component3.mType = MyItemType.Three;
			component3.mIndex = k;
			component3.ItemChecked += UICheckItem;
			component3.ItemCheckedPlayer += UICheckItemPlayer;
			TroopsPageList.Add(component3);
		}
		mTroopsGrid.repositionNow = true;
	}

	private GameObject GetNewGameObject(GameObject prefab, Transform parentTrans)
	{
		GameObject gameObject = Object.Instantiate(prefab);
		gameObject.transform.parent = parentTrans;
		gameObject.transform.localPosition = Vector3.zero;
		gameObject.transform.localScale = Vector3.one;
		gameObject.transform.localRotation = Quaternion.identity;
		return gameObject;
	}

	private void CreatIntegration(int pageIndex)
	{
		ClearSelected();
		List<TeamInfo> list = new List<TeamInfo>();
		if (pageIndex < IntegrationPageCount - 1)
		{
			list = mTotalTeamList.GetRange(pageIndex * IntegrationPerPageCount, IntegrationPerPageCount);
		}
		else if (pageIndex == IntegrationPageCount - 1)
		{
			if (IntegrationPageCount * IntegrationPerPageCount > mTotalTeamList.Count)
			{
				list = mTotalTeamList.GetRange(pageIndex * IntegrationPerPageCount, mTotalTeamList.Count % IntegrationPerPageCount);
			}
			else if (IntegrationPageCount * IntegrationPerPageCount == mTotalTeamList.Count)
			{
				list = mTotalTeamList.GetRange(pageIndex * IntegrationPerPageCount, IntegrationPerPageCount);
			}
		}
		foreach (CSUI_TeamListItem integrationPage in IntegrationPageList)
		{
			integrationPage.ClearInfo();
		}
		for (int i = 0; i < list.Count; i++)
		{
			IntegrationPageList[i].SetInfo(list[i]._group, list[i]._killCount, list[i]._deathCount, list[i]._point);
		}
		mIntegrationPageCountText.text = IntegrationPageIndex + 1 + "/" + IntegrationPageCount;
	}

	private void CreatPlayers(int pageIndex)
	{
		ClearSelected();
		if (AllPlayers == null || AllPlayers.Count <= 0)
		{
			return;
		}
		List<PlayerNetwork> list = new List<PlayerNetwork>();
		if (pageIndex < PlayersPageCount - 1)
		{
			list = AllPlayers.GetRange(pageIndex * PlayersPerPageCount, PlayersPerPageCount);
		}
		else if (pageIndex == PlayersPageCount - 1)
		{
			if (PlayersPageCount * PlayersPerPageCount > AllPlayers.Count)
			{
				list = AllPlayers.GetRange(pageIndex * PlayersPerPageCount, AllPlayers.Count % PlayersPerPageCount);
			}
			else if (PlayersPageCount * PlayersPerPageCount == AllPlayers.Count)
			{
				list = AllPlayers.GetRange(pageIndex * PlayersPerPageCount, PlayersPerPageCount);
			}
		}
		if (list == null || list.Count <= 0 || PlayersPageList == null || PlayersPageList.Count <= 0)
		{
			return;
		}
		foreach (CSUI_TeamListItem playersPage in PlayersPageList)
		{
			if (null != playersPage)
			{
				playersPage.ClearInfo();
			}
		}
		for (int i = 0; i < list.Count; i++)
		{
			if (i < PlayersPageList.Count)
			{
				PlayerNetwork playerNetwork = list[i];
				if (null != playerNetwork && playerNetwork.Battle != null)
				{
					PlayersPageList[i].SetInfo(playerNetwork.TeamId, playerNetwork.RoleName, playerNetwork.Battle._killCount, playerNetwork.Battle._deathCount, playerNetwork.Battle._point, playerNetwork);
				}
			}
		}
		mPlayersPageCountText.text = PlayersPageIndex + 1 + "/" + PlayersPageCount;
	}

	private void CreatTroops(int pageIndex, List<PlayerNetwork> _player)
	{
		ClearSelected();
		if (_player == null)
		{
			return;
		}
		int num = _player.Count % TroopsPerPageCount;
		int num2 = _player.Count / TroopsPerPageCount;
		if (num == 0)
		{
			TroopsPageCount = num2;
		}
		else
		{
			TroopsPageCount = num2 + 1;
		}
		List<PlayerNetwork> list = new List<PlayerNetwork>();
		if (pageIndex < TroopsPageCount - 1)
		{
			list = _player.GetRange(pageIndex * TroopsPerPageCount, TroopsPerPageCount);
		}
		else if (pageIndex == TroopsPageCount - 1)
		{
			if (TroopsPageCount * TroopsPerPageCount > _player.Count)
			{
				list = _player.GetRange(pageIndex * TroopsPerPageCount, _player.Count % TroopsPerPageCount);
			}
			else if (TroopsPageCount * TroopsPerPageCount == _player.Count)
			{
				list = _player.GetRange(pageIndex * TroopsPerPageCount, TroopsPerPageCount);
			}
		}
		foreach (CSUI_TeamListItem troopsPage in TroopsPageList)
		{
			troopsPage.ClearInfo();
			troopsPage.OnAgreementBtnEvent -= OnAgreeJoin;
		}
		for (int i = 0; i < list.Count; i++)
		{
			TroopsPageList[i].SetInfo(list[i].RoleName, list[i].Battle._killCount, list[i].Battle._deathCount, list[i].Battle._point, list[i]);
			TroopsPageList[i].OnAgreementBtnEvent += OnAgreeJoin;
		}
		mTroopsPageCountText.text = TroopsPageIndex + 1 + "/" + TroopsPageCount;
	}

	private void SetTeamNameToJoin(MyPlayer mp)
	{
	}

	public void JoinTeamList(PlayerNetwork pnet)
	{
		_currentIndex++;
	}

	private void Awake()
	{
		m_Instance = this;
	}

	private void Start()
	{
		InitGrid();
		if (PeGameMgr.gameType == PeGameMgr.EGameType.Survive)
		{
			mTroopsBtn.SetActive(value: true);
			m_CheckBtnBg.transform.localScale = new Vector3(356f, 35f, 1f);
		}
		else
		{
			mTroopsBtn.SetActive(value: false);
			m_CheckBtnBg.transform.localScale = new Vector3(248f, 35f, 1f);
		}
	}

	private void Update()
	{
		if (Input.GetKeyDown(KeyCode.A))
		{
		}
		if (null == PlayerNetwork.mainPlayer)
		{
			return;
		}
		if (PeGameMgr.gameType == PeGameMgr.EGameType.Survive)
		{
			if (!mTroopsBtn.activeSelf)
			{
				mTroopsBtn.SetActive(value: true);
			}
			if (_creatTeam.gameObject.activeSelf)
			{
				_creatTeam.gameObject.SetActive(value: false);
			}
			if (PlayerNetwork.mainPlayer.isOriginTeam)
			{
				if (GroupNetwork.IsEmpty(PlayerNetwork.mainPlayer.TeamId))
				{
					if (!_joinTeam.gameObject.activeSelf)
					{
						_joinTeam.gameObject.SetActive(value: true);
					}
					if (!_invitation.gameObject.activeSelf)
					{
						_invitation.gameObject.SetActive(value: true);
					}
					if (mKickBtn.gameObject.activeSelf)
					{
						mKickBtn.gameObject.SetActive(value: false);
					}
					if (mReferredBtn.gameObject.activeSelf)
					{
						mReferredBtn.gameObject.SetActive(value: false);
					}
					if (mBreakBtn.gameObject.activeSelf)
					{
						mBreakBtn.gameObject.SetActive(value: false);
					}
					if (mQuitBtn.gameObject.activeSelf)
					{
						mQuitBtn.gameObject.SetActive(value: false);
					}
				}
				else
				{
					if (_joinTeam.gameObject.activeSelf)
					{
						_joinTeam.gameObject.SetActive(value: false);
					}
					if (!_invitation.gameObject.activeSelf)
					{
						_invitation.gameObject.SetActive(value: true);
					}
					if (!mKickBtn.gameObject.activeSelf)
					{
						mKickBtn.gameObject.SetActive(value: true);
					}
					if (!mReferredBtn.gameObject.activeSelf)
					{
						mReferredBtn.gameObject.SetActive(value: true);
					}
					if (!mBreakBtn.gameObject.activeSelf)
					{
						mBreakBtn.gameObject.SetActive(value: true);
					}
					if (mQuitBtn.gameObject.activeSelf)
					{
						mQuitBtn.gameObject.SetActive(value: false);
					}
				}
			}
			else
			{
				if (_joinTeam.gameObject.activeSelf)
				{
					_joinTeam.gameObject.SetActive(value: false);
				}
				if (_invitation.gameObject.activeSelf)
				{
					_invitation.gameObject.SetActive(value: false);
				}
				if (mKickBtn.gameObject.activeSelf)
				{
					mKickBtn.gameObject.SetActive(value: false);
				}
				if (mReferredBtn.gameObject.activeSelf)
				{
					mReferredBtn.gameObject.SetActive(value: false);
				}
				if (mBreakBtn.gameObject.activeSelf)
				{
					mBreakBtn.gameObject.SetActive(value: false);
				}
				if (!mQuitBtn.gameObject.activeSelf)
				{
					mQuitBtn.gameObject.SetActive(value: true);
				}
			}
			return;
		}
		if (mTroopsBtn.activeSelf)
		{
			mTroopsBtn.SetActive(value: false);
		}
		if (_creatTeam.gameObject.activeSelf)
		{
			_creatTeam.gameObject.SetActive(value: false);
		}
		if (_joinTeam.gameObject.activeSelf)
		{
			_joinTeam.gameObject.SetActive(value: false);
		}
		if (_invitation.gameObject.activeSelf)
		{
			_invitation.gameObject.SetActive(value: false);
		}
		if (PlayerNetwork.mainPlayer.TeamId == -1)
		{
			if (mKickBtn.gameObject.activeSelf)
			{
				mKickBtn.gameObject.SetActive(value: false);
			}
			if (mReferredBtn.gameObject.activeSelf)
			{
				mReferredBtn.gameObject.SetActive(value: false);
			}
			if (mBreakBtn.gameObject.activeSelf)
			{
				mBreakBtn.gameObject.SetActive(value: false);
			}
			if (mQuitBtn.gameObject.activeSelf)
			{
				mQuitBtn.gameObject.SetActive(value: false);
			}
			if (_creatTeam.disable)
			{
				_creatTeam.disable = false;
			}
			if (_joinTeam.disable)
			{
				_joinTeam.disable = false;
			}
			if (!_invitation.disable)
			{
				_invitation.disable = true;
			}
		}
		else
		{
			if (!GroupNetwork.TeamExists(PlayerNetwork.mainPlayer.TeamId))
			{
				return;
			}
			if (!PlayerNetwork.mainPlayer.isOriginTeam)
			{
				if (mKickBtn.gameObject.activeSelf)
				{
					mKickBtn.gameObject.SetActive(value: false);
				}
				if (mReferredBtn.gameObject.activeSelf)
				{
					mReferredBtn.gameObject.SetActive(value: false);
				}
				if (mBreakBtn.gameObject.activeSelf)
				{
					mBreakBtn.gameObject.SetActive(value: false);
				}
				if (!mQuitBtn.gameObject.activeSelf)
				{
					mQuitBtn.gameObject.SetActive(value: true);
				}
				if (!_creatTeam.disable)
				{
					_creatTeam.disable = true;
				}
				if (!_joinTeam.disable)
				{
					_joinTeam.disable = true;
				}
				if (!_invitation.disable)
				{
					_invitation.disable = true;
				}
			}
			else if (GroupNetwork.GetLeaderId(PlayerNetwork.mainPlayer.TeamId) == PlayerNetwork.mainPlayerId)
			{
				if (!mKickBtn.gameObject.activeSelf)
				{
					mKickBtn.gameObject.SetActive(value: true);
				}
				if (!mReferredBtn.gameObject.activeSelf)
				{
					mReferredBtn.gameObject.SetActive(value: true);
				}
				if (!mBreakBtn.gameObject.activeSelf)
				{
					mBreakBtn.gameObject.SetActive(value: true);
				}
				if (!mQuitBtn.gameObject.activeSelf)
				{
					mQuitBtn.gameObject.SetActive(value: true);
				}
				if (!_creatTeam.disable)
				{
					_creatTeam.disable = true;
				}
				if (!_joinTeam.disable)
				{
					_joinTeam.disable = true;
				}
				if (_invitation.disable)
				{
					_invitation.disable = false;
				}
			}
		}
	}

	private void PageIntegrationOnActive(bool active)
	{
		if (active)
		{
			Debug.Log("点击了综合按钮");
			mIntegrationPage.SetActive(value: true);
			mPlayersPage.SetActive(value: false);
			mTroopsPage.SetActive(value: false);
			BtnType = MyType.Integration;
		}
	}

	private void PagePlayersOnActive(bool active)
	{
		if (active)
		{
			Debug.Log("点击了玩家按钮");
			mIntegrationPage.SetActive(value: false);
			mPlayersPage.SetActive(value: true);
			mTroopsPage.SetActive(value: false);
			BtnType = MyType.Players;
		}
	}

	private void PageTroopsOnActive(bool active)
	{
		if (active)
		{
			Debug.Log("点击了队伍按钮");
			mIntegrationPage.SetActive(value: false);
			mPlayersPage.SetActive(value: false);
			mTroopsPage.SetActive(value: true);
			BtnType = MyType.Troops;
		}
	}

	private void OnRightBtn()
	{
		if (IntegrationPageIndex < IntegrationPageCount - 1)
		{
			IntegrationPageIndex++;
			CreatIntegration(IntegrationPageIndex);
		}
	}

	private void OnRightBtnEnd()
	{
		if (IntegrationPageIndex < IntegrationPageCount - 1)
		{
			IntegrationPageIndex = IntegrationPageCount - 1;
			CreatIntegration(IntegrationPageIndex);
		}
	}

	private void OnLeftBtn()
	{
		if (IntegrationPageIndex > 0)
		{
			IntegrationPageIndex--;
			CreatIntegration(IntegrationPageIndex);
		}
	}

	private void OnLeftBtnEnd()
	{
		if (IntegrationPageIndex > 0)
		{
			IntegrationPageIndex = 0;
			CreatIntegration(IntegrationPageIndex);
		}
	}

	private void OnRightBtnPlayers()
	{
		if (PlayersPageIndex < PlayersPageCount - 1)
		{
			PlayersPageIndex++;
			CreatPlayers(PlayersPageIndex);
		}
	}

	private void OnRightBtnEndPlayers()
	{
		if (PlayersPageIndex < PlayersPageCount - 1)
		{
			PlayersPageIndex = PlayersPageCount - 1;
			CreatPlayers(PlayersPageIndex);
		}
	}

	private void OnLeftBtnPlayers()
	{
		if (PlayersPageIndex > 0)
		{
			PlayersPageIndex--;
			CreatPlayers(PlayersPageIndex);
		}
	}

	private void OnLeftBtnEndPlayers()
	{
		if (PlayersPageIndex > 0)
		{
			PlayersPageIndex = 0;
			CreatPlayers(PlayersPageIndex);
		}
	}

	private void OnRightBtnTroops()
	{
		if (TroopsPageIndex < TroopsPageCount - 1)
		{
			TroopsPageIndex++;
			CreatTroops(TroopsPageIndex, _MemberLis);
		}
	}

	private void OnRightBtnEndTroops()
	{
		if (TroopsPageIndex < TroopsPageCount - 1)
		{
			TroopsPageIndex = TroopsPageCount - 1;
			CreatTroops(TroopsPageIndex, _MemberLis);
		}
	}

	private void OnLeftBtnTroops()
	{
		if (TroopsPageIndex > 0)
		{
			TroopsPageIndex--;
			CreatTroops(TroopsPageIndex, _MemberLis);
		}
	}

	private void OnLeftBtnEndTroops()
	{
		if (TroopsPageIndex > 0)
		{
			TroopsPageIndex = 0;
			CreatTroops(TroopsPageIndex, _MemberLis);
		}
	}

	private void OnSetFreeJoinBtn()
	{
		if (!m_FreeJoin)
		{
			mYesSpr.SetActive(value: true);
			mNoSpr.SetActive(value: false);
			m_FreeJoin = true;
		}
		else
		{
			mYesSpr.SetActive(value: false);
			mNoSpr.SetActive(value: true);
			m_FreeJoin = false;
		}
	}

	private void OnCreatTeamBtn()
	{
		if (CSUI_TeamInfoMgr.CreatTeamEvent != null)
		{
			CSUI_TeamInfoMgr.CreatTeamEvent();
		}
	}

	private void OnJoinTeamBtn()
	{
		if (CSUI_TeamInfoMgr.JoinTeamEvent != null && _currentChosedPlayer != null)
		{
			CSUI_TeamInfoMgr.JoinTeamEvent(_currentChosedPlayer.TeamId, m_FreeJoin);
		}
	}

	private void OnInvitationBtn()
	{
		if (CSUI_TeamInfoMgr.OnInvitationEvent != null && _currentChosedPlayer != null)
		{
			CSUI_TeamInfoMgr.OnInvitationEvent(_currentChosedPlayer.Id);
		}
	}

	private void OnKickTeamBtn()
	{
		if (CSUI_TeamInfoMgr.KickTeamEvent != null)
		{
			CSUI_TeamInfoMgr.KickTeamEvent(_currentChosedPlayer);
		}
	}

	private void OnDeliverToBtn()
	{
		if (CSUI_TeamInfoMgr.OnDeliverToEvent != null && null != _currentChosedPlayer)
		{
			CSUI_TeamInfoMgr.OnDeliverToEvent(_currentChosedPlayer.Id);
		}
	}

	private void OnDissolveBtn()
	{
		if (CSUI_TeamInfoMgr.OnDissolveEvent != null)
		{
			CSUI_TeamInfoMgr.OnDissolveEvent();
		}
	}

	private void OnQuitTeamBtn()
	{
		if (CSUI_TeamInfoMgr.OnMemberQuitTeamEvent != null)
		{
			CSUI_TeamInfoMgr.OnMemberQuitTeamEvent();
		}
	}

	private void GetAllPlayers()
	{
		AllPlayers = NetworkInterface.Get<PlayerNetwork>();
		AllPlayers.Sort(delegate(PlayerNetwork x, PlayerNetwork y)
		{
			if (x.TeamId == -1)
			{
				return 1;
			}
			return (x.TeamId <= y.TeamId) ? (-1) : 0;
		});
		Debug.Log(AllPlayers.Count);
	}

	private void InfoSum()
	{
		TeamData[] teamInfos = GroupNetwork.GetTeamInfos();
		mTotalTeamList.Clear();
		if (teamInfos == null || teamInfos.Length <= 0)
		{
			return;
		}
		for (int i = 0; i < teamInfos.Length; i++)
		{
			if (teamInfos[i].Members.Count == 0)
			{
				continue;
			}
			TeamInfo teamInfo = new TeamInfo();
			teamInfo._group = teamInfos[i].TeamId;
			foreach (PlayerNetwork member in teamInfos[i].Members)
			{
				if (null != member && member.Battle != null)
				{
					teamInfo._killCount += member.Battle._killCount;
					teamInfo._deathCount += member.Battle._deathCount;
					teamInfo._point += member.Battle._point;
				}
			}
			mTotalTeamList.Add(teamInfo);
		}
	}

	private void ClearSelected()
	{
		ClearChosedPlayer();
		for (int i = 0; i < IntegrationPageList.Count; i++)
		{
			IntegrationPageList[i].SetSelected(_isSelected: false);
		}
		for (int j = 0; j < PlayersPageList.Count; j++)
		{
			PlayersPageList[j].SetSelected(_isSelected: false);
		}
		for (int k = 0; k < TroopsPageList.Count; k++)
		{
			TroopsPageList[k].SetSelected(_isSelected: false);
		}
	}

	private void UICheckItem(int _index, MyItemType _type)
	{
		ClearSelected();
		switch (_type)
		{
		case MyItemType.One:
			IntegrationPageList[_index].SetSelected(_isSelected: true);
			break;
		case MyItemType.Two:
			PlayersPageList[_index].SetSelected(_isSelected: true);
			break;
		case MyItemType.Three:
			TroopsPageList[_index].SetSelected(_isSelected: true);
			break;
		}
	}

	private void UICheckItemPlayer(PlayerNetwork pnet)
	{
		if (pnet != null)
		{
			_currentChosedPlayer = pnet;
		}
	}

	private void ClearChosedPlayer()
	{
		_currentChosedPlayer = null;
	}

	public void RefreshTeamGrid(int _teamId)
	{
		if (!base.isActiveAndEnabled || _teamId != PlayerNetwork.mainPlayer.TeamId)
		{
			return;
		}
		_MemberLis.Clear();
		TeamData teamInfo = GroupNetwork.GetTeamInfo(_teamId);
		if (teamInfo != null)
		{
			foreach (PlayerNetwork member in teamInfo.Members)
			{
				_MemberLis.Add(member);
			}
		}
		GroupNetwork.GetJoinRequest(_MemberLis);
		CreatTroops(TroopsPageIndex, _MemberLis);
	}

	public void OnCreatTeam(int teamId)
	{
		if (PlayerNetwork.mainPlayer.TeamId == teamId)
		{
			RefreshTeamGrid(teamId);
		}
		GetAllPlayers();
		CreatPlayers(PlayersPageIndex);
	}

	public void Invitation(PlayerNetwork _inviter)
	{
		if (!(_inviter == null))
		{
			mInviter = _inviter;
			string text = string.Format(PELocalization.GetString(8000504), _inviter.RoleName);
			MessageBox_N.ShowYNBox(text, OnYes);
		}
	}

	private void OnYes()
	{
		CSUI_TeamInfoMgr.AcceptJoinTeamEvent(mInviter.Id, mInviter.TeamId);
	}

	public void ApprovalJoin(int teamId)
	{
		if (teamId != -1 && teamId == PlayerNetwork.mainPlayer.TeamId)
		{
			RefreshTeamGrid(teamId);
		}
	}

	public void JoinApply(int teamId)
	{
		if (teamId != -1 && PlayerNetwork.mainPlayerId == GroupNetwork.GetLeaderId(teamId))
		{
			RefreshTeamGrid(teamId);
		}
	}

	public void KickPlayer(int teamId)
	{
		if (teamId != -1 && PlayerNetwork.mainPlayer.TeamId == teamId)
		{
			RefreshTeamGrid(teamId);
		}
		else if (PlayerNetwork.mainPlayer.TeamId == -1)
		{
			ClearTroops();
		}
	}

	private void ClearTroops()
	{
		foreach (CSUI_TeamListItem troopsPage in TroopsPageList)
		{
			troopsPage.ClearInfo();
			troopsPage.OnAgreementBtnEvent -= OnAgreeJoin;
		}
	}

	private void OnAgreeJoin(bool _isAgree, PlayerNetwork _pnet)
	{
		CSUI_TeamInfoMgr.OnAgreeJoinEvent(_isAgree, _pnet);
	}

	public void LeaderDeliver(int teamId)
	{
		if (teamId != -1 && PlayerNetwork.mainPlayer.TeamId == teamId)
		{
			RefreshTeamGrid(teamId);
		}
	}

	public void DissolveTeam(int teamId)
	{
		if (teamId != -1 && teamId == PlayerNetwork.mainPlayer.TeamId)
		{
			RefreshTeamGrid(teamId);
		}
	}

	public void QuitTeam(int teamId)
	{
		if (teamId != -1 && PlayerNetwork.mainPlayer.TeamId == teamId)
		{
			RefreshTeamGrid(teamId);
		}
		else if (PlayerNetwork.mainPlayer.TeamId == -1)
		{
			ClearTroops();
		}
	}
}

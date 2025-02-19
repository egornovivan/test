using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using CustomData;
using Pathea;
using UnityEngine;

public class RoomGui_N : UIStaticWnd
{
	public GameObject ForcePrefab;

	public GameObject ForceInfoGo;

	public GameObject ForceChatGo;

	public UITable ForceTable;

	public UILabel SartBtnText;

	public UIButton StartBtn;

	public UISlicedSprite StarBtnBg;

	private static RoomGui_N m_Instance;

	private RoomInfo_N m_RoomInfoComp;

	private RoomChat_N m_RoomChatComp;

	private List<ForceItem_N> m_ForceItemList;

	private bool m_IsRoomMaster;

	private Dictionary<int, Dictionary<int, int>> m_ForceDic;

	private IsoDownLoadInfo m_IsoDownloadInfo;

	private bool m_IsReady;

	private bool m_Repositioning;

	private bool m_InitAllForce;

	public static RoomGui_N Instance => m_Instance;

	private void Awake()
	{
		m_Instance = this;
		m_IsoDownloadInfo = default(IsoDownLoadInfo);
		m_ForceDic = new Dictionary<int, Dictionary<int, int>>();
		m_ForceItemList = new List<ForceItem_N>();
		m_RoomInfoComp = ForceInfoGo.GetComponent<RoomInfo_N>();
		m_RoomChatComp = ForceChatGo.GetComponent<RoomChat_N>();
		m_Repositioning = false;
		m_InitAllForce = false;
	}

	private void RefreshIsoProcess()
	{
		m_RoomInfoComp.UpdateIsoSpeed(m_IsoDownloadInfo.mLoadSpeed + " kb/s");
		m_RoomInfoComp.UpdateIsoCount(m_IsoDownloadInfo.mLoadCount + "/" + m_IsoDownloadInfo.mNeedCount);
		if (m_IsoDownloadInfo.mNeedCount != 0)
		{
			m_RoomInfoComp.UpdateIsoProcess(Convert.ToSingle(m_IsoDownloadInfo.mLoadCount) / Convert.ToSingle(m_IsoDownloadInfo.mNeedCount));
			if (m_IsoDownloadInfo.mLoadCount == m_IsoDownloadInfo.mNeedCount)
			{
				StartGame();
			}
		}
	}

	private void EnableBtnStar(bool value)
	{
		StartBtn.isEnabled = value;
		BoxCollider component = StartBtn.gameObject.GetComponent<BoxCollider>();
		if (component != null)
		{
			component.enabled = value;
		}
		if (value)
		{
			StarBtnBg.spriteName = "but_start_on";
		}
		else
		{
			StarBtnBg.spriteName = "but_start_off";
		}
	}

	private void OnStart()
	{
		if (!m_IsReady)
		{
			m_IsReady = true;
			SartBtnText.text = PELocalization.GetString(8000540);
			StartGame();
		}
		else
		{
			m_IsReady = false;
			SartBtnText.text = PELocalization.GetString(8000378);
		}
	}

	private void StartGame()
	{
		if (m_IsReady && BaseNetwork.MainPlayer != null)
		{
			BaseNetwork.MainPlayer.RequestChangeStatus(ENetworkState.Ready);
		}
	}

	private void OnBack()
	{
		MessageBox_N.ShowYNBox(PELocalization.GetString(8000075), PeSceneCtrl.Instance.GotoLobbyScene);
	}

	private void InitAllForce()
	{
		List<ForceDesc> roomForceList = Singleton<ForceSetting>.Instance.HumanForce;
		List<PlayerDesc> humanPlayer = Singleton<ForceSetting>.Instance.HumanPlayer;
		for (int i = 0; i < roomForceList.Count(); i++)
		{
			GameObject gameObject = UnityEngine.Object.Instantiate(ForcePrefab);
			gameObject.transform.parent = ForceTable.transform;
			gameObject.transform.localPosition = Vector3.zero;
			gameObject.transform.localScale = Vector3.one;
			gameObject.gameObject.name = "ForceItem" + i.ToString("D4");
			ForceItem_N component = gameObject.GetComponent<ForceItem_N>();
			component.RepositionEvent = delegate
			{
				RepostionForceTable();
			};
			component.ChangePlayerForceEvent = ChangePlayerTeamToNet;
			component.KickPlayerEvent = KickPlayerToNet;
			List<PlayerDesc> roleInfos = humanPlayer.Where((PlayerDesc a) => a.Force == roomForceList[i].ID).ToList();
			component.SetForceInfo(roomForceList[i], roleInfos);
			m_ForceItemList.Add(component);
			m_ForceDic.Add(roomForceList[i].ID, new Dictionary<int, int>());
		}
		RepostionForceTable();
	}

	private void RepostionForceTable()
	{
		if (m_Repositioning)
		{
			StopCoroutine("RepositionForceIterator");
			m_Repositioning = false;
		}
		StartCoroutine("RepositionForceIterator");
	}

	private IEnumerator RepositionForceIterator()
	{
		if (!m_Repositioning)
		{
			m_Repositioning = true;
			yield return null;
			ForceTable.Reposition();
			m_Repositioning = false;
		}
	}

	private void TryRemovePlayer(int playerID)
	{
		ForceItem_N forceItem_N = FindForceItemByPlayerID(playerID);
		if (forceItem_N != null)
		{
			m_ForceDic[forceItem_N.GetForceID()].Remove(playerID);
			forceItem_N.ExitForceByNet(playerID);
		}
	}

	private ForceItem_N FindForceItemByPlayerID(int playerID)
	{
		if (m_ForceDic != null && m_ForceDic.Count() > 0)
		{
			int levelForceID = -1;
			foreach (KeyValuePair<int, Dictionary<int, int>> item in m_ForceDic)
			{
				if (item.Value.ContainsKey(playerID))
				{
					levelForceID = item.Key;
					break;
				}
			}
			return m_ForceItemList.FirstOrDefault((ForceItem_N item) => item.GetForceID() == levelForceID);
		}
		return null;
	}

	public override void Show()
	{
		base.Show();
		m_IsReady = false;
		m_RoomInfoComp.UpdateInfo();
		m_RoomChatComp.SendMsgEvent = SyncRoomMsgToNet;
	}

	public static void UpdateDownLoadInfo(int leftLoad, int speed)
	{
		if (null != Instance)
		{
			int value = Instance.m_IsoDownloadInfo.mNeedCount - leftLoad;
			value = Mathf.Clamp(value, 0, Instance.m_IsoDownloadInfo.mNeedCount);
			Instance.m_IsoDownloadInfo.mLoadCount = value;
			Instance.m_IsoDownloadInfo.mLoadSpeed = speed;
			if (Instance.isShow)
			{
				Instance.RefreshIsoProcess();
			}
		}
	}

	public static void SetDownLoadInfo(int totalLoad)
	{
		if (null != Instance)
		{
			Instance.m_IsoDownloadInfo.mNeedCount = Mathf.Max(0, totalLoad);
			if (Instance.isShow)
			{
				Instance.RefreshIsoProcess();
			}
		}
	}

	public static void SetMapInfo(string info)
	{
		if (null != Instance)
		{
			Instance.m_RoomInfoComp.UpdateMapInfo(info);
		}
	}

	public static void InitRoomForceByNet()
	{
		if (!(null == Instance) && !Instance.m_InitAllForce)
		{
			Instance.InitAllForce();
			Instance.m_InitAllForce = true;
		}
	}

	public static void InitRoomPlayerByNet(RoomPlayerInfo playerInfo)
	{
		if (null == Instance)
		{
			return;
		}
		InitRoomForceByNet();
		Instance.m_IsRoomMaster = GameClientNetwork.MasterId == BaseNetwork.MainPlayer.Id;
		if (playerInfo.mId != -1)
		{
			if (Instance.m_ForceDic.ContainsKey(playerInfo.mFocreID))
			{
				Dictionary<int, int> dictionary = Instance.m_ForceDic[playerInfo.mFocreID];
				dictionary[playerInfo.mId] = playerInfo.mRoleID;
				ChangeRoomPlayerByNet(playerInfo);
				return;
			}
			Debug.Log("Find Not contains ForceID: " + playerInfo.mFocreID + "  PlayerID: " + playerInfo.mId + "  RoleID: " + playerInfo.mRoleID);
		}
	}

	public static void ChangeRoomPlayerByNet(RoomPlayerInfo playerInfo)
	{
		if (null == Instance)
		{
			return;
		}
		Instance.TryRemovePlayer(playerInfo.mId);
		if (Instance.m_ForceDic != null && Instance.m_ForceDic.Count() > 0)
		{
			ForceItem_N forceItem_N = Instance.m_ForceItemList.FirstOrDefault((ForceItem_N item) => item.GetForceID() == playerInfo.mFocreID);
			if (null != forceItem_N)
			{
				forceItem_N.JoinForceByNet(playerInfo, Instance.m_IsRoomMaster);
				Instance.m_ForceDic[playerInfo.mFocreID].Add(playerInfo.mId, playerInfo.mRoleID);
				Instance.ActiveStartBtn();
			}
		}
	}

	public static void RemoveRoomPlayerByNet(int playerId)
	{
		if (!(null == Instance))
		{
			Instance.TryRemovePlayer(playerId);
		}
	}

	public static void ChangePlayerDelayByNet(int playerID, int delay)
	{
		if (!(null == Instance))
		{
			ForceItem_N forceItem_N = Instance.FindForceItemByPlayerID(playerID);
			if (forceItem_N != null)
			{
				forceItem_N.ChangePlayerDelay(playerID, delay);
			}
		}
	}

	public static void ChangePlayerStateByNet(int playerID, int state)
	{
		if (!(null == Instance))
		{
			ForceItem_N forceItem_N = Instance.FindForceItemByPlayerID(playerID);
			if (forceItem_N != null)
			{
				forceItem_N.ChangePlayerState(playerID, state);
			}
		}
	}

	public static void GetNewMsgByNet(string playerName, string msg)
	{
		if (!(null == Instance))
		{
			string empty = string.Empty;
			empty = ((!(BaseNetwork.MainPlayer.RoleName == playerName)) ? "EDB1A6" : "99C68B");
			Instance.m_RoomChatComp.AddMsg(playerName, msg, empty);
		}
	}

	public static void KickPlayerByNet(int playerInstanceId)
	{
		if (!(null == Instance))
		{
			Instance.TryRemovePlayer(playerInstanceId);
			if (BaseNetwork.MainPlayer.Id == playerInstanceId)
			{
				MessageBox_N.ShowOkBox(PELocalization.GetString(8000183), PeSceneCtrl.Instance.GotoLobbyScene);
			}
		}
	}

	public void KickPlayerToNet(int playerInstanceId)
	{
		if (null != BaseNetwork.MainPlayer)
		{
			BaseNetwork.MainPlayer.KickPlayer(playerInstanceId);
		}
	}

	public void SyncRoomMsgToNet(string msg)
	{
		if (null != BaseNetwork.MainPlayer)
		{
			BaseNetwork.MainPlayer.SendMsg(msg);
		}
	}

	public void ChangePlayerTeamToNet(int forceId, int roleId)
	{
		if (!m_ForceDic.ContainsKey(forceId))
		{
			return;
		}
		int id = BaseNetwork.MainPlayer.Id;
		if (m_ForceDic[forceId].ContainsKey(id))
		{
			if (m_ForceDic[forceId][id] != roleId)
			{
				BaseNetwork.MainPlayer.RequestChangeTeam(forceId, roleId);
			}
		}
		else
		{
			BaseNetwork.MainPlayer.RequestChangeTeam(forceId, roleId);
		}
	}

	public void ActiveStartBtn()
	{
		if (PeGameMgr.IsCustom)
		{
			m_ForceItemList.ForEach(delegate(ForceItem_N a)
			{
				if (!a.CheckFixRoleIsFull())
				{
					EnableBtnStar(value: false);
				}
			});
			EnableBtnStar(value: true);
		}
		else
		{
			EnableBtnStar(value: true);
		}
	}
}

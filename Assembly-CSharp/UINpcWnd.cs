using System;
using System.Collections.Generic;
using Pathea;
using Pathea.PeEntityExt;
using Pathea.PeEntityExtMotion_Move;
using Pathea.PeEntityExtTrans;
using UnityEngine;

public class UINpcWnd : UIBaseWnd
{
	public delegate void OpenEvent(PeEntity npc);

	public MissionSelItem_N mPrefab;

	public UISprite mHeadSpr;

	public UILabel mNamelabel;

	public UITable mUITable;

	public UITexture mHeadTex;

	private List<MissionSelItem_N> mMissionItemList = new List<MissionSelItem_N>();

	public PeEntity m_CurSelNpc;

	private List<int> m_MissionList = new List<int>();

	private List<int> m_MissionListReply = new List<int>();

	private PeEntity m_Player;

	private bool m_bShowShop;

	private bool m_bShowStorage;

	private int mSelIndex;

	private PeEntity mSayHalotarget;

	private bool mSayHalo;

	public bool canShow = true;

	public int BtnClickMission;

	public PeEntity MPlayer
	{
		set
		{
			m_Player = value;
		}
	}

	public event OpenEvent ReviveOpenHandler;

	public void AddOpenEvent(OpenEvent handler)
	{
		this.ReviveOpenHandler = (OpenEvent)Delegate.Combine(this.ReviveOpenHandler, handler);
	}

	public void DeletOpenEvent(OpenEvent handler)
	{
		this.ReviveOpenHandler = (OpenEvent)Delegate.Remove(this.ReviveOpenHandler, handler);
	}

	public void SetCurSelNpc(PeEntity npc, bool sayHalo = false)
	{
		if (PeSingleton<PeCreature>.Instance.mainPlayer == null)
		{
			return;
		}
		m_Player = PeSingleton<PeCreature>.Instance.mainPlayer;
		m_CurSelNpc = npc;
		if (null == m_CurSelNpc)
		{
			return;
		}
		if (sayHalo && m_CurSelNpc != mSayHalotarget)
		{
			mSayHalotarget = m_CurSelNpc;
			mSayHalo = true;
		}
		if (m_CurSelNpc.GetUserData() is NpcMissionData { mInFollowMission: false })
		{
			m_CurSelNpc.CmdFaceToPoint(PeSingleton<PeCreature>.Instance.mainPlayer.ExtGetPos());
			m_CurSelNpc.CmdStartTalk();
			StroyManager.Instance.SetTalking(m_CurSelNpc, string.Empty);
			m_CurSelNpc.SayHiRandom();
			UpdateMission();
			ChangeHeadTex(m_CurSelNpc);
			if (this.ReviveOpenHandler != null)
			{
				this.ReviveOpenHandler(m_CurSelNpc);
			}
		}
	}

	public void ChangeHeadTex(PeEntity npc)
	{
		mNamelabel.text = npc.ExtGetName();
		if (EntityCreateMgr.Instance.IsRandomNpc(npc))
		{
			mHeadTex.enabled = true;
			mHeadSpr.enabled = false;
			mHeadTex.mainTexture = npc.ExtGetFaceTex();
			if (mHeadTex.mainTexture == null)
			{
				mHeadTex.enabled = false;
				mHeadSpr.enabled = true;
				mHeadSpr.spriteName = npc.ExtGetFaceIcon();
			}
		}
		else
		{
			mHeadTex.enabled = false;
			mHeadSpr.enabled = true;
			mHeadSpr.spriteName = npc.ExtGetFaceIcon();
		}
	}

	public bool CheckAddMissionListID(int id, NpcMissionData missionData)
	{
		if (id == 0)
		{
			return false;
		}
		if (id == 888 || MissionManager.HasRandomMission(id))
		{
			for (int i = 0; i < missionData.m_RecruitMissionList.Count; i++)
			{
				if (!MissionManager.Instance.HadCompleteMission(missionData.m_RecruitMissionList[i]))
				{
					return false;
				}
			}
			if (RMRepository.m_RandomFieldMap.ContainsKey(id) && missionData.mCurComMisNum >= RMRepository.m_RandomFieldMap[id].TargetIDMap.Count)
			{
				return false;
			}
		}
		else if ((id == 480 || id == 481) && MissionManager.Instance.HadCompleteMission(444) && !MissionManager.Instance.HadCompleteMission(507))
		{
			CSCreator creator = CSMain.GetCreator(0);
			if (creator == null)
			{
				if (!MissionManager.Instance.HadCompleteMission(505))
				{
					if (PeGameMgr.IsMulti)
					{
						MissionManager.Instance.RequestCompleteMission(554);
					}
					else
					{
						MissionManager.Instance.CompleteMission(554);
					}
					GameUI.Instance.mNPCTalk.UpdateNpcTalkInfo(505, 1);
					GameUI.Instance.mNPCTalk.PreShow();
				}
				return false;
			}
			if (creator.Assembly == null)
			{
				if (!MissionManager.Instance.HadCompleteMission(505))
				{
					if (PeGameMgr.IsMulti)
					{
						MissionManager.Instance.RequestCompleteMission(554);
					}
					else
					{
						MissionManager.Instance.CompleteMission(554);
					}
					GameUI.Instance.mNPCTalk.UpdateNpcTalkInfo(505, 1);
					GameUI.Instance.mNPCTalk.PreShow();
				}
				return false;
			}
			int mgCampNpcCount = StroyManager.Instance.GetMgCampNpcCount();
			int emptyBedCnt = CSMain.s_MgCreator.GetEmptyBedCnt();
			if (emptyBedCnt < mgCampNpcCount)
			{
				if (!MissionManager.Instance.HadCompleteMission(506))
				{
					GameUI.Instance.mNPCTalk.UpdateNpcTalkInfo(506, 1);
					GameUI.Instance.mNPCTalk.PreShow();
				}
				return false;
			}
			if (!MissionManager.Instance.HadCompleteMission(507))
			{
				GameUI.Instance.mNPCTalk.UpdateNpcTalkInfo(507, 1);
				GameUI.Instance.mNPCTalk.PreShow();
			}
			return false;
		}
		if (!MissionManager.Instance.IsGetTakeMission(id))
		{
			return false;
		}
		return true;
	}

	private bool CheckAddMissionReplyID(int id)
	{
		MissionCommonData missionCommonData = MissionManager.GetMissionCommonData(id);
		if (missionCommonData == null)
		{
			return false;
		}
		if ((id == 9137 || id == 9138) && missionCommonData.m_iReplyNpc != m_CurSelNpc.Id)
		{
			return false;
		}
		if (!missionCommonData.IsTalkMission() && (!MissionManager.Instance.HasMission(id) || MissionManager.Instance.HadCompleteMission(id)))
		{
			return false;
		}
		if (MissionRepository.IsAutoReplyMission(id))
		{
			return false;
		}
		if (MissionRepository.GetMissionNpcListName(id, bspe: true) == string.Empty)
		{
			if (PeGameMgr.IsMulti)
			{
				MissionManager.Instance.RequestCompleteMission(id);
			}
			else
			{
				MissionManager.Instance.CompleteMission(id);
				MissionCommonData missionCommonData2 = MissionManager.GetMissionCommonData(id);
				if (missionCommonData2 != null && missionCommonData2.m_Type != 0)
				{
					canShow = false;
				}
			}
			return false;
		}
		return true;
	}

	private void AddCSCreatorMission(NpcMissionData missionData)
	{
		CSCreator creator = CSMain.GetCreator(0);
		if (creator == null || creator.Assembly == null || (m_CurSelNpc != null && m_CurSelNpc.Id < 20000 && m_CurSelNpc.Id > 19990))
		{
			return;
		}
		int num = 0;
		if (!m_CurSelNpc.IsRecruited())
		{
			num = (creator.CanAddNpc() ? 191 : 204);
		}
		if (missionData.m_CSRecruitMissionList.Count < 1)
		{
			if (num != 0)
			{
				m_MissionList.Add(num);
			}
		}
		else
		{
			if (missionData.m_CSRecruitMissionList[0] <= 0)
			{
				return;
			}
			for (int i = 0; i < missionData.m_CSRecruitMissionList.Count; i++)
			{
				if (!MissionManager.Instance.HadCompleteMission(missionData.m_CSRecruitMissionList[i]))
				{
					return;
				}
			}
			if (num != 0)
			{
				m_MissionList.Add(num);
			}
		}
	}

	public void UpdateMission()
	{
		canShow = true;
		if (m_Player == null || m_CurSelNpc == null)
		{
			return;
		}
		m_MissionList.Clear();
		m_MissionListReply.Clear();
		int num = 0;
		m_bShowShop = false;
		m_bShowStorage = false;
		if (!(m_CurSelNpc.GetUserData() is NpcMissionData npcMissionData))
		{
			return;
		}
		int num2 = 0;
		num2 = (PeGameMgr.IsStory ? m_CurSelNpc.Id : ((!npcMissionData.m_bRandomNpc) ? m_CurSelNpc.Id : npcMissionData.m_Rnpc_ID));
		if (!PeGameMgr.IsTutorial)
		{
			StoreData npcStoreData = StoreRepository.GetNpcStoreData(num2);
			if (npcStoreData != null && npcStoreData.itemList.Count > 0)
			{
				m_bShowShop = true;
			}
		}
		for (int i = 0; i < npcMissionData.m_MissionList.Count; i++)
		{
			num = npcMissionData.m_MissionList[i];
			if (CheckAddMissionListID(num, npcMissionData))
			{
				num = StroyManager.Instance.ParseIDByColony(num);
				m_MissionList.Add(num);
			}
		}
		if (!AdQuestChainLimit(AdRMRepository.m_AdRandomGroup[npcMissionData.m_QCID]) && CheckAddMissionListID(npcMissionData.m_RandomMission, npcMissionData))
		{
			m_MissionList.Add(npcMissionData.m_RandomMission);
		}
		for (int j = 0; j < npcMissionData.m_MissionListReply.Count; j++)
		{
			if (CheckAddMissionReplyID(npcMissionData.m_MissionListReply[j]))
			{
				m_MissionListReply.Add(npcMissionData.m_MissionListReply[j]);
			}
		}
		AddCSCreatorMission(npcMissionData);
		ResetMissionList();
	}

	private bool AdQuestChainLimit(AdRandomGroup group)
	{
		if (group.m_preLimit.Count == 0)
		{
			return false;
		}
		if (group.m_requstAll)
		{
			for (int i = 0; i < group.m_preLimit.Count; i++)
			{
				if (!MissionManager.Instance.m_PlayerMission.HadCompleteMissionAnyNum(group.m_preLimit[i]))
				{
					return true;
				}
			}
			return false;
		}
		for (int j = 0; j < group.m_preLimit.Count; j++)
		{
			if (MissionManager.Instance.m_PlayerMission.HadCompleteMissionAnyNum(group.m_preLimit[j]))
			{
				return false;
			}
		}
		return true;
	}

	public void GetMutexID(int curMisID, ref List<int> idlist)
	{
		idlist.Clear();
		GameUI.Instance.mNPCTalk.m_selectMissionSource = curMisID;
		MissionCommonData missionCommonData = MissionManager.GetMissionCommonData(curMisID);
		if (missionCommonData == null || missionCommonData.m_GuanLianList.Count == 0)
		{
			return;
		}
		for (int i = 0; i < missionCommonData.m_GuanLianList.Count; i++)
		{
			if (MissionManager.Instance.m_PlayerMission.IsGetTakeMission(missionCommonData.m_GuanLianList[i], isPreLimitOn: false))
			{
				idlist.Add(missionCommonData.m_GuanLianList[i]);
			}
		}
	}

	private void AddMissionItem(int missionId)
	{
		MissionSelItem_N missionSelItem_N = UnityEngine.Object.Instantiate(mPrefab);
		missionSelItem_N.gameObject.name = "ItemName" + mSelIndex;
		mSelIndex++;
		missionSelItem_N.transform.parent = mUITable.transform;
		missionSelItem_N.transform.localPosition = Vector3.zero;
		missionSelItem_N.transform.localRotation = Quaternion.identity;
		missionSelItem_N.transform.localScale = Vector3.one;
		string text = MissionRepository.GetMissionNpcListName(missionId, bspe: false);
		if (missionId == 888 && EntityCreateMgr.Instance.IsRandomNpc(m_CurSelNpc) && m_CurSelNpc.GetUserData() is NpcMissionData npcMissionData && (npcMissionData.mCurComMisNum >= npcMissionData.mCompletedMissionCount || UINPCTalk.m_QuickZM))
		{
			text = "[ffff00]" + text + "[-]";
		}
		missionSelItem_N.SetMission(missionId, text, this, m_Player);
		mMissionItemList.Add(missionSelItem_N);
	}

	private void AddMissionReplyItem(int missionId)
	{
		MissionSelItem_N missionSelItem_N = UnityEngine.Object.Instantiate(mPrefab);
		missionSelItem_N.gameObject.name = "ItemName" + mSelIndex;
		mSelIndex++;
		missionSelItem_N.transform.parent = mUITable.transform;
		missionSelItem_N.transform.localPosition = Vector3.zero;
		missionSelItem_N.transform.localRotation = Quaternion.identity;
		missionSelItem_N.transform.localScale = Vector3.one;
		string empty = string.Empty;
		empty = MissionRepository.GetMissionNpcListName(missionId, bspe: true);
		missionSelItem_N.SetMission(missionId, empty, this, m_Player);
		mMissionItemList.Add(missionSelItem_N);
	}

	private void ShowStorage()
	{
		MissionSelItem_N missionSelItem_N = UnityEngine.Object.Instantiate(mPrefab);
		missionSelItem_N.gameObject.name = "ItemName" + mSelIndex;
		mSelIndex++;
		missionSelItem_N.transform.parent = mUITable.transform;
		missionSelItem_N.transform.localPosition = Vector3.zero;
		missionSelItem_N.transform.localRotation = Quaternion.identity;
		missionSelItem_N.transform.localScale = Vector3.one;
		missionSelItem_N.SetMission(-3, PELocalization.GetString(8000139), "cangkuarr", this, m_Player);
		mMissionItemList.Add(missionSelItem_N);
	}

	private void AddShopItem(int missionId)
	{
		MissionSelItem_N missionSelItem_N = UnityEngine.Object.Instantiate(mPrefab);
		missionSelItem_N.gameObject.name = "ItemName" + mSelIndex;
		mSelIndex++;
		missionSelItem_N.transform.parent = mUITable.transform;
		missionSelItem_N.transform.localPosition = Vector3.zero;
		missionSelItem_N.transform.localRotation = Quaternion.identity;
		missionSelItem_N.transform.localScale = Vector3.one;
		missionSelItem_N.SetMission(missionId, PELocalization.GetString(8000011), "ShopIntalk", this, m_Player);
		mMissionItemList.Add(missionSelItem_N);
	}

	private void AddCloseItem(int missionId)
	{
		MissionSelItem_N missionSelItem_N = UnityEngine.Object.Instantiate(mPrefab);
		missionSelItem_N.gameObject.name = "ItemName" + mSelIndex;
		mSelIndex++;
		missionSelItem_N.transform.parent = mUITable.transform;
		missionSelItem_N.transform.localPosition = Vector3.zero;
		missionSelItem_N.transform.localRotation = Quaternion.identity;
		missionSelItem_N.transform.localScale = Vector3.one;
		missionSelItem_N.SetMission(missionId, PELocalization.GetString(8000010), "SubMission", this, m_Player);
		mMissionItemList.Add(missionSelItem_N);
	}

	private void ResetMissionList()
	{
		mSelIndex = 0;
		for (int num = mMissionItemList.Count - 1; num >= 0; num--)
		{
			mMissionItemList[num].transform.parent = null;
			UnityEngine.Object.Destroy(mMissionItemList[num].gameObject);
		}
		mMissionItemList.Clear();
		for (int i = 0; i < m_MissionList.Count; i++)
		{
			if (m_MissionList[i] == 888)
			{
				if (!(m_CurSelNpc.GetUserData() is NpcMissionData npcMissionData))
				{
					return;
				}
				if (npcMissionData.mCurComMisNum < npcMissionData.mCompletedMissionCount)
				{
					continue;
				}
			}
			AddMissionItem(m_MissionList[i]);
		}
		for (int j = 0; j < m_MissionListReply.Count; j++)
		{
			AddMissionReplyItem(m_MissionListReply[j]);
		}
		if (m_bShowShop)
		{
			AddShopItem(-1);
		}
		if (m_bShowStorage)
		{
			ShowStorage();
		}
		AddCloseItem(-2);
		mUITable.Reposition();
	}

	private void NpcOnColonyRoad()
	{
		if (!GameUI.Instance.mNPCTalk.isPlayingTalk)
		{
			List<int> list = new List<int>();
			list.Add(4079);
			GameUI.Instance.mNPCTalk.UpdateNpcTalkInfo(list);
			GameUI.Instance.mNPCTalk.PreShow();
		}
	}

	public override void Show()
	{
		if (!canShow || null == m_CurSelNpc)
		{
			return;
		}
		if (m_CurSelNpc.NpcCmpt != null && m_CurSelNpc.NpcCmpt.CsBacking)
		{
			NpcOnColonyRoad();
		}
		else if (m_CurSelNpc.GetUserData() is NpcMissionData npcMissionData)
		{
			if (npcMissionData.mInFollowMission)
			{
				m_CurSelNpc.CmdStopTalk();
				StroyManager.Instance.RemoveReq(m_CurSelNpc, EReqType.Dialogue);
			}
			else
			{
				PlayerStartAudio();
				base.Show();
			}
		}
	}

	private void BaseShow()
	{
		base.Show();
	}

	protected override void OnClose()
	{
		if (m_CurSelNpc != null)
		{
			m_CurSelNpc.SayByeRandom();
		}
		PlayerEndAudio();
		Hide();
	}

	protected override void OnHide()
	{
		base.OnHide();
		if (!(m_CurSelNpc == null))
		{
			m_CurSelNpc.CmdFaceToPoint(Vector3.zero);
			m_CurSelNpc.CmdStopTalk();
			mSayHalotarget = null;
			if ((!GameUI.Instance.mNPCTalk.isPlayingTalk || GameUI.Instance.mNPCTalk.CurTalkInfoIsRadio()) && !GameUI.Instance.mShopWnd.isShopping)
			{
				StroyManager.Instance.RemoveReq(m_CurSelNpc, EReqType.Dialogue);
			}
		}
	}

	public void OnMutexBtnClick(int missionId)
	{
		ActiveWnd();
		if (missionId == -3)
		{
			if (null == m_CurSelNpc)
			{
				Debug.LogError("open storage, but npc is null");
			}
			NpcStorage npcStorage = null;
			npcStorage = ((!GameConfig.IsMultiMode) ? NpcStorageMgr.GetSinglePlayerStorage() : NpcStorageMgr.GetStorage(m_Player.Id));
			if (npcStorage == null)
			{
				Debug.LogError(string.Concat(m_CurSelNpc, " is has no storage."));
			}
			Hide();
		}
		switch (missionId)
		{
		case -1:
		{
			if (!(m_CurSelNpc.GetUserData() is NpcMissionData npcMissionData))
			{
				return;
			}
			int num = 0;
			num = (PeGameMgr.IsStory ? m_CurSelNpc.Id : ((!npcMissionData.m_bRandomNpc) ? m_CurSelNpc.Id : npcMissionData.m_Rnpc_ID));
			if (!GameConfig.IsMultiMode)
			{
				if (GameUI.Instance.mShopWnd.UpdataShop(StoreRepository.GetNpcStoreData(num)))
				{
					GameUI.Instance.mShopWnd.Show();
				}
			}
			else if (null != PlayerNetwork.mainPlayer)
			{
				PlayerNetwork.mainPlayer.RequestShopData(m_CurSelNpc.Id);
			}
			return;
		}
		case -2:
			OnClose();
			return;
		}
		if (m_MissionList.Contains(missionId))
		{
			if (!MissionManager.Instance.CheckCSCreatorMis(missionId) || !MissionManager.Instance.CheckHeroMis())
			{
				return;
			}
			if (MissionManager.Instance.IsTempLimit(missionId))
			{
				if (!GameUI.Instance.mNPCTalk.isPlayingTalk)
				{
					GameUI.Instance.mNPCTalk.UpdateNpcTalkInfo(new List<int> { 4080 });
					GameUI.Instance.mNPCTalk.PreShow();
				}
				else
				{
					GameUI.Instance.mNPCTalk.UpdateNpcTalkInfo(new List<int> { 4080 }, null, IsClearTalkList: false);
				}
				Hide();
				return;
			}
			BtnClickMission = missionId;
			if (MissionRepository.HaveTalkOP(missionId))
			{
				Hide();
				GameUI.Instance.mNPCTalk.UpdateNpcTalkInfo(missionId, 1);
				GameUI.Instance.mNPCTalk.NormalOrSP(0);
				GameUI.Instance.mNPCTalk.PreShow();
			}
			else
			{
				MissionManager.Instance.SetGetTakeMission(missionId, m_CurSelNpc, MissionManager.TakeMissionType.TakeMissionType_Get);
				Hide();
			}
			BtnClickMission = 0;
			return;
		}
		if (!m_MissionListReply.Contains(missionId))
		{
			return;
		}
		if (MissionManager.Instance.IsReplyMission(missionId))
		{
			if (PeGameMgr.IsMulti)
			{
				MissionManager.Instance.RequestCompleteMission(missionId);
				return;
			}
			MissionManager.Instance.CompleteMission(missionId);
			UpdateMission();
		}
		else if (MissionRepository.HaveTalkIN(missionId))
		{
			Hide();
			GameUI.Instance.mNPCTalk.UpdateNpcTalkInfo(missionId, 2);
			GameUI.Instance.mNPCTalk.PreShow();
		}
	}

	private void ReposTable()
	{
		base.gameObject.SetActive(value: true);
		mUITable.Reposition();
		base.gameObject.SetActive(value: false);
	}

	private void PlayerStartAudio()
	{
		if (!mSayHalo)
		{
			return;
		}
		mSayHalo = false;
		if (!(null == m_CurSelNpc) && m_CurSelNpc.proto == EEntityProto.Npc)
		{
			NpcProtoDb.Item item = NpcProtoDb.Get(m_CurSelNpc.ProtoID);
			if (item != null && item.chart1 != null && item.chart1.Length > 0)
			{
				AudioManager.instance.Create(m_CurSelNpc.position + 1.8f * Vector3.up, item.chart1[UnityEngine.Random.Range(0, item.chart1.Length)], m_CurSelNpc.tr);
			}
		}
	}

	private void PlayerEndAudio()
	{
		if (!(null == m_CurSelNpc) && m_CurSelNpc.proto == EEntityProto.Npc)
		{
			NpcProtoDb.Item item = NpcProtoDb.Get(m_CurSelNpc.ProtoID);
			if (item != null && item.chart2 != null && item.chart2.Length > 0)
			{
				AudioManager.instance.Create(m_CurSelNpc.position + 1.8f * Vector3.up, item.chart2[UnityEngine.Random.Range(0, item.chart2.Length)], m_CurSelNpc.tr);
			}
		}
	}
}

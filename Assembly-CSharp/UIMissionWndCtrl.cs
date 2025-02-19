using System.Collections.Generic;
using ItemAsset;
using UnityEngine;

public class UIMissionWndCtrl : UIBaseWnd
{
	public UIMissionTree mMainStroyTree;

	public UIMissionTree mSideQuestTree;

	public UICheckbox mCkMainStory;

	public UICheckbox mCkSideQuest;

	public Grid_N mPrefabGrid_N;

	public MissionTargetItem_N mPrefabTarget;

	public UIGrid mRewardsGrid;

	public UITable mTargetGrid;

	public UILabel mSPLabel;

	public UILabel mDesLabel;

	public UILabel mGiverName;

	public UISprite mGiverSpr;

	public UISprite mGiverBg;

	public UILabel mSubmitName;

	public UISprite mSubmitSpr;

	public UISprite mSubmitBg;

	public UIScrollBar mTargetScroll;

	public UIScrollBar mRewardsScroll;

	private List<MissionTargetItem_N> mTargetList = new List<MissionTargetItem_N>();

	private List<Grid_N> mRewardsList = new List<Grid_N>();

	private bool fristShowMissionTrack = true;

	private UIMissionMgr.MissionView delView;

	public override void Show()
	{
		base.Show();
		if (fristShowMissionTrack)
		{
			GameUI.Instance.mMissionTrackWnd.Show();
			fristShowMissionTrack = false;
		}
	}

	protected override void InitWindow()
	{
		base.InitWindow();
		UIMissionMgr.Instance.e_AddMission += AddMission;
		UIMissionMgr.Instance.e_DeleteMission += DeleteMission;
		UIMissionMgr.Instance.e_ReflahMissionWnd += ReflashMissionWnd;
		mMainStroyTree.e_ChangeSelectedNode += MissionNodeOnSelectChange;
		mSideQuestTree.e_ChangeSelectedNode += MissionNodeOnSelectChange;
		ReGetAllMission();
	}

	public void ReGetAllMission()
	{
		ClearMission();
		ClearMissionContent();
		foreach (KeyValuePair<int, UIMissionMgr.MissionView> item in UIMissionMgr.Instance.m_MissonMap)
		{
			AddMission(item.Value);
		}
	}

	private void AddMission(UIMissionMgr.MissionView misView)
	{
		UIMissionNode uIMissionNode = null;
		if (misView.mMissionType == MissionType.MissionType_Main || misView.mMissionType == MissionType.MissionType_Time)
		{
			uIMissionNode = mMainStroyTree.AddMissionNode(null, misView.mMissionTitle);
		}
		else if (misView.mMissionType == MissionType.MissionType_Sub)
		{
			uIMissionNode = mSideQuestTree.AddMissionNode(null, misView.mMissionTitle);
		}
		uIMissionNode.mCheckBoxTag.isChecked = misView.mMissionTag;
		uIMissionNode.mData = misView;
		uIMissionNode.e_BtnDelete += MissionNodeOnDelete;
		uIMissionNode.e_CheckedTg += MissionNodeOnCheckTag;
		SelectMissionNode(uIMissionNode);
	}

	private void DeleteMission(UIMissionMgr.MissionView misView)
	{
		UIMissionNode node = null;
		if (misView.mMissionType == MissionType.MissionType_Main || misView.mMissionType == MissionType.MissionType_Time)
		{
			node = mMainStroyTree.mNodes.Find(delegate(UIMissionNode nd)
			{
				UIMissionMgr.MissionView missionView = nd.mData as UIMissionMgr.MissionView;
				return missionView == misView;
			});
			mMainStroyTree.DeleteMissionNode(node);
			node = ((mMainStroyTree.mNodes.Count != 0) ? mMainStroyTree.mNodes[0] : null);
		}
		else if (misView.mMissionType == MissionType.MissionType_Sub)
		{
			node = mSideQuestTree.mNodes.Find(delegate(UIMissionNode nd)
			{
				UIMissionMgr.MissionView missionView2 = nd.mData as UIMissionMgr.MissionView;
				return missionView2 == misView;
			});
			mSideQuestTree.DeleteMissionNode(node);
			node = ((mSideQuestTree.mNodes.Count != 0) ? mSideQuestTree.mNodes[0] : null);
		}
		SelectMissionNode(node);
	}

	public void ClearMission()
	{
		mMainStroyTree.Clear();
		mSideQuestTree.Clear();
	}

	private void SelectMissionNode(UIMissionNode node)
	{
		if (node == null)
		{
			ClearMissionContent();
		}
		else
		{
			ReFlashMissionContent(node);
		}
	}

	private void ReflashMissionWnd()
	{
		if (!(mMainStroyTree == null) && !(mSideQuestTree == null))
		{
			UIMissionNode node = ((!mMainStroyTree.gameObject.activeSelf) ? mSideQuestTree.mSelectedNode : mMainStroyTree.mSelectedNode);
			SelectMissionNode(node);
		}
	}

	public void ClearMissionContent()
	{
		for (int i = 0; i < mTargetList.Count; i++)
		{
			mTargetList[i].transform.parent = null;
			Object.Destroy(mTargetList[i].gameObject);
		}
		mTargetList.Clear();
		for (int j = 0; j < mRewardsList.Count; j++)
		{
			mRewardsList[j].transform.parent = null;
			Object.Destroy(mRewardsList[j].gameObject);
		}
		mRewardsList.Clear();
		mDesLabel.text = string.Empty;
		mSPLabel.text = string.Empty;
		mGiverName.text = string.Empty;
		mSubmitName.text = string.Empty;
		mGiverSpr.spriteName = "Null";
		mSubmitSpr.spriteName = "Null";
		mGiverSpr.transform.parent.gameObject.SetActive(value: false);
		mSubmitSpr.transform.parent.gameObject.SetActive(value: false);
	}

	public void SelectMissionNodeByData(object data)
	{
		if (data == null || !(data is UIMissionMgr.MissionView))
		{
			return;
		}
		UIMissionMgr.MissionView misView = (UIMissionMgr.MissionView)data;
		if (misView.mMissionType == MissionType.MissionType_Main || misView.mMissionType == MissionType.MissionType_Time)
		{
			mCkMainStory.isChecked = true;
			mCkSideQuest.isChecked = false;
			BtnMainStroy();
			UIMissionNode uIMissionNode = mMainStroyTree.mNodes.Find((UIMissionNode a) => a.mParent == null && a.mData != null && a.mData is UIMissionMgr.MissionView && ((UIMissionMgr.MissionView)a.mData).mMissionID == misView.mMissionID);
			if (null != uIMissionNode)
			{
				if (null != mMainStroyTree.mSelectedNode)
				{
					mMainStroyTree.mSelectedNode.Selected = false;
				}
				mMainStroyTree.mSelectedNode = uIMissionNode;
				uIMissionNode.Selected = true;
				ReFlashMissionContent(uIMissionNode);
			}
		}
		else
		{
			if (misView.mMissionType != MissionType.MissionType_Sub)
			{
				return;
			}
			mCkMainStory.isChecked = false;
			mCkSideQuest.isChecked = true;
			BtnSideQuest();
			UIMissionNode uIMissionNode2 = mSideQuestTree.mNodes.Find((UIMissionNode a) => a.mParent == null && a.mData != null && a.mData is UIMissionMgr.MissionView && ((UIMissionMgr.MissionView)a.mData).mMissionID == misView.mMissionID);
			if (null != uIMissionNode2)
			{
				if (null != mSideQuestTree.mSelectedNode)
				{
					mSideQuestTree.mSelectedNode.Selected = false;
				}
				mSideQuestTree.mSelectedNode = uIMissionNode2;
				uIMissionNode2.Selected = true;
				ReFlashMissionContent(uIMissionNode2);
			}
		}
	}

	private void ReFlashMissionContent(UIMissionNode node)
	{
		if (mTargetScroll != null)
		{
			mTargetScroll.scrollValue = 0f;
		}
		ClearMissionContent();
		if (node.mData is UIMissionMgr.MissionView missionView)
		{
			mDesLabel.text = missionView.mMissionDesc;
			int mMissionID = missionView.mMissionID;
			MissionCommonData missionCommonData = MissionManager.GetMissionCommonData(mMissionID);
			if (missionCommonData != null && missionCommonData.addSpValue > 0)
			{
				mSPLabel.text = "SP + [ffff00]" + missionCommonData.addSpValue + "[-]";
			}
			SetGiver(missionView.mMissionStartNpc.mNpcIcoStr, missionView.mMissionStartNpc.mName);
			SetSubmit(missionView.mMissionReplyNpc.mNpcIcoStr, missionView.mMissionReplyNpc.mName);
			for (int i = 0; i < missionView.mTargetList.Count; i++)
			{
				AddTarget(missionView.mTargetList[i]);
			}
			mTargetGrid.Reposition();
			for (int j = 0; j < missionView.mRewardsList.Count; j++)
			{
				AddRewards(missionView.mRewardsList[j]);
			}
			mRewardsGrid.Reposition();
			if (mRewardsScroll != null)
			{
				mRewardsScroll.scrollValue = 0f;
			}
		}
	}

	private void AddTarget(UIMissionMgr.TargetShow target)
	{
		MissionTargetItem_N missionTargetItem_N = Object.Instantiate(mPrefabTarget);
		missionTargetItem_N.transform.parent = mTargetGrid.transform;
		missionTargetItem_N.transform.localPosition = new Vector3(0f, 0f, -1f);
		missionTargetItem_N.transform.localRotation = Quaternion.identity;
		missionTargetItem_N.transform.localScale = Vector3.one;
		missionTargetItem_N.SetTarget(target);
		mTargetList.Add(missionTargetItem_N);
	}

	private void AddRewards(ItemSample itemGrid)
	{
		Grid_N grid_N = Object.Instantiate(mPrefabGrid_N);
		grid_N.transform.parent = mRewardsGrid.transform;
		grid_N.transform.localPosition = new Vector3(0f, 0f, -1f);
		grid_N.transform.localRotation = Quaternion.identity;
		grid_N.transform.localScale = Vector3.one;
		grid_N.SetItem(itemGrid);
		mRewardsList.Add(grid_N);
	}

	private void SetGiver(string IconName, string name)
	{
		bool flag = true;
		if (name == null || name.Length == 0)
		{
			flag = false;
		}
		mGiverSpr.enabled = flag;
		mGiverName.enabled = flag;
		mGiverBg.enabled = flag;
		mGiverSpr.transform.parent.gameObject.SetActive(value: true);
		mGiverSpr.spriteName = IconName;
		mGiverSpr.MakePixelPerfect();
		mGiverName.text = name;
	}

	private void SetSubmit(string IconName, string name)
	{
		bool flag = true;
		if (name == null || name.Length == 0)
		{
			flag = false;
		}
		mSubmitSpr.enabled = flag;
		mSubmitName.enabled = flag;
		mSubmitBg.enabled = flag;
		mSubmitSpr.transform.parent.gameObject.SetActive(value: true);
		mSubmitSpr.spriteName = IconName;
		mSubmitSpr.MakePixelPerfect();
		mSubmitName.text = name;
	}

	private void BtnMainStroy()
	{
		mSideQuestTree.gameObject.SetActive(value: false);
		mMainStroyTree.gameObject.SetActive(value: true);
		SelectMissionNode(mMainStroyTree.mSelectedNode);
	}

	private void BtnSideQuest()
	{
		mMainStroyTree.gameObject.SetActive(value: false);
		mSideQuestTree.gameObject.SetActive(value: true);
		SelectMissionNode(mSideQuestTree.mSelectedNode);
	}

	private void MissionNodeOnDelete(object sender)
	{
		UIMissionNode uIMissionNode = sender as UIMissionNode;
		if (uIMissionNode == null)
		{
			return;
		}
		delView = uIMissionNode.mData as UIMissionMgr.MissionView;
		if (delView == null)
		{
			return;
		}
		MissionCommonData missionCommonData = MissionManager.GetMissionCommonData(delView.mMissionID);
		if (missionCommonData != null)
		{
			if (!missionCommonData.m_bGiveUp)
			{
				new PeTipMsg(PELocalization.GetString(8000174), PeTipMsg.EMsgLevel.Warning);
			}
			else
			{
				MessageBox_N.ShowYNBox(PELocalization.GetString(8000066), DeleteMissionOk);
			}
		}
	}

	private void DeleteMissionOk()
	{
		if ((bool)UIMissionMgr.Instance)
		{
			UIMissionMgr.Instance.DeleteMission(delView);
		}
		delView = null;
	}

	private void MissionNodeOnSelectChange(object sender)
	{
		UIMissionNode uIMissionNode = sender as UIMissionNode;
		if (!(uIMissionNode == null))
		{
			SelectMissionNode(uIMissionNode);
		}
	}

	private void MissionNodeOnCheckTag(object sender, bool isChecked)
	{
		UIMissionNode uIMissionNode = sender as UIMissionNode;
		if (!(uIMissionNode == null) && uIMissionNode.mData is UIMissionMgr.MissionView missionView)
		{
			missionView.mMissionTag = isChecked;
			UIMissionMgr.Instance.CheckMissionTag(missionView);
		}
	}
}

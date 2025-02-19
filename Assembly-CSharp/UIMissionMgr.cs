using System.Collections.Generic;
using ItemAsset;
using Pathea;
using Pathea.PeEntityExt;
using UnityEngine;

public class UIMissionMgr : MonoBehaviour
{
	public class MissionView
	{
		public int mMissionID;

		public MissionType mMissionType;

		public string mMissionTitle;

		public string mMissionDesc;

		public bool mMissionTag;

		public bool mComplete;

		public NpcInfo mMissionStartNpc;

		public NpcInfo mMissionEndNpc;

		public NpcInfo mMissionReplyNpc;

		public bool NeedArrow;

		public Vector3 mEndMisPos;

		public int mAttachOnId;

		public List<TargetShow> mTargetList;

		public List<ItemSample> mRewardsList;

		public MissionView()
		{
			mTargetList = new List<TargetShow>();
			mRewardsList = new List<ItemSample>();
			mMissionStartNpc = new NpcInfo();
			mMissionEndNpc = new NpcInfo();
			mMissionReplyNpc = new NpcInfo();
			mComplete = false;
			mMissionTag = true;
			mEndMisPos = Vector3.zero;
			NeedArrow = false;
		}

		public static bool MatchID(TargetShow ite, int targetid)
		{
			return ite.mID == targetid;
		}
	}

	public class GetableMisView
	{
		public int mMissionID;

		public string mMissionTitle;

		public Vector3 mPosition;

		public NpcInfo TargetNpcInfo;

		public int mAttachOnId;

		public GetableMisView(int missionID, string misTitle, Vector3 positon, int attachOnID)
		{
			mMissionID = missionID;
			mMissionTitle = misTitle;
			mPosition = positon;
			TargetNpcInfo = new NpcInfo();
			mAttachOnId = attachOnID;
		}
	}

	public class NpcInfo
	{
		public string mName;

		public string mNpcIcoStr;
	}

	public class TargetShow
	{
		public string mContent;

		public List<string> mIconName;

		public bool mComplete;

		public int mCount;

		public int mMaxCount;

		public int mID;

		public Vector3 mPosition;

		public float Radius;

		public int mAttachOnID;

		public TargetShow(int id = 0)
		{
			mID = id;
			mContent = string.Empty;
			mIconName = new List<string>();
			mCount = 0;
			mMaxCount = 0;
			mComplete = false;
			mPosition = Vector3.zero;
			Radius = -1f;
		}
	}

	public delegate void MissionViewEvent(MissionView misView);

	public delegate void BaseMissionEvent();

	public delegate void UnMisViewEvent(GetableMisView unMisView);

	private static UIMissionMgr mInstance;

	private MissionLabelMgr mLabelMissionMgr;

	public Dictionary<int, MissionView> m_MissonMap = new Dictionary<int, MissionView>();

	public Dictionary<int, GetableMisView> m_GetableMissonMap = new Dictionary<int, GetableMisView>();

	public static UIMissionMgr Instance => mInstance;

	public event MissionViewEvent e_AddMission;

	public event MissionViewEvent e_DeleteMission;

	public event MissionViewEvent e_CheckTagMission;

	public event BaseMissionEvent e_ReflahMissionWnd;

	public event UnMisViewEvent e_AddGetableMission;

	public event UnMisViewEvent e_DelGetableMission;

	private void Awake()
	{
		mInstance = this;
		ClearMission();
		mLabelMissionMgr = new MissionLabelMgr();
	}

	public void ClearMission()
	{
		m_MissonMap.Clear();
		m_GetableMissonMap.Clear();
	}

	public MissionView GetMissionView(int missionID)
	{
		if (m_MissonMap.ContainsKey(missionID))
		{
			return m_MissonMap[missionID];
		}
		return null;
	}

	public void RefalshMissionWnd()
	{
		if (this.e_ReflahMissionWnd != null)
		{
			this.e_ReflahMissionWnd();
		}
	}

	public bool AddMission(MissionView misView, bool isIgnoreHadCreate = false)
	{
		if (!isIgnoreHadCreate)
		{
			if (m_MissonMap.ContainsKey(misView.mMissionID))
			{
				return false;
			}
			m_MissonMap[misView.mMissionID] = misView;
		}
		if (this.e_AddMission != null)
		{
			this.e_AddMission(misView);
		}
		return true;
	}

	public bool DeleteMission(int missionID)
	{
		if (!m_MissonMap.ContainsKey(missionID))
		{
			return false;
		}
		if (this.e_DeleteMission != null)
		{
			this.e_DeleteMission(m_MissonMap[missionID]);
		}
		return m_MissonMap.Remove(missionID);
	}

	public bool DeleteMission(MissionView misView)
	{
		if (misView == null)
		{
			return false;
		}
		return DeleteMission(misView.mMissionID);
	}

	public void UpdateGetableMission()
	{
		List<int> list = null;
		foreach (int key in m_GetableMissonMap.Keys)
		{
			if (!MissionManager.Instance.IsGetTakeMission(key))
			{
				this.e_DelGetableMission(m_GetableMissonMap[key]);
				if (list == null)
				{
					list = new List<int>();
				}
				list.Add(key);
			}
		}
		if (list == null)
		{
			return;
		}
		foreach (int item in list)
		{
			m_GetableMissonMap.Remove(item);
		}
	}

	public void DeleteMission(PeEntity npc)
	{
		if (!PeGameMgr.IsMulti || !(npc.GetUserData() is NpcMissionData npcMissionData))
		{
			return;
		}
		if (npc.proto == EEntityProto.Npc)
		{
			foreach (int mission in npcMissionData.m_MissionList)
			{
				if (MissionManager.Instance.IsGetTakeMission(mission) && m_GetableMissonMap.ContainsKey(mission) && this.e_DelGetableMission != null)
				{
					this.e_DelGetableMission(m_GetableMissonMap[mission]);
				}
			}
			{
				foreach (int item in npcMissionData.m_MissionListReply)
				{
					if (!MissionManager.Instance.HadCompleteMission(item) && m_MissonMap.ContainsKey(item) && this.e_DeleteMission != null)
					{
						this.e_DeleteMission(m_MissonMap[item]);
					}
				}
				return;
			}
		}
		if (npc.proto == EEntityProto.RandomNpc && PeGameMgr.IsStory && npcMissionData.m_RandomMission != 0)
		{
			if (m_GetableMissonMap.ContainsKey(npcMissionData.m_RandomMission) && this.e_DelGetableMission != null)
			{
				this.e_DelGetableMission(m_GetableMissonMap[npcMissionData.m_RandomMission]);
			}
			if (m_MissonMap.ContainsKey(npcMissionData.m_RandomMission) && this.e_DeleteMission != null)
			{
				this.e_DeleteMission(m_MissonMap[npcMissionData.m_RandomMission]);
			}
		}
	}

	public void AddMission(PeEntity npc)
	{
		if (!PeGameMgr.IsMulti || !(npc.GetUserData() is NpcMissionData npcMissionData))
		{
			return;
		}
		MissionCommonData missionCommonData;
		if (npc.proto == EEntityProto.Npc)
		{
			foreach (int mission in npcMissionData.m_MissionList)
			{
				if (m_GetableMissonMap.ContainsKey(mission))
				{
					missionCommonData = MissionManager.GetMissionCommonData(mission);
					if (missionCommonData != null)
					{
						GetableMisView getableMisView = new GetableMisView(mission, missionCommonData.m_MissionName, npc.position, npc.Id);
						getableMisView.TargetNpcInfo.mName = npc.enityInfoCmpt.characterName.fullName;
						getableMisView.TargetNpcInfo.mNpcIcoStr = ((!string.IsNullOrEmpty(npc.enityInfoCmpt.faceIconBig)) ? npc.enityInfoCmpt.faceIconBig : "npc_big_Unknown");
						Instance.AddGetableMission(getableMisView, isIgnoreHadCreate: true);
					}
				}
			}
			{
				foreach (int item in npcMissionData.m_MissionListReply)
				{
					if (!m_MissonMap.ContainsKey(item))
					{
						continue;
					}
					missionCommonData = MissionManager.GetMissionCommonData(item);
					if (missionCommonData == null)
					{
						continue;
					}
					Dictionary<string, string> missionFlagType = MissionManager.Instance.m_PlayerMission.GetMissionFlagType(item);
					if (missionFlagType != null)
					{
						MissionView missionView = new MissionView();
						missionView.mMissionID = missionCommonData.m_ID;
						missionView.mMissionType = missionCommonData.m_Type;
						missionView.mMissionTitle = missionCommonData.m_MissionName;
						npc = PeSingleton<EntityMgr>.Instance.Get(missionCommonData.m_iNpc);
						if (npc != null)
						{
							missionView.mMissionStartNpc.mName = npc.enityInfoCmpt.characterName.fullName;
							missionView.mMissionStartNpc.mNpcIcoStr = ((!string.IsNullOrEmpty(npc.enityInfoCmpt.faceIconBig)) ? npc.enityInfoCmpt.faceIconBig : "npc_big_Unknown");
						}
						npc = PeSingleton<EntityMgr>.Instance.Get(missionCommonData.m_iReplyNpc);
						if (npc != null)
						{
							missionView.mMissionEndNpc.mName = npc.enityInfoCmpt.characterName.fullName;
							missionView.mMissionEndNpc.mNpcIcoStr = ((!string.IsNullOrEmpty(npc.enityInfoCmpt.faceIconBig)) ? npc.enityInfoCmpt.faceIconBig : "npc_big_Unknown");
							missionView.mEndMisPos = npc.position;
							missionView.mAttachOnId = npc.Id;
							missionView.NeedArrow = true;
						}
						MissionManager.Instance.ParseMissionFlag(missionCommonData, missionFlagType, missionView);
						Instance.AddMission(missionView, isIgnoreHadCreate: true);
						Instance.RefalshMissionWnd();
					}
				}
				return;
			}
		}
		if (npc.proto != EEntityProto.RandomNpc || !PeGameMgr.IsStory || npcMissionData.m_RandomMission == 0)
		{
			return;
		}
		missionCommonData = MissionManager.GetMissionCommonData(npcMissionData.m_RandomMission);
		if (missionCommonData == null)
		{
			return;
		}
		if (m_GetableMissonMap.ContainsKey(npcMissionData.m_RandomMission))
		{
			GetableMisView getableMisView2 = new GetableMisView(npcMissionData.m_RandomMission, missionCommonData.m_MissionName, npc.position, npc.Id);
			getableMisView2.TargetNpcInfo.mName = npc.enityInfoCmpt.characterName.fullName;
			getableMisView2.TargetNpcInfo.mNpcIcoStr = ((!string.IsNullOrEmpty(npc.enityInfoCmpt.faceIconBig)) ? npc.enityInfoCmpt.faceIconBig : "npc_big_Unknown");
			Instance.AddGetableMission(getableMisView2, isIgnoreHadCreate: true);
		}
		if (!m_MissonMap.ContainsKey(npcMissionData.m_RandomMission))
		{
			return;
		}
		Dictionary<string, string> missionFlagType2 = MissionManager.Instance.m_PlayerMission.GetMissionFlagType(npcMissionData.m_RandomMission);
		if (missionFlagType2 != null)
		{
			MissionView missionView2 = new MissionView();
			missionView2.mMissionID = missionCommonData.m_ID;
			missionView2.mMissionType = missionCommonData.m_Type;
			missionView2.mMissionTitle = missionCommonData.m_MissionName;
			npc = PeSingleton<EntityMgr>.Instance.Get(missionCommonData.m_iNpc);
			if (npc != null)
			{
				missionView2.mMissionStartNpc.mName = npc.enityInfoCmpt.characterName.fullName;
				missionView2.mMissionStartNpc.mNpcIcoStr = ((!string.IsNullOrEmpty(npc.enityInfoCmpt.faceIconBig)) ? npc.enityInfoCmpt.faceIconBig : "npc_big_Unknown");
			}
			npc = PeSingleton<EntityMgr>.Instance.Get(missionCommonData.m_iReplyNpc);
			if (npc != null)
			{
				missionView2.mMissionEndNpc.mName = npc.enityInfoCmpt.characterName.fullName;
				missionView2.mMissionEndNpc.mNpcIcoStr = ((!string.IsNullOrEmpty(npc.enityInfoCmpt.faceIconBig)) ? npc.enityInfoCmpt.faceIconBig : "npc_big_Unknown");
				missionView2.mEndMisPos = npc.position;
				missionView2.mAttachOnId = npc.Id;
				missionView2.NeedArrow = true;
			}
			MissionManager.Instance.ParseMissionFlag(missionCommonData, missionFlagType2, missionView2);
			Instance.AddMission(missionView2, isIgnoreHadCreate: true);
			Instance.RefalshMissionWnd();
		}
	}

	public void CheckMissionTag(MissionView misView)
	{
		if (this.e_CheckTagMission != null)
		{
			this.e_CheckTagMission(misView);
		}
	}

	public bool AddGetableMission(GetableMisView unMisView, bool isIgnoreHadCreate = false)
	{
		if (!isIgnoreHadCreate)
		{
			if (m_GetableMissonMap.ContainsKey(unMisView.mMissionID))
			{
				return false;
			}
			m_GetableMissonMap[unMisView.mMissionID] = unMisView;
		}
		if (this.e_AddGetableMission != null)
		{
			this.e_AddGetableMission(unMisView);
		}
		return true;
	}

	public bool DeleteGetableMission(GetableMisView unMisView)
	{
		return DeleteGetableMission(unMisView.mMissionID);
	}

	public bool DeleteGetableMission(int missionID)
	{
		if (!m_GetableMissonMap.ContainsKey(missionID))
		{
			return false;
		}
		if (this.e_DelGetableMission != null)
		{
			this.e_DelGetableMission(m_GetableMissonMap[missionID]);
		}
		return m_GetableMissonMap.Remove(missionID);
	}
}

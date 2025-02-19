using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Pathea.PeEntityExt;
using PeEvent;
using PETools;
using SkillSystem;
using UnityEngine;

namespace Pathea;

public class ServantLeaderCmpt : PeCmpt
{
	public class ServantChanged : EventArg
	{
		public bool isAdd;

		public PeEntity servant;

		public ServantChanged(bool add, PeEntity entity)
		{
			isAdd = add;
			servant = entity;
		}
	}

	private const int Version_0000 = 0;

	private const int Version_Current = 0;

	private const int MaxFollower = 2;

	private static ServantLeaderCmpt mInstance;

	private Action_Sleep mSleepAction;

	private Event<ServantChanged> mEventor = new Event<ServantChanged>();

	public NpcCmpt[] mFollowers;

	public List<NpcCmpt> mForcedFollowers = new List<NpcCmpt>();

	private PeEntity mpeEntity;

	private List<NpcCmpt> initReq;

	public static ServantLeaderCmpt Instance
	{
		get
		{
			if (mInstance == null && PeSingleton<PeCreature>.Instance.mainPlayer != null)
			{
				mInstance = PeSingleton<PeCreature>.Instance.mainPlayer.GetCmpt<ServantLeaderCmpt>();
			}
			return mInstance;
		}
	}

	public Event<ServantChanged> changeEventor => mEventor;

	public static int mMaxFollower => 2;

	public int RQFollowersIndex
	{
		get
		{
			if (MissionManager.Instance != null && MissionManager.Instance.m_PlayerMission != null && MissionManager.Instance.m_PlayerMission.followers != null)
			{
				return MissionManager.Instance.m_PlayerMission.followers.Count;
			}
			return 0;
		}
	}

	public PeEntity peEntity => mpeEntity;

	public event Action<int> FreeNpcEventHandler;

	private bool ValidateIndex(int index)
	{
		if (index < 0 || index >= mFollowers.Length)
		{
			return false;
		}
		return true;
	}

	public NpcCmpt GetServant(int index)
	{
		if (!ValidateIndex(index))
		{
			Debug.LogError("error follower index:" + index);
			return null;
		}
		return mFollowers[index];
	}

	public NpcCmpt[] GetServants()
	{
		return mFollowers;
	}

	public void SevantLostController(PeTrans leader)
	{
		for (int i = 0; i < mFollowers.Length; i++)
		{
			Vector3 equalPositionToStand = PEUtil.GetEqualPositionToStand(PEUtil.MainCamTransform.position, -PEUtil.MainCamTransform.forward, leader.position, -leader.forward, 3f);
			if (mFollowers[i] != null)
			{
				mFollowers[i].Req_Translate(equalPositionToStand);
			}
		}
	}

	public override void Awake()
	{
		base.Awake();
		mInstance = this;
		mFollowers = new NpcCmpt[2];
		mpeEntity = GetComponent<PeEntity>();
		this.FreeNpcEventHandler = (Action<int>)Delegate.Combine(this.FreeNpcEventHandler, new Action<int>(PlayerNetwork.RequestDismissNpc));
	}

	public override void Start()
	{
		base.Start();
		Invoke("InitFollower", 0.5f);
		if (PeSingleton<PeCreature>.Instance.mainPlayer != null)
		{
			mInstance = PeSingleton<PeCreature>.Instance.mainPlayer.GetCmpt<ServantLeaderCmpt>();
		}
		if (Singleton<PeEventGlobal>.Instance != null && Singleton<PeEventGlobal>.Instance.DestroyEvent != null)
		{
			Singleton<PeEventGlobal>.Instance.DestroyEvent.AddListener(OnFollowerEntityDestroy);
		}
	}

	private void InitFollower()
	{
		if (mFollowers == null)
		{
			mFollowers = new NpcCmpt[2];
		}
		if (initReq != null)
		{
			for (int i = 0; i < initReq.Count; i++)
			{
				InitSleepInfo(initReq[i]);
			}
			initReq.Clear();
			initReq = null;
		}
	}

	public void InitSleepInfo(NpcCmpt npc)
	{
		if (mSleepAction == null)
		{
			mSleepAction = base.Entity.motionMgr.GetAction<Action_Sleep>();
		}
		if (mSleepAction != null)
		{
			mSleepAction.startSleepEvt += npc.OnLeaderSleep;
			mSleepAction.endSleepEvt += npc.OnLeaderEndSleep;
		}
	}

	public override void OnDestroy()
	{
		base.OnDestroy();
		this.FreeNpcEventHandler = (Action<int>)Delegate.Remove(this.FreeNpcEventHandler, new Action<int>(PlayerNetwork.RequestDismissNpc));
	}

	private void OnFollowerEntityDestroy(SkEntity entity)
	{
		if (!(entity == null) && !entity.Equals(null))
		{
			PeEntity component = entity.GetComponent<PeEntity>();
			if (!(component == null) && !component.Equals(null) && !(component.NpcCmpt == null) && !component.NpcCmpt.Equals(null) && ContainsServant(component.NpcCmpt))
			{
				RemoveServant(component.NpcCmpt);
			}
		}
	}

	public void OnFreeNpc(int id)
	{
		if (this.FreeNpcEventHandler != null)
		{
			this.FreeNpcEventHandler(id);
		}
	}

	public bool ContainsServant(NpcCmpt follower)
	{
		return new List<NpcCmpt>(mFollowers).Contains(follower);
	}

	public void AddForcedServant(NpcCmpt forcedServant, bool isMove = false)
	{
		PeEntity component = forcedServant.GetComponent<PeEntity>();
		if (!mForcedFollowers.Contains(forcedServant) && !(component == null))
		{
			mForcedFollowers.Add(forcedServant);
			if (isMove)
			{
				StroyManager.Instance.Translate(component, EntityCreateMgr.Instance.GetPlayerTrans().position);
			}
			StroyManager.Instance.FollowTarget(component, PeSingleton<PeCreature>.Instance.mainPlayer.Id, Vector3.zero, 0, 0f);
		}
	}

	public void RemoveForcedServant(NpcCmpt forcedServant)
	{
		if (mForcedFollowers.Contains(forcedServant))
		{
			mForcedFollowers.Remove(forcedServant);
			NpcMissionData npcMissionData = forcedServant.Follwerentity.GetUserData() as NpcMissionData;
			npcMissionData.mInFollowMission = false;
			if (!(forcedServant.GetComponent<PeEntity>() == null) && !MissionManager.Instance.m_PlayerMission.followers.Contains(forcedServant))
			{
				StroyManager.Instance.RemoveReq(forcedServant.GetComponent<PeEntity>(), EReqType.FollowTarget);
			}
		}
	}

	public bool AddServant(NpcCmpt follower)
	{
		if (ContainsServant(follower))
		{
			return false;
		}
		for (int i = 0; i < mFollowers.Length; i++)
		{
			if (null == mFollowers[i])
			{
				mFollowers[i] = follower;
				if (UINPCfootManMgr.Instance != null)
				{
					UINPCfootManMgr.Instance.GetFollowerAlive();
				}
				follower.SetServantLeader(this);
				follower.NpcControlCmdId = 19;
				PeEntityCreator.RecruitRandomNpc(follower.Entity);
				InitSleepInfo(mFollowers[i]);
				if (UIMissionMgr.Instance != null)
				{
					UIMissionMgr.Instance.DeleteMission(follower.Entity);
				}
				changeEventor.Dispatch(new ServantChanged(add: true, follower.Entity));
				InGameAidData.CheckGetServant(follower.Entity.Id);
				SteamAchievementsSystem.Instance.OnGameStateChange(Eachievement.Eleven);
				return true;
			}
		}
		return false;
	}

	public bool AddServant(NpcCmpt follower, int index)
	{
		if (ContainsServant(follower))
		{
			return false;
		}
		bool flag = true;
		for (int i = 0; i < mFollowers.Length; i++)
		{
			if (mFollowers[i] == null)
			{
				flag = false;
				break;
			}
		}
		if (flag)
		{
			return false;
		}
		for (int j = 0; j < mFollowers.Length; j++)
		{
			if (j != index)
			{
				continue;
			}
			if (mFollowers[j] != null)
			{
				List<NpcCmpt> list = mFollowers.Where((NpcCmpt a) => a != null).ToList();
				list.Insert(j, follower);
				if (list.Count < 2)
				{
					for (int k = 0; k < 2 - list.Count; k++)
					{
						list.Add(null);
					}
				}
				mFollowers = list.ToArray();
			}
			else
			{
				mFollowers[j] = follower;
			}
			if (UINPCfootManMgr.Instance != null)
			{
				UINPCfootManMgr.Instance.GetFollowerAlive();
			}
			mFollowers = mFollowers.OrderBy((NpcCmpt a) => a == null).ToArray();
			follower.SetServantLeader(this);
			follower.NpcControlCmdId = 19;
			PeEntityCreator.RecruitRandomNpc(follower.Entity);
			InitSleepInfo(follower);
			changeEventor.Dispatch(new ServantChanged(add: false, follower.Entity));
			return true;
		}
		return false;
	}

	public bool RemoveServant(NpcCmpt follower)
	{
		for (int i = 0; i < mFollowers.Length; i++)
		{
			if (follower == mFollowers[i])
			{
				follower.NpcControlCmdId = 1;
				follower.SetServantLeader(null);
				PeEntity component = follower.GetComponent<PeEntity>();
				if (UIMissionMgr.Instance != null)
				{
					UIMissionMgr.Instance.AddMission(component);
				}
				MissionManager.Instance.m_PlayerMission.UpdateNpcMissionTex(component);
				return RemoveServant(i);
			}
		}
		return false;
	}

	public bool RemoveServant(int index)
	{
		if (ValidateIndex(index))
		{
			PeEntity entity = mFollowers[index].Entity;
			mFollowers[index].NpcControlCmdId = 1;
			mSleepAction.startSleepEvt -= mFollowers[index].OnLeaderSleep;
			mSleepAction.endSleepEvt -= mFollowers[index].OnLeaderEndSleep;
			mFollowers[index].RemoveSleepBuff();
			mFollowers[index].SetServantLeader(null);
			if (mFollowers[index].Entity != null && mFollowers[index].Entity.proto == EEntityProto.RandomNpc)
			{
				PeEntityCreator.ExileRandomNpc(mFollowers[index].Entity);
			}
			mFollowers[index] = null;
			mFollowers = mFollowers.OrderBy((NpcCmpt a) => a == null).ToArray();
			changeEventor.Dispatch(new ServantChanged(add: false, entity));
			if (UINPCfootManMgr.Instance != null)
			{
				UINPCfootManMgr.Instance.GetFollowerAlive();
			}
			return true;
		}
		return false;
	}

	public int GetServantNum()
	{
		int num = 0;
		for (int i = 0; i < mFollowers.Length; i++)
		{
			if (mFollowers[i] != null)
			{
				num++;
			}
		}
		return num;
	}

	public override void Serialize(BinaryWriter w)
	{
		base.Serialize(w);
		w.Write(0);
		w.Write(mFollowers.Length);
		for (int i = 0; i < mFollowers.Length; i++)
		{
			if (mFollowers[i] == null)
			{
				w.Write(0);
			}
			else
			{
				w.Write(mFollowers[i].Entity.Id);
			}
		}
	}

	public override void Deserialize(BinaryReader r)
	{
		base.Deserialize(r);
		r.ReadInt32();
		int num = r.ReadInt32();
		initReq = new List<NpcCmpt>();
		for (int i = 0; i < num; i++)
		{
			int entityId = r.ReadInt32();
			PeEntity peEntity = PeSingleton<EntityMgr>.Instance.Get(entityId);
			if (peEntity != null)
			{
				NpcCmpt component = peEntity.GetComponent<NpcCmpt>();
				if (component != null)
				{
					AddServant(component);
					initReq.Add(component);
				}
			}
		}
	}

	public override string ToString()
	{
		NpcCmpt servant = GetServant(0);
		NpcCmpt servant2 = GetServant(1);
		return $"leader:{base.Entity.Id}, follower1:{((!(null == servant)) ? servant.Entity.Id : (-11))}, follower2:{((!(null == servant2)) ? servant2.Entity.Id : (-11))}";
	}
}

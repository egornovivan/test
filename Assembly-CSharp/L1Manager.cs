using System;
using System.Collections.Generic;
using Pathea;
using Pathea.PeEntityExtTrans;
using UnityEngine;
using WhiteCat.UnityExtension;

public class L1Manager : MonoBehaviour
{
	public static L1Manager instance;

	public Dictionary<int, bool> isTrigger;

	private Dictionary<int, Vector3> triggerPoint;

	[HideInInspector]
	public List<PeEntity> followers;

	private void Start()
	{
		instance = this;
		instance.isTrigger = new Dictionary<int, bool>();
		instance.triggerPoint = new Dictionary<int, Vector3>();
		FindPointsTriggers();
		followers = new List<PeEntity>();
	}

	private void FindPointsTriggers()
	{
		base.transform.TraverseHierarchy(delegate(Transform trans, int n)
		{
			string text = trans.gameObject.name;
			if (text.Length >= 6 && !(text.Substring(0, 5) != "Point"))
			{
				int result = -1;
				if (IsNumberic(text.Substring(5, 1), out result) && !triggerPoint.ContainsKey(result))
				{
					triggerPoint.Add(result, trans.position);
				}
			}
		});
		for (int i = 1; i < 7; i++)
		{
			instance.isTrigger.Add(i, value: false);
		}
	}

	private bool IsNumberic(string s, out int result)
	{
		result = -1;
		try
		{
			result = Convert.ToInt32(s);
			return true;
		}
		catch
		{
			return false;
		}
	}

	public void FindFollowers(List<PeEntity> tmp)
	{
		foreach (NpcCmpt follower in MissionManager.Instance.m_PlayerMission.followers)
		{
			if (follower != null)
			{
				tmp.Add(follower.Follwerentity);
			}
		}
		NpcCmpt[] mFollowers = ServantLeaderCmpt.Instance.mFollowers;
		foreach (NpcCmpt npcCmpt in mFollowers)
		{
			if (npcCmpt != null)
			{
				tmp.Add(npcCmpt.Follwerentity);
			}
		}
		foreach (NpcCmpt mForcedFollower in ServantLeaderCmpt.Instance.mForcedFollowers)
		{
			if (mForcedFollower != null)
			{
				tmp.Add(mForcedFollower.Follwerentity);
			}
		}
	}

	public void SetPosition(int triggerNum)
	{
		foreach (PeEntity follower in followers)
		{
			follower.ExtSetPos(triggerPoint[triggerNum]);
		}
	}
}

using System.Collections.Generic;
using Pathea;
using UnityEngine;

namespace Behave.Runtime;

[BehaveAction(typeof(BTNpcFollowerGather), "NpcFollowerGather")]
public class BTNpcFollowerGather : BTNormal
{
	private class Data
	{
		[Behave]
		public float GatherRadius;

		[Behave]
		public float Probability;

		public float mStartTime;

		public Action_Gather Gather;
	}

	private Data m_Data;

	private GlobalTreeInfo glassInfo;

	private List<GlobalTreeInfo> mGlobalTreeInfos;

	private bool reached;

	private void GetGlobalTreeInfoForGather()
	{
		if (null != LSubTerrainMgr.Instance)
		{
			mGlobalTreeInfos = LSubTerrainMgr.Picking(base.position, base.transform.forward, includeTrees: false, m_Data.GatherRadius, 360f);
		}
		else if (null != RSubTerrainMgr.Instance)
		{
			mGlobalTreeInfos = RSubTerrainMgr.Picking(base.position, base.transform.forward, includeTrees: false, m_Data.GatherRadius, 360f);
		}
	}

	private bool DetectionTreeIndex(GlobalTreeInfo _treeInfo, PeEntity _entity)
	{
		for (int i = 0; i < base.NpcMaster.mFollowers.Length; i++)
		{
			if (base.NpcMaster.mFollowers[i] != null && base.NpcMaster.mFollowers[i].GatherprotoTypeIdx == _treeInfo._treeInfo.m_protoTypeIdx)
			{
				return false;
			}
		}
		_entity.NpcCmpt.GatherprotoTypeIdx = _treeInfo._treeInfo.m_protoTypeIdx;
		return true;
	}

	private BehaveResult Init(Tree sender)
	{
		if (!GetData(sender, ref m_Data))
		{
			return BehaveResult.Failure;
		}
		if (!base.IsNpc || !base.IsNpcFollower || !Enemy.IsNullOrInvalid(base.attackEnemy))
		{
			return BehaveResult.Failure;
		}
		if (Random.value > m_Data.Probability)
		{
			return BehaveResult.Failure;
		}
		GetGlobalTreeInfoForGather();
		if (mGlobalTreeInfos == null || mGlobalTreeInfos.Count <= 0)
		{
			return BehaveResult.Failure;
		}
		glassInfo = mGlobalTreeInfos[Random.Range(0, mGlobalTreeInfos.Count)];
		if (glassInfo == null)
		{
			return BehaveResult.Failure;
		}
		if (!DetectionTreeIndex(glassInfo, base.entity))
		{
			return BehaveResult.Failure;
		}
		m_Data.Gather = SetGlobalGatherInfo(glassInfo);
		if (m_Data.Gather == null && !m_Data.Gather.CanDoAction())
		{
			return BehaveResult.Failure;
		}
		reached = false;
		base.entity.NpcCmpt.AddTalkInfo(ENpcTalkType.Follower_Gather, ENpcSpeakType.TopHead, canLoop: true);
		return BehaveResult.Running;
	}

	private BehaveResult Tick(Tree sender)
	{
		if (SkEntitySubTerrain.Instance.GetTreeHP(glassInfo.WorldPos) <= float.Epsilon || !Enemy.IsNullOrInvalid(base.attackEnemy))
		{
			EndAction(PEActionType.Gather);
			return BehaveResult.Failure;
		}
		if (reached)
		{
			DoAction(PEActionType.Gather);
		}
		if (IsReached(base.position, glassInfo.WorldPos) && !reached)
		{
			StopMove();
			reached = true;
			m_Data.mStartTime = Time.time;
		}
		else
		{
			if (Stucking())
			{
				if (base.entity.viewCmpt == null || !base.entity.viewCmpt.hasView)
				{
					return BehaveResult.Failure;
				}
			}
			else
			{
				reached = true;
			}
			MoveToPosition(glassInfo.WorldPos, SpeedState.Run);
		}
		return BehaveResult.Running;
	}

	private void Reset(Tree sender)
	{
		if (base.entity.NpcCmpt != null)
		{
			base.entity.NpcCmpt.GatherprotoTypeIdx = -99;
			base.entity.NpcCmpt.RmoveTalkInfo(ENpcTalkType.Follower_Gather);
			glassInfo = null;
		}
	}
}

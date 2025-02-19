using System;
using System.Collections.Generic;
using NaturalResAsset;
using UnityEngine;

namespace Pathea;

[Serializable]
public class Action_Gather : PEAction
{
	private const int SkillID = 20110003;

	private const float GatherMaxDis = 3f;

	private bool m_EndAnim;

	private GlobalTreeInfo mOpTreeInfo;

	private GlobalTreeInfo mFindTreeInfo;

	public override PEActionType ActionType => PEActionType.Gather;

	public GlobalTreeInfo treeInfo
	{
		get
		{
			return mOpTreeInfo;
		}
		set
		{
			mOpTreeInfo = (mFindTreeInfo = value);
		}
	}

	public bool UpdateOPTreeInfo()
	{
		mFindTreeInfo = null;
		if (null == base.trans)
		{
			return false;
		}
		if (Vector3.Distance(base.trans.position, base.trans.position) > 3f)
		{
			return false;
		}
		List<GlobalTreeInfo> list;
		if (null != LSubTerrainMgr.Instance)
		{
			list = LSubTerrainMgr.Picking(base.trans.position, Vector3.forward, includeTrees: false, 3f, 360f);
		}
		else
		{
			if (!(null != RSubTerrainMgr.Instance))
			{
				return false;
			}
			list = RSubTerrainMgr.Picking(base.trans.position, Vector3.forward, includeTrees: false, 3f, 360f);
		}
		for (int i = 0; i < list.Count; i++)
		{
			NaturalRes terrainResData = NaturalRes.GetTerrainResData(list[i]._treeInfo.m_protoTypeIdx + 1000);
			if (terrainResData == null || terrainResData.m_type != 10)
			{
				continue;
			}
			if (!PeCamera.cursorLocked)
			{
				if (null != LSubTerrainMgr.Instance)
				{
					Vector3 worldPos = list[i].WorldPos;
					Bounds bounds = default(Bounds);
					bounds.SetMinMax(worldPos + list[i]._treeInfo.m_heightScale * LSubTerrainMgr.Instance.GlobalPrototypeBounds[list[i]._treeInfo.m_protoTypeIdx].min, worldPos + list[i]._treeInfo.m_heightScale * LSubTerrainMgr.Instance.GlobalPrototypeBounds[list[i]._treeInfo.m_protoTypeIdx].max);
					if (!bounds.IntersectRay(PeCamera.mouseRay))
					{
						continue;
					}
				}
				else if (null != RSubTerrainMgr.Instance)
				{
					Vector3 pos = list[i]._treeInfo.m_pos;
					Bounds bounds2 = default(Bounds);
					bounds2.SetMinMax(pos + list[i]._treeInfo.m_heightScale * RSubTerrainMgr.Instance.GlobalPrototypeBounds[list[i]._treeInfo.m_protoTypeIdx].min, pos + list[i]._treeInfo.m_heightScale * RSubTerrainMgr.Instance.GlobalPrototypeBounds[list[i]._treeInfo.m_protoTypeIdx].max);
					if (!bounds2.IntersectRay(PeCamera.mouseRay))
					{
						continue;
					}
				}
			}
			mFindTreeInfo = list[i];
			break;
		}
		return null != mFindTreeInfo;
	}

	public override bool CanDoAction(PEActionParam para = null)
	{
		return null != base.trans && mFindTreeInfo != null && !base.motionMgr.isInAimState;
	}

	public override void PreDoAction()
	{
		base.PreDoAction();
		base.motionMgr.SetMaskState(PEActionMask.Gather, state: true);
		mOpTreeInfo = mFindTreeInfo;
	}

	public override void DoAction(PEActionParam para = null)
	{
		if (null != base.anim)
		{
			base.anim.ResetTrigger("ResetFullBody");
			base.anim.SetTrigger("Gather");
		}
		m_EndAnim = false;
	}

	public override bool Update()
	{
		if (null != base.anim)
		{
			if (m_EndAnim)
			{
				base.motionMgr.SetMaskState(PEActionMask.Gather, state: false);
				return true;
			}
			return false;
		}
		return true;
	}

	public override void EndImmediately()
	{
		base.motionMgr.SetMaskState(PEActionMask.Gather, state: false);
		if (null != base.anim)
		{
			base.anim.SetTrigger("ResetFullBody");
			base.anim.ResetTrigger("Gather");
		}
		mOpTreeInfo = (mFindTreeInfo = null);
	}

	private void Gather()
	{
		if (mOpTreeInfo != null && null != base.skillCmpt)
		{
			if (SkEntitySubTerrain.Instance.GetTreeHP(mOpTreeInfo.WorldPos) <= float.Epsilon)
			{
				base.motionMgr.EndAction(ActionType);
			}
			else
			{
				base.skillCmpt.StartSkill(SkEntitySubTerrain.Instance, 20110003);
			}
		}
	}

	protected override void OnAnimEvent(string eventParam)
	{
		if (base.motionMgr.IsActionRunning(ActionType))
		{
			switch (eventParam)
			{
			case "Gather":
				Gather();
				break;
			case "GatherEnd":
				m_EndAnim = true;
				break;
			}
		}
	}
}

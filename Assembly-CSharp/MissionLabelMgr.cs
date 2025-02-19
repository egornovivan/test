using System.Collections.Generic;
using Pathea;
using PeMap;
using UnityEngine;

public class MissionLabelMgr
{
	public MissionLabelMgr()
	{
		UIMissionMgr.Instance.e_AddMission += OnAddMission;
		UIMissionMgr.Instance.e_DeleteMission += OnDeleteMission;
		UIMissionMgr.Instance.e_AddGetableMission += OnAddGetableMission;
		UIMissionMgr.Instance.e_DelGetableMission += OnDelGetableMission;
	}

	private void OnAddMission(UIMissionMgr.MissionView misView)
	{
		if (misView == null)
		{
			return;
		}
		if (misView.mEndMisPos != Vector3.zero)
		{
			MissionLabel item = new MissionLabel(misView.mMissionID, MissionLabelType.misLb_end, misView.mEndMisPos, misView.mMissionTitle, -1f, misView.NeedArrow, misView.mAttachOnId);
			PeSingleton<LabelMgr>.Instance.Add(item);
		}
		foreach (UIMissionMgr.TargetShow mTarget in misView.mTargetList)
		{
			if (!mTarget.mComplete && mTarget.mPosition != Vector3.zero)
			{
				MissionLabel item2 = new MissionLabel(misView.mMissionID, MissionLabelType.misLb_target, mTarget.mPosition, mTarget.mContent, mTarget.Radius, needArrow: false, mTarget.mAttachOnID, mTarget);
				PeSingleton<LabelMgr>.Instance.Add(item2);
			}
		}
	}

	private void OnDeleteMission(UIMissionMgr.MissionView misView)
	{
		if (misView == null)
		{
			return;
		}
		List<ILabel> list = PeSingleton<LabelMgr>.Instance.FindAll((ILabel itr) => RemoveMissionLabel(misView.mMissionID, itr));
		foreach (ILabel item in list)
		{
			PeSingleton<LabelMgr>.Instance.Remove(item);
		}
		list.Clear();
	}

	private bool RemoveMissionLabel(int missionID, ILabel label)
	{
		if (label.GetType() != ELabelType.Mission)
		{
			return false;
		}
		if (!(label is MissionLabel))
		{
			return false;
		}
		MissionLabel missionLabel = label as MissionLabel;
		if (missionLabel.m_type == MissionLabelType.misLb_unActive)
		{
			return false;
		}
		return missionLabel.m_missionID == missionID;
	}

	private void OnAddGetableMission(UIMissionMgr.GetableMisView getableView)
	{
		if (getableView != null && getableView.mPosition != Vector3.zero)
		{
			MissionLabel item = new MissionLabel(getableView.mMissionID, MissionLabelType.misLb_unActive, getableView.mPosition, getableView.mMissionTitle, -1f, needArrow: false, getableView.mAttachOnId);
			PeSingleton<LabelMgr>.Instance.Add(item);
		}
	}

	private void OnDelGetableMission(UIMissionMgr.GetableMisView getableView)
	{
		if (getableView == null)
		{
			return;
		}
		List<ILabel> list = PeSingleton<LabelMgr>.Instance.FindAll((ILabel itr) => RemoveGetableLabel(getableView.mMissionID, itr));
		foreach (ILabel item in list)
		{
			PeSingleton<LabelMgr>.Instance.Remove(item);
		}
		list.Clear();
	}

	private bool RemoveGetableLabel(int missionID, ILabel label)
	{
		if (label.GetType() != ELabelType.Mission)
		{
			return false;
		}
		if (!(label is MissionLabel))
		{
			return false;
		}
		MissionLabel missionLabel = label as MissionLabel;
		if (missionLabel.m_type != MissionLabelType.misLb_unActive)
		{
			return false;
		}
		return missionLabel.m_missionID == missionID;
	}
}

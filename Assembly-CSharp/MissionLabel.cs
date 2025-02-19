using PeMap;
using UnityEngine;

public class MissionLabel : ILabel
{
	public int m_missionID;

	public MissionLabelType m_type;

	public UIMissionMgr.TargetShow m_target;

	public bool IsComplete;

	public int m_attachOnID;

	public bool NeedOneRefreshPos;

	private string m_text = string.Empty;

	private Vector3 m_postion;

	private float m_raidus;

	private bool m_needArrow;

	public MissionLabel(int missionID, MissionLabelType type, Vector3 pos, string text, float raidus, bool needArrow, int attachOnId, UIMissionMgr.TargetShow target = null)
	{
		m_type = type;
		m_postion = pos;
		m_text = text;
		m_raidus = raidus;
		m_missionID = missionID;
		m_needArrow = needArrow;
		m_attachOnID = attachOnId;
		m_target = target;
		UIMissionMgr.MissionView missionView = UIMissionMgr.Instance.GetMissionView(m_missionID);
		if (missionView != null)
		{
			IsComplete = missionView.mComplete;
		}
	}

	int ILabel.GetIcon()
	{
		return m_type switch
		{
			MissionLabelType.misLb_unActive => 25, 
			MissionLabelType.misLb_target => 13, 
			MissionLabelType.misLb_end => 26, 
			_ => 0, 
		};
	}

	Vector3 ILabel.GetPos()
	{
		return m_postion;
	}

	string ILabel.GetText()
	{
		return (!(m_text == "0")) ? m_text : string.Empty;
	}

	bool ILabel.FastTravel()
	{
		return false;
	}

	ELabelType ILabel.GetType()
	{
		return ELabelType.Mission;
	}

	bool ILabel.NeedArrow()
	{
		return m_needArrow;
	}

	float ILabel.GetRadius()
	{
		return m_raidus;
	}

	EShow ILabel.GetShow()
	{
		switch (m_type)
		{
		case MissionLabelType.misLb_unActive:
			if (MissionManager.IsMainMission(m_missionID))
			{
				return EShow.All;
			}
			return EShow.MinMap;
		case MissionLabelType.misLb_target:
			return EShow.All;
		case MissionLabelType.misLb_end:
			return EShow.All;
		default:
			return EShow.Max;
		}
	}

	public void SetLabelPos(Vector3 v, bool needOneRefreshPos = false)
	{
		m_postion = v;
		NeedOneRefreshPos = needOneRefreshPos;
	}

	public Color32 GetMissionColor()
	{
		Color32 result = new Color32(byte.MaxValue, byte.MaxValue, byte.MaxValue, byte.MaxValue);
		switch (m_type)
		{
		case MissionLabelType.misLb_target:
			return MissionMapLabelColor.MissionTargetCol;
		case MissionLabelType.misLb_unActive:
		case MissionLabelType.misLb_end:
			if (m_type == MissionLabelType.misLb_end && !IsComplete)
			{
				return MissionMapLabelColor.UnFinishedCol;
			}
			return (!MissionManager.IsMainMission(m_missionID)) ? MissionMapLabelColor.SideLineCol : MissionMapLabelColor.MainLineCol;
		default:
			return result;
		}
	}

	public bool CompareTo(ILabel label)
	{
		if (label is MissionLabel)
		{
			MissionLabel missionLabel = (MissionLabel)label;
			if (m_missionID == missionLabel.m_missionID && m_type == missionLabel.m_type && missionLabel.m_postion == m_postion && missionLabel.m_raidus == m_raidus)
			{
				return true;
			}
			return false;
		}
		return false;
	}
}

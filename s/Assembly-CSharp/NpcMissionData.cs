using System.Collections.Generic;
using System.IO;
using PETools;
using UnityEngine;

public class NpcMissionData
{
	public byte mCurComMisNum;

	public int mCompletedMissionCount;

	public int m_RandomMission;

	public List<int> m_MissionList = new List<int>();

	public List<int> m_MissionListReply = new List<int>();

	public List<int> m_RecruitMissionList = new List<int>();

	public List<int> m_CSRecruitMissionList = new List<int>();

	public int m_RecruitMissionNum;

	public int m_Rnpc_ID = -1;

	public Vector3 m_Pos;

	public int m_CurMissionGroup = 1;

	public int m_CurGroupTimes;

	public int m_QCID;

	public void AddMissionListReply(int id)
	{
		if (!m_MissionListReply.Contains(id))
		{
			m_MissionListReply.Add(id);
		}
	}

	public byte[] Serialize()
	{
		return PETools.Serialize.Export(delegate(BinaryWriter w)
		{
			BufferHelper.Serialize(w, mCurComMisNum);
			BufferHelper.Serialize(w, mCompletedMissionCount);
			BufferHelper.Serialize(w, m_RandomMission);
			BufferHelper.Serialize(w, m_RecruitMissionNum);
			BufferHelper.Serialize(w, m_Rnpc_ID);
			BufferHelper.Serialize(w, m_CurMissionGroup);
			BufferHelper.Serialize(w, m_CurGroupTimes);
			BufferHelper.Serialize(w, m_QCID);
			BufferHelper.Serialize(w, m_Pos);
			BufferHelper.Serialize(w, m_MissionList.Count);
			foreach (int mission in m_MissionList)
			{
				BufferHelper.Serialize(w, mission);
			}
			BufferHelper.Serialize(w, m_MissionListReply.Count);
			foreach (int item in m_MissionListReply)
			{
				BufferHelper.Serialize(w, item);
			}
			BufferHelper.Serialize(w, m_RecruitMissionList.Count);
			foreach (int recruitMission in m_RecruitMissionList)
			{
				BufferHelper.Serialize(w, recruitMission);
			}
		});
	}

	public void Deserialize(byte[] buffer)
	{
		PETools.Serialize.Import(buffer, delegate(BinaryReader r)
		{
			mCurComMisNum = BufferHelper.ReadByte(r);
			mCompletedMissionCount = BufferHelper.ReadInt32(r);
			m_RandomMission = BufferHelper.ReadInt32(r);
			m_RecruitMissionNum = BufferHelper.ReadInt32(r);
			m_Rnpc_ID = BufferHelper.ReadInt32(r);
			m_CurMissionGroup = BufferHelper.ReadInt32(r);
			m_CurGroupTimes = BufferHelper.ReadInt32(r);
			m_QCID = BufferHelper.ReadInt32(r);
			BufferHelper.ReadVector3(r, out m_Pos);
			int num = BufferHelper.ReadInt32(r);
			for (int i = 0; i < num; i++)
			{
				int item = BufferHelper.ReadInt32(r);
				m_MissionList.Add(item);
			}
			num = BufferHelper.ReadInt32(r);
			for (int j = 0; j < num; j++)
			{
				int item2 = BufferHelper.ReadInt32(r);
				m_MissionListReply.Add(item2);
			}
			num = BufferHelper.ReadInt32(r);
			for (int k = 0; k < num; k++)
			{
				int item3 = BufferHelper.ReadInt32(r);
				m_RecruitMissionList.Add(item3);
			}
		});
	}
}

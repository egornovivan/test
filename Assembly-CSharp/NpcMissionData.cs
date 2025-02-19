using System.Collections.Generic;
using System.IO;
using PETools;
using uLink;
using UnityEngine;

public class NpcMissionData
{
	public byte mCurComMisNum;

	public int mCompletedMissionCount;

	public bool m_bRandomNpc;

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

	public bool m_bColonyOrder = true;

	public bool mInFollowMission;

	public void AddMissionListReply(int id)
	{
		if (!m_MissionListReply.Contains(id))
		{
			m_MissionListReply.Add(id);
		}
	}

	public void RemoveMissionListReply(int id)
	{
		if (!m_MissionListReply.Contains(id))
		{
			m_MissionListReply.Remove(id);
		}
	}

	public byte[] Write(int npcid)
	{
		MemoryStream memoryStream = new MemoryStream();
		BinaryWriter binaryWriter = new BinaryWriter(memoryStream);
		binaryWriter.Write(npcid);
		binaryWriter.Write(m_Rnpc_ID);
		binaryWriter.Write(m_QCID);
		binaryWriter.Write(m_CurMissionGroup);
		binaryWriter.Write(m_CurGroupTimes);
		binaryWriter.Write(mCurComMisNum);
		binaryWriter.Write(mCompletedMissionCount);
		binaryWriter.Write(m_RandomMission);
		binaryWriter.Write(m_RecruitMissionNum);
		binaryWriter.Write(m_MissionList.Count);
		for (int i = 0; i < m_MissionList.Count; i++)
		{
			binaryWriter.Write(m_MissionList[i]);
		}
		binaryWriter.Write(m_MissionListReply.Count);
		for (int j = 0; j < m_MissionListReply.Count; j++)
		{
			binaryWriter.Write(m_MissionListReply[j]);
		}
		byte[] result = memoryStream.ToArray();
		binaryWriter.Close();
		memoryStream.Close();
		return result;
	}

	public int Read(byte[] buffer)
	{
		if (buffer.Length == 0)
		{
			return -1;
		}
		MemoryStream memoryStream = new MemoryStream(buffer);
		BinaryReader binaryReader = new BinaryReader(memoryStream);
		int num = binaryReader.ReadInt32();
		if (num == -1)
		{
			return num;
		}
		m_Rnpc_ID = binaryReader.ReadInt32();
		m_QCID = binaryReader.ReadInt32();
		m_CurMissionGroup = binaryReader.ReadInt32();
		m_CurGroupTimes = binaryReader.ReadInt32();
		mCurComMisNum = binaryReader.ReadByte();
		mCompletedMissionCount = binaryReader.ReadInt32();
		m_RandomMission = binaryReader.ReadInt32();
		m_RecruitMissionNum = binaryReader.ReadInt32();
		int num2 = binaryReader.ReadInt32();
		for (int i = 0; i < num2; i++)
		{
			m_MissionList.Add(binaryReader.ReadInt32());
		}
		num2 = binaryReader.ReadInt32();
		for (int j = 0; j < num2; j++)
		{
			m_MissionListReply.Add(binaryReader.ReadInt32());
		}
		binaryReader.Close();
		memoryStream.Close();
		return num;
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
			m_bRandomNpc = true;
		});
	}

	public static object Deserialize(uLink.BitStream stream, params object[] codecOptions)
	{
		NpcMissionData npcMissionData = new NpcMissionData();
		npcMissionData.mCurComMisNum = stream.Read<byte>(new object[0]);
		npcMissionData.mCompletedMissionCount = stream.Read<int>(new object[0]);
		npcMissionData.m_RandomMission = stream.Read<int>(new object[0]);
		npcMissionData.m_RecruitMissionNum = stream.Read<int>(new object[0]);
		npcMissionData.m_Rnpc_ID = stream.Read<int>(new object[0]);
		npcMissionData.m_CurMissionGroup = stream.Read<int>(new object[0]);
		npcMissionData.m_CurGroupTimes = stream.Read<int>(new object[0]);
		npcMissionData.m_QCID = stream.Read<int>(new object[0]);
		npcMissionData.m_Pos = stream.Read<Vector3>(new object[0]);
		int[] collection = stream.Read<int[]>(new object[0]);
		npcMissionData.m_MissionList.Clear();
		npcMissionData.m_MissionList.AddRange(collection);
		int[] collection2 = stream.Read<int[]>(new object[0]);
		npcMissionData.m_MissionListReply.Clear();
		npcMissionData.m_MissionListReply.AddRange(collection2);
		int[] collection3 = stream.Read<int[]>(new object[0]);
		npcMissionData.m_RecruitMissionList.Clear();
		npcMissionData.m_RecruitMissionList.AddRange(collection3);
		return npcMissionData;
	}

	public static void Serialize(uLink.BitStream stream, object value, params object[] codecOptions)
	{
		NpcMissionData npcMissionData = (NpcMissionData)value;
		stream.Write(npcMissionData.mCurComMisNum);
		stream.Write(npcMissionData.mCompletedMissionCount);
		stream.Write(npcMissionData.m_RandomMission);
		stream.Write(npcMissionData.m_RecruitMissionNum);
		stream.Write(npcMissionData.m_Rnpc_ID);
		stream.Write(npcMissionData.m_CurMissionGroup);
		stream.Write(npcMissionData.m_CurGroupTimes);
		stream.Write(npcMissionData.m_QCID);
		stream.Write(npcMissionData.m_Pos);
		stream.Write(npcMissionData.m_MissionList.ToArray());
		stream.Write(npcMissionData.m_MissionListReply.ToArray());
		stream.Write(npcMissionData.m_RecruitMissionList.ToArray());
	}
}

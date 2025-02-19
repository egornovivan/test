using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Pathea;

public class ColonyTrain : ColonyBase
{
	public const int MAX_INSTRUCTOR_NUM = 8;

	public const int MAX_TRAINEE_NUM = 8;

	public const int MAX_WORKER_COUNT = 24;

	public const int MAX_SKILL_COUNT = 5;

	private CSTrainData _MyData;

	public override int MaxWorkerCount => 24;

	public bool IsTraining => m_Time >= 0f;

	public CSTrainData Data => _MyData;

	public List<int> InstructorList => Data.instructors;

	public List<int> TraineeList => Data.trainees;

	public ETrainingType trainingType
	{
		get
		{
			return (ETrainingType)Data.trainingType;
		}
		set
		{
			Data.trainingType = (int)value;
		}
	}

	public int InstructorNpcId
	{
		get
		{
			return Data.instructorNpcId;
		}
		set
		{
			Data.instructorNpcId = value;
		}
	}

	public int TraineeNpcId
	{
		get
		{
			return Data.traineeNpcId;
		}
		set
		{
			Data.traineeNpcId = value;
		}
	}

	public List<int> LearningSkills => Data.LearningSkillIds;

	public float m_CurTime
	{
		get
		{
			return Data.m_CurTime;
		}
		set
		{
			Data.m_CurTime = value;
		}
	}

	public float m_Time
	{
		get
		{
			return Data.m_Time;
		}
		set
		{
			Data.m_Time = value;
		}
	}

	public ColonyTrain(ColonyNetwork network)
	{
		SetNetwork(network);
		_RecordData = new CSTrainData();
		_MyData = (CSTrainData)_RecordData;
		LoadData();
	}

	public override bool IsWorking()
	{
		if (ColonyMgr._Instance.HasCoreAndPower(_Network.TeamId, _Network.transform.position))
		{
			return true;
		}
		return false;
	}

	public override void InitMyData()
	{
	}

	public override void InitNpc()
	{
		List<ColonyNpc> teamNpcs = ColonyNpcMgr.GetTeamNpcs(base.TeamId);
		if (teamNpcs == null || teamNpcs.Count <= 0)
		{
			return;
		}
		foreach (ColonyNpc item in teamNpcs)
		{
			if (item.m_Occupation != 7 || !AddWorker(item))
			{
				continue;
			}
			item.m_WorkRoomID = base.Id;
			if (item.trainerType != 0)
			{
				switch (item.trainerType)
				{
				case ETrainerType.Instructor:
					AddInstructor(item);
					break;
				case ETrainerType.Trainee:
					AddTrainee(item);
					break;
				}
			}
			item.Save();
		}
		SyncSave();
	}

	public void InitNpcData(ColonyNpc cn)
	{
		switch (cn.trainerType)
		{
		case ETrainerType.Instructor:
			AddInstructor(cn);
			break;
		case ETrainerType.Trainee:
			AddTrainee(cn);
			break;
		}
	}

	public bool AddInstructor(ColonyNpc p)
	{
		if (InstructorList.Contains(p._npcID))
		{
			return true;
		}
		if (InstructorList.Count >= 8)
		{
			return false;
		}
		InstructorList.Add(p._npcID);
		if (TraineeNpcId == p._npcID)
		{
			TraineeNpcId = -1;
			if (m_Time >= 0f)
			{
				StopTraining();
			}
		}
		TraineeList.Remove(p._npcID);
		p.trainerType = ETrainerType.Instructor;
		p.Save();
		SyncSave();
		return true;
	}

	public bool AddTrainee(ColonyNpc p)
	{
		if (TraineeList.Contains(p._npcID))
		{
			return true;
		}
		if (TraineeList.Count >= 8)
		{
			return false;
		}
		TraineeList.Add(p._npcID);
		if (InstructorNpcId == p._npcID)
		{
			InstructorNpcId = -1;
			if (m_Time >= 0f)
			{
				StopTraining();
			}
		}
		InstructorList.Remove(p._npcID);
		p.trainerType = ETrainerType.Trainee;
		p.Save();
		SyncSave();
		return true;
	}

	private float CountSkillFinalTime(List<int> skillIds)
	{
		float num = 0f;
		foreach (int skillId in skillIds)
		{
			num += NpcAbility.GetLearnTime(skillId);
		}
		return num;
	}

	private float CountAttributeFinalTime(ColonyNpc instructorNpc, ColonyNpc traineeNpc)
	{
		return CSTrainingInfo.m_BaseTime;
	}

	public void StartSkillCounter(List<int> skillIds)
	{
		float finalTime = CountSkillFinalTime(skillIds);
		Data.LearningSkillIds = skillIds;
		StartCounter(0f, finalTime);
	}

	public void StartAttributeCounter(ColonyNpc instructorNpc, ColonyNpc traineeNpc)
	{
		float finalTime = CountAttributeFinalTime(instructorNpc, traineeNpc);
		StartCounter(0f, finalTime);
	}

	public void StopTraining()
	{
		StopCounter();
		SetNpcIsTraining(flag: false);
	}

	public void StartCounterFromRecord()
	{
	}

	public void StartCounter(float curTime, float finalTime)
	{
		m_CurTime = curTime;
		m_Time = finalTime;
	}

	public void StopCounter()
	{
		Data.m_CurTime = -1f;
		Data.m_Time = -1f;
	}

	public override void MyUpdate()
	{
		if (IsWorking() && m_Time >= 0f && CheckNpcPosition())
		{
			if (m_CurTime < m_Time)
			{
				m_CurTime += 1f;
				UpdateTimeTick(m_CurTime);
			}
			if (m_CurTime >= m_Time)
			{
				TrainFinish();
			}
		}
	}

	protected override void UpdateTimeTick(float curTime)
	{
		if ((int)curTime % 5 == 0)
		{
			_Network.RPCOthers(EPacketType.PT_CL_TRN_SyncCounter, curTime, m_Time);
		}
	}

	public void TrainFinish()
	{
		m_Time = -1f;
		m_CurTime = -1f;
		if (trainingType == ETrainingType.Skill)
		{
			ColonyNpc npcByID = ColonyNpcMgr.GetNpcByID(TraineeNpcId);
			List<int> list = new List<int>(npcByID.GetNpcAllSkill);
			List<int> coverSkill = new List<int>();
			foreach (int learningSkill in LearningSkills)
			{
				List<int> list2 = NpcAbility.FindCoverAbilityId(learningSkill).ToList();
				foreach (int item in list2)
				{
					if (list.Contains(item))
					{
						coverSkill.Add(item);
					}
				}
			}
			list.RemoveAll((int it) => coverSkill.Contains(it));
			int num = list.Count + LearningSkills.Count - 5;
			if (num > 0)
			{
				Random random = new Random();
				for (int i = 0; i < num; i++)
				{
					list.RemoveAt(random.Next(list.Count));
				}
			}
			list.AddRange(LearningSkills);
			npcByID.GetNpcAllSkill = list;
			_Network.RPCOthers(EPacketType.PT_CL_TRN_SkillTrainFinish, list.ToArray(), TraineeNpcId);
			SetNpcIsTraining(flag: false);
		}
		else
		{
			ColonyNpc npcByID2 = ColonyNpcMgr.GetNpcByID(InstructorNpcId);
			ColonyNpc npcByID3 = ColonyNpcMgr.GetNpcByID(TraineeNpcId);
			if (!npcByID3.CanAttributeUp())
			{
				return;
			}
			AttribType type = ((!npcByID2.IsRandomNpc) ? AttPlusNPCData.GetProtoMaxAttribute(npcByID2.ProtoId, npcByID2._skEntity) : AttPlusNPCData.GetRandMaxAttribute(npcByID2.ProtoId, npcByID2._skEntity));
			AttPlusNPCData.AttrPlus.RandomInt Rand = default(AttPlusNPCData.AttrPlus.RandomInt);
			AttPlusNPCData.GetRandom(npcByID2.ProtoId, type, out Rand);
			float value = new Random().Next(Rand.m_Min, Rand.m_Max + 1);
			npcByID3.AttributeUpgrade(type, value);
			_Network.RPCOthers(EPacketType.PT_CL_TRN_AttributeTrainFinish, InstructorNpcId, TraineeNpcId, npcByID3.UpgradeTimes);
			SetNpcIsTraining(flag: false);
		}
		SyncSave();
	}

	public override bool AddWorker(ColonyNpc npc)
	{
		if (base.AddWorker(npc))
		{
			if (npc.trainerType != 0)
			{
				switch (npc.trainerType)
				{
				case ETrainerType.Instructor:
					AddInstructor(npc);
					break;
				case ETrainerType.Trainee:
					AddTrainee(npc);
					break;
				}
			}
			return true;
		}
		return false;
	}

	public override bool RemoveWorker(ColonyNpc npc)
	{
		if (base.RemoveWorker(npc))
		{
			InstructorList.Remove(npc._npcID);
			TraineeList.Remove(npc._npcID);
			if (InstructorNpcId == npc._npcID)
			{
				InstructorNpcId = -1;
				if (m_Time >= 0f)
				{
					StopCounter();
				}
			}
			if (TraineeNpcId == npc._npcID)
			{
				TraineeNpcId = -1;
				if (m_Time >= 0f)
				{
					StopCounter();
				}
			}
			SyncSave();
			return true;
		}
		return false;
	}

	public override void DestroySelf()
	{
		base.DestroySelf();
	}

	public void SetNpcIsTraining(bool flag)
	{
		ColonyNpc npcByID = ColonyNpcMgr.GetNpcByID(InstructorNpcId);
		ColonyNpc npcByID2 = ColonyNpcMgr.GetNpcByID(TraineeNpcId);
		if (npcByID != null)
		{
			npcByID.IsTraining = flag;
			npcByID.trainingType = trainingType;
			if (!flag)
			{
				InstructorNpcId = -1;
			}
			npcByID.Save();
		}
		if (npcByID2 != null)
		{
			npcByID2.IsTraining = flag;
			npcByID2.trainingType = trainingType;
			if (!flag)
			{
				TraineeNpcId = -1;
			}
			npcByID2.Save();
		}
	}

	public bool CheckInstructorAndTraineeId(int instructorId, int traineeId)
	{
		ColonyNpc npcByID = ColonyNpcMgr.GetNpcByID(instructorId);
		ColonyNpc npcByID2 = ColonyNpcMgr.GetNpcByID(traineeId);
		if (npcByID == null || npcByID2 == null)
		{
			return false;
		}
		if (npcByID.m_Occupation != 7 || npcByID2.m_Occupation != 7)
		{
			return false;
		}
		if (!InstructorList.Contains(instructorId) || !TraineeList.Contains(traineeId))
		{
			return false;
		}
		return true;
	}

	public bool CheckTrainerId(int trainerId)
	{
		ColonyNpc npcByID = ColonyNpcMgr.GetNpcByID(trainerId);
		if (npcByID == null || npcByID.m_Occupation != 7)
		{
			return false;
		}
		return true;
	}

	public bool CheckInstructorId(int instructorId)
	{
		ColonyNpc npcByID = ColonyNpcMgr.GetNpcByID(instructorId);
		if (npcByID == null || npcByID.m_Occupation != 7)
		{
			return false;
		}
		if (!InstructorList.Contains(instructorId))
		{
			return false;
		}
		return true;
	}

	public bool CheckTraineeId(int traineeId)
	{
		ColonyNpc npcByID = ColonyNpcMgr.GetNpcByID(traineeId);
		if (npcByID == null || npcByID.m_Occupation != 7)
		{
			return false;
		}
		if (!TraineeList.Contains(traineeId))
		{
			return false;
		}
		return true;
	}

	public bool CheckNpcPosition()
	{
		if (InstructorNpcId <= 0 || TraineeNpcId <= 0)
		{
			return false;
		}
		ColonyNpc npcByID = ColonyNpcMgr.GetNpcByID(InstructorNpcId);
		if (npcByID == null)
		{
			return false;
		}
		if ((npcByID.Pos - base.Pos).magnitude > 4f)
		{
			return false;
		}
		ColonyNpc npcByID2 = ColonyNpcMgr.GetNpcByID(TraineeNpcId);
		if (npcByID2 == null)
		{
			return false;
		}
		if ((npcByID2.Pos - base.Pos).magnitude > 4f)
		{
			return false;
		}
		return true;
	}

	public override void CombomData(BinaryWriter writer)
	{
		int count = InstructorList.Count;
		BufferHelper.Serialize(writer, count);
		for (int i = 0; i < count; i++)
		{
			BufferHelper.Serialize(writer, InstructorList[i]);
		}
		int count2 = TraineeList.Count;
		BufferHelper.Serialize(writer, count2);
		for (int j = 0; j < count2; j++)
		{
			BufferHelper.Serialize(writer, TraineeList[j]);
		}
		BufferHelper.Serialize(writer, InstructorNpcId);
		BufferHelper.Serialize(writer, TraineeNpcId);
		BufferHelper.Serialize(writer, (int)trainingType);
		int count3 = LearningSkills.Count;
		BufferHelper.Serialize(writer, count3);
		for (int k = 0; k < count3; k++)
		{
			BufferHelper.Serialize(writer, LearningSkills[k]);
		}
		BufferHelper.Serialize(writer, m_CurTime);
		BufferHelper.Serialize(writer, m_Time);
	}

	public override void ParseData(byte[] data, int ver)
	{
		using MemoryStream input = new MemoryStream(data);
		using BinaryReader reader = new BinaryReader(input);
		int num = BufferHelper.ReadInt32(reader);
		Data.instructors = new List<int>();
		for (int i = 0; i < num; i++)
		{
			InstructorList.Add(BufferHelper.ReadInt32(reader));
		}
		int num2 = BufferHelper.ReadInt32(reader);
		Data.trainees = new List<int>();
		for (int j = 0; j < num2; j++)
		{
			TraineeList.Add(BufferHelper.ReadInt32(reader));
		}
		InstructorNpcId = BufferHelper.ReadInt32(reader);
		TraineeNpcId = BufferHelper.ReadInt32(reader);
		trainingType = (ETrainingType)BufferHelper.ReadInt32(reader);
		int num3 = BufferHelper.ReadInt32(reader);
		Data.LearningSkillIds = new List<int>();
		for (int k = 0; k < num3; k++)
		{
			LearningSkills.Add(BufferHelper.ReadInt32(reader));
		}
		m_CurTime = BufferHelper.ReadSingle(reader);
		m_Time = BufferHelper.ReadSingle(reader);
	}

	public byte[] AllDataToByte()
	{
		using MemoryStream memoryStream = new MemoryStream();
		using BinaryWriter writer = new BinaryWriter(memoryStream);
		CombomData(writer);
		return memoryStream.ToArray();
	}

	public override void DestroySomeData()
	{
		base.DestroySomeData();
	}
}

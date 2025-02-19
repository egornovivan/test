using System.Collections.Generic;

namespace CSRecord;

public class CSTrainData : CSObjectData
{
	public List<int> instructors = new List<int>();

	public List<int> trainees = new List<int>();

	public int instructorNpcId;

	public int traineeNpcId;

	public int trainingType;

	public List<int> LearningSkillIds = new List<int>();

	public float m_CurTime = -1f;

	public float m_Time = -1f;

	public CSTrainData()
	{
		dType = 11;
	}
}

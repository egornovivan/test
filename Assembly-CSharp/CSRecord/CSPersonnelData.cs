using UnityEngine;

namespace CSRecord;

public class CSPersonnelData : CSDefaultData
{
	public int m_State;

	public int m_DwellingsID;

	public int m_WorkRoomID;

	public int m_Occupation;

	public int m_WorkMode;

	public Vector3 m_GuardPos;

	public int m_ProcessingIndex = -1;

	public bool m_IsProcessing;

	public int m_TrainerType;

	public int m_TrainingType;

	public bool m_IsTraining;
}

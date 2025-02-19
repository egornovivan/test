using UnityEngine;

public class FarmWorkInfo
{
	public FarmPlantLogic m_Plant;

	public Vector3 m_Pos;

	public ClodChunk m_ClodChunk;

	public FarmWorkInfo(FarmPlantLogic plant)
	{
		if (plant == null)
		{
			Debug.LogError("Giving plant must be not null.");
			Debug.DebugBreak();
		}
		m_Plant = plant;
		m_Pos = plant.mPos;
	}

	public FarmWorkInfo(ClodChunk clodChunk, Vector3 pos)
	{
		m_ClodChunk = clodChunk;
		m_Pos = pos;
	}
}

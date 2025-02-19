using UnityEngine;

public class Min_Max_Int
{
	public int m_Min;

	public int m_Max;

	public int Random()
	{
		return UnityEngine.Random.Range(m_Min, m_Max);
	}
}

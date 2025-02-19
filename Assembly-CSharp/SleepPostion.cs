using UnityEngine;

public class SleepPostion
{
	public Vector3 _Doorpos;

	public Vector3 _Pos;

	public float _Rate;

	private int id;

	private bool m_Occpyied;

	public int _Id
	{
		get
		{
			return id;
		}
		set
		{
			id = value;
		}
	}

	public bool Occpyied
	{
		get
		{
			return m_Occpyied;
		}
		set
		{
			m_Occpyied = value;
		}
	}
}

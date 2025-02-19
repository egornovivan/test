using UnityEngine;

namespace Pathea;

public class EnemyMessage
{
	private EEnemyMsg m_Type;

	private float m_Time;

	private bool m_Dirty;

	private object m_Data;

	public EEnemyMsg Type => m_Type;

	public bool Dirty
	{
		get
		{
			if (Time.time - m_Time > 0.3f)
			{
				m_Dirty = false;
			}
			return m_Dirty;
		}
		set
		{
			m_Dirty = value;
		}
	}

	public object Data => m_Data;

	public EnemyMessage(EEnemyMsg msg, object data = null)
	{
		m_Type = msg;
		m_Data = data;
		m_Time = Time.time;
	}
}

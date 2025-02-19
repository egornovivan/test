public class TowerInfoUIData
{
	private int m_MissionID;

	private float m_PreTime;

	private float m_RemainTime = -1f;

	private int m_CurCount;

	private int m_MaxCount;

	private bool m_bRefurbish;

	private int m_CurWavesRemaining;

	private int m_TotalWaves;

	public int MissionID
	{
		get
		{
			return m_MissionID;
		}
		set
		{
			m_MissionID = value;
		}
	}

	public float PreTime
	{
		get
		{
			return m_PreTime;
		}
		set
		{
			m_PreTime = value;
			UITowerInfo.Instance.Show();
			UITowerInfo.Instance.SetPrepTime(m_PreTime);
		}
	}

	public float RemainTime
	{
		get
		{
			return m_RemainTime;
		}
		set
		{
			m_RemainTime = value;
		}
	}

	public int MaxCount
	{
		get
		{
			return m_MaxCount;
		}
		set
		{
			m_MaxCount = value;
		}
	}

	public int CurCount
	{
		get
		{
			return m_CurCount;
		}
		set
		{
			m_CurCount = value;
		}
	}

	public bool bRefurbish
	{
		get
		{
			return m_bRefurbish;
		}
		set
		{
			m_bRefurbish = value;
		}
	}

	public int CurWavesRemaining
	{
		get
		{
			return m_CurWavesRemaining;
		}
		set
		{
			m_CurWavesRemaining = value;
		}
	}

	public int TotalWaves
	{
		get
		{
			return m_TotalWaves;
		}
		set
		{
			m_TotalWaves = value;
		}
	}

	public int curCount()
	{
		if (!m_bRefurbish || true)
		{
			return m_CurCount;
		}
		m_bRefurbish = false;
		m_CurCount = MissionManager.Instance.GetTowerDefineKillNum(m_MissionID);
		return m_CurCount;
	}

	public void Cleanup()
	{
		m_MissionID = 0;
		m_PreTime = 0f;
		m_CurCount = 0;
		m_MaxCount = 0;
		m_bRefurbish = false;
	}
}

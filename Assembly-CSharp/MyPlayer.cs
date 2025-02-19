public class MyPlayer
{
	private bool m_IsRequest;

	private bool m_IsCaptain;

	private string m_TeamName = string.Empty;

	private int m_TeamNumber;

	private string m_PlayerName = string.Empty;

	private int m_Kill;

	private int m_Death;

	private int m_Score;

	public bool IsRequest
	{
		get
		{
			return m_IsRequest;
		}
		set
		{
			m_IsRequest = value;
		}
	}

	public bool IsCaptain
	{
		get
		{
			return m_IsCaptain;
		}
		set
		{
			m_IsCaptain = value;
		}
	}

	public string TeamName
	{
		get
		{
			return m_TeamName;
		}
		set
		{
			m_TeamName = value;
		}
	}

	public int TeamNumber
	{
		get
		{
			return m_TeamNumber;
		}
		set
		{
			m_TeamNumber = value;
		}
	}

	public string PlayerName
	{
		get
		{
			return m_PlayerName;
		}
		set
		{
			m_PlayerName = value;
		}
	}

	public int Kill
	{
		get
		{
			return m_Kill;
		}
		set
		{
			m_Kill = value;
		}
	}

	public int Death
	{
		get
		{
			return m_Death;
		}
		set
		{
			m_Death = value;
		}
	}

	public int Score
	{
		get
		{
			return m_Score;
		}
		set
		{
			m_Score = value;
		}
	}
}

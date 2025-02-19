using System.Collections.Generic;

public class AdRandomGroup
{
	public int m_ID;

	public int m_FinishTimes;

	public int m_Area;

	private bool m_IsMultiMode;

	public Dictionary<int, List<GroupInfo>> m_GroupList;

	public bool IsMultiMode
	{
		get
		{
			return m_IsMultiMode || ServerConfig.IsStory;
		}
		set
		{
			m_IsMultiMode = value;
		}
	}

	public AdRandomGroup()
	{
		m_GroupList = new Dictionary<int, List<GroupInfo>>();
	}
}

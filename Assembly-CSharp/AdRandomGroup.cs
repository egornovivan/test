using System.Collections.Generic;

public class AdRandomGroup
{
	public int m_ID;

	public bool m_requstAll;

	public List<int> m_preLimit;

	public int m_FinishTimes;

	public int m_Area;

	public bool m_IsMultiMode;

	public Dictionary<int, List<GroupInfo>> m_GroupList;

	public AdRandomGroup()
	{
		m_preLimit = new List<int>();
		m_GroupList = new Dictionary<int, List<GroupInfo>>();
	}
}

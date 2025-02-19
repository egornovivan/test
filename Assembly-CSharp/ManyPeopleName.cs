using System.Collections.Generic;

public class ManyPeopleName
{
	private List<string> m_NameList = new List<string>();

	public List<string> NameList => m_NameList;

	public ManyPeopleName(params string[] names)
	{
		if (names.Length > 0)
		{
			m_NameList.AddRange(names);
		}
	}
}

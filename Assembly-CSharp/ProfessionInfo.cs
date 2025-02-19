using System.Collections.Generic;

public class ProfessionInfo
{
	private string m_ProfessionName;

	private List<ManyPeopleName> m_ManyPeopleList;

	public string ProfessionName => m_ProfessionName;

	public List<ManyPeopleName> ManyPeopleList => m_ManyPeopleList;

	public ProfessionInfo(string professionName, List<ManyPeopleName> manyPeopleList)
	{
		m_ProfessionName = professionName;
		m_ManyPeopleList = manyPeopleList;
	}
}

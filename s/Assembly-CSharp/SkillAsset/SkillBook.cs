using System.Collections.Generic;

namespace SkillAsset;

public class SkillBook
{
	public enum SortType
	{
		SORTTYPE_TIME,
		SORTTYPE_ALPHABET
	}

	private SortType m_sortType;

	private int m_skillPoint;

	internal List<int> m_mergeSkillIDs;

	internal List<int> m_effSkillIDs;

	internal List<int> m_mergeSkillIDsSorted;

	public SkillBook()
	{
		m_mergeSkillIDs = new List<int>();
		m_effSkillIDs = new List<int>();
	}

	public SkillBook(List<int> mergeSkillIDs, List<int> effSkillIDs)
	{
		m_mergeSkillIDs = mergeSkillIDs;
		m_effSkillIDs = effSkillIDs;
	}

	public void SortMergeSkill(SortType type)
	{
		switch (type)
		{
		case SortType.SORTTYPE_TIME:
			m_mergeSkillIDsSorted = new List<int>(m_mergeSkillIDs);
			break;
		case SortType.SORTTYPE_ALPHABET:
			m_mergeSkillIDsSorted = new List<int>(m_mergeSkillIDs);
			break;
		}
	}
}

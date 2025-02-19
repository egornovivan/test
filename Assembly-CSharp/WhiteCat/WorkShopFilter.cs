namespace WhiteCat;

public class WorkShopFilter
{
	private int m_ID;

	private int m_Level;

	private string m_Tag;

	private readonly string m_LevelSpace = "    ";

	public int ID => m_ID;

	public int Level => m_Level;

	public string Tag => m_Tag;

	public WorkShopFilter(int id, int lv, string tag)
	{
		m_ID = id;
		m_Level = lv;
		m_Tag = tag;
	}

	public string GetNameByID()
	{
		string text = PELocalization.GetString(m_ID);
		if (!string.IsNullOrEmpty(text))
		{
			if (m_Level > 1)
			{
				for (int i = 0; i < m_Level - 1; i++)
				{
					text = m_LevelSpace + text;
				}
			}
			return text;
		}
		return string.Empty;
	}
}

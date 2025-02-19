using System.Collections.Generic;

public class BSAction
{
	private List<BSModify> m_Modifies = new List<BSModify>();

	public void AddModify(BSModify modify)
	{
		m_Modifies.Add(modify);
	}

	public bool Undo()
	{
		for (int num = m_Modifies.Count - 1; num >= 0; num--)
		{
			if (!m_Modifies[num].Undo())
			{
				return false;
			}
		}
		return true;
	}

	public bool Redo()
	{
		for (int i = 0; i < m_Modifies.Count; i++)
		{
			if (!m_Modifies[i].Redo())
			{
				return false;
			}
		}
		return true;
	}

	public bool IsEmpty()
	{
		return m_Modifies.Count == 0;
	}

	public void ClearNullModify()
	{
		for (int num = m_Modifies.Count - 1; num >= 0; num--)
		{
			if (m_Modifies[num].IsNull())
			{
				m_Modifies.RemoveAt(num);
			}
		}
	}

	public bool Do()
	{
		return Redo();
	}
}

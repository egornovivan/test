using System.Collections.Generic;

public static class BSHistory
{
	private const int s_MaxCount = 5;

	private static List<BSAction> m_Undos = new List<BSAction>();

	private static List<BSAction> m_Redos = new List<BSAction>();

	public static void Clear()
	{
		m_Redos.Clear();
		m_Undos.Clear();
	}

	public static void AddAction(BSAction action)
	{
		ClearNullAction();
		if (m_Undos.Count == 5)
		{
			m_Undos.RemoveAt(0);
		}
		m_Undos.Add(action);
		m_Redos.Clear();
	}

	public static void Undo()
	{
		ClearNullAction();
		if (m_Undos.Count != 0)
		{
			int index = m_Undos.Count - 1;
			m_Undos[index].Undo();
			m_Redos.Add(m_Undos[index]);
			m_Undos.RemoveAt(index);
		}
	}

	public static void Redo()
	{
		ClearNullAction();
		if (m_Redos.Count != 0)
		{
			int index = m_Redos.Count - 1;
			m_Redos[index].Redo();
			m_Undos.Add(m_Redos[index]);
			m_Redos.RemoveAt(index);
		}
	}

	public static void ClearNullAction()
	{
		for (int num = m_Undos.Count - 1; num >= 0; num--)
		{
			m_Undos[num].ClearNullModify();
			if (m_Undos[num].IsEmpty())
			{
				m_Undos.RemoveAt(num);
			}
		}
		for (int num2 = m_Redos.Count - 1; num2 >= 0; num2--)
		{
			m_Redos[num2].ClearNullModify();
			if (m_Redos[num2].IsEmpty())
			{
				m_Redos.RemoveAt(num2);
			}
		}
	}
}

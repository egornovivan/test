using System.Collections.Generic;

namespace Pathea;

public class NpcThinkAgent
{
	private List<EThinkingType> m_NeedThink;

	public NpcThinkAgent()
	{
		m_NeedThink = new List<EThinkingType>();
	}

	public bool ContainsType(EThinkingType type)
	{
		for (int i = 0; i < m_NeedThink.Count; i++)
		{
			if (m_NeedThink[i] == type)
			{
				return true;
			}
		}
		return false;
	}

	public void AddThink(EThinkingType type)
	{
		if (!ContainsType(type))
		{
			m_NeedThink.Add(type);
		}
	}

	public bool RemoveThink(EThinkingType type)
	{
		if (ContainsType(type))
		{
			return m_NeedThink.Remove(type);
		}
		return false;
	}

	public void RunThinking()
	{
	}

	public bool CanDo(EThinkingType curtype)
	{
		if (m_NeedThink.Count == 0)
		{
			return true;
		}
		return m_NeedThink[m_NeedThink.Count - 1] == curtype;
	}

	public bool hasthinkType(EThinkingType curtype)
	{
		for (int i = 0; i < m_NeedThink.Count; i++)
		{
			if (m_NeedThink[i] == curtype)
			{
				return true;
			}
		}
		return false;
	}

	public EThinkingType GetNowDo()
	{
		return m_NeedThink[m_NeedThink.Count - 1];
	}
}

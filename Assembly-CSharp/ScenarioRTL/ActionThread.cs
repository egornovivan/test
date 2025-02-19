using System;
using ScenarioRTL.IO;

namespace ScenarioRTL;

public class ActionThread
{
	private StatementRaw[] m_Raw;

	private Action[] m_ActionCaches;

	private int m_CurrIndex;

	private Action m_CurrAction;

	public Action<ActionThread> onFinished;

	public Trigger trigger { get; private set; }

	public int group { get; private set; }

	public int triggerIndex { get; private set; }

	public int currIndex => m_CurrIndex;

	public Action currAction => m_CurrAction;

	public bool isFinished => m_CurrIndex >= m_Raw.Length;

	public ActionThread(Trigger _trigger, int _triggerIdx, int _group, StatementRaw[] raw, Action[] action_caches, int cur_idx = 0)
	{
		trigger = _trigger;
		group = _group;
		triggerIndex = _triggerIdx;
		m_Raw = raw;
		m_ActionCaches = action_caches;
		m_CurrIndex = cur_idx;
	}

	public void ProcessAction()
	{
		while (m_Raw.Length > m_CurrIndex)
		{
			if (m_CurrAction == null)
			{
				CreateCurrAction();
			}
			if (m_CurrAction == null)
			{
				m_CurrIndex++;
				continue;
			}
			if (m_CurrAction.Logic())
			{
				m_CurrAction = null;
				m_CurrIndex++;
				continue;
			}
			break;
		}
		if (isFinished && onFinished != null)
		{
			onFinished(this);
		}
	}

	public void CreateCurrAction()
	{
		if (m_Raw.Length > m_CurrIndex)
		{
			if (m_ActionCaches[m_CurrIndex] == null)
			{
				m_CurrAction = Asm.CreateActionInstance(m_Raw[m_CurrIndex].classname);
			}
			else
			{
				m_CurrAction = m_ActionCaches[m_CurrIndex];
			}
			if (m_CurrAction != null)
			{
				m_CurrAction.Init(trigger, m_Raw[m_CurrIndex]);
			}
		}
	}
}

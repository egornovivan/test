using System;
using System.Collections.Generic;
using System.IO;
using ScenarioRTL.IO;

namespace ScenarioRTL;

public class Trigger
{
	private TriggerRaw m_Raw;

	public List<Condition[]> m_Conditions;

	public List<Action[]> m_ActionCache;

	private int m_ThreadCounter;

	public int repeat { get; private set; }

	public int index { get; private set; }

	public bool enabled { get; private set; }

	public string name => m_Raw.name;

	public bool multiThreaded => m_Raw.multiThreaded;

	public List<EventListener> eventListeners { get; set; }

	public Mission mission { get; private set; }

	public int threadCounter => m_ThreadCounter;

	public bool isAlive => repeat != 0;

	public Trigger(TriggerRaw _raw, Mission _mission, int _idx)
	{
		m_Raw = _raw;
		mission = _mission;
		index = _idx;
		repeat = _raw.repeat;
		enabled = true;
	}

	public Action[] GetActionCache(int grp_idx)
	{
		if (grp_idx >= 0 && grp_idx < m_ActionCache.Count)
		{
			return m_ActionCache[grp_idx];
		}
		return null;
	}

	internal void InitConditions()
	{
		if (m_Conditions == null)
		{
			m_Conditions = new List<Condition[]>();
			for (int i = 0; i < m_Raw.conditions.Count; i++)
			{
				Condition[] array = new Condition[m_Raw.conditions[i].Length];
				for (int j = 0; j < array.Length; j++)
				{
					Condition condition = Asm.CreateConditionInstance(m_Raw.conditions[i][j].classname);
					if (condition != null)
					{
						condition.Init(this, m_Raw.conditions[i][j]);
						array[j] = condition;
					}
				}
				m_Conditions.Add(array);
			}
			return;
		}
		for (int k = 0; k < m_Conditions.Count; k++)
		{
			for (int l = 0; l < m_Conditions[k].Length; l++)
			{
				m_Conditions[k][l].Init(this, m_Raw.conditions[k][l]);
			}
		}
	}

	internal void StartProcessCondition()
	{
		ConditionThread.Create(this);
	}

	internal void StartProcessAction()
	{
		repeat--;
		for (int i = 0; i < m_Raw.actions.Count; i++)
		{
			FillActionCache(i);
			ActionThread actionThread = new ActionThread(this, index, i, m_Raw.actions[i], m_ActionCache[i]);
			actionThread.ProcessAction();
			if (!actionThread.isFinished)
			{
				mission.scenario.AddActionThread(actionThread);
				RegisterActionThreadEvent(actionThread);
			}
		}
		if (m_ThreadCounter != 0 && !multiThreaded)
		{
			enabled = false;
		}
	}

	internal void FillActionCache(int grp_idx)
	{
		_initActionCache();
		for (int i = 0; i < m_Raw.actions[grp_idx].Length; i++)
		{
			StatementRaw statementRaw = m_Raw.actions[grp_idx][i];
			if (Asm.ActionIsRecyclable(statementRaw.classname))
			{
				if (m_ActionCache[grp_idx][i] == null)
				{
					m_ActionCache[grp_idx][i] = Asm.CreateActionInstance(statementRaw.classname);
				}
				if (m_ActionCache[grp_idx][i] != null)
				{
					m_ActionCache[grp_idx][i].Init(this, statementRaw);
				}
			}
		}
	}

	private void _initActionCache()
	{
		if (m_ActionCache == null)
		{
			m_ActionCache = new List<Action[]>();
			for (int i = 0; i < m_Raw.actions.Count; i++)
			{
				Action[] item = new Action[m_Raw.actions[i].Length];
				m_ActionCache.Add(item);
			}
		}
	}

	internal void RegisterActionThreadEvent(ActionThread thread)
	{
		m_ThreadCounter = Math.Min(0, m_ThreadCounter + 1);
		thread.onFinished = (Action<ActionThread>)Delegate.Combine(thread.onFinished, new Action<ActionThread>(OnActionThreadFinished));
	}

	internal void UnregisterActionThreadEvent(ActionThread thread)
	{
		m_ThreadCounter = Math.Max(0, m_ThreadCounter - 1);
		thread.onFinished = (Action<ActionThread>)Delegate.Remove(thread.onFinished, new Action<ActionThread>(OnActionThreadFinished));
	}

	private void OnActionThreadFinished(ActionThread thread)
	{
		UnregisterActionThreadEvent(thread);
		if (m_ThreadCounter == 0 && !multiThreaded)
		{
			enabled = true;
		}
	}

	internal void Import(BinaryReader r)
	{
		repeat = r.ReadInt32();
		enabled = r.ReadBoolean();
	}

	internal void Export(BinaryWriter w)
	{
		w.Write(repeat);
		w.Write(enabled);
	}

	public override string ToString()
	{
		if (m_Raw != null)
		{
			return "Trigger [" + name + "]";
		}
		return "Unknown Trigger";
	}
}

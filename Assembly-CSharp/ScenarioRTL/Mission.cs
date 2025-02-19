using System.Collections.Generic;
using ScenarioRTL.IO;

namespace ScenarioRTL;

public class Mission
{
	private MissionRaw m_Raw;

	private Trigger[] m_Triggers;

	private List<EventListener> m_EventListeners;

	public int instId { get; private set; }

	public int dataId => m_Raw.id;

	public ParamRaw properties => m_Raw.properties;

	public bool enabled { get; private set; }

	public Trigger[] triggers => m_Triggers;

	public Scenario scenario { get; private set; }

	public Mission(int instid, MissionRaw raw, Scenario scene)
	{
		instId = instid;
		m_Raw = raw;
		scenario = scene;
	}

	internal void Init()
	{
		m_EventListeners = new List<EventListener>(16);
		m_Triggers = new Trigger[m_Raw.triggers.Length];
		for (int i = 0; i < m_Raw.triggers.Length; i++)
		{
			TriggerRaw triggerRaw = m_Raw.triggers[i];
			m_Triggers[i] = new Trigger(m_Raw.triggers[i], this, i);
			List<EventListener> list = new List<EventListener>();
			for (int j = 0; j < triggerRaw.events.Length; j++)
			{
				EventListener eventListener = Asm.CreateEventListenerInstance(triggerRaw.events[j].classname);
				if (eventListener != null)
				{
					eventListener.Init(m_Triggers[i], triggerRaw.events[j]);
					m_EventListeners.Add(eventListener);
					list.Add(eventListener);
				}
			}
			m_Triggers[i].eventListeners = list;
		}
		m_EventListeners.Sort((EventListener lhs, EventListener rhs) => lhs.order - rhs.order);
	}

	internal void Run()
	{
		for (int i = 0; i < m_EventListeners.Count; i++)
		{
			m_EventListeners[i].Listen();
		}
		enabled = true;
	}

	internal void Resume()
	{
		for (int i = 0; i < m_EventListeners.Count; i++)
		{
			m_EventListeners[i].Listen();
		}
		enabled = true;
	}

	internal void Close()
	{
		for (int i = 0; i < m_EventListeners.Count; i++)
		{
			m_EventListeners[i].Close();
		}
		enabled = false;
	}

	internal void Pause()
	{
		for (int i = 0; i < m_EventListeners.Count; i++)
		{
			m_EventListeners[i].Close();
		}
		enabled = false;
	}

	public override string ToString()
	{
		string text = ((!enabled) ? "[Disabled]" : "[Running]");
		if (m_Raw != null)
		{
			return "Mission Instance [" + instId + "] : [" + dataId + " " + m_Raw.name + "] " + text;
		}
		return "Unknown Mission";
	}
}

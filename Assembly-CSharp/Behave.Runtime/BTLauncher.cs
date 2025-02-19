using System.Collections.Generic;

namespace Behave.Runtime;

public class BTLauncher : Singleton<BTLauncher>
{
	private const int s_MaxCount = 10000000;

	private int m_Count;

	private Dictionary<int, BTAgent> m_Agents = new Dictionary<int, BTAgent>();

	public int Instantiate(string btPath, IAgent agent, bool isLaunch = true)
	{
		if (!string.IsNullOrEmpty(btPath))
		{
			BTAgent bTAgent = BTResolver.Instantiate(btPath, agent);
			if (bTAgent != null)
			{
				if (isLaunch)
				{
					bTAgent.Start();
				}
				int num = ++m_Count;
				m_Agents.Add(num, bTAgent);
				return num;
			}
		}
		return -1;
	}

	public BTAgent GetAgent(int id)
	{
		if (m_Agents.ContainsKey(id))
		{
			return m_Agents[id];
		}
		return null;
	}

	public bool Excute(int id)
	{
		if (m_Agents.ContainsKey(id))
		{
			m_Agents[id].Start();
			return true;
		}
		return false;
	}

	public bool IsStart(int id)
	{
		if (m_Agents.ContainsKey(id))
		{
			return m_Agents[id].IsStart();
		}
		return false;
	}

	public bool Pause(int id, bool value)
	{
		if (m_Agents.ContainsKey(id))
		{
			m_Agents[id].Pause(value);
			return true;
		}
		return false;
	}

	public void PauseAll(bool value)
	{
		foreach (KeyValuePair<int, BTAgent> agent in m_Agents)
		{
			agent.Value.Pause(value);
		}
	}

	public bool Reset(int id)
	{
		if (m_Agents.ContainsKey(id))
		{
			m_Agents[id].Reset();
			return true;
		}
		return false;
	}

	public bool Stop(int id)
	{
		if (m_Agents.ContainsKey(id))
		{
			m_Agents[id].Stop();
			return true;
		}
		return false;
	}

	public void Remove(int id)
	{
		if (m_Agents.ContainsKey(id))
		{
			m_Agents[id].Destroy();
			m_Agents.Remove(id);
		}
	}

	public void Clear()
	{
		foreach (KeyValuePair<int, BTAgent> agent in m_Agents)
		{
			agent.Value.Destroy();
		}
		m_Agents.Clear();
	}

	private void OnLevelWasLoaded(int level)
	{
		m_Count = 10000000;
		Clear();
	}
}

using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

namespace Behave.Runtime;

public class BTAgent
{
	private string m_BtPath;

	private string m_LibraryName;

	private string m_TreeName;

	private IAgent m_Agent;

	private Tree m_Tree;

	private BTCoroutine m_Runner;

	private bool m_Pause;

	private List<BTAction> m_Actions;

	private BehaveResult m_LastTickResult;

	public string btPath => m_BtPath;

	public BTAgent(string btPath, string libraryName, string treeName)
	{
		m_BtPath = btPath;
		m_LibraryName = libraryName;
		m_TreeName = treeName;
		m_Pause = false;
		m_Runner = null;
		m_Agent = null;
		m_Actions = new List<BTAction>();
		m_Tree = Reflecter.Instance.Instantiate(libraryName, treeName);
	}

	public BTAgent Clone()
	{
		BTAgent bTAgent = new BTAgent(m_BtPath, m_LibraryName, m_TreeName);
		for (int i = 0; i < m_Actions.Count; i++)
		{
			bTAgent.RegisterAction(m_Actions[i].Clone());
		}
		return bTAgent;
	}

	private void SetTreeForward(Tree tree, BTAction action)
	{
		try
		{
			int id = (int)Enum.Parse(tree.LibraryActions, action.Name);
			MethodInfo method = action.GetType().GetMethod("Start", BindingFlags.Instance | BindingFlags.NonPublic);
			MethodInfo method2 = action.GetType().GetMethod("Init", BindingFlags.Instance | BindingFlags.NonPublic, null, new Type[1] { typeof(Tree) }, null);
			MethodInfo method3 = action.GetType().GetMethod("Tick", BindingFlags.Instance | BindingFlags.NonPublic, null, new Type[1] { typeof(Tree) }, null);
			MethodInfo method4 = action.GetType().GetMethod("Reset", BindingFlags.Instance | BindingFlags.NonPublic, null, new Type[1] { typeof(Tree) }, null);
			method?.Invoke(action, null);
			if (method2 != null)
			{
				tree.SetInitForward(id, Delegate.CreateDelegate(typeof(TickForward), action, method2) as TickForward);
			}
			if (method3 != null)
			{
				tree.SetTickForward(id, Delegate.CreateDelegate(typeof(TickForward), action, method3) as TickForward);
			}
			if (method4 != null)
			{
				tree.SetResetForward(id, Delegate.CreateDelegate(typeof(ResetForward), action, method4) as ResetForward);
			}
		}
		catch (Exception)
		{
		}
	}

	private bool CanRun()
	{
		if (m_Pause || m_Agent.Equals(null))
		{
			return false;
		}
		IBehave behave = m_Agent as IBehave;
		if (!behave.Equals(null) && !behave.BehaveActive)
		{
			return false;
		}
		return true;
	}

	private IEnumerator Runner()
	{
		WaitForSeconds wait = new WaitForSeconds(1f / m_Tree.Frequency);
		while (m_Tree != null)
		{
			if (CanRun())
			{
				try
				{
					m_LastTickResult = m_Tree.Tick(m_Agent, null);
				}
				catch (Exception ex)
				{
					Exception e = ex;
					Debug.LogWarning("Tree Tick Error : " + e);
				}
				if (m_LastTickResult != 0)
				{
					m_Tree.Reset();
				}
			}
			yield return wait;
		}
	}

	public List<BTAction> GetActions()
	{
		return m_Actions;
	}

	public BTAction GetAction(string actionName)
	{
		return m_Actions.Find((BTAction ret) => ret.Name == actionName);
	}

	public bool Contains(string actionName)
	{
		return m_Actions.Find((BTAction ret) => ret.Name == actionName) != null;
	}

	public void RegisterAction(BTAction action)
	{
		if (action != null)
		{
			SetTreeForward(m_Tree, action);
			if (m_Agent != null && !m_Agent.Equals(null))
			{
				action.SetAgent(m_Agent);
			}
			m_Actions.Add(action);
		}
	}

	public void SetAgent(IAgent agent)
	{
		if (m_Agent == null || m_Agent.Equals(null) || !m_Agent.Equals(agent))
		{
			m_Agent = agent;
			for (int i = 0; i < m_Actions.Count; i++)
			{
				m_Actions[i].SetAgent(m_Agent);
			}
			if (m_Runner == null)
			{
				m_Runner = new BTCoroutine(Singleton<BTLauncher>.Instance, Runner());
			}
		}
	}

	public void Tick()
	{
		if (m_Tree != null)
		{
			m_Tree.Tick(BehaveAgent.Agent, null);
		}
	}

	public void Start()
	{
		if (m_Runner != null)
		{
			m_Runner.Start();
		}
	}

	public void Pause(bool isPause)
	{
		if (m_Runner != null)
		{
			m_Pause = isPause;
		}
	}

	public bool IsStart()
	{
		if (m_Runner != null)
		{
			return m_Runner.IsStart;
		}
		return false;
	}

	public void Reset()
	{
		if (m_Runner != null)
		{
			m_Runner.Stop();
			m_Runner.Start();
		}
	}

	public void Stop()
	{
		if (m_Tree != null)
		{
			m_Tree.Reset(m_Agent, m_Tree, null);
		}
		if (m_Runner != null)
		{
			m_Runner.Stop();
		}
	}

	public void Destroy()
	{
		Stop();
		m_Tree = null;
		m_Runner = null;
		m_Agent = null;
	}
}

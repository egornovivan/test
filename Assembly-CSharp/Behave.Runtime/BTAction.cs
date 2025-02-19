using System;
using System.Collections.Generic;
using System.Reflection;

namespace Behave.Runtime;

public class BTAction
{
	private string m_Name;

	private IAgent m_Agent;

	internal Dictionary<string, object> m_TreeDataList;

	public string Name => m_Name;

	public BTAction()
	{
		m_Name = "Unknown";
		m_TreeDataList = new Dictionary<string, object>();
	}

	public BTAction Clone()
	{
		BTAction bTAction = Activator.CreateInstance(GetType()) as BTAction;
		bTAction.m_Name = m_Name;
		foreach (KeyValuePair<string, object> treeData in m_TreeDataList)
		{
			object obj = Activator.CreateInstance(treeData.Value.GetType());
			List<FieldInfo> list = BTResolver.s_Fields[m_Name];
			for (int i = 0; i < list.Count; i++)
			{
				list[i].SetValue(obj, list[i].GetValue(treeData.Value));
			}
			bTAction.m_TreeDataList.Add(treeData.Key, obj);
		}
		return bTAction;
	}

	public void SetName(string argName)
	{
		m_Name = argName;
	}

	public void SetAgent(IAgent argAgent)
	{
		if (m_Agent == null || m_Agent.Equals(null) || !m_Agent.Equals(argAgent))
		{
			m_Agent = argAgent;
			InitAgent(argAgent);
		}
	}

	public void AddData(string name, object obj)
	{
		if (!m_TreeDataList.ContainsKey(name))
		{
			m_TreeDataList.Add(name, obj);
		}
		else
		{
			m_TreeDataList[name] = obj;
		}
	}

	public Dictionary<string, object> GetDatas()
	{
		return m_TreeDataList;
	}

	internal virtual void InitAgent(IAgent argAgent)
	{
	}
}

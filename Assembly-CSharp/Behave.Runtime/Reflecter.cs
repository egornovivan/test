using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

namespace Behave.Runtime;

public class Reflecter
{
	private static Reflecter _Instance;

	private List<BehaveAction> m_BehaveActions;

	private List<BehaveLibrary> m_BehaveLibrarys;

	public static Reflecter Instance
	{
		get
		{
			if (_Instance == null)
			{
				_Instance = new Reflecter();
			}
			return _Instance;
		}
	}

	public Reflecter()
	{
		InitAction();
		InitLibrary();
	}

	private void InitAction()
	{
		m_BehaveActions = new List<BehaveAction>();
		Type[] types = Assembly.GetAssembly(typeof(BTAction)).GetTypes();
		Type[] array = types;
		foreach (Type type in array)
		{
			if (!type.IsSubclassOf(typeof(BTAction)))
			{
				continue;
			}
			object[] customAttributes = type.GetCustomAttributes(inherit: true);
			object[] array2 = customAttributes;
			foreach (object obj in array2)
			{
				if (typeof(BehaveAction).IsInstanceOfType(obj))
				{
					m_BehaveActions.Add(obj as BehaveAction);
				}
			}
		}
	}

	private void InitLibrary()
	{
		m_BehaveLibrarys = new List<BehaveLibrary>();
		string[] s_Librarys = BTConfig.s_Librarys;
		foreach (string libName in s_Librarys)
		{
			m_BehaveLibrarys.Add(new BehaveLibrary(libName));
		}
	}

	public BTAction CreateAction(string actionName)
	{
		Type type = m_BehaveActions.Find((BehaveAction ret) => ret.name.Equals(actionName))?.type;
		if (type == null)
		{
			Debug.LogError("Can't find action : [" + actionName + "]");
			return null;
		}
		BTAction bTAction = Activator.CreateInstance(type) as BTAction;
		bTAction.SetName(actionName);
		return bTAction;
	}

	public Tree Instantiate(string libraryName, string treeName)
	{
		return m_BehaveLibrarys.Find((BehaveLibrary ret) => ret.Match(libraryName))?.Instantiate(treeName);
	}

	public List<string> GetActionsOfTree(string libraryName, string treeName)
	{
		return m_BehaveLibrarys.Find((BehaveLibrary ret) => ret.Match(libraryName))?.GetActions(treeName);
	}

	public List<string> GetTrees(string libraryName)
	{
		return m_BehaveLibrarys.Find((BehaveLibrary ret) => ret.Match(libraryName))?.GetTrees();
	}

	public Type[] GetActions()
	{
		List<Type> list = new List<Type>();
		foreach (BehaveAction behaveAction in m_BehaveActions)
		{
			list.Add(behaveAction.type);
		}
		return list.ToArray();
	}
}

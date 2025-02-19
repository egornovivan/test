using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

namespace Behave.Runtime;

public class BehaveLibrary
{
	private string m_LibraryName;

	private string m_LibraryDllName;

	private string m_LibraryTypeName;

	private Assembly m_LiraryAssembly;

	private Type m_LibraryType;

	private Type m_TreeType;

	private MethodInfo m_InstantiateFunc;

	private Dictionary<string, Type> m_TreeTypes;

	private Dictionary<string, List<string>> m_TreeActions;

	public string error
	{
		get
		{
			if (m_LiraryAssembly == null)
			{
				return "Can't find assembly : " + m_LibraryDllName;
			}
			if (m_LibraryType == null)
			{
				return "Can't find library : " + m_LibraryTypeName;
			}
			if (m_TreeType == null || !m_TreeType.IsSubclassOf(typeof(Enum)))
			{
				return "Tree type of " + m_LibraryName + " is not right!";
			}
			return string.Empty;
		}
	}

	public BehaveLibrary(string libName)
	{
		m_LibraryName = libName;
		m_LibraryDllName = GetLibraryDll(m_LibraryName);
		m_LibraryTypeName = GetLibraryType(m_LibraryName);
		m_LiraryAssembly = Assembly.Load(m_LibraryDllName);
		if (m_LiraryAssembly != null)
		{
			m_LibraryType = m_LiraryAssembly.GetType(m_LibraryTypeName);
			if (m_LibraryType != null)
			{
				m_TreeType = m_LibraryType.GetNestedType("TreeType");
				m_InstantiateFunc = m_LibraryType.GetMethod("InstantiateTree", BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public, null, new Type[2]
				{
					m_TreeType,
					typeof(IAgent)
				}, null);
			}
		}
		if (error == string.Empty)
		{
			InitTreeType();
			InitTreeActions();
		}
	}

	public bool Match(string libraryName)
	{
		return m_LibraryName.Equals(libraryName) && error == string.Empty;
	}

	public List<string> GetTrees()
	{
		return new List<string>(m_TreeActions.Keys);
	}

	public List<string> GetActions(string treeName)
	{
		return (!m_TreeActions.ContainsKey(treeName)) ? null : m_TreeActions[treeName];
	}

	public Tree Instantiate(string treeName)
	{
		if (error != string.Empty)
		{
			Debug.LogError(error);
			return null;
		}
		try
		{
			object obj = Enum.Parse(m_TreeType, treeName);
			return (obj == null) ? null : (m_InstantiateFunc.Invoke(null, new object[2] { obj, null }) as Tree);
		}
		catch (Exception ex)
		{
			Debug.LogError("[" + m_LibraryName + "][" + treeName + "]" + ex);
			return null;
		}
	}

	private string GetLibraryDll(string libName)
	{
		return libName + "Build";
	}

	private string GetLibraryType(string libName)
	{
		return "BL" + libName;
	}

	private string GetTreeType(string treeName)
	{
		object treeEnum = GetTreeEnum(treeName);
		return (treeEnum == null) ? string.Empty : ("BT" + m_LibraryName + "T" + (int)treeEnum);
	}

	private object GetTreeEnum(string treeName)
	{
		try
		{
			return Enum.Parse(m_TreeType, treeName);
		}
		catch (Exception message)
		{
			Debug.LogWarning(message);
			return null;
		}
	}

	private void InitTreeType()
	{
		m_TreeTypes = new Dictionary<string, Type>();
		Type[] types = m_LiraryAssembly.GetTypes();
		Type[] array = types;
		foreach (Type type in array)
		{
			if (type.IsSubclassOf(typeof(Tree)) && !m_TreeTypes.ContainsKey(type.Name))
			{
				m_TreeTypes.Add(type.Name, type);
			}
		}
	}

	private void InitTreeActions()
	{
		m_TreeActions = new Dictionary<string, List<string>>();
		string[] names = Enum.GetNames(m_TreeType);
		string[] array = names;
		foreach (string text in array)
		{
			if (text.Equals("Unknown"))
			{
				continue;
			}
			string treeType = GetTreeType(text);
			if (!(treeType != string.Empty) || !m_TreeTypes.ContainsKey(treeType) || m_TreeTypes[treeType] == null)
			{
				continue;
			}
			if (!m_TreeActions.ContainsKey(text))
			{
				m_TreeActions.Add(text, new List<string>());
			}
			FieldInfo[] fields = m_TreeTypes[treeType].GetFields(BindingFlags.Instance | BindingFlags.NonPublic);
			FieldInfo[] array2 = fields;
			foreach (FieldInfo fieldInfo in array2)
			{
				if (fieldInfo.Name.Contains("ARF"))
				{
					string item = fieldInfo.Name.Substring("ARF".Length);
					if (!m_TreeActions[text].Contains(item))
					{
						m_TreeActions[text].Add(item);
					}
				}
			}
		}
	}
}

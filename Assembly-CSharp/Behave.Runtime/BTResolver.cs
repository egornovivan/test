using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Xml;
using UnityEngine;

namespace Behave.Runtime;

public class BTResolver
{
	public static Dictionary<string, BTAgent> s_btAgents = new Dictionary<string, BTAgent>();

	public static Dictionary<string, List<FieldInfo>> s_Fields = new Dictionary<string, List<FieldInfo>>();

	private static bool _btCached = false;

	public static void RegisterToCache(string btPath)
	{
		if (string.IsNullOrEmpty(btPath) || s_btAgents.ContainsKey(btPath))
		{
			return;
		}
		if (_btCached)
		{
			BTAgent bTAgent = Resolver(btPath);
			if (bTAgent != null)
			{
				s_btAgents.Add(btPath, bTAgent);
			}
		}
		else
		{
			s_btAgents.Add(btPath, null);
		}
	}

	public static IEnumerator ApplyCacheBt()
	{
		_btCached = true;
		List<string> keys = new List<string>(s_btAgents.Keys);
		int nKeys = keys.Count;
		for (int i = 0; i < nKeys; i++)
		{
			string btPath = keys[i];
			if (s_btAgents[btPath] == null)
			{
				s_btAgents[btPath] = Resolver(btPath);
				yield return 0;
			}
		}
	}

	public static BTAgent Pop(string btPath)
	{
		BTAgent result = null;
		if (s_btAgents.TryGetValue(btPath, out var value) && value != null)
		{
			result = value.Clone();
		}
		else
		{
			Debug.Log("Can't find behave tree : " + btPath);
		}
		return result;
	}

	public static BTAgent Instantiate(string btPath, IAgent agent)
	{
		BTAgent bTAgent = Pop(btPath);
		bTAgent?.SetAgent(agent);
		return bTAgent;
	}

	private static BTAgent Resolver(string btPath)
	{
		BTAgent bTAgent = null;
		TextAsset textAsset = UnityEngine.Resources.Load(btPath) as TextAsset;
		if (textAsset != null && !string.IsNullOrEmpty(textAsset.text))
		{
			try
			{
				XmlDocument xmlDocument = new XmlDocument();
				XmlReaderSettings xmlReaderSettings = new XmlReaderSettings();
				xmlReaderSettings.IgnoreComments = true;
				XmlReader xmlReader = XmlReader.Create(new StringReader(textAsset.text), xmlReaderSettings);
				xmlDocument.Load(xmlReader);
				XmlElement xmlElement = xmlDocument.SelectSingleNode("Tree") as XmlElement;
				string attributeString = XmlUtil.GetAttributeString(xmlElement, "Library");
				string attributeString2 = XmlUtil.GetAttributeString(xmlElement, "Tree");
				bTAgent = new BTAgent(btPath, attributeString, attributeString2);
				bTAgent.Tick();
				XmlNodeList elementsByTagName = xmlElement.GetElementsByTagName("Action");
				foreach (XmlNode item in elementsByTagName)
				{
					XmlElement e = item as XmlElement;
					string attributeString3 = XmlUtil.GetAttributeString(e, "Type");
					BTAction bTAction = bTAgent.GetAction(attributeString3);
					if (bTAction == null)
					{
						bTAction = Reflecter.Instance.CreateAction(attributeString3);
						bTAgent.RegisterAction(bTAction);
					}
					if (bTAction != null)
					{
						InitData(btPath, bTAction, e);
					}
				}
			}
			catch (Exception ex)
			{
				Debug.LogError(string.Concat("[", btPath, "]<", ex, ">"));
			}
		}
		return bTAgent;
	}

	private static void InitData(string btPath, BTAction action, XmlElement e)
	{
		if (!s_Fields.ContainsKey(action.Name))
		{
			s_Fields.Add(action.Name, new List<FieldInfo>());
		}
		if (action == null || e == null)
		{
			return;
		}
		object[] customAttributes = action.GetType().GetCustomAttributes(inherit: true);
		for (int i = 0; i < customAttributes.Length; i++)
		{
			if (!typeof(BehaveAction).IsInstanceOfType(customAttributes[i]))
			{
				continue;
			}
			BehaveAction behaveAction = customAttributes[i] as BehaveAction;
			XmlNodeList elementsByTagName = e.GetElementsByTagName("Data");
			for (int j = 0; j < elementsByTagName.Count; j++)
			{
				XmlElement e2 = elementsByTagName[j] as XmlElement;
				if (behaveAction.dataType == null)
				{
					continue;
				}
				object obj = Activator.CreateInstance(behaveAction.dataType);
				string attributeString = XmlUtil.GetAttributeString(e2, "Name");
				FieldInfo[] fields = obj.GetType().GetFields(BindingFlags.Instance | BindingFlags.Public);
				foreach (FieldInfo field in fields)
				{
					object[] customAttributes2 = field.GetCustomAttributes(inherit: true);
					for (int l = 0; l < customAttributes2.Length; l++)
					{
						if (typeof(BehaveAttribute).IsInstanceOfType(customAttributes2[l]))
						{
							if (s_Fields[action.Name].Find((FieldInfo ret) => ret.Name == field.Name) == null)
							{
								s_Fields[action.Name].Add(field);
							}
							string text = SetValue(field, obj, e2);
							if (text != string.Empty)
							{
								Debug.LogError(btPath + "-Action:" + action.Name + "-Data : " + attributeString + "-[" + text + "]");
							}
						}
					}
				}
				action.AddData(attributeString, obj);
			}
		}
	}

	private static string SetValue(FieldInfo field, object obj, XmlElement e)
	{
		if (!e.HasAttribute(field.Name))
		{
			return string.Empty;
		}
		try
		{
			if (field.FieldType.Equals(typeof(int)))
			{
				field.SetValue(obj, XmlUtil.GetAttributeInt32(e, field.Name));
			}
			else if (field.FieldType.Equals(typeof(float)))
			{
				field.SetValue(obj, XmlUtil.GetAttributeFloat(e, field.Name));
			}
			else if (field.FieldType.Equals(typeof(bool)))
			{
				field.SetValue(obj, XmlUtil.GetAttributeBool(e, field.Name));
			}
			else if (field.FieldType.Equals(typeof(Vector3)))
			{
				field.SetValue(obj, XmlUtil.GetAttributeVector3(e, field.Name));
			}
			else if (field.FieldType.Equals(typeof(string)))
			{
				field.SetValue(obj, XmlUtil.GetAttributeString(e, field.Name));
			}
			else if (field.FieldType.Equals(typeof(int[])))
			{
				field.SetValue(obj, XmlUtil.GetAttributeInt32Array(e, field.Name));
			}
			else if (field.FieldType.Equals(typeof(float[])))
			{
				field.SetValue(obj, XmlUtil.GetAttributeFloatArray(e, field.Name));
			}
			else if (field.FieldType.Equals(typeof(string[])))
			{
				field.SetValue(obj, XmlUtil.GetAttributeStringArray(e, field.Name));
			}
			else if (field.FieldType.Equals(typeof(Vector3[])))
			{
				field.SetValue(obj, XmlUtil.GetAttributeVector3Array(e, field.Name));
			}
			else if (field.FieldType.Equals(typeof(List<int>)))
			{
				field.SetValue(obj, new List<int>(XmlUtil.GetAttributeInt32Array(e, field.Name)));
			}
			else if (field.FieldType.Equals(typeof(List<float>)))
			{
				field.SetValue(obj, new List<float>(XmlUtil.GetAttributeFloatArray(e, field.Name)));
			}
			else if (field.FieldType.Equals(typeof(List<string>)))
			{
				field.SetValue(obj, new List<string>(XmlUtil.GetAttributeStringArray(e, field.Name)));
			}
			else if (field.FieldType.Equals(typeof(List<Vector3>)))
			{
				field.SetValue(obj, new List<Vector3>(XmlUtil.GetAttributeVector3Array(e, field.Name)));
			}
			else
			{
				Debug.LogError("Can not find type : " + field.FieldType.ToString());
			}
		}
		catch (Exception ex)
		{
			return "Obj : " + obj.GetType().Name + " --> Field : " + field.Name + " --> " + ex;
		}
		return string.Empty;
	}
}

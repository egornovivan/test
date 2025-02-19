using System;
using System.Collections.Generic;
using System.Xml;
using UnityEngine;

namespace PETools;

public class XmlUtil
{
	public static string GetAttributeString(XmlElement e, string name)
	{
		if (e != null && !string.IsNullOrEmpty(name) && e.HasAttribute(name))
		{
			return e.GetAttribute(name);
		}
		return string.Empty;
	}

	public static int GetAttributeInt32(XmlElement e, string name)
	{
		if (e != null && !string.IsNullOrEmpty(name) && e.HasAttribute(name))
		{
			try
			{
				return Convert.ToInt32(e.GetAttribute(name));
			}
			catch (Exception message)
			{
				Debug.LogError(message);
			}
		}
		return 0;
	}

	public static bool GetAttributeBool(XmlElement e, string name)
	{
		if (e != null && !string.IsNullOrEmpty(name) && e.HasAttribute(name))
		{
			try
			{
				return Convert.ToBoolean(e.GetAttribute(name));
			}
			catch (Exception message)
			{
				Debug.LogError(message);
			}
		}
		return false;
	}

	public static float GetAttributeFloat(XmlElement e, string name)
	{
		if (e != null && !string.IsNullOrEmpty(name) && e.HasAttribute(name))
		{
			try
			{
				return Convert.ToSingle(e.GetAttribute(name));
			}
			catch (Exception message)
			{
				Debug.LogError(message);
			}
		}
		return 0f;
	}

	public static Color32 GetAttributeColor(XmlElement e, string name)
	{
		if (e != null && !string.IsNullOrEmpty(name) && e.HasAttribute(name))
		{
			try
			{
				return PEUtil.ToColor32(e.GetAttribute(name), ',');
			}
			catch (Exception message)
			{
				Debug.LogError(message);
			}
		}
		return new Color32(0, 0, 0, 0);
	}

	public static string GetNodeString(XmlElement e, string name)
	{
		if (e != null && !string.IsNullOrEmpty(name))
		{
			XmlNode xmlNode = e.SelectSingleNode(name);
			if (xmlNode != null)
			{
				return xmlNode.InnerText;
			}
		}
		return string.Empty;
	}

	public static int GetNodeInt32(XmlElement e, string name)
	{
		if (e != null && !string.IsNullOrEmpty(name))
		{
			XmlNode xmlNode = e.SelectSingleNode(name);
			if (xmlNode != null)
			{
				try
				{
					return Convert.ToInt32(xmlNode.InnerText);
				}
				catch (Exception message)
				{
					Debug.LogError(message);
				}
			}
		}
		return 0;
	}

	public static float GetNodeFloat(XmlElement e, string name)
	{
		if (e != null && !string.IsNullOrEmpty(name))
		{
			XmlNode xmlNode = e.SelectSingleNode(name);
			if (xmlNode != null)
			{
				try
				{
					return Convert.ToSingle(xmlNode.InnerText);
				}
				catch (Exception message)
				{
					Debug.LogError(message);
				}
			}
		}
		return 0f;
	}

	public static bool GetNodeBool(XmlElement e, string name)
	{
		if (e != null && !string.IsNullOrEmpty(name))
		{
			XmlNode xmlNode = e.SelectSingleNode(name);
			if (xmlNode != null)
			{
				try
				{
					return Convert.ToBoolean(xmlNode.InnerText);
				}
				catch (Exception message)
				{
					Debug.LogError(message);
				}
			}
		}
		return false;
	}

	public static Color32 GetNodeColor32(XmlElement e, string name)
	{
		if (e != null && !string.IsNullOrEmpty(name))
		{
			XmlNode xmlNode = e.SelectSingleNode(name);
			if (xmlNode != null)
			{
				try
				{
					return PEUtil.ToColor32(xmlNode.InnerText, ',');
				}
				catch (Exception message)
				{
					Debug.LogError(message);
				}
			}
		}
		return new Color32(0, 0, 0, 0);
	}

	public static List<string> GetNodeStringList(XmlElement e, string tag)
	{
		List<string> list = new List<string>();
		if (e != null && !string.IsNullOrEmpty(tag))
		{
			XmlNodeList elementsByTagName = e.GetElementsByTagName(tag);
			foreach (XmlNode item in elementsByTagName)
			{
				list.Add(item.InnerText);
			}
		}
		return list;
	}

	public static List<int> GetNodeInt32List(XmlElement e, string tag)
	{
		List<int> list = new List<int>();
		if (e != null && !string.IsNullOrEmpty(tag))
		{
			XmlNodeList elementsByTagName = e.GetElementsByTagName(tag);
			foreach (XmlNode item in elementsByTagName)
			{
				try
				{
					list.Add(Convert.ToInt32(item.InnerText));
				}
				catch (Exception message)
				{
					Debug.LogError(message);
				}
			}
		}
		return list;
	}

	public static List<float> GetNodeFloatList(XmlElement e, string tag)
	{
		List<float> list = new List<float>();
		if (e != null && !string.IsNullOrEmpty(tag))
		{
			XmlNodeList elementsByTagName = e.GetElementsByTagName(tag);
			foreach (XmlNode item in elementsByTagName)
			{
				try
				{
					list.Add(Convert.ToSingle(item.InnerText));
				}
				catch (Exception message)
				{
					Debug.LogError(message);
				}
			}
		}
		return list;
	}
}

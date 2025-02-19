using System;
using System.Xml;
using UnityEngine;

namespace Behave.Runtime;

public class XmlUtil
{
	public static string GetAttributeString(XmlElement e, string name)
	{
		return e.GetAttribute(name);
	}

	public static string[] GetAttributeStringArray(XmlElement e, string name)
	{
		return StringUtil.ToArrayString(e.GetAttribute(name), ',');
	}

	public static int GetAttributeInt32(XmlElement e, string name)
	{
		return Convert.ToInt32(e.GetAttribute(name));
	}

	public static int[] GetAttributeInt32Array(XmlElement e, string name)
	{
		return StringUtil.ToArrayInt32(e.GetAttribute(name), ',');
	}

	public static bool GetAttributeBool(XmlElement e, string name)
	{
		return Convert.ToBoolean(e.GetAttribute(name));
	}

	public static float GetAttributeFloat(XmlElement e, string name)
	{
		return Convert.ToSingle(e.GetAttribute(name));
	}

	public static float[] GetAttributeFloatArray(XmlElement e, string name)
	{
		return StringUtil.ToArraySingle(e.GetAttribute(name), ',');
	}

	public static Color32 GetAttributeColor(XmlElement e, string name)
	{
		return StringUtil.ToColor32(e.GetAttribute(name), ',');
	}

	public static Vector3 GetAttributeVector3(XmlElement e, string name)
	{
		return StringUtil.ToVector3(e.GetAttribute(name), ',');
	}

	public static Vector3[] GetAttributeVector3Array(XmlElement e, string name)
	{
		return StringUtil.ToArrayVector3(e.GetAttribute(name), '|', ',');
	}
}

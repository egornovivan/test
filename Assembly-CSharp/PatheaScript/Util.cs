using System;
using System.Text;
using System.Xml;
using Pathea;
using UnityEngine;

namespace PatheaScript;

public static class Util
{
	private static StringBuilder s_strBuff = new StringBuilder(64);

	public static string GetStrHhMmSs(this UTimer t)
	{
		UTimer.TimeStruct timeStruct = t.TickToTimeStruct(t.Tick);
		int hour = timeStruct.Hour;
		int minute = timeStruct.Minute;
		int second = timeStruct.Second;
		s_strBuff.Length = 0;
		s_strBuff.Append((char)(48 + hour / 10));
		s_strBuff.Append((char)(48 + hour % 10));
		s_strBuff.Append(':');
		s_strBuff.Append((char)(48 + minute / 10));
		s_strBuff.Append((char)(48 + minute % 10));
		s_strBuff.Append(':');
		s_strBuff.Append((char)(48 + second / 10));
		s_strBuff.Append((char)(48 + second % 10));
		return s_strBuff.ToString();
	}

	public static string GetStrHhMm(this UTimer t)
	{
		UTimer.TimeStruct timeStruct = t.TickToTimeStruct(t.Tick);
		int hour = timeStruct.Hour;
		int minute = timeStruct.Minute;
		s_strBuff.Length = 0;
		s_strBuff.Append((char)(48 + hour / 10));
		s_strBuff.Append((char)(48 + hour % 10));
		s_strBuff.Append(':');
		s_strBuff.Append((char)(48 + minute / 10));
		s_strBuff.Append((char)(48 + minute % 10));
		return s_strBuff.ToString();
	}

	public static string GetStrPXZ(this PeTrans trans)
	{
		s_strBuff.Length = 0;
		s_strBuff.Append('P');
		s_strBuff.Append(':');
		s_strBuff.Append((int)trans.position.x);
		s_strBuff.Append(',');
		s_strBuff.Append((int)trans.position.z);
		return s_strBuff.ToString();
	}

	public static string GetChunkName(IntVector4 cpos)
	{
		s_strBuff.Length = 0;
		s_strBuff.Append('C');
		s_strBuff.Append('h');
		s_strBuff.Append('n');
		s_strBuff.Append('k');
		s_strBuff.Append('_');
		s_strBuff.Append(cpos.x);
		s_strBuff.Append('_');
		s_strBuff.Append(cpos.y);
		s_strBuff.Append('_');
		s_strBuff.Append(cpos.z);
		return s_strBuff.ToString();
	}

	public static string Unescape(string origin)
	{
		return Uri.UnescapeDataString(origin);
	}

	public static string GetStmtName(XmlNode xmlNode)
	{
		if (xmlNode.Name != "STMT")
		{
			throw new Exception("not STMT node.");
		}
		return xmlNode.Attributes["stmt"].Value;
	}

	public static int GetEventPriority(XmlNode xmlNode)
	{
		return GetInt(xmlNode, "order");
	}

	public static Variable.EScope GetVarScope(XmlNode xmlNode)
	{
		return GetInt(xmlNode, "scope") switch
		{
			1 => Variable.EScope.Gloabel, 
			2 => Variable.EScope.Script, 
			3 => Variable.EScope.Trigger, 
			_ => Variable.EScope.Trigger, 
		};
	}

	public static PsScript.EResult GetScriptResult(XmlNode xmlNode)
	{
		return GetInt(xmlNode, "result") switch
		{
			1 => PsScript.EResult.Accomplished, 
			2 => PsScript.EResult.Failed, 
			3 => PsScript.EResult.Abort, 
			_ => PsScript.EResult.Max, 
		};
	}

	public static VarRef GetVarRef(XmlNode xmlNode, string name, Trigger trigger)
	{
		string @string = GetString(xmlNode, name);
		return new VarRef(@string, trigger);
	}

	public static VarRef GetVarRefOrValue(XmlNode xmlNode, string name, VarValue.EType eType, Trigger trigger)
	{
		VarRef varRef = null;
		string @string = GetString(xmlNode, name);
		if (IsVarName(@string))
		{
			string varName = GetVarName(@string);
			return new VarRef(varName, trigger);
		}
		VarValue varValue = GetVarValue(@string, eType);
		return new VarRef(varValue);
	}

	private static bool IsVarName(string text)
	{
		if (string.IsNullOrEmpty(text))
		{
			return false;
		}
		if (text.Length < 3)
		{
			return false;
		}
		if (!text.StartsWith("%"))
		{
			return false;
		}
		if (!text.EndsWith("%"))
		{
			return false;
		}
		return true;
	}

	private static string GetVarName(string text)
	{
		return text.Substring(1, text.Length - 2);
	}

	private static VarValue GetVarValue(string text, VarValue.EType eType)
	{
		return eType switch
		{
			VarValue.EType.Int => new VarValue(int.Parse(text)), 
			VarValue.EType.Bool => text switch
			{
				"0" => false, 
				"1" => true, 
				_ => false, 
			}, 
			VarValue.EType.Float => new VarValue(float.Parse(text)), 
			VarValue.EType.Vector3 => new VarValue(GetVector3(text)), 
			VarValue.EType.String => new VarValue(text), 
			VarValue.EType.Var => new VarValue((object)text), 
			_ => throw new Exception("error value type"), 
		};
	}

	public static Vector3 GetVector3(string text)
	{
		string[] array = text.Split(',');
		float x = float.Parse(array[0]);
		float y = float.Parse(array[1]);
		float z = float.Parse(array[2]);
		return new Vector3(x, y, z);
	}

	public static int GetInt(XmlNode xmlNode, string name)
	{
		return int.Parse(xmlNode.Attributes[name].Value);
	}

	public static bool GetBool(XmlNode xmlNode, string name)
	{
		return bool.Parse(xmlNode.Attributes[name].Value);
	}

	public static string GetString(XmlNode xmlNode, string name)
	{
		return Unescape(xmlNode.Attributes[name].Value);
	}

	private static VarValue TryParseVarValue(XmlNode xmlNode, string name)
	{
		string text = Unescape(xmlNode.Attributes[name].Value);
		return TryParseVarValue(text);
	}

	public static VarValue TryParseVarValue(string text)
	{
		if (string.IsNullOrEmpty(text))
		{
			return new VarValue();
		}
		if (int.TryParse(text, out var result))
		{
			return new VarValue(result);
		}
		if (bool.TryParse(text, out var result2))
		{
			return new VarValue(result2);
		}
		if (float.TryParse(text, out var result3))
		{
			return new VarValue(result3);
		}
		if (TryParseVector3(text, out var _))
		{
			return new VarValue(result3);
		}
		return new VarValue(text);
	}

	private static bool TryParseVector3(string text, out Vector3 vector)
	{
		vector = Vector3.zero;
		if (string.IsNullOrEmpty(text))
		{
			return false;
		}
		string[] array = text.Split(',');
		if (array.Length != 3)
		{
			return false;
		}
		float result = 0f;
		if (!float.TryParse(array[0], out result))
		{
			return false;
		}
		float result2 = 0f;
		if (!float.TryParse(array[1], out result2))
		{
			return false;
		}
		float result3 = 0f;
		if (!float.TryParse(array[2], out result3))
		{
			return false;
		}
		vector.Set(result, result2, result3);
		return true;
	}
}

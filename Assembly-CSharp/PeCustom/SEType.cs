using System;
using System.Globalization;
using UnityEngine;

namespace PeCustom;

public static class SEType
{
	public static NumberStyles floatStyle = NumberStyles.Integer | NumberStyles.AllowDecimalPoint;

	public static bool TryParse_VECTOR(string s, out Vector3 vec)
	{
		vec = Vector3.zero;
		string[] array = s.Split(new string[1] { "," }, StringSplitOptions.None);
		if (array.Length == 0)
		{
			return false;
		}
		if (array.Length == 1)
		{
			string s2 = array[0];
			float result = 0f;
			if (!float.TryParse(s2, floatStyle, NumberFormatInfo.CurrentInfo, out result))
			{
				return false;
			}
			vec.x = result;
			return true;
		}
		if (array.Length == 2)
		{
			string s3 = array[0];
			float result2 = 0f;
			if (!float.TryParse(s3, floatStyle, NumberFormatInfo.CurrentInfo, out result2))
			{
				return false;
			}
			vec.x = result2;
			string s4 = array[1];
			float result3 = 0f;
			if (!float.TryParse(s4, floatStyle, NumberFormatInfo.CurrentInfo, out result3))
			{
				return false;
			}
			vec.y = result3;
			return true;
		}
		if (array.Length == 3)
		{
			string s5 = array[0];
			float result4 = 0f;
			if (!float.TryParse(s5, floatStyle, NumberFormatInfo.CurrentInfo, out result4))
			{
				return false;
			}
			vec.x = result4;
			string s6 = array[1];
			float result5 = 0f;
			if (!float.TryParse(s6, floatStyle, NumberFormatInfo.CurrentInfo, out result5))
			{
				return false;
			}
			vec.y = result5;
			string s7 = array[2];
			float result6 = 0f;
			if (!float.TryParse(s7, floatStyle, NumberFormatInfo.CurrentInfo, out result6))
			{
				return false;
			}
			vec.z = result6;
			return true;
		}
		return false;
	}

	public static string ToString_VECTOR(Vector3 vec)
	{
		return vec.x.ToString("0.###") + ", " + vec.y.ToString("0.###") + ", " + vec.z.ToString("0.###");
	}

	public static bool TryParse_RECT(string s, out Rect vec)
	{
		vec = new Rect(0f, 0f, 0f, 0f);
		string[] array = s.Split(new string[1] { "," }, StringSplitOptions.None);
		if (array.Length == 0)
		{
			return false;
		}
		if (array.Length == 1)
		{
			string s2 = array[0];
			float result = 0f;
			if (!float.TryParse(s2, floatStyle, NumberFormatInfo.CurrentInfo, out result))
			{
				return false;
			}
			vec.x = result;
			return true;
		}
		if (array.Length == 2)
		{
			string s3 = array[0];
			float result2 = 0f;
			if (!float.TryParse(s3, floatStyle, NumberFormatInfo.CurrentInfo, out result2))
			{
				return false;
			}
			vec.x = result2;
			string s4 = array[1];
			float result3 = 0f;
			if (!float.TryParse(s4, floatStyle, NumberFormatInfo.CurrentInfo, out result3))
			{
				return false;
			}
			vec.y = result3;
			return true;
		}
		if (array.Length == 3)
		{
			string s5 = array[0];
			float result4 = 0f;
			if (!float.TryParse(s5, floatStyle, NumberFormatInfo.CurrentInfo, out result4))
			{
				return false;
			}
			vec.x = result4;
			string s6 = array[1];
			float result5 = 0f;
			if (!float.TryParse(s6, floatStyle, NumberFormatInfo.CurrentInfo, out result5))
			{
				return false;
			}
			vec.y = result5;
			string s7 = array[2];
			float result6 = 0f;
			if (!float.TryParse(s7, floatStyle, NumberFormatInfo.CurrentInfo, out result6))
			{
				return false;
			}
			vec.width = result6;
			return true;
		}
		if (array.Length == 4)
		{
			string s8 = array[0];
			float result7 = 0f;
			if (!float.TryParse(s8, floatStyle, NumberFormatInfo.CurrentInfo, out result7))
			{
				return false;
			}
			vec.x = result7;
			string s9 = array[1];
			float result8 = 0f;
			if (!float.TryParse(s9, floatStyle, NumberFormatInfo.CurrentInfo, out result8))
			{
				return false;
			}
			vec.y = result8;
			string s10 = array[2];
			float result9 = 0f;
			if (!float.TryParse(s10, floatStyle, NumberFormatInfo.CurrentInfo, out result9))
			{
				return false;
			}
			vec.width = result9;
			string s11 = array[3];
			float result10 = 0f;
			if (!float.TryParse(s11, floatStyle, NumberFormatInfo.CurrentInfo, out result10))
			{
				return false;
			}
			vec.height = result10;
			return true;
		}
		return false;
	}

	public static string ToString_RECT(Rect vec)
	{
		return vec.x.ToString("0.###") + ", " + vec.y.ToString("0.###") + ", " + vec.width.ToString("0.###") + ", " + vec.height.ToString("0.###");
	}

	public static bool TryParse_COLOR(string s, out Color32 vec)
	{
		vec = new Color32(0, 0, 0, 0);
		int length = s.Length;
		if (length != 9 && length != 7)
		{
			return false;
		}
		if (s[0] != '#')
		{
			return false;
		}
		try
		{
			string value = s.Substring(1, length - 1);
			uint num = Convert.ToUInt32(value, 16);
			vec.b = (byte)(num & 0xFF);
			vec.g = (byte)((num >> 8) & 0xFF);
			vec.r = (byte)((num >> 16) & 0xFF);
			vec.a = (byte)((num >> 24) & 0xFF);
			if (length == 7)
			{
				vec.a = byte.MaxValue;
			}
			return true;
		}
		catch
		{
			return false;
		}
	}

	public static string ToString_COLOR(Color32 vec)
	{
		if (vec.a != byte.MaxValue)
		{
			return string.Format("#{0}{1}{2}{3}", vec.a.ToString("X").PadLeft(2, '0'), vec.r.ToString("X").PadLeft(2, '0'), vec.g.ToString("X").PadLeft(2, '0'), vec.b.ToString("X").PadLeft(2, '0'));
		}
		return string.Format("#{0}{1}{2}", vec.r.ToString("X").PadLeft(2, '0'), vec.g.ToString("X").PadLeft(2, '0'), vec.b.ToString("X").PadLeft(2, '0'));
	}

	public static bool TryParse_RANGE(string s, out RANGE range)
	{
		if (s == "0")
		{
			range = RANGE.nowhere;
			return true;
		}
		if (s == "-1")
		{
			range = RANGE.anywhere;
			return true;
		}
		range = RANGE.anywhere;
		string[] array = s.Split(new string[1] { ":" }, StringSplitOptions.None);
		if (array.Length != 3)
		{
			return false;
		}
		array[0] = array[0];
		if (array[0] == "B" || array[0] == "B'")
		{
			range.type = RANGE.RANGETYPE.Box;
			if (array[0].Length == 1)
			{
				range.inverse = false;
			}
			else
			{
				range.inverse = true;
			}
			if (!TryParse_VECTOR(array[1], out range.center))
			{
				return false;
			}
			if (!TryParse_VECTOR(array[2], out range.extend))
			{
				return false;
			}
			return true;
		}
		if (array[0] == "S" || array[0] == "S'")
		{
			range.type = RANGE.RANGETYPE.Sphere;
			if (array[0].Length == 1)
			{
				range.inverse = false;
			}
			else
			{
				range.inverse = true;
			}
			if (!TryParse_VECTOR(array[1], out range.center))
			{
				return false;
			}
			if (!float.TryParse(array[2], floatStyle, NumberFormatInfo.CurrentInfo, out range.radius))
			{
				return false;
			}
			return true;
		}
		if (array[0] == "C" || array[0] == "C'")
		{
			range.type = RANGE.RANGETYPE.Circle;
			if (array[0].Length == 1)
			{
				range.inverse = false;
			}
			else
			{
				range.inverse = true;
			}
			if (!TryParse_VECTOR(array[1], out range.center))
			{
				return false;
			}
			if (!float.TryParse(array[2], floatStyle, NumberFormatInfo.CurrentInfo, out range.radius))
			{
				return false;
			}
			return true;
		}
		return false;
	}

	public static string ToString_RANGE(RANGE range)
	{
		string empty = string.Empty;
		switch (range.type)
		{
		case RANGE.RANGETYPE.Anywhere:
			return (!range.inverse) ? "-1" : "0";
		case RANGE.RANGETYPE.Box:
			empty = ((!range.inverse) ? "B:{0},{1},{2}:{3},{4},{5}" : "B':{0},{1},{2}:{3},{4},{5}");
			return string.Format(empty, range.center.x, range.center.y, range.center.z, range.extend.x, range.extend.y, range.extend.z);
		case RANGE.RANGETYPE.Sphere:
			empty = ((!range.inverse) ? "S:{0},{1},{2}:{3}" : "S':{0},{1},{2}:{3}");
			return string.Format(empty, range.center.x, range.center.y, range.center.z, range.radius);
		case RANGE.RANGETYPE.Circle:
			empty = ((!range.inverse) ? "C:{0},{1},{2}:{3}" : "C':{0},{1},{2}:{3}");
			return string.Format(empty, range.center.x, range.center.y, range.center.z, range.radius);
		default:
			return "0";
		}
	}

	public static bool TryParse_DIRRANGE(string s, out DIRRANGE range)
	{
		if (s == "0")
		{
			range = DIRRANGE.nodir;
			return true;
		}
		if (s == "-1")
		{
			range = DIRRANGE.anydir;
			return true;
		}
		range = DIRRANGE.anydir;
		string[] array = s.Split(new string[1] { ":" }, StringSplitOptions.None);
		if (array.Length != 3)
		{
			return false;
		}
		array[0] = array[0];
		if (array[0] == "C" || array[0] == "C'")
		{
			range.type = DIRRANGE.DIRRANGETYPE.Cone;
			if (array[0].Length == 1)
			{
				range.inverse = false;
			}
			else
			{
				range.inverse = true;
			}
			Vector3 vec = Vector3.zero;
			if (!TryParse_VECTOR(array[1], out range.directrix))
			{
				return false;
			}
			if (!TryParse_VECTOR(array[2], out vec))
			{
				return false;
			}
			range.error.x = vec.x;
			range.error.y = vec.y;
			return true;
		}
		if (array[0] == "F" || array[0] == "F'")
		{
			range.type = DIRRANGE.DIRRANGETYPE.Fan;
			if (array[0].Length == 1)
			{
				range.inverse = false;
			}
			else
			{
				range.inverse = true;
			}
			Vector3 vec2 = Vector3.zero;
			if (!TryParse_VECTOR(array[1], out range.directrix))
			{
				return false;
			}
			if (!TryParse_VECTOR(array[2], out vec2))
			{
				return false;
			}
			range.error.x = vec2.x;
			range.error.y = vec2.y;
			return true;
		}
		return false;
	}

	public static string ToString_DIRRANGE(DIRRANGE range)
	{
		string empty = string.Empty;
		switch (range.type)
		{
		case DIRRANGE.DIRRANGETYPE.Anydirection:
			return (!range.inverse) ? "-1" : "0";
		case DIRRANGE.DIRRANGETYPE.Cone:
			empty = ((!range.inverse) ? "C:{0},{1},{2}:{3}" : "C':{0},{1},{2}:{3}");
			return string.Format(empty, range.directrix.x, range.directrix.y, range.directrix.z, range.error.x);
		case DIRRANGE.DIRRANGETYPE.Fan:
			empty = ((!range.inverse) ? "F:{0},{1},{2}:{3},{4}" : "F':{0},{1},{2}:{3},{4}");
			return string.Format(empty, range.directrix.x, range.directrix.y, range.directrix.z, range.error.x, range.error.y);
		default:
			return "0";
		}
	}

	public static bool TryParse_OBJECT(string s, out OBJECT obj)
	{
		obj = default(OBJECT);
		if (s == "-1")
		{
			obj.type = OBJECT.OBJECTTYPE.AnyObject;
			obj.Group = -1;
			obj.Id = -1;
			return true;
		}
		string[] array = s.Split(new string[1] { "/" }, StringSplitOptions.None);
		if (array.Length == 3)
		{
			if (array[0] == "P:")
			{
				obj.type = OBJECT.OBJECTTYPE.Player;
			}
			else if (array[0] == "W:")
			{
				obj.type = OBJECT.OBJECTTYPE.WorldObject;
			}
			else if (array[0] == "I:")
			{
				obj.type = OBJECT.OBJECTTYPE.ItemProto;
			}
			else
			{
				if (!(array[0] == "M:"))
				{
					return false;
				}
				obj.type = OBJECT.OBJECTTYPE.MonsterProto;
			}
			if (!int.TryParse(array[1], out obj.Group))
			{
				return false;
			}
			if (!int.TryParse(array[2], out obj.Id))
			{
				return false;
			}
			if (obj.type == OBJECT.OBJECTTYPE.Player)
			{
				if (obj.Id == 0 && obj.Group == -1)
				{
					return true;
				}
				if (obj.Id == -1 && obj.Group == -1)
				{
					return true;
				}
				if (obj.Id == -2 && obj.Group == -1)
				{
					return true;
				}
				if (obj.Id == -1 && obj.Group == -2)
				{
					return true;
				}
				if (obj.Id == -1 && obj.Group == -3)
				{
					return true;
				}
				if (obj.Id == -1 && obj.Group == -4)
				{
					return true;
				}
				if (obj.Id >= 0 && obj.Group == 0)
				{
					return true;
				}
				if (obj.Id == -1 && obj.Group >= 0)
				{
					return true;
				}
				return false;
			}
			if (obj.type == OBJECT.OBJECTTYPE.WorldObject)
			{
				if (obj.Group == -1 && obj.Id == -1)
				{
					return true;
				}
				if (obj.Group >= 0 && obj.Id == -1)
				{
					return true;
				}
				if (obj.Group >= 0 && obj.Id > 0)
				{
					return true;
				}
				return false;
			}
			if (obj.type == OBJECT.OBJECTTYPE.ItemProto || obj.type == OBJECT.OBJECTTYPE.MonsterProto)
			{
				if (obj.Group == -1 && obj.Id == -1)
				{
					return true;
				}
				if (obj.Group >= 0 && obj.Id == -1)
				{
					return true;
				}
				if (obj.Id > 0)
				{
					return true;
				}
				return false;
			}
			return true;
		}
		return false;
	}

	public static string ToString_OBJECT(OBJECT obj)
	{
		if (obj.type == OBJECT.OBJECTTYPE.AnyObject)
		{
			return "-1";
		}
		string empty = string.Empty;
		if (obj.type == OBJECT.OBJECTTYPE.Player)
		{
			empty = "P:/";
		}
		else if (obj.type == OBJECT.OBJECTTYPE.WorldObject)
		{
			empty = "W:/";
		}
		else if (obj.type == OBJECT.OBJECTTYPE.ItemProto)
		{
			empty = "I:/";
		}
		else
		{
			if (obj.type != OBJECT.OBJECTTYPE.MonsterProto)
			{
				return string.Empty;
			}
			empty = "M:/";
		}
		return empty + obj.Group + "/" + obj.Id;
	}
}

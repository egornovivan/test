using System;
using System.Xml;
using UnityEngine;

public static class XMLIO
{
	private static Type BoolType = typeof(bool);

	private static Type LongType = typeof(long);

	private static Type IntType = typeof(int);

	private static Type ShortType = typeof(short);

	private static Type SByteType = typeof(sbyte);

	private static Type ULongType = typeof(ulong);

	private static Type UIntType = typeof(uint);

	private static Type UShortType = typeof(ushort);

	private static Type ByteType = typeof(byte);

	private static Type SingleType = typeof(float);

	private static Type DoubleType = typeof(double);

	private static Type StringType = typeof(string);

	private static Type Vector2Type = typeof(Vector2);

	private static Type Vector3Type = typeof(Vector3);

	private static Type Vector4Type = typeof(Vector4);

	private static Type QuaternionType = typeof(Quaternion);

	private static Type Color32Type = typeof(Color32);

	private static Type ColorType = typeof(Color);

	public static string WriteValue(string attr, object value, Type value_type, bool necessary, object default_value)
	{
		if (value == null)
		{
			Debug.LogError("XMLIO::WriteValue (value is null)");
			return string.Empty;
		}
		if (!necessary && value.Equals(default_value))
		{
			return string.Empty;
		}
		if (value_type == IntType || value_type == SingleType || value_type == BoolType)
		{
			return $"{attr}=\"{value}\" ";
		}
		if (value_type == StringType)
		{
			return $"{attr}=\"{Uri.EscapeDataString((string)value)}\" ";
		}
		if (value_type == Vector3Type)
		{
			Vector3 vector = (Vector3)value;
			if (!necessary && default_value != null && default_value is float num && vector.x == num && vector.y == num && vector.z == num)
			{
				return string.Empty;
			}
			return string.Format("{0}x=\"{1}\" {0}y=\"{2}\" {0}z=\"{3}\" ", attr, vector.x, vector.y, vector.z);
		}
		if (value_type == QuaternionType)
		{
			Quaternion quaternion = (Quaternion)value;
			if (quaternion == Quaternion.identity)
			{
				return string.Empty;
			}
			return string.Format("{0}x=\"{1}\" {0}y=\"{2}\" {0}z=\"{3}\" {0}w=\"{4}\" ", attr, quaternion.x, quaternion.y, quaternion.z, quaternion.w);
		}
		if (value_type.IsEnum)
		{
			return $"{attr}=\"{value.ToString()}\" ";
		}
		if (value_type == Color32Type)
		{
			Color32 color = (Color32)value;
			return string.Format("{0}=\"{1}{2}{3}{4}\" ", attr, color.a.ToString("X").PadLeft(2, '0'), color.r.ToString("X").PadLeft(2, '0'), color.g.ToString("X").PadLeft(2, '0'), color.b.ToString("X").PadLeft(2, '0'));
		}
		if (value_type == ColorType)
		{
			Color32 color2 = (Color)value;
			return string.Format("{0}=\"{1}{2}{3}{4}\" ", attr, color2.a.ToString("X").PadLeft(2, '0'), color2.r.ToString("X").PadLeft(2, '0'), color2.g.ToString("X").PadLeft(2, '0'), color2.b.ToString("X").PadLeft(2, '0'));
		}
		if (value_type == Vector2Type)
		{
			Vector2 vector2 = (Vector2)value;
			if (!necessary && default_value != null && default_value is float num2 && vector2.x == num2 && vector2.y == num2)
			{
				return string.Empty;
			}
			return string.Format("{0}x=\"{1}\" {0}y=\"{2}\" ", attr, vector2.x, vector2.y);
		}
		if (value_type == Vector4Type)
		{
			Vector4 vector3 = (Vector4)value;
			if (!necessary && default_value != null && default_value is float num3 && vector3.x == num3 && vector3.y == num3 && vector3.z == num3 && vector3.w == num3)
			{
				return string.Empty;
			}
			return string.Format("{0}x=\"{1}\" {0}y=\"{2}\" {0}z=\"{3}\" {0}w=\"{4}\" ", attr, vector3.x, vector3.y, vector3.z, vector3.w);
		}
		if (value_type == LongType || value_type == ULongType || value_type == DoubleType || value_type == UIntType || value_type == ShortType || value_type == UShortType || value_type == ByteType || value_type == SByteType)
		{
			return $"{attr}=\"{value}\" ";
		}
		Debug.LogWarning("Type '" + value_type.Name + "' cannot convert to xml");
		return string.Empty;
	}

	public static object ReadValue(XmlElement xml, string attr, Type value_type, bool necessary, object default_value)
	{
		if (value_type == IntType)
		{
			try
			{
				if (xml.HasAttribute(attr))
				{
					return XmlConvert.ToInt32(xml.Attributes[attr].Value);
				}
			}
			catch
			{
				Debug.LogWarning("XMLIO: attribute '" + attr + "' reading error");
			}
			if (necessary)
			{
				throw new Exception("XMLIO: attribute '" + attr + "' is necessary");
			}
			if (default_value == null)
			{
				return 0;
			}
			if (!(default_value is int))
			{
				return 0;
			}
			return (int)default_value;
		}
		if (value_type == SingleType)
		{
			try
			{
				if (xml.HasAttribute(attr))
				{
					return XmlConvert.ToSingle(xml.Attributes[attr].Value);
				}
			}
			catch
			{
				Debug.LogWarning("XMLIO: attribute '" + attr + "' reading error");
			}
			if (necessary)
			{
				throw new Exception("XMLIO: attribute '" + attr + "' is necessary");
			}
			if (default_value != null)
			{
				if (default_value is int)
				{
					return (float)(int)default_value;
				}
				if (default_value is float)
				{
					return (float)default_value;
				}
				if (default_value is double)
				{
					return (float)(double)default_value;
				}
			}
			return 0f;
		}
		if (value_type == BoolType)
		{
			try
			{
				if (xml.HasAttribute(attr))
				{
					return xml.Attributes[attr].Value.ToLower() == "true";
				}
			}
			catch
			{
				Debug.LogWarning("XMLIO: attribute '" + attr + "' reading error");
			}
			if (necessary)
			{
				throw new Exception("XMLIO: attribute '" + attr + "' is necessary");
			}
			if (default_value == null)
			{
				return null;
			}
			if (!(default_value is bool))
			{
				return false;
			}
			return (bool)default_value;
		}
		if (value_type == StringType)
		{
			try
			{
				if (xml.HasAttribute(attr))
				{
					return Uri.UnescapeDataString(xml.Attributes[attr].Value);
				}
			}
			catch
			{
				Debug.LogWarning("XMLIO: attribute '" + attr + "' reading error");
			}
			if (necessary)
			{
				throw new Exception("XMLIO: attribute '" + attr + "' is necessary");
			}
			if (default_value == null)
			{
				return string.Empty;
			}
			if (!(default_value is string))
			{
				return string.Empty;
			}
			return (string)default_value;
		}
		if (value_type == Vector3Type)
		{
			float x = 0f;
			float y = 0f;
			float z = 0f;
			bool flag = false;
			bool flag2 = false;
			bool flag3 = false;
			try
			{
				if (xml.HasAttribute(attr + "x"))
				{
					x = XmlConvert.ToSingle(xml.Attributes[attr + "x"].Value);
					flag = true;
				}
				else if (necessary)
				{
					throw new Exception("XMLIO: attribute '" + attr + "' is necessary");
				}
			}
			catch
			{
				Debug.LogWarning("XMLIO: attribute '" + attr + "x' reading error");
			}
			try
			{
				if (xml.HasAttribute(attr + "y"))
				{
					y = XmlConvert.ToSingle(xml.Attributes[attr + "y"].Value);
					flag2 = true;
				}
				else if (necessary)
				{
					throw new Exception("XMLIO: attribute '" + attr + "' is necessary");
				}
			}
			catch
			{
				Debug.LogWarning("XMLIO: attribute '" + attr + "y' reading error");
			}
			try
			{
				if (xml.HasAttribute(attr + "z"))
				{
					z = XmlConvert.ToSingle(xml.Attributes[attr + "z"].Value);
					flag3 = true;
				}
				else if (necessary)
				{
					throw new Exception("XMLIO: attribute '" + attr + "' is necessary");
				}
			}
			catch
			{
				Debug.LogWarning("XMLIO: attribute '" + attr + "z' reading error");
			}
			if (flag && flag2 && flag3)
			{
				return new Vector3(x, y, z);
			}
			float num = 0f;
			if (default_value != null)
			{
				if (default_value is int)
				{
					num = (int)default_value;
				}
				if (default_value is float)
				{
					num = (float)default_value;
				}
				if (default_value is double)
				{
					num = (float)(double)default_value;
				}
			}
			if (!flag)
			{
				x = num;
			}
			if (!flag2)
			{
				y = num;
			}
			if (!flag3)
			{
				z = num;
			}
			return new Vector3(x, y, z);
		}
		if (value_type == QuaternionType)
		{
			float num2 = 0f;
			float num3 = 0f;
			float num4 = 0f;
			float num5 = 0f;
			try
			{
				if (!xml.HasAttribute(attr + "x"))
				{
					return Quaternion.identity;
				}
				num2 = XmlConvert.ToSingle(xml.Attributes[attr + "x"].Value);
			}
			catch
			{
				Debug.LogWarning("XMLIO: attribute '" + attr + "x' reading error");
				return Quaternion.identity;
			}
			try
			{
				if (!xml.HasAttribute(attr + "y"))
				{
					return Quaternion.identity;
				}
				num3 = XmlConvert.ToSingle(xml.Attributes[attr + "y"].Value);
			}
			catch
			{
				Debug.LogWarning("XMLIO: attribute '" + attr + "y' reading error");
				return Quaternion.identity;
			}
			try
			{
				if (!xml.HasAttribute(attr + "z"))
				{
					return Quaternion.identity;
				}
				num4 = XmlConvert.ToSingle(xml.Attributes[attr + "z"].Value);
			}
			catch
			{
				Debug.LogWarning("XMLIO: attribute '" + attr + "z' reading error");
				return Quaternion.identity;
			}
			try
			{
				if (!xml.HasAttribute(attr + "w"))
				{
					return Quaternion.identity;
				}
				num5 = XmlConvert.ToSingle(xml.Attributes[attr + "w"].Value);
			}
			catch
			{
				Debug.LogWarning("XMLIO: attribute '" + attr + "w' reading error");
				return Quaternion.identity;
			}
			Vector4 vector = new Vector4(num2, num3, num4, num5);
			if (vector.magnitude < 0.001f)
			{
				return Quaternion.identity;
			}
			vector.Normalize();
			Quaternion quaternion = new Quaternion(vector.x, vector.y, vector.z, vector.w);
			return quaternion;
		}
		if (value_type.IsEnum)
		{
			try
			{
				if (xml.HasAttribute(attr))
				{
					return Enum.Parse(value_type, xml.Attributes[attr].Value);
				}
			}
			catch
			{
				Debug.LogWarning("XMLIO: attribute '" + attr + "' reading error");
			}
			if (necessary)
			{
				throw new Exception("XMLIO: attribute '" + attr + "' is necessary");
			}
			if (default_value == null)
			{
				if (default_value is string)
				{
					return Enum.Parse(value_type, (string)default_value);
				}
				if (default_value.GetType() == value_type)
				{
					return default_value;
				}
			}
			return Enum.ToObject(value_type, 0);
		}
		if (value_type == Color32Type)
		{
			string str = "FFFFFFFF";
			try
			{
				if (xml.HasAttribute(attr))
				{
					str = xml.Attributes[attr].Value;
					return StringToColor32(str);
				}
			}
			catch
			{
				Debug.LogWarning("XMLIO: attribute '" + attr + "' reading error");
			}
			if (necessary)
			{
				throw new Exception("XMLIO: attribute '" + attr + "' is necessary");
			}
			if (default_value != null && default_value is string)
			{
				str = (string)default_value;
			}
			try
			{
				return StringToColor32(str);
			}
			catch
			{
				return new Color32(byte.MaxValue, byte.MaxValue, byte.MaxValue, byte.MaxValue);
			}
		}
		if (value_type == ColorType)
		{
			string str2 = "FFFFFFFF";
			try
			{
				if (xml.HasAttribute(attr))
				{
					str2 = xml.Attributes[attr].Value;
					return (Color)StringToColor32(str2);
				}
			}
			catch
			{
				Debug.LogWarning("XMLIO: attribute '" + attr + "' reading error");
			}
			if (necessary)
			{
				throw new Exception("XMLIO: attribute '" + attr + "' is necessary");
			}
			if (default_value != null && default_value is string)
			{
				str2 = (string)default_value;
			}
			try
			{
				return (Color)StringToColor32(str2);
			}
			catch
			{
				return Color.white;
			}
		}
		if (value_type == Vector2Type)
		{
			float x2 = 0f;
			float y2 = 0f;
			bool flag4 = false;
			bool flag5 = false;
			try
			{
				if (xml.HasAttribute(attr + "x"))
				{
					x2 = XmlConvert.ToSingle(xml.Attributes[attr + "x"].Value);
					flag4 = true;
				}
				else if (necessary)
				{
					throw new Exception("XMLIO: attribute '" + attr + "' is necessary");
				}
			}
			catch
			{
				Debug.LogWarning("XMLIO: attribute '" + attr + "x' reading error");
			}
			try
			{
				if (xml.HasAttribute(attr + "y"))
				{
					y2 = XmlConvert.ToSingle(xml.Attributes[attr + "y"].Value);
					flag5 = true;
				}
				else if (necessary)
				{
					throw new Exception("XMLIO: attribute '" + attr + "' is necessary");
				}
			}
			catch
			{
				Debug.LogWarning("XMLIO: attribute '" + attr + "y' reading error");
			}
			if (flag4 && flag5)
			{
				return new Vector2(x2, y2);
			}
			float num6 = 0f;
			if (default_value != null)
			{
				if (default_value is int)
				{
					num6 = (int)default_value;
				}
				if (default_value is float)
				{
					num6 = (float)default_value;
				}
				if (default_value is double)
				{
					num6 = (float)(double)default_value;
				}
			}
			if (!flag4)
			{
				x2 = num6;
			}
			if (!flag5)
			{
				y2 = num6;
			}
			return new Vector2(x2, y2);
		}
		if (value_type == Vector4Type)
		{
			float x3 = 0f;
			float y3 = 0f;
			float z2 = 0f;
			float w = 0f;
			bool flag6 = false;
			bool flag7 = false;
			bool flag8 = false;
			bool flag9 = false;
			try
			{
				if (xml.HasAttribute(attr + "x"))
				{
					x3 = XmlConvert.ToSingle(xml.Attributes[attr + "x"].Value);
					flag6 = true;
				}
				else if (necessary)
				{
					throw new Exception("XMLIO: attribute '" + attr + "' is necessary");
				}
			}
			catch
			{
				Debug.LogWarning("XMLIO: attribute '" + attr + "x' reading error");
			}
			try
			{
				if (xml.HasAttribute(attr + "y"))
				{
					y3 = XmlConvert.ToSingle(xml.Attributes[attr + "y"].Value);
					flag7 = true;
				}
				else if (necessary)
				{
					throw new Exception("XMLIO: attribute '" + attr + "' is necessary");
				}
			}
			catch
			{
				Debug.LogWarning("XMLIO: attribute '" + attr + "y' reading error");
			}
			try
			{
				if (xml.HasAttribute(attr + "z"))
				{
					z2 = XmlConvert.ToSingle(xml.Attributes[attr + "z"].Value);
					flag8 = true;
				}
				else if (necessary)
				{
					throw new Exception("XMLIO: attribute '" + attr + "' is necessary");
				}
			}
			catch
			{
				Debug.LogWarning("XMLIO: attribute '" + attr + "z' reading error");
			}
			try
			{
				if (xml.HasAttribute(attr + "w"))
				{
					w = XmlConvert.ToSingle(xml.Attributes[attr + "w"].Value);
					flag9 = true;
				}
				else if (necessary)
				{
					throw new Exception("XMLIO: attribute '" + attr + "' is necessary");
				}
			}
			catch
			{
				Debug.LogWarning("XMLIO: attribute '" + attr + "w' reading error");
			}
			if (flag6 && flag7 && flag8 && flag9)
			{
				return new Vector4(x3, y3, z2, w);
			}
			float num7 = 0f;
			if (default_value != null)
			{
				if (default_value is int)
				{
					num7 = (int)default_value;
				}
				if (default_value is float)
				{
					num7 = (float)default_value;
				}
				if (default_value is double)
				{
					num7 = (float)(double)default_value;
				}
			}
			if (!flag6)
			{
				x3 = num7;
			}
			if (!flag7)
			{
				y3 = num7;
			}
			if (!flag8)
			{
				z2 = num7;
			}
			if (!flag9)
			{
				w = num7;
			}
			return new Vector4(x3, y3, z2, w);
		}
		if (value_type == DoubleType)
		{
			try
			{
				if (xml.HasAttribute(attr))
				{
					return XmlConvert.ToDouble(xml.Attributes[attr].Value);
				}
			}
			catch
			{
				Debug.LogWarning("XMLIO: attribute '" + attr + "' reading error");
			}
			if (necessary)
			{
				throw new Exception("XMLIO: attribute '" + attr + "' is necessary");
			}
			if (default_value != null)
			{
				if (default_value is int)
				{
					return (double)(int)default_value;
				}
				if (default_value is float)
				{
					return (double)default_value;
				}
				if (default_value is double)
				{
					return (double)default_value;
				}
			}
			return 0.0;
		}
		if (value_type == LongType || value_type == ULongType || value_type == UIntType || value_type == ShortType || value_type == UShortType || value_type == ByteType || value_type == SByteType)
		{
			ulong num8 = 0uL;
			bool flag10 = false;
			try
			{
				if (xml.HasAttribute(attr))
				{
					num8 = XmlConvert.ToUInt64(xml.Attributes[attr].Value);
					flag10 = true;
				}
			}
			catch
			{
				Debug.LogWarning("XMLIO: attribute '" + attr + "' reading error");
			}
			if (!flag10)
			{
				if (necessary)
				{
					throw new Exception("XMLIO: attribute '" + attr + "' is necessary");
				}
				if (default_value != null)
				{
					if (default_value is ulong)
					{
						num8 = (ulong)default_value;
					}
					if (default_value is long)
					{
						num8 = (ulong)(long)default_value;
					}
					if (default_value is int)
					{
						num8 = (ulong)(int)default_value;
					}
					if (default_value is uint)
					{
						num8 = (uint)default_value;
					}
				}
			}
			if (value_type == LongType)
			{
				return (long)num8;
			}
			if (value_type == ULongType)
			{
				return num8;
			}
			if (value_type == UIntType)
			{
				return (uint)num8;
			}
			if (value_type == ShortType)
			{
				return (short)num8;
			}
			if (value_type == UShortType)
			{
				return (ushort)num8;
			}
			if (value_type == ByteType)
			{
				return (byte)num8;
			}
			return (sbyte)num8;
		}
		if (necessary)
		{
			throw new Exception("XMLIO: attribute '" + attr + "' is necessary and not readable");
		}
		Debug.Log("XMLIO: attribute '" + attr + "' is not readable");
		return null;
	}

	private static Color32 StringToColor32(string str)
	{
		long num = Convert.ToInt64(str, 16);
		Color32 result = default(Color32);
		result.b = (byte)(num & 0xFF);
		num >>= 8;
		result.g = (byte)(num & 0xFF);
		num >>= 8;
		result.r = (byte)(num & 0xFF);
		num >>= 8;
		result.a = (byte)(num & 0xFF);
		return result;
	}
}

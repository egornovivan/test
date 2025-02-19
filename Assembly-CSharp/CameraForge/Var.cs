using System;
using System.Globalization;
using System.IO;
using UnityEngine;

namespace CameraForge;

public class Var
{
	public EVarType type;

	public bool value_b;

	public int value_i;

	public float value_f;

	public Vector4 value_v;

	public Quaternion value_q;

	private string _value_str;

	public static Var Null
	{
		get
		{
			Var var = new Var();
			var.type = EVarType.Null;
			return var;
		}
	}

	public bool isNull => type == EVarType.Null;

	public Color value_c
	{
		get
		{
			return new Color(value_v.x, value_v.y, value_v.z, value_v.w);
		}
		set
		{
			value_v = new Vector4(value.r, value.g, value.b, value.a);
		}
	}

	public string value_str
	{
		get
		{
			if (type != EVarType.String)
			{
				return ToEditString(editing: false);
			}
			return _value_str;
		}
		set
		{
			_value_str = value;
		}
	}

	public override string ToString()
	{
		if (isNull)
		{
			return "{null}";
		}
		return $"{value_str} [{type.ToString()}]";
	}

	public string ToShortString()
	{
		if (isNull)
		{
			return "{null}";
		}
		return $"{value_str}";
	}

	public string ToEditString(bool editing)
	{
		if (type == EVarType.Null)
		{
			if (editing)
			{
				return string.Empty;
			}
			return "?";
		}
		if (type == EVarType.Bool)
		{
			return value_b.ToString();
		}
		if (type == EVarType.Int)
		{
			return value_i.ToString();
		}
		if (type == EVarType.Float)
		{
			return value_f.ToString();
		}
		if (type == EVarType.Vector)
		{
			string text = string.Empty;
			if (!editing)
			{
				text = "0.##";
			}
			string text2 = value_v.x.ToString(text) + "," + value_v.y.ToString(text) + "," + value_v.z.ToString(text);
			if (value_v.w != 0f)
			{
				text2 = text2 + "," + value_v.w.ToString(text);
			}
			return text2;
		}
		if (type == EVarType.Quaternion)
		{
			string text3 = string.Empty;
			if (!editing)
			{
				text3 = "0.##";
			}
			Vector3 eulerAngles = value_q.eulerAngles;
			return "q:" + eulerAngles.x.ToString(text3) + "," + eulerAngles.y.ToString(text3) + "," + eulerAngles.z.ToString(text3);
		}
		if (type == EVarType.Color)
		{
			string text4 = string.Empty;
			if (!editing)
			{
				text4 = "0.##";
			}
			return value_c.r.ToString(text4) + "," + value_c.g.ToString(text4) + "," + value_c.b.ToString(text4) + "," + value_c.a.ToString(text4);
		}
		if (type == EVarType.String)
		{
			if (editing)
			{
				return "'" + _value_str;
			}
			return "\"" + _value_str + "\"";
		}
		return "?";
	}

	public static Var Parse(string str)
	{
		str = str.Trim();
		if (string.IsNullOrEmpty(str))
		{
			return 0f;
		}
		if (str[0] == '\'')
		{
			return str.Substring(1, str.Length - 1);
		}
		if (str.Length >= 2 && str.Substring(0, 2) == "q:")
		{
			Vector3 zero = Vector3.zero;
			string text = str.Substring(2, str.Length - 2);
			string[] array = text.Split(new string[1] { "," }, StringSplitOptions.None);
			for (int i = 0; i < array.Length && i < 3; i++)
			{
				float result = 0f;
				float.TryParse(array[i].Trim(), out result);
				zero[i] = result;
			}
			return Quaternion.Euler(zero);
		}
		if (bool.TryParse(str, out var result2))
		{
			return result2;
		}
		if (float.TryParse(str, NumberStyles.Float, NumberFormatInfo.CurrentInfo, out var result3))
		{
			return result3;
		}
		Vector4 vector = Vector3.zero;
		string[] array2 = str.Split(new string[1] { "," }, StringSplitOptions.None);
		for (int j = 0; j < array2.Length && j < 4; j++)
		{
			float result4 = 0f;
			if (!float.TryParse(array2[j].Trim(), out result4))
			{
				return Null;
			}
			vector[j] = result4;
		}
		return vector;
	}

	internal void Write(BinaryWriter w)
	{
		if (isNull)
		{
			w.Write(0);
			return;
		}
		w.Write((int)type);
		w.Write(value_b);
		w.Write(value_i);
		w.Write(value_i);
		w.Write(value_f);
		w.Write(value_v.x);
		w.Write(value_v.y);
		w.Write(value_v.z);
		w.Write(value_v.w);
		w.Write(value_q.x);
		w.Write(value_q.y);
		w.Write(value_q.z);
		w.Write(value_q.w);
		w.Write(value_v.x);
		w.Write(value_v.y);
		w.Write(value_v.z);
		w.Write(value_v.w);
		if (_value_str == null)
		{
			_value_str = string.Empty;
		}
		w.Write(_value_str);
	}

	internal void Read(BinaryReader r)
	{
		type = (EVarType)r.ReadInt32();
		if (!isNull)
		{
			value_b = r.ReadBoolean();
			value_i = r.ReadInt32();
			value_i = r.ReadInt32();
			value_f = r.ReadSingle();
			value_v.x = r.ReadSingle();
			value_v.y = r.ReadSingle();
			value_v.z = r.ReadSingle();
			value_v.w = r.ReadSingle();
			value_q.x = r.ReadSingle();
			value_q.y = r.ReadSingle();
			value_q.z = r.ReadSingle();
			value_q.w = r.ReadSingle();
			r.ReadSingle();
			r.ReadSingle();
			r.ReadSingle();
			r.ReadSingle();
			_value_str = r.ReadString();
		}
	}

	public static implicit operator Var(bool value)
	{
		Var var = new Var();
		var.type = EVarType.Bool;
		var.value_b = value;
		var.value_f = (var.value_i = (value ? 1 : 0));
		var.value_v = Vector4.one * var.value_f;
		var._value_str = string.Empty;
		return var;
	}

	public static implicit operator Var(int value)
	{
		Var var = new Var();
		var.type = EVarType.Int;
		var.value_b = value != 0;
		var.value_f = (var.value_i = value);
		var.value_v = Vector4.one * var.value_f;
		var._value_str = string.Empty;
		return var;
	}

	public static implicit operator Var(float value)
	{
		Var var = new Var();
		var.type = EVarType.Float;
		var.value_b = value != 0f;
		var.value_i = (int)value;
		var.value_f = value;
		var.value_v = Vector4.one * var.value_f;
		var._value_str = string.Empty;
		return var;
	}

	public static implicit operator Var(double value)
	{
		Var var = new Var();
		var.type = EVarType.Float;
		var.value_b = value != 0.0;
		var.value_i = (int)value;
		var.value_f = (float)value;
		var.value_v = Vector4.one * var.value_f;
		var._value_str = string.Empty;
		return var;
	}

	public static implicit operator Var(Vector2 value)
	{
		Var var = new Var();
		var.type = EVarType.Vector;
		var.value_b = value != Vector2.zero;
		var.value_i = (int)value.x;
		var.value_f = value.x;
		var.value_v = new Vector4(value.x, value.y, 0f, 0f);
		var.value_q = Quaternion.Euler(new Vector3(value.x, value.y, 0f));
		var._value_str = string.Empty;
		return var;
	}

	public static implicit operator Var(Vector3 value)
	{
		Var var = new Var();
		var.type = EVarType.Vector;
		var.value_b = value != Vector3.zero;
		var.value_i = (int)value.x;
		var.value_f = value.x;
		var.value_v = new Vector4(value.x, value.y, value.z, 0f);
		var.value_q = Quaternion.Euler(new Vector3(value.x, value.y, value.z));
		var._value_str = string.Empty;
		return var;
	}

	public static implicit operator Var(Vector4 value)
	{
		Var var = new Var();
		var.type = EVarType.Vector;
		var.value_b = value != Vector4.zero;
		var.value_i = (int)value.x;
		var.value_f = value.x;
		var.value_v = value;
		var.value_q = Quaternion.Euler(new Vector3(value.x, value.y, value.z));
		var._value_str = string.Empty;
		return var;
	}

	public static implicit operator Var(Quaternion value)
	{
		Var var = new Var();
		var.type = EVarType.Quaternion;
		var.value_b = value != Quaternion.identity;
		var.value_i = (int)value.w;
		var.value_f = value.w;
		Vector3 eulerAngles = value.eulerAngles;
		var.value_v = new Vector4(eulerAngles.x, eulerAngles.y, eulerAngles.z, 0f);
		var.value_q = value;
		var._value_str = string.Empty;
		return var;
	}

	public static implicit operator Var(Color value)
	{
		Var var = new Var();
		var.type = EVarType.Color;
		var.value_b = value != Color.clear;
		var.value_i = (int)value.r;
		var.value_f = value.r;
		var.value_v = new Vector4(value.r, value.g, value.b, value.a);
		var.value_q = Quaternion.identity;
		var._value_str = string.Empty;
		return var;
	}

	public static implicit operator Var(string value)
	{
		Var var = new Var();
		var.type = EVarType.String;
		var.value_b = value.ToLower() != "true";
		float.TryParse(value, out var.value_f);
		var.value_i = (int)var.value_f;
		var.value_v = Vector4.one * var.value_f;
		var.value_q = Quaternion.identity;
		var._value_str = value;
		return var;
	}
}

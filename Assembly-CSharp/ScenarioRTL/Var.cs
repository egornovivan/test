using System;
using System.Globalization;

namespace ScenarioRTL;

[Serializable]
public struct Var
{
	public string data;

	public static Var zero => new Var(string.Empty);

	public bool isNull => string.IsNullOrEmpty(data);

	public bool isString
	{
		get
		{
			if (string.IsNullOrEmpty(data))
			{
				return true;
			}
			if (data.Length < 2)
			{
				return false;
			}
			if (data[0] == '\'' && data[data.Length - 1] == '\'')
			{
				return true;
			}
			return false;
		}
	}

	public bool isBool
	{
		get
		{
			if (string.IsNullOrEmpty(data))
			{
				return true;
			}
			if (data == "true")
			{
				return true;
			}
			if (data == "false")
			{
				return true;
			}
			return false;
		}
	}

	public bool isNumber
	{
		get
		{
			if (string.IsNullOrEmpty(data))
			{
				return true;
			}
			if (data[0] == '\'')
			{
				return false;
			}
			double result = 0.0;
			if (double.TryParse(data, NumberStyles.Float, NumberFormatInfo.CurrentInfo, out result))
			{
				return true;
			}
			return false;
		}
	}

	public bool isInteger
	{
		get
		{
			if (string.IsNullOrEmpty(data))
			{
				return true;
			}
			if (data[0] == '\'')
			{
				return false;
			}
			int result = 0;
			if (int.TryParse(data, out result))
			{
				return true;
			}
			return false;
		}
	}

	public Var(string _data)
	{
		data = _data;
	}

	private static double ToDouble(string s)
	{
		if (string.IsNullOrEmpty(s))
		{
			return 0.0;
		}
		if (s.Trim() == string.Empty)
		{
			return 0.0;
		}
		if (s[0] == '\'')
		{
			return 0.0;
		}
		double result = 0.0;
		if (double.TryParse(s, NumberStyles.Float, NumberFormatInfo.CurrentInfo, out result))
		{
			return result;
		}
		return 0.0;
	}

	public override string ToString()
	{
		return data;
	}

	public static implicit operator Var(bool val)
	{
		return new Var((!val) ? "false" : "true");
	}

	public static implicit operator Var(long val)
	{
		return new Var(val.ToString());
	}

	public static implicit operator Var(int val)
	{
		return new Var(val.ToString());
	}

	public static implicit operator Var(short val)
	{
		return new Var(val.ToString());
	}

	public static implicit operator Var(sbyte val)
	{
		return new Var(val.ToString());
	}

	public static implicit operator Var(ulong val)
	{
		return new Var(val.ToString());
	}

	public static implicit operator Var(uint val)
	{
		return new Var(val.ToString());
	}

	public static implicit operator Var(ushort val)
	{
		return new Var(val.ToString());
	}

	public static implicit operator Var(byte val)
	{
		return new Var(val.ToString());
	}

	public static implicit operator Var(float val)
	{
		return new Var(val.ToString());
	}

	public static implicit operator Var(double val)
	{
		return new Var(val.ToString());
	}

	public static implicit operator Var(string val)
	{
		return new Var("'" + val + "'");
	}

	public static implicit operator bool(Var val)
	{
		return val.data != null && val.data == "true";
	}

	public static implicit operator long(Var val)
	{
		return (long)ToDouble(val.data);
	}

	public static implicit operator int(Var val)
	{
		return (int)ToDouble(val.data);
	}

	public static implicit operator short(Var val)
	{
		return (short)ToDouble(val.data);
	}

	public static implicit operator sbyte(Var val)
	{
		return (sbyte)ToDouble(val.data);
	}

	public static implicit operator ulong(Var val)
	{
		return (ulong)ToDouble(val.data);
	}

	public static implicit operator uint(Var val)
	{
		return (uint)ToDouble(val.data);
	}

	public static implicit operator ushort(Var val)
	{
		return (ushort)ToDouble(val.data);
	}

	public static implicit operator byte(Var val)
	{
		return (byte)ToDouble(val.data);
	}

	public static implicit operator float(Var val)
	{
		return (float)ToDouble(val.data);
	}

	public static implicit operator double(Var val)
	{
		return ToDouble(val.data);
	}

	public static implicit operator string(Var val)
	{
		if (string.IsNullOrEmpty(val.data))
		{
			return string.Empty;
		}
		return (!val.isString) ? val.data : val.data.Substring(1, val.data.Length - 2);
	}
}

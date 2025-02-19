using System;
using System.Globalization;
using PeCustom;
using UnityEngine;

namespace ScenarioRTL;

public static class Utility
{
	public static bool Compare(bool lhs, bool rhs, ECompare comp)
	{
		int num = (lhs ? 1 : 0);
		int num2 = (rhs ? 1 : 0);
		return comp switch
		{
			ECompare.Greater => num > num2, 
			ECompare.GreaterEqual => num >= num2, 
			ECompare.Equal => num == num2, 
			ECompare.NotEqual => num != num2, 
			ECompare.LessEqual => num <= num2, 
			ECompare.Less => num < num2, 
			_ => false, 
		};
	}

	public static bool Compare(int lhs, int rhs, ECompare comp)
	{
		return comp switch
		{
			ECompare.Greater => lhs > rhs, 
			ECompare.GreaterEqual => lhs >= rhs, 
			ECompare.Equal => lhs == rhs, 
			ECompare.NotEqual => lhs != rhs, 
			ECompare.LessEqual => lhs <= rhs, 
			ECompare.Less => lhs < rhs, 
			_ => false, 
		};
	}

	public static bool Compare(float lhs, int rhs, ECompare comp)
	{
		return comp switch
		{
			ECompare.Greater => lhs > (float)rhs, 
			ECompare.GreaterEqual => lhs >= (float)rhs, 
			ECompare.Equal => lhs == (float)rhs, 
			ECompare.NotEqual => lhs != (float)rhs, 
			ECompare.LessEqual => lhs <= (float)rhs, 
			ECompare.Less => lhs < (float)rhs, 
			_ => false, 
		};
	}

	public static bool Compare(int lhs, float rhs, ECompare comp)
	{
		return comp switch
		{
			ECompare.Greater => (float)lhs > rhs, 
			ECompare.GreaterEqual => (float)lhs >= rhs, 
			ECompare.Equal => (float)lhs == rhs, 
			ECompare.NotEqual => (float)lhs != rhs, 
			ECompare.LessEqual => (float)lhs <= rhs, 
			ECompare.Less => (float)lhs < rhs, 
			_ => false, 
		};
	}

	public static bool Compare(float lhs, float rhs, ECompare comp)
	{
		return comp switch
		{
			ECompare.Greater => lhs > rhs, 
			ECompare.GreaterEqual => lhs >= rhs, 
			ECompare.Equal => lhs == rhs, 
			ECompare.NotEqual => lhs != rhs, 
			ECompare.LessEqual => lhs <= rhs, 
			ECompare.Less => lhs < rhs, 
			_ => false, 
		};
	}

	public static bool Compare(double lhs, float rhs, ECompare comp)
	{
		return comp switch
		{
			ECompare.Greater => lhs > (double)rhs, 
			ECompare.GreaterEqual => lhs >= (double)rhs, 
			ECompare.Equal => lhs == (double)rhs, 
			ECompare.NotEqual => lhs != (double)rhs, 
			ECompare.LessEqual => lhs <= (double)rhs, 
			ECompare.Less => lhs < (double)rhs, 
			_ => false, 
		};
	}

	public static bool Compare(float lhs, double rhs, ECompare comp)
	{
		return comp switch
		{
			ECompare.Greater => (double)lhs > rhs, 
			ECompare.GreaterEqual => (double)lhs >= rhs, 
			ECompare.Equal => (double)lhs == rhs, 
			ECompare.NotEqual => (double)lhs != rhs, 
			ECompare.LessEqual => (double)lhs <= rhs, 
			ECompare.Less => (double)lhs < rhs, 
			_ => false, 
		};
	}

	public static bool Compare(double lhs, double rhs, ECompare comp)
	{
		return comp switch
		{
			ECompare.Greater => lhs > rhs, 
			ECompare.GreaterEqual => lhs >= rhs, 
			ECompare.Equal => lhs == rhs, 
			ECompare.NotEqual => lhs != rhs, 
			ECompare.LessEqual => lhs <= rhs, 
			ECompare.Less => lhs < rhs, 
			_ => false, 
		};
	}

	public static bool Compare(int lhs, double rhs, ECompare comp)
	{
		return comp switch
		{
			ECompare.Greater => (double)lhs > rhs, 
			ECompare.GreaterEqual => (double)lhs >= rhs, 
			ECompare.Equal => (double)lhs == rhs, 
			ECompare.NotEqual => (double)lhs != rhs, 
			ECompare.LessEqual => (double)lhs <= rhs, 
			ECompare.Less => (double)lhs < rhs, 
			_ => false, 
		};
	}

	public static bool Compare(double lhs, int rhs, ECompare comp)
	{
		return comp switch
		{
			ECompare.Greater => lhs > (double)rhs, 
			ECompare.GreaterEqual => lhs >= (double)rhs, 
			ECompare.Equal => lhs == (double)rhs, 
			ECompare.NotEqual => lhs != (double)rhs, 
			ECompare.LessEqual => lhs <= (double)rhs, 
			ECompare.Less => lhs < (double)rhs, 
			_ => false, 
		};
	}

	public static bool Compare(string lhs, string rhs, ECompare comp)
	{
		return comp switch
		{
			ECompare.Greater => string.Compare(lhs, rhs) > 0, 
			ECompare.GreaterEqual => string.Compare(lhs, rhs) >= 0, 
			ECompare.Equal => string.Compare(lhs, rhs) == 0, 
			ECompare.NotEqual => string.Compare(lhs, rhs) != 0, 
			ECompare.LessEqual => string.Compare(lhs, rhs) <= 0, 
			ECompare.Less => string.Compare(lhs, rhs) < 0, 
			_ => false, 
		};
	}

	public static bool CompareVar(Var lhs, Var rhs, ECompare comp)
	{
		if (lhs.isInteger && rhs.isInteger)
		{
			return Compare((int)lhs, (int)rhs, comp);
		}
		if (lhs.isNumber && rhs.isNumber)
		{
			return Compare((double)lhs, (double)rhs, comp);
		}
		if (lhs.isString && rhs.isString)
		{
			return Compare((string)lhs, (string)rhs, comp);
		}
		if (lhs.isBool && rhs.isBool)
		{
			return Compare((bool)lhs, (bool)rhs, comp);
		}
		if (lhs.isBool && rhs.isInteger)
		{
			return Compare(lhs, (int)rhs != 0, comp);
		}
		if (lhs.isInteger && rhs.isBool)
		{
			return Compare((int)lhs != 0, rhs, comp);
		}
		return lhs;
	}

	public static bool Function(bool lhs, bool rhs, EFunc func)
	{
		return func switch
		{
			EFunc.SetTo => rhs, 
			EFunc.Plus => lhs || rhs, 
			EFunc.Minus => lhs ^ rhs, 
			EFunc.Multiply => lhs && rhs, 
			EFunc.Divide => lhs && rhs, 
			EFunc.Mod => false, 
			EFunc.Power => lhs, 
			EFunc.XOR => lhs ^ rhs, 
			_ => rhs, 
		};
	}

	public static int Function(int lhs, int rhs, EFunc func)
	{
		return func switch
		{
			EFunc.SetTo => rhs, 
			EFunc.Plus => lhs + rhs, 
			EFunc.Minus => lhs - rhs, 
			EFunc.Multiply => lhs * rhs, 
			EFunc.Divide => (rhs != 0) ? (lhs / rhs) : 0, 
			EFunc.Mod => (rhs != 0) ? (lhs % rhs) : 0, 
			EFunc.Power => (lhs != 0) ? ((int)Math.Pow(lhs, rhs)) : 0, 
			EFunc.XOR => lhs ^ rhs, 
			_ => rhs, 
		};
	}

	public static float Function(float lhs, float rhs, EFunc func)
	{
		return func switch
		{
			EFunc.SetTo => rhs, 
			EFunc.Plus => lhs + rhs, 
			EFunc.Minus => lhs - rhs, 
			EFunc.Multiply => lhs * rhs, 
			EFunc.Divide => (rhs != 0f) ? (lhs / rhs) : 0f, 
			EFunc.Mod => (rhs != 0f) ? (lhs % rhs) : 0f, 
			EFunc.Power => (lhs != 0f) ? ((float)Math.Pow(lhs, rhs)) : 0f, 
			EFunc.XOR => (int)lhs ^ (int)rhs, 
			_ => rhs, 
		};
	}

	public static double Function(double lhs, double rhs, EFunc func)
	{
		return func switch
		{
			EFunc.SetTo => rhs, 
			EFunc.Plus => lhs + rhs, 
			EFunc.Minus => lhs - rhs, 
			EFunc.Multiply => lhs * rhs, 
			EFunc.Divide => (rhs != 0.0) ? (lhs / rhs) : 0.0, 
			EFunc.Mod => (rhs != 0.0) ? (lhs % rhs) : 0.0, 
			EFunc.Power => (lhs != 0.0) ? Math.Pow(lhs, rhs) : 0.0, 
			EFunc.XOR => (int)lhs ^ (int)rhs, 
			_ => rhs, 
		};
	}

	public static string Function(string lhs, string rhs, EFunc func)
	{
		return func switch
		{
			EFunc.SetTo => rhs, 
			EFunc.Plus => lhs + rhs, 
			_ => rhs, 
		};
	}

	public static Var FunctionVar(Var lhs, Var rhs, EFunc func)
	{
		if (func == EFunc.SetTo)
		{
			return rhs;
		}
		if (lhs.isInteger && rhs.isInteger)
		{
			return Function((int)lhs, (int)rhs, func);
		}
		if (lhs.isNumber && rhs.isNumber)
		{
			return Function((double)lhs, (double)rhs, func);
		}
		if (lhs.isString && rhs.isString)
		{
			return Function((string)lhs, (string)rhs, func);
		}
		if (lhs.isBool && rhs.isBool)
		{
			return Function((bool)lhs, (bool)rhs, func);
		}
		if (lhs.isBool && rhs.isInteger)
		{
			return Function(lhs, (int)rhs != 0, func);
		}
		if (lhs.isInteger && rhs.isBool)
		{
			return Function((int)lhs != 0, rhs, func);
		}
		return lhs;
	}

	public static string GetVarName(string value)
	{
		int length = value.Length;
		if (length < 3)
		{
			return null;
		}
		if (value[0] == '%' && value[length - 1] == '%')
		{
			return value.Substring(1, length - 2);
		}
		return null;
	}

	public static int ToEnumInt(string value)
	{
		int result = 0;
		if (int.TryParse(value, out result))
		{
			return result;
		}
		return 0;
	}

	public static int ToInt(VarScope vs, string value)
	{
		int result = 0;
		if (vs != null)
		{
			string varName = GetVarName(value);
			if (varName != null)
			{
				return vs[varName];
			}
		}
		if (int.TryParse(value, out result))
		{
			return result;
		}
		return 0;
	}

	public static ECompare ToCompare(string value)
	{
		int result = 0;
		if (int.TryParse(value, out result))
		{
			return (ECompare)result;
		}
		return (ECompare)0;
	}

	public static EFunc ToFunc(string value)
	{
		int result = 0;
		if (int.TryParse(value, out result))
		{
			return (EFunc)result;
		}
		return EFunc.SetTo;
	}

	public static bool ToBool(VarScope vs, string value)
	{
		int result = 0;
		if (vs != null)
		{
			string varName = GetVarName(value);
			if (varName != null)
			{
				return vs[varName];
			}
		}
		if (int.TryParse(value, out result))
		{
			return result != 0;
		}
		if (value.ToLower() == "true")
		{
			return true;
		}
		return false;
	}

	public static float ToSingle(VarScope vs, string value)
	{
		float result = 0f;
		if (vs != null)
		{
			string varName = GetVarName(value);
			if (varName != null)
			{
				return vs[varName];
			}
		}
		if (float.TryParse(value, NumberStyles.Float, NumberFormatInfo.CurrentInfo, out result))
		{
			return result;
		}
		return 0f;
	}

	public static double ToDouble(VarScope vs, string value)
	{
		double result = 0.0;
		if (vs != null)
		{
			string varName = GetVarName(value);
			if (varName != null)
			{
				return vs[varName];
			}
		}
		if (double.TryParse(value, NumberStyles.Float, NumberFormatInfo.CurrentInfo, out result))
		{
			return result;
		}
		return 0.0;
	}

	public static string ToText(VarScope vs, string value)
	{
		if (vs != null)
		{
			string varName = GetVarName(value);
			if (varName != null)
			{
				return vs[varName];
			}
		}
		return value;
	}

	public static Var ToVar(VarScope vs, string value)
	{
		if (vs != null)
		{
			string varName = GetVarName(value);
			if (varName != null)
			{
				return vs[varName];
			}
		}
		return new Var(value);
	}

	public static string ToVarname(string value)
	{
		string varName = GetVarName(value);
		if (varName != null)
		{
			return varName;
		}
		return value;
	}

	public static Vector3 ToVector(VarScope vs, string value)
	{
		if (vs != null)
		{
			string varName = GetVarName(value);
			if (varName != null)
			{
				value = vs[varName].data;
			}
		}
		if (SEType.TryParse_VECTOR(value, out var vec))
		{
			return vec;
		}
		return Vector3.zero;
	}

	public static Rect ToRect(VarScope vs, string value)
	{
		if (vs != null)
		{
			string varName = GetVarName(value);
			if (varName != null)
			{
				value = vs[varName].data;
			}
		}
		if (SEType.TryParse_RECT(value, out var vec))
		{
			return vec;
		}
		return new Rect(0f, 0f, 0f, 0f);
	}

	public static Color ToColor(VarScope vs, string value)
	{
		if (vs != null)
		{
			string varName = GetVarName(value);
			if (varName != null)
			{
				value = vs[varName].data;
			}
		}
		if (SEType.TryParse_COLOR(value, out var vec))
		{
			return vec;
		}
		return Color.white;
	}

	public static RANGE ToRange(VarScope vs, string value)
	{
		if (vs != null)
		{
			string varName = GetVarName(value);
			if (varName != null)
			{
				value = vs[varName].data;
			}
		}
		if (SEType.TryParse_RANGE(value, out var range))
		{
			return range;
		}
		return RANGE.nowhere;
	}

	public static DIRRANGE ToDirRange(VarScope vs, string value)
	{
		if (vs != null)
		{
			string varName = GetVarName(value);
			if (varName != null)
			{
				value = vs[varName].data;
			}
		}
		if (SEType.TryParse_DIRRANGE(value, out var range))
		{
			return range;
		}
		return DIRRANGE.nodir;
	}

	public static OBJECT ToObject(string value)
	{
		if (SEType.TryParse_OBJECT(value, out var obj))
		{
			return obj;
		}
		return OBJECT.Null;
	}
}

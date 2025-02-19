using System;
using CustomData;

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
}

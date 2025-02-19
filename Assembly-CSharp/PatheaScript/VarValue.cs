using System;
using UnityEngine;

namespace PatheaScript;

[Serializable]
public class VarValue
{
	public enum EType
	{
		Int,
		Bool,
		Float,
		Vector3,
		String,
		Var,
		Max
	}

	private object mValue;

	private EType mEType;

	private VarValue ConvertToInt
	{
		get
		{
			if (mValue == null)
			{
				return new VarValue(0);
			}
			return new VarValue(int.Parse((string)mValue));
		}
	}

	private VarValue ConvertToBool
	{
		get
		{
			if (mValue == null)
			{
				return new VarValue(v: false);
			}
			return new VarValue(bool.Parse((string)mValue));
		}
	}

	private VarValue ConvertToFloat
	{
		get
		{
			if (mValue == null)
			{
				return new VarValue(0f);
			}
			return new VarValue(float.Parse((string)mValue));
		}
	}

	private VarValue ConvertToVector3
	{
		get
		{
			if (mValue == null)
			{
				return new VarValue(Vector3.zero);
			}
			return new VarValue(Util.GetVector3((string)mValue));
		}
	}

	private VarValue ConvertToText
	{
		get
		{
			if (mValue == null)
			{
				return new VarValue(string.Empty);
			}
			return new VarValue((string)mValue);
		}
	}

	public bool IsNil => mValue == null && mEType == EType.Var;

	public VarValue(int v)
	{
		mValue = v;
		mEType = EType.Int;
	}

	public VarValue(bool v)
	{
		mValue = v;
		mEType = EType.Bool;
	}

	public VarValue(float v)
	{
		mValue = v;
		mEType = EType.Float;
	}

	public VarValue(Vector3 v)
	{
		mValue = v;
		mEType = EType.Vector3;
	}

	public VarValue(string v)
	{
		mValue = v;
		mEType = EType.String;
	}

	public VarValue(object v)
	{
		mValue = v;
		mEType = EType.Var;
	}

	public VarValue()
	{
		mValue = null;
		mEType = EType.Var;
	}

	private VarValue Convert(EType eType)
	{
		return eType switch
		{
			EType.Int => ConvertToInt, 
			EType.Bool => ConvertToBool, 
			EType.Float => ConvertToFloat, 
			EType.Vector3 => ConvertToVector3, 
			EType.String => ConvertToText, 
			_ => throw new Exception("no default value"), 
		};
	}

	public override string ToString()
	{
		return string.Format("[{0},{1}]", mEType, (mValue != null) ? mValue : "null");
	}

	public override bool Equals(object obj)
	{
		return this == obj as VarValue;
	}

	public override int GetHashCode()
	{
		return base.GetHashCode();
	}

	public static VarValue Power(VarValue lhs, VarValue rhs)
	{
		if (!lhs.IsNil || !rhs.IsNil)
		{
			if (lhs.mEType == rhs.mEType)
			{
				if (rhs.mEType == EType.Int)
				{
					return new VarValue((int)Mathf.Pow((int)lhs.mValue, (int)rhs.mValue));
				}
				if (rhs.mEType == EType.Float)
				{
					return new VarValue(Mathf.Pow((float)lhs.mValue, (float)rhs.mValue));
				}
			}
			else if (rhs.mEType == EType.Float)
			{
				if (lhs.mEType == EType.Int)
				{
					return new VarValue((int)Mathf.Pow((int)lhs.mValue, (float)rhs.mValue));
				}
			}
			else if (rhs.mEType == EType.Int && lhs.mEType == EType.Float)
			{
				return new VarValue(Mathf.Pow((float)lhs.mValue, (int)rhs.mValue));
			}
		}
		throw new NotSupportedException(lhs.ToString() + " Pow(" + rhs.ToString() + ") not supported.");
	}

	public static VarValue operator ^(VarValue lhs, VarValue rhs)
	{
		if ((!lhs.IsNil || !rhs.IsNil) && lhs.mEType == rhs.mEType)
		{
			if (rhs.mEType == EType.Bool)
			{
				return new VarValue(!(bool)rhs.mValue);
			}
			if (rhs.mEType == EType.Int)
			{
				return new VarValue((int)lhs.mValue ^ (int)rhs.mValue);
			}
		}
		throw new NotSupportedException(lhs.ToString() + " ^ " + rhs.ToString() + " not supported.");
	}

	public static VarValue operator +(VarValue lhs, VarValue rhs)
	{
		if (!lhs.IsNil || !rhs.IsNil)
		{
			if (lhs.mEType == rhs.mEType)
			{
				if (lhs.mEType == EType.Int)
				{
					return new VarValue((int)lhs.mValue + (int)rhs.mValue);
				}
				if (lhs.mEType == EType.Float)
				{
					return new VarValue((float)lhs.mValue + (float)rhs.mValue);
				}
				if (lhs.mEType == EType.Vector3)
				{
					return new VarValue((Vector3)lhs.mValue + (Vector3)rhs.mValue);
				}
				if (lhs.mEType == EType.String)
				{
					return new VarValue($"{lhs.mValue}{rhs.mValue}");
				}
				if (lhs.mEType == EType.Var)
				{
					return Util.TryParseVarValue((string)lhs.mValue) + Util.TryParseVarValue((string)rhs.mValue);
				}
			}
			else
			{
				if (lhs.mEType == EType.Var)
				{
					return lhs.Convert(rhs.mEType) + rhs;
				}
				if (rhs.mEType == EType.Var)
				{
					return lhs + rhs.Convert(lhs.mEType);
				}
			}
		}
		throw new NotSupportedException(lhs.ToString() + " + " + rhs.ToString() + "not supported.");
	}

	public static VarValue operator -(VarValue lhs, VarValue rhs)
	{
		if (lhs.mEType == rhs.mEType)
		{
			if (lhs.mEType == EType.Int)
			{
				return new VarValue((int)lhs.mValue - (int)rhs.mValue);
			}
			if (lhs.mEType == EType.Float)
			{
				return new VarValue((float)lhs.mValue - (float)rhs.mValue);
			}
			if (lhs.mEType == EType.Vector3)
			{
				return new VarValue((Vector3)lhs.mValue - (Vector3)rhs.mValue);
			}
			if (lhs.mEType == EType.Var)
			{
				return Util.TryParseVarValue((string)lhs.mValue) - Util.TryParseVarValue((string)rhs.mValue);
			}
		}
		else
		{
			if (lhs.mEType == EType.Var)
			{
				return lhs.Convert(rhs.mEType) - rhs;
			}
			if (rhs.mEType == EType.Var)
			{
				return lhs - rhs.Convert(lhs.mEType);
			}
		}
		throw new NotSupportedException(lhs.ToString() + " - " + rhs.ToString() + "not supported.");
	}

	public static VarValue operator *(VarValue lhs, VarValue rhs)
	{
		if (lhs.mEType == rhs.mEType)
		{
			if (lhs.mEType == EType.Int)
			{
				return new VarValue((int)lhs.mValue - (int)rhs.mValue);
			}
			if (lhs.mEType == EType.Float)
			{
				return new VarValue((float)lhs.mValue - (float)rhs.mValue);
			}
			if (lhs.mEType == EType.Var)
			{
				return Util.TryParseVarValue((string)lhs.mValue) * Util.TryParseVarValue((string)rhs.mValue);
			}
		}
		else
		{
			if (lhs.mEType == EType.Var)
			{
				return lhs.Convert(rhs.mEType) * rhs;
			}
			if (rhs.mEType == EType.Var)
			{
				return lhs * rhs.Convert(lhs.mEType);
			}
		}
		throw new NotSupportedException(lhs.ToString() + " * " + rhs.ToString() + "not supported.");
	}

	public static VarValue operator /(VarValue lhs, VarValue rhs)
	{
		if (lhs.mEType == rhs.mEType)
		{
			if (lhs.mEType == EType.Int)
			{
				return new VarValue((int)lhs.mValue / (int)rhs.mValue);
			}
			if (lhs.mEType == EType.Float)
			{
				return new VarValue((float)lhs.mValue / (float)rhs.mValue);
			}
			if (lhs.mEType == EType.Var)
			{
				return Util.TryParseVarValue((string)lhs.mValue) / Util.TryParseVarValue((string)rhs.mValue);
			}
		}
		else
		{
			if (lhs.mEType == EType.Var)
			{
				return lhs.Convert(rhs.mEType) / rhs;
			}
			if (rhs.mEType == EType.Var)
			{
				return lhs / rhs.Convert(lhs.mEType);
			}
		}
		throw new NotSupportedException(lhs.ToString() + " / " + rhs.ToString() + "not supported.");
	}

	public static VarValue operator %(VarValue lhs, VarValue rhs)
	{
		if (lhs.mEType == rhs.mEType)
		{
			if (lhs.mEType == EType.Int)
			{
				return new VarValue((int)lhs.mValue % (int)rhs.mValue);
			}
			if (lhs.mEType == EType.Var)
			{
				return Util.TryParseVarValue((string)lhs.mValue) % Util.TryParseVarValue((string)rhs.mValue);
			}
		}
		else
		{
			if (lhs.mEType == EType.Var)
			{
				return lhs.Convert(rhs.mEType) % rhs;
			}
			if (rhs.mEType == EType.Var)
			{
				return lhs % rhs.Convert(lhs.mEType);
			}
		}
		throw new NotSupportedException(lhs.ToString() + " % " + rhs.ToString() + "not supported.");
	}

	public static bool operator ==(VarValue lhs, VarValue rhs)
	{
		if (object.ReferenceEquals(lhs, rhs))
		{
			return true;
		}
		if (object.ReferenceEquals(lhs, null) || object.ReferenceEquals(rhs, null))
		{
			return false;
		}
		if (lhs.mEType == rhs.mEType)
		{
			if (lhs.mEType == EType.Int)
			{
				return (int)lhs.mValue == (int)rhs.mValue;
			}
			if (lhs.mEType == EType.Bool)
			{
				return (bool)lhs.mValue == (bool)rhs.mValue;
			}
			if (lhs.mEType == EType.Float)
			{
				return (float)lhs.mValue == (float)rhs.mValue;
			}
			if (lhs.mEType == EType.Vector3)
			{
				return (Vector3)lhs.mValue == (Vector3)rhs.mValue;
			}
			if (lhs.mEType == EType.String)
			{
				return object.Equals(lhs.mValue, rhs.mValue);
			}
			if (lhs.mEType == EType.Var)
			{
				return Util.TryParseVarValue((string)lhs.mValue) == Util.TryParseVarValue((string)rhs.mValue);
			}
		}
		else
		{
			if (lhs.mEType == EType.Var)
			{
				return lhs.Convert(rhs.mEType) == rhs;
			}
			if (rhs.mEType == EType.Var)
			{
				return lhs == rhs.Convert(lhs.mEType);
			}
		}
		throw new NotSupportedException(lhs.ToString() + " == " + rhs.ToString() + "not supported.");
	}

	public static bool operator !=(VarValue lhs, VarValue rhs)
	{
		return !(lhs == rhs);
	}

	public static bool operator >(VarValue lhs, VarValue rhs)
	{
		if (lhs.mEType == rhs.mEType)
		{
			if (lhs.mEType == EType.Int)
			{
				return (int)lhs.mValue > (int)rhs.mValue;
			}
			if (lhs.mEType == EType.Float)
			{
				return (float)lhs.mValue > (float)rhs.mValue;
			}
			if (lhs.mEType == EType.Var)
			{
				return Util.TryParseVarValue((string)lhs.mValue) > Util.TryParseVarValue((string)rhs.mValue);
			}
		}
		else
		{
			if (lhs.mEType == EType.Var)
			{
				return lhs.Convert(rhs.mEType) > rhs;
			}
			if (rhs.mEType == EType.Var)
			{
				return lhs > rhs.Convert(lhs.mEType);
			}
		}
		throw new NotSupportedException(lhs.ToString() + " > " + rhs.ToString() + "not supported.");
	}

	public static bool operator >=(VarValue lhs, VarValue rhs)
	{
		return lhs > rhs || lhs == rhs;
	}

	public static bool operator <(VarValue lhs, VarValue rhs)
	{
		if (lhs.mEType == rhs.mEType)
		{
			if (lhs.mEType == EType.Int)
			{
				return (int)lhs.mValue < (int)rhs.mValue;
			}
			if (lhs.mEType == EType.Float)
			{
				return (float)lhs.mValue < (float)rhs.mValue;
			}
			if (lhs.mEType == EType.Var)
			{
				return Util.TryParseVarValue((string)lhs.mValue) < Util.TryParseVarValue((string)rhs.mValue);
			}
		}
		else
		{
			if (lhs.mEType == EType.Var)
			{
				return lhs.Convert(rhs.mEType) < rhs;
			}
			if (rhs.mEType == EType.Var)
			{
				return lhs < rhs.Convert(lhs.mEType);
			}
		}
		throw new NotSupportedException(lhs.ToString() + " < " + rhs.ToString() + "not supported.");
	}

	public static bool operator <=(VarValue lhs, VarValue rhs)
	{
		return lhs < rhs || lhs == rhs;
	}

	public static implicit operator VarValue(int v)
	{
		return new VarValue(v);
	}

	public static explicit operator int(VarValue v)
	{
		if (v.mEType != 0)
		{
			throw new NotSupportedException(v.ToString() + "convert to int not supported.");
		}
		return (int)v.mValue;
	}

	public static implicit operator VarValue(bool v)
	{
		return new VarValue(v);
	}

	public static explicit operator bool(VarValue v)
	{
		if (v.mEType != EType.Bool)
		{
			throw new NotSupportedException(v.ToString() + "convert to bool not supported.");
		}
		return (bool)v.mValue;
	}

	public static implicit operator VarValue(float v)
	{
		return new VarValue(v);
	}

	public static explicit operator float(VarValue v)
	{
		if (v.mEType != EType.Float)
		{
			throw new NotSupportedException(v.ToString() + "convert to float not supported.");
		}
		return (float)v.mValue;
	}

	public static implicit operator VarValue(Vector3 v)
	{
		return new VarValue(v);
	}

	public static explicit operator Vector3(VarValue v)
	{
		if (v.mEType != EType.Vector3)
		{
			throw new NotSupportedException(v.ToString() + "convert to Vector3 not supported.");
		}
		return (Vector3)v.mValue;
	}

	public static implicit operator VarValue(string v)
	{
		return new VarValue(v);
	}

	public static explicit operator string(VarValue v)
	{
		if (v.mEType != EType.String)
		{
			throw new NotSupportedException(v.ToString() + "convert to string not supported.");
		}
		return (string)v.mValue;
	}
}

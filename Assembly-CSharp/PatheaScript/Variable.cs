using System;

namespace PatheaScript;

[Serializable]
public class Variable
{
	public enum EScope
	{
		Gloabel,
		Script,
		Trigger,
		Max
	}

	private VarValue mValue;

	public VarValue Value
	{
		get
		{
			if (null == mValue)
			{
				mValue = new VarValue();
			}
			return mValue;
		}
		set
		{
			mValue = value;
		}
	}

	public Variable(VarValue v)
	{
		mValue = v;
	}

	public Variable()
	{
		mValue = null;
	}

	public override string ToString()
	{
		return $"Variable:{Value}";
	}
}

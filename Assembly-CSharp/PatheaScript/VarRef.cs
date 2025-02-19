using System;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

namespace PatheaScript;

public class VarRef
{
	private string mVarName;

	private Trigger mTrigger;

	private Variable mVariable;

	public VarValue Value
	{
		get
		{
			return Var.Value;
		}
		private set
		{
			if (mVariable == null)
			{
				Var = new Variable(value);
			}
			else
			{
				Var.Value = value;
			}
		}
	}

	public Variable Var
	{
		get
		{
			if (mVariable == null)
			{
				if (mTrigger == null)
				{
					throw new Exception("no trigger set");
				}
				mVariable = mTrigger.GetVar(mVarName);
				if (mVariable == null)
				{
					throw new Exception("no variable found with name:" + mVarName);
				}
			}
			return mVariable;
		}
		private set
		{
			mVariable = value;
		}
	}

	public string Name => mVarName;

	public VarRef(string varName, Trigger trigger)
	{
		mVarName = varName;
		mTrigger = trigger;
	}

	public VarRef(VarValue v)
	{
		mVarName = null;
		mTrigger = null;
		Value = v;
	}

	public override string ToString()
	{
		return $"Variable[{Name}:{Value}]";
	}

	public static VarRef Deserialize(Stream stream, Trigger trigger)
	{
		using BinaryReader binaryReader = new BinaryReader(stream);
		if (binaryReader.ReadBoolean())
		{
			BinaryFormatter binaryFormatter = new BinaryFormatter();
			VarValue v = binaryFormatter.Deserialize(stream) as VarValue;
			return new VarRef(v);
		}
		string varName = binaryReader.ReadString();
		return new VarRef(varName, trigger);
	}

	public static bool Serialize(Stream stream, VarRef varRef)
	{
		using BinaryWriter binaryWriter = new BinaryWriter(stream);
		if (string.IsNullOrEmpty(varRef.Name))
		{
			binaryWriter.Write(value: true);
			BinaryFormatter binaryFormatter = new BinaryFormatter();
			binaryFormatter.Serialize(stream, varRef.Value);
		}
		else
		{
			binaryWriter.Write(value: false);
			binaryWriter.Write(varRef.Name);
		}
		return true;
	}
}

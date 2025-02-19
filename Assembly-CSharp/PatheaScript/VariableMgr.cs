using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

namespace PatheaScript;

[Serializable]
public class VariableMgr
{
	private Dictionary<string, Variable> mDicVar;

	public Variable GetVar(string varName)
	{
		if (mDicVar == null)
		{
			return null;
		}
		if (mDicVar.ContainsKey(varName))
		{
			return mDicVar[varName];
		}
		return null;
	}

	public bool AddVar(string varName, Variable var)
	{
		if (var == null)
		{
			Debug.LogError("add null variable to manager");
			return false;
		}
		if (mDicVar == null)
		{
			mDicVar = new Dictionary<string, Variable>(5);
		}
		if (mDicVar.ContainsKey(varName))
		{
			Debug.LogError("variable [" + varName + "] exist");
			return false;
		}
		mDicVar.Add(varName, var);
		return true;
	}

	public static byte[] Export(VariableMgr mgr)
	{
		MemoryStream memoryStream = new MemoryStream();
		BinaryFormatter binaryFormatter = new BinaryFormatter();
		binaryFormatter.Serialize(memoryStream, mgr);
		return memoryStream.ToArray();
	}

	public static VariableMgr Import(byte[] data)
	{
		MemoryStream serializationStream = new MemoryStream(data, writable: false);
		BinaryFormatter binaryFormatter = new BinaryFormatter();
		return binaryFormatter.Deserialize(serializationStream) as VariableMgr;
	}
}

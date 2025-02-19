using System.Collections.Generic;
using System.IO;
using UnityEngine;

public static class CSDataMgr
{
	public const int VERSION000 = 15042418;

	public const int VERSION001 = 15091800;

	public const int VERSION002 = 15110600;

	public const int VERSION003 = 15111720;

	public const int VERSION004 = 16052514;

	public const int VERSION005 = 16071518;

	public const int VERSION006 = 16072317;

	public const int VERSION007 = 16091400;

	public const int VERSION008 = 16101900;

	public const int VERSION009 = 16102000;

	public const int VERSION010 = 16102100;

	public const int CUR_VERSION = 16102100;

	private static bool debugSwitch = false;

	public static Dictionary<int, CSDataInst> m_DataInsts = new Dictionary<int, CSDataInst>();

	public static bool HasDataInst()
	{
		return m_DataInsts.Count != 0;
	}

	public static CSDataInst CreateDataInst(int id, CSConst.CreatorType type)
	{
		if (m_DataInsts.ContainsKey(id))
		{
			Debug.Log("Still have this data inst.");
			return m_DataInsts[id];
		}
		CSDataInst cSDataInst = new CSDataInst();
		cSDataInst.m_ID = id;
		cSDataInst.m_Type = type;
		m_DataInsts.Add(id, cSDataInst);
		return cSDataInst;
	}

	public static void RemoveDataInst(int ID)
	{
		if (m_DataInsts.ContainsKey(ID))
		{
			m_DataInsts[ID].ClearData();
		}
	}

	public static void Clear()
	{
		foreach (CSDataInst value in m_DataInsts.Values)
		{
			value.ClearData();
		}
		m_DataInsts.Clear();
		CSClodMgr.Clear();
		CSClodsMgr.Clear();
	}

	public static int GenerateNewID()
	{
		int num = 0;
		foreach (int key in m_DataInsts.Keys)
		{
			if (num < key)
			{
				num = key;
			}
		}
		return num + 1;
	}

	public static void Import(byte[] buffer)
	{
		Clear();
		if (buffer == null || buffer.Length < 8)
		{
			return;
		}
		if (debugSwitch)
		{
			Debug.Log("<color=yellow>Start to Import CSDataMgr</color>");
		}
		MemoryStream input = new MemoryStream(buffer);
		BinaryReader binaryReader = new BinaryReader(input);
		int num = binaryReader.ReadInt32();
		if (debugSwitch)
		{
			Debug.Log("<color=yellow>version:" + num + "</color>");
		}
		if (num != 16102100 && debugSwitch)
		{
			Debug.LogWarning("The version of ColonyrecordMgr is newer than the record.");
		}
		if (num < 15042418)
		{
			return;
		}
		int num2 = binaryReader.ReadInt32();
		for (int i = 0; i < num2; i++)
		{
			CSDataInst cSDataInst = new CSDataInst();
			if (debugSwitch)
			{
				Debug.Log("<color=yellow>" + i + " count: " + num2 + "</color>");
			}
			cSDataInst.m_ID = binaryReader.ReadInt32();
			if (debugSwitch)
			{
				Debug.Log("<color=yellow>m_ID: " + cSDataInst.m_ID + "</color>");
			}
			cSDataInst.Import(binaryReader);
			m_DataInsts.Add(cSDataInst.m_ID, cSDataInst);
		}
		CSClodMgr.Init();
		CSClodMgr.Instance.Import(binaryReader);
		CSClodsMgr.Init();
		CSClodsMgr.Instance.Import(binaryReader);
	}

	public static void Export(BinaryWriter w)
	{
		if (debugSwitch)
		{
			Debug.Log("<color=yellow>Start to Export CSDataMgr</color>");
		}
		w.Write(16102100);
		w.Write(m_DataInsts.Count);
		foreach (KeyValuePair<int, CSDataInst> dataInst in m_DataInsts)
		{
			if (debugSwitch)
			{
				Debug.Log("<color=yellow>Key(m_ID): " + dataInst.Key + "</color>");
			}
			w.Write(dataInst.Key);
			dataInst.Value.Export(w);
		}
		CSClodMgr.Instance.Export(w);
		CSClodsMgr.Instance.Export(w);
	}
}

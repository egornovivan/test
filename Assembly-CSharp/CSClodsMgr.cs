using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class CSClodsMgr
{
	private static CSClodsMgr s_Instance;

	private Dictionary<int, CSClod> m_Clods;

	public static CSClodsMgr Instance => s_Instance;

	public CSClodsMgr()
	{
		m_Clods = new Dictionary<int, CSClod>();
	}

	public static void Init()
	{
		if (s_Instance == null)
		{
			s_Instance = new CSClodsMgr();
		}
	}

	public static CSClod CreateClod(int id)
	{
		if (Instance == null)
		{
			Debug.Log("The CSClodsMgr is null.");
			return null;
		}
		if (!Instance.m_Clods.ContainsKey(id))
		{
			Instance.m_Clods.Add(id, new CSClod(id));
		}
		return Instance.m_Clods[id];
	}

	public static void RemoveClod(int id)
	{
		if (Instance == null)
		{
			Debug.Log("The CSClodsMgr is null.");
			return;
		}
		Instance.m_Clods.Clear();
		Instance.m_Clods.Remove(id);
	}

	public static void Clear()
	{
		if (Instance == null)
		{
			Debug.Log("The CSClodsMgr is null.");
			return;
		}
		foreach (KeyValuePair<int, CSClod> clod in Instance.m_Clods)
		{
			clod.Value.Clear();
		}
		Instance.m_Clods.Clear();
	}

	public void Import(BinaryReader r)
	{
		int num = r.ReadInt32();
		if (num >= 15042418)
		{
			int num2 = r.ReadInt32();
			for (int i = 0; i < num2; i++)
			{
				int num3 = r.ReadInt32();
				CSClod cSClod = new CSClod(num3);
				cSClod.Import(r);
				m_Clods.Add(num3, cSClod);
			}
		}
	}

	public void Export(BinaryWriter w)
	{
		w.Write(16102100);
		w.Write(m_Clods.Count);
		foreach (KeyValuePair<int, CSClod> clod in m_Clods)
		{
			w.Write(clod.Key);
			clod.Value.Export(w);
		}
	}
}

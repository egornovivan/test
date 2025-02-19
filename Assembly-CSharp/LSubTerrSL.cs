using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public static class LSubTerrSL
{
	public const int VERSION = 257;

	public static Dictionary<int, List<Vector3>> m_mapDelPos;

	public static event Action OnLSubTerrSLInitEvent;

	public static void Init()
	{
		if (m_mapDelPos != null)
		{
			foreach (KeyValuePair<int, List<Vector3>> mapDelPo in m_mapDelPos)
			{
				mapDelPo.Value.Clear();
			}
			m_mapDelPos.Clear();
			m_mapDelPos = null;
		}
		m_mapDelPos = new Dictionary<int, List<Vector3>>();
		if (LSubTerrSL.OnLSubTerrSLInitEvent != null)
		{
			LSubTerrSL.OnLSubTerrSLInitEvent();
		}
	}

	public static void Clear()
	{
		if (m_mapDelPos != null)
		{
			foreach (KeyValuePair<int, List<Vector3>> mapDelPo in m_mapDelPos)
			{
				mapDelPo.Value.Clear();
			}
			m_mapDelPos.Clear();
			m_mapDelPos = null;
		}
		else
		{
			Debug.LogError("LSubTerrSL haven't initialized!");
		}
	}

	public static void AddDeletedTree(Vector3 pos)
	{
		if (m_mapDelPos == null)
		{
			Debug.LogError("LSubTerrSL haven't initialized!");
			return;
		}
		int key = LSubTerrUtils.WorldPosToIndex(pos);
		pos.x -= (float)Mathf.FloorToInt(pos.x / 256f) * 256f;
		pos.z -= (float)Mathf.FloorToInt(pos.z / 256f) * 256f;
		if (m_mapDelPos.ContainsKey(key))
		{
			if (!m_mapDelPos[key].Contains(pos))
			{
				m_mapDelPos[key].Add(pos);
			}
		}
		else
		{
			List<Vector3> list = new List<Vector3>();
			list.Add(pos);
			m_mapDelPos.Add(key, list);
		}
	}

	public static void AddDeletedTree(int index, TreeInfo treeinfo)
	{
		if (m_mapDelPos == null)
		{
			Debug.LogError("LSubTerrSL haven't initialized!");
			return;
		}
		Vector3 pos = treeinfo.m_pos;
		pos.x *= 256f;
		pos.y *= 3000f;
		pos.z *= 256f;
		if (m_mapDelPos.ContainsKey(index))
		{
			m_mapDelPos[index].Add(pos);
			return;
		}
		List<Vector3> list = new List<Vector3>();
		list.Add(pos);
		m_mapDelPos.Add(index, list);
	}

	public static void Import(byte[] buffer)
	{
		if (buffer == null || buffer.Length < 8)
		{
			return;
		}
		Init();
		MemoryStream memoryStream = new MemoryStream(buffer);
		BinaryReader binaryReader = new BinaryReader(memoryStream);
		int num = binaryReader.ReadInt32();
		if (num != 257)
		{
			Debug.LogWarning("The version of LSubTerrSL is newer than the record.");
		}
		int num2 = num;
		if (num2 == 257)
		{
			int num3 = binaryReader.ReadInt32();
			for (int i = 0; i < num3; i++)
			{
				int key = binaryReader.ReadInt32();
				float x = binaryReader.ReadSingle();
				float y = binaryReader.ReadSingle();
				float z = binaryReader.ReadSingle();
				if (m_mapDelPos.ContainsKey(key))
				{
					m_mapDelPos[key].Add(new Vector3(x, y, z));
					continue;
				}
				List<Vector3> list = new List<Vector3>();
				list.Add(new Vector3(x, y, z));
				m_mapDelPos.Add(key, list);
			}
		}
		binaryReader.Close();
		memoryStream.Close();
	}

	public static void Export(BinaryWriter w)
	{
		if (m_mapDelPos == null)
		{
			Debug.LogError("LSubTerrSL haven't initialized!");
			return;
		}
		int num = 0;
		foreach (KeyValuePair<int, List<Vector3>> mapDelPo in m_mapDelPos)
		{
			num += mapDelPo.Value.Count;
		}
		w.Write(257);
		w.Write(num);
		foreach (KeyValuePair<int, List<Vector3>> mapDelPo2 in m_mapDelPos)
		{
			foreach (Vector3 item in mapDelPo2.Value)
			{
				w.Write(mapDelPo2.Key);
				w.Write(item.x);
				w.Write(item.y);
				w.Write(item.z);
			}
		}
	}
}

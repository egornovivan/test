using System;
using System.Collections.Generic;
using System.IO;
using Pathea.Maths;
using UnityEngine;

public class GrassDataSL
{
	public const int VERSION = 257;

	public static Dictionary<INTVECTOR3, INTVECTOR3> m_mapDelPos;

	public static event Action OnGrassDataInitEvent;

	public static void Init()
	{
		if (m_mapDelPos != null)
		{
			m_mapDelPos.Clear();
		}
		else
		{
			m_mapDelPos = new Dictionary<INTVECTOR3, INTVECTOR3>();
		}
		if (GrassDataSL.OnGrassDataInitEvent != null)
		{
			GrassDataSL.OnGrassDataInitEvent();
		}
	}

	public static void Clear()
	{
		if (m_mapDelPos != null)
		{
			m_mapDelPos.Clear();
		}
		else
		{
			Debug.LogError("LSubTerrSL haven't initialized!");
		}
	}

	public static void AddDeletedGrass(INTVECTOR3 pos)
	{
		if (m_mapDelPos == null)
		{
			Debug.LogError("LSubTerrSL haven't initialized!");
		}
		else if (!m_mapDelPos.ContainsKey(pos))
		{
			m_mapDelPos.Add(pos, pos);
		}
	}

	public static void Import(byte[] buffer)
	{
		if (buffer == null || buffer.Length < 8)
		{
			return;
		}
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
				INTVECTOR3 iNTVECTOR = default(INTVECTOR3);
				iNTVECTOR.x = binaryReader.ReadInt32();
				iNTVECTOR.y = binaryReader.ReadInt32();
				iNTVECTOR.z = binaryReader.ReadInt32();
				m_mapDelPos.Add(iNTVECTOR, iNTVECTOR);
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
		w.Write(257);
		w.Write(m_mapDelPos.Count);
		foreach (INTVECTOR3 value in m_mapDelPos.Values)
		{
			w.Write(value.x);
			w.Write(value.y);
			w.Write(value.z);
		}
	}
}

using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class CSClodMgr
{
	public const byte TerrainType = 19;

	public const int CHUNK_SIZE = 8;

	private static CSClodMgr s_Instance;

	public Dictionary<IntVec3, Vector3> m_ClodLocas;

	private Dictionary<IntVec3, int> m_IdleClods;

	public Dictionary<IntVec3, ClodChunk> m_ClodChunks;

	private Dictionary<IntVec3, int> m_IdleChunks;

	public static CSClodMgr Instance => s_Instance;

	public CSClodMgr()
	{
		m_ClodLocas = new Dictionary<IntVec3, Vector3>();
		m_IdleClods = new Dictionary<IntVec3, int>();
		m_ClodChunks = new Dictionary<IntVec3, ClodChunk>();
		m_IdleChunks = new Dictionary<IntVec3, int>();
	}

	public static void Init()
	{
		if (s_Instance != null)
		{
			Debug.Log("The CSCloMgr is areadly.");
		}
		else
		{
			s_Instance = new CSClodMgr();
		}
	}

	public static void Clear()
	{
		if (s_Instance != null)
		{
			s_Instance.m_ClodLocas.Clear();
			s_Instance.m_IdleClods.Clear();
			s_Instance.m_IdleChunks.Clear();
			s_Instance.m_ClodChunks.Clear();
		}
	}

	public static void AddClod(Vector3 pos, bool dirty = false)
	{
		if (s_Instance == null)
		{
			Debug.Log("Clod manager is not exist.");
			return;
		}
		IntVec3 clodIndex = new IntVec3(pos);
		IntVec3 chunkIndex = GetChunkIndex(clodIndex);
		if (!s_Instance.m_ClodChunks.ContainsKey(chunkIndex))
		{
			ClodChunk clodChunk = new ClodChunk();
			clodChunk.m_ChunkIndex = chunkIndex;
			s_Instance.m_ClodChunks.Add(chunkIndex, clodChunk);
			s_Instance.m_ClodChunks[chunkIndex].AddClod(pos, dirty);
		}
		else
		{
			s_Instance.m_ClodChunks[chunkIndex].AddClod(pos, dirty);
		}
		s_Instance.m_IdleChunks[chunkIndex] = 0;
	}

	public static void DeleteClod(Vector3 pos)
	{
		if (s_Instance == null)
		{
			Debug.Log("Clod manager is not exist.");
			return;
		}
		IntVec3 chunkIndex = GetChunkIndex(new IntVec3(pos));
		if (s_Instance.m_ClodChunks.ContainsKey(chunkIndex))
		{
			s_Instance.m_ClodChunks[chunkIndex].DeleteClod(pos);
			if (s_Instance.m_ClodChunks[chunkIndex].m_Clods.Count == 0)
			{
				s_Instance.m_IdleChunks.Remove(chunkIndex);
				s_Instance.m_ClodChunks.Remove(chunkIndex);
			}
		}
	}

	public static void DirtyTheClod(Vector3 pos, bool dirty)
	{
		if (s_Instance == null)
		{
			Debug.Log("Clod manager is not exist.");
			return;
		}
		IntVec3 chunkIndex = GetChunkIndex(new IntVec3(pos));
		if (s_Instance.m_ClodChunks.ContainsKey(chunkIndex))
		{
			s_Instance.m_ClodChunks[chunkIndex].DirtyTheClod(pos, dirty);
			if (!dirty)
			{
				s_Instance.m_IdleChunks[chunkIndex] = 0;
			}
		}
	}

	public static Vector3 FindCleanClod()
	{
		if (s_Instance == null)
		{
			Debug.Log("Clod manager is not exist.");
			return Vector3.zero;
		}
		return Vector3.zero;
	}

	public static Vector3 FindCleanClod(Vector3 center, float radius)
	{
		if (s_Instance == null)
		{
			Debug.Log("Clod manager is not exist.");
			return Vector3.zero;
		}
		if (s_Instance.m_IdleClods.Count != 0)
		{
			float num = radius * radius;
			foreach (IntVec3 key in s_Instance.m_IdleClods.Keys)
			{
				if (new Vector3(center.x - (float)key.x, center.y - (float)key.y, center.z - (float)key.z).sqrMagnitude < num)
				{
					return s_Instance.m_ClodLocas[key];
				}
			}
			return Vector3.zero;
		}
		return Vector3.zero;
	}

	public static IntVec3 GetChunkIndex(IntVec3 clodIndex)
	{
		return new IntVec3(clodIndex.x / 8, clodIndex.y / 8, clodIndex.z / 8);
	}

	public static ClodChunk FindCleanChunk(Vector3 center, float radius)
	{
		if (s_Instance == null)
		{
			Debug.Log("Clod manager is not exist.");
			return null;
		}
		if (s_Instance.m_IdleChunks.Count != 0)
		{
			float num = (radius + 1f) * (radius + 1f);
			foreach (IntVec3 key in s_Instance.m_IdleChunks.Keys)
			{
				if (new Vector3(center.x - (float)(key.x * 8), center.y - (float)(key.y * 8), center.z - (float)(key.z * 8)).sqrMagnitude < num && s_Instance.m_ClodChunks[key].m_IdleClods.Count != 0)
				{
					return s_Instance.m_ClodChunks[key];
				}
			}
		}
		return null;
	}

	public static void DirtyTheChunk(IntVec3 chunkIndex, bool dirty)
	{
		if (s_Instance == null)
		{
			Debug.Log("Clod manager is not exist.");
		}
		else if (s_Instance.m_ClodChunks.ContainsKey(chunkIndex))
		{
			if (dirty)
			{
				s_Instance.m_IdleChunks.Remove(chunkIndex);
			}
			else
			{
				s_Instance.m_IdleChunks[chunkIndex] = 0;
			}
		}
	}

	public void Import(BinaryReader r)
	{
		int num = r.ReadInt32();
		if (num < 15042418)
		{
			return;
		}
		int num2 = r.ReadInt32();
		for (int i = 0; i < num2; i++)
		{
			ClodChunk clodChunk = new ClodChunk();
			clodChunk.Import(r, num);
			m_ClodChunks.Add(clodChunk.m_ChunkIndex, clodChunk);
			m_IdleChunks.Add(clodChunk.m_ChunkIndex, 0);
		}
		num2 = r.ReadInt32();
		for (int j = 0; j < num2; j++)
		{
			ClodChunk clodChunk2 = new ClodChunk();
			clodChunk2.Import(r, num);
			m_ClodChunks.Add(clodChunk2.m_ChunkIndex, clodChunk2);
			if (clodChunk2.HasIdleClods())
			{
				m_IdleChunks[clodChunk2.m_ChunkIndex] = 0;
			}
		}
	}

	public void Export(BinaryWriter w)
	{
		w.Write(15042418);
		w.Write(m_IdleChunks.Count);
		Dictionary<IntVec3, ClodChunk> dictionary = new Dictionary<IntVec3, ClodChunk>();
		foreach (KeyValuePair<IntVec3, ClodChunk> clodChunk2 in m_ClodChunks)
		{
			dictionary.Add(clodChunk2.Key, clodChunk2.Value);
		}
		foreach (IntVec3 key in m_IdleChunks.Keys)
		{
			ClodChunk clodChunk = m_ClodChunks[key];
			clodChunk.Export(w);
			dictionary.Remove(key);
		}
		w.Write(dictionary.Count);
		foreach (KeyValuePair<IntVec3, ClodChunk> item in dictionary)
		{
			item.Value.Export(w);
		}
	}
}

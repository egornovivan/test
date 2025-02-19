using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class CSClod
{
	public const byte TerrainType = 19;

	public const int CHUNK_SIZE = 8;

	public int ID;

	public Dictionary<IntVec3, ClodChunk> m_ClodChunks;

	private Dictionary<IntVec3, int> m_IdleChunks;

	public CSClod(int id)
	{
		m_ClodChunks = new Dictionary<IntVec3, ClodChunk>();
		m_IdleChunks = new Dictionary<IntVec3, int>();
		ID = id;
	}

	public void Clear()
	{
		m_IdleChunks.Clear();
		m_ClodChunks.Clear();
	}

	public void AddClod(Vector3 pos, bool dirty = false)
	{
		IntVec3 clodIndex = new IntVec3(pos);
		IntVec3 chunkIndex = GetChunkIndex(clodIndex);
		if (!m_ClodChunks.ContainsKey(chunkIndex))
		{
			ClodChunk clodChunk = new ClodChunk();
			clodChunk.m_ChunkIndex = chunkIndex;
			m_ClodChunks.Add(chunkIndex, clodChunk);
			m_ClodChunks[chunkIndex].AddClod(pos, dirty);
		}
		else
		{
			m_ClodChunks[chunkIndex].AddClod(pos, dirty);
		}
		m_IdleChunks[chunkIndex] = 0;
	}

	public void DeleteClod(Vector3 pos)
	{
		IntVec3 chunkIndex = GetChunkIndex(new IntVec3(pos));
		if (m_ClodChunks.ContainsKey(chunkIndex))
		{
			m_ClodChunks[chunkIndex].DeleteClod(pos);
			if (m_ClodChunks[chunkIndex].m_Clods.Count == 0)
			{
				m_IdleChunks.Remove(chunkIndex);
				m_ClodChunks.Remove(chunkIndex);
			}
		}
	}

	public void DirtyTheClod(Vector3 pos, bool dirty)
	{
		IntVec3 chunkIndex = GetChunkIndex(new IntVec3(pos));
		if (m_ClodChunks.ContainsKey(chunkIndex))
		{
			m_ClodChunks[chunkIndex].DirtyTheClod(pos, dirty);
			if (!dirty)
			{
				m_IdleChunks[chunkIndex] = 0;
			}
		}
	}

	public static IntVec3 GetChunkIndex(IntVec3 clodIndex)
	{
		return new IntVec3(clodIndex.x / 8, clodIndex.y / 8, clodIndex.z / 8);
	}

	public ClodChunk FindCleanChunk(Vector3 center, float radius)
	{
		if (m_IdleChunks.Count != 0)
		{
			float num = (radius + 4f) * (radius + 4f);
			foreach (IntVec3 key in m_IdleChunks.Keys)
			{
				if (new Vector3(center.x - (float)(key.x * 8), center.y - (float)(key.y * 8), center.z - (float)(key.z * 8)).sqrMagnitude < num && m_ClodChunks[key].m_IdleClods.Count != 0)
				{
					return m_ClodChunks[key];
				}
			}
		}
		return null;
	}

	public ClodChunk FindHasIdleClodsChunk(Vector3 center, float radius, CSFarm farm, int plantItemid, out Vector3 pos)
	{
		if (m_IdleChunks.Count != 0)
		{
			float num = (radius + 8f) * (radius + 8f);
			foreach (IntVec3 key in m_IdleChunks.Keys)
			{
				if (new Vector3(center.x - (float)(key.x * 8), center.y - (float)(key.y * 8), center.z - (float)(key.z * 8)).sqrMagnitude < num && m_ClodChunks[key].HasIdleClodsInRange(radius, center) && m_ClodChunks[key].FindBetterClod(center, radius, farm, plantItemid, out pos))
				{
					return m_ClodChunks[key];
				}
			}
		}
		pos = Vector3.zero;
		return null;
	}

	public void DirtyTheChunk(IntVec3 chunkIndex, bool dirty)
	{
		if (m_ClodChunks.ContainsKey(chunkIndex))
		{
			if (dirty)
			{
				m_IdleChunks.Remove(chunkIndex);
			}
			else
			{
				m_IdleChunks[chunkIndex] = 0;
			}
		}
	}

	public bool Import(BinaryReader r)
	{
		int num = r.ReadInt32();
		if (num >= 15042418)
		{
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
		return true;
	}

	public void Export(BinaryWriter w)
	{
		w.Write(16102100);
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

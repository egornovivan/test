using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class ClodChunk
{
	public IntVec3 m_ChunkIndex;

	public Dictionary<IntVec3, Vector3> m_Clods;

	public Dictionary<IntVec3, int> m_IdleClods;

	public Vector3 ReservedPos = Vector3.zero;

	public ClodChunk()
	{
		m_Clods = new Dictionary<IntVec3, Vector3>();
		m_IdleClods = new Dictionary<IntVec3, int>();
	}

	public void AddClod(Vector3 pos, bool dirty = false)
	{
		IntVec3 key = new IntVec3(pos);
		if (!m_Clods.ContainsKey(key))
		{
			m_Clods.Add(key, pos);
			if (!dirty && !m_IdleClods.ContainsKey(key))
			{
				m_IdleClods.Add(key, 0);
			}
		}
	}

	public void FreeClodByBouds(int plantItemId, Vector3 pos, bool dirty = false)
	{
		Bounds plantBounds = PlantInfo.GetPlantBounds(plantItemId, pos);
		foreach (IntVec3 key2 in m_Clods.Keys)
		{
			if (plantBounds.Contains(m_Clods[key2]))
			{
				IntVec3 key = new IntVec3(m_Clods[key2]);
				if (!dirty && !m_IdleClods.ContainsKey(key))
				{
					m_IdleClods.Add(key, 0);
				}
			}
		}
	}

	public void DeleteClod(Vector3 pos)
	{
		IntVec3 key = new IntVec3(pos);
		if (m_Clods.ContainsKey(key))
		{
			m_IdleClods.Remove(key);
			m_Clods.Remove(key);
		}
	}

	public void DirtyTheClod(Vector3 pos, bool dirty)
	{
		IntVec3 key = new IntVec3(pos);
		if (m_Clods.ContainsKey(key))
		{
			if (dirty)
			{
				m_IdleClods.Remove(key);
			}
			else
			{
				m_IdleClods[key] = 0;
			}
		}
	}

	public void DirtyTheClodByPlantBounds(int plantItemid, Vector3 pos, bool dirty)
	{
		Bounds plantBounds = PlantInfo.GetPlantBounds(plantItemid, pos);
		foreach (IntVec3 key in m_Clods.Keys)
		{
			if (plantBounds.Contains(m_Clods[key]))
			{
				DirtyTheClod(m_Clods[key], dirty);
			}
		}
	}

	public bool FindCleanClod(out Vector3 pos)
	{
		if (m_IdleClods.Count != 0)
		{
			using Dictionary<IntVec3, int>.KeyCollection.Enumerator enumerator = m_IdleClods.Keys.GetEnumerator();
			if (enumerator.MoveNext())
			{
				IntVec3 current = enumerator.Current;
				pos = m_Clods[current];
				return true;
			}
		}
		pos = Vector3.zero;
		return false;
	}

	public bool FindBetterClod(Vector3 center, float radius, CSFarm farm, int plantItemid, out Vector3 pos)
	{
		if (m_IdleClods.Count != 0)
		{
			foreach (IntVec3 key in m_IdleClods.Keys)
			{
				if (new Vector3((float)key.x - center.x, (float)key.y - center.y, (float)key.z - center.z).magnitude < radius && farm.checkRroundCanPlant(plantItemid, m_Clods[key]))
				{
					pos = m_Clods[key];
					return true;
				}
			}
		}
		pos = Vector3.zero;
		return false;
	}

	public bool HasIdleClods()
	{
		return m_IdleClods.Count != 0;
	}

	public bool HasIdleClodsInRange(float radius, Vector3 center)
	{
		foreach (IntVec3 key in m_IdleClods.Keys)
		{
			if (new Vector3((float)key.x - center.x, (float)key.y - center.y, (float)key.z - center.z).magnitude < radius)
			{
				return true;
			}
		}
		return false;
	}

	public void Import(BinaryReader r, int VERSION)
	{
		if (VERSION >= 15042418)
		{
			IntVec3 intVec = new IntVec3();
			intVec.x = r.ReadInt32();
			intVec.y = r.ReadInt32();
			intVec.z = r.ReadInt32();
			m_ChunkIndex = intVec;
			int num = r.ReadInt32();
			for (int i = 0; i < num; i++)
			{
				Vector3 vector = new Vector3(r.ReadSingle(), r.ReadSingle(), r.ReadSingle());
				IntVec3 key = new IntVec3(vector);
				m_Clods.Add(key, vector);
				m_IdleClods.Add(key, 0);
			}
			num = r.ReadInt32();
			for (int j = 0; j < num; j++)
			{
				Vector3 vector2 = new Vector3(r.ReadSingle(), r.ReadSingle(), r.ReadSingle());
				IntVec3 key2 = new IntVec3(vector2);
				m_Clods.Add(key2, vector2);
			}
		}
	}

	public void Export(BinaryWriter w)
	{
		w.Write(m_ChunkIndex.x);
		w.Write(m_ChunkIndex.y);
		w.Write(m_ChunkIndex.z);
		Dictionary<IntVec3, Vector3> dictionary = new Dictionary<IntVec3, Vector3>();
		foreach (KeyValuePair<IntVec3, Vector3> clod in m_Clods)
		{
			dictionary.Add(clod.Key, clod.Value);
		}
		w.Write(m_IdleClods.Count);
		foreach (IntVec3 key in m_IdleClods.Keys)
		{
			Vector3 vector = dictionary[key];
			w.Write(vector.x);
			w.Write(vector.y);
			w.Write(vector.z);
			dictionary.Remove(key);
		}
		w.Write(dictionary.Count);
		foreach (KeyValuePair<IntVec3, Vector3> item in dictionary)
		{
			w.Write(item.Value.x);
			w.Write(item.Value.y);
			w.Write(item.Value.z);
		}
	}
}

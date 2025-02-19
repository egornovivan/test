using System;
using System.Collections.Generic;
using Mono.Data.SqliteClient;
using Pathea;
using UnityEngine;

public class AIErodeMap
{
	public int id;

	public Vector3 pos;

	public float radius;

	private static List<AIErodeMap> m_blankData = new List<AIErodeMap>();

	private static List<AIErodeMap> m_data = new List<AIErodeMap>();

	private static int m_nextId;

	public static void LoadData()
	{
		SqliteDataReader sqliteDataReader = LocalDatabase.Instance.ReadFullTable("aispawn_blank");
		while (sqliteDataReader.Read())
		{
			AIErodeMap aIErodeMap = new AIErodeMap();
			aIErodeMap.id = m_blankData.Count;
			aIErodeMap.pos = AiUtil.ToVector3(sqliteDataReader.GetString(sqliteDataReader.GetOrdinal("position")));
			aIErodeMap.radius = Convert.ToSingle(sqliteDataReader.GetString(sqliteDataReader.GetOrdinal("radius")));
			m_blankData.Add(aIErodeMap);
		}
	}

	public static void ResetErodeData()
	{
		m_data.Clear();
		if (PeGameMgr.IsStory)
		{
			m_data.AddRange(m_blankData);
		}
		m_nextId = m_data.Count;
	}

	public static int AddErode(Vector3 position, float radius)
	{
		AIErodeMap aIErodeMap = new AIErodeMap();
		aIErodeMap.id = m_nextId++;
		aIErodeMap.pos = position;
		aIErodeMap.radius = radius;
		m_data.Add(aIErodeMap);
		return aIErodeMap.id;
	}

	public static void UpdateErode(int id, Vector3 center, float radius = 0f)
	{
		AIErodeMap aIErodeMap = m_data.Find((AIErodeMap ret) => ret.id == id);
		if (aIErodeMap != null)
		{
			if (center != Vector3.zero)
			{
				aIErodeMap.pos = center;
			}
			if (radius > float.Epsilon)
			{
				aIErodeMap.radius = radius;
			}
		}
	}

	public static void RemoveErode(int id)
	{
		AIErodeMap aIErodeMap = m_data.Find((AIErodeMap ret) => ret.id == id);
		if (aIErodeMap != null)
		{
			m_data.Remove(aIErodeMap);
		}
	}

	public static AIErodeMap IsInErodeArea2D(Vector3 position)
	{
		return m_data.Find((AIErodeMap ret) => Match2D(ret, position));
	}

	public static AIErodeMap IsInScaledErodeArea2D(Vector3 position, float fScale)
	{
		return m_data.Find((AIErodeMap ret) => MatchScaled2D(ret, position, fScale));
	}

	public static AIErodeMap IsInErodeArea(Vector3 position)
	{
		return m_data.Find((AIErodeMap ret) => Match(ret, position));
	}

	private static bool Match2D(AIErodeMap data, Vector3 position)
	{
		Vector3 vector = data.pos - position;
		vector.y = 0f;
		return vector.sqrMagnitude < data.radius * data.radius;
	}

	private static bool MatchScaled2D(AIErodeMap data, Vector3 position, float fScale)
	{
		Vector3 vector = data.pos - position;
		vector.y = 0f;
		return vector.sqrMagnitude < data.radius * data.radius * fScale * fScale;
	}

	private static bool Match(AIErodeMap data, Vector3 position)
	{
		return (data.pos - position).sqrMagnitude < data.radius * data.radius;
	}
}

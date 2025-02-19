using System;
using System.Collections.Generic;
using Mono.Data.SqliteClient;
using UnityEngine;

public class AIResource
{
	public int id;

	public int aiId;

	public float height;

	public float minScale;

	public float maxScale;

	public string name;

	public string path;

	private static List<AIResource> m_data = new List<AIResource>();

	public static AIResource Find(int argId)
	{
		return m_data.Find((AIResource ret) => ret.id == argId);
	}

	public static void LoadData()
	{
		SqliteDataReader sqliteDataReader = LocalDatabase.Instance.ReadFullTable("path");
		while (sqliteDataReader.Read())
		{
			AIResource aIResource = new AIResource();
			aIResource.id = Convert.ToInt32(sqliteDataReader.GetString(sqliteDataReader.GetOrdinal("id")));
			aIResource.aiId = Convert.ToInt32(sqliteDataReader.GetString(sqliteDataReader.GetOrdinal("aiid")));
			aIResource.name = sqliteDataReader.GetString(sqliteDataReader.GetOrdinal("name"));
			aIResource.path = sqliteDataReader.GetString(sqliteDataReader.GetOrdinal("path"));
			aIResource.height = Convert.ToSingle(sqliteDataReader.GetString(sqliteDataReader.GetOrdinal("height")));
			aIResource.minScale = Convert.ToSingle(sqliteDataReader.GetString(sqliteDataReader.GetOrdinal("minscale")));
			aIResource.maxScale = Convert.ToSingle(sqliteDataReader.GetString(sqliteDataReader.GetOrdinal("maxscale")));
			if (Find(aIResource.id) != null)
			{
				if (LogFilter.logDebug)
				{
					Debug.LogError("Can't have the same id!");
				}
			}
			else
			{
				m_data.Add(aIResource);
			}
		}
	}
}

using System;
using System.Collections.Generic;
using Mono.Data.SqliteClient;
using UnityEngine;

public static class Cutscene
{
	private static Dictionary<int, string> paths;

	public static void LoadData()
	{
		paths = new Dictionary<int, string>();
		SqliteDataReader sqliteDataReader = LocalDatabase.Instance.ReadFullTable("CutsceneClip");
		while (sqliteDataReader.Read())
		{
			int key = Convert.ToInt32(sqliteDataReader.GetString(sqliteDataReader.GetOrdinal("id")));
			string @string = sqliteDataReader.GetString(sqliteDataReader.GetOrdinal("clippath"));
			paths[key] = @string;
		}
	}

	public static CutsceneClip PlayClip(int id)
	{
		if (paths.ContainsKey(id))
		{
			GameObject gameObject = Resources.Load(paths[id]) as GameObject;
			GameObject gameObject2 = UnityEngine.Object.Instantiate(gameObject);
			gameObject2.name = gameObject.name;
			gameObject2.transform.position = gameObject.transform.position;
			gameObject2.transform.rotation = gameObject.transform.rotation;
			CutsceneClip component = gameObject2.GetComponent<CutsceneClip>();
			component.isEditMode = false;
			return component;
		}
		return null;
	}

	public static bool TooFar(int id)
	{
		if (paths.ContainsKey(id))
		{
			GameObject gameObject = Resources.Load(paths[id]) as GameObject;
			if (gameObject == null)
			{
				return false;
			}
			if (Block45Man.self.LodMan._Lod0ViewBounds.Contains(gameObject.transform.position))
			{
				return false;
			}
		}
		return true;
	}
}

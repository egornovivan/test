using System;
using Mono.Data.SqliteClient;
using Pathea;
using UnityEngine;

namespace PeMap;

public class StoryStaticPoint
{
	public static int GetCamp(int nameId)
	{
		SqliteDataReader sqliteDataReader = LocalDatabase.Instance.SelectWhereSingle("MapIcon", "*", "Name", " = ", "'" + nameId + "'");
		if (sqliteDataReader.Read())
		{
			return Convert.ToInt32(sqliteDataReader.GetString(sqliteDataReader.GetOrdinal("Camp")));
		}
		return -1;
	}

	public static int GetIDByNameID(int nameId)
	{
		SqliteDataReader sqliteDataReader = LocalDatabase.Instance.SelectWhereSingle("MapIcon", "*", "Name", " = ", "'" + nameId + "'");
		if (sqliteDataReader.Read())
		{
			return Convert.ToInt32(sqliteDataReader.GetString(sqliteDataReader.GetOrdinal("ID")));
		}
		return -1;
	}

	public static void Load()
	{
		SqliteDataReader sqliteDataReader = LocalDatabase.Instance.ReadFullTable("MapIcon");
		while (sqliteDataReader.Read())
		{
			StaticPoint staticPoint = new StaticPoint();
			staticPoint.ID = sqliteDataReader.GetInt32(0);
			staticPoint.textId = Convert.ToInt32(sqliteDataReader.GetString(1));
			string[] array = sqliteDataReader.GetString(2).Split(',');
			staticPoint.position = new Vector3(Convert.ToSingle(array[0]), Convert.ToSingle(array[1]), Convert.ToSingle(array[2]));
			staticPoint.icon = Convert.ToInt32(sqliteDataReader.GetString(3));
			staticPoint.distance = Convert.ToSingle(sqliteDataReader.GetString(4));
			staticPoint.campId = Convert.ToInt32(sqliteDataReader.GetString(5));
			if (staticPoint.campId >= 0)
			{
				staticPoint.fastTravel = true;
			}
			else
			{
				staticPoint.fastTravel = false;
			}
			staticPoint.soundID = Convert.ToInt32(sqliteDataReader.GetString(6));
			PeSingleton<StaticPoint.Mgr>.Instance.Add(staticPoint);
		}
	}
}

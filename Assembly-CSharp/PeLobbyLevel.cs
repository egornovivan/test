using System;
using System.Collections.Generic;
using Mono.Data.SqliteClient;
using Pathea;

public class PeLobbyLevel
{
	public class Mgr : PeSingleton<Mgr>, IPesingleton
	{
		private List<PeLobbyLevel> data;

		private PeLobbyLevel MaxLevel => (data.Count <= 0) ? null : data[data.Count - 1];

		void IPesingleton.Init()
		{
			LoadData();
		}

		public void LoadData()
		{
			SqliteDataReader sqliteDataReader = LocalDatabase.Instance.ReadFullTable("lobbylevel");
			data = new List<PeLobbyLevel>();
			while (sqliteDataReader.Read())
			{
				PeLobbyLevel peLobbyLevel = new PeLobbyLevel();
				peLobbyLevel.level = Convert.ToInt32(sqliteDataReader.GetString(sqliteDataReader.GetOrdinal("level")));
				peLobbyLevel.exp = Convert.ToInt32(sqliteDataReader.GetString(sqliteDataReader.GetOrdinal("exp")));
				peLobbyLevel.nextExp = Convert.ToInt32(sqliteDataReader.GetString(sqliteDataReader.GetOrdinal("next")));
				data.Add(peLobbyLevel);
			}
			data.Sort(delegate(PeLobbyLevel x, PeLobbyLevel y)
			{
				if (x.level == y.level)
				{
					return 0;
				}
				return (x.level > y.level) ? 1 : (-1);
			});
		}

		public PeLobbyLevel GetLevel(float exp)
		{
			if (exp <= 0f)
			{
				return (data.Count <= 0) ? null : data[0];
			}
			if (exp >= (float)MaxLevel.exp)
			{
				return MaxLevel;
			}
			foreach (PeLobbyLevel datum in data)
			{
				if (exp < (float)datum.exp)
				{
					return datum;
				}
			}
			return null;
		}
	}

	public int level;

	public int exp;

	public int nextExp;
}

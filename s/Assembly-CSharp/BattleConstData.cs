using Mono.Data.SqliteClient;

internal class BattleConstData
{
	internal int _camp_min;

	internal int _camp_max;

	internal int _player_max;

	internal float _win_point;

	internal int _win_site;

	internal int _win_kill;

	internal float _points_kill;

	internal float _points_assist;

	internal float _points_fell;

	internal float _points_dig;

	internal float _points_build;

	internal float _points_capture;

	internal float _points_site;

	internal int _meat_kill;

	internal int _meat_assist;

	internal int _meat_site;

	internal int _site_interval;

	internal int _meat_time;

	private static BattleConstData _instance;

	internal static BattleConstData Instance
	{
		get
		{
			if (_instance == null)
			{
				_instance = new BattleConstData();
			}
			return _instance;
		}
	}

	internal static void LoadData()
	{
		SqliteDataReader sqliteDataReader = LocalDatabase.Instance.ReadFullTable("multi_AS");
		while (sqliteDataReader.Read())
		{
			Instance._camp_min = sqliteDataReader.GetInt32(sqliteDataReader.GetOrdinal("camp_min"));
			Instance._camp_max = sqliteDataReader.GetInt32(sqliteDataReader.GetOrdinal("camp_max"));
			Instance._player_max = sqliteDataReader.GetInt32(sqliteDataReader.GetOrdinal("player_max"));
			Instance._win_point = sqliteDataReader.GetFloat(sqliteDataReader.GetOrdinal("win_point"));
			Instance._win_site = sqliteDataReader.GetInt32(sqliteDataReader.GetOrdinal("win_site"));
			Instance._win_kill = sqliteDataReader.GetInt32(sqliteDataReader.GetOrdinal("win_kill"));
			Instance._points_kill = sqliteDataReader.GetFloat(sqliteDataReader.GetOrdinal("point_kill"));
			Instance._points_assist = sqliteDataReader.GetFloat(sqliteDataReader.GetOrdinal("point_assist"));
			Instance._points_fell = sqliteDataReader.GetFloat(sqliteDataReader.GetOrdinal("point_fell"));
			Instance._points_dig = sqliteDataReader.GetFloat(sqliteDataReader.GetOrdinal("point_dig"));
			Instance._points_build = sqliteDataReader.GetFloat(sqliteDataReader.GetOrdinal("point_build"));
			Instance._points_capture = sqliteDataReader.GetFloat(sqliteDataReader.GetOrdinal("point_capture"));
			Instance._points_site = sqliteDataReader.GetFloat(sqliteDataReader.GetOrdinal("point_site"));
			Instance._meat_kill = sqliteDataReader.GetInt32(sqliteDataReader.GetOrdinal("meat_kill"));
			Instance._meat_assist = sqliteDataReader.GetInt32(sqliteDataReader.GetOrdinal("meat_assist"));
			Instance._meat_site = sqliteDataReader.GetInt32(sqliteDataReader.GetOrdinal("meat_site"));
			Instance._site_interval = sqliteDataReader.GetInt32(sqliteDataReader.GetOrdinal("site_interval"));
			Instance._meat_time = sqliteDataReader.GetInt32(sqliteDataReader.GetOrdinal("meat_time"));
		}
	}
}

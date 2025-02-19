using System.Collections.Generic;
using Mono.Data.SqliteClient;

public class PELocalization
{
	private static Dictionary<string, Dictionary<int, string>> ContentMap;

	public static void LoadData()
	{
		ContentMap = new Dictionary<string, Dictionary<int, string>>();
		ContentMap["english"] = new Dictionary<int, string>();
		ContentMap["chinese"] = new Dictionary<int, string>();
		SqliteDataReader sqliteDataReader = LocalDatabase.Instance.ReadFullTable("Translation");
		while (sqliteDataReader.Read())
		{
			int @int = sqliteDataReader.GetInt32(sqliteDataReader.GetOrdinal("ID"));
			ContentMap["english"][@int] = sqliteDataReader.GetString(sqliteDataReader.GetOrdinal("English"));
			ContentMap["chinese"][@int] = sqliteDataReader.GetString(sqliteDataReader.GetOrdinal("Chinese"));
		}
	}

	public static string GetString(int stringId)
	{
		if (ContentMap != null && ContentMap[SystemSettingData.Instance.mLanguage].ContainsKey(stringId))
		{
			return ContentMap[SystemSettingData.Instance.mLanguage][stringId];
		}
		return string.Empty;
	}
}

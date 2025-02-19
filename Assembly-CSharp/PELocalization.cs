using System.Collections.Generic;
using Mono.Data.SqliteClient;

public static class PELocalization
{
	private static int gIdxId = -1;

	private static int gIdxLangCur;

	private static int gIdxLangEng;

	private static List<string> gLangs;

	private static Dictionary<string, int> gEnIdMap;

	private static Dictionary<int, string> gCurStrMap;

	public static void LoadData(string strDbPath = null)
	{
		if (gIdxId >= 0)
		{
			return;
		}
		gIdxId = -1;
		gIdxLangCur = 0;
		gIdxLangEng = 0;
		gLangs = new List<string>();
		gEnIdMap = new Dictionary<string, int>();
		gCurStrMap = new Dictionary<int, string>();
		SqliteAccessCS sqliteAccessCS = ((strDbPath == null) ? LocalDatabase.Instance : new SqliteAccessCS(strDbPath));
		SqliteDataReader sqliteDataReader = sqliteAccessCS.ReadFullTable("Translation");
		int fieldCount = sqliteDataReader.FieldCount;
		for (int i = 0; i < fieldCount; i++)
		{
			string text = sqliteDataReader.GetName(i).ToLower();
			if (text == "id")
			{
				gIdxId = i;
				continue;
			}
			gLangs.Add(text);
			if (text == SystemSettingData.Instance.mLanguage)
			{
				gIdxLangCur = i;
			}
			if (text == "english")
			{
				gIdxLangEng = i;
			}
		}
		while (sqliteDataReader.Read())
		{
			int @int = sqliteDataReader.GetInt32(gIdxId);
			gCurStrMap[@int] = sqliteDataReader.GetString(gIdxLangCur);
			if (gIdxLangCur != gIdxLangEng)
			{
				gEnIdMap[sqliteDataReader.GetString(gIdxLangEng).ToLower()] = @int;
			}
		}
		sqliteDataReader.Close();
		if (strDbPath != null)
		{
			sqliteAccessCS.CloseDB();
		}
	}

	public static string GetString(int strId)
	{
		if (gCurStrMap.TryGetValue(strId, out var value))
		{
			return value;
		}
		return string.Empty;
	}

	public static string ToLocalizationString(this string origin)
	{
		if (gIdxLangCur != gIdxLangEng && gEnIdMap.TryGetValue(origin.ToLower(), out var value))
		{
			return gCurStrMap[value];
		}
		return origin;
	}
}

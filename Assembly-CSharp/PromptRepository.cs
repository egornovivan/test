using System;
using System.Collections.Generic;
using Mono.Data.SqliteClient;

public class PromptRepository
{
	public static Dictionary<int, PromptData> m_PromptMap = new Dictionary<int, PromptData>();

	public static PromptData GetPromptData(int type)
	{
		return (!m_PromptMap.ContainsKey(type)) ? null : m_PromptMap[type];
	}

	public static string GetPromptContent(int type)
	{
		PromptData promptData = GetPromptData(type);
		if (promptData == null)
		{
			return string.Empty;
		}
		return promptData.m_Content;
	}

	public static void LoadData()
	{
		SqliteDataReader sqliteDataReader = LocalDatabase.Instance.ReadFullTable("System_prompt");
		while (sqliteDataReader.Read())
		{
			PromptData promptData = new PromptData();
			promptData.m_Type = Convert.ToInt32(sqliteDataReader.GetString(sqliteDataReader.GetOrdinal("id")));
			promptData.m_Content = sqliteDataReader.GetString(sqliteDataReader.GetOrdinal("hint"));
			m_PromptMap.Add(promptData.m_Type, promptData);
		}
	}
}

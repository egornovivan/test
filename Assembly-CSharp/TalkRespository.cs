using System;
using System.Collections.Generic;
using Mono.Data.SqliteClient;
using UnityEngine;

public class TalkRespository
{
	public static Dictionary<int, TalkData> m_TalkMap = new Dictionary<int, TalkData>();

	public static TalkData GetTalkData(int TalkID)
	{
		return (!m_TalkMap.ContainsKey(TalkID)) ? null : m_TalkMap[TalkID];
	}

	public static void LoadData()
	{
		SqliteDataReader sqliteDataReader = LocalDatabase.Instance.ReadFullTable("Scenario");
		while (sqliteDataReader.Read())
		{
			TalkData talkData = new TalkData();
			talkData.m_ID = Convert.ToInt32(sqliteDataReader.GetString(sqliteDataReader.GetOrdinal("talk_id")));
			talkData.m_NpcID = Convert.ToInt32(sqliteDataReader.GetString(sqliteDataReader.GetOrdinal("npc_id")));
			string @string = sqliteDataReader.GetString(sqliteDataReader.GetOrdinal("OtherNPC"));
			string[] array;
			if (@string != "0")
			{
				array = @string.Split(',');
				string[] array2 = array;
				foreach (string value in array2)
				{
					talkData.m_otherNpc.Add(Convert.ToInt32(value));
				}
			}
			@string = sqliteDataReader.GetString(sqliteDataReader.GetOrdinal("talk_content"));
			array = @string.Split(':');
			int strId;
			if (array.Length == 3)
			{
				strId = Convert.ToInt32(array[0]);
				talkData.m_Content = PELocalization.GetString(strId);
				talkData.m_SoundID = Convert.ToInt32(array[1]);
				talkData.m_ClipName = array[2];
			}
			strId = Convert.ToInt32(sqliteDataReader.GetString(sqliteDataReader.GetOrdinal("isRadio")));
			talkData.isRadio = strId == 1;
			talkData.needLangSkill = Convert.ToInt32(sqliteDataReader.GetString(sqliteDataReader.GetOrdinal("LanguageSkill")));
			@string = sqliteDataReader.GetString(sqliteDataReader.GetOrdinal("TalkTo"));
			array = @string.Split(',');
			if (array.Length == 3)
			{
				float x = Convert.ToSingle(array[0]);
				float y = Convert.ToSingle(array[1]);
				float z = Convert.ToSingle(array[2]);
				talkData.talkToNpcidOrVecter3 = new Vector3(x, y, z);
			}
			else if (array.Length == 1)
			{
				talkData.talkToNpcidOrVecter3 = Convert.ToInt32(array[0]);
			}
			@string = sqliteDataReader.GetString(sqliteDataReader.GetOrdinal("MoveTo"));
			array = @string.Split(',');
			if (array.Length == 3)
			{
				float x2 = Convert.ToSingle(array[0]);
				float y2 = Convert.ToSingle(array[1]);
				float z2 = Convert.ToSingle(array[2]);
				talkData.moveTonpcidOrvecter3 = new Vector3(x2, y2, z2);
			}
			else if (array.Length == 1)
			{
				talkData.moveTonpcidOrvecter3 = Convert.ToInt32(array[0]);
			}
			talkData.m_moveType = Convert.ToInt32(sqliteDataReader.GetString(sqliteDataReader.GetOrdinal("MoveSpeed")));
			@string = sqliteDataReader.GetString(sqliteDataReader.GetOrdinal("EndNPCTalk"));
			if (@string != "0")
			{
				array = @string.Split(',');
				string[] array3 = array;
				foreach (string value2 in array3)
				{
					talkData.m_endOtherNpc.Add(Convert.ToInt32(value2));
				}
			}
			m_TalkMap.Add(talkData.m_ID, talkData);
		}
	}
}

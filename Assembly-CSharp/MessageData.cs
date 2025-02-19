using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Mono.Data.SqliteClient;
using UnityEngine;

public class MessageData
{
	private const int m_MsgVersion = 1607160;

	private int m_ID;

	private int[] m_MissionIDs;

	private string m_Topic;

	private string m_Form;

	private string m_To;

	private string m_Title;

	private string m_Content;

	private string m_End;

	private string m_Date;

	public static Dictionary<int, MessageData> AllMsgDataDic = new Dictionary<int, MessageData>();

	public static Dictionary<int, int[]> AllMissionIDDic = new Dictionary<int, int[]>();

	public static List<int> ActiveMsgDataIDs = new List<int>();

	public static Action<int> AddMsgEvent;

	public int ID => m_ID;

	public int[] MissionIDs => m_MissionIDs;

	public string Topic => m_Topic;

	public string Form => m_Form;

	public string To => m_To;

	public string Title => m_Title;

	public string Content => m_Content;

	public string End => m_End;

	public string Date => m_Date;

	public MessageData(int id, int[] missionIDs, string topic, string form, string to, string date, string title, string content, string end)
	{
		m_ID = id;
		m_MissionIDs = missionIDs;
		m_Topic = topic;
		m_Form = form;
		m_To = to;
		m_Title = title;
		m_Content = content;
		m_End = end;
		m_Date = date;
	}

	public static void LoadData()
	{
		SqliteDataReader sqliteDataReader = LocalDatabase.Instance.ReadFullTable("MessagesData");
		MessageData messageData = null;
		while (sqliteDataReader.Read())
		{
			int num = Convert.ToInt32(sqliteDataReader.GetString(sqliteDataReader.GetOrdinal("ID")));
			string @string = sqliteDataReader.GetString(sqliteDataReader.GetOrdinal("MissionID"));
			int strId = Convert.ToInt32(sqliteDataReader.GetString(sqliteDataReader.GetOrdinal("Topic")));
			string string2 = sqliteDataReader.GetString(sqliteDataReader.GetOrdinal("FromAddress"));
			string string3 = sqliteDataReader.GetString(sqliteDataReader.GetOrdinal("ToAddress"));
			int strId2 = Convert.ToInt32(sqliteDataReader.GetString(sqliteDataReader.GetOrdinal("Date")));
			int strId3 = Convert.ToInt32(sqliteDataReader.GetString(sqliteDataReader.GetOrdinal("Title")));
			int strId4 = Convert.ToInt32(sqliteDataReader.GetString(sqliteDataReader.GetOrdinal("Content")));
			int strId5 = Convert.ToInt32(sqliteDataReader.GetString(sqliteDataReader.GetOrdinal("End")));
			string[] array = @string.Split(',');
			int[] array2 = new int[array.Length];
			for (int i = 0; i < array.Length; i++)
			{
				array2[i] = Convert.ToInt32(array[i]);
			}
			messageData = new MessageData(num, array2, PELocalization.GetString(strId), string2, string3, PELocalization.GetString(strId2), PELocalization.GetString(strId3), PELocalization.GetString(strId4), PELocalization.GetString(strId5));
			AllMsgDataDic.Add(num, messageData);
			AllMissionIDDic.Add(num, array2);
		}
	}

	public static void AddMsgByCompletedMissionID(int missionID)
	{
		if (AllMsgDataDic == null || AllMsgDataDic.Count <= 0)
		{
			return;
		}
		Dictionary<int, int[]> dictionary = AllMissionIDDic.Where((KeyValuePair<int, int[]> a) => a.Value.Contains(missionID)).ToDictionary((KeyValuePair<int, int[]> k) => k.Key, (KeyValuePair<int, int[]> v) => v.Value);
		foreach (KeyValuePair<int, int[]> item in dictionary)
		{
			if (!ActiveMsgDataIDs.Contains(item.Key) && item.Value.Length == item.Value.Count((int id) => MissionManager.Instance.HadCompleteMissionAnyNum(id)))
			{
				ActiveMsgDataIDs.Add(item.Key);
				if (AddMsgEvent != null)
				{
					AddMsgEvent(item.Key);
				}
			}
		}
	}

	public static bool Deserialize(byte[] data)
	{
		ActiveMsgDataIDs.Clear();
		try
		{
			MemoryStream input = new MemoryStream(data, writable: false);
			using (BinaryReader binaryReader = new BinaryReader(input))
			{
				if (binaryReader.ReadInt32() != 1607160)
				{
					Debug.LogWarning("MessageData version valid! ");
					return false;
				}
				int num = binaryReader.ReadInt32();
				for (int i = 0; i < num; i++)
				{
					ActiveMsgDataIDs.Add(binaryReader.ReadInt32());
				}
			}
			return true;
		}
		catch (Exception ex)
		{
			Debug.LogWarning("MessageData deserialize error: " + ex);
			return false;
		}
	}

	public static byte[] Serialize()
	{
		try
		{
			MemoryStream memoryStream = new MemoryStream(200);
			using (BinaryWriter binaryWriter = new BinaryWriter(memoryStream))
			{
				binaryWriter.Write(1607160);
				binaryWriter.Write(ActiveMsgDataIDs.Count);
				for (int i = 0; i < ActiveMsgDataIDs.Count; i++)
				{
					binaryWriter.Write(ActiveMsgDataIDs[i]);
				}
			}
			return memoryStream.ToArray();
		}
		catch (Exception message)
		{
			Debug.LogWarning(message);
			return null;
		}
	}

	public static void Clear()
	{
		ActiveMsgDataIDs.Clear();
	}
}

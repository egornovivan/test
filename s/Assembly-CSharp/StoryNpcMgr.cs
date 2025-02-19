using System;
using System.Collections.Generic;
using Mono.Data.SqliteClient;
using Pathea;
using UnityEngine;

public class StoryNpcMgr
{
	private static Dictionary<int, StoryNpc> storyNpcDic = new Dictionary<int, StoryNpc>();

	public static void Add(int npcId, StoryNpc npc)
	{
		if (!storyNpcDic.ContainsKey(npcId) && npc != null)
		{
			storyNpcDic.Add(npcId, npc);
		}
	}

	public static StoryNpc GetNpc(int npcId)
	{
		if (storyNpcDic.ContainsKey(npcId))
		{
			return storyNpcDic[npcId];
		}
		return null;
	}

	public static void LoadStoryNpc()
	{
		SqliteDataReader sqliteDataReader = LocalDatabase.Instance.ReadFullTable("NPC");
		while (sqliteDataReader.Read())
		{
			StoryNpc storyNpc = new StoryNpc();
			storyNpc._npcId = Convert.ToInt32(sqliteDataReader.GetString(sqliteDataReader.GetOrdinal("NPC_ID")));
			storyNpc._prototypeNpc = Convert.ToInt32(sqliteDataReader.GetString(sqliteDataReader.GetOrdinal("PrototypeNPC")));
			string @string = sqliteDataReader.GetString(sqliteDataReader.GetOrdinal("startpoint"));
			string[] array = @string.Split(',');
			if (array.Length < 3)
			{
				Debug.LogError("Npc's StartPoint is Error");
				break;
			}
			float x = Convert.ToSingle(array[0]);
			float y = Convert.ToSingle(array[1]);
			float z = Convert.ToSingle(array[2]);
			storyNpc._startPoint = new Vector3(x, y, z);
			@string = sqliteDataReader.GetString(sqliteDataReader.GetOrdinal("training_pos"));
			if (@string == "0,0,0")
			{
				@string = "10000,0,0";
			}
			array = @string.Split(',');
			if (array.Length < 3)
			{
				Debug.LogError("Npc's training_pos is Error");
			}
			else
			{
				float x2 = Convert.ToSingle(array[0]);
				float y2 = Convert.ToSingle(array[1]);
				float z2 = Convert.ToSingle(array[2]);
				storyNpc._trainingPos = new Vector3(x2, y2, z2);
			}
			string string2 = sqliteDataReader.GetString(sqliteDataReader.GetOrdinal("money"));
			storyNpc.npcMoney = RandomNpcDb.NpcMoney.LoadFromText(string2);
			string string3 = sqliteDataReader.GetString(sqliteDataReader.GetOrdinal("weapon"));
			if (!string.IsNullOrEmpty(string3) && string3 != "0")
			{
				array = string3.Split(',');
				storyNpc._equipments = new int[array.Length];
				for (int i = 0; i < array.Length; i++)
				{
					storyNpc._equipments[i] = Convert.ToInt32(array[i]);
				}
			}
			@string = sqliteDataReader.GetString(sqliteDataReader.GetOrdinal("item"));
			string[] array2 = @string.Split(';');
			for (int j = 0; j < array2.Length; j++)
			{
				string[] array3 = array2[j].Split(',');
				if (array3.Length >= 2)
				{
					StoryNpc.NpcHadItems npcHadItems = new StoryNpc.NpcHadItems();
					npcHadItems._itemId = Convert.ToInt32(array3[0]);
					npcHadItems._itemNum = Convert.ToInt32(array3[1]);
					storyNpc._items.Add(npcHadItems);
				}
			}
			storyNpc._revive = Convert.ToInt32(sqliteDataReader.GetString(sqliteDataReader.GetOrdinal("revive")));
			string string4 = sqliteDataReader.GetString(sqliteDataReader.GetOrdinal("speciality"));
			if (!string.IsNullOrEmpty(string4) && string4 != "0")
			{
				string[] array4 = string4.Split(',');
				storyNpc._npcSkillIds = new int[array4.Length];
				for (int k = 0; k < array4.Length; k++)
				{
					storyNpc._npcSkillIds[k] = int.Parse(array4[k]);
				}
			}
			Add(storyNpc._npcId, storyNpc);
		}
	}
}

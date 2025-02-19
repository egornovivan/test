using System;
using System.Collections.Generic;
using Mono.Data.SqliteClient;
using PETools;
using UnityEngine;

public class HumanSoundData
{
	public int id;

	public int sex;

	public List<int> owners;

	public List<List<KeyValuePair<int, float>>> sounds;

	private static List<HumanSoundData> s_HumanSoundData;

	public static void LoadData()
	{
		s_HumanSoundData = new List<HumanSoundData>();
		SqliteDataReader sqliteDataReader = LocalDatabase.Instance.ReadFullTable("human_effects");
		while (sqliteDataReader.Read())
		{
			HumanSoundData humanSoundData = new HumanSoundData();
			humanSoundData.sounds = new List<List<KeyValuePair<int, float>>>();
			humanSoundData.id = sqliteDataReader.GetInt32(sqliteDataReader.GetOrdinal("ID"));
			humanSoundData.sex = sqliteDataReader.GetInt32(sqliteDataReader.GetOrdinal("Sex"));
			humanSoundData.owners = new List<int>(PEUtil.ToArrayInt32(sqliteDataReader.GetString(sqliteDataReader.GetOrdinal("Owner")), ','));
			string @string = sqliteDataReader.GetString(sqliteDataReader.GetOrdinal("Effects"));
			string[] array = PEUtil.ToArrayString(@string, ';');
			string[] array2 = array;
			foreach (string text in array2)
			{
				List<KeyValuePair<int, float>> list = new List<KeyValuePair<int, float>>();
				string[] array3 = PEUtil.ToArrayString(text, ',');
				for (int j = 0; j < array3.Length; j++)
				{
					string[] array4 = text.Split('_');
					if (array4.Length == 2)
					{
						int key = Convert.ToInt32(array4[0]);
						float value = Convert.ToSingle(array4[1]);
						list.Add(new KeyValuePair<int, float>(key, value));
					}
				}
				humanSoundData.sounds.Add(list);
			}
			s_HumanSoundData.Add(humanSoundData);
		}
	}

	public static int[] GetSoundID(int id, int sex, int owner = 0)
	{
		HumanSoundData humanSoundData = s_HumanSoundData.Find((HumanSoundData ret) => ret.id == id && ret.sex == sex);
		if (humanSoundData != null)
		{
			return humanSoundData.GetPlaySoundID();
		}
		return new int[0];
	}

	private int[] GetPlaySoundID()
	{
		List<int> list = new List<int>();
		foreach (List<KeyValuePair<int, float>> sound in sounds)
		{
			float value = UnityEngine.Random.value;
			float num = 0f;
			foreach (KeyValuePair<int, float> item in sound)
			{
				num += item.Value;
				if (value <= num)
				{
					list.Add(item.Key);
					break;
				}
			}
		}
		return list.ToArray();
	}
}

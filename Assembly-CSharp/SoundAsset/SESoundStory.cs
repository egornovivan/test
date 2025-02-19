using System;
using System.Collections.Generic;
using Mono.Data.SqliteClient;
using UnityEngine;

namespace SoundAsset;

public class SESoundStory
{
	public int type;

	public int colorR;

	public string soundInfoStr;

	public Dictionary<int, float> sounds;

	public static List<SESoundStory> s_tblSeSoundInfo;

	public static int GetRandomAudioClip(int type, float colorR)
	{
		return s_tblSeSoundInfo.Find((SESoundStory ret) => ret.type == type && Mathf.Abs((float)ret.colorR - colorR * 255f) < 2f)?.GetRandomAudioClip() ?? (-1);
	}

	public static void LoadData()
	{
		s_tblSeSoundInfo = new List<SESoundStory>();
		SqliteDataReader sqliteDataReader = LocalDatabase.Instance.ReadFullTable("soundspawn");
		while (sqliteDataReader.Read())
		{
			SESoundStory sESoundStory = new SESoundStory();
			sESoundStory.type = Convert.ToInt32(sqliteDataReader.GetString(sqliteDataReader.GetOrdinal("type")));
			sESoundStory.colorR = Convert.ToInt32(sqliteDataReader.GetString(sqliteDataReader.GetOrdinal("num")));
			sESoundStory.soundInfoStr = sqliteDataReader.GetString(sqliteDataReader.GetOrdinal("soundInfo"));
			sESoundStory.StringToSounds();
			s_tblSeSoundInfo.Add(sESoundStory);
		}
	}

	private void StringToSounds()
	{
		sounds = new Dictionary<int, float>();
		string[] array = soundInfoStr.Split(';');
		string[] array2 = array;
		foreach (string text in array2)
		{
			string[] array3 = text.Split(',');
			if (array3.Length == 2)
			{
				int key = Convert.ToInt32(array3[0]);
				float value = Convert.ToSingle(array3[1]);
				if (!sounds.ContainsKey(key))
				{
					sounds.Add(key, value);
				}
			}
		}
	}

	private int GetRandomAudioClip()
	{
		int result = -1;
		float num = 0f;
		float value = UnityEngine.Random.value;
		foreach (KeyValuePair<int, float> sound in sounds)
		{
			num += sound.Value;
			if (value <= num)
			{
				result = sound.Key;
				break;
			}
		}
		return result;
	}
}

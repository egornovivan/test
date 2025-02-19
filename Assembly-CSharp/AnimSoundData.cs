using System;
using System.Collections.Generic;
using Mono.Data.SqliteClient;

public class AnimSoundData
{
	public int id;

	public string name;

	public string soundStr;

	private Dictionary<string, int> mSoundList = new Dictionary<string, int>();

	private static List<AnimSoundData> s_tblAnimSoundData = new List<AnimSoundData>();

	public static int GetAnimationSound(int modelId, string anim)
	{
		AnimSoundData animSoundData = s_tblAnimSoundData.Find((AnimSoundData ret) => ret != null && ret.id == modelId);
		if (animSoundData != null && animSoundData.mSoundList.ContainsKey(anim))
		{
			return animSoundData.mSoundList[anim];
		}
		return -1;
	}

	public static void LoadData()
	{
		SqliteDataReader sqliteDataReader = LocalDatabase.Instance.ReadFullTable("MonsterAnimationSound");
		while (sqliteDataReader.Read())
		{
			AnimSoundData animSoundData = new AnimSoundData();
			animSoundData.id = sqliteDataReader.GetInt32(sqliteDataReader.GetOrdinal("model_id"));
			animSoundData.name = sqliteDataReader.GetString(sqliteDataReader.GetOrdinal("model_name"));
			animSoundData.soundStr = sqliteDataReader.GetString(sqliteDataReader.GetOrdinal("anim_music"));
			animSoundData.InitSoundList(animSoundData.soundStr);
			s_tblAnimSoundData.Add(animSoundData);
		}
	}

	private void InitSoundList(string str)
	{
		string[] array = AiUtil.Split(str, ';');
		string[] array2 = array;
		foreach (string value in array2)
		{
			string[] array3 = AiUtil.Split(value, '_');
			if (array3.Length == 2)
			{
				string key = array3[0];
				int value2 = Convert.ToInt32(array3[1]);
				if (!mSoundList.ContainsKey(key))
				{
					mSoundList.Add(key, value2);
				}
			}
		}
	}
}

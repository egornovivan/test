using System;
using System.Collections.Generic;
using Mono.Data.SqliteClient;
using UnityEngine;

namespace SoundAsset;

public class SESoundBuff
{
	private const string SoundPath = "Sound/";

	public int mID;

	public string mName;

	public bool mLoop;

	public int mAudioType;

	public float mDoppler;

	public float mSpatial;

	public float mVolume;

	public float mMinDistance;

	public float mMaxDistance;

	public AudioRolloffMode mMode = AudioRolloffMode.Linear;

	public static List<SESoundBuff> s_tblSeSoundBuffs;

	public static void LoadData()
	{
		s_tblSeSoundBuffs = new List<SESoundBuff>();
		SqliteDataReader sqliteDataReader = LocalDatabase.Instance.ReadFullTable("sound");
		while (sqliteDataReader.Read())
		{
			SESoundBuff sESoundBuff = new SESoundBuff();
			sESoundBuff.mID = Convert.ToInt32(sqliteDataReader.GetString(sqliteDataReader.GetOrdinal("_id")));
			sESoundBuff.mName = Convert.ToString(sqliteDataReader.GetString(sqliteDataReader.GetOrdinal("_name")));
			sESoundBuff.mLoop = Convert.ToBoolean(sqliteDataReader.GetInt32(sqliteDataReader.GetOrdinal("loop")));
			sESoundBuff.mAudioType = Convert.ToInt32(sqliteDataReader.GetString(sqliteDataReader.GetOrdinal("soundType")));
			sESoundBuff.mMode = (AudioRolloffMode)Convert.ToInt32(sqliteDataReader.GetString(sqliteDataReader.GetOrdinal("rolloffType")));
			sESoundBuff.mDoppler = Convert.ToSingle(sqliteDataReader.GetString(sqliteDataReader.GetOrdinal("doppler")));
			sESoundBuff.mSpatial = Convert.ToSingle(sqliteDataReader.GetString(sqliteDataReader.GetOrdinal("spatial")));
			sESoundBuff.mVolume = Convert.ToSingle(sqliteDataReader.GetString(sqliteDataReader.GetOrdinal("volume")));
			sESoundBuff.mMinDistance = Convert.ToSingle(sqliteDataReader.GetString(sqliteDataReader.GetOrdinal("minDistance")));
			sESoundBuff.mMaxDistance = Convert.ToSingle(sqliteDataReader.GetString(sqliteDataReader.GetOrdinal("maxDistance")));
			sESoundBuff.mVolume = Mathf.Clamp01(sESoundBuff.mVolume);
			s_tblSeSoundBuffs.Add(sESoundBuff);
		}
	}

	public static bool MatchId(SESoundBuff iter, int id)
	{
		return iter.mID == id;
	}

	public static SESoundBuff GetSESoundData(int id)
	{
		return s_tblSeSoundBuffs.Find((SESoundBuff ret) => MatchId(ret, id));
	}
}

using System;
using System.Collections.Generic;
using System.Threading;
using Mono.Data.SqliteClient;
using UnityEngine;

namespace SkillSystem;

public class SkData
{
	internal int _id;

	internal int _desc;

	internal string _icon;

	internal string _name;

	internal bool _interruptable;

	internal float _coolingTime;

	internal int _coolingTimeType;

	internal float _coolingTimeShared;

	internal float _pretimeOfPrepare;

	internal float _postimeOfPrepare;

	internal SkEffect _effPrepare;

	internal SkCond _condToLoop;

	internal float[] _pretimeOfMain;

	internal float[] _timeOfMain;

	internal float[] _postimeOfMain;

	internal SkEffect _effMainOneTime;

	internal SkEffect[] _effMainEachTime;

	internal List<List<SkTriggerEvent>> _events = new List<List<SkTriggerEvent>>();

	internal float _pretimeOfEnding;

	internal float _postimeOfEnding;

	internal SkEffect _effEnding;

	internal static Dictionary<int, SkData> s_SkillTbl;

	internal float GetPretimeOfMain(int idx)
	{
		return (idx >= _pretimeOfMain.Length) ? _pretimeOfMain[_pretimeOfMain.Length - 1] : _pretimeOfMain[idx];
	}

	internal float GetTimeOfMain(int idx)
	{
		return (idx >= _timeOfMain.Length) ? _timeOfMain[_timeOfMain.Length - 1] : _timeOfMain[idx];
	}

	internal float GetPostimeOfMain(int idx)
	{
		return (idx >= _postimeOfMain.Length) ? _postimeOfMain[_postimeOfMain.Length - 1] : _postimeOfMain[idx];
	}

	internal List<SkTriggerEvent> GetEvents(int idx)
	{
		return (idx >= _events.Count) ? _events[_events.Count - 1] : _events[idx];
	}

	internal void TryApplyEachEffOfMain(int idx, SkEntity tar, SkRuntimeInfo skrt)
	{
		((idx >= _effMainEachTime.Length) ? _effMainEachTime[_effMainEachTime.Length - 1] : _effMainEachTime[idx])?.Apply(tar, skrt);
	}

	internal static float[] ToSingleArray(string desc)
	{
		string[] array = desc.Split(';');
		int num = array.Length;
		float[] array2 = new float[num];
		for (int i = 0; i < num; i++)
		{
			array2[i] = Convert.ToSingle(array[i]);
		}
		return array2;
	}

	internal static SkEffect[] ToSkEffectArray(string desc)
	{
		string[] array = desc.Split(';');
		int num = array.Length;
		SkEffect[] array2 = new SkEffect[num];
		for (int i = 0; i < num; i++)
		{
			array2[i] = null;
			int key = Convert.ToInt32(array[i]);
			SkEffect.s_SkEffectTbl.TryGetValue(key, out array2[i]);
		}
		return array2;
	}

	public static void LoadData()
	{
		if (s_SkillTbl != null)
		{
			return;
		}
		SkEffect.LoadData();
		SkBuff.LoadData();
		SkTriggerEvent.LoadData();
		SqliteDataReader sqliteDataReader = LocalDatabase.Instance.ReadFullTable("skMain");
		s_SkillTbl = new Dictionary<int, SkData>();
		while (sqliteDataReader.Read())
		{
			SkData skData = new SkData();
			skData._id = Convert.ToInt32(sqliteDataReader.GetString(sqliteDataReader.GetOrdinal("_id")));
			skData._desc = Convert.ToInt32(sqliteDataReader.GetString(sqliteDataReader.GetOrdinal("_desc")));
			skData._icon = sqliteDataReader.GetString(sqliteDataReader.GetOrdinal("_icon"));
			skData._name = sqliteDataReader.GetString(sqliteDataReader.GetOrdinal("_name"));
			skData._interruptable = sqliteDataReader.GetString(sqliteDataReader.GetOrdinal("_interruptable")).Equals("1");
			skData._coolingTime = Convert.ToSingle(sqliteDataReader.GetString(sqliteDataReader.GetOrdinal("_coolingTime")));
			skData._coolingTimeType = Convert.ToInt32(sqliteDataReader.GetString(sqliteDataReader.GetOrdinal("_coolingTimeType")));
			skData._coolingTimeShared = Convert.ToSingle(sqliteDataReader.GetString(sqliteDataReader.GetOrdinal("_coolingTimeShared")));
			skData._pretimeOfPrepare = Convert.ToSingle(sqliteDataReader.GetString(sqliteDataReader.GetOrdinal("_pretimeOfPrepare")));
			skData._postimeOfPrepare = Convert.ToSingle(sqliteDataReader.GetString(sqliteDataReader.GetOrdinal("_postimeOfPrepare")));
			SkEffect.s_SkEffectTbl.TryGetValue(Convert.ToInt32(sqliteDataReader.GetString(sqliteDataReader.GetOrdinal("_effPrepare"))), out skData._effPrepare);
			skData._condToLoop = SkCond.Create(sqliteDataReader.GetString(sqliteDataReader.GetOrdinal("_cond")));
			skData._pretimeOfMain = ToSingleArray(sqliteDataReader.GetString(sqliteDataReader.GetOrdinal("_pretimeOfMain")));
			skData._timeOfMain = ToSingleArray(sqliteDataReader.GetString(sqliteDataReader.GetOrdinal("_timeOfMain")));
			skData._postimeOfMain = ToSingleArray(sqliteDataReader.GetString(sqliteDataReader.GetOrdinal("_postimeOfMain")));
			skData._effMainEachTime = ToSkEffectArray(sqliteDataReader.GetString(sqliteDataReader.GetOrdinal("_effMainEachTime")));
			SkEffect.s_SkEffectTbl.TryGetValue(Convert.ToInt32(sqliteDataReader.GetString(sqliteDataReader.GetOrdinal("_effMainOneTime"))), out skData._effMainOneTime);
			string[] array = sqliteDataReader.GetString(sqliteDataReader.GetOrdinal("_triggerEvents")).Split(';');
			string[] array2 = array;
			foreach (string text in array2)
			{
				List<SkTriggerEvent> list = new List<SkTriggerEvent>();
				string[] array3 = text.Split(',');
				string[] array4 = array3;
				foreach (string value in array4)
				{
					int key = Convert.ToInt32(value);
					SkTriggerEvent.s_SkTriggerEventTbl.TryGetValue(key, out var value2);
					if (value2 != null)
					{
						list.Add(value2);
					}
				}
				skData._events.Add(list);
			}
			skData._pretimeOfEnding = Convert.ToSingle(sqliteDataReader.GetString(sqliteDataReader.GetOrdinal("_pretimeOfEnding")));
			skData._postimeOfEnding = Convert.ToSingle(sqliteDataReader.GetString(sqliteDataReader.GetOrdinal("_postimeOfEnding")));
			SkEffect.s_SkEffectTbl.TryGetValue(Convert.ToInt32(sqliteDataReader.GetString(sqliteDataReader.GetOrdinal("_effEnding"))), out skData._effEnding);
			try
			{
				s_SkillTbl.Add(skData._id, skData);
			}
			catch (Exception ex)
			{
				Debug.LogError("Exception on skMain " + skData._id + " " + ex);
			}
		}
		new Thread(SkInst.s_ExpCompiler.Compile).Start();
	}
}

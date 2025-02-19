using System;
using System.Collections.Generic;
using Mono.Data.SqliteClient;
using UnityEngine;

namespace SkillSystem;

public class SkBuff
{
	internal int _id;

	internal int _desc;

	internal string _icon;

	internal string _name;

	internal int _type;

	internal int _priority;

	internal int _stackLimit;

	internal float _lifeTime;

	internal float _interval;

	internal SkAttribsModifier _mods;

	internal SkEffect _eff;

	internal SkEffect _effBeg;

	internal SkEffect _effEnd;

	internal static Dictionary<int, SkBuff> s_SkBuffTbl;

	internal static int s_maxId;

	public static void LoadData()
	{
		if (s_SkBuffTbl != null)
		{
			return;
		}
		SqliteDataReader sqliteDataReader = LocalDatabase.Instance.ReadFullTable("skBuff");
		s_SkBuffTbl = new Dictionary<int, SkBuff>();
		while (sqliteDataReader.Read())
		{
			SkBuff skBuff = new SkBuff();
			skBuff._id = Convert.ToInt32(sqliteDataReader.GetString(sqliteDataReader.GetOrdinal("_id")));
			skBuff._desc = Convert.ToInt32(sqliteDataReader.GetString(sqliteDataReader.GetOrdinal("_desc")));
			skBuff._icon = sqliteDataReader.GetString(sqliteDataReader.GetOrdinal("_icon"));
			skBuff._name = sqliteDataReader.GetString(sqliteDataReader.GetOrdinal("_name"));
			skBuff._type = Convert.ToInt32(sqliteDataReader.GetString(sqliteDataReader.GetOrdinal("_type")));
			skBuff._priority = Convert.ToInt32(sqliteDataReader.GetString(sqliteDataReader.GetOrdinal("_priority")));
			skBuff._stackLimit = Convert.ToInt32(sqliteDataReader.GetString(sqliteDataReader.GetOrdinal("_stackLimit")));
			skBuff._lifeTime = Convert.ToSingle(sqliteDataReader.GetString(sqliteDataReader.GetOrdinal("_timeActive")));
			skBuff._interval = Convert.ToSingle(sqliteDataReader.GetString(sqliteDataReader.GetOrdinal("_timeInterval")));
			skBuff._mods = SkAttribsModifier.Create(sqliteDataReader.GetString(sqliteDataReader.GetOrdinal("_mods")));
			SkEffect.s_SkEffectTbl.TryGetValue(Convert.ToInt32(sqliteDataReader.GetString(sqliteDataReader.GetOrdinal("_eff"))), out skBuff._eff);
			SkEffect.s_SkEffectTbl.TryGetValue(Convert.ToInt32(sqliteDataReader.GetString(sqliteDataReader.GetOrdinal("_effBeg"))), out skBuff._effBeg);
			SkEffect.s_SkEffectTbl.TryGetValue(Convert.ToInt32(sqliteDataReader.GetString(sqliteDataReader.GetOrdinal("_effEnd"))), out skBuff._effEnd);
			try
			{
				s_SkBuffTbl.Add(skBuff._id, skBuff);
			}
			catch (Exception ex)
			{
				Debug.LogError("Exception on skBuff " + skBuff._id + " " + ex);
			}
			if (s_maxId < skBuff._id)
			{
				s_maxId = skBuff._id;
			}
		}
	}

	public static int CreateBuff(int tmplBuffId, string modDesc)
	{
		SkBuff skBuff = s_SkBuffTbl[tmplBuffId];
		SkBuff skBuff2 = new SkBuff();
		skBuff2._id = ++s_maxId;
		skBuff2._icon = skBuff._icon;
		skBuff2._desc = skBuff._desc;
		skBuff2._name = skBuff._name;
		skBuff2._type = skBuff._type;
		skBuff2._priority = skBuff._priority;
		skBuff2._stackLimit = skBuff._stackLimit;
		skBuff2._lifeTime = skBuff._lifeTime;
		skBuff2._interval = skBuff._interval;
		skBuff2._mods = SkAttribsModifier.Create(modDesc);
		skBuff2._eff = skBuff._eff;
		skBuff2._effBeg = skBuff._effBeg;
		skBuff2._effEnd = skBuff._effEnd;
		s_SkBuffTbl.Add(skBuff2._id, skBuff2);
		if (s_maxId < skBuff2._id)
		{
			s_maxId = skBuff2._id;
		}
		return s_maxId;
	}
}

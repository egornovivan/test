using System;
using System.Collections;
using System.Collections.Generic;
using Mono.Data.SqliteClient;
using UnityEngine;

namespace SkillAsset;

public class EffSkillBuff
{
	public const int MIN_CD_TYPE = 1;

	public const int NUM_CD_TYPE = 12;

	public const int MAX_CD_TYPE = 12;

	public const int CustomIdStart = 100000000;

	public int m_id;

	public string m_buffName;

	public string m_iconImgPath;

	public string m_buffHint;

	public int m_buffType;

	public int m_StackLimit;

	public int m_Priority;

	public float m_timeActive;

	public float m_timeInterval;

	public float m_hpChange;

	public short m_buffSp;

	public float m_timeCoolingChange;

	public float m_spdChange;

	public float m_atkChange;

	public float m_atkChangeP;

	public float m_atkDistChange;

	public float m_defChange;

	public float m_defChangeP;

	public float m_block;

	public float m_jumpHeight;

	public float m_fallInjuries;

	public float m_hpMaxChange;

	public float m_hpMaxChangeP;

	public float m_satiationMaxChange;

	public float m_comfortMaxChange;

	public float m_satiationDecSpdChange;

	public float m_comfortDecSpdChange;

	public float m_collectLv;

	public float m_satiationChange;

	public float m_comfortChange;

	public short m_resGotMultiplier;

	public float m_resGotRadius;

	public int m_changeCamp;

	private static int mMaxBuffId;

	public static List<string> s_tblEffSkillBuffsColName_CN;

	public static List<string> s_tblEffSkillBuffsColName_EN;

	public static List<EffSkillBuff> s_tblEffSkillBuffs;

	public static object elem(int id, string colname)
	{
		EffSkillBuff effSkillBuff = s_tblEffSkillBuffs.Find((EffSkillBuff iterSkill0) => MatchId(iterSkill0, id));
		if (effSkillBuff != null)
		{
			if (colname.CompareTo("_timeCoolingChange") == 0)
			{
				return effSkillBuff.m_timeCoolingChange;
			}
			if (colname.CompareTo("_spdChange") == 0)
			{
				return effSkillBuff.m_timeCoolingChange;
			}
			if (colname.CompareTo("_typeSpd") == 0)
			{
				return effSkillBuff.m_timeCoolingChange;
			}
			if (colname.CompareTo("_type3Time") == 0)
			{
				return effSkillBuff.m_timeCoolingChange;
			}
			if (colname.CompareTo("_type4Time") == 0)
			{
				return effSkillBuff.m_timeCoolingChange;
			}
			if (colname.CompareTo("_damage") == 0)
			{
				return effSkillBuff.m_timeCoolingChange;
			}
			if (colname.CompareTo("_atkChange") == 0)
			{
				return effSkillBuff.m_timeCoolingChange;
			}
			if (colname.CompareTo("_defChange") == 0)
			{
				return effSkillBuff.m_timeCoolingChange;
			}
			if (colname.CompareTo("_block") == 0)
			{
				return effSkillBuff.m_timeCoolingChange;
			}
			if (colname.CompareTo("_jumpHeight") == 0)
			{
				return effSkillBuff.m_timeCoolingChange;
			}
			if (colname.CompareTo("_fallInjuries") == 0)
			{
				return effSkillBuff.m_timeCoolingChange;
			}
			if (colname.CompareTo("_hpMaxChange") == 0)
			{
				return effSkillBuff.m_timeCoolingChange;
			}
			if (colname.CompareTo("_satiationMaxChange") == 0)
			{
				return effSkillBuff.m_timeCoolingChange;
			}
			if (colname.CompareTo("_comfortMaxChange") == 0)
			{
				return effSkillBuff.m_timeCoolingChange;
			}
			if (colname.CompareTo("_sitiationDecSpdChange") == 0)
			{
				return effSkillBuff.m_timeCoolingChange;
			}
			if (colname.CompareTo("_comfortDecSpdChange") == 0)
			{
				return effSkillBuff.m_timeCoolingChange;
			}
			if (colname.CompareTo("_extralPoint") == 0)
			{
				return effSkillBuff.m_timeCoolingChange;
			}
			if (colname.CompareTo("_extralResource") == 0)
			{
				return effSkillBuff.m_timeCoolingChange;
			}
		}
		return null;
	}

	public IEnumerator Exec(SkillRunner caster, SkillRunner buffHost, EffSkillBuffInst buffInst)
	{
		if (!buffHost.m_effSkillBuffManager.Add(buffInst))
		{
			yield break;
		}
		int times = 1;
		float timeInterval = m_timeActive;
		if (timeInterval > -1E-45f)
		{
			if (m_timeInterval > float.Epsilon)
			{
				times = (int)(m_timeActive / m_timeInterval);
				timeInterval = m_timeInterval;
			}
			for (int i = 0; i < times; i++)
			{
				if (!GameConfig.IsMultiMode)
				{
					buffHost.ApplyHpChange(null, m_hpChange, 0f, 0);
					buffHost.ApplyComfortChange(m_comfortChange);
					buffHost.ApplySatiationChange(m_satiationChange);
					buffHost.ApplyBuffContinuous(caster, m_buffSp);
				}
				yield return new WaitForSeconds(timeInterval);
			}
			buffHost.m_effSkillBuffManager.Remove(buffInst);
			yield break;
		}
		times = 0;
		while (true)
		{
			yield return 0;
		}
	}

	public static void LoadData()
	{
		mMaxBuffId = 0;
		SqliteDataReader sqliteDataReader = LocalDatabase.Instance.ReadFullTable("buff");
		int fieldCount = sqliteDataReader.FieldCount;
		s_tblEffSkillBuffsColName_CN = new List<string>(fieldCount);
		sqliteDataReader.Read();
		for (int i = 0; i < fieldCount; i++)
		{
			s_tblEffSkillBuffsColName_CN.Add(sqliteDataReader.GetString(i));
		}
		s_tblEffSkillBuffsColName_EN = new List<string>(fieldCount);
		sqliteDataReader.Read();
		for (int j = 0; j < fieldCount; j++)
		{
			s_tblEffSkillBuffsColName_EN.Add(sqliteDataReader.GetString(j));
		}
		s_tblEffSkillBuffs = new List<EffSkillBuff>();
		while (sqliteDataReader.Read())
		{
			EffSkillBuff effSkillBuff = new EffSkillBuff();
			effSkillBuff.m_id = Convert.ToInt32(sqliteDataReader.GetString(sqliteDataReader.GetOrdinal("_buffid")));
			effSkillBuff.m_buffName = sqliteDataReader.GetString(sqliteDataReader.GetOrdinal("_buffname"));
			effSkillBuff.m_iconImgPath = sqliteDataReader.GetString(sqliteDataReader.GetOrdinal("_bufficon"));
			effSkillBuff.m_buffHint = PELocalization.GetString(Convert.ToInt32(sqliteDataReader.GetString(sqliteDataReader.GetOrdinal("_buffhint"))));
			effSkillBuff.m_buffType = Convert.ToInt32(sqliteDataReader.GetString(sqliteDataReader.GetOrdinal("_type")));
			effSkillBuff.m_StackLimit = Convert.ToInt32(sqliteDataReader.GetString(sqliteDataReader.GetOrdinal("_buffStackLimit")));
			effSkillBuff.m_Priority = Convert.ToInt32(sqliteDataReader.GetString(sqliteDataReader.GetOrdinal("_priority")));
			effSkillBuff.m_changeCamp = Convert.ToInt32(sqliteDataReader.GetString(sqliteDataReader.GetOrdinal("_changeCamp")));
			effSkillBuff.m_timeActive = Convert.ToSingle(sqliteDataReader.GetString(sqliteDataReader.GetOrdinal("_timeActive")));
			effSkillBuff.m_timeInterval = Convert.ToSingle(sqliteDataReader.GetString(sqliteDataReader.GetOrdinal("_timeInterval")));
			effSkillBuff.m_hpChange = Convert.ToSingle(sqliteDataReader.GetString(sqliteDataReader.GetOrdinal("_hpChange")));
			ReadBuffSp(sqliteDataReader, effSkillBuff);
			effSkillBuff.m_timeCoolingChange = Convert.ToSingle(sqliteDataReader.GetString(sqliteDataReader.GetOrdinal("_timeCoolingChange")));
			effSkillBuff.m_spdChange = Convert.ToSingle(sqliteDataReader.GetString(sqliteDataReader.GetOrdinal("_spdChange")));
			effSkillBuff.m_atkChange = Convert.ToSingle(sqliteDataReader.GetString(sqliteDataReader.GetOrdinal("_atkChange")));
			effSkillBuff.m_atkChangeP = Convert.ToSingle(sqliteDataReader.GetString(sqliteDataReader.GetOrdinal("_atkChangeP")));
			effSkillBuff.m_atkDistChange = Convert.ToSingle(sqliteDataReader.GetString(sqliteDataReader.GetOrdinal("_atkDistChange")));
			effSkillBuff.m_defChange = Convert.ToSingle(sqliteDataReader.GetString(sqliteDataReader.GetOrdinal("_defChange")));
			effSkillBuff.m_defChangeP = Convert.ToSingle(sqliteDataReader.GetString(sqliteDataReader.GetOrdinal("_defChangeP")));
			effSkillBuff.m_block = Convert.ToSingle(sqliteDataReader.GetString(sqliteDataReader.GetOrdinal("_block")));
			effSkillBuff.m_jumpHeight = Convert.ToSingle(sqliteDataReader.GetString(sqliteDataReader.GetOrdinal("_jumpHeight")));
			effSkillBuff.m_fallInjuries = Convert.ToSingle(sqliteDataReader.GetString(sqliteDataReader.GetOrdinal("_fallInjuries")));
			effSkillBuff.m_hpMaxChange = Convert.ToSingle(sqliteDataReader.GetString(sqliteDataReader.GetOrdinal("_hpMaxChange")));
			effSkillBuff.m_hpMaxChangeP = Convert.ToSingle(sqliteDataReader.GetString(sqliteDataReader.GetOrdinal("_hpMaxChangeP")));
			effSkillBuff.m_satiationMaxChange = Convert.ToSingle(sqliteDataReader.GetString(sqliteDataReader.GetOrdinal("_satiationMaxChange")));
			effSkillBuff.m_comfortMaxChange = Convert.ToSingle(sqliteDataReader.GetString(sqliteDataReader.GetOrdinal("_comfortMaxChange")));
			effSkillBuff.m_satiationDecSpdChange = Convert.ToSingle(sqliteDataReader.GetString(sqliteDataReader.GetOrdinal("_sitiationDecSpdChange")));
			effSkillBuff.m_comfortDecSpdChange = Convert.ToSingle(sqliteDataReader.GetString(sqliteDataReader.GetOrdinal("_comfortDecSpdChange")));
			effSkillBuff.m_collectLv = Convert.ToSingle(sqliteDataReader.GetString(sqliteDataReader.GetOrdinal("_collectLv")));
			effSkillBuff.m_resGotMultiplier = Convert.ToInt16(sqliteDataReader.GetString(sqliteDataReader.GetOrdinal("_resoucesGot")));
			effSkillBuff.m_resGotRadius = Convert.ToInt16(sqliteDataReader.GetString(sqliteDataReader.GetOrdinal("_resGotRadius")));
			s_tblEffSkillBuffs.Add(effSkillBuff);
		}
	}

	public static void AddNewBuff(EffSkillBuff buff)
	{
		int num = s_tblEffSkillBuffs.FindIndex((EffSkillBuff itr) => MatchId(itr, buff.m_id));
		if (num == -1)
		{
			s_tblEffSkillBuffs.Add(buff);
		}
		else
		{
			s_tblEffSkillBuffs[num] = buff;
		}
	}

	public static void ClearCustomBuff()
	{
		int num = s_tblEffSkillBuffs.Count - 1;
		while (num >= 0 && s_tblEffSkillBuffs[num].m_id >= 100000000)
		{
			s_tblEffSkillBuffs.RemoveAt(num);
			num--;
		}
	}

	public static void RemoveBuff(EffSkillBuff buff)
	{
		if (s_tblEffSkillBuffs.Contains(buff))
		{
			s_tblEffSkillBuffs.Remove(buff);
		}
	}

	public static bool MatchId(EffSkillBuff iter, int id)
	{
		return iter.m_id == id;
	}

	private static void ReadBuffSp(SqliteDataReader reader, EffSkillBuff buff)
	{
		string @string = reader.GetString(reader.GetOrdinal("_buffSp"));
		string[] array = @string.Split(',');
		buff.m_buffSp = 0;
		string[] array2 = array;
		foreach (string value in array2)
		{
			int num = Convert.ToInt32(value);
			if (num > 0)
			{
				buff.m_buffSp |= (short)(1 << num - 1);
			}
		}
	}
}

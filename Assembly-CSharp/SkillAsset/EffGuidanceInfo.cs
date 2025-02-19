using System;
using System.Collections.Generic;
using AiAsset;
using Mono.Data.SqliteClient;
using Pathea;
using UnityEngine;

namespace SkillAsset;

public class EffGuidanceInfo
{
	internal int m_damageType;

	internal float m_timeCost;

	internal float m_timeInterval;

	internal float m_hpChangeOnce;

	internal float m_hpChangePercent;

	internal float m_ComfortChange;

	internal float m_OxygenChange;

	internal float m_durChangeOnce;

	internal float m_sitiationChangeOnce;

	internal float m_thirstLvChangeOnce;

	internal float m_distRepelOnce;

	internal List<EffSkillBuff> m_buffList;

	internal List<int> m_buffIDList;

	internal List<int> m_effIdList;

	internal List<string> m_animNameList;

	internal List<int> m_TargetEffIDList;

	internal List<int> m_GuidanceSound;

	internal Dictionary<int, float> mPropertyChange;

	internal float m_DelayTime;

	internal float m_SoundDelayTime;

	internal GroundScope m_GroundScope;

	internal EffGuidanceInfo()
	{
		m_buffIDList = new List<int>();
		m_buffList = new List<EffSkillBuff>();
		m_effIdList = new List<int>();
		m_animNameList = new List<string>();
		m_TargetEffIDList = new List<int>();
		m_GuidanceSound = new List<int>();
		mPropertyChange = new Dictionary<int, float>();
	}

	public EffGuidanceInfo(int buffID)
	{
		m_timeCost = 0f;
		m_timeInterval = 0f;
		m_hpChangeOnce = 0f;
		m_hpChangePercent = 0f;
		m_durChangeOnce = 0f;
		m_sitiationChangeOnce = 0f;
		m_thirstLvChangeOnce = 0f;
		m_distRepelOnce = 0f;
		m_effIdList = null;
		m_TargetEffIDList = null;
		EffSkillBuff effSkillBuff = EffSkillBuff.s_tblEffSkillBuffs.Find((EffSkillBuff iter0) => EffSkillBuff.MatchId(iter0, buffID));
		if (effSkillBuff != null)
		{
			m_buffList.Add(effSkillBuff);
		}
	}

	internal static EffGuidanceInfo Create(SqliteDataReader reader)
	{
		EffGuidanceInfo ret = new EffGuidanceInfo();
		ret.m_damageType = Convert.ToInt32(reader.GetString(reader.GetOrdinal("_attacktype")));
		ret.m_timeCost = Convert.ToSingle(reader.GetString(reader.GetOrdinal("_guidanceTime")));
		ret.m_timeInterval = Convert.ToSingle(reader.GetString(reader.GetOrdinal("_timeInterval")));
		ret.m_hpChangeOnce = Convert.ToSingle(reader.GetString(reader.GetOrdinal("_harmBase")));
		ret.m_hpChangePercent = Convert.ToSingle(reader.GetString(reader.GetOrdinal("_harmPercent")));
		ret.m_ComfortChange = Convert.ToSingle(reader.GetString(reader.GetOrdinal("_sitiationReply")));
		ret.m_OxygenChange = Convert.ToSingle(reader.GetString(reader.GetOrdinal("_waterReply")));
		ret.m_durChangeOnce = Convert.ToSingle(reader.GetString(reader.GetOrdinal("_damage")));
		ret.m_sitiationChangeOnce = Convert.ToSingle(reader.GetString(reader.GetOrdinal("_sitiationReply")));
		ret.m_thirstLvChangeOnce = Convert.ToSingle(reader.GetString(reader.GetOrdinal("_waterReply")));
		ret.m_distRepelOnce = Convert.ToSingle(reader.GetString(reader.GetOrdinal("_distRepel")));
		ret.m_buffIDList = EffSkill.ToListInt32P(reader.GetString(reader.GetOrdinal("_addBuff")));
		ret.m_effIdList = EffSkill.ToListInt32P(reader.GetString(reader.GetOrdinal("casting_eff")));
		ret.m_TargetEffIDList = EffSkill.ToListInt32P(reader.GetString(reader.GetOrdinal("target_eff")));
		ret.m_GuidanceSound = EffSkill.ToListInt32P(reader.GetString(reader.GetOrdinal("_guidanceSound")));
		ret.m_DelayTime = Convert.ToSingle(reader.GetString(reader.GetOrdinal("_guidanceDelay")));
		ret.m_SoundDelayTime = Convert.ToSingle(reader.GetString(reader.GetOrdinal("_delayG")));
		ret.m_GroundScope = GroundScope.Create(reader);
		string @string = reader.GetString(reader.GetOrdinal("_prChange1"));
		if ("0" != @string)
		{
			string[] array = reader.GetString(reader.GetOrdinal("_prChange1")).Split(';');
			for (int i = 0; i < array.Length; i++)
			{
				string[] array2 = array[i].Split(',');
				ret.mPropertyChange[Convert.ToInt32(array2[0])] = Convert.ToSingle(array2[1]);
			}
		}
		if (ret.m_buffIDList != null)
		{
			for (int j = 0; j < ret.m_buffIDList.Count; j++)
			{
				EffSkillBuff effSkillBuff = EffSkillBuff.s_tblEffSkillBuffs.Find((EffSkillBuff iter0) => EffSkillBuff.MatchId(iter0, ret.m_buffIDList[j]));
				if (effSkillBuff != null)
				{
					ret.m_buffList.Add(effSkillBuff);
				}
			}
		}
		ret.m_animNameList = EffSkill.ToListString(reader.GetString(reader.GetOrdinal("_guidanceAction")));
		return ret;
	}

	internal bool TakeEffectProxy(SkillRunner caster, List<ISkillTarget> targetList, int skillId, bool bAffectCaster)
	{
		if (m_TargetEffIDList != null)
		{
			foreach (int targetEffID in m_TargetEffIDList)
			{
				foreach (ISkillTarget target in targetList)
				{
					EffectManager.Instance.Instantiate(targetEffID, target.GetPosition(), Quaternion.identity);
				}
			}
		}
		bool result = false;
		if (bAffectCaster)
		{
			targetList.Add(caster);
		}
		for (int i = 0; i < targetList.Count; i++)
		{
			SkillRunner skillRunner;
			if (!((skillRunner = targetList[i] as SkillRunner) != null) || m_buffList == null)
			{
				continue;
			}
			for (int j = 0; j < m_buffList.Count; j++)
			{
				if (!(m_buffList[j].m_timeActive < -2f))
				{
					if (m_buffList[j].m_timeActive < -1f)
					{
						EffSkillBuffInst.TakeEffect(caster, skillRunner, m_buffList[j], skillId);
					}
					else
					{
						EffSkillBuffInst.TakeEffect(caster, skillRunner, m_buffList[j], skillId);
					}
				}
			}
			for (int k = 0; k < m_buffIDList.Count; k++)
			{
				skillRunner.BuffAttribs += m_buffIDList[k];
			}
		}
		if (bAffectCaster)
		{
			targetList.Remove(caster);
		}
		return result;
	}

	internal bool TakeEffect(SkillRunner caster, List<ISkillTarget> targetList, int skillId)
	{
		return true;
	}

	private void DestoryTerrain(SkillRunner caster)
	{
		float durChangeOnce = m_durChangeOnce;
		if (durChangeOnce < -1E-45f)
		{
			durChangeOnce = caster.GetAttribute(AttribType.Atk) * m_hpChangePercent;
			durChangeOnce *= AiDamageTypeData.GetDamageScale((m_damageType != 0) ? m_damageType : AiDamageTypeData.GetDamageType(caster), 10);
		}
	}
}

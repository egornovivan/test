using System;
using System.Collections.Generic;
using Mono.Data.SqliteClient;

namespace SkillAsset;

public class EffPrepareInfo
{
	internal float m_timeCost;

	internal List<int> m_effIdList;

	internal List<string> m_animNameList;

	internal List<int> m_ReadySound;

	internal float m_DelayR;

	internal static EffPrepareInfo Create(SqliteDataReader reader)
	{
		float num = Convert.ToSingle(reader.GetString(reader.GetOrdinal("_timePrep")));
		List<int> list = EffSkill.ToListInt32P(reader.GetString(reader.GetOrdinal("precast_eff")));
		List<string> list2 = EffSkill.ToListString(reader.GetString(reader.GetOrdinal("_prepareAction")));
		List<int> readySound = EffSkill.ToListInt32P(reader.GetString(reader.GetOrdinal("_readySound")));
		float delayR = Convert.ToSingle(reader.GetString(reader.GetOrdinal("_delayR")));
		if (num < float.Epsilon && list == null && list2 == null)
		{
			return null;
		}
		EffPrepareInfo effPrepareInfo = new EffPrepareInfo();
		effPrepareInfo.m_timeCost = num;
		effPrepareInfo.m_effIdList = list;
		effPrepareInfo.m_animNameList = list2;
		effPrepareInfo.m_ReadySound = readySound;
		effPrepareInfo.m_DelayR = delayR;
		return effPrepareInfo;
	}

	internal void Prepare(SkillRunner caster, ISkillTarget target)
	{
		if (m_animNameList != null)
		{
			caster.ApplyAnim(m_animNameList);
		}
		if (m_effIdList != null)
		{
			caster.ApplyEffect(m_effIdList, target);
		}
	}
}

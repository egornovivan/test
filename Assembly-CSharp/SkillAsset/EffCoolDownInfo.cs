using System;
using Mono.Data.SqliteClient;

namespace SkillAsset;

public class EffCoolDownInfo
{
	internal float m_timeCost;

	internal short m_type;

	internal float m_timeShared;

	internal static EffCoolDownInfo Create(SqliteDataReader reader)
	{
		EffCoolDownInfo effCoolDownInfo = new EffCoolDownInfo();
		effCoolDownInfo.m_timeCost = Convert.ToSingle(reader.GetString(reader.GetOrdinal("_timeCooling")));
		effCoolDownInfo.m_type = Convert.ToInt16(reader.GetString(reader.GetOrdinal("_typeTimeCooling")));
		effCoolDownInfo.m_timeShared = Convert.ToSingle(reader.GetString(reader.GetOrdinal("_shareTimeCooling")));
		return effCoolDownInfo;
	}
}

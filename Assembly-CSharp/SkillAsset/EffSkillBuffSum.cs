using System.Collections.Generic;

namespace SkillAsset;

public class EffSkillBuffSum
{
	public int m_camp;

	public short m_buffSp;

	public float m_spd = 1f;

	public float m_atk;

	public float m_atkDist;

	public float m_def;

	public float m_block = 1f;

	public float m_hpMax;

	public float m_satiationMax;

	public float m_comfortMax;

	public float m_satiationDecSpd;

	public float m_comfortDecSpd;

	public float m_jumpHeight;

	public short m_resGotMultiplier;

	public float m_resGotRadius;

	public EffSkillBuffSum()
	{
		m_camp = -1;
		m_buffSp = 0;
		m_spd = 1f;
		m_atk = 0f;
		m_atkDist = 0f;
		m_def = 0f;
		m_block = 1f;
		m_hpMax = 0f;
		m_satiationMax = 0f;
		m_comfortMax = 0f;
		m_satiationDecSpd = 1f;
		m_comfortDecSpd = 1f;
		m_jumpHeight = 0f;
		m_resGotMultiplier = 0;
		m_resGotRadius = 0.1f;
	}

	public void Clear()
	{
		m_camp = -1;
		m_buffSp = 0;
		m_spd = 1f;
		m_atk = 0f;
		m_atkDist = 0f;
		m_def = 0f;
		m_block = 1f;
		m_hpMax = 0f;
		m_satiationMax = 0f;
		m_comfortMax = 0f;
		m_satiationDecSpd = 1f;
		m_comfortDecSpd = 1f;
		m_jumpHeight = 0f;
		m_resGotMultiplier = 0;
		m_resGotRadius = 0.1f;
	}

	public EffSkillBuffSum SumupToMe(List<EffSkillBuffInst> buffInstList)
	{
		for (int i = 0; i < buffInstList.Count; i++)
		{
			EffSkillBuff buff = buffInstList[i].m_buff;
			m_camp = buff.m_changeCamp;
			m_buffSp |= buff.m_buffSp;
			m_spd *= 1f + buff.m_spdChange;
			m_atk += buff.m_atkChange;
			m_atkDist += buff.m_atkDistChange;
			m_def += buff.m_defChange;
			m_block *= 1f + buff.m_block;
			m_hpMax += buff.m_hpMaxChange;
			m_satiationMax += buff.m_satiationMaxChange;
			m_comfortMax += buff.m_comfortMaxChange;
			m_satiationDecSpd *= 1f + buff.m_satiationDecSpdChange;
			m_comfortDecSpd *= 1f + buff.m_comfortDecSpdChange;
			m_jumpHeight += buff.m_jumpHeight;
			m_resGotMultiplier += buff.m_resGotMultiplier;
			m_resGotRadius += buff.m_resGotRadius;
		}
		return this;
	}

	public static EffSkillBuffSum Sumup(EffSkillBuffSum buffSum, List<EffSkillBuffInst> buffInstList)
	{
		EffSkillBuffSum effSkillBuffSum = ((buffSum == null) ? new EffSkillBuffSum() : ((EffSkillBuffSum)buffSum.MemberwiseClone()));
		return effSkillBuffSum.SumupToMe(buffInstList);
	}

	public static EffSkillBuffSum Sumup(List<EffSkillBuffInst> buffInstList)
	{
		return Sumup(null, buffInstList);
	}
}

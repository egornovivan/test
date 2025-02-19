using System.Collections.Generic;
using Pathea;
using UnityEngine;

public class PEAbnormalNotice
{
	private List<PEAbnormalTrigger> m_Triggers = new List<PEAbnormalTrigger>();

	private List<PEAbnormalHit> m_HitRates = new List<PEAbnormalHit>();

	private List<PEAbnormalEff> m_Effs = new List<PEAbnormalEff>();

	private PeEntity m_Entity;

	private PEAbnormalNoticeData m_Data;

	public void Init(PeEntity entity, PEAbnormalNoticeData data)
	{
		m_Entity = entity;
		m_Data = data;
		InitTriggers();
		InitHits();
		InitEffects();
	}

	public void Update()
	{
		bool flag = m_Triggers.Count > 0;
		for (int i = 0; i < m_Triggers.Count; i++)
		{
			flag = flag && m_Triggers[i].Hit();
			m_Triggers[i].Update();
		}
		if (flag && !m_Entity.IsDeath())
		{
			CheckHit();
		}
		for (int j = 0; j < m_HitRates.Count; j++)
		{
			m_HitRates[j].Update();
		}
	}

	private void CheckHit()
	{
		float num = 1f;
		for (int i = 0; i < m_HitRates.Count; i++)
		{
			m_HitRates[i].preHit = num > float.Epsilon;
			num *= m_HitRates[i].HitRate();
		}
		if (num > float.Epsilon && Random.value < num)
		{
			ApplyEffect();
		}
	}

	private void ApplyEffect()
	{
		for (int i = 0; i < m_Effs.Count; i++)
		{
			m_Effs[i].Do();
		}
		for (int j = 0; j < m_Triggers.Count; j++)
		{
			m_Triggers.Clear();
		}
	}

	private void InitTriggers()
	{
		if (m_Data.trigger_TimeInterval > float.Epsilon)
		{
			PEAT_Time pEAT_Time = new PEAT_Time();
			pEAT_Time.interval = m_Data.trigger_TimeInterval;
			m_Triggers.Add(pEAT_Time);
		}
		if (m_Data.trigger_AbnormalHit != null && null != m_Entity.Alnormal)
		{
			PEAT_AbnormalHit pEAT_AbnormalHit = new PEAT_AbnormalHit();
			pEAT_AbnormalHit.hitAbnormals = m_Data.trigger_AbnormalHit;
			m_Entity.Alnormal.evtStart += pEAT_AbnormalHit.OnHitAbnormal;
			m_Triggers.Add(pEAT_AbnormalHit);
		}
	}

	private void InitHits()
	{
		if (m_Data.hit_Attr != null)
		{
			PEAH_Attr pEAH_Attr = new PEAH_Attr();
			pEAH_Attr.attrs = m_Data.hit_Attr;
			pEAH_Attr.entity = m_Entity;
			m_HitRates.Add(pEAH_Attr);
		}
		if (m_Data.hit_AreaTime != null)
		{
			PEAH_AreaTime pEAH_AreaTime = new PEAH_AreaTime();
			pEAH_AreaTime.entity = m_Entity;
			pEAH_AreaTime.values = m_Data.hit_AreaTime;
			m_HitRates.Add(pEAH_AreaTime);
		}
		if (m_Data.hit_HitRate > float.Epsilon)
		{
			PEAH_Rate pEAH_Rate = new PEAH_Rate();
			pEAH_Rate.rate = m_Data.hit_HitRate;
			m_HitRates.Add(pEAH_Rate);
		}
	}

	private void InitEffects()
	{
		if (m_Data.eff_HumanAudio != 0)
		{
			PEAE_HumanAudio pEAE_HumanAudio = new PEAE_HumanAudio();
			pEAE_HumanAudio.sex = (int)((!(null != m_Entity.commonCmpt)) ? PeSex.Female : m_Entity.commonCmpt.sex);
			pEAE_HumanAudio.audioID = m_Data.eff_HumanAudio;
			pEAE_HumanAudio.entity = m_Entity;
			m_Effs.Add(pEAE_HumanAudio);
		}
		if (m_Data.eff_Contents != null)
		{
			PEAE_Content pEAE_Content = new PEAE_Content();
			pEAE_Content.contentIDs = m_Data.eff_Contents;
			pEAE_Content.entityInfo = m_Entity.enityInfoCmpt;
			m_Effs.Add(pEAE_Content);
		}
	}
}

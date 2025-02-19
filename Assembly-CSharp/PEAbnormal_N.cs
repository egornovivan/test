using System;
using System.Collections.Generic;
using System.IO;
using Pathea;
using PETools;
using SkillSystem;
using UnityEngine;

public class PEAbnormal_N
{
	private const int Version = 100;

	private PeEntity m_Entity;

	private AbnormalData m_Data;

	private bool m_bReqApplyEff;

	private AbnormalConditionCmpt m_AbnormalCmpt;

	private List<PEAbnormalTrigger> m_Triggers = new List<PEAbnormalTrigger>();

	private List<PEAbnormalHit> m_HitRates = new List<PEAbnormalHit>();

	private List<PEAbnormalEff> m_Effs = new List<PEAbnormalEff>();

	private List<PEAbnormalTrigger> m_RemoveTriggers = new List<PEAbnormalTrigger>();

	private List<PEAbnormalHit> m_RemoveHits = new List<PEAbnormalHit>();

	private List<PEAbnormalEff> m_RemoveEffs = new List<PEAbnormalEff>();

	private List<int> m_NeedSaveBuffList = new List<int>();

	private List<int> m_SaveBuffList = new List<int>();

	private bool m_EndImm;

	public virtual PEAbnormalType type => m_Data.type;

	public bool hasEffect { get; protected set; }

	public bool effectEnd
	{
		get
		{
			for (int i = 0; i < m_Effs.Count; i++)
			{
				if (!m_Effs[i].effectEnd)
				{
					return false;
				}
			}
			return true;
		}
	}

	private event Action<PEAbnormalType> evtStart;

	private event Action<PEAbnormalType> evtEnd;

	public void Init(PEAbnormalType abnormalType, AbnormalConditionCmpt ctrlCmpt, PeEntity entity, Action<PEAbnormalType> startEvtFunc, Action<PEAbnormalType> endEvtFunc)
	{
		m_AbnormalCmpt = ctrlCmpt;
		m_Entity = entity;
		this.evtStart = (Action<PEAbnormalType>)Delegate.Combine(this.evtStart, startEvtFunc);
		this.evtEnd = (Action<PEAbnormalType>)Delegate.Combine(this.evtEnd, endEvtFunc);
		InitData(abnormalType);
	}

	public byte[] Serialize()
	{
		if (!hasEffect)
		{
			return null;
		}
		m_SaveBuffList.Clear();
		for (int i = 0; i < m_NeedSaveBuffList.Count; i++)
		{
			if (m_Entity.skEntity.GetSkBuffInst(m_NeedSaveBuffList[i]) != null)
			{
				m_SaveBuffList.Add(m_NeedSaveBuffList[i]);
			}
		}
		return PETools.Serialize.Export(delegate(BinaryWriter w)
		{
			w.Write(100);
			w.Write(m_SaveBuffList.Count);
			for (int j = 0; j < m_SaveBuffList.Count; j++)
			{
				w.Write(m_SaveBuffList[j]);
			}
		});
	}

	public void Deserialize(byte[] data)
	{
		if (data == null)
		{
			return;
		}
		PETools.Serialize.Import(data, delegate(BinaryReader r)
		{
			int num = r.ReadInt32();
			if (num == 100)
			{
				int num2 = r.ReadInt32();
				if (num2 > 0)
				{
					hasEffect = true;
					while (num2-- > 0)
					{
						SkEntity.MountBuff(m_Entity.skEntity, r.ReadInt32(), null, null);
					}
				}
			}
		});
		m_bReqApplyEff = true;
	}

	public void OnDie()
	{
		if (m_Data.deathRemove)
		{
			EndCondition();
		}
	}

	public void OnRevive()
	{
	}

	public void StartCondition()
	{
		if (!hasEffect)
		{
			ApplyEffect();
		}
	}

	public void EndCondition()
	{
		if (hasEffect)
		{
			ApplyEndEffect();
		}
	}

	public void Update()
	{
		if (m_bReqApplyEff)
		{
			m_bReqApplyEff = false;
			ApplyEffect();
		}
		if (!hasEffect && m_Data.updateByModel && !m_Entity.biologyViewCmpt.hasView)
		{
			return;
		}
		if (hasEffect)
		{
			bool flag = m_RemoveTriggers.Count > 0;
			for (int i = 0; i < m_RemoveTriggers.Count; i++)
			{
				flag = flag && m_RemoveTriggers[i].Hit();
			}
			if (flag)
			{
				CheckRemove();
			}
			for (int j = 0; j < m_RemoveHits.Count; j++)
			{
				m_RemoveHits[j].Update();
			}
			for (int k = 0; k < m_Effs.Count; k++)
			{
				m_Effs[k].Update();
			}
		}
		else
		{
			bool flag2 = m_Triggers.Count > 0;
			for (int l = 0; l < m_Triggers.Count; l++)
			{
				flag2 = flag2 && m_Triggers[l].Hit();
			}
			if (flag2 && !m_AbnormalCmpt.Entity.IsDeath())
			{
				CheckHit();
			}
			for (int m = 0; m < m_HitRates.Count; m++)
			{
				m_HitRates[m].Update();
			}
		}
		for (int n = 0; n < m_Triggers.Count; n++)
		{
			m_Triggers[n].Update();
		}
		for (int num = 0; num < m_RemoveTriggers.Count; num++)
		{
			m_RemoveTriggers[num].Update();
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
		if (num > float.Epsilon && UnityEngine.Random.value < num)
		{
			ApplyEffect();
		}
	}

	private void ApplyEffect()
	{
		try
		{
			for (int i = 0; i < m_Effs.Count; i++)
			{
				m_Effs[i].Do();
			}
			for (int j = 0; j < m_HitRates.Count; j++)
			{
				m_HitRates[j].Clear();
			}
		}
		catch (Exception message)
		{
			Debug.LogError(message);
		}
		if (this.evtStart != null)
		{
			this.evtStart(m_Data.type);
		}
		hasEffect = true;
		ClearTriggerHit();
		if (m_EndImm)
		{
			ApplyEndEffect();
		}
	}

	private void CheckRemove()
	{
		float num = 1f;
		for (int i = 0; i < m_RemoveHits.Count; i++)
		{
			m_RemoveHits[i].preHit = num > float.Epsilon;
			num *= m_RemoveHits[i].HitRate();
		}
		if (num > float.Epsilon && UnityEngine.Random.value < num)
		{
			ApplyEndEffect();
		}
	}

	private void ApplyEndEffect()
	{
		hasEffect = false;
		if (this.evtEnd != null)
		{
			this.evtEnd(m_Data.type);
		}
		try
		{
			for (int i = 0; i < m_Effs.Count; i++)
			{
				m_Effs[i].End();
			}
			for (int j = 0; j < m_RemoveEffs.Count; j++)
			{
				m_RemoveEffs[j].Do();
			}
			for (int k = 0; k < m_RemoveHits.Count; k++)
			{
				m_RemoveHits[k].Clear();
			}
		}
		catch (Exception message)
		{
			Debug.LogError(message);
		}
		ClearTriggerHit();
	}

	private void ClearTriggerHit()
	{
		for (int i = 0; i < m_Triggers.Count; i++)
		{
			m_Triggers[i].Clear();
		}
		for (int j = 0; j < m_HitRates.Count; j++)
		{
			m_HitRates[j].Clear();
		}
		for (int k = 0; k < m_RemoveTriggers.Count; k++)
		{
			m_RemoveTriggers[k].Clear();
		}
		for (int l = 0; l < m_RemoveHits.Count; l++)
		{
			m_RemoveHits[l].Clear();
		}
	}

	private void InitData(PEAbnormalType abnormalType)
	{
		m_Data = AbnormalData.GetData(abnormalType);
		if (m_Data == null)
		{
			Debug.LogError("Can't find AbnormalData ID:" + abnormalType);
			return;
		}
		InitTriggers();
		InitHits();
		InitEffects();
		InitRemoveTriggers();
		InitRemoveHits();
		InitRemoveEffects();
	}

	private void InitTriggers()
	{
		if (m_Data.trigger_TimeInterval > float.Epsilon)
		{
			PEAT_Time pEAT_Time = new PEAT_Time();
			pEAT_Time.interval = m_Data.trigger_TimeInterval;
			m_Triggers.Add(pEAT_Time);
		}
		if (m_Data.trigger_BuffAdd != null)
		{
			PEAT_Event_IntArray pEAT_Event_IntArray = new PEAT_Event_IntArray();
			pEAT_Event_IntArray.intValues = m_Data.trigger_BuffAdd;
			AddSaveBuffs(pEAT_Event_IntArray.intValues);
			m_AbnormalCmpt.evtBuffAdd += pEAT_Event_IntArray.OnIntEvent;
			m_Triggers.Add(pEAT_Event_IntArray);
		}
		if (m_Data.trigger_ItemGet != null)
		{
			PEAT_Event_IntArray pEAT_Event_IntArray2 = new PEAT_Event_IntArray();
			pEAT_Event_IntArray2.intValues = m_Data.trigger_ItemGet;
			m_AbnormalCmpt.evtItemAdd += pEAT_Event_IntArray2.OnIntEvent;
			m_Triggers.Add(pEAT_Event_IntArray2);
		}
		if (m_Data.trigger_Damage)
		{
			PEAT_Event pEAT_Event = new PEAT_Event();
			m_AbnormalCmpt.evtDamage += pEAT_Event.OnEvent;
			m_Triggers.Add(pEAT_Event);
		}
		if (m_Data.trigger_InWater)
		{
			PEAT_InWater pEAT_InWater = new PEAT_InWater();
			pEAT_InWater.view = m_AbnormalCmpt.Entity.biologyViewCmpt;
			pEAT_InWater.passenger = m_AbnormalCmpt.Entity.passengerCmpt;
			m_Triggers.Add(pEAT_InWater);
		}
	}

	private void InitHits()
	{
		if (m_Data.hit_MutexAbnormal != null)
		{
			PEAH_Abnormal pEAH_Abnormal = new PEAH_Abnormal();
			pEAH_Abnormal.abnormalCmpt = m_AbnormalCmpt;
			pEAH_Abnormal.abnormals = m_Data.hit_MutexAbnormal;
			pEAH_Abnormal.abnormalExist = false;
			m_HitRates.Add(pEAH_Abnormal);
		}
		if (m_Data.hit_PreAbnormal != null)
		{
			PEAH_Abnormal pEAH_Abnormal2 = new PEAH_Abnormal();
			pEAH_Abnormal2.abnormalCmpt = m_AbnormalCmpt;
			pEAH_Abnormal2.abnormals = m_Data.hit_PreAbnormal;
			pEAH_Abnormal2.abnormalExist = true;
			m_HitRates.Add(pEAH_Abnormal2);
		}
		if (m_Data.hit_BuffID != null)
		{
			PEAH_Buff pEAH_Buff = new PEAH_Buff();
			pEAH_Buff.buffList = m_Data.hit_BuffID;
			AddSaveBuffs(pEAH_Buff.buffList);
			pEAH_Buff.entity = m_Entity.skEntity;
			pEAH_Buff.buffExist = true;
			m_HitRates.Add(pEAH_Buff);
		}
		if (m_Data.hit_Attr != null)
		{
			PEAH_Attr pEAH_Attr = new PEAH_Attr();
			pEAH_Attr.attrs = m_Data.hit_Attr;
			pEAH_Attr.entity = m_Entity;
			m_HitRates.Add(pEAH_Attr);
		}
		if (m_Data.hit_Damage != null)
		{
			PEAH_Damage pEAH_Damage = new PEAH_Damage();
			pEAH_Damage.attr = m_Data.hit_Damage;
			m_AbnormalCmpt.evtDamage += pEAH_Damage.OnGetDamage;
			m_HitRates.Add(pEAH_Damage);
		}
		if (m_Data.hit_TimeInterval > float.Epsilon)
		{
			PEAH_TimeThreshold pEAH_TimeThreshold = new PEAH_TimeThreshold();
			pEAH_TimeThreshold.time = m_Data.hit_TimeInterval;
			m_HitRates.Add(pEAH_TimeThreshold);
		}
		if (m_Data.hit_AreaTime != null)
		{
			PEAH_AreaTime pEAH_AreaTime = new PEAH_AreaTime();
			pEAH_AreaTime.entity = m_Entity;
			pEAH_AreaTime.values = m_Data.hit_AreaTime;
			m_HitRates.Add(pEAH_AreaTime);
		}
		if (m_Data.hit_RainTime > float.Epsilon)
		{
			PEAH_RainTime pEAH_RainTime = new PEAH_RainTime();
			pEAH_RainTime.time = m_Data.hit_RainTime;
			m_HitRates.Add(pEAH_RainTime);
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
		if (m_Data.eff_BuffAddList != null)
		{
			PEAE_Buff pEAE_Buff = new PEAE_Buff();
			pEAE_Buff.buffList = m_Data.eff_BuffAddList;
			AddSaveBuffs(pEAE_Buff.buffList);
			pEAE_Buff.addBuff = true;
			pEAE_Buff.entity = m_Entity.skEntity;
			m_Effs.Add(pEAE_Buff);
		}
		if (m_Data.eff_Anim != null && "0" != m_Data.eff_Anim)
		{
			PEAE_Anim pEAE_Anim = new PEAE_Anim();
			string[] array = m_Data.eff_Anim.Split(',');
			pEAE_Anim.effAnim = array[0];
			pEAE_Anim.actionType = ((array.Length <= 1) ? 1 : Convert.ToInt32(array[1]));
			pEAE_Anim.entity = m_Entity;
			m_Effs.Add(pEAE_Anim);
		}
		if (m_Data.eff_Camera != null)
		{
			PEAE_CameraEffect pEAE_CameraEffect = new PEAE_CameraEffect();
			pEAE_CameraEffect.effCamera = m_Data.eff_Camera;
			pEAE_CameraEffect.entity = m_Entity;
			m_AbnormalCmpt.evtAbnormalAttack += pEAE_CameraEffect.OnAbnormalAttack;
			m_Effs.Add(pEAE_CameraEffect);
		}
		if (m_Data.eff_AbnormalRemove != null)
		{
			PEAE_Abnormal pEAE_Abnormal = new PEAE_Abnormal();
			pEAE_Abnormal.abnormalCmpt = m_AbnormalCmpt;
			pEAE_Abnormal.abnormalType = m_Data.eff_AbnormalRemove;
			pEAE_Abnormal.addAbnormal = false;
			m_Effs.Add(pEAE_Abnormal);
		}
		if (m_Data.eff_Particles != null)
		{
			PEAE_ParticleEffect pEAE_ParticleEffect = new PEAE_ParticleEffect();
			pEAE_ParticleEffect.effectID = m_Data.eff_Particles;
			pEAE_ParticleEffect.entity = m_Entity;
			m_Effs.Add(pEAE_ParticleEffect);
		}
		if (Color.black != m_Data.eff_SkinColor)
		{
			PEAE_SkinColor pEAE_SkinColor = new PEAE_SkinColor();
			pEAE_SkinColor.avatar = m_Entity.biologyViewCmpt as AvatarCmpt;
			pEAE_SkinColor.color = m_Data.eff_SkinColor;
			m_Effs.Add(pEAE_SkinColor);
		}
		if (m_Data.eff_BodyWeight != null)
		{
			PEAE_BodyWeight pEAE_BodyWeight = new PEAE_BodyWeight();
			pEAE_BodyWeight.avatar = m_Entity.biologyViewCmpt as AvatarCmpt;
			pEAE_BodyWeight.datas = m_Data.eff_BodyWeight;
			m_Effs.Add(pEAE_BodyWeight);
		}
	}

	private void InitRemoveTriggers()
	{
		m_EndImm = m_Data.rt_Immediate;
		if (!m_EndImm)
		{
			if (m_Data.rt_TimeInterval > float.Epsilon)
			{
				PEAT_Time pEAT_Time = new PEAT_Time();
				pEAT_Time.interval = m_Data.rt_TimeInterval;
				m_RemoveTriggers.Add(pEAT_Time);
			}
			if (m_Data.rt_BuffRemove != null)
			{
				PEAT_Event_IntArray pEAT_Event_IntArray = new PEAT_Event_IntArray();
				pEAT_Event_IntArray.intValues = m_Data.rt_BuffRemove;
				m_AbnormalCmpt.evtBuffRemove += pEAT_Event_IntArray.OnIntEvent;
				m_RemoveTriggers.Add(pEAT_Event_IntArray);
			}
			if (m_Data.rt_EffectEnd)
			{
				PEAT_EffectEnd pEAT_EffectEnd = new PEAT_EffectEnd();
				pEAT_EffectEnd.abnormal = this;
				m_RemoveTriggers.Add(pEAT_EffectEnd);
			}
			if (m_Data.rt_OutsideWater)
			{
				PEAT_Event pEAT_Event = new PEAT_Event();
				m_AbnormalCmpt.evtOutWater += pEAT_Event.OnEvent;
				m_RemoveTriggers.Add(pEAT_Event);
			}
		}
	}

	private void InitRemoveHits()
	{
		if (m_Data.rh_BuffList != null)
		{
			PEAH_Buff pEAH_Buff = new PEAH_Buff();
			pEAH_Buff.buffList = m_Data.hit_BuffID;
			pEAH_Buff.entity = m_Entity.skEntity;
			pEAH_Buff.buffExist = false;
			m_RemoveHits.Add(pEAH_Buff);
		}
		if (m_Data.rh_Attr != null)
		{
			PEAH_Attr pEAH_Attr = new PEAH_Attr();
			pEAH_Attr.attrs = m_Data.rh_Attr;
			pEAH_Attr.entity = m_Entity;
			m_RemoveHits.Add(pEAH_Attr);
		}
	}

	private void InitRemoveEffects()
	{
		if (m_Data.re_BuffRemove != null)
		{
			PEAE_Buff pEAE_Buff = new PEAE_Buff();
			pEAE_Buff.entity = m_Entity.skEntity;
			pEAE_Buff.buffList = m_Data.re_BuffRemove;
			pEAE_Buff.addBuff = false;
			m_RemoveEffs.Add(pEAE_Buff);
		}
		if (m_Data.re_BuffAdd != null)
		{
			PEAE_Buff pEAE_Buff2 = new PEAE_Buff();
			pEAE_Buff2.entity = m_Entity.skEntity;
			pEAE_Buff2.buffList = m_Data.re_BuffAdd;
			pEAE_Buff2.addBuff = true;
			m_RemoveEffs.Add(pEAE_Buff2);
		}
		if (m_Data.re_AbnormalAdd != null)
		{
			PEAE_Abnormal pEAE_Abnormal = new PEAE_Abnormal();
			pEAE_Abnormal.abnormalType = m_Data.re_AbnormalAdd;
			pEAE_Abnormal.addAbnormal = true;
			pEAE_Abnormal.abnormalCmpt = m_AbnormalCmpt;
			m_RemoveEffs.Add(pEAE_Abnormal);
		}
		if (m_Data.re_Anim != null && "0" != m_Data.re_Anim)
		{
			PEAE_Anim pEAE_Anim = new PEAE_Anim();
			string[] array = m_Data.eff_Anim.Split(',');
			pEAE_Anim.effAnim = array[0];
			pEAE_Anim.actionType = ((array.Length <= 1) ? 1 : Convert.ToInt32(array[1]));
			pEAE_Anim.entity = m_Entity;
			m_RemoveEffs.Add(pEAE_Anim);
		}
		if (m_Data.re_Camera != null)
		{
			PEAE_CameraEffect pEAE_CameraEffect = new PEAE_CameraEffect();
			pEAE_CameraEffect.effCamera = m_Data.re_Camera;
			pEAE_CameraEffect.entity = m_Entity;
			m_AbnormalCmpt.evtAbnormalAttack += pEAE_CameraEffect.OnAbnormalAttack;
			m_RemoveEffs.Add(pEAE_CameraEffect);
		}
		if (m_Data.re_Particles != null)
		{
			PEAE_ParticleEffect pEAE_ParticleEffect = new PEAE_ParticleEffect();
			pEAE_ParticleEffect.effectID = m_Data.re_Particles;
			pEAE_ParticleEffect.entity = m_Entity;
			m_RemoveEffs.Add(pEAE_ParticleEffect);
		}
	}

	private void AddSaveBuffs(int[] buffs)
	{
		for (int i = 0; i < buffs.Length; i++)
		{
			if (!m_NeedSaveBuffList.Contains(buffs[i]))
			{
				m_NeedSaveBuffList.Add(buffs[i]);
			}
		}
	}
}

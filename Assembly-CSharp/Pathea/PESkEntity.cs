using System;
using System.Collections.Generic;
using PETools;
using SkillSystem;
using UnityEngine;

namespace Pathea;

public class PESkEntity : SkEntity
{
	[Serializable]
	public class Attr
	{
		public AttribType m_Type;

		public float m_Value;
	}

	public const int AttribsCnt = 97;

	public const int MaskBitsCnt = 32;

	private List<PeEntity> m_Attackers = new List<PeEntity>();

	public int[] m_InitBuffList;

	public Attr[] m_Attrs;

	public bool isDead { get; protected set; }

	public float HPPercent => GetAttribute(AttribType.Hp, bSum: false) / GetAttribute(AttribType.HpMax, bSum: false);

	public static event Action<SkEntity> entityDeadEvent;

	public static event Action<SkEntity, SkEntity, float> entityAttackEvent;

	public event Action<SkEntity, SkEntity> deathEvent;

	public event Action<SkEntity> reviveEvent;

	public event Action<SkEntity, float> attackEvent;

	public event Action<SkEntity, float> onHpChange;

	public event Action<SkEntity, float> onHpReduce;

	public event Action<SkEntity, float> onHpRecover;

	public event Action<SkEntity> onSkillEvent;

	public event Action<SkEntity> onWeaponAttack;

	public event Action<PeEntity> OnEnemyEnter;

	public event Action<PeEntity> OnEnemyExit;

	public event Action<PeEntity> OnEnemyAchieve;

	public event Action<PeEntity> OnEnemyLost;

	public event Action<PeEntity> OnBeEnemyEnter;

	public event Action<PeEntity> OnBeEnemyExit;

	public event Action<float> onHpMaxChange;

	public event Action onStaminaReduce;

	public event Action onSheildReduce;

	public event Action<Vector3> onTranslate;

	private event Action<AttribType, float, float> m_AttrChangeEvent;

	private event Action<List<ItemToPack>> m_PakageChangeEvent;

	public float GetAttribute(AttribType type, bool bSum = true)
	{
		return GetAttribute((int)type, bSum);
	}

	public void SetAttribute(AttribType type, float value, bool eventOff = true)
	{
		SetAttribute((int)type, value, eventOff);
	}

	public virtual void InitEntity(SkEntity parent = null, bool[] useParentMasks = null)
	{
		if (parent != null && useParentMasks != null)
		{
			Init(OnNumAttribs, OnPutInPak, parent, useParentMasks);
		}
		else if (parent != null || useParentMasks != null)
		{
			Debug.LogError("Invalid Arguments to init skill entity");
		}
		else
		{
			Init(OnNumAttribs, OnPutInPak, 97);
		}
		if (m_Attrs != null)
		{
			Attr[] attrs = m_Attrs;
			foreach (Attr attr in attrs)
			{
				SetAttribute((int)attr.m_Type, attr.m_Value);
			}
		}
		if (m_InitBuffList != null)
		{
			int[] initBuffList = m_InitBuffList;
			foreach (int buffId in initBuffList)
			{
				SkEntity.MountBuff(this, buffId, new List<int>(), new List<float>());
			}
		}
	}

	protected virtual void CheckInitAttr()
	{
		if (GetAttribute(AttribType.Hp) <= 0f)
		{
			OnDeath(null);
		}
		else
		{
			SetAutoBuffActive(active: true);
		}
	}

	protected void SetAutoBuffActive(bool active)
	{
		if (m_InitBuffList == null)
		{
			return;
		}
		if (active)
		{
			int[] initBuffList = m_InitBuffList;
			foreach (int buffId in initBuffList)
			{
				SkEntity.MountBuff(this, buffId, new List<int>(), new List<float>());
			}
		}
		else
		{
			int[] initBuffList2 = m_InitBuffList;
			foreach (int id in initBuffList2)
			{
				CancelBuffById(id);
			}
		}
	}

	public void AddAttrListener(Action<AttribType, float, float> func)
	{
		this.m_AttrChangeEvent = (Action<AttribType, float, float>)Delegate.Combine(this.m_AttrChangeEvent, func);
	}

	public void RemoveAttrListener(Action<AttribType, float, float> func)
	{
		this.m_AttrChangeEvent = (Action<AttribType, float, float>)Delegate.Remove(this.m_AttrChangeEvent, func);
	}

	private void OnNumAttribs(int attType, float oldVal, float newVal)
	{
		CheckAttrEvent((AttribType)attType, oldVal, newVal);
		if (this.m_AttrChangeEvent != null)
		{
			this.m_AttrChangeEvent((AttribType)attType, oldVal, newVal);
		}
	}

	public void DispatchEnemyEnterEvent(PeEntity enemy)
	{
		if (this.OnEnemyEnter != null)
		{
			this.OnEnemyEnter(enemy);
		}
	}

	public void DispatchEnemyExitEvent(PeEntity enemy)
	{
		if (this.OnEnemyExit != null)
		{
			this.OnEnemyExit(enemy);
		}
	}

	public void DispatchEnemyAchieveEvent(PeEntity enemy)
	{
		if (this.OnEnemyAchieve != null)
		{
			this.OnEnemyAchieve(enemy);
		}
	}

	public void DispatchEnemyLostEvent(PeEntity enemy)
	{
		if (this.OnEnemyLost != null)
		{
			this.OnEnemyLost(enemy);
		}
	}

	public void DispatchBeEnemyEnterEvent(PeEntity attacker)
	{
		if (this.OnBeEnemyEnter != null)
		{
			this.OnBeEnemyEnter(attacker);
		}
	}

	public void DispatchBeEnemyExitEvent(PeEntity attacker)
	{
		if (this.OnBeEnemyExit != null)
		{
			this.OnBeEnemyExit(attacker);
		}
	}

	public void DispatchHPChangeEvent(SkEntity caster, float hpChange)
	{
		if (this.onHpChange != null)
		{
			this.onHpChange(caster, hpChange);
			Singleton<PeEventGlobal>.Instance.HPChangeEvent.Invoke(this, caster, hpChange);
		}
		if (hpChange < -1E-45f)
		{
			if (this.onHpReduce != null)
			{
				this.onHpReduce(caster, Mathf.Abs(hpChange));
			}
			if (PESkEntity.entityAttackEvent != null)
			{
				PESkEntity.entityAttackEvent(this, caster, Mathf.Abs(hpChange));
			}
			PESkEntity pESkEntity = PEUtil.GetCaster(caster) as PESkEntity;
			if (pESkEntity != null && pESkEntity.attackEvent != null)
			{
				pESkEntity.attackEvent(this, Mathf.Abs(hpChange));
			}
			Singleton<PeEventGlobal>.Instance.HPReduceEvent.Invoke(this, caster, Mathf.Abs(hpChange));
		}
		if (hpChange > float.Epsilon)
		{
			if (this.onHpRecover != null)
			{
				this.onHpRecover(caster, Mathf.Abs(hpChange));
			}
			Singleton<PeEventGlobal>.Instance.HPRecoverEvent.Invoke(this, caster, Mathf.Abs(hpChange));
		}
	}

	public void DispatchTargetSkill(SkEntity caster)
	{
		if (this.onSkillEvent != null)
		{
			this.onSkillEvent(caster);
		}
	}

	public void DispatchWeaponAttack(SkEntity caster)
	{
		if (this.onWeaponAttack != null)
		{
			this.onWeaponAttack(caster);
		}
	}

	public void DispatchOnTranslate(Vector3 pos)
	{
		if (this.onTranslate != null)
		{
			this.onTranslate(pos);
		}
	}

	private void DispatchHPMaxChangeEvent(float newVal)
	{
		if (this.onHpMaxChange != null)
		{
			this.onHpMaxChange(newVal);
		}
	}

	private void DispatchDeathEvent(SkEntity caster)
	{
		if (this.deathEvent != null)
		{
			this.deathEvent(this, caster);
		}
		if (PESkEntity.entityDeadEvent != null)
		{
			PESkEntity.entityDeadEvent(this);
		}
		if (GetComponent<ItemDropPeEntity>() == null)
		{
			PeEntity component = GetComponent<PeEntity>();
			if (component != null && (component.ItemDropId > 0 || GetSpecialItem.ExistSpecialItem(component)))
			{
				PeSingleton<LootItemMgr>.Instance.RequestCreateLootItem(component);
			}
		}
		Singleton<PeEventGlobal>.Instance.DeathEvent.Invoke(this, caster);
	}

	protected void DispatchReviveEvent()
	{
		if (this.reviveEvent != null)
		{
			this.reviveEvent(this);
		}
		if (GetComponent<ItemDropPeEntity>() != null)
		{
			UnityEngine.Object.Destroy(GetComponent<ItemDropPeEntity>());
		}
	}

	private void DispatchStaminaEvent(float staminaChange)
	{
		if (staminaChange < -1E-45f && this.onStaminaReduce != null)
		{
			this.onStaminaReduce();
		}
	}

	private void DispatchSheildEvent(float sheildChange)
	{
		if (sheildChange < -1E-45f && this.onSheildReduce != null)
		{
			this.onSheildReduce();
		}
	}

	private void OnDeath(SkEntity caster)
	{
		DispatchDeathEvent(caster);
		SetAutoBuffActive(active: false);
		isDead = true;
	}

	private void CheckAttrEvent(AttribType attType, float oldVal, float newVal)
	{
		switch (attType)
		{
		case AttribType.Hp:
		{
			SkEntity caster = ((!PeGameMgr.IsSingle) ? GetNetCasterToModAttrib((int)attType) : GetCasterToModAttrib((int)attType));
			float num = newVal - oldVal;
			if (Mathf.Abs(num) > float.Epsilon)
			{
				if (!PeGameMgr.IsMulti)
				{
					DispatchHPChangeEvent(caster, num);
				}
				if (GetAttribute(AttribType.Hp) < GetAttribute(AttribType.HpMax))
				{
					PeSingleton<HPChangeEventDataMan>.Instance.OnHpChange(this, caster, num);
				}
			}
			if (newVal <= 0f && oldVal > 0f && this.deathEvent != null)
			{
				OnDeath(caster);
			}
			break;
		}
		case AttribType.HpMax:
			DispatchHPMaxChangeEvent(newVal);
			break;
		case AttribType.Stamina:
			if (oldVal != newVal)
			{
				DispatchStaminaEvent(newVal - oldVal);
				if (oldVal > newVal)
				{
					_lastestTimeOfConsumingStamina = Time.time;
				}
			}
			break;
		case AttribType.Shield:
			if (oldVal != newVal)
			{
				DispatchSheildEvent(newVal - oldVal);
			}
			break;
		}
		if (oldVal != newVal)
		{
			PESingleton<AttribInspectoscope>.Instance.CheckAttrib(this, attType, newVal);
		}
	}

	public void AddPakageListener(Action<List<ItemToPack>> func)
	{
		this.m_PakageChangeEvent = (Action<List<ItemToPack>>)Delegate.Combine(this.m_PakageChangeEvent, func);
	}

	public void RemovePakageListener(Action<List<ItemToPack>> func)
	{
		this.m_PakageChangeEvent = (Action<List<ItemToPack>>)Delegate.Remove(this.m_PakageChangeEvent, func);
	}

	protected virtual void OnPutInPak(List<ItemToPack> itemsToPack)
	{
		if (this.m_PakageChangeEvent != null)
		{
			this.m_PakageChangeEvent(itemsToPack);
		}
	}
}

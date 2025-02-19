using Pathea;
using Pathea.Effect;
using SkillSystem;
using UnityEngine;

public class ShootWeaponEffect : MonoBehaviour, ISkEffectEntity
{
	private SkInst m_Inst;

	SkInst ISkEffectEntity.Inst
	{
		set
		{
			m_Inst = value;
		}
	}

	private void Start()
	{
		if (m_Inst == null || null == m_Inst._caster)
		{
			Object.Destroy(base.gameObject);
			return;
		}
		PeEntity component = m_Inst._caster.GetComponent<PeEntity>();
		if (null != component)
		{
			component.SendMsg(EMsg.Battle_OnShoot);
		}
		Object.Destroy(base.gameObject);
	}
}

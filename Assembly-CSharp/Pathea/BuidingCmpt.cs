using System.Collections.Generic;
using SkillSystem;

namespace Pathea;

public class BuidingCmpt : PeCmpt
{
	private new void Start()
	{
		if (base.Entity.peSkEntity != null)
		{
			base.Entity.peSkEntity.onHpReduce += OnDamage;
			base.Entity.peSkEntity.attackEvent += OnAttack;
			base.Entity.peSkEntity.onSkillEvent += OnSkillTarget;
		}
	}

	private void Update()
	{
	}

	private void OnAttack(SkEntity skEntity, float damage)
	{
		PeEntity component = skEntity.GetComponent<PeEntity>();
		if (component != null && component != base.Entity)
		{
			NpcHatreTargets.Instance.TryAddInTarget(base.Entity, component, damage);
		}
	}

	private void OnDamage(SkEntity entity, float damage)
	{
		if (!(null == base.Entity.peSkEntity) && !(null == entity))
		{
			PeEntity component = entity.GetComponent<PeEntity>();
			if (!(component == base.Entity))
			{
				NpcHatreTargets.Instance.TryAddInTarget(base.Entity, component, damage, trans: true);
			}
		}
	}

	private void OnSkillTarget(SkEntity caster)
	{
		if (null == base.Entity.peSkEntity || null == caster)
		{
			return;
		}
		int playerID = (int)base.Entity.peSkEntity.GetAttribute(91);
		PeEntity component = caster.GetComponent<PeEntity>();
		if (component == base.Entity)
		{
			return;
		}
		float radius = ((!component.IsBoss) ? 64f : 128f);
		bool flag = false;
		if (GameConfig.IsMultiClient)
		{
			if (Singleton<ForceSetting>.Instance.GetForceType(playerID) == EPlayerType.Human)
			{
				flag = true;
			}
		}
		else if (Singleton<ForceSetting>.Instance.GetForceID(playerID) == 1)
		{
			flag = true;
		}
		if (!flag)
		{
			return;
		}
		List<PeEntity> entities = PeSingleton<EntityMgr>.Instance.GetEntities(base.Entity.peTrans.position, radius, playerID, isDeath: false, base.Entity);
		for (int i = 0; i < entities.Count; i++)
		{
			if (!(entities[i] == null) && !entities[i].Equals(base.Entity) && entities[i].target != null)
			{
				entities[i].target.OnTargetSkill(component.skEntity);
			}
		}
	}
}

using Pathea;
using Pathea.Projectile;
using UnityEngine;

public class PEAbnormalAttackTrigger : MonoBehaviour
{
	public bool costByProjectileHit;

	public bool deletSelf = true;

	public PEAbnormalAttack[] abnormalAttacks;

	private void Start()
	{
		if (costByProjectileHit)
		{
			SkProjectile component = GetComponent<SkProjectile>();
			if (null != component)
			{
				component.onCastSkill += delegate
				{
					CheckTrigger();
				};
			}
			else
			{
				costByProjectileHit = false;
			}
		}
		if (!costByProjectileHit)
		{
			CheckTrigger();
		}
		if (deletSelf)
		{
			Object.Destroy(base.gameObject);
		}
	}

	private void CheckTrigger()
	{
		if (abnormalAttacks == null || !(null != PeSingleton<PeCreature>.Instance.mainPlayer))
		{
			return;
		}
		AbnormalConditionCmpt cmpt = PeSingleton<PeCreature>.Instance.mainPlayer.GetCmpt<AbnormalConditionCmpt>();
		if (!(null != cmpt))
		{
			return;
		}
		PEAbnormalAttack[] array = abnormalAttacks;
		foreach (PEAbnormalAttack pEAbnormalAttack in array)
		{
			if (Vector3.SqrMagnitude(cmpt.Entity.position - base.transform.position) < pEAbnormalAttack.radius * pEAbnormalAttack.radius)
			{
				cmpt.ApplyAbnormalAttack(pEAbnormalAttack, base.transform.position);
			}
		}
	}
}

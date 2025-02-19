using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Pathea.Projectile;

public class SPGDifferentTarget : SkProjectileGroup
{
	[SerializeField]
	private int[] projectileIDList;

	public IEnumerator SpawnProjectile()
	{
		if (projectileIDList.Length == 0)
		{
			Object.Destroy(base.gameObject);
			yield break;
		}
		Transform castTrans = GetCasterTrans();
		if (null == castTrans)
		{
			Debug.LogWarning("Can't find caster");
			Object.Destroy(base.gameObject);
			yield break;
		}
		bool immediately = projectileInterval < float.Epsilon;
		TargetCmpt targetCmpt = castTrans.GetComponentInParent<TargetCmpt>();
		if (!(null != targetCmpt))
		{
			yield break;
		}
		List<Enemy> enemies = targetCmpt.GetEnemies();
		if (enemies == null)
		{
			yield break;
		}
		for (int i = 0; i < projectileCount; i++)
		{
			Transform targetTrans = null;
			if (enemies.Count > 0)
			{
				int targetIndex = i % enemies.Count;
				if (enemies[targetIndex] != null || null != enemies[targetIndex].entityTarget || null != enemies[targetIndex].entityTarget.tr)
				{
					targetTrans = enemies[targetIndex].entityTarget.tr;
				}
			}
			int castProjectileID = projectileIDList[i % projectileIDList.Length];
			Singleton<ProjectileBuilder>.Instance.Register(castProjectileID, castTrans, targetTrans, i, immediately);
			if (!immediately)
			{
				yield return new WaitForSeconds(projectileInterval);
			}
		}
	}

	public override void SetData(ProjectileData data, Transform caster, Transform emitter, Transform target, Vector3 targetPosition, int index)
	{
		base.SetData(data, caster, emitter, target, targetPosition, index);
		StartCoroutine(SpawnProjectile());
	}
}

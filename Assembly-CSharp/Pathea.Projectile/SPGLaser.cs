using System.Collections;
using UnityEngine;

namespace Pathea.Projectile;

public class SPGLaser : SkProjectileGroup
{
	public float distane;

	private bool m_Spawned;

	public IEnumerator SpawnProjectile()
	{
		Transform castTrans = GetCasterTrans();
		if (null == castTrans)
		{
			Debug.LogWarning("Can't find caster");
			yield break;
		}
		bool immediately = projectileInterval < float.Epsilon;
		for (int i = 0; i < projectileCount; i++)
		{
			if (m_Target != null)
			{
				Singleton<ProjectileBuilder>.Instance.Register(projectileID, castTrans, base.transform, m_Target, i, immediately);
			}
			else if (m_TargetPosition != Vector3.zero)
			{
				Singleton<ProjectileBuilder>.Instance.Register(projectileID, castTrans, base.transform, m_TargetPosition, i, immediately);
			}
			if (!immediately)
			{
				yield return new WaitForSeconds(projectileInterval);
			}
		}
		Object.Destroy(base.gameObject);
	}

	public new void Update()
	{
		base.Update();
		if (!m_Spawned)
		{
			Vector3 b = ((!(m_Target != null)) ? m_TargetPosition : m_Target.position);
			if (Vector3.Distance(base.transform.position, b) < distane)
			{
				m_Spawned = true;
				StartCoroutine(SpawnProjectile());
			}
		}
	}
}

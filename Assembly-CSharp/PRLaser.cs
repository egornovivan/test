using System.Collections;
using UnityEngine;

public class PRLaser : Projectile
{
	public float delayTime;

	public float intervalTime;

	private Collider myCollider;

	private float nextStandardTime;

	public new void Start()
	{
		base.Start();
		StartCoroutine(logic());
	}

	private bool CheckTargetValid()
	{
		if (emitRunner == null)
		{
			return false;
		}
		return true;
	}

	private Collider GetDamageCollider()
	{
		if (emitTransform != null)
		{
			Vector3 direction = base.transform.position - emitTransform.position;
			RaycastHit[] hits = Physics.SphereCastAll(emitTransform.position, Projectile.DamageRadius, direction, direction.magnitude + 0.5f);
			hits = AiUtil.SortHitInfoFromDistance(hits);
			RaycastHit[] array = hits;
			for (int i = 0; i < array.Length; i++)
			{
				RaycastHit raycastHit = array[i];
				if (!IsIgnoreCollider(raycastHit.collider))
				{
					return raycastHit.collider;
				}
			}
		}
		return null;
	}

	private IEnumerator logic()
	{
		yield return new WaitForSeconds(delayTime);
		while (CheckTargetValid())
		{
			myCollider = GetDamageCollider();
			if (myCollider != null)
			{
				TriggerColliderInterval(myCollider);
				yield return new WaitForSeconds(intervalTime);
			}
			else
			{
				yield return new WaitForSeconds(0.02f);
			}
		}
		DestroyProjectile();
	}
}

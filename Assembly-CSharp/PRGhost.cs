using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PRGhost : Projectile
{
	public float attackInterval;

	private List<Collider> targets = new List<Collider>();

	public new void Start()
	{
		base.Start();
		StartCoroutine(logic());
	}

	private IEnumerator logic()
	{
		while (true)
		{
			if (targets.Count == 0)
			{
				yield return new WaitForSeconds(0.02f);
				continue;
			}
			if (attackInterval == 0f)
			{
				break;
			}
			foreach (Collider c in targets)
			{
				if (!(c == null))
				{
					TriggerColliderInterval(c);
				}
			}
			yield return new WaitForSeconds(attackInterval);
		}
		TriggerCollider(targets[0]);
	}

	private void OnTriggerEnter(Collider other)
	{
		if (IsIgnoreCollider(other))
		{
			return;
		}
		if (targets.Count != 0)
		{
			foreach (Collider target in targets)
			{
				if (target == null || !(target.transform == other.transform))
				{
					continue;
				}
				return;
			}
		}
		targets.Add(other);
	}

	private void OnTriggerExit(Collider other)
	{
		if (targets.Contains(other))
		{
			targets.Remove(other);
		}
	}
}

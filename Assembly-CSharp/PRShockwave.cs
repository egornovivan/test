using System.Collections.Generic;
using UnityEngine;

public class PRShockwave : Projectile
{
	public Transform trigger;

	public float maxRadius;

	private float progress;

	private float radius;

	private List<Collider> coll = new List<Collider>();

	private void OnTriggerEnter(Collider other)
	{
		if (!IsIgnoreCollider(other) && !coll.Contains(other))
		{
			coll.Add(other);
			TriggerCollider(other);
		}
	}

	public new void Update()
	{
		base.Update();
		if (trigger != null)
		{
			progress += Time.deltaTime / existTime;
			radius = maxRadius * progress;
			trigger.localScale = new Vector3(radius, trigger.localScale.y, radius);
		}
	}
}

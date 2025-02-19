using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class PRBullet : Projectile
{
	public byte effType;

	public new void Update()
	{
		base.Update();
		CheckMovementCollision();
	}
}

public class PRCannonball : Projectile
{
	public new void Update()
	{
		base.Update();
		CheckMovementCollision();
	}
}

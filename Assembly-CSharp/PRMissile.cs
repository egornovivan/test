public class PRMissile : Projectile
{
	public new void Update()
	{
		base.Update();
		CheckMovementCollision();
	}
}

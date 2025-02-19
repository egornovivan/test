public class PRArrow : Projectile
{
	public byte effType;

	public new void Update()
	{
		base.Update();
		CheckMovementCollision();
	}
}

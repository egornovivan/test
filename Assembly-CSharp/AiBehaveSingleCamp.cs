public class AiBehaveSingleCamp : AiBehave
{
	public override bool isSingle => true;

	public override bool isMember => false;

	public override bool isActive => base.isActive;
}

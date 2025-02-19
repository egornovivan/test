namespace Pathea;

public abstract class MultiGameOfficial : MultiGame
{
	public const string YirdMain = "main";

	protected override string GetDefaultYirdName()
	{
		return "main";
	}
}

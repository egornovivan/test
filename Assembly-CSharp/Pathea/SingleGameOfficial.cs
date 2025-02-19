namespace Pathea;

public abstract class SingleGameOfficial : SingleGame
{
	public const string YirdMain = "main";

	protected override string GetDefaultYirdName()
	{
		return "main";
	}
}

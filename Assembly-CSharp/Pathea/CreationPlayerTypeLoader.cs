using Pathea.GameLoader;

namespace Pathea;

public class CreationPlayerTypeLoader : PlayerTypeLoader
{
	public override void Load()
	{
		CreationModeLoader();
	}

	private static void CreationModeLoader()
	{
		PeLauncher.Instance.Add(new OpenVCEditor());
	}
}

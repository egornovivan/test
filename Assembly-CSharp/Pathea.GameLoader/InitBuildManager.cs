using UnityEngine;

namespace Pathea.GameLoader;

internal class InitBuildManager : ModuleLoader
{
	public InitBuildManager(bool newGame)
		: base(newGame)
	{
	}

	protected override void New()
	{
		LaunchBuildSystem();
		PeSingleton<UIBlockSaver>.Instance.New();
	}

	protected override void Restore()
	{
		LaunchBuildSystem();
		PeSingleton<UIBlockSaver>.Instance.Restore();
	}

	private void LaunchBuildSystem()
	{
		Object.Instantiate(Resources.Load("Prefab/Building System")).name = "Building System";
	}
}

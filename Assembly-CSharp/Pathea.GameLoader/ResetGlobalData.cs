namespace Pathea.GameLoader;

internal class ResetGlobalData : PeLauncher.ILaunchable
{
	void PeLauncher.ILaunchable.Launch()
	{
		AIErodeMap.ResetErodeData();
		PeSingleton<PeGameSummary.Mgr>.Instance.Init();
		DigTerrainManager.ClearBlockInfo();
		if (!PeGameMgr.IsAdventure)
		{
			RandomMapConfig.useSkillTree = false;
			RandomMapConfig.openAllScripts = false;
		}
	}
}

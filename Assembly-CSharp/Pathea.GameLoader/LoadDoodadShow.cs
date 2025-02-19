namespace Pathea.GameLoader;

internal class LoadDoodadShow : PeLauncher.ILaunchable
{
	void PeLauncher.ILaunchable.Launch()
	{
		for (int i = 335; i < 342; i++)
		{
			ChangeDoodadShowVar(i, tmp: false);
		}
	}

	private void ChangeDoodadShowVar(int n, bool tmp)
	{
		PeEntity[] doodadEntities = PeSingleton<EntityMgr>.Instance.GetDoodadEntities(n);
		if (doodadEntities.Length > 0)
		{
			SceneDoodadLodCmpt component = doodadEntities[0].GetComponent<SceneDoodadLodCmpt>();
			if (component != null)
			{
				component.IsShown = tmp;
			}
		}
	}
}

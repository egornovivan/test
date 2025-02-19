namespace Pathea.GameLoader;

internal abstract class LoadRandomTerrain : LoadTerrain
{
	public LoadRandomTerrain(bool bNew)
		: base(bNew)
	{
	}

	protected override void Load()
	{
		SceneMan.MaxLod = SystemSettingData.Instance.RandomTerrainLevel;
		PeGrassSystem.Refresh(SystemSettingData.Instance.GrassDensity, (int)SystemSettingData.Instance.GrassLod);
		base.Load();
	}
}

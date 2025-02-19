namespace Pathea.GameLoader;

internal class LoadEditedTerrain : LoadTerrain
{
	public LoadEditedTerrain(bool bNew, string path)
		: base(bNew)
	{
		VFVoxelTerrain.MapDataPath_Zip = path;
	}

	protected override void Load()
	{
		SceneMan.MaxLod = SystemSettingData.Instance.TerrainLevel;
		PeGrassSystem.Refresh(SystemSettingData.Instance.GrassDensity, (int)SystemSettingData.Instance.GrassLod);
		base.Load();
	}
}

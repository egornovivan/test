namespace Pathea.GameLoader;

internal class LoadEditedGrass : LoadGrass
{
	public LoadEditedGrass(bool bNew, string dataDir)
		: base(bNew)
	{
		PeGrassDataIO_Story.originalSubTerrainDir = dataDir;
	}

	protected override string GetGrassPrefabName()
	{
		return "PE Grass System";
	}
}

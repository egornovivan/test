namespace Pathea.GameLoader;

internal class LoadGrassRandom : LoadGrass
{
	public LoadGrassRandom(bool bNew)
		: base(bNew)
	{
	}

	protected override string GetGrassPrefabName()
	{
		return "PE Random Grass System";
	}
}

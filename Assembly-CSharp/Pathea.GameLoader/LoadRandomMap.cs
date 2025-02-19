using PeMap;

namespace Pathea.GameLoader;

internal class LoadRandomMap : LoadMap
{
	public LoadRandomMap(bool bNew)
		: base(bNew)
	{
	}

	protected override void New()
	{
		base.New();
		MapRunner.UpdateTile(value: true);
	}

	protected override void Restore()
	{
		base.Restore();
		MapRunner.UpdateTile(value: true);
	}
}

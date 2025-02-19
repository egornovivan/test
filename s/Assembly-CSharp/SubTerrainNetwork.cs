using Pathea;

public class SubTerrainNetwork : SkNetworkInterface
{
	private static SubTerrainNetwork _instance;

	protected override void OnPEStart()
	{
		_id = IdGenerator.CurItemId;
		base.OnPEStart();
	}

	private void uLink_OnServerInitialized()
	{
		AddSkEntity();
	}
}

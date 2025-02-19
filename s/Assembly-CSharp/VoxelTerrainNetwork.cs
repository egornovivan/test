using Pathea;

public class VoxelTerrainNetwork : SkNetworkInterface
{
	private static VoxelTerrainNetwork _instance;

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

using UnityEngine;

public class VoxelTerrainNetwork : SkNetworkInterface
{
	private static VoxelTerrainNetwork _instance;

	public static VoxelTerrainNetwork Instance
	{
		get
		{
			if (_instance == null)
			{
				GameObject gameObject = GameObject.Find("SKVoxelTerrainNetwork");
				if (!(gameObject != null))
				{
					return null;
				}
				_instance = gameObject.GetComponent<VoxelTerrainNetwork>();
			}
			return _instance;
		}
	}

	protected override void OnPEAwake()
	{
		_instance = this;
	}

	protected override void OnPEStart()
	{
		base.OnPEStart();
		BindSkAction();
	}

	public void Init()
	{
		_id = base.OwnerView.viewID.id;
		OnSpawned(VFVoxelTerrain.self.gameObject);
		if (runner != null && runner.SkEntityBase != null)
		{
			runner.SkEntityBase.SetAttribute(91, 10f);
		}
	}
}

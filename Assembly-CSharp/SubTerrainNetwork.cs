using UnityEngine;

public class SubTerrainNetwork : SkNetworkInterface
{
	private static SubTerrainNetwork _instance;

	public static SubTerrainNetwork Instance
	{
		get
		{
			if (_instance == null)
			{
				GameObject gameObject = GameObject.Find("SKSubTerrainNetwork");
				if (!(gameObject != null))
				{
					return null;
				}
				_instance = gameObject.GetComponent<SubTerrainNetwork>();
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
		OnSpawned(SkEntitySubTerrain.Instance.gameObject);
		if (runner != null && runner.SkEntityPE != null)
		{
			runner.SkEntityPE.SetNet(this);
		}
	}
}

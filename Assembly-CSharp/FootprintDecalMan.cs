using Pathea;
using UnityEngine;

public class FootprintDecalMan : MonoBehaviour
{
	private const int CntFootPrintPlayer = 5;

	private const int CntFootPrintNPC = 2;

	private static GameObject FootPrintGrp;

	public Transform[] _lrFoot = new Transform[2];

	public GameObject _fpSeedGoLR;

	public float _thresVelOfMove = 0.5f;

	public float _rayLength = 0.5f;

	public HumanPhyCtrl _ctrlr;

	[HideInInspector]
	public MotionMgrCmpt _mmc;

	private int _cntFootPrint = 3;

	[HideInInspector]
	public int _curFoot;

	[HideInInspector]
	public int[] _curFpIdx = new int[2];

	[HideInInspector]
	public FootprintDecal[,] _fpGoUpdates;

	[HideInInspector]
	public float _fpLastLRFootDistance;

	[HideInInspector]
	public bool _fpbPlayerInMove;

	[HideInInspector]
	public bool[] _fpbFootInMove = new bool[2];

	[HideInInspector]
	public Vector3[] _fpLastFootsPos = new Vector3[2];

	public Transform FootPrintsParent
	{
		get
		{
			if (FootPrintGrp != null || (FootPrintGrp = GameObject.Find("FootPrints")) != null)
			{
				return FootPrintGrp.transform;
			}
			FootPrintGrp = new GameObject("FootPrints");
			FootPrintGrp.transform.parent = base.transform.parent;
			FootPrintGrp.transform.position = Vector3.zero;
			FootPrintGrp.transform.rotation = Quaternion.identity;
			return FootPrintGrp.transform;
		}
	}

	private void Start()
	{
		if (_fpSeedGoLR == null)
		{
			return;
		}
		try
		{
			_cntFootPrint = ((!(GetComponentInParent<MainPlayerCmpt>() != null)) ? 2 : 5);
			_fpGoUpdates = new FootprintDecal[2, _cntFootPrint];
			_mmc = GetComponentInParent<MotionMgrCmpt>();
			int num = ((_mmc.GetComponent<CommonCmpt>().sex != PeSex.Male) ? 1 : 0);
			Texture mainTexture = Resources.Load((num != 1) ? "Texture2D/footprint_m" : "Texture2D/footprint_f") as Texture;
			_fpSeedGoLR.GetComponent<Renderer>().sharedMaterial.mainTexture = mainTexture;
			PeSingleton<FootprintDecalMgr>.Instance.Register(this);
		}
		catch
		{
		}
	}

	public void UpdateDecals()
	{
		for (int i = 0; i < 2; i++)
		{
			for (int j = 0; j < _cntFootPrint; j++)
			{
				if (_fpGoUpdates[i, j] != null)
				{
					_fpGoUpdates[i, j].UpdateDecal();
				}
			}
		}
	}
}

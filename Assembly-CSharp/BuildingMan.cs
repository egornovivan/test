using System;
using UnityEngine;

public class BuildingMan : MonoBehaviour
{
	public enum EBrushType
	{
		None,
		Point,
		Box,
		Select,
		Iso,
		B45Diagonal,
		IsoSelectBrush
	}

	[Serializable]
	public class CBrushRes
	{
		public GameObject pointBrush;

		public GameObject boxBrush;

		public GameObject SelectBrush;

		public GameObject IsoBrush;

		public GameObject diagonalBrush;

		public GameObject IsoSelectBrush;
	}

	public delegate void DCreateBrushNotify(BSBrush brush, EBrushType type);

	private EBrushType m_BrushType;

	private static BuildingMan _self = null;

	public static string s_IsoPath = "/PlanetExplorers/BuildingIso/";

	public static string s_IsoExt = ".biso";

	private IBSDataSource m_ActiveData;

	public static IBSDataSource[] Datas = new IBSDataSource[2]
	{
		new BSVoxelData(),
		new BSBlock45Data()
	};

	public byte MaterialType = 2;

	public BSGLNearVoxelIndicator voxelIndicator;

	public BSGLNearBlockIndicator blockIndicator;

	public GameObject brushGroup;

	public GUISkin guiSkin;

	private BSBrush m_ActiveBrush;

	[SerializeField]
	private CBrushRes brushPrefabs;

	public BSPattern pattern;

	public Material patternMeshMat;

	public EBrushType BrushType => m_BrushType;

	public static BuildingMan Self => _self;

	public IBSDataSource ActiveData => m_ActiveData;

	public static BSVoxelData Voxels => Datas[0] as BSVoxelData;

	public static BSBlock45Data Blocks => Datas[1] as BSBlock45Data;

	public BSBrush activeBrush => m_ActiveBrush;

	public event DCreateBrushNotify onCreateBrush;

	public static BSBrush CreateBrush(EBrushType type)
	{
		if (Self == null)
		{
			Debug.LogError("Building manipulator");
			return null;
		}
		if (type == Self.m_BrushType)
		{
			return Self.m_ActiveBrush;
		}
		if (Self.m_ActiveBrush != null)
		{
			UnityEngine.Object.Destroy(Self.m_ActiveBrush.gameObject);
		}
		BSBrush bSBrush = null;
		switch (type)
		{
		case EBrushType.Point:
			bSBrush = BSBrush.Create<BSPointBrush>(Self.brushPrefabs.pointBrush, Self.brushGroup.transform);
			break;
		case EBrushType.Box:
			bSBrush = BSBrush.Create<BSBoxBrush>(Self.brushPrefabs.boxBrush, Self.brushGroup.transform);
			break;
		case EBrushType.Select:
			bSBrush = BSBrush.Create<BSMiscBrush>(Self.brushPrefabs.SelectBrush, Self.brushGroup.transform);
			break;
		case EBrushType.Iso:
			bSBrush = BSBrush.Create<BSIsoBrush>(Self.brushPrefabs.IsoBrush, Self.brushGroup.transform);
			break;
		case EBrushType.B45Diagonal:
			bSBrush = BSBrush.Create<BSB45DiagonalBrush>(Self.brushPrefabs.diagonalBrush, Self.brushGroup.transform);
			break;
		case EBrushType.IsoSelectBrush:
			bSBrush = BSBrush.Create<BSIsoSelectBrush>(Self.brushPrefabs.IsoSelectBrush, Self.brushGroup.transform);
			break;
		}
		Self.m_BrushType = type;
		if (bSBrush == null)
		{
			return null;
		}
		Self.m_ActiveBrush = bSBrush;
		bSBrush.pattern = Self.pattern;
		bSBrush.dataSource = Self.m_ActiveData;
		Self.voxelIndicator.minVol = bSBrush.minvol;
		if (Self.onCreateBrush != null)
		{
			Self.onCreateBrush(bSBrush, type);
		}
		return bSBrush;
	}

	public static void Clear()
	{
		if (Self.m_ActiveBrush != null)
		{
			UnityEngine.Object.Destroy(Self.m_ActiveBrush.gameObject);
		}
		BSHistory.Clear();
	}

	private void OnGUI()
	{
	}

	private void Awake()
	{
		if (_self != null)
		{
			Debug.LogWarning("There is alread a Building manipulator ");
		}
		else
		{
			_self = this;
		}
		patternMeshMat.renderQueue = 3000;
	}

	private void OnDestroy()
	{
	}

	private void Update()
	{
		if (BSInput.s_Undo)
		{
			BSHistory.Undo();
		}
		else if (BSInput.s_Redo)
		{
			BSHistory.Redo();
		}
		if (pattern == null)
		{
			pattern = BSPattern.DefaultB1;
		}
		IBSDataSource iBSDataSource = null;
		if (pattern.type != EBSVoxelType.Block)
		{
			iBSDataSource = ((pattern.type != 0) ? null : Voxels);
		}
		else
		{
			iBSDataSource = Blocks;
			if (Block45Man.self._b45Materials.Length > MaterialType)
			{
				pattern.MeshMat = Block45Man.self._b45Materials[MaterialType];
			}
			else
			{
				pattern.MeshMat = Block45Man.self._b45Materials[0];
			}
		}
		if (m_ActiveBrush != null)
		{
			voxelIndicator.minVol = m_ActiveBrush.minvol;
			m_ActiveBrush.dataSource = iBSDataSource;
			m_ActiveBrush.pattern = pattern;
			m_ActiveBrush.materialType = MaterialType;
			if (m_ActiveBrush.pattern.type == EBSVoxelType.Block)
			{
				blockIndicator.enabled = true;
				voxelIndicator.enabled = false;
			}
			else if (m_ActiveBrush.pattern.type == EBSVoxelType.Voxel)
			{
				blockIndicator.enabled = false;
				voxelIndicator.enabled = true;
			}
		}
		else
		{
			blockIndicator.enabled = false;
			voxelIndicator.enabled = false;
		}
	}
}

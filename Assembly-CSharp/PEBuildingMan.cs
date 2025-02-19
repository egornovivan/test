using System.Collections.Generic;
using System.IO;
using ItemAsset;
using Pathea;
using UnityEngine;

public class PEBuildingMan : MonoBehaviour
{
	public delegate void DVoxelModifyNotify(IntVector3[] indexes, BSVoxel[] voxels, BSVoxel[] oldvoxels, EBSBrushMode mode, IBSDataSource d);

	private static PEBuildingMan _self;

	[SerializeField]
	private BuildingMan _manipulator;

	private BSPattern m_Pattern;

	public bool selectVoxel = true;

	public bool IsGod;

	[SerializeField]
	private PEIsoCapture _IsoCapturePrefab;

	private PEIsoCapture m_IsoCaputure;

	public bool GUITest;

	private Dictionary<int, int> _costsItems = new Dictionary<int, int>();

	private Dictionary<int, int> _playerItems = new Dictionary<int, int>();

	public static PEBuildingMan Self => _self;

	public BuildingMan Manipulator => _manipulator;

	public BSPattern Pattern
	{
		get
		{
			return _manipulator.pattern;
		}
		set
		{
			if (_manipulator.BrushType == BuildingMan.EBrushType.Point || _manipulator.BrushType == BuildingMan.EBrushType.Box || _manipulator.BrushType == BuildingMan.EBrushType.B45Diagonal)
			{
				m_Pattern = value;
				_manipulator.pattern = value;
			}
		}
	}

	public Bounds brushBound
	{
		get
		{
			if (Manipulator.activeBrush == null)
			{
				return default(Bounds);
			}
			return Manipulator.activeBrush.brushBound;
		}
	}

	public PEIsoCapture IsoCaputure => m_IsoCaputure;

	public event DVoxelModifyNotify onVoxelMotify;

	public static int GetBlockItemProtoID(byte matIndex)
	{
		if (BSBlockMatMap.s_MatToItem.ContainsKey(matIndex))
		{
			return BSBlockMatMap.s_MatToItem[matIndex];
		}
		return -1;
	}

	public static int GetBlockMaterialType(int proto_id)
	{
		if (BSBlockMatMap.s_ItemToMat.ContainsKey(proto_id))
		{
			return BSBlockMatMap.s_ItemToMat[proto_id];
		}
		return -1;
	}

	public static int GetVoxelItemProtoID(byte matIndex)
	{
		return BSVoxelMatMap.GetItemID(matIndex);
	}

	public BSIsoHeadData[] ExtractTheHeaders()
	{
		string path = GameConfig.GetUserDataPath() + BuildingMan.s_IsoPath;
		if (!Directory.Exists(path))
		{
			Directory.CreateDirectory(path);
		}
		string[] files = Directory.GetFiles(path);
		List<BSIsoHeadData> list = new List<BSIsoHeadData>();
		string[] array = files;
		foreach (string text in array)
		{
			if (text.Contains(".biso"))
			{
				BSIsoData.ExtractHeader(text, out var iso_header);
				int num = text.LastIndexOf('/') + 1;
				int num2 = text.LastIndexOf('.');
				iso_header.Name = text.Substring(num, num2 - num);
				list.Add(iso_header);
			}
		}
		return list.ToArray();
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
		BSVoxelModify.onModifyCheck += OnCheckVoxelModify;
		_manipulator.onCreateBrush += OnCreateBrush;
		m_IsoCaputure = Object.Instantiate(_IsoCapturePrefab);
		m_IsoCaputure.transform.parent = base.transform;
		m_IsoCaputure.transform.localPosition = Vector3.zero;
		m_IsoCaputure.transform.localRotation = Quaternion.identity;
	}

	private void OnDestroy()
	{
		BSVoxelModify.onModifyCheck -= OnCheckVoxelModify;
		_manipulator.onCreateBrush -= OnCreateBrush;
	}

	private void Update()
	{
		if (_manipulator.BrushType == BuildingMan.EBrushType.Select)
		{
			if (selectVoxel)
			{
				_manipulator.pattern = BSPattern.DefaultV1;
			}
			else
			{
				_manipulator.pattern = BSPattern.DefaultB1;
			}
		}
		else if (_manipulator.BrushType == BuildingMan.EBrushType.Point || _manipulator.BrushType == BuildingMan.EBrushType.Box || _manipulator.BrushType == BuildingMan.EBrushType.B45Diagonal)
		{
			if (m_Pattern != null)
			{
				_manipulator.pattern = m_Pattern;
			}
		}
		else
		{
			_manipulator.pattern = null;
		}
		if (_manipulator.BrushType != BuildingMan.EBrushType.Box)
		{
			return;
		}
		BSBoxBrush bSBoxBrush = _manipulator.activeBrush as BSBoxBrush;
		if (!(bSBoxBrush != null))
		{
			return;
		}
		Vector3 size = bSBoxBrush.Size;
		int num = Mathf.RoundToInt(size.x) * Mathf.RoundToInt(size.y) * Mathf.RoundToInt(size.z) * bSBoxBrush.pattern.size;
		if (PeSingleton<PeCreature>.Instance.mainPlayer == null)
		{
			return;
		}
		PackageCmpt cmpt = PeSingleton<PeCreature>.Instance.mainPlayer.GetCmpt<PackageCmpt>();
		if (bSBoxBrush.dataSource == BuildingMan.Blocks)
		{
			int blockItemProtoID = GetBlockItemProtoID(bSBoxBrush.materialType);
			if (cmpt.GetItemCount(blockItemProtoID) >= Mathf.CeilToInt((float)num / 4f))
			{
				bSBoxBrush.forceShowRemoveColor = false;
			}
			else
			{
				bSBoxBrush.forceShowRemoveColor = true;
			}
		}
		else if (bSBoxBrush.dataSource == BuildingMan.Voxels)
		{
			int voxelItemProtoID = GetVoxelItemProtoID(bSBoxBrush.materialType);
			if (cmpt.GetItemCount(voxelItemProtoID) >= num)
			{
				bSBoxBrush.forceShowRemoveColor = false;
			}
			else
			{
				bSBoxBrush.forceShowRemoveColor = true;
			}
		}
	}

	private void OnGUI()
	{
		if (!GUITest)
		{
			return;
		}
		int num = 20;
		int num2 = 250;
		GUI.BeginGroup(new Rect(Screen.width / 2 - 250, 50f, num2, 500f));
		int num3 = 0;
		GUI.Label(new Rect(0f, num3 * num, num2, num), "Items :");
		num3++;
		foreach (KeyValuePair<int, int> costsItem in _costsItems)
		{
			GUI.Label(new Rect(0f, num3 * num, 50f, num), "Item ID:");
			GUI.Label(new Rect(50f, num3 * num, 50f, num), costsItem.Key.ToString());
			GUI.Label(new Rect(100f, num3 * num, 50f, num), " Count:");
			GUI.Label(new Rect(150f, num3 * num, 50f, num), costsItem.Value.ToString());
			num3++;
		}
		GUI.Label(new Rect(0f, num3 * num, 50f, num), "Player Count:");
		num3++;
		foreach (KeyValuePair<int, int> playerItem in _playerItems)
		{
			GUI.Label(new Rect(0f, num3 * num, 50f, num), "Item ID:");
			GUI.Label(new Rect(50f, num3 * num, 50f, num), playerItem.Key.ToString());
			GUI.Label(new Rect(100f, num3 * num, 50f, num), " Count:");
			GUI.Label(new Rect(150f, num3 * num, 50f, num), playerItem.Value.ToString());
			num3++;
		}
		GUI.EndGroup();
	}

	private bool OnCheckVoxelModify(int opType, IntVector3[] indexes, BSVoxel[] voxels, BSVoxel[] oldvoxels, EBSBrushMode mode, IBSDataSource ds)
	{
		if (IsGod)
		{
			return true;
		}
		if (PeSingleton<PeCreature>.Instance.mainPlayer == null)
		{
			return false;
		}
		bool flag = true;
		switch (mode)
		{
		case EBSBrushMode.Add:
		{
			Dictionary<int, int> dictionary4 = new Dictionary<int, int>();
			int num5 = 1;
			for (int k = 0; k < voxels.Length; k++)
			{
				BSVoxel bSVoxel = voxels[k];
				int num6 = 0;
				if (ds == BuildingMan.Blocks)
				{
					if (bSVoxel.IsExtendable())
					{
						if (!bSVoxel.IsExtendableRoot())
						{
							num6 = GetBlockItemProtoID((byte)(bSVoxel.materialType >> 2));
							num5 = 1;
						}
						else
						{
							num5 = 0;
						}
					}
					else
					{
						num6 = GetBlockItemProtoID(bSVoxel.materialType);
					}
				}
				else if (ds == BuildingMan.Voxels)
				{
					num6 = GetVoxelItemProtoID(bSVoxel.materialType);
				}
				if (num6 > 0 && num6 != 0)
				{
					if (dictionary4.ContainsKey(num6))
					{
						Dictionary<int, int> dictionary5;
						Dictionary<int, int> dictionary6 = (dictionary5 = dictionary4);
						int key3;
						int key4 = (key3 = num6);
						key3 = dictionary5[key3];
						dictionary6[key4] = key3 + num5;
					}
					else
					{
						dictionary4.Add(num6, num5);
					}
				}
			}
			_costsItems = dictionary4;
			float num7 = 1f;
			if (ds == BuildingMan.Blocks)
			{
				num7 = 1 << BSBlock45Data.s_ScaleInverted;
			}
			PackageCmpt cmpt2 = PeSingleton<PeCreature>.Instance.mainPlayer.GetCmpt<PackageCmpt>();
			_playerItems.Clear();
			foreach (KeyValuePair<int, int> item in dictionary4)
			{
				_playerItems.Add(item.Key, cmpt2.GetItemCount(item.Key));
				if (cmpt2.GetItemCount(item.Key) < Mathf.CeilToInt((float)item.Value / num7))
				{
					flag = false;
				}
			}
			if (flag)
			{
				if (GameConfig.IsMultiMode)
				{
					if (null == PlayerNetwork.mainPlayer)
					{
						return false;
					}
					if (!PeGameMgr.IsMultiCoop && VArtifactUtil.IsInTownBallArea(PlayerNetwork.mainPlayer._pos))
					{
						new PeTipMsg(PELocalization.GetString(8000864), PeTipMsg.EMsgLevel.Warning);
						return false;
					}
					PlayerNetwork.mainPlayer.RequestRedo(opType, indexes, oldvoxels, voxels, mode, ds.DataType, ds.Scale);
					DigTerrainManager.BlockClearGrass(ds, indexes);
					return true;
				}
				string text3 = string.Empty;
				foreach (KeyValuePair<int, int> item2 in dictionary4)
				{
					if (cmpt2.Destory(item2.Key, Mathf.CeilToInt((float)item2.Value / num7)))
					{
						string text2 = text3;
						text3 = text2 + "\r\n Rmove Item from player package ID[" + item2.Key + "] count - " + item2.Value;
					}
				}
				if (ds == BuildingMan.Blocks)
				{
					for (int l = 0; l < indexes.Length; l++)
					{
						Vector3 voxelPos = new Vector3((float)indexes[l].x * ds.Scale, (float)indexes[l].y * ds.Scale, (float)indexes[l].z * ds.Scale) - ds.Offset;
						PeGrassSystem.DeleteAtPos(voxelPos);
						PeGrassSystem.DeleteAtPos(new Vector3(voxelPos.x, voxelPos.y - 1f, voxelPos.z));
					}
				}
				else if (ds == BuildingMan.Voxels)
				{
					for (int m = 0; m < indexes.Length; m++)
					{
						Vector3 voxelPos2 = new Vector3(indexes[m].x, indexes[m].y, indexes[m].z);
						PeGrassSystem.DeleteAtPos(voxelPos2);
						PeGrassSystem.DeleteAtPos(new Vector3(voxelPos2.x, voxelPos2.y - 1f, voxelPos2.z));
					}
				}
			}
			else
			{
				new PeTipMsg(PELocalization.GetString(821000001), PeTipMsg.EMsgLevel.Warning);
			}
			break;
		}
		case EBSBrushMode.Subtract:
		{
			Dictionary<int, int> dictionary = new Dictionary<int, int>();
			int num = 1;
			for (int i = 0; i < oldvoxels.Length; i++)
			{
				BSVoxel voxel = oldvoxels[i];
				int num2 = 0;
				if (ds == BuildingMan.Blocks)
				{
					if (voxel.IsExtendable())
					{
						if (!voxel.IsExtendableRoot())
						{
							num2 = GetBlockItemProtoID((byte)(voxel.materialType >> 2));
							num = 1;
						}
						else
						{
							num = 0;
						}
					}
					else if (!BuildingMan.Blocks.VoxelIsZero(voxel, 0f))
					{
						num2 = GetBlockItemProtoID(voxel.materialType);
					}
				}
				else if (ds == BuildingMan.Voxels && !BuildingMan.Voxels.VoxelIsZero(voxel, 1f))
				{
					num2 = GetVoxelItemProtoID(voxel.materialType);
				}
				if (num2 > 0)
				{
					if (dictionary.ContainsKey(num2))
					{
						Dictionary<int, int> dictionary2;
						Dictionary<int, int> dictionary3 = (dictionary2 = dictionary);
						int key;
						int key2 = (key = num2);
						key = dictionary2[key];
						dictionary3[key2] = key + num;
					}
					else
					{
						dictionary.Add(num2, num);
					}
				}
			}
			float num3 = 1f;
			if (ds == BuildingMan.Blocks)
			{
				num3 = 1 << BSBlock45Data.s_ScaleInverted;
			}
			PlayerPackageCmpt cmpt = PeSingleton<PeCreature>.Instance.mainPlayer.GetCmpt<PlayerPackageCmpt>();
			MaterialItem[] array = new MaterialItem[dictionary.Count];
			int num4 = 0;
			foreach (KeyValuePair<int, int> item3 in dictionary)
			{
				array[num4] = new MaterialItem
				{
					protoId = item3.Key,
					count = Mathf.FloorToInt((float)item3.Value / num3)
				};
				num4++;
			}
			flag = cmpt.package.CanAdd(array);
			if (!flag)
			{
				break;
			}
			if (GameConfig.IsMultiMode)
			{
				if (null == PlayerNetwork.mainPlayer)
				{
					return false;
				}
				PlayerNetwork.mainPlayer.RequestRedo(opType, indexes, oldvoxels, voxels, mode, ds.DataType, ds.Scale);
				return true;
			}
			string text = string.Empty;
			MaterialItem[] array2 = array;
			foreach (MaterialItem materialItem in array2)
			{
				if (materialItem.count != 0)
				{
					cmpt.Add(materialItem.protoId, materialItem.count);
				}
				string text2 = text;
				text = text2 + "Add Item from player package ID[" + materialItem.protoId + "] count - " + materialItem.count + "\r\n";
			}
			Debug.LogWarning(text);
			break;
		}
		}
		if (flag && this.onVoxelMotify != null)
		{
			this.onVoxelMotify(indexes, voxels, oldvoxels, mode, ds);
		}
		return flag;
	}

	private void OnCreateBrush(BSBrush brush, BuildingMan.EBrushType type)
	{
	}
}

using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class LSubTerrain
{
	private int X;

	private int Z;

	private IntVector3 _tmpVec3 = IntVector3.Zero;

	private bool m_FinishedProcess;

	private int _dataLen;

	private List<TreeInfo> m_listTrees = new List<TreeInfo>();

	private Dictionary<int, TreeInfo> m_mapTrees = new Dictionary<int, TreeInfo>();

	private Dictionary<TreeInfo, TreeInfo> m_mapTwoFeetTrees = new Dictionary<TreeInfo, TreeInfo>();

	public int xIndex => X;

	public int zIndex => Z;

	public IntVector3 iPos => new IntVector3(X, 0, Z);

	public Vector3 wPos => new Vector3((float)X * 256f, 0f, (float)Z * 256f);

	public int Index
	{
		get
		{
			return LSubTerrUtils.PosToIndex(X, Z);
		}
		set
		{
			IntVector3 intVector = LSubTerrUtils.IndexToPos(value);
			X = intVector.x;
			Z = intVector.z;
		}
	}

	public int DataLen => _dataLen;

	public int TreeCnt => m_listTrees.Count;

	public int MapKeyCnt => m_mapTrees.Count;

	public bool HasData => _dataLen > 0;

	public bool FinishedProcess => m_FinishedProcess;

	public TreeInfo GetTreeInfoListAtPos(IntVector3 pos)
	{
		int key = LSubTerrUtils.TreePosToKey(pos);
		TreeInfo value = null;
		if (m_mapTrees.TryGetValue(key, out value))
		{
			return value;
		}
		return null;
	}

	public void ApplyData(byte[] data, int len)
	{
		if (len <= 0)
		{
			m_FinishedProcess = true;
			return;
		}
		TreeInfo value;
		using (MemoryStream memoryStream = new MemoryStream(data))
		{
			BinaryReader binaryReader = new BinaryReader(memoryStream);
			int num = binaryReader.ReadInt32();
			for (int i = 0; i != num; i++)
			{
				binaryReader.ReadInt32();
				binaryReader.ReadInt32();
				binaryReader.ReadInt32();
				int num2 = binaryReader.ReadInt32();
				for (int j = 0; j != num2; j++)
				{
					value = TreeInfo.GetTI();
					value.m_clr.r = binaryReader.ReadSingle();
					value.m_clr.g = binaryReader.ReadSingle();
					value.m_clr.b = binaryReader.ReadSingle();
					value.m_clr.a = binaryReader.ReadSingle();
					value.m_heightScale = binaryReader.ReadSingle();
					value.m_lightMapClr.r = binaryReader.ReadSingle();
					value.m_lightMapClr.g = binaryReader.ReadSingle();
					value.m_lightMapClr.b = binaryReader.ReadSingle();
					value.m_lightMapClr.a = binaryReader.ReadSingle();
					value.m_pos.x = binaryReader.ReadSingle();
					value.m_pos.y = binaryReader.ReadSingle();
					value.m_pos.z = binaryReader.ReadSingle();
					value.m_protoTypeIdx = binaryReader.ReadInt32();
					value.m_widthScale = binaryReader.ReadSingle();
					AddTreeInfo(value);
				}
			}
			binaryReader.Close();
			memoryStream.Close();
		}
		if (LSubTerrSL.m_mapDelPos.TryGetValue(Index, out var value2))
		{
			int count = value2.Count;
			for (int k = 0; k < count; k++)
			{
				Vector3 posInTile = value2[k];
				_tmpVec3.x = Mathf.FloorToInt(posInTile.x);
				_tmpVec3.y = Mathf.FloorToInt(posInTile.y);
				_tmpVec3.z = Mathf.FloorToInt(posInTile.z);
				int key = LSubTerrUtils.TreePosToKey(_tmpVec3);
				if (!m_mapTrees.TryGetValue(key, out value))
				{
					continue;
				}
				value = value.FindTi(posInTile);
				if (value != null)
				{
					TreeInfo treeInfo = DeleteTreeInfo(value);
					if (treeInfo != null)
					{
						DeleteTreeInfo(treeInfo);
					}
				}
			}
		}
		List<LSubTerrLayerOption> layers = LSubTerrainMgr.Instance.Layers;
		List<TreeInfo>[] array = new List<TreeInfo>[layers.Count];
		for (int num3 = layers.Count - 1; num3 >= 0; num3--)
		{
			array[num3] = new List<TreeInfo>();
		}
		foreach (TreeInfo listTree in m_listTrees)
		{
			float num4 = LSubTerrainMgr.Instance.GlobalPrototypeBounds[listTree.m_protoTypeIdx].extents.y * 2f;
			for (int num5 = layers.Count - 1; num5 >= 0; num5--)
			{
				if (layers[num5].MinTreeHeight <= num4 && num4 < layers[num5].MaxTreeHeight)
				{
					array[num5].Add(listTree);
					break;
				}
			}
		}
		for (int num6 = layers.Count - 1; num6 >= 0; num6--)
		{
			LSubTerrainMgr.Instance.LayerCreators[num6].AddTreeBatch(Index, array[num6]);
		}
		m_FinishedProcess = true;
	}

	public TreeInfo AddTreeInfo(Vector3 wpos, int prototype, float wScale, float hScale)
	{
		TreeInfo tI = TreeInfo.GetTI();
		tI.m_clr = Color.white;
		tI.m_lightMapClr = Color.white;
		tI.m_widthScale = wScale;
		tI.m_heightScale = hScale;
		tI.m_protoTypeIdx = prototype;
		tI.m_pos = LSubTerrUtils.TreeWorldPosToTerrainPos(wpos);
		return AddTreeInfo(tI);
	}

	private TreeInfo AddTreeInfo(TreeInfo ti)
	{
		if (ti.m_protoTypeIdx == 63 || (double)ti.m_pos.x < 1E-05 || (double)ti.m_pos.z < 1E-05 || (double)ti.m_pos.x > 0.99999 || (double)ti.m_pos.z > 0.99999)
		{
			TreeInfo.FreeTI(ti);
			return null;
		}
		_tmpVec3.x = Mathf.FloorToInt(ti.m_pos.x * 256f);
		_tmpVec3.y = Mathf.FloorToInt(ti.m_pos.y * 3000f);
		_tmpVec3.z = Mathf.FloorToInt(ti.m_pos.z * 256f);
		int key = LSubTerrUtils.TreePosToKey(_tmpVec3);
		if (m_mapTrees.TryGetValue(key, out var value))
		{
			value.AttachTi(ti);
		}
		else
		{
			m_mapTrees.Add(key, ti);
		}
		m_listTrees.Add(ti);
		if (LSubTerrainMgr.HasCollider(ti.m_protoTypeIdx) || LSubTerrainMgr.HasLight(ti.m_protoTypeIdx))
		{
			key = LSubTerrUtils.TreeWorldPosTo32Key(LSubTerrUtils.TreeTerrainPosToWorldPos(X, Z, ti.m_pos));
			if (!LSubTerrainMgr.Instance.m_map32Trees.TryGetValue(key, out var value2))
			{
				value2 = new List<TreeInfo>();
				LSubTerrainMgr.Instance.m_map32Trees.Add(key, value2);
			}
			value2.Add(ti);
		}
		LTreePlaceHolderInfo treePlaceHolderInfo = LSubTerrainMgr.GetTreePlaceHolderInfo(ti.m_protoTypeIdx);
		if (treePlaceHolderInfo != null)
		{
			TreeInfo tI = TreeInfo.GetTI();
			tI.m_clr = Color.white;
			tI.m_heightScale = treePlaceHolderInfo.m_HeightScale * ti.m_heightScale;
			tI.m_lightMapClr = Color.white;
			Vector3 terrOffset = treePlaceHolderInfo.TerrOffset;
			terrOffset.x *= ti.m_widthScale;
			terrOffset.y *= ti.m_heightScale;
			terrOffset.z *= ti.m_widthScale;
			tI.m_pos = ti.m_pos + terrOffset;
			tI.m_protoTypeIdx = 63;
			tI.m_widthScale = treePlaceHolderInfo.m_WidthScale * ti.m_widthScale;
			_tmpVec3.x = Mathf.FloorToInt(tI.m_pos.x * 256f);
			_tmpVec3.y = Mathf.FloorToInt(tI.m_pos.y * 3000f);
			_tmpVec3.z = Mathf.FloorToInt(tI.m_pos.z * 256f);
			key = LSubTerrUtils.TreePosToKey(_tmpVec3);
			if (m_mapTrees.TryGetValue(key, out value))
			{
				value.AttachTi(tI);
			}
			else
			{
				m_mapTrees.Add(key, tI);
			}
			m_listTrees.Add(tI);
			m_mapTwoFeetTrees.Add(tI, ti);
			m_mapTwoFeetTrees.Add(ti, tI);
		}
		return ti;
	}

	public TreeInfo DeleteTreeInfo(TreeInfo ti)
	{
		_tmpVec3.x = Mathf.FloorToInt(ti.m_pos.x * 256f);
		_tmpVec3.y = Mathf.FloorToInt(ti.m_pos.y * 3000f);
		_tmpVec3.z = Mathf.FloorToInt(ti.m_pos.z * 256f);
		int key = LSubTerrUtils.TreePosToKey(_tmpVec3);
		TreeInfo.RemoveTiFromDict(m_mapTrees, key, ti);
		if (m_listTrees.Remove(ti))
		{
			TreeInfo.FreeTI(ti);
		}
		key = LSubTerrUtils.TreeWorldPosTo32Key(LSubTerrUtils.TreeTerrainPosToWorldPos(X, Z, ti.m_pos));
		if (LSubTerrainMgr.Instance.m_map32Trees.TryGetValue(key, out var value))
		{
			value.Remove(ti);
			if (value.Count == 0)
			{
				LSubTerrainMgr.Instance.m_map32Trees.Remove(key);
			}
		}
		if (m_mapTwoFeetTrees.TryGetValue(ti, out var value2))
		{
			m_mapTwoFeetTrees.Remove(ti);
			m_mapTwoFeetTrees.Remove(value2);
			return value2;
		}
		return null;
	}

	public void Release()
	{
		_dataLen = 0;
		if (m_mapTrees != null)
		{
			m_mapTrees.Clear();
		}
		if (m_listTrees != null)
		{
			TreeInfo.FreeTIs(m_listTrees);
			m_listTrees.Clear();
		}
		if (!(LSubTerrainMgr.Instance != null))
		{
			return;
		}
		for (int i = 0; i < LSubTerrainMgr.Instance.Layers.Count; i++)
		{
			LSubTerrainMgr.Instance.LayerCreators[i].DelTreeBatch(Index);
		}
		for (int j = X * 8; j < X * 8 + 8; j++)
		{
			for (int k = Z * 8; k < Z * 8 + 8; k++)
			{
				int key = LSubTerrUtils.Tree32PosTo32Key(j, k);
				LSubTerrainMgr.Instance.m_map32Trees.Remove(key);
			}
		}
	}

	public byte[] ExportSubTerrainData()
	{
		byte[] array = new byte[0];
		using MemoryStream memoryStream = new MemoryStream();
		BinaryWriter binaryWriter = new BinaryWriter(memoryStream);
		binaryWriter.Write(m_listTrees.Count);
		foreach (TreeInfo listTree in m_listTrees)
		{
			binaryWriter.Write(0);
			binaryWriter.Write(0);
			binaryWriter.Write(0);
			binaryWriter.Write(1);
			binaryWriter.Write(listTree.m_clr.r);
			binaryWriter.Write(listTree.m_clr.g);
			binaryWriter.Write(listTree.m_clr.b);
			binaryWriter.Write(listTree.m_clr.a);
			binaryWriter.Write(listTree.m_heightScale);
			binaryWriter.Write(listTree.m_lightMapClr.r);
			binaryWriter.Write(listTree.m_lightMapClr.g);
			binaryWriter.Write(listTree.m_lightMapClr.b);
			binaryWriter.Write(listTree.m_lightMapClr.a);
			binaryWriter.Write(listTree.m_pos.x);
			binaryWriter.Write(listTree.m_pos.y);
			binaryWriter.Write(listTree.m_pos.z);
			binaryWriter.Write(listTree.m_protoTypeIdx);
			binaryWriter.Write(listTree.m_widthScale);
		}
		array = memoryStream.ToArray();
		binaryWriter.Close();
		memoryStream.Close();
		return array;
	}

	public void SaveCache()
	{
		if (Application.isEditor)
		{
			byte[] array = ExportSubTerrainData();
			string userDataPath = GameConfig.GetUserDataPath();
			if (!Directory.Exists(userDataPath + "/PlanetExplorers/CreateData/SubTerrains"))
			{
				Directory.CreateDirectory(userDataPath + "/PlanetExplorers/CreateData/SubTerrains");
			}
			string text = userDataPath + "/PlanetExplorers/CreateData/SubTerrains/";
			using FileStream fileStream = new FileStream(text + "cache_" + Index + ".subter", FileMode.Create, FileAccess.ReadWrite, FileShare.ReadWrite);
			BinaryWriter binaryWriter = new BinaryWriter(fileStream);
			binaryWriter.Write(array, 0, array.Length);
			binaryWriter.Close();
			fileStream.Close();
		}
	}
}

using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using Pathea;
using UnityEngine;

public class VFVoxelWater : MonoBehaviour, ILODNodeDataMan, IVxChunkHelperProc
{
	public const string ArchiveKey = "ArchiveKeyVoxelWater";

	public const byte c_iSeaWaterType = 128;

	public const byte c_iSurfaceVol = 128;

	public const int c_Sig = 1;

	public static VFVoxelWater self;

	public static float c_fWaterLvl = 97f;

	public static byte[][] s_surfaceChunkData = null;

	public static VFVoxel c_WaterSource = new VFVoxel(byte.MaxValue, 128);

	public static bool s_bSeaInSight;

	public static int s_layer;

	public VFVoxelSave SaveLoad;

	[SerializeField]
	private Material _waterMat;

	private byte[] _tmpSurfChunkData4Req = new byte[85750];

	private MCOutputData _tmpSeaSurfaceVerts = new MCOutputData(null, null, null, null);

	private MCOutputData[] _seaSurfaceVerts = new MCOutputData[5];

	private List<SurfExtractReqMC> _seaSurfaceChunkReqs = new List<SurfExtractReqMC>();

	private List<IntVector4> _dirtyChunkPosList = new List<IntVector4>();

	private EulerianFluidProcessor _fluidProcessor;

	private IVxDataSource _voxels;

	private IVxDataLoader _dataLoader;

	private int _idxInLODNodeData;

	private static int _frameCnt;

	public Material WaterMat
	{
		get
		{
			return _waterMat;
		}
		set
		{
			if (_waterMat != value)
			{
				_waterMat = value;
				MeshRenderer[] componentsInChildren = base.gameObject.GetComponentsInChildren<MeshRenderer>();
				int num = componentsInChildren.Length;
				for (int i = 0; i < num; i++)
				{
					componentsInChildren[i].sharedMaterial = _waterMat;
				}
			}
		}
	}

	public IVxDataSource Voxels => _voxels;

	public int IdxInLODNodeData
	{
		get
		{
			return _idxInLODNodeData;
		}
		set
		{
			_idxInLODNodeData = value;
		}
	}

	public LODOctreeMan LodMan { get; set; }

	public IVxSurfExtractor SurfExtractor => SurfExtractorsMan.VxSurfExtractor;

	public int ChunkSig => 1;

	private void ProcGenSeaSurface(SurfExtractReqMC req)
	{
		_tmpSeaSurfaceVerts.Clone(_seaSurfaceVerts[req._chunk.LOD]);
		req.AddOutData(_tmpSeaSurfaceVerts);
		VFVoxelChunkGo vFVoxelChunkGo = VFVoxelChunkGo.CreateChunkGo(req, _waterMat, s_layer);
		vFVoxelChunkGo.transform.parent = base.gameObject.transform;
		req._chunkData = _tmpSurfChunkData4Req;
		req._chunk.AttachChunkGo(vFVoxelChunkGo, req);
	}

	private void Awake()
	{
		self = this;
		s_bSeaInSight = false;
		s_layer = 4;
	}

	public void Import(PeRecordReader r)
	{
		ApplyQuality(SystemSettingData.Instance.WaterRefraction, SystemSettingData.Instance.WaterDepth);
		SaveLoad = new VFVoxelSave("ArchiveKeyVoxelWater", AddtionalReader, AddtionalWriter);
		SaveLoad.Import(r);
		if (s_surfaceChunkData == null)
		{
			InitSufaceChunkData();
		}
	}

	private void LateUpdate()
	{
		s_bSeaInSight = false;
	}

	private void OnDestroy()
	{
		if (SurfExtractor != null)
		{
			SurfExtractor.Reset();
		}
		if (_dataLoader != null)
		{
			_dataLoader.Close();
		}
		c_fWaterLvl = 97f;
		Debug.Log("Mem size before vfTerrain destroyed all chunks :" + GC.GetTotalMemory(forceFullCollection: true));
		if (base.gameObject.transform.childCount > 0)
		{
			for (int num = base.gameObject.transform.childCount - 1; num >= 0; num--)
			{
				GameObject obj = base.gameObject.transform.GetChild(num).gameObject;
				UnityEngine.Object.DestroyImmediate(obj);
			}
		}
		Resources.UnloadUnusedAssets();
		self = null;
		VFVoxelSave.Clean();
	}

	private void StartFluidProcessor(string para)
	{
		StartCoroutine(CoFuildProcess());
	}

	private IEnumerator CoFuildProcess()
	{
		while (true)
		{
			if (SurfExtractor.IsAllClear && VFVoxelTerrain.TerrainVoxelComplete)
			{
				_fluidProcessor.UpdateFluid(bRebuild: true);
				int n = _fluidProcessor.DirtyChunkList.Count;
				for (int i = 0; i < n; i++)
				{
					SaveLoad.AddChunkToSaveList(_fluidProcessor.DirtyChunkList[i]);
				}
			}
			yield return 0;
		}
	}

	public void OnWriteVoxelOfTerrain(LODOctreeNode node, byte oldVol, byte newVol, int idxVol)
	{
		if (newVol < oldVol)
		{
			IntVector4 item = new IntVector4(node.CX, node.CY, node.CZ, node.Lod);
			if (!_fluidProcessor.DirtyChunkPosList.Contains(item))
			{
				_fluidProcessor.DirtyChunkPosList.Add(item);
			}
		}
	}

	private void AddtionalReader(BinaryReader br)
	{
		_dirtyChunkPosList = new List<IntVector4>();
		int num = br.ReadInt32();
		for (int i = 0; i < num; i++)
		{
			int x_ = br.ReadInt32();
			int y_ = br.ReadInt32();
			int z_ = br.ReadInt32();
			int w_ = br.ReadInt32();
			_dirtyChunkPosList.Add(new IntVector4(x_, y_, z_, w_));
		}
	}

	private void AddtionalWriter(BinaryWriter bw)
	{
		int count = _fluidProcessor.DirtyChunkPosList.Count;
		bw.Write(count);
		for (int i = 0; i < count; i++)
		{
			IntVector4 intVector = _fluidProcessor.DirtyChunkPosList[i];
			bw.Write(intVector.x);
			bw.Write(intVector.y);
			bw.Write(intVector.z);
			bw.Write(intVector.w);
		}
	}

	public void ApplyQuality(bool opt0, bool opt1)
	{
	}

	public bool IsInWater(Vector3 pos)
	{
		return IsInWater(pos.x, pos.y, pos.z);
	}

	public bool IsInWater(float fx, float fy, float fz)
	{
		try
		{
			if (SingleGameStory.curType == SingleGameStory.StoryScene.PajaShip || RandomDunGenUtil.IsDungeonPosY(fy))
			{
				float num = 50f;
				return Physics.Raycast(new Vector3(fx, fy + num, fz), Vector3.down, num, 16);
			}
			if (PeGameMgr.IsMulti && PlayerNetwork.mainPlayer != null && PlayerNetwork.mainPlayer._curSceneId != 0)
			{
				float num2 = 50f;
				return Physics.Raycast(new Vector3(fx, fy + num2, fz), Vector3.down, num2, 16);
			}
			if (_voxels == null)
			{
				return false;
			}
			int num3 = Mathf.RoundToInt(fx);
			int num4 = Mathf.RoundToInt(fy);
			int num5 = Mathf.RoundToInt(fz);
			int cx = num3 >> 5;
			int cy = num4 >> 5;
			int cz = num5 >> 5;
			VFVoxelChunkData vFVoxelChunkData = _voxels.readChunk(cx, cy, cz);
			if (vFVoxelChunkData == null || vFVoxelChunkData.DataVT.Length < 2)
			{
				return c_fWaterLvl > fy;
			}
			if (vFVoxelChunkData.IsHollow)
			{
				return vFVoxelChunkData.DataVT[0] switch
				{
					0 => false, 
					byte.MaxValue => true, 
					_ => c_fWaterLvl > fy, 
				};
			}
			int num6 = VFVoxelChunkData.OneIndex(num3 & 0x1F, num4 & 0x1F, num5 & 0x1F);
			int num7 = num6 << 1;
			byte b = vFVoxelChunkData.DataVT[num7];
			if (b == 0)
			{
				return false;
			}
			if (b == byte.MaxValue)
			{
				return true;
			}
			byte b2;
			if (b < 128)
			{
				b2 = vFVoxelChunkData.DataVT[num7 - 70];
				if (b2 >= 128)
				{
					return (float)(128 - b2) / (float)(b - b2) + (float)num4 - 1f > fy;
				}
				b2 = vFVoxelChunkData.DataVT[num7 + 70];
				if (b2 >= 128)
				{
					return (float)(128 - b) / (float)(b2 - b) + (float)num4 < fy;
				}
				return false;
			}
			b2 = vFVoxelChunkData.DataVT[num7 - 70];
			if (b2 < 128)
			{
				return (float)(128 - b2) / (float)(b - b2) + (float)num4 - 1f < fy;
			}
			b2 = vFVoxelChunkData.DataVT[num7 + 70];
			if (b2 < 128)
			{
				return (float)(128 - b) / (float)(b2 - b) + (float)num4 > fy;
			}
			return true;
		}
		catch
		{
		}
		return false;
	}

	public float DownToWaterSurface(float fx, float fy, float fz)
	{
		int num = Mathf.RoundToInt(fx);
		int num2 = Mathf.RoundToInt(fy);
		int num3 = Mathf.RoundToInt(fz);
		int cx = num >> 5;
		int num4 = num2 >> 5;
		int cz = num3 >> 5;
		int num5 = num2 & 0x1F;
		int num6 = VFVoxelChunkData.OneIndex(num & 0x1F, 31, num3 & 0x1F) << 1;
		int num7 = num6 - (31 - num5) * 70;
		while (num4 >= 0)
		{
			VFVoxelChunkData vFVoxelChunkData = _voxels.readChunk(cx, num4, cz);
			if (vFVoxelChunkData == null || vFVoxelChunkData.DataVT.Length < 2)
			{
				return fy - c_fWaterLvl;
			}
			if (vFVoxelChunkData.IsHollow)
			{
				switch (vFVoxelChunkData.DataVT[0])
				{
				case 0:
					num4--;
					num5 = 31;
					num7 = num6;
					break;
				case byte.MaxValue:
					return -1f;
				default:
					return fy - c_fWaterLvl;
				}
				continue;
			}
			byte[] dataVT = vFVoxelChunkData.DataVT;
			while (num5 >= 0)
			{
				byte b = dataVT[num7];
				if (b >= 128)
				{
					byte b2 = dataVT[num7 + 70];
					if (b2 >= 128)
					{
						return -1f;
					}
					float num8 = (float)(128 - b) / (float)(b2 - b) + (float)num5;
					return fy - ((float)(num4 << 5) + num8);
				}
				num5--;
				num7 -= 70;
			}
			num4--;
			num5 = 31;
			num7 = num6;
		}
		return -1f;
	}

	public float UpToWaterSurface(float fx, float fy, float fz)
	{
		if (SingleGameStory.curType == SingleGameStory.StoryScene.PajaShip || RandomDunGenUtil.IsDungeonPosY(fy))
		{
			float num = 20f;
			if (Physics.Raycast(new Vector3(fx, fy + num, fz), Vector3.down, out var hitInfo, num, 16))
			{
				return Vector3.Distance(new Vector3(fx, fy, fz), hitInfo.point);
			}
			return -1f;
		}
		if (PeGameMgr.IsMulti && PlayerNetwork.mainPlayer != null && PlayerNetwork.mainPlayer._curSceneId != 0)
		{
			float num2 = 20f;
			if (Physics.Raycast(new Vector3(fx, fy + num2, fz), Vector3.down, out var hitInfo2, num2, 16))
			{
				return Vector3.Distance(new Vector3(fx, fy, fz), hitInfo2.point);
			}
			return -1f;
		}
		int num3 = Mathf.RoundToInt(fx);
		int num4 = Mathf.RoundToInt(fy);
		int num5 = Mathf.RoundToInt(fz);
		int cx = num3 >> 5;
		int num6 = num4 >> 5;
		int cz = num5 >> 5;
		int num7 = num4 & 0x1F;
		int num8 = VFVoxelChunkData.OneIndex(num3 & 0x1F, 0, num5 & 0x1F) << 1;
		int num9 = num8 + num7 * 70;
		while (num6 < 32)
		{
			VFVoxelChunkData vFVoxelChunkData = _voxels.readChunk(cx, num6, cz);
			if (vFVoxelChunkData == null || vFVoxelChunkData.DataVT.Length < 2)
			{
				return c_fWaterLvl - fy;
			}
			if (vFVoxelChunkData.IsHollow)
			{
				switch (vFVoxelChunkData.DataVT[0])
				{
				case byte.MaxValue:
					num6++;
					num7 = 0;
					num9 = num8;
					break;
				case 0:
					return -1f;
				default:
					return c_fWaterLvl - fy;
				}
				continue;
			}
			byte[] dataVT = vFVoxelChunkData.DataVT;
			while (num7 < 32)
			{
				byte b = dataVT[num9];
				if (b < 128)
				{
					byte b2 = dataVT[num9 - 70];
					if (b2 < 128)
					{
						return -1f;
					}
					float num10 = (float)(128 - b2) / (float)(b - b2) + (float)num7 - 1f;
					return (float)(num6 << 5) + num10 - fy;
				}
				num7++;
				num9 += 70;
			}
			num6++;
			num7 = 0;
			num9 = num8;
		}
		return -1f;
	}

	private void SetSurfaceMeshes()
	{
		lock (_seaSurfaceChunkReqs)
		{
			int count = _seaSurfaceChunkReqs.Count;
			for (int i = 0; i < count; i++)
			{
				_seaSurfaceChunkReqs[i].OnReqFinished();
			}
			_seaSurfaceChunkReqs.Clear();
		}
	}

	public void OnWaterDataLoad(VFVoxelChunkData chunkData, byte[] chunkDataVT, bool bFromPool)
	{
		if (chunkDataVT.Length != 2 || chunkDataVT[0] != 128)
		{
			chunkData.OnDataLoaded(chunkDataVT, bFromPool);
			return;
		}
		int num = 1 << chunkData.LOD;
		if (c_fWaterLvl >= (float)(chunkData.ChunkPosLod.y + num << 5))
		{
			chunkData.OnDataLoaded(VFVoxelChunkData.S_ChunkDataWaterSolid);
			return;
		}
		if (_seaSurfaceVerts[chunkData.LOD] == null)
		{
			float y = (c_fWaterLvl - (float)(chunkData.ChunkPosLod.y << 5)) / (float)num;
			Vector3[] verts = new Vector3[4]
			{
				new Vector3(0f, y, 0f),
				new Vector3(0f, y, 32f),
				new Vector3(32f, y, 0f),
				new Vector3(32f, y, 32f)
			};
			Vector2[] norm = new Vector2[4]
			{
				default(Vector2),
				default(Vector2),
				default(Vector2),
				default(Vector2)
			};
			Vector2[] norm2t = new Vector2[4]
			{
				default(Vector2),
				default(Vector2),
				default(Vector2),
				default(Vector2)
			};
			int[] indice = new int[6] { 0, 1, 2, 1, 3, 2 };
			MCOutputData mCOutputData = new MCOutputData(verts, norm, norm2t, indice);
			_seaSurfaceVerts[chunkData.LOD] = mCOutputData;
		}
		lock (_seaSurfaceChunkReqs)
		{
			_seaSurfaceChunkReqs.Add(SurfExtractReqMC.Get(chunkData, ProcGenSeaSurface));
		}
		chunkData.SetDataVT(chunkDataVT);
	}

	public ILODNodeData CreateLODNodeData(LODOctreeNode node)
	{
		VFVoxelChunkData vFVoxelChunkData = new VFVoxelChunkData(node);
		vFVoxelChunkData.HelperProc = this;
		return vFVoxelChunkData;
	}

	public void ProcPostLodInit()
	{
		object dataLoader2;
		if (VFVoxelTerrain.RandomMap)
		{
			IVxDataLoader dataLoader = VFVoxelTerrain.self.DataLoader;
			dataLoader2 = dataLoader;
		}
		else
		{
			dataLoader2 = new VFDataReader((!string.IsNullOrEmpty(VFVoxelTerrain.MapDataPath_Zip)) ? (VFVoxelTerrain.MapDataPath_Zip + "/water") : string.Empty, OnWaterDataLoad);
		}
		_dataLoader = (IVxDataLoader)dataLoader2;
		_voxels = new VFLODDataSource(LodMan.LodTreeNodes, IdxInLODNodeData);
		_fluidProcessor = new EulerianFluidProcessor();
		_fluidProcessor.DirtyChunkPosList.AddRange(_dirtyChunkPosList);
		StartCoroutine(CoFuildProcess());
	}

	public void ProcPostLodUpdate()
	{
		s_bSeaInSight = false;
		SetSurfaceMeshes();
	}

	public void ProcPostLodRefresh()
	{
		VFVoxelChunkData.EndAllReqs();
		_dataLoader.ProcessReqs();
	}

	public void ChunkProcPreSetDataVT(ILODNodeData ndata, byte[] data, bool bFromPool)
	{
		VFVoxelChunkData item = ndata as VFVoxelChunkData;
		if (!GameConfig.IsMultiMode && data == VFVoxelChunkData.S_ChunkDataNull && SaveLoad.ChunkSaveList.Contains(item))
		{
			SaveLoad.SaveChunksInListToTmpFile();
		}
	}

	public void ChunkProcPreLoadData(ILODNodeData ndata)
	{
		VFVoxelChunkData vFVoxelChunkData = ndata as VFVoxelChunkData;
		byte[] array = SaveLoad.TryGetChunkData(vFVoxelChunkData.ChunkPosLod);
		if (array != null)
		{
			vFVoxelChunkData.OnDataLoaded(array, bFromPool: true);
		}
		else
		{
			_dataLoader.AddRequest(vFVoxelChunkData);
		}
	}

	public bool ChunkProcExtractData(ILODNodeData ndata)
	{
		VFVoxelChunkData vFVoxelChunkData = ndata as VFVoxelChunkData;
		byte b = vFVoxelChunkData.DataVT[0];
		if (b == 128)
		{
			return false;
		}
		VFVoxelChunkData.ExpandHollowChunkData(vFVoxelChunkData);
		return true;
	}

	public VFVoxel ChunkProcExtractData(ILODNodeData ndata, int x, int y, int z)
	{
		VFVoxelChunkData vFVoxelChunkData = ndata as VFVoxelChunkData;
		byte b = vFVoxelChunkData.DataVT[0];
		byte type = vFVoxelChunkData.DataVT[1];
		if (b != 128)
		{
			return new VFVoxel(b, type);
		}
		int num = y + (vFVoxelChunkData.ChunkPosLod.y << 5);
		float num2 = ((vFVoxelChunkData.LOD != 0) ? (c_fWaterLvl / (float)(1 << vFVoxelChunkData.LOD)) : c_fWaterLvl);
		if (num2 >= (float)num + 0.5f)
		{
			return new VFVoxel(byte.MaxValue, 128);
		}
		if (num2 <= (float)num - 0.5f)
		{
			return new VFVoxel(0, 0);
		}
		if (Mathf.Abs(num2 - (float)num) < 0.1f)
		{
			return new VFVoxel(128, 128);
		}
		if (num2 > (float)num)
		{
			return new VFVoxel((byte)(128f + (num2 - (float)num) * 128f), 128);
		}
		return new VFVoxel((byte)(255f - 128f / (num2 - (float)num + 1f)), 128);
	}

	public void ChunkProcPostGenMesh(IVxSurfExtractReq ireq)
	{
		SurfExtractReqMC surfExtractReqMC = ireq as SurfExtractReqMC;
		VFVoxelChunkGo vFVoxelChunkGo = VFVoxelChunkGo.CreateChunkGo(surfExtractReqMC, _waterMat, s_layer);
		if (vFVoxelChunkGo != null)
		{
			vFVoxelChunkGo.transform.parent = base.gameObject.transform;
		}
		surfExtractReqMC._chunk.AttachChunkGo(vFVoxelChunkGo, surfExtractReqMC);
	}

	public void OnBegUpdateNodeData(ILODNodeData ndata)
	{
	}

	public void OnEndUpdateNodeData(ILODNodeData ndata)
	{
	}

	public void OnDestroyNodeData(ILODNodeData ndata)
	{
	}

	private void FixedUpdate()
	{
		if (LodMan == null || !(LodMan.Observer != null) || _frameCnt == Time.frameCount || !VFVoxelTerrain.bChunkColliderRebuilding)
		{
			return;
		}
		_frameCnt = Time.frameCount;
		Vector3 position = LodMan.Observer.position;
		int num = (int)position.x >> 5;
		int num2 = (int)position.y >> 5;
		int num3 = (int)position.z >> 5;
		int count = VFVoxelTerrain.OrderedOffsetList.Count;
		for (int i = 0; i < count; i++)
		{
			IntVector3 intVector = VFVoxelTerrain.OrderedOffsetList[i];
			VFVoxelChunkData chunk = _voxels.readChunk(intVector.x + num, intVector.y + num2, intVector.z + num3);
			VFVoxelChunkGo vFVoxelChunkGo = VFVoxelTerrain.GenOneCollider(chunk);
			if (null != vFVoxelChunkGo)
			{
				vFVoxelChunkGo.OnColliderReady();
				break;
			}
		}
	}

	public static void InitSufaceChunkData()
	{
		s_surfaceChunkData = new byte[5][];
		for (int i = 0; i <= 4; i++)
		{
			float num = c_fWaterLvl / (float)(1 << i);
			int num2 = (int)(num + 0.5f);
			float num3 = num - (float)(int)num;
			int num4 = (num2 & 0x1F) + 1;
			byte b = ((!(num3 < 0.5f)) ? ((byte)(255.999f * (1f - 0.5f / num3))) : ((byte)(128f / (1f - num3))));
			s_surfaceChunkData[i] = new byte[85750];
			byte[] array = s_surfaceChunkData[i];
			int num5 = 0;
			int num6 = (35 - num4 - 1) * 70;
			for (int j = 0; j < 35; j++)
			{
				for (int k = 0; k < num4; k++)
				{
					for (int l = 0; l < 35; l++)
					{
						array[num5++] = byte.MaxValue;
						array[num5++] = 128;
					}
				}
				for (int m = 0; m < 35; m++)
				{
					array[num5++] = b;
					array[num5++] = 128;
				}
				num5 += num6;
			}
		}
	}

	public static void ExpandSurfaceChunkData(VFVoxelChunkData cdata)
	{
		cdata.DataVT[0] = (cdata.DataVT[1] = 0);
		VFVoxelChunkData.ExpandHollowChunkData(cdata);
		Array.Copy(s_surfaceChunkData[cdata.LOD], cdata.DataVT, 85750);
	}
}

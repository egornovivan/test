using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using NaturalResAsset;
using Pathea;
using Pathea.Effect;
using Pathea.Projectile;
using SkillSystem;
using UnityEngine;

public class VFVoxelTerrain : SkEntity, ILODNodeDataMan, IVxChunkHelperProc
{
	private class VoxelInAttack
	{
		public float invalidTimePoint;

		public byte volume;

		public byte type;
	}

	public delegate void DirtyVoxelEvent(Vector3 pos, byte terrainType);

	public delegate void DelegateChunksColliderRebuild(Bounds bounds);

	public const string ArchiveKey = "ArchiveKeyVoxelTerrain";

	private const float atkActiveTime = 10f;

	public static VFVoxelTerrain self;

	public static bool TerrainVoxelComplete = false;

	public static bool TerrainColliderComplete = false;

	public static bool bChunkColliderRebuilding = true;

	public static string MapDataPath_Zip = null;

	public bool saveTerrain;

	private int lastSaveChunkTime;

	public VFVoxelSave SaveLoad;

	public Material _defMat;

	private IVxDataSource _voxels;

	private IVxDataLoader _dataLoader;

	private LODDataUpdate _lodDataUpdate;

	private TransvoxelGoCreator _transGoCreator;

	private List<SurfExtractReqMC> _lstReqsToFin = new List<SurfExtractReqMC>();

	private int _idxInLODNodeData;

	private static int _frameCnt;

	private static List<VFVoxelChunkGo> s_tmpVfGoSubs = new List<VFVoxelChunkGo>(4);

	public static List<IntVector3> OrderedOffsetList = new List<IntVector3>();

	private Dictionary<IntVector3, VoxelInAttack> listVoxelInAttack = new Dictionary<IntVector3, VoxelInAttack>();

	public IVxDataSource Voxels => _voxels;

	public IVxDataLoader DataLoader => _dataLoader;

	public LODDataUpdate LodDataUpdate => _lodDataUpdate;

	public TransvoxelGoCreator TransGoCreator => _transGoCreator;

	public Bounds ViewBounds => LodMan._viewBounds;

	public bool IsInGenerating => SurfExtractor.IsIdle;

	public static bool RandomMap { get; set; }

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

	public int ChunkSig => 0;

	public static event DirtyVoxelEvent onDirtyVoxel;

	public static event DelegateChunksColliderRebuild ChunksColliderRebuildHandler;

	private void InitSkEntity()
	{
		Init(onAlterAttribs, null, 97);
	}

	private void onAlterAttribs(int idx, float oldValue, float newValue)
	{
		switch (idx)
		{
		case 0:
		{
			SkEntity casterToModAttrib2 = GetCasterToModAttrib(idx);
			if (null != casterToModAttrib2)
			{
				ProcChangeTypes(casterToModAttrib2);
			}
			break;
		}
		case 1:
		{
			SkEntity casterToModAttrib = GetCasterToModAttrib(idx);
			if (null != casterToModAttrib)
			{
				ProcDigVoxels(casterToModAttrib);
			}
			break;
		}
		}
	}

	private void ProcDigVoxels(SkEntity caster)
	{
		SkInst skInst = SkRuntimeInfo.Current as SkInst;
		IDigTerrain digTerrain = caster as IDigTerrain;
		IntVector4 intVector = null;
		if (digTerrain == null || (intVector = digTerrain.digPosType) == IntVector4.Zero || (skInst != null && !(skInst._target is VFVoxelTerrain)))
		{
			return;
		}
		float num = caster.GetAttribute(25) * (1f + caster.GetAttribute(21));
		float attribute = caster.GetAttribute(22);
		float attribute2 = caster.GetAttribute(23);
		float attribute3 = caster.GetAttribute(21);
		bool flag = intVector.w == 1;
		IntVector3 xYZ = intVector.XYZ;
		if (!GameConfig.IsMultiMode)
		{
			List<VFVoxel> removeList = new List<VFVoxel>();
			DigTerrainManager.DigTerrain(xYZ, num * ((!flag) ? 1f : 5f), attribute, attribute3, ref removeList, flag);
			if (!flag)
			{
				return;
			}
			bool bGetSpItems = false;
			if (caster is SkAliveEntity)
			{
				SkAliveEntity skAliveEntity = (SkAliveEntity)caster;
				SkillTreeUnitMgr cmpt = skAliveEntity.Entity.GetCmpt<SkillTreeUnitMgr>();
				bGetSpItems = cmpt.CheckMinerGetRare();
			}
			Dictionary<int, int> resouce = DigTerrainManager.GetResouce(removeList, attribute2, bGetSpItems);
			if (resouce.Count <= 0)
			{
				return;
			}
			List<int> list = new List<int>(resouce.Count * 2);
			foreach (int key in resouce.Keys)
			{
				list.Add(key);
				list.Add(resouce[key]);
			}
			caster.Pack += list.ToArray();
		}
		else if (caster != null && caster._net != null)
		{
			bool bGetSpItems2 = false;
			if (flag && caster is SkAliveEntity)
			{
				SkAliveEntity skAliveEntity2 = (SkAliveEntity)caster;
				SkillTreeUnitMgr cmpt2 = skAliveEntity2.Entity.GetCmpt<SkillTreeUnitMgr>();
				bGetSpItems2 = cmpt2.CheckMinerGetRare();
			}
			DigTerrainManager.DigTerrainNetwork((SkNetworkInterface)caster._net, xYZ, num * ((!flag) ? 1f : 5f), attribute, attribute2, flag, bGetSpItems2, 0.3f);
		}
	}

	private void ProcChangeTypes(SkEntity caster)
	{
		IDigTerrain digTerrain = caster as IDigTerrain;
		IntVector4 intVector = null;
		if (digTerrain != null && (intVector = digTerrain.digPosType) != IntVector4.Zero)
		{
			float radius = 2f;
			byte targetType = (byte)Mathf.RoundToInt(_attribs.sums[0]);
			IntVector3 xYZ = intVector.XYZ;
			if (caster is SkProjectile)
			{
				SkEntity skEntityCaster = ((SkProjectile)caster).GetSkEntityCaster();
				DigTerrainManager.ChangeTerrain(xYZ, radius, targetType, skEntityCaster);
			}
			else
			{
				DigTerrainManager.ChangeTerrain(xYZ, radius, targetType, caster);
			}
		}
	}

	public override void ApplyEff(int effId, SkRuntimeInfo info)
	{
		base.ApplyEff(effId, info);
		Singleton<EffectBuilder>.Instance.RegisterEffectFromSkill(effId, info, base.transform);
	}

	private void Awake()
	{
		self = this;
		if (GameConfig.IsMultiMode && VoxelTerrainNetwork.Instance != null)
		{
			ImportNet();
			VoxelTerrainNetwork.Instance.Init();
		}
		int defLayer = 12;
		VFVoxelChunkGo.DefMat = (VFTransVoxelGo._defMat = _defMat);
		VFVoxelChunkGo.DefLayer = (VFTransVoxelGo._defLayer = defLayer);
		VFVoxelChunkGo.DefParent = (VFTransVoxelGo._defParent = base.transform);
		_transGoCreator = new TransvoxelGoCreator();
	}

	public void Import(PeRecordReader r)
	{
		if (!PeGameMgr.IsMulti)
		{
			SaveLoad = new VFVoxelSave("ArchiveKeyVoxelTerrain");
			SaveLoad.Import(r);
			InitSkEntity();
		}
	}

	public void ImportNet()
	{
		if (!PeGameMgr.IsSingle)
		{
			SaveLoad = new VFVoxelSave("ArchiveKeyVoxelTerrain");
			SaveLoad.Import(null);
			InitSkEntity();
		}
	}

	private void Start()
	{
		_transGoCreator.IsTransvoxelEnabled = LODOctreeMan._maxLod > 0;
		if (_transGoCreator.IsTransvoxelEnabled)
		{
			VFGoPool<VFTransVoxelGo>.PreAlloc(20 << LODOctreeMan._maxLod);
		}
		VFGoPool<VFVoxelChunkGo>.PreAlloc(4000);
		if (_lodDataUpdate != null)
		{
			_lodDataUpdate.init();
		}
		PrepareColliderOrder();
		StartResetLOD();
	}

	private void OnDestroy()
	{
		if (_lodDataUpdate != null)
		{
			_lodDataUpdate.Stop();
			_lodDataUpdate = null;
		}
		if (_transGoCreator != null)
		{
			_transGoCreator.IsTransvoxelEnabled = false;
			_transGoCreator = null;
		}
		if (SurfExtractor != null)
		{
			SurfExtractor.Reset();
		}
		if (_dataLoader != null)
		{
			_dataLoader.Close();
		}
		if (base.gameObject.transform.childCount > 0)
		{
			for (int num = base.gameObject.transform.childCount - 1; num >= 0; num--)
			{
				GameObject obj = base.gameObject.transform.GetChild(num).gameObject;
				UnityEngine.Object.DestroyImmediate(obj);
			}
		}
		Resources.UnloadUnusedAssets();
		TerrainVoxelComplete = false;
		TerrainColliderComplete = false;
		bChunkColliderRebuilding = true;
		self = null;
		VFVoxelSave.Clean();
	}

	public void StartResetLOD(Transform t = null)
	{
		StartCoroutine(ResetLOD());
	}

	private IEnumerator ResetLOD(Transform t = null)
	{
		while (LodMan == null)
		{
			yield return 0;
		}
		Transform old = LodMan.Observer;
		LodMan.Observer = null;
		TerrainVoxelComplete = false;
		TerrainColliderComplete = false;
		bChunkColliderRebuilding = true;
		LodMan.Reset();
		while (LodMan.IsFirstRefreshed)
		{
			yield return 0;
		}
		LodMan.Observer = ((!(t != null)) ? old : t);
		yield return StartCoroutine(CheckTerrainInitStatus());
	}

	private IEnumerator CheckTerrainInitStatus()
	{
		bool bFin = false;
		while (!bFin)
		{
			if (!TerrainVoxelComplete && LodMan.IsFirstRefreshed && DataLoader.IsIdle && SurfExtractor.IsAllClear)
			{
				TerrainVoxelComplete = true;
				yield return 0;
			}
			if (!TerrainColliderComplete && TerrainVoxelComplete && !bChunkColliderRebuilding)
			{
				TerrainColliderComplete = true;
				yield return 0;
			}
			yield return 0;
		}
	}

	public ILODNodeData CreateLODNodeData(LODOctreeNode node)
	{
		VFVoxelChunkData vFVoxelChunkData = new VFVoxelChunkData(node);
		vFVoxelChunkData.HelperProc = this;
		return vFVoxelChunkData;
	}

	public void ProcPostLodInit()
	{
		if (RandomMap)
		{
			_dataLoader = new VFDataRTGen(RandomMapConfig.RandSeed);
			_voxels = new VFLODDataSource(LodMan.LodTreeNodes, IdxInLODNodeData);
		}
		else
		{
			_dataLoader = new VFDataReader((!string.IsNullOrEmpty(MapDataPath_Zip)) ? (MapDataPath_Zip + "/map") : string.Empty);
			_voxels = new VFLODDataSource(LodMan.LodTreeNodes, IdxInLODNodeData);
			_lodDataUpdate = new LODDataUpdate();
		}
	}

	public void ProcPostLodUpdate()
	{
		if (!GameConfig.IsMultiMode && Environment.TickCount - lastSaveChunkTime > 15000 && saveTerrain)
		{
			StartCoroutine(VFVoxelSave.CoSaveAllChunksInList());
			lastSaveChunkTime = Environment.TickCount;
		}
		_transGoCreator.UpdateTransMesh();
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
			DataLoader.AddRequest(vFVoxelChunkData);
		}
	}

	public bool ChunkProcExtractData(ILODNodeData ndata)
	{
		VFVoxelChunkData vFVoxelChunkData = ndata as VFVoxelChunkData;
		byte b = vFVoxelChunkData.DataVT[0];
		byte b2 = vFVoxelChunkData.DataVT[1];
		byte[] array = VFVoxelChunkData.s_ChunkDataPool.Get();
		if (b != 0)
		{
			int num = 0;
			while (num < 85750)
			{
				array[num++] = b;
				array[num++] = b2;
			}
		}
		else
		{
			Array.Clear(array, 0, 85750);
		}
		vFVoxelChunkData.SetDataVT(array, bFromPool: true);
		return true;
	}

	public VFVoxel ChunkProcExtractData(ILODNodeData ndata, int x, int y, int z)
	{
		VFVoxelChunkData vFVoxelChunkData = ndata as VFVoxelChunkData;
		return new VFVoxel(vFVoxelChunkData.DataVT[0], vFVoxelChunkData.DataVT[1]);
	}

	public void ChunkProcPostGenMesh(IVxSurfExtractReq ireq)
	{
		SurfExtractReqMC surfExtractReqMC = ireq as SurfExtractReqMC;
		VFVoxelChunkGo vfGo = VFVoxelChunkGo.CreateChunkGo(surfExtractReqMC);
		surfExtractReqMC._chunk.AttachChunkGo(vfGo, surfExtractReqMC);
	}

	public void OnBegUpdateNodeData(ILODNodeData ndata)
	{
		if (LODOctreeMan._maxLod == 0)
		{
			VFVoxelChunkData vFVoxelChunkData = ndata as VFVoxelChunkData;
			if (vFVoxelChunkData.LOD == 0 && vFVoxelChunkData.IsNodePosChange())
			{
				SceneChunkDependence.Instance.ValidListRemove(vFVoxelChunkData.ChunkPosLod, EDependChunkType.ChunkTerMask);
			}
		}
	}

	public void OnEndUpdateNodeData(ILODNodeData ndata)
	{
		VFVoxelChunkData vFVoxelChunkData = ndata as VFVoxelChunkData;
		if (vFVoxelChunkData.LOD == 0 && vFVoxelChunkData.IsEmpty)
		{
			SceneChunkDependence.Instance.ValidListAdd(vFVoxelChunkData.ChunkPosLod, EDependChunkType.ChunkTerEmp);
		}
	}

	public void OnDestroyNodeData(ILODNodeData ndata)
	{
		VFVoxelChunkData vFVoxelChunkData = ndata as VFVoxelChunkData;
		if (vFVoxelChunkData.LOD == 0)
		{
			SceneChunkDependence.Instance.ValidListRemove(vFVoxelChunkData.ChunkPosLod, EDependChunkType.ChunkTerMask);
		}
	}

	public static int CompareCPosForColliderSort(IntVector3 c1, IntVector3 c2)
	{
		int num = c1.x * c1.x + c1.z * c1.z - (c2.x * c2.x + c2.z * c2.z);
		if (num == 0)
		{
			return c2.y - c1.y;
		}
		return num;
	}

	public static void PrepareColliderOrder()
	{
		OrderedOffsetList.Clear();
		int num = (LODOctreeMan._yLodRootChunkCount >> 1) - 1;
		if (num < 2)
		{
			num = 2;
		}
		int num2 = 3;
		int num3 = 3;
		for (int i = -num2; i <= num2; i++)
		{
			for (int j = -num3; j <= num3; j++)
			{
				for (int k = -num; k <= num; k++)
				{
					OrderedOffsetList.Add(new IntVector3(i, k, j));
				}
			}
		}
		OrderedOffsetList.Sort(CompareCPosForColliderSort);
	}

	public static VFVoxelChunkGo GenOneCollider(VFVoxelChunkData chunk)
	{
		VFVoxelChunkGo chunkGo;
		if (chunk == null || (chunkGo = chunk.ChunkGo) == null)
		{
			return null;
		}
		if (null != chunkGo.Mc.sharedMesh)
		{
			return null;
		}
		bChunkColliderRebuilding = true;
		s_tmpVfGoSubs.Clear();
		chunkGo.GetComponentsInChildren(s_tmpVfGoSubs);
		int count = s_tmpVfGoSubs.Count;
		for (int i = 0; i < count; i++)
		{
			s_tmpVfGoSubs[i].Mc.sharedMesh = null;
			s_tmpVfGoSubs[i].Mc.sharedMesh = s_tmpVfGoSubs[i].Mf.sharedMesh;
		}
		return chunkGo;
	}

	private void FixedUpdate()
	{
		if (LodMan == null || !(LodMan.Observer != null) || _frameCnt == Time.frameCount)
		{
			return;
		}
		_frameCnt = Time.frameCount;
		Vector3 position = LodMan.Observer.position;
		Vector3 vector = position - LodMan.LastRefreshPos;
		int num = (int)position.x >> 5;
		int num2 = (int)position.y >> 5;
		int num3 = (int)position.z >> 5;
		VFVoxelChunkGo vFVoxelChunkGo = null;
		if (LodMan.LastRefreshPos.y > 0f && vector.sqrMagnitude > 256f)
		{
			VFVoxelChunkData vFVoxelChunkData = _voxels.readChunk(num, num2, num3);
			vFVoxelChunkGo = GenOneCollider(vFVoxelChunkData);
			if (null == vFVoxelChunkGo)
			{
				Vector3 vector2 = vector + position;
				int cx = (int)vector2.x >> 5;
				int cy = (int)vector2.y >> 5;
				int cz = (int)vector2.z >> 5;
				vFVoxelChunkData = _voxels.readChunk(cx, cy, cz);
				vFVoxelChunkGo = GenOneCollider(vFVoxelChunkData);
			}
			if (null != vFVoxelChunkGo && vFVoxelChunkData.ChunkGo == vFVoxelChunkGo)
			{
				SceneChunkDependence.Instance.ValidListAdd(vFVoxelChunkGo.Data.ChunkPosLod, EDependChunkType.ChunkTerCol);
				vFVoxelChunkGo.OnColliderReady();
			}
		}
		if (null == vFVoxelChunkGo)
		{
			int count = OrderedOffsetList.Count;
			for (int i = 0; i < count; i++)
			{
				IntVector3 intVector = OrderedOffsetList[i];
				VFVoxelChunkData vFVoxelChunkData = _voxels.readChunk(intVector.x + num, intVector.y + num2, intVector.z + num3);
				vFVoxelChunkGo = GenOneCollider(vFVoxelChunkData);
				if (null != vFVoxelChunkGo)
				{
					if (vFVoxelChunkData.ChunkGo == vFVoxelChunkGo)
					{
						SceneChunkDependence.Instance.ValidListAdd(vFVoxelChunkGo.Data.ChunkPosLod, EDependChunkType.ChunkTerCol);
						vFVoxelChunkGo.OnColliderReady();
					}
					break;
				}
			}
		}
		if (null == vFVoxelChunkGo)
		{
			bChunkColliderRebuilding = false;
		}
	}

	public bool IsPosHasCollider(IntVector4 nodePos)
	{
		if (nodePos.w != 0)
		{
			return false;
		}
		VFVoxelChunkData vFVoxelChunkData = _voxels.readChunk(nodePos.x >> 5, nodePos.y >> 5, nodePos.z >> 5, nodePos.w);
		return vFVoxelChunkData != null && vFVoxelChunkData.ChunkGo != null && vFVoxelChunkData.ChunkGo.Mc != null;
	}

	public bool IsPosInGenerating(ref Vector3 pos)
	{
		int cx = (int)pos.x >> 5;
		int cy = (int)pos.y >> 5;
		int cz = (int)pos.z >> 5;
		VFVoxelChunkData vFVoxelChunkData = _voxels.readChunk(cx, cy, cz);
		if (vFVoxelChunkData == null)
		{
			return true;
		}
		if (vFVoxelChunkData.ChunkGo != null && vFVoxelChunkData.ChunkGo.Mc.sharedMesh != null)
		{
			return false;
		}
		if (vFVoxelChunkData.BuildStep == 0 && vFVoxelChunkData.ChunkGo == null)
		{
			return false;
		}
		return true;
	}

	public bool IsInTerrain(float fx, float fy, float fz)
	{
		int num = Mathf.RoundToInt(fx);
		int num2 = Mathf.RoundToInt(fy);
		int num3 = Mathf.RoundToInt(fz);
		int cx = num >> 5;
		int cy = num2 >> 5;
		int cz = num3 >> 5;
		VFVoxelChunkData vFVoxelChunkData = _voxels.readChunk(cx, cy, cz);
		if (vFVoxelChunkData == null || vFVoxelChunkData.DataVT.Length < 2)
		{
			return false;
		}
		byte b;
		if (vFVoxelChunkData.IsHollow)
		{
			b = vFVoxelChunkData.DataVT[0];
			if (b < 128)
			{
				return false;
			}
			return true;
		}
		int num4 = VFVoxelChunkData.OneIndex(num & 0x1F, num2 & 0x1F, num3 & 0x1F);
		int num5 = num4 << 1;
		b = vFVoxelChunkData.DataVT[num5];
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
			b2 = vFVoxelChunkData.DataVT[num5 - 70];
			if (b2 >= 128)
			{
				return (float)(128 - b2) / (float)(b - b2) + (float)num2 - 1f > fy;
			}
			b2 = vFVoxelChunkData.DataVT[num5 + 70];
			if (b2 >= 128)
			{
				return (float)(128 - b) / (float)(b2 - b) + (float)num2 < fy;
			}
			return false;
		}
		b2 = vFVoxelChunkData.DataVT[num5 - 70];
		if (b2 < 128)
		{
			return (float)(128 - b2) / (float)(b - b2) + (float)num2 - 1f < fy;
		}
		b2 = vFVoxelChunkData.DataVT[num5 + 70];
		if (b2 < 128)
		{
			return (float)(128 - b) / (float)(b2 - b) + (float)num2 > fy;
		}
		return true;
	}

	public void SafeCheckVelocity(PhysicsCharacterMotor rigidbody)
	{
		Vector3 pos = rigidbody.transform.position;
		if (pos.y < 0f)
		{
			rigidbody.transform.position = pos - pos.y * Vector3.up;
			return;
		}
		if (IsPosInGenerating(ref pos))
		{
			rigidbody.FreezeGravity = true;
			return;
		}
		rigidbody.FreezeGravity = false;
		float num = 1500f;
		float maxDistance = 400f;
		int num2 = 12;
		if (!Physics.Raycast(pos + Vector3.up * num, Vector3.down, out var hitInfo, num - 0.1f, 1 << num2) || (!(pos.y >= hitInfo.point.y - 1.5f) && Physics.Raycast(pos + Vector3.up, Vector3.down, maxDistance, 1 << num2)))
		{
			return;
		}
		VFVoxel vFVoxel = self.Voxels.SafeRead((int)pos.x, (int)pos.y, (int)pos.z);
		if (vFVoxel.Type != 0 && vFVoxel.Volume > 127)
		{
			vFVoxel = self.Voxels.SafeRead((int)pos.x, (int)pos.y - 1, (int)pos.z);
			if (vFVoxel.Type != 0 && vFVoxel.Volume > 127)
			{
				Debug.Log("PlayerFallenCheckFix");
				rigidbody.transform.position = hitInfo.point;
			}
		}
	}

	private bool AttackVoxel(int vx, int vy, int vz, byte volumeDec)
	{
		VFVoxel vFVoxel = Voxels.SafeRead(vx, vy, vz);
		NaturalRes terrainResData = NaturalRes.GetTerrainResData(vFVoxel.Type);
		if (terrainResData == null)
		{
			Debug.LogWarning("Failed to get NaturalRes !! ResType = " + vFVoxel.Type + " ---> position = " + vx + " " + vy + string.Empty + vz);
			return false;
		}
		float time = Time.time;
		IntVector3 key = new IntVector3(vx, vy, vz);
		VoxelInAttack value = null;
		if (listVoxelInAttack.TryGetValue(key, out value))
		{
			int num = ((!(time < value.invalidTimePoint) || vFVoxel.Type != value.type) ? ((int)((float)(int)vFVoxel.Volume - (float)(int)volumeDec * terrainResData.m_duration)) : ((int)((float)(int)value.volume - (float)(int)volumeDec * terrainResData.m_duration)));
			if (num > 0)
			{
				value.invalidTimePoint = time + 10f;
				value.volume = (byte)num;
				value.type = vFVoxel.Type;
				return false;
			}
			listVoxelInAttack.Remove(key);
			Voxels.SafeWrite(vx, vy, vz, new VFVoxel(0));
			return true;
		}
		int num2 = (int)((float)(int)vFVoxel.Volume - (float)(int)volumeDec * terrainResData.m_duration);
		if (num2 > 0)
		{
			value = new VoxelInAttack();
			value.invalidTimePoint = time + 10f;
			value.volume = (byte)num2;
			value.type = vFVoxel.Type;
			List<IntVector3> list = new List<IntVector3>();
			for (int i = 0; i < listVoxelInAttack.Count; i++)
			{
				KeyValuePair<IntVector3, VoxelInAttack> keyValuePair = listVoxelInAttack.ElementAt(i);
				if (time < keyValuePair.Value.invalidTimePoint)
				{
					break;
				}
				list.Add(keyValuePair.Key);
			}
			for (int j = 0; j < list.Count; j++)
			{
				listVoxelInAttack.Remove(list[j]);
			}
			listVoxelInAttack.Add(key, value);
			return false;
		}
		Voxels.SafeWrite(vx, vy, vz, new VFVoxel(0));
		return true;
	}

	public void AlterVoxel(int vx, int vy, int vz, VFVoxel voxel, bool writeType, bool writeVolume)
	{
		VFVoxel vFVoxel = Voxels.SafeRead(vx, vy, vz);
		if ((float)(int)vFVoxel.Volume < 0.5f)
		{
			vFVoxel.Volume = 0;
		}
		Voxels.SafeWrite(vx, vy, vz, new VFVoxel((byte)Mathf.Clamp(vFVoxel.Volume + (writeVolume ? voxel.Volume : 0), 0, 255), (!writeType) ? vFVoxel.Type : voxel.Type));
	}

	public void AlterVoxel(Vector3 position, VFVoxel voxel, bool writeType, bool writeVolume)
	{
		AlterVoxel((int)position.x, (int)position.y, (int)position.z, voxel, writeType, writeVolume);
	}

	public void AlterVoxelInBuild(int vx, int vy, int vz, VFVoxel voxel)
	{
		Voxels.SafeWrite(vx, vy, vz, voxel);
		int count = Voxels.DirtyChunkList.Count;
		for (int i = 0; i < count; i++)
		{
			SaveLoad.AddChunkToSaveList(Voxels.DirtyChunkList[i]);
		}
	}

	public void AlterVoxelInBuild(Vector3 position, VFVoxel voxel)
	{
		AlterVoxelInBuild((int)position.x, (int)position.y, (int)position.z, voxel);
	}

	public void AlterVoxelInChunk(int vx, int vy, int vz, VFVoxel voxel, bool writeType, bool writeVolume)
	{
		if (AttackVoxel(vx, vy, vz, voxel.Volume))
		{
			int num = UnityEngine.Random.Range(1, 3);
			string path = "Prefab/Particle/FX_voxel_block_collapsing_0" + num;
			GameObject gameObject = Resources.Load(path) as GameObject;
			if (gameObject != null)
			{
				GameObject obj = UnityEngine.Object.Instantiate(gameObject, new Vector3(vx, vy, vz), Quaternion.identity) as GameObject;
				UnityEngine.Object.Destroy(obj, 2.5f);
			}
			int count = Voxels.DirtyChunkList.Count;
			for (int i = 0; i < count; i++)
			{
				SaveLoad.AddChunkToSaveList(Voxels.DirtyChunkList[i]);
			}
		}
	}

	public void AlterVoxelListInChunk(List<IntVector3> intPos, float durDec)
	{
		float num = Mathf.Clamp(durDec, 0f, 255f);
		VFVoxel voxel = new VFVoxel((byte)num, 0);
		int count = intPos.Count;
		for (int i = 0; i < count; i++)
		{
			IntVector3 intVector = intPos[i];
			AlterVoxelInChunk(intVector.x, intVector.y, intVector.z, voxel, writeType: true, writeVolume: true);
		}
	}

	public void AlterVoxelBox(int x, int y, int z, int width, int height, int depth, float volume, byte voxelType, bool writeType)
	{
		int num = x - width / 2;
		int num2 = y - height / 2;
		int num3 = z - depth / 2;
		int num4 = x + width / 2;
		int num5 = y + height / 2;
		int num6 = z + depth / 2;
		for (int i = num; i < num4; i++)
		{
			for (int j = num2; j < num5; j++)
			{
				for (int k = num3; k < num6; k++)
				{
					VFVoxel voxel = new VFVoxel(VFVoxel.ToNormByte(volume), voxelType);
					AlterVoxel(i, j, k, voxel, writeType, writeVolume: true);
				}
			}
		}
	}

	public void AlterVoxelSphere(int x, int y, int z, int radius, float coreVolume, byte voxelType, bool writeType)
	{
		int num = x - radius;
		int num2 = y - radius;
		int num3 = z - radius;
		int num4 = x + radius;
		int num5 = y + radius;
		int num6 = z + radius;
		float num7 = radius;
		for (int i = num; i < num4; i++)
		{
			for (int j = num2; j < num5; j++)
			{
				for (int k = num3; k < num6; k++)
				{
					float num8 = num7 - new Vector3(i - x, j - y, k - z).magnitude;
					if (num8 > 0f)
					{
						VFVoxel voxel = new VFVoxel(VFVoxel.ToNormByte((!(num8 >= coreVolume)) ? (num8 % coreVolume) : coreVolume), voxelType);
						AlterVoxel(i, j, k, voxel, writeType, writeVolume: true);
					}
				}
			}
		}
	}

	public VFVoxel GetRaycastHitVoxel(RaycastHit hitInfo, out IntVector3 voxelPos)
	{
		Vector3 point = hitInfo.point;
		if (0.05f > Mathf.Abs(hitInfo.normal.normalized.x))
		{
			point.x = Mathf.RoundToInt(point.x);
		}
		else
		{
			point.x = ((!(hitInfo.normal.x > 0f)) ? Mathf.Ceil(point.x) : Mathf.Floor(point.x));
		}
		if (0.05f > Mathf.Abs(hitInfo.normal.normalized.y))
		{
			point.y = Mathf.RoundToInt(point.y);
		}
		else
		{
			point.y = ((!(hitInfo.normal.y > 0f)) ? Mathf.Ceil(point.y) : Mathf.Floor(point.y));
		}
		if (0.05f > Mathf.Abs(hitInfo.normal.normalized.z))
		{
			point.z = Mathf.RoundToInt(point.z);
		}
		else
		{
			point.z = ((!(hitInfo.normal.z > 0f)) ? Mathf.Ceil(point.z) : Mathf.Floor(point.z));
		}
		voxelPos = new Vector3(Mathf.Round(point.x), Mathf.Round(point.y), Mathf.Round(point.z));
		VFVoxel result = self.Voxels.SafeRead(voxelPos.x, voxelPos.y, voxelPos.z);
		float num = 0f;
		while (result.Volume == 0)
		{
			num += 0.1f;
			if (num > 1.5f)
			{
				break;
			}
			Vector3 vector = hitInfo.point - hitInfo.normal * num;
			voxelPos = new Vector3(Mathf.Round(vector.x), Mathf.Round(vector.y), Mathf.Round(vector.z));
			result = self.Voxels.SafeRead(voxelPos.x, voxelPos.y, voxelPos.z);
		}
		return result;
	}
}

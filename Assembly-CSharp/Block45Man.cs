using System;
using System.Collections.Generic;
using System.IO;
using Pathea;
using Pathea.Effect;
using SkillSystem;
using UnityEngine;

public class Block45Man : SkEntity, ILODNodeDataMan, IVxChunkHelperProc
{
	public static Block45Man self;

	public Material[] _b45Materials;

	private Block45OctDataSource _dataSource;

	public int _dbgLogicX;

	public int _dbgLogicY;

	public int _dbgLogicZ;

	public int _dbg0;

	public int _dbg1;

	public bool _dbgRead;

	public bool _dbgWrite;

	public bool _dbgLODNode;

	public int _dbgLODNodeX;

	public int _dbgLODNodeY;

	public int _dbgLODNodeZ;

	public int _dbgLODNodeW;

	private IntVector3 _tmpBoundPos = new IntVector3();

	private List<Block45OctNode> _tmpLstBlock45Datas = new List<Block45OctNode>();

	private int _idxInLODNodeData;

	private PeRecordReader _record;

	private IntVector3 _boundSize = new IntVector3(LODOctreeMan._xLod0VoxelCount, LODOctreeMan._yLod0VoxelCount, LODOctreeMan._zLod0VoxelCount);

	private IntVector3 _boundPos = new IntVector3();

	private IntVector4 _lodCenterPos = new IntVector4();

	private List<Block45OctNode> _lstBlock45Datas = new List<Block45OctNode>();

	private bool _bBuildColliderAsync;

	private static int _frameCnt;

	private bool colliderBuilding;

	public Block45OctDataSource DataSource => _dataSource;

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

	public IVxSurfExtractor SurfExtractor => SurfExtractorsMan.B45BuildSurfExtractor;

	public int ChunkSig => 0;

	public bool isColliderBuilding => colliderBuilding;

	public event DelegateBlock45ColliderCreated OnColliderCreated;

	public event DelegateBlock45ColliderCreated OnColliderDestroy;

	private void InitSkEntity()
	{
		Init(onAlterAttribs, null, 97);
	}

	private void onAlterAttribs(int idx, float oldValue, float newValue)
	{
		switch (idx)
		{
		case 0:
			new PeTipMsg(PELocalization.GetString(DigTerrainManager.DonotChangeVoxelNotice), string.Empty, PeTipMsg.EMsgLevel.Norm);
			break;
		case 1:
		{
			SkEntity casterToModAttrib = GetCasterToModAttrib(idx);
			if (null != casterToModAttrib)
			{
				ProcDigB45s(casterToModAttrib);
			}
			break;
		}
		}
	}

	private void ProcDigB45s(SkEntity caster)
	{
		SkInst skInst = SkRuntimeInfo.Current as SkInst;
		IDigTerrain digTerrain = caster as IDigTerrain;
		IntVector4 intVector = null;
		if (digTerrain != null && (intVector = digTerrain.digPosType) != IntVector4.Zero && (skInst == null || skInst._target is Block45Man))
		{
			float attribute = caster.GetAttribute(25);
			float num = caster.GetAttribute(22);
			float height = caster.GetAttribute(22);
			PeEntity peEntity = ((!(caster != null)) ? null : caster.GetComponent<PeEntity>());
			if (peEntity != null && peEntity.monster != null)
			{
				num = peEntity.bounds.extents.x + 1f;
				height = peEntity.bounds.extents.y + 1f;
			}
			if (skInst != null && skInst._colInfo != null)
			{
				int num2 = 0;
				num2 = ((num < 3f) ? 258 : ((!(num < 5f)) ? 260 : 259));
				Singleton<EffectBuilder>.Instance.Register(num2, null, skInst._colInfo.hitPos, Quaternion.identity);
			}
			if (!GameConfig.IsMultiMode)
			{
				List<B45Block> removeList = new List<B45Block>();
				DigTerrainManager.DigBlock45(intVector.ToIntVector3(), attribute * 0.2f, num, height, ref removeList, square: false);
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
		_dataSource = new Block45OctDataSource(AddOctNewNodeToAttach);
		int defLayer = 12;
		Block45ChunkGo._defLayer = defLayer;
		Block45ChunkGo._defMats = _b45Materials;
		Block45ChunkGo._defParent = base.transform;
		VFGoPool<Block45ChunkGo>.PreAlloc(64);
	}

	private void Start()
	{
		InitSkEntity();
		_bBuildColliderAsync = true;
	}

	private void OnDestroy()
	{
		if (SurfExtractor != null)
		{
			SurfExtractor.Reset();
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
	}

	private void testDataSource()
	{
		MemoryStream memoryStream = new MemoryStream();
		BinaryWriter binaryWriter = new BinaryWriter(memoryStream);
		int num = 2;
		binaryWriter.Write(num);
		int num2 = num;
		if (num2 == 2)
		{
			binaryWriter.Write(3);
			binaryWriter.Write(1);
			binaryWriter.Write(1);
			binaryWriter.Write(1);
			binaryWriter.Write((byte)12);
			binaryWriter.Write((byte)1);
			binaryWriter.Write(-1);
			binaryWriter.Write(1);
			binaryWriter.Write(-1);
			binaryWriter.Write((byte)13);
			binaryWriter.Write((byte)1);
			binaryWriter.Write(24454);
			binaryWriter.Write(248);
			binaryWriter.Write(12188);
			binaryWriter.Write((byte)11);
			binaryWriter.Write((byte)1);
		}
		binaryWriter.Close();
		memoryStream.Close();
		byte[] array = memoryStream.ToArray();
		_dataSource.ImportData(memoryStream.ToArray());
		byte[] array2 = _dataSource.ExportData();
		string text = string.Empty;
		for (int i = 0; i < array.Length; i++)
		{
			text = text + array[i] + ", ";
		}
		MonoBehaviour.print(text);
		text = string.Empty;
		for (int j = 0; j < array.Length; j++)
		{
			text = text + array2[j] + ", ";
		}
		MonoBehaviour.print(text);
	}

	public void AddOctNewNodeToAttach(Block45OctNode octNode)
	{
		LODOctreeNode parentNodeWithPos = LodMan.GetParentNodeWithPos(octNode._pos.ToVector3(), octNode._pos.w);
		octNode.AttachLODNode((Block45LODNodeData)parentNodeWithPos._data[IdxInLODNodeData]);
	}

	public ILODNodeData CreateLODNodeData(LODOctreeNode node)
	{
		Block45LODNodeData block45LODNodeData = new Block45LODNodeData(node);
		block45LODNodeData.HelperProc = this;
		return block45LODNodeData;
	}

	public void ProcPostLodInit()
	{
		DelayedLoad();
	}

	public void ProcPostLodUpdate()
	{
		if (_dbgRead)
		{
			B45Block b45Block = _dataSource.Read(_dbgLogicX, _dbgLogicY, _dbgLogicZ);
			_dbg0 = b45Block.blockType;
			_dbg1 = b45Block.materialType;
			_dbgRead = false;
		}
		if (_dbgWrite)
		{
			_dataSource.Write(new B45Block((byte)_dbg0, (byte)_dbg1), _dbgLogicX, _dbgLogicY, _dbgLogicZ);
			_dbgWrite = false;
		}
	}

	public void ProcPostLodRefresh()
	{
	}

	public void ChunkProcPreSetDataVT(ILODNodeData cdata, byte[] data, bool bFromPool)
	{
	}

	public void ChunkProcPreLoadData(ILODNodeData nData)
	{
		if (_dataSource == null || _dataSource.RootNode == null)
		{
			return;
		}
		Block45LODNodeData block45LODNodeData = nData as Block45LODNodeData;
		int boundSize = 1 << 5 + block45LODNodeData.ChunkPosLod.w;
		_tmpBoundPos.x = block45LODNodeData.ChunkPosLod.x << 5;
		_tmpBoundPos.y = block45LODNodeData.ChunkPosLod.y << 5;
		_tmpBoundPos.z = block45LODNodeData.ChunkPosLod.z << 5;
		_tmpLstBlock45Datas.Clear();
		lock (block45LODNodeData)
		{
			Block45OctNode.FindNodesCenterInside(_tmpBoundPos, boundSize, block45LODNodeData.LOD, _dataSource.RootNode, ref _tmpLstBlock45Datas);
			block45LODNodeData.SetBlock45Datas(_tmpLstBlock45Datas);
		}
	}

	public bool ChunkProcExtractData(ILODNodeData ndata)
	{
		return true;
	}

	public VFVoxel ChunkProcExtractData(ILODNodeData ndata, int x, int y, int z)
	{
		return default(VFVoxel);
	}

	public void ChunkProcPostGenMesh(IVxSurfExtractReq ireq)
	{
		SurfExtractReqB45 surfExtractReqB = ireq as SurfExtractReqB45;
		Block45ChunkGo b45Go = Block45ChunkGo.CreateChunkGo(surfExtractReqB);
		surfExtractReqB._chunkData.AttachChunkGo(b45Go);
	}

	public void OnBegUpdateNodeData(ILODNodeData ndata)
	{
		if (LODOctreeMan._maxLod == 0)
		{
			Block45LODNodeData block45LODNodeData = ndata as Block45LODNodeData;
			if (block45LODNodeData.LOD == 0 && block45LODNodeData.IsNodePosChange())
			{
				SceneChunkDependence.Instance.ValidListRemove(block45LODNodeData.ChunkPosLod, EDependChunkType.ChunkBlkMask);
			}
		}
	}

	public void OnEndUpdateNodeData(ILODNodeData ndata)
	{
		Block45LODNodeData block45LODNodeData = ndata as Block45LODNodeData;
		if (block45LODNodeData.LOD == 0 && block45LODNodeData.IsAllOctNodeReady())
		{
			EDependChunkType type = ((!block45LODNodeData.IsEmpty) ? EDependChunkType.ChunkBlkCol : EDependChunkType.ChunkBlkEmp);
			SceneChunkDependence.Instance.ValidListAdd(block45LODNodeData.ChunkPosLod, type);
		}
	}

	public void OnDestroyNodeData(ILODNodeData ndata)
	{
		Block45LODNodeData block45LODNodeData = ndata as Block45LODNodeData;
		IntVector4 chunkPosLod = block45LODNodeData.ChunkPosLod;
		if (chunkPosLod != null && chunkPosLod.w == 0)
		{
			SceneChunkDependence.Instance.ValidListRemove(chunkPosLod, EDependChunkType.ChunkBlkMask);
		}
	}

	private static void Dig(IntVector3 blockUnitPos, float durDec, ref List<B45Block> removeList)
	{
		B45Block item = self._dataSource.SafeRead(blockUnitPos.x, blockUnitPos.y, blockUnitPos.z);
		if (item.blockType != 0)
		{
			if (durDec >= 127f)
			{
				item.blockType = 0;
				self._dataSource.SafeWrite(new B45Block(0), blockUnitPos.x, blockUnitPos.y, blockUnitPos.z);
				removeList.Add(item);
			}
		}
	}

	public static int DigBlock(IntVector3 intPos, float durDec, float radius, float height, ref List<B45Block> removeList, bool square = true)
	{
		for (int i = -Mathf.RoundToInt(radius); i <= Mathf.RoundToInt(radius); i++)
		{
			for (int j = -Mathf.RoundToInt(radius); j <= Mathf.RoundToInt(radius); j++)
			{
				for (int k = -Mathf.RoundToInt(height); k <= Mathf.RoundToInt(height); k++)
				{
					IntVector3 blockUnitPos = new IntVector3(intPos.x + i, intPos.y + k, intPos.z + j);
					float num = i * i + k * k + j * j;
					if (square || !(num > radius * radius))
					{
						Dig(blockUnitPos, durDec, ref removeList);
					}
				}
			}
		}
		return removeList.Count;
	}

	public void Export(PeRecordWriter w)
	{
		BinaryWriter binaryWriter = w.binaryWriter;
		if (binaryWriter == null)
		{
			Debug.LogError("On WriteRecord FileStream is null!");
		}
		else
		{
			_dataSource.Export(binaryWriter);
		}
	}

	public void Import(PeRecordReader r)
	{
		_record = r;
	}

	private void DelayedLoad()
	{
		if (_record != null && _record.Open())
		{
			BinaryReader binaryReader = _record.binaryReader;
			_dataSource.Import(binaryReader);
			_record.Close();
			_record = null;
		}
	}

	private void FixedUpdate()
	{
		if (LodMan == null || !(LodMan.Observer != null) || _frameCnt == Time.frameCount || _dataSource.RootNode == null)
		{
			return;
		}
		_frameCnt = Time.frameCount;
		_lstBlock45Datas.Clear();
		if (_bBuildColliderAsync)
		{
			_lodCenterPos.x = Mathf.FloorToInt(LodMan.LastRefreshPos.x);
			_lodCenterPos.y = Mathf.FloorToInt(LodMan.LastRefreshPos.y);
			_lodCenterPos.z = Mathf.FloorToInt(LodMan.LastRefreshPos.z);
			_lodCenterPos.w = 0;
			Block45OctNode nodeRO = Block45OctNode.GetNodeRO(_lodCenterPos, _dataSource.RootNode);
			if (nodeRO != null && nodeRO.NodeData != null)
			{
				nodeRO = nodeRO.NodeData.PickNodeToSetCol();
				if (nodeRO != null)
				{
					_lstBlock45Datas.Add(nodeRO);
				}
			}
		}
		if (_lstBlock45Datas.Count <= 0)
		{
			_boundPos = LodMan._Lod0ViewBounds.min;
			GetNodesToGenCol0(_boundPos, _boundSize, _dataSource.RootNode, ref _lstBlock45Datas);
		}
		int count = _lstBlock45Datas.Count;
		for (int i = 0; i < count; i++)
		{
			Block45OctNode block45OctNode = _lstBlock45Datas[i];
			if (block45OctNode != null && !(block45OctNode.ChunkGo == null))
			{
				colliderBuilding = true;
				block45OctNode.ChunkGo.OnSetCollider();
				if (block45OctNode.NodeData != null && block45OctNode.NodeData.IsAllOctNodeReady())
				{
					SceneChunkDependence.Instance.ValidListAdd(block45OctNode.NodeData.ChunkPosLod, EDependChunkType.ChunkBlkCol);
				}
				if (_bBuildColliderAsync)
				{
					return;
				}
			}
		}
		colliderBuilding = false;
	}

	private void GetNodesToGenCol0(IntVector3 boundPos, IntVector3 boundSize, Block45OctNode root, ref List<Block45OctNode> outNodesList)
	{
		if (root._pos.w == 0)
		{
			if (root.IsCenterInside(boundPos, boundSize) && root.ChunkGo != null && root.ChunkGo._mc != null && root.ChunkGo._mc.sharedMesh == null)
			{
				outNodesList.Add(root);
			}
		}
		else if (!root.IsLeaf && root.IsOverlapped(boundPos, boundSize) && root._children != null)
		{
			for (int i = 0; i < 8; i++)
			{
				GetNodesToGenCol0(boundPos, boundSize, root._children[i], ref outNodesList);
			}
		}
	}

	private void GetNodesToGenCol1(IntVector3 boundPos, IntVector3 boundSize, Block45OctNode root, ref List<Block45OctNode> outNodesList)
	{
		if (root._pos.w == 0)
		{
			if (root.IsWholeInside(boundPos, boundSize) && root.ChunkGo != null && root.ChunkGo._mc.sharedMesh == null)
			{
				outNodesList.Add(root);
			}
		}
		else
		{
			if (root.IsLeaf)
			{
				return;
			}
			if (root.IsWholeInside(boundPos, boundSize))
			{
				AddChildrenNodesToGenCol(root, ref outNodesList);
			}
			else if (root.IsOverlapped(boundPos, boundSize))
			{
				for (int i = 0; i < 8; i++)
				{
					GetNodesToGenCol1(boundPos, boundSize, root._children[i], ref outNodesList);
				}
			}
		}
	}

	private void AddChildrenNodesToGenCol(Block45OctNode root, ref List<Block45OctNode> outNodesList)
	{
		if (root._pos.w == 0)
		{
			if (root.ChunkGo != null && root.ChunkGo._mc.sharedMesh == null)
			{
				outNodesList.Add(root);
			}
		}
		else if (!root.IsLeaf)
		{
			for (int i = 0; i < 8; i++)
			{
				AddChildrenNodesToGenCol(root._children[i], ref outNodesList);
			}
		}
	}

	public void AttachEvents(DelegateBlock45ColliderCreated created = null, DelegateBlock45ColliderCreated destroy = null)
	{
		this.OnColliderCreated = (DelegateBlock45ColliderCreated)Delegate.Combine(this.OnColliderCreated, created);
		this.OnColliderDestroy = (DelegateBlock45ColliderCreated)Delegate.Combine(this.OnColliderDestroy, destroy);
	}

	public void DetachEvents(DelegateBlock45ColliderCreated created = null, DelegateBlock45ColliderCreated destroy = null)
	{
		this.OnColliderCreated = (DelegateBlock45ColliderCreated)Delegate.Remove(this.OnColliderCreated, created);
		this.OnColliderDestroy = (DelegateBlock45ColliderCreated)Delegate.Remove(this.OnColliderDestroy, destroy);
	}

	public void onPlayerPosReady(Transform trans)
	{
	}

	public void OnBlock45ColCreated(Block45ChunkGo b45Go)
	{
		if (this.OnColliderCreated != null)
		{
			this.OnColliderCreated(b45Go);
		}
	}

	public void OnBlock45ColDestroy(Block45ChunkGo b45Go)
	{
		if (this.OnColliderDestroy != null)
		{
			this.OnColliderDestroy(b45Go);
		}
	}
}

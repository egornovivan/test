using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

public class Block45Building : MonoBehaviour
{
	public Material[] blockMaterials;

	public static Block45Building self;

	private bool CreateColliderAtMeshGen;

	public Transform _observer;

	private IB45DataSource _blockDS;

	private BiLookup<int, B45ChunkData> chunkRebuildList;

	public LODDataUpdate lodDataUpdate;

	public bool TestMode;

	public int MeshMode;

	public bool DebugMode;

	public bool GlobalInstance;

	private int mVersion = 2;

	public GameObject VizCube;

	private Dictionary<IntVector3, B45Block> mInitChunkData = new Dictionary<IntVector3, B45Block>();

	private bool colliderBuilding;

	private cpuBlock45 b45proc;

	private B45LODNode root;

	private ChunkMeshMerger cmm = new ChunkMeshMerger();

	private bool bBuildColliderAsync;

	private B45ChunkGoCreator b45creator;

	private List<B45ChunkData> chunkSaveList = new List<B45ChunkData>();

	private Dictionary<IntVector3, byte[]> m_SaveBuffer;

	private VoxelFileMan vfile;

	private int inc;

	public int RebuildListCount => chunkRebuildList.Count;

	public IB45DataSource Voxels => _blockDS;

	public BiLookup<int, B45ChunkData> ChunkRebuildList => chunkRebuildList;

	public bool isColliderBuilding => colliderBuilding;

	public event DelegateB45ColliderCreated OnColliderCreated;

	public event DelegateB45ColliderCreated OnColliderDestroy;

	public void AttachEvents(DelegateB45ColliderCreated created = null, DelegateB45ColliderCreated destroy = null)
	{
		this.OnColliderCreated = (DelegateB45ColliderCreated)Delegate.Combine(this.OnColliderCreated, created);
		this.OnColliderDestroy = (DelegateB45ColliderCreated)Delegate.Combine(this.OnColliderDestroy, destroy);
	}

	public void DetachEvents(DelegateB45ColliderCreated created = null, DelegateB45ColliderCreated destroy = null)
	{
		this.OnColliderCreated = (DelegateB45ColliderCreated)Delegate.Remove(this.OnColliderCreated, created);
		this.OnColliderDestroy = (DelegateB45ColliderCreated)Delegate.Remove(this.OnColliderDestroy, destroy);
	}

	private void Awake()
	{
		if (GlobalInstance)
		{
			self = this;
		}
		bBuildColliderAsync = false;
		colliderBuilding = true;
		b45proc = new cpuBlock45();
		b45proc.init();
		chunkRebuildList = new BiLookup<int, B45ChunkData>();
		_blockDS = new B45OctreeDataSource(chunkRebuildList, this);
	}

	public void SetMeshMode(int mode)
	{
		MeshMode = mode;
	}

	public void Start()
	{
		if (TestMode)
		{
			onPlayerPosReady(_observer);
		}
	}

	private void testbv()
	{
		MemoryStream memoryStream = new MemoryStream();
		BinaryWriter binaryWriter = new BinaryWriter(memoryStream);
		binaryWriter.Write(mVersion);
		int num = mVersion;
		if (num == 2)
		{
			int value = 1;
			binaryWriter.Write(value);
			binaryWriter.Write(1);
			binaryWriter.Write(2);
			binaryWriter.Write(3);
			binaryWriter.Write((byte)11);
			binaryWriter.Write((byte)1);
		}
		binaryWriter.Close();
		memoryStream.Close();
		byte[] array = memoryStream.ToArray();
		Import(memoryStream.ToArray());
		byte[] array2 = Export();
		string text = string.Empty;
		for (int i = 0; i < array.Length; i++)
		{
			text = text + array[i] + ", ";
		}
		text = string.Empty;
		for (int j = 0; j < array.Length; j++)
		{
			text = text + array2[j] + ", ";
		}
	}

	private void testLOD()
	{
		root = new B45LODNode(new IntVector4(0, 0, 0, 5), null, 0);
		B45LODNode.splitAt(root, new IntVector3(10, 20, 30), 3);
		B45LODNode.makeCubeRec(root);
	}

	private void SetChunksMesh(List<IntVector4> chunkPosList, List<B45ChunkData> chunks, uint numChunks)
	{
		List<Mesh> outputMesh = b45proc.getOutputMesh();
		cmm.Merge(chunkPosList, chunks, numChunks, outputMesh);
	}

	public void FinishMerge()
	{
		cmm.truncateLastMesh();
		List<ChunkMeshMerger.MeshStruct> reorganizedMeshes = cmm.GetReorganizedMeshes();
		for (int i = 0; i < reorganizedMeshes.Count; i++)
		{
			GameObject gameObject = new GameObject();
			gameObject.isStatic = true;
			MeshFilter meshFilter = gameObject.AddComponent<MeshFilter>();
			MeshCollider meshCollider = gameObject.AddComponent<MeshCollider>();
			MeshRenderer meshRenderer = gameObject.AddComponent<MeshRenderer>();
			meshCollider.sharedMesh = null;
			meshRenderer.sharedMaterial = blockMaterials[0];
			Mesh mesh = new Mesh();
			mesh.vertices = reorganizedMeshes[i].vertices;
			mesh.uv = reorganizedMeshes[i].uv;
			mesh.SetTriangles(reorganizedMeshes[i].triangles, 0);
			mesh.normals = reorganizedMeshes[i].normals;
			mesh.name = "b45mergedmesh_" + i;
			meshFilter.sharedMesh = mesh;
			B45ChunkGo b45ChunkGo = gameObject.AddComponent<B45ChunkGo>();
			b45ChunkGo._mesh = mesh;
			b45ChunkGo._meshCollider = meshCollider;
			b45ChunkGo.OnColliderCreated += OnColliderCreatedFunc;
			b45ChunkGo.OnColliderDestroy += OnColliderDestroyFunc;
			if (CreateColliderAtMeshGen)
			{
				SetChunkCollider(b45ChunkGo);
			}
			gameObject.transform.parent = base.transform;
			gameObject.layer = 12;
		}
		b45proc.clearOutputMesh();
	}

	private void OnColliderCreatedFunc(B45ChunkGo b45Chunk)
	{
		if (this.OnColliderCreated != null)
		{
			this.OnColliderCreated(b45Chunk);
		}
	}

	private void OnColliderDestroyFunc(B45ChunkGo b45Chunk)
	{
		if (this.OnColliderDestroy != null)
		{
			this.OnColliderDestroy(b45Chunk);
		}
	}

	private void SetChunksMeshByMaterial(List<IntVector4> chunkPosList, List<B45ChunkData> chunks, uint numChunks)
	{
		List<List<Mesh>> outputMeshByMaterial = b45proc.getOutputMeshByMaterial();
		for (int i = 0; i < chunks.Count; i++)
		{
			if (chunks[i].IsChunkInReq || !chunkPosList[i].Equals(chunks[i].ChunkPosLod))
			{
				continue;
			}
			for (int j = 0; j < 256; j++)
			{
				Mesh mesh = outputMeshByMaterial[i][j];
				if (!(mesh == null) && mesh.vertexCount != 0)
				{
					GameObject gameObject = new GameObject();
					MeshFilter meshFilter = gameObject.AddComponent<MeshFilter>();
					MeshCollider meshCollider = gameObject.AddComponent<MeshCollider>();
					MeshRenderer meshRenderer = gameObject.AddComponent<MeshRenderer>();
					meshCollider.sharedMesh = null;
					mesh.name = "b45mesh_" + j + "_" + chunks[i].GenChunkIdentifier();
					meshFilter.sharedMesh = mesh;
					meshRenderer.sharedMaterial = GetComponent<MeshRenderer>().sharedMaterials[j];
					B45ChunkGo b45ChunkGo = gameObject.AddComponent<B45ChunkGo>();
					b45ChunkGo._mesh = mesh;
					b45ChunkGo._meshCollider = meshCollider;
					b45ChunkGo.OnColliderCreated += OnColliderCreatedFunc;
					b45ChunkGo.OnColliderDestroy += OnColliderDestroyFunc;
					if (CreateColliderAtMeshGen)
					{
						SetChunkCollider(b45ChunkGo);
					}
					gameObject.transform.parent = base.transform;
					chunks[i].AttachChunkGo(b45ChunkGo, j);
					gameObject.layer = 12;
				}
			}
		}
		b45proc.clearOutputMesh();
	}

	private void SetChunksMeshSM(List<int> chunkStampsList, List<B45ChunkData> chunks, uint numChunks)
	{
		List<Mesh> outputMesh = b45proc.getOutputMesh();
		List<int[]> usedMaterialIndicesList = b45proc.usedMaterialIndicesList;
		for (int i = 0; i < chunks.Count; i++)
		{
			int stamp = chunkStampsList[i];
			if (!chunks[i].IsStampIdentical(stamp) || i >= outputMesh.Count)
			{
				continue;
			}
			Mesh mesh = outputMesh[i];
			if (!(mesh == null))
			{
				GameObject gameObject = new GameObject();
				MeshFilter meshFilter = gameObject.AddComponent<MeshFilter>();
				MeshCollider meshCollider = gameObject.AddComponent<MeshCollider>();
				MeshRenderer meshRenderer = gameObject.AddComponent<MeshRenderer>();
				meshCollider.sharedMesh = null;
				mesh.name = "b45mesh_" + chunks[i].GenChunkIdentifier();
				meshFilter.sharedMesh = mesh;
				List<Material> list = new List<Material>();
				for (int j = 0; j < mesh.subMeshCount; j++)
				{
					list.Add(blockMaterials[usedMaterialIndicesList[i][j]]);
				}
				meshRenderer.sharedMaterials = list.ToArray();
				B45ChunkGo b45ChunkGo = gameObject.AddComponent<B45ChunkGo>();
				b45ChunkGo._mesh = mesh;
				b45ChunkGo._meshCollider = meshCollider;
				b45ChunkGo.OnColliderCreated += OnColliderCreatedFunc;
				b45ChunkGo.OnColliderDestroy += OnColliderDestroyFunc;
				if (CreateColliderAtMeshGen)
				{
					SetChunkCollider(b45ChunkGo);
				}
				chunks[i].safeToRemoveCollider();
				gameObject.transform.parent = base.transform;
				chunks[i].AttachChunkGo(b45ChunkGo);
				gameObject.layer = 12;
			}
		}
		b45proc.clearOutputMesh();
	}

	private void SetChunkCollider(B45ChunkGo goChunk)
	{
		goChunk.SetCollider();
		B45ChunkGo[] componentsInChildren = goChunk.GetComponentsInChildren<B45ChunkGo>();
		for (int i = 0; i < componentsInChildren.Length; i++)
		{
			componentsInChildren[i].SetCollider();
		}
	}

	private IEnumerator SetChunksCollider()
	{
		Dictionary<IntVector4, B45ChunkData> chunks = ((B45OctreeDataSource)_blockDS).ChunksDictionary;
		while (true)
		{
			if (_observer == null)
			{
				yield return 0;
				continue;
			}
			foreach (KeyValuePair<IntVector4, B45ChunkData> kvp in chunks)
			{
				B45ChunkData chunk = kvp.Value;
				if (chunk == null || kvp.Key.w != 0)
				{
					continue;
				}
				B45ChunkGo goChunk = chunk.ChunkGo;
				if (!(goChunk != null) || !(goChunk._meshCollider.sharedMesh == null))
				{
					continue;
				}
				colliderBuilding = true;
				SetChunkCollider(goChunk);
				chunk.safeToRemoveCollider();
				chunk.setNotInQueue();
				if (!bBuildColliderAsync)
				{
					continue;
				}
				goto IL_0187;
			}
			colliderBuilding = false;
			goto IL_0187;
			IL_0187:
			yield return 0;
		}
	}

	public void onPlayerPosReady(Transform trans)
	{
		_observer = trans;
		((B45OctreeDataSource)_blockDS).OctreeUpdate(_observer.transform.position * 2f);
		b45creator = new B45ChunkGoCreator();
		b45creator.Start(this, chunkRebuildList, SetChunksMeshSM, b45proc);
		StartCoroutine(SetChunksCollider());
		bBuildColliderAsync = true;
	}

	private void Update()
	{
		if (_observer == null)
		{
			if (null != Camera.main)
			{
				onPlayerPosReady(Camera.main.transform);
			}
		}
		else
		{
			((B45OctreeDataSource)_blockDS).OctreeUpdate((_observer.transform.position - base.transform.position) * 2f);
		}
		if (TestMode)
		{
			DebugSaveLoad();
		}
	}

	private void OnDestroy()
	{
		if (lodDataUpdate != null)
		{
			lodDataUpdate.Stop();
		}
		Debug.Log("Mem size before vfTerrain destroyed all chunks :" + GC.GetTotalMemory(forceFullCollection: true));
		if (base.gameObject.transform.childCount > 0)
		{
			for (int num = base.gameObject.transform.childCount - 1; num >= 0; num--)
			{
				GameObject obj = base.gameObject.transform.GetChild(num).gameObject;
				UnityEngine.Object.DestroyImmediate(obj);
			}
		}
	}

	public void AlterBlockInBuild(int vx, int vy, int vz, B45Block blk)
	{
		int count = chunkRebuildList.Count;
		_blockDS.SafeWrite(vx, vy, vz, blk);
		int count2 = chunkRebuildList.Count;
		for (int i = count; i < count2; i++)
		{
			AddChunkToSaveList(chunkRebuildList[i]);
		}
	}

	public void AddChunkToSaveList(B45ChunkData vc)
	{
		if (!chunkSaveList.Contains(vc))
		{
			chunkSaveList.Add(vc);
		}
	}

	public byte[][] GetChunkData()
	{
		IEnumerable<byte[]> source = chunkSaveList.Select((B45ChunkData iter) => iter.DataVT);
		return source.ToArray();
	}

	public int GetChunkDataCount()
	{
		return chunkSaveList.Count;
	}

	public void ResetSaveList()
	{
		chunkSaveList.Clear();
	}

	public byte[] Export()
	{
		BlockVectorNode bvtRoot = ((B45OctreeDataSource)_blockDS).bvtRoot;
		MemoryStream memoryStream = new MemoryStream();
		BinaryWriter binaryWriter = new BinaryWriter(memoryStream);
		binaryWriter.Write(mVersion);
		int num = mVersion;
		if (num == 2)
		{
			((B45OctreeDataSource)_blockDS).ConvertAllToBlockVectors();
			int value = BlockVectorNode.rec_count(bvtRoot);
			binaryWriter.Write(value);
			BlockVectorNode.rec_append(bvtRoot, binaryWriter);
		}
		binaryWriter.Close();
		memoryStream.Close();
		return memoryStream.ToArray();
	}

	private void DebugSaveLoad()
	{
		string path = "tmp.bin";
		if (Input.GetKeyUp(KeyCode.F9))
		{
			byte[] buffer = null;
			using (FileStream fileStream = new FileStream(path, FileMode.Open, FileAccess.Read))
			{
				BinaryReader binaryReader = new BinaryReader(fileStream);
				buffer = new byte[fileStream.Length];
				MonoBehaviour.print(string.Empty + binaryReader.Read(buffer, 0, (int)fileStream.Length) + " bytes read.");
				binaryReader.Close();
				fileStream.Close();
			}
			Import(buffer);
		}
		if (Input.GetKeyUp(KeyCode.F10))
		{
			byte[] buffer2 = Export();
			using FileStream fileStream2 = new FileStream(path, FileMode.Create, FileAccess.Write);
			BinaryWriter binaryWriter = new BinaryWriter(fileStream2);
			binaryWriter.Write(buffer2);
			binaryWriter.Close();
			fileStream2.Close();
		}
		if (!Input.GetKeyUp(KeyCode.O))
		{
		}
	}

	private bool compareSeg(byte[] arr, int ind0, int ind1)
	{
		for (int i = 0; i < 14; i++)
		{
			if (arr[ind0 + i] != arr[ind1 + i])
			{
				return false;
			}
		}
		return true;
	}

	private void randombuild()
	{
		int num = 1;
		int num2 = 1;
		int num3 = 62;
		int num4 = 16;
		if (Input.GetKeyUp(KeyCode.Alpha5))
		{
			_blockDS.SafeWrite(1, 63, 63, new B45Block(B45Block.MakeBlockType(1, 0), 4));
			_blockDS.SafeWrite(1, 64, 64, new B45Block(B45Block.MakeBlockType(1, 0), 4));
			_blockDS.SafeWrite(1, 63, 64, new B45Block(B45Block.MakeBlockType(1, 0), 4));
			_blockDS.SafeWrite(1, 64, 63, new B45Block(B45Block.MakeBlockType(1, 0), 4));
		}
		if (Input.GetKeyDown(KeyCode.Alpha1))
		{
			num += inc;
			num2 += inc;
			num3 += inc;
			for (int i = 0; i < 1; i++)
			{
				for (int j = 0; j < num4; j++)
				{
					for (int k = 0; k < num4; k++)
					{
						_blockDS.SafeWrite(k + num, j + num2, i + num3, new B45Block(B45Block.MakeBlockType(1, 0), 2));
					}
				}
			}
			inc++;
		}
		if (Input.GetKeyDown(KeyCode.Alpha2))
		{
			num += inc;
			num2 += inc;
			num3 += inc;
			for (int l = 0; l < num4; l++)
			{
				for (int m = 0; m < 1; m++)
				{
					for (int n = 0; n < num4; n++)
					{
						_blockDS.SafeWrite(n + num, m + num2, l + num3, new B45Block(B45Block.MakeBlockType(1, 0), 3));
					}
				}
			}
			inc++;
		}
		if (!Input.GetKeyDown(KeyCode.Alpha3))
		{
			return;
		}
		num += inc;
		num2 += inc;
		num3 += inc;
		for (int num5 = 0; num5 < num4; num5++)
		{
			for (int num6 = 0; num6 < num4; num6++)
			{
				for (int num7 = 0; num7 < 1; num7++)
				{
					_blockDS.SafeWrite(num7 + num, num6 + num2, num5 + num3, new B45Block(B45Block.MakeBlockType(1, 0), 4));
				}
			}
		}
		inc++;
	}

	public void Import(byte[] buffer)
	{
		BlockVectorNode bvtRoot = ((B45OctreeDataSource)_blockDS).bvtRoot;
		((B45OctreeDataSource)_blockDS).Clear();
		MemoryStream memoryStream = new MemoryStream(buffer);
		BinaryReader binaryReader = new BinaryReader(memoryStream);
		int num = binaryReader.ReadInt32();
		int num2 = num;
		if (num2 == 2)
		{
			int num3 = binaryReader.ReadInt32();
			for (int i = 0; i < num3; i++)
			{
				int num4 = binaryReader.ReadInt32();
				int num5 = binaryReader.ReadInt32();
				int num6 = binaryReader.ReadInt32();
				IntVector3 intVector = new IntVector3(num4, num5, num6);
				try
				{
					bvtRoot = ((B45OctreeDataSource)_blockDS).bvtRoot.reroot(intVector);
				}
				catch (Exception ex)
				{
					Debug.LogWarning(string.Concat("Unexpected exception while importing", intVector, ex));
					break;
				}
				((B45OctreeDataSource)_blockDS).bvtRoot = bvtRoot;
				BlockVectorNode blockVectorNode = BlockVectorNode.readNode(new IntVector3(num4, num5, num6), bvtRoot);
				if (blockVectorNode.blockVectors == null)
				{
					blockVectorNode.blockVectors = new List<BlockVector>();
				}
				num4 &= 7;
				num5 &= 7;
				num6 &= 7;
				blockVectorNode.blockVectors.Add(new BlockVector(num4 + 1, num5 + 1, num6 + 1, binaryReader.ReadByte(), binaryReader.ReadByte()));
			}
		}
		binaryReader.Close();
		memoryStream.Close();
	}
}

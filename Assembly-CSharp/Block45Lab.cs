using System.Collections.Generic;
using UnityEngine;

public class Block45Lab : MonoBehaviour
{
	private GameObject primitiveGO;

	private List<GameObject> chunkGOList = new List<GameObject>();

	public GameObject[] primitiveTypeList;

	public Material[] blockMaterials;

	private byte[] chunkData = new byte[2000];

	private byte[] chunkData2 = new byte[2000];

	private B45LODCompute lodCompute;

	private float angle = 90f;

	private float[] fourptsX = new float[4] { 0.5f, 0.25f, 0.5f, 0.75f };

	private float[] fourptsY = new float[4] { 0.25f, 0.5f, 0.75f, 0.5f };

	public void Start()
	{
		cpuBlock45.Inst.init();
		test4();
		lodCompute = new B45LODCompute();
		lodCompute.Init();
		List<byte[]> list = new List<byte[]>();
		list.Add(chunkData);
		list.Add(chunkData);
		list.Add(chunkData);
		list.Add(chunkData);
		list.Add(chunkData);
		list.Add(chunkData);
		list.Add(chunkData);
		list.Add(chunkData);
		chunkData2 = lodCompute.Compute(list);
		getResult(chunkData);
	}

	private void makeType(int pType)
	{
		GameObject gameObject = Object.Instantiate(primitiveTypeList[pType]);
		primitiveGO = gameObject;
	}

	private void CreatePrimitive(Vector3[] slopeVerts, int[] slopeVertIndices, Vector2[] uv, int material_index = 0)
	{
		GameObject gameObject = new GameObject("slope" + material_index);
		MeshRenderer meshRenderer = gameObject.AddComponent<MeshRenderer>();
		MeshFilter meshFilter = gameObject.AddComponent<MeshFilter>();
		Mesh mesh = new Mesh();
		mesh.vertices = slopeVerts;
		mesh.SetTriangles(slopeVertIndices, 0);
		mesh.uv = uv;
		mesh.RecalculateNormals();
		meshFilter.sharedMesh = mesh;
		MeshCollider meshCollider = gameObject.AddComponent<MeshCollider>();
		meshCollider.sharedMesh = mesh;
		meshRenderer.sharedMaterial = blockMaterials[material_index];
		primitiveGO = gameObject;
	}

	private void CreateB45ChunkGO(Mesh meshList, int[] usedMatIndices)
	{
		GameObject gameObject = new GameObject("b45mesh");
		MeshRenderer meshRenderer = gameObject.AddComponent<MeshRenderer>();
		MeshFilter meshFilter = gameObject.AddComponent<MeshFilter>();
		meshFilter.sharedMesh = meshList;
		MeshCollider meshCollider = gameObject.AddComponent<MeshCollider>();
		meshCollider.sharedMesh = meshList;
		List<Material> list = new List<Material>();
		for (int i = 0; i < meshList.subMeshCount; i++)
		{
			list.Add(blockMaterials[usedMatIndices[i]]);
		}
		meshRenderer.sharedMaterials = list.ToArray();
		chunkGOList.Add(gameObject);
	}

	private void getResult(byte[] inputChunkData)
	{
		cpuBlock45.Inst.AddChunkVolumeData(inputChunkData);
		cpuBlock45.Inst.computeIsosurface();
		List<Mesh> outputMesh = cpuBlock45.Inst.getOutputMesh();
		List<int[]> usedMaterialIndicesList = cpuBlock45.Inst.usedMaterialIndicesList;
		CreateB45ChunkGO(outputMesh[0], usedMaterialIndicesList[0]);
		cpuBlock45.Inst.clearOutputMesh();
	}

	private void getResult(List<byte[]> inputChunkData)
	{
		for (int i = 0; i < inputChunkData.Count; i++)
		{
			cpuBlock45.Inst.AddChunkVolumeData(inputChunkData[i]);
		}
		cpuBlock45.Inst.computeIsosurface();
		List<Mesh> outputMesh = cpuBlock45.Inst.getOutputMesh();
		List<int[]> usedMaterialIndicesList = cpuBlock45.Inst.usedMaterialIndicesList;
		for (int j = 0; j < inputChunkData.Count; j++)
		{
			CreateB45ChunkGO(outputMesh[j], usedMaterialIndicesList[j]);
		}
		cpuBlock45.Inst.clearOutputMesh();
	}

	private void writeExtendedBlock(int x, int y, int z, int primitiveType, int rotation, int extendDir, int length, byte materialType)
	{
		B45Block.MakeExtendableBlock(primitiveType, rotation, extendDir, length, materialType, out var block, out var block2);
		chunkData[2 * B45ChunkData.OneIndex(x, y, z)] = block.blockType;
		chunkData[2 * B45ChunkData.OneIndex(x, y, z) + 1] = block.materialType;
		int num = 2 * B45ChunkData.OneIndex(x + Block45Kernel._2BitToExDir[extendDir * 3], y + Block45Kernel._2BitToExDir[extendDir * 3 + 1], z + Block45Kernel._2BitToExDir[extendDir * 3 + 2]);
		chunkData[num] = block2.blockType;
		chunkData[1 + num] = block2.materialType;
		for (int i = 2; i < length; i++)
		{
			num = 2 * B45ChunkData.OneIndex(x + Block45Kernel._2BitToExDir[extendDir * 3] * i, y + Block45Kernel._2BitToExDir[extendDir * 3 + 1] * i, z + Block45Kernel._2BitToExDir[extendDir * 3 + 2] * i);
			chunkData[num] = (byte)extendDir;
			chunkData[1 + num] = (byte)(length - 2);
		}
	}

	private void testextendedblock()
	{
		writeExtendedBlock(0, 0, 0, 2, 0, 0, 3, 1);
	}

	private void test1()
	{
		chunkData[2 * B45ChunkData.OneIndex(0, 0, 0)] = B45Block.MakeBlockType(1, 0);
		chunkData[2 * B45ChunkData.OneIndex(0, 0, 0) + 1] = 0;
		chunkData[2 * B45ChunkData.OneIndex(0, 0, 1)] = B45Block.MakeBlockType(2, 0);
		chunkData[2 * B45ChunkData.OneIndex(0, 0, 1) + 1] = 1;
		chunkData[2 * B45ChunkData.OneIndex(0, 0, 2)] = B45Block.MakeBlockType(4, 0);
		chunkData[2 * B45ChunkData.OneIndex(0, 0, 2) + 1] = 2;
		chunkData[2 * B45ChunkData.OneIndex(0, 0, 3)] = B45Block.MakeBlockType(3, 0);
		chunkData[2 * B45ChunkData.OneIndex(0, 0, 3) + 1] = 3;
	}

	private void test2()
	{
		Random.seed = 1223;
		for (int i = 0; i < 8; i++)
		{
			for (int j = 0; j < 8; j++)
			{
				int rotation = Mathf.RoundToInt(Random.value * 256f) % 4;
				chunkData[2 * B45ChunkData.OneIndex(j, 0, i)] = B45Block.MakeBlockType((j + i * 8) % 6 + 1, rotation);
				rotation = Mathf.RoundToInt(Random.value * 256f) % 4;
				chunkData[2 * B45ChunkData.OneIndex(j, 1, i)] = B45Block.MakeBlockType((j + i * 8) % 6 + 1, rotation);
			}
		}
	}

	private void test3()
	{
		Random.seed = 1223;
		for (int i = 0; i < 8; i++)
		{
			for (int j = 0; j < 8; j++)
			{
				int rotation = Mathf.RoundToInt(Random.value * 256f) % 4;
				chunkData[2 * B45ChunkData.OneIndex(j, 0, i)] = B45Block.MakeBlockType((j + i * 8) % 6 + 1, rotation);
				chunkData[2 * B45ChunkData.OneIndex(j, 0, i) + 1] = (byte)(j % 3);
				rotation = Mathf.RoundToInt(Random.value * 256f) % 4;
				chunkData[2 * B45ChunkData.OneIndex(j, 1, i)] = B45Block.MakeBlockType((j + i * 8) % 6 + 1, rotation);
				chunkData[2 * B45ChunkData.OneIndex(j, 1, i) + 1] = (byte)(i % 3);
			}
		}
	}

	private void test4()
	{
		IntVector3 intVector = new IntVector3();
		int num = 0;
		chunkData[2 * B45ChunkData.OneIndex(intVector.x, intVector.y, intVector.z)] = B45Block.MakeBlockType(1, 0);
		chunkData[2 * B45ChunkData.OneIndex(intVector.x, intVector.y, intVector.z) + 1] = 0;
		chunkData[2 * B45ChunkData.OneIndex(intVector.x, intVector.y, intVector.z + 1)] = B45Block.MakeBlockType(1, 0);
		chunkData[2 * B45ChunkData.OneIndex(intVector.x, intVector.y, intVector.z + 1) + 1] = 0;
		chunkData[2 * B45ChunkData.OneIndex(intVector.x + 1, intVector.y, intVector.z + 1)] = B45Block.MakeBlockType(1, 0);
		chunkData[2 * B45ChunkData.OneIndex(intVector.x + 1, intVector.y, intVector.z + 1) + 1] = 0;
		chunkData[2 * B45ChunkData.OneIndex(intVector.x + 1, intVector.y, intVector.z)] = B45Block.MakeBlockType(1, 0);
		chunkData[2 * B45ChunkData.OneIndex(intVector.x + 1, intVector.y, intVector.z) + 1] = 0;
		chunkData[2 * B45ChunkData.OneIndex(intVector.x, intVector.y + 1, intVector.z)] = B45Block.MakeBlockType(1, 0);
		chunkData[2 * B45ChunkData.OneIndex(intVector.x, intVector.y + 1, intVector.z) + 1] = 0;
		chunkData[2 * B45ChunkData.OneIndex(intVector.x, intVector.y + 1, intVector.z + 1)] = B45Block.MakeBlockType(1, 0);
		chunkData[2 * B45ChunkData.OneIndex(intVector.x, intVector.y + 1, intVector.z + 1) + 1] = 0;
		chunkData[2 * B45ChunkData.OneIndex(intVector.x + 1, intVector.y + 1, intVector.z + 1)] = B45Block.MakeBlockType(1, 0);
		chunkData[2 * B45ChunkData.OneIndex(intVector.x + 1, intVector.y + 1, intVector.z + 1) + 1] = 0;
		chunkData[2 * B45ChunkData.OneIndex(intVector.x + 1, intVector.y + 1, intVector.z)] = B45Block.MakeBlockType(1, 0);
		chunkData[2 * B45ChunkData.OneIndex(intVector.x + 1, intVector.y + 1, intVector.z) + 1] = 0;
		intVector = new IntVector3(2);
		num = 3;
		chunkData[2 * B45ChunkData.OneIndex(intVector.x, intVector.y, intVector.z)] = B45Block.MakeBlockType(1, num);
		chunkData[2 * B45ChunkData.OneIndex(intVector.x, intVector.y, intVector.z) + 1] = 1;
		chunkData[2 * B45ChunkData.OneIndex(intVector.x, intVector.y, intVector.z + 1)] = B45Block.MakeBlockType(2, num);
		chunkData[2 * B45ChunkData.OneIndex(intVector.x, intVector.y, intVector.z + 1) + 1] = 1;
		chunkData[2 * B45ChunkData.OneIndex(intVector.x + 1, intVector.y, intVector.z + 1)] = B45Block.MakeBlockType(2, num);
		chunkData[2 * B45ChunkData.OneIndex(intVector.x + 1, intVector.y, intVector.z + 1) + 1] = 1;
		chunkData[2 * B45ChunkData.OneIndex(intVector.x + 1, intVector.y, intVector.z)] = B45Block.MakeBlockType(1, num);
		chunkData[2 * B45ChunkData.OneIndex(intVector.x + 1, intVector.y, intVector.z) + 1] = 1;
		chunkData[2 * B45ChunkData.OneIndex(intVector.x, intVector.y + 1, intVector.z)] = B45Block.MakeBlockType(2, num);
		chunkData[2 * B45ChunkData.OneIndex(intVector.x, intVector.y + 1, intVector.z) + 1] = 1;
		chunkData[2 * B45ChunkData.OneIndex(intVector.x, intVector.y + 1, intVector.z + 1)] = B45Block.MakeBlockType(0, num);
		chunkData[2 * B45ChunkData.OneIndex(intVector.x, intVector.y + 1, intVector.z + 1) + 1] = 1;
		chunkData[2 * B45ChunkData.OneIndex(intVector.x + 1, intVector.y + 1, intVector.z + 1)] = B45Block.MakeBlockType(0, num);
		chunkData[2 * B45ChunkData.OneIndex(intVector.x + 1, intVector.y + 1, intVector.z + 1) + 1] = 1;
		chunkData[2 * B45ChunkData.OneIndex(intVector.x + 1, intVector.y + 1, intVector.z)] = B45Block.MakeBlockType(2, num);
		chunkData[2 * B45ChunkData.OneIndex(intVector.x + 1, intVector.y + 1, intVector.z) + 1] = 1;
		intVector = new IntVector3(4);
		num = 0;
		chunkData[2 * B45ChunkData.OneIndex(intVector.x, intVector.y, intVector.z)] = B45Block.MakeBlockType(1, num);
		chunkData[2 * B45ChunkData.OneIndex(intVector.x, intVector.y, intVector.z) + 1] = 1;
		chunkData[2 * B45ChunkData.OneIndex(intVector.x, intVector.y, intVector.z + 1)] = B45Block.MakeBlockType(1, num);
		chunkData[2 * B45ChunkData.OneIndex(intVector.x, intVector.y, intVector.z + 1) + 1] = 1;
		chunkData[2 * B45ChunkData.OneIndex(intVector.x + 1, intVector.y, intVector.z + 1)] = B45Block.MakeBlockType(2, num);
		chunkData[2 * B45ChunkData.OneIndex(intVector.x + 1, intVector.y, intVector.z + 1) + 1] = 1;
		chunkData[2 * B45ChunkData.OneIndex(intVector.x + 1, intVector.y, intVector.z)] = B45Block.MakeBlockType(2, num);
		chunkData[2 * B45ChunkData.OneIndex(intVector.x + 1, intVector.y, intVector.z) + 1] = 1;
		chunkData[2 * B45ChunkData.OneIndex(intVector.x, intVector.y + 1, intVector.z)] = B45Block.MakeBlockType(2, num);
		chunkData[2 * B45ChunkData.OneIndex(intVector.x, intVector.y + 1, intVector.z) + 1] = 1;
		chunkData[2 * B45ChunkData.OneIndex(intVector.x, intVector.y + 1, intVector.z + 1)] = B45Block.MakeBlockType(2, num);
		chunkData[2 * B45ChunkData.OneIndex(intVector.x, intVector.y + 1, intVector.z + 1) + 1] = 1;
		chunkData[2 * B45ChunkData.OneIndex(intVector.x + 1, intVector.y + 1, intVector.z + 1)] = B45Block.MakeBlockType(0, num);
		chunkData[2 * B45ChunkData.OneIndex(intVector.x + 1, intVector.y + 1, intVector.z + 1) + 1] = 1;
		chunkData[2 * B45ChunkData.OneIndex(intVector.x + 1, intVector.y + 1, intVector.z)] = B45Block.MakeBlockType(0, num);
		chunkData[2 * B45ChunkData.OneIndex(intVector.x + 1, intVector.y + 1, intVector.z) + 1] = 1;
	}

	private void Update()
	{
		Debug.DrawLine(new Vector3(0f, 0f, 0f), new Vector3(10f, 0f, 0f), Color.red);
		Debug.DrawLine(new Vector3(0f, 0f, 0f), new Vector3(0f, 10f, 0f), Color.green);
		Debug.DrawLine(new Vector3(0f, 0f, 0f), new Vector3(0f, 0f, 10f), Color.blue);
		if (Input.GetKeyUp(KeyCode.T))
		{
			genList();
		}
		if (Input.GetKeyUp(KeyCode.R))
		{
			Vector3 point = new Vector3(0.5f, 0f, 0.5f);
			primitiveGO.transform.RotateAround(point, Vector3.up, angle);
		}
		for (int i = 1; i <= 6; i++)
		{
			if (Input.GetKeyUp((KeyCode)(49 + i - 1)))
			{
				Object.DestroyImmediate(primitiveGO);
				makeType(i);
			}
		}
	}

	private void genList()
	{
		string text = string.Empty;
		int num = 0;
		for (int i = 0; i < 4; i++)
		{
			Debug.DrawRay(new Vector3(-0.1f, fourptsY[i], 1f - fourptsX[i]), new Vector3(1f, 0f, 0f), Color.white);
			if (Physics.Raycast(new Vector3(-0.1f, fourptsY[i], 1f - fourptsX[i]), new Vector3(1f, 0f, 0f), out var _, 0.11f))
			{
				text += "1, ";
				num += 1 << ((3 - i) & 0x1F);
			}
			else
			{
				text += "0, ";
			}
		}
		MonoBehaviour.print(text + " 0x" + num.ToString("X"));
	}

	private void genListTopBottom()
	{
		string text = string.Empty;
		for (int i = 0; i < 4; i++)
		{
			Debug.DrawRay(new Vector3(fourptsX[i], fourptsY[i], -0.1f), new Vector3(0f, 0f, 0.11f), Color.white);
			text = ((!Physics.Raycast(new Vector3(fourptsX[i], fourptsY[i], -0.1f), new Vector3(0f, 0f, 1f), out var _, 0.11f)) ? (text + "0, ") : (text + "1, "));
		}
		MonoBehaviour.print(text);
	}

	private void OnDrawGizmos()
	{
		for (int i = 0; i < 8; i++)
		{
			Gizmos.DrawIcon(Block45Kernel.indexedCoords[i], string.Empty + i + ".png", allowScaling: false);
		}
	}
}

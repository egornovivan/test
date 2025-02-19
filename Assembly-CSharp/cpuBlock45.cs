using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class cpuBlock45
{
	public const uint MAX_CHUNKS = 32u;

	private static cpuBlock45 inst;

	private List<byte[]> chunkDataList;

	private List<Mesh> outputMesh;

	public List<int[]> usedMaterialIndicesList;

	private List<List<Mesh>> outputMeshByMaterial;

	private Block45Kernel swBlock45;

	public static cpuBlock45 Inst
	{
		get
		{
			if (inst == null)
			{
				inst = new cpuBlock45();
				inst.init();
			}
			return inst;
		}
	}

	public uint numChunks()
	{
		return (uint)chunkDataList.Count;
	}

	public void AddChunkVolumeData(byte[] volumeData)
	{
		chunkDataList.Add(volumeData);
	}

	public void Cleanup()
	{
		swBlock45 = null;
	}

	public void computeIsosurface()
	{
		IEnumerator enumerator = computeIsosurfaceAsyn();
		while (enumerator.MoveNext())
		{
		}
	}

	public void clearOutputMesh()
	{
		for (int i = 0; i < outputMesh.Count; i++)
		{
			outputMesh[i] = null;
		}
		outputMesh.Clear();
		for (int j = 0; j < outputMeshByMaterial.Count; j++)
		{
			outputMeshByMaterial[j] = null;
		}
		outputMeshByMaterial.Clear();
		usedMaterialIndicesList.Clear();
	}

	public List<Mesh> getOutputMesh()
	{
		return outputMesh;
	}

	public List<List<Mesh>> getOutputMeshByMaterial()
	{
		return outputMeshByMaterial;
	}

	public IEnumerator computeIsosurfaceAsyn()
	{
		int chunksRebuilt = 0;
		if (chunkDataList == null)
		{
			yield break;
		}
		for (int i = 0; i < chunkDataList.Count; i++)
		{
			if (chunkDataList[i] != null && chunkDataList[i].Length > 0)
			{
				swBlock45.setInputChunkData(chunkDataList[i]);
				Mesh ret = swBlock45.RebuildMeshSM();
				usedMaterialIndicesList.Add(swBlock45.getMaterialMap());
				outputMesh.Add(ret);
				chunksRebuilt++;
				if (chunksRebuilt % 30 == 0)
				{
					chunksRebuilt = 0;
					yield return 0;
				}
			}
		}
		chunkDataList.Clear();
	}

	public void init()
	{
		swBlock45 = new Block45Kernel();
		outputMesh = new List<Mesh>();
		outputMeshByMaterial = new List<List<Mesh>>();
		chunkDataList = new List<byte[]>();
		usedMaterialIndicesList = new List<int[]>();
	}
}

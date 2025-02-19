using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class cpuMarchingCubes
{
	public const uint MAX_CHUNKS = 32u;

	private uint _numChunks;

	private List<byte[]> chunkDataList;

	private List<Mesh> outputMesh;

	private MarchingCubesSW swMarchingCubes;

	public uint numChunks()
	{
		return _numChunks;
	}

	public void AddChunkVolumeData(byte[] volumeData)
	{
		_numChunks++;
		chunkDataList.Add(volumeData);
	}

	public void Cleanup()
	{
		swMarchingCubes = null;
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
	}

	public List<Mesh> getOutputMesh()
	{
		return outputMesh;
	}

	public IEnumerator computeIsosurfaceAsyn()
	{
		if (chunkDataList == null)
		{
			yield break;
		}
		for (int i = 0; i < chunkDataList.Count; i++)
		{
			if (chunkDataList[i] != null)
			{
				swMarchingCubes.setInputChunkData(chunkDataList[i]);
				Mesh ret = swMarchingCubes.RebuildMesh();
				outputMesh.Add(ret);
				yield return 0;
			}
		}
		chunkDataList.Clear();
		_numChunks = 0u;
	}

	public void init()
	{
		swMarchingCubes = new MarchingCubesSW();
		outputMesh = new List<Mesh>();
		chunkDataList = new List<byte[]>();
	}
}

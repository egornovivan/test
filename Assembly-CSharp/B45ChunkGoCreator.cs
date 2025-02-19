using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class B45ChunkGoCreator
{
	public delegate void OnComputeComplete(List<int> chunkStampsList, List<B45ChunkData> chunks, uint numChunks);

	public bool _bAsyncBuildMode;

	private List<int> _stampsInBuildList;

	private List<B45ChunkData> _chunksInBuildList;

	private BiLookup<int, B45ChunkData> _chunkToBuildList;

	private MonoBehaviour _mono;

	private Stack<OnComputeComplete> _setMeshFuncStack;

	private Stack<BiLookup<int, B45ChunkData>> _buildListStack;

	private OnComputeComplete _setChunkMesh;

	private cpuBlock45 b45proc;

	public bool IsIdle => oclMarchingCube.numChunks == 0 && _chunkToBuildList.Count == 0;

	public void Start(MonoBehaviour parentMono, BiLookup<int, B45ChunkData> chunkToComputeList, OnComputeComplete setChunkMeshFunc, cpuBlock45 _b45proc, bool bAsyncMode = false)
	{
		b45proc = _b45proc;
		_mono = parentMono;
		_setChunkMesh = setChunkMeshFunc;
		_chunkToBuildList = chunkToComputeList;
		_bAsyncBuildMode = bAsyncMode;
		_mono.StartCoroutine(rebuildChunksInList());
		_setMeshFuncStack = new Stack<OnComputeComplete>();
		_buildListStack = new Stack<BiLookup<int, B45ChunkData>>();
		b45proc.init();
	}

	private void addChunksToOcl()
	{
		_stampsInBuildList.Clear();
		_chunksInBuildList.Clear();
		for (int i = 0; i <= 4; i++)
		{
			int count = _chunkToBuildList.Count;
			for (int num = count - 1; num >= 0; num--)
			{
				int keyByIdx_Unsafe = _chunkToBuildList.GetKeyByIdx_Unsafe(num);
				B45ChunkData valueByKey_Unsafe = _chunkToBuildList.GetValueByKey_Unsafe(keyByIdx_Unsafe);
				if (!valueByKey_Unsafe.IsStampIdentical(keyByIdx_Unsafe))
				{
					_chunkToBuildList.RemoveAt(num);
					Debug.Log(string.Concat("RemoveChunkInSet", valueByKey_Unsafe.ChunkPosLod, ":", keyByIdx_Unsafe, "|", valueByKey_Unsafe.StampOfChnkUpdating));
				}
				else if (valueByKey_Unsafe.LOD == i)
				{
					_chunkToBuildList.RemoveAt(num);
					_stampsInBuildList.Add(keyByIdx_Unsafe);
					_chunksInBuildList.Add(valueByKey_Unsafe);
					b45proc.AddChunkVolumeData(valueByKey_Unsafe.DataVT);
					if (b45proc.numChunks() >= 31)
					{
						return;
					}
				}
			}
		}
	}

	private IEnumerator rebuildChunksInList()
	{
		_stampsInBuildList = new List<int>();
		_chunksInBuildList = new List<B45ChunkData>();
		while (true)
		{
			if (_chunkToBuildList.Count > 0 && b45proc.numChunks() == 0)
			{
				addChunksToOcl();
				if (b45proc.numChunks() != 0)
				{
					uint numChunks = b45proc.numChunks();
					if (!_bAsyncBuildMode)
					{
						b45proc.computeIsosurface();
						_setChunkMesh(_stampsInBuildList, _chunksInBuildList, numChunks);
						_chunksInBuildList.Clear();
						continue;
					}
					yield return _mono.StartCoroutine(b45proc.computeIsosurfaceAsyn());
					_setChunkMesh(_stampsInBuildList, _chunksInBuildList, numChunks);
					_chunksInBuildList.Clear();
				}
			}
			yield return 0;
		}
	}

	public bool IsChunkInBuild(B45ChunkData chunk)
	{
		return _chunkToBuildList.ContainsValue(chunk) || _chunksInBuildList.Contains(chunk);
	}
}

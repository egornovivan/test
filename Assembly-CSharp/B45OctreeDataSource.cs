using System;
using System.Collections.Generic;
using UnityEngine;

public class B45OctreeDataSource : IB45DataSource
{
	public readonly int[,] nearChunkOfs = new int[8, 3]
	{
		{ 0, 0, 0 },
		{ 1, 0, 0 },
		{ 0, 1, 0 },
		{ 1, 1, 0 },
		{ 0, 0, 1 },
		{ 1, 0, 1 },
		{ 0, 1, 1 },
		{ 1, 1, 1 }
	};

	private Block45Building b45Building;

	private Dictionary<IntVector4, B45ChunkData> _chunks;

	private BiLookup<int, B45ChunkData> _chunkRebuildList;

	private ChunkColliderMan colliderMan = new ChunkColliderMan();

	public BlockVectorNode bvtRoot;

	private List<B45ChunkData> deferredRemoveList = new List<B45ChunkData>();

	public Dictionary<IntVector4, B45ChunkData> ChunksDictionary => _chunks;

	public byte this[IntVector3 idx, int lod] => Read(idx.x, idx.y, idx.z, lod).blockType;

	public B45OctreeDataSource(BiLookup<int, B45ChunkData> chunkRebuildList, Block45Building _b45Building)
	{
		_chunkRebuildList = chunkRebuildList;
		_chunks = new Dictionary<IntVector4, B45ChunkData>();
		bvtRoot = new BlockVectorNode(new IntVector4(0, 0, 0, 1), null, 0);
		b45Building = _b45Building;
	}

	public B45ChunkData readChunk(int x, int y, int z, int lod = 0)
	{
		IntVector4 intVector = new IntVector4(x, y, z, lod);
		if (!_chunks.ContainsKey(intVector))
		{
			_chunks[intVector] = CreateChunk(intVector);
		}
		return _chunks[intVector];
	}

	public void writeChunk(int x, int y, int z, B45ChunkData vc, int lod = 0)
	{
		try
		{
			IntVector4 key = new IntVector4(x, y, z, lod);
			_chunks[key] = vc;
		}
		catch (Exception)
		{
			Debug.LogError(string.Format("writeB45Chunk Exception. Max Length:({0}, {1}, {2}}), CurPos({3}, {4}, {5})"));
		}
	}

	public B45Block Read(int x, int y, int z, int lod = 0)
	{
		int num = 3;
		int x_ = x >> num;
		int y_ = y >> num;
		int z_ = z >> num;
		int x2 = x & 7;
		int y2 = y & 7;
		int z2 = z & 7;
		IntVector4 key = new IntVector4(x_, y_, z_, lod);
		if (!_chunks.ContainsKey(key))
		{
			return new B45Block(0, 0);
		}
		return _chunks[key].ReadVoxelAtIdx(x2, y2, z2);
	}

	private bool isOutOfMeshDistance(IntVector3 index)
	{
		IntVector3 zero = IntVector3.Zero;
		zero.x = index.x << 3;
		zero.y = index.y << 3;
		zero.z = index.z << 3;
		if (b45Building._observer == null)
		{
			return true;
		}
		return !BlockVectorNode.isCloseTo_static(zero, b45Building._observer.position * 2f);
	}

	public int Write(int x, int y, int z, B45Block voxel, int lod = 0)
	{
		int num = 3;
		int num2 = x >> num;
		int num3 = y >> num;
		int num4 = z >> num;
		int x_ = num2;
		int y_ = num3;
		int z_ = num4;
		int num5 = x & 7;
		int num6 = y & 7;
		int num7 = z & 7;
		IntVector4 intVector = new IntVector4(x_, y_, z_, lod);
		if (isOutOfMeshDistance(intVector.XYZ))
		{
			writeToBlockVectors(intVector.XYZ, new IntVector3(num5, num6, num7), voxel.blockType, voxel.materialType);
		}
		else
		{
			if (!_chunks.ContainsKey(intVector))
			{
				_chunks[intVector] = CreateChunk(intVector);
			}
			if (!_chunks[intVector].WriteVoxelAtIdx(num5, num6, num7, voxel))
			{
				return 0;
			}
		}
		int num8 = 1;
		int num9 = 7;
		int num10 = 0;
		int num11 = 0;
		int num12 = 0;
		int num13 = 128;
		if (num5 < num8)
		{
			num10 = -1;
			num13 |= 0x11;
		}
		else if (num5 >= num9)
		{
			num10 = 1;
			num13 |= 1;
		}
		if (num6 < num8)
		{
			num11 = -1;
			num13 |= 0x22;
		}
		else if (num6 >= num9)
		{
			num11 = 1;
			num13 |= 2;
		}
		if (num7 < num8)
		{
			num12 = -1;
			num13 |= 0x44;
		}
		else if (num7 >= num9)
		{
			num12 = 1;
			num13 |= 4;
		}
		if (num13 != 128)
		{
			for (int i = 1; i < 8; i++)
			{
				if ((num13 & i) != i)
				{
					continue;
				}
				int num14 = num10 * nearChunkOfs[i, 0];
				int num15 = num11 * nearChunkOfs[i, 1];
				int num16 = num12 * nearChunkOfs[i, 2];
				x_ = num2 + num14;
				y_ = num3 + num15;
				z_ = num4 + num16;
				intVector = new IntVector4(x_, y_, z_, lod);
				if (isOutOfMeshDistance(intVector.XYZ))
				{
					writeToBlockVectors(intVector.XYZ, new IntVector3(num5 - num14 * 8, num6 - num15 * 8, num7 - num16 * 8), voxel.blockType, voxel.materialType);
					continue;
				}
				if (!_chunks.ContainsKey(intVector))
				{
					_chunks[intVector] = CreateChunk(intVector);
				}
				try
				{
					if (!_chunks[intVector].WriteVoxelAtIdx(num5 - num14 * 8, num6 - num15 * 8, num7 - num16 * 8, voxel))
					{
						num13 |= i << 8;
					}
				}
				catch (Exception)
				{
					Debug.LogError("Unexpected block45 write at(" + x_ + "," + y_ + "," + z_ + ")");
				}
			}
		}
		return num13;
	}

	private void writeToBlockVectors(IntVector3 index, IntVector3 localIndex, byte b0, byte b1)
	{
		IntVector3 zero = IntVector3.Zero;
		zero.x = index.x << 3;
		zero.y = index.y << 3;
		zero.z = index.z << 3;
		try
		{
			BlockVectorNode blockVectorNode = bvtRoot.reroot(zero);
			bvtRoot = blockVectorNode;
		}
		catch (Exception ex)
		{
			Debug.LogWarning(string.Concat("Unexpected exception while writing block to", index, ex));
			return;
		}
		BlockVectorNode blockVectorNode2 = BlockVectorNode.readNode(zero, bvtRoot);
		blockVectorNode2.write(localIndex.x + 1, localIndex.y + 1, localIndex.z + 1, b0, b1);
	}

	public B45Block SafeRead(int x, int y, int z, int lod = 0)
	{
		return Read(x, y, z, lod);
	}

	public bool SafeWrite(int x, int y, int z, B45Block voxel, int lod = 0)
	{
		Write(x, y, z, voxel, lod);
		return true;
	}

	public void Clear()
	{
		BlockVectorNode.Clear(bvtRoot);
		B45LODNode.merge(bvtRoot);
		_chunks.Clear();
	}

	private int countpoints()
	{
		int num = 0;
		foreach (KeyValuePair<IntVector4, B45ChunkData> chunk in _chunks)
		{
			num += chunk.Value.getFillRate();
		}
		return num;
	}

	public void OctreeUpdate(IntVector3 cursorPos)
	{
		List<BlockVectorNode> list = new List<BlockVectorNode>();
		BlockVectorNode.rec_find(cursorPos, bvtRoot, list);
		int num = 0;
		int num2 = 0;
		int num3 = 0;
		if (b45Building.DebugMode)
		{
			num = BlockVectorNode.rec_count(bvtRoot);
			num2 = BlockVectorNode.rec_count_dbg(bvtRoot);
			num3 = countpoints();
		}
		for (int i = 0; i < list.Count; i++)
		{
			BlockVectorNode blockVectorNode = list[i];
			IntVector4 zero = IntVector4.Zero;
			zero.x = blockVectorNode.Pos.x >> 3;
			zero.y = blockVectorNode.Pos.y >> 3;
			zero.z = blockVectorNode.Pos.z >> 3;
			if (!_chunks.ContainsKey(zero))
			{
				B45ChunkData b45ChunkData = (blockVectorNode.chunk = CreateChunk(zero));
				b45ChunkData._bvNode = blockVectorNode;
				_chunks[zero] = b45ChunkData;
				b45ChunkData.bp();
			}
			if (blockVectorNode.blockVectors != null && blockVectorNode.chunk != null)
			{
				blockVectorNode.chunk.bp();
				try
				{
					BlockVectorNode.BlockVectors2ChunkData(blockVectorNode.blockVectors, blockVectorNode.chunk.DataVT);
				}
				catch
				{
				}
				blockVectorNode.blockVectors.Clear();
				blockVectorNode.blockVectors = null;
			}
		}
		if (b45Building.DebugMode)
		{
			int num4 = BlockVectorNode.rec_count(bvtRoot);
			int num5 = BlockVectorNode.rec_count_dbg(bvtRoot);
			int num6 = countpoints();
			Debug.LogError("B45 Octree State: " + num + " / " + num2 + " / " + num3 + " ------------- " + num4 + " / " + num5 + " / " + num6);
		}
		List<B45ChunkData> list2 = new List<B45ChunkData>();
		foreach (KeyValuePair<IntVector4, B45ChunkData> chunk in _chunks)
		{
			BlockVectorNode bvNode = chunk.Value._bvNode;
			B45ChunkData value = chunk.Value;
			if (bvNode == null)
			{
				Debug.LogError("node null!");
			}
			if (!bvNode.isCloseTo(cursorPos) && value != null)
			{
				value.bp();
				list2.Add(value);
			}
		}
		for (int j = 0; j < list2.Count; j++)
		{
			B45ChunkData b45ChunkData2 = list2[j];
			if (b45ChunkData2 != null && !b45ChunkData2.isBeingDestroyed)
			{
				b45ChunkData2.setBeingDestroyed();
				b45ChunkData2.bp();
				b45ChunkData2._bvNode.blockVectors = BlockVectorNode.ChunkData2BlockVectors(b45ChunkData2.DataVT);
				b45ChunkData2._bvNode.isByteArrayMode = false;
				b45ChunkData2._bvNode.removeCube();
				b45ChunkData2._bvNode.chunk = null;
				deferredRemoveList.Add(b45ChunkData2);
			}
		}
		deferredRemoval();
	}

	private void deferredRemoval()
	{
		for (int num = deferredRemoveList.Count - 1; num >= 0; num--)
		{
			B45ChunkData b45ChunkData = deferredRemoveList[num];
			b45ChunkData.bp();
			if (b45ChunkData != null && !b45ChunkData.isInQueue && !colliderMan.isColliderBeingRebuilt(b45ChunkData.ChunkPos.XYZ))
			{
				b45ChunkData.bp();
				if (!_chunks.Remove(new IntVector4(b45ChunkData.ChunkPos.XYZ, 0)))
				{
					Debug.LogError("a chunk can not be removed from the dictionary");
				}
				b45ChunkData.ClearMem();
				b45ChunkData.DestroyChunkGO();
				deferredRemoveList.RemoveAt(num);
			}
		}
	}

	public void ConvertAllToBlockVectors()
	{
		List<BlockVectorNode> list = new List<BlockVectorNode>();
		foreach (KeyValuePair<IntVector4, B45ChunkData> chunk2 in _chunks)
		{
			BlockVectorNode bvNode = chunk2.Value._bvNode;
			if (bvNode == null)
			{
				Debug.LogError("node null!");
			}
			list.Add(bvNode);
		}
		for (int i = 0; i < list.Count; i++)
		{
			BlockVectorNode blockVectorNode = list[i];
			B45ChunkData chunk = blockVectorNode.chunk;
			if (chunk != null)
			{
				chunk.bp();
				blockVectorNode.blockVectors = BlockVectorNode.ChunkData2BlockVectors(chunk.DataVT);
			}
		}
	}

	private B45ChunkData CreateChunk(IntVector4 index)
	{
		B45ChunkData b45ChunkData = new B45ChunkData(colliderMan);
		b45ChunkData.BuildList = _chunkRebuildList;
		writeChunk(index.x, index.y, index.z, b45ChunkData);
		b45ChunkData.ChunkPosLod_w = new IntVector4(index.x, index.y, index.z, 0);
		b45ChunkData.bp();
		b45ChunkData.AddToBuildList();
		IntVector3 zero = IntVector3.Zero;
		zero.x = index.x << 3;
		zero.y = index.y << 3;
		zero.z = index.z << 3;
		try
		{
			BlockVectorNode blockVectorNode = bvtRoot.reroot(zero);
			bvtRoot = blockVectorNode;
		}
		catch (Exception ex)
		{
			Debug.LogWarning(string.Concat("Unexpected exception while creating chunk to", index, ex));
			return b45ChunkData;
		}
		BlockVectorNode blockVectorNode2 = BlockVectorNode.readNode(zero, bvtRoot);
		if (blockVectorNode2.chunk != null)
		{
			return b45ChunkData;
		}
		blockVectorNode2.chunk = b45ChunkData;
		b45ChunkData._bvNode = blockVectorNode2;
		blockVectorNode2.isByteArrayMode = true;
		return b45ChunkData;
	}
}

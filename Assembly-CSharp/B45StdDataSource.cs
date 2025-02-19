using System;
using System.Collections.Generic;
using UnityEngine;

public class B45StdDataSource : IB45DataSource
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

	private int _chunkNumX;

	private int _chunkNumY;

	private int _chunkNumZ;

	private Dictionary<IntVector3, B45ChunkData> _chunks;

	private BiLookup<int, B45ChunkData> _chunkRebuildList;

	private Dictionary<IntVector3, B45Block> _SaveData;

	public byte this[IntVector3 idx, int lod] => Read(idx.x, idx.y, idx.z, lod).blockType;

	public B45StdDataSource(int width, int height, int depth, BiLookup<int, B45ChunkData> chunkRebuildList)
	{
		_chunkNumX = width;
		_chunkNumY = height;
		_chunkNumZ = depth;
		_chunkRebuildList = chunkRebuildList;
		_chunks = new Dictionary<IntVector3, B45ChunkData>();
		_SaveData = new Dictionary<IntVector3, B45Block>();
	}

	public B45ChunkData readChunk(int x, int y, int z, int lod = 0)
	{
		IntVector3 intVector = new IntVector3(x % _chunkNumX, y % _chunkNumY, z % _chunkNumZ);
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
			IntVector3 key = new IntVector3(x % _chunkNumX, y % _chunkNumY, z % _chunkNumZ);
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
		int x_ = (x >> num) % _chunkNumX;
		int y_ = (y >> num) % _chunkNumY;
		int z_ = (z >> num) % _chunkNumZ;
		int x2 = x & 7;
		int y2 = y & 7;
		int z2 = z & 7;
		IntVector3 key = new IntVector3(x_, y_, z_);
		if (!_chunks.ContainsKey(key))
		{
			return new B45Block(0, 0);
		}
		return _chunks[key].ReadVoxelAtIdx(x2, y2, z2);
	}

	public int Write(int x, int y, int z, B45Block voxel, int lod = 0)
	{
		int num = 3;
		int num2 = x >> num;
		int num3 = y >> num;
		int num4 = z >> num;
		int x_ = num2 % _chunkNumX;
		int y_ = num3 % _chunkNumY;
		int z_ = num4 % _chunkNumZ;
		int num5 = x & 7;
		int num6 = y & 7;
		int num7 = z & 7;
		IntVector3 intVector = new IntVector3(x_, y_, z_);
		if (!_chunks.ContainsKey(intVector))
		{
			_chunks[intVector] = CreateChunk(intVector);
		}
		if (!_chunks[intVector].WriteVoxelAtIdx(num5, num6, num7, voxel))
		{
			return 0;
		}
		if (voxel.blockType >> 2 == 0)
		{
			_SaveData.Remove(new IntVector3(x, y, z));
		}
		else
		{
			_SaveData[new IntVector3(x, y, z)] = voxel;
		}
		int num8 = 1;
		int num9 = 7;
		int num10 = 0;
		int num11 = 0;
		int num12 = 0;
		int num13 = 128;
		if (num5 < num8 && num2 > 0)
		{
			num10 = -1;
			num13 |= 0x11;
		}
		else if (num5 >= num9 && num2 < 2511)
		{
			num10 = 1;
			num13 |= 1;
		}
		if (num6 < num8 && num3 > 0)
		{
			num11 = -1;
			num13 |= 0x22;
		}
		else if (num6 >= num9 && num3 < 255)
		{
			num11 = 1;
			num13 |= 2;
		}
		if (num7 < num8 && num4 > 0)
		{
			num12 = -1;
			num13 |= 0x44;
		}
		else if (num7 >= num9 && num4 < 2511)
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
				x_ = (num2 + num14) % _chunkNumX;
				y_ = (num3 + num15) % _chunkNumY;
				z_ = (num4 + num16) % _chunkNumZ;
				intVector = new IntVector3(x_, y_, z_);
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

	public B45Block SafeRead(int x, int y, int z, int lod = 0)
	{
		return Read(x, y, z, lod);
	}

	public bool SafeWrite(int x, int y, int z, B45Block voxel, int lod = 0)
	{
		Write(x, y, z, voxel, lod);
		return true;
	}

	private B45ChunkData CreateChunk(IntVector3 index)
	{
		B45ChunkData b45ChunkData = new B45ChunkData(new byte[2]);
		b45ChunkData.BuildList = _chunkRebuildList;
		writeChunk(index.x, index.y, index.z, b45ChunkData);
		b45ChunkData.ChunkPosLod_w = new IntVector4(index.x, index.y, index.z, 0);
		b45ChunkData.AddToBuildList();
		return b45ChunkData;
	}

	public void CleanupOBChunks()
	{
		IntVector3 zero = IntVector3.Zero;
		IntVector3 zero2 = IntVector3.Zero;
		List<IntVector3> list = new List<IntVector3>();
		foreach (IntVector3 key2 in _chunks.Keys)
		{
			if (Mathf.Abs(key2.x - zero.x) > zero2.x || Mathf.Abs(key2.y - zero.y) > zero2.y || Mathf.Abs(key2.z - zero.z) > zero2.z)
			{
				list.Add(key2);
			}
		}
		for (int i = 0; i < list.Count; i++)
		{
			IntVector3 key = list[i];
			B45ChunkData b45ChunkData = _chunks[key];
			if (b45ChunkData != null)
			{
				b45ChunkData.ClearMem();
				b45ChunkData.DestroyChunkGO();
			}
		}
	}

	public Dictionary<IntVector3, B45Block> GetSaveDate()
	{
		return _SaveData;
	}

	public void ApplySaveData(Dictionary<IntVector3, B45Block> saveData)
	{
		foreach (IntVector3 key in saveData.Keys)
		{
			SafeWrite(key.x, key.y, key.z, saveData[key]);
		}
	}
}

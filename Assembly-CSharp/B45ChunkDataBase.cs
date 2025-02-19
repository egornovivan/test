using System.Collections.Generic;
using System.Linq;
using uLink;

public class B45ChunkDataBase
{
	public IntVector3 _chunkPos;

	public int _lod;

	public int svn_key;

	public int svn_key_ba;

	public List<UpdateVector> updateVectors;

	public List<UVKeyCount> uvVersionKeys;

	public byte[] _chunkData;

	public int[] UpdateDatas => updateVectors.Select((UpdateVector iter) => iter.Data).ToArray();

	public bool IsHollow => _chunkData.Length == 2;

	public B45ChunkDataBase(byte[] _rawData)
	{
		_chunkData = _rawData;
	}

	public B45ChunkDataBase(B45Block blk)
	{
		_chunkData = new byte[2];
		_chunkData[0] = blk.blockType;
		_chunkData[1] = blk.materialType;
	}

	public static void WriteChunkBase(BitStream stream, object obj, params object[] codecOptions)
	{
		if (obj is B45ChunkDataBase b45ChunkDataBase)
		{
			stream.Write(b45ChunkDataBase._chunkPos);
			stream.Write(b45ChunkDataBase._lod);
			stream.Write(b45ChunkDataBase.svn_key);
			stream.Write(b45ChunkDataBase.svn_key_ba);
			stream.Write(b45ChunkDataBase._chunkData);
		}
	}

	public static B45ChunkDataBase ReadChunkBase(BitStream stream, params object[] codecOptions)
	{
		B45ChunkDataBase b45ChunkDataBase = new B45ChunkDataBase(null);
		stream.TryRead<IntVector3>(out b45ChunkDataBase._chunkPos);
		stream.TryRead<int>(out b45ChunkDataBase._lod);
		stream.TryRead<int>(out b45ChunkDataBase.svn_key);
		stream.TryRead<int>(out b45ChunkDataBase.svn_key_ba);
		stream.TryRead<byte[]>(out b45ChunkDataBase._chunkData);
		return b45ChunkDataBase;
	}

	public void InitUpdateVectors()
	{
		updateVectors = new List<UpdateVector>();
		uvVersionKeys = new List<UVKeyCount>();
		uvVersionKeys.Add(new UVKeyCount());
		svn_key = 1;
		svn_key_ba = 1;
	}
}

using uLink;

public class B45ChunkDataHeader
{
	public IntVector3 _chunkPos;

	public int _lod;

	public int svn_key;

	public int svn_key_ba;

	public static void WriteChunkHeader(BitStream stream, object obj, params object[] codecOptions)
	{
		if (obj is B45ChunkDataHeader b45ChunkDataHeader)
		{
			stream.Write(b45ChunkDataHeader._chunkPos);
			stream.Write(b45ChunkDataHeader.svn_key);
			stream.Write(b45ChunkDataHeader.svn_key_ba);
		}
	}

	public static B45ChunkDataHeader ReadChunkHeader(BitStream stream, params object[] codecOptions)
	{
		B45ChunkDataHeader b45ChunkDataHeader = new B45ChunkDataHeader();
		stream.TryRead<IntVector3>(out b45ChunkDataHeader._chunkPos);
		stream.TryRead<int>(out b45ChunkDataHeader.svn_key);
		stream.TryRead<int>(out b45ChunkDataHeader.svn_key_ba);
		return b45ChunkDataHeader;
	}
}

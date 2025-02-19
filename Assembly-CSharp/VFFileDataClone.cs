using System.IO;

public class VFFileDataClone
{
	public const int FSetDataCountXZ = 48;

	public const int FSetDataCountY = 23;

	public const int FSetOfsDataLen = 211968;

	public static byte[] ofsData = new byte[211968];

	public FileStream fs;

	public int len;

	public IntVector3 _pos = new IntVector3(-1, -1, -1);

	public int _lod;

	public void GetOfs(IntVector4 piecePos, out int pieceDataOfs, out int pieceDataLen)
	{
		int num = piecePos.x % 48 >> _lod;
		int num2 = piecePos.z % 48 >> _lod;
		int num3 = piecePos.y >> _lod;
		int num4 = (num * (48 >> _lod) + num2) * (23 >> _lod) + num3;
		num4 *= 4;
		pieceDataOfs = ofsData[num4] + (ofsData[num4 + 1] << 8) + (ofsData[num4 + 2] << 16) + (ofsData[num4 + 3] << 24);
		int num5 = (48 >> _lod) * (48 >> _lod) * (23 >> _lod) * 4;
		if (num4 >= num5 - 4)
		{
			pieceDataLen = len - pieceDataOfs;
		}
		else
		{
			pieceDataLen = ofsData[num4 + 4] + (ofsData[num4 + 5] << 8) + (ofsData[num4 + 6] << 16) + (ofsData[num4 + 7] << 24) - pieceDataOfs;
		}
	}

	public static void PiecePos2FileIndex(IntVector4 piecePos, out IntVector4 fileIndex)
	{
		fileIndex = new IntVector4(piecePos.x / 48, 0, piecePos.z / 48, piecePos.w);
	}

	public static void WorldChunkPosToPiecePos(IntVector4 chunkPos, out IntVector4 piecePos)
	{
		int w = chunkPos.w;
		int num = w + 2;
		piecePos = new IntVector4(chunkPos.x >> num << w, chunkPos.y >> num << w, chunkPos.z >> num << w, chunkPos.w);
	}

	public static bool Match(VFFileDataClone fileData, IntVector4 fileIndex)
	{
		return fileData._pos.Equals(fileIndex.XYZ) && fileData._lod == fileIndex.w;
	}
}

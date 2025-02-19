using System;
using System.Runtime.InteropServices;
using UnityEngine;

public class VFPieceDataClone
{
	public static byte[] unzippedDataBuffer = new byte[5488256];

	public static int unzippedDataLen = 0;

	public static IntVector4 unzippedDataDesc = new IntVector4(-1, -1, -1, -1);

	public IntVector3 _pos = new IntVector3(-1, -1, -1);

	public int _lod;

	public byte[] _data;

	[DllImport("lz4_dll")]
	public static extern int LZ4_DllLoad();

	[DllImport("lz4_dll")]
	public static extern int LZ4_compress(byte[] source, byte[] dest, int isize);

	[DllImport("lz4_dll")]
	public static extern int LZ4_uncompress(byte[] source, byte[] dest, int osize);

	[DllImport("lz4_dll")]
	public static extern int LZ4_uncompress_unknownOutputSize(byte[] source, byte[] dest, int isize, int maxOutputSize);

	public static bool Match(VFPieceDataClone pieceData, IntVector3 piecePos, int lod)
	{
		return pieceData._pos.Equals(piecePos) && pieceData._lod == lod;
	}

	public bool IsHollow()
	{
		return _data.Length == 2;
	}

	public void Decompress()
	{
		if (!IsHollow() && (unzippedDataDesc.x != _pos.x || unzippedDataDesc.y != _pos.y || unzippedDataDesc.z != _pos.z || unzippedDataDesc.w != _lod))
		{
			unzippedDataLen = LZ4_uncompress_unknownOutputSize(_data, unzippedDataBuffer, _data.Length, unzippedDataBuffer.Length);
			if (unzippedDataLen < 0)
			{
				Debug.LogError("[VFDATAReaderClone]Failed to decompress vfdata.@" + unzippedDataDesc);
			}
			unzippedDataDesc.x = _pos.x;
			unzippedDataDesc.y = _pos.y;
			unzippedDataDesc.z = _pos.z;
			unzippedDataDesc.w = _lod;
		}
	}

	public void SetChunkData(VFVoxelChunkData chunkData)
	{
		if (IsHollow())
		{
			chunkData.SetDataVT(new byte[2]
			{
				_data[0],
				_data[1]
			});
			return;
		}
		byte[] array = unzippedDataBuffer;
		int num = unzippedDataLen;
		int num2 = 0;
		int num3 = (chunkData.ChunkPosLod.x >> _lod) % 4;
		int num4 = (chunkData.ChunkPosLod.y >> _lod) % 4;
		int num5 = (chunkData.ChunkPosLod.z >> _lod) % 4;
		int num6 = 16 * num3 + 4 * num4 + num5;
		int num7 = 63;
		num2 += 4 * num6;
		int num8 = array[num2] + (array[num2 + 1] << 8) + (array[num2 + 2] << 16) + (array[num2 + 3] << 24);
		int num9 = ((num6 >= num7) ? num : (array[num2 + 4] + (array[num2 + 5] << 8) + (array[num2 + 6] << 16) + (array[num2 + 7] << 24)));
		int num10 = num9 - num8;
		byte[] array2 = new byte[num10];
		Array.Copy(array, num8, array2, 0, num10);
		chunkData.SetDataVT(array2);
	}
}

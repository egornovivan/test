using System;
using System.Collections.Generic;
using System.IO;

public class VoxelFileBA
{
	private string fileName;

	private FileStream fs;

	private byte[] binaryBuffer;

	public FileHeader fileHeader;

	private bool chkOfsInVectorForm;

	private int chunkCount;

	private int chunkRawDataLength;

	private int phase2StartOfs;

	public IntVector3[] chunkCoords;

	private int[] chunkOffsets;

	public int[] svn_keys_ba;

	public VoxelFileBA(string _fileName)
	{
		fileName = _fileName + "_ba.bin";
	}

	public int GetChunkCount()
	{
		return chunkCount;
	}

	public bool ReadHeader()
	{
		try
		{
			fs = new FileStream(fileName, FileMode.Open, FileAccess.Read, FileShare.Read);
		}
		catch (Exception)
		{
			return false;
		}
		if (fs.Length == 0L)
		{
			fs.Close();
			return false;
		}
		byte[] array = new byte[2];
		fs.Read(array, 0, 2);
		if ((array[1] & 1) <= 0)
		{
			binaryBuffer = new byte[fs.Length - 2];
			fs.Read(binaryBuffer, 0, (int)fs.Length - 2);
		}
		fs.Close();
		fileHeader.cellLength = binaryBuffer[1];
		fileHeader.chunkType = binaryBuffer[2];
		fileHeader.chunkSize = binaryBuffer[3];
		fileHeader.chunkPrefix = binaryBuffer[4];
		fileHeader.chunkPostfix = binaryBuffer[5];
		fileHeader.chunkCountX = ByteArrayHelper.to_ushort(binaryBuffer, 6);
		fileHeader.chunkCountY = ByteArrayHelper.to_ushort(binaryBuffer, 8);
		fileHeader.chunkCountZ = ByteArrayHelper.to_ushort(binaryBuffer, 10);
		fileHeader.voxelRes = ByteArrayHelper.to_ushort(binaryBuffer, 12);
		fileHeader.chunkOffsetDesc = binaryBuffer[14];
		chkOfsInVectorForm = fileHeader.chunkOffsetDesc >> 4 > 0;
		chunkRawDataLength = fileHeader.chunkSize * fileHeader.chunkSize * fileHeader.chunkSize * fileHeader.cellLength;
		int num = 0;
		if (chkOfsInVectorForm)
		{
			chunkCount = ByteArrayHelper.to_int(binaryBuffer, 15);
			num = 19;
			phase2StartOfs = num + chunkCount * 11;
			chunkCoords = new IntVector3[chunkCount];
			chunkOffsets = new int[chunkCount];
			svn_keys_ba = new int[chunkCount];
			for (int i = 0; i < chunkCount; i++)
			{
				int num2 = num + i * 11;
				chunkCoords[i] = ByteArrayHelper.to_IntVector3(binaryBuffer, num2);
				chunkOffsets[i] = ByteArrayHelper.to_int(binaryBuffer, num2 + 3);
				svn_keys_ba[i] = ByteArrayHelper.to_int(binaryBuffer, num2 + 7);
			}
		}
		return true;
	}

	private byte makeChunkOffsetDesc(bool isInOfsMode, int unitLen)
	{
		byte b = (byte)unitLen;
		return (byte)(b | (isInOfsMode ? 16 : 0));
	}

	public B45ChunkDataBase ReadChunkData(int nth)
	{
		int num = phase2StartOfs + chunkOffsets[nth];
		B45ChunkDataBase b45ChunkDataBase;
		if (binaryBuffer[num] == 0)
		{
			byte[] array = new byte[chunkRawDataLength];
			Array.Copy(binaryBuffer, num + 1, array, 0, chunkRawDataLength);
			b45ChunkDataBase = new B45ChunkDataBase(array);
		}
		else
		{
			B45Block blk = default(B45Block);
			blk.blockType = binaryBuffer[num + 1];
			blk.materialType = binaryBuffer[num + 1 + 1];
			b45ChunkDataBase = new B45ChunkDataBase(blk);
		}
		b45ChunkDataBase._chunkPos = new IntVector3(chunkCoords[nth]);
		b45ChunkDataBase.svn_key_ba = svn_keys_ba[nth];
		return b45ChunkDataBase;
	}

	public void InitHeader()
	{
		fileHeader.version = 0;
		fileHeader.cellLength = 2;
		fileHeader.chunkType = 1;
		fileHeader.chunkSize = 10;
		fileHeader.chunkPrefix = 1;
		fileHeader.chunkPostfix = 1;
		fileHeader.chunkCountX = 32;
		fileHeader.chunkCountY = 8;
		fileHeader.chunkCountZ = 32;
		fileHeader.voxelRes = 1;
		fileHeader.chunkOffsetDesc = makeChunkOffsetDesc(isInOfsMode: true, 1);
	}

	public void WriteBAHeader(bool compressed, List<B45ChunkDataBase> chunkDataList)
	{
		fs = new FileStream(fileName, FileMode.Create, FileAccess.Write, FileShare.Write);
		byte[] array = new byte[2];
		if (compressed)
		{
			array[1] = (byte)(array[1] | 1);
		}
		fs.Write(array, 0, 2);
		byte[] array2 = new byte[15]
		{
			0, fileHeader.cellLength, fileHeader.chunkType, fileHeader.chunkSize, fileHeader.chunkPrefix, fileHeader.chunkPostfix, 0, 0, 0, 0,
			0, 0, 0, 0, 0
		};
		ByteArrayHelper.ushort_to(array2, 6, fileHeader.chunkCountX);
		ByteArrayHelper.ushort_to(array2, 8, fileHeader.chunkCountY);
		ByteArrayHelper.ushort_to(array2, 10, fileHeader.chunkCountZ);
		ByteArrayHelper.ushort_to(array2, 12, fileHeader.voxelRes);
		array2[14] = fileHeader.chunkOffsetDesc;
		fs.Write(array2, 0, 15);
		chkOfsInVectorForm = fileHeader.chunkOffsetDesc >> 4 > 0;
		int num = 4 + chunkDataList.Count * 11;
		byte[] array3 = new byte[num];
		chunkRawDataLength = fileHeader.chunkSize * fileHeader.chunkSize * fileHeader.chunkSize * fileHeader.cellLength;
		if (chkOfsInVectorForm)
		{
			ByteArrayHelper.int_to(array3, 0, chunkDataList.Count);
			chunkOffsets = new int[chunkDataList.Count + 1];
			chunkOffsets[0] = 0;
			for (int i = 1; i <= chunkDataList.Count; i++)
			{
				int num2 = ((!chunkDataList[i - 1].IsHollow) ? (1 + chunkRawDataLength) : (1 + fileHeader.cellLength));
				chunkOffsets[i] = chunkOffsets[i - 1] + num2;
			}
			for (int i = 0; i < chunkDataList.Count; i++)
			{
				B45ChunkDataBase b45ChunkDataBase = chunkDataList[i];
				ByteArrayHelper.IntVector3_to(array3, 4 + i * 11, b45ChunkDataBase._chunkPos);
				ByteArrayHelper.int_to(array3, 4 + i * 11 + 3, chunkOffsets[i]);
				ByteArrayHelper.int_to(array3, 4 + i * 11 + 7, b45ChunkDataBase.svn_key_ba);
			}
			fs.Write(array3, 0, num);
		}
		WriteChunkData(chunkDataList);
		fs.Close();
	}

	private void WriteChunkData(List<B45ChunkDataBase> chunkDataList)
	{
		byte[] array = new byte[chunkOffsets[chunkOffsets.Length - 1]];
		for (int i = 0; i < chunkDataList.Count; i++)
		{
			B45ChunkDataBase b45ChunkDataBase = chunkDataList[i];
			array[chunkOffsets[i]] = (byte)(b45ChunkDataBase.IsHollow ? 1 : 0);
			int num = chunkOffsets[i] + 1;
			if (b45ChunkDataBase.IsHollow)
			{
				Array.Copy(b45ChunkDataBase._chunkData, 0, array, chunkOffsets[i] + 1, 2);
				num += 2;
			}
			else
			{
				Array.Copy(b45ChunkDataBase._chunkData, 0, array, chunkOffsets[i] + 1, chunkRawDataLength);
				num += chunkRawDataLength;
			}
		}
		fs.Write(array, 0, array.Length);
	}
}

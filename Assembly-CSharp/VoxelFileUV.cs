using System;
using System.Collections.Generic;
using System.IO;

public class VoxelFileUV
{
	private string fileName;

	private FileStream fs;

	private byte[] binaryBuffer;

	private byte chunkOffsetDesc;

	private bool chkOfsInVectorForm;

	private int chunkCount;

	private int chunkRawDataLength;

	private int phase2StartOfs;

	private IntVector3[] chunkCoords;

	private int[] chunkOffsets;

	public int[] svn_keys_uv;

	public VoxelFileUV(string _fileName)
	{
		fileName = _fileName + "_uv.bin";
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
		if (fs.Length < 5)
		{
			fs.Close();
			return false;
		}
		chunkOffsetDesc = (byte)fs.ReadByte();
		if ((chunkOffsetDesc & 0x20) <= 0)
		{
			binaryBuffer = new byte[fs.Length - 1];
			fs.Read(binaryBuffer, 0, (int)fs.Length - 1);
		}
		fs.Close();
		chkOfsInVectorForm = chunkOffsetDesc >> 4 > 0;
		int num = 0;
		if (chkOfsInVectorForm)
		{
			chunkCount = ByteArrayHelper.to_int(binaryBuffer, 0);
			num = 4;
			phase2StartOfs = num + chunkCount * 11;
			chunkCoords = new IntVector3[chunkCount];
			chunkOffsets = new int[chunkCount];
			svn_keys_uv = new int[chunkCount];
			for (int i = 0; i < chunkCount; i++)
			{
				int num2 = num + i * 11;
				chunkCoords[i] = ByteArrayHelper.to_IntVector3(binaryBuffer, num2);
				chunkOffsets[i] = ByteArrayHelper.to_int(binaryBuffer, num2 + 3);
				svn_keys_uv[i] = ByteArrayHelper.to_int(binaryBuffer, num2 + 7);
			}
		}
		return true;
	}

	private byte makeChunkOffsetDesc(bool isInOfsMode, int unitLen)
	{
		byte b = (byte)unitLen;
		return (byte)(b | (isInOfsMode ? 16 : 0));
	}

	public B45ChunkDataBase AttachUVData(int nth, B45ChunkDataBase cd)
	{
		int num = phase2StartOfs + chunkOffsets[nth];
		int num2 = ByteArrayHelper.to_int(binaryBuffer, num);
		int num3 = ByteArrayHelper.to_int(binaryBuffer, num + 4);
		cd.svn_key = svn_keys_uv[nth];
		cd.uvVersionKeys = new List<UVKeyCount>();
		cd.updateVectors = new List<UpdateVector>();
		for (int i = 0; i < num3; i++)
		{
			int num4 = num + 8 + i * 6;
			UVKeyCount uVKeyCount = new UVKeyCount();
			uVKeyCount.svn_key = ByteArrayHelper.to_int(binaryBuffer, num4);
			uVKeyCount.count = ByteArrayHelper.to_ushort(binaryBuffer, num4 + 4);
			cd.uvVersionKeys.Add(uVKeyCount);
		}
		int num5 = 6 * num3;
		for (int j = 0; j < num2; j++)
		{
			int num6 = num + 8 + num5 + j * 4;
			UpdateVector updateVector = new UpdateVector();
			updateVector.xyz0 = binaryBuffer[num6];
			updateVector.xyz1 = binaryBuffer[num6 + 1];
			updateVector.voxelData0 = binaryBuffer[num6 + 2];
			updateVector.voxelData1 = binaryBuffer[num6 + 3];
			cd.updateVectors.Add(updateVector);
		}
		return cd;
	}

	public void InitHeader()
	{
		chunkOffsetDesc = makeChunkOffsetDesc(isInOfsMode: true, 1);
	}

	public void WriteUVHeader(bool compressed, List<B45ChunkDataBase> chunkDataList)
	{
		fs = new FileStream(fileName, FileMode.Create, FileAccess.Write, FileShare.Write);
		byte b = chunkOffsetDesc;
		if (compressed)
		{
			b |= 0x20;
		}
		fs.WriteByte(b);
		chkOfsInVectorForm = chunkOffsetDesc >> 4 > 0;
		int num = 4 + chunkDataList.Count * 11;
		byte[] array = new byte[num];
		if (chkOfsInVectorForm)
		{
			ByteArrayHelper.int_to(array, 0, chunkDataList.Count);
			chunkOffsets = new int[chunkDataList.Count + 1];
			chunkOffsets[0] = 0;
			for (int i = 1; i <= chunkDataList.Count; i++)
			{
				List<UVKeyCount> uvVersionKeys = chunkDataList[i - 1].uvVersionKeys;
				int num2 = 0;
				if (uvVersionKeys != null)
				{
					num2 = uvVersionKeys.Count * 6;
				}
				List<UpdateVector> updateVectors = chunkDataList[i - 1].updateVectors;
				int num3 = 0;
				if (updateVectors != null)
				{
					num3 = updateVectors.Count * 4;
				}
				int num4 = 8 + num2 + num3;
				chunkOffsets[i] = chunkOffsets[i - 1] + num4;
			}
			for (int i = 0; i < chunkDataList.Count; i++)
			{
				B45ChunkDataBase b45ChunkDataBase = chunkDataList[i];
				ByteArrayHelper.IntVector3_to(array, 4 + i * 11, b45ChunkDataBase._chunkPos);
				ByteArrayHelper.int_to(array, 4 + i * 11 + 3, chunkOffsets[i]);
				ByteArrayHelper.int_to(array, 4 + i * 11 + 7, b45ChunkDataBase.svn_key);
			}
			fs.Write(array, 0, num);
		}
		WriteUVData(chunkDataList);
		fs.Close();
	}

	private void WriteUVData(List<B45ChunkDataBase> chunkDataList)
	{
		byte[] array = new byte[chunkOffsets[chunkOffsets.Length - 1]];
		for (int i = 0; i < chunkDataList.Count; i++)
		{
			B45ChunkDataBase b45ChunkDataBase = chunkDataList[i];
			if (b45ChunkDataBase.updateVectors == null)
			{
				ByteArrayHelper.int_to(array, chunkOffsets[i], 0);
			}
			else
			{
				ByteArrayHelper.int_to(array, chunkOffsets[i], b45ChunkDataBase.updateVectors.Count);
			}
			if (b45ChunkDataBase.uvVersionKeys == null)
			{
				ByteArrayHelper.int_to(array, chunkOffsets[i] + 4, 0);
			}
			else
			{
				ByteArrayHelper.int_to(array, chunkOffsets[i] + 4, b45ChunkDataBase.uvVersionKeys.Count);
			}
			int num = chunkOffsets[i] + 8;
			if (b45ChunkDataBase.uvVersionKeys != null)
			{
				for (int j = 0; j < b45ChunkDataBase.uvVersionKeys.Count; j++)
				{
					ByteArrayHelper.int_to(array, num + j * 6, b45ChunkDataBase.uvVersionKeys[j].svn_key);
					ByteArrayHelper.ushort_to(array, num + j * 6 + 4, b45ChunkDataBase.uvVersionKeys[j].count);
				}
				int num2 = num + b45ChunkDataBase.uvVersionKeys.Count * 6;
				if (b45ChunkDataBase.updateVectors != null)
				{
					for (int k = 0; k < b45ChunkDataBase.updateVectors.Count; k++)
					{
						array[num2 + k * 4] = b45ChunkDataBase.updateVectors[k].xyz0;
						array[num2 + k * 4 + 1] = b45ChunkDataBase.updateVectors[k].xyz1;
						array[num2 + k * 4 + 2] = b45ChunkDataBase.updateVectors[k].voxelData0;
						array[num2 + k * 4 + 3] = b45ChunkDataBase.updateVectors[k].voxelData1;
					}
				}
			}
			b45ChunkDataBase.updateVectors = null;
		}
		fs.Write(array, 0, array.Length);
	}
}

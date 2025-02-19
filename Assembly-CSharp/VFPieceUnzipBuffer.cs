using System;
using UnityEngine;

public class VFPieceUnzipBuffer
{
	public byte[] zippedDataBuffer = new byte[5510208];

	public byte[] unzippedDataBuffer = new byte[5488256];

	public int zippedDataLen;

	public int unzippedDataLen;

	public IntVector4 unzippedDataDesc = new IntVector4(-1, -1, -1, -1);

	public bool IsHollow()
	{
		return zippedDataLen == 2;
	}

	public bool IsHitCache(int px, int py, int pz, int lod)
	{
		return unzippedDataDesc.x == px && unzippedDataDesc.y == py && unzippedDataDesc.z == pz && unzippedDataDesc.w == lod;
	}

	public void Decompress(int px, int py, int pz, int lod)
	{
		if (!IsHollow() && !IsHitCache(px, py, pz, lod))
		{
			unzippedDataLen = LZ4.LZ4_uncompress_unknownOutputSize(zippedDataBuffer, unzippedDataBuffer, zippedDataLen, unzippedDataBuffer.Length);
			if (unzippedDataLen < 0)
			{
				Debug.LogError("[VFDATAReader]Failed to decompress vfdata.@" + px + py + pz + lod);
			}
			else
			{
				unzippedDataDesc.x = px;
				unzippedDataDesc.y = py;
				unzippedDataDesc.z = pz;
				unzippedDataDesc.w = lod;
			}
		}
	}

	public void SetChunkData(VFVoxelChunkData chunkData, ChunkDataLoadedProcessor chunkDataProc)
	{
		if (IsHollow())
		{
			byte b = zippedDataBuffer[0];
			byte b2 = zippedDataBuffer[1];
			byte[] array = b switch
			{
				0 => VFVoxelChunkData.S_ChunkDataAir, 
				128 => VFVoxelChunkData.S_ChunkDataWaterPlane, 
				byte.MaxValue => VFVoxelChunkData.S_ChunkDataSolid[b2], 
				_ => new byte[2] { b, b2 }, 
			};
			if (chunkDataProc == null)
			{
				chunkData.OnDataLoaded(array);
			}
			else
			{
				chunkDataProc(chunkData, array, fromPool: false);
			}
			return;
		}
		byte[] array2 = unzippedDataBuffer;
		int num = unzippedDataLen;
		int num2 = 0;
		int num3 = VFFileUtil.ChunkPos2IndexInPiece(chunkData.ChunkPosLod);
		int num4 = 63;
		num2 += 4 * num3;
		try
		{
			int num5 = array2[num2] + (array2[num2 + 1] << 8) + (array2[num2 + 2] << 16) + (array2[num2 + 3] << 24);
			int num6 = ((num3 >= num4) ? num : (array2[num2 + 4] + (array2[num2 + 5] << 8) + (array2[num2 + 6] << 16) + (array2[num2 + 7] << 24)));
			int num7 = num6 - num5;
			switch (num7)
			{
			case 85750:
			{
				byte[] array = VFVoxelChunkData.s_ChunkDataPool.Get();
				Array.Copy(array2, num5, array, 0, 85750);
				if (chunkDataProc == null)
				{
					chunkData.OnDataLoaded(array, bFromPool: true);
				}
				else
				{
					chunkDataProc(chunkData, array, fromPool: true);
				}
				break;
			}
			case 2:
			{
				byte b = array2[num5];
				byte b2 = array2[num5 + 1];
				byte[] array = b switch
				{
					0 => VFVoxelChunkData.S_ChunkDataAir, 
					128 => VFVoxelChunkData.S_ChunkDataWaterPlane, 
					byte.MaxValue => VFVoxelChunkData.S_ChunkDataSolid[b2], 
					_ => new byte[2] { b, b2 }, 
				};
				if (chunkDataProc == null)
				{
					chunkData.OnDataLoaded(array);
				}
				else
				{
					chunkDataProc(chunkData, array, fromPool: false);
				}
				break;
			}
			default:
				Debug.LogWarning("[VFDATAReader]Unsupported data length(" + num7 + ")@" + chunkData.ChunkPosLod);
				break;
			}
		}
		catch
		{
			Debug.LogWarning("[VFDATAReader]Failed to read Chunk" + chunkData.ChunkPosLod);
		}
	}
}

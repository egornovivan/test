using System;
using System.IO;
using UnityEngine;

public static class VFDataReaderClone
{
	public static VFFileDataClone GetFileSetSub(IntVector4 fileIndex)
	{
		int w = fileIndex.w;
		VFFileDataClone vFFileDataClone = new VFFileDataClone();
		vFFileDataClone._pos.x = fileIndex.x;
		vFFileDataClone._pos.y = fileIndex.y;
		vFFileDataClone._pos.z = fileIndex.z;
		vFFileDataClone._lod = w;
		if (string.IsNullOrEmpty(VFVoxelTerrain.MapDataPath_Zip))
		{
			vFFileDataClone.fs = null;
		}
		else
		{
			string text = VFVoxelTerrain.MapDataPath_Zip + "/map_x" + fileIndex.x + "_y" + fileIndex.z + ((w == 0) ? ".voxelform" : ("_" + w + ".voxelform"));
			try
			{
				int count = (48 >> w) * (48 >> w) * (23 >> w) * 4;
				vFFileDataClone.fs = new FileStream(text, FileMode.Open, FileAccess.Read, FileShare.Read);
				vFFileDataClone.len = (int)vFFileDataClone.fs.Length;
				vFFileDataClone.fs.Read(VFFileDataClone.ofsData, 0, count);
			}
			catch (Exception)
			{
				Debug.LogError("[VFDataReader] Failed to read " + text);
				vFFileDataClone.fs = null;
			}
		}
		return vFFileDataClone;
	}

	public static VFPieceDataClone GetPieceDataSub(IntVector4 piecePos, VFFileDataClone fileSet)
	{
		VFPieceDataClone vFPieceDataClone = new VFPieceDataClone();
		vFPieceDataClone._pos.x = piecePos.x;
		vFPieceDataClone._pos.y = piecePos.y;
		vFPieceDataClone._pos.z = piecePos.z;
		vFPieceDataClone._lod = fileSet._lod;
		if (fileSet.fs == null)
		{
			vFPieceDataClone._data = new byte[2];
		}
		else
		{
			fileSet.GetOfs(piecePos, out var pieceDataOfs, out var pieceDataLen);
			vFPieceDataClone._data = new byte[pieceDataLen];
			fileSet.fs.Seek(pieceDataOfs, SeekOrigin.Begin);
			fileSet.fs.Read(vFPieceDataClone._data, 0, pieceDataLen);
			fileSet.fs.Close();
		}
		return vFPieceDataClone;
	}

	public static bool GetPieceData(out VFPieceDataClone pieceData, IntVector4 piecePos)
	{
		VFFileDataClone.PiecePos2FileIndex(piecePos, out var fileIndex);
		VFFileDataClone vFFileDataClone = null;
		vFFileDataClone = GetFileSetSub(fileIndex);
		pieceData = GetPieceDataSub(piecePos, vFFileDataClone);
		vFFileDataClone = null;
		return false;
	}

	public static void AddReadRequest(VFVoxelChunkData chunkData)
	{
		IntVector4 chunkPosLod = chunkData.ChunkPosLod;
		if (chunkPosLod.x >= 0 && chunkPosLod.x < 576 && chunkPosLod.y >= 0 && chunkPosLod.y < VoxelTerrainConstants.WorldMaxCY(chunkPosLod.w) && chunkPosLod.z >= 0 && chunkPosLod.z < 576)
		{
			VFFileDataClone.WorldChunkPosToPiecePos(chunkPosLod, out var piecePos);
			VFFileDataClone.PiecePos2FileIndex(piecePos, out var fileIndex);
			VFPieceDataClone pieceDataSub = GetPieceDataSub(piecePos, GetFileSetSub(fileIndex));
			pieceDataSub.Decompress();
			pieceDataSub.SetChunkData(chunkData);
			pieceDataSub._data = null;
			pieceDataSub = null;
		}
	}
}

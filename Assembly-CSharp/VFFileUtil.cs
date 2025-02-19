using System;
using System.IO;
using System.Linq;
using UnityEngine;

internal class VFFileUtil
{
	public const int OfsDataLen = 211968;

	public const int MaxFileIndex = 45;

	public static void PreOpenFile(string strDataFilePrefix, int x, int z, int lod, out FileStream file, out long len)
	{
		file = null;
		len = 0L;
		string text = strDataFilePrefix + "_x" + x + "_y" + z + ((lod == 0) ? ".voxelform" : ("_" + lod + ".voxelform"));
		try
		{
			file = new FileStream(text, FileMode.Open, FileAccess.Read, FileShare.Read);
			len = file.Length;
		}
		catch (Exception)
		{
			Debug.LogWarning("[VFDataReader] Failed to read " + text);
		}
	}

	public static void PreOpenAllFiles(string strDataFilePrefix, out FileStream[] fileStreams, out long[] fileLens)
	{
		fileLens = new long[45];
		fileStreams = new FileStream[45];
		if (string.IsNullOrEmpty(strDataFilePrefix))
		{
			Debug.LogError("[VFDataReader]Failed to open file because DataFilePrefix is null");
			return;
		}
		int num = 0;
		for (int i = 0; i <= 4; i++)
		{
			for (int j = 0; j < 3; j++)
			{
				for (int k = 0; k < 3; k++)
				{
					PreOpenFile(strDataFilePrefix, k, j, i, out fileStreams[num], out fileLens[num]);
					num++;
				}
			}
		}
	}

	public static void CloseAllFiles(ref FileStream curFileStream, ref FileStream[] fileStreams, ref long[] fileLens)
	{
		if (fileStreams != null)
		{
			if (fileStreams.Contains(curFileStream))
			{
				curFileStream = null;
			}
			FileStream[] array = fileStreams;
			for (int i = 0; i < array.Length; i++)
			{
				array[i]?.Close();
			}
			fileStreams = null;
		}
		if (curFileStream != null)
		{
			curFileStream.Close();
			curFileStream = null;
		}
		fileLens = null;
	}

	public static void PiecePos2FilePos(int px, int py, int pz, int lod, out int fx, out int fz)
	{
		fx = px / 48;
		fz = pz / 48;
	}

	public static void WorldChunkPosToPiecePos(IntVector4 chunkPos, out int px, out int py, out int pz)
	{
		int num = chunkPos.w + 2;
		px = chunkPos.x >> num << chunkPos.w;
		py = chunkPos.y >> num << chunkPos.w;
		pz = chunkPos.z >> num << chunkPos.w;
	}

	public static int ChunkPos2IndexInPiece(IntVector4 chunkPos)
	{
		int w = chunkPos.w;
		int num = 3;
		int num2 = (chunkPos.x >> w) & num;
		int num3 = (chunkPos.y >> w) & num;
		int num4 = (chunkPos.z >> w) & num;
		return 16 * num2 + 4 * num3 + num4;
	}

	public static int PiecePos2IndexInFile(int px, int py, int pz, int lod)
	{
		int num = px % 48 >> lod;
		int num2 = pz % 48 >> lod;
		int num3 = py >> lod;
		return (num * (48 >> lod) + num2) * (23 >> lod) + num3;
	}
}

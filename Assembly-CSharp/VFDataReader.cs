using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

public class VFDataReader : IVxDataLoader
{
	private struct ReqValue
	{
		public int px;

		public int py;

		public int pz;

		public int pw;

		public int stamp;

		public bool PosEqual(int x, int y, int z, int w)
		{
			return x == px && y == py && z == pz && w == pw;
		}
	}

	public delegate byte[] ProcToMergeChunkAtPos(IntVector4 chunkPos, byte[] baseChunkData);

	private Dictionary<VFVoxelChunkData, ReqValue> _chunkReqList = new Dictionary<VFVoxelChunkData, ReqValue>();

	private bool _bImmMode = true;

	private string _dataFilePrefix;

	private long[] _fileLens;

	private FileStream[] _fileStreams;

	private ChunkDataLoadedProcessor _chunkDataProc;

	public VFPieceUnzipBuffer _buff = new VFPieceUnzipBuffer();

	private long _curFileLen;

	private FileStream _curFileStream;

	private int _curFileX;

	private int _curFileZ;

	private int _curFileLod = -1;

	private byte[] _ofsData = new byte[211968];

	public bool IsIdle => _chunkReqList.Count == 0;

	public bool ImmMode
	{
		get
		{
			return _bImmMode;
		}
		set
		{
			_bImmMode = value;
		}
	}

	public VFDataReader(string dataFilePrefix, ChunkDataLoadedProcessor chunkDataProc = null, bool bPreopenFiles = true)
	{
		_dataFilePrefix = dataFilePrefix;
		_chunkDataProc = chunkDataProc;
		if (bPreopenFiles)
		{
			VFFileUtil.PreOpenAllFiles(_dataFilePrefix, out _fileStreams, out _fileLens);
		}
	}

	private void GetPieceOfsNLen(long fileLen, int px, int py, int pz, int lod, out int pieceDataOfs, out int pieceDataLen)
	{
		int num = VFFileUtil.PiecePos2IndexInFile(px, py, pz, lod) * 4;
		pieceDataOfs = _ofsData[num] + (_ofsData[num + 1] << 8) + (_ofsData[num + 2] << 16) + (_ofsData[num + 3] << 24);
		int num2 = (48 >> lod) * (48 >> lod) * (23 >> lod) * 4;
		if (num >= num2 - 4)
		{
			pieceDataLen = (int)(fileLen - pieceDataOfs);
		}
		else
		{
			pieceDataLen = _ofsData[num + 4] + (_ofsData[num + 5] << 8) + (_ofsData[num + 6] << 16) + (_ofsData[num + 7] << 24) - pieceDataOfs;
		}
	}

	private void FetchFile(int fx, int fz, int lod)
	{
		if (fx != _curFileX || fz != _curFileZ || lod != _curFileLod)
		{
			if (_fileStreams != null)
			{
				int num = lod * 3 * 3 + fz * 3 + fx;
				_curFileStream = _fileStreams[num];
				_curFileLen = _fileLens[num];
			}
			else
			{
				VFFileUtil.PreOpenFile(_dataFilePrefix, fx, fz, lod, out _curFileStream, out _curFileLen);
			}
			if (_curFileStream != null)
			{
				int count = (48 >> lod) * (48 >> lod) * (23 >> lod) * 4;
				_curFileStream.Seek(0L, SeekOrigin.Begin);
				_curFileStream.Read(_ofsData, 0, count);
			}
			_curFileX = fx;
			_curFileZ = fz;
			_curFileLod = lod;
		}
	}

	private void ReadPieceDataToBuff(int px, int py, int pz, int lod, int fx, int fz)
	{
		FetchFile(fx, fz, lod);
		if (_curFileStream == null)
		{
			_buff.zippedDataLen = 2;
			_buff.zippedDataBuffer[0] = 0;
			_buff.zippedDataBuffer[1] = 0;
		}
		else
		{
			GetPieceOfsNLen(_curFileLen, px, py, pz, lod, out var pieceDataOfs, out var pieceDataLen);
			_buff.zippedDataLen = pieceDataLen;
			_curFileStream.Seek(pieceDataOfs, SeekOrigin.Begin);
			_curFileStream.Read(_buff.zippedDataBuffer, 0, _buff.zippedDataLen);
		}
	}

	public void ReadPieceDataToBuff(int px, int py, int pz, int lod)
	{
		VFFileUtil.PiecePos2FilePos(px, py, pz, lod, out var fx, out var fz);
		ReadPieceDataToBuff(px, py, pz, lod, fx, fz);
	}

	public void Close()
	{
		VFFileUtil.CloseAllFiles(ref _curFileStream, ref _fileStreams, ref _fileLens);
	}

	public void AddRequest(VFVoxelChunkData chunkData)
	{
		IntVector4 chunkPosLod = chunkData.ChunkPosLod;
		ReqValue value;
		if (chunkPosLod.x < 0 || chunkPosLod.x >= 576 || chunkPosLod.y < 0 || chunkPosLod.y >= VoxelTerrainConstants.WorldMaxCY(chunkPosLod.w) || chunkPosLod.z < 0 || chunkPosLod.z >= 576)
		{
			chunkData.OnDataLoaded(VFVoxelChunkData.S_ChunkDataAir);
		}
		else if (_bImmMode)
		{
			VFFileUtil.WorldChunkPosToPiecePos(chunkPosLod, out var px, out var py, out var pz);
			ReadPieceDataToBuff(px, py, pz, chunkPosLod.w);
			_buff.Decompress(px, py, pz, chunkPosLod.w);
			_buff.SetChunkData(chunkData, _chunkDataProc);
		}
		else if (!_chunkReqList.TryGetValue(chunkData, out value) || !chunkData.IsStampIdentical(value.stamp))
		{
			VFFileUtil.WorldChunkPosToPiecePos(chunkPosLod, out value.px, out value.py, out value.pz);
			value.pw = chunkPosLod.w;
			value.stamp = chunkData.StampOfUpdating;
			_chunkReqList[chunkData] = value;
		}
	}

	public void ProcessReqs()
	{
		if (_chunkReqList.Count == 0)
		{
			return;
		}
		List<VFVoxelChunkData> list = _chunkReqList.Keys.Cast<VFVoxelChunkData>().ToList();
		do
		{
			int count = list.Count;
			VFVoxelChunkData vFVoxelChunkData = list[count - 1];
			list.RemoveAt(count - 1);
			ReqValue reqValue = _chunkReqList[vFVoxelChunkData];
			int px = reqValue.px;
			int py = reqValue.py;
			int pz = reqValue.pz;
			int pw = reqValue.pw;
			int stamp = reqValue.stamp;
			if (!vFVoxelChunkData.IsStampIdentical(stamp))
			{
				continue;
			}
			ReadPieceDataToBuff(px, py, pz, pw);
			_buff.Decompress(px, py, pz, pw);
			_buff.SetChunkData(vFVoxelChunkData, _chunkDataProc);
			for (int num = count - 2; num >= 0; num--)
			{
				vFVoxelChunkData = list[num];
				reqValue = _chunkReqList[vFVoxelChunkData];
				if (reqValue.PosEqual(px, py, pz, pw))
				{
					list.RemoveAt(num);
					if (vFVoxelChunkData.IsStampIdentical(reqValue.stamp))
					{
						_buff.SetChunkData(vFVoxelChunkData, _chunkDataProc);
					}
				}
			}
		}
		while (list.Count > 0);
		_chunkReqList.Clear();
	}

	public void ReplaceChunkDatas(List<IntVector4> newChunkPosList, ProcToMergeChunkAtPos mergeChunkDataAtPos)
	{
		Dictionary<IntVector4, Dictionary<IntVector4, List<IntVector4>>> dictionary = new Dictionary<IntVector4, Dictionary<IntVector4, List<IntVector4>>>();
		int count = newChunkPosList.Count;
		for (int i = 0; i < count; i++)
		{
			IntVector4 intVector = newChunkPosList[i];
			VFFileUtil.WorldChunkPosToPiecePos(intVector, out var px, out var py, out var pz);
			VFFileUtil.PiecePos2FilePos(px, py, pz, intVector.w, out var fx, out var fz);
			IntVector4 key = new IntVector4(fx, 0, fz, intVector.w);
			IntVector4 key2 = new IntVector4(px, py, pz, intVector.w);
			if (!dictionary.ContainsKey(key))
			{
				dictionary.Add(key, new Dictionary<IntVector4, List<IntVector4>>());
			}
			if (!dictionary[key].ContainsKey(key2))
			{
				dictionary[key].Add(key2, new List<IntVector4>());
			}
			dictionary[key][key2].Add(intVector);
		}
		List<IntVector4> list = dictionary.Keys.Cast<IntVector4>().ToList();
		int count2 = list.Count;
		for (int j = 0; j < count2; j++)
		{
			IntVector4 intVector2 = list[j];
			ReplacePiecesInFile(dictionary[intVector2], mergeChunkDataAtPos, intVector2);
		}
	}

	public void ReplacePiecesInFile(Dictionary<IntVector4, List<IntVector4>> piece2chunkList, ProcToMergeChunkAtPos mergeChunkDataAtPos, IntVector4 fileIndex)
	{
		int w = fileIndex.w;
		string text = _dataFilePrefix + "_x" + fileIndex.x + "_y" + fileIndex.z + ((w == 0) ? ".tmp" : ("_" + w + ".tmp"));
		string text2 = _dataFilePrefix + "_x" + fileIndex.x + "_y" + fileIndex.z + ((w == 0) ? ".voxelform" : ("_" + w + ".voxelform"));
		using (FileStream fileStream = new FileStream(text2, FileMode.Open, FileAccess.Read, FileShare.Read))
		{
			using FileStream fileStream2 = new FileStream(text, FileMode.Create);
			int num = (48 >> w) * (48 >> w) * (23 >> w);
			int num2 = num * 4;
			byte[] array = new byte[num2];
			fileStream.Read(array, 0, num2);
			byte[] array2 = new byte[num2];
			fileStream2.Seek(num2, SeekOrigin.Begin);
			List<IntVector4> source = piece2chunkList.Keys.Cast<IntVector4>().ToList();
			List<IntVector4> list = source.OrderBy((IntVector4 p) => VFFileUtil.PiecePos2IndexInFile(p.x, p.y, p.z, p.w)).ToList();
			int num3 = 0;
			int i = 0;
			int num4 = 0;
			int num5 = 0;
			int num6 = 0;
			byte[] array3 = new byte[5510208];
			int count = list.Count;
			for (int j = 0; j < count; j++)
			{
				IntVector4 intVector = list[j];
				int num7;
				for (num7 = VFFileUtil.PiecePos2IndexInFile(intVector.x, intVector.y, intVector.z, intVector.w); i < num7; i++)
				{
					num3 = i * 4;
					num4 = array[num3] + (array[num3 + 1] << 8) + (array[num3 + 2] << 16) + (array[num3 + 3] << 24);
					num5 = ((num3 < num2 - 4) ? (array[num3 + 4] + (array[num3 + 5] << 8) + (array[num3 + 6] << 16) + (array[num3 + 7] << 24) - num4) : ((int)fileStream.Length - num4));
					fileStream.Seek(num4, SeekOrigin.Begin);
					fileStream.Read(array3, 0, num5);
					num6 = (int)fileStream2.Position;
					array2[num3] = (byte)(num6 & 0xFF);
					array2[num3 + 1] = (byte)((num6 >> 8) & 0xFF);
					array2[num3 + 2] = (byte)((num6 >> 16) & 0xFF);
					array2[num3 + 3] = (byte)((num6 >> 24) & 0xFF);
					fileStream2.Write(array3, 0, num5);
				}
				ReplaceChunksInPiece(piece2chunkList[intVector], mergeChunkDataAtPos, intVector);
				num6 = (int)fileStream2.Position;
				num3 = num7 * 4;
				array2[num3] = (byte)(num6 & 0xFF);
				array2[num3 + 1] = (byte)((num6 >> 8) & 0xFF);
				array2[num3 + 2] = (byte)((num6 >> 16) & 0xFF);
				array2[num3 + 3] = (byte)((num6 >> 24) & 0xFF);
				fileStream2.Write(_buff.zippedDataBuffer, 0, _buff.zippedDataLen);
				i++;
			}
			for (; i < num; i++)
			{
				num3 = i * 4;
				num4 = array[num3] + (array[num3 + 1] << 8) + (array[num3 + 2] << 16) + (array[num3 + 3] << 24);
				num5 = ((num3 < num2 - 4) ? (array[num3 + 4] + (array[num3 + 5] << 8) + (array[num3 + 6] << 16) + (array[num3 + 7] << 24) - num4) : ((int)fileStream.Length - num4));
				fileStream.Seek(num4, SeekOrigin.Begin);
				fileStream.Read(array3, 0, num5);
				num6 = (int)fileStream2.Position;
				array2[num3] = (byte)(num6 & 0xFF);
				array2[num3 + 1] = (byte)((num6 >> 8) & 0xFF);
				array2[num3 + 2] = (byte)((num6 >> 16) & 0xFF);
				array2[num3 + 3] = (byte)((num6 >> 24) & 0xFF);
				fileStream2.Write(array3, 0, num5);
			}
			fileStream2.Seek(0L, SeekOrigin.Begin);
			fileStream2.Write(array2, 0, num2);
		}
		VFFileUtil.CloseAllFiles(ref _curFileStream, ref _fileStreams, ref _fileLens);
		string destFileName = string.Format(text2 + ".{0:MMdd_hhmmss}", DateTime.Now);
		File.Move(text2, destFileName);
		File.Move(text, text2);
	}

	public void ReplaceChunksInPiece(List<IntVector4> chunkPosList, ProcToMergeChunkAtPos mergeChunkDataAtPos, IntVector4 piecePos)
	{
		ReadPieceDataToBuff(piecePos.x, piecePos.y, piecePos.z, piecePos.w);
		_buff.Decompress(piecePos.x, piecePos.y, piecePos.z, piecePos.w);
		List<IntVector4> list = chunkPosList.OrderBy((IntVector4 pos) => VFFileUtil.ChunkPos2IndexInPiece(pos)).ToList();
		byte[] unzippedDataBuffer = _buff.unzippedDataBuffer;
		int unzippedDataLen = _buff.unzippedDataLen;
		byte[] array = null;
		if (_buff.IsHollow())
		{
			array = new byte[_buff.zippedDataLen];
			Array.Copy(_buff.zippedDataBuffer, array, _buff.zippedDataLen);
		}
		int num = 63;
		int i = 0;
		List<byte[]> list2 = new List<byte[]>();
		byte[] array2 = null;
		int num2 = 0;
		int count = list.Count;
		for (int j = 0; j < count; j++)
		{
			IntVector4 chunkPos = list[j];
			int num3;
			for (num3 = VFFileUtil.ChunkPos2IndexInPiece(chunkPos); i <= num3; i++)
			{
				if (_buff.IsHollow())
				{
					list2.Add(array);
					num2 += array.Length;
					continue;
				}
				int num4 = 4 * i;
				int num5 = unzippedDataBuffer[num4] + (unzippedDataBuffer[num4 + 1] << 8) + (unzippedDataBuffer[num4 + 2] << 16) + (unzippedDataBuffer[num4 + 3] << 24);
				int num6 = ((i >= num) ? unzippedDataLen : (unzippedDataBuffer[num4 + 4] + (unzippedDataBuffer[num4 + 5] << 8) + (unzippedDataBuffer[num4 + 6] << 16) + (unzippedDataBuffer[num4 + 7] << 24)));
				int num7 = num6 - num5;
				array2 = new byte[num7];
				Array.Copy(unzippedDataBuffer, num5, array2, 0, num7);
				list2.Add(array2);
				num2 += num7;
			}
			num2 -= list2[num3].Length;
			list2[num3] = mergeChunkDataAtPos(chunkPos, list2[num3]);
			num2 += list2[num3].Length;
		}
		for (; i <= num; i++)
		{
			if (_buff.IsHollow())
			{
				list2.Add(array);
				num2 += array.Length;
				continue;
			}
			int num8 = 4 * i;
			int num9 = unzippedDataBuffer[num8] + (unzippedDataBuffer[num8 + 1] << 8) + (unzippedDataBuffer[num8 + 2] << 16) + (unzippedDataBuffer[num8 + 3] << 24);
			int num10 = ((i >= num) ? unzippedDataLen : (unzippedDataBuffer[num8 + 4] + (unzippedDataBuffer[num8 + 5] << 8) + (unzippedDataBuffer[num8 + 6] << 16) + (unzippedDataBuffer[num8 + 7] << 24)));
			int num11 = num10 - num9;
			array2 = new byte[num11];
			Array.Copy(unzippedDataBuffer, num9, array2, 0, num11);
			list2.Add(array2);
			num2 += num11;
		}
		int num12 = 256;
		_buff.unzippedDataLen = num12 + num2;
		byte[] unzippedDataBuffer2 = _buff.unzippedDataBuffer;
		int num13 = num12;
		for (int k = 0; k < 64; k++)
		{
			byte[] array3 = list2[k];
			Array.Copy(array3, 0, unzippedDataBuffer2, num13, array3.Length);
			unzippedDataBuffer2[k * 4] = (byte)(num13 & 0xFF);
			unzippedDataBuffer2[k * 4 + 1] = (byte)((num13 >> 8) & 0xFF);
			unzippedDataBuffer2[k * 4 + 2] = (byte)((num13 >> 16) & 0xFF);
			unzippedDataBuffer2[k * 4 + 3] = (byte)((num13 >> 24) & 0xFF);
			num13 += array3.Length;
		}
		_buff.zippedDataLen = LZ4.LZ4_compress(_buff.unzippedDataBuffer, _buff.zippedDataBuffer, _buff.unzippedDataLen);
	}

	public VFVoxelChunkData ReadChunkImm(IntVector4 cpos)
	{
		VFVoxelChunkData vFVoxelChunkData = new VFVoxelChunkData(null);
		vFVoxelChunkData.ChunkPosLod_w = cpos;
		if (cpos.x < 0 || cpos.x >= 576 || cpos.y < 0 || cpos.y >= VoxelTerrainConstants.WorldMaxCY(cpos.w) || cpos.z < 0 || cpos.z >= 576)
		{
			vFVoxelChunkData.OnDataLoaded(VFVoxelChunkData.S_ChunkDataAir);
			return vFVoxelChunkData;
		}
		VFFileUtil.WorldChunkPosToPiecePos(cpos, out var px, out var py, out var pz);
		ReadPieceDataToBuff(px, py, pz, cpos.w);
		_buff.Decompress(px, py, pz, cpos.w);
		_buff.SetChunkData(vFVoxelChunkData, _chunkDataProc);
		return vFVoxelChunkData;
	}

	public static byte[] MinusChunkData(byte[] chunk1, byte[] chunk2, int lod)
	{
		if (chunk1.Length == 2 && chunk1[0] == 0)
		{
			return chunk1;
		}
		if (chunk2.Length == 2)
		{
			if (chunk2[0] == 0)
			{
				return chunk1;
			}
			if (chunk2[0] == byte.MaxValue)
			{
				return new byte[2];
			}
			Debug.LogWarning("Unrecognized chunk:" + chunk2[0] + "," + chunk2[1]);
			return new byte[2];
		}
		byte[] array = new byte[85750];
		if (chunk1.Length != 2)
		{
			for (int i = 0; i < 85750; i += 2)
			{
				if (chunk2[i] >= byte.MaxValue)
				{
					continue;
				}
				array[i] = chunk1[i];
				array[i + 1] = chunk1[i + 1];
				if (chunk1[i] < 192)
				{
					int num = i - 70;
					if (num >= 0)
					{
						array[num] = chunk1[num];
						array[num + 1] = chunk1[num + 1];
					}
				}
			}
		}
		else if (chunk1[0] == byte.MaxValue)
		{
			byte b = chunk1[1];
			for (int j = 0; j < 85750; j += 2)
			{
				if (chunk2[j] < byte.MaxValue)
				{
					array[j] = byte.MaxValue;
					array[j + 1] = b;
				}
			}
		}
		else
		{
			if (chunk1[0] != 128)
			{
				Debug.LogWarning("Unrecognized chunk:" + chunk1[0] + "," + chunk1[1]);
				return chunk1;
			}
			float num2 = VFVoxelWater.c_fWaterLvl / (float)(1 << lod);
			int num3 = (int)(num2 + 0.5f);
			float num4 = num2 - (float)(int)num2;
			int num5 = (num3 & 0x1F) + 1;
			byte b2 = ((!(num4 < 0.5f)) ? ((byte)(255.999f * (1f - 0.5f / num4))) : ((byte)(128f / (1f - num4))));
			int num6 = 0;
			int num7 = (35 - num5 - 1) * 70;
			for (int k = 0; k < 35; k++)
			{
				for (int l = 0; l < num5; l++)
				{
					for (int m = 0; m < 35; m++)
					{
						if (chunk2[num6] < byte.MaxValue)
						{
							array[num6] = byte.MaxValue;
							array[num6 + 1] = 128;
						}
						num6 += 2;
					}
				}
				for (int n = 0; n < 35; n++)
				{
					if (chunk2[num6] < byte.MaxValue)
					{
						array[num6] = b2;
						array[num6 + 1] = 128;
					}
					num6 += 2;
				}
				num6 += num7;
			}
		}
		return array;
	}

	public static byte[] MergeChunkData(byte[] chunk1, byte[] chunk2, int lod)
	{
		if (chunk1.Length != chunk2.Length)
		{
			byte[] array;
			byte[] array2;
			if (chunk1.Length < chunk2.Length)
			{
				array = chunk1;
				array2 = chunk2;
			}
			else
			{
				array = chunk2;
				array2 = chunk1;
			}
			if (array[0] == 0)
			{
				return array2;
			}
			if (array[0] == byte.MaxValue)
			{
				return array;
			}
			if (array[0] != 128)
			{
				Debug.LogWarning("Unrecognized chunk:" + array[0] + "," + array[1]);
				return array;
			}
			float num = VFVoxelWater.c_fWaterLvl / (float)(1 << lod);
			int num2 = (int)(num + 0.5f);
			float num3 = num - (float)(int)num;
			int num4 = (num2 & 0x1F) + 1;
			byte b = ((!(num3 < 0.5f)) ? ((byte)(255.999f * (1f - 0.5f / num3))) : ((byte)(128f / (1f - num3))));
			byte[] array3 = new byte[85750];
			Array.Copy(array2, array3, 85750);
			int num5 = 0;
			int num6 = (35 - num4 - 1) * 70;
			for (int i = 0; i < 35; i++)
			{
				for (int j = 0; j < num4; j++)
				{
					for (int k = 0; k < 35; k++)
					{
						array3[num5++] = byte.MaxValue;
						array3[num5++] = 128;
					}
				}
				for (int l = 0; l < 35; l++)
				{
					if (array2[num5] > b)
					{
						num5++;
						num5++;
					}
					else
					{
						array3[num5++] = b;
						array3[num5++] = 128;
					}
				}
				num5 += num6;
			}
			return array3;
		}
		int num7 = chunk1.Length;
		byte[] array4 = new byte[num7];
		Array.Copy(chunk1, array4, num7);
		for (int m = 0; m < num7; m += 2)
		{
			if (array4[m] < chunk2[m])
			{
				array4[m] = chunk2[m];
				array4[m + 1] = chunk2[m + 1];
			}
		}
		return array4;
	}
}

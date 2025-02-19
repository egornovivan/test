using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using Pathea;
using UnityEngine;

public class VFVoxelSave
{
	private static readonly int s_ver = 1606230;

	private static List<VFVoxelSave> s_VoxelSaveList = new List<VFVoxelSave>();

	private string _archiveKey = string.Empty;

	private Action<BinaryReader> _addtionalReader;

	private Action<BinaryWriter> _addtionalWriter;

	private List<VFVoxelChunkData> _chunkListToSave = new List<VFVoxelChunkData>();

	private Dictionary<IntVector4, long> _modifiedChunksInfo = new Dictionary<IntVector4, long>();

	private FileStream _tmpVoxelFileStream;

	public Dictionary<IntVector4, long> modifiedChunksInfoDic => _modifiedChunksInfo;

	public List<VFVoxelChunkData> ChunkSaveList => _chunkListToSave;

	public VFVoxelSave(string archiveKey, Action<BinaryReader> reader = null, Action<BinaryWriter> writer = null)
	{
		int count = s_VoxelSaveList.Count;
		_tmpVoxelFileStream = OpenTmpVoxelFileStream(count);
		s_VoxelSaveList.Add(this);
		_archiveKey = archiveKey;
		_addtionalReader = reader;
		_addtionalWriter = writer;
	}

	private FileStream OpenTmpVoxelFileStream(int index)
	{
		string tempPath = Path.GetTempPath();
		try
		{
			return new FileStream(tempPath + "/voxel" + index + ".tmp", FileMode.Create, FileAccess.ReadWrite, FileShare.ReadWrite);
		}
		catch (Exception ex)
		{
			Debug.LogError("Create tmp file error!" + ex);
			return null;
		}
	}

	private void CloseTmpVoxelFileStream()
	{
		if (_tmpVoxelFileStream != null)
		{
			_tmpVoxelFileStream.Close();
		}
	}

	public void Import(PeRecordReader r)
	{
		if (r == null || _tmpVoxelFileStream == null || !r.Open())
		{
			return;
		}
		BinaryReader binaryReader = r.binaryReader;
		int num = binaryReader.ReadInt32();
		if (num != s_ver)
		{
			Debug.LogError("[VoxelSave]:Error version:" + num + "|" + s_ver);
			r.Close();
			return;
		}
		int num2 = binaryReader.ReadInt32();
		byte[] array = VFVoxelChunkData.s_ChunkDataPool.Get();
		BinaryWriter binaryWriter = new BinaryWriter(_tmpVoxelFileStream);
		_tmpVoxelFileStream.Seek(0L, SeekOrigin.Begin);
		binaryWriter.Write(num);
		binaryWriter.Write(num2);
		for (int i = 0; i < num2; i++)
		{
			binaryReader.Read(array, 0, 85750);
			_tmpVoxelFileStream.Write(array, 0, 85750);
		}
		VFVoxelChunkData.s_ChunkDataPool.Free(array);
		_modifiedChunksInfo.Clear();
		for (int j = 0; j < num2; j++)
		{
			int x_ = binaryReader.ReadInt32();
			int y_ = binaryReader.ReadInt32();
			int z_ = binaryReader.ReadInt32();
			int w_ = binaryReader.ReadInt32();
			long value = binaryReader.ReadInt64();
			_modifiedChunksInfo.Add(new IntVector4(x_, y_, z_, w_), value);
		}
		if (_addtionalReader != null)
		{
			_addtionalReader(binaryReader);
		}
		r.Close();
	}

	public void Export(PeRecordWriter w)
	{
		if (_tmpVoxelFileStream == null)
		{
			return;
		}
		if (!GameConfig.IsMultiMode)
		{
			SaveChunksInListToTmpFile();
		}
		BinaryWriter binaryWriter = w.binaryWriter;
		if (binaryWriter == null)
		{
			Debug.LogError("On WriteRecord FileStream is null!");
			return;
		}
		byte[] array = VFVoxelChunkData.s_ChunkDataPool.Get();
		int count = _modifiedChunksInfo.Count;
		binaryWriter.Write(s_ver);
		binaryWriter.Write(count);
		_tmpVoxelFileStream.Seek(8L, SeekOrigin.Begin);
		for (int i = 0; i < count; i++)
		{
			_tmpVoxelFileStream.Read(array, 0, 85750);
			binaryWriter.Write(array, 0, 85750);
		}
		VFVoxelChunkData.s_ChunkDataPool.Free(array);
		foreach (KeyValuePair<IntVector4, long> item in _modifiedChunksInfo)
		{
			IntVector4 key = item.Key;
			binaryWriter.Write(key.x);
			binaryWriter.Write(key.y);
			binaryWriter.Write(key.z);
			binaryWriter.Write(key.w);
			binaryWriter.Write(item.Value);
		}
		if (_addtionalWriter != null)
		{
			_addtionalWriter(binaryWriter);
		}
	}

	public void SaveChunkListImmediately(List<VFVoxelChunkData> vcds)
	{
		int count = vcds.Count;
		for (int i = 0; i < count; i++)
		{
			AddChunkToSaveList(vcds[i]);
		}
		if (GameConfig.IsMultiMode)
		{
			return;
		}
		for (int j = 0; j < _chunkListToSave.Count; j++)
		{
			if (_chunkListToSave[j] != null)
			{
				SaveChunkToTmpFile(_chunkListToSave[j]);
			}
		}
		_chunkListToSave.Clear();
	}

	public byte[] TryGetChunkData(IntVector4 cpos, bool useChunkDataPool = true)
	{
		if (_tmpVoxelFileStream == null)
		{
			return null;
		}
		long value = 0L;
		if (_modifiedChunksInfo.TryGetValue(cpos, out value))
		{
			_tmpVoxelFileStream.Seek(value, SeekOrigin.Begin);
			byte[] array = ((!useChunkDataPool) ? new byte[85750] : VFVoxelChunkData.s_ChunkDataPool.Get());
			_tmpVoxelFileStream.Read(array, 0, 85750);
			return array;
		}
		return null;
	}

	public void AddChunkToSaveList(VFVoxelChunkData vc)
	{
		if (!_chunkListToSave.Contains(vc))
		{
			_chunkListToSave.Add(vc);
		}
		PeSingleton<ArchiveMgr>.Instance.SaveMe(_archiveKey);
	}

	public bool SaveChunkToTmpFile(VFVoxelChunkData chunk)
	{
		if (chunk.DataVT.Length == 0)
		{
			Debug.LogError("[SaveChunk]Data is null:" + chunk.ChunkPosLod);
			return false;
		}
		if (chunk.IsHollow)
		{
			Debug.LogError("[SaveChunk]Data is Hollow:" + chunk.ChunkPosLod);
			return false;
		}
		long value = 0L;
		if (!_modifiedChunksInfo.TryGetValue(chunk.ChunkPosLod, out value))
		{
			value = (_modifiedChunksInfo[new IntVector4(chunk.ChunkPosLod)] = _tmpVoxelFileStream.Length);
		}
		_tmpVoxelFileStream.Seek(value, SeekOrigin.Begin);
		_tmpVoxelFileStream.Write(chunk.DataVT, 0, chunk.DataVT.Length);
		return true;
	}

	public void SaveChunksInListToTmpFile()
	{
		for (int i = 0; i < _chunkListToSave.Count; i++)
		{
			if (_chunkListToSave[i] != null)
			{
				SaveChunkToTmpFile(_chunkListToSave[i]);
			}
		}
		_chunkListToSave.Clear();
	}

	public static IEnumerator CoSaveAllChunksInList()
	{
		int n = s_VoxelSaveList.Count;
		for (int i = 0; i < n; i++)
		{
			s_VoxelSaveList[i].SaveChunksInListToTmpFile();
		}
		yield return 0;
	}

	public static void SaveAllChunksInList()
	{
		int count = s_VoxelSaveList.Count;
		for (int i = 0; i < count; i++)
		{
			s_VoxelSaveList[i].SaveChunksInListToTmpFile();
		}
	}

	public static void Clean()
	{
		s_VoxelSaveList.Clear();
	}
}

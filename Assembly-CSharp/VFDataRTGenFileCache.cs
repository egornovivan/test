using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using UnityEngine;

public class VFDataRTGenFileCache
{
	public const int MagicCode = 16102000;

	public const int MagicCodeLen = 4;

	public const int MaxCacheFiles = 10;

	private string _cacheFilePathName;

	private Dictionary<IntVector4, VFTerTileCacheDesc> _voxelTileCacheDescsList = new Dictionary<IntVector4, VFTerTileCacheDesc>();

	private FileStream _voxelTileCachesFS;

	private BinaryReader _br;

	private BinaryWriter _bw;

	public VFDataRTGenFileCache(string cacheFilePathName)
	{
		_cacheFilePathName = cacheFilePathName;
		if (SystemSettingData.Instance.VoxelCacheEnabled)
		{
			LoadVoxelTileCacheDescs();
		}
	}

	public static string GetCacheFilePathName(string strSeed)
	{
		MD5 mD = new MD5CryptoServiceProvider();
		byte[] array = mD.ComputeHash(Encoding.UTF8.GetBytes(strSeed));
		string text = BitConverter.ToString(array);
		text = text.Replace("-", string.Empty);
		return GameConfig.VoxelCacheDataPath + text;
	}

	public static void ClearAllCache()
	{
		if (!Directory.Exists(GameConfig.VoxelCacheDataPath))
		{
			return;
		}
		string[] files = Directory.GetFiles(GameConfig.VoxelCacheDataPath);
		int num = files.Length;
		for (int i = 0; i < num; i++)
		{
			try
			{
				File.Delete(files[i]);
			}
			catch (Exception ex)
			{
				Debug.LogWarning("[VoxelCache]Failed to delete an invalid cache file:" + files[i] + ex);
			}
		}
	}

	private void ClearInvalidCache()
	{
		if (!Directory.Exists(GameConfig.VoxelCacheDataPath))
		{
			return;
		}
		string[] files = Directory.GetFiles(GameConfig.VoxelCacheDataPath);
		int num = files.Length;
		for (int i = 0; i < num; i++)
		{
			string text = files[i];
			try
			{
				bool flag = false;
				using (FileStream fileStream = new FileStream(text, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
				{
					BinaryReader binaryReader = new BinaryReader(fileStream);
					if (fileStream.Length < 4 || binaryReader.ReadInt32() != 16102000)
					{
						flag = true;
					}
				}
				if (flag)
				{
					File.Delete(text);
				}
			}
			catch (Exception ex)
			{
				Debug.LogWarning("[VoxelCache]Failed to delete an invalid cache file:" + text + ex);
			}
		}
	}

	private void LoadVoxelTileCacheDescs()
	{
		if (_voxelTileCachesFS != null)
		{
			_voxelTileCachesFS.Close();
		}
		try
		{
			if (!Directory.Exists(GameConfig.VoxelCacheDataPath))
			{
				Directory.CreateDirectory(GameConfig.VoxelCacheDataPath);
			}
			_voxelTileCachesFS = new FileStream(_cacheFilePathName, FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.ReadWrite);
			_br = new BinaryReader(_voxelTileCachesFS);
			_bw = new BinaryWriter(_voxelTileCachesFS);
			long length = _voxelTileCachesFS.Length;
			if (length < 4 || _br.ReadInt32() != 16102000)
			{
				_voxelTileCachesFS.SetLength(0L);
				_bw.Write(16102000);
				_bw.Flush();
				ClearInvalidCache();
				length = _voxelTileCachesFS.Length;
			}
			VFTerTileCacheDesc vFTerTileCacheDesc;
			for (long num = _voxelTileCachesFS.Position; num < length; num += vFTerTileCacheDesc.dataLen + 24)
			{
				_voxelTileCachesFS.Seek(num, SeekOrigin.Begin);
				vFTerTileCacheDesc = VFTerTileCacheDesc.ReadDescFromCache(_br);
				if (vFTerTileCacheDesc.xzlh.w == VFDataRTGen.s_noiseHeight && vFTerTileCacheDesc.dataLen > 0)
				{
					_voxelTileCacheDescsList.Add(vFTerTileCacheDesc.xzlh, vFTerTileCacheDesc);
					continue;
				}
				Debug.LogWarning(string.Concat("[VFDataRTGen]Error: Unrecognized voxel tile,discard the following data.", vFTerTileCacheDesc.xzlh, ":", vFTerTileCacheDesc.dataLen));
				_voxelTileCachesFS.SetLength(num);
				length = _voxelTileCachesFS.Length;
			}
		}
		catch (Exception ex)
		{
			Debug.LogWarning("[VFDataRTGen]Error:Failed to open/create voxel cache file:" + ex);
			Close();
		}
	}

	public void SaveDataToFileCaches(int bitMask, VFTile tile, double[][] nData, float[][] hData, float[][] gData, RandomMapType[][] tData)
	{
		if (_voxelTileCachesFS == null)
		{
			return;
		}
		IntVector4 intVector = new IntVector4(tile.tileX, tile.tileZ, tile.tileL, tile.tileH);
		if (_voxelTileCacheDescsList.ContainsKey(intVector))
		{
			Debug.LogWarning("[VFDataRTGen]:Try to append a existing voxel tile to cache file." + intVector);
			return;
		}
		try
		{
			_voxelTileCachesFS.Seek(0L, SeekOrigin.End);
			VFTerTileCacheDesc value = VFTerTileCacheDesc.WriteDataToCache(_bw, bitMask, tile, nData, hData, gData, tData);
			_voxelTileCacheDescsList.Add(intVector, value);
		}
		catch (Exception ex)
		{
			string text = ex.ToString();
			Debug.LogWarning(string.Concat("[VFDataRTGen]:Failed to append voxel tile to cache file", intVector, ex));
			if (text.Contains("IOException: Win32 IO returned 112."))
			{
				GameLog.HandleExceptionInThread(ex);
			}
		}
	}

	public void Close()
	{
		if (_voxelTileCachesFS != null)
		{
			try
			{
				_br.Close();
				_bw.Close();
				_voxelTileCachesFS.Close();
			}
			catch
			{
				Debug.LogWarning("[VFDataRTGen]Error:Failed to close the opening voxel cache file.");
			}
			_voxelTileCachesFS = null;
		}
	}

	public VFTerTileCacheDesc FillTileDataWithFileCache(IntVector4 xzlh, VFTile tile, double[][] nData, float[][] hData, float[][] gData, RandomMapType[][] tData)
	{
		VFTerTileCacheDesc value = null;
		if (_voxelTileCacheDescsList.TryGetValue(xzlh, out value))
		{
			try
			{
				value.ReadDataFromCache(_br, tile, nData, hData, gData, tData);
			}
			catch
			{
				Debug.LogWarning("[VFDataRTGen]Error:Failed to read data from cache " + xzlh);
				return null;
			}
		}
		return value;
	}
}

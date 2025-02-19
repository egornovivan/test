using System;
using System.IO;
using UnityEngine;

namespace Pathea;

internal class SwapSpace
{
	public delegate bool NeedCopy(FileInfo fileInfo);

	private DirectoryInfo mDirInfo;

	public void Init(string dir = null)
	{
		if (string.IsNullOrEmpty(dir))
		{
			string tempPath = Path.GetTempPath();
			Debug.Log("<color=aqua>temp path:" + tempPath + "</color>");
			dir = Path.Combine(tempPath, "planet_explorers_swap");
		}
		try
		{
			DirectoryInfo directoryInfo = new DirectoryInfo(dir);
			if (directoryInfo.Exists)
			{
				FileSystemInfo[] fileSystemInfos = directoryInfo.GetFileSystemInfos();
				if (fileSystemInfos == null || fileSystemInfos.Length == 0)
				{
					mDirInfo = directoryInfo;
					return;
				}
				Directory.Delete(dir, recursive: true);
			}
			mDirInfo = Directory.CreateDirectory(dir);
		}
		catch (Exception ex)
		{
			GameLog.HandleIOException(ex);
		}
	}

	public bool CopyTo(string dirDst, Action<FileInfo> action = null)
	{
		try
		{
			CopyDir(mDirInfo, new DirectoryInfo(dirDst), action);
			return true;
		}
		catch (Exception ex)
		{
			GameLog.HandleIOException(ex);
			return false;
		}
	}

	public bool CopyFrom(string dirSrc, NeedCopy needCopy = null)
	{
		if (!Directory.Exists(dirSrc))
		{
			return false;
		}
		try
		{
			CopyDir(new DirectoryInfo(dirSrc), mDirInfo, null, needCopy);
			return true;
		}
		catch (Exception ex)
		{
			GameLog.HandleIOException(ex);
			return false;
		}
	}

	private static void CopyDir(DirectoryInfo dirSrc, DirectoryInfo dirDst, Action<FileInfo> action = null, NeedCopy needCopy = null)
	{
		FileInfo[] files = dirSrc.GetFiles();
		DirectoryInfo[] directories = dirSrc.GetDirectories();
		if ((files == null || files.Length == 0) && (directories == null || directories.Length == 0))
		{
			return;
		}
		if (!dirDst.Exists)
		{
			dirDst.Create();
		}
		FileInfo[] array = files;
		foreach (FileInfo fileInfo in array)
		{
			if (needCopy == null || needCopy(fileInfo))
			{
				FileInfo obj = fileInfo.CopyTo(Path.Combine(dirDst.FullName, fileInfo.Name), overwrite: true);
				action?.Invoke(obj);
			}
		}
		DirectoryInfo[] array2 = directories;
		foreach (DirectoryInfo directoryInfo in array2)
		{
			CopyDir(directoryInfo, new DirectoryInfo(Path.Combine(dirDst.FullName, directoryInfo.Name)), action, needCopy);
		}
	}
}

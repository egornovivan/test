using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace Pathea;

public class Archive
{
	public class Header
	{
		public const int Version_0 = 0;

		public const int Version_1 = 1;

		public const int Version_2 = 2;

		public const int Version_3 = 3;

		public const int Version_4 = 4;

		public const int Version_5 = 5;

		private const int CurrentVersion = 5;

		private const int VerLength = 4;

		private const int GuidLength = 16;

		private const int VerGuidLength = 20;

		private int mVersion = 5;

		private Guid mGuid;

		public int version => mVersion;

		private static Guid GenerateUid()
		{
			return Guid.NewGuid();
		}

		public void NewGuid()
		{
			mGuid = GenerateUid();
		}

		public void Write(BinaryWriter w)
		{
			try
			{
				w.Seek(0, SeekOrigin.Begin);
				w.Write(5);
				w.Write(mGuid.ToByteArray());
			}
			catch (Exception ex)
			{
				GameLog.HandleIOException(ex);
			}
		}

		public void BeginWriteCheckSum(BinaryWriter w)
		{
			try
			{
				w.Seek(20, SeekOrigin.Begin);
				w.Write(0L);
			}
			catch (Exception ex)
			{
				GameLog.HandleIOException(ex);
			}
		}

		public void EndWriteCheckSum(BinaryWriter w)
		{
			try
			{
				w.Seek(20, SeekOrigin.Begin);
				w.Write(w.BaseStream.Length);
			}
			catch (Exception ex)
			{
				GameLog.HandleIOException(ex);
			}
		}

		public bool Read(BinaryReader r)
		{
			try
			{
				mVersion = r.ReadInt32();
				if (mVersion != 5)
				{
					return false;
				}
				byte[] b = r.ReadBytes(16);
				mGuid = new Guid(b);
				long num = r.ReadInt64();
				if (num != r.BaseStream.Length)
				{
					Debug.LogError("Error check sum");
					return false;
				}
				return true;
			}
			catch (Exception ex)
			{
				GameLog.HandleIOException(ex);
				return false;
			}
		}

		public bool IsMatch(Header other)
		{
			if (other == null)
			{
				return false;
			}
			return 0 == mGuid.CompareTo(other.mGuid);
		}
	}

	private class ArchiveIndexFile
	{
		private const string IndexFileName = "index";

		private const int VERSION_0000 = 0;

		private const int CURRENT_VERSION = 0;

		private Dictionary<string, ArchiveIndex> mDicIndex = new Dictionary<string, ArchiveIndex>(10);

		private Dictionary<string, ArchiveIndex> mYirdDicIndex = new Dictionary<string, ArchiveIndex>(10);

		private string mYirdName;

		private Header mHeader;

		public Header header
		{
			get
			{
				return mHeader;
			}
			set
			{
				mHeader = value;
			}
		}

		public string yirdName
		{
			get
			{
				return mYirdName;
			}
			set
			{
				mYirdName = value;
			}
		}

		private static string GetIndexFilePath(string dir)
		{
			return GetFilePath(dir, "index");
		}

		public string GetYirdDir(string dir)
		{
			return Path.Combine(dir, yirdName);
		}

		public void Add(string key, ArchiveIndex index)
		{
			if (index.yird)
			{
				mYirdDicIndex.Add(key, index);
			}
			else
			{
				mDicIndex.Add(key, index);
			}
		}

		public ArchiveIndex GetArchiveIndex(string key)
		{
			if (mDicIndex.ContainsKey(key))
			{
				return mDicIndex[key];
			}
			if (mYirdDicIndex.ContainsKey(key))
			{
				return mYirdDicIndex[key];
			}
			return null;
		}

		public bool Write(string dir)
		{
			if (!Write(mHeader, GetIndexFilePath(dir), mDicIndex))
			{
				return false;
			}
			if (!Write(mHeader, GetIndexFilePath(GetYirdDir(dir)), mYirdDicIndex))
			{
				return false;
			}
			return true;
		}

		private static bool Write(Header header, string path, Dictionary<string, ArchiveIndex> dic)
		{
			try
			{
				using FileStream output = new FileStream(path, FileMode.Create, FileAccess.Write);
				using BinaryWriter binaryWriter = new BinaryWriter(output);
				header.Write(binaryWriter);
				header.BeginWriteCheckSum(binaryWriter);
				binaryWriter.Write(dic.Count);
				foreach (KeyValuePair<string, ArchiveIndex> item in dic)
				{
					binaryWriter.Write(item.Key);
					item.Value.Write(binaryWriter);
				}
				header.EndWriteCheckSum(binaryWriter);
			}
			catch (Exception message)
			{
				Debug.LogWarning(message);
				return false;
			}
			return true;
		}

		private bool Load(string dir)
		{
			string indexFilePath = GetIndexFilePath(dir);
			header = Load(indexFilePath, mDicIndex);
			return header != null;
		}

		public bool LoadYird(string dir, string yirdName)
		{
			if (string.IsNullOrEmpty(yirdName))
			{
				return false;
			}
			mYirdName = yirdName;
			string indexFilePath = GetIndexFilePath(GetYirdDir(dir));
			Header header = Load(indexFilePath, mYirdDicIndex, this.header);
			if (header == null)
			{
				return false;
			}
			return true;
		}

		private static Header Load(string path, Dictionary<string, ArchiveIndex> dic, Header curHeader = null)
		{
			if (!File.Exists(path))
			{
				return null;
			}
			try
			{
				using FileStream input = new FileStream(path, FileMode.Open, FileAccess.Read);
				using BinaryReader binaryReader = new BinaryReader(input);
				Header header = new Header();
				if (!header.Read(binaryReader))
				{
					return null;
				}
				if (curHeader != null && !curHeader.IsMatch(header))
				{
					return null;
				}
				dic.Clear();
				int num = binaryReader.ReadInt32();
				for (int i = 0; i < num; i++)
				{
					string key = binaryReader.ReadString();
					ArchiveIndex archiveIndex = new ArchiveIndex();
					archiveIndex.Read(binaryReader);
					dic.Add(key, archiveIndex);
				}
				return header;
			}
			catch (Exception message)
			{
				Debug.LogWarning(message);
				return null;
			}
		}

		public static ArchiveIndexFile LoadFromFile(string dir, string yirdName = null)
		{
			ArchiveIndexFile archiveIndexFile = new ArchiveIndexFile();
			if (!archiveIndexFile.Load(dir))
			{
				return null;
			}
			archiveIndexFile.LoadYird(dir, yirdName);
			return archiveIndexFile;
		}
	}

	public delegate bool SaveRecordObj(ArchiveObj obj);

	public const string FileExtention = "arc";

	private ArchiveIndexFile mIndexFile;

	private string mDir;

	public string dir => mDir;

	public Archive(string dir)
	{
		mDir = dir;
	}

	public byte[] GetData(string key)
	{
		return GetReader(key)?.ReadBytesDirect();
	}

	public bool LoadYird(string yirdName)
	{
		if (mIndexFile == null)
		{
			return false;
		}
		return mIndexFile.LoadYird(dir, yirdName);
	}

	public Header GetHeader()
	{
		if (mIndexFile == null)
		{
			return null;
		}
		return mIndexFile.header;
	}

	public PeRecordReader GetReader(string key)
	{
		if (mIndexFile == null)
		{
			return null;
		}
		ArchiveIndex archiveIndex = mIndexFile.GetArchiveIndex(key);
		if (archiveIndex == null)
		{
			return null;
		}
		string yirdDir;
		if (archiveIndex.yird)
		{
			if (string.IsNullOrEmpty(mIndexFile.yirdName))
			{
				Debug.LogError("yird name is empty or null");
				return null;
			}
			yirdDir = mIndexFile.GetYirdDir(dir);
		}
		else
		{
			yirdDir = dir;
		}
		return new PeRecordReader(archiveIndex, yirdDir, mIndexFile.header);
	}

	public static string GetFilePath(string dir, string name)
	{
		return Path.Combine(dir, name + ".arc");
	}

	public bool WriteToFile(ArchiveObj.List serializeObjList, string yirdName, Header header, SaveRecordObj saveRecordObj)
	{
		try
		{
			if (!Directory.Exists(dir))
			{
				Directory.CreateDirectory(dir);
			}
			mIndexFile = new ArchiveIndexFile();
			mIndexFile.yirdName = yirdName;
			mIndexFile.header = header;
			bool bOldIdxFileLoaded = false;
			ArchiveIndexFile oldIndexFile = null;
			serializeObjList.Foreach(delegate(ArchiveObj serializeObj)
			{
				bool flag = true;
				if (saveRecordObj != null)
				{
					flag = saveRecordObj(serializeObj);
				}
				if (flag)
				{
					WriteRecord(mIndexFile, serializeObj);
				}
				else
				{
					if (!bOldIdxFileLoaded)
					{
						oldIndexFile = ArchiveIndexFile.LoadFromFile(dir, yirdName);
						bOldIdxFileLoaded = true;
					}
					CopyIndex(mIndexFile, oldIndexFile, serializeObj);
				}
			});
			mIndexFile.Write(dir);
		}
		catch (Exception message)
		{
			Debug.LogWarning(message);
			return false;
		}
		return true;
	}

	private static void CopyIndex(ArchiveIndexFile indexFile, ArchiveIndexFile oldIndexFile, ArchiveObj serializeObj)
	{
		if (oldIndexFile == null)
		{
			return;
		}
		serializeObj.Foreach(delegate(ArchiveObj.SerializeObj obj)
		{
			ArchiveIndex archiveIndex = oldIndexFile.GetArchiveIndex(obj.key);
			if (archiveIndex != null)
			{
				indexFile.Add(obj.key, archiveIndex);
			}
		});
	}

	private void WriteRecord(ArchiveIndexFile indexFile, ArchiveObj serializeObj)
	{
		if (serializeObj.hasNonYird)
		{
			string filePath = GetFilePath(dir, serializeObj.recordName);
			FileStream fileStream = new FileStream(filePath, FileMode.Create, FileAccess.Write);
			try
			{
				BinaryWriter writer = new BinaryWriter(fileStream);
				try
				{
					indexFile.header.Write(writer);
					indexFile.header.BeginWriteCheckSum(writer);
					serializeObj.Foreach(delegate(ArchiveObj.SerializeObj obj)
					{
						if (!obj.yird)
						{
							long position = fileStream.Position;
							obj.serialize.Serialize(new PeRecordWriter(obj.key, writer));
							long position2 = fileStream.Position;
							indexFile.Add(obj.key, new ArchiveIndex(serializeObj.recordName, obj.yird, position, position2));
						}
					});
					indexFile.header.EndWriteCheckSum(writer);
				}
				finally
				{
					if (writer != null)
					{
						((IDisposable)writer).Dispose();
					}
				}
			}
			finally
			{
				if (fileStream != null)
				{
					((IDisposable)fileStream).Dispose();
				}
			}
		}
		if (!serializeObj.hasYird)
		{
			return;
		}
		string yirdDir = indexFile.GetYirdDir(dir);
		string filePath2 = GetFilePath(yirdDir, serializeObj.recordName);
		if (!Directory.Exists(yirdDir))
		{
			Directory.CreateDirectory(yirdDir);
		}
		FileStream yirdFileStream = new FileStream(filePath2, FileMode.Create, FileAccess.Write);
		try
		{
			BinaryWriter yirdWriter = new BinaryWriter(yirdFileStream);
			try
			{
				indexFile.header.Write(yirdWriter);
				indexFile.header.BeginWriteCheckSum(yirdWriter);
				serializeObj.Foreach(delegate(ArchiveObj.SerializeObj obj)
				{
					if (obj.yird)
					{
						long position3 = yirdFileStream.Position;
						obj.serialize.Serialize(new PeRecordWriter(obj.key, yirdWriter));
						long position4 = yirdFileStream.Position;
						indexFile.Add(obj.key, new ArchiveIndex(serializeObj.recordName, obj.yird, position3, position4));
					}
				});
				indexFile.header.EndWriteCheckSum(yirdWriter);
			}
			finally
			{
				if (yirdWriter != null)
				{
					((IDisposable)yirdWriter).Dispose();
				}
			}
		}
		finally
		{
			if (yirdFileStream != null)
			{
				((IDisposable)yirdFileStream).Dispose();
			}
		}
	}

	public bool LoadFromFile()
	{
		try
		{
			if (!Directory.Exists(dir))
			{
				return false;
			}
			mIndexFile = ArchiveIndexFile.LoadFromFile(dir);
			return mIndexFile != null;
		}
		catch (Exception message)
		{
			Debug.LogWarning(message);
			return false;
		}
	}
}

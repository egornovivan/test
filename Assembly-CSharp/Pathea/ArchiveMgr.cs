using System;
using System.IO;
using PeEvent;
using UnityEngine;

namespace Pathea;

public class ArchiveMgr : MonoLikeSingleton<ArchiveMgr>
{
	public enum ESave
	{
		Min = 0,
		MinAuto = 0,
		Auto1 = 0,
		Auto2 = 1,
		Auto3 = 2,
		MaxAuto = 3,
		MinUser = 3,
		User1 = 3,
		User2 = 4,
		User3 = 5,
		User4 = 6,
		User5 = 7,
		User6 = 8,
		User7 = 9,
		User8 = 10,
		User9 = 11,
		User10 = 12,
		User11 = 13,
		User12 = 14,
		User13 = 15,
		User14 = 16,
		User15 = 17,
		User16 = 18,
		User17 = 19,
		User18 = 20,
		User19 = 21,
		User20 = 22,
		MaxUser = 23,
		Max = 23,
		New = 23
	}

	public class ArchiveEvent : EventArg
	{
		public ESave eSave;
	}

	public const string RecordNameWorld = "world";

	private const string UserSaveFileNamePrefix = "save";

	private const string AutoSaveFileNamePrefix = "auto";

	private const string ArchiveDirName = "GameSave";

	private Event<ArchiveEvent> mEventor;

	private ArchiveObj.List mArchiveObjList = new ArchiveObj.List(30);

	private string mYirdName;

	private Archive mCurArchive;

	private ESave mCurSave = ESave.MaxUser;

	private SwapSpace mSwapSpace = new SwapSpace();

	private Archive.Header[] mArchiveHeaders = new Archive.Header[23];

	public Event<ArchiveEvent> eventor
	{
		get
		{
			if (mEventor == null)
			{
				mEventor = new Event<ArchiveEvent>(this);
			}
			return mEventor;
		}
	}

	private string yirdName => mYirdName;

	public bool autoSave { get; private set; }

	private string CreateCurArchive(ESave eSave)
	{
		string archiveDir = GetArchiveDir(eSave);
		mCurSave = eSave;
		mCurArchive = new Archive(archiveDir);
		return archiveDir;
	}

	private void RemoveCurArchive()
	{
		mCurSave = ESave.MaxUser;
		mCurArchive = null;
	}

	private static string GetArchiveRootDir()
	{
		return Path.Combine(GameConfig.GetPeUserDataPath(), "GameSave");
	}

	private static string GetArchiveDir(string dirName)
	{
		return Path.Combine(GetArchiveRootDir(), dirName);
	}

	private static string GetArchiveDir(ESave eSave)
	{
		string dirName = ((eSave >= ESave.MaxAuto) ? ("save" + (int)(eSave - 3)) : ("auto" + (int)eSave));
		return GetArchiveDir(dirName);
	}

	private Archive.Header GetHeader(ESave eSave)
	{
		return mArchiveHeaders[(int)eSave];
	}

	private void SetHeader(ESave eSave, Archive.Header header)
	{
		mArchiveHeaders[(int)eSave] = header;
	}

	private void SaveArchive(ESave eSave)
	{
		eventor.Dispatch(new ArchiveEvent
		{
			eSave = eSave
		});
		string dirDst = CreateCurArchive(eSave);
		Archive.Header header = GetHeader(eSave);
		if (header == null)
		{
			header = new Archive.Header();
			header.NewGuid();
			SetHeader(eSave, header);
			if (!mSwapSpace.CopyTo(dirDst, delegate(FileInfo fileInfo)
			{
				using FileStream output = fileInfo.Open(FileMode.Open, FileAccess.Write);
				using BinaryWriter w = new BinaryWriter(output);
				header.Write(w);
			}))
			{
				Debug.LogError("[Save]:Failed to save game at " + eSave);
				return;
			}
		}
		mCurArchive.WriteToFile(mArchiveObjList, yirdName, header, delegate(ArchiveObj recordObj)
		{
			if (eSave >= ESave.Min && eSave < ESave.MaxAuto)
			{
				bool saveFlag = recordObj.GetSaveFlag(eSave);
				recordObj.ResetSaveFlag(eSave);
				return saveFlag;
			}
			return true;
		});
	}

	public void Register(string key, ISerializable serializableObj, bool yird = false, string recordName = "world", bool saveFlagResetValue = true)
	{
		mArchiveObjList.Add(key, serializableObj, yird, recordName, saveFlagResetValue);
	}

	public bool SaveMe(string key)
	{
		ArchiveObj archiveObj = mArchiveObjList.FindByKey(key);
		if (archiveObj == null)
		{
			return false;
		}
		archiveObj.SetAllFlag(flag: true);
		return true;
	}

	public bool LoadYird(string yirdName)
	{
		mYirdName = yirdName;
		if (mCurArchive == null)
		{
			return false;
		}
		return mCurArchive.LoadYird(yirdName);
	}

	public byte[] GetData(string key)
	{
		return mCurArchive.GetData(key);
	}

	public int GetCurArvhiveVersion()
	{
		Archive.Header header = mCurArchive.GetHeader();
		return header.version;
	}

	public PeRecordReader GetReader(string key)
	{
		return mCurArchive.GetReader(key);
	}

	public void QuitSave()
	{
		Debug.Log("<color=aqua> game quit, save archive to auto1. </color>");
		Save(ESave.Min);
	}

	public void Save(ESave eSave)
	{
		if (!GameLog.IsFatalError && PeGameMgr.playerType != PeGameMgr.EPlayerType.Tutorial)
		{
			autoSave = eSave < ESave.MaxAuto;
			SaveArchive(eSave);
			autoSave = false;
		}
	}

	public void LoadAndCleanSwap(ESave eSave)
	{
		mSwapSpace.Init();
		string text = Load(eSave);
		if (string.IsNullOrEmpty(text))
		{
			return;
		}
		Archive.Header header = mCurArchive.GetHeader();
		mSwapSpace.CopyFrom(text, delegate(FileInfo fileInfo)
		{
			try
			{
				using FileStream input = fileInfo.Open(FileMode.Open, FileAccess.Read, FileShare.Read);
				using BinaryReader r = new BinaryReader(input);
				Archive.Header header2 = new Archive.Header();
				if (header2.Read(r) && header.IsMatch(header2))
				{
					return true;
				}
			}
			catch (Exception ex)
			{
				Debug.LogWarning("[LoadAndCleanSwap]" + ex);
			}
			return false;
		});
	}

	public string Load(ESave eSave)
	{
		string result = null;
		if (eSave != ESave.MaxUser)
		{
			result = CreateCurArchive(eSave);
			if (!mCurArchive.LoadFromFile())
			{
				result = null;
			}
		}
		return result;
	}

	public void Delete(ESave eSave)
	{
		if (eSave == mCurSave)
		{
			RemoveCurArchive();
		}
		try
		{
			Directory.Delete(GetArchiveDir(eSave), recursive: true);
		}
		catch
		{
		}
	}
}

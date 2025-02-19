using System;
using System.Collections.Generic;
using UnityEngine;

namespace Pathea;

public class ArchiveObj
{
	public class SerializeObj
	{
		public bool yird;

		public string key;

		public ISerializable serialize;
	}

	public class List
	{
		private string mDirName;

		private List<ArchiveObj> mList;

		public List(int capacity)
		{
			mList = new List<ArchiveObj>(capacity);
		}

		public ArchiveObj FindByKey(string key)
		{
			return mList.Find((ArchiveObj item) => item.Find(key) != null);
		}

		private ArchiveObj Get(string recordName)
		{
			return mList.Find((ArchiveObj item) => (item.recordName == recordName) ? true : false);
		}

		public bool Add(string key, ISerializable serializableObj, bool yird, string recordName, bool saveFlagResetValue)
		{
			if (serializableObj == null)
			{
				return false;
			}
			if (FindByKey(key) != null)
			{
				Debug.LogError("serialize obj existed: " + key);
				return false;
			}
			ArchiveObj archiveObj = Get(recordName);
			if (archiveObj == null)
			{
				archiveObj = new ArchiveObj(recordName, saveFlagResetValue);
				mList.Add(archiveObj);
			}
			else
			{
				archiveObj.SetSaveFlagResetValue(saveFlagResetValue);
			}
			archiveObj.Add(key, serializableObj, yird);
			return true;
		}

		public void Foreach(Action<ArchiveObj> action)
		{
			mList.ForEach(action);
		}
	}

	private string mRecordName;

	private bool mHasYird;

	private bool mHasNonYird;

	private List<SerializeObj> mItemList = new List<SerializeObj>(5);

	private bool[] mSaveFlag = new bool[3];

	private bool mSaveFlagResetValue;

	public string recordName => mRecordName;

	public bool hasYird
	{
		get
		{
			return mHasYird;
		}
		set
		{
			mHasYird = value;
		}
	}

	public bool hasNonYird
	{
		get
		{
			return mHasNonYird;
		}
		set
		{
			mHasNonYird = value;
		}
	}

	public ArchiveObj(string recordName, bool saveNeedResetValue)
	{
		mRecordName = recordName;
		mSaveFlagResetValue = saveNeedResetValue;
		SetAllFlag(mSaveFlagResetValue);
	}

	public void SetAllFlag(bool flag)
	{
		for (int i = 0; i < 3; i++)
		{
			mSaveFlag[i] = flag;
		}
	}

	private void SetSaveFlagResetValue(bool value)
	{
		mSaveFlagResetValue = value || mSaveFlagResetValue;
	}

	public bool GetSaveFlag(ArchiveMgr.ESave eSave)
	{
		return mSaveFlag[(int)eSave];
	}

	public void ResetSaveFlag(ArchiveMgr.ESave eSave)
	{
		mSaveFlag[(int)eSave] = mSaveFlagResetValue;
	}

	public void Foreach(Action<SerializeObj> action)
	{
		mItemList.ForEach(action);
	}

	private SerializeObj Find(string key)
	{
		return mItemList.Find((SerializeObj item) => (item.key == key) ? true : false);
	}

	private void Add(string key, ISerializable serializableObj, bool yird)
	{
		mItemList.Add(new SerializeObj
		{
			key = key,
			serialize = serializableObj,
			yird = yird
		});
		if (yird)
		{
			mHasYird = true;
		}
		else
		{
			mHasNonYird = true;
		}
	}
}

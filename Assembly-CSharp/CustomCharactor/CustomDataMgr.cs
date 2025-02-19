using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace CustomCharactor;

public class CustomDataMgr
{
	private const int MaxItemCount = 100;

	private static CustomDataMgr instance;

	private List<CustomData> mCustomDataList;

	private List<string> mFilePathList;

	public static CustomDataMgr Instance
	{
		get
		{
			if (instance == null)
			{
				instance = new CustomDataMgr();
			}
			return instance;
		}
	}

	private string DirPath => Path.Combine(GameConfig.GetUserDataPath(), "PlanetExplorers/CustomCharacter");

	public CustomData Current { get; set; }

	public List<CustomData> CustomDataList => mCustomDataList;

	public int dataCount => mCustomDataList.Count;

	private CustomDataMgr()
	{
		mCustomDataList = new List<CustomData>();
		mFilePathList = new List<string>();
	}

	private string GetFilePath(int index)
	{
		return Path.Combine(DirPath, "Custom_" + index + ".dat");
	}

	private void CheckDir()
	{
		if (!Directory.Exists(DirPath))
		{
			Directory.CreateDirectory(DirPath);
		}
	}

	public CustomData GetCustomData(int index)
	{
		if (index >= dataCount)
		{
			return null;
		}
		CustomData customData = new CustomData();
		byte[] data = mCustomDataList[index].Serialize();
		customData.Deserialize(data);
		return customData;
	}

	public Texture2D GetDataHeadIco(int index)
	{
		if (index >= dataCount)
		{
			return null;
		}
		return mCustomDataList[index].headIcon;
	}

	public void LoadAllData()
	{
		mCustomDataList.Clear();
		mFilePathList.Clear();
		CheckDir();
		string[] files = Directory.GetFiles(DirPath);
		string[] array = files;
		foreach (string text in array)
		{
			CustomData customData = LoadData(text);
			if (customData != null)
			{
				mCustomDataList.Add(customData);
				mFilePathList.Add(text);
			}
		}
	}

	public void DeleteData(int index)
	{
		if (index < dataCount && File.Exists(mFilePathList[index]))
		{
			File.Delete(mFilePathList[index]);
			mFilePathList.RemoveAt(index);
			mCustomDataList.RemoveAt(index);
		}
	}

	public bool SaveData(CustomData appearData)
	{
		int num = dataCount;
		string filePath;
		do
		{
			num++;
			filePath = GetFilePath(num);
		}
		while (File.Exists(filePath));
		return SaveData(num, appearData);
	}

	private bool SaveData(int index, CustomData appearData)
	{
		if (appearData == null)
		{
			return false;
		}
		CheckDir();
		using (FileStream fileStream = new FileStream(GetFilePath(index), FileMode.Create, FileAccess.Write))
		{
			byte[] array = appearData.Serialize();
			fileStream.Write(array, 0, array.Length);
		}
		mCustomDataList.Add(appearData);
		mFilePathList.Add(GetFilePath(index));
		return true;
	}

	private CustomData LoadData(string path)
	{
		if (!File.Exists(path))
		{
			return null;
		}
		try
		{
			using FileStream fileStream = new FileStream(path, FileMode.Open, FileAccess.Read);
			if (fileStream.Length > 0)
			{
				byte[] array = new byte[fileStream.Length];
				fileStream.Read(array, 0, array.Length);
				CustomData customData = new CustomData();
				customData.Deserialize(array);
				return customData;
			}
		}
		catch
		{
			return null;
		}
		return null;
	}
}

using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class RecentRoomDataManager
{
	public List<RecentRoomData> mRecentRoomList = new List<RecentRoomData>();

	private string roleFileName = string.Empty;

	public RecentRoomDataManager(string _roleName)
	{
		if (!string.IsNullOrEmpty(_roleName))
		{
			roleFileName = _roleName + ".rds";
			char[] invalidFileNameChars = Path.GetInvalidFileNameChars();
			char[] array = invalidFileNameChars;
			foreach (char oldChar in array)
			{
				roleFileName = roleFileName.Replace(oldChar, '_');
			}
		}
	}

	public void AddItem(long _UID, string _RoomName, string _Creator, string _Version)
	{
		RecentRoomData recentRoomData = mRecentRoomList.Find((RecentRoomData rd) => rd.mUID == _UID);
		if (recentRoomData == null)
		{
			RecentRoomData recentRoomData2 = new RecentRoomData();
			recentRoomData2.mUID = _UID;
			recentRoomData2.mRoomName = _RoomName;
			recentRoomData2.mCreator = _Creator;
			recentRoomData2.mVersion = _Version;
			mRecentRoomList.Insert(0, recentRoomData2);
			SaveToFile();
		}
	}

	public void DeleteItem(long _UID)
	{
		RecentRoomData recentRoomData = mRecentRoomList.Find((RecentRoomData rd) => rd.mUID == _UID);
		if (recentRoomData != null)
		{
			mRecentRoomList.Remove(recentRoomData);
			SaveToFile();
		}
	}

	public bool SaveToFile()
	{
		if (string.IsNullOrEmpty(roleFileName))
		{
			return false;
		}
		string text = GameConfig.GetUserDataPath() + "/PlanetExplorers/Config/";
		if (!Directory.Exists(text))
		{
			Directory.CreateDirectory(text);
		}
		text += roleFileName;
		try
		{
			using (FileStream fileStream = new FileStream(text, FileMode.Create, FileAccess.Write))
			{
				BinaryWriter binaryWriter = new BinaryWriter(fileStream);
				SaveData(binaryWriter);
				binaryWriter.Close();
				fileStream.Close();
			}
			return true;
		}
		catch (Exception ex)
		{
			Debug.LogWarning("Save RecentData Error:  " + ex.ToString());
			return false;
		}
	}

	public bool LoadFromFile()
	{
		if (string.IsNullOrEmpty(roleFileName))
		{
			return false;
		}
		string text = GameConfig.GetUserDataPath() + "/PlanetExplorers/Config/";
		if (!Directory.Exists(text))
		{
			Directory.CreateDirectory(text);
		}
		text += roleFileName;
		if (!File.Exists(text))
		{
			return true;
		}
		try
		{
			using (FileStream fileStream = new FileStream(text, FileMode.Open, FileAccess.Read))
			{
				BinaryReader binaryReader = new BinaryReader(fileStream);
				ReadData(binaryReader);
				binaryReader.Close();
				fileStream.Close();
			}
			return true;
		}
		catch (Exception ex)
		{
			Debug.LogWarning("Load RecentData Error:  " + ex.ToString());
			return false;
		}
	}

	private void SaveData(BinaryWriter bw)
	{
		int count = mRecentRoomList.Count;
		bw.Write(count);
		for (int i = 0; i < count; i++)
		{
			bw.Write(mRecentRoomList[i].mUID);
			bw.Write(mRecentRoomList[i].mRoomName);
			bw.Write(mRecentRoomList[i].mCreator);
			bw.Write(mRecentRoomList[i].mVersion);
		}
	}

	private void ReadData(BinaryReader br)
	{
		mRecentRoomList.Clear();
		int num = br.ReadInt32();
		for (int i = 0; i < num; i++)
		{
			RecentRoomData recentRoomData = new RecentRoomData();
			recentRoomData.mUID = br.ReadInt64();
			recentRoomData.mRoomName = br.ReadString();
			recentRoomData.mCreator = br.ReadString();
			recentRoomData.mVersion = br.ReadString();
			mRecentRoomList.Add(recentRoomData);
		}
	}
}

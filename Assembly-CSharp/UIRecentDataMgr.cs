using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class UIRecentDataMgr : MonoBehaviour
{
	private static UIRecentDataMgr mInstance;

	private Dictionary<string, int> mIntMap;

	private Dictionary<string, float> mFloatMap;

	private Dictionary<string, string> mStringMap;

	private Dictionary<string, Vector3> mVector3Map;

	private FileStream mFileStream;

	public static UIRecentDataMgr Instance => mInstance;

	private void Awake()
	{
		mInstance = this;
		mIntMap = new Dictionary<string, int>();
		mFloatMap = new Dictionary<string, float>();
		mStringMap = new Dictionary<string, string>();
		mVector3Map = new Dictionary<string, Vector3>();
		OpenFile();
		Load();
	}

	private void OnDestroy()
	{
		try
		{
			UIStateMgr.Instance.SaveUIPostion();
			CloseFile();
		}
		catch
		{
		}
	}

	public void SaveUIRecentData()
	{
		Save();
	}

	public int GetIntValue(string key, int DefaltValue)
	{
		if (!mIntMap.ContainsKey(key))
		{
			mIntMap[key] = DefaltValue;
		}
		return mIntMap[key];
	}

	public float GetFloatValue(string key, float DefaltValue)
	{
		if (!mFloatMap.ContainsKey(key))
		{
			mFloatMap[key] = DefaltValue;
		}
		return mFloatMap[key];
	}

	public string GetStringValue(string key, string DefaltValue)
	{
		if (!mStringMap.ContainsKey(key))
		{
			mStringMap[key] = DefaltValue;
		}
		return mStringMap[key];
	}

	public Vector3 GetVector3Value(string key, Vector3 DefaltValue)
	{
		if (!mVector3Map.ContainsKey(key))
		{
			mVector3Map[key] = DefaltValue;
		}
		return mVector3Map[key];
	}

	public void SetIntValue(string key, int vlaue)
	{
		mIntMap[key] = vlaue;
	}

	public void SetFloatValue(string key, float value)
	{
		mFloatMap[key] = value;
	}

	public void SetStringValue(string key, string value)
	{
		mStringMap[key] = value;
	}

	public void SetVector3Value(string key, Vector3 value)
	{
		mVector3Map[key] = value;
	}

	private void OpenFile()
	{
		string text = GameConfig.GetUserDataPath() + "/PlanetExplorers/Config/";
		if (!Directory.Exists(text))
		{
			Directory.CreateDirectory(text);
		}
		text += "UIRencent.urds";
		if (!File.Exists(text))
		{
			File.Create(text);
		}
		try
		{
			mFileStream = new FileStream(text, FileMode.Open, FileAccess.ReadWrite);
		}
		catch
		{
			Debug.LogError("Open UIRencent Error!");
		}
	}

	private void CloseFile()
	{
		if (mFileStream != null)
		{
			mFileStream.Close();
		}
	}

	private bool Load()
	{
		if (mFileStream == null)
		{
			return false;
		}
		try
		{
			BinaryReader binaryReader = new BinaryReader(mFileStream);
			binaryReader.BaseStream.Seek(0L, SeekOrigin.Begin);
			ReadData(binaryReader);
			return true;
		}
		catch
		{
			Debug.LogError("Read UIRecent file faild");
			return false;
		}
	}

	private bool Save()
	{
		try
		{
			BinaryWriter binaryWriter = new BinaryWriter(mFileStream);
			binaryWriter.Seek(0, SeekOrigin.Begin);
			SaveData(binaryWriter);
			return true;
		}
		catch
		{
			Debug.LogError("Read UIRecent file faild");
			return false;
		}
	}

	private bool LoadConfigFile()
	{
		string text = GameConfig.GetUserDataPath() + "/PlanetExplorers/Config/";
		if (!Directory.Exists(text))
		{
			Directory.CreateDirectory(text);
		}
		text += "UIRencent.urds";
		if (!File.Exists(text))
		{
			return false;
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
		catch
		{
			Debug.LogError("Load UIRencent Error!");
			return false;
		}
	}

	private void SaveConFigFile()
	{
		string text = GameConfig.GetUserDataPath() + "/PlanetExplorers/Config/";
		if (!Directory.Exists(text))
		{
			Directory.CreateDirectory(text);
		}
		text += "UIRencent.urds";
		try
		{
			using FileStream fileStream = new FileStream(text, FileMode.Create, FileAccess.Write);
			BinaryWriter binaryWriter = new BinaryWriter(fileStream);
			SaveData(binaryWriter);
			binaryWriter.Close();
			fileStream.Close();
		}
		catch
		{
			Debug.LogError("Save UIRencent Error!");
		}
	}

	private void SaveData(BinaryWriter bw)
	{
		bw.Write(GetGameVersion());
		int count = mIntMap.Keys.Count;
		bw.Write(count);
		foreach (string key in mIntMap.Keys)
		{
			bw.Write(key);
			bw.Write(mIntMap[key]);
		}
		count = mFloatMap.Keys.Count;
		bw.Write(count);
		foreach (string key2 in mFloatMap.Keys)
		{
			bw.Write(key2);
			bw.Write(mFloatMap[key2]);
		}
		count = mStringMap.Keys.Count;
		bw.Write(count);
		foreach (string key3 in mStringMap.Keys)
		{
			bw.Write(key3);
			bw.Write(mStringMap[key3]);
		}
		count = mVector3Map.Keys.Count;
		bw.Write(count);
		foreach (string key4 in mVector3Map.Keys)
		{
			bw.Write(key4);
			bw.Write(mVector3Map[key4].x);
			bw.Write(mVector3Map[key4].y);
			bw.Write(mVector3Map[key4].z);
		}
	}

	private void ReadData(BinaryReader br)
	{
		string text = br.ReadString();
		if (GetGameVersion() != text || text.Length == 0)
		{
			Debug.LogWarning("The game version is change on load ui recent data!");
		}
		int num = br.ReadInt32();
		for (int i = 0; i < num; i++)
		{
			string key = br.ReadString();
			mIntMap[key] = br.ReadInt32();
		}
		num = br.ReadInt32();
		for (int j = 0; j < num; j++)
		{
			string key2 = br.ReadString();
			mFloatMap[key2] = br.ReadSingle();
		}
		num = br.ReadInt32();
		for (int k = 0; k < num; k++)
		{
			string key3 = br.ReadString();
			mStringMap[key3] = br.ReadString();
		}
		num = br.ReadInt32();
		for (int l = 0; l < num; l++)
		{
			string key4 = br.ReadString();
			float x = br.ReadSingle();
			float y = br.ReadSingle();
			float z = br.ReadSingle();
			mVector3Map[key4] = new Vector3(x, y, z);
		}
	}

	private string GetGameVersion()
	{
		return GameConfig.GameVersion;
	}
}

using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Xml;
using Pathea.IO;
using PETools;
using UnityEngine;

namespace Pathea;

public class CustomGameData
{
	public class Mgr : PeSingleton<Mgr>, IPesingleton
	{
		private Dictionary<string, CustomGameData> mDatas = new Dictionary<string, CustomGameData>(5);

		public CustomGameData curGameData;

		void IPesingleton.Init()
		{
		}

		public CustomGameData GetCustomData(string UID, string Path = null)
		{
			if (mDatas.ContainsKey(UID))
			{
				return mDatas[UID];
			}
			if (Path == null)
			{
				return null;
			}
			CustomGameData customGameData = new CustomGameData();
			customGameData.Load(Path);
			mDatas.Add(UID, customGameData);
			return customGameData;
		}

		public YirdData GetYirdData(string UID, string yirdName = null)
		{
			return GetCustomData(UID)?.GetYirdData(yirdName);
		}

		public List<CustomGameData> GetCustomGameList()
		{
			return null;
		}
	}

	private const int _index = 0;

	public List<string> mWorldNames = new List<string>(1);

	public List<PlayerDesc> mPlayerDescs = new List<PlayerDesc>(1);

	public List<ForceDesc> mForceDescs = new List<ForceDesc>(10);

	private List<PlayerDesc> mHumanDescs = new List<PlayerDesc>(1);

	private int mWorldIndex;

	private int mPlayerIndex;

	private string mDir;

	private List<YirdData> mList = new List<YirdData>(1);

	private static readonly string s_WorldsDir = "Worlds";

	private static readonly string s_ScenarioDir = "Scenario";

	public PlayerDesc[] humanDescs => mHumanDescs.ToArray();

	public YirdData curYirdData => (mList.Count > mWorldIndex) ? mList[mWorldIndex] : null;

	public PlayerDesc curPlayer
	{
		get
		{
			if (PeGameMgr.IsMulti)
			{
				return BaseNetwork.curPlayerDesc;
			}
			return (mPlayerDescs.Count > mPlayerIndex) ? mHumanDescs[mPlayerIndex] : null;
		}
	}

	public int WorldIndex
	{
		get
		{
			return mWorldIndex;
		}
		set
		{
			mWorldIndex = value;
		}
	}

	public string name => Path.GetFileNameWithoutExtension(mDir);

	public Texture screenshot => defaultYird?.screenshot;

	public Vector3 size => defaultYird?.size ?? Vector3.zero;

	public string missionDir => Path.Combine(Path.Combine(mDir, s_ScenarioDir), "Missions");

	private YirdData defaultYird
	{
		get
		{
			if (mList.Count > 0)
			{
				return mList[0];
			}
			return null;
		}
	}

	public bool DeterminePlayer(int index)
	{
		if (mHumanDescs.Count <= index || index < -1)
		{
			return false;
		}
		mPlayerIndex = index;
		mWorldIndex = mHumanDescs[index].StartWorld;
		return true;
	}

	public YirdData GetYirdData(string yirdName = null)
	{
		if (string.IsNullOrEmpty(yirdName))
		{
			return defaultYird;
		}
		return mList.Find((YirdData item) => (item.name == yirdName) ? true : false);
	}

	public YirdData GetYirdData(int index)
	{
		if (index < 0 || index >= mList.Count)
		{
			return null;
		}
		return mList[index];
	}

	public void ForEach(Action<YirdData> action)
	{
		mList.ForEach(action);
	}

	public bool Load(string dir)
	{
		string text = Path.Combine(dir, s_ScenarioDir);
		if (!Directory.Exists(text))
		{
			return false;
		}
		string text2 = Path.Combine(text, "WorldSettings.xml");
		if (!File.Exists(text2))
		{
			return false;
		}
		string empty = string.Empty;
		empty = StringIO.LoadFromFile(text2, Encoding.UTF8);
		XmlDocument xmlDocument = new XmlDocument();
		StringReader txtReader = new StringReader(empty);
		xmlDocument.Load(txtReader);
		XmlNode xmlNode = xmlDocument.SelectSingleNode("WORLDSETTINGS");
		XmlNodeList elementsByTagName = ((XmlElement)xmlNode).GetElementsByTagName("WORLD");
		foreach (XmlNode item2 in elementsByTagName)
		{
			XmlElement e = item2 as XmlElement;
			string attributeString = XmlUtil.GetAttributeString(e, "path");
			mWorldNames.Add(attributeString);
		}
		string text3 = Path.Combine(text, "ForceSettings.xml");
		if (!File.Exists(text3))
		{
			return false;
		}
		empty = StringIO.LoadFromFile(text3, Encoding.UTF8);
		Singleton<ForceSetting>.Instance.Load(empty);
		mForceDescs.Clear();
		mPlayerDescs.Clear();
		mForceDescs.AddRange(Singleton<ForceSetting>.Instance.m_Forces);
		mPlayerDescs.AddRange(Singleton<ForceSetting>.Instance.m_Players);
		mHumanDescs.AddRange(Singleton<ForceSetting>.Instance.m_Players.FindAll((PlayerDesc ret) => ret.Type == EPlayerType.Human));
		if (mWorldNames.Count == 0)
		{
			return false;
		}
		string path = Path.Combine(dir, s_WorldsDir);
		foreach (string mWorldName in mWorldNames)
		{
			string dir2 = Path.Combine(path, mWorldName);
			YirdData item = new YirdData(dir2);
			mList.Add(item);
		}
		mDir = dir;
		return true;
	}
}

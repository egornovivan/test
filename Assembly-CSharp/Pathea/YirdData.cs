using System;
using System.Collections.Generic;
using System.IO;
using System.Xml;
using UnityEngine;

namespace Pathea;

public class YirdData
{
	private Texture2D mTexture;

	private string mDir;

	private Vector3 mSize = Vector3.zero;

	public string name => Path.GetFileNameWithoutExtension(mDir);

	public Texture screenshot
	{
		get
		{
			if (mTexture == null)
			{
				if (!File.Exists(screenshotPath))
				{
					return null;
				}
				mTexture = new Texture2D(100, 100, TextureFormat.ARGB32, mipmap: false, linear: true);
				byte[] array = File.ReadAllBytes(screenshotPath);
				if (array != null)
				{
					mTexture.LoadImage(array);
					mTexture.Apply(updateMipmaps: false, makeNoLongerReadable: true);
				}
			}
			return mTexture;
		}
	}

	public string screenshotPath => Path.Combine(mDir, "Minimap.png");

	public string projDataPath => Path.Combine(mDir, "ProjectSettings.dat");

	public string worldSettingsPath => Path.Combine(mDir, "WorldSettings.xml");

	public string terrainPath => mDir;

	public string treePath => mDir;

	public string grassPath => mDir;

	public Vector3 size
	{
		get
		{
			if (mSize == Vector3.zero)
			{
				VProjectSettings vProjectSettings = new VProjectSettings();
				if (vProjectSettings.LoadFromFile(projDataPath))
				{
					return vProjectSettings.size;
				}
			}
			return mSize;
		}
	}

	public Vector3 defaultPlayerPos => new Vector3(size.x / 2f, 128f, size.z / 2f);

	public YirdData(string dir)
	{
		mDir = dir;
	}

	public IEnumerable<WEItem> GetItems()
	{
		return GetDatas<WEItem>("ITEM");
	}

	public IEnumerable<WEDoodad> GetDoodads()
	{
		return GetDatas<WEDoodad>("DOODAD");
	}

	public IEnumerable<WEMonster> GetMonsters()
	{
		return GetDatas<WEMonster>("MONSTER");
	}

	public IEnumerable<WENPC> GetNpcs()
	{
		return GetDatas<WENPC>("NPC");
	}

	public IEnumerable<WEEffect> GetEffects()
	{
		return GetDatas<WEEffect>("EFFECT");
	}

	private IEnumerable<T> GetDatas<T>(string nodeName) where T : VEObject, new()
	{
		XmlNodeList xmlNodeList = GetXmlNodeList(nodeName);
		if (xmlNodeList == null || xmlNodeList.Count == 0)
		{
			return null;
		}
		T[] array = new T[xmlNodeList.Count];
		for (int i = 0; i < xmlNodeList.Count; i++)
		{
			if (xmlNodeList[i] is XmlElement xmlelem)
			{
				array[i] = new T();
				array[i].Parse(xmlelem);
			}
		}
		return array;
	}

	private XmlNodeList GetXmlNodeList(string nodeName)
	{
		string filename = Path.Combine(mDir, "WorldEntity.xml");
		XmlDocument xmlDocument = new XmlDocument();
		try
		{
			xmlDocument.Load(filename);
		}
		catch (Exception ex)
		{
			GameLog.HandleIOException(ex, GameLog.EIOFileType.InstallFiles);
		}
		return xmlDocument.SelectNodes("WORLDDATA/ENTITIES//" + nodeName);
	}
}

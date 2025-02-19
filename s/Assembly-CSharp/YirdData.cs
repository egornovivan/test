using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml;
using UnityEngine;

public class YirdData
{
	protected string mDir;

	protected Vector3 mSize = Vector3.zero;

	public string name => Path.GetFileNameWithoutExtension(mDir);

	public string worldSettingsPath => Path.Combine(mDir, "WorldSettings.xml");

	public string projDataPath => Path.Combine(mDir, "ProjectSettings.dat");

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
		return GetDatas<WEItem>("ITEM").ToArray();
	}

	public IEnumerable<WEDoodad> GetDoodads()
	{
		return GetDatas<WEDoodad>("DOODAD").ToArray();
	}

	public IEnumerable<WEMonster> GetMonsters()
	{
		return GetDatas<WEMonster>("MONSTER").ToArray();
	}

	public IEnumerable<WENPC> GetNpcs()
	{
		return GetDatas<WENPC>("NPC").ToArray();
	}

	public IEnumerable<WEEffect> GetEffects()
	{
		return GetDatas<WEEffect>("EFFECT").ToArray();
	}

	private IEnumerable<T> GetDatas<T>(string nodeName) where T : VEObject, new()
	{
		XmlNodeList nodeList = GetXmlNodeList(nodeName);
		if (nodeList == null || nodeList.Count == 0)
		{
			yield break;
		}
		T[] items = new T[nodeList.Count];
		for (int i = 0; i < nodeList.Count; i++)
		{
			if (nodeList[i] is XmlElement node)
			{
				items[i] = new T();
				items[i].Parse(node);
				yield return items[i];
			}
		}
	}

	private XmlNodeList GetXmlNodeList(string nodeName)
	{
		string filename = Path.Combine(mDir, "WorldEntity.xml");
		XmlDocument xmlDocument = new XmlDocument();
		xmlDocument.Load(filename);
		return xmlDocument.SelectNodes("WORLDDATA/ENTITIES//" + nodeName);
	}
}

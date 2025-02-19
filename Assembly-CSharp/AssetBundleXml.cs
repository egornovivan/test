using System.IO;
using System.Xml.Serialization;
using UnityEngine;

public class AssetBundleXml
{
	public bool isItem;

	public bool isTower;

	public bool isGroup;

	public bool isNative;

	public bool isMonster;

	private static AssetBundleXml _instance;

	public static AssetBundleXml instance
	{
		get
		{
			if (_instance == null)
			{
				SerializeAssetBundleXml(Application.dataPath + "/AssetBundles.xml");
			}
			return _instance;
		}
	}

	public bool IsEnable(string path)
	{
		string directoryName = Path.GetDirectoryName(path);
		if (directoryName.Contains("Monster"))
		{
			return isMonster;
		}
		if (directoryName.Contains("Native"))
		{
			return isNative;
		}
		if (directoryName.Contains("Group"))
		{
			return isGroup;
		}
		if (directoryName.Contains("Tower"))
		{
			return isTower;
		}
		if (directoryName.Contains("Item"))
		{
			return isItem;
		}
		return true;
	}

	private static void SerializeAssetBundleXml(string fileName)
	{
		if (File.Exists(fileName))
		{
			using (FileStream fileStream = new FileStream(fileName, FileMode.Open, FileAccess.Read))
			{
				if (fileStream != null)
				{
					XmlSerializer xmlSerializer = new XmlSerializer(typeof(AssetBundleXml));
					_instance = xmlSerializer.Deserialize(fileStream) as AssetBundleXml;
				}
				else
				{
					Debug.LogError("Do not have any correct AssetBundles file!!");
				}
				return;
			}
		}
		using FileStream fileStream2 = new FileStream(fileName, FileMode.Create, FileAccess.Write);
		if (fileStream2 != null)
		{
			_instance = new AssetBundleXml();
			_instance.isItem = true;
			_instance.isTower = true;
			_instance.isGroup = true;
			_instance.isNative = true;
			_instance.isMonster = true;
			XmlSerializer xmlSerializer2 = new XmlSerializer(typeof(AssetBundleXml));
			xmlSerializer2.Serialize(fileStream2, _instance);
		}
		else
		{
			Debug.LogError("Do not have any correct Config file!!");
		}
	}
}

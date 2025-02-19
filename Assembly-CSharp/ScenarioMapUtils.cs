using System.Collections.Generic;
using System.IO;
using System.Text;
using Pathea.IO;

public static class ScenarioMapUtils
{
	private static Dictionary<string, ScenarioMapDesc> detectedMaps = new Dictionary<string, ScenarioMapDesc>();

	public static ScenarioMapDesc[] GetMapList(string path)
	{
		string[] directories = Directory.GetDirectories(path, "*", SearchOption.TopDirectoryOnly);
		List<ScenarioMapDesc> list = new List<ScenarioMapDesc>();
		string[] array = directories;
		foreach (string text in array)
		{
			string text2 = text + "/MAP.uid";
			if (File.Exists(text2))
			{
				string uid = StringIO.LoadFromFile(text2, Encoding.UTF8);
				uid = uid.Trim();
				if (uid.Length == 32 && list.Find((ScenarioMapDesc iter) => iter.UID == uid) == null)
				{
					ScenarioMapDesc scenarioMapDesc = new ScenarioMapDesc(uid, text);
					list.Add(scenarioMapDesc);
					detectedMaps[uid] = scenarioMapDesc;
				}
			}
		}
		return list.ToArray();
	}

	public static ScenarioMapDesc GetMapByUID(string uid, string rootpath)
	{
		if (detectedMaps.ContainsKey(uid))
		{
			return detectedMaps[uid];
		}
		string[] files = Directory.GetFiles(rootpath, "MAP.uid", SearchOption.AllDirectories);
		string[] array = files;
		foreach (string text in array)
		{
			string text2 = StringIO.LoadFromFile(text, Encoding.UTF8);
			text2 = text2.Trim();
			if (text2 == uid && text2.Length == 32)
			{
				FileInfo fileInfo = new FileInfo(text);
				ScenarioMapDesc scenarioMapDesc = new ScenarioMapDesc(uid, fileInfo.DirectoryName);
				detectedMaps[uid] = scenarioMapDesc;
				return scenarioMapDesc;
			}
		}
		return null;
	}

	public static string GetMapUID(string path)
	{
		string text = path + "/MAP.uid";
		if (File.Exists(text))
		{
			string text2 = StringIO.LoadFromFile(text, Encoding.UTF8);
			text2 = text2.Trim();
			if (text2.Length == 32)
			{
				return text2;
			}
		}
		return null;
	}

	public static ScenarioIntegrityCheck CheckIntegrityByUID(string uid, string rootpath)
	{
		ScenarioMapDesc mapByUID = GetMapByUID(uid, rootpath);
		if (mapByUID == null)
		{
			return new ScenarioIntegrityCheck(string.Empty);
		}
		return new ScenarioIntegrityCheck(mapByUID.Path);
	}

	public static ScenarioIntegrityCheck CheckIntegrityByPath(string path)
	{
		return new ScenarioIntegrityCheck(path);
	}
}

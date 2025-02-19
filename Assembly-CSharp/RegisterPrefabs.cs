using System.Collections.Generic;
using System.IO;
using uLink;
using UnityEngine;

[RequireComponent(typeof(uLink.RegisterPrefabs))]
public class RegisterPrefabs : UnityEngine.MonoBehaviour
{
	public List<string> Paths = new List<string>();

	private uLink.RegisterPrefabs Register;

	public void SavePath()
	{
		string path = Path.Combine(Application.dataPath, "PrefabsPath.txt");
		using FileStream fileStream = new FileStream(path, FileMode.OpenOrCreate, FileAccess.Write);
		if (fileStream == null)
		{
			return;
		}
		using StreamWriter streamWriter = new StreamWriter(fileStream);
		if (streamWriter == null)
		{
			return;
		}
		foreach (string path2 in Paths)
		{
			streamWriter.WriteLine(path2);
		}
	}
}

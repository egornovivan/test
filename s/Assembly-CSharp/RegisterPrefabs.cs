using System.Collections.Generic;
using System.IO;
using System.Text;
using uLink;
using UnityEngine;

[RequireComponent(typeof(uLink.RegisterPrefabs))]
public class RegisterPrefabs : UnityEngine.MonoBehaviour
{
	public uLink.RegisterPrefabs Register;

	public List<string> Paths = new List<string>();

	public void SavePath()
	{
		string path = Path.Combine(Application.dataPath, "PrefabsPath.path");
		if (Paths.Count >= 1)
		{
			File.WriteAllLines(path, Paths.ToArray(), Encoding.UTF8);
		}
	}

	public void Reimport()
	{
	}
}

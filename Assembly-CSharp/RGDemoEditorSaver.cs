using System.IO;
using RedGrass;
using UnityEngine;

public class RGDemoEditorSaver : MonoBehaviour
{
	public RGSimpleEditor editor;

	public static string s_FileDirectory => Application.dataPath + "/Caches/";

	private void OnGUI()
	{
		if (GUI.Button(new Rect(Screen.width - 350, 50f, 200f, 50f), "Save Caches"))
		{
			SaveCaches();
		}
	}

	private void OnDestroy()
	{
		SaveCaches();
	}

	private void Awake()
	{
	}

	public void SaveCaches()
	{
		if (editor == null || editor.isEmpty)
		{
			return;
		}
		string text = ".";
		if (!Directory.Exists(text + "/Caches"))
		{
			Directory.CreateDirectory(text + "/Caches");
		}
		string[] files = Directory.GetFiles(text + "/Caches/");
		string text2 = text + "/Caches/";
		int num = -1;
		for (int i = 0; i < files.Length; i++)
		{
			string text3 = files[i].Substring(files[i].LastIndexOf(".") + 1);
			if (!(text3 != "gs"))
			{
				using FileStream fileStream = new FileStream(files[i], FileMode.Open, FileAccess.ReadWrite, FileShare.ReadWrite);
				BinaryReader binaryReader = new BinaryReader(fileStream);
				int num2 = binaryReader.ReadInt32();
				num = ((num2 <= num) ? num : num2);
				fileStream.Close();
			}
		}
		num++;
		using (FileStream fileStream2 = new FileStream(text2 + "grass_cache_" + num + ".gs", FileMode.Create, FileAccess.ReadWrite, FileShare.ReadWrite))
		{
			BinaryWriter binaryWriter = new BinaryWriter(fileStream2);
			RedGrassInstance[] addGrasses = editor.addGrasses;
			binaryWriter.Write(num);
			binaryWriter.Write(addGrasses.Length);
			RedGrassInstance[] array = addGrasses;
			foreach (RedGrassInstance redGrassInstance in array)
			{
				redGrassInstance.WriteToStream(binaryWriter);
			}
			fileStream2.Close();
		}
		editor.Clear();
	}
}

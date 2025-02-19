using System;
using System.IO;
using UnityEngine;

namespace Pathea;

public class PeScreenshot : MonoBehaviour
{
	private void Update()
	{
		if (PeInput.Get(PeInput.LogicFunction.ScreenCapture))
		{
			CaptureToFile();
		}
	}

	private static void CaptureToFile()
	{
		Texture2D tex = GetTex();
		if (!(null == tex))
		{
			SaveTex(tex);
		}
	}

	private static void SaveTex(Texture2D tex)
	{
		try
		{
			string text = Application.dataPath + "/ScreenCapture";
			if (!Directory.Exists(text))
			{
				Directory.CreateDirectory(text);
			}
			string path = text + "/PE_" + DateTime.Now.ToShortDateString().Replace(" ", string.Empty).Replace("/", ".") + "_" + DateTime.Now.ToShortTimeString().Replace(":", ".").Replace(" ", string.Empty) + "." + DateTime.Now.Second + ".png";
			File.WriteAllBytes(path, tex.EncodeToPNG());
		}
		catch (Exception ex)
		{
			Debug.Log("Failed to save screen capture! " + ex.ToString());
		}
	}

	public static Texture2D GetTex()
	{
		Camera main = Camera.main;
		main.targetTexture = new RenderTexture(Screen.width, Screen.height, 32);
		Texture2D result = PhotoStudio.RTImage(main);
		main.targetTexture = null;
		return result;
	}
}

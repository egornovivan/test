using System;
using System.IO;
using System.Text;
using UnityEngine;

namespace Pathea.IO;

public static class FileUtil
{
	public static bool LoadTextureFromFile(Texture2D tex, string filename)
	{
		if (tex == null)
		{
			return false;
		}
		if (!File.Exists(filename))
		{
			return false;
		}
		byte[] array = null;
		try
		{
			FileStream fileStream = new FileStream(filename, FileMode.Open);
			if (fileStream.Length > 268435456 || fileStream.Length < 8)
			{
				fileStream.Close();
				return false;
			}
			array = new byte[(int)fileStream.Length];
			fileStream.Read(array, 0, (int)fileStream.Length);
			fileStream.Close();
		}
		catch (Exception)
		{
			return false;
		}
		return tex.LoadImage(array);
	}

	public static bool SaveTextureToFile(Texture2D tex, string filename)
	{
		if (tex == null)
		{
			return false;
		}
		byte[] array = tex.EncodeToPNG();
		try
		{
			FileStream fileStream = new FileStream(filename, FileMode.Create);
			fileStream.Write(array, 0, array.Length);
			fileStream.Close();
			return true;
		}
		catch (Exception)
		{
			return false;
		}
	}

	public static byte[] LoadBytes(string path)
	{
		try
		{
			using FileStream fileStream = new FileStream(path, FileMode.Open, FileAccess.Read);
			BinaryReader binaryReader = new BinaryReader(fileStream);
			byte[] result = binaryReader.ReadBytes((int)fileStream.Length);
			binaryReader.Close();
			fileStream.Close();
			return result;
		}
		catch (Exception message)
		{
			Debug.LogWarning(message);
		}
		return new byte[0];
	}

	public static Shader LoadShader(string path)
	{
		using FileStream fileStream = new FileStream(path, FileMode.Open, FileAccess.Read);
		BinaryReader binaryReader = new BinaryReader(fileStream);
		byte[] bytes = binaryReader.ReadBytes((int)fileStream.Length);
		binaryReader.Close();
		fileStream.Close();
		string @string = Encoding.UTF8.GetString(bytes);
		Material material = new Material(@string);
		return material.shader;
	}

	public static void SaveBytes(string path, byte[] bytes)
	{
		try
		{
			using FileStream fileStream = new FileStream(path, FileMode.Create, FileAccess.Write);
			BinaryWriter binaryWriter = new BinaryWriter(fileStream);
			binaryWriter.Write(bytes);
			binaryWriter.Close();
			fileStream.Close();
		}
		catch (Exception message)
		{
			Debug.LogWarning(message);
		}
	}
}

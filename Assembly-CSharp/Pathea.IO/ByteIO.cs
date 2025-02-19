using System;
using System.IO;
using UnityEngine;

namespace Pathea.IO;

public static class ByteIO
{
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
		catch
		{
		}
		return new byte[0];
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

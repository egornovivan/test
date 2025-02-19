using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;

public static class MD5Helper
{
	public static string CretaeMD5(string fileName)
	{
		string result = string.Empty;
		FileStream fileStream = null;
		try
		{
			fileStream = new FileStream(fileName, FileMode.Open, FileAccess.Read, FileShare.Read);
			MD5 mD = MD5.Create();
			byte[] values = mD.ComputeHash(fileStream);
			mD.Clear();
			result = ByteArrayToHexString(values);
		}
		catch (Exception ex)
		{
			LogManager.Error(ex);
		}
		finally
		{
			if (fileStream != null)
			{
				fileStream.Close();
				fileStream.Dispose();
			}
		}
		return result;
	}

	public static string CretaeMD5(Stream stream)
	{
		MD5 mD = MD5.Create();
		byte[] values = mD.ComputeHash(stream);
		mD.Clear();
		return ByteArrayToHexString(values);
	}

	public static string CretaeMD5(byte[] buffer, int offset, int count)
	{
		MD5 mD = MD5.Create();
		byte[] values = mD.ComputeHash(buffer, offset, count);
		mD.Clear();
		return ByteArrayToHexString(values);
	}

	public static string MD5Encoding(string rawPass)
	{
		MD5 mD = MD5.Create();
		byte[] bytes = Encoding.UTF8.GetBytes(rawPass);
		byte[] values = mD.ComputeHash(bytes);
		mD.Clear();
		return ByteArrayToHexString(values);
	}

	public static string MD5Encoding(Stream stream)
	{
		MD5 mD = MD5.Create();
		byte[] values = mD.ComputeHash(stream);
		mD.Clear();
		return ByteArrayToHexString(values);
	}

	private static string ByteArrayToHexString(byte[] values)
	{
		StringBuilder stringBuilder = new StringBuilder();
		foreach (byte b in values)
		{
			stringBuilder.AppendFormat("{0:X2}", b);
		}
		return stringBuilder.ToString();
	}
}

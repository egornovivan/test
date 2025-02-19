using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;

public static class MD5Helper
{
	public static string CretaeMD5(string fileName)
	{
		string empty = string.Empty;
		try
		{
			FileStream fileStream = new FileStream(fileName, FileMode.Open, FileAccess.Read, FileShare.Read);
			MD5CryptoServiceProvider mD5CryptoServiceProvider = new MD5CryptoServiceProvider();
			byte[] values = mD5CryptoServiceProvider.ComputeHash(fileStream);
			empty = ByteArrayToHexString(values);
			fileStream.Close();
			fileStream.Dispose();
			return empty;
		}
		catch (Exception ex)
		{
			throw ex;
		}
	}

	public static string CretaeMD5(Stream stream)
	{
		MD5CryptoServiceProvider mD5CryptoServiceProvider = new MD5CryptoServiceProvider();
		byte[] values = mD5CryptoServiceProvider.ComputeHash(stream);
		return ByteArrayToHexString(values);
	}

	public static string CretaeMD5(byte[] buffer, int offset, int count)
	{
		MD5CryptoServiceProvider mD5CryptoServiceProvider = new MD5CryptoServiceProvider();
		byte[] values = mD5CryptoServiceProvider.ComputeHash(buffer, offset, count);
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

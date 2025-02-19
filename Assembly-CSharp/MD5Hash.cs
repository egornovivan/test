using System.IO;
using System.Security.Cryptography;
using System.Text;
using UnityEngine;

public class MD5Hash : MonoBehaviour
{
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

	public static string MD5Encoding(byte[] buffer, int offset, int count)
	{
		MD5 mD = MD5.Create();
		byte[] values = mD.ComputeHash(buffer, offset, count);
		mD.Clear();
		return ByteArrayToHexString(values);
	}

	private static string ByteArrayToHexString(byte[] values)
	{
		string text = string.Empty;
		foreach (byte b in values)
		{
			text += b.ToString("X").PadLeft(2, '0');
		}
		return text;
	}
}

using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using Microsoft.Win32;
using UnityEngine;

public class Activation
{
	private static Activation instance;

	private string peacRegisterName = "peac";

	private DESCryptoServiceProvider desCryptoProvider;

	public static Activation Instance
	{
		get
		{
			if (instance == null)
			{
				instance = new Activation();
			}
			return instance;
		}
	}

	public bool Activated { get; set; }

	public string UniqueIdentifier => SystemInfo.deviceUniqueIdentifier;

	private Activation()
	{
		byte[] key = new byte[8] { 123, 223, 1, 81, 11, 243, 78, 16 };
		byte[] iV = new byte[8] { 120, 230, 10, 21, 10, 22, 31, 46 };
		desCryptoProvider = new DESCryptoServiceProvider();
		desCryptoProvider.Key = key;
		desCryptoProvider.IV = iV;
	}

	private string GetActivationString()
	{
		string text = DESCrypto(UniqueIdentifier);
		if (string.IsNullOrEmpty(text))
		{
			Debug.LogError("GetActivationstring return empty string.");
			return string.Empty;
		}
		return MD5Encoding(text);
	}

	private string DESCrypto(string plainText)
	{
		try
		{
			byte[] bytes = Encoding.Unicode.GetBytes(plainText);
			MemoryStream memoryStream = new MemoryStream();
			ICryptoTransform transform = desCryptoProvider.CreateEncryptor();
			CryptoStream cryptoStream = new CryptoStream(memoryStream, transform, CryptoStreamMode.Write);
			cryptoStream.Write(bytes, 0, bytes.Length);
			cryptoStream.FlushFinalBlock();
			return Convert.ToBase64String(memoryStream.ToArray());
		}
		catch (Exception)
		{
			Debug.Log("Crypto error");
			return string.Empty;
		}
	}

	private string DESCryptoDe(string cipherText)
	{
		try
		{
			byte[] array = Convert.FromBase64String(cipherText);
			MemoryStream memoryStream = new MemoryStream();
			ICryptoTransform transform = desCryptoProvider.CreateDecryptor();
			CryptoStream cryptoStream = new CryptoStream(memoryStream, transform, CryptoStreamMode.Write);
			cryptoStream.Write(array, 0, array.Length);
			cryptoStream.FlushFinalBlock();
			return Encoding.Unicode.GetString(memoryStream.ToArray());
		}
		catch (Exception)
		{
			Debug.Log("CryptoDe error");
			return string.Empty;
		}
	}

	public string MD5Encoding(string rawPass)
	{
		try
		{
			MD5 mD = MD5.Create();
			byte[] bytes = Encoding.UTF8.GetBytes(rawPass);
			byte[] array = mD.ComputeHash(bytes);
			StringBuilder stringBuilder = new StringBuilder();
			byte[] array2 = array;
			foreach (byte b in array2)
			{
				stringBuilder.Append(b.ToString("x2"));
			}
			return stringBuilder.ToString();
		}
		catch (Exception message)
		{
			Debug.Log(message);
			return string.Empty;
		}
	}

	private string GetRegisterKey()
	{
		try
		{
			RegistryKey registryKey = Registry.CurrentUser.OpenSubKey("Software\\Pathea");
			if (registryKey != null)
			{
				object value = registryKey.GetValue(peacRegisterName);
				return value.ToString();
			}
			Debug.LogError("Get Stored Activate info failed.");
			return string.Empty;
		}
		catch (Exception message)
		{
			Debug.LogWarning(message);
			return string.Empty;
		}
	}

	private bool WriteRegisterKey(string Value)
	{
		try
		{
			RegistryKey registryKey = Registry.CurrentUser.CreateSubKey("SoftWare\\Pathea");
			registryKey.SetValue(peacRegisterName, Value);
			registryKey.Flush();
			registryKey.Close();
		}
		catch (Exception message)
		{
			Debug.Log("Write Key Failed.");
			Debug.LogWarning(message);
			return false;
		}
		return true;
	}

	public void CheckActivatation()
	{
		Activated = true;
	}

	public bool Activate()
	{
		string activationString = GetActivationString();
		if (!string.IsNullOrEmpty(activationString))
		{
			WriteRegisterKey(activationString);
		}
		CheckActivatation();
		return Activated;
	}

	public void Deactivate()
	{
		WriteRegisterKey(string.Empty);
		CheckActivatation();
	}
}

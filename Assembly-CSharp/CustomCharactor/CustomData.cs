using System;
using System.IO;
using AppearBlendShape;
using PETools;
using UnityEngine;

namespace CustomCharactor;

public class CustomData
{
	private const int VERSION_0000 = 0;

	private const int CURRENT_VERSION = 0;

	public string charactorName { get; set; }

	public Texture2D headIcon { get; set; }

	public ESex sex { get; set; }

	public AvatarData nudeAvatarData { get; set; }

	public AppearData appearData { get; set; }

	public CustomData()
	{
		charactorName = "default";
		sex = ESex.Male;
	}

	public static CustomData DefaultMale()
	{
		CustomData customData = new CustomData();
		customData.sex = ESex.Male;
		customData.nudeAvatarData = new AvatarData();
		customData.nudeAvatarData.SetMaleBody();
		customData.appearData = new AppearData();
		customData.appearData.Default();
		customData.charactorName = "PlayerName";
		return customData;
	}

	public static CustomData DefaultFemale()
	{
		CustomData customData = new CustomData();
		customData.sex = ESex.Female;
		customData.nudeAvatarData = new AvatarData();
		customData.nudeAvatarData.SetFemaleBody();
		customData.appearData = new AppearData();
		customData.appearData.Default();
		customData.charactorName = "PlayerName";
		return customData;
	}

	public static CustomData RandomMale(string excludeHead)
	{
		CustomData customData = new CustomData();
		customData.sex = ESex.Male;
		customData.nudeAvatarData = new AvatarData();
		customData.nudeAvatarData.RandSetMaleBody(excludeHead);
		customData.appearData = new AppearData();
		customData.appearData.Default();
		customData.charactorName = "PlayerName";
		return customData;
	}

	public static CustomData RandomFemale(string excludeHead)
	{
		CustomData customData = new CustomData();
		customData.sex = ESex.Female;
		customData.nudeAvatarData = new AvatarData();
		customData.nudeAvatarData.RandSetFemaleBody(excludeHead);
		customData.appearData = new AppearData();
		customData.appearData.Default();
		customData.charactorName = "PlayerName";
		return customData;
	}

	public byte[] Serialize()
	{
		try
		{
			MemoryStream memoryStream = new MemoryStream(200);
			using (BinaryWriter binaryWriter = new BinaryWriter(memoryStream))
			{
				binaryWriter.Write(0);
				binaryWriter.Write(charactorName);
				if (null == headIcon)
				{
					PETools.Serialize.WriteBytes(null, binaryWriter);
				}
				else
				{
					PETools.Serialize.WriteBytes(headIcon.EncodeToPNG(), binaryWriter);
				}
				binaryWriter.Write((int)sex);
				if (nudeAvatarData == null)
				{
					PETools.Serialize.WriteBytes(null, binaryWriter);
				}
				else
				{
					PETools.Serialize.WriteBytes(nudeAvatarData.Serialize(), binaryWriter);
				}
				if (appearData == null)
				{
					PETools.Serialize.WriteBytes(null, binaryWriter);
				}
				else
				{
					PETools.Serialize.WriteBytes(appearData.Serialize(), binaryWriter);
				}
			}
			return memoryStream.ToArray();
		}
		catch (Exception message)
		{
			Debug.LogWarning(message);
			return null;
		}
	}

	public bool Deserialize(byte[] data)
	{
		try
		{
			MemoryStream input = new MemoryStream(data, writable: false);
			using BinaryReader binaryReader = new BinaryReader(input);
			int num = binaryReader.ReadInt32();
			if (num > 0)
			{
				Debug.LogWarning("version:" + num + " greater than current version:" + 0);
				return false;
			}
			charactorName = binaryReader.ReadString();
			byte[] array = PETools.Serialize.ReadBytes(binaryReader);
			if (array != null && array.Length > 0)
			{
				headIcon = new Texture2D(2, 2, TextureFormat.ARGB32, mipmap: false);
				headIcon.LoadImage(array);
				headIcon.Apply();
			}
			sex = (ESex)binaryReader.ReadInt32();
			nudeAvatarData = new AvatarData();
			nudeAvatarData.Deserialize(PETools.Serialize.ReadBytes(binaryReader));
			appearData = new AppearData();
			appearData.Deserialize(PETools.Serialize.ReadBytes(binaryReader));
			return true;
		}
		catch (Exception message)
		{
			Debug.LogWarning(message);
			return false;
		}
	}
}

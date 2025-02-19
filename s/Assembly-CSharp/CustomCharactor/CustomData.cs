using System.IO;
using Pathea;
using PETools;
using UnityEngine;

namespace CustomCharactor;

public class CustomData
{
	public enum ESex
	{
		Male,
		Female,
		Max
	}

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

	public static CustomData CreateCustomData(PeSex sex)
	{
		CustomData customData = null;
		customData = ((sex != PeSex.Female) ? RandomMale() : RandomFemale());
		customData.appearData.Random();
		customData.charactorName = null;
		return customData;
	}

	public static CustomData DefaultMale()
	{
		CustomData customData = new CustomData();
		customData.sex = ESex.Male;
		customData.nudeAvatarData = new AvatarData();
		customData.nudeAvatarData.DefaultMaleBody();
		customData.appearData = new AppearData();
		customData.charactorName = "PlayerName";
		return customData;
	}

	public static CustomData DefaultFemale()
	{
		CustomData customData = new CustomData();
		customData.sex = ESex.Female;
		customData.nudeAvatarData = new AvatarData();
		customData.nudeAvatarData.DefaultFemaleBody();
		customData.appearData = new AppearData();
		customData.charactorName = "PlayerName";
		return customData;
	}

	public static CustomData RandomMale()
	{
		CustomData customData = new CustomData();
		customData.sex = ESex.Male;
		customData.nudeAvatarData = new AvatarData();
		customData.nudeAvatarData.RandomMaleBody();
		customData.appearData = new AppearData();
		customData.charactorName = "PlayerName";
		return customData;
	}

	public static CustomData RandomFemale()
	{
		CustomData customData = new CustomData();
		customData.sex = ESex.Female;
		customData.nudeAvatarData = new AvatarData();
		customData.nudeAvatarData.RandomFemaleBody();
		customData.appearData = new AppearData();
		customData.charactorName = "PlayerName";
		return customData;
	}

	public byte[] Serialize()
	{
		return PETools.Serialize.Export(delegate(BinaryWriter w)
		{
			w.Write(0);
			w.Write(charactorName);
			if (null == headIcon)
			{
				PETools.Serialize.WriteBytes(null, w);
			}
			else
			{
				PETools.Serialize.WriteBytes(headIcon.EncodeToPNG(), w);
			}
			w.Write((int)sex);
			if (nudeAvatarData == null)
			{
				PETools.Serialize.WriteBytes(null, w);
			}
			else
			{
				PETools.Serialize.WriteBytes(nudeAvatarData.Serialize(), w);
			}
			if (appearData == null)
			{
				PETools.Serialize.WriteBytes(null, w);
			}
			else
			{
				PETools.Serialize.WriteBytes(appearData.Serialize(), w);
			}
		});
	}

	public void Deserialize(byte[] data)
	{
		PETools.Serialize.Import(data, delegate(BinaryReader r)
		{
			int num = r.ReadInt32();
			if (num > 0)
			{
				Debug.LogWarning("version:" + num + " greater than current version:" + 0);
			}
			else
			{
				charactorName = r.ReadString();
				byte[] array = PETools.Serialize.ReadBytes(r);
				if (array != null && array.Length > 0)
				{
					headIcon = new Texture2D(2, 2, TextureFormat.ARGB32, mipmap: false);
					headIcon.LoadImage(array);
					headIcon.Apply();
				}
				sex = (ESex)r.ReadInt32();
				nudeAvatarData = new AvatarData();
				nudeAvatarData.Deserialize(PETools.Serialize.ReadBytes(r));
				appearData = new AppearData();
				appearData.Deserialize(PETools.Serialize.ReadBytes(r));
			}
		});
	}
}

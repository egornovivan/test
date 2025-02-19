using System.Collections.Generic;
using System.IO;
using PETools;
using UnityEngine;

namespace CustomCharactor;

public class AvatarData
{
	public enum ESlot
	{
		HairF,
		HairT,
		HairB,
		Head,
		Torso,
		Hands,
		Legs,
		Feet,
		Back,
		Max
	}

	private const int VERSION_0000 = 0;

	private const int VERSION_0001 = 1;

	private const int CURRENT_VERSION = 1;

	private Dictionary<ESlot, string> mAvatarParts;

	public AvatarData()
	{
		mAvatarParts = new Dictionary<ESlot, string>(10);
	}

	public string GetPart(ESlot part)
	{
		if (mAvatarParts.ContainsKey(part))
		{
			return mAvatarParts[part];
		}
		return null;
	}

	public void SetPart(ESlot part, string info)
	{
		if (part == ESlot.Max)
		{
			Debug.LogWarning("error slot");
		}
		else
		{
			mAvatarParts[part] = info;
		}
	}

	public byte[] Serialize()
	{
		return PETools.Serialize.Export(delegate(BinaryWriter w)
		{
			w.Write(1);
			w.Write(mAvatarParts.Count);
			foreach (KeyValuePair<ESlot, string> mAvatarPart in mAvatarParts)
			{
				w.Write((int)mAvatarPart.Key);
				w.Write(mAvatarPart.Value);
			}
		});
	}

	public void Deserialize(byte[] data)
	{
		PETools.Serialize.Import(data, delegate(BinaryReader r)
		{
			int num = r.ReadInt32();
			if (num > 1)
			{
				Debug.LogWarning("version:" + num + " greater than current version:" + 1);
			}
			else
			{
				int num2 = r.ReadInt32();
				for (int i = 0; i < num2; i++)
				{
					int part = r.ReadInt32();
					string info = r.ReadString();
					SetPart((ESlot)part, info);
				}
			}
		});
	}

	public void DefaultMaleBody()
	{
		SetPart(ESlot.HairF, CustomMetaData.InstanceMale.GetHair(1).modelPath[0]);
		SetPart(ESlot.HairT, CustomMetaData.InstanceMale.GetHair(1).modelPath[1]);
		SetPart(ESlot.HairB, CustomMetaData.InstanceMale.GetHair(1).modelPath[2]);
		SetPart(ESlot.Head, CustomMetaData.InstanceMale.GetHead(0).modelPath);
		SetPart(ESlot.Torso, CustomMetaData.InstanceMale.upperBody);
		SetPart(ESlot.Legs, CustomMetaData.InstanceMale.lowerBody);
		SetPart(ESlot.Hands, CustomMetaData.InstanceMale.hands);
		SetPart(ESlot.Feet, CustomMetaData.InstanceMale.feet);
	}

	public void DefaultFemaleBody()
	{
		SetPart(ESlot.HairF, CustomMetaData.InstanceFemale.GetHair(1).modelPath[0]);
		SetPart(ESlot.HairT, CustomMetaData.InstanceFemale.GetHair(1).modelPath[1]);
		SetPart(ESlot.HairB, CustomMetaData.InstanceFemale.GetHair(1).modelPath[2]);
		SetPart(ESlot.Torso, CustomMetaData.InstanceFemale.upperBody);
		SetPart(ESlot.Legs, CustomMetaData.InstanceFemale.lowerBody);
		SetPart(ESlot.Hands, CustomMetaData.InstanceFemale.hands);
		SetPart(ESlot.Feet, CustomMetaData.InstanceFemale.feet);
		SetPart(ESlot.Head, CustomMetaData.InstanceFemale.GetHead(1).modelPath);
	}

	public void RandomMaleBody()
	{
		int index = Random.Range(0, CustomMetaData.InstanceMale.GetHeadCount());
		SetPart(ESlot.HairF, CustomMetaData.InstanceMale.GetHair(index).modelPath[0]);
		SetPart(ESlot.HairT, CustomMetaData.InstanceMale.GetHair(index).modelPath[1]);
		SetPart(ESlot.HairB, CustomMetaData.InstanceMale.GetHair(index).modelPath[2]);
		SetPart(ESlot.Torso, CustomMetaData.InstanceMale.upperBody);
		SetPart(ESlot.Legs, CustomMetaData.InstanceMale.lowerBody);
		SetPart(ESlot.Hands, CustomMetaData.InstanceMale.hands);
		SetPart(ESlot.Feet, CustomMetaData.InstanceMale.feet);
		int index2 = Random.Range(0, CustomMetaData.InstanceMale.GetHeadCount());
		string modelPath = CustomMetaData.InstanceMale.GetHead(index2).modelPath;
		SetPart(ESlot.Head, modelPath);
	}

	public void RandomFemaleBody()
	{
		int index = Random.Range(0, CustomMetaData.InstanceFemale.GetHeadCount());
		SetPart(ESlot.HairF, CustomMetaData.InstanceFemale.GetHair(index).modelPath[0]);
		SetPart(ESlot.HairT, CustomMetaData.InstanceFemale.GetHair(index).modelPath[1]);
		SetPart(ESlot.HairB, CustomMetaData.InstanceFemale.GetHair(index).modelPath[2]);
		SetPart(ESlot.Torso, CustomMetaData.InstanceFemale.upperBody);
		SetPart(ESlot.Legs, CustomMetaData.InstanceFemale.lowerBody);
		SetPart(ESlot.Hands, CustomMetaData.InstanceFemale.hands);
		SetPart(ESlot.Feet, CustomMetaData.InstanceFemale.feet);
		int index2 = Random.Range(0, CustomMetaData.InstanceFemale.GetHeadCount());
		string modelPath = CustomMetaData.InstanceFemale.GetHead(index2).modelPath;
		SetPart(ESlot.Head, modelPath);
	}

	public static ESlot GetSlot(int mask)
	{
		return mask switch
		{
			1024 => ESlot.HairF, 
			1 => ESlot.HairT, 
			2048 => ESlot.HairB, 
			4096 => ESlot.Head, 
			2 => ESlot.Torso, 
			4 => ESlot.Hands, 
			64 => ESlot.Legs, 
			128 => ESlot.Feet, 
			256 => ESlot.Back, 
			_ => ESlot.Max, 
		};
	}

	public static IEnumerable<string> GetParts(AvatarData cur, AvatarData origin)
	{
		List<string> list = new List<string>(10);
		for (int i = 0; i < 9; i++)
		{
			string part = GetPart((ESlot)i, cur, origin);
			if (!string.IsNullOrEmpty(part) && !list.Contains(part))
			{
				list.Add(part);
			}
		}
		return list;
	}

	private static string GetPart(ESlot slot, AvatarData cur, AvatarData origin)
	{
		string part = cur.GetPart(slot);
		if (string.IsNullOrEmpty(part))
		{
			part = origin.GetPart(slot);
		}
		return part;
	}
}

using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace CustomCharactor;

public class AvatarData : IEnumerable<string>, IEnumerable
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
		Max
	}

	private const int VersionWithBaseModel = 150911;

	private const int CURRENT_VERSION = 150911;

	private const int c_nSlots = 8;

	public string _baseModel = string.Empty;

	private string[] _avatarParts = new string[8];

	public string this[ESlot part]
	{
		get
		{
			return _avatarParts[(int)part];
		}
		set
		{
			_avatarParts[(int)part] = value;
		}
	}

	public AvatarData(AvatarData avData = null)
	{
		if (avData != null)
		{
			_baseModel = avData._baseModel;
			avData._avatarParts.CopyTo(_avatarParts, 0);
		}
	}

	IEnumerator IEnumerable.GetEnumerator()
	{
		return GetEnumerator();
	}

	public void SetPart(ESlot part, string info)
	{
		this[part] = info;
	}

	public override bool Equals(object obj)
	{
		AvatarData avatarData = (AvatarData)obj;
		if (avatarData._baseModel != _baseModel)
		{
			return false;
		}
		for (int i = 0; i < 8; i++)
		{
			if (avatarData._avatarParts[i] != _avatarParts[i])
			{
				return false;
			}
		}
		return true;
	}

	public override int GetHashCode()
	{
		int num = _baseModel.GetHashCode();
		for (int i = 0; i < 8; i++)
		{
			if (!string.IsNullOrEmpty(_avatarParts[i]))
			{
				num += _avatarParts[i].GetHashCode();
			}
		}
		return num;
	}

	public IEnumerator<string> GetEnumerator()
	{
		string[] avatarParts = _avatarParts;
		for (int i = 0; i < avatarParts.Length; i++)
		{
			yield return avatarParts[i];
		}
	}

	private void SetBodyParts(CustomMetaData mdata, int idxHair, int idxHead)
	{
		_baseModel = mdata.baseModel;
		SetPart(ESlot.HairF, mdata.GetHair(idxHair).modelPath[0]);
		SetPart(ESlot.HairT, mdata.GetHair(idxHair).modelPath[1]);
		SetPart(ESlot.HairB, mdata.GetHair(idxHair).modelPath[2]);
		SetPart(ESlot.Head, mdata.GetHead(idxHead).modelPath);
		SetPart(ESlot.Torso, mdata.torso);
		SetPart(ESlot.Legs, mdata.legs);
		SetPart(ESlot.Hands, mdata.hands);
		SetPart(ESlot.Feet, mdata.feet);
	}

	public void SetMaleBody()
	{
		SetBodyParts(CustomMetaData.InstanceMale, 1, 0);
	}

	public void SetFemaleBody()
	{
		SetBodyParts(CustomMetaData.InstanceFemale, 1, 0);
	}

	public void RandSetMaleBody(string excludeHead)
	{
		int idxHair = UnityEngine.Random.Range(0, CustomMetaData.InstanceMale.GetHeadCount());
		int num;
		string modelPath;
		do
		{
			num = UnityEngine.Random.Range(0, CustomMetaData.InstanceMale.GetHeadCount());
			modelPath = CustomMetaData.InstanceMale.GetHead(num).modelPath;
		}
		while (!(modelPath != excludeHead));
		SetBodyParts(CustomMetaData.InstanceMale, idxHair, num);
	}

	public void RandSetFemaleBody(string excludeHead)
	{
		int idxHair = UnityEngine.Random.Range(0, CustomMetaData.InstanceFemale.GetHeadCount());
		int num;
		string modelPath;
		do
		{
			num = UnityEngine.Random.Range(0, CustomMetaData.InstanceFemale.GetHeadCount());
			modelPath = CustomMetaData.InstanceFemale.GetHead(num).modelPath;
		}
		while (!(modelPath != excludeHead));
		SetBodyParts(CustomMetaData.InstanceFemale, idxHair, num);
	}

	public bool IsInvalid()
	{
		return _avatarParts == null || string.IsNullOrEmpty(_avatarParts[3]) || "0" == _avatarParts[3];
	}

	public byte[] Serialize()
	{
		try
		{
			MemoryStream memoryStream = new MemoryStream(100);
			using (BinaryWriter binaryWriter = new BinaryWriter(memoryStream))
			{
				binaryWriter.Write(150911);
				binaryWriter.Write(0);
				binaryWriter.Write(_baseModel);
				int num = 0;
				int num2 = _avatarParts.Length;
				for (int i = 0; i < num2; i++)
				{
					if (!string.IsNullOrEmpty(_avatarParts[i]))
					{
						binaryWriter.Write(i);
						binaryWriter.Write(_avatarParts[i]);
						num++;
					}
				}
				binaryWriter.Seek(4, SeekOrigin.Begin);
				binaryWriter.Write(num);
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
			int num2 = binaryReader.ReadInt32();
			if (num < 150911)
			{
				bool flag = false;
				for (int i = 0; i < num2; i++)
				{
					int num3 = binaryReader.ReadInt32();
					string text = binaryReader.ReadString();
					if (num3 >= 0 && num3 < _avatarParts.Length)
					{
						_avatarParts[num3] = text;
					}
					flag |= text.ToLower().Contains("female");
				}
				_baseModel = ((!flag) ? CustomMetaData.InstanceMale.baseModel : CustomMetaData.InstanceFemale.baseModel);
			}
			else
			{
				_baseModel = binaryReader.ReadString();
				for (int j = 0; j < num2; j++)
				{
					int num4 = binaryReader.ReadInt32();
					string text2 = binaryReader.ReadString();
					if (num4 >= 0 && num4 < _avatarParts.Length)
					{
						_avatarParts[num4] = text2;
					}
				}
			}
			return true;
		}
		catch (Exception ex)
		{
			Debug.LogWarning("CustomData.Deserialize Error:" + ex.Message);
			return false;
		}
	}

	public static IEnumerable<ESlot> GetSlot(int mask)
	{
		List<ESlot> list = new List<ESlot>(8);
		if ((mask & 0x400) != 0)
		{
			list.Add(ESlot.HairF);
		}
		if ((mask & 1) != 0)
		{
			list.Add(ESlot.HairT);
		}
		if ((mask & 0x800) != 0)
		{
			list.Add(ESlot.HairB);
		}
		if ((mask & 0x1000) != 0)
		{
			list.Add(ESlot.Head);
		}
		if ((mask & 2) != 0)
		{
			list.Add(ESlot.Torso);
		}
		if ((mask & 4) != 0)
		{
			list.Add(ESlot.Hands);
		}
		if ((mask & 0x40) != 0)
		{
			list.Add(ESlot.Legs);
		}
		if ((mask & 0x80) != 0)
		{
			list.Add(ESlot.Feet);
		}
		return list;
	}

	public static IEnumerable<string> GetParts(AvatarData cur, AvatarData origin)
	{
		List<string> list = new List<string>(10);
		for (int i = 0; i < 8; i++)
		{
			string part = GetPart((ESlot)i, cur, origin);
			if (!string.IsNullOrEmpty(part) && !list.Contains(part))
			{
				list.Add(part);
			}
		}
		return list;
	}

	private static string GetPart(ESlot slot, AvatarData cur, AvatarData ori)
	{
		return string.IsNullOrEmpty(cur._avatarParts[(int)slot]) ? ori._avatarParts[(int)slot] : cur._avatarParts[(int)slot];
	}

	public static AvatarData Merge(AvatarData hPrior, AvatarData lPrior)
	{
		AvatarData avatarData = new AvatarData(lPrior);
		if (hPrior != null)
		{
			for (int i = 0; i < 8; i++)
			{
				if (!string.IsNullOrEmpty(hPrior._avatarParts[i]))
				{
					avatarData._avatarParts[i] = hPrior._avatarParts[i];
				}
			}
		}
		return avatarData;
	}
}

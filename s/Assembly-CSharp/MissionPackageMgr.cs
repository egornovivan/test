using System.Collections.Generic;
using System.IO;
using System.Linq;
using ItemAsset;

public class MissionPackageMgr
{
	public static Dictionary<int, ItemPackage> _missionPackageMgr = new Dictionary<int, ItemPackage>();

	public static ItemPackage GetPak(int team)
	{
		if (!_missionPackageMgr.ContainsKey(team))
		{
			_missionPackageMgr.Add(team, new ItemPackage(100, 100, 100, 100));
		}
		return _missionPackageMgr[team];
	}

	public static byte[] Export(int team)
	{
		ItemPackage pak = GetPak(team);
		byte[] array = null;
		using MemoryStream memoryStream = new MemoryStream();
		using BinaryWriter binaryWriter = new BinaryWriter(memoryStream);
		binaryWriter.Write(pak.MaxItemNum);
		binaryWriter.Write(pak.MaxEquipNum);
		binaryWriter.Write(pak.MaxResourceNum);
		binaryWriter.Write(pak.MaxArmorNum);
		int[] array2 = pak.GetItemObjIDs(0).ToArray();
		binaryWriter.Write(array2.Length);
		int[] array3 = array2;
		foreach (int value in array3)
		{
			binaryWriter.Write(value);
		}
		int[] array4 = pak.GetItemObjIDs(1).ToArray();
		binaryWriter.Write(array4.Length);
		int[] array5 = array4;
		foreach (int value2 in array5)
		{
			binaryWriter.Write(value2);
		}
		int[] array6 = pak.GetItemObjIDs(2).ToArray();
		binaryWriter.Write(array6.Length);
		int[] array7 = array6;
		foreach (int value3 in array7)
		{
			binaryWriter.Write(value3);
		}
		int[] array8 = pak.GetItemObjIDs(3).ToArray();
		binaryWriter.Write(array8.Length);
		int[] array9 = array8;
		foreach (int value4 in array9)
		{
			binaryWriter.Write(value4);
		}
		binaryWriter.Close();
		array = memoryStream.ToArray();
		memoryStream.Close();
		return array;
	}

	public static void Import(byte[] buffer, int team)
	{
		if (buffer == null || buffer.Length <= 0)
		{
			return;
		}
		using MemoryStream memoryStream = new MemoryStream(buffer);
		using (BinaryReader binaryReader = new BinaryReader(memoryStream))
		{
			int num = binaryReader.ReadInt32();
			int num2 = binaryReader.ReadInt32();
			int num3 = binaryReader.ReadInt32();
			int armorMax = binaryReader.ReadInt32();
			GetPak(team).ExtendPackage(num, num, num, armorMax);
			int num4 = binaryReader.ReadInt32();
			for (int i = 0; i < num4; i++)
			{
				int num5 = binaryReader.ReadInt32();
				if (num5 != -1)
				{
					ItemObject itemByID = ItemManager.GetItemByID(num5);
					GetPak(team).SetItem(itemByID, i, 0);
				}
				else
				{
					GetPak(team).SetItem(null, i, 0);
				}
			}
			num4 = binaryReader.ReadInt32();
			for (int j = 0; j < num4; j++)
			{
				int num6 = binaryReader.ReadInt32();
				if (num6 != -1)
				{
					ItemObject itemByID2 = ItemManager.GetItemByID(num6);
					GetPak(team).SetItem(itemByID2, j, 1);
				}
				else
				{
					GetPak(team).SetItem(null, j, 1);
				}
			}
			num4 = binaryReader.ReadInt32();
			for (int k = 0; k < num4; k++)
			{
				int num7 = binaryReader.ReadInt32();
				if (num7 != -1)
				{
					ItemObject itemByID3 = ItemManager.GetItemByID(num7);
					GetPak(team).SetItem(itemByID3, k, 2);
				}
				else
				{
					GetPak(team).SetItem(null, k, 2);
				}
			}
			num4 = binaryReader.ReadInt32();
			for (int l = 0; l < num4; l++)
			{
				int num8 = binaryReader.ReadInt32();
				if (num8 != -1)
				{
					ItemObject itemByID4 = ItemManager.GetItemByID(num8);
					GetPak(team).SetItem(itemByID4, l, 3);
				}
				else
				{
					GetPak(team).SetItem(null, l, 3);
				}
			}
			binaryReader.Close();
		}
		memoryStream.Close();
	}
}

using System;
using System.IO;
using Pathea.Maths;
using UnityEngine;

public class VProjectSettings
{
	private static Vector3 scale = new Vector3(256f, 128f, 256f);

	public static string Header = "VProjectSettings";

	public static int CurrentVersion = 1;

	public INTVECTOR3 WorldPileCount = INTVECTOR3.zero;

	public Vector3 size => new Vector3(scale.x * (float)WorldPileCount.x, scale.y * (float)WorldPileCount.y, scale.z * (float)WorldPileCount.z);

	public INTVECTOR3 WorldSize => WorldPileCount;

	public bool SaveToFile(string filename)
	{
		try
		{
			using (FileStream fileStream = new FileStream(filename, FileMode.Create))
			{
				BinaryWriter binaryWriter = new BinaryWriter(fileStream);
				binaryWriter.Write(Header);
				binaryWriter.Write(CurrentVersion);
				binaryWriter.Write(WorldPileCount.x);
				binaryWriter.Write(WorldPileCount.y);
				binaryWriter.Write(WorldPileCount.z);
				binaryWriter.Close();
				fileStream.Close();
			}
			return true;
		}
		catch (Exception)
		{
			return false;
		}
	}

	public bool LoadFromFile(string filename)
	{
		try
		{
			bool result = true;
			using (FileStream fileStream = new FileStream(filename, FileMode.Open))
			{
				BinaryReader binaryReader = new BinaryReader(fileStream);
				string strA = binaryReader.ReadString();
				if (string.Compare(strA, Header) == 0)
				{
					int num = binaryReader.ReadInt32();
					int num2 = num;
					if (num2 == 1)
					{
						WorldPileCount.x = binaryReader.ReadInt32();
						WorldPileCount.y = binaryReader.ReadInt32();
						WorldPileCount.z = binaryReader.ReadInt32();
					}
					else
					{
						result = false;
					}
				}
				else
				{
					result = false;
				}
				binaryReader.Close();
				fileStream.Close();
			}
			return result;
		}
		catch (Exception)
		{
			return false;
		}
	}
}

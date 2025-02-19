using System;
using System.Collections.Generic;
using System.IO;
using Mono.Data.SqliteClient;
using UnityEngine;

public class BlockBuilding
{
	public static Dictionary<int, BlockBuilding> s_tblBlockBuildingMap;

	public int mId;

	public string mPath;

	private string mNpcIdNum;

	public Vector3 BoundSize = Vector3.zero;

	public static void LoadBuilding()
	{
		s_tblBlockBuildingMap = new Dictionary<int, BlockBuilding>();
		SqliteDataReader sqliteDataReader = LocalDatabase.Instance.ReadFullTable("BlockBuilding");
		while (sqliteDataReader.Read())
		{
			BlockBuilding blockBuilding = new BlockBuilding();
			blockBuilding.mId = Convert.ToInt32(sqliteDataReader.GetString(sqliteDataReader.GetOrdinal("ID")));
			blockBuilding.mPath = sqliteDataReader.GetString(sqliteDataReader.GetOrdinal("FilePath"));
			blockBuilding.mNpcIdNum = sqliteDataReader.GetString(sqliteDataReader.GetOrdinal("NPCId_Num"));
			s_tblBlockBuildingMap[blockBuilding.mId] = blockBuilding;
		}
	}

	public static BlockBuilding GetBuilding(int id)
	{
		return s_tblBlockBuildingMap[id];
	}

	public void GetBuildingInfo(out Dictionary<IntVector3, B45Block> blocks)
	{
		Bounds bounds = new Bounds(Vector3.zero, Vector3.zero);
		blocks = new Dictionary<IntVector3, B45Block>();
		TextAsset textAsset = Resources.Load(mPath) as TextAsset;
		MemoryStream memoryStream = new MemoryStream(textAsset.bytes);
		BinaryReader binaryReader = new BinaryReader(memoryStream);
		int num = binaryReader.ReadInt32();
		int num2 = num;
		if (num2 == 2)
		{
			int num3 = binaryReader.ReadInt32();
			for (int i = 0; i < num3; i++)
			{
				IntVector3 intVector = new IntVector3(binaryReader.ReadInt32(), binaryReader.ReadInt32(), binaryReader.ReadInt32());
				B45Block value = new B45Block(binaryReader.ReadByte(), binaryReader.ReadByte());
				if (value.blockType >> 2 != 0)
				{
					blocks[intVector] = value;
					intVector = new IntVector3(intVector);
					intVector.x++;
					intVector.z++;
					bounds.Encapsulate(GameWorld.MinBrushSize * intVector.ToVector3());
				}
			}
		}
		binaryReader.Close();
		memoryStream.Close();
	}
}

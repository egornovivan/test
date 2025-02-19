using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Mono.Data.SqliteClient;
using UnityEngine;

public class BlockBuilding
{
	public static Dictionary<int, BlockBuilding> s_tblBlockBuildingMap;

	public int mId;

	public int mDoodadProtoId;

	public string mPath;

	private string mNpcIdPosRotStand;

	private string mNpcIdPosRotMove;

	public Vector2 mSize;

	public Vector3 BoundSize = Vector3.zero;

	public static void LoadBuilding()
	{
		s_tblBlockBuildingMap = new Dictionary<int, BlockBuilding>();
		SqliteDataReader sqliteDataReader = LocalDatabase.Instance.ReadFullTable("Townhouse");
		while (sqliteDataReader.Read())
		{
			BlockBuilding blockBuilding = new BlockBuilding();
			blockBuilding.mId = Convert.ToInt32(sqliteDataReader.GetString(sqliteDataReader.GetOrdinal("ID")));
			blockBuilding.mDoodadProtoId = Convert.ToInt32(sqliteDataReader.GetString(sqliteDataReader.GetOrdinal("PrototypeDoodad_Id")));
			blockBuilding.mPath = sqliteDataReader.GetString(sqliteDataReader.GetOrdinal("FilePath"));
			blockBuilding.mNpcIdPosRotStand = sqliteDataReader.GetString(sqliteDataReader.GetOrdinal("NPCId_Num_Stand"));
			blockBuilding.mNpcIdPosRotMove = sqliteDataReader.GetString(sqliteDataReader.GetOrdinal("NPCId_Num_Move"));
			string @string = sqliteDataReader.GetString(sqliteDataReader.GetOrdinal("Size"));
			string[] array = @string.Split(',');
			blockBuilding.mSize = new Vector2(float.Parse(array[0]), float.Parse(array[1]));
			s_tblBlockBuildingMap[blockBuilding.mId] = blockBuilding;
		}
	}

	public static BlockBuilding GetBuilding(int id)
	{
		return s_tblBlockBuildingMap[id];
	}

	public static BlockBuilding GetBuilding(string fileName)
	{
		foreach (int key in s_tblBlockBuildingMap.Keys)
		{
			if (s_tblBlockBuildingMap[key].mPath.Contains(fileName))
			{
				return s_tblBlockBuildingMap[key];
			}
		}
		return null;
	}

	public void GetBuildingInfo(out Vector3 size, out Dictionary<IntVector3, B45Block> blocks, out List<Vector3> npcPosition, out List<CreatItemInfo> itemList, out Dictionary<int, BuildingNpc> npcIdPosRot)
	{
		Bounds bounds = new Bounds(Vector3.zero, Vector3.zero);
		blocks = new Dictionary<IntVector3, B45Block>();
		npcPosition = new List<Vector3>();
		itemList = new List<CreatItemInfo>();
		npcIdPosRot = new Dictionary<int, BuildingNpc>();
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
					bounds.Encapsulate(BSBlock45Data.s_Scale * intVector.ToVector3());
				}
			}
		}
		binaryReader.Close();
		memoryStream.Close();
		BoundSize = (size = bounds.size);
		UnityEngine.Object @object = Resources.Load(mPath + "SubInfo");
		if (null != @object)
		{
			textAsset = @object as TextAsset;
			memoryStream = new MemoryStream(textAsset.bytes);
			binaryReader = new BinaryReader(memoryStream);
			int num4 = binaryReader.ReadInt32();
			int num5 = binaryReader.ReadInt32();
			switch (num4)
			{
			case 1:
			{
				for (int l = 0; l < num5; l++)
				{
					npcPosition.Add(new Vector3(binaryReader.ReadSingle(), binaryReader.ReadSingle(), binaryReader.ReadSingle()));
				}
				break;
			}
			case 2:
			{
				for (int j = 0; j < num5; j++)
				{
					npcPosition.Add(new Vector3(binaryReader.ReadSingle(), binaryReader.ReadSingle(), binaryReader.ReadSingle()));
				}
				num5 = binaryReader.ReadInt32();
				for (int k = 0; k < num5; k++)
				{
					CreatItemInfo creatItemInfo = new CreatItemInfo();
					creatItemInfo.mPos = new Vector3(binaryReader.ReadSingle(), binaryReader.ReadSingle(), binaryReader.ReadSingle());
					creatItemInfo.mRotation = Quaternion.Euler(new Vector3(binaryReader.ReadSingle(), binaryReader.ReadSingle(), binaryReader.ReadSingle()));
					creatItemInfo.mItemId = binaryReader.ReadInt32();
					itemList.Add(creatItemInfo);
				}
				break;
			}
			}
			binaryReader.Close();
			memoryStream.Close();
		}
		if (!mNpcIdPosRotStand.Equals(string.Empty) && mNpcIdPosRotStand.Length > 1)
		{
			string[] array = mNpcIdPosRotStand.Split('_');
			for (int m = 0; m < array.Count(); m++)
			{
				string[] array2 = array[m].Split('~');
				int num6 = Convert.ToInt32(array2[0]);
				string[] array3 = array2[1].Split(';');
				string[] array4 = array3[0].Split(',');
				Vector3 pos = new Vector3(float.Parse(array4[0]), float.Parse(array4[1]), float.Parse(array4[2]));
				float rotY = float.Parse(array3[1]);
				BuildingNpc value2 = new BuildingNpc(num6, pos, rotY, isStand: true);
				npcIdPosRot.Add(num6, value2);
			}
		}
		if (!mNpcIdPosRotMove.Equals(string.Empty) && mNpcIdPosRotMove.Length > 1)
		{
			string[] array5 = mNpcIdPosRotMove.Split('_');
			for (int n = 0; n < array5.Count(); n++)
			{
				string[] array6 = array5[n].Split('~');
				int num7 = Convert.ToInt32(array6[0]);
				string[] array7 = array6[1].Split(';');
				string[] array8 = array7[0].Split(',');
				Vector3 pos2 = new Vector3(float.Parse(array8[0]), float.Parse(array8[1]), float.Parse(array8[2]));
				float rotY2 = float.Parse(array7[1]);
				BuildingNpc value3 = new BuildingNpc(num7, pos2, rotY2, isStand: false);
				npcIdPosRot.Add(num7, value3);
			}
		}
	}

	public void GetNpcInfo(out List<BuildingNpc> buildingNpcs)
	{
		buildingNpcs = new List<BuildingNpc>();
		if (!mNpcIdPosRotStand.Equals(string.Empty) && mNpcIdPosRotStand.Length > 1)
		{
			string[] array = mNpcIdPosRotStand.Split('_');
			for (int i = 0; i < array.Count(); i++)
			{
				string[] array2 = array[i].Split('~');
				int id = Convert.ToInt32(array2[0]);
				string[] array3 = array2[1].Split(';');
				string[] array4 = array3[0].Split(',');
				Vector3 pos = new Vector3(float.Parse(array4[0]), float.Parse(array4[1]), float.Parse(array4[2]));
				float rotY = float.Parse(array3[1]);
				BuildingNpc item = new BuildingNpc(id, pos, rotY, isStand: true);
				buildingNpcs.Add(item);
			}
		}
		if (!mNpcIdPosRotMove.Equals(string.Empty) && mNpcIdPosRotMove.Length > 1)
		{
			string[] array5 = mNpcIdPosRotMove.Split('_');
			for (int j = 0; j < array5.Count(); j++)
			{
				string[] array6 = array5[j].Split('~');
				int id2 = Convert.ToInt32(array6[0]);
				string[] array7 = array6[1].Split(';');
				string[] array8 = array7[0].Split(',');
				Vector3 pos2 = new Vector3(float.Parse(array8[0]), float.Parse(array8[1]), float.Parse(array8[2]));
				float rotY2 = float.Parse(array7[1]);
				BuildingNpc item2 = new BuildingNpc(id2, pos2, rotY2, isStand: false);
				buildingNpcs.Add(item2);
			}
		}
	}
}

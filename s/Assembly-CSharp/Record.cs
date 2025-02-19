using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class Record
{
	public enum RecordDefine
	{
		MAX_MISSION_COUNT = 1000,
		MAX_MISSIONFLAG_LENGTH = 10,
		MAX_MISSIONVALUE_LENGTH = 18,
		MAX_MISSION_NUM = 200,
		MAX_SHORTCUT_NUM = 10
	}

	public struct stMissionInfo
	{
		public int MissionID;

		public Dictionary<string, string> MissionFalg;
	}

	public struct stShortCutInfo
	{
		public int ShortCutID;

		public short ShortCutNum;
	}

	public struct stItemInfo
	{
		public int ItemObjID;

		public short ItemNum;
	}

	public class stChunkFlag
	{
		public byte flags;

		public Dictionary<IntVector4, int> info = new Dictionary<IntVector4, int>();
	}

	public struct stPlayerCreateInfo
	{
		public Color EyeColor;

		public Color HairColor;

		public Color SkinColor;

		public int[] mHairItem;

		public float[] mCurrentFaceData;

		public float[] mCurrentBodyCustomData;

		public float PlayerHeight;

		public float PlayerWidth;

		public int Sex;

		public Vector3 LeavePos;
	}

	public struct stVFVoxelInfo
	{
		public Dictionary<int, stChunkFlag> ChunkModifyFlags;

		public Dictionary<IntVector2, IntVector3> subTerDataDesc;
	}

	public struct stOther
	{
		public int idx;

		public Vector3 pos;

		public int gameTime;

		public int playTime;

		public DateTime mTime;

		public int GameMode;

		public int RandSeed;

		public int VegetationId;

		public int RandomMapID;

		public byte NewBieFlag;

		public float CurSpeed;

		public int Sex;

		public string PlayerName;

		public int ComMissionCount;

		public int PNGSize;

		public byte[] SaveTexture;
	}

	public class stNpcAttribute
	{
		public int Camp;

		public float UnarmedAtk;

		public float UnarmedAtkDist;

		public float InitDef;

		public int InitHP;

		public int Satiation;

		public int Comfort;

		public int MainWeaponID;

		public int SecondWeaponID;

		public int ArmorID;

		public float WalkSpeed;

		public float RunSpeed;

		public float JumpHeight;

		public float HorizonAngle;

		public float HorizonRad;

		public float WatchAngle;

		public float WatchRad;

		public int TurnSpeed;

		public List<int> SkillList = new List<int>();

		public string NpcIcon = string.Empty;

		public int ModelID;

		public Vector3 StartPoint;

		public int RandMissID;

		public byte MaxMissionNum;
	}

	public class stShopData
	{
		public int ItemObjID;

		public double CreateTime;

		public stShopData()
		{
		}

		public stShopData(int itemObjId, double createTime)
		{
			ItemObjID = itemObjId;
			CreateTime = createTime;
		}
	}

	public class stShopInfo
	{
		public Dictionary<int, stShopData> ShopList;

		public stShopInfo()
		{
			ShopList = new Dictionary<int, stShopData>();
		}
	}

	public class stDrawItem
	{
		public int mId;

		public Vector3 mPos;

		public Quaternion mRotation;
	}

	public class stMissionData
	{
		public MissionCommonData commonData;

		public TypeMonsterData monsterData;

		public TypeCollectData collectData;

		public TypeFollowData followData;

		public TypeSearchData searchData;

		public TypeUseItemData useData;

		public TypeMessengerData messData;

		public TypeTowerDefendsData towerData;
	}

	private const int File_OldVersion = 53;

	private const int File_Version = 60;

	public static int Cur_Version;

	public static int m_Auto = 2;

	public static void WriteColor(BinaryWriter pBW, Color pColor)
	{
		pBW.Write(pColor.r);
		pBW.Write(pColor.g);
		pBW.Write(pColor.b);
		pBW.Write(pColor.a);
	}

	public static void WriteVector3(BinaryWriter pBW, Vector3 pVector3)
	{
		pBW.Write(pVector3.x);
		pBW.Write(pVector3.y);
		pBW.Write(pVector3.z);
	}

	public static void WriteVector4(BinaryWriter pBW, IntVector4 pVector4)
	{
		pBW.Write(pVector4.x);
		pBW.Write(pVector4.y);
		pBW.Write(pVector4.z);
		pBW.Write(pVector4.w);
	}

	public static Color ReadColor(BinaryReader pbr)
	{
		return new Color(pbr.ReadSingle(), pbr.ReadSingle(), pbr.ReadSingle(), pbr.ReadSingle());
	}

	public static Vector3 ReadVector3(BinaryReader pbr)
	{
		return new Vector3(pbr.ReadSingle(), pbr.ReadSingle(), pbr.ReadSingle());
	}

	public static IntVector4 ReadVector4(BinaryReader pbr)
	{
		return new IntVector4(pbr.ReadInt32(), pbr.ReadInt32(), pbr.ReadInt32(), pbr.ReadInt32());
	}
}

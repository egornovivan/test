using System;
using System.Collections.Generic;
using Mono.Data.SqliteClient;

namespace CustomCharactor;

public class CustomMetaData
{
	public class Hair
	{
		public string icon;

		public string[] modelPath;
	}

	public class Head
	{
		public string icon;

		public string modelPath;
	}

	public class FaceTex
	{
		public string icon;

		public string path;
	}

	public class MetaData
	{
		public int mID;

		public int mType;

		public int mSex;

		public string mIcon;

		public string mModelPath;

		public MetaData(SqliteDataReader reader)
		{
			mID = Convert.ToInt32(reader.GetString(reader.GetOrdinal("ID")));
			mType = Convert.ToInt32(reader.GetString(reader.GetOrdinal("Type")));
			mSex = Convert.ToInt32(reader.GetString(reader.GetOrdinal("Sex")));
			mIcon = reader.GetString(reader.GetOrdinal("Icon"));
			mModelPath = reader.GetString(reader.GetOrdinal("ModelPath"));
		}
	}

	private static CustomMetaData instanceMale;

	private static CustomMetaData instanceFemale;

	private List<Head> headArray;

	private List<Hair> hairArray;

	public string upperBody;

	public string lowerBody;

	public string hands;

	public string feet;

	private static List<MetaData> mMetaDataList = new List<MetaData>();

	public static CustomMetaData InstanceMale
	{
		get
		{
			if (instanceMale == null)
			{
				instanceMale = new CustomMetaData();
				instanceMale.InitMale();
			}
			return instanceMale;
		}
	}

	public static CustomMetaData InstanceFemale
	{
		get
		{
			if (instanceFemale == null)
			{
				instanceFemale = new CustomMetaData();
				instanceFemale.InitFemale();
			}
			return instanceFemale;
		}
	}

	private void InitMale()
	{
		headArray = new List<Head>();
		headArray.Clear();
		hairArray = new List<Hair>();
		hairArray.Clear();
		foreach (MetaData mMetaData in mMetaDataList)
		{
			if (mMetaData.mSex == 2)
			{
				if (mMetaData.mType == 1)
				{
					Head head = new Head();
					head.icon = mMetaData.mIcon;
					head.modelPath = mMetaData.mModelPath;
					headArray.Add(head);
				}
				else if (mMetaData.mType == 2)
				{
					Hair hair = new Hair();
					hair.icon = mMetaData.mIcon;
					hair.modelPath = mMetaData.mModelPath.Split(';');
					hairArray.Add(hair);
				}
			}
		}
		upperBody = "Model/PlayerModel/Male-torso_0";
		lowerBody = "Model/PlayerModel/Male-legs_0";
		hands = "Model/PlayerModel/Male-hands_0";
		feet = "Model/PlayerModel/Male-feet_0";
	}

	private void InitFemale()
	{
		headArray = new List<Head>();
		headArray.Clear();
		hairArray = new List<Hair>();
		hairArray.Clear();
		foreach (MetaData mMetaData in mMetaDataList)
		{
			if (mMetaData.mSex == 1)
			{
				if (mMetaData.mType == 1)
				{
					Head head = new Head();
					head.icon = mMetaData.mIcon;
					head.modelPath = mMetaData.mModelPath;
					headArray.Add(head);
				}
				else if (mMetaData.mType == 2)
				{
					Hair hair = new Hair();
					hair.icon = mMetaData.mIcon;
					hair.modelPath = mMetaData.mModelPath.Split(';');
					hairArray.Add(hair);
				}
			}
		}
		upperBody = "Model/PlayerModel/Female-torso_0";
		lowerBody = "Model/PlayerModel/Female-legs_0";
		hands = "Model/PlayerModel/Female-hands_0";
		feet = "Model/PlayerModel/Female-feet_0";
	}

	public int GetHeadCount()
	{
		return headArray.Count;
	}

	public Head GetHead(int index)
	{
		if (index >= headArray.Count || index < 0)
		{
			return null;
		}
		return headArray[index];
	}

	public int GetHeadIndex(string path)
	{
		for (int i = 0; i < headArray.Count; i++)
		{
			if (headArray[i].modelPath == path)
			{
				return i;
			}
		}
		return -1;
	}

	public int GetHairCount()
	{
		return hairArray.Count;
	}

	public Hair GetHair(int index)
	{
		if (index >= hairArray.Count || index < 0)
		{
			return null;
		}
		return hairArray[index];
	}

	public int GetHairIndex(string hairPath_0)
	{
		for (int i = 0; i < hairArray.Count; i++)
		{
			if (hairArray[i].modelPath[0] == hairPath_0)
			{
				return i;
			}
		}
		return -1;
	}

	public static void LoadData()
	{
		mMetaDataList.Clear();
		SqliteDataReader sqliteDataReader = LocalDatabase.Instance.ReadFullTable("CustomMetaData");
		while (sqliteDataReader.Read())
		{
			MetaData item = new MetaData(sqliteDataReader);
			mMetaDataList.Add(item);
		}
	}
}

using UnityEngine;

public class AppearanceData
{
	public class BodyData
	{
		public string mFilePath;

		public string mName;

		public int mSlot;

		public BodyData(string pPath, string pName, int pSlot)
		{
			mFilePath = pPath;
			mName = pName;
			mSlot = pSlot;
		}
	}

	public class BodyDataMapping
	{
		private string[] mPaths;

		public int mSlot;

		public BodyDataMapping(string pName, int pSlot)
		{
			mPaths = pName.Split(';');
			mSlot = pSlot;
		}

		public int GetCount()
		{
			return mPaths.Length;
		}

		public void GetDataInfo(int index, out string path, out string name)
		{
			string[] array = mPaths[index].Split('-');
			path = array[0];
			name = array[1];
		}

		public string GetIconName(int index)
		{
			string[] array = mPaths[index].Split('-');
			return array[2];
		}
	}

	public static readonly int[] mBodyCombineElement = new int[9] { 1024, 1, 2048, 4096, 2, 4, 64, 128, 256 };

	public static readonly string[][] mBodilyElements = new string[13][]
	{
		new string[5] { "Head", "Scale", "Dummy01 Head", "Dummy R Eyeball", "Dummy L Eyeball" },
		new string[3] { "Neck", "Scale", "Dummy01 Neck" },
		new string[5] { "Thorax", "Scale", "Dummy01 Spine2", "Dummy01 R Clavicle", "Dummy01 L Clavicle" },
		new string[4] { "Waist", "Scale", "Dummy01 Spine1", "Dummy01 Spine" },
		new string[3] { "Belly", "Scale", "Dummy01 Belly" },
		new string[4] { "Breast", "Scale", "Dummy01 L Breast", "Dummy01 R Breast" },
		new string[3] { "PelvicCavity", "Scale", "Dummy01 Pelvis" },
		new string[6] { "UpperArm", "Scale", "Dummy01 L UpperArm", "Dummy02 L UpperArm", "Dummy01 R UpperArm", "Dummy02 R UpperArm" },
		new string[6] { "Forearm", "Scale", "Dummy01 L Forearm", "Dummy02 L Forearm", "Dummy01 R Forearm", "Dummy02 R Forearm" },
		new string[4] { "Hand", "Scale", "Dummy01 L Hand", "Dummy01 R Hand" },
		new string[6] { "Thigh", "Scale", "Dummy01 L Thigh", "Dummy02 L Thigh", "Dummy01 R Thigh", "Dummy02 R Thigh" },
		new string[6] { "Calf", "Scale", "Dummy01 L Calf", "Dummy02 L Calf", "Dummy01 R Calf", "Dummy02 R Calf" },
		new string[6] { "Foot", "Scale", "Dummy01 L Foot", "Dummy01 L Toe", "Dummy01 R Foot", "Dummy01 R Toe" }
	};

	internal int mID;

	internal string mPrefabName;

	internal string mFBXPath;

	internal BodyDataMapping[] mBodyData = new BodyDataMapping[mBodyCombineElement.Length];

	internal int[] mCustomStyle = new int[mBodyCombineElement.Length];

	internal Vector3[][] mBodyCustomDatas = new Vector3[mBodilyElements.Length][];

	internal float[] mCurrentBodyCustomData = new float[mBodilyElements.Length];

	internal Color mSkinColor = new Color(0.88f, 0.88f, 0.88f, 1f);

	internal Color mEyeColor = new Color(0.88f, 0.88f, 0.88f, 1f);

	internal Color mHairColor = new Color(0.88f, 0.88f, 0.88f, 1f);

	internal float mHeight = 1f;

	internal float mWith = 1f;

	internal int mSex = 2;

	public AppearanceData(AppearanceData other)
	{
		for (int i = 0; i < mBodyCombineElement.Length; i++)
		{
			mCustomStyle[i] = 0;
		}
		mID = other.mID;
		mPrefabName = other.mPrefabName;
		mSex = other.mSex;
		mFBXPath = other.mFBXPath;
		mBodyData = other.mBodyData;
		mBodyCustomDatas = other.mBodyCustomDatas;
		mEyeColor = other.mEyeColor;
		mHairColor = other.mHairColor;
		mSkinColor = other.mSkinColor;
		mWith = other.mWith;
		mHeight = other.mHeight;
	}

	public AppearanceData()
	{
		for (int i = 0; i < mBodilyElements.Length; i++)
		{
			mBodyCustomDatas[i] = new Vector3[(mBodilyElements[i].Length - 2) * 3];
		}
		for (int j = 0; j < mBodyCombineElement.Length; j++)
		{
			mCustomStyle[j] = 0;
		}
	}

	public static bool IsCombineType(int pSlot)
	{
		for (int i = 0; i < mBodyCombineElement.Length; i++)
		{
			if (mBodyCombineElement[i] == pSlot)
			{
				return true;
			}
		}
		return false;
	}

	public bool IsSameHair(int hairIndex)
	{
		return mCustomStyle[0] == hairIndex;
	}

	public Color GetSkinColor()
	{
		return mSkinColor;
	}

	public Color GetEyeColor()
	{
		return mEyeColor;
	}

	public Color GetHairColor()
	{
		return mHairColor;
	}

	public float GetHeight()
	{
		return mHeight;
	}

	public float GetWith()
	{
		return mWith;
	}

	public float[] GetCurBodyCustomData()
	{
		return mCurrentBodyCustomData;
	}
}

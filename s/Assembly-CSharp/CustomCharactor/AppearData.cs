using System.IO;
using PETools;
using UnityEngine;

namespace CustomCharactor;

public class AppearData
{
	private const int VERSION_0000 = 0;

	private const int VERSION_0001 = 1;

	private const int CURRENT_VERSION = 1;

	private static readonly Color[] s_skinColors = new Color[10]
	{
		Color.white,
		new Color(0.9098f, 0.8471f, 0.6941f),
		new Color(0.4039f, 0.3373f, 0.0157f),
		new Color(0.9255f, 0.7725f, 0.7216f),
		new Color(0.251f, 0.2392f, 0.1725f),
		new Color(0.7961f, 0.6784f, 0.5255f),
		new Color(0.7451f, 0.6078f, 0.5647f),
		new Color(0.9333f, 0.8941f, 0.796f),
		new Color(0.7804f, 0.6941f, 0.5843f),
		new Color(0.8118f, 0.7961f, 0.7882f)
	};

	private static readonly Color[] s_hairColors = new Color[10]
	{
		new Color(0.5255f, 0.5255f, 0.5255f),
		new Color(0.8157f, 0.4078f, 0.1647f),
		new Color(0.3451f, 0.1294f, 0.0157f),
		new Color(0.8549f, 0.4078f, 0.4941f),
		new Color(0.902f, 0.6157f, 0.4118f),
		new Color(0.7922f, 0.9059f, 0.4196f),
		new Color(0.6275f, 0.4078f, 0.8667f),
		new Color(0.4353f, 0.5451f, 0.8783f),
		new Color(1f, 0.7843f, 0.1255f),
		new Color(0.7098f, 0.2902f, 0.302f)
	};

	private float[] mMorphWeightArray = new float[27];

	public Color mEyeColor;

	public Color mLipColor;

	public Color mSkinColor;

	public Color mHairColor;

	public AppearData()
	{
		Default();
	}

	public void Default()
	{
		mSkinColor = Color.white;
		mEyeColor = Color.white;
		mHairColor = Color.white;
		mLipColor = Color.red;
		for (int i = 0; i < 27; i++)
		{
			SetWeight((EMorphItem)i, 0f);
		}
	}

	public void Random()
	{
		mSkinColor = s_skinColors[UnityEngine.Random.Range(0, s_skinColors.Length)];
		mEyeColor = Color.white;
		mHairColor = s_hairColors[UnityEngine.Random.Range(0, s_hairColors.Length)];
		mLipColor = Color.red;
		RandomMorphWeight();
	}

	public float GetWeight(EMorphItem eMorphItem)
	{
		return mMorphWeightArray[(int)eMorphItem];
	}

	public void SetWeight(EMorphItem eMorphItem, float weight)
	{
		if (eMorphItem != EMorphItem.MaxUpperbody && eMorphItem != EMorphItem.LegWaist && eMorphItem != EMorphItem.MaxHand && eMorphItem != EMorphItem.MaxLowerbody && eMorphItem != EMorphItem.TorsoUpperLeg)
		{
			mMorphWeightArray[(int)eMorphItem] = Mathf.Clamp(weight, -1f, 1f);
			if (eMorphItem == EMorphItem.Belly)
			{
				mMorphWeightArray[21] = mMorphWeightArray[18];
			}
			if (eMorphItem == EMorphItem.Waist)
			{
				mMorphWeightArray[22] = mMorphWeightArray[19];
			}
			if (eMorphItem == EMorphItem.LowerLeg)
			{
				mMorphWeightArray[26] = mMorphWeightArray[24];
			}
			if (eMorphItem == EMorphItem.LowerArm)
			{
				mMorphWeightArray[25] = mMorphWeightArray[17];
			}
			if (eMorphItem == EMorphItem.UpperLeg)
			{
				mMorphWeightArray[20] = mMorphWeightArray[23];
			}
		}
	}

	public void RandomMorphWeight()
	{
		for (int i = 0; i < 27; i++)
		{
			SetWeight((EMorphItem)i, UnityEngine.Random.Range(-1f, 1f));
		}
	}

	public void MaxMorphWeight()
	{
		for (int i = 0; i < 27; i++)
		{
			SetWeight((EMorphItem)i, 1f);
		}
	}

	public void MinMorphWeight()
	{
		for (int i = 0; i < 27; i++)
		{
			SetWeight((EMorphItem)i, -1f);
		}
	}

	public byte[] Serialize()
	{
		return PETools.Serialize.Export(delegate(BinaryWriter w)
		{
			w.Write(1);
			w.Write(mMorphWeightArray.Length);
			for (int i = 0; i < mMorphWeightArray.Length; i++)
			{
				w.Write(mMorphWeightArray[i]);
			}
			PETools.Serialize.WriteColor(w, mEyeColor);
			PETools.Serialize.WriteColor(w, mLipColor);
			PETools.Serialize.WriteColor(w, mSkinColor);
			PETools.Serialize.WriteColor(w, mHairColor);
		});
	}

	public void Deserialize(byte[] data)
	{
		PETools.Serialize.Import(data, delegate(BinaryReader r)
		{
			int num = r.ReadInt32();
			if (num > 1)
			{
				Debug.LogError("version:" + num + " greater than current version:" + 1);
			}
			else
			{
				int num2 = r.ReadInt32();
				if (num2 == 27)
				{
					for (int i = 0; i < num2; i++)
					{
						mMorphWeightArray[i] = r.ReadSingle();
					}
					mEyeColor = PETools.Serialize.ReadColor(r);
					mLipColor = PETools.Serialize.ReadColor(r);
					mSkinColor = PETools.Serialize.ReadColor(r);
					mHairColor = PETools.Serialize.ReadColor(r);
				}
			}
		});
	}
}

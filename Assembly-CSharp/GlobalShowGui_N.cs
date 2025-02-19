using System.Collections.Generic;
using ItemAsset;
using UnityEngine;

public class GlobalShowGui_N : MonoBehaviour
{
	private const float WeightTime = 0.5f;

	private static GlobalShowGui_N mInstance;

	public GlobalShowItem_N mPrefab;

	public UILabel mReviveTimeLabal;

	private float mReviveTime;

	private List<ItemSample> mShowList = new List<ItemSample>();

	private List<string> mShowStrList = new List<string>();

	private float mWeightTime = 0.5f;

	public static GlobalShowGui_N Instance => mInstance;

	private void Awake()
	{
		mInstance = this;
	}

	public static void ShowString(string showString)
	{
		if (null != mInstance)
		{
			mInstance.AddShow(showString);
		}
	}

	public void AddShow(ItemSample itemGrid)
	{
		if (itemGrid.protoId < 100000000)
		{
			mShowList.Add(itemGrid);
		}
	}

	public void AddShow(string des)
	{
		mShowStrList.Add(des);
	}

	public void SetReviveTime(float reviveTime)
	{
		mReviveTime = reviveTime;
		if (mReviveTime > 0f)
		{
			mReviveTimeLabal.enabled = true;
		}
	}

	private void Update()
	{
		if (mWeightTime > 0f)
		{
			mWeightTime -= Time.deltaTime;
		}
		else if (mShowStrList.Count > 0)
		{
			GlobalShowItem_N globalShowItem_N = Object.Instantiate(mPrefab);
			globalShowItem_N.transform.parent = base.transform;
			globalShowItem_N.transform.localPosition = Vector3.zero;
			globalShowItem_N.transform.localRotation = Quaternion.identity;
			globalShowItem_N.transform.localScale = Vector3.one;
			globalShowItem_N.InitItem(mShowStrList[0]);
			mShowStrList.RemoveAt(0);
			mWeightTime = 0.5f;
		}
		else if (mShowList.Count > 0)
		{
			GlobalShowItem_N globalShowItem_N2 = Object.Instantiate(mPrefab);
			globalShowItem_N2.transform.parent = base.transform;
			globalShowItem_N2.transform.localPosition = Vector3.zero;
			globalShowItem_N2.transform.localRotation = Quaternion.identity;
			globalShowItem_N2.transform.localScale = Vector3.one;
			globalShowItem_N2.InitItem(mShowList[0]);
			mShowList.RemoveAt(0);
			mWeightTime = 0.5f;
		}
		if (mReviveTime > 0f)
		{
			mReviveTime -= Time.deltaTime;
			if (mReviveTime <= 0f)
			{
				mReviveTime = 0f;
				mReviveTimeLabal.enabled = false;
			}
			mReviveTimeLabal.text = "ReviveTime:" + (int)mReviveTime;
		}
	}
}

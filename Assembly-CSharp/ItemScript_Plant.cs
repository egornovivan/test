using UnityEngine;

public class ItemScript_Plant : ItemScript
{
	public delegate void PlantUpdated();

	private const double VarPerOp = 30.0;

	private FarmPlantLogic mPlant;

	private int mGrowTimeIndex;

	private bool mIsDead;

	private GameObject mModel;

	private byte mTerrainType;

	public PlantUpdated plantUpdated;

	public GameObject mLowerWaterTex;

	public GameObject mLowerCleanTex;

	private bool mInitBounds;

	public override void OnConstruct()
	{
		base.OnConstruct();
		mPlant = GetComponentInParent<FarmPlantLogic>();
		if (null == mPlant)
		{
			Debug.LogError(string.Concat(base.transform.position, "No logic layer!"));
			return;
		}
		if (GetComponent<DragItemMousePickPlant>() == null)
		{
			base.gameObject.AddComponent<DragItemMousePickPlant>();
		}
		mGrowTimeIndex = mPlant.mGrowTimeIndex;
		mIsDead = mPlant.mDead;
		ResetModel(mPlant.mGrowTimeIndex, mPlant.mPlantInfo);
		UpdatModel();
	}

	public void ResetModel(int index, PlantInfo plantInfo, bool isdead = false)
	{
		if (!mInitBounds)
		{
			InitBounds(plantInfo);
		}
		if (index > plantInfo.mGrowTime.Length - 1)
		{
			index = plantInfo.mGrowTime.Length - 1;
		}
		mGrowTimeIndex = index;
		mIsDead = isdead;
		if (null != mModel)
		{
			Object.Destroy(mModel);
		}
		if (plantInfo.mDeadModePath[index] != "0")
		{
			Object original = ((!mIsDead) ? Resources.Load(plantInfo.mModelPath[index]) : Resources.Load(plantInfo.mDeadModePath[index]));
			mModel = Object.Instantiate(original) as GameObject;
			mModel.transform.parent = base.rootGameObject.transform;
			mModel.transform.localPosition = Vector3.zero;
			mModel.transform.localScale = ((mGrowTimeIndex != 1) ? Vector3.one : (0.5f * Vector3.one));
			mModel.transform.rotation = Quaternion.identity;
			ColliderClickTrigger colliderClickTrigger = mModel.AddComponent<ColliderClickTrigger>();
			colliderClickTrigger.mTrigerTarget = base.gameObject;
			colliderClickTrigger.mLeftBtn = false;
			colliderClickTrigger.mRightBtn = true;
		}
		if (plantUpdated != null)
		{
			plantUpdated();
		}
	}

	public void UpdatModel()
	{
		if (!(null != mPlant))
		{
			return;
		}
		if (mGrowTimeIndex != mPlant.mGrowTimeIndex || mIsDead != mPlant.mDead)
		{
			ResetModel(mPlant.mGrowTimeIndex, mPlant.mPlantInfo, mPlant.mDead);
		}
		if (!mPlant.mDead && !mPlant.IsRipe)
		{
			if (mLowerWaterTex.activeSelf != mPlant.NeedWater)
			{
				mLowerWaterTex.SetActive(mPlant.NeedWater);
			}
			if (mLowerCleanTex.activeSelf != mPlant.NeedClean)
			{
				mLowerCleanTex.SetActive(mPlant.NeedClean);
			}
		}
		else
		{
			if (mLowerWaterTex.activeSelf)
			{
				mLowerWaterTex.SetActive(value: false);
			}
			if (mLowerCleanTex.activeSelf)
			{
				mLowerCleanTex.SetActive(value: false);
			}
		}
		if (null != mModel && null != mModel.GetComponent<Collider>())
		{
			float y = mModel.GetComponent<Collider>().bounds.max.y + 0.2f;
			mLowerWaterTex.transform.position = new Vector3(mLowerWaterTex.transform.position.x, y, mLowerWaterTex.transform.position.z);
			mLowerCleanTex.transform.position = new Vector3(mLowerCleanTex.transform.position.x, y, mLowerCleanTex.transform.position.z);
		}
		if (plantUpdated != null)
		{
			plantUpdated();
		}
	}

	private void InitBounds(PlantInfo plantInfo)
	{
		mInitBounds = true;
		ItemDraggingBounds itemDraggingBounds = base.gameObject.AddComponent<ItemDraggingBounds>();
		Vector3 size = plantInfo.mSize * Vector3.one;
		size.y = plantInfo.mHeight;
		itemDraggingBounds.ResetBounds(0.5f * plantInfo.mHeight * Vector3.up, size);
	}
}

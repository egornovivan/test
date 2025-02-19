using System.Collections.Generic;
using Railway;
using UnityEngine;

public class UIRailLine : MonoBehaviour
{
	public delegate void ClickRailStation(bool isConnect, int mLineIndex, int mStationID);

	public GameObject mLine;

	public GameObject mStage;

	public GameObject mEndStage;

	public UISprite mSprCar;

	public Route mRoute;

	public int mScale = 1;

	public bool mCanSeleted = true;

	public bool isSelected;

	public List<UIRailStation> mStationList = new List<UIRailStation>();

	public List<Vector2> mStagePosList = new List<Vector2>();

	private List<UISprite> mSprStageList = new List<UISprite>();

	private List<UITiledSprite> mTsLineList = new List<UITiledSprite>();

	public event ClickRailStation mSelectedLine;

	public void Init(Route _Route)
	{
		mRoute = _Route;
	}

	public UIRailStation FindStation(int stationID)
	{
		return mStationList.Find((UIRailStation st) => st.mRailPointData.id == stationID);
	}

	public int FindStationIndex(int stationID)
	{
		return mStationList.FindIndex((UIRailStation st) => st.mRailPointData.id == stationID);
	}

	public void CreateLine()
	{
		if (mLine == null || mStage == null || mStagePosList == null)
		{
			return;
		}
		for (int i = 0; i < mRoute.pointCount; i++)
		{
			GameObject gameObject = ((mRoute.GetPointByIndex(i).pointType != Point.EType.End) ? Object.Instantiate(mStage) : Object.Instantiate(mEndStage));
			gameObject.transform.parent = base.gameObject.transform;
			if (mRoute.GetPointByIndex(i).pointType == Point.EType.Joint)
			{
				gameObject.transform.localScale = new Vector3(5 * mScale, 5 * mScale, -1f);
			}
			else if (mRoute.GetPointByIndex(i).pointType == Point.EType.End)
			{
				gameObject.transform.localScale = new Vector3(14 * mScale, 12 * mScale, -1f);
			}
			else
			{
				gameObject.transform.localScale = new Vector3(10 * mScale, 10 * mScale, -1f);
			}
			UIRailStation component = gameObject.GetComponent<UIRailStation>();
			if (component != null)
			{
				component.Init(mRoute.GetPointByIndex(i));
				component.mClickStatge = OnClickStatge;
			}
			mStationList.Add(component);
			gameObject.SetActive(value: true);
			UISprite component2 = gameObject.GetComponent<UISprite>();
			mSprStageList.Add(component2);
			if (i != 0)
			{
				GameObject gameObject2 = Object.Instantiate(mLine);
				gameObject2.transform.parent = base.gameObject.transform;
				gameObject2.transform.localScale = new Vector3(10 * mScale, 10 * mScale, -1f);
				gameObject2.SetActive(value: true);
				UITiledSprite component3 = gameObject2.GetComponent<UITiledSprite>();
				mTsLineList.Add(component3);
				component = gameObject2.GetComponent<UIRailStation>();
				if (component != null && i - 1 >= 0)
				{
					component.Init(mRoute.GetPointByIndex(i - 1));
					component.mClickStatge = OnClickStatge;
				}
			}
		}
	}

	public void ResetLinePos(UIMapCtrl mMapCtrl)
	{
		mStagePosList.Clear();
		for (int i = 0; i < mRoute.pointCount; i++)
		{
			Vector3 vector = new Vector3(mRoute.GetPointByIndex(i).position.x, mRoute.GetPointByIndex(i).position.z, 0f);
			Vector3 vector2 = vector - mMapCtrl.mMapCamera.gameObject.transform.localPosition;
			int mCameraSizeCount = mMapCtrl.mCameraSizeCount;
			mStagePosList.Add(new Vector2(vector2.x / (float)mCameraSizeCount, vector2.y / (float)mCameraSizeCount));
		}
		UpdateLinePos();
	}

	private void UpdateLinePos()
	{
		if (mLine == null || mStage == null || mStagePosList == null)
		{
			return;
		}
		for (int i = 0; i < mStagePosList.Count && i < mSprStageList.Count; i++)
		{
			float x = mStagePosList[i].x;
			float y = mStagePosList[i].y;
			mSprStageList[i].gameObject.transform.localPosition = new Vector3(x, y, -1f);
			if (i != 0)
			{
				x = (mStagePosList[i].x + mStagePosList[i - 1].x) / 2f;
				y = (mStagePosList[i].y + mStagePosList[i - 1].y) / 2f;
				float num = mStagePosList[i].x - mStagePosList[i - 1].x;
				float num2 = mStagePosList[i].y - mStagePosList[i - 1].y;
				mTsLineList[i - 1].gameObject.transform.localPosition = new Vector3(x, y, -1f);
				mTsLineList[i - 1].gameObject.transform.localEulerAngles = new Vector3(0f, 0f, Mathf.Atan2(num2, num) * 57.29578f);
				float x2 = Mathf.Sqrt(num * num + num2 * num2);
				mTsLineList[i - 1].gameObject.transform.localScale = new Vector3(x2, 4f, -1f);
			}
		}
	}

	private void OnClickStatge(int mStationID)
	{
		if (mCanSeleted && this.mSelectedLine != null)
		{
			this.mSelectedLine(isConnect: true, mRoute.id, mStationID);
		}
	}

	public void SetSelected(bool _isSelected)
	{
		for (int i = 0; i < mSprStageList.Count; i++)
		{
			if (i < mRoute.pointCount)
			{
				if (mRoute.GetPointByIndex(i).pointType == Point.EType.End)
				{
					mSprStageList[i].spriteName = ((!_isSelected) ? "railbegin_1" : "railbegin");
				}
				else
				{
					mSprStageList[i].spriteName = ((!_isSelected) ? "railpoint_1" : "railpoint");
				}
			}
		}
		foreach (UITiledSprite mTsLine in mTsLineList)
		{
			mTsLine.spriteName = ((!_isSelected) ? "railline_1" : "railline");
		}
		isSelected = _isSelected;
	}

	public void UpdateCarPos(UIMapCtrl mMapCtrl)
	{
		if (mRoute.trainId == -1 || base.gameObject == null || mSprCar == null)
		{
			if (mSprCar != null)
			{
				mSprCar.enabled = false;
			}
			return;
		}
		mSprCar.enabled = true;
		Vector3 forward = default(Vector3);
		Vector3 up = default(Vector3);
		Vector3 trainPosition = mRoute.GetTrainPosition(out forward, out up);
		Vector3 vector = new Vector3(trainPosition.x, trainPosition.z, 0f);
		Vector3 vector2 = vector - mMapCtrl.mMapCamera.gameObject.transform.localPosition;
		int mCameraSizeCount = mMapCtrl.mCameraSizeCount;
		Vector2 vector3 = new Vector2(vector2.x / (float)mCameraSizeCount, vector2.y / (float)mCameraSizeCount);
		mSprCar.transform.localPosition = new Vector3(vector3.x, vector3.y, 0f);
		if (mRoute.moveDir == -1)
		{
			mSprCar.transform.localEulerAngles = new Vector3(180f, 0f, Mathf.Atan2(forward.x, forward.z) * 57.29578f + 180f);
		}
		else
		{
			mSprCar.transform.localEulerAngles = new Vector3(180f, 0f, Mathf.Atan2(forward.x, forward.z) * 57.29578f);
		}
	}

	private void Start()
	{
	}
}

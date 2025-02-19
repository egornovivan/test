using System.Collections.Generic;
using Railway;
using UnityEngine;

public class UIDisconnectionLine : MonoBehaviour
{
	public delegate void ClickRailStation(bool isConnect, int mLineIndex, int mStationID);

	public GameObject mLine;

	public GameObject mStage;

	public GameObject mEndStage;

	public bool mCanSeleted = true;

	public bool isSelected;

	public int mScale = 1;

	public List<Point> mPointList;

	public int mIndex = -1;

	public List<UIRailStation> mStationList = new List<UIRailStation>();

	public List<Vector2> mStagePosList = new List<Vector2>();

	private List<UISprite> mSprStageList = new List<UISprite>();

	private List<UITiledSprite> mTsLineList = new List<UITiledSprite>();

	public event ClickRailStation mSelectedLine;

	private void Start()
	{
	}

	private void Update()
	{
	}

	public UIRailStation FindStation(int stationID)
	{
		return mStationList.Find((UIRailStation st) => st.mRailPointData.id == stationID);
	}

	public int FindStationIndex(int stationID)
	{
		return mStationList.FindIndex((UIRailStation st) => st.mRailPointData.id == stationID);
	}

	public void Init(List<Point> _PointList, int index)
	{
		mPointList = _PointList;
		mIndex = index;
	}

	public void CreateLine()
	{
		if (mLine == null || mStage == null || mStagePosList == null)
		{
			return;
		}
		for (int i = 0; i < mPointList.Count; i++)
		{
			GameObject gameObject = ((mPointList[i].pointType != Point.EType.End) ? Object.Instantiate(mStage) : Object.Instantiate(mEndStage));
			gameObject.transform.parent = base.gameObject.transform;
			if (mPointList[i].pointType == Point.EType.Joint)
			{
				gameObject.transform.localScale = new Vector3(5 * mScale, 5 * mScale, -1f);
			}
			else if (mPointList[i].pointType == Point.EType.End)
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
				component.Init(mPointList[i]);
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
				gameObject2.SetActive(value: false);
				UITiledSprite component3 = gameObject2.GetComponent<UITiledSprite>();
				mTsLineList.Add(component3);
				component = gameObject2.GetComponent<UIRailStation>();
				if (component != null && i - 1 >= 0)
				{
					component.Init(mPointList[i - 1]);
					component.mClickStatge = OnClickStatge;
				}
			}
		}
	}

	private void OnClickStatge(int mStationID)
	{
		if (mCanSeleted && this.mSelectedLine != null)
		{
			this.mSelectedLine(isConnect: false, mIndex, mStationID);
		}
	}

	public void SetSelected(bool _isSelected)
	{
		for (int i = 0; i < mSprStageList.Count; i++)
		{
			if (i < mPointList.Count)
			{
				if (mPointList[i].pointType == Point.EType.End)
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
			mTsLine.gameObject.SetActive(_isSelected);
		}
		isSelected = _isSelected;
	}

	public void ResetLinePos(UIMapCtrl mMapCtrl)
	{
		mStagePosList.Clear();
		for (int i = 0; i < mPointList.Count; i++)
		{
			Vector3 vector = new Vector3(mPointList[i].position.x, mPointList[i].position.z, 0f);
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
			int num = (int)mStagePosList[i].x;
			int num2 = (int)mStagePosList[i].y;
			mSprStageList[i].gameObject.transform.localPosition = new Vector3(num, num2, -1f);
			if (i != 0)
			{
				num = (int)((mStagePosList[i].x + mStagePosList[i - 1].x) / 2f);
				num2 = (int)((mStagePosList[i].y + mStagePosList[i - 1].y) / 2f);
				int num3 = (int)(mStagePosList[i].x - mStagePosList[i - 1].x);
				int num4 = (int)(mStagePosList[i].y - mStagePosList[i - 1].y);
				mTsLineList[i - 1].gameObject.transform.localPosition = new Vector3(num, num2, -1f);
				mTsLineList[i - 1].gameObject.transform.localEulerAngles = new Vector3(0f, 0f, Mathf.Atan2(num4, num3) * 57.29578f);
				int num5 = (int)Mathf.Sqrt(num3 * num3 + num4 * num4);
				mTsLineList[i - 1].gameObject.transform.localScale = new Vector3(num5, 4f, -1f);
			}
		}
	}
}

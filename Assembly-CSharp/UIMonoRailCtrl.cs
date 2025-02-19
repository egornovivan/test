using System;
using System.Collections.Generic;
using ItemAsset;
using Pathea;
using PatheaScript;
using Railway;
using UnityEngine;

public class UIMonoRailCtrl : UIBaseWidget
{
	public delegate void OnWayPointEnevt(Point mRailPointData);

	public delegate void OnCreateWayEnevt(Point mRailPointData, string _LineName);

	private static UIMonoRailCtrl mInstance;

	public UILabel mTitleLineName;

	public UIInput mLineName;

	public UILabel mLine;

	public UILabel mLbStationCount;

	public UILabel mLbNodeCount;

	public UILabel mLbRangeAblity;

	public UILabel mLbOneWaytime;

	public Grid_N mRailIcon;

	public GameObject mBtnStop;

	public UILabel mLbBtnStop;

	public GameObject mLineInfo;

	public GameObject mStationInfo;

	public UILabel mTitleStationName;

	public UIInput mStationName;

	public UILabel mArriveTime;

	public UIInput mRedestanceTime;

	public GameObject mLinesContent;

	public GameObject mLinePrefab;

	public GameObject mDisLinePrefab;

	public UIMapCtrl mMapCtrl;

	public GameObject mMapMash;

	public UISprite mPlayerPos;

	private List<UIRailLine> mRailLineList = new List<UIRailLine>();

	private List<UIDisconnectionLine> mDisRailLineList = new List<UIDisconnectionLine>();

	private List<Route> mRouteList = new List<Route>();

	private int mSelectedLineId = -1;

	private int mSelectedLineIndex = -1;

	private UIInput mLastFouceInput;

	private bool isConnectLine;

	private UIRailStation mSelectedStation;

	public static UIMonoRailCtrl Instance => mInstance;

	public event OnWayPointEnevt e_BtnStop;

	public event OnCreateWayEnevt e_BtnStar;

	public event System.Action e_InitWindow;

	public event Action<int, ItemObject> e_SetTrain;

	public event Action<int, int> e_SetTrainToStation;

	public event Action<int, string> e_ResetPointName;

	public event Action<int, string> e_ResetRouteName;

	public event Action<int, float> e_ResetPointTime;

	private void Start()
	{
	}

	private void Update()
	{
		if (!(GameUI.Instance.mMainPlayer == null))
		{
			UpdatePlayerPos();
			UpdateCarState();
			UpdateInputState();
			UpdateStationArriveTime();
		}
	}

	private void UpdateInputState()
	{
		if (mLineName.selected)
		{
			mLastFouceInput = mLineName;
		}
		else if (mLastFouceInput == mLineName)
		{
			OnLineNameChange(mLineName.text);
			mLastFouceInput = null;
		}
		if (mStationName.selected)
		{
			mLastFouceInput = mStationName;
		}
		else if (mLastFouceInput == mStationName)
		{
			OnStationNameChange(mStationName.text);
			mLastFouceInput = null;
		}
		if (mRedestanceTime.selected)
		{
			mLastFouceInput = mRedestanceTime;
		}
		else if (mLastFouceInput == mRedestanceTime)
		{
			OnStationResidenceTimeChange(mRedestanceTime.text);
			mLastFouceInput = null;
		}
	}

	private void UpdateCarState()
	{
		for (int i = 0; i < mRailLineList.Count; i++)
		{
			mRailLineList[i].UpdateCarPos(mMapCtrl);
		}
	}

	private void UpdatePlayerPos()
	{
		Vector3 position = GameUI.Instance.mMainPlayer.position;
		Vector3 vector = new Vector3(position.x, position.z, 0f) - mMapCtrl.mMapCamera.gameObject.transform.localPosition;
		int mCameraSizeCount = mMapCtrl.mCameraSizeCount;
		Vector2 vector2 = new Vector2(vector.x / (float)mCameraSizeCount, vector.y / (float)mCameraSizeCount);
		mPlayerPos.transform.localPosition = new Vector3(vector2.x, vector2.y, 0f);
		mPlayerPos.transform.localRotation = Quaternion.Euler(0f, 0f, 0f - GameUI.Instance.mMainPlayer.tr.rotation.eulerAngles.y);
	}

	public override void OnCreate()
	{
		mInstance = this;
		UIMapCtrl uIMapCtrl = mMapCtrl;
		uIMapCtrl.mMapMove = (OnGuiBtnClicked)Delegate.Combine(uIMapCtrl.mMapMove, new OnGuiBtnClicked(UpdateAllRailLinePos));
		UIMapCtrl uIMapCtrl2 = mMapCtrl;
		uIMapCtrl2.mMapZoomed = (OnGuiBtnClicked)Delegate.Combine(uIMapCtrl2.mMapZoomed, new OnGuiBtnClicked(UpdateAllRailLinePos));
		mRailIcon.SetItemPlace(ItemPlaceType.IPT_Rail, 0);
		mRailIcon.onLeftMouseClicked = PickTrain;
		mRailIcon.onDropItem = DropTrain;
		ClearUIInfo();
		mMapMash.SetActive(!GameConfig.IsMultiMode);
		base.OnCreate();
	}

	protected override void InitWindow()
	{
		base.InitWindow();
		if (this.e_InitWindow != null)
		{
			this.e_InitWindow();
		}
	}

	private void ClearUIInfo()
	{
		mTitleLineName.text = string.Empty;
		mLineName.text = string.Empty;
		mLbStationCount.text = string.Empty;
		mLbNodeCount.text = string.Empty;
		mLbRangeAblity.text = string.Empty;
		mLbOneWaytime.text = string.Empty;
		mTitleStationName.text = string.Empty;
		mStationName.text = string.Empty;
		mArriveTime.text = string.Empty;
		mRedestanceTime.text = string.Empty;
		mBtnStop.SetActive(value: false);
		mRailIcon.SetItem(null);
	}

	public override void Show()
	{
		if (mMapCtrl != null && null != GameUI.Instance.mMainPlayer)
		{
			Vector3 position = GameUI.Instance.mMainPlayer.position;
			Vector3 localPosition = mMapCtrl.mMapCamera.transform.localPosition;
			localPosition.y = position.z;
			localPosition.x = position.x;
			mMapCtrl.mMapCamera.transform.localPosition = localPosition;
		}
		UpdateAllRailLinePos();
		base.Show();
	}

	public void AddMonoRail(Route route)
	{
		if (!mRouteList.Contains(route))
		{
			mRouteList.Add(route);
			AddUIRailLine(route);
		}
	}

	public void RemoveMonoRail(Route route)
	{
		if (mRouteList.Contains(route))
		{
			DeleteUIRailLine(route);
			mRouteList.Remove(route);
			mRailIcon.SetItem(null);
		}
	}

	public void ReDrawDisRailLine(List<List<Point>> mRailPointList)
	{
		RemoveAllDisLine();
		for (int i = 0; i < mRailPointList.Count; i++)
		{
			GameObject gameObject = UnityEngine.Object.Instantiate(mDisLinePrefab);
			gameObject.transform.parent = mLinesContent.transform;
			gameObject.transform.localPosition = new Vector3(0f, 0f, 0f);
			gameObject.transform.localScale = new Vector3(1f, 1f, 1f);
			UIDisconnectionLine component = gameObject.GetComponent<UIDisconnectionLine>();
			component.Init(mRailPointList[i], i);
			component.CreateLine();
			component.ResetLinePos(mMapCtrl);
			component.mSelectedLine += OnSeletedLine;
			mDisRailLineList.Add(component);
		}
	}

	private void RemoveAllDisLine()
	{
		foreach (UIDisconnectionLine mDisRailLine in mDisRailLineList)
		{
			mDisRailLine.transform.parent = null;
			UnityEngine.Object.Destroy(mDisRailLine.gameObject);
		}
		mDisRailLineList.Clear();
	}

	private void AddUIRailLine(Route _rail)
	{
		GameObject gameObject = UnityEngine.Object.Instantiate(mLinePrefab);
		gameObject.transform.parent = mLinesContent.transform;
		gameObject.transform.localPosition = new Vector3(0f, 0f, 0f);
		gameObject.transform.localScale = new Vector3(1f, 1f, 1f);
		UIRailLine component = gameObject.GetComponent<UIRailLine>();
		component.Init(_rail);
		component.CreateLine();
		component.ResetLinePos(mMapCtrl);
		component.mSelectedLine += OnSeletedLine;
		mRailLineList.Add(component);
	}

	private void DeleteUIRailLine(Route _rail)
	{
		UIRailLine uIRailLine = mRailLineList.Find((UIRailLine li) => li.mRoute == _rail);
		if (uIRailLine != null)
		{
			UnityEngine.Object.Destroy(uIRailLine.gameObject);
			mRailLineList.Remove(uIRailLine);
		}
	}

	public void UpdateAllRailLinePos()
	{
		UpdateCarState();
		for (int i = 0; i < mRailLineList.Count; i++)
		{
			mRailLineList[i].ResetLinePos(mMapCtrl);
		}
		for (int j = 0; j < mDisRailLineList.Count; j++)
		{
			mDisRailLineList[j].ResetLinePos(mMapCtrl);
		}
	}

	public void UpdateSelectedStation()
	{
		if (null != mSelectedStation)
		{
			Point point = PeSingleton<Manager>.Instance.GetPoint(mSelectedStation.mRailPointData.id);
			if (point != null)
			{
				OnSeletedLine(point.routeId != -1, point.id);
			}
			else
			{
				ClearUIInfo();
			}
		}
	}

	public void OnSeletedLine(bool isConnect, int stationID)
	{
		int num = -1;
		if (isConnect)
		{
			for (int i = 0; i < mRailLineList.Count; i++)
			{
				num = mRailLineList[i].FindStationIndex(stationID);
				if (num != -1)
				{
					OnSeletedLine(isConnect: true, mRailLineList[i].mRoute.id, stationID);
					break;
				}
			}
			return;
		}
		for (int j = 0; j < mDisRailLineList.Count; j++)
		{
			num = mDisRailLineList[j].FindStationIndex(stationID);
			if (num != -1)
			{
				OnSeletedLine(isConnect: false, mDisRailLineList[j].mIndex, stationID);
				break;
			}
		}
	}

	private void OnSeletedLine(bool isConnect, int routeID, int stationID)
	{
		isConnectLine = isConnect;
		ChangeUILineInfo(isConnect);
		if (mSelectedLineId != -1)
		{
			UIRailLine uIRailLine = mRailLineList.Find((UIRailLine li) => li.mRoute.id == mSelectedLineId);
			if (uIRailLine != null)
			{
				uIRailLine.SetSelected(_isSelected: false);
			}
			UIDisconnectionLine uIDisconnectionLine = mDisRailLineList.Find((UIDisconnectionLine li) => li.mIndex == mSelectedLineId);
			if (uIDisconnectionLine != null)
			{
				uIDisconnectionLine.SetSelected(_isSelected: false);
			}
		}
		if (isConnect)
		{
			UIRailLine line = mRailLineList.Find((UIRailLine li) => li.mRoute.id == routeID);
			if (line == null)
			{
				ClearLineInfo();
				return;
			}
			mSelectedLineIndex = mRailLineList.FindIndex((UIRailLine li) => li == line);
			line.SetSelected(_isSelected: true);
			mSelectedLineId = routeID;
			SetLineInfo(line.mRoute);
			if (mSelectedStation != null)
			{
				mSelectedStation.SetSelected(isSelected: false);
			}
			UIRailStation uIRailStation = line.FindStation(stationID);
			if (uIRailStation != null)
			{
				uIRailStation.SetSelected(isSelected: true);
				mSelectedStation = uIRailStation;
			}
			SetSatationInfo(uIRailStation.mRailPointData);
			EnableBtn(mBtnStop, value: true);
			return;
		}
		int index = routeID;
		UIDisconnectionLine uIDisconnectionLine2 = mDisRailLineList.Find((UIDisconnectionLine li) => li.mIndex == index);
		if (uIDisconnectionLine2 == null)
		{
			ClearLineInfo();
			return;
		}
		uIDisconnectionLine2.SetSelected(_isSelected: true);
		mSelectedLineId = index;
		if (mSelectedStation != null)
		{
			mSelectedStation.SetSelected(isSelected: false);
		}
		UIRailStation uIRailStation2 = uIDisconnectionLine2.FindStation(stationID);
		if (uIRailStation2 != null)
		{
			uIRailStation2.SetSelected(isSelected: true);
			mSelectedStation = uIRailStation2;
		}
		SetSatationInfo(uIRailStation2.mRailPointData);
		bool value = PERailwayCtrl.CheckRoute(uIRailStation2.mRailPointData);
		EnableBtn(mBtnStop, value);
	}

	private void EnableBtn(GameObject btn, bool value)
	{
		UISlicedSprite componentInChildren = btn.GetComponentInChildren<UISlicedSprite>();
		BoxCollider component = btn.GetComponent<BoxCollider>();
		if (componentInChildren != null)
		{
			componentInChildren.spriteName = ((!value) ? "LoginBtn_off" : "LoginBtn_on");
		}
		if (component != null)
		{
			component.enabled = value;
		}
	}

	private void ChangeUILineInfo(bool isConnect)
	{
		mLine.gameObject.SetActive(isConnect);
		mTitleLineName.gameObject.SetActive(isConnect);
		mLineName.gameObject.transform.parent.gameObject.SetActive(isConnect);
		mLbStationCount.gameObject.transform.parent.gameObject.SetActive(isConnect);
		mLbNodeCount.gameObject.transform.parent.gameObject.SetActive(isConnect);
		mLbRangeAblity.gameObject.transform.parent.gameObject.SetActive(isConnect);
		mLbOneWaytime.gameObject.transform.parent.gameObject.SetActive(isConnect);
		mLbBtnStop.text = ((!isConnect) ? PELocalization.GetString(8000605) : PELocalization.GetString(8000604));
		mRedestanceTime.enabled = !isConnect;
		mBtnStop.SetActive(value: true);
		mRailIcon.SetItem(null);
		mRailIcon.gameObject.SetActive(isConnect);
	}

	private void SetLineInfo(Route route)
	{
		Route.Stats stats = route.GetStats();
		mTitleLineName.text = route.name;
		mLineName.text = route.name;
		mLbStationCount.text = stats.stationNum.ToString();
		mLbNodeCount.text = stats.jointNum.ToString();
		mLbRangeAblity.text = stats.totalIntDis + " M";
		PETimer tmpTimer = PETimerUtil.GetTmpTimer();
		tmpTimer.Second = route.singleTripTime;
		mLbOneWaytime.text = tmpTimer.GetStrHhMmSs();
		mRailIcon.SetItem(route.trainId);
	}

	private void ClearLineInfo()
	{
		mTitleLineName.text = string.Empty;
		mLineName.text = string.Empty;
		mLbStationCount.text = string.Empty;
		mLbNodeCount.text = string.Empty;
		mLbRangeAblity.text = string.Empty;
		mLbOneWaytime.text = string.Empty;
		mRailIcon.SetItem(null);
	}

	private void SetSatationInfo(Point satation)
	{
		mTitleStationName.text = satation.name;
		mStationName.text = satation.name;
		PETimer tmpTimer = PETimerUtil.GetTmpTimer();
		tmpTimer.Second = satation.realStayTime;
		int num = Convert.ToInt32(tmpTimer.Minute);
		mRedestanceTime.text = num.ToString();
	}

	private void UpdateStationArriveTime()
	{
		if (!(mSelectedStation == null))
		{
			float arriveTime = mSelectedStation.mRailPointData.GetArriveTime();
			if (arriveTime < float.Epsilon)
			{
				mArriveTime.text = "--";
				return;
			}
			PETimer tmpTimer = PETimerUtil.GetTmpTimer();
			tmpTimer.Second = arriveTime;
			mArriveTime.text = tmpTimer.GetStrHhMmSs();
		}
	}

	private void OnLineNameChange(string text)
	{
		if (!(string.Empty != text.Trim()) || !isConnectLine)
		{
			return;
		}
		UIRailLine uIRailLine = mRailLineList.Find((UIRailLine li) => li.mRoute.id == mSelectedLineId);
		if (!(uIRailLine != null) || !(uIRailLine.mRoute.name != text))
		{
			return;
		}
		if (!PeSingleton<Manager>.Instance.IsRouteNameExist(text))
		{
			uIRailLine.mRoute.name = text;
			mTitleLineName.text = text;
			if (this.e_ResetRouteName != null)
			{
				this.e_ResetRouteName(uIRailLine.mRoute.id, text);
			}
		}
		else
		{
			MessageBox_N.ShowOkBox(UIMsgBoxInfo.ReNameNotice.GetString());
		}
		mLineName.text = uIRailLine.mRoute.name;
	}

	private void OnStationNameChange(string text)
	{
		if (!(string.Empty != text.Trim()) || !(mSelectedStation != null) || !(mSelectedStation.mRailPointData.name != text))
		{
			return;
		}
		if (!PeSingleton<Manager>.Instance.IsPointNameExist(text))
		{
			mSelectedStation.mRailPointData.name = text;
			mTitleStationName.text = text;
			if (this.e_ResetPointName != null)
			{
				this.e_ResetPointName(mSelectedStation.mRailPointData.id, text);
			}
		}
		else
		{
			MessageBox_N.ShowOkBox(UIMsgBoxInfo.ReNameNotice.GetString());
		}
		mStationName.text = mSelectedStation.mRailPointData.name;
	}

	private void OnStationResidenceTimeChange(string text)
	{
		if (!(mSelectedStation != null) || text.Length <= 0)
		{
			return;
		}
		PETimer tmpTimer = PETimerUtil.GetTmpTimer();
		tmpTimer.Minute = Convert.ToDouble(text);
		if (tmpTimer.Minute > 0.0)
		{
			Route routeByPointId = PeSingleton<Manager>.Instance.GetRouteByPointId(mSelectedStation.mRailPointData.id);
			if (routeByPointId != null)
			{
				routeByPointId.SetStayTime(mSelectedStation.mRailPointData.id, Convert.ToSingle(tmpTimer.Second));
			}
			else
			{
				Point point = PeSingleton<Manager>.Instance.GetPoint(mSelectedStation.mRailPointData.id);
				point.stayTime = Convert.ToSingle(tmpTimer.Second);
			}
			if (this.e_ResetPointTime != null)
			{
				this.e_ResetPointTime(mSelectedStation.mRailPointData.id, Convert.ToSingle(tmpTimer.Second));
			}
		}
		tmpTimer.Second = mSelectedStation.mRailPointData.realStayTime;
		mRedestanceTime.text = ((int)tmpTimer.Minute).ToString();
	}

	private void BtnStopOnClick()
	{
		if (mSelectedStation != null)
		{
			if (this.e_BtnStop != null && isConnectLine)
			{
				this.e_BtnStop(mSelectedStation.mRailPointData);
			}
			else if (this.e_BtnStar != null && !isConnectLine)
			{
				this.e_BtnStar(mSelectedStation.mRailPointData, mLineName.text);
			}
		}
	}

	private void BtnLineTitleLeftOnClick()
	{
		if (!isConnectLine)
		{
			return;
		}
		mSelectedLineIndex--;
		if (mSelectedLineIndex >= 0 && mSelectedLineIndex < mRailLineList.Count)
		{
			UIRailLine uIRailLine = mRailLineList[mSelectedLineIndex];
			OnSeletedLine(isConnect: true, uIRailLine.mRoute.id, uIRailLine.mRoute.GetPointByIndex(0).id);
		}
		else if (mRailLineList.Count > 0)
		{
			mSelectedLineIndex = mRailLineList.Count - 1;
			UIRailLine uIRailLine2 = mRailLineList[mSelectedLineIndex];
			if (uIRailLine2.mRoute.pointCount > 0)
			{
				OnSeletedLine(isConnect: true, uIRailLine2.mRoute.id, uIRailLine2.mRoute.GetPointByIndex(0).id);
			}
		}
	}

	private void BtnLineTitleRightOnClick()
	{
		if (!isConnectLine)
		{
			return;
		}
		mSelectedLineIndex++;
		if (mSelectedLineIndex >= 0 && mSelectedLineIndex < mRailLineList.Count)
		{
			UIRailLine uIRailLine = mRailLineList[mSelectedLineIndex];
			if (uIRailLine.mRoute.pointCount > 0)
			{
				OnSeletedLine(isConnect: true, uIRailLine.mRoute.id, uIRailLine.mRoute.GetPointByIndex(0).id);
			}
		}
		else if (mRailLineList.Count > 0)
		{
			mSelectedLineIndex = 0;
			UIRailLine uIRailLine2 = mRailLineList[mSelectedLineIndex];
			if (uIRailLine2.mRoute.pointCount > 0)
			{
				OnSeletedLine(isConnect: true, uIRailLine2.mRoute.id, uIRailLine2.mRoute.GetPointByIndex(0).id);
			}
		}
	}

	private void BtnStationTitleLeftOnClick()
	{
		if (mSelectedStation == null && (bool)mSelectedStation)
		{
			return;
		}
		if (isConnectLine)
		{
			UIRailLine uIRailLine = mRailLineList.Find((UIRailLine li) => li.mRoute.id == mSelectedLineId);
			if (!(uIRailLine == null) && uIRailLine.mRoute.pointCount != 0)
			{
				int num = uIRailLine.FindStationIndex(mSelectedStation.mRailPointData.id);
				do
				{
					num--;
				}
				while (num >= 0 && num < uIRailLine.mRoute.pointCount && uIRailLine.mRoute.GetPointByIndex(num).pointType == Point.EType.Joint);
				if (num < 0 || num >= uIRailLine.mRoute.pointCount)
				{
					num = uIRailLine.mRoute.pointCount - 1;
				}
				if (uIRailLine.mRoute.pointCount > 0)
				{
					OnSeletedLine(isConnect: true, uIRailLine.mRoute.id, uIRailLine.mRoute.GetPointByIndex(num).id);
				}
			}
			return;
		}
		UIDisconnectionLine uIDisconnectionLine = mDisRailLineList.Find((UIDisconnectionLine li) => li.mIndex == mSelectedLineId);
		if (!(uIDisconnectionLine == null) && uIDisconnectionLine.mPointList.Count != 0)
		{
			int num2 = uIDisconnectionLine.FindStationIndex(mSelectedStation.mRailPointData.id);
			do
			{
				num2--;
			}
			while (num2 >= 0 && num2 < uIDisconnectionLine.mPointList.Count && uIDisconnectionLine.mPointList[num2].pointType == Point.EType.Joint);
			if (num2 < 0 || num2 >= uIDisconnectionLine.mPointList.Count)
			{
				num2 = uIDisconnectionLine.mPointList.Count - 1;
			}
			if (uIDisconnectionLine.mPointList.Count > 0)
			{
				OnSeletedLine(isConnect: false, uIDisconnectionLine.mIndex, uIDisconnectionLine.mPointList[num2].id);
			}
		}
	}

	private void BtnStationTitleRightOnClick()
	{
		if (mSelectedStation == null)
		{
			return;
		}
		if (isConnectLine)
		{
			UIRailLine uIRailLine = mRailLineList.Find((UIRailLine li) => li.mRoute.id == mSelectedLineId);
			if (!(uIRailLine == null) && uIRailLine.mRoute.pointCount != 0)
			{
				int num = uIRailLine.FindStationIndex(mSelectedStation.mRailPointData.id);
				do
				{
					num++;
				}
				while (num >= 0 && num < uIRailLine.mRoute.pointCount && uIRailLine.mRoute.GetPointByIndex(num).pointType == Point.EType.Joint);
				if (num < 0 || num >= uIRailLine.mRoute.pointCount)
				{
					num = 0;
				}
				if (uIRailLine.mRoute.pointCount > 0)
				{
					OnSeletedLine(isConnect: true, uIRailLine.mRoute.id, uIRailLine.mRoute.GetPointByIndex(num).id);
				}
			}
			return;
		}
		UIDisconnectionLine uIDisconnectionLine = mDisRailLineList.Find((UIDisconnectionLine li) => li.mIndex == mSelectedLineId);
		if (!(uIDisconnectionLine == null) && uIDisconnectionLine.mPointList.Count != 0)
		{
			int num2 = uIDisconnectionLine.FindStationIndex(mSelectedStation.mRailPointData.id);
			do
			{
				num2++;
			}
			while (num2 >= 0 && num2 < uIDisconnectionLine.mPointList.Count && uIDisconnectionLine.mPointList[num2].pointType == Point.EType.Joint);
			if (num2 < 0 || num2 >= uIDisconnectionLine.mPointList.Count)
			{
				num2 = 0;
			}
			if (uIDisconnectionLine.mPointList.Count > 0)
			{
				OnSeletedLine(isConnect: false, uIDisconnectionLine.mIndex, uIDisconnectionLine.mPointList[num2].id);
			}
		}
	}

	public void PickTrain(Grid_N grid)
	{
		if (null != mSelectedStation)
		{
			Route route = PeSingleton<Manager>.Instance.GetRoute(mSelectedStation.mRailPointData.routeId);
			if (!route.trainRunning && route != null)
			{
				SelectItem_N.Instance.SetItem(grid.ItemObj, grid.ItemPlace, grid.ItemIndex);
			}
		}
	}

	public void DropTrain(Grid_N grid)
	{
		if (null == mSelectedStation || this.e_SetTrain == null)
		{
			return;
		}
		ItemObject itemObj = SelectItem_N.Instance.ItemObj;
		if (itemObj == null)
		{
			return;
		}
		Train cmpt = itemObj.GetCmpt<Train>();
		if (cmpt == null)
		{
			return;
		}
		int routeId = mSelectedStation.mRailPointData.routeId;
		Route route = PeSingleton<Manager>.Instance.GetRoute(routeId);
		if (!route.trainRunning)
		{
			mRailIcon.SetItem(itemObj);
			this.e_SetTrain(routeId, itemObj);
			if (this.e_SetTrainToStation != null)
			{
				this.e_SetTrainToStation(mSelectedStation.mRailPointData.routeId, mSelectedStation.mRailPointData.id);
			}
		}
	}

	public Point GetSelPoint()
	{
		if (null != mSelectedStation)
		{
			return mSelectedStation.mRailPointData;
		}
		return null;
	}
}

using System.Collections.Generic;
using Pathea;
using Railway;
using UnityEngine;

public class RailwayPointGuiCtrl : MonoBehaviour
{
	private static RailwayPointGuiCtrl mInstance;

	private Point mOpPoint;

	private bool mInputSel;

	private List<Point> mNearPoints;

	private List<bool> mLinkEnable;

	public static RailwayPointGuiCtrl Instance => mInstance;

	public RailwayPointGui_N mPointGui => GameUI.Instance.mRailwayPoint;

	private void Awake()
	{
		mInstance = this;
		mNearPoints = new List<Point>();
		mLinkEnable = new List<bool>();
	}

	private void Start()
	{
		mPointGui.e_PreMenuChange += OnPrePointChange;
		mPointGui.e_NextPointChange += OnNextPointChange;
		mPointGui.RecycleEvent += OnRecycleBtn;
	}

	public void SetInfo(Point point)
	{
		if (point == null)
		{
			if (mPointGui.IsOpen())
			{
				mPointGui.Hide();
			}
			return;
		}
		if (!mPointGui.IsOpen())
		{
			mPointGui.Show();
		}
		mOpPoint = point;
		mNearPoints = PeSingleton<Manager>.Instance.GetNearPoint(mOpPoint);
		mLinkEnable.Clear();
		for (int i = 0; i < mNearPoints.Count; i++)
		{
			if (mNearPoints[i].routeId != -1)
			{
				mLinkEnable.Add(item: false);
			}
			else
			{
				mLinkEnable.Add(mOpPoint.nextPointId != mNearPoints[i].id && mOpPoint.prePointId != mNearPoints[i].id && Manager.CheckLinkState(mOpPoint, mNearPoints[i]));
			}
		}
		mPointGui.PointName = mOpPoint.name;
		ResetPrePoint();
		ResetNextPoint();
		PeSingleton<Manager>.Instance.GetRouteByPointId(mOpPoint.id);
	}

	private void ResetPrePoint()
	{
		List<string> list = new List<string>();
		int num = -1;
		list.Add("Null");
		for (int i = 0; i < mNearPoints.Count; i++)
		{
			if (mLinkEnable[i])
			{
				list.Add(mNearPoints[i].name);
			}
			else
			{
				list.Add("[ff0000]" + mNearPoints[i].name + "[-]");
			}
			if (mNearPoints[i].id == mOpPoint.prePointId)
			{
				num = i;
			}
		}
		mPointGui.SetPrePoint(list, num + 1);
	}

	private void ResetNextPoint()
	{
		List<string> list = new List<string>();
		int num = -1;
		list.Add("Null");
		for (int i = 0; i < mNearPoints.Count; i++)
		{
			if (mLinkEnable[i])
			{
				list.Add(mNearPoints[i].name);
			}
			else
			{
				list.Add("[ff0000]" + mNearPoints[i].name + "[-]");
			}
			if (mNearPoints[i].id == mOpPoint.nextPointId)
			{
				num = i;
			}
		}
		mPointGui.SetNextPoint(list, num + 1);
	}

	public void OnRecycleBtn()
	{
		PeSingleton<RailwayOperate>.Instance.RequestRemovePoint(mOpPoint.id);
	}

	public void OnPrePointChange(int index, string text)
	{
		if (index == 0)
		{
			PeSingleton<RailwayOperate>.Instance.RequestChangePrePoint(mOpPoint.id, -1);
		}
		else if (mLinkEnable[index - 1])
		{
			PeSingleton<RailwayOperate>.Instance.RequestChangePrePoint(mOpPoint.id, mNearPoints[index - 1].id);
		}
		mPointGui.Hide();
	}

	public void OnNextPointChange(int index, string text)
	{
		if (index == 0)
		{
			PeSingleton<RailwayOperate>.Instance.RequestChangeNextPoint(mOpPoint, -1);
		}
		else if (mLinkEnable[index - 1])
		{
			PeSingleton<RailwayOperate>.Instance.RequestChangeNextPoint(mOpPoint, mNearPoints[index - 1].id);
		}
		mPointGui.Hide();
	}

	private void Update()
	{
		if (!mPointGui.IsOpen())
		{
			return;
		}
		if (mInputSel && !mPointGui.mNameInput.selected && mOpPoint != null && mPointGui.PointName != mOpPoint.name && mPointGui.PointName.Trim() != string.Empty)
		{
			if (PeSingleton<Manager>.Instance.IsPointNameExist(mPointGui.mNameInput.text))
			{
				MessageBox_N.ShowOkBox(UIMsgBoxInfo.ReNameNotice.GetString());
			}
			else
			{
				mOpPoint.name = mPointGui.mNameInput.text;
			}
			mPointGui.PointName = mOpPoint.name;
		}
		mInputSel = mPointGui.mNameInput.selected;
		if (null != GameUI.Instance.mMainPlayer && mOpPoint != null && Vector3.Distance(GameUI.Instance.mMainPlayer.position, mOpPoint.position) > 30f)
		{
			mPointGui.Hide();
		}
	}
}

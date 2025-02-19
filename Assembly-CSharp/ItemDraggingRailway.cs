using System.Collections.Generic;
using Pathea;
using PETools;
using Railway;
using UnityEngine;

public class ItemDraggingRailway : ItemDraggingBase
{
	public Point.EType mType;

	public CrossLine mLine;

	public Transform mLinkPoint;

	public CrossLine subLine;

	private bool mShowPositionErrorTips;

	private bool mShowDistanceErrorTips;

	private bool mShowLinkErrorTips;

	private UTimer mNoticeTimer;

	private Point mPrePoint;

	private Vector3 m_LinkPos1;

	private Vector3 m_LinkPos2;

	public static readonly float StepHeight = 0.5f;

	public static readonly int MaxStepTime = 30;

	private List<int> mIsolatePoints;

	private List<int> isolatePoints
	{
		get
		{
			if (mIsolatePoints == null)
			{
				mIsolatePoints = PeSingleton<Manager>.Instance.GetIsolatePoint();
			}
			return mIsolatePoints;
		}
	}

	public override void OnDragOut()
	{
		base.OnDragOut();
		mNoticeTimer = new UTimer();
		mNoticeTimer.ElapseSpeed = -1f;
		mNoticeTimer.Second = 1.0;
	}

	public override bool OnPutDown()
	{
		int num = -1;
		if (mPrePoint != null)
		{
			num = mPrePoint.id;
		}
		if (GameConfig.IsMultiClient)
		{
			if (VArtifactUtil.IsInTownBallArea(base.transform.position))
			{
				new PeTipMsg(PELocalization.GetString(8000864), PeTipMsg.EMsgLevel.Warning);
				return true;
			}
			if (null != PlayerNetwork.mainPlayer)
			{
				PlayerNetwork.mainPlayer.RPCServer(EPacketType.PT_InGame_Railway_AddPoint, base.transform.position, mType, num, itemDragging.itemObj.instanceId);
			}
		}
		else
		{
			RemoveTrees();
			PeSingleton<RailwayOperate>.Instance.RequestAddPoint(base.transform.position, mType, num, itemDragging.itemObj.instanceId);
		}
		return base.OnPutDown();
	}

	public override bool OnDragging(Ray cameraRay)
	{
		bool flag = base.OnDragging(cameraRay);
		mShowPositionErrorTips = !flag;
		if (flag)
		{
			TryLink();
		}
		else
		{
			BreakLink();
		}
		return flag;
	}

	public override void OnRotate()
	{
	}

	private void EstablishLink(Vector3 pos1, Vector3 pos2)
	{
		mLine.m_Begin = pos1;
		mLine.m_End = pos2;
		if (!mLine.gameObject.activeSelf)
		{
			mLine.gameObject.SetActive(value: true);
		}
	}

	private void EstablishSubLink(Vector3 pos1, Vector3 pos2)
	{
		subLine.m_Begin = pos1;
		subLine.m_End = pos2;
		if (!subLine.gameObject.activeSelf)
		{
			subLine.gameObject.SetActive(value: true);
		}
	}

	private void BreakLink()
	{
		if (mLine.gameObject.activeSelf)
		{
			mLine.gameObject.SetActive(value: false);
		}
		if (subLine.gameObject.activeSelf)
		{
			subLine.gameObject.SetActive(value: false);
		}
	}

	private bool TryLink()
	{
		mPrePoint = FindNearestIsolatePoint(mLinkPoint.position);
		if (mPrePoint == null)
		{
			BreakLink();
			return false;
		}
		m_LinkPos1 = mLinkPoint.position;
		m_LinkPos2 = mPrePoint.GetLinkPosition();
		Transform trans = mPrePoint.GetTrans();
		bool flag = false;
		for (int i = 0; i < MaxStepTime; i++)
		{
			if (Manager.CheckLinkState(m_LinkPos1, m_LinkPos2, base.transform, trans))
			{
				flag = true;
				EstablishSubLink(mLinkPoint.position, m_LinkPos2);
				base.transform.position += (float)i * StepHeight * Vector3.up;
				EstablishLink(m_LinkPos1, m_LinkPos2);
				break;
			}
			m_LinkPos1 += StepHeight * Vector3.up;
		}
		if (!flag)
		{
			BreakLink();
			mPrePoint = null;
			return false;
		}
		return true;
	}

	private Point FindNearestIsolatePoint(Vector3 pos, float dis = 80f)
	{
		Point result = null;
		float num = dis * dis;
		foreach (int isolatePoint in isolatePoints)
		{
			Point point = PeSingleton<Manager>.Instance.GetPoint(isolatePoint);
			if (point != null)
			{
				float sqrMagnitude = (pos - point.GetLinkPosition()).sqrMagnitude;
				if (sqrMagnitude < num)
				{
					num = sqrMagnitude;
					result = point;
				}
			}
		}
		return result;
	}

	private void Update()
	{
		UpdateTips();
	}

	private void OnGUI()
	{
		GUI.color = Color.green;
		if (mShowPositionErrorTips)
		{
			GUILayout.Label(UIMsgBoxInfo.CannotPut.GetString());
		}
		if (mTooFar)
		{
			GUILayout.Label(UIMsgBoxInfo.DistanceNotMatch.GetString());
		}
	}

	private void UpdateTips()
	{
		mNoticeTimer.Update(Time.deltaTime);
		if (mNoticeTimer.Second < 0.0)
		{
			mNoticeTimer.Second = 1.0;
			if (mShowPositionErrorTips)
			{
				GlobalShowGui_N.ShowString(UIMsgBoxInfo.CannotPut.GetString());
				mShowPositionErrorTips = false;
			}
			else if (mShowDistanceErrorTips)
			{
				GlobalShowGui_N.ShowString(UIMsgBoxInfo.DistanceNotMatch.GetString());
				mShowDistanceErrorTips = false;
			}
			else if (mShowLinkErrorTips)
			{
				GlobalShowGui_N.ShowString(UIMsgBoxInfo.ConnectError.GetString());
				mShowLinkErrorTips = false;
			}
		}
	}

	private void RemoveTrees()
	{
		Vector3 vector = m_LinkPos1 - m_LinkPos2;
		Vector3 normalized = vector.normalized;
		RaycastHit[] array = Physics.RaycastAll(m_LinkPos2, normalized, vector.magnitude, 2097152, QueryTriggerInteraction.Ignore);
		for (int i = 0; i < array.Length; i++)
		{
			GlobalTreeInfo treeinfo = PEUtil.GetTreeinfo(array[i].collider);
			if (treeinfo != null)
			{
				DigTerrainManager.RemoveTree(treeinfo);
			}
		}
	}
}

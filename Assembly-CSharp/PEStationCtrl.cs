using Pathea;
using Railway;
using UnityEngine;

[RequireComponent(typeof(RailwayStation))]
public class PEStationCtrl : MousePickableChildCollider
{
	private const float MouseOpRange = 150f;

	[SerializeField]
	private Transform GetOffPos;

	private RailwayStation mStation;

	private Route mRoute;

	private PeTrans playerTrans;

	private PassengerCmpt mMainPlayerPassenger;

	private bool rotating;

	private Vector3 rotation = Vector3.zero;

	public RailwayStation station => mStation;

	public Vector3 getOffPos => (!(null != GetOffPos)) ? (base.transform.position + 1.5f * Vector3.up) : GetOffPos.position;

	private bool openMenuEnable => null == mRoute;

	public bool isJoint => mStation.Point.pointType == Point.EType.Joint;

	public Point point => PeSingleton<Manager>.Instance.GetPoint(mStation.pointId);

	private Vector3 playerPos
	{
		get
		{
			if (playerTrans == null && HasMainPlayer())
			{
				playerTrans = PeSingleton<PeCreature>.Instance.mainPlayer.peTrans;
			}
			if (playerTrans != null)
			{
				return playerTrans.position;
			}
			return Vector3.zero;
		}
		set
		{
			if (playerTrans != null)
			{
				playerTrans.position = value;
			}
		}
	}

	private int mainPlayerId => PeSingleton<PeCreature>.Instance.mainPlayerId;

	private PassengerCmpt mainPlayerPassenger
	{
		get
		{
			if (!HasMainPlayer())
			{
				return null;
			}
			if (mMainPlayerPassenger == null)
			{
				mMainPlayerPassenger = PeSingleton<PeCreature>.Instance.mainPlayer.passengerCmpt;
			}
			return mMainPlayerPassenger;
		}
	}

	protected override string tipsText
	{
		get
		{
			string text = mStation.Point.name;
			if (openMenuEnable)
			{
				text = text + "\n" + PELocalization.GetString(8000129);
			}
			if (GetGetOnGetOffEnable())
			{
				text = ((!MainPlayerOnTrain()) ? (text + "\n" + PELocalization.GetString(8000130)) : (text + "\n" + PELocalization.GetString(8000131)));
			}
			if (isJoint)
			{
				text = text + "\n" + PELocalization.GetString(8000150);
			}
			return text;
		}
	}

	private bool HasMainPlayer()
	{
		return -1 != PeSingleton<PeCreature>.Instance.mainPlayerId;
	}

	private bool MainPlayerOnTrain()
	{
		if (null == mainPlayerPassenger)
		{
			return false;
		}
		return mainPlayerPassenger.IsOnRail;
	}

	private void CheckMenu()
	{
		if (openMenuEnable && PeInput.Get(PeInput.LogicFunction.OpenItemMenu) && !(null == RailwayPointGuiCtrl.Instance))
		{
			RailwayPointGuiCtrl.Instance.SetInfo(point);
		}
	}

	private void CheckPlayerTakeOnOrTakeOff()
	{
		if (PeInput.Get(PeInput.LogicFunction.InteractWithItem) && GetGetOnGetOffEnable())
		{
			if (MainPlayerOnTrain())
			{
				PeSingleton<RailwayOperate>.Instance.RequestGetOffTrain(mRoute.id, mainPlayerId, GetOffPos.position);
			}
			else
			{
				PeSingleton<RailwayOperate>.Instance.RequestGetOnTrain(mRoute.id, mainPlayerId);
			}
		}
	}

	private void RotUpDir(float angle)
	{
		Quaternion quaternion = Quaternion.Euler(rotation);
		Vector3 axis = quaternion * Vector3.forward;
		rotation = (Quaternion.AngleAxis(angle, axis) * quaternion).eulerAngles;
	}

	private void CheckRot()
	{
		if (isJoint && PeInput.Get(PeInput.LogicFunction.Item_RotateItemPress))
		{
			if (!rotating)
			{
				rotation = point.rotation;
			}
			RotUpDir(180f * Time.deltaTime);
			rotating = true;
		}
		else if (rotating)
		{
			rotating = false;
			PeSingleton<RailwayOperate>.Instance.RequestChangePointRot(mStation.Point.id, rotation);
		}
	}

	protected override void OnStart()
	{
		base.OnStart();
		mStation = GetComponent<RailwayStation>();
	}

	protected override void CheckOperate()
	{
		base.CheckOperate();
		CheckMenu();
		CheckPlayerTakeOnOrTakeOff();
		CheckRot();
	}

	protected override bool CheckPick(Ray camMouseRay, out float dis)
	{
		if (base.CheckPick(camMouseRay, out dis) && dis < 150f)
		{
			mRoute = PeSingleton<Manager>.Instance.GetRoute(mStation.Point.routeId);
			PeSingleton<MousePicker>.Instance.UpdateTis();
			return true;
		}
		return false;
	}

	private bool GetGetOnGetOffEnable()
	{
		if (isJoint)
		{
			return false;
		}
		if (mRoute == null)
		{
			return false;
		}
		if (null == mRoute.train)
		{
			return false;
		}
		if (!HasMainPlayer())
		{
			return false;
		}
		if (Vector3.Distance(mRoute.train.transform.position, mStation.mJointPoint.position) > 1f)
		{
			return false;
		}
		if (Vector3.Distance(mRoute.train.transform.position, playerPos) > 10f)
		{
			return false;
		}
		return true;
	}
}

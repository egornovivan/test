using System.Collections.Generic;
using Pathea;
using PETools;
using Railway;
using UnityEngine;

public class RailwayTrain : MousePickableChildCollider
{
	private const int ArriveSoundID = 2495;

	private const int DepartSoundID = 2496;

	private const int RunSoundID = 2497;

	[SerializeField]
	private List<RailwaySeat> m_SeatList;

	public Route mRoute;

	private PeEntity mMainPlayer;

	private Point stayStation;

	private AudioController mRunAudio;

	private PeEntity mainPlayer
	{
		get
		{
			if (null == mMainPlayer)
			{
				mMainPlayer = PeSingleton<MainPlayer>.Instance.entity;
			}
			return mMainPlayer;
		}
	}

	private AudioController runAudio
	{
		get
		{
			if (null == mRunAudio)
			{
				mRunAudio = AudioManager.instance.Create(base.transform.position, 2497, base.transform, isPlay: false, isDelete: false);
			}
			return mRunAudio;
		}
	}

	protected override string tipsText
	{
		get
		{
			if (null == mainPlayer || mRoute == null)
			{
				return string.Empty;
			}
			string text = mRoute.name;
			if (GetGetOnGetOffEnable())
			{
				text = ((!mainPlayer.passengerCmpt.IsOnRail) ? (text + "\n" + PELocalization.GetString(8000130)) : (text + "\n" + PELocalization.GetString(8000131)));
			}
			return text;
		}
	}

	private void Awake()
	{
		Transform transform = base.transform.FindChild("monorail_cart/Master_Point");
		m_SeatList = new List<RailwaySeat>(transform.childCount);
		foreach (Transform item in transform)
		{
			m_SeatList.Add(item.GetComponent<RailwaySeat>());
		}
	}

	public bool HasPassenger()
	{
		foreach (RailwaySeat seat in m_SeatList)
		{
			if (seat.passenger != null)
			{
				return true;
			}
		}
		return false;
	}

	public bool AddPassenger(IPassenger pas)
	{
		RailwaySeat railwaySeat = m_SeatList.Find((RailwaySeat s) => s.passenger == null);
		if (null == railwaySeat)
		{
			return false;
		}
		return railwaySeat.SetPassenger(pas);
	}

	public bool HasEmptySeat()
	{
		if (m_SeatList == null)
		{
			return false;
		}
		for (int i = 0; i < m_SeatList.Count; i++)
		{
			if (m_SeatList[i].passenger == null)
			{
				return true;
			}
		}
		return false;
	}

	public bool RemovePassenger(IPassenger pas, Vector3 getOffPos)
	{
		RailwaySeat seat = GetSeat(pas);
		if (null == seat)
		{
			return false;
		}
		return seat.ResetPassenger(getOffPos);
	}

	public bool RemovePassenger(IPassenger pas)
	{
		return RemovePassenger(pas, GetGetOffPos());
	}

	private RailwaySeat GetSeat(IPassenger pas)
	{
		return m_SeatList.Find((RailwaySeat s) => (s.passenger == pas) ? true : false);
	}

	public void ClearPassenger()
	{
		m_SeatList.ForEach(delegate(RailwaySeat s)
		{
			s.ResetPassenger(GetGetOffPos());
		});
	}

	private Vector3 GetGetOffPos()
	{
		return PEUtil.GetRandomPosition(base.transform.position + base.transform.right * 3f, 0f, 1f);
	}

	private bool GetGetOnGetOffEnable()
	{
		if (mRoute == null)
		{
			return false;
		}
		if (stayStation == null || stayStation.pointType == Point.EType.Joint)
		{
			return false;
		}
		if (Vector3.Distance(base.transform.position, mainPlayer.position) > 10f)
		{
			return false;
		}
		return true;
	}

	private void Update()
	{
		if (mRoute != null)
		{
			if (stayStation != null && mRoute.stayStation == null)
			{
				AudioManager.instance.Create(base.transform.position, 2496, base.transform);
				runAudio.PlayAudio(0.5f);
			}
			else if (stayStation == null && mRoute.stayStation != null)
			{
				AudioManager.instance.Create(base.transform.position, 2495, base.transform);
				runAudio.StopAudio(0.2f);
			}
			stayStation = mRoute.stayStation;
		}
	}

	protected override bool CheckPick(Ray camMouseRay, out float dis)
	{
		if (base.CheckPick(camMouseRay, out dis))
		{
			PeSingleton<MousePicker>.Instance.UpdateTis();
			return true;
		}
		return false;
	}

	protected override void CheckOperate()
	{
		if (!(null == mainPlayer) && mRoute != null && PeInput.Get(PeInput.LogicFunction.InteractWithItem) && GetGetOnGetOffEnable())
		{
			if (mainPlayer.passengerCmpt.IsOnRail)
			{
				PEStationCtrl component = stayStation.station.GetComponent<PEStationCtrl>();
				PeSingleton<RailwayOperate>.Instance.RequestGetOffTrain(mRoute.id, mainPlayer.Id, (!(null != component)) ? (base.transform.position + 2.5f * Vector3.up) : component.getOffPos);
			}
			else
			{
				PeSingleton<RailwayOperate>.Instance.RequestGetOnTrain(mRoute.id, mainPlayer.Id);
			}
		}
	}
}

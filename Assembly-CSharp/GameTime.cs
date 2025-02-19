using System.Collections.Generic;
using uLink;
using UnityEngine;

public class GameTime : UnityEngine.MonoBehaviour
{
	public delegate void CancelTimePassFunc();

	public const double StoryModeStartDay = 94.35;

	public const double BuildModeStartDay = 94.5;

	public static PETimer Timer = null;

	public static UTimer PlayTime = null;

	public static float NormalTimeSpeed = 12f;

	public static bool AbnormalityTimePass = false;

	private static List<GameObject> locks = new List<GameObject>();

	private static bool _passingTime = false;

	private static long _targetTick;

	public static float DeltaTime => (Timer == null) ? 0f : (Time.deltaTime * Timer.ElapseSpeed);

	public static event CancelTimePassFunc OnCancelTimePass;

	public static void ClearTimerPass()
	{
		if (AbnormalityTimePass && GameTime.OnCancelTimePass != null)
		{
			GameTime.OnCancelTimePass();
		}
	}

	public static void Lock(GameObject obj)
	{
		if (!locks.Contains(obj))
		{
			locks.Add(obj);
		}
	}

	public static void UnLock(GameObject obj)
	{
		if (locks.Contains(obj))
		{
			locks.Remove(obj);
		}
	}

	private void Start()
	{
		Timer = new PETimer();
		Timer.ElapseSpeed = 0f;
		PlayTime = new UTimer();
		PlayTime.ElapseSpeed = 1f;
	}

	private void Update()
	{
		if (Timer == null)
		{
			return;
		}
		if (_passingTime && Timer.Tick >= _targetTick)
		{
			Timer.Tick = _targetTick;
			_passingTime = false;
			_targetTick = 0L;
			Timer.ElapseSpeed = NormalTimeSpeed;
		}
		Timer.Update(Time.deltaTime);
		PlayTime.Update(Time.deltaTime);
		if (!Application.isEditor)
		{
			return;
		}
		if (Input.GetKeyDown(KeyCode.Keypad1))
		{
			Timer.ElapseSpeed = 10000f;
		}
		if (Input.GetKeyUp(KeyCode.Keypad1))
		{
			Timer.ElapseSpeed = NormalTimeSpeed;
		}
		if (Input.GetKey(KeyCode.Keypad1))
		{
			Timer.ElapseSpeed *= 1.01f;
			if (Timer.ElapseSpeed > 1000000f)
			{
				Timer.ElapseSpeed = 1000000f;
			}
		}
	}

	public static void PassTime(double gameTime, double trueTime = 1.0)
	{
		if (trueTime < 0.10000000149011612)
		{
			trueTime = 0.10000000149011612;
		}
		PETimer pETimer = new PETimer();
		pETimer.Tick = Timer.Tick;
		PETimer pETimer2 = new PETimer();
		pETimer2.Second = gameTime;
		pETimer.AddTime(pETimer2);
		_targetTick = pETimer.Tick;
		Timer.ElapseSpeed = (float)(gameTime / trueTime);
		_passingTime = true;
	}

	public static void RPC_S2C_SyncInitGameTimer(uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
		Timer.Tick = stream.Read<long>(new object[0]);
		Timer.ElapseSpeed = stream.Read<float>(new object[0]);
		ServerAdministrator.updataFlag = true;
	}

	public static void RPC_S2C_SyncGameTimer(uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
		Timer.Tick = stream.Read<long>(new object[0]);
	}

	public static void RPC_S2C_ElapseSpeedShift(uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
		Timer.ElapseSpeed = stream.Read<float>(new object[0]);
	}
}

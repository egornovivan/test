using uLink;
using UnityEngine;

public class GameTime : UnityEngine.MonoBehaviour
{
	public const float NormalTimeSpeed = 12f;

	public const float DayTimeSpeed = 12f;

	public const float NightTimeSpeed = 120f;

	public const double StoryModeStartDay = 94.35;

	public const double BuildModeStartDay = 94.5;

	public const double MultiModeStartSecond = 32000.0;

	private static GameTime mInstance;

	public static PETimer Timer;

	public static UTimer PlayTime;

	private float savedElapseSpeed;

	public static GameTime Instance => mInstance;

	public static float DeltaTime => (Timer == null) ? 0f : (Time.deltaTime * Timer.ElapseSpeed);

	private void Awake()
	{
		mInstance = this;
		Timer = new PETimer();
		if (Timer.CycleInDay < -0.25)
		{
			Timer.ElapseSpeed = 120f;
		}
		else
		{
			Timer.ElapseSpeed = 12f;
		}
		Timer.Second = 32000.0;
		PlayTime = new UTimer();
		PlayTime.ElapseSpeed = 1f;
	}

	public void Init()
	{
		if (ServerConfig.GameTimeSecond > 0.0)
		{
			Timer.Second = ServerConfig.GameTimeSecond;
			if (Application.isEditor)
			{
				Debug.Log("<color=yellow>init Second:" + Timer.Second + "</color>");
			}
		}
		InvokeRepeating("SyncTime", 0f, 30f);
	}

	private void Start()
	{
	}

	private void Update()
	{
		if (!NetInterface.IsServer || Timer == null)
		{
			return;
		}
		PlayTime.Update(Time.deltaTime);
		Timer.Update(Time.deltaTime);
		if (Timer.CycleInDay < -0.25)
		{
			if (Timer.ElapseSpeed == 12f)
			{
				NetworkManager.SyncProxy(EPacketType.PT_Common_ElapseSpeedShift, 120f);
				Timer.ElapseSpeed = 120f;
			}
		}
		else if (Timer.ElapseSpeed == 120f)
		{
			NetworkManager.SyncProxy(EPacketType.PT_Common_ElapseSpeedShift, 12f);
			Timer.ElapseSpeed = 12f;
		}
		if (Application.isEditor)
		{
			if (Input.GetKeyDown(KeyCode.Keypad1))
			{
				savedElapseSpeed = Timer.ElapseSpeed;
				Timer.ElapseSpeed = 10000f;
			}
			if (Input.GetKeyUp(KeyCode.Keypad1))
			{
				Timer.ElapseSpeed = savedElapseSpeed;
				SyncTimeImmediately();
			}
			if (Input.GetKey(KeyCode.Keypad1))
			{
				Timer.ElapseSpeed *= 1.01f;
				if (Timer.ElapseSpeed > 1000000f)
				{
					Timer.ElapseSpeed = 1000000f;
				}
			}
			if (Input.GetKeyDown(KeyCode.Keypad2))
			{
				SyncTime();
			}
		}
		if (!Application.isEditor)
		{
		}
	}

	public static void SyncTimer(uLink.NetworkPlayer peer)
	{
		if (NetInterface.IsServer)
		{
			NetworkManager.SyncPeer(peer, EPacketType.PT_Common_InitGameTimer, Timer.Tick, Timer.ElapseSpeed, ServerConfig.m_PeriodTime, ServerConfig.m_EquaAngle, ServerConfig.LocalLatitude);
		}
	}

	private void SyncTime()
	{
		if (NetInterface.IsServer)
		{
			NetworkManager.SyncProxy(EPacketType.PT_Common_InitGameTimer, Timer.Tick, Timer.ElapseSpeed, ServerConfig.m_PeriodTime, ServerConfig.m_EquaAngle, ServerConfig.LocalLatitude);
		}
	}

	public static void SyncTimeImmediately()
	{
		if (NetInterface.IsServer)
		{
			NetworkManager.SyncProxy(EPacketType.PT_Common_GameTimer, Timer.Tick);
		}
	}
}

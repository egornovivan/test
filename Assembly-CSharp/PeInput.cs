using System;
using System.Collections.Generic;
using System.IO;
using System.Xml;
using CameraForge;
using InControl;
using PeCustom;
using UnityEngine;

public static class PeInput
{
	public enum LogicFunction
	{
		DrawWeapon,
		Attack,
		BegDigging,
		EndDigging,
		BegShooting,
		EndShooting,
		BegLaserWarming,
		EndLaserWarming,
		Build,
		Item_Drag,
		Item_Drop,
		UI_SkipDialog,
		TalkToNpc,
		OpenItemMenu,
		GatherHerb,
		DrawWater,
		PickBody,
		SheatheWeapon,
		Cut,
		RotateCam,
		UI_RMouseOp,
		Item_Use,
		Item_Equip,
		Item_CancelDrag,
		PushForwardCam,
		PullBackwardCam,
		AutoRunOnOff,
		TakeForwardVehicleOnOff,
		ClimbForwardLadderOnOff,
		InteractWithItem,
		Item_RotateItem,
		Item_RotateItemPress,
		WorldMapUI,
		MissionUI,
		PackageUI,
		CharacterUI,
		ReplicationUI,
		SkillUI,
		FollowersUI,
		HandheldPcUI,
		CreationSystem,
		ColonyUI,
		FriendsUI,
		ShopUI,
		EnterChat,
		SendChat,
		UI_SkipDialog2,
		UI_Confirm,
		TalkMenuUI,
		GameMenuUI,
		OptionsUI,
		UI_CloseUI,
		UI_Cancel,
		ChangeContrlMode,
		SaveMenuUI,
		LoadMenuUI,
		ScreenCapture,
		QuickBar1,
		QuickBar2,
		QuickBar3,
		QuickBar4,
		QuickBar5,
		QuickBar6,
		QuickBar7,
		QuickBar8,
		QuickBar9,
		QuickBar10,
		PrevQuickBar,
		NextQuickBar,
		LiberatiePerspective,
		MoveForward,
		DodgeForward,
		MoveBackward,
		DodgeBackward,
		MoveLeft,
		DodgeLeft,
		MoveRight,
		DodgeRight,
		Jump,
		SwimUp,
		Jet,
		Reload,
		SwitchWalkRun,
		Sprint,
		Block,
		UI_SkipDialog1,
		BuildMode,
		Build_FreeBuildModeOnOff,
		Build_TweakSelectionArea_Up,
		Build_TweakSelectionArea_Dn,
		Build_TweakSelectionArea_Lt,
		Build_TweakSelectionArea_Rt,
		Build_TweakSelectionArea_PgUp,
		Build_TweakSelectionArea_PgDn,
		Build_RotateOnAxis,
		Build_Shortcut1,
		Build_Shortcut2,
		Build_Shortcut3,
		Build_Shortcut4,
		Build_Shortcut5,
		Build_Shortcut6,
		Build_Shortcut7,
		Build_Undo,
		Build_Redo,
		Build_ChangeBrush,
		Build_DeleteSelection,
		Build_CrossSelection,
		Build_SubtractSelection,
		Build_AddSelection,
		Build_Extrude,
		Build_Squash,
		Vehicle_BegUnfixedShooting,
		Vehicle_EndUnfixedShooting,
		Vehicle_BegFixedShooting,
		Vehicle_EndFixedShooting,
		Vehicle_Brake,
		Vehicle_LiftUp,
		Vehicle_LiftDown,
		Vehicle_AttackModeOnOff,
		Vehicle_Sprint,
		SwitchLight,
		MissleTarget,
		MissleLaunch,
		VehicleWeaponGrp1,
		VehicleWeaponGrp2,
		VehicleWeaponGrp3,
		VehicleWeaponGrp4
	}

	private class KeyExcluders
	{
		public KeyCode _key;

		public List<LogicInput> _excluders = new List<LogicInput>();

		public KeyExcluders(KeyCode key)
		{
			_key = key;
		}
	}

	private class LogicInput
	{
		public KeyJoySettingPair KeyJoy;

		private KeyPressType _keyPressType;

		private KeyPressType _joyPressType;

		private float[] _keyPara;

		private float[] _joyPara;

		public Func<bool> Excluder;

		public Func<bool> PressTst;

		public Func<bool> JoyTst;

		public LogicInput Alternate;

		public LogicInput(KeyJoySettingPair keyJoy, Func<bool> excluder = null, KeyPressType keyPressType = KeyPressType.Down, float[] keyPara = null, KeyPressType joyPressType = KeyPressType.JoyStickBegin, float[] joyPara = null, LogicInput alternate = null)
		{
			_keyPressType = keyPressType;
			_joyPressType = joyPressType;
			KeyJoy = keyJoy;
			Excluder = excluder;
			Alternate = alternate;
		}

		public void ResetPressTstFunc()
		{
			Excluder = (Func<bool>)Delegate.Remove(Excluder, new Func<bool>(KeyMaskExcluderShift));
			Excluder = (Func<bool>)Delegate.Remove(Excluder, new Func<bool>(KeyMaskExcluderCtrl));
			Excluder = (Func<bool>)Delegate.Remove(Excluder, new Func<bool>(KeyMaskExcluderAlt));
			PressTst = CreatePressTestFunc(KeyJoy._key, _keyPressType, _keyPara);
			if (KeyJoy._joy != 0 || _joyPressType >= KeyPressType.JoyStickBegin)
			{
				JoyTst = CreatePressTestFuncJoy(KeyJoy._joy, _joyPressType, _keyPara);
			}
			KeyCode key = KeyJoy._key;
			int num = (int)(KeyJoy._key & (KeyCode)28672);
			if (num != 0)
			{
				if ((num & 0x1000) != 0)
				{
					Excluder = (Func<bool>)Delegate.Combine(Excluder, new Func<bool>(KeyMaskExcluderShift));
				}
				if ((num & 0x2000) != 0)
				{
					Excluder = (Func<bool>)Delegate.Combine(Excluder, new Func<bool>(KeyMaskExcluderCtrl));
				}
				if ((num & 0x4000) != 0)
				{
					Excluder = (Func<bool>)Delegate.Combine(Excluder, new Func<bool>(KeyMaskExcluderAlt));
				}
				key = KeyJoy._key - num;
			}
			if (Excluder != null)
			{
				int num2 = s_inputExcluders.FindIndex((KeyExcluders x) => x._key == key);
				if (num2 < 0)
				{
					s_inputExcluders.Add(new KeyExcluders(key));
					num2 = s_inputExcluders.Count - 1;
				}
				s_inputExcluders[num2]._excluders.Add(this);
			}
			if (Alternate != null)
			{
				Alternate.ResetPressTstFunc();
			}
		}
	}

	private class ClickPressInfo
	{
		public KeyCode _key;

		public float _interval;

		public float _lastDown;

		private static List<ClickPressInfo> s_clickPressKeys = new List<ClickPressInfo>();

		public ClickPressInfo(KeyCode key, float[] para)
		{
			_key = key;
			_interval = ((para != null) ? para[0] : 0.5f);
			_lastDown = -1f;
		}

		public static bool TryGetValue(KeyCode key, out ClickPressInfo info)
		{
			info = null;
			int num = s_clickPressKeys.FindIndex((ClickPressInfo x) => x._key == key);
			if (num < 0)
			{
				return false;
			}
			info = s_clickPressKeys[num];
			return true;
		}

		public static void Register(KeyCode key, float[] para)
		{
			int num = s_clickPressKeys.FindIndex((ClickPressInfo x) => x._key == key);
			if (num < 0)
			{
				s_clickPressKeys.Add(new ClickPressInfo(key, para));
			}
			else
			{
				s_clickPressKeys[num] = new ClickPressInfo(key, para);
			}
		}

		public static void Clear()
		{
			s_clickPressKeys.Clear();
		}
	}

	private class ClickCDPressInfo
	{
		public KeyCode _key;

		public float _cd;

		public float _interval;

		public float _lastDown;

		public float _lastClick;

		private static List<ClickCDPressInfo> s_clickCDPressKeys = new List<ClickCDPressInfo>();

		public ClickCDPressInfo(KeyCode key, float[] para)
		{
			_key = key;
			_cd = ((para != null) ? para[0] : 2f);
			_interval = ((para != null && para.Length < 2) ? para[1] : 0.2f);
			_lastDown = -1f;
			_lastClick = -1f;
		}

		public static bool TryGetValue(KeyCode key, out ClickCDPressInfo info)
		{
			info = null;
			int num = s_clickCDPressKeys.FindIndex((ClickCDPressInfo x) => x._key == key);
			if (num < 0)
			{
				return false;
			}
			info = s_clickCDPressKeys[num];
			return true;
		}

		public static void Register(KeyCode key, float[] para)
		{
			int num = s_clickCDPressKeys.FindIndex((ClickCDPressInfo x) => x._key == key);
			if (num < 0)
			{
				s_clickCDPressKeys.Add(new ClickCDPressInfo(key, para));
			}
			else
			{
				s_clickCDPressKeys[num] = new ClickCDPressInfo(key, para);
			}
		}

		public static void Clear()
		{
			s_clickCDPressKeys.Clear();
		}
	}

	private class DoublePressInfo
	{
		public KeyCode _key;

		public float _interval;

		public float _lastDown0;

		public float _lastDown1;

		private static List<DoublePressInfo> s_doublePressKeys = new List<DoublePressInfo>();

		public DoublePressInfo(KeyCode key, float[] para)
		{
			_key = key;
			_interval = ((para != null) ? para[0] : 0.2f);
			_lastDown0 = -1f;
			_lastDown1 = -1f;
		}

		public static bool TryGetValue(KeyCode key, out DoublePressInfo info)
		{
			info = null;
			int num = s_doublePressKeys.FindIndex((DoublePressInfo x) => x._key == key);
			if (num < 0)
			{
				return false;
			}
			info = s_doublePressKeys[num];
			return true;
		}

		public static void Register(KeyCode key, float[] para)
		{
			int num = s_doublePressKeys.FindIndex((DoublePressInfo x) => x._key == key);
			if (num < 0)
			{
				s_doublePressKeys.Add(new DoublePressInfo(key, para));
			}
			else
			{
				s_doublePressKeys[num] = new DoublePressInfo(key, para);
			}
		}

		public static void Clear()
		{
			s_doublePressKeys.Clear();
		}
	}

	private class LongPressInfo
	{
		public KeyCode _key;

		public float _term;

		public float _startDown;

		private static List<LongPressInfo> s_longPressKeys = new List<LongPressInfo>();

		public LongPressInfo(KeyCode key, float[] para)
		{
			_key = key;
			_term = ((para != null) ? para[0] : 0.3f);
			_startDown = 0f;
		}

		public static bool TryGetValue(KeyCode key, out LongPressInfo info)
		{
			info = null;
			int num = s_longPressKeys.FindIndex((LongPressInfo x) => x._key == key);
			if (num < 0)
			{
				return false;
			}
			info = s_longPressKeys[num];
			return true;
		}

		public static void Register(KeyCode key, float[] para)
		{
			int num = s_longPressKeys.FindIndex((LongPressInfo x) => x._key == key);
			if (num < 0)
			{
				s_longPressKeys.Add(new LongPressInfo(key, para));
			}
			else
			{
				s_longPressKeys[num] = new LongPressInfo(key, para);
			}
		}

		public static void Clear()
		{
			s_longPressKeys.Clear();
		}
	}

	private class JoyAxisStateInfo
	{
		public InputControlType controlType;

		public bool positiveDoubleDown;

		public bool negativeDoubleDown;

		public float axisValue;

		private float m_LastCheckTime;

		private float m_LastPositiveDownTime;

		private float m_LastNegativeDownTime;

		private float m_DoubleInterval;

		private static readonly float ThresholdValue = 0.7f;

		private static List<JoyAxisStateInfo> s_JoyAxisStateInfos = new List<JoyAxisStateInfo>();

		private JoyAxisStateInfo(InputControlType type, float[] para)
		{
			controlType = type;
			m_DoubleInterval = ((para != null) ? para[0] : 0.3f);
		}

		private JoyAxisStateInfo UpdateInfo()
		{
			if (Time.time - m_LastCheckTime > float.Epsilon)
			{
				m_LastCheckTime = Time.time;
				float num = InputManager.ActiveDevice.GetControl(controlType);
				bool flag = num >= ThresholdValue;
				bool flag2 = num <= 0f - ThresholdValue;
				bool flag3 = axisValue < ThresholdValue && flag;
				bool flag4 = axisValue > 0f - ThresholdValue && flag2;
				if (flag3)
				{
					positiveDoubleDown = Time.time - m_LastPositiveDownTime <= m_DoubleInterval;
					m_LastPositiveDownTime = Time.time;
				}
				else
				{
					positiveDoubleDown = false;
				}
				if (flag4)
				{
					negativeDoubleDown = Time.time - m_LastNegativeDownTime <= m_DoubleInterval;
					m_LastNegativeDownTime = Time.time;
				}
				else
				{
					negativeDoubleDown = false;
				}
				axisValue = num;
			}
			return this;
		}

		public static JoyAxisStateInfo GetValue(InputControlType type)
		{
			int index = s_JoyAxisStateInfos.FindIndex((JoyAxisStateInfo x) => x.controlType == type);
			return s_JoyAxisStateInfos[index].UpdateInfo();
		}

		public static void Register(InputControlType type, float[] para)
		{
			int num = s_JoyAxisStateInfos.FindIndex((JoyAxisStateInfo x) => x.controlType == type);
			if (num < 0)
			{
				s_JoyAxisStateInfos.Add(new JoyAxisStateInfo(type, para));
			}
		}

		public static void Clear()
		{
			s_JoyAxisStateInfos.Clear();
		}
	}

	private enum KeyPressType
	{
		Down = 0,
		Up = 1,
		UpHPrior = 2,
		Press = 3,
		PressHPrior = 4,
		Click = 5,
		ClickCD = 6,
		DoublePress = 7,
		LongPress = 8,
		DirU = 9,
		DirD = 10,
		DirR = 11,
		DirL = 12,
		MouseWheelU = 13,
		MouseWheelD = 14,
		ClickNoMove = 15,
		JoyStickBegin = 16,
		JoyDown = 16,
		JoyUp = 17,
		JoyPress = 18,
		JoyStickUpDoublePress = 19,
		JoyStickDownDoublePress = 20,
		JoyStickRightDoublePress = 21,
		JoyStickLeftDoublePress = 22,
		Max = 23
	}

	public class KeyJoySettingPair
	{
		public KeyCode _key;

		public InputControlType _joy;

		public string _keyDesc;

		public string _joyDesc;

		public bool _keyLock;

		public bool _joyLock;

		public KeyJoySettingPair(KeyCode key0, InputControlType key1, bool lock0 = false, bool lock1 = false, string desc0 = "", string desc1 = "")
		{
			_key = key0;
			_joy = key1;
			_keyDesc = desc0;
			_joyDesc = desc1;
			_keyLock = lock0;
			_joyLock = lock1;
		}

		public KeyJoySettingPair(KeyJoySettingPair src)
		{
			_key = src._key;
			_joy = src._joy;
			_keyDesc = src._keyDesc;
			_joyDesc = src._joyDesc;
			_keyLock = src._keyLock;
			_joyLock = src._joyLock;
		}

		public void Clone(KeyJoySettingPair src)
		{
			_key = src._key;
			_joy = src._joy;
			_keyDesc = src._keyDesc;
			_joyDesc = src._joyDesc;
			_keyLock = src._keyLock;
			_joyLock = src._joyLock;
		}

		public static implicit operator KeyJoySettingPair(KeyCode key)
		{
			return new KeyJoySettingPair(key, InputControlType.None, lock0: false, lock1: false, string.Empty, string.Empty);
		}
	}

	public enum ESettingsGeneral
	{
		Attack,
		Shoot,
		Dig,
		Build,
		Cut,
		PutOn,
		PutAway,
		Interact,
		Gather,
		Take,
		ZoomInCamera,
		ZoomOutCamera,
		AutoRun,
		Interact_Talk_Cut_Gather_Take,
		WorldMap,
		Mission,
		Inventory,
		CharacterStats,
		Rotate,
		ReplicationMenu,
		SkillMenu,
		FollowersMenu,
		HandheldPC,
		CreationSystem,
		ColonyMenu,
		FriendsMenu,
		Shop,
		OpenChatWindow_Send,
		GameMenu,
		EscMenu_CloseAllUI,
		ChangeContrlMode,
		SaveMenu,
		LoadMenu,
		ScreenCapture,
		QuickBar1,
		QuickBar2,
		QuickBar3,
		QuickBar4,
		QuickBar5,
		QuickBar6,
		QuickBar7,
		QuickBar8,
		QuickBar9,
		QuickBar10,
		PrevQuickBar,
		NextQuickBar,
		LiberatiePerspective,
		Max
	}

	private enum ESettingsChrCtrl
	{
		MoveForward,
		MoveBack,
		MoveLeft,
		MoveRight,
		Jump_SwimmingUp,
		Reload,
		Walk_Run,
		Sprint,
		Block,
		Max
	}

	public enum ESettingsBuildMd
	{
		BuildMode,
		FreeBuildingMode,
		TweakSelectionArea1,
		TweakSelectionArea2,
		TweakSelectionArea3,
		TweakSelectionArea4,
		TweakSelectionArea5,
		TweakSelectionArea6,
		RotateOnAxis,
		Undo,
		Redo,
		ChangeBrush,
		DeleteSelection,
		CrossSelection,
		SubtractSelection,
		AddSelection,
		Extrude,
		Squash,
		TopQuickBar1,
		TopQuickBar2,
		TopQuickBar3,
		TopQuickBar4,
		TopQuickBar5,
		TopQuickBar6,
		TopQuickBar7,
		Max
	}

	private enum ESettingsVehicle
	{
		Fire1,
		Fire2,
		Brake,
		ThrottleDown,
		FireMode,
		Sprint,
		VehicleLight,
		MissileLock,
		MissileLaunch,
		VehicleWeaponGroup1Toggle,
		VehicleWeaponGroup2Toggle,
		VehicleWeaponGroup3Toggle,
		VehicleWeaponGroup4Toggle,
		Max
	}

	private const int ShiftMask = 4096;

	private const int CtrlMask = 8192;

	private const int AltMask = 16384;

	private const int KeyMask = 28672;

	private const string RTAxis = "JoyRT";

	private const string LeftStickVAxis = "LeftStickVertical";

	private const string LeftStickHAxis = "LeftStickHorizontal";

	private static List<KeyExcluders> s_inputExcluders;

	private static LogicInput s_curLogicInput;

	private static List<LogicInput> s_logicInputConf;

	public static bool enable;

	public static bool UsingJoyStick;

	private static float s_rMouseLastDown;

	private static Vector2 s_rMousePosWhileDown;

	private static bool s_arrowAxisEnable;

	private static float s_curAxisH;

	private static float s_curAxisV;

	private static KeyCode s_keyAxisU;

	private static KeyCode s_keyAxisD;

	private static KeyCode s_keyAxisR;

	private static KeyCode s_keyAxisL;

	private static readonly string s_inputConfRootName;

	private static readonly string s_inputConfVersion;

	private static readonly int[] s_strIdGeneral;

	private static KeyJoySettingPair[] s_settingsGeneral;

	private static readonly KeyJoySettingPair[] s_settingsGeneralDef;

	private static readonly int[] s_strIdChrCtrl;

	private static KeyJoySettingPair[] s_settingsChrCtrl;

	private static readonly KeyJoySettingPair[] s_settingsChrCtrlDef;

	private static readonly int[] s_strIdBuildMd;

	private static KeyJoySettingPair[] s_settingsBuildMd;

	private static readonly KeyJoySettingPair[] s_settingsBuildMdDef;

	private static readonly int[] s_strIdVehicle;

	private static KeyJoySettingPair[] s_settingsVehicle;

	private static readonly KeyJoySettingPair[] s_settingsVehicleDef;

	private static readonly KeyJoySettingPair[][] s_settingsAll;

	private static readonly KeyJoySettingPair[][] s_settingsAllDef;

	public static bool ArrowAxisEnable
	{
		get
		{
			return s_arrowAxisEnable;
		}
		set
		{
			s_arrowAxisEnable = false;
		}
	}

	public static KeyJoySettingPair[][] SettingsAll => s_settingsAll;

	public static KeyJoySettingPair[] SettingsGeneral => s_settingsGeneral;

	public static KeyJoySettingPair[] SettingsChrCtrl => s_settingsChrCtrl;

	public static KeyJoySettingPair[] SettingsBuildMd => s_settingsBuildMd;

	public static KeyJoySettingPair[] SettingsVehicle => s_settingsVehicle;

	static PeInput()
	{
		s_rMouseLastDown = Time.time;
		s_rMousePosWhileDown = Vector2.zero;
		s_arrowAxisEnable = false;
		s_curAxisH = 0f;
		s_curAxisV = 0f;
		s_inputConfRootName = "InputSettings";
		s_inputConfVersion = "20161114";
		s_strIdGeneral = new int[47]
		{
			10100, 10289, 10183, 10184, 10185, 10287, 10286, 10102, 10284, 10285,
			10103, 10104, 10101, 10114, 10123, 10124, 10125, 10126, 10116, 10127,
			10128, 10129, 10130, 10131, 10132, 10133, 10134, 10148, 10288, 10150,
			10152, 10156, 10157, 10158, 10138, 10139, 10140, 10141, 10142, 10143,
			10144, 10145, 10146, 10147, 10282, 10283, 10081
		};
		s_settingsGeneral = new KeyJoySettingPair[47]
		{
			new KeyJoySettingPair(KeyCode.Mouse0, InputControlType.Action3, lock0: false, lock1: false, string.Empty, string.Empty),
			new KeyJoySettingPair(KeyCode.Mouse0, InputControlType.RightTrigger, lock0: false, lock1: false, string.Empty, string.Empty),
			new KeyJoySettingPair(KeyCode.Mouse0, InputControlType.Action3, lock0: false, lock1: false, string.Empty, string.Empty),
			KeyCode.Mouse0,
			new KeyJoySettingPair(KeyCode.Mouse0, InputControlType.Action3, lock0: false, lock1: false, string.Empty, string.Empty),
			new KeyJoySettingPair(KeyCode.Mouse0, InputControlType.Action3, lock0: false, lock1: false, string.Empty, string.Empty),
			new KeyJoySettingPair(KeyCode.Mouse1, InputControlType.Action2, lock0: false, lock1: false, string.Empty, string.Empty),
			new KeyJoySettingPair(KeyCode.Mouse1, InputControlType.Action4, lock0: false, lock1: false, string.Empty, string.Empty),
			new KeyJoySettingPair(KeyCode.Mouse1, InputControlType.Action4, lock0: false, lock1: false, string.Empty, string.Empty),
			new KeyJoySettingPair(KeyCode.Mouse1, InputControlType.Action4, lock0: false, lock1: false, string.Empty, string.Empty),
			new KeyJoySettingPair(KeyCode.Mouse2, InputControlType.None, lock0: true, lock1: true, string.Empty, string.Empty),
			new KeyJoySettingPair(KeyCode.Mouse2, InputControlType.None, lock0: true, lock1: true, string.Empty, string.Empty),
			new KeyJoySettingPair(KeyCode.Mouse2, InputControlType.LeftStickButton, lock0: false, lock1: true, string.Empty, string.Empty),
			new KeyJoySettingPair(KeyCode.E, InputControlType.Action4, lock0: false, lock1: false, string.Empty, string.Empty),
			KeyCode.M,
			KeyCode.Q,
			KeyCode.I,
			KeyCode.C,
			KeyCode.T,
			KeyCode.Y,
			AltKey(KeyCode.T),
			AltKey(KeyCode.F),
			KeyCode.H,
			KeyCode.J,
			AltKey(KeyCode.C),
			AltKey(KeyCode.Z),
			AltKey(KeyCode.X),
			KeyCode.Return,
			KeyCode.BackQuote,
			new KeyJoySettingPair(KeyCode.Escape, InputControlType.Start, lock0: false, lock1: true, string.Empty, string.Empty),
			KeyCode.F5,
			KeyCode.F9,
			KeyCode.F10,
			KeyCode.F12,
			new KeyJoySettingPair(KeyCode.Alpha1, InputControlType.DPadUp, lock0: false, lock1: true, string.Empty, string.Empty),
			new KeyJoySettingPair(KeyCode.Alpha2, InputControlType.DPadLeft, lock0: false, lock1: true, string.Empty, string.Empty),
			new KeyJoySettingPair(KeyCode.Alpha3, InputControlType.DPadDown, lock0: false, lock1: true, string.Empty, string.Empty),
			new KeyJoySettingPair(KeyCode.Alpha4, InputControlType.DPadRight, lock0: false, lock1: true, string.Empty, string.Empty),
			KeyCode.Alpha5,
			KeyCode.Alpha6,
			KeyCode.Alpha7,
			KeyCode.Alpha8,
			KeyCode.Alpha9,
			KeyCode.Alpha0,
			KeyCode.Comma,
			KeyCode.Period,
			KeyCode.LeftAlt
		};
		s_settingsGeneralDef = new KeyJoySettingPair[47]
		{
			new KeyJoySettingPair(KeyCode.Mouse0, InputControlType.Action3, lock0: false, lock1: false, string.Empty, string.Empty),
			new KeyJoySettingPair(KeyCode.Mouse0, InputControlType.RightTrigger, lock0: false, lock1: false, string.Empty, string.Empty),
			new KeyJoySettingPair(KeyCode.Mouse0, InputControlType.Action3, lock0: false, lock1: false, string.Empty, string.Empty),
			KeyCode.Mouse0,
			new KeyJoySettingPair(KeyCode.Mouse0, InputControlType.Action3, lock0: false, lock1: false, string.Empty, string.Empty),
			new KeyJoySettingPair(KeyCode.Mouse0, InputControlType.Action3, lock0: false, lock1: false, string.Empty, string.Empty),
			new KeyJoySettingPair(KeyCode.Mouse1, InputControlType.Action2, lock0: false, lock1: false, string.Empty, string.Empty),
			new KeyJoySettingPair(KeyCode.Mouse1, InputControlType.Action4, lock0: false, lock1: false, string.Empty, string.Empty),
			new KeyJoySettingPair(KeyCode.Mouse1, InputControlType.Action4, lock0: false, lock1: false, string.Empty, string.Empty),
			new KeyJoySettingPair(KeyCode.Mouse1, InputControlType.Action4, lock0: false, lock1: false, string.Empty, string.Empty),
			new KeyJoySettingPair(KeyCode.Mouse2, InputControlType.None, lock0: true, lock1: true, string.Empty, string.Empty),
			new KeyJoySettingPair(KeyCode.Mouse2, InputControlType.None, lock0: true, lock1: true, string.Empty, string.Empty),
			new KeyJoySettingPair(KeyCode.Mouse2, InputControlType.LeftStickButton, lock0: false, lock1: true, string.Empty, string.Empty),
			new KeyJoySettingPair(KeyCode.E, InputControlType.Action4, lock0: false, lock1: false, string.Empty, string.Empty),
			KeyCode.M,
			KeyCode.Q,
			KeyCode.I,
			KeyCode.C,
			KeyCode.T,
			KeyCode.Y,
			AltKey(KeyCode.T),
			AltKey(KeyCode.F),
			KeyCode.H,
			KeyCode.J,
			AltKey(KeyCode.C),
			AltKey(KeyCode.Z),
			AltKey(KeyCode.X),
			KeyCode.Return,
			KeyCode.BackQuote,
			new KeyJoySettingPair(KeyCode.Escape, InputControlType.Start, lock0: false, lock1: true, string.Empty, string.Empty),
			KeyCode.F5,
			KeyCode.F9,
			KeyCode.F10,
			KeyCode.F12,
			new KeyJoySettingPair(KeyCode.Alpha1, InputControlType.DPadUp, lock0: false, lock1: true, string.Empty, string.Empty),
			new KeyJoySettingPair(KeyCode.Alpha2, InputControlType.DPadLeft, lock0: false, lock1: true, string.Empty, string.Empty),
			new KeyJoySettingPair(KeyCode.Alpha3, InputControlType.DPadDown, lock0: false, lock1: true, string.Empty, string.Empty),
			new KeyJoySettingPair(KeyCode.Alpha4, InputControlType.DPadRight, lock0: false, lock1: true, string.Empty, string.Empty),
			KeyCode.Alpha5,
			KeyCode.Alpha6,
			KeyCode.Alpha7,
			KeyCode.Alpha8,
			KeyCode.Alpha9,
			KeyCode.Alpha0,
			KeyCode.Comma,
			KeyCode.Period,
			KeyCode.LeftAlt
		};
		s_strIdChrCtrl = new int[9] { 10105, 10107, 10109, 10111, 10113, 10115, 10118, 10119, 10120 };
		s_settingsChrCtrl = new KeyJoySettingPair[9]
		{
			new KeyJoySettingPair(KeyCode.W, InputControlType.LeftStickY, lock0: false, lock1: true, string.Empty, string.Empty),
			new KeyJoySettingPair(KeyCode.S, InputControlType.LeftStickY, lock0: false, lock1: true, string.Empty, string.Empty),
			new KeyJoySettingPair(KeyCode.A, InputControlType.LeftStickX, lock0: false, lock1: true, string.Empty, string.Empty),
			new KeyJoySettingPair(KeyCode.D, InputControlType.LeftStickX, lock0: false, lock1: true, string.Empty, string.Empty),
			new KeyJoySettingPair(KeyCode.Space, InputControlType.Action1, lock0: false, lock1: false, string.Empty, string.Empty),
			new KeyJoySettingPair(KeyCode.R, InputControlType.Action3, lock0: false, lock1: false, string.Empty, string.Empty),
			KeyCode.Z,
			new KeyJoySettingPair(KeyCode.LeftShift, InputControlType.RightBumper, lock0: false, lock1: false, string.Empty, string.Empty),
			new KeyJoySettingPair(KeyCode.LeftControl, InputControlType.LeftBumper, lock0: false, lock1: false, string.Empty, string.Empty)
		};
		s_settingsChrCtrlDef = new KeyJoySettingPair[9]
		{
			new KeyJoySettingPair(KeyCode.W, InputControlType.LeftStickY, lock0: false, lock1: true, string.Empty, string.Empty),
			new KeyJoySettingPair(KeyCode.S, InputControlType.LeftStickY, lock0: false, lock1: true, string.Empty, string.Empty),
			new KeyJoySettingPair(KeyCode.A, InputControlType.LeftStickX, lock0: false, lock1: true, string.Empty, string.Empty),
			new KeyJoySettingPair(KeyCode.D, InputControlType.LeftStickX, lock0: false, lock1: true, string.Empty, string.Empty),
			new KeyJoySettingPair(KeyCode.Space, InputControlType.Action1, lock0: false, lock1: false, string.Empty, string.Empty),
			new KeyJoySettingPair(KeyCode.R, InputControlType.Action3, lock0: false, lock1: false, string.Empty, string.Empty),
			KeyCode.Z,
			new KeyJoySettingPair(KeyCode.LeftShift, InputControlType.RightBumper, lock0: false, lock1: false, string.Empty, string.Empty),
			new KeyJoySettingPair(KeyCode.LeftControl, InputControlType.LeftBumper, lock0: false, lock1: false, string.Empty, string.Empty)
		};
		s_strIdBuildMd = new int[25]
		{
			10122, 10186, 10159, 10160, 10161, 10162, 10163, 10164, 10165, 10190,
			10191, 10192, 10193, 10194, 10195, 10196, 10197, 10198, 10166, 10167,
			10168, 10169, 10170, 10171, 10172
		};
		s_settingsBuildMd = new KeyJoySettingPair[25]
		{
			KeyCode.B,
			KeyCode.F,
			new KeyJoySettingPair(KeyCode.UpArrow, InputControlType.None, lock0: true, lock1: true, string.Empty, string.Empty),
			new KeyJoySettingPair(KeyCode.DownArrow, InputControlType.None, lock0: true, lock1: true, string.Empty, string.Empty),
			new KeyJoySettingPair(KeyCode.LeftArrow, InputControlType.None, lock0: true, lock1: true, string.Empty, string.Empty),
			new KeyJoySettingPair(KeyCode.RightArrow, InputControlType.None, lock0: true, lock1: true, string.Empty, string.Empty),
			new KeyJoySettingPair(KeyCode.PageUp, InputControlType.None, lock0: true, lock1: true, string.Empty, string.Empty),
			new KeyJoySettingPair(KeyCode.PageDown, InputControlType.None, lock0: true, lock1: true, string.Empty, string.Empty),
			new KeyJoySettingPair(KeyCode.G, InputControlType.None, lock0: true, lock1: true, string.Empty, string.Empty),
			new KeyJoySettingPair(CtrlKey(KeyCode.Z), InputControlType.None, lock0: true, lock1: true, string.Empty, string.Empty),
			new KeyJoySettingPair(CtrlKey(KeyCode.X), InputControlType.None, lock0: true, lock1: true, string.Empty, string.Empty),
			new KeyJoySettingPair(KeyCode.Tab, InputControlType.None, lock0: true, lock1: true, string.Empty, string.Empty),
			new KeyJoySettingPair(KeyCode.Delete, InputControlType.None, lock0: true, lock1: true, string.Empty, string.Empty),
			new KeyJoySettingPair(KeyCode.LeftControl, InputControlType.None, lock0: true, lock1: true, string.Empty, string.Empty),
			new KeyJoySettingPair(KeyCode.LeftAlt, InputControlType.None, lock0: true, lock1: true, string.Empty, string.Empty),
			new KeyJoySettingPair(KeyCode.LeftShift, InputControlType.None, lock0: true, lock1: true, string.Empty, string.Empty),
			new KeyJoySettingPair(ShiftKey(KeyCode.PageUp), InputControlType.None, lock0: true, lock1: true, string.Empty, string.Empty),
			new KeyJoySettingPair(ShiftKey(KeyCode.PageDown), InputControlType.None, lock0: true, lock1: true, string.Empty, string.Empty),
			ShiftKey(KeyCode.Alpha1),
			ShiftKey(KeyCode.Alpha2),
			ShiftKey(KeyCode.Alpha3),
			ShiftKey(KeyCode.Alpha4),
			ShiftKey(KeyCode.Alpha5),
			ShiftKey(KeyCode.Alpha6),
			ShiftKey(KeyCode.Alpha7)
		};
		s_settingsBuildMdDef = new KeyJoySettingPair[25]
		{
			KeyCode.B,
			KeyCode.F,
			new KeyJoySettingPair(KeyCode.UpArrow, InputControlType.None, lock0: true, lock1: true, string.Empty, string.Empty),
			new KeyJoySettingPair(KeyCode.DownArrow, InputControlType.None, lock0: true, lock1: true, string.Empty, string.Empty),
			new KeyJoySettingPair(KeyCode.LeftArrow, InputControlType.None, lock0: true, lock1: true, string.Empty, string.Empty),
			new KeyJoySettingPair(KeyCode.RightArrow, InputControlType.None, lock0: true, lock1: true, string.Empty, string.Empty),
			new KeyJoySettingPair(KeyCode.PageUp, InputControlType.None, lock0: true, lock1: true, string.Empty, string.Empty),
			new KeyJoySettingPair(KeyCode.PageDown, InputControlType.None, lock0: true, lock1: true, string.Empty, string.Empty),
			new KeyJoySettingPair(KeyCode.G, InputControlType.None, lock0: true, lock1: true, string.Empty, string.Empty),
			new KeyJoySettingPair(CtrlKey(KeyCode.Z), InputControlType.None, lock0: true, lock1: true, string.Empty, string.Empty),
			new KeyJoySettingPair(CtrlKey(KeyCode.X), InputControlType.None, lock0: true, lock1: true, string.Empty, string.Empty),
			new KeyJoySettingPair(KeyCode.Tab, InputControlType.None, lock0: true, lock1: true, string.Empty, string.Empty),
			new KeyJoySettingPair(KeyCode.Delete, InputControlType.None, lock0: true, lock1: true, string.Empty, string.Empty),
			new KeyJoySettingPair(KeyCode.LeftControl, InputControlType.None, lock0: true, lock1: true, string.Empty, string.Empty),
			new KeyJoySettingPair(KeyCode.LeftAlt, InputControlType.None, lock0: true, lock1: true, string.Empty, string.Empty),
			new KeyJoySettingPair(KeyCode.LeftShift, InputControlType.None, lock0: true, lock1: true, string.Empty, string.Empty),
			new KeyJoySettingPair(ShiftKey(KeyCode.PageUp), InputControlType.None, lock0: true, lock1: true, string.Empty, string.Empty),
			new KeyJoySettingPair(ShiftKey(KeyCode.PageDown), InputControlType.None, lock0: true, lock1: true, string.Empty, string.Empty),
			ShiftKey(KeyCode.Alpha1),
			ShiftKey(KeyCode.Alpha2),
			ShiftKey(KeyCode.Alpha3),
			ShiftKey(KeyCode.Alpha4),
			ShiftKey(KeyCode.Alpha5),
			ShiftKey(KeyCode.Alpha6),
			ShiftKey(KeyCode.Alpha7)
		};
		s_strIdVehicle = new int[13]
		{
			10212, 10213, 10201, 10202, 10203, 10204, 10205, 10206, 10207, 10208,
			10209, 10210, 10211
		};
		s_settingsVehicle = new KeyJoySettingPair[13]
		{
			KeyCode.Mouse0,
			KeyCode.Mouse1,
			KeyCode.Space,
			KeyCode.LeftAlt,
			KeyCode.F,
			KeyCode.LeftShift,
			KeyCode.L,
			KeyCode.Z,
			KeyCode.X,
			KeyCode.F1,
			KeyCode.F2,
			KeyCode.F3,
			KeyCode.F4
		};
		s_settingsVehicleDef = new KeyJoySettingPair[13]
		{
			KeyCode.Mouse0,
			KeyCode.Mouse1,
			KeyCode.Space,
			KeyCode.LeftAlt,
			KeyCode.F,
			KeyCode.LeftShift,
			KeyCode.L,
			KeyCode.Z,
			KeyCode.X,
			KeyCode.F1,
			KeyCode.F2,
			KeyCode.F3,
			KeyCode.F4
		};
		s_settingsAll = new KeyJoySettingPair[4][] { s_settingsGeneral, s_settingsChrCtrl, s_settingsBuildMd, s_settingsVehicle };
		s_settingsAllDef = new KeyJoySettingPair[4][] { s_settingsGeneralDef, s_settingsChrCtrlDef, s_settingsBuildMdDef, s_settingsVehicleDef };
		s_inputExcluders = new List<KeyExcluders>();
		s_curLogicInput = null;
		s_logicInputConf = new List<LogicInput>
		{
			new LogicInput(s_settingsGeneral[5], null, KeyPressType.Click),
			new LogicInput(s_settingsGeneral[0]),
			new LogicInput(s_settingsGeneral[0]),
			new LogicInput(s_settingsGeneral[0], null, KeyPressType.UpHPrior, null, KeyPressType.JoyUp),
			new LogicInput(s_settingsGeneral[1]),
			new LogicInput(s_settingsGeneral[1], null, KeyPressType.UpHPrior, null, KeyPressType.JoyUp),
			new LogicInput(s_settingsGeneral[0]),
			new LogicInput(s_settingsGeneral[0], null, KeyPressType.UpHPrior),
			new LogicInput(s_settingsGeneral[3], null, KeyPressType.Click),
			new LogicInput(KeyCode.Mouse0, null, KeyPressType.PressHPrior),
			new LogicInput(KeyCode.Mouse0, null, KeyPressType.Up),
			new LogicInput(KeyCode.Mouse0, MouseExcluder, KeyPressType.Click),
			new LogicInput(s_settingsGeneral[7], null, KeyPressType.Click, null, KeyPressType.JoyStickBegin, null, new LogicInput(s_settingsGeneral[13], null, KeyPressType.Click)),
			new LogicInput(s_settingsGeneral[7], null, KeyPressType.ClickNoMove),
			new LogicInput(s_settingsGeneral[8], null, KeyPressType.Click, null, KeyPressType.JoyStickBegin, null, new LogicInput(s_settingsGeneral[13], null, KeyPressType.Click)),
			new LogicInput(s_settingsGeneral[2], null, KeyPressType.Press),
			new LogicInput(s_settingsGeneral[9], null, KeyPressType.Click, null, KeyPressType.JoyStickBegin, null, new LogicInput(s_settingsGeneral[13], null, KeyPressType.Click)),
			new LogicInput(s_settingsGeneral[6], null, KeyPressType.ClickNoMove),
			new LogicInput(s_settingsGeneral[4], null, KeyPressType.Press, null, KeyPressType.Press, null, new LogicInput(s_settingsGeneral[13], null, KeyPressType.Press)),
			new LogicInput(KeyCode.Mouse1, null, KeyPressType.LongPress),
			new LogicInput(KeyCode.Mouse1, null, KeyPressType.Click),
			new LogicInput(KeyCode.Mouse1, null, KeyPressType.Click),
			new LogicInput(KeyCode.Mouse1, null, KeyPressType.ClickNoMove),
			new LogicInput(KeyCode.Mouse1, MouseExcluder, KeyPressType.Click),
			new LogicInput(s_settingsGeneral[10], null, KeyPressType.Click),
			new LogicInput(s_settingsGeneral[11], null, KeyPressType.Click),
			new LogicInput(s_settingsGeneral[12], null, KeyPressType.Click),
			new LogicInput(s_settingsGeneral[13], null, KeyPressType.Click),
			new LogicInput(s_settingsGeneral[13], null, KeyPressType.Click),
			new LogicInput(s_settingsGeneral[13], null, KeyPressType.Click),
			new LogicInput(s_settingsGeneral[18], null, KeyPressType.Click),
			new LogicInput(s_settingsGeneral[18], null, KeyPressType.Press),
			new LogicInput(s_settingsGeneral[14], null, KeyPressType.Click),
			new LogicInput(s_settingsGeneral[15], null, KeyPressType.Click),
			new LogicInput(s_settingsGeneral[16], null, KeyPressType.Click),
			new LogicInput(s_settingsGeneral[17], null, KeyPressType.Click),
			new LogicInput(s_settingsGeneral[19], null, KeyPressType.Click),
			new LogicInput(s_settingsGeneral[20], null, KeyPressType.Click),
			new LogicInput(s_settingsGeneral[21], null, KeyPressType.Click),
			new LogicInput(s_settingsGeneral[22], null, KeyPressType.Click),
			new LogicInput(s_settingsGeneral[23], null, KeyPressType.Click),
			new LogicInput(s_settingsGeneral[24], null, KeyPressType.Click),
			new LogicInput(s_settingsGeneral[25], null, KeyPressType.Click),
			new LogicInput(s_settingsGeneral[26], null, KeyPressType.Click),
			new LogicInput(s_settingsGeneral[27], null, KeyPressType.Click),
			new LogicInput(s_settingsGeneral[27], null, KeyPressType.Click),
			new LogicInput(s_settingsGeneral[27], UIDlgExcluder, KeyPressType.Click),
			new LogicInput(s_settingsGeneral[27], UIDlgExcluder, KeyPressType.Click),
			new LogicInput(s_settingsGeneral[27], null, KeyPressType.Click),
			new LogicInput(s_settingsGeneral[28], null, KeyPressType.Click),
			new LogicInput(s_settingsGeneral[29], null, KeyPressType.Click),
			new LogicInput(s_settingsGeneral[29], null, KeyPressType.Click),
			new LogicInput(s_settingsGeneral[29], null, KeyPressType.Click),
			new LogicInput(s_settingsGeneral[30], null, KeyPressType.Click),
			new LogicInput(s_settingsGeneral[31], null, KeyPressType.Click),
			new LogicInput(s_settingsGeneral[32], null, KeyPressType.Click),
			new LogicInput(s_settingsGeneral[33], null, KeyPressType.Click),
			new LogicInput(s_settingsGeneral[34], null, KeyPressType.Click),
			new LogicInput(s_settingsGeneral[35], null, KeyPressType.Click),
			new LogicInput(s_settingsGeneral[36], null, KeyPressType.Click),
			new LogicInput(s_settingsGeneral[37], null, KeyPressType.Click),
			new LogicInput(s_settingsGeneral[38], null, KeyPressType.Click),
			new LogicInput(s_settingsGeneral[39], null, KeyPressType.Click),
			new LogicInput(s_settingsGeneral[40], null, KeyPressType.Click),
			new LogicInput(s_settingsGeneral[41], null, KeyPressType.Click),
			new LogicInput(s_settingsGeneral[42], null, KeyPressType.Click),
			new LogicInput(s_settingsGeneral[43], null, KeyPressType.Click),
			new LogicInput(s_settingsGeneral[44], null, KeyPressType.Click),
			new LogicInput(s_settingsGeneral[45], null, KeyPressType.Click),
			new LogicInput(s_settingsGeneral[46], null, KeyPressType.Press),
			new LogicInput(s_settingsChrCtrl[0], null, KeyPressType.DirU),
			new LogicInput(s_settingsChrCtrl[0], null, KeyPressType.DoublePress, null, KeyPressType.JoyStickUpDoublePress),
			new LogicInput(s_settingsChrCtrl[1], null, KeyPressType.DirD),
			new LogicInput(s_settingsChrCtrl[1], null, KeyPressType.DoublePress, null, KeyPressType.JoyStickDownDoublePress),
			new LogicInput(s_settingsChrCtrl[2], null, KeyPressType.DirL),
			new LogicInput(s_settingsChrCtrl[2], null, KeyPressType.DoublePress, null, KeyPressType.JoyStickLeftDoublePress),
			new LogicInput(s_settingsChrCtrl[3], null, KeyPressType.DirR),
			new LogicInput(s_settingsChrCtrl[3], null, KeyPressType.DoublePress, null, KeyPressType.JoyStickRightDoublePress),
			new LogicInput(s_settingsChrCtrl[4]),
			new LogicInput(s_settingsChrCtrl[4], null, KeyPressType.Press, null, KeyPressType.JoyPress),
			new LogicInput(s_settingsChrCtrl[4], null, KeyPressType.Press, null, KeyPressType.JoyPress),
			new LogicInput(s_settingsChrCtrl[5], null, KeyPressType.Click),
			new LogicInput(s_settingsChrCtrl[6], null, KeyPressType.Click),
			new LogicInput(s_settingsChrCtrl[7], null, KeyPressType.Press, null, KeyPressType.JoyPress),
			new LogicInput(s_settingsChrCtrl[8], null, KeyPressType.Press, null, KeyPressType.JoyPress),
			new LogicInput(s_settingsChrCtrl[4], UIDlgExcluder, KeyPressType.Click),
			new LogicInput(s_settingsBuildMd[0], null, KeyPressType.Click),
			new LogicInput(s_settingsBuildMd[1], null, KeyPressType.Click),
			new LogicInput(s_settingsBuildMd[2], null, KeyPressType.Click),
			new LogicInput(s_settingsBuildMd[3], null, KeyPressType.Click),
			new LogicInput(s_settingsBuildMd[4], null, KeyPressType.Click),
			new LogicInput(s_settingsBuildMd[5], null, KeyPressType.Click),
			new LogicInput(s_settingsBuildMd[6], null, KeyPressType.Click),
			new LogicInput(s_settingsBuildMd[7], null, KeyPressType.Click),
			new LogicInput(s_settingsBuildMd[8], null, KeyPressType.Click),
			new LogicInput(s_settingsBuildMd[18], null, KeyPressType.Click),
			new LogicInput(s_settingsBuildMd[19], null, KeyPressType.Click),
			new LogicInput(s_settingsBuildMd[20], null, KeyPressType.Click),
			new LogicInput(s_settingsBuildMd[21], null, KeyPressType.Click),
			new LogicInput(s_settingsBuildMd[22], null, KeyPressType.Click),
			new LogicInput(s_settingsBuildMd[23], null, KeyPressType.Click),
			new LogicInput(s_settingsBuildMd[24], null, KeyPressType.Click),
			new LogicInput(s_settingsBuildMd[9], null, KeyPressType.Press),
			new LogicInput(s_settingsBuildMd[10], null, KeyPressType.Press),
			new LogicInput(s_settingsBuildMd[11], null, KeyPressType.Press),
			new LogicInput(s_settingsBuildMd[12], null, KeyPressType.Press),
			new LogicInput(s_settingsBuildMd[13], null, KeyPressType.Press),
			new LogicInput(s_settingsBuildMd[14], null, KeyPressType.Press),
			new LogicInput(s_settingsBuildMd[15], null, KeyPressType.Press),
			new LogicInput(s_settingsBuildMd[16], null, KeyPressType.Press),
			new LogicInput(s_settingsBuildMd[17], null, KeyPressType.Press),
			new LogicInput(s_settingsVehicle[0]),
			new LogicInput(s_settingsVehicle[0], null, KeyPressType.Up),
			new LogicInput(s_settingsVehicle[1]),
			new LogicInput(s_settingsVehicle[1], null, KeyPressType.Up),
			new LogicInput(s_settingsVehicle[2], null, KeyPressType.Press),
			new LogicInput(s_settingsVehicle[2], null, KeyPressType.Press),
			new LogicInput(s_settingsVehicle[3], null, KeyPressType.Press),
			new LogicInput(s_settingsVehicle[4], null, KeyPressType.Click),
			new LogicInput(s_settingsVehicle[5], null, KeyPressType.Press),
			new LogicInput(s_settingsVehicle[6], null, KeyPressType.Click),
			new LogicInput(s_settingsVehicle[7], null, KeyPressType.Click),
			new LogicInput(s_settingsVehicle[8], null, KeyPressType.Click),
			new LogicInput(s_settingsVehicle[9], null, KeyPressType.Click),
			new LogicInput(s_settingsVehicle[10], null, KeyPressType.Click),
			new LogicInput(s_settingsVehicle[11], null, KeyPressType.Click),
			new LogicInput(s_settingsVehicle[12], null, KeyPressType.Click)
		};
		enable = true;
		Input.ResetInputAxes();
		ResetSetting();
		InputModule.SetAxis("Mouse ScrollWheel", GetMouseWheel);
	}

	private static bool MouseExcluder()
	{
		return UIMouseEvent.opAnyGUI;
	}

	private static bool UIDlgExcluder()
	{
		return GameUI.Instance != null && GameUI.Instance.mNPCTalk != null && GameUI.Instance.mNPCTalk.isShow && GameUI.Instance.mNPCTalk.IsCanSkip();
	}

	private static bool KeyMaskExcluderShift()
	{
		return Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift);
	}

	private static bool KeyMaskExcluderAlt()
	{
		return Input.GetKey(KeyCode.LeftAlt) || Input.GetKey(KeyCode.RightAlt);
	}

	private static bool KeyMaskExcluderCtrl()
	{
		return Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl);
	}

	private static void ResetLogicInput()
	{
		s_inputExcluders.Clear();
		ResetPressInfo();
		int count = s_logicInputConf.Count;
		for (int i = 0; i < count; i++)
		{
			s_logicInputConf[i].ResetPressTstFunc();
		}
	}

	private static bool InputEnable()
	{
		if (!enable)
		{
			return false;
		}
		if (PlotLensAnimation.IsPlaying)
		{
			return false;
		}
		if (PlayAnimAction.playerAniming)
		{
			return false;
		}
		if (StroyManager.isPausing)
		{
			return false;
		}
		if (GameConfig.IsInVCE)
		{
			return false;
		}
		if (UICamera.inputHasFocus)
		{
			return false;
		}
		return true;
	}

	private static bool NotExcluderByOther()
	{
		int count = s_inputExcluders.Count;
		for (int i = 0; i < count; i++)
		{
			if (s_inputExcluders[i]._key != s_curLogicInput.KeyJoy._key)
			{
				continue;
			}
			List<LogicInput> excluders = s_inputExcluders[i]._excluders;
			count = excluders.Count;
			for (int j = 0; j < count; j++)
			{
				if (excluders[j] != s_curLogicInput && excluders[j].Excluder())
				{
					return false;
				}
			}
			break;
		}
		return true;
	}

	private static bool CheckLogicInput(LogicInput conf)
	{
		if (conf.Excluder != null && !conf.Excluder())
		{
			return false;
		}
		s_curLogicInput = conf;
		bool flag = conf.PressTst();
		bool flag2 = SystemSettingData.Instance.UseController && conf.JoyTst != null && conf.JoyTst();
		if (flag)
		{
			UsingJoyStick = false;
		}
		if (flag2)
		{
			UsingJoyStick = true;
		}
		return flag || flag2;
	}

	public static bool Get(LogicFunction key)
	{
		if (!InputEnable())
		{
			return false;
		}
		for (LogicInput logicInput = s_logicInputConf[(int)key]; logicInput != null; logicInput = logicInput.Alternate)
		{
			if (CheckLogicInput(logicInput))
			{
				return true;
			}
		}
		return false;
	}

	public static float GetAxisH()
	{
		if (!InputEnable())
		{
			return 0f;
		}
		return s_curAxisH;
	}

	public static float GetAxisV()
	{
		if (!InputEnable())
		{
			return 0f;
		}
		return s_curAxisV;
	}

	public static object GetMouseWheel()
	{
		if (!InputEnable())
		{
			return 0;
		}
		if (MouseExcluder())
		{
			return 0;
		}
		return Input.GetAxis("Mouse ScrollWheel");
	}

	public static void Update()
	{
		if (s_arrowAxisEnable)
		{
			UpdateAxisWithArrowKey();
		}
		else
		{
			UpdateAxisWithoutArrowKey();
		}
	}

	public static KeyCode GetKeyCodeByLogicFunKey(LogicFunction funKey)
	{
		if (funKey < LogicFunction.DrawWeapon || (int)funKey >= s_logicInputConf.Count)
		{
			return KeyCode.None;
		}
		LogicInput logicInput = s_logicInputConf[(int)funKey];
		if (logicInput == null || logicInput.KeyJoy == null)
		{
			return KeyCode.None;
		}
		return logicInput.KeyJoy._key;
	}

	public static KeyCode ShiftKey(KeyCode key)
	{
		return 4096 + key;
	}

	public static KeyCode CtrlKey(KeyCode key)
	{
		return 8192 + key;
	}

	public static KeyCode CtrlShiftKey(KeyCode key)
	{
		return 12288 + key;
	}

	public static KeyCode AltKey(KeyCode key)
	{
		return 16384 + key;
	}

	public static KeyCode AltShiftKey(KeyCode key)
	{
		return 20480 + key;
	}

	public static string ToStr(this KeyCode key)
	{
		string text = string.Empty;
		int num = (int)(key & (KeyCode)28672);
		KeyCode keyCode = key;
		if (num != 0)
		{
			keyCode = key - num;
			text = (((num & 0x1000) == 0) ? string.Empty : "Shift+") + (((num & 0x2000) == 0) ? string.Empty : "Ctrl+") + (((num & 0x4000) == 0) ? string.Empty : "Alt+");
		}
		if (keyCode >= KeyCode.Alpha0 && keyCode <= KeyCode.Alpha9)
		{
			return text + (int)(keyCode - 48);
		}
		return keyCode switch
		{
			KeyCode.Mouse0 => text + "LButton", 
			KeyCode.Mouse1 => text + "RButton", 
			KeyCode.Mouse2 => text + "MButton", 
			_ => text + keyCode, 
		};
	}

	public static string ToStrShort(this KeyCode key)
	{
		int num = (int)(key & (KeyCode)28672);
		KeyCode keyCode = key - num;
		string text = ((num == 0) ? key.ToString() : ((((num & 0x1000) == 0) ? string.Empty : "Sh+") + (((num & 0x2000) == 0) ? string.Empty : "Ct+") + (((num & 0x4000) == 0) ? string.Empty : "Al+") + keyCode));
		return text.Replace("Button", string.Empty).Replace("Alpha", string.Empty);
	}

	public static bool IsClickNoMove(this KeyCode key)
	{
		if (key != KeyCode.Mouse0 && key != KeyCode.Mouse1 && key != KeyCode.Mouse2)
		{
			return Input.GetKeyUp(key) && NotExcluderByOther();
		}
		if (Input.GetKeyDown(KeyCode.Mouse1) && NotExcluderByOther())
		{
			s_rMouseLastDown = Time.time;
			s_rMousePosWhileDown = Input.mousePosition;
		}
		else if (Input.GetKeyUp(KeyCode.Mouse1) && NotExcluderByOther())
		{
			float num = Vector2.Distance(s_rMousePosWhileDown, Input.mousePosition);
			if (num < 20f && Time.time - s_rMouseLastDown < 0.25f)
			{
				return true;
			}
		}
		return false;
	}

	public static bool IsClickPress(this KeyCode key)
	{
		if (Input.GetKeyDown(key) && NotExcluderByOther() && ClickPressInfo.TryGetValue(key, out var info))
		{
			info._lastDown = Time.time;
		}
		else if (Input.GetKeyUp(key) && NotExcluderByOther() && ClickPressInfo.TryGetValue(key, out info))
		{
			float num = Time.time - info._lastDown;
			if (num < info._interval)
			{
				return true;
			}
		}
		return false;
	}

	public static bool IsClickCDPress(this KeyCode key)
	{
		bool keyDown = Input.GetKeyDown(key);
		bool keyUp = Input.GetKeyUp(key);
		if ((keyDown || keyUp) && NotExcluderByOther() && ClickCDPressInfo.TryGetValue(key, out var info))
		{
			if (Time.time - info._lastClick < info._cd)
			{
				return false;
			}
			if (keyDown)
			{
				info._lastDown = Time.time;
			}
			else if (keyUp)
			{
				float num = Time.time - info._lastDown;
				if (num < info._interval)
				{
					info._lastClick = Time.time;
					return true;
				}
			}
		}
		return false;
	}

	public static bool IsDoublePress(this KeyCode key)
	{
		if (Input.GetKeyDown(key) && NotExcluderByOther() && DoublePressInfo.TryGetValue(key, out var info))
		{
			info._lastDown1 = Time.time;
			float num = Time.time - info._lastDown0;
			if (num < info._interval)
			{
				return true;
			}
			return false;
		}
		if (Input.GetKeyUp(key) && NotExcluderByOther() && DoublePressInfo.TryGetValue(key, out info))
		{
			info._lastDown0 = info._lastDown1;
		}
		return false;
	}

	public static bool IsLongPress(this KeyCode key)
	{
		if (LongPressInfo.TryGetValue(key, out var info))
		{
			if (!Input.GetKey(key) || !NotExcluderByOther())
			{
				info._startDown = 0f;
				return false;
			}
			if (info._startDown < float.Epsilon)
			{
				info._startDown = Time.time;
				return false;
			}
			return Time.time - info._startDown > info._term;
		}
		return false;
	}

	private static void UpdateAxisWithoutArrowKey()
	{
		if (Input.GetKeyDown(s_keyAxisU))
		{
			s_curAxisV = 1f;
			UsingJoyStick = false;
		}
		else if (Input.GetKeyDown(s_keyAxisD))
		{
			s_curAxisV = -1f;
			UsingJoyStick = false;
		}
		if (!Input.GetKey(s_keyAxisU) && !Input.GetKey(s_keyAxisD))
		{
			s_curAxisV = ((!SystemSettingData.Instance.UseController) ? 0f : ((float)InputManager.ActiveDevice.LeftStickY));
			if (Mathf.Abs(s_curAxisV) > 0.1f)
			{
				UsingJoyStick = true;
			}
		}
		else if (!Input.GetKey(s_keyAxisU))
		{
			s_curAxisV = -1f;
		}
		else if (!Input.GetKey(s_keyAxisD))
		{
			s_curAxisV = 1f;
		}
		if (Input.GetKeyDown(s_keyAxisR))
		{
			s_curAxisH = 1f;
			UsingJoyStick = false;
		}
		else if (Input.GetKeyDown(s_keyAxisL))
		{
			s_curAxisH = -1f;
			UsingJoyStick = false;
		}
		if (!Input.GetKey(s_keyAxisR) && !Input.GetKey(s_keyAxisL))
		{
			s_curAxisH = ((!SystemSettingData.Instance.UseController) ? 0f : ((float)InputManager.ActiveDevice.LeftStickX));
			if (Mathf.Abs(s_curAxisH) > 0.1f)
			{
				UsingJoyStick = true;
			}
		}
		else if (!Input.GetKey(s_keyAxisR))
		{
			s_curAxisH = -1f;
		}
		else if (!Input.GetKey(s_keyAxisL))
		{
			s_curAxisH = 1f;
		}
		if (Mathf.Abs(InputManager.ActiveDevice.RightStickX) > 0.1f || Mathf.Abs(InputManager.ActiveDevice.RightStickY) > 0.1f)
		{
			UsingJoyStick = true;
		}
		else if (Mathf.Abs(Input.GetAxis("Mouse X")) > 0.1f || Mathf.Abs(Input.GetAxis("Mouse Y")) > 0.1f)
		{
			UsingJoyStick = false;
		}
	}

	private static void UpdateAxisWithArrowKey()
	{
		if (Input.GetKeyDown(s_keyAxisU) || Input.GetKeyDown(KeyCode.UpArrow))
		{
			s_curAxisV = 1f;
			UsingJoyStick = false;
		}
		else if (Input.GetKeyDown(s_keyAxisD) || Input.GetKeyDown(KeyCode.DownArrow))
		{
			s_curAxisV = -1f;
			UsingJoyStick = false;
		}
		if (!Input.GetKey(s_keyAxisU) && !Input.GetKey(KeyCode.UpArrow) && !Input.GetKey(s_keyAxisD) && !Input.GetKey(KeyCode.DownArrow))
		{
			s_curAxisV = ((!SystemSettingData.Instance.UseController) ? 0f : ((float)InputManager.ActiveDevice.LeftStickY));
			if (Mathf.Abs(s_curAxisV) > 0.1f)
			{
				UsingJoyStick = true;
			}
		}
		else if (!Input.GetKey(s_keyAxisU) && !Input.GetKey(KeyCode.UpArrow))
		{
			s_curAxisV = -1f;
		}
		else if (!Input.GetKey(s_keyAxisD) && !Input.GetKey(KeyCode.DownArrow))
		{
			s_curAxisV = 1f;
		}
		if (Input.GetKeyDown(s_keyAxisR) || Input.GetKeyDown(KeyCode.RightArrow))
		{
			s_curAxisH = 1f;
			UsingJoyStick = false;
		}
		else if (Input.GetKeyDown(s_keyAxisL) || Input.GetKeyDown(KeyCode.LeftArrow))
		{
			s_curAxisH = -1f;
			UsingJoyStick = false;
		}
		if (!Input.GetKey(s_keyAxisR) && !Input.GetKey(KeyCode.RightArrow) && !Input.GetKey(s_keyAxisL) && !Input.GetKey(KeyCode.LeftArrow))
		{
			s_curAxisH = ((!SystemSettingData.Instance.UseController) ? 0f : ((float)InputManager.ActiveDevice.LeftStickX));
			if (Mathf.Abs(s_curAxisH) > 0.1f)
			{
				UsingJoyStick = true;
			}
		}
		else if (!Input.GetKey(s_keyAxisR) && !Input.GetKey(KeyCode.RightArrow))
		{
			s_curAxisH = -1f;
		}
		else if (!Input.GetKey(s_keyAxisL) && !Input.GetKey(KeyCode.LeftArrow))
		{
			s_curAxisH = 1f;
		}
		if (Mathf.Abs(InputManager.ActiveDevice.RightStickX) > 0.1f || Mathf.Abs(InputManager.ActiveDevice.RightStickY) > 0.1f)
		{
			UsingJoyStick = true;
		}
		else if (Mathf.Abs(Input.GetAxis("Mouse X")) > 0.1f || Mathf.Abs(Input.GetAxis("Mouse Y")) > 0.1f)
		{
			UsingJoyStick = false;
		}
	}

	private static Func<bool> CreatePressTestFunc(KeyCode key, KeyPressType pressType, float[] para)
	{
		if (pressType < KeyPressType.JoyStickBegin)
		{
			if (key == KeyCode.None)
			{
				return () => false;
			}
			int num = (int)(key & (KeyCode)28672);
			if (num != 0)
			{
				key -= num;
			}
		}
		switch (pressType)
		{
		default:
			return () => Input.GetKeyUp(key) && NotExcluderByOther();
		case KeyPressType.Down:
			return () => Input.GetKeyDown(key) && NotExcluderByOther();
		case KeyPressType.Press:
			return () => Input.GetKey(key) && NotExcluderByOther();
		case KeyPressType.UpHPrior:
			return () => Input.GetKeyUp(key);
		case KeyPressType.PressHPrior:
			return () => Input.GetKey(key);
		case KeyPressType.ClickNoMove:
			return () => key.IsClickNoMove();
		case KeyPressType.Click:
			ClickPressInfo.Register(key, para);
			return () => key.IsClickPress();
		case KeyPressType.ClickCD:
			ClickCDPressInfo.Register(key, para);
			return () => key.IsClickCDPress();
		case KeyPressType.DoublePress:
			DoublePressInfo.Register(key, para);
			return () => key.IsDoublePress();
		case KeyPressType.LongPress:
			LongPressInfo.Register(key, para);
			return () => key.IsLongPress();
		case KeyPressType.DirU:
			s_keyAxisU = key;
			return () => s_curAxisV > float.Epsilon;
		case KeyPressType.DirD:
			s_keyAxisD = key;
			return () => s_curAxisV < -1E-45f;
		case KeyPressType.DirR:
			s_keyAxisR = key;
			return () => s_curAxisH > float.Epsilon;
		case KeyPressType.DirL:
			s_keyAxisL = key;
			return () => s_curAxisH < -1E-45f;
		case KeyPressType.MouseWheelU:
			return () => Input.GetAxis("Mouse ScrollWheel") > float.Epsilon;
		case KeyPressType.MouseWheelD:
			return () => Input.GetAxis("Mouse ScrollWheel") < -1E-45f;
		}
	}

	private static Func<bool> CreatePressTestFuncJoy(InputControlType key, KeyPressType pressType, float[] para)
	{
		if (key == InputControlType.None)
		{
			return () => false;
		}
		switch (pressType)
		{
		default:
			return () => false;
		case KeyPressType.JoyStickBegin:
			return () => InputManager.ActiveDevice.GetControl(key).WasPressed;
		case KeyPressType.JoyUp:
			return () => InputManager.ActiveDevice.GetControl(key).WasReleased;
		case KeyPressType.JoyPress:
			return () => InputManager.ActiveDevice.GetControl(key).IsPressed;
		case KeyPressType.JoyStickUpDoublePress:
			JoyAxisStateInfo.Register(InputControlType.LeftStickY, para);
			return () => JoyAxisStateInfo.GetValue(InputControlType.LeftStickY).positiveDoubleDown;
		case KeyPressType.JoyStickDownDoublePress:
			JoyAxisStateInfo.Register(InputControlType.LeftStickY, para);
			return () => JoyAxisStateInfo.GetValue(InputControlType.LeftStickY).negativeDoubleDown;
		case KeyPressType.JoyStickRightDoublePress:
			JoyAxisStateInfo.Register(InputControlType.LeftStickX, para);
			return () => JoyAxisStateInfo.GetValue(InputControlType.LeftStickX).positiveDoubleDown;
		case KeyPressType.JoyStickLeftDoublePress:
			JoyAxisStateInfo.Register(InputControlType.LeftStickX, para);
			return () => JoyAxisStateInfo.GetValue(InputControlType.LeftStickX).negativeDoubleDown;
		}
	}

	private static void ResetPressInfo()
	{
		ClickPressInfo.Clear();
		ClickCDPressInfo.Clear();
		DoublePressInfo.Clear();
		LongPressInfo.Clear();
		JoyAxisStateInfo.Clear();
	}

	public static int StrIdOfGeneral(int i)
	{
		return s_strIdGeneral[i];
	}

	public static int StrIdOfChrCtrl(int i)
	{
		return s_strIdChrCtrl[i];
	}

	public static int StrIdOfBuildMd(int i)
	{
		return s_strIdBuildMd[i];
	}

	public static int StrIdOfVehicle(int i)
	{
		return s_strIdVehicle[i];
	}

	private static string GetSettingName(int type, int index)
	{
		return type switch
		{
			0 => ((ESettingsGeneral)index).ToString(), 
			1 => ((ESettingsChrCtrl)index).ToString(), 
			2 => ((ESettingsBuildMd)index).ToString(), 
			3 => ((ESettingsVehicle)index).ToString(), 
			_ => string.Empty, 
		};
	}

	private static int GetSettingIndex(int type, string name)
	{
		return type switch
		{
			0 => (int)Enum.Parse(typeof(ESettingsGeneral), name), 
			1 => (int)Enum.Parse(typeof(ESettingsChrCtrl), name), 
			2 => (int)Enum.Parse(typeof(ESettingsBuildMd), name), 
			3 => (int)Enum.Parse(typeof(ESettingsVehicle), name), 
			_ => -1, 
		};
	}

	public static void ResetSetting()
	{
		for (int i = 0; i < s_settingsAll.Length; i++)
		{
			for (int j = 0; j < s_settingsAll[i].Length; j++)
			{
				s_settingsAll[i][j].Clone(s_settingsAllDef[i][j]);
			}
		}
		ResetLogicInput();
	}

	public static void SaveInputConfig(string configFile, bool bApply = true)
	{
		XmlDocument xmlDocument = new XmlDocument();
		try
		{
			using FileStream inStream = new FileStream(configFile, FileMode.Open, FileAccess.Read, FileShare.Read);
			xmlDocument.Load(inStream);
		}
		catch (Exception ex)
		{
			GameLog.HandleIOException(ex, GameLog.EIOFileType.Settings);
			xmlDocument = new XmlDocument();
		}
		XmlElement xmlElement = (XmlElement)xmlDocument.DocumentElement.SelectSingleNode(s_inputConfRootName);
		if (xmlElement == null)
		{
			xmlElement = xmlDocument.CreateElement(s_inputConfRootName);
			xmlDocument.DocumentElement.AppendChild(xmlElement);
		}
		else
		{
			xmlElement.RemoveAll();
		}
		xmlElement.SetAttribute("Ver", s_inputConfVersion);
		for (int i = 0; i < s_settingsAll.Length; i++)
		{
			XmlElement xmlElement2 = xmlDocument.CreateElement(((EPeInputSettingsType)i).ToString());
			xmlElement.AppendChild(xmlElement2);
			for (int j = 0; j < s_settingsAll[i].Length; j++)
			{
				XmlElement xmlElement3 = xmlDocument.CreateElement(GetSettingName(i, j));
				xmlElement2.AppendChild(xmlElement3);
				KeyJoySettingPair keyJoySettingPair = s_settingsAll[i][j];
				int key = (int)keyJoySettingPair._key;
				xmlElement3.SetAttribute("Key", key.ToString());
				int joy = (int)keyJoySettingPair._joy;
				xmlElement3.SetAttribute("Joy", joy.ToString());
				xmlElement3.SetAttribute("KeyLock", Convert.ToString(keyJoySettingPair._keyLock));
				xmlElement3.SetAttribute("JoyLock", Convert.ToString(keyJoySettingPair._joyLock));
				xmlElement3.SetAttribute("KeyDes", keyJoySettingPair._keyDesc);
				xmlElement3.SetAttribute("JoyDes", keyJoySettingPair._joyDesc);
			}
		}
		try
		{
			using FileStream outStream = new FileStream(configFile, FileMode.Create, FileAccess.Write, FileShare.None);
			xmlDocument.Save(outStream);
		}
		catch (Exception ex2)
		{
			GameLog.HandleIOException(ex2, GameLog.EIOFileType.Settings);
		}
		if (bApply)
		{
			ResetLogicInput();
		}
	}

	public static void LoadInputConfig(string configFile)
	{
		XmlDocument xmlDocument = new XmlDocument();
		try
		{
			using FileStream inStream = new FileStream(configFile, FileMode.Open, FileAccess.Read, FileShare.Read);
			xmlDocument.Load(inStream);
		}
		catch (Exception ex)
		{
			GameLog.HandleIOException(ex, GameLog.EIOFileType.Settings);
			xmlDocument = new XmlDocument();
		}
		XmlElement xmlElement = (XmlElement)xmlDocument.DocumentElement.SelectSingleNode(s_inputConfRootName);
		if (xmlElement == null || !xmlElement.HasAttribute("Ver") || xmlElement.GetAttribute("Ver") != s_inputConfVersion)
		{
			SaveInputConfig(configFile, bApply: false);
			return;
		}
		bool flag = false;
		foreach (XmlNode childNode in xmlElement.ChildNodes)
		{
			try
			{
				int num = (int)Enum.Parse(typeof(EPeInputSettingsType), childNode.Name);
				KeyJoySettingPair[] array = s_settingsAll[num];
				foreach (XmlNode childNode2 in childNode.ChildNodes)
				{
					try
					{
						XmlElement xmlElement2 = (XmlElement)childNode2;
						KeyCode key = (KeyCode)Convert.ToInt32(xmlElement2.GetAttribute("Key"));
						InputControlType key2 = (InputControlType)Convert.ToInt32(xmlElement2.GetAttribute("Joy"));
						bool @lock = Convert.ToBoolean(xmlElement2.GetAttribute("KeyLock"));
						bool lock2 = Convert.ToBoolean(xmlElement2.GetAttribute("JoyLock"));
						string attribute = xmlElement2.GetAttribute("KeyDes");
						string attribute2 = xmlElement2.GetAttribute("JoyDes");
						int settingIndex = GetSettingIndex(num, childNode2.Name);
						array[settingIndex].Clone(new KeyJoySettingPair(key, key2, @lock, lock2, attribute, attribute2));
					}
					catch
					{
						flag = true;
						Debug.LogError("[PeInput]Error occured while reading xmlnode:" + childNode.Name + "-" + childNode2.Name);
					}
				}
			}
			catch
			{
				flag = true;
				Debug.LogError("[PeInput]Error occured while reading settings type:" + childNode.Name);
			}
		}
		KeyJoySettingPair[] array2 = s_settingsAll[1];
		if (array2[0]._joy == InputControlType.None)
		{
			array2[0]._joy = InputControlType.LeftStickY;
		}
		if (array2[1]._joy == InputControlType.None)
		{
			array2[1]._joy = InputControlType.LeftStickY;
		}
		if (array2[2]._joy == InputControlType.None)
		{
			array2[2]._joy = InputControlType.LeftStickX;
		}
		if (array2[3]._joy == InputControlType.None)
		{
			array2[3]._joy = InputControlType.LeftStickX;
		}
		if (flag)
		{
			SaveInputConfig(configFile);
		}
		else
		{
			ResetLogicInput();
		}
	}
}

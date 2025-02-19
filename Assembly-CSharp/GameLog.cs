using System;
using Pathea;
using UnityEngine;

public class GameLog : MonoBehaviour
{
	private enum EStr
	{
		Quit = 0,
		SendReport = 1,
		ErrorOccured = 2,
		Sending = 3,
		SendAndQuit = 4,
		StepsToReproduce = 5,
		OclError = 6,
		NotEnoughSpace = 7,
		CorruptSave = 8,
		CorruptFile = 9,
		NoAuthority = 10,
		MissingNetFramework = 11,
		FileNotFound = 9
	}

	private enum ErrGuiStep
	{
		Step_Idle,
		Step_Report,
		Step_QuitGame,
		Step_Dying
	}

	public enum EIOFileType
	{
		SaveData,
		Settings,
		InstallFiles,
		Other
	}

	private static readonly string[] c_strListEn = new string[12]
	{
		"Quit", "Send  Report", "A fatal error has occured, the game can not continue! \r\nPressing 'Send Report' will send us helpful debugging information that may \r\nhelp us resolve this issue in the future.", "Sending ", "Send  and  Quit", "Steps to reproduce:", "Error occurred in OpenCL kernel, which might be caused by Display Driver Stopped Responding.\r\n\r\nPlease go to launcher options and select [Cpu] in the OpenCL Calculation options if this error occurs repeatedly", "There is not enough space on the disk!", "Save data corrupted or autosave failed!", "Some files are corrupt, please verify the integrity of the game cache.",
		"No authorization to access a file, please check file permissions and run the game as administrator!", "Missing .NET Framework 3.5! \r\n\r\nSteam would install .NET framework 3.5 before running the game, but the installation seems to have failed!\r\nYou can manually install it. See more details on http://steamcommunity.com/app/237870/discussions/0/343788552540901475/"
	};

	private static readonly string[] c_strListCn = new string[12]
	{
		"退 出 游 戏", "发 送 报 告", "出错啦!游戏发生错误需要关闭! \r\n按下 '发送报告' 会给我们发送错误信息报告,帮助我们解决这个问题.", "发 送 中 ", "发 送 并 退 出", "重现Bug的步骤:", "OpenCL内核运行出错, 可能是因为显示驱动停止响应.\r\n\r\n如果该问题反复发生, 请在游戏启动器选项里的[OpenCL Calculation]选择[CPU].", "磁盘空间不足!", "存档损坏或自动存档失败!", "文件损坏, 请验证游戏完整性!",
		"访问文件时没有权限, 请检查文件是否可读写, 并以管理员权限运行游戏!", "缺少.NET Framework 3.5! \r\n\r\nSteam会在游戏启动之前安装.NET framework 3.5, 但是安装可能被取消或失败了!\r\n你可以手动安装它. 安装过程请参考http://steamcommunity.com/app/237870/discussions/0/343788552540901475/"
	};

	private static string[] _strListInUse = c_strListEn;

	private static string _strRuntimeError = string.Empty;

	private static string _strToThrowToMainThread = string.Empty;

	public GUISkin GSkin;

	public string Tips = "[Ctrl]+[Alt]+[L][O][G] to show log";

	private ErrGuiStep _bugReportStep;

	private DateTime _lastLogTime = default(DateTime);

	private string _reproduceDesc = string.Empty;

	public static bool IsFatalError => !string.IsNullOrEmpty(_strRuntimeError);

	private void Awake()
	{
		_bugReportStep = ErrGuiStep.Step_Idle;
		UnityEngine.Object.DontDestroyOnLoad(this);
	}

	private void Start()
	{
	}

	private void OnEnable()
	{
		Application.logMessageReceived += HandleLog;
	}

	private void OnDisable()
	{
		Application.logMessageReceived -= HandleLog;
	}

	private void OnGUI()
	{
		GUI.skin = GSkin;
		GUI.depth = -1000;
		switch (_bugReportStep)
		{
		case ErrGuiStep.Step_Report:
		{
			Time.timeScale = 0f;
			GUI.Label(new Rect(0f, 0f, Screen.width, Screen.height), string.Empty, "BlackMask");
			GUI.Label(new Rect(40f, 40f, 24f, 24f), string.Empty, "ErrorTip");
			GUI.Label(new Rect(70f, 46f, Screen.width - 140, Screen.height - 70), _strRuntimeError, "WhiteVerdanaText");
			Rect position = new Rect((Screen.width - 566) / 2, (Screen.height - 156) / 2, 566f, 156f);
			GUI.BeginGroup(position, string.Empty, "MsgBoxWindow");
			GUI.Label(new Rect(1f, 7f, position.width, 26f), "System Error", "MsgCaptionTextSD");
			GUI.Label(new Rect(0f, 6f, position.width, 26f), "System Error", "MsgCaptionText");
			GUI.Label(new Rect(12f, 36f, 240f, 20f), _strListInUse[5], "WhiteVerdanaText");
			_reproduceDesc = GUI.TextArea(new Rect(20f, 56f, 526f, 60f), _reproduceDesc);
			int num = ((int)position.width - 108) / 2;
			if (BugReporter.IsSending)
			{
				string text = _strListInUse[3];
				int length = (int)Time.realtimeSinceStartup % text.Length;
				GUI.Button(new Rect(num, 116f, 108f, 24f), text.Substring(0, length), "ButtonStyle");
			}
			else if (GUI.Button(new Rect(num, 116f, 108f, 24f), _strListInUse[4], "ButtonStyle"))
			{
				BugReporter.SendEmailAsync(_strRuntimeError + "\nReproduce Steps:\n" + _reproduceDesc, 5, delegate
				{
					_bugReportStep = ErrGuiStep.Step_QuitGame;
				});
			}
			GUI.EndGroup();
			return;
		}
		case ErrGuiStep.Step_QuitGame:
			_bugReportStep = ErrGuiStep.Step_Dying;
			Debug.Log(_lastLogTime.ToString("G") + "[Quit Game Unexpectedly]");
			Application.Quit();
			return;
		case ErrGuiStep.Step_Dying:
			return;
		}
		if (_strRuntimeError.Length > 0)
		{
			Time.timeScale = 0f;
			GUI.Label(new Rect(0f, 0f, Screen.width, Screen.height), string.Empty, "BlackMask");
			GUI.Label(new Rect(40f, 40f, 24f, 24f), string.Empty, "ErrorTip");
			GUI.Label(new Rect(70f, 46f, Screen.width - 140, Screen.height - 70), _strRuntimeError, "WhiteVerdanaText");
			Rect position2 = new Rect((Screen.width - 566) / 2, (Screen.height - 156) / 2, 566f, 156f);
			GUI.BeginGroup(position2, string.Empty, "MsgBoxWindow");
			GUI.Label(new Rect(1f, 7f, position2.width, 26f), "System Error", "MsgCaptionTextSD");
			GUI.Label(new Rect(0f, 6f, position2.width, 26f), "System Error", "MsgCaptionText");
			GUI.Label(new Rect(25f, 46f, 48f, 48f), string.Empty, "ErrorSignal");
			GUI.Label(new Rect(88f, 59f, 450f, 90f), _strListInUse[2], "WhiteVerdanaText");
			int num2 = ((int)position2.width - 108) / 2;
			if (GUI.Button(new Rect(num2 - 100, 112f, 108f, 24f), _strListInUse[0], "ButtonStyle"))
			{
				_bugReportStep = ErrGuiStep.Step_QuitGame;
			}
			if (GUI.Button(new Rect(num2 + 100, 112f, 108f, 24f), _strListInUse[1], "ButtonStyle"))
			{
				_bugReportStep = ErrGuiStep.Step_Report;
			}
			GUI.EndGroup();
		}
		else if (!string.IsNullOrEmpty(_strToThrowToMainThread))
		{
			Debug.LogError(_strToThrowToMainThread);
			_strToThrowToMainThread = string.Empty;
		}
	}

	private void HandleLog(string logString, string stackTrace, LogType type)
	{
		if ((type != LogType.Assert && type != 0 && type != LogType.Exception) || !string.IsNullOrEmpty(_strRuntimeError))
		{
			return;
		}
		int num = logString.IndexOf("Exception");
		int num2 = logString.IndexOf("\n");
		if (num2 < 0)
		{
			num2 = logString.Length;
		}
		if (num >= 0 && num < num2)
		{
			_strListInUse = ((!SystemSettingData.Instance.IsChinese) ? c_strListEn : c_strListCn);
			if (logString.Contains("OclKernelError"))
			{
				_strRuntimeError = _strListInUse[6];
			}
			else if (logString.Contains("IOException: Win32 IO returned 112.") || logString.Contains("IOException: Disk full."))
			{
				_strRuntimeError = _strListInUse[7] + "\r\n\r\n" + logString + "\r\n" + stackTrace;
			}
			else if (logString.Contains("SaveDataCorrupt"))
			{
				_strRuntimeError = _strListInUse[8] + "\r\n\r\n" + logString + "\r\n" + stackTrace;
			}
			else if (logString.Contains("FilesCorrupt") || logString.Contains("IO.EndOfStreamException"))
			{
				_strRuntimeError = _strListInUse[9] + "\r\n\r\n" + logString + "\r\n" + stackTrace;
			}
			else if (logString.Contains("IO.FileNotFoundException"))
			{
				_strRuntimeError = _strListInUse[9] + "\r\n\r\n" + logString + "\r\n" + stackTrace;
			}
			else if (logString.Contains("UnauthorizedAccessException"))
			{
				_strRuntimeError = _strListInUse[10] + "\r\n\r\n" + logString + "\r\n" + stackTrace;
			}
			else if (logString.Contains("DllNotFoundException"))
			{
				_strRuntimeError = _strListInUse[11];
			}
			else
			{
				_strRuntimeError = logString + "\r\n\r\n" + stackTrace;
			}
			_lastLogTime = DateTime.Now;
			try
			{
				Cursor.lockState = (Screen.fullScreen ? CursorLockMode.Confined : CursorLockMode.None);
				Cursor.visible = true;
				PeCamera.SetVar("ForceShowCursor", true);
			}
			catch
			{
			}
		}
	}

	public static void HandleIOException(Exception ex, EIOFileType type = EIOFileType.SaveData)
	{
		string text = ex.ToString();
		if (text.Contains("IOException: Win32 IO returned 112."))
		{
			Debug.LogError(text);
			return;
		}
		if (text.Contains("UnauthorizedAccessException"))
		{
			Debug.LogError(text);
			return;
		}
		switch (type)
		{
		case EIOFileType.SaveData:
			if (PeSingleton<ArchiveMgr>.Instance != null && PeSingleton<ArchiveMgr>.Instance.autoSave)
			{
				Debug.LogWarning("AutoSaveDataCorrupt:" + text);
			}
			else
			{
				Debug.LogError("SaveDataCorrupt:" + text);
			}
			break;
		case EIOFileType.InstallFiles:
			Debug.LogError("FilesCorrupt:" + text);
			break;
		default:
			Debug.LogWarning(ex);
			break;
		}
	}

	public static void HandleExceptionInThread(Exception ex)
	{
		string text = ex.ToString();
		if (text.Contains("IOException: Win32 IO returned 112."))
		{
			_strToThrowToMainThread = text;
		}
	}
}

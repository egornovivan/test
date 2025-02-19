using System;
using System.IO;
using FMOD;
using FMOD.Studio;
using UnityEngine;

public class FMODAudioSystem : MonoBehaviour
{
	public string[] pluginPaths = new string[0];

	public static bool isShutDown;

	public static FMOD.Studio.System system
	{
		get
		{
			if (FMOD_StudioSystem.instance != null && FMOD_StudioSystem.instance.System != null && FMOD_StudioSystem.instance.System.isValid())
			{
				return FMOD_StudioSystem.instance.System;
			}
			return null;
		}
	}

	private string pluginPath
	{
		get
		{
			if (Application.platform == RuntimePlatform.WindowsEditor)
			{
				return Application.dataPath + "/Plugins/x86_64";
			}
			if (Application.platform == RuntimePlatform.WindowsPlayer || Application.platform == RuntimePlatform.OSXEditor || Application.platform == RuntimePlatform.OSXPlayer || Application.platform == RuntimePlatform.OSXDashboardPlayer || Application.platform == RuntimePlatform.LinuxPlayer)
			{
				return Application.dataPath + "/Plugins";
			}
			if (Application.platform == RuntimePlatform.IPhonePlayer)
			{
				UnityUtil.LogError("DSP Plugins not currently supported on iOS, contact support@fmod.org for more information");
				return string.Empty;
			}
			if (Application.platform == RuntimePlatform.Android)
			{
				DirectoryInfo directoryInfo = new DirectoryInfo(Application.persistentDataPath);
				string text = directoryInfo.Parent.Name;
				return "/data/data/" + text + "/lib";
			}
			UnityUtil.LogError("Unknown platform!");
			return string.Empty;
		}
	}

	private void OnApplicationQuit()
	{
		isShutDown = true;
	}

	private void Awake()
	{
		try
		{
			UnityEngine.Object.DontDestroyOnLoad(this);
		}
		catch (Exception message)
		{
			UnityEngine.Debug.LogWarning(message);
		}
	}

	private void loadBank(string fileName)
	{
		string streamingAssetPath = getStreamingAssetPath(fileName);
		Bank bank = null;
		RESULT rESULT = FMOD_StudioSystem.instance.System.loadBankFile(streamingAssetPath, LOAD_BANK_FLAGS.NORMAL, out bank);
		switch (rESULT)
		{
		case RESULT.ERR_VERSION:
			UnityUtil.LogError($"Bank {fileName} built with an incompatible version of FMOD Studio.");
			break;
		default:
			UnityUtil.LogError($"Bank {fileName} encountered a loading error {rESULT.ToString()}");
			break;
		case RESULT.OK:
		case RESULT.ERR_EVENT_ALREADY_LOADED:
			break;
		}
		UnityUtil.Log("bank load: " + ((!(bank != null)) ? "failed!!" : "suceeded"));
	}

	private string getStreamingAssetPath(string fileName)
	{
		string empty = string.Empty;
		empty = ((Application.platform == RuntimePlatform.Android) ? ("jar:file://" + Application.dataPath + "!/assets") : ((Application.platform != RuntimePlatform.MetroPlayerARM && Application.platform != RuntimePlatform.MetroPlayerX86 && Application.platform != RuntimePlatform.MetroPlayerX64) ? Application.streamingAssetsPath : "ms-appx:///Data/StreamingAssets"));
		return empty + "/" + fileName;
	}

	private void Initialize()
	{
		UnityUtil.Log("Initialize FMOD Audio System");
		LoadPlugins();
		string streamingAssetPath = getStreamingAssetPath("FMOD_bank_list.txt");
		UnityUtil.Log("Loading Banks");
		try
		{
			string[] array = File.ReadAllLines(streamingAssetPath);
			string[] array2 = array;
			foreach (string text in array2)
			{
				UnityUtil.Log("Load " + text);
				loadBank(text);
			}
		}
		catch (Exception ex)
		{
			UnityUtil.LogError("Cannot read " + streamingAssetPath + ": " + ex.Message + " : No banks loaded.");
		}
	}

	private void Start()
	{
		FMODAudioListener componentInChildren = GetComponentInChildren<FMODAudioListener>();
		if (componentInChildren == null)
		{
			AudioListener componentInChildren2 = GetComponentInChildren<AudioListener>();
			if (componentInChildren2 != null)
			{
				componentInChildren = componentInChildren2.gameObject.AddComponent<FMODAudioListener>();
			}
			else
			{
				componentInChildren = base.gameObject.AddComponent<FMODAudioListener>();
			}
		}
		FMOD_StudioSystem.CreateMono(base.gameObject);
		Initialize();
	}

	private void LoadPlugins()
	{
		FMOD.System system = null;
		ERRCHECK(FMOD_StudioSystem.instance.System.getLowLevelSystem(out system));
		if (Application.platform == RuntimePlatform.IPhonePlayer && pluginPaths.Length != 0)
		{
			UnityUtil.LogError("DSP Plugins not currently supported on iOS, contact support@fmod.org for more information");
			return;
		}
		string[] array = pluginPaths;
		foreach (string rawName in array)
		{
			string text = pluginPath + "/" + GetPluginFileName(rawName);
			UnityUtil.Log("Loading plugin: " + text);
			if (!File.Exists(text))
			{
				UnityUtil.LogWarning("plugin not found: " + text);
			}
			ERRCHECK(system.loadPlugin(text, out var _));
		}
	}

	private string GetPluginFileName(string rawName)
	{
		if (Application.platform == RuntimePlatform.WindowsEditor || Application.platform == RuntimePlatform.WindowsPlayer)
		{
			return rawName + ".dll";
		}
		if (Application.platform == RuntimePlatform.OSXEditor || Application.platform == RuntimePlatform.OSXPlayer || Application.platform == RuntimePlatform.OSXDashboardPlayer)
		{
			return rawName + ".dylib";
		}
		if (Application.platform == RuntimePlatform.Android || Application.platform == RuntimePlatform.LinuxPlayer)
		{
			return "lib" + rawName + ".so";
		}
		UnityUtil.LogError("Unknown platform!");
		return string.Empty;
	}

	private void ERRCHECK(RESULT result)
	{
		UnityUtil.ERRCHECK(result);
	}
}

using System.Collections.Generic;
using Behave.Runtime;
using UnityEngine;

public class GlobalBehaviour : MonoBehaviour
{
	public delegate bool GlobalEvent();

	public static string currentSceneName;

	public static string BadGfxDeviceName = null;

	private static List<GlobalEvent> _golobalEventList = new List<GlobalEvent>();

	public static void RegisterEvent(GlobalEvent func)
	{
		if (!_golobalEventList.Contains(func))
		{
			_golobalEventList.Add(func);
		}
	}

	private static void UnRegisterEvent(GlobalEvent func)
	{
		_golobalEventList.Remove(func);
	}

	private static void RunGolobalEvent()
	{
		foreach (GlobalEvent golobalEvent in _golobalEventList)
		{
			if (golobalEvent != null && golobalEvent())
			{
				UnRegisterEvent(golobalEvent);
				break;
			}
		}
	}

	private void Awake()
	{
		string graphicsDeviceName = SystemInfo.graphicsDeviceName;
		string processorType = SystemInfo.processorType;
		Debug.Log("[processorType]:" + processorType);
		Debug.Log("[graphicsDeviceName]:" + graphicsDeviceName);
		if (graphicsDeviceName.Contains("Intel"))
		{
			if (graphicsDeviceName.Contains("HD"))
			{
				BadGfxDeviceName = graphicsDeviceName;
			}
		}
		else if (graphicsDeviceName.Contains("Radeon") && processorType.Contains("APU"))
		{
			if (processorType.Contains("HD") && graphicsDeviceName.Contains("HD"))
			{
				BadGfxDeviceName = graphicsDeviceName;
			}
			else
			{
				string text = "Rx Graphics";
				string value = processorType.Substring(processorType.Length - text.Length);
				if (graphicsDeviceName.Contains(value))
				{
					BadGfxDeviceName = graphicsDeviceName;
				}
			}
		}
		Object.DontDestroyOnLoad(base.gameObject);
		LocalDatabase.LoadAllData();
		SystemSettingData.Instance.LoadSystemData();
		SurfExtractorsMan.CheckGenSurfExtractor();
		Singleton<PeLogicGlobal>.Instance.Init();
		PeCamera.Init();
		StartCoroutine(BTResolver.ApplyCacheBt());
	}

	private void OnApplicationQuit()
	{
		SurfExtractorsMan.CleanUp();
		LocalDatabase.FreeAllData();
		SystemSettingData.Save();
	}

	private void Update()
	{
		PeInput.Update();
		PeEnv.Update();
		RunGolobalEvent();
	}

	private void LateUpdate()
	{
		PeCamera.Update();
		currentSceneName = Application.loadedLevelName;
		SurfExtractorsMan.PostProc();
	}
}

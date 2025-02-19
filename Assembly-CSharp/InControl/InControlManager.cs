using System;
using System.Collections.Generic;
using UnityEngine;

namespace InControl;

public class InControlManager : Singleton<InControlManager>
{
	public bool logDebugInfo;

	public bool invertYAxis;

	public bool enableXInput;

	public bool useFixedUpdate;

	public bool dontDestroyOnLoad;

	public List<string> customProfiles = new List<string>();

	private void OnEnable()
	{
		if (logDebugInfo)
		{
			Debug.Log(string.Concat("InControl (version ", InputManager.Version, ")"));
			Logger.OnLogMessage += HandleOnLogMessage;
		}
		InputManager.InvertYAxis = invertYAxis;
		InputManager.EnableXInput = enableXInput;
		InputManager.SetupInternal();
		foreach (string customProfile in customProfiles)
		{
			Type type = Type.GetType(customProfile);
			if (type == null)
			{
				Debug.LogError("Cannot find class for custom profile: " + customProfile);
				continue;
			}
			UnityInputDeviceProfile profile = Activator.CreateInstance(type) as UnityInputDeviceProfile;
			InputManager.AttachDevice(new UnityInputDevice(profile));
		}
		if (dontDestroyOnLoad)
		{
			UnityEngine.Object.DontDestroyOnLoad(this);
		}
	}

	private void OnDisable()
	{
		InputManager.ResetInternal();
	}

	private void Update()
	{
		if (!useFixedUpdate || Mathf.Approximately(Time.timeScale, 0f))
		{
			InputManager.UpdateInternal();
		}
	}

	private void FixedUpdate()
	{
		if (useFixedUpdate)
		{
			InputManager.UpdateInternal();
		}
	}

	private void OnApplicationFocus(bool focusState)
	{
		InputManager.OnApplicationFocus(focusState);
	}

	private void OnApplicationPause(bool pauseState)
	{
		InputManager.OnApplicationPause(pauseState);
	}

	private void OnApplicationQuit()
	{
		InputManager.OnApplicationQuit();
	}

	private void HandleOnLogMessage(LogMessage logMessage)
	{
		switch (logMessage.type)
		{
		case LogMessageType.Info:
			Debug.Log(logMessage.text);
			break;
		case LogMessageType.Warning:
			Debug.LogWarning(logMessage.text);
			break;
		case LogMessageType.Error:
			Debug.LogError(logMessage.text);
			break;
		}
	}
}

using System;
using System.Collections.Generic;
using UnityEngine;

namespace InControl;

public class UnityInputDeviceManager : InputDeviceManager
{
	private const float deviceRefreshInterval = 1f;

	private float deviceRefreshTimer;

	private List<UnityInputDeviceProfile> deviceProfiles = new List<UnityInputDeviceProfile>();

	private bool keyboardDevicesAttached;

	private string joystickHash = string.Empty;

	private static string JoystickHash
	{
		get
		{
			string[] joystickNames = Input.GetJoystickNames();
			return joystickNames.Length + ": " + string.Join(", ", joystickNames);
		}
	}

	public UnityInputDeviceManager()
	{
		AutoDiscoverDeviceProfiles();
		RefreshDevices();
	}

	public override void Update(ulong updateTick, float deltaTime)
	{
		deviceRefreshTimer += deltaTime;
		if (string.IsNullOrEmpty(joystickHash) || deviceRefreshTimer >= 1f)
		{
			deviceRefreshTimer = 0f;
			if (joystickHash != JoystickHash)
			{
				Logger.LogInfo("Change in Unity attached joysticks detected; refreshing device list.");
				RefreshDevices();
			}
		}
	}

	private void RefreshDevices()
	{
		AttachKeyboardDevices();
		DetectAttachedJoystickDevices();
		DetectDetachedJoystickDevices();
		joystickHash = JoystickHash;
	}

	private void AttachDevice(UnityInputDevice device)
	{
		devices.Add(device);
		InputManager.AttachDevice(device);
	}

	private void AttachKeyboardDevices()
	{
		int count = deviceProfiles.Count;
		for (int i = 0; i < count; i++)
		{
			UnityInputDeviceProfile unityInputDeviceProfile = deviceProfiles[i];
			if (unityInputDeviceProfile.IsNotJoystick && unityInputDeviceProfile.IsSupportedOnThisPlatform)
			{
				AttachKeyboardDeviceWithConfig(unityInputDeviceProfile);
			}
		}
	}

	private void AttachKeyboardDeviceWithConfig(UnityInputDeviceProfile config)
	{
		if (!keyboardDevicesAttached)
		{
			UnityInputDevice device = new UnityInputDevice(config);
			AttachDevice(device);
			keyboardDevicesAttached = true;
		}
	}

	private void DetectAttachedJoystickDevices()
	{
		try
		{
			string[] joystickNames = Input.GetJoystickNames();
			for (int i = 0; i < joystickNames.Length; i++)
			{
				DetectAttachedJoystickDevice(i + 1, joystickNames[i]);
			}
		}
		catch (Exception ex)
		{
			Logger.LogError(ex.Message);
			Logger.LogError(ex.StackTrace);
		}
	}

	private void DetectAttachedJoystickDevice(int unityJoystickId, string unityJoystickName)
	{
		if (unityJoystickName == "WIRED CONTROLLER" || unityJoystickName == " WIRED CONTROLLER" || unityJoystickName.IndexOf("webcam", StringComparison.OrdinalIgnoreCase) != -1 || (InputManager.UnityVersion <= new VersionInfo(4, 5) && (Application.platform == RuntimePlatform.OSXEditor || Application.platform == RuntimePlatform.OSXPlayer || Application.platform == RuntimePlatform.OSXWebPlayer) && unityJoystickName == "Unknown Wireless Controller") || (InputManager.UnityVersion >= new VersionInfo(4, 6, 3) && (Application.platform == RuntimePlatform.WindowsEditor || Application.platform == RuntimePlatform.WindowsPlayer || Application.platform == RuntimePlatform.WindowsWebPlayer) && string.IsNullOrEmpty(unityJoystickName)))
		{
			return;
		}
		UnityInputDeviceProfile unityInputDeviceProfile = deviceProfiles.Find((UnityInputDeviceProfile config) => config.HasJoystickName(unityJoystickName));
		if (unityInputDeviceProfile == null)
		{
			unityInputDeviceProfile = deviceProfiles.Find((UnityInputDeviceProfile config) => config.HasLastResortRegex(unityJoystickName));
		}
		UnityInputDeviceProfile unityInputDeviceProfile2 = null;
		if (unityInputDeviceProfile == null)
		{
			unityInputDeviceProfile2 = new UnityUnknownDeviceProfile(unityJoystickName);
			deviceProfiles.Add(unityInputDeviceProfile2);
		}
		else
		{
			unityInputDeviceProfile2 = unityInputDeviceProfile;
		}
		int count = devices.Count;
		for (int i = 0; i < count; i++)
		{
			InputDevice inputDevice = devices[i];
			if (inputDevice is UnityInputDevice unityInputDevice && unityInputDevice.IsConfiguredWith(unityInputDeviceProfile2, unityJoystickId))
			{
				Logger.LogInfo("Device \"" + unityJoystickName + "\" is already configured with " + unityInputDeviceProfile2.Name);
				return;
			}
		}
		if (!unityInputDeviceProfile2.IsHidden)
		{
			UnityInputDevice device = new UnityInputDevice(unityInputDeviceProfile2, unityJoystickId);
			AttachDevice(device);
			if (unityInputDeviceProfile == null)
			{
				Logger.LogWarning("Device " + unityJoystickId + " with name \"" + unityJoystickName + "\" does not match any known profiles.");
			}
			else
			{
				Logger.LogInfo("Device " + unityJoystickId + " matched profile " + unityInputDeviceProfile2.GetType().Name + " (" + unityInputDeviceProfile2.Name + ")");
			}
		}
		else
		{
			Logger.LogInfo("Device " + unityJoystickId + " matching profile " + unityInputDeviceProfile2.GetType().Name + " (" + unityInputDeviceProfile2.Name + ") is hidden and will not be attached.");
		}
	}

	private void DetectDetachedJoystickDevices()
	{
		string[] joystickNames = Input.GetJoystickNames();
		for (int num = devices.Count - 1; num >= 0; num--)
		{
			UnityInputDevice unityInputDevice = devices[num] as UnityInputDevice;
			if (!unityInputDevice.Profile.IsNotJoystick && (joystickNames.Length < unityInputDevice.JoystickId || !unityInputDevice.Profile.HasJoystickOrRegexName(joystickNames[unityInputDevice.JoystickId - 1])))
			{
				devices.Remove(unityInputDevice);
				InputManager.DetachDevice(unityInputDevice);
				Logger.LogInfo("Detached device: " + unityInputDevice.Profile.Name);
			}
		}
	}

	private void AutoDiscoverDeviceProfiles()
	{
		string[] profiles = UnityInputDeviceProfileList.Profiles;
		foreach (string typeName in profiles)
		{
			UnityInputDeviceProfile unityInputDeviceProfile = (UnityInputDeviceProfile)Activator.CreateInstance(Type.GetType(typeName));
			if (unityInputDeviceProfile.IsSupportedOnThisPlatform)
			{
				deviceProfiles.Add(unityInputDeviceProfile);
			}
		}
	}
}

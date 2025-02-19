using System;
using System.Collections.Generic;
using UnityEngine;
using XInputDotNetPure;

namespace InControl;

public class XInputDeviceManager : InputDeviceManager
{
	private bool[] deviceConnected = new bool[4];

	public XInputDeviceManager()
	{
		for (int i = 0; i < 4; i++)
		{
			devices.Add(new XInputDevice(i));
		}
		Update(0uL, 0f);
	}

	public override void Update(ulong updateTick, float deltaTime)
	{
		for (int i = 0; i < 4; i++)
		{
			XInputDevice xInputDevice = devices[i] as XInputDevice;
			if (!xInputDevice.IsConnected)
			{
				xInputDevice.Update(updateTick, deltaTime);
			}
			if (xInputDevice.IsConnected != deviceConnected[i])
			{
				if (xInputDevice.IsConnected)
				{
					InputManager.AttachDevice(xInputDevice);
				}
				else
				{
					InputManager.DetachDevice(xInputDevice);
				}
				deviceConnected[i] = xInputDevice.IsConnected;
			}
		}
	}

	public static bool CheckPlatformSupport(ICollection<string> errors)
	{
		if (Application.platform != RuntimePlatform.WindowsPlayer && Application.platform != RuntimePlatform.WindowsEditor)
		{
			return false;
		}
		try
		{
			GamePad.GetState(PlayerIndex.One);
		}
		catch (DllNotFoundException ex)
		{
			errors?.Add(ex.Message + ".dll could not be found or is missing a dependency.");
			return false;
		}
		return true;
	}

	public static void Enable()
	{
		List<string> list = new List<string>();
		if (CheckPlatformSupport(list))
		{
			InputManager.HideDevicesWithProfile(typeof(Xbox360WinProfile));
			InputManager.HideDevicesWithProfile(typeof(XboxOneWinProfile));
			InputManager.HideDevicesWithProfile(typeof(LogitechF710ModeXWinProfile));
			InputManager.HideDevicesWithProfile(typeof(LogitechF310ModeXWinProfile));
			InputManager.AddDeviceManager<XInputDeviceManager>();
			return;
		}
		foreach (string item in list)
		{
			Logger.LogError(item);
		}
	}
}

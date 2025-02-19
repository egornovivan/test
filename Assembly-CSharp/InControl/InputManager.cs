using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using UnityEngine;

namespace InControl;

public class InputManager
{
	public static readonly VersionInfo Version = VersionInfo.InControlVersion();

	private static List<InputDeviceManager> inputDeviceManagers = new List<InputDeviceManager>();

	private static InputDevice activeDevice = InputDevice.Null;

	private static List<InputDevice> devices = new List<InputDevice>();

	public static ReadOnlyCollection<InputDevice> Devices;

	public static bool InvertYAxis;

	private static bool enableXInput;

	private static bool isSetup;

	private static float initialTime;

	private static float currentTime;

	private static float lastUpdateTime;

	private static ulong currentTick;

	private static VersionInfo? unityVersion;

	public static string Platform { get; private set; }

	public static bool MenuWasPressed { get; private set; }

	private static InputDevice DefaultActiveDevice => (devices.Count <= 0) ? InputDevice.Null : devices[0];

	public static InputDevice ActiveDevice
	{
		get
		{
			return (activeDevice != null) ? activeDevice : InputDevice.Null;
		}
		private set
		{
			activeDevice = ((value != null) ? value : InputDevice.Null);
		}
	}

	public static bool EnableXInput
	{
		get
		{
			return enableXInput;
		}
		set
		{
			enableXInput = value;
		}
	}

	public static VersionInfo UnityVersion
	{
		get
		{
			if (!unityVersion.HasValue)
			{
				unityVersion = VersionInfo.UnityVersion();
			}
			return unityVersion.Value;
		}
	}

	public static event Action OnSetup;

	public static event Action<ulong, float> OnUpdate;

	public static event Action<InputDevice> OnDeviceAttached;

	public static event Action<InputDevice> OnDeviceDetached;

	public static event Action<InputDevice> OnActiveDeviceChanged;

	[Obsolete("Calling InputManager.Setup() manually is deprecated. Use the InControlManager component instead.")]
	public static void Setup()
	{
		SetupInternal();
	}

	internal static void SetupInternal()
	{
		if (!isSetup)
		{
			Platform = (SystemInfo.operatingSystem + " " + SystemInfo.deviceModel).ToUpper();
			initialTime = 0f;
			currentTime = 0f;
			lastUpdateTime = 0f;
			currentTick = 0uL;
			inputDeviceManagers.Clear();
			devices.Clear();
			Devices = new ReadOnlyCollection<InputDevice>(devices);
			activeDevice = InputDevice.Null;
			isSetup = true;
			if (enableXInput)
			{
				XInputDeviceManager.Enable();
			}
			if (InputManager.OnSetup != null)
			{
				InputManager.OnSetup();
				InputManager.OnSetup = null;
			}
			if (true)
			{
				AddDeviceManager<UnityInputDeviceManager>();
			}
		}
	}

	[Obsolete("Calling InputManager.Reset() manually is deprecated. Use the InControlManager component instead.")]
	public static void Reset()
	{
		ResetInternal();
	}

	internal static void ResetInternal()
	{
		InputManager.OnSetup = null;
		InputManager.OnUpdate = null;
		InputManager.OnActiveDeviceChanged = null;
		InputManager.OnDeviceAttached = null;
		InputManager.OnDeviceDetached = null;
		inputDeviceManagers.Clear();
		devices.Clear();
		activeDevice = InputDevice.Null;
		isSetup = false;
	}

	private static void AssertIsSetup()
	{
		if (!isSetup)
		{
			throw new Exception("InputManager is not initialized. Call InputManager.Setup() first.");
		}
	}

	[Obsolete("Calling InputManager.Update() manually is deprecated. Use the InControlManager component instead.")]
	public static void Update()
	{
		UpdateInternal();
	}

	internal static void UpdateInternal()
	{
		AssertIsSetup();
		if (InputManager.OnSetup != null)
		{
			InputManager.OnSetup();
			InputManager.OnSetup = null;
		}
		currentTick++;
		UpdateCurrentTime();
		float deltaTime = currentTime - lastUpdateTime;
		UpdateDeviceManagers(deltaTime);
		PreUpdateDevices(deltaTime);
		UpdateDevices(deltaTime);
		PostUpdateDevices(deltaTime);
		UpdateActiveDevice();
		lastUpdateTime = currentTime;
	}

	internal static void OnApplicationFocus(bool focusState)
	{
		if (focusState)
		{
			return;
		}
		int count = devices.Count;
		for (int i = 0; i < count; i++)
		{
			InputControl[] controls = devices[i].Controls;
			int num = controls.Length;
			for (int j = 0; j < num; j++)
			{
				controls[j]?.SetZeroTick();
			}
		}
	}

	internal static void OnApplicationPause(bool pauseState)
	{
	}

	internal static void OnApplicationQuit()
	{
	}

	private static void UpdateActiveDevice()
	{
		InputDevice inputDevice = ActiveDevice;
		int count = devices.Count;
		for (int i = 0; i < count; i++)
		{
			InputDevice inputDevice2 = devices[i];
			if (ActiveDevice == InputDevice.Null || inputDevice2.LastChangedAfter(ActiveDevice))
			{
				ActiveDevice = inputDevice2;
			}
		}
		if (inputDevice != ActiveDevice && InputManager.OnActiveDeviceChanged != null)
		{
			InputManager.OnActiveDeviceChanged(ActiveDevice);
		}
	}

	public static void AddDeviceManager(InputDeviceManager inputDeviceManager)
	{
		AssertIsSetup();
		inputDeviceManagers.Add(inputDeviceManager);
		inputDeviceManager.Update(currentTick, currentTime - lastUpdateTime);
	}

	public static void AddDeviceManager<T>() where T : InputDeviceManager, new()
	{
		if (!HasDeviceManager<T>())
		{
			AddDeviceManager(new T());
		}
	}

	public static bool HasDeviceManager<T>() where T : InputDeviceManager
	{
		int count = inputDeviceManagers.Count;
		for (int i = 0; i < count; i++)
		{
			if (inputDeviceManagers[i] is T)
			{
				return true;
			}
		}
		return false;
	}

	private static void UpdateCurrentTime()
	{
		if (initialTime < float.Epsilon)
		{
			initialTime = Time.realtimeSinceStartup;
		}
		currentTime = Mathf.Max(0f, Time.realtimeSinceStartup - initialTime);
	}

	private static void UpdateDeviceManagers(float deltaTime)
	{
		int count = inputDeviceManagers.Count;
		for (int i = 0; i < count; i++)
		{
			InputDeviceManager inputDeviceManager = inputDeviceManagers[i];
			inputDeviceManager.Update(currentTick, deltaTime);
		}
	}

	private static void PreUpdateDevices(float deltaTime)
	{
		MenuWasPressed = false;
		int count = devices.Count;
		for (int i = 0; i < count; i++)
		{
			InputDevice inputDevice = devices[i];
			inputDevice.PreUpdate(currentTick, deltaTime);
		}
	}

	private static void UpdateDevices(float deltaTime)
	{
		int count = devices.Count;
		for (int i = 0; i < count; i++)
		{
			InputDevice inputDevice = devices[i];
			inputDevice.Update(currentTick, deltaTime);
		}
		if (InputManager.OnUpdate != null)
		{
			InputManager.OnUpdate(currentTick, deltaTime);
		}
	}

	private static void PostUpdateDevices(float deltaTime)
	{
		int count = devices.Count;
		for (int i = 0; i < count; i++)
		{
			InputDevice inputDevice = devices[i];
			inputDevice.PostUpdate(currentTick, deltaTime);
			if (inputDevice.MenuWasPressed)
			{
				MenuWasPressed = true;
			}
		}
	}

	public static void AttachDevice(InputDevice inputDevice)
	{
		AssertIsSetup();
		if (inputDevice.IsSupportedOnThisPlatform)
		{
			devices.Add(inputDevice);
			devices.Sort((InputDevice d1, InputDevice d2) => d1.SortOrder.CompareTo(d2.SortOrder));
			if (InputManager.OnDeviceAttached != null)
			{
				InputManager.OnDeviceAttached(inputDevice);
			}
			if (ActiveDevice == InputDevice.Null)
			{
				ActiveDevice = inputDevice;
			}
		}
	}

	public static void DetachDevice(InputDevice inputDevice)
	{
		AssertIsSetup();
		devices.Remove(inputDevice);
		devices.Sort((InputDevice d1, InputDevice d2) => d1.SortOrder.CompareTo(d2.SortOrder));
		if (ActiveDevice == inputDevice)
		{
			ActiveDevice = InputDevice.Null;
		}
		if (InputManager.OnDeviceDetached != null)
		{
			InputManager.OnDeviceDetached(inputDevice);
		}
	}

	public static void HideDevicesWithProfile(Type type)
	{
		if (type.IsSubclassOf(typeof(UnityInputDeviceProfile)))
		{
			UnityInputDeviceProfile.Hide(type);
		}
	}
}

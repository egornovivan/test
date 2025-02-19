using UnityEngine;

namespace InControl;

public class UnityInputDevice : InputDevice
{
	public const int MaxDevices = 10;

	public const int MaxButtons = 20;

	public const int MaxAnalogs = 20;

	internal int JoystickId { get; private set; }

	public UnityInputDeviceProfile Profile { get; protected set; }

	public override bool IsSupportedOnThisPlatform => Profile.IsSupportedOnThisPlatform;

	public override bool IsKnown => Profile.IsKnown;

	public UnityInputDevice(UnityInputDeviceProfile profile, int joystickId)
		: base(profile.Name)
	{
		Initialize(profile, joystickId);
	}

	public UnityInputDevice(UnityInputDeviceProfile profile)
		: base(profile.Name)
	{
		Initialize(profile, 0);
	}

	private void Initialize(UnityInputDeviceProfile profile, int joystickId)
	{
		Profile = profile;
		base.Meta = Profile.Meta;
		int analogCount = Profile.AnalogCount;
		for (int i = 0; i < analogCount; i++)
		{
			InputControlMapping inputControlMapping = Profile.AnalogMappings[i];
			InputControl inputControl = AddControl(inputControlMapping.Target, inputControlMapping.Handle);
			inputControl.Sensitivity = Profile.Sensitivity;
			inputControl.UpperDeadZone = Profile.UpperDeadZone;
			inputControl.LowerDeadZone = Profile.LowerDeadZone;
		}
		int buttonCount = Profile.ButtonCount;
		for (int j = 0; j < buttonCount; j++)
		{
			InputControlMapping inputControlMapping2 = Profile.ButtonMappings[j];
			AddControl(inputControlMapping2.Target, inputControlMapping2.Handle);
		}
		JoystickId = joystickId;
		if (joystickId != 0)
		{
			SortOrder = 100 + joystickId;
			string meta = base.Meta;
			base.Meta = meta + " [id: " + joystickId + "]";
		}
	}

	public override void Update(ulong updateTick, float deltaTime)
	{
		if (Profile == null)
		{
			return;
		}
		int analogCount = Profile.AnalogCount;
		for (int i = 0; i < analogCount; i++)
		{
			InputControlMapping inputControlMapping = Profile.AnalogMappings[i];
			InputControl control = GetControl(inputControlMapping.Target);
			float value = inputControlMapping.Source.GetValue(this);
			if (inputControlMapping.IgnoreInitialZeroValue && control.IsOnZeroTick && Mathf.Abs(value) < Mathf.Epsilon)
			{
				control.RawValue = null;
				control.PreValue = null;
				continue;
			}
			float value2 = inputControlMapping.MapValue(value);
			if (inputControlMapping.Raw)
			{
				control.RawValue = Combine(control.RawValue, value2);
			}
			else
			{
				control.PreValue = Combine(control.PreValue, value2);
			}
		}
		int buttonCount = Profile.ButtonCount;
		for (int j = 0; j < buttonCount; j++)
		{
			InputControlMapping inputControlMapping2 = Profile.ButtonMappings[j];
			bool state = inputControlMapping2.Source.GetState(this);
			UpdateWithState(inputControlMapping2.Target, state, updateTick);
		}
	}

	private float Combine(float? value1, float value2)
	{
		if (value1.HasValue)
		{
			return (!(Mathf.Abs(value1.Value) > Mathf.Abs(value2))) ? value2 : value1.Value;
		}
		return value2;
	}

	public bool IsConfiguredWith(UnityInputDeviceProfile deviceProfile, int joystickId)
	{
		return Profile == deviceProfile && JoystickId == joystickId;
	}
}

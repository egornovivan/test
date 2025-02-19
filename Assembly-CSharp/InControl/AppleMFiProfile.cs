namespace InControl;

[AutoDiscover]
public class AppleMFiProfile : UnityInputDeviceProfile
{
	public AppleMFiProfile()
	{
		base.Name = "Apple MFi Controller";
		base.Meta = "Apple MFi Controller on iOS";
		SupportedPlatforms = new string[1] { "iPhone" };
		LastResortRegex = string.Empty;
		base.LowerDeadZone = 0.05f;
		base.UpperDeadZone = 0.95f;
		base.ButtonMappings = new InputControlMapping[13]
		{
			new InputControlMapping
			{
				Handle = "A",
				Target = InputControlType.Action1,
				Source = UnityInputDeviceProfile.Button14
			},
			new InputControlMapping
			{
				Handle = "B",
				Target = InputControlType.Action2,
				Source = UnityInputDeviceProfile.Button13
			},
			new InputControlMapping
			{
				Handle = "X",
				Target = InputControlType.Action3,
				Source = UnityInputDeviceProfile.Button15
			},
			new InputControlMapping
			{
				Handle = "Y",
				Target = InputControlType.Action4,
				Source = UnityInputDeviceProfile.Button12
			},
			new InputControlMapping
			{
				Handle = "DPad Up",
				Target = InputControlType.DPadUp,
				Source = UnityInputDeviceProfile.Button4
			},
			new InputControlMapping
			{
				Handle = "DPad Down",
				Target = InputControlType.DPadDown,
				Source = UnityInputDeviceProfile.Button6
			},
			new InputControlMapping
			{
				Handle = "DPad Left",
				Target = InputControlType.DPadLeft,
				Source = UnityInputDeviceProfile.Button7
			},
			new InputControlMapping
			{
				Handle = "DPad Right",
				Target = InputControlType.DPadRight,
				Source = UnityInputDeviceProfile.Button5
			},
			new InputControlMapping
			{
				Handle = "Left Bumper",
				Target = InputControlType.LeftBumper,
				Source = UnityInputDeviceProfile.Button8
			},
			new InputControlMapping
			{
				Handle = "Right Bumper",
				Target = InputControlType.RightBumper,
				Source = UnityInputDeviceProfile.Button9
			},
			new InputControlMapping
			{
				Handle = "Pause",
				Target = InputControlType.Pause,
				Source = UnityInputDeviceProfile.Button0
			},
			new InputControlMapping
			{
				Handle = "Left Trigger",
				Target = InputControlType.LeftTrigger,
				Source = UnityInputDeviceProfile.Button10
			},
			new InputControlMapping
			{
				Handle = "Right Trigger",
				Target = InputControlType.RightTrigger,
				Source = UnityInputDeviceProfile.Button11
			}
		};
		base.AnalogMappings = new InputControlMapping[4]
		{
			new InputControlMapping
			{
				Handle = "Left Stick X",
				Target = InputControlType.LeftStickX,
				Source = UnityInputDeviceProfile.Analog0
			},
			new InputControlMapping
			{
				Handle = "Left Stick Y",
				Target = InputControlType.LeftStickY,
				Source = UnityInputDeviceProfile.Analog1,
				Invert = true
			},
			new InputControlMapping
			{
				Handle = "Right Stick X",
				Target = InputControlType.RightStickX,
				Source = UnityInputDeviceProfile.Analog2
			},
			new InputControlMapping
			{
				Handle = "Right Stick Y",
				Target = InputControlType.RightStickY,
				Source = UnityInputDeviceProfile.Analog3,
				Invert = true
			}
		};
	}
}

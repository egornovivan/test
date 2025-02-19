namespace InControl;

[AutoDiscover]
public class PlayStation3WinProfile : UnityInputDeviceProfile
{
	public PlayStation3WinProfile()
	{
		base.Name = "PlayStation 3 Controller";
		base.Meta = "PlayStation 3 Controller on Windows (via MotioninJoy Gamepad Tool)";
		SupportedPlatforms = new string[1] { "Windows" };
		JoystickNames = new string[1] { "MotioninJoy Virtual Game Controller" };
		base.ButtonMappings = new InputControlMapping[13]
		{
			new InputControlMapping
			{
				Handle = "Cross",
				Target = InputControlType.Action1,
				Source = UnityInputDeviceProfile.Button2
			},
			new InputControlMapping
			{
				Handle = "Circle",
				Target = InputControlType.Action2,
				Source = UnityInputDeviceProfile.Button1
			},
			new InputControlMapping
			{
				Handle = "Square",
				Target = InputControlType.Action3,
				Source = UnityInputDeviceProfile.Button3
			},
			new InputControlMapping
			{
				Handle = "Triangle",
				Target = InputControlType.Action4,
				Source = UnityInputDeviceProfile.Button0
			},
			new InputControlMapping
			{
				Handle = "Left Bumper",
				Target = InputControlType.LeftBumper,
				Source = UnityInputDeviceProfile.Button4
			},
			new InputControlMapping
			{
				Handle = "Right Bumper",
				Target = InputControlType.RightBumper,
				Source = UnityInputDeviceProfile.Button5
			},
			new InputControlMapping
			{
				Handle = "Left Trigger",
				Target = InputControlType.LeftTrigger,
				Source = UnityInputDeviceProfile.Button6
			},
			new InputControlMapping
			{
				Handle = "Right Trigger",
				Target = InputControlType.RightTrigger,
				Source = UnityInputDeviceProfile.Button7
			},
			new InputControlMapping
			{
				Handle = "Select",
				Target = InputControlType.Select,
				Source = UnityInputDeviceProfile.Button8
			},
			new InputControlMapping
			{
				Handle = "Left Stick Button",
				Target = InputControlType.LeftStickButton,
				Source = UnityInputDeviceProfile.Button9
			},
			new InputControlMapping
			{
				Handle = "Right Stick Button",
				Target = InputControlType.RightStickButton,
				Source = UnityInputDeviceProfile.Button10
			},
			new InputControlMapping
			{
				Handle = "Start",
				Target = InputControlType.Start,
				Source = UnityInputDeviceProfile.Button11
			},
			new InputControlMapping
			{
				Handle = "System",
				Target = InputControlType.System,
				Source = UnityInputDeviceProfile.Button12
			}
		};
		base.AnalogMappings = new InputControlMapping[10]
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
				Source = UnityInputDeviceProfile.Analog5,
				Invert = true
			},
			new InputControlMapping
			{
				Handle = "DPad Left",
				Target = InputControlType.DPadLeft,
				Source = UnityInputDeviceProfile.Analog8,
				SourceRange = InputControlMapping.Range.Negative,
				TargetRange = InputControlMapping.Range.Negative,
				Invert = true
			},
			new InputControlMapping
			{
				Handle = "DPad Right",
				Target = InputControlType.DPadRight,
				Source = UnityInputDeviceProfile.Analog8,
				SourceRange = InputControlMapping.Range.Positive,
				TargetRange = InputControlMapping.Range.Positive
			},
			new InputControlMapping
			{
				Handle = "DPad Up",
				Target = InputControlType.DPadUp,
				Source = UnityInputDeviceProfile.Analog9,
				SourceRange = InputControlMapping.Range.Positive,
				TargetRange = InputControlMapping.Range.Positive
			},
			new InputControlMapping
			{
				Handle = "DPad Down",
				Target = InputControlType.DPadDown,
				Source = UnityInputDeviceProfile.Analog9,
				SourceRange = InputControlMapping.Range.Negative,
				TargetRange = InputControlMapping.Range.Negative,
				Invert = true
			},
			new InputControlMapping
			{
				Handle = "Tilt X",
				Target = InputControlType.TiltX,
				Source = UnityInputDeviceProfile.Analog3
			},
			new InputControlMapping
			{
				Handle = "Tilt Y",
				Target = InputControlType.TiltY,
				Source = UnityInputDeviceProfile.Analog4
			}
		};
	}
}

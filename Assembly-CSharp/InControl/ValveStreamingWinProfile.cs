namespace InControl;

[AutoDiscover]
public class ValveStreamingWinProfile : UnityInputDeviceProfile
{
	public ValveStreamingWinProfile()
	{
		base.Name = "Valve Streaming Gamepad";
		base.Meta = "Valve Streaming Gamepad on Windows";
		SupportedPlatforms = new string[1] { "Windows" };
		JoystickNames = new string[1] { "Controller (Valve Streaming Gamepad)" };
		base.ButtonMappings = new InputControlMapping[10]
		{
			new InputControlMapping
			{
				Handle = "A",
				Target = InputControlType.Action1,
				Source = UnityInputDeviceProfile.Button0
			},
			new InputControlMapping
			{
				Handle = "B",
				Target = InputControlType.Action2,
				Source = UnityInputDeviceProfile.Button1
			},
			new InputControlMapping
			{
				Handle = "X",
				Target = InputControlType.Action3,
				Source = UnityInputDeviceProfile.Button2
			},
			new InputControlMapping
			{
				Handle = "Y",
				Target = InputControlType.Action4,
				Source = UnityInputDeviceProfile.Button3
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
				Handle = "Left Stick Button",
				Target = InputControlType.LeftStickButton,
				Source = UnityInputDeviceProfile.Button8
			},
			new InputControlMapping
			{
				Handle = "Right Stick Button",
				Target = InputControlType.RightStickButton,
				Source = UnityInputDeviceProfile.Button9
			},
			new InputControlMapping
			{
				Handle = "Back",
				Target = InputControlType.Select,
				Source = UnityInputDeviceProfile.Button6
			},
			new InputControlMapping
			{
				Handle = "Start",
				Target = InputControlType.Start,
				Source = UnityInputDeviceProfile.Button7
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
				Source = UnityInputDeviceProfile.Analog3
			},
			new InputControlMapping
			{
				Handle = "Right Stick Y",
				Target = InputControlType.RightStickY,
				Source = UnityInputDeviceProfile.Analog4,
				Invert = true
			},
			new InputControlMapping
			{
				Handle = "DPad Left",
				Target = InputControlType.DPadLeft,
				Source = UnityInputDeviceProfile.Analog5,
				SourceRange = InputControlMapping.Range.Negative,
				TargetRange = InputControlMapping.Range.Negative,
				Invert = true
			},
			new InputControlMapping
			{
				Handle = "DPad Right",
				Target = InputControlType.DPadRight,
				Source = UnityInputDeviceProfile.Analog5,
				SourceRange = InputControlMapping.Range.Positive,
				TargetRange = InputControlMapping.Range.Positive
			},
			new InputControlMapping
			{
				Handle = "DPad Up",
				Target = InputControlType.DPadUp,
				Source = UnityInputDeviceProfile.Analog6,
				SourceRange = InputControlMapping.Range.Positive,
				TargetRange = InputControlMapping.Range.Positive
			},
			new InputControlMapping
			{
				Handle = "DPad Down",
				Target = InputControlType.DPadDown,
				Source = UnityInputDeviceProfile.Analog6,
				SourceRange = InputControlMapping.Range.Negative,
				TargetRange = InputControlMapping.Range.Negative,
				Invert = true
			},
			new InputControlMapping
			{
				Handle = "Left Trigger",
				Target = InputControlType.LeftTrigger,
				Source = UnityInputDeviceProfile.Analog2,
				SourceRange = InputControlMapping.Range.Positive,
				TargetRange = InputControlMapping.Range.Positive
			},
			new InputControlMapping
			{
				Handle = "Right Trigger",
				Target = InputControlType.RightTrigger,
				Source = UnityInputDeviceProfile.Analog2,
				SourceRange = InputControlMapping.Range.Negative,
				TargetRange = InputControlMapping.Range.Negative,
				Invert = true
			}
		};
	}
}

namespace InControl;

[AutoDiscover]
public class AndroidTVRemoteProfile : UnityInputDeviceProfile
{
	public AndroidTVRemoteProfile()
	{
		base.Name = "Android TV Remote";
		base.Meta = "Android TV Remotet on Android TV";
		SupportedPlatforms = new string[1] { "Android" };
		JoystickNames = new string[2] { "touch-input", "navigation-input" };
		base.ButtonMappings = new InputControlMapping[1]
		{
			new InputControlMapping
			{
				Handle = "A",
				Target = InputControlType.Action1,
				Source = UnityInputDeviceProfile.Button0
			}
		};
		base.AnalogMappings = new InputControlMapping[4]
		{
			new InputControlMapping
			{
				Handle = "DPad Left",
				Target = InputControlType.DPadLeft,
				Source = UnityInputDeviceProfile.Analog4,
				SourceRange = InputControlMapping.Range.Negative,
				TargetRange = InputControlMapping.Range.Negative,
				Invert = true
			},
			new InputControlMapping
			{
				Handle = "DPad Right",
				Target = InputControlType.DPadRight,
				Source = UnityInputDeviceProfile.Analog4,
				SourceRange = InputControlMapping.Range.Positive,
				TargetRange = InputControlMapping.Range.Positive
			},
			new InputControlMapping
			{
				Handle = "DPad Up",
				Target = InputControlType.DPadUp,
				Source = UnityInputDeviceProfile.Analog5,
				SourceRange = InputControlMapping.Range.Negative,
				TargetRange = InputControlMapping.Range.Negative
			},
			new InputControlMapping
			{
				Handle = "DPad Down",
				Target = InputControlType.DPadDown,
				Source = UnityInputDeviceProfile.Analog5,
				SourceRange = InputControlMapping.Range.Positive,
				TargetRange = InputControlMapping.Range.Positive,
				Invert = true
			}
		};
	}
}

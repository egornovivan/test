using UnityEngine;

namespace InControl;

[AutoDiscover]
public class AmazonFireTVRemote : UnityInputDeviceProfile
{
	public AmazonFireTVRemote()
	{
		base.Name = "Amazon Fire TV Remote";
		base.Meta = "Amazon Fire TV Remote on Amazon Fire TV";
		SupportedPlatforms = new string[2] { "Amazon AFTB", "Amazon AFTM" };
		JoystickNames = new string[2]
		{
			string.Empty,
			"Amazon Fire TV Remote"
		};
		base.ButtonMappings = new InputControlMapping[2]
		{
			new InputControlMapping
			{
				Handle = "A",
				Target = InputControlType.Action1,
				Source = UnityInputDeviceProfile.Button0
			},
			new InputControlMapping
			{
				Handle = "Back",
				Target = InputControlType.Select,
				Source = UnityInputDeviceProfile.KeyCodeButton(KeyCode.Escape)
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
				TargetRange = InputControlMapping.Range.Negative,
				Invert = true
			},
			new InputControlMapping
			{
				Handle = "DPad Down",
				Target = InputControlType.DPadDown,
				Source = UnityInputDeviceProfile.Analog5,
				SourceRange = InputControlMapping.Range.Positive,
				TargetRange = InputControlMapping.Range.Positive
			}
		};
	}
}

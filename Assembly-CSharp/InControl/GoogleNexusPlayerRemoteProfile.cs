using UnityEngine;

namespace InControl;

[AutoDiscover]
public class GoogleNexusPlayerRemoteProfile : UnityInputDeviceProfile
{
	public GoogleNexusPlayerRemoteProfile()
	{
		base.Name = "Google Nexus Player Remote";
		base.Meta = "Google Nexus Player Remote";
		SupportedPlatforms = new string[1] { "Android" };
		JoystickNames = new string[1] { "Google Nexus Remote" };
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
				Target = InputControlType.Back,
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

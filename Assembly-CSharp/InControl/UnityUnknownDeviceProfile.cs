namespace InControl;

public class UnityUnknownDeviceProfile : UnityInputDeviceProfile
{
	public override bool IsKnown => false;

	public UnityUnknownDeviceProfile(string joystickName)
	{
		base.Name = "Unknown Device";
		if (joystickName != string.Empty)
		{
			base.Name = base.Name + " (" + joystickName + ")";
		}
		base.Meta = string.Empty;
		base.Sensitivity = 1f;
		base.LowerDeadZone = 0.2f;
		SupportedPlatforms = null;
		JoystickNames = new string[1] { joystickName };
		base.AnalogMappings = new InputControlMapping[20];
		for (int i = 0; i < 20; i++)
		{
			base.AnalogMappings[i] = new InputControlMapping
			{
				Handle = "Analog " + i,
				Source = UnityInputDeviceProfile.Analog(i),
				Target = (InputControlType)(35 + i)
			};
		}
		base.ButtonMappings = new InputControlMapping[20];
		for (int j = 0; j < 20; j++)
		{
			base.ButtonMappings[j] = new InputControlMapping
			{
				Handle = "Button " + j,
				Source = UnityInputDeviceProfile.Button(j),
				Target = (InputControlType)(55 + j)
			};
		}
	}
}

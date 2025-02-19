using UnityEngine;

namespace InControl;

public class UnityButtonSource : InputControlSource
{
	private int buttonId;

	private static string[,] buttonQueries;

	public UnityButtonSource(int buttonId)
	{
		this.buttonId = buttonId;
		SetupButtonQueries();
	}

	public float GetValue(InputDevice inputDevice)
	{
		return (!GetState(inputDevice)) ? 0f : 1f;
	}

	public bool GetState(InputDevice inputDevice)
	{
		int joystickId = (inputDevice as UnityInputDevice).JoystickId;
		string buttonKey = GetButtonKey(joystickId, buttonId);
		return Input.GetKey(buttonKey);
	}

	private static void SetupButtonQueries()
	{
		if (buttonQueries != null)
		{
			return;
		}
		buttonQueries = new string[10, 20];
		for (int i = 1; i <= 10; i++)
		{
			for (int j = 0; j < 20; j++)
			{
				buttonQueries[i - 1, j] = "joystick " + i + " button " + j;
			}
		}
	}

	private static string GetButtonKey(int joystickId, int buttonId)
	{
		return buttonQueries[joystickId - 1, buttonId];
	}
}

using UnityEngine;

namespace InControl;

public class UnityAnalogSource : InputControlSource
{
	private int analogId;

	private static string[,] analogQueries;

	public UnityAnalogSource(int analogId)
	{
		this.analogId = analogId;
		SetupAnalogQueries();
	}

	public float GetValue(InputDevice inputDevice)
	{
		int joystickId = (inputDevice as UnityInputDevice).JoystickId;
		string analogKey = GetAnalogKey(joystickId, analogId);
		return Input.GetAxisRaw(analogKey);
	}

	public bool GetState(InputDevice inputDevice)
	{
		return !Mathf.Approximately(GetValue(inputDevice), 0f);
	}

	private static void SetupAnalogQueries()
	{
		if (analogQueries != null)
		{
			return;
		}
		analogQueries = new string[10, 20];
		for (int i = 1; i <= 10; i++)
		{
			for (int j = 0; j < 20; j++)
			{
				analogQueries[i - 1, j] = "joystick " + i + " analog " + j;
			}
		}
	}

	private static string GetAnalogKey(int joystickId, int analogId)
	{
		return analogQueries[joystickId - 1, analogId];
	}
}

using UnityEngine;

namespace InControl;

public class UnityKeyCodeAxisSource : InputControlSource
{
	private KeyCode negativeKeyCode;

	private KeyCode positiveKeyCode;

	public UnityKeyCodeAxisSource(KeyCode negativeKeyCode, KeyCode positiveKeyCode)
	{
		this.negativeKeyCode = negativeKeyCode;
		this.positiveKeyCode = positiveKeyCode;
	}

	public float GetValue(InputDevice inputDevice)
	{
		int num = 0;
		if (Input.GetKey(negativeKeyCode))
		{
			num--;
		}
		if (Input.GetKey(positiveKeyCode))
		{
			num++;
		}
		return num;
	}

	public bool GetState(InputDevice inputDevice)
	{
		return !Mathf.Approximately(GetValue(inputDevice), 0f);
	}
}

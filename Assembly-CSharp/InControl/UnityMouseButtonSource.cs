using UnityEngine;

namespace InControl;

public class UnityMouseButtonSource : InputControlSource
{
	private int buttonId;

	public UnityMouseButtonSource(int buttonId)
	{
		this.buttonId = buttonId;
	}

	public float GetValue(InputDevice inputDevice)
	{
		return (!GetState(inputDevice)) ? 0f : 1f;
	}

	public bool GetState(InputDevice inputDevice)
	{
		return Input.GetMouseButton(buttonId);
	}
}

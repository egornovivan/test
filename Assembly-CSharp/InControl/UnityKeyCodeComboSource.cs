using UnityEngine;

namespace InControl;

public class UnityKeyCodeComboSource : InputControlSource
{
	private KeyCode[] keyCodeList;

	public UnityKeyCodeComboSource(params KeyCode[] keyCodeList)
	{
		this.keyCodeList = keyCodeList;
	}

	public float GetValue(InputDevice inputDevice)
	{
		return (!GetState(inputDevice)) ? 0f : 1f;
	}

	public bool GetState(InputDevice inputDevice)
	{
		for (int i = 0; i < keyCodeList.Length; i++)
		{
			if (!Input.GetKey(keyCodeList[i]))
			{
				return false;
			}
		}
		return true;
	}
}

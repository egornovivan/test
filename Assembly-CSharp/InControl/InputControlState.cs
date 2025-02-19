using UnityEngine;

namespace InControl;

public struct InputControlState
{
	public bool State;

	public float Value;

	public void Reset()
	{
		Value = 0f;
		State = false;
	}

	public void Set(float value)
	{
		Value = value;
		State = !Mathf.Approximately(value, 0f);
	}

	public void Set(float value, float threshold)
	{
		Value = value;
		State = Mathf.Abs(value) > threshold;
	}

	public void Set(bool state)
	{
		State = state;
		Value = ((!state) ? 0f : 1f);
	}

	public static implicit operator bool(InputControlState state)
	{
		return state.State;
	}

	public static implicit operator float(InputControlState state)
	{
		return state.Value;
	}

	public static bool operator ==(InputControlState a, InputControlState b)
	{
		return Mathf.Approximately(a.Value, b.Value);
	}

	public static bool operator !=(InputControlState a, InputControlState b)
	{
		return !Mathf.Approximately(a.Value, b.Value);
	}
}

using System;

namespace InControl;

public class OneAxisInputControl
{
	private InputControlState thisState;

	private InputControlState lastState;

	public ulong UpdateTick { get; private set; }

	public bool State => thisState.State;

	public bool LastState => lastState.State;

	public float Value => thisState.Value;

	public float LastValue => lastState.Value;

	public bool HasChanged => thisState != lastState;

	public bool IsPressed => thisState.State;

	public bool WasPressed => (bool)thisState && !lastState;

	public bool WasReleased => !thisState && (bool)lastState;

	public void UpdateWithValue(float value, ulong updateTick, float stateThreshold)
	{
		if (UpdateTick > updateTick)
		{
			throw new InvalidOperationException("A control cannot be updated with an earlier tick.");
		}
		lastState = thisState;
		thisState.Set(value, stateThreshold);
		if (thisState != lastState)
		{
			UpdateTick = updateTick;
		}
	}

	public static implicit operator bool(OneAxisInputControl control)
	{
		return control.State;
	}

	public static implicit operator float(OneAxisInputControl control)
	{
		return control.Value;
	}
}

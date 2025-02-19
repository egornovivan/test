using System;
using UnityEngine;

namespace InControl;

public class InputControl
{
	public static readonly InputControl Null = new InputControl("NullInputControl");

	public float Sensitivity = 1f;

	public float LowerDeadZone;

	public float UpperDeadZone = 1f;

	private InputControlState thisState;

	private InputControlState lastState;

	private InputControlState tempState;

	private ulong zeroTick;

	internal float? RawValue;

	internal float? PreValue;

	public string Handle { get; protected set; }

	public InputControlType Target { get; protected set; }

	public ulong UpdateTick { get; protected set; }

	public bool IsButton { get; protected set; }

	internal bool IsOnZeroTick => UpdateTick == zeroTick;

	public bool State => thisState.State;

	public bool LastState => lastState.State;

	public float Value => thisState.Value;

	public float LastValue => lastState.Value;

	public bool HasChanged => thisState != lastState;

	public bool IsPressed => thisState.State;

	public bool WasPressed => (bool)thisState && !lastState;

	public bool WasReleased => !thisState && (bool)lastState;

	public bool IsNull => this == Null;

	public bool IsNotNull => this != Null;

	public InputControlType? Obverse => Target switch
	{
		InputControlType.LeftStickX => InputControlType.LeftStickY, 
		InputControlType.LeftStickY => InputControlType.LeftStickX, 
		InputControlType.RightStickX => InputControlType.RightStickY, 
		InputControlType.RightStickY => InputControlType.RightStickX, 
		_ => null, 
	};

	private InputControl(string handle)
	{
		Handle = handle;
	}

	public InputControl(string handle, InputControlType target)
	{
		Handle = handle;
		Target = target;
		IsButton = (target >= InputControlType.Action1 && target <= InputControlType.Action4) || (target >= InputControlType.Button0 && target <= InputControlType.Button19);
	}

	public void UpdateWithState(bool state, ulong updateTick)
	{
		if (IsNull)
		{
			throw new InvalidOperationException("A null control cannot be updated.");
		}
		if (UpdateTick > updateTick)
		{
			throw new InvalidOperationException("A control cannot be updated with an earlier tick.");
		}
		tempState.Set(state || tempState.State);
	}

	public void UpdateWithValue(float value, ulong updateTick)
	{
		if (IsNull)
		{
			throw new InvalidOperationException("A null control cannot be updated.");
		}
		if (UpdateTick > updateTick)
		{
			throw new InvalidOperationException("A control cannot be updated with an earlier tick.");
		}
		if (Mathf.Abs(value) > Mathf.Abs(tempState.Value))
		{
			tempState.Set(value);
		}
	}

	internal void PreUpdate(ulong updateTick)
	{
		RawValue = null;
		PreValue = null;
		lastState = thisState;
		tempState.Reset();
	}

	internal void PostUpdate(ulong updateTick)
	{
		thisState = tempState;
		if (thisState != lastState)
		{
			UpdateTick = updateTick;
		}
	}

	internal void SetZeroTick()
	{
		zeroTick = UpdateTick;
	}

	public override string ToString()
	{
		return $"[InputControl: Handle={Handle}, Value={Value}]";
	}

	public static implicit operator bool(InputControl control)
	{
		return control.State;
	}

	public static implicit operator float(InputControl control)
	{
		return control.Value;
	}
}

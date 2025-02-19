using System;
using UnityEngine;

namespace InControl;

public class InputDevice
{
	public static readonly InputDevice Null = new InputDevice("NullInputDevice");

	internal int SortOrder = int.MaxValue;

	public string Name { get; protected set; }

	public string Meta { get; protected set; }

	public ulong LastChangeTick { get; protected set; }

	public InputControl[] Controls { get; protected set; }

	public TwoAxisInputControl LeftStick { get; protected set; }

	public TwoAxisInputControl RightStick { get; protected set; }

	public TwoAxisInputControl DPad { get; protected set; }

	private Vector2 DPadVector
	{
		get
		{
			float x = ((!DPadLeft.State) ? DPadRight.Value : (0f - DPadLeft.Value));
			float num = ((!DPadUp.State) ? (0f - DPadDown.Value) : DPadUp.Value);
			float y = ((!InputManager.InvertYAxis) ? num : (0f - num));
			return new Vector2(x, y).normalized;
		}
	}

	public virtual bool IsSupportedOnThisPlatform => true;

	public virtual bool IsKnown => true;

	public bool MenuWasPressed => GetControl(InputControlType.Back).WasPressed || GetControl(InputControlType.Start).WasPressed || GetControl(InputControlType.Select).WasPressed || GetControl(InputControlType.System).WasPressed || GetControl(InputControlType.Pause).WasPressed || GetControl(InputControlType.Menu).WasPressed;

	public InputControl AnyButton
	{
		get
		{
			int length = Controls.GetLength(0);
			for (int i = 0; i < length; i++)
			{
				InputControl inputControl = Controls[i];
				if (inputControl != null && inputControl.IsButton && inputControl.IsPressed)
				{
					return inputControl;
				}
			}
			return InputControl.Null;
		}
	}

	public InputControl LeftStickX => GetControl(InputControlType.LeftStickX);

	public InputControl LeftStickY => GetControl(InputControlType.LeftStickY);

	public InputControl RightStickX => GetControl(InputControlType.RightStickX);

	public InputControl RightStickY => GetControl(InputControlType.RightStickY);

	public InputControl DPadUp => GetControl(InputControlType.DPadUp);

	public InputControl DPadDown => GetControl(InputControlType.DPadDown);

	public InputControl DPadLeft => GetControl(InputControlType.DPadLeft);

	public InputControl DPadRight => GetControl(InputControlType.DPadRight);

	public InputControl Action1 => GetControl(InputControlType.Action1);

	public InputControl Action2 => GetControl(InputControlType.Action2);

	public InputControl Action3 => GetControl(InputControlType.Action3);

	public InputControl Action4 => GetControl(InputControlType.Action4);

	public InputControl LeftTrigger => GetControl(InputControlType.LeftTrigger);

	public InputControl RightTrigger => GetControl(InputControlType.RightTrigger);

	public InputControl LeftBumper => GetControl(InputControlType.LeftBumper);

	public InputControl RightBumper => GetControl(InputControlType.RightBumper);

	public InputControl LeftStickButton => GetControl(InputControlType.LeftStickButton);

	public InputControl RightStickButton => GetControl(InputControlType.RightStickButton);

	public float DPadX => DPad.X;

	public float DPadY => DPad.Y;

	public TwoAxisInputControl Direction => (DPad.UpdateTick <= LeftStick.UpdateTick) ? LeftStick : DPad;

	public InputDevice(string name)
	{
		Name = name;
		Meta = string.Empty;
		LastChangeTick = 0uL;
		Controls = new InputControl[76];
		LeftStick = new TwoAxisInputControl();
		RightStick = new TwoAxisInputControl();
		DPad = new TwoAxisInputControl();
	}

	public InputControl GetControl(InputControlType inputControlType)
	{
		InputControl inputControl = Controls[(int)inputControlType];
		return inputControl ?? InputControl.Null;
	}

	public static InputControlType GetInputControlTypeByName(string inputControlName)
	{
		return (InputControlType)(int)Enum.Parse(typeof(InputControlType), inputControlName);
	}

	public InputControl GetControlByName(string inputControlName)
	{
		InputControlType inputControlTypeByName = GetInputControlTypeByName(inputControlName);
		return GetControl(inputControlTypeByName);
	}

	public InputControl AddControl(InputControlType inputControlType, string handle)
	{
		InputControl inputControl = new InputControl(handle, inputControlType);
		Controls[(int)inputControlType] = inputControl;
		return inputControl;
	}

	public void UpdateWithState(InputControlType inputControlType, bool state, ulong updateTick)
	{
		GetControl(inputControlType).UpdateWithState(state, updateTick);
	}

	public void UpdateWithValue(InputControlType inputControlType, float value, ulong updateTick)
	{
		GetControl(inputControlType).UpdateWithValue(value, updateTick);
	}

	public void PreUpdate(ulong updateTick, float deltaTime)
	{
		int length = Controls.GetLength(0);
		for (int i = 0; i < length; i++)
		{
			Controls[i]?.PreUpdate(updateTick);
		}
	}

	public virtual void Update(ulong updateTick, float deltaTime)
	{
	}

	public void PostUpdate(ulong updateTick, float deltaTime)
	{
		int length = Controls.GetLength(0);
		for (int i = 0; i < length; i++)
		{
			InputControl inputControl = Controls[i];
			if (inputControl != null)
			{
				if (inputControl.RawValue.HasValue)
				{
					inputControl.UpdateWithValue(inputControl.RawValue.Value, updateTick);
				}
				else if (inputControl.PreValue.HasValue)
				{
					inputControl.UpdateWithValue(ProcessAnalogControlValue(inputControl, deltaTime), updateTick);
				}
				inputControl.PostUpdate(updateTick);
				if (inputControl.HasChanged)
				{
					LastChangeTick = updateTick;
				}
			}
		}
		LeftStick.Update(LeftStickX, LeftStickY, updateTick);
		RightStick.Update(RightStickX, RightStickY, updateTick);
		Vector2 dPadVector = DPadVector;
		DPad.Update(dPadVector.x, dPadVector.y, updateTick);
	}

	private float ProcessAnalogControlValue(InputControl control, float deltaTime)
	{
		float value = control.PreValue.Value;
		InputControlType? obverse = control.Obverse;
		if (obverse.HasValue)
		{
			InputControl control2 = GetControl(obverse.Value);
			value = ((!control2.PreValue.HasValue) ? ApplyDeadZone(value, control.LowerDeadZone, control.UpperDeadZone) : ApplyCircularDeadZone(value, control2.PreValue.Value, control.LowerDeadZone, control.UpperDeadZone));
		}
		else
		{
			value = ApplyDeadZone(value, control.LowerDeadZone, control.UpperDeadZone);
		}
		return ApplySmoothing(value, control.LastValue, deltaTime, control.Sensitivity);
	}

	private float ApplyDeadZone(float value, float lowerDeadZone, float upperDeadZone)
	{
		return Mathf.InverseLerp(lowerDeadZone, upperDeadZone, Mathf.Abs(value)) * Mathf.Sign(value);
	}

	private float ApplyCircularDeadZone(float axisValue1, float axisValue2, float lowerDeadZone, float upperDeadZone)
	{
		Vector2 vector = new Vector2(axisValue1, axisValue2);
		float num = Mathf.InverseLerp(lowerDeadZone, upperDeadZone, vector.magnitude);
		return (vector.normalized * num).x;
	}

	private float ApplySmoothing(float thisValue, float lastValue, float deltaTime, float sensitivity)
	{
		if (Mathf.Approximately(sensitivity, 1f))
		{
			return thisValue;
		}
		float maxDelta = deltaTime * sensitivity * 100f;
		if (Mathf.Sign(lastValue) != Mathf.Sign(thisValue))
		{
			lastValue = 0f;
		}
		return Mathf.MoveTowards(lastValue, thisValue, maxDelta);
	}

	public bool LastChangedAfter(InputDevice otherDevice)
	{
		return LastChangeTick > otherDevice.LastChangeTick;
	}

	public virtual void Vibrate(float leftMotor, float rightMotor)
	{
	}

	public void Vibrate(float intensity)
	{
		Vibrate(intensity, intensity);
	}
}

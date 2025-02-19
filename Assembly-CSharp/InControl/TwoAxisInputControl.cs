using UnityEngine;

namespace InControl;

public class TwoAxisInputControl
{
	private bool thisState;

	private bool lastState;

	public static float StateThreshold;

	public float X { get; protected set; }

	public float Y { get; protected set; }

	public OneAxisInputControl Left { get; protected set; }

	public OneAxisInputControl Right { get; protected set; }

	public OneAxisInputControl Up { get; protected set; }

	public OneAxisInputControl Down { get; protected set; }

	public ulong UpdateTick { get; protected set; }

	public bool State => thisState;

	public bool HasChanged => thisState != lastState;

	public Vector2 Vector => new Vector2(X, Y);

	internal TwoAxisInputControl()
	{
		Left = new OneAxisInputControl();
		Right = new OneAxisInputControl();
		Up = new OneAxisInputControl();
		Down = new OneAxisInputControl();
	}

	internal void Update(float x, float y, ulong updateTick)
	{
		lastState = thisState;
		X = x;
		Y = y;
		Left.UpdateWithValue(Mathf.Clamp01(0f - X), updateTick, StateThreshold);
		Right.UpdateWithValue(Mathf.Clamp01(X), updateTick, StateThreshold);
		if (InputManager.InvertYAxis)
		{
			Up.UpdateWithValue(Mathf.Clamp01(0f - Y), updateTick, StateThreshold);
			Down.UpdateWithValue(Mathf.Clamp01(Y), updateTick, StateThreshold);
		}
		else
		{
			Up.UpdateWithValue(Mathf.Clamp01(Y), updateTick, StateThreshold);
			Down.UpdateWithValue(Mathf.Clamp01(0f - Y), updateTick, StateThreshold);
		}
		thisState = Up.State || Down.State || Left.State || Right.State;
		if (thisState != lastState)
		{
			UpdateTick = updateTick;
		}
	}

	public static implicit operator bool(TwoAxisInputControl control)
	{
		return control.thisState;
	}

	public static implicit operator Vector2(TwoAxisInputControl control)
	{
		return control.Vector;
	}

	public static implicit operator Vector3(TwoAxisInputControl control)
	{
		return new Vector3(control.X, control.Y);
	}
}

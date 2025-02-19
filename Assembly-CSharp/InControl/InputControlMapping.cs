using UnityEngine;

namespace InControl;

public class InputControlMapping
{
	public class Range
	{
		public static Range Complete = new Range
		{
			Minimum = -1f,
			Maximum = 1f
		};

		public static Range Positive = new Range
		{
			Minimum = 0f,
			Maximum = 1f
		};

		public static Range Negative = new Range
		{
			Minimum = -1f,
			Maximum = 0f
		};

		public float Minimum;

		public float Maximum;
	}

	public InputControlSource Source;

	public InputControlType Target;

	public bool Invert;

	public float Scale = 1f;

	public bool Raw;

	public bool IgnoreInitialZeroValue;

	public Range SourceRange = Range.Complete;

	public Range TargetRange = Range.Complete;

	private string handle;

	public string Handle
	{
		get
		{
			return (!string.IsNullOrEmpty(handle)) ? handle : Target.ToString();
		}
		set
		{
			handle = value;
		}
	}

	private bool IsYAxis => Target == InputControlType.LeftStickY || Target == InputControlType.RightStickY;

	public float MapValue(float value)
	{
		float num;
		if (Raw)
		{
			num = value * Scale;
		}
		else
		{
			value = Mathf.Clamp(value * Scale, -1f, 1f);
			if (value < SourceRange.Minimum || value > SourceRange.Maximum)
			{
				return 0f;
			}
			float t = Mathf.InverseLerp(SourceRange.Minimum, SourceRange.Maximum, value);
			num = Mathf.Lerp(TargetRange.Minimum, TargetRange.Maximum, t);
		}
		if (Invert ^ (IsYAxis && InputManager.InvertYAxis))
		{
			num = 0f - num;
		}
		return num;
	}
}

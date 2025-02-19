using System;
using System.Collections.Generic;
using UnityEngine;

namespace WhiteCat.Internal;

[Serializable]
public class Spline
{
	private const float minError = 0.001f;

	private const float maxError = 1000f;

	private const int minSegments = 8;

	private const int maxSegments = 80000;

	private const float segmentsFactor = 0.1f;

	[SerializeField]
	private Vector3 _c0;

	[SerializeField]
	private Vector3 _c1;

	[SerializeField]
	private Vector3 _c2;

	[SerializeField]
	private Vector3 _c3;

	[SerializeField]
	private float _error = 0.01f;

	[SerializeField]
	private Vector2[] _samples;

	public float error
	{
		get
		{
			return _error;
		}
		set
		{
			_error = Mathf.Clamp(value, 0.001f, 1000f);
			_samples = null;
		}
	}

	public bool isSamplesInvalid => Utility.IsNullOrEmpty(_samples);

	public int samplesCount
	{
		get
		{
			if (Utility.IsNullOrEmpty(_samples))
			{
				CalculateLength();
			}
			return _samples.Length;
		}
	}

	public float totalLength
	{
		get
		{
			if (Utility.IsNullOrEmpty(_samples))
			{
				CalculateLength();
			}
			return _samples[_samples.Length - 1].y;
		}
	}

	public void SetCardinalParameters(Vector3 p0, Vector3 p1, Vector3 p2, Vector3 p3, float tension = 0.5f)
	{
		_c0 = p1;
		_c1 = (p2 - p0) * tension;
		_c2 = (p2 - p1) * 3f - (p3 - p1) * tension - (_c1 + _c1);
		_c3 = (p3 - p1) * tension - (p2 - p1) * 2f + _c1;
		_samples = null;
	}

	public void SetBezierParameters(Vector3 p0, Vector3 p1, Vector3 p2, Vector3 p3)
	{
		_c0 = p0;
		_c1 = 3f * (p1 - p0);
		_c2 = 3f * (p2 - 2f * p1 + p0);
		_c3 = p3 + 3f * (p1 - p2) - p0;
		_samples = null;
	}

	public Vector3 GetPoint(float t)
	{
		return _c0 + t * _c1 + t * t * _c2 + t * t * t * _c3;
	}

	public Vector3 GetDerivative(float t)
	{
		return _c1 + 2f * t * _c2 + 3f * t * t * _c3;
	}

	public Vector3 GetSecondDerivative(float t)
	{
		return _c2 + _c2 + 6f * t * _c3;
	}

	public void ClearSamples()
	{
		_samples = null;
	}

	public Vector2 GetSample(int index)
	{
		if (Utility.IsNullOrEmpty(_samples))
		{
			CalculateLength();
		}
		return _samples[Mathf.Clamp(index, 0, _samples.Length - 1)];
	}

	public void CalculateLength()
	{
		if (!Utility.IsNullOrEmpty(_samples))
		{
			return;
		}
		Vector3 vector = _c0;
		Vector2 zero = Vector2.zero;
		Vector2 item = Vector2.zero;
		float num = float.MaxValue;
		float num2 = float.MinValue;
		for (int i = 1; i <= 8; i++)
		{
			Vector3 point = GetPoint((float)i / 8f);
			zero.y += (point - vector).magnitude;
			vector = point;
		}
		int num3 = Mathf.Clamp((int)(zero.y * 0.1f / _error) + 1, 8, 80000);
		List<Vector2> list = new List<Vector2>((int)((float)num3 * 0.1f) + 1);
		list.Add(item);
		vector = _c0;
		zero = Vector2.zero;
		Vector2 vector2 = default(Vector2);
		for (int j = 1; j <= num3; j++)
		{
			Vector3 point = GetPoint(zero.x = (float)j / (float)num3);
			zero.y += (point - vector).magnitude;
			vector = point;
			float num4 = (zero.y + _error - item.y) / (zero.x - item.x);
			float num5 = (zero.y - _error - item.y) / (zero.x - item.x);
			if (num4 < num2 || num5 > num)
			{
				vector2.x = (float)(j - 1) / (float)num3;
				vector2.y = (vector2.x - item.x) * (num2 + num) * 0.5f + item.y;
				list.Add(item = vector2);
				num = (zero.y + _error - item.y) / (zero.x - item.x);
				num2 = (zero.y - _error - item.y) / (zero.x - item.x);
				continue;
			}
			if (num4 < num)
			{
				num = num4;
			}
			if (num5 > num2)
			{
				num2 = num5;
			}
		}
		list.Add(zero);
		_samples = list.ToArray();
	}

	public float GetLength(float t)
	{
		if (Utility.IsNullOrEmpty(_samples))
		{
			CalculateLength();
		}
		if (t >= 1f)
		{
			return _samples[_samples.Length - 1].y;
		}
		if (t <= 0f)
		{
			return 0f;
		}
		int num = (int)(t * (float)_samples.Length);
		Vector2 vector;
		if (_samples[num].x > t)
		{
			while (_samples[--num].x > t)
			{
			}
			vector = _samples[num++];
		}
		else
		{
			while (_samples[++num].x < t)
			{
			}
			vector = _samples[num - 1];
		}
		return vector.y + (t - vector.x) * (_samples[num].y - vector.y) / (_samples[num].x - vector.x);
	}

	public float GetPositionAtLength(float s)
	{
		if (Utility.IsNullOrEmpty(_samples))
		{
			CalculateLength();
		}
		if (s >= _samples[_samples.Length - 1].y)
		{
			return 1f;
		}
		if (s <= 0f)
		{
			return 0f;
		}
		int num = (int)(s / _samples[_samples.Length - 1].y * (float)_samples.Length);
		Vector2 vector;
		if (_samples[num].y > s)
		{
			while (_samples[--num].y > s)
			{
			}
			vector = _samples[num++];
		}
		else
		{
			while (_samples[++num].y < s)
			{
			}
			vector = _samples[num - 1];
		}
		return vector.x + (s - vector.y) * (_samples[num].x - vector.x) / (_samples[num].y - vector.y);
	}

	public float GetClosestPosition(Vector3 given, float segmentLength, int minSegments = 8, int maxSegments = 64)
	{
		int num = Mathf.Clamp((int)(totalLength / segmentLength), minSegments, maxSegments);
		float t = 0f;
		float num2 = 0f;
		float num3 = (given - _c0).sqrMagnitude;
		for (int i = 1; i <= num; i++)
		{
			float sqrMagnitude = (given - GetPoint(num2 = (float)i / (float)num)).sqrMagnitude;
			if (sqrMagnitude < num3)
			{
				t = num2;
				num3 = sqrMagnitude;
			}
		}
		Vector3 vector = given - GetPoint(t);
		Vector3 derivative = GetDerivative(t);
		if (Vector3.Dot(vector, derivative) > 0f)
		{
			return GetPositionAtLength(GetLength(t) + Vector3.Project(vector, derivative).magnitude);
		}
		return GetPositionAtLength(GetLength(t) - Vector3.Project(vector, derivative).magnitude);
	}
}

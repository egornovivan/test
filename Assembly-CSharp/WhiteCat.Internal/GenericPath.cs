using System;
using System.Collections.Generic;
using UnityEngine;

namespace WhiteCat.Internal;

public abstract class GenericPath<Node, Spline> : Path where Node : PathNode where Spline : PathSpline
{
	[SerializeField]
	[GetSet("isCircular")]
	protected bool _isCircular;

	[GetSet("lengthError")]
	[SerializeField]
	protected float _lengthError;

	[SerializeField]
	protected List<Node> _nodes;

	[SerializeField]
	protected List<Spline> _splines;

	[SerializeField]
	protected int _invalidSplineLengthIndex;

	public override bool isCircular
	{
		get
		{
			return _isCircular;
		}
		set
		{
			if (value != _isCircular)
			{
				_isCircular = value;
				TriggerOnChangeEvents();
			}
		}
	}

	public override float lengthError
	{
		get
		{
			return _lengthError;
		}
		set
		{
			_lengthError = Mathf.Clamp(value, 0.001f, 1000f);
			for (int i = 0; i < _splines.Count; i++)
			{
				Spline val = _splines[i];
				val.error = _lengthError;
			}
			_invalidSplineLengthIndex = 0;
			TriggerOnChangeEvents();
		}
	}

	public override int nodesCount => _nodes.Count;

	public override bool isLengthValid => _invalidSplineLengthIndex >= splinesCount;

	public override float pathTotalLength
	{
		get
		{
			int num = splinesCount - 1;
			CalculatePathLength(num);
			return _splines[num].pathLength;
		}
	}

	protected abstract void InitializeNodesAndSplines();

	protected abstract void UpdateSplineParameters(int splineIndex);

	private void Awake()
	{
		if (Utility.IsNullOrEmpty(_nodes))
		{
			Reset();
		}
		else
		{
			base.transform.hasChanged = false;
		}
	}

	private void Reset()
	{
		_isCircular = false;
		_lengthError = 0.01f;
		_invalidSplineLengthIndex = 0;
		InitializeNodesAndSplines();
		base.transform.hasChanged = true;
	}

	public override void CheckAndResetAllSplines()
	{
		if (base.transform.hasChanged)
		{
			int count = _splines.Count;
			for (int i = 0; i < count; i++)
			{
				UpdateSplineParameters(i);
			}
			base.transform.hasChanged = false;
			TriggerOnChangeEvents();
		}
	}

	public override void CalculatePathLength(int splineIndex)
	{
		CheckAndResetAllSplines();
		while (_invalidSplineLengthIndex <= splineIndex)
		{
			Spline val = _splines[_invalidSplineLengthIndex];
			Spline val2 = _splines[_invalidSplineLengthIndex];
			val.pathLength = val2.totalLength + ((_invalidSplineLengthIndex != 0) ? _splines[_invalidSplineLengthIndex - 1].pathLength : 0f);
			_invalidSplineLengthIndex++;
		}
	}

	public override float GetPathLength(int splineIndex)
	{
		CalculatePathLength(splineIndex);
		return _splines[splineIndex].pathLength;
	}

	public override float GetPathLength(int splineIndex, float splineTime)
	{
		CalculatePathLength(splineIndex);
		Spline val = _splines[splineIndex];
		return val.GetLength(splineTime) + ((splineIndex != 0) ? _splines[splineIndex - 1].pathLength : 0f);
	}

	public override void ClearAllSamples()
	{
		for (int i = 0; i < _splines.Count; i++)
		{
			Spline val = _splines[i];
			val.ClearSamples();
		}
		_invalidSplineLengthIndex = 0;
	}

	public override Vector3 GetNodeLocalPosition(int nodeIndex)
	{
		return _nodes[nodeIndex].localPosition;
	}

	public override Vector3 GetNodePosition(int nodeIndex)
	{
		return base.transform.TransformPoint(_nodes[nodeIndex].localPosition);
	}

	public override void SetNodePosition(int nodeIndex, Vector3 position)
	{
		SetNodeLocalPosition(nodeIndex, base.transform.InverseTransformPoint(position));
	}

	public override Quaternion GetNodeLocalRotation(int nodeIndex)
	{
		return _nodes[nodeIndex].localRotation;
	}

	public override Quaternion GetNodeRotation(int nodeIndex)
	{
		return Quaternion.LookRotation(base.transform.TransformVector(_nodes[nodeIndex].localRotation * Vector3.forward), base.transform.TransformVector(_nodes[nodeIndex].localRotation * Vector3.up));
	}

	public override void SetNodeRotation(int nodeIndex, Quaternion rotation)
	{
		SetNodeLocalRotation(nodeIndex, Quaternion.LookRotation(base.transform.InverseTransformVector(rotation * Vector3.forward), base.transform.InverseTransformVector(rotation * Vector3.up)));
	}

	public override Vector3 GetSplinePoint(int splineIndex, float splineTime)
	{
		CheckAndResetAllSplines();
		Spline val = _splines[splineIndex];
		return val.GetPoint(splineTime);
	}

	public override Vector3 GetSplineDerivative(int splineIndex, float splineTime)
	{
		CheckAndResetAllSplines();
		Spline val = _splines[splineIndex];
		return val.GetDerivative(splineTime);
	}

	public override Vector3 GetSplineSecondDerivative(int splineIndex, float splineTime)
	{
		CheckAndResetAllSplines();
		Spline val = _splines[splineIndex];
		return val.GetSecondDerivative(splineTime);
	}

	public override Quaternion GetSplineRotation(int splineIndex, float splineTime, Vector3 upwards, bool reverseForward = false)
	{
		CheckAndResetAllSplines();
		Vector3 forward;
		if (reverseForward)
		{
			Spline val = _splines[splineIndex];
			forward = -val.GetDerivative(splineTime);
		}
		else
		{
			Spline val2 = _splines[splineIndex];
			forward = val2.GetDerivative(splineTime);
		}
		return Quaternion.LookRotation(forward, upwards);
	}

	public override bool IsSplineSamplesInvalid(int splineIndex)
	{
		CheckAndResetAllSplines();
		Spline val = _splines[splineIndex];
		return val.isSamplesInvalid;
	}

	public override int GetSplineSamplesCount(int splineIndex)
	{
		CheckAndResetAllSplines();
		Spline val = _splines[splineIndex];
		return val.samplesCount;
	}

	public override float GetSplineTotalLength(int splineIndex)
	{
		CheckAndResetAllSplines();
		Spline val = _splines[splineIndex];
		return val.totalLength;
	}

	public override float GetSplineLength(int splineIndex, float splineTime)
	{
		CheckAndResetAllSplines();
		Spline val = _splines[splineIndex];
		return val.GetLength(splineTime);
	}

	public override float GetSplinePositionAtLength(int splineIndex, float splineLength)
	{
		CheckAndResetAllSplines();
		Spline val = _splines[splineIndex];
		return val.GetPositionAtLength(splineLength);
	}

	public override float GetClosestSplinePosition(int splineIndex, Vector3 given, float segmentLength, int minSegments = 8, int maxSegments = 64)
	{
		CheckAndResetAllSplines();
		Spline val = _splines[splineIndex];
		return val.GetClosestPosition(given, segmentLength, minSegments, maxSegments);
	}

	public override void GetPathPositionAtPathLength(float pathLength, ref int splineIndex, ref float splineTime)
	{
		int num = splinesCount - 1;
		CalculatePathLength(num);
		float pathLength2 = _splines[num].pathLength;
		if (_isCircular)
		{
			pathLength = (pathLength2 + pathLength % pathLength2) % pathLength2;
		}
		else
		{
			if (pathLength <= 0f)
			{
				splineIndex = 0;
				splineTime = 0f;
				return;
			}
			if (pathLength >= pathLength2)
			{
				splineIndex = num;
				splineTime = 1f;
				return;
			}
		}
		if (splineIndex < 0)
		{
			splineIndex = (int)(pathLength / pathLength2 * (float)num);
		}
		float pathLength3;
		if (_splines[splineIndex].pathLength > pathLength)
		{
			do
			{
				if (splineIndex == 0)
				{
					Spline val = _splines[0];
					splineTime = val.GetPositionAtLength(pathLength);
					return;
				}
			}
			while (_splines[--splineIndex].pathLength > pathLength);
			pathLength3 = _splines[splineIndex].pathLength;
			splineIndex++;
		}
		else
		{
			while (_splines[++splineIndex].pathLength < pathLength)
			{
			}
			pathLength3 = _splines[splineIndex - 1].pathLength;
		}
		Spline val2 = _splines[splineIndex];
		splineTime = val2.GetPositionAtLength(pathLength - pathLength3);
	}

	public override void GetClosestPathPosition(Vector3 given, float segmentLength, out int splineIndex, out float splineTime, int minSegmentsEverySpline = 8, int maxSegmentsEverySpline = 64)
	{
		CheckAndResetAllSplines();
		float num = float.MaxValue;
		splineIndex = 0;
		splineTime = 0f;
		for (int num2 = splinesCount - 1; num2 >= 0; num2--)
		{
			Spline val = _splines[num2];
			float closestPosition = val.GetClosestPosition(given, segmentLength, minSegmentsEverySpline, maxSegmentsEverySpline);
			Spline val2 = _splines[num2];
			float sqrMagnitude = (val2.GetPoint(closestPosition) - given).sqrMagnitude;
			if (sqrMagnitude < num)
			{
				splineIndex = num2;
				splineTime = closestPosition;
				num = sqrMagnitude;
			}
		}
	}

	public override bool GetNextSplinePosition(ref int index, ref float time, float minDeltaAngle, float maxDeltaAngle, Func<int, float, Quaternion> getRotation = null)
	{
		if (getRotation == null)
		{
			getRotation = (int i, float t) => GetSplineRotation(i, t);
		}
		bool flag = false;
		Quaternion a = getRotation(index, time);
		float num3;
		do
		{
			int num = index;
			float num2 = time;
			time += 0.1f;
			if (time >= 1f)
			{
				index++;
				time = 0f;
				if (index >= splinesCount)
				{
					index = splinesCount - 1;
					time = 1f;
					flag = true;
				}
			}
			num3 = Quaternion.Angle(a, getRotation(index, time));
			if (!(num3 > maxDeltaAngle))
			{
				continue;
			}
			int num4 = index;
			float num5 = time;
			while (true)
			{
				time = (num2 + num5) * 0.5f;
				if (num != num4)
				{
					if (1f - num2 > num5)
					{
						index = num;
						time += 0.5f;
					}
					else
					{
						index = num4;
						time -= 0.5f;
					}
				}
				num3 = Quaternion.Angle(a, getRotation(index, time));
				if (num3 > maxDeltaAngle)
				{
					num4 = index;
					num5 = time;
					continue;
				}
				if (!(num3 < minDeltaAngle))
				{
					break;
				}
				num = index;
				num2 = time;
			}
			return false;
		}
		while (!(num3 >= minDeltaAngle) && !flag);
		return flag;
	}
}

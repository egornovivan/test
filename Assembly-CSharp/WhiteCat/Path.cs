using System;
using UnityEngine;

namespace WhiteCat;

[ExecuteInEditMode]
[DisallowMultipleComponent]
public abstract class Path : BaseBehaviour
{
	public abstract bool isCircular { get; set; }

	public abstract float lengthError { get; set; }

	public abstract int nodesCount { get; }

	public abstract int splinesCount { get; }

	public abstract bool isNodeRemovable { get; }

	public abstract bool isLengthValid { get; }

	public abstract float pathTotalLength { get; }

	public event Action onChange;

	protected void TriggerOnChangeEvents()
	{
		if (this.onChange != null)
		{
			this.onChange();
		}
	}

	public abstract void CheckAndResetAllSplines();

	public abstract void CalculatePathLength(int splineIndex);

	public abstract float GetPathLength(int splineIndex);

	public abstract float GetPathLength(int splineIndex, float splineTime);

	public abstract void ClearAllSamples();

	public abstract void InsertNode(int nodeIndex);

	public abstract void RemoveNode(int nodeIndex);

	public abstract Vector3 GetNodeLocalPosition(int nodeIndex);

	public abstract void SetNodeLocalPosition(int nodeIndex, Vector3 localPosition);

	public abstract Vector3 GetNodePosition(int nodeIndex);

	public abstract void SetNodePosition(int nodeIndex, Vector3 position);

	public abstract Quaternion GetNodeLocalRotation(int nodeIndex);

	public abstract void SetNodeLocalRotation(int nodeIndex, Quaternion localRotation);

	public abstract Quaternion GetNodeRotation(int nodeIndex);

	public abstract void SetNodeRotation(int nodeIndex, Quaternion rotation);

	public abstract Vector3 GetSplinePoint(int splineIndex, float splineTime);

	public abstract Vector3 GetSplineDerivative(int splineIndex, float splineTime);

	public abstract Vector3 GetSplineSecondDerivative(int splineIndex, float splineTime);

	public abstract Quaternion GetSplineRotation(int splineIndex, float splineTime, Vector3 upwards, bool reverseForward = false);

	public abstract Quaternion GetSplineRotation(int splineIndex, float splineTime, bool reverseForward = false);

	public abstract bool IsSplineSamplesInvalid(int splineIndex);

	public abstract int GetSplineSamplesCount(int splineIndex);

	public abstract float GetSplineTotalLength(int splineIndex);

	public abstract float GetSplineLength(int splineIndex, float splineTime);

	public abstract float GetSplinePositionAtLength(int splineIndex, float splineLength);

	public abstract float GetClosestSplinePosition(int splineIndex, Vector3 given, float segmentLength, int minSegments = 8, int maxSegments = 64);

	public abstract void GetPathPositionAtPathLength(float pathLength, ref int splineIndex, ref float splineTime);

	public abstract void GetClosestPathPosition(Vector3 given, float segmentLength, out int splineIndex, out float splineTime, int minSegmentsEverySpline = 8, int maxSegmentsEverySpline = 64);

	public abstract bool GetNextSplinePosition(ref int index, ref float time, float minDeltaAngle, float maxDeltaAngle, Func<int, float, Quaternion> getRotation = null);
}

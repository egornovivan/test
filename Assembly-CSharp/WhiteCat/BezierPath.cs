using System.Collections.Generic;
using UnityEngine;
using WhiteCat.Internal;

namespace WhiteCat;

[AddComponentMenu("White Cat/Path/Bezier Path")]
public class BezierPath : GenericPath<BezierNode, BezierSpline>
{
	public override int splinesCount => (!_isCircular) ? (_splines.Count - 1) : _splines.Count;

	public override bool isNodeRemovable => _nodes.Count > 2;

	protected override void InitializeNodesAndSplines()
	{
		_nodes = new List<BezierNode>(4);
		_splines = new List<BezierSpline>(4);
		_nodes.Add(new BezierNode(new Vector3(0f, 0f, 0f), Quaternion.identity, 3f, 3f));
		_nodes.Add(new BezierNode(new Vector3(0f, 0f, 10f), Quaternion.identity, 3f, 3f));
		_splines.Add(new BezierSpline(_lengthError));
		_splines.Add(new BezierSpline(_lengthError));
	}

	protected override void UpdateSplineParameters(int splineIndex)
	{
		int count = _splines.Count;
		splineIndex = (splineIndex + count) % count;
		_splines[splineIndex].SetBezierParameters(base.transform.TransformPoint(_nodes[splineIndex].localPosition), base.transform.TransformPoint(_nodes[splineIndex].localForwardPoint), base.transform.TransformPoint(_nodes[(splineIndex + 1) % count].localBackPoint), base.transform.TransformPoint(_nodes[(splineIndex + 1) % count].localPosition));
		if (_invalidSplineLengthIndex > splineIndex)
		{
			_invalidSplineLengthIndex = splineIndex;
		}
	}

	public void InsertNode(int nodeIndex, Vector3 localPosition, Quaternion localRotation, float forwardTangent, float backTangent)
	{
		CheckAndResetAllSplines();
		BezierNode item = new BezierNode(localPosition, localRotation, forwardTangent, backTangent);
		BezierSpline item2 = new BezierSpline(_lengthError);
		_nodes.Insert(nodeIndex, item);
		_splines.Insert(nodeIndex, item2);
		UpdateSplineParameters(nodeIndex - 1);
		UpdateSplineParameters(nodeIndex);
		TriggerOnChangeEvents();
	}

	public override void InsertNode(int nodeIndex)
	{
		CheckAndResetAllSplines();
		int count = _nodes.Count;
		Vector3 localPosition;
		Quaternion quaternion;
		float num;
		if (_isCircular || (nodeIndex > 0 && nodeIndex < count))
		{
			BezierSpline bezierSpline = _splines[(nodeIndex - 1 + count) % count];
			BezierNode bezierNode = _nodes[(nodeIndex - 1 + count) % count];
			BezierNode bezierNode2 = _nodes[nodeIndex % count];
			localPosition = base.transform.InverseTransformPoint(bezierSpline.GetPoint(0.5f));
			quaternion = Quaternion.LookRotation(base.transform.InverseTransformVector(bezierSpline.GetDerivative(0.5f)), Quaternion.Slerp(bezierNode.localRotation, bezierNode2.localRotation, 0.5f) * Vector3.up);
			num = (bezierNode.localForwardTangent + bezierNode2.localBackTangent) * 0.5f;
		}
		else if (nodeIndex == 0)
		{
			quaternion = _nodes[0].localRotation;
			localPosition = _nodes[0].localPosition + quaternion * Vector3.back * (_nodes[1].localPosition - _nodes[0].localPosition).magnitude;
			num = _nodes[0].localBackTangent;
		}
		else
		{
			quaternion = _nodes[count - 1].localRotation;
			localPosition = _nodes[count - 1].localPosition + quaternion * Vector3.forward * (_nodes[count - 2].localPosition - _nodes[count - 1].localPosition).magnitude;
			num = _nodes[count - 1].localForwardTangent;
		}
		InsertNode(nodeIndex, localPosition, quaternion, num, num);
	}

	public override void RemoveNode(int nodeIndex)
	{
		if (_nodes.Count > 2)
		{
			CheckAndResetAllSplines();
			_nodes.RemoveAt(nodeIndex);
			_splines.RemoveAt(nodeIndex);
			UpdateSplineParameters(nodeIndex - 1);
			TriggerOnChangeEvents();
		}
	}

	public override void SetNodeLocalPosition(int nodeIndex, Vector3 localPosition)
	{
		CheckAndResetAllSplines();
		_nodes[nodeIndex].localPosition = localPosition;
		UpdateSplineParameters(nodeIndex - 1);
		UpdateSplineParameters(nodeIndex);
		TriggerOnChangeEvents();
	}

	public override void SetNodeLocalRotation(int nodeIndex, Quaternion localRotation)
	{
		CheckAndResetAllSplines();
		_nodes[nodeIndex].localRotation = localRotation;
		UpdateSplineParameters(nodeIndex - 1);
		UpdateSplineParameters(nodeIndex);
		TriggerOnChangeEvents();
	}

	public float GetNodeLocalForwardTangent(int nodeIndex)
	{
		return _nodes[nodeIndex].localForwardTangent;
	}

	public void SetNodeLocalForwardTangent(int nodeIndex, float localForwardTangent)
	{
		CheckAndResetAllSplines();
		_nodes[nodeIndex].localForwardTangent = localForwardTangent;
		UpdateSplineParameters(nodeIndex);
		TriggerOnChangeEvents();
	}

	public float GetNodeLocalBackTangent(int nodeIndex)
	{
		return _nodes[nodeIndex].localBackTangent;
	}

	public void SetNodeLocalBackTangent(int nodeIndex, float localBackTangent)
	{
		CheckAndResetAllSplines();
		_nodes[nodeIndex].localBackTangent = localBackTangent;
		UpdateSplineParameters(nodeIndex - 1);
		TriggerOnChangeEvents();
	}

	public Vector3 GetNodeForwardPoint(int nodeIndex)
	{
		return base.transform.TransformPoint(_nodes[nodeIndex].localForwardPoint);
	}

	public void SetNodeForwardPoint(int nodeIndex, Vector3 forwardPoint)
	{
		CheckAndResetAllSplines();
		_nodes[nodeIndex].localForwardPoint = base.transform.InverseTransformPoint(forwardPoint);
		UpdateSplineParameters(nodeIndex - 1);
		UpdateSplineParameters(nodeIndex);
		TriggerOnChangeEvents();
	}

	public Vector3 GetNodeBackPoint(int nodeIndex)
	{
		return base.transform.TransformPoint(_nodes[nodeIndex].localBackPoint);
	}

	public void SetNodeBackPoint(int nodeIndex, Vector3 backPoint)
	{
		CheckAndResetAllSplines();
		_nodes[nodeIndex].localBackPoint = base.transform.InverseTransformPoint(backPoint);
		UpdateSplineParameters(nodeIndex - 1);
		UpdateSplineParameters(nodeIndex);
		TriggerOnChangeEvents();
	}

	public override Quaternion GetSplineRotation(int splineIndex, float splineTime, bool reverseForward = false)
	{
		CheckAndResetAllSplines();
		return Quaternion.LookRotation((!reverseForward) ? _splines[splineIndex].GetDerivative(splineTime) : (-_splines[splineIndex].GetDerivative(splineTime)), base.transform.TransformVector(Quaternion.Slerp(_nodes[splineIndex].localRotation, _nodes[(splineIndex + 1) % _nodes.Count].localRotation, splineTime) * Vector3.up));
	}
}

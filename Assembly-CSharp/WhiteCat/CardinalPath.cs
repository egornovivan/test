using System.Collections.Generic;
using UnityEngine;
using WhiteCat.Internal;

namespace WhiteCat;

[AddComponentMenu("White Cat/Path/Cardinal Path")]
public class CardinalPath : GenericPath<CardinalNode, CardinalSpline>
{
	public override int splinesCount => (!_isCircular) ? (_splines.Count - 3) : _splines.Count;

	public override bool isNodeRemovable => _nodes.Count > 4;

	protected override void InitializeNodesAndSplines()
	{
		_nodes = new List<CardinalNode>(4);
		_splines = new List<CardinalSpline>(4);
		for (int i = -1; i < 3; i++)
		{
			_nodes.Add(new CardinalNode(new Vector3(0f, 0f, i * 10), Quaternion.identity));
			_splines.Add(new CardinalSpline(_lengthError, 0.5f));
		}
	}

	protected override void UpdateSplineParameters(int splineIndex)
	{
		int count = _splines.Count;
		splineIndex = (splineIndex + count) % count;
		_splines[splineIndex].SetCardinalParameters(base.transform.TransformPoint(_nodes[splineIndex].localPosition), base.transform.TransformPoint(_nodes[(splineIndex + 1) % count].localPosition), base.transform.TransformPoint(_nodes[(splineIndex + 2) % count].localPosition), base.transform.TransformPoint(_nodes[(splineIndex + 3) % count].localPosition), _splines[splineIndex].tension);
		if (_invalidSplineLengthIndex > splineIndex)
		{
			_invalidSplineLengthIndex = splineIndex;
		}
	}

	private void UpdateNodeRotation(int nodeIndex)
	{
		nodeIndex = (nodeIndex + _nodes.Count) % _nodes.Count;
		int index = ((nodeIndex != 0) ? (nodeIndex - 1) : (_nodes.Count - 1));
		int index2 = (nodeIndex + 1) % _nodes.Count;
		_nodes[nodeIndex].localRotation = Quaternion.LookRotation(_nodes[index2].localPosition - _nodes[index].localPosition, _nodes[nodeIndex].localRotation * Vector3.up);
	}

	public void InsertNode(int nodeIndex, Vector3 localPosition, Vector3 localUpwards)
	{
		CheckAndResetAllSplines();
		int index = ((nodeIndex != 0) ? (nodeIndex - 1) : (_nodes.Count - 1));
		int index2 = nodeIndex % _nodes.Count;
		CardinalNode item = new CardinalNode(localPosition, Quaternion.LookRotation(_nodes[index2].localPosition - _nodes[index].localPosition, localUpwards));
		CardinalSpline item2 = new CardinalSpline(_lengthError, _splines[index].tension);
		_nodes.Insert(nodeIndex, item);
		_splines.Insert(nodeIndex, item2);
		UpdateSplineParameters(nodeIndex - 3);
		UpdateSplineParameters(nodeIndex - 2);
		UpdateSplineParameters(nodeIndex - 1);
		UpdateSplineParameters(nodeIndex);
		UpdateNodeRotation(nodeIndex - 1);
		UpdateNodeRotation(nodeIndex + 1);
		TriggerOnChangeEvents();
	}

	public override void InsertNode(int nodeIndex)
	{
		CheckAndResetAllSplines();
		int count = _nodes.Count;
		Vector3 localPosition;
		Vector3 localUpwards;
		if (_isCircular || (nodeIndex > 0 && nodeIndex < count))
		{
			localPosition = base.transform.InverseTransformPoint(_splines[(nodeIndex - 2 + count) % count].GetPoint(0.5f));
			localUpwards = Quaternion.Slerp(_nodes[(nodeIndex - 1 + count) % count].localRotation, _nodes[nodeIndex % count].localRotation, 0.5f) * Vector3.up;
		}
		else if (nodeIndex == 0)
		{
			localPosition = _nodes[0].localPosition * 2f - _nodes[1].localPosition;
			localUpwards = _nodes[0].localRotation * Vector3.up;
		}
		else
		{
			localPosition = _nodes[count - 1].localPosition * 2f - _nodes[count - 2].localPosition;
			localUpwards = _nodes[count - 1].localRotation * Vector3.up;
		}
		InsertNode(nodeIndex, localPosition, localUpwards);
	}

	public override void RemoveNode(int nodeIndex)
	{
		if (_nodes.Count > 4)
		{
			CheckAndResetAllSplines();
			_nodes.RemoveAt(nodeIndex);
			_splines.RemoveAt(nodeIndex);
			UpdateSplineParameters(nodeIndex - 3);
			UpdateSplineParameters(nodeIndex - 2);
			UpdateSplineParameters(nodeIndex - 1);
			UpdateNodeRotation(nodeIndex);
			UpdateNodeRotation(nodeIndex - 1);
			TriggerOnChangeEvents();
		}
	}

	public override void SetNodeLocalPosition(int nodeIndex, Vector3 localPosition)
	{
		CheckAndResetAllSplines();
		_nodes[nodeIndex].localPosition = localPosition;
		UpdateSplineParameters(nodeIndex - 3);
		UpdateSplineParameters(nodeIndex - 2);
		UpdateSplineParameters(nodeIndex - 1);
		UpdateSplineParameters(nodeIndex);
		UpdateNodeRotation(nodeIndex - 1);
		UpdateNodeRotation(nodeIndex + 1);
		TriggerOnChangeEvents();
	}

	public override void SetNodeLocalRotation(int nodeIndex, Quaternion localRotation)
	{
		_nodes[nodeIndex].localRotation = Quaternion.LookRotation(_nodes[nodeIndex].localRotation * Vector3.forward, localRotation * Vector3.up);
		TriggerOnChangeEvents();
	}

	public override Quaternion GetSplineRotation(int splineIndex, float splineTime, bool reverseForward = false)
	{
		CheckAndResetAllSplines();
		return Quaternion.LookRotation((!reverseForward) ? _splines[splineIndex].GetDerivative(splineTime) : (-_splines[splineIndex].GetDerivative(splineTime)), base.transform.TransformVector(Quaternion.Slerp(_nodes[(splineIndex + 1) % _nodes.Count].localRotation, _nodes[(splineIndex + 2) % _nodes.Count].localRotation, splineTime) * Vector3.up));
	}

	public float GetSplineTension(int splineIndex)
	{
		return _splines[splineIndex].tension;
	}

	public void SetSplineTension(int splineIndex, float tension)
	{
		CheckAndResetAllSplines();
		_splines[splineIndex].tension = tension;
		UpdateSplineParameters(splineIndex);
		TriggerOnChangeEvents();
	}
}

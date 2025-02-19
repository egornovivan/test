using System;
using UnityEngine;

namespace WhiteCat.Internal;

[Serializable]
public class BezierNode : PathNode
{
	[SerializeField]
	private float _localForwardTangent;

	[SerializeField]
	private float _localBackTangent;

	public float localForwardTangent
	{
		get
		{
			return _localForwardTangent;
		}
		set
		{
			_localForwardTangent = Mathf.Clamp(value, 0.001f, 1000f);
		}
	}

	public float localBackTangent
	{
		get
		{
			return _localBackTangent;
		}
		set
		{
			_localBackTangent = Mathf.Clamp(value, 0.001f, 1000f);
		}
	}

	public Vector3 localForwardPoint
	{
		get
		{
			return localPosition + localRotation * new Vector3(0f, 0f, _localForwardTangent);
		}
		set
		{
			value -= localPosition;
			localForwardTangent = value.magnitude;
			localRotation = Quaternion.LookRotation(value, localRotation * Vector3.up);
		}
	}

	public Vector3 localBackPoint
	{
		get
		{
			return localPosition + localRotation * new Vector3(0f, 0f, 0f - _localBackTangent);
		}
		set
		{
			value = localPosition - value;
			localBackTangent = value.magnitude;
			localRotation = Quaternion.LookRotation(value, localRotation * Vector3.up);
		}
	}

	public BezierNode(Vector3 localPosition, Quaternion localRotation, float localForwardTangent, float localBackTangent)
		: base(localPosition, localRotation)
	{
		this.localForwardTangent = localForwardTangent;
		this.localBackTangent = localBackTangent;
	}
}
